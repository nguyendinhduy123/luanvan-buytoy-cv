using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace buytoy.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }
        public IActionResult ReturnPolicy() => View();
        public IActionResult ShippingPolicy() => View();
        public IActionResult Terms()
        {
            return View();
        }

        public async Task<IActionResult> Index(string sort_by = "", string startprice = "", string endprice = "")
        {

            // Lấy tất cả sản phẩm có brand và category
            IQueryable<ProductModel> products = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand);

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out var startPriceValue) && decimal.TryParse(endprice, out var endPriceValue))
                {
                    products = products.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }

            // Sắp xếp
            switch (sort_by)
            {
                case "price_increase":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_decrease":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "price_newest":
                    products = products.OrderByDescending(p => p.Id);
                    break;
                case "price_oldest":
                    products = products.OrderBy(p => p.Id);
                    break;
                default:
                    products = products.OrderByDescending(p => p.Id);
                    break;
            }

            // Lấy min/max giá từ toàn bộ sản phẩm để hiển thị filter
            ViewBag.minprice = await _dataContext.Products.MinAsync(p => p.Price);
            ViewBag.maxprice = await _dataContext.Products.MaxAsync(p => p.Price);
            ViewBag.sort_key = sort_by;
            ViewBag.count = await products.CountAsync();

            // Sliders
            var sliders = await _dataContext.Sliders.Where(s => s.Status == 1).ToListAsync();
            ViewBag.Sliders = sliders;

            return View(await products.ToListAsync());
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contact.FirstOrDefaultAsync();
            if (contact == null)
            {
                contact = new ContactModel
                {
                    Name = "Chưa cập nhật",
                    Email = "support@tiemdochoi.com",
                    Phone = "0000 000 000",
                    LogoImg = "default.png",
                    Description = "Thông tin chưa cập nhật",
                    Map = "<p>Bản đồ chưa có</p>"
                };
            }
            return View(contact);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SendContact(string Name, string Email, string Message)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Message))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToAction("Contact");
            }

            var contactMsg = new ContactMessageModel
            {
                Name = Name,
                Email = Email,
                Message = Message,
                SentAt = DateTime.Now
            };

            _dataContext.ContactMessages.Add(contactMsg);
            _dataContext.SaveChanges();

            TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
            return RedirectToAction("Contact");
        }


        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         select new { User = u, Product = p, Compares = c })
                               .ToListAsync();

            return View(compare_product);
        }
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);

            _dataContext.Compares.Remove(compare);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "So sánh đã được xóa thành công";
            return RedirectToAction("Compare", "Home");
        }
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

            _dataContext.Wishlists.Remove(wishlist);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Yêu thích đã được xóa thành công";
            return RedirectToAction("Wishlist", "Home");
        }
        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductId equals p.Id
                                          select new { Product = p, Wishlists = w })
                               .ToListAsync();

            return View(wishlist_product);
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(int Id, WishlistModel wishlistmodel)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            _dataContext.Wishlists.Add(wishlistProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to wishlisht Successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to wishlist table.");
            }

        }
        [HttpPost]
        public async Task<IActionResult> AddCompare(int Id)
        {
            var user = await _userManager.GetUserAsync(User);

            var compareProduct = new CompareModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            _dataContext.Compares.Add(compareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to compare Successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to compare table.");
            }

        }
        [HttpPost]
        public async Task<IActionResult> SyncWishlist([FromBody] List<int> ids)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || ids == null || !ids.Any())
                return BadRequest();

            var existing = _dataContext.Wishlists
                .Where(w => w.UserId == user.Id)
                .Select(w => w.ProductId)
                .ToHashSet();

            var newItems = ids.Distinct().Where(id => !existing.Contains(id)).ToList();

            foreach (var id in newItems)
            {
                _dataContext.Wishlists.Add(new WishlistModel
                {
                    ProductId = id,
                    UserId = user.Id
                });
            }

            await _dataContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SyncCompare([FromBody] List<int> ids)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || ids == null || !ids.Any())
                return BadRequest();

            var existing = _dataContext.Compares
                .Where(c => c.UserId == user.Id)
                .Select(c => c.ProductId)
                .ToHashSet();

            var newItems = ids.Distinct().Where(id => !existing.Contains(id)).ToList();

            foreach (var id in newItems)
            {
                _dataContext.Compares.Add(new CompareModel
                {
                    ProductId = id,
                    UserId = user.Id
                });
            }

            await _dataContext.SaveChangesAsync();
            return Ok();
        }
        public IActionResult Blog()
        {
            // Bạn có thể load bài viết từ DB ở đây
            return View();
        }
        public async Task<IActionResult> News()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-1);
            var news = await _dataContext.News
                .Where(n => n.CreatedAt >= thirtyDaysAgo)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(news);
        }
        public async Task<IActionResult> NewsArchive()
        {
            var cutoffDate = DateTime.Now.AddDays(-1);
            var oldNews = await _dataContext.News
                .Where(n => n.CreatedAt < cutoffDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(oldNews);
        }
        public IActionResult About()
        {
            return View();
        }

    }
}

