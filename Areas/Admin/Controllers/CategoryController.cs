using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Category")]
    [Authorize(Roles = "Publisher,Author,Admin")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _env;

        public CategoryController(DataContext context, IWebHostEnvironment env)
        {
            _dataContext = context;
            _env = env;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _dataContext.Categories.ToList();
            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = category.Count();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

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
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã có trong database");
                    return View(category);
                }

                if (category.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_env.WebRootPath, "media/categories");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(category.ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await category.ImageUpload.CopyToAsync(stream);
                    }
                    category.Image = imageName;
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");

                if (category.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_env.WebRootPath, "media/categories");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(category.ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await category.ImageUpload.CopyToAsync(stream);
                    }
                    category.Image = imageName;
                }

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);

            if (!string.IsNullOrEmpty(category.Image))
            {
                string filePath = Path.Combine(_env.WebRootPath, "media/categories", category.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Danh mục đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
