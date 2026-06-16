using Dapper;
using Microsoft.Data.SqlClient;
using ProductionDashboard.Models;
using System.Data;

namespace ProductionDashboard.Services
{
    public interface IProductionService
    {
        Task<List<string>> GetProductsAsync();
        Task<List<ProductionMetric>> GetMetricsAsync(string product, string shift);
        Task<List<ShiftMetric>> GetHistoryAsync(string fromdate, string todate);
    }

    public class ProductionService : IProductionService
    {
        private readonly string _connectionString;
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(IConfiguration config, ILogger<ProductionService> logger)
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

        public async Task<List<ShiftMetric>> GetHistoryAsync(string fromdate, string todate)
        {
            var result = new List<ShiftMetric>();

            var formats = new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };

            if (!DateTime.TryParseExact(fromdate, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime fromDt))
                throw new Exception($"Invalid fromdate: '{fromdate}'");

            if (!DateTime.TryParseExact(todate, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime toDt))
                throw new Exception($"Invalid todate: '{todate}'");

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("PRO_GETHONEYWELLV200DETAILS_History", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@fromdate", SqlDbType.Date).Value = fromDt;
            cmd.Parameters.Add("@todate", SqlDbType.Date).Value = toDt;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new ShiftMetric
                {
                    HourRange = reader["HourRange"]?.ToString(),
                    Shift = reader["Shift"]?.ToString(),
                    DateValue = reader["DateValue"]?.ToString(),
                    TesterGroup = reader["TesterGroup"]?.ToString(),
                    PassQty = reader["PassQty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PassQty"]),
                    FailQty = reader["FailQty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FailQty"]),
                    TotalCount = reader["TotalCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalCount"]),
                    Target = reader["Target"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Target"]),
                    Actual = reader["Actual"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Actual"]),
                    Gap = reader["Gap"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Gap"]),
                    LossPercent = reader["LossPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["LossPercent"])
                });
            }

            return result;
        }

        //public async Task<List<ProductionMetric>> GetMetricsAsync(string product, string shift)
        //{
        //    try
        //    {
        //        using var conn = new SqlConnection(_connectionString);
        //        var sql = "PRO_GETHONEYWELLV200DETAILS";
        //        var result = await conn.QueryAsync<ProductionMetric>(sql, new { Product = product, Shift = shift });
        //        return result.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning(ex, "DB unavailable – using mock metrics for {Product}/{Shift}", product, shift);
        //        return GetMockMetrics(product, shift);
        //    }
        //}
        private int GetHourSlot(string hourRange)
        {
            return hourRange switch
            {
                "08-09" => 1,
                "09-10" => 2,
                "10-11" => 3,
                "11-12" => 4,
                "12-13" => 5,
                "13-14" => 6,
                "14-15" => 7,
                "15-16" => 8,
                "16-17" => 9,
                "17-18" => 10,
                _ => 0
            };
        }
        //       public async Task<List<ProductionMetric>> GetMetricsAsync(string product, string shift)
        //       {
        //           try
        //           {
        //               using var conn = new SqlConnection(_connectionString);

        //               var result = await conn.QueryAsync<ProductionMetric>(
        //    "PRO_GETHONEYWELLV200DETAILS",
        //    new
        //    {
        //        @datevalue = DateTime.Now.ToString("dd-MM-yyyy")
        //    },
        //    commandType: CommandType.StoredProcedure
        //);

        //               var metrics = result
        //                   .Select(x => new ProductionMetric
        //                   {
        //                       HourSlot = GetHourSlot(x.HourRange), // 08-09 => 1
        //                       HourRange = x.HourRange,
        //                       Shift = x.Shift,
        //                       TesterGroup = x.TesterGroup,

        //                       PassQty = x.PassQty,
        //                       FailQty = x.FailQty,

        //                       Actual = x.Actual,
        //                       Target = x.Target,

        //                       Gap = x.Gap,
        //                       LossPercent = x.LossPercent
        //                   })
        //                   .OrderBy(x => x.HourSlot)
        //                   .ThenBy(x => x.TesterGroup)
        //                   .ToList();

        //               return metrics;
        //           }
        //           catch (Exception ex)
        //           {
        //               _logger.LogWarning(ex,
        //                   "DB unavailable – using mock metrics for {Product}/{Shift}",
        //                   product,
        //                   shift);

        //               return GetMockMetrics(product, shift);
        //           }
        //       }

        public async Task<List<ProductionMetric>> GetMetricsAsync(string product, string shift)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);

                var result = await conn.QueryAsync<dynamic>(
     "PRO_GETHONEYWELLV200DETAILS_Test",
     new
     {
         datevalue = DateTime.Now.ToString("dd-MM-yyyy")
     },
     commandType: CommandType.StoredProcedure
 );

                string dbShift = shift switch
                {
                    "Morning" => "SHIFT-A",
                    "Afternoon" => "SHIFT-B",
                    "Night" => "SHIFT-C",
                    _ => shift
                };

                var metrics = result.Select(x => new ProductionMetric
                {
                    HourRange = x.HourRange ?? "",
                    Shift = x.Shift ?? "",
                    TesterGroup = x.TesterGroup ?? "",

                    PassQty = Convert.ToDecimal(x.PassQty ?? 0),
                    FailQty = Convert.ToDecimal(x.FailQty ?? 0),
                    TotalCount = Convert.ToDecimal(x.TotalCount ?? 0),

                    Target = Convert.ToDecimal(x.Target ?? 0),
                    Gap = Convert.ToDecimal(x.Gap ?? 0),
                    LossPercent = Convert.ToDecimal(x.LossPercent ?? 0),

                    Actual = Convert.ToDecimal(x.Actual ?? 0),

                    HourSlot = GetHourSlot((string)(x.HourRange ?? ""))
                })
                .Where(x => x.Shift == dbShift)
                .OrderBy(x => x.HourSlot)
                .ThenBy(x => x.TesterGroup)
                .ToList();

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "DB unavailable – using mock metrics for {Product}/{Shift}",
                    product, shift);

                return GetMockMetrics(product, shift);
            }
        }
        // ── Mock data (used when SQL Server is not connected) ──────────────────
        private static List<string> GetMockProducts() =>
            new() { "V200", "V201", "V202", "V300", "V301" };

        private static List<ProductionMetric> GetMockMetrics(string product, string shift)
        {
            var rng = new Random(product.GetHashCode() ^ shift.GetHashCode());
            var result = new List<ProductionMetric>();
            for (int h = 1; h <= 8; h++)
            {
                result.Add(new ProductionMetric
                {
                    Product  = product,
                    Shift    = shift,
                    HourSlot = h,
                    FCT1_Target = 100, FCT1_Actual = rng.Next(80, 115),
                    FCT2_Target = 95,  FCT2_Actual = rng.Next(75, 110),
                    RecordedAt  = DateTime.Now,
                });
            }
            return result;
        }
    }
}
