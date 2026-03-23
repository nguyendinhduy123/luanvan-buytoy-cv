    using buytoy.Models;
    using buytoy.Models.ViewModels;
    using buytoy.Repository;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System.Text.RegularExpressions;

    namespace buytoy.Controllers
    {
        public class CartController : Controller
        {
            private readonly DataContext _dataContext;

            public CartController(DataContext context)
            {
                _dataContext = context;
            }

            public IActionResult Index(ShippingModel shippingModel)
            {
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

                // Lấy phí vận chuyển từ cookie
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                if (shippingPriceCookie != null)
                {
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
                }

                // Lấy mã giảm giá từ cookie
                var couponTitle = Request.Cookies["CouponTitle"];
                decimal discountAmount = 0;

                if (!string.IsNullOrEmpty(couponTitle))
                {
                    // Tách Name và Description từ cookie "SALE10 | giảm 10%"
                    var parts = couponTitle.Split(" | ");
                    var name = parts[0];
                    var description = parts.Length > 1 ? parts[1] : "";

                    var coupon = _dataContext.Coupons
                        .FirstOrDefault(c => c.Name == name && c.Description == description);

                    if (coupon != null && coupon.Quantity > 0 && coupon.DateExpired >= DateTime.Now)
                    {
                        // Tìm % từ chuỗi Description (ví dụ: "giảm 10%")
                        var match = Regex.Match(coupon.Description, @"(\d+)%");
                        if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal percent))
                        {
                            var grandTotal = cartItems.Sum(x => x.Quantity * x.Price);
                            discountAmount = grandTotal * percent / 100;
                        }
                    }
                }

                CartItemViewModel cartVM = new()
                {
                    CartItems = cartItems,
                    GrandTotal = cartItems.Sum(x => x.Quantity * x.Price) - discountAmount,
                    ShippingCost = shippingPrice,
                    CouponCode = couponTitle,
                     DiscountAmount = discountAmount
                };

                return View(cartVM);
            }


            public IActionResult Checkout()
            {
                return View("~/Views/Checkout/Index.cshtml");
            }

            public async Task<IActionResult> Add(int Id)
            {
                var product = await _dataContext.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == Id);

                if (product == null)
                {
                    TempData["error"] = "Sản phẩm không tồn tại.";
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                var cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

                if (cartItem == null)
                    cart.Add(new CartItemModel(product));
                else
                    cartItem.Quantity += 1;

                HttpContext.Session.SetJson("Cart", cart);
                TempData["success"] = "Thêm sản phẩm vào giỏ hàng thành công.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            public async Task<IActionResult> Decrease(int Id)
            {
                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
                CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

                if (cartItem.Quantity > 1)
                    cartItem.Quantity--;
                else
                    cart.Remove(cartItem);

                UpdateCartInSession(cart);
                TempData["success"] = "Giảm số lượng sản phẩm thành công.";
                return RedirectToAction("Index");
            }

            public async Task<IActionResult> Increase(int Id)
            {
                var product = await _dataContext.Products.FindAsync(Id);
                var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
                var cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

                if (cartItem != null && product != null)
                {
                    if (product.Quantity > cartItem.Quantity)
                    {
                        cartItem.Quantity++;
                        TempData["success"] = "Đã tăng số lượng sản phẩm.";
                    }
                    else
                    {
                        TempData["success"] = "Đã đạt số lượng tối đa trong kho!";
                    }
                }

                UpdateCartInSession(cart);
                return RedirectToAction("Index");
            }

            public async Task<IActionResult> Remove(int Id)
            {
                var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
                cart.RemoveAll(p => p.ProductId == Id);

                UpdateCartInSession(cart);
                TempData["success"] = "Xóa sản phẩm thành công.";
                return RedirectToAction("Index");
            }

            public async Task<IActionResult> Clear()
            {
                HttpContext.Session.Remove("Cart");
                TempData["success"] = "Xóa tất cả sản phẩm.";
                return RedirectToAction("Index");
            }

            [HttpPost]
            public async Task<IActionResult> GetShipping(string tinh, string quan, string phuong)
            {
                var existingShipping = await _dataContext.Shippings
                    .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                decimal shippingPrice = existingShipping?.Price ?? 50000;
                var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true
                });

                return Json(new { shippingPrice });
            }

            [HttpPost]
            public async Task<IActionResult> GetCoupon(string coupon_value)
            {
                var validCoupon = await _dataContext.Coupons
                    .FirstOrDefaultAsync(x => x.Name == coupon_value && x.Quantity >= 1);

                if (validCoupon == null)
                    return Ok(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết lượt" });

                if (validCoupon.DateExpired < DateTime.Now)
                    return Ok(new { success = false, message = "Mã giảm giá đã hết hạn" });

                string couponTitle = $"{validCoupon.Name} | {validCoupon.Description}";

                // Lấy giỏ hàng hiện tại để tính giảm giá
                var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new();
                decimal grandTotal = cartItems.Sum(x => x.Quantity * x.Price);

                decimal discountAmount = 0;
                var match = System.Text.RegularExpressions.Regex.Match(validCoupon.Description, @"(\d+)%");
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal percent))
                {
                    discountAmount = grandTotal * percent / 100;
                }

                // Lưu coupon title vào cookie
                Response.Cookies.Append("CouponTitle", couponTitle, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                // Lưu số tiền giảm vào cookie
                Response.Cookies.Append("DiscountAmount", JsonConvert.SerializeObject(discountAmount), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

            return Ok(new
            {
                success = true,
                message = "Áp dụng mã giảm giá thành công",
                coupon = couponTitle,
                discountAmount = discountAmount  // <-- Thêm dòng này
            });
        }

         

            [HttpPost]
            public IActionResult DeleteShipping()
            {
                Response.Cookies.Delete("ShippingPrice");
                return Json(new { success = true });
            }

            private void UpdateCartInSession(List<CartItemModel> cart)
            {
                if (cart == null || cart.Count == 0)
                    HttpContext.Session.Remove("Cart");
                else
                    HttpContext.Session.SetJson("Cart", cart);
            }
        }
    }
