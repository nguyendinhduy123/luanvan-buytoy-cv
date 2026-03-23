using buytoy.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace buytoy.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel bupbe = new CategoryModel { Name = "Bupbe", Slug = "bupbe", Description = "bupbe is cute in the world", Status = 1 };
                CategoryModel lego = new CategoryModel { Name = "Logo", Slug = "logo", Description = "logo is cute in the world", Status = 1 };
                BrandModel disney = new BrandModel { Name = "Disney", Slug = "disney", Description = "disney is brand in the world", Status = 1 };

                BrandModel ninjago = new BrandModel { Name = "Ninjago", Slug = "ninjago", Description = "ninjago is brand in the world", Status = 1 };
                _context.Products.AddRange(
                    new ProductModel { Name = "Bupbe", Slug = "bupbe", Description = "Bupbe is the best ", Image = "1.jpg", Category = bupbe,Brand = disney, Price = 20000 },

                new ProductModel { Name = "Ninjago", Slug = "ninjago", Description = "Ninjago is the best ", Image = "2.jpg", Category = lego,Brand = ninjago , Price = 30000 }
                );
                _context.SaveChanges();
            }
        }
    }
}
