using FluentAssertions;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using Xunit;

namespace VRCGPUTool.Tests;

public class ScheduleEvaluatorTests
{
    // ─────────────────────────────────────────────────────────
    // DayFlagOf
    // ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(DayOfWeek.Monday, ScheduleDays.Monday)]
    [InlineData(DayOfWeek.Tuesday, ScheduleDays.Tuesday)]
    [InlineData(DayOfWeek.Wednesday, ScheduleDays.Wednesday)]
    [InlineData(DayOfWeek.Thursday, ScheduleDays.Thursday)]
    [InlineData(DayOfWeek.Friday, ScheduleDays.Friday)]
    [InlineData(DayOfWeek.Saturday, ScheduleDays.Saturday)]
    [InlineData(DayOfWeek.Sunday, ScheduleDays.Sunday)]
    public void DayFlagOf_ReturnsCorrectFlag(DayOfWeek dow, ScheduleDays expected)
    {
        ScheduleEvaluator.DayFlagOf(dow).Should().Be(expected);
    }

    // ─────────────────────────────────────────────────────────
    // DateLabel
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void DateLabel_SameDay_ReturnsToday()
    {
        var now = new DateTime(2025, 6, 10, 15, 0, 0);

        ScheduleEvaluator.DateLabel(now, now).Should().Be("今日");
    }

    [Fact]
    public void DateLabel_Tomorrow_ReturnsTomorrow()
    {
        var now = new DateTime(2025, 6, 10, 15, 0, 0);
        var tomorrow = now.AddDays(1);

        ScheduleEvaluator.DateLabel(tomorrow, now).Should().Be("明日");
    }

    [Theory]
    [InlineData(2, "6/12(木)")]
    [InlineData(7, "6/17(火)")]
    public void DateLabel_OtherDate_ReturnsMonthSlashDayWithDow(int daysAhead, string expected)
    {
        var now = new DateTime(2025, 6, 10, 15, 0, 0);
        var dt = now.AddDays(daysAhead);

        ScheduleEvaluator.DateLabel(dt, now).Should().Be(expected);
    }

    [Fact]
    public void DateLabel_PastDate_ReturnsMonthSlashDayWithDow()
    {
        var now = new DateTime(2025, 6, 10, 15, 0, 0);
        var past = new DateTime(2025, 3, 5, 8, 0, 0); // 水曜

        ScheduleEvaluator.DateLabel(past, now).Should().Be("3/5(水)");
    }

    // ─────────────────────────────────────────────────────────
    // IsSlotActiveNow — 基本ケース
    // ─────────────────────────────────────────────────────────

    // 開始 = 終了のスロットは常に非活性
    [Fact]
    public void IsSlotActiveNow_StartEqualsEnd_ReturnsFalse()
    {
        var slot = MakeSlot(startH: 10, startM: 0, endH: 10, endM: 0, ScheduleDays.Monday);
        var now = Monday(10, 0);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // IsSlotActiveNow — 同日スロット (start < end)
    // ─────────────────────────────────────────────────────────

    // 範囲内・正しい曜日 → true
    [Fact]
    public void IsSlotActiveNow_WithinRange_CorrectDay_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(12, 0);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 範囲内・曜日不一致 → false
    [Fact]
    public void IsSlotActiveNow_WithinRange_WrongDay_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 12, 0, 0); // Tuesday

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 開始前 → false
    [Fact]
    public void IsSlotActiveNow_BeforeStart_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(8, 59);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 終了後 → false
    [Fact]
    public void IsSlotActiveNow_AfterEnd_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(17, 1);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 開始ちょうど → true（>= startMin）
    [Fact]
    public void IsSlotActiveNow_AtExactStart_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(9, 0);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 終了ちょうど → false（< endMin）
    [Fact]
    public void IsSlotActiveNow_AtExactEnd_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(17, 0);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // スロット無効 (Enabled=false) → IsSlotActiveNow は Enabled を見ないため true
    [Fact]
    public void IsSlotActiveNow_DisabledSlot_ReturnsTrueRegardlessOfEnabled()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        slot.Enabled = false;
        var now = Monday(12, 0);

