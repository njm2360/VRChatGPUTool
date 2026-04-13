using System.Text.Json;
using VRCGPUTool.Models;

namespace VRCGPUTool.Infrastructure;

internal static class ConfigMigration
{
    public static AppConfig MigrateV1ToV2(JsonElement root)
    {
        var config = new AppConfig();

        int startHour = 0, startMinute = 0, endHour = 0, endMinute = 0;

        if (root.TryGetProperty("BeginHour", out var bh) && bh.TryGetInt32(out var bhVal)) startHour = bhVal;
        if (root.TryGetProperty("BeginMinute", out var bm) && bm.TryGetInt32(out var bmVal)) startMinute = bmVal;
        if (root.TryGetProperty("EndHour", out var eh) && eh.TryGetInt32(out var ehVal)) endHour = ehVal;
        if (root.TryGetProperty("EndMinute", out var em) && em.TryGetInt32(out var emVal)) endMinute = emVal;

        config.Schedules.Add(new ScheduleSlot
        {
            StartHour = startHour,
            StartMinute = startMinute,
            EndHour = endHour,
            EndMinute = endMinute,
        });

        if (root.TryGetProperty("PowerLimitSetting", out var pl) && pl.TryGetInt32(out var plVal))
            config.PowerLimitWatts = plVal;

        if (root.TryGetProperty("UnlimitPLSetting", out var upl) && upl.TryGetInt32(out var uplVal))
            config.RestoreToWatts = uplVal;

        if (root.TryGetProperty("RestoreGPUPLDefault", out var rd)
            && (rd.ValueKind == JsonValueKind.True || rd.ValueKind == JsonValueKind.False))
            config.RestoreDefaultOnUnlimit = rd.GetBoolean();

        return config;
    }
}
