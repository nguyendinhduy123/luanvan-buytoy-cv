using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Controllers
{
    [Route("category")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("{slug}")]
        public async Task<IActionResult> Index(string slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            var category = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
            {
                // Gợi ý: chuyển sang View NotFound riêng đẹp hơn thay vì Redirect
                return View("NotFound");
            }

            ViewBag.Slug = slug;

            IQueryable<ProductModel> productsByCategory = _dataContext.Products
                .Where(p => p.CategoryId == category.Id);

            // Lọc theo khoảng giá nếu có
            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out var startPriceValue) && decimal.TryParse(endprice, out var endPriceValue))
                {
                    productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }

            // Sắp xếp theo lựa chọn
            switch (sort_by)
            {
                case "price_increase":
                    productsByCategory = productsByCategory.OrderBy(p => p.Price);
                    break;
                case "price_decrease":
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
                    break;
                case "price_newest":
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                    break;
                case "price_oldest":
                    productsByCategory = productsByCategory.OrderBy(p => p.Id);
                    break;
                default:
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                    break;
            }

            // Tính min/max nếu có sản phẩm, tránh lỗi Sequence contains no elements
            var allProductsInCategory = await _dataContext.Products
                .Where(p => p.CategoryId == category.Id)
                .ToListAsync();

            if (allProductsInCategory.Any())
            {
                ViewBag.minprice = allProductsInCategory.Min(p => p.Price);
                ViewBag.maxprice = allProductsInCategory.Max(p => p.Price);
            }
            else
            {
                ViewBag.minprice = 0;
                ViewBag.maxprice = 0;
            }

            ViewBag.sort_key = sort_by;
            ViewBag.count = await productsByCategory.CountAsync();

            return View(await productsByCategory.ToListAsync());
        }
    }
}
