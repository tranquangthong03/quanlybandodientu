using Microsoft.EntityFrameworkCore;
using QuanLyShopDienTu.Models;

namespace QuanLyShopDienTu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SanPham> SanPham { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Đảm bảo tên bảng là SanPham và DanhMuc (không có 's')
            modelBuilder.Entity<SanPham>().ToTable("SanPham");
            modelBuilder.Entity<DanhMuc>().ToTable("DanhMuc");

            // Cấu hình DECIMAL cho GiaTien
            modelBuilder.Entity<SanPham>()
                .Property(p => p.GiaTien)
                .HasColumnType("decimal(18, 2)");
        }
    }
}