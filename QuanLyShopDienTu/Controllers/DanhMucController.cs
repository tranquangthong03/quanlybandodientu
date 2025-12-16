using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyShopDienTu.Data;
using QuanLyShopDienTu.Models;
using System.Linq; // Cần thiết để dùng lệnh .Where() và .Contains()
using System.Threading.Tasks;

namespace QuanLyShopDienTu.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhMucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /DanhMuc/Index
        // Đã cập nhật: Thêm tham số searchString (có thể null)
        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Tạo câu truy vấn cơ bản lấy tất cả danh mục (chưa chạy xuống database)
            var danhMucs = from d in _context.DanhMuc
                           select d;

            // 2. Nếu có từ khóa tìm kiếm (người dùng nhập vào ô search)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Lọc danh sách theo tên (chứa từ khóa)
                danhMucs = danhMucs.Where(s => s.TenDanhMuc.Contains(searchString));

                // Lưu từ khóa lại để hiển thị trên ô input sau khi reload trang
                ViewData["CurrentFilter"] = searchString;
            }

            // 3. Thực thi truy vấn và trả về View
            return View(await danhMucs.ToListAsync());
        }

        // GET: /DanhMuc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /DanhMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDanhMuc")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: /DanhMuc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMuc.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound();
            }
            return View(danhMuc);
        }

        // POST: /DanhMuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenDanhMuc")] DanhMuc danhMuc)
        {
            if (id != danhMuc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DanhMuc.Any(e => e.Id == danhMuc.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: /DanhMuc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMuc.FirstOrDefaultAsync(m => m.Id == id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            return View(danhMuc);
        }

        // POST: /DanhMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMuc.FindAsync(id);
            if (danhMuc != null)
            {
                _context.DanhMuc.Remove(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}