using buytoy.Models;
using buytoy.Models.ViewModels;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace buytoy.Controllers
{
    public class ProductController :Controller

    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            var products = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(products); // ✅ Truyền model vào View
        }

        //public async Task<IActionResult> Category(int id)
        //{
        //    var products = await _dataContext.Products
        //        .Include(p => p.Category)
        //        .Include(p => p.Brand)
        //        .Where(p => p.CategoryId == id)
        //        .ToListAsync(); // phải dùng ToListAsync vì đang await

        //    return View(products);
        //}

        
       



        public IActionResult Wishlist()
        {
            return View();
        }
        public IActionResult Compare()
        {
            return View();
        }

       public async Task<IActionResult> Details(int Id)
{
    if (Id == 0) return RedirectToAction("Index");

    var product = await _dataContext.Products
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .FirstOrDefaultAsync(p => p.Id == Id);

    if (product == null) return NotFound();

    var relatedProducts = await _dataContext.Products
        .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
        .OrderByDescending(p => p.Id)
        .Take(8)
        .ToListAsync();

    var ratings = await _dataContext.Ratings
        .Where(r => r.ProductId == product.Id)
        .OrderByDescending(r => r.Id)
        .ToListAsync();

    double avgStar = 0;
    int totalRatings = ratings.Count;
    if (totalRatings > 0)
    {
        avgStar = Math.Round(ratings.Average(r => r.Star), 1);
    }

    var viewModel = new ProductDetailsViewModel
    {
        ProductDetails = product,
        Ratings = ratings,
        StarAverage = avgStar,
        TotalRatings = totalRatings
    };

    ViewBag.Related = relatedProducts;
            //var recentlyViewed = new List<ProductModel>();
            string cookieKey = "RecentlyViewed";
            string existing = Request.Cookies[cookieKey];
            List<string> viewedList = string.IsNullOrEmpty(existing) ? new List<string>() : existing.Split(',').ToList();

            // Ghi nhận sản phẩm đã xem
            if (!viewedList.Contains(Id.ToString()))
            {
                viewedList.Insert(0, Id.ToString()); // mới nhất lên đầu
                if (viewedList.Count > 10) viewedList = viewedList.Take(10).ToList();

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true
                };
                Response.Cookies.Append(cookieKey, string.Join(",", viewedList), cookieOptions);
            }

            // Lấy danh sách sản phẩm đã xem (ngoại trừ sản phẩm hiện tại)
            var ids = viewedList.Where(id => id != Id.ToString()).Select(int.Parse).ToList();
            var recentlyViewed = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => ids.Contains(p.Id))
                .ToList();

            // Sắp xếp theo thứ tự trong cookie
            recentlyViewed = ids.Select(id => recentlyViewed.FirstOrDefault(p => p.Id == id))
                                .Where(p => p != null)
                                .Take(8)
                                .ToList();

            ViewBag.RecentlyViewed = recentlyViewed;


            return View(viewModel);
}



        [HttpPost]
        public async Task<IActionResult> CommentProduct(ProductDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rating = new RatingModel
                {
                    ProductId = model.ProductId, // Sử dụng ProductId từ form
                    Name = model.Name,
                    Email = model.Email,
                    Comment = model.Comment,
                    Star = model.Star ?? 5
                };

                _dataContext.Ratings.Add(rating);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá thành công!";
                return RedirectToAction("Details", new { id = model.ProductId });
            }

            TempData["error"] = "Vui lòng kiểm tra lại thông tin đánh giá.";
            return RedirectToAction("Details", new { id = model.ProductId });
        }

        public IActionResult ByAge(string age)
        {
            if (string.IsNullOrEmpty(age)) return NotFound();

            var ageRange = age.Split('-');
            if (ageRange.Length != 2) return NotFound();

            if (!int.TryParse(ageRange[0], out int minAge) || !int.TryParse(ageRange[1], out int maxAge))
                return NotFound();

            var products = _dataContext.Products
                .Where(p => p.MinAge <= maxAge && p.MaxAge >= minAge)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            ViewData["Title"] = $"Sản phẩm cho bé từ {minAge} đến {maxAge} tuổi";
            return View(products);
        }
        public IActionResult ByAgeGroup(string group)
        {
            List<ProductModel> products;

            if (group == "kid")
            {
                products = _dataContext.Products
                    .Where(p => p.MaxAge <= 12)
                    .ToList();
            }
            else if (group == "adult")
            {
                products = _dataContext.Products
                    .Where(p => p.MinAge > 12)
                    .ToList();
            }
            else
            {
                products = new List<ProductModel>(); // hoặc redirect 404
            }

            ViewBag.Title = group == "kid" ? "Đồ chơi cho trẻ em" : "Đồ chơi người lớn";
            return View("Index", products); // dùng lại view hiển thị danh sách sản phẩm
        }

        [HttpGet]
        public IActionResult SearchSuggest(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(new { });

            var results = _dataContext.Products
                .Where(p => p.Name.ToLower().Contains(term.ToLower()))
                .Select(p => new
                {
                    label = p.Name,
                    value = p.Name,
                    image = string.IsNullOrEmpty(p.Image) ? "/media/no-image.png" : "/media/products/" + p.Image,
                    price = p.Price
                })
                .Take(10)
                .ToList();

            return Json(results);
        }


        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();

            //ViewBag.Keyword = searchTerm;
            ViewBag.SearchTerm = searchTerm;
            return View(products);
        }

    }
}

