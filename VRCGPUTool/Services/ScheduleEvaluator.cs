using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public static class ScheduleEvaluator
{
    // ─────────────────────────────────────────
    // 曜日変換
    // ─────────────────────────────────────────

    public static ScheduleDays DayFlagOf(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Monday => ScheduleDays.Monday,
        DayOfWeek.Tuesday => ScheduleDays.Tuesday,
        DayOfWeek.Wednesday => ScheduleDays.Wednesday,
        DayOfWeek.Thursday => ScheduleDays.Thursday,
        DayOfWeek.Friday => ScheduleDays.Friday,
        DayOfWeek.Saturday => ScheduleDays.Saturday,
        DayOfWeek.Sunday => ScheduleDays.Sunday,
        _ => ScheduleDays.None,
    };

    // ─────────────────────────────────────────
    // 表示ラベル
    // ─────────────────────────────────────────

    public static string DateLabel(DateTime dt, DateTime now)
    {
        var today = now.Date;
        if (dt.Date == today) return "今日";
        if (dt.Date == today.AddDays(1)) return "明日";
        return $"{dt.Month}/{dt.Day}({DateTimeLabels.DayLabel(dt.DayOfWeek)})";
    }

    // ─────────────────────────────────────────
    // スロット判定
    // ─────────────────────────────────────────

    public static bool IsSlotActiveNow(ScheduleSlot slot, DateTime now)
    {
        int currentMin = now.Hour * 60 + now.Minute;
        int startMin = slot.StartHour * 60 + slot.StartMinute;
        int endMin = slot.EndHour * 60 + slot.EndMinute;

        if (startMin == endMin) return false;

        if (startMin < endMin)
        {
            // 同日内スロット
            return slot.Days.HasFlag(DayFlagOf(now.DayOfWeek))
                && currentMin >= startMin && currentMin < endMin;
        }
        else
        {
            // 日をまたぐスロット
            if (currentMin >= startMin)
                return slot.Days.HasFlag(DayFlagOf(now.DayOfWeek));         // 今日が開始日
            else
                return currentMin < endMin
                    && slot.Days.HasFlag(DayFlagOf(now.AddDays(-1).DayOfWeek)); // 昨日が開始日
        }
    }

    public static bool IsStartTrigger(ScheduleSlot slot, int currentMin, ScheduleDays today)
    {
        int startMin = slot.StartHour * 60 + slot.StartMinute;
        int endMin = slot.EndHour * 60 + slot.EndMinute;

        if (startMin == endMin || startMin != currentMin) return false;

        return slot.Days.HasFlag(today);
    }

    public static bool IsEndTrigger(ScheduleSlot slot, int currentMin, ScheduleDays today, ScheduleDays yesterday)
    {
        int startMin = slot.StartHour * 60 + slot.StartMinute;
        int endMin = slot.EndHour * 60 + slot.EndMinute;

        if (startMin == endMin || endMin != currentMin) return false;

        var checkDay = startMin > endMin ? yesterday : today;
        return slot.Days.HasFlag(checkDay);
    }

    // ─────────────────────────────────────────
    // サマリーテキスト生成
    // ─────────────────────────────────────────

    public static string GetSummaryText(IReadOnlyList<ScheduleSlot> schedules, bool isLimiting, DateTime now)
    {
        var enabledSlots = schedules.Where(s => s.Enabled).ToList();
        if (enabledSlots.Count == 0)
            return "スケジュールなし";

        if (isLimiting)
            return BuildLimitingText(enabledSlots, now);
        else
            return BuildNextSlotText(enabledSlots, now);
    }

    private static (ScheduleSlot Slot, DateTime Dt)? FindFirstUpcoming(
        List<ScheduleSlot> enabledSlots,
        DateTime now,
        Func<ScheduleSlot, DateTime, DateTime> timeSelector)
    {
        for (int offset = 0; offset <= 7; offset++)
        {
            var date = now.Date.AddDays(offset);
            var dayFlag = DayFlagOf(date.DayOfWeek);

            var found = enabledSlots
                .Where(s => s.Days.HasFlag(dayFlag))
                .Select(s => (Slot: s, Dt: timeSelector(s, date)))
                .Where(x => x.Dt > now)
                .OrderBy(x => x.Dt)
                .Cast<(ScheduleSlot, DateTime)?>()
                .FirstOrDefault();

            if (found is { } result) return result;
        }
        return null;
    }

    private static string BuildLimitingText(List<ScheduleSlot> enabledSlots, DateTime now)
    {
        for (int offset = 0; offset <= 7; offset++)
        {
            var date = now.Date.AddDays(offset);
            var dayFlag = DayFlagOf(date.DayOfWeek);
            var yesterdayFlag = DayFlagOf(date.AddDays(-1).DayOfWeek);

            var endTimes = enabledSlots
                .SelectMany(s => CandidateEndTimes(s, date, dayFlag, yesterdayFlag))
                .Where(dt => dt > now)
                .Distinct()
                .Order();

            foreach (var endDt in endTimes)
            {
                if (!enabledSlots.Any(s => IsSlotActiveNow(s, endDt)))
                    return $"{DateLabel(endDt, now)} {endDt:H:mm} に解除";
            }
        }
        return "制限中 (終了なし)";
    }

    private static IEnumerable<DateTime> CandidateEndTimes(
        ScheduleSlot s, DateTime date, ScheduleDays dayFlag, ScheduleDays yesterdayFlag)
    {
        int startMin = s.StartHour * 60 + s.StartMinute;
        int endMin = s.EndHour * 60 + s.EndMinute;
        bool overnight = startMin > endMin;

        if (!overnight && s.Days.HasFlag(dayFlag))
            yield return date.AddHours(s.EndHour).AddMinutes(s.EndMinute);

        if (overnight && s.Days.HasFlag(dayFlag))
            yield return date.AddDays(1).AddHours(s.EndHour).AddMinutes(s.EndMinute);

        if (overnight && s.Days.HasFlag(yesterdayFlag))
            yield return date.AddHours(s.EndHour).AddMinutes(s.EndMinute);
    }

    private static string BuildNextSlotText(List<ScheduleSlot> enabledSlots, DateTime now)
    {
        var found = FindFirstUpcoming(enabledSlots, now,
            (s, date) => date.AddHours(s.StartHour).AddMinutes(s.StartMinute));

        if (found is not { } f) return "スケジュールなし";

        var (slot, startDt) = f;

        bool sameTime = slot.StartHour * 60 + slot.StartMinute
                     == slot.EndHour * 60 + slot.EndMinute;
        if (sameTime)
            return $"{DateLabel(startDt, now)} {startDt:H:mm} から制限 (終了なし)";

        var date2 = startDt.Date;
        var endBase = date2.AddHours(slot.EndHour).AddMinutes(slot.EndMinute);
        var endDt = endBase > startDt ? endBase : endBase.AddDays(1);

        // 終了時刻にアクティブな別スロットがあれば（連続・重複）終端を延伸する。
        var ceiling = now.AddDays(8);
        endDt = ExtendEndTime(enabledSlots, endDt, ceiling);

        if (enabledSlots.Any(s => IsSlotActiveNow(s, endDt)))
            return $"{DateLabel(startDt, now)} {startDt:H:mm} から制限 (終了なし)";

        var startLabel = DateLabel(startDt, now);
        var endLabel = DateLabel(endDt, now);

        return startLabel == endLabel
            ? $"{startLabel} {startDt:H:mm} 〜 {endDt:H:mm} まで制限"
            : $"{startLabel} {startDt:H:mm} 〜 {endLabel} {endDt:H:mm} まで制限";
    }

    // 制限ブロックの終端を、連続・重複するスロットが尽きるまで延伸する。
    // ceiling を超える延伸は行わない（無限ループ防止）。
    private static DateTime ExtendEndTime(List<ScheduleSlot> enabledSlots, DateTime endDt, DateTime ceiling)
    {
        bool extended;
        do
        {
            extended = false;
            foreach (var s in enabledSlots)
            {
                if (!IsSlotActiveNow(s, endDt)) continue;

                var slotEndBase = endDt.Date.AddHours(s.EndHour).AddMinutes(s.EndMinute);
                var slotEnd = slotEndBase > endDt ? slotEndBase : slotEndBase.AddDays(1);

                if (slotEnd > endDt && slotEnd <= ceiling)
                {
                    endDt = slotEnd;
                    extended = true;
                    break;
                }
            }
        }
        while (extended);

        return endDt;
    }
}
