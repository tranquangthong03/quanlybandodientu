using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyShopDienTu.Data;
using QuanLyShopDienTu.Models;

namespace QuanLyShopDienTu.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Dùng để quản lý file ảnh

        public SanPhamController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper method để chuẩn bị danh sách Danh mục
        private void PopulateDanhMucs(object selectedDanhMuc = null)
        {
            var danhMucsQuery = _context.DanhMuc.OrderBy(d => d.TenDanhMuc);
            ViewBag.DanhMucId = new SelectList(danhMucsQuery, "Id", "TenDanhMuc", selectedDanhMuc);
        }

        // GET: /SanPham/Index
        // Hiển thị danh sách sản phẩm (bao gồm tên danh mục)
        public async Task<IActionResult> Index()
        {
            // Eager loading: Bao gồm thông tin DanhMuc cho mỗi SanPham
            var sanPhams = _context.SanPham.Include(s => s.DanhMuc);
            return View(await sanPhams.ToListAsync());
        }

        // GET: /SanPham/Create
        public IActionResult Create()
        {
            PopulateDanhMucs();
            return View();
        }

        // POST: /SanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham, IFormFile HinhAnh)
        {
            // Loại bỏ lỗi ModelState liên quan đến thuộc tính điều hướng (DanhMuc)
            ModelState.Remove(nameof(sanPham.DanhMuc));

            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (HinhAnh != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + HinhAnh.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnh.CopyToAsync(fileStream);
                    }
                    sanPham.HinhAnhUrl = uniqueFileName;
                }
                else
                {
                    sanPham.HinhAnhUrl = "default.jpg"; // Gán ảnh mặc định nếu không upload
                }

                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            PopulateDanhMucs(sanPham.DanhMucId);
            return View(sanPham);
        }

        // GET: /SanPham/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPham.FindAsync(id);
            if (sanPham == null) return NotFound();

            PopulateDanhMucs(sanPham.DanhMucId);
            return View(sanPham);
        }

        // POST: /SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sanPham, IFormFile HinhAnh)
        {
            if (id != sanPham.Id) return NotFound();

            ModelState.Remove(nameof(sanPham.DanhMuc));

            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh (tương tự như Create, cần thêm logic xóa ảnh cũ nếu có)
                if (HinhAnh != null)
                {
                    // [Tùy chọn: Thêm logic xóa ảnh cũ ở đây]
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + HinhAnh.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnh.CopyToAsync(fileStream);
                    }
                    sanPham.HinhAnhUrl = uniqueFileName;
                }

                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SanPham.Any(e => e.Id == sanPham.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDanhMucs(sanPham.DanhMucId);
            return View(sanPham);
        }

        // GET: /SanPham/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Bao gồm DanhMuc để hiển thị tên khi xóa
            var sanPham = await _context.SanPham.Include(s => s.DanhMuc)
                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (sanPham == null) return NotFound();

            return View(sanPham);
        }

        // POST: /SanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanPham = await _context.SanPham.FindAsync(id);
            if (sanPham != null)
            {
                // [Tùy chọn: Thêm logic xóa file ảnh vật lý khỏi server]

                _context.SanPham.Remove(sanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}