using Microsoft.AspNetCore.Mvc;
using Webdienlanh.Models;

namespace Webdienlanh.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly AppDbContext _context;

        public TechnicianController(AppDbContext context)
        {
            _context = context;
        }

        // Trang công việc của Thợ (Hiển thị danh sách đơn & Số dư ví thực tế)
        public IActionResult Index()
        {
            var orders = _context.BookingOrders.OrderByDescending(o => o.ServiceDate).ToList();

            // Lấy thông tin tài khoản thợ đầu tiên từ CSDL để hiển thị ví
            var tech = _context.Technicians.FirstOrDefault();
            if (tech != null)
            {
                ViewBag.WalletBalance = tech.WalletBalance;
            }
            else
            {
                ViewBag.WalletBalance = 500000m; // Mặc định 500k nếu chưa có dữ liệu thợ
            }

            return View(orders);
        }

        // Chức năng để Thợ NHẬN ĐƠN (status = 1) hoặc ĐANG ĐẾN (status = 2)
        [HttpPost]
        public IActionResult UpdateStatus(int id, int status)
        {
            var order = _context.BookingOrders.Find(id);
            var tech = _context.Technicians.FirstOrDefault(); // Lấy thợ hiện tại

            if (order != null)
            {
                // BƯỚC 1: KIỂM TRA & NHẬN CA (Status = 1)
                if (status == 1)
                {
                    // Chặn nếu ví dưới 500k
                    if (tech != null && tech.WalletBalance < 500000m)
                    {
                        TempData["ErrorMessage"] = "Số dư ví của bạn hiện tại không đủ 500.000đ. Vui lòng nạp thêm tiền để tiếp tục nhận đơn mới!";
                        return RedirectToAction("Index");
                    }

                    // Gán ID của thợ vào đơn hàng để khách hàng nhìn thấy nút Chat/Gọi điện!
                    if (tech != null)
                    {
                        order.TechnicianId = tech.Id;
                    }

                    TempData["SuccessMessage"] = "Đã nhận đơn thành công! Hãy liên hệ khách hàng ngay nhé.";
                }

                // BƯỚC 2: THỢ XEM BẢN ĐỒ VÀ BÁO ĐANG ĐẾN (Status = 2)
                else if (status == 2)
                {
                    TempData["SuccessMessage"] = "Đã mở bản đồ! Hệ thống đã báo cho khách biết bạn đang đến.";
                }

                // Cập nhật trạng thái
                order.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Chức năng HOÀN THÀNH ĐƠN (Status = 3) & TÍNH TOÁN DÒNG TIỀN VÍ THỢ
        [HttpPost]
        public IActionResult CompleteOrder(int id, bool isTransfer, decimal orderTotal = 250000)
        {
            var order = _context.BookingOrders.Find(id);
            var tech = _context.Technicians.FirstOrDefault();

            if (order != null)
            {
                order.Status = 3; // Đã đổi thành 3 cho chuẩn quy trình 4 bước

                if (tech != null && orderTotal > 0)
                {
                    // Đã sửa lỗi cú pháp nhân chia
                    decimal platformFee = orderTotal * 0.10m; // Tính 10% phí sàn

                    if (isTransfer)
                    {
                        // TRƯỜNG HỢP 1: CHUYỂN KHOẢN (Tiền về Admin, thợ nhận 90%)
                        decimal technicianShare = orderTotal - platformFee;
                        tech.WalletBalance += technicianShare;

                        TempData["SuccessMessage"] = $"Khách đã chuyển khoản thành công! Sàn giữ {platformFee:N0}đ (10% phí). Ví của bạn được CỘNG {technicianShare:N0}đ.";
                    }
                    else
                    {
                        // TRƯỜNG HỢP 2: TIỀN MẶT (Thợ giữ 100% tiền, hệ thống trừ 10% phí vào ví)
                        tech.WalletBalance -= platformFee;

                        TempData["SuccessMessage"] = $"Hoàn thành đơn tiền mặt! Bạn thu trực tiếp {orderTotal:N0}đ. Hệ thống đã TRỪ {platformFee:N0}đ (10% phí sàn) vào ví.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "Tuyệt vời! Đã hoàn thành công việc.";
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Chức năng TRẢ ĐƠN HOẶC HOÀN TÁC (Đưa về status 0 và hoàn trả lại số dư ví tương ứng)
        [HttpPost]
        public IActionResult ReturnOrder(int id)
        {
            var order = _context.BookingOrders.Find(id);
            var tech = _context.Technicians.FirstOrDefault();

            if (order != null)
            {
                // Nếu đơn hàng trước đó đã hoàn thành (status = 3) mà nay thợ bấm hoàn tác về 0
                if (order.Status == 3 && tech != null)
                {
                    decimal estimatedTotal = 250000m; // Giá trị đơn mẫu
                    decimal platformFee = estimatedTotal * 0.10m; // Đã sửa cú pháp

                    if (order.PaymentMethod == "Chuyển khoản")
                    {
                        // Hoàn tác chuyển khoản: Trừ lại phần đã cộng trước đó vào ví
                        decimal technicianShare = estimatedTotal - platformFee; // Đã sửa cú pháp
                        tech.WalletBalance -= technicianShare;
                    }
                    else
                    {
                        // Hoàn tác tiền mặt: Cộng trả lại 10% phí đã bị trừ trước đó vào ví
                        tech.WalletBalance += platformFee;
                    }
                }

                // Khi trả đơn, nhớ xóa luôn thông tin người thợ khỏi đơn hàng
                order.TechnicianId = null;
                order.Status = 0; // Trả về trạng thái Chờ nhận ban đầu

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đã hoàn tác đơn hàng về trạng thái chờ nhận và cập nhật lại số dư ví!";
            }
            return RedirectToAction("Index");
        }
    }
}