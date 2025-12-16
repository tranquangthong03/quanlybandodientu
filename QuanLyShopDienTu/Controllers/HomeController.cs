using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyShopDienTu.Models;
using System.Data;
using System.Xml.Linq;

namespace QuanLyShopDienTu.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private string GetConnectionString() => _configuration.GetConnectionString("DefaultConnection");

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Trang chính: Hiển thị danh sách sản phẩm
        public IActionResult Index()
        {
            // Sử dụng ADO.NET cơ bản để lấy dữ liệu (hoặc dùng EF Core)
            List<SanPham> danhSach = new List<SanPham>();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT s.*, d.TenDanhMuc FROM SanPham s JOIN DanhMuc d ON s.DanhMucId = d.Id";
                conn.Open();
                using (SqlDataReader reader = new SqlCommand(query, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        danhSach.Add(new SanPham
                        {
                            Id = (int)reader["Id"],
                            TenSanPham = reader["TenSanPham"].ToString(),
                            GiaTien = (decimal)reader["GiaTien"],
                            SoLuong = (int)reader["SoLuong"],
                            HinhAnhUrl = reader["HinhAnhUrl"]?.ToString(),
                            MoTa = reader["MoTa"]?.ToString(),
                            DanhMucId = (int)reader["DanhMucId"],
                            // Tạo đối tượng DanhMuc tạm thời để hiển thị tên danh mục
                            DanhMuc = new DanhMuc { TenDanhMuc = reader["TenDanhMuc"].ToString() }
                        });
                    }
                }
            }
            return View(danhSach);
        }

        // ==========================================
        // CHỨC NĂNG XUẤT (EXPORT) XML
        // ==========================================
        [HttpGet]
        public IActionResult ExportToXml()
        {
            DataSet ds = new DataSet("ShopDienTuData");

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, TenSanPham, GiaTien, SoLuong, HinhAnhUrl, MoTa, DanhMucId FROM SanPham";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.Fill(ds, "SanPham");
            }

            // Đặt HinhAnhUrl thành Attribute (tùy chọn)
            if (ds.Tables["SanPham"]?.Columns.Contains("HinhAnhUrl") == true)
            {
                ds.Tables["SanPham"].Columns["HinhAnhUrl"].ColumnMapping = MappingType.Attribute;
            }

            MemoryStream stream = new MemoryStream();
            ds.WriteXml(stream, XmlWriteMode.IgnoreSchema);
            stream.Position = 0;

            return File(stream, "application/xml", "SanPham_Export.xml");
        }

        // ==========================================
        // CHỨC NĂNG NHẬP (IMPORT) XML
        // ==========================================
        [HttpPost]
        public IActionResult ImportFromXml(IFormFile xmlFile)
        {
            if (xmlFile == null || xmlFile.Length == 0)
            {
                TempData["Message"] = "Vui lòng chọn một tệp XML hợp lệ.";
                return RedirectToAction("Index");
            }

            try
            {
                DataSet ds = new DataSet();
                using (var stream = xmlFile.OpenReadStream())
                {
                    ds.ReadXml(stream);
                }

                if (ds.Tables.Contains("SanPham"))
                {
                    DataTable dt = ds.Tables["SanPham"];
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();

                        // Lệnh DELETE để làm sạch bảng trước khi Import
                        new SqlCommand("DELETE FROM SanPham", conn).ExecuteNonQuery();

                        // Sử dụng SqlBulkCopy để chèn dữ liệu nhanh chóng
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                        {
                            bulkCopy.DestinationTableName = "SanPham";

                            // Ánh xạ cột (Đảm bảo tên cột trùng khớp)
                            bulkCopy.ColumnMappings.Add("Id", "Id");
                            bulkCopy.ColumnMappings.Add("TenSanPham", "TenSanPham");
                            bulkCopy.ColumnMappings.Add("GiaTien", "GiaTien");
                            bulkCopy.ColumnMappings.Add("SoLuong", "SoLuong");
                            bulkCopy.ColumnMappings.Add("HinhAnhUrl", "HinhAnhUrl");
                            bulkCopy.ColumnMappings.Add("MoTa", "MoTa");
                            bulkCopy.ColumnMappings.Add("DanhMucId", "DanhMucId");

                            bulkCopy.WriteToServer(dt);
                        }
                    }
                    TempData["Message"] = $"Đã nhập thành công {dt.Rows.Count} sản phẩm từ XML.";
                }
                else
                {
                    TempData["Message"] = "Tệp XML không chứa bảng 'SanPham' hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Lỗi khi nhập XML: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}