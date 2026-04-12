namespace VRCGPUTool.Models;

public sealed class PriceSlot
{
    public int Hour { get; set; }
    public double UnitPrice { get; set; }
}

public sealed class ElectricityProfile
{
    public bool UseDayOfWeek { get; set; } = false;
    public List<PriceSlot> WeekdaySlots { get; set; } = [];
    public List<PriceSlot> HolidaySlots { get; set; } = [];

    /// <summary>
    /// 各時 (0-23) の単価 (円/kWh) を返す。
    /// UseDayOfWeek=true のとき isWeekday で平日・休日を切り替える。
    /// </summary>
    public double[] ComputeHourlyPrices(bool isWeekday)
    {
        var slots = (UseDayOfWeek && !isWeekday) ? HolidaySlots : WeekdaySlots;
        return ComputeFromSlots(slots);
    }

    private static double[] ComputeFromSlots(List<PriceSlot> slots)
    {
        double[] prices = new double[24];
        if (slots.Count == 0) return prices;

        var sorted = slots.OrderBy(s => s.Hour).ToArray();

        for (int h = 0; h < 24; h++)
        {
            double u = 0;
            foreach (var slot in sorted)
            {
                if (slot.Hour <= h) u = slot.UnitPrice;
                else break;
            }
            prices[h] = u;
        }

        return prices;
    }
}
