using Microsoft.AspNetCore.Mvc;
using ProductionDashboard.Models;
using ProductionDashboard.Services;

namespace ProductionDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductionService _svc;

        public HomeController(IProductionService svc) => _svc = svc;

        //public async Task<IActionResult> Dashboard(string product = "")
        //{
        //    var products = await _svc.GetProductsAsync();
        //    if (string.IsNullOrEmpty(product) && products.Any())
        //        product = products.First();

        //    var vm = new DashboardViewModel
        //    {
        //        Products        = products,
        //        SelectedProduct = product,
        //        NightShift      = await _svc.GetMetricsAsync(product, "Night"),
        //        MorningShift    = await _svc.GetMetricsAsync(product, "Morning"),
        //        AfternoonShift  = await _svc.GetMetricsAsync(product, "Afternoon"),
        //    };
        //    return View(vm);
        //}

        //public JsonResult DashBoardHistory(string product = "", string fromdate = "", string todate = "")
        //{
        //    var products = _svc.GetProductsAsync().Result;
        //    if (string.IsNullOrEmpty(product) && products.Any())
        //        product = products.First();

        //    // Support both dd-MM-yyyy (from SP) and yyyy-MM-dd (from date input directly)
        //    var formats = new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };

        //    DateTime fromDt = DateTime.ParseExact(fromdate, formats,
        //                          System.Globalization.CultureInfo.InvariantCulture,
        //                          System.Globalization.DateTimeStyles.None);

        //    DateTime toDt = DateTime.ParseExact(todate, formats,
        //                          System.Globalization.CultureInfo.InvariantCulture,
        //                          System.Globalization.DateTimeStyles.None)
        //                          .AddDays(1).AddSeconds(-1);  // include the full end day

        //    var nightData = _svc.GetMetricsAsync(product, "Night").Result;
        //    var morningData = _svc.GetMetricsAsync(product, "Morning").Result;
        //    var afternoonData = _svc.GetMetricsAsync(product, "Afternoon").Result;

        //    var historyData = new
        //    {
        //        NightShift = nightData.Where(m => m.DateValue >= fromDt && m.DateValue <= toDt),
        //        MorningShift = morningData.Where(m => m.DateValue >= fromDt && m.DateValue <= toDt),
        //        AfternoonShift = afternoonData.Where(m => m.DateValue >= fromDt && m.DateValue <= toDt)
        //    };

        //    return Json(historyData);
        //}
        // ── Serves the History page (GET from redirect) ──────────────────────
        [HttpGet]
        public async Task<IActionResult> DashBoardHistoryView(
     string product = "",
     string fromdate = "",
     string todate = "")
        {
            var products = await _svc.GetProductsAsync();
            if (string.IsNullOrEmpty(product) && products.Any())
                product = products.First();

            var formats = new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
            if (!DateTime.TryParseExact(fromdate, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _) ||
                !DateTime.TryParseExact(todate, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _))
            {
                return RedirectToAction("Index");
            }

          
            var allData = await _svc.GetHistoryAsync(fromdate, todate);

            System.Diagnostics.Debug.WriteLine($"fromdate='{fromdate}', todate='{todate}', count={allData.Count}");

            if (!allData.Any())
                throw new Exception($"No data returned. fromdate='{fromdate}' todate='{todate}'");

            var vm = new DashboardHistoryViewModel
            {
                Product = product,
                FromDate = fromdate,
                ToDate = todate,
                Products = products.ToList(),
                NightShift = allData.Where(m => m.Shift == "SHIFT-C").ToList(),
                MorningShift = allData.Where(m => m.Shift == "SHIFT-A").ToList(),
                AfternoonShift = allData.Where(m => m.Shift == "SHIFT-B").ToList()
            };

            return View("DashBoardHistory", vm);
        }

        public async Task<IActionResult> Dashboard(string product = "", string? shift = null)
        {
            var products = await _svc.GetProductsAsync();

            if (string.IsNullOrEmpty(product) && products.Any())
                product = products.First();

            // Dynamic shift detection
            if (string.IsNullOrEmpty(shift))
            {
                var now = DateTime.Now.TimeOfDay;

                if (now >= new TimeSpan(4, 0, 0) &&
                    now < new TimeSpan(16, 0, 0))
                {
                    shift = "Morning";
                }
                else if (now >= new TimeSpan(16, 0, 0) &&
                         now < new TimeSpan(20, 0, 0))
                {
                    shift = "Afternoon";
                }
                else
                {
                    shift = "Night";
                }
            }

            var vm = new DashboardViewModel
            {
                Products = products,
                SelectedProduct = product,
                SelectedShift = shift,

                // Only selected shift data
                ShiftMetrics = await _svc.GetMetricsAsync(product, shift)
            };

            return View(vm);
        }
        //[HttpGet("/api/metrics")]
        //public async Task<IActionResult> GetMetrics(string product, string shift)
        //{
        //    var data = await _svc.GetMetricsAsync(product, shift);
        //    return Json(data);
        //}
        [HttpGet("/api/metrics")]
        public async Task<IActionResult> GetMetrics(string product, string? shift = null)
        {
            // Dynamic shift detection
            if (string.IsNullOrEmpty(shift))
            {
                var now = DateTime.Now.TimeOfDay;

                if (now >= new TimeSpan(4, 0, 0) &&
                    now < new TimeSpan(16, 0, 0))
                {
                    shift = "Morning";
                }
                else if (now >= new TimeSpan(16, 0, 0) &&
                         now < new TimeSpan(20, 0, 0))
                {
                    shift = "Afternoon";
                }
                else
                {
                    shift = "Night";
                }
            }

            var data = await _svc.GetMetricsAsync(product, shift);

            return Json(data);
        }

        [HttpGet("/api/products")]
        public async Task<IActionResult> GetProducts()
        {
            var data = await _svc.GetProductsAsync();
            return Json(data);
        }
    }
}
