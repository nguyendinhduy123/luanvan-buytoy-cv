using buytoy.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace buytoy.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=LAPTOP-MKKBPORT\\SQLEXPRESS;Database=bandochoi;Trusted_Connection=True;TrustServerCertificate=True;Command Timeout=180;");
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<RatingModel> Ratings { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }

        public DbSet<ProductModel> Products { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        public DbSet<ContactModel> Contact { get; set; }
        
        public DbSet<WishlistModel> Wishlists { get; set; }
        public DbSet<CompareModel> Compares { get; set; }
        public DbSet<ProductQuantityModel> Quantities { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }

        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<StatisticalModel> Statisticals { get; set; }
        public DbSet<MomoInfoModel> MomoInfos { get; set; }
        public DbSet<SupplierModel> Suppliers { get; set; }
        public DbSet<ImportReceiptModel> ImportReceipts { get; set; }
        public DbSet<ImportReceiptDetail> ImportReceiptDetails { get; set; }
        public DbSet<NewsModel> News { get; set; }
        public DbSet<VnpayModel> Vnpays { get; set; }
        public DbSet<test> tests { get; set; }
        public DbSet<ContactMessageModel> ContactMessages { get; set; }

    }
}
