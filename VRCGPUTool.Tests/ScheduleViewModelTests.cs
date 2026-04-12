using FluentAssertions;
using VRCGPUTool.Models;
using VRCGPUTool.ViewModels;
using Xunit;

namespace VRCGPUTool.Tests;

// ─────────────────────────────────────────────────────────
// ScheduleSlotViewModel
// ─────────────────────────────────────────────────────────
public class ScheduleSlotViewModelTests
{
    // モデル → ViewModel へすべてのフィールドが正しくマップされる
    [Fact]
    public void Constructor_MapsAllFieldsFromModel()
    {
        var slot = new ScheduleSlot
        {
            Enabled = true,
            StartHour = 8,
            StartMinute = 30,
            EndHour = 22,
            EndMinute = 15,
            Days = ScheduleDays.Monday | ScheduleDays.Wednesday | ScheduleDays.Friday,
        };

        var vm = new ScheduleSlotViewModel(slot);

        vm.Enabled.Should().BeTrue();
        vm.StartHour.Should().Be(8);
        vm.StartMinute.Should().Be(30);
        vm.EndHour.Should().Be(22);
        vm.EndMinute.Should().Be(15);
    }

    // 曜日フラグが bool プロパティに正しく展開される
    [Fact]
    public void Constructor_MapsDaysFlagsToProperties()
    {
        var slot = new ScheduleSlot
        {
            Days = ScheduleDays.Monday | ScheduleDays.Wednesday | ScheduleDays.Saturday,
        };

        var vm = new ScheduleSlotViewModel(slot);

        vm.Monday.Should().BeTrue();
        vm.Tuesday.Should().BeFalse();
        vm.Wednesday.Should().BeTrue();
        vm.Thursday.Should().BeFalse();
        vm.Friday.Should().BeFalse();
        vm.Saturday.Should().BeTrue();
        vm.Sunday.Should().BeFalse();
    }

    // 曜日なし（None）のスロットはすべて false
    [Fact]
    public void Constructor_AllDaysFalse_WhenNone()
    {
        var vm = new ScheduleSlotViewModel(new ScheduleSlot { Days = ScheduleDays.None });

        vm.Monday.Should().BeFalse();
        vm.Sunday.Should().BeFalse();
    }

    // ViewModel → モデル への変換が正確に行われる
    [Fact]
    public void ToModel_PreservesAllFields()
    {
        var original = new ScheduleSlot
        {
            Enabled = false,
            StartHour = 0,
            StartMinute = 0,
            EndHour = 23,
            EndMinute = 59,
            Days = ScheduleDays.Saturday | ScheduleDays.Sunday,
        };

        var restored = new ScheduleSlotViewModel(original).ToModel();

        restored.Enabled.Should().BeFalse();
        restored.StartHour.Should().Be(0);
        restored.StartMinute.Should().Be(0);
        restored.EndHour.Should().Be(23);
        restored.EndMinute.Should().Be(59);
        restored.Days.Should().Be(ScheduleDays.Saturday | ScheduleDays.Sunday);
    }

    // モデル → VM → モデル のラウンドトリップで値が保たれる
    [Theory]
    [InlineData(ScheduleDays.Monday)]
    [InlineData(ScheduleDays.Monday | ScheduleDays.Friday | ScheduleDays.Sunday)]
    [InlineData(ScheduleDays.None)]
    public void ToModel_RoundtripPreservesDays(ScheduleDays days)
    {
        var original = new ScheduleSlot { Days = days };

        new ScheduleSlotViewModel(original).ToModel().Days.Should().Be(days);
    }

    // 開始と終了が異なる場合は IsValid = true
    [Theory]
    [InlineData(9, 0, 17, 0)]
    [InlineData(0, 0, 0, 1)]
    [InlineData(23, 59, 0, 0)]
    public void IsValid_StartDiffersFromEnd_ReturnsTrue(int sh, int sm, int eh, int em)
    {
        var vm = new ScheduleSlotViewModel(new ScheduleSlot
        { StartHour = sh, StartMinute = sm, EndHour = eh, EndMinute = em });

        vm.IsValid.Should().BeTrue();
    }

