using Microsoft.EntityFrameworkCore;

namespace Webdienlanh.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng trong cơ sở dữ liệu
        public DbSet<BookingOrder> BookingOrders { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; } // Bảng quản lý tài khoản

        public DbSet<Technician> Technicians { get; set; } // Bảng quản lý tài khoản dành riêng cho Thợ

        public DbSet<Message> Messages { get; set; } // THÊM MỚI: Bảng lưu tin nhắn chat giữa Khách và Thợ
    }
}