namespace ProductionDashboard.Models
{
    public class ProductionMetricold
    {
        public int Id { get; set; }
        public string Product { get; set; } = "";
        public string Shift { get; set; } = "";   // "Morning","Afternoon","Night"
        public int HourSlot { get; set; }          // 1-8
        public decimal FCT1_Target { get; set; }
        public decimal FCT1_Actual { get; set; }
        public decimal FCT2_Target { get; set; }
        public decimal FCT2_Actual { get; set; }
        public decimal FCT3_Target { get; set; }
        public decimal FCT3_Actual { get; set; }
        public decimal RF1_Target { get; set; }
        public decimal RF1_Actual { get; set; }
        public decimal RF2_Target { get; set; }
        public decimal RF2_Actual { get; set; }
        public decimal RTC1_Target { get; set; }
        public decimal RTC1_Actual { get; set; }
        public decimal VOL1_Target { get; set; }
        public decimal VOL1_Actual { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public class DashboardViewModel
    {
        public List<string> Products { get; set; } = new();
        public string SelectedProduct { get; set; } = "";
        public List<ProductionMetricold> MorningShift { get; set; } = new();
        public List<ProductionMetricold> AfternoonShift { get; set; } = new();
        public List<ProductionMetricold> NightShift { get; set; } = new();
    }

    public static class ShiftConfig
    {
        public static readonly Dictionary<string, (string Label, string Start, string End)> Shifts = new()
        {
            ["Night"]     = ("Night Shift",     "8:00 PM",  "4:00 AM"),
            ["Morning"]   = ("Morning Shift",   "4:00 AM",  "12:00 PM"),
            ["Afternoon"] = ("Afternoon Shift", "12:00 PM", "8:00 PM"),
        };
    }
}
