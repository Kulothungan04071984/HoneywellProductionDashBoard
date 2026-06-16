using Microsoft.AspNetCore.Mvc;
using ProductionDashboard.Models;
using ProductionDashboard.Services;

namespace ProductionDashboard.Controllers
{
    public class HomeoldController : Controller
    {
        private readonly IProductionService _svc;

        public HomeoldController(IProductionService svc) => _svc = svc;

        public async Task<IActionResult> Index(string product = "")
        {
            var products = await _svc.GetProductsAsync();
            if (string.IsNullOrEmpty(product) && products.Any())
                product = products.First();

            var vm = new DashboardViewModel
            {
                Products        = products,
                SelectedProduct = product,
                NightShift      = await _svc.GetMetricsAsync(product, "Night"),
                MorningShift    = await _svc.GetMetricsAsync(product, "Morning"),
                AfternoonShift  = await _svc.GetMetricsAsync(product, "Afternoon"),
            };
            return View(vm);
        }

        [HttpGet("/api/metrics")]
        public async Task<IActionResult> GetMetrics(string product, string shift)
        {
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
