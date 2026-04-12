using FluentAssertions;
using Moq;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using VRCGPUTool.ViewModels;
using Xunit;

namespace VRCGPUTool.Tests;

public class UnitPriceSettingViewModelTests
{
    // ─────────────────────────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────────────────────────

    private static (UnitPriceSettingViewModel vm, Mock<IElectricityProfileService> mockService)
        CreateViewModel(ElectricityProfile? profile = null)
    {
        var mock = new Mock<IElectricityProfileService>();
        mock.Setup(s => s.SaveAsync(It.IsAny<ElectricityProfile>()))
            .Returns(Task.CompletedTask);

        profile ??= new ElectricityProfile();
        var vm = new UnitPriceSettingViewModel(mock.Object, profile);
        return (vm, mock);
    }

    // ─────────────────────────────────────────────────────────
    // コンストラクタ
    // ─────────────────────────────────────────────────────────

    // UseDayOfWeek=false → HolidayUseSameAsWeekday=true
    [Fact]
    public void Constructor_UseDayOfWeekFalse_HolidayUseSameAsWeekdayIsTrue()
    {
        var (vm, _) = CreateViewModel(new ElectricityProfile { UseDayOfWeek = false });

        vm.HolidayUseSameAsWeekday.Should().BeTrue();
    }

    // UseDayOfWeek=true → HolidayUseSameAsWeekday=false
    [Fact]
    public void Constructor_UseDayOfWeekTrue_HolidayUseSameAsWeekdayIsFalse()
    {
        var (vm, _) = CreateViewModel(new ElectricityProfile { UseDayOfWeek = true });

        vm.HolidayUseSameAsWeekday.Should().BeFalse();
    }

    // 平日スロットが WeekdayProfile に読み込まれる
    [Fact]
    public void Constructor_LoadsWeekdaySlotsIntoWeekdayProfile()
    {
        var profile = new ElectricityProfile
        {
            WeekdaySlots =
            [
                new PriceSlot { Hour = 0, UnitPrice = 20.0 },
                new PriceSlot { Hour = 8, UnitPrice = 30.0 },
            ]
        };
        var (vm, _) = CreateViewModel(profile);

        vm.WeekdayProfile.Slots.Should().Contain(s => s.Hour == 8 && s.UnitPrice == 30.0);
    }

    // 休日スロットが HolidayProfile に読み込まれる
    [Fact]
    public void Constructor_LoadsHolidaySlotsIntoHolidayProfile()
    {
        var profile = new ElectricityProfile
        {
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 18.0 }]
        };
        var (vm, _) = CreateViewModel(profile);

        vm.HolidayProfile.Slots.Should().Contain(s => s.Hour == 0 && s.UnitPrice == 18.0);
    }

    // ─────────────────────────────────────────────────────────
    // CopyWeekdayToHoliday コマンド
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void CopyWeekdayToHolidayCommand_CopiesSlotsToHolidayProfile()
    {
        var profile = new ElectricityProfile
        {
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }, new PriceSlot { Hour = 8, UnitPrice = 40.0 }],
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 10.0 }],
        };
        var (vm, _) = CreateViewModel(profile);

        vm.CopyWeekdayToHolidayCommand.Execute(null);

        vm.HolidayProfile.Slots
            .Select(s => s.Hour)
            .Should().BeEquivalentTo(vm.WeekdayProfile.Slots.Select(s => s.Hour));
        vm.HolidayProfile.Slots
            .Select(s => s.UnitPrice)
            .Should().BeEquivalentTo(vm.WeekdayProfile.Slots.Select(s => s.UnitPrice));
    }

    // ─────────────────────────────────────────────────────────
    // SaveAndCloseAsync コマンド
    // ─────────────────────────────────────────────────────────

    // window=null で呼び出しても例外なく完了し、SaveAsync が 1 回呼ばれる
    [Fact]
    public async Task SaveAndCloseCommand_NullWindow_CallsServiceSaveOnce()
    {
        var (vm, mockService) = CreateViewModel();

        await vm.SaveAndCloseCommand.ExecuteAsync(null);

        mockService.Verify(s => s.SaveAsync(It.IsAny<ElectricityProfile>()), Times.Once);
    }

    // HolidayUseSameAsWeekday=true のとき保存後 UseDayOfWeek=false になる
    [Fact]
    public async Task SaveAndCloseCommand_HolidayUseSameAsWeekday_SetsUseDayOfWeekFalse()
    {
        var profile = new ElectricityProfile();
        var (vm, _) = CreateViewModel(profile);
        vm.HolidayUseSameAsWeekday = true;

        await vm.SaveAndCloseCommand.ExecuteAsync(null);

        profile.UseDayOfWeek.Should().BeFalse();
    }

    // HolidayUseSameAsWeekday=false のとき保存後 UseDayOfWeek=true になる
    [Fact]
    public async Task SaveAndCloseCommand_NotSameAsWeekday_SetsUseDayOfWeekTrue()
    {
        var profile = new ElectricityProfile();
        var (vm, _) = CreateViewModel(profile);
        vm.HolidayUseSameAsWeekday = false;

        await vm.SaveAndCloseCommand.ExecuteAsync(null);

        profile.UseDayOfWeek.Should().BeTrue();
    }

    // HolidayUseSameAsWeekday=true のとき HolidaySlots は空になる
    [Fact]
    public async Task SaveAndCloseCommand_SameAsWeekday_ClearsHolidaySlots()
    {
        var profile = new ElectricityProfile
        {
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 18.0 }]
        };
        var (vm, _) = CreateViewModel(profile);
        vm.HolidayUseSameAsWeekday = true;

        await vm.SaveAndCloseCommand.ExecuteAsync(null);

        profile.HolidaySlots.Should().BeEmpty();
    }

    // 保存後に WeekdaySlots が VM の現在値で更新される
    [Fact]
    public async Task SaveAndCloseCommand_WritesWeekdaySlotsToProfile()
    {
        var profile = new ElectricityProfile();
        var (vm, _) = CreateViewModel(profile);

        vm.WeekdayProfile.LoadFromSlots(
        [
            new PriceSlot { Hour = 0, UnitPrice = 25.0 },
            new PriceSlot { Hour = 8, UnitPrice = 35.0 },
        ]);

        await vm.SaveAndCloseCommand.ExecuteAsync(null);

        profile.WeekdaySlots.Should().HaveCount(2);
        profile.WeekdaySlots.Should().Contain(s => s.Hour == 8 && s.UnitPrice == 35.0);
    }

    // HolidayUseSameAsWeekday=false のとき HolidaySlots が HolidayProfile の内容で保存される
    [Fact]
    public async Task SaveAndCloseCommand_NotSameAsWeekday_WritesHolidaySlotsToProfile()
    {
        var profile = new ElectricityProfile();
        var (vm, _) = CreateViewModel(profile);

        vm.HolidayUseSameAsWeekday = false;
        vm.HolidayProfile.LoadFromSlots(
        [
            new PriceSlot { Hour = 0, UnitPrice = 18.0 },
            new PriceSlot { Hour = 22, UnitPrice = 12.0 },
        ]);

        await vm.SaveAndCloseCommand.ExecuteAsync(null);

        profile.HolidaySlots.Should().HaveCount(2);
        profile.HolidaySlots.Should().Contain(s => s.Hour == 22 && s.UnitPrice == 12.0);
    }
}
