using buytoy.Models;
using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TestController : Controller
    {
        private readonly DataContext _context;

        public TestController(DataContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var data = await _context.tests.ToListAsync();
            return View(data);
        }

        
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(test model)
        {
            if (ModelState.IsValid)
            {
                
                _context.tests.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

      
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.tests.FindAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(test model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.tests.FindAsync(model.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = model.Name;
                existing.Create = model.Create;
                existing.Update = model.Update;
                existing.Delete = model.Delete;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }


        
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.tests.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.tests.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
