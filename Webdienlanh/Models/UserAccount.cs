namespace Webdienlanh.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string Username { get; set; } // Tên đăng nhập
        public string Password { get; set; } // Mật khẩu
        public string Role { get; set; }     // Phân quyền: "Admin" hoặc "Tho"
        public string FullName { get; set; } // Họ tên hiển thị
    }
}