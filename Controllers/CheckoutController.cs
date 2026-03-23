using Azure.Core;
using buytoy.Areas.Admin.Repository;
using buytoy.Models;
using buytoy.Models.ViewModels;
using buytoy.Models.Vnpay;
using buytoy.Repository;
using buytoy.Services.Momo;
using buytoy.Services.Vnpay;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace buytoy.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private decimal GetCartTotal()
        {
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            return cart.Sum(item => item.Price * item.Quantity);
        }
        

        public CheckoutController(IEmailSender emailSender, DataContext context, IMomoService momoService, IVnPayService vnPayService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }
        [HttpPost]
        public IActionResult DeleteCoupon()
        {
            Response.Cookies.Delete("CouponTitle");
            Response.Cookies.Delete("DiscountAmount"); 
            return Json(new { success = true });
        }

        public IActionResult Index()
        {
           
            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["warning"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            decimal grandTotal = cartItems.Sum(item => item.Price * item.Quantity);

            
            string couponCode = Request.Cookies["CouponTitle"];
            decimal discountAmount = 0;
            string discountCookie = Request.Cookies["DiscountAmount"];
            if (!string.IsNullOrEmpty(discountCookie) && decimal.TryParse(discountCookie, out var parsedDiscount))
            {
                discountAmount = parsedDiscount;
            }

            // Lấy phí vận chuyển từ cookie (nếu có)
            decimal shippingFee = 0;
            string shippingCookie = Request.Cookies["ShippingPrice"];
            if (!string.IsNullOrEmpty(shippingCookie))
            {
                shippingFee = JsonConvert.DeserializeObject<decimal>(shippingCookie);
            }

            // Tạo ViewModel
            var viewModel = new CartItemViewModel
            {
                CartItems = cartItems,
                GrandTotal = grandTotal,
                ShippingCost = shippingFee,
                DiscountAmount = discountAmount,
                CouponCode = couponCode
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(
     decimal ShippingCost,
     decimal DiscountAmount,
     string CouponCode, string OrderId, string ShippingOption, string PaymentMethod = "COD"
 )

        {
            if (ShippingOption == "express")
            {
                ShippingCost += 20000;
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            if (!cartItems.Any())
            {
                TempData["warning"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            string ordercode = Guid.NewGuid().ToString();
            decimal totalPrice = cartItems.Sum(x => x.Price * x.Quantity);
            decimal finalTotal = totalPrice + ShippingCost - DiscountAmount;

            var orderItem = new OrderModel
            {
                OrderCode = ordercode,
                
                UserName = userEmail,
                Status = 1,
                CreatedDate = DateTime.Now,
                ShippingCost = ShippingCost,
                CouponCode = CouponCode,
                DiscountAmount = DiscountAmount,
                TotalPrice = totalPrice,
                FinalTotal = finalTotal,
                 PaymentMethod = string.IsNullOrEmpty(OrderId) ? "COD" : "Momo"  
            };

            _dataContext.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            foreach (var cart in cartItems)
            {
                var orderdetail = new OrderDetails
                {
                    UserName = userEmail,
                    OrderCode = ordercode,
                    ProductId = (int)cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };

                var product = await _dataContext.Products.FirstAsync(p => p.Id == cart.ProductId);
                product.Quantity -= cart.Quantity;
                product.Sold += cart.Quantity;

                _dataContext.Update(product);
                _dataContext.Add(orderdetail);
            }

            await _dataContext.SaveChangesAsync();

            string emailSubject = "Xác nhận đơn hàng - Tiệm đồ chơi của Di";
            string emailBody = $@"
        <h3>Xin chào {userEmail},</h3>
        <p>Bạn đã đặt hàng thành công tại <b>Tiệm đồ chơi của Di</b>.</p>
        <p><b>Mã đơn hàng:</b> {ordercode}</p>
        <p><b>Tổng tiền:</b> {finalTotal:N0}₫</p>
        <p><b>Phí vận chuyển:</b> {ShippingCost:N0}₫</p>
        <p><b>Giảm giá:</b> {DiscountAmount:N0}₫</p>
        <hr>
        <p>Chúng tôi sẽ liên hệ bạn sớm để xác nhận đơn.</p>
        <p>Trân trọng,</p>
        <p><b>Tiệm đồ chơi của Di</b></p>
    ";

            await _emailSender.SendEmailAsync(userEmail, emailSubject, emailBody);

            HttpContext.Session.Remove("Cart");

            // ✅ Xóa cookie cũ nếu có
            Response.Cookies.Delete("CouponTitle");
            Response.Cookies.Delete("DiscountAmount");
            Response.Cookies.Delete("ShippingPrice");

            TempData["success"] = "Đơn hàng đã được tạo, vui lòng chờ duyệt đơn hàng nhé.";
            return RedirectToAction("History", "Account");
        }



        // Trang cảm ơn
        public IActionResult ThankYou()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MomoPayment(string FullName, decimal ShippingCost, decimal DiscountAmount)
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ tên để thanh toán." });
            }

            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng rỗng." });
            }

            decimal totalPrice = cartItems.Sum(x => x.Price * x.Quantity);
            decimal finalTotal = totalPrice + ShippingCost - DiscountAmount;

            var momoRequest = new OrderInfoModel
            {
                FullName = FullName,
                Amount = (double)finalTotal,
                //OrderInformation = $"Thanh toán đơn hàng tại Tiệm đồ chơi của Di cho {FullName}"


            };

            var response = await _momoService.CreatePaymentAsync(momoRequest);

            if (response != null && !string.IsNullOrEmpty(response.PayUrl))
            {
                return Json(new { payUrl = response.PayUrl });
            }

            return Json(new { success = false, message = "Không thể tạo liên kết thanh toán Momo." });
        }


        //[HttpGet]
        //public IActionResult MomoReturn()
        //{
        //    var resultCode = Request.Query["resultCode"];
        //    var result = _momoService.PaymentExecuteAsync(Request.Query);

        //    if (resultCode != "0")
        //    {
        //        TempData["error"] = "Thanh toán không thành công hoặc đã bị hủy.";
        //        return RedirectToAction("Index", "Cart");
        //    }

        //    // TODO: Bạn có thể lưu đơn hàng tại đây nếu chưa lưu trong CreatePaymentAsync.

        //    TempData["success"] = $"Thanh toán thành công đơn hàng #{result.OrderId}, số tiền: {result.Amount}₫";
        //    return RedirectToAction("ThankYou", "Checkout");
        //}

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var query = HttpContext.Request.Query;
            var result = _momoService.PaymentExecuteAsync(query);

            bool isSuccess = query["resultCode"] == "0";
            string message = isSuccess ? "Thanh toán MOMO thành công!" : $"Thanh toán thất bại: {query["message"]}";

            ViewData["ResultMessage"] = message;

            if (isSuccess)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                string orderId = query["orderId"];

                var momoRecord = new MomoInfoModel
                {
                    OrderId = orderId,
                    FullName = userEmail,
                    Amount = decimal.Parse(query["amount"]),
                    OrderInfo = query["orderInfo"],
                    DatePaid = DateTime.Now
                };
                _dataContext.Add(momoRecord);
                await _dataContext.SaveChangesAsync();

                decimal shippingCost = 0;
                decimal discountAmount = 0;
                string couponCode = "";
                string shippingOption = "standard";

                if (Request.Cookies["ShippingPrice"] != null)
                    shippingCost = JsonConvert.DeserializeObject<decimal>(Request.Cookies["ShippingPrice"]);
                if (Request.Cookies["DiscountAmount"] != null)
                    discountAmount = JsonConvert.DeserializeObject<decimal>(Request.Cookies["DiscountAmount"]);
                if (Request.Cookies["CouponTitle"] != null)
                    couponCode = Request.Cookies["CouponTitle"];
                if (Request.Cookies["ShippingOption"] != null)
                    shippingOption = Request.Cookies["ShippingOption"];

                await Checkout(shippingCost, discountAmount, couponCode, orderId, shippingOption, "MoMo");
            }

            return View("PaymentCallBack", result);
        }


        [HttpPost]
        public IActionResult VnpayPayment(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ" });
            }

            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new();
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống" });
            }

            decimal total = cart.Sum(i => i.Price * i.Quantity);
            total += model.ShippingCost;
            total -= model.DiscountAmount;

            var paymentModel = new PaymentInformationModel
            {
                Name = model.FullName,
                Amount =(double) total,
                OrderDescription = "Thanh toán đơn hàng tại Tiệm đồ chơi của Di",
                OrderType = "other"
            };

            var url = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
            return Json(new { paymentUrl = url });
        }


        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> VnpayReturn()
        {
            var response = Request.Query;
            string resultCode = response["vnp_ResponseCode"];
            string amount = response["vnp_Amount"];
            string orderInfo = response["vnp_OrderInfo"];
            string orderId = response["vnp_TxnRef"]; // hoặc response["vnp_OrderId"] nếu có

            bool isSuccess = resultCode == "00";

            if (isSuccess)
            {
                // ✅ Thêm vào database: bạn có thể ghi lịch sử giao dịch tại đây nếu muốn

                // Lấy dữ liệu từ Cookie
                decimal shippingCost = 0;
                decimal discountAmount = 0;
                string couponCode = "";
                string shippingOption = "standard";

                if (Request.Cookies["ShippingPrice"] != null)
                    shippingCost = JsonConvert.DeserializeObject<decimal>(Request.Cookies["ShippingPrice"]);
                if (Request.Cookies["DiscountAmount"] != null)
                    discountAmount = JsonConvert.DeserializeObject<decimal>(Request.Cookies["DiscountAmount"]);
                if (Request.Cookies["CouponTitle"] != null)
                    couponCode = Request.Cookies["CouponTitle"];
                if (Request.Cookies["ShippingOption"] != null)
                    shippingOption = Request.Cookies["ShippingOption"];

                // 🧠 Gọi lại hàm Checkout để lưu đơn hàng
                await Checkout(shippingCost, discountAmount, couponCode, orderId, shippingOption, "VNPAY");

                TempData["success"] = $"Thanh toán thành công: {orderInfo} - Số tiền: {decimal.Parse(amount) / 100:N0}₫";
            }
            else
            {
                TempData["error"] = "Thanh toán thất bại hoặc bị hủy.";
            }

            return RedirectToAction("ThankYou");
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                TempData["Error"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction("Index");
            }

            couponCode = couponCode.Trim().ToLower(); // chuẩn hóa

            decimal total = GetCartTotal();
            decimal discountAmount = 0;

            // ✅ Xử lý mã dạng "giam10", "giam30"
            if (couponCode.StartsWith("giam") && int.TryParse(couponCode.Substring(4), out int percent))
            {
                if (percent > 0 && percent <= 100)
                {
                    discountAmount = total * percent / 100;

                    // Lưu cookie
                    Response.Cookies.Append("CouponTitle", couponCode, new CookieOptions { Expires = DateTime.Now.AddMinutes(30) });
                    Response.Cookies.Append("DiscountAmount", discountAmount.ToString(), new CookieOptions { Expires = DateTime.Now.AddMinutes(30) });

                    TempData["Success"] = $"Áp dụng mã {couponCode.ToUpper()} - giảm {percent}% ({discountAmount:N0}₫)";
                    return RedirectToAction("Index");
                }
            }

            // ❗Nếu không phải dạng "giam10", thì kiểm tra trong database như cũ
            var coupon = _dataContext.Coupons
                .FirstOrDefault(c => c.Name.ToLower() == couponCode && c.Status == 1
                    && c.DateStart <= DateTime.Now && c.DateExpired >= DateTime.Now && c.Quantity > 0);

            if (coupon == null)
            {
                TempData["Error"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Index");
            }

            // Nếu là mã trong DB
            if (coupon.IsPercentage)
            {
                discountAmount = total * (coupon.Value / 100);
            }
            else
            {
                discountAmount = coupon.Value;
            }

            if (discountAmount > total)
                discountAmount = total;

            Response.Cookies.Append("CouponTitle", coupon.Name, new CookieOptions { Expires = DateTime.Now.AddMinutes(30) });
            Response.Cookies.Append("DiscountAmount", discountAmount.ToString(), new CookieOptions { Expires = DateTime.Now.AddMinutes(30) });

            TempData["Success"] = $"Đã áp dụng mã giảm giá {coupon.Name} - giảm {(coupon.IsPercentage ? $"{coupon.Value}%" : $"{discountAmount:N0}₫")}.";

            return RedirectToAction("Index");
        }




    }


}

