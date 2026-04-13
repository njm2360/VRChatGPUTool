using FluentAssertions;
using VRCGPUTool.Models;
using VRCGPUTool.ViewModels;
using Xunit;

namespace VRCGPUTool.Tests;

public class DayProfileViewModelTests
{
    // ─────────────────────────────────────────────────────────
    // コンストラクタ / LoadFromSlots
    // ─────────────────────────────────────────────────────────

    // デフォルトコンストラクタは hour=0 のスロットを 1 つ持つ
    [Fact]
    public void Constructor_HasSingleFixedSlotAtHour0()
    {
        var vm = new DayProfileViewModel();

        vm.Slots.Should().HaveCount(1);
        vm.Slots[0].Hour.Should().Be(0);
        vm.Slots[0].IsFixed.Should().BeTrue();
    }

    // LoadFromSlots: hour=0 のスロットが含まれていない場合は自動追加される
    [Fact]
    public void LoadFromSlots_WithoutHour0_AutoAddsHour0Slot()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots([new PriceSlot { Hour = 8, UnitPrice = 30.0 }]);

        vm.Slots.Should().HaveCount(2);
        vm.Slots.Should().Contain(s => s.Hour == 0);
    }

    // LoadFromSlots: hour=0 が含まれている場合は重複しない
    [Fact]
    public void LoadFromSlots_WithHour0_DoesNotDuplicate()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 0, UnitPrice = 20.0 },
            new PriceSlot { Hour = 8, UnitPrice = 30.0 },
        ]);

        vm.Slots.Count(s => s.Hour == 0).Should().Be(1);
    }

    // LoadFromSlots: スロットが Hour 昇順に並ぶ
    [Fact]
    public void LoadFromSlots_SortsByHourAscending()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 22, UnitPrice = 20.0 },
            new PriceSlot { Hour = 0,  UnitPrice = 15.0 },
            new PriceSlot { Hour = 8,  UnitPrice = 30.0 },
        ]);

        vm.Slots.Select(s => s.Hour).Should().BeInAscendingOrder();
    }

    // ─────────────────────────────────────────────────────────
    // ToSlots
    // ─────────────────────────────────────────────────────────

    // ToSlots は Hour 昇順で PriceSlot のリストを返す
    [Fact]
    public void ToSlots_ReturnsSortedPriceSlots()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 0,  UnitPrice = 20.0 },
            new PriceSlot { Hour = 8,  UnitPrice = 30.0 },
            new PriceSlot { Hour = 22, UnitPrice = 20.0 },
        ]);

        var slots = vm.ToSlots();

        slots.Should().HaveCount(3);
        slots.Select(s => s.Hour).Should().BeInAscendingOrder();
        slots[1].Hour.Should().Be(8);
        slots[1].UnitPrice.Should().Be(30.0);
    }

    // ─────────────────────────────────────────────────────────
    // CopyFrom
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void CopyFrom_CopiesAllSlots()
    {
        var source = new DayProfileViewModel();
        source.LoadFromSlots(
        [
            new PriceSlot { Hour = 0,  UnitPrice = 25.0 },
            new PriceSlot { Hour = 22, UnitPrice = 20.0 },
        ]);

        var dest = new DayProfileViewModel();
        dest.CopyFrom(source);

        dest.Slots.Should().HaveCount(source.Slots.Count);
        dest.Slots.Select(s => s.Hour).Should().BeEquivalentTo(source.Slots.Select(s => s.Hour));
        dest.Slots.Select(s => s.UnitPrice).Should().BeEquivalentTo(source.Slots.Select(s => s.UnitPrice));
    }

    // ─────────────────────────────────────────────────────────
    // AddSlot コマンド
    // ─────────────────────────────────────────────────────────

    // 重複しない時刻ならスロットが追加される
    [Fact]
    public void AddSlotCommand_ValidHour_AddsSlot()
    {
        var vm = new DayProfileViewModel(); // hour=0 が存在
        vm.InputHour = 8;
        vm.InputPrice = 30.0;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(2);
        vm.Slots.Should().Contain(s => s.Hour == 8 && s.UnitPrice == 30.0);
        vm.ErrorMessage.Should().BeNullOrEmpty();
    }

    // 既存と同じ時刻は追加できない
    [Fact]
    public void AddSlotCommand_DuplicateHour_SetsErrorMessage()
    {
        var vm = new DayProfileViewModel(); // hour=0 が存在
        vm.InputHour = 0; // 重複

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(1); // 増えない
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // 最大スロット数を超えると追加できない
    [Fact]
    public void AddSlotCommand_AtMaxSlots_SetsErrorMessage()
    {
        var vm = new DayProfileViewModel();

        // MaxSlots(8) 個まで埋める (hour=0 は既存)
        for (int h = 1; h < DayProfileViewModel.MaxSlots; h++)
        {
            vm.InputHour = h;
            vm.AddSlotCommand.Execute(null);
        }

        vm.Slots.Should().HaveCount(DayProfileViewModel.MaxSlots);

        // もう 1 つ追加しようとする
        vm.InputHour = DayProfileViewModel.MaxSlots;
        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(DayProfileViewModel.MaxSlots); // 増えない
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // 追加後は Hour 昇順に並び替えられる
    [Fact]
    public void AddSlotCommand_SlotsRemainSorted()
    {
        var vm = new DayProfileViewModel(); // hour=0 存在
        vm.InputHour = 12; vm.AddSlotCommand.Execute(null);
        vm.InputHour = 6; vm.AddSlotCommand.Execute(null);

        vm.Slots.Select(s => s.Hour).Should().BeInAscendingOrder();
    }

    // ─────────────────────────────────────────────────────────
    // RemoveSlot コマンド
    // ─────────────────────────────────────────────────────────

    // IsFixed=false のスロットは削除できる
    [Fact]
    public void RemoveSlotCommand_NonFixedSlot_RemovesIt()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 8;
        vm.AddSlotCommand.Execute(null);

        var target = vm.Slots.First(s => s.Hour == 8);
        vm.SelectedSlot = target;

        vm.RemoveSlotCommand.Execute(null);

        vm.Slots.Should().NotContain(s => s.Hour == 8);
    }

    // IsFixed=true (hour=0) のスロットは CanExecute が false になる
    [Fact]
    public void RemoveSlotCommand_FixedSlot_CannotExecute()
    {
        var vm = new DayProfileViewModel();
        vm.SelectedSlot = vm.Slots.First(s => s.IsFixed);

        vm.RemoveSlotCommand.CanExecute(null).Should().BeFalse();
    }

    // SelectedSlot が null なら CanExecute は false
    [Fact]
    public void RemoveSlotCommand_NullSelection_CannotExecute()
    {
        var vm = new DayProfileViewModel();
        vm.SelectedSlot = null;

        vm.RemoveSlotCommand.CanExecute(null).Should().BeFalse();
    }

    // 削除後は SelectedSlot が null になる
    [Fact]
    public void RemoveSlotCommand_AfterRemoval_SelectedSlotIsNull()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 8;
        vm.AddSlotCommand.Execute(null);

        vm.SelectedSlot = vm.Slots.First(s => s.Hour == 8);
        vm.RemoveSlotCommand.Execute(null);

        vm.SelectedSlot.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────
    // CopyFrom ディープコピー
    // ─────────────────────────────────────────────────────────

    // コピー元を変更してもコピー先に影響しない
    [Fact]
    public void CopyFrom_IsDeepCopy()
    {
        var source = new DayProfileViewModel();
        source.LoadFromSlots([new PriceSlot { Hour = 0, UnitPrice = 10.0 }]);

        var dest = new DayProfileViewModel();
        dest.CopyFrom(source);

        // コピー後にコピー元のスロットを変更する
        source.Slots[0].UnitPrice = 99.0;

        dest.Slots[0].UnitPrice.Should().Be(10.0);
    }

    // ─────────────────────────────────────────────────────────
    // HourBlocks / Legend (RefreshChart)
    // ─────────────────────────────────────────────────────────

    // HourBlocks は常に 24 エントリ
    [Fact]
    public void HourBlocks_AlwaysHas24Entries()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots([new PriceSlot { Hour = 0, UnitPrice = 15.0 }]);

        vm.HourBlocks.Should().HaveCount(24);
    }

    // Legend のエントリ数はスロット数と一致する
    [Fact]
    public void Legend_CountMatchesSlotCount()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 0,  UnitPrice = 15.0 },
            new PriceSlot { Hour = 8,  UnitPrice = 30.0 },
            new PriceSlot { Hour = 22, UnitPrice = 20.0 },
        ]);

        vm.Legend.Should().HaveCount(3);
    }

    // 各時間ブロックが正しいスロットに割り当てられる
    // スロット: hour=0(idx=0), hour=8(idx=1)
    // h=5  → hour=0 スロット (Palette[0])
    // h=8  → hour=8 スロット (Palette[1])
    // h=23 → hour=8 スロット (Palette[1])
    [Fact]
    public void HourBlocks_AssignsCorrectSlotToEachHour()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 0, UnitPrice = 15.0 },
            new PriceSlot { Hour = 8, UnitPrice = 30.0 },
        ]);

        vm.HourBlocks[5].Fill.Should().BeSameAs(DayProfileViewModel.Palette[0]);
        vm.HourBlocks[8].Fill.Should().BeSameAs(DayProfileViewModel.Palette[1]);
        vm.HourBlocks[23].Fill.Should().BeSameAs(DayProfileViewModel.Palette[1]);
    }

    // HourBlock の ToolTip に単価が含まれる
    [Fact]
    public void HourBlocks_ToolTipContainsUnitPrice()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots([new PriceSlot { Hour = 0, UnitPrice = 25.5 }]);

        vm.HourBlocks[10].ToolTip.Should().Contain("25.50");
    }

    // Legend のラベルフォーマットが正しい
    [Fact]
    public void Legend_LabelFormat_IsCorrect()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots([new PriceSlot { Hour = 8, UnitPrice = 30.0 }]);

        // hour=0 (自動追加) と hour=8 の 2 スロット
        vm.Legend.Should().Contain(l => l.Label == "0時～  0.00 円/kWh");
        vm.Legend.Should().Contain(l => l.Label == "8時～  30.00 円/kWh");
    }

    // スロットの UnitPrice を直接変更すると HourBlocks が再生成される
    [Fact]
    public void OnSlotItemChanged_UnitPriceChanged_RefreshesHourBlocks()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots([new PriceSlot { Hour = 0, UnitPrice = 10.0 }]);

        var before = vm.HourBlocks[0].ToolTip;
        vm.Slots[0].UnitPrice = 50.0;

        vm.HourBlocks[0].ToolTip.Should().NotBe(before);
        vm.HourBlocks[0].ToolTip.Should().Contain("50.00");
    }

    // ─────────────────────────────────────────────────────────
    // DataGrid 直接編集での Hour 重複防止
    // ─────────────────────────────────────────────────────────

    // DataGrid で Hour を既存スロットと同じ値に変更すると元に戻る
    [Fact]
    public void SlotHour_DirectEdit_DuplicateReverted()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 0,  UnitPrice = 10.0 },
            new PriceSlot { Hour = 8,  UnitPrice = 20.0 },
        ]);

        // hour=8 のスロットを hour=0 に変更しようとする（重複）
        var slot8 = vm.Slots.First(s => s.Hour == 8);
        slot8.Hour = 0;

        // 元の 8 に戻っているはず
        slot8.Hour.Should().Be(8);
        vm.Slots.Select(s => s.Hour).Should().OnlyHaveUniqueItems();
    }

    // DataGrid で Hour を重複しない値に変更すると受け入れられる
    [Fact]
    public void SlotHour_DirectEdit_NonDuplicateAccepted()
    {
        var vm = new DayProfileViewModel();
        vm.LoadFromSlots(
        [
            new PriceSlot { Hour = 0, UnitPrice = 10.0 },
            new PriceSlot { Hour = 8, UnitPrice = 20.0 },
        ]);

        var slot8 = vm.Slots.First(s => s.Hour == 8);
        slot8.Hour = 12;

        slot8.Hour.Should().Be(12);
        vm.Slots.Select(s => s.Hour).Should().OnlyHaveUniqueItems();
    }

    // ─────────────────────────────────────────────────────────
    // 開始時バリデーション (0〜23)
    // ─────────────────────────────────────────────────────────

    // AddSlot: 負数はエラーになりスロットが追加されない
    [Fact]
    public void AddSlotCommand_NegativeHour_SetsErrorAndDoesNotAdd()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = -1;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(1);
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // AddSlot: 24 以上はエラーになりスロットが追加されない
    [Fact]
    public void AddSlotCommand_HourAbove23_SetsErrorAndDoesNotAdd()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 24;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(1);
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // AddSlot: 23 は追加できる (上限境界)
    [Fact]
    public void AddSlotCommand_HourAt23_AddsSlot()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 23;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(2);
        vm.Slots.Should().Contain(s => s.Hour == 23);
        vm.ErrorMessage.Should().BeNullOrEmpty();
    }

    // PriceSlotItem: 負数を代入すると 0 に丸められる
    [Fact]
    public void PriceSlotItem_NegativeHour_ClampedToMin()
    {
        var item = new PriceSlotItem();
        item.Hour = -131;

        item.Hour.Should().Be(PriceSlotItem.MinHour);
    }

    // PriceSlotItem: 24 以上を代入すると 23 に丸められる
    [Fact]
    public void PriceSlotItem_HourAboveMax_ClampedToMax()
    {
        var item = new PriceSlotItem();
        item.Hour = 13131310;

        item.Hour.Should().Be(PriceSlotItem.MaxHour);
    }

    // PriceSlotItem: IsFixed=true のスロットは Hour が丸められない
    [Fact]
    public void PriceSlotItem_FixedSlot_HourNotClamped()
    {
        var item = new PriceSlotItem { IsFixed = true, Hour = 0 };
        item.Hour = 0; // 固定スロットは 0 のまま (丸め処理をスキップ)

        item.Hour.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────
    // 単価上限 (100 円/kWh)
    // ─────────────────────────────────────────────────────────

    // AddSlot: 負数単価はエラーになりスロットが追加されない
    [Fact]
    public void AddSlotCommand_NegativePrice_SetsErrorAndDoesNotAdd()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 8;
        vm.InputPrice = -1.0;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(1);
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // AddSlot: 0 円は追加できる (下限境界)
    [Fact]
    public void AddSlotCommand_PriceAtZero_AddsSlot()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 8;
        vm.InputPrice = 0.0;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(2);
        vm.Slots.Should().Contain(s => s.Hour == 8 && s.UnitPrice == 0.0);
        vm.ErrorMessage.Should().BeNullOrEmpty();
    }

    // PriceSlotItem: 負数を代入すると 0 に丸められる
    [Fact]
    public void PriceSlotItem_NegativeUnitPrice_ClampedToMin()
    {
        var item = new PriceSlotItem();
        item.UnitPrice = -50.0;

        item.UnitPrice.Should().Be(PriceSlotItem.MinUnitPrice);
    }

    // AddSlot: 100 円超はエラーになりスロットが追加されない
    [Fact]
    public void AddSlotCommand_PriceExceedsMax_SetsErrorAndDoesNotAdd()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 8;
        vm.InputPrice = 100.01;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(1);
        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // AddSlot: ちょうど 100 円は追加できる
    [Fact]
    public void AddSlotCommand_PriceAtMax_AddsSlot()
    {
        var vm = new DayProfileViewModel();
        vm.InputHour = 8;
        vm.InputPrice = 100.0;

        vm.AddSlotCommand.Execute(null);

        vm.Slots.Should().HaveCount(2);
        vm.Slots.Should().Contain(s => s.Hour == 8 && s.UnitPrice == 100.0);
        vm.ErrorMessage.Should().BeNullOrEmpty();
    }

    // PriceSlotItem: 100 円超を直接代入すると 100 に丸められる
    [Fact]
    public void PriceSlotItem_UnitPriceAboveMax_ClampedToMax()
    {
        var item = new PriceSlotItem();
        item.UnitPrice = 150.0;

        item.UnitPrice.Should().Be(PriceSlotItem.MaxUnitPrice);
    }

    // PriceSlotItem: ちょうど 100 円は丸められない
    [Fact]
    public void PriceSlotItem_UnitPriceAtMax_NotClamped()
    {
        var item = new PriceSlotItem();
        item.UnitPrice = 100.0;

        item.UnitPrice.Should().Be(100.0);
    }
}
