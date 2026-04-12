using System.Text.Json;
using FluentAssertions;
using VRCGPUTool.Infrastructure;
using Xunit;

namespace VRCGPUTool.Tests;

public class ConfigMigrationTests
{
    private static JsonElement ParseJson(string json) =>
        JsonDocument.Parse(json).RootElement;

    // ─────────────────────────────────────────────────────────
    // スケジュール時刻の移行
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void MigrateV1ToV2_MapsScheduleTimes()
    {
        var root = ParseJson("""
            {
                "BeginHour": 9, "BeginMinute": 30,
                "EndHour": 22,  "EndMinute": 15
            }
            """);

        var config = ConfigMigration.MigrateV1ToV2(root);

        config.Schedules.Should().HaveCount(1);
        var slot = config.Schedules[0];
        slot.StartHour.Should().Be(9);
        slot.StartMinute.Should().Be(30);
        slot.EndHour.Should().Be(22);
        slot.EndMinute.Should().Be(15);
    }

    // ─────────────────────────────────────────────────────────
    // 電力制限値の移行
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void MigrateV1ToV2_MapsPowerLimitWatts()
    {
        var root = ParseJson("""{ "PowerLimitSetting": 250 }""");

        var config = ConfigMigration.MigrateV1ToV2(root);

        config.PowerLimitWatts.Should().Be(250);
    }

    [Fact]
    public void MigrateV1ToV2_MapsRestoreToWatts()
    {
        var root = ParseJson("""{ "UnlimitPLSetting": 320 }""");

        var config = ConfigMigration.MigrateV1ToV2(root);

        config.RestoreToWatts.Should().Be(320);
    }

    // ─────────────────────────────────────────────────────────
    // RestoreGPUPLDefault フラグの移行
    // ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MigrateV1ToV2_MapsRestoreDefaultOnUnlimit(bool value)
    {
        var root = ParseJson($$"""{ "RestoreGPUPLDefault": {{value.ToString().ToLower()}} }""");

        var config = ConfigMigration.MigrateV1ToV2(root);

        config.RestoreDefaultOnUnlimit.Should().Be(value);
    }

    // ─────────────────────────────────────────────────────────
    // 存在しないキーはデフォルト値のまま
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void MigrateV1ToV2_MissingKeys_UsesDefaults()
    {
        var root = ParseJson("{}");

        var config = ConfigMigration.MigrateV1ToV2(root);

        // スケジュールのデフォルト時刻は 0
        config.Schedules.Should().HaveCount(1);
        config.Schedules[0].StartHour.Should().Be(0);
        config.Schedules[0].EndHour.Should().Be(0);

        // AppConfig のデフォルト値が使われる
        config.PowerLimitWatts.Should().Be(new VRCGPUTool.Models.AppConfig().PowerLimitWatts);
        config.RestoreDefaultOnUnlimit.Should().Be(new VRCGPUTool.Models.AppConfig().RestoreDefaultOnUnlimit);
    }

    // 全フィールドがそろった完全な v1 JSON を移行できる
    [Fact]
    public void MigrateV1ToV2_FullV1Json_MapsAllFields()
    {
        var root = ParseJson("""
            {
                "BeginHour": 8,  "BeginMinute": 0,
                "EndHour": 23,   "EndMinute": 0,
                "PowerLimitSetting": 200,
                "UnlimitPLSetting": 320,
                "RestoreGPUPLDefault": false
            }
            """);

        var config = ConfigMigration.MigrateV1ToV2(root);

        config.Schedules[0].StartHour.Should().Be(8);
        config.Schedules[0].EndHour.Should().Be(23);
        config.PowerLimitWatts.Should().Be(200);
        config.RestoreToWatts.Should().Be(320);
        config.RestoreDefaultOnUnlimit.Should().BeFalse();
    }
}