        // IsSlotActiveNow 自体は Enabled を見ない（呼び出し元でフィルタする設計）
        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // IsSlotActiveNow — 日またぎスロット (start > end)
    // ─────────────────────────────────────────────────────────

    // 開始後の時刻・今日が開始曜日 → true
    [Fact]
    public void IsSlotActiveNow_Overnight_AfterStart_TodayIsStartDay_ReturnsTrue()
    {
        // 月曜 22:00 〜 火曜 06:00
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = Monday(23, 0); // 月曜 23:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 開始後の時刻・今日が開始曜日でない → false
    [Fact]
    public void IsSlotActiveNow_Overnight_AfterStart_WrongDay_ReturnsFalse()
    {
        // 火曜 22:00 〜 06:00
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Tuesday);
        var now = Monday(23, 0); // 月曜 23:00（火曜スロットは対象外）

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 開始時刻ちょうど → true
    [Fact]
    public void IsSlotActiveNow_Overnight_AtExactStart_ReturnsTrue()
    {
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = Monday(22, 0);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 翌日 0:00〜終了前・昨日が開始曜日 → true
    [Fact]
    public void IsSlotActiveNow_Overnight_BeforeEnd_YesterdayIsStartDay_ReturnsTrue()
    {
        // 月曜 22:00 〜 06:00、今は火曜 02:00
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 2, 0, 0); // Tuesday 02:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 翌日・昨日が開始曜日でない → false
    [Fact]
    public void IsSlotActiveNow_Overnight_BeforeEnd_WrongYesterday_ReturnsFalse()
    {
        // 火曜 22:00 〜 06:00、今は火曜 02:00（昨日は月曜、火曜スロットではない）
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Tuesday);
        var now = new DateTime(2025, 6, 10, 2, 0, 0); // Tuesday 02:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 翌日・終了時刻ちょうど → false（終了済み）
    [Fact]
    public void IsSlotActiveNow_Overnight_AtExactEnd_ReturnsFalse()
    {
        // 月曜 22:00 〜 06:00、今は火曜 06:00（終了ちょうど）
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 6, 0, 0); // Tuesday 06:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 翌日・終了時刻より後 → false（終了済み）
    [Fact]
    public void IsSlotActiveNow_Overnight_AfterEnd_ReturnsFalse()
    {
        // 月曜 22:00 〜 06:00、今は火曜 07:00（終了後）
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 7, 0, 0); // Tuesday 07:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // 開始前の時刻・今日が開始曜日 → false（まだ始まっていない）
    [Fact]
    public void IsSlotActiveNow_Overnight_BeforeStart_ReturnsFalse()
    {
        // 月曜 22:00 〜 06:00、今は月曜 21:59（開始前）
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = Monday(21, 59);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // IsStartTrigger
    // ─────────────────────────────────────────────────────────

    // 開始時刻ちょうど・正しい曜日 → true
    [Fact]
    public void IsStartTrigger_AtExactStart_CorrectDay_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(9, 0);

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeTrue();
    }

    // 開始時刻ちょうど・曜日不一致 → false
    [Fact]
    public void IsStartTrigger_AtExactStart_WrongDay_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 9, 0, 0); // Tuesday

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeFalse();
    }

