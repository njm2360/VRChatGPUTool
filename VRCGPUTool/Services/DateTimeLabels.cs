namespace VRCGPUTool.Services;

public static class DateTimeLabels
{
    public static string DayLabel(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Monday => "月",
        DayOfWeek.Tuesday => "火",
        DayOfWeek.Wednesday => "水",
        DayOfWeek.Thursday => "木",
        DayOfWeek.Friday => "金",
        DayOfWeek.Saturday => "土",
        DayOfWeek.Sunday => "日",
        _ => "",
    };
}
