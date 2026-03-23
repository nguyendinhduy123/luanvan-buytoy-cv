using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    [Authorize(Roles = "Publisher,Author,Admin")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _env;

        public BrandController(DataContext context, IWebHostEnvironment env)
        {
            _dataContext = context;
            _env = env;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<BrandModel> brand = _dataContext.Brands.ToList();
            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = brand.Count();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

            var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();
            ViewBag.Pager = pager;

            return View(data);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                var existing = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (existing != null)
                {
                    ModelState.AddModelError("", "Tên thương hiệu đã tồn tại.");
                    return View(brand);
                }

                // Xử lý ảnh nếu có upload
                if (brand.ImageUpload != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "media/brands");
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(brand.ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await brand.ImageUpload.CopyToAsync(stream);
                    }

                    brand.Image = fileName;
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm thương hiệu thành công";
                return RedirectToAction("Index");
            }

            return View(brand);
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            var brand = await _dataContext.Brands.FindAsync(Id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                var existing = await _dataContext.Brands.FindAsync(brand.Id);
                if (existing == null) return NotFound();

                existing.Name = brand.Name;
                existing.Description = brand.Description;
                existing.Status = brand.Status;
                existing.Slug = brand.Name.Replace(" ", "-").ToLower();

                // Nếu có ảnh mới
                if (brand.ImageUpload != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "media/brands");
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(brand.ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await brand.ImageUpload.CopyToAsync(stream);
                    }

                    existing.Image = fileName;
                }

                _dataContext.Update(existing);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật thương hiệu thành công";
                return RedirectToAction("Index");
            }

            return View(brand);
        }

        public async Task<IActionResult> Delete(int Id)
        {
            var brand = await _dataContext.Brands.FindAsync(Id);
            if (brand == null) return NotFound();

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Thương hiệu đã được xóa";
            return RedirectToAction("Index");
        }
    }
}
