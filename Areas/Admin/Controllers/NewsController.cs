using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : Controller
    {
        private readonly DataContext _context;

        public NewsController(DataContext context)
        {
            _context = context;
        }

        // 👉 Hiển thị tin mới (trong 30 ngày)
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var archivedDate = now.AddDays(-1);

            var recentNews = await _context.News
                .Where(n => n.CreatedAt >= archivedDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(recentNews);
        }

        // 👉 Hiển thị tin cũ (trước 30 ngày)
        public async Task<IActionResult> Archive()
        {
            var archivedDate = DateTime.Now.AddDays(-1);

            var oldNews = await _context.News
                .Where(n => n.CreatedAt < archivedDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(oldNews);
        }

        // 👉 Trang tạo tin tức mới
        public IActionResult Create()
        {
            return View();
        }

        // 👉 Xử lý tạo tin tức mới
        [HttpPost]
        public async Task<IActionResult> Create(NewsModel model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now; // Ghi thời gian tạo
                _context.News.Add(model);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm tin tức thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 👉 Xóa tin tức
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.News.FindAsync(id);
            if (item != null)
            {
                _context.News.Remove(item);
                await _context.SaveChangesAsync();
                TempData["success"] = "Xóa tin tức thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
