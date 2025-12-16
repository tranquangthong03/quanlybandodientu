namespace QuanLyShopDienTu.Models
{
    public class DanhMuc
    {
        public int Id { get; set; }
        public string TenDanhMuc { get; set; }
        public ICollection<SanPham> SanPhams { get; set; } // Liên kết 1-nhiều
    }
}