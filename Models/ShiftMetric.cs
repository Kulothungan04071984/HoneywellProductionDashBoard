namespace ProductionDashboard.Models
{
    public class ShiftMetric
    {
        //public DateTime DateValue { get; set; }
        //public int HourSlot { get; set; }
        //public string HourRange { get; set; }
        //public string TesterGroup { get; set; }
        //public decimal Target { get; set; }
        //public decimal Actual { get; set; }

        public string HourRange { get; set; }  // HOURINTERVAL
        public string Shift { get; set; }  // SHIFTVALUE
        public string DateValue { get; set; }  // DATEVALUE
        public string TesterGroup { get; set; }  // TesterGroup
        public int PassQty { get; set; }  // PassQty
        public int FailQty { get; set; }  // FailQty
        public int TotalCount { get; set; }  // TotalCount
        public int Target { get; set; }  // Target
        public int Actual { get; set; }  // Actual
        public int Gap { get; set; }  // Gap
        public decimal LossPercent { get; set; }  // LossPercent
    }
}
