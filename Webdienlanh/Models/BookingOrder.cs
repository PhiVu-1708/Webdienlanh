using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webdienlanh.Models
{
    public class BookingOrder
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian")]
        public DateTime ServiceDate { get; set; }

        public string? Notes { get; set; } // Dấu ? nghĩa là có thể để trống

        public string SelectedServices { get; set; } // Chứa danh sách dịch vụ khách chọn

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Tự động lưu ngày đặt

        // Trạng thái đơn hàng (0: Chờ nhận, 1: Đang đi sửa, 2: Đã hoàn thành)
        public int Status { get; set; } = 0;

        // Phương thức thanh toán ("Tiền mặt" hoặc "Chuyển khoản")
        public string PaymentMethod { get; set; } = "Tiền mặt";

        // ==========================================================
        // BỔ SUNG: KHÓA NGOẠI LIÊN KẾT VỚI BẢNG THỢ (TECHNICIANS)
        // ==========================================================
        public int? TechnicianId { get; set; } // Cho phép null khi đơn chưa có thợ nhận

        [ForeignKey("TechnicianId")]
        public virtual Technician? Technician { get; set; }
    }
}