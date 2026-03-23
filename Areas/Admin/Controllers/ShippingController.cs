using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = "Publisher,Author,Admin")]
    public class ShippingController : Controller
    {
        private readonly DataContext _dataContext;

        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }

        // Hiển thị danh sách địa chỉ giao hàng
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var danhSachShipping = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shippings = danhSachShipping;
            return View();
        }

        // Lưu thông tin địa chỉ giao hàng mới từ form Ajax
        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, decimal price)
        {
            // Gán thông tin địa chỉ từ client gửi về
            shippingModel.City = tinh;
            shippingModel.District = quan;
            shippingModel.Ward = phuong;
            shippingModel.Price = price;

            try
            {
                // Kiểm tra trùng lặp theo Tỉnh + Quận + Phường
                var isTrungLap = await _dataContext.Shippings
                    .AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                if (isTrungLap)
                {
                    return Ok(new { duplicate = true, message = "Địa chỉ giao hàng đã tồn tại trong hệ thống." });
                }

                _dataContext.Shippings.Add(shippingModel);
                await _dataContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Thêm địa chỉ giao hàng thành công." });
            }
            catch (Exception)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi thêm địa chỉ giao hàng.");
            }
        }

        // Xóa địa chỉ giao hàng theo ID
        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _dataContext.Shippings.FindAsync(Id);

            if (shipping == null)
            {
                TempData["error"] = "Không tìm thấy địa chỉ giao hàng.";
                return RedirectToAction("Index");
            }

            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Địa chỉ giao hàng đã được xóa thành công.";
            return RedirectToAction("Index");
        }
    }
}
