namespace ProductionDashboard.Models
{
    public class DashboardHistoryViewModel
    {
        public string Product { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<string> Products { get; set; }

        public List<ShiftMetric> NightShift { get; set; }
        public List<ShiftMetric> MorningShift { get; set; }
        public List<ShiftMetric> AfternoonShift { get; set; }
    }
}
