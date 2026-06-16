using Dapper;
using Microsoft.Data.SqlClient;
using ProductionDashboard.Models;

namespace ProductionDashboard.Services
{
    public interface IProductionService
    {
        Task<List<string>> GetProductsAsync();
        Task<List<ProductionMetricold>> GetMetricsAsync(string product, string shift);
    }

    public class ProductionServiceold : IProductionService
    {
        private readonly string _connectionString;
        private readonly ILogger<ProductionServiceold> _logger;

        public ProductionServiceold(IConfiguration config, ILogger<ProductionServiceold> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
            _logger = logger;
        }

        public async Task<List<string>> GetProductsAsync()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var products = await conn.QueryAsync<string>(
                    "SELECT DISTINCT Product FROM ProductionMetrics ORDER BY Product");
                return products.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DB unavailable – using mock products");
                return GetMockProducts();
            }
        }

        public async Task<List<ProductionMetricold>> GetMetricsAsync(string product, string shift)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var sql = @"
                    SELECT * FROM ProductionMetrics
                    WHERE Product = @Product AND Shift = @Shift
                    ORDER BY HourSlot";
                var result = await conn.QueryAsync<ProductionMetricold>(sql, new { Product = product, Shift = shift });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DB unavailable – using mock metrics for {Product}/{Shift}", product, shift);
                return GetMockMetrics(product, shift);
            }
        }

        // ── Mock data (used when SQL Server is not connected) ──────────────────
        private static List<string> GetMockProducts() =>
            new() { "V200", "V201", "V202", "V300", "V301" };

        private static List<ProductionMetricold> GetMockMetrics(string product, string shift)
        {
            var rng = new Random(product.GetHashCode() ^ shift.GetHashCode());
            var result = new List<ProductionMetricold>();
            for (int h = 1; h <= 8; h++)
            {
                result.Add(new ProductionMetricold
                {
                    Product  = product,
                    Shift    = shift,
                    HourSlot = h,
                    FCT1_Target = 100, FCT1_Actual = rng.Next(80, 115),
                    FCT2_Target = 95,  FCT2_Actual = rng.Next(75, 110),
                    FCT3_Target = 90,  FCT3_Actual = rng.Next(70, 105),
                    RF1_Target  = 120, RF1_Actual  = rng.Next(95, 135),
                    RF2_Target  = 110, RF2_Actual  = rng.Next(88, 125),
                    RTC1_Target = 85,  RTC1_Actual = rng.Next(65, 100),
                    VOL1_Target = 200, VOL1_Actual = rng.Next(160, 230),
                    RecordedAt  = DateTime.Now,
                });
            }
            return result;
        }
    }
}
