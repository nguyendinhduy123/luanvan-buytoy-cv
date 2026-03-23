using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Admin/Product")]
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class ProductController : Controller
    {

        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ProductController(DataContext context ,  IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnviroment = webHostEnvironment;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong database");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
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
            return View(product);
        
    }
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }
       [Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductModel product)
		{
			var existed_product = _dataContext.Products.Find(product.Id); //tìm sp theo id product
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			if (ModelState.IsValid)
			{
				product.Slug = product.Name.Replace(" ", "-");

				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();
					existed_product.Image = imageName;
				}


                // Update other product properties
                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.Price = product.Price;
                existed_product.CategoryId = product.CategoryId;
                existed_product.BrandId = product.BrandId;
                existed_product.MinAge = product.MinAge;
                existed_product.MaxAge = product.MaxAge;

                // ... other properties
                _dataContext.Update(existed_product);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật sản phẩm thành công";
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
			return View(product);
		}
        [HttpPost]
        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (!string.Equals(product.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/products");
                string oldfilePath = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(oldfilePath))
                {
                    System.IO.File.Delete(oldfilePath);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "sản phẩm đã được xóa thành công";
            return RedirectToAction("Index");
        }
        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            
            var productbyquantity = await _dataContext.Quantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productbyquantity;
            ViewBag.ProductId = Id;
            var product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
                return NotFound();

            var model = new ProductQuantityModel
            {
                ProductId = product.Id,
                DateCreated = DateTime.Now
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("UpdateMoreQuantity")]
        public async Task<IActionResult> UpdateMoreQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = await _dataContext.Products.FindAsync(productQuantityModel.ProductId);
            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index");
            }

            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.DateCreated = DateTime.Now;

            _dataContext.Quantities.Add(productQuantityModel);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật số lượng thành công.";

            return RedirectToAction("Index"); // hoặc Edit nếu muốn
        }



    }
}