    // 開始と終了が同じ場合は IsValid = false
    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 30)]
    [InlineData(23, 59)]
    public void IsValid_StartEqualsEnd_ReturnsFalse(int hour, int minute)
    {
        var vm = new ScheduleSlotViewModel(new ScheduleSlot
        { StartHour = hour, StartMinute = minute, EndHour = hour, EndMinute = minute });

        vm.IsValid.Should().BeFalse();
    }

    // プロパティ変更で IsValid が再評価される
    [Fact]
    public void IsValid_ChangesToFalse_WhenEndSetToMatchStart()
    {
        var vm = new ScheduleSlotViewModel(new ScheduleSlot
        { StartHour = 9, StartMinute = 0, EndHour = 17, EndMinute = 0 });

        vm.IsValid.Should().BeTrue();

        vm.EndHour = 9;
        vm.EndMinute = 0;

        vm.IsValid.Should().BeFalse();
    }

    // StartHour/StartMinute 変更でも IsValid が再評価される
    [Fact]
    public void IsValid_ChangesToFalse_WhenStartSetToMatchEnd()
    {
        var vm = new ScheduleSlotViewModel(new ScheduleSlot
        { StartHour = 9, StartMinute = 0, EndHour = 17, EndMinute = 0 });

        vm.IsValid.Should().BeTrue();

        vm.StartHour = 17;
        vm.StartMinute = 0;

        vm.IsValid.Should().BeFalse();
    }

    // デフォルトコンストラクタは 0:00 == 0:00 なので IsValid = false
    [Fact]
    public void DefaultConstructor_IsValid_False()
    {
        var vm = new ScheduleSlotViewModel();

        vm.IsValid.Should().BeFalse();
    }

    // 全曜日フラグがすべての bool プロパティに展開される
    [Fact]
    public void Constructor_AllDaysSet_AllPropertiesTrue()
    {
        var allDays = ScheduleDays.Monday | ScheduleDays.Tuesday | ScheduleDays.Wednesday
                    | ScheduleDays.Thursday | ScheduleDays.Friday
                    | ScheduleDays.Saturday | ScheduleDays.Sunday;
        var vm = new ScheduleSlotViewModel(new ScheduleSlot { Days = allDays });

        vm.Monday.Should().BeTrue();
        vm.Tuesday.Should().BeTrue();
        vm.Wednesday.Should().BeTrue();
        vm.Thursday.Should().BeTrue();
        vm.Friday.Should().BeTrue();
        vm.Saturday.Should().BeTrue();
        vm.Sunday.Should().BeTrue();
    }

    // 構築後に曜日 bool を変更すると ToModel() に反映される
    [Fact]
    public void ToModel_ReflectsPropertyChangesAfterConstruction()
    {
        var vm = new ScheduleSlotViewModel(new ScheduleSlot { Days = ScheduleDays.None })
        {
            Monday = true,
            Wednesday = true,
        };

        var model = vm.ToModel();
        model.Days.Should().HaveFlag(ScheduleDays.Monday);
        model.Days.Should().HaveFlag(ScheduleDays.Wednesday);
        model.Days.Should().NotHaveFlag(ScheduleDays.Tuesday);
        model.Days.Should().NotHaveFlag(ScheduleDays.Friday);
    }
}

