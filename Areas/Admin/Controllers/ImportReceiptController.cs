using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImportReceiptController : Controller
    {
        private readonly DataContext _context;

        public ImportReceiptController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.ImportReceipts.Include(r => r.Supplier).ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name");
            ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int SupplierId, List<int> ProductId, List<int> Quantity, List<decimal> Price)
        {
            decimal total = 0;
            for (int i = 0; i < Price.Count; i++)
            {
                total += Price[i] * Quantity[i];
            }

            var receipt = new ImportReceiptModel
            {
                SupplierId = SupplierId,
                ImportDate = DateTime.Now,
                TotalAmount = total
            };

            _context.Add(receipt);
            await _context.SaveChangesAsync();

            for (int i = 0; i < ProductId.Count; i++)
            {
                var detail = new ImportReceiptDetail
                {
                    ImportReceiptId = receipt.Id,
                    ProductId = ProductId[i],
                    Quantity = Quantity[i],
                    Price = Price[i]
                };
                _context.Add(detail);

                var product = await _context.Products.FindAsync(ProductId[i]);
                product.Quantity += Quantity[i];
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã thêm phiếu nhập.";
            return RedirectToAction("Index");
        }
    }
}
