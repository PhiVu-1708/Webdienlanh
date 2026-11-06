using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Webdienlanh.Models;

namespace Webdienlanh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // Biến kết nối cơ sở dữ liệu

        // Tiêm cả Logger và AppDbContext vào Controller
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Hiển thị trang Đặt Lịch
        public IActionResult Booking()
        {
            return View();
        }

        // POST: Nhận dữ liệu đặt lịch từ form và lưu vào SQL Server
        [HttpPost]
        public IActionResult Booking(BookingOrder order, List<string>? services)
        {
            // 1. Kiểm tra danh sách dịch vụ (hỗ trợ cả biến services lẫn thuộc tính SelectedServices)
            if ((services == null || services.Count == 0) && string.IsNullOrEmpty(order.SelectedServices))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 dịch vụ!";
                return RedirectToAction("Booking");
            }

            // Gộp các dịch vụ thành chuỗi văn bản nếu nhận dữ liệu từ List<string>
            if (services != null && services.Count > 0)
            {
                order.SelectedServices = string.Join(", ", services);
            }

            // 2. Gán mặc định phương thức thanh toán nếu giao diện chưa truyền giá trị
            if (string.IsNullOrEmpty(order.PaymentMethod))
            {
                order.PaymentMethod = "Tiền mặt";
            }

            // 3. Khởi tạo ngày đặt và trạng thái đơn ban đầu (0: Chờ thợ nhận)
            order.CreatedAt = DateTime.Now;
            order.Status = 0;

            // 4. Lưu đơn đặt hàng mới vào CSDL
            _context.BookingOrders.Add(order);
            _context.SaveChanges();

            // 5. Gửi thông báo thành công ra màn hình
            TempData["SuccessMessage"] = $"Cảm ơn {order.CustomerName}! Yêu cầu đặt lịch đã được ghi nhận. Kỹ thuật viên sẽ gọi lại trong 5 phút.";

            return RedirectToAction("Booking");
        }

        // =======================================================
        // TÍNH NĂNG TRA CỨU ĐƠN HÀNG "VIP" BẰNG SỐ ĐIỆN THOẠI
        // =======================================================

        // Xử lý tra cứu tiến độ (Dùng trực tiếp cho View TrackOrder.cshtml)
        [HttpGet]
        [HttpPost]
        public IActionResult TrackOrder(string phone)
        {
            // Luôn nạp danh sách kỹ thuật viên để View render thông tin thợ đã nhận ca
            ViewBag.Technicians = _context.Technicians.ToList();

            if (string.IsNullOrEmpty(phone))
            {
                return View();
            }

            // Tìm tất cả đơn hàng khớp với số điện thoại (sắp xếp từ mới nhất)
            var orders = _context.BookingOrders
                .Where(o => o.Phone == phone)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            ViewBag.SearchedPhone = phone;

            return View(orders);
        }

        // Điều hướng phụ: giữ lại Tracking để nếu menu/link cũ bấm vào không bị 404
        public IActionResult Tracking(string phoneNumber)
        {
            return RedirectToAction("TrackOrder", new { phone = phoneNumber });
        }

        // Trang Quản Trị: Xem danh sách khách hàng đặt lịch
        public IActionResult AdminBookings()
        {
            var orders = _context.BookingOrders.OrderByDescending(o => o.CreatedAt).ToList();
            return View(orders);
        }

        // Hàm gọi trang Quy Trình
        public IActionResult Workflow()
        {
            return View();
        }

        // Hàm gọi trang Chính Sách
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}