// ─────────────────────────────────────────────────────────
// ScheduleSettingViewModel
// ─────────────────────────────────────────────────────────
public class ScheduleSettingViewModelTests
{
    [Fact]
    public void Constructor_EmptyList_CreatesEmptySlots()
    {
        var vm = new ScheduleSettingViewModel([]);

        vm.Slots.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithSlots_CreatesCorrectCount()
    {
        var slots = new[]
        {
            new ScheduleSlot { StartHour = 8 },
            new ScheduleSlot { StartHour = 22 },
        };

        var vm = new ScheduleSettingViewModel(slots);

        vm.Slots.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_SlotsHaveCorrectValues()
    {
        var slot = new ScheduleSlot
        {
            StartHour = 9,
            StartMinute = 0,
            EndHour = 18,
            EndMinute = 30,
            Days = ScheduleDays.Monday | ScheduleDays.Tuesday,
        };

        var vm = new ScheduleSettingViewModel([slot]);

        var slotVm = vm.Slots[0];
        slotVm.StartHour.Should().Be(9);
        slotVm.EndHour.Should().Be(18);
        slotVm.Monday.Should().BeTrue();
        slotVm.Tuesday.Should().BeTrue();
        slotVm.Wednesday.Should().BeFalse();
    }

    [Fact]
    public void AddSlotCommand_IncreasesSlotCount()
    {
        var vm = new ScheduleSettingViewModel([]);

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSlotCommand_DecreasesSlotCount()
    {
        var vm = new ScheduleSettingViewModel([new ScheduleSlot()]);
        var target = vm.Slots[0];

        vm.RemoveSlotCommand.Execute(target);

        vm.Slots.Should().BeEmpty();
    }

    // 全スロットが有効なら SaveCommand が実行可能
    [Fact]
    public void SaveCommand_CanExecute_WhenAllSlotsValid()
    {
        var slots = new[]
        {
            new ScheduleSlot { StartHour = 9,  EndHour = 17 },
            new ScheduleSlot { StartHour = 20, EndHour = 22 },
        };
        var vm = new ScheduleSettingViewModel(slots);

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    // 開始=終了のスロットがあれば SaveCommand が実行不可
    [Fact]
    public void SaveCommand_CannotExecute_WhenAnySlotInvalid()
    {
        var slots = new[]
        {
            new ScheduleSlot { StartHour = 9, EndHour = 17 },
            new ScheduleSlot { StartHour = 10, EndHour = 10 }, // 開始=終了
        };
        var vm = new ScheduleSettingViewModel(slots);

        vm.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    // スロット追加後に時刻を揃えると SaveCommand が実行不可になる
    [Fact]
    public void SaveCommand_BecomesInvalid_WhenSlotTimesSetEqual()
    {
        var vm = new ScheduleSettingViewModel([new ScheduleSlot { StartHour = 9, EndHour = 17 }]);
        vm.SaveCommand.CanExecute(null).Should().BeTrue();

        vm.Slots[0].EndHour = 9;
        vm.Slots[0].EndMinute = 0;

        vm.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    // 無効なスロットを修正すると SaveCommand が再び実行可能になる
    [Fact]
    public void SaveCommand_BecomesValid_AfterFixingInvalidSlot()
    {
        var vm = new ScheduleSettingViewModel([new ScheduleSlot { StartHour = 10, EndHour = 10 }]);
        vm.SaveCommand.CanExecute(null).Should().BeFalse();

        vm.Slots[0].EndHour = 18;

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    // スロットがゼロ件のとき SaveCommand は実行可能
    [Fact]
    public void SaveCommand_CanExecute_WhenNoSlots()
    {
        var vm = new ScheduleSettingViewModel([]);

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void GetSlots_ReturnsAllSlotsAsModels()
    {
        var slots = new[]
        {
            new ScheduleSlot { StartHour = 8,  Days = ScheduleDays.Monday },
            new ScheduleSlot { StartHour = 22, Days = ScheduleDays.Friday },
        };
        var vm = new ScheduleSettingViewModel(slots);

        var result = vm.GetSlots();

        result.Should().HaveCount(2);
        result[0].StartHour.Should().Be(8);
        result[1].StartHour.Should().Be(22);
    }

    // 無効スロットを削除すると SaveCommand が実行可能に戻る
    [Fact]
    public void SaveCommand_BecomesValid_WhenInvalidSlotIsRemoved()
    {
        var vm = new ScheduleSettingViewModel([new ScheduleSlot { StartHour = 10, EndHour = 10 }]);
        vm.SaveCommand.CanExecute(null).Should().BeFalse();

        vm.RemoveSlotCommand.Execute(vm.Slots[0]);

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    // AddSlot 後のスロットは IsValid = true（開始と終了が 1 時間ずれるため）
    [Fact]
    public void AddSlotCommand_NewSlotIsValid()
    {
        var vm = new ScheduleSettingViewModel([]);

        vm.AddSlotCommand.Execute(null);

        vm.Slots[0].IsValid.Should().BeTrue();
    }

    // 構築後にスロットを変更して GetSlots() に反映される
    [Fact]
    public void GetSlots_ReflectsPropertyChangesAfterConstruction()
    {
        var vm = new ScheduleSettingViewModel([new ScheduleSlot { StartHour = 9, EndHour = 17, Days = ScheduleDays.Monday }]);

        vm.Slots[0].StartHour = 10;
        vm.Slots[0].Friday = true;

        var result = vm.GetSlots();
        result[0].StartHour.Should().Be(10);
        result[0].Days.Should().HaveFlag(ScheduleDays.Monday);
        result[0].Days.Should().HaveFlag(ScheduleDays.Friday);
    }

    // ─────────────────────────────────────────────────────────
    // スロット上限 (20 個)
    // ─────────────────────────────────────────────────────────

    // MaxSlots 個未満のときは AddSlotCommand が実行可能
    [Fact]
    public void AddSlotCommand_CanExecute_WhenBelowMax()
    {
        var vm = new ScheduleSettingViewModel([]);

        vm.AddSlotCommand.CanExecute(null).Should().BeTrue();
    }

    // MaxSlots 個に達したら AddSlotCommand が実行不可
    [Fact]
    public void AddSlotCommand_CannotExecute_WhenAtMaxSlots()
    {
        var slots = Enumerable.Repeat(new ScheduleSlot { StartHour = 9, EndHour = 17 }, ScheduleSettingViewModel.MaxSlots);
        var vm = new ScheduleSettingViewModel(slots);

        vm.AddSlotCommand.CanExecute(null).Should().BeFalse();
    }

    // MaxSlots 個に達した状態で Execute しても増えない
    [Fact]
    public void AddSlotCommand_AtMaxSlots_DoesNotAdd()
    {
        var slots = Enumerable.Repeat(new ScheduleSlot { StartHour = 9, EndHour = 17 }, ScheduleSettingViewModel.MaxSlots);
        var vm = new ScheduleSettingViewModel(slots);

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(ScheduleSettingViewModel.MaxSlots);
    }

    // 上限に達した後スロットを1つ削除すると AddSlotCommand が再び実行可能
    [Fact]
    public void AddSlotCommand_CanExecute_AfterRemovingFromMax()
    {
        var slots = Enumerable.Repeat(new ScheduleSlot { StartHour = 9, EndHour = 17 }, ScheduleSettingViewModel.MaxSlots);
        var vm = new ScheduleSettingViewModel(slots);

        vm.RemoveSlotCommand.Execute(vm.Slots[0]);

        vm.AddSlotCommand.CanExecute(null).Should().BeTrue();
    }
}
