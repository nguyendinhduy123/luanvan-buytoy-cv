using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewController : Controller
    {
        private readonly DataContext _context;

        public ReviewController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Ratings
                .Include(r => r.Product)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Ratings.FindAsync(id);
            if (review == null) return NotFound();

            _context.Ratings.Remove(review);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa đánh giá thành công.";
            return RedirectToAction("Index");
        }
    }
}
