using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "Publisher,Author,Admin")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        public DashboardController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            // Tổng số lượng
            ViewBag.CountProduct = _dataContext.Products.Count();
            ViewBag.TotalStock = _dataContext.Products.Sum(p => p.Quantity);

            ViewBag.CountOrder = _dataContext.Orders.Count();
            ViewBag.CountCategory = _dataContext.Categories.Count();
            ViewBag.CountUser = _dataContext.Users.Count();

            // Trạng thái đơn hàng
            ViewBag.PendingOrders = _dataContext.Orders.Count(o => o.Status == 0);
            ViewBag.ShippingOrders = _dataContext.Orders.Count(o => o.Status == 1);
            ViewBag.CompletedOrders = _dataContext.Orders.Count(o => o.Status == 2);
            ViewBag.CanceledOrders = _dataContext.Orders.Count(o => o.Status == 3);


            // Tổng doanh thu
            ViewBag.TotalRevenue = _dataContext.Statisticals.Sum(s => s.Revenue);

            // Dữ liệu biểu đồ
            var chartData = _dataContext.Statisticals
                .OrderBy(s => s.DateCreated)
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                }).ToList();
            ViewBag.ChartData = chartData;

            // Top 10 sản phẩm đắt nhất
            ViewBag.TopExpensiveToys = _dataContext.Products
                .OrderByDescending(p => p.Price)
                .Take(10)
                .Select(p => new { ProductName = p.Name, Price = p.Price })
                .ToList();


            // Top khách hàng
            ViewBag.TopCustomers = _dataContext.Orders
              .GroupBy(o => new { o.UserId, o.Email, o.UserName })
              .Select(g => new
              {
                  UserId = g.Key.UserId,
                  Email = g.Key.Email,
                  UserName = g.Key.UserName,
                  TotalSpent = g.Sum(o =>
                      (o.TotalPrice - o.DiscountAmount + o.ShippingCost) < 0 ? 0 :
                      (o.TotalPrice - o.DiscountAmount + o.ShippingCost)),
                  OrderCount = g.Count()
              })
              .OrderByDescending(g => g.TotalSpent)
              .Take(5)
              .ToList();


            // Tổng sản phẩm
            ViewBag.AllProducts = _dataContext.Products
    .Include(p => p.Category) // nếu có liên kết với bảng Category
    .Select(p => new
    {
        ProductId = p.Id,
        Name = p.Name,
        CategoryName = p.Category != null ? p.Category.Name : "Không có danh mục",
        Price = p.Price,
        Quantity = p.Quantity,
        Sold = _dataContext.OrderDetails
                    .Where(od => od.ProductId == p.Id)
                    .Sum(od => (int?)od.Quantity) ?? 0,
        CreatedDate = p.CreatedDate,
        Status = p.Quantity == 0 ? "Hết hàng" : (p.Quantity < 10 ? "Sắp hết hàng" : "Còn hàng")
    })
    .OrderByDescending(p => p.CreatedDate)
    .ToList();

            // Sản phẩm sắp hết hàng
            ViewBag.LowStockProducts = _dataContext.Products
                .Where(p => p.Quantity < 10)
                .OrderBy(p => p.Quantity)
                .Take(10)
                .ToList();

            return View();
        }
        

        [HttpPost]
        [Route("GetChartData")]
        public IActionResult GetChartData(string filterRange)
        {
            DateTime startDate = DateTime.Today;
            switch (filterRange)
            {
                case "7days":
                    startDate = DateTime.Today.AddDays(-6); break;
                case "1month":
                    startDate = DateTime.Today.AddMonths(-1); break;
                case "3months":
                    startDate = DateTime.Today.AddMonths(-3); break;
                case "1year":
                    startDate = DateTime.Today.AddYears(-1); break;
                default:
                    startDate = DateTime.Today.AddDays(-6); break;
            }

            var rawData = _dataContext.Statisticals
                .Where(s => s.DateCreated.Date >= startDate)
                .OrderBy(s => s.DateCreated)
                .ToList();

            var data = rawData.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            }).ToList();

            return Json(data);
        }

        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var rawData = _dataContext.Statisticals
                .Where(s => s.DateCreated.Date >= startDate.Date && s.DateCreated.Date <= endDate.Date)
                .OrderBy(s => s.DateCreated)
                .ToList();

            var data = rawData.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            }).ToList();

            return Json(data);
        }

        [HttpPost]
        [Route("SyncStatisticalData")]
        public IActionResult SyncStatisticalData()
        {
            var groupedData = _dataContext.Orders
                .Join(_dataContext.OrderDetails,
                    o => o.OrderCode,
                    od => od.OrderCode,
                    (o, od) => new
                    {
                        Date = o.CreatedDate.Date,
                        Quantity = od.Quantity,
                        Revenue = od.Quantity * od.Price
                    })
                .GroupBy(x => x.Date)
                .Select(g => new StatisticalModel
                {
                    DateCreated = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Sold = g.Count(),
                    Revenue = g.Sum(x => x.Revenue),
                    Profit = g.Sum(x => x.Revenue) * 0.2m
                })
                .ToList();

            int updatedCount = 0;

            foreach (var item in groupedData)
            {
                var existing = _dataContext.Statisticals.FirstOrDefault(s => s.DateCreated.Date == item.DateCreated.Date);
                if (existing != null)
                {
                    existing.Quantity = item.Quantity;
                    existing.Sold = item.Sold;
                    existing.Revenue = item.Revenue;
                    existing.Profit = item.Profit;
                }
                else
                {
                    _dataContext.Statisticals.Add(item);
                }
                updatedCount++;
            }

            _dataContext.SaveChanges();
            return Json(new { success = true, count = updatedCount });
        }
    }
}
