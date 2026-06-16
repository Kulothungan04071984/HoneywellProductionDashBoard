
namespace ProductionDashboard.Models
{
    public class ProductionMetric
    {
        //public int Id { get; set; }
        //public string Product { get; set; } = "";
        //public string Shift { get; set; } = "";   // "Morning","Afternoon","Night"
        //public int HourSlot { get; set; }          // 1-8
        public decimal FCT1_Target { get; set; }
        public decimal FCT1_Actual { get; set; }
        public decimal FCT2_Target { get; set; }
        public decimal FCT2_Actual { get; set; }
        public DateTime RecordedAt { get; set; }

        //public string HourRange { get; set; }

        //public string TesterGroup { get; set; }

        //public decimal PassQty { get; set; }

        //public decimal FailQty { get; set; }

        //public decimal Actual { get; set; }

        //public decimal Target { get; set; }

        //public decimal Gap { get; set; }

        //public decimal LossPercent { get; set; }
        public int Id { get; set; }
        public string Product { get; set; } = "";

        // Map SP column names exactly
        public string HourRange { get; set; } = "";   // HOURINTERVAL
        public string Shift { get; set; } = "";   // SHIFTVALUE
        public string TesterGroup { get; set; } = "";   // TesterGroup (LEFT(TESTTYPE,8))

        public decimal PassQty { get; set; }         // PassCount
        public decimal FailQty { get; set; }         // FailCount
        public decimal TotalCount { get; set; }         // TotalCount
        public decimal Target { get; set; }         // PlanQty
        public decimal Gap { get; set; }         // DeltaQty
        public decimal LossPercent { get; set; }         // DeltaPercent

        // Computed in service after fetch
        public int HourSlot { get; set; }
        public decimal Actual { get; set; }

        public DateTime DateValue { get; set; }
    }

    public class DashboardViewModel
    {
        public List<string> Products { get; set; } = new();
        public string SelectedProduct { get; set; } = "";
        public string SelectedShift { get; set; } = "";
        public List<ProductionMetric> ShiftMetrics { get; set; } = new();
        //public List<ProductionMetric> MorningShift { get; set; } = new();
        //public List<ProductionMetric> AfternoonShift { get; set; } = new();
        //public List<ProductionMetric> NightShift { get; set; } = new();
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
