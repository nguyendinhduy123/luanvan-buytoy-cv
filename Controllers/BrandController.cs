using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Controllers
{
    [Route("brand")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("{slug}")]
        public async Task<IActionResult> Index(string slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            var brand = await _dataContext.Brands.FirstOrDefaultAsync(b => b.Slug == slug);

            if (brand == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Slug = slug;

            // Lấy danh sách sản phẩm theo thương hiệu
            IQueryable<ProductModel> productsByBrand = _dataContext.Products
                .Where(p => p.BrandId == brand.Id);

            // Lọc theo khoảng giá nếu có
            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out var startPriceValue) &&
                    decimal.TryParse(endprice, out var endPriceValue))
                {
                    productsByBrand = productsByBrand
                        .Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }

            // Sắp xếp theo loại được chọn
            productsByBrand = sort_by switch
            {
                "price_increase" => productsByBrand.OrderBy(p => p.Price),
                "price_decrease" => productsByBrand.OrderByDescending(p => p.Price),
                "price_newest" => productsByBrand.OrderByDescending(p => p.Id),
                "price_oldest" => productsByBrand.OrderBy(p => p.Id),
                _ => productsByBrand.OrderByDescending(p => p.Id)
            };

            // Lấy tất cả sản phẩm thuộc brand để lấy min/max giá
            var allProductsInBrand = await _dataContext.Products
                .Where(p => p.BrandId == brand.Id)
                .ToListAsync();

            ViewBag.minprice = allProductsInBrand.Any() ? allProductsInBrand.Min(p => p.Price) : 0;
            ViewBag.maxprice = allProductsInBrand.Any() ? allProductsInBrand.Max(p => p.Price) : 0;

            ViewBag.sort_key = sort_by;
            ViewBag.count = await productsByBrand.CountAsync();

            return View(await productsByBrand.ToListAsync());
        }
    }
}