    // 開始時刻でない → false
    [Fact]
    public void IsStartTrigger_NotAtStartTime_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(9, 1);

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeFalse();
    }

    // 開始 = 終了のスロットは常に false
    [Fact]
    public void IsStartTrigger_StartEqualsEnd_ReturnsFalse()
    {
        var slot = MakeSlot(10, 0, 10, 0, ScheduleDays.Monday);
        var now = Monday(10, 0);

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeFalse();
    }

    // 日またぎ: 開始時刻ちょうど・正しい曜日 → true
    [Fact]
    public void IsStartTrigger_Overnight_AtExactStart_CorrectDay_ReturnsTrue()
    {
        var slot = MakeSlot(23, 0, 1, 0, ScheduleDays.Monday);
        var now = Monday(23, 0);

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeTrue();
    }

    // 日またぎ: 開始時刻ちょうど・曜日不一致 → false
    [Fact]
    public void IsStartTrigger_Overnight_AtExactStart_WrongDay_ReturnsFalse()
    {
        var slot = MakeSlot(23, 0, 1, 0, ScheduleDays.Tuesday);
        var now = Monday(23, 0);

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // IsEndTrigger — 同日スロット (start < end)
    // ─────────────────────────────────────────────────────────

    // 終了時刻ちょうど・正しい曜日 → true
    [Fact]
    public void IsEndTrigger_SameDay_AtExactEnd_CorrectDay_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(17, 0);

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeTrue();
    }

    // 終了時刻ちょうど・曜日不一致 → false
    [Fact]
    public void IsEndTrigger_SameDay_AtExactEnd_WrongDay_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 17, 0, 0); // Tuesday

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // 終了時刻でない → false
    [Fact]
    public void IsEndTrigger_SameDay_NotAtEndTime_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(16, 59);

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // 開始 = 終了のスロットは常に false
    [Fact]
    public void IsEndTrigger_StartEqualsEnd_ReturnsFalse()
    {
        var slot = MakeSlot(10, 0, 10, 0, ScheduleDays.Monday);
        var now = Monday(10, 0);

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // IsEndTrigger — 日またぎスロット (start > end)
    // ─────────────────────────────────────────────────────────

    // 翌日の終了時刻ちょうど・昨日が開始曜日 → true（バグ修正の核心）
    [Fact]
    public void IsEndTrigger_Overnight_AtExactEnd_YesterdayIsStartDay_ReturnsTrue()
    {
        // 月曜 23:00 〜 火曜 01:00、今は火曜 01:00
        var slot = MakeSlot(23, 0, 1, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 1, 0, 0); // Tuesday 01:00

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeTrue();
    }

    // 翌日の終了時刻ちょうど・昨日が開始曜日でない → false
    [Fact]
    public void IsEndTrigger_Overnight_AtExactEnd_WrongYesterday_ReturnsFalse()
    {
        // 火曜 23:00 〜 01:00、今は火曜 01:00（昨日=月曜、火曜スロットではない）
        var slot = MakeSlot(23, 0, 1, 0, ScheduleDays.Tuesday);
        var now = new DateTime(2025, 6, 10, 1, 0, 0); // Tuesday 01:00

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // 開始日当日に終了時刻と同じ時刻 → false（まだ開始前の同時刻）
    [Fact]
    public void IsEndTrigger_Overnight_AtEndTime_OnStartDay_ReturnsFalse()
    {
        // 月曜 23:00 〜 01:00、今は月曜 01:00（昨日=日曜、月曜スロットではない）
        var slot = MakeSlot(23, 0, 1, 0, ScheduleDays.Monday);
        var now = Monday(1, 0); // Monday 01:00

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // 翌日・終了時刻でない → false
    [Fact]
    public void IsEndTrigger_Overnight_NotAtEndTime_ReturnsFalse()
    {
        // 月曜 23:00 〜 01:00、今は火曜 00:30
        var slot = MakeSlot(23, 0, 1, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 0, 30, 0); // Tuesday 00:30

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — スロットなし
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void GetSummaryText_NoSlots_ReturnsNoSchedule()
    {
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([], isLimiting: false, now).Should().Be("スケジュールなし");
    }

    [Fact]
    public void GetSummaryText_AllDisabled_ReturnsNoSchedule()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        slot.Enabled = false;
        var now = Monday(8, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now).Should().Be("スケジュールなし");
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — 制限中（次の解除時刻を表示）
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void GetSummaryText_Limiting_NextEndToday_ShowsTodayAndTime()
    {
        // 月曜 09:00〜17:00、制限中、今は 12:00 → 「今日 17:00 に解除」
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("今日 17:00 に解除");
    }

    [Fact]
    public void GetSummaryText_Limiting_NextEndTomorrow_ShowsTomorrow()
    {
        // 火曜 09:00〜17:00、今は月曜 12:00 → 「明日 17:00 に解除」（明日は曜日なし）
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Tuesday);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("明日 17:00 に解除");
    }

    [Fact]
    public void GetSummaryText_Limiting_NextEndDayAfterTomorrow_ShowsDateWithDow()
    {
        // 水曜 09:00〜17:00、今は月曜 12:00 → 「6/11(水) 17:00 に解除」
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Wednesday);
        var now = Monday(12, 0); // 2025-06-09(月)

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("6/11(水) 17:00 に解除");
    }

    [Fact]
    public void GetSummaryText_Limiting_NoEndWithin7Days_ReturnsLimiting()
    {
        // Days=None のスロットはどの曜日にもマッチしないため解除時刻が見つからない → 「制限中」
        var slot = MakeSlot(9, 0, 10, 0, ScheduleDays.None);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("制限中 (終了なし)");
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — 未制限（次の開始スロットを表示）
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void GetSummaryText_NotLimiting_NextStartToday_SameDay_ShowsRange()
    {
        // 月曜 14:00〜18:00、今は 12:00 → 「今日 14:00 〜 18:00 まで制限」
        var slot = MakeSlot(14, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now)
            .Should().Be("今日 14:00 〜 18:00 まで制限");
    }

    [Fact]
    public void GetSummaryText_NotLimiting_OvernightSlot_ShowsCrossDay()
    {
        // 月曜 22:00〜火曜 06:00、今は月曜 12:00 → 「今日 22:00 〜 明日 06:00 まで制限」
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now)
            .Should().Be("今日 22:00 〜 明日 6:00 まで制限");
    }

    [Fact]
    public void GetSummaryText_NotLimiting_SameTimeSlot_ShowsFromOnly()
    {
        // StartHour == EndHour && StartMinute == EndMinute → 「今日 10:00 から制限」
        var slot = MakeSlot(10, 0, 10, 0, ScheduleDays.Monday);
        var now = Monday(9, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now)
            .Should().Be("今日 10:00 から制限 (終了なし)");
    }

    [Fact]
    public void GetSummaryText_NotLimiting_NoFutureSlot_ReturnsNoSchedule()
    {
        // Days=None のスロットはどの曜日にもマッチしないため次の開始が見つからない → 「スケジュールなし」
        var slot = MakeSlot(14, 0, 18, 0, ScheduleDays.None);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now)
            .Should().Be("スケジュールなし");
    }

    [Fact]
    public void GetSummaryText_NotLimiting_NextStartTomorrow_ShowsTomorrow()
    {
        // 火曜 10:00〜12:00、今は月曜 20:00 → 「明日 10:00 〜 12:00 まで制限」
        var slot = MakeSlot(10, 0, 12, 0, ScheduleDays.Tuesday);
        var now = Monday(20, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now)
            .Should().Be("明日 10:00 〜 12:00 まで制限");
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — 重複スロット
    // ─────────────────────────────────────────────────────────

    // スロットA(10:00-18:00)の内側にスロットB(12:00-14:00)が収まる場合、
    // 制限中の「解除時刻」はBが終わる14:00ではなくAが終わる18:00
    [Fact]
    public void GetSummaryText_Limiting_OverlappingSlots_InnerEndsFirst_ShowsOuterEndTime()
    {
        var slotA = MakeSlot(10, 0, 18, 0, ScheduleDays.Monday);
        var slotB = MakeSlot(12, 0, 14, 0, ScheduleDays.Monday);
        var now = Monday(13, 0); // 両スロットのアクティブ中

        ScheduleEvaluator.GetSummaryText([slotA, slotB], isLimiting: true, now)
            .Should().Be("今日 18:00 に解除");
    }

    // 部分重複(A: 10:00-14:00、B: 12:00-18:00)の場合も正しい終了時刻
    [Fact]
    public void GetSummaryText_Limiting_PartiallyOverlappingSlots_ShowsLastEndTime()
    {
        var slotA = MakeSlot(10, 0, 14, 0, ScheduleDays.Monday);
        var slotB = MakeSlot(12, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(13, 0); // 両スロットのアクティブ中

        ScheduleEvaluator.GetSummaryText([slotA, slotB], isLimiting: true, now)
            .Should().Be("今日 18:00 に解除");
    }

    // 重複していないスロット(A: 10:00-12:00、B: 14:00-18:00)で制限中のとき、
    // 現在のスロット(A)の終了時刻を表示（B の開始は無関係）
    [Fact]
    public void GetSummaryText_Limiting_NonOverlappingSlots_ShowsCurrentSlotEnd()
    {
        var slotA = MakeSlot(10, 0, 12, 0, ScheduleDays.Monday);
        var slotB = MakeSlot(14, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(11, 0); // Aのみアクティブ

        ScheduleEvaluator.GetSummaryText([slotA, slotB], isLimiting: true, now)
            .Should().Be("今日 12:00 に解除");
    }

    // IsSlotActiveNow: 重複スロットの境界で両方の状態が正しいか確認
    [Fact]
    public void IsSlotActiveNow_OverlappingSlots_BothActiveInSharedRange()
    {
        var slotA = MakeSlot(10, 0, 18, 0, ScheduleDays.Monday);
        var slotB = MakeSlot(12, 0, 14, 0, ScheduleDays.Monday);
        var now = Monday(13, 0);

        ScheduleEvaluator.IsSlotActiveNow(slotA, now).Should().BeTrue();
        ScheduleEvaluator.IsSlotActiveNow(slotB, now).Should().BeTrue();
    }

    // IsSlotActiveNow: 内側スロット(B)終了後、外側スロット(A)のみアクティブ
    [Fact]
    public void IsSlotActiveNow_OverlappingSlots_AfterInnerEnds_OuterStillActive()
    {
        var slotA = MakeSlot(10, 0, 18, 0, ScheduleDays.Monday);
        var slotB = MakeSlot(12, 0, 14, 0, ScheduleDays.Monday);
        var atBEnd = Monday(14, 0); // Bの終了時刻ちょうど

        ScheduleEvaluator.IsSlotActiveNow(slotA, atBEnd).Should().BeTrue();  // Aはまだアクティブ
        ScheduleEvaluator.IsSlotActiveNow(slotB, atBEnd).Should().BeFalse(); // Bは終了
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — 連続スロット（終端延伸）
    // ─────────────────────────────────────────────────────────

    // スロット1(03:00-05:00)とスロット2(05:00-18:00)が連続する場合、
    // 未制限時のサマリは「3:00 〜 18:00 まで制限」
    [Fact]
    public void GetSummaryText_NotLimiting_ContiguousSlots_ShowsMergedRange()
    {
        var slot1 = MakeSlot(3, 0, 5, 0, ScheduleDays.Monday);
        var slot2 = MakeSlot(5, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(1, 0); // 3時前

        ScheduleEvaluator.GetSummaryText([slot1, slot2], isLimiting: false, now)
            .Should().Be("今日 3:00 〜 18:00 まで制限");
    }

    // 3スロット連鎖(03:00-05:00, 05:00-10:00, 10:00-18:00)でも正しく18:00まで
    [Fact]
    public void GetSummaryText_NotLimiting_ThreeContiguousSlots_ShowsMergedRange()
    {
        var slot1 = MakeSlot(3, 0, 5, 0, ScheduleDays.Monday);
        var slot2 = MakeSlot(5, 0, 10, 0, ScheduleDays.Monday);
        var slot3 = MakeSlot(10, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(1, 0);

        ScheduleEvaluator.GetSummaryText([slot1, slot2, slot3], isLimiting: false, now)
            .Should().Be("今日 3:00 〜 18:00 まで制限");
    }

    // 連続していないスロット(03:00-05:00, 06:00-18:00)は延伸しない
    [Fact]
    public void GetSummaryText_NotLimiting_GapBetweenSlots_ShowsFirstSlotOnly()
    {
        var slot1 = MakeSlot(3, 0, 5, 0, ScheduleDays.Monday);
        var slot2 = MakeSlot(6, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(1, 0);

        ScheduleEvaluator.GetSummaryText([slot1, slot2], isLimiting: false, now)
            .Should().Be("今日 3:00 〜 5:00 まで制限");
    }

    // 制限中（isLimiting=true）でも連続スロットの正しい解除時刻を表示
    [Fact]
    public void GetSummaryText_Limiting_ContiguousSlots_ShowsLastEndTime()
    {
        var slot1 = MakeSlot(3, 0, 5, 0, ScheduleDays.Monday);
        var slot2 = MakeSlot(5, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(4, 0); // slot1 アクティブ中

        ScheduleEvaluator.GetSummaryText([slot1, slot2], isLimiting: true, now)
            .Should().Be("今日 18:00 に解除");
    }

    // スロット1(03:00-05:00)とスロット2(05:00-03:00 日またぎ)が全曜日で24時間をカバーする場合、
    // 無限ループにならず「から制限」と表示する
    [Fact]
    public void GetSummaryText_NotLimiting_FullDayCoverageAllDays_ShowsFromOnly()
    {
        var allDays = ScheduleDays.Monday | ScheduleDays.Tuesday | ScheduleDays.Wednesday
                    | ScheduleDays.Thursday | ScheduleDays.Friday | ScheduleDays.Saturday | ScheduleDays.Sunday;
        var slot1 = MakeSlot(3, 0, 5, 0, allDays); // 03:00-05:00（全曜日）
        var slot2 = MakeSlot(5, 0, 3, 0, allDays); // 05:00-翌03:00（全曜日）
        var now = Monday(1, 0); // 03時前

        // 例外を投げず、終了時刻なしの表示になること
        ScheduleEvaluator.GetSummaryText([slot1, slot2], isLimiting: false, now)
            .Should().Be("今日 3:00 から制限 (終了なし)");
    }

    // 3スロット(03-11, 11-19, 19-03)が特定曜日のみで24時間をカバーする場合、
    // 翌日の終了時刻が正しく表示される（曜日限定なので有限）
    [Fact]
    public void GetSummaryText_NotLimiting_FullDayCoverageSpecificDay_ShowsEndTime()
    {
        var slot1 = MakeSlot(3, 0, 11, 0, ScheduleDays.Monday); // 03:00-11:00（月曜のみ）
        var slot2 = MakeSlot(11, 0, 19, 0, ScheduleDays.Monday); // 11:00-19:00（月曜のみ）
        var slot3 = MakeSlot(19, 0, 3, 0, ScheduleDays.Monday); // 19:00-翌03:00（月曜のみ）
        var now = Monday(1, 0); // 03時前

        // 月曜のみなので火曜03:00で有限終了 → 範囲表示
        ScheduleEvaluator.GetSummaryText([slot1, slot2, slot3], isLimiting: false, now)
            .Should().Be("今日 3:00 〜 明日 3:00 まで制限");
    }

    // 24時間カバー（全曜日）で制限中の場合、解除時刻が見つからず「制限中」になること
    [Fact]
    public void GetSummaryText_Limiting_FullDayCoverageAllDays_ShowsLimiting()
    {
        var allDays = ScheduleDays.Monday | ScheduleDays.Tuesday | ScheduleDays.Wednesday
                    | ScheduleDays.Thursday | ScheduleDays.Friday | ScheduleDays.Saturday | ScheduleDays.Sunday;
        var slot1 = MakeSlot(3, 0, 5, 0, allDays);
        var slot2 = MakeSlot(5, 0, 3, 0, allDays);
        var now = Monday(4, 0); // slot1 アクティブ中

        ScheduleEvaluator.GetSummaryText([slot1, slot2], isLimiting: true, now)
            .Should().Be("制限中 (終了なし)");
    }

    // ─────────────────────────────────────────────────────────
    // IsSlotActiveNow — 深夜0時境界
    // ─────────────────────────────────────────────────────────

    // 日またぎスロット: 翌日ちょうど深夜0:00 → true（endMin>0 なら継続中）
    [Fact]
    public void IsSlotActiveNow_Overnight_AtMidnight_BeforeEnd_ReturnsTrue()
    {
        // 月曜 22:00〜06:00、今は火曜 00:00（深夜0時ちょうど）
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 0, 0, 0); // Tuesday 00:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 日またぎスロット: 終了が深夜0:00 (endMin=0)、開始後 → true
    [Fact]
    public void IsSlotActiveNow_OvernightEndsAtMidnight_AfterStart_ReturnsTrue()
    {
        // 月曜 22:00〜00:00、今は月曜 23:00
        var slot = MakeSlot(22, 0, 0, 0, ScheduleDays.Monday);
        var now = Monday(23, 0);

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 日またぎスロット: 終了が深夜0:00 (endMin=0)、翌日0:00ちょうど → false（終了済み）
    [Fact]
    public void IsSlotActiveNow_OvernightEndsAtMidnight_AtMidnight_ReturnsFalse()
    {
        // 月曜 22:00〜00:00、今は火曜 00:00（終了ちょうど）
        var slot = MakeSlot(22, 0, 0, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 0, 0, 0); // Tuesday 00:00

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // IsEndTrigger — 深夜0時終了スロット (endMin=0)
    // ─────────────────────────────────────────────────────────

    // 終了が深夜0:00 の日またぎスロット: 翌日0:00ちょうど → true
    [Fact]
    public void IsEndTrigger_OvernightEndsAtMidnight_AtMidnight_ReturnsTrue()
    {
        // 月曜 22:00〜00:00、今は火曜 00:00
        var slot = MakeSlot(22, 0, 0, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 0, 0, 0); // Tuesday 00:00

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeTrue();
    }

    // 終了が深夜0:00 の日またぎスロット: 昨日が開始曜日でない → false
    [Fact]
    public void IsEndTrigger_OvernightEndsAtMidnight_WrongYesterday_ReturnsFalse()
    {
        // 火曜 22:00〜00:00、今は火曜 00:00（昨日=月曜、火曜スロットではない）
        var slot = MakeSlot(22, 0, 0, 0, ScheduleDays.Tuesday);
        var now = new DateTime(2025, 6, 10, 0, 0, 0); // Tuesday 00:00（昨日=月曜）

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // IsSlotActiveNow / IsStartTrigger / IsEndTrigger — 複数曜日フラグ
    // ─────────────────────────────────────────────────────────

    // 複数曜日フラグ: 対象曜日のいずれかに一致 → true
    [Fact]
    public void IsSlotActiveNow_MultipleDays_MatchingDay_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday | ScheduleDays.Wednesday);
        var now = Monday(12, 0); // 月曜

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeTrue();
    }

    // 複数曜日フラグ: どの曜日にも一致しない → false
    [Fact]
    public void IsSlotActiveNow_MultipleDays_NonMatchingDay_ReturnsFalse()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday | ScheduleDays.Wednesday);
        var now = new DateTime(2025, 6, 10, 12, 0, 0); // 火曜

        ScheduleEvaluator.IsSlotActiveNow(slot, now).Should().BeFalse();
    }

    // IsStartTrigger: 複数曜日フラグで対象曜日 → true
    [Fact]
    public void IsStartTrigger_MultipleDays_MatchingDay_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday | ScheduleDays.Wednesday);
        var now = Monday(9, 0);

        ScheduleEvaluator.IsStartTrigger(slot, MinOf(now), TodayFlag(now)).Should().BeTrue();
    }

    // IsEndTrigger: 複数曜日フラグで対象曜日 → true
    [Fact]
    public void IsEndTrigger_MultipleDays_MatchingDay_ReturnsTrue()
    {
        var slot = MakeSlot(9, 0, 17, 0, ScheduleDays.Monday | ScheduleDays.Wednesday);
        var now = Monday(17, 0);

        ScheduleEvaluator.IsEndTrigger(slot, MinOf(now), TodayFlag(now), YesterdayFlag(now)).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — 今日に複数スロット、先頭はすでに過ぎた
    // ─────────────────────────────────────────────────────────

    // 今日2スロット(08:00-09:00, 14:00-18:00)、今は10:00 → 先頭スロットは過去、次は14:00
    [Fact]
    public void GetSummaryText_NotLimiting_TwoSlotsToday_FirstPast_ShowsSecondSlot()
    {
        var slot1 = MakeSlot(8, 0, 9, 0, ScheduleDays.Monday);
        var slot2 = MakeSlot(14, 0, 18, 0, ScheduleDays.Monday);
        var now = Monday(10, 0);

        ScheduleEvaluator.GetSummaryText([slot1, slot2], isLimiting: false, now)
            .Should().Be("今日 14:00 〜 18:00 まで制限");
    }

    // 今日のスロットがすでに過去、7日後（来週同曜日）が次の開始になる
    [Fact]
    public void GetSummaryText_NotLimiting_TodaysSlotPast_ShowsNextWeek()
    {
        // 月曜 08:00-09:00、now = 月曜 10:00（スロットは終了済み）
        // → 7日後の月曜 08:00 が次の開始
        var slot = MakeSlot(8, 0, 9, 0, ScheduleDays.Monday);
        var now = Monday(10, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: false, now)
            .Should().Be("6/16(月) 8:00 〜 9:00 まで制限");
    }

    // ─────────────────────────────────────────────────────────
    // GetSummaryText — 制限中、日またぎスロット
    // ─────────────────────────────────────────────────────────

    // 月曜 22:00〜06:00 のスロット、今は月曜 23:00（開始後）→ 明日 6:00 に解除
    [Fact]
    public void GetSummaryText_Limiting_OvernightSlot_AfterStart_ShowsTomorrowEnd()
    {
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = Monday(23, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("明日 6:00 に解除");
    }

    // 月曜 22:00〜06:00 のスロット、今は火曜 02:00（翌日の終了前）→ 今日 6:00 に解除
    [Fact]
    public void GetSummaryText_Limiting_OvernightSlot_NextDayBeforeEnd_ShowsTodayEnd()
    {
        var slot = MakeSlot(22, 0, 6, 0, ScheduleDays.Monday);
        var now = new DateTime(2025, 6, 10, 2, 0, 0); // Tuesday 02:00

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("今日 6:00 に解除");
    }

    // 今日の解除時刻がすでに過去、7日後（来週同曜日）が次の解除になる
    [Fact]
    public void GetSummaryText_Limiting_TodaysEndPast_ShowsNextWeek()
    {
        // 月曜 09:00-10:00、now = 月曜 12:00（解除時刻の 10:00 は過去）
        // → 7日後の月曜 10:00 に解除
        var slot = MakeSlot(9, 0, 10, 0, ScheduleDays.Monday);
        var now = Monday(12, 0);

        ScheduleEvaluator.GetSummaryText([slot], isLimiting: true, now)
            .Should().Be("6/16(月) 10:00 に解除");
    }

    // ─────────────────────────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────────────────────────

    /// <summary>2025-06-09 (月曜日) の指定時刻</summary>
    private static DateTime Monday(int hour, int minute)
        => new(2025, 6, 9, hour, minute, 0); // 2025-06-09 is Monday

    private static int MinOf(DateTime dt)
        => dt.Hour * 60 + dt.Minute;

    private static ScheduleDays TodayFlag(DateTime dt)
        => ScheduleEvaluator.DayFlagOf(dt.DayOfWeek);

    private static ScheduleDays YesterdayFlag(DateTime dt)
        => ScheduleEvaluator.DayFlagOf(dt.AddDays(-1).DayOfWeek);

    private static ScheduleSlot MakeSlot(
        int startH, int startM, int endH, int endM, ScheduleDays days) => new()
        {
            Enabled = true,
            StartHour = startH,
            StartMinute = startM,
            EndHour = endH,
            EndMinute = endM,
            Days = days,
        };
}
