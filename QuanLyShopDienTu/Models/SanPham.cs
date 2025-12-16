namespace QuanLyShopDienTu.Models
{
    public class SanPham
    {
        public int Id { get; set; }
        public string TenSanPham { get; set; }
        public decimal GiaTien { get; set; }
        public int SoLuong { get; set; }
        public string HinhAnhUrl { get; set; }
        public string MoTa { get; set; }

        // Khóa ngoại (Foreign Key)
        public int DanhMucId { get; set; }
        public DanhMuc DanhMuc { get; set; }
    }
}