using System;
using System.ComponentModel.DataAnnotations;

namespace Webdienlanh.Models
{
    public class Technician
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Số dư ví khởi tạo mặc định là 10.000.000đ
        public decimal WalletBalance { get; set; } = 10000000m;
    }
}