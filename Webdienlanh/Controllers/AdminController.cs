using Microsoft.AspNetCore.Mvc;
using Webdienlanh.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace Webdienlanh.Controllers
{
    // ==========================================
    // CÁC LỚP DỮ LIỆU VIEW MODEL (DTO)
    // ==========================================
    public class TechViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public decimal WalletBalance { get; set; }
        public string CCCD { get; set; }
        public string DOB { get; set; }
        public string Origin { get; set; }
        public string Address { get; set; }
        public string ImagePath { get; set; }
    }

    public class TechApplication
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Area { get; set; }
        public string Experience { get; set; }
        public string Services { get; set; }
        public string Status { get; set; }
        public DateTime AppliedDate { get; set; }
    }

    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        // DANH SÁCH ỨNG VIÊN CHỜ DUYỆT TẠM THỜI TRONG BỘ NHỚ
        public static List<TechApplication> PendingApplications = new List<TechApplication>();

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================
        // CỔNG NHẬN ĐƠN ỨNG TUYỂN TỪ FORM ĐĂNG KÝ NGOÀI TRANG CHỦ
        // ==========================================
        [HttpPost]
        public IActionResult SubmitApplication(string fullName, string phone, string area, string experience, string services)
        {
            try
            {
                int newId = PendingApplications.Count > 0 ? PendingApplications.Max(a => a.Id) + 1 : 1;

                var newApp = new TechApplication
                {
                    Id = newId,
                    FullName = fullName?.Trim(),
                    Phone = phone?.Trim(),
                    Area = area,
                    Experience = experience,
                    Services = services,
                    Status = "Chờ duyệt",
                    AppliedDate = DateTime.Now
                };

                PendingApplications.Add(newApp);
                TempData["SuccessMessage"] = "Gửi hồ sơ thành công! BPV sẽ liên hệ duyệt trong vòng 24h.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nhận hồ sơ ứng tuyển.");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi nộp hồ sơ. Vui lòng thử lại!";
            }

            return RedirectToRefererOrDefault("Index");
        }

        // ==========================================
        // 1. TRANG CHỦ ADMIN (INDEX)
        // ==========================================
        public IActionResult Index()
        {
            var orders = _context.BookingOrders.OrderByDescending(o => o.CreatedAt).ToList();
            var technicians = GetRealTechnicians(); // Lấy danh sách thợ thật từ DB

            ViewBag.Technicians = technicians;
            ViewBag.OnlineTechs = technicians.Select(t => new TechViewModel
            {
                Id = t.Id,
                Name = t.FullName,
                Status = "Rảnh",
                Avatar = !string.IsNullOrEmpty(t.FullName) ? t.FullName.Substring(0, 1).ToUpper() : "T",
                Phone = t.Phone,
                WalletBalance = t.WalletBalance
            }).ToList();

            return View(orders);
        }

        // ==========================================
        // 2. TRANG QUẢN LÝ ĐƠN HÀNG
        // ==========================================
        public IActionResult ManageOrders()
        {
            var orders = _context.BookingOrders.OrderByDescending(o => o.CreatedAt).ToList();
            ViewBag.OnlineTechs = GetRealTechnicians();
            return View(orders);
        }

        // ==========================================
        // 3. TRANG QUẢN LÝ THỢ (HỒ SƠ THẬT)
        // ==========================================
        public IActionResult ManageTechs()
        {
            var technicians = GetRealTechnicians();
            var techProfiles = technicians.Select(t => {
                var model = new TechViewModel
                {
                    Id = t.Id,
                    Name = t.FullName ?? "Chưa cập nhật",
                    Phone = t.Phone ?? "Chưa cập nhật",
                    WalletBalance = t.WalletBalance,
                    Status = "Đang hoạt động"
                };

                // Demo logic hiển thị chi tiết (Có thể mở rộng sau nếu cần)
                if (model.Name.Contains("Vũ")) { model.CCCD = "074206010227"; model.DOB = "17/08/2006"; model.Origin = "Tiền Giang"; model.Address = "Bình Dương"; model.ImagePath = "/images/vu.png"; }
                else if (model.Name.Contains("Pháp")) { model.CCCD = "077206000865"; model.DOB = "31/10/2006"; model.Origin = "Quảng Ngãi"; model.Address = "Bình Dương"; model.ImagePath = "/images/phap.png"; }
                else if (model.Name.Contains("Bình")) { model.CCCD = "042206006341"; model.DOB = "25/08/2006"; model.Origin = "Hà Tĩnh"; model.Address = "Hà Tĩnh"; model.ImagePath = "/images/binh.jpg"; }
                else { model.CCCD = "Đã xác thực"; model.DOB = "Bảo mật"; model.Origin = "Bảo mật"; model.Address = "Hệ thống BPV"; model.ImagePath = ""; }

                return model;
            }).ToList();

            return View(techProfiles);
        }

        // ==========================================
        // 4. CHỨC NĂNG DUYỆT HỒ SƠ ỨNG VIÊN
        // ==========================================
        public IActionResult ManageApplications()
        {
            var apps = PendingApplications.OrderByDescending(a => a.AppliedDate).ToList();
            return View(apps);
        }

        [HttpPost]
        public IActionResult ApproveApplication(int id)
        {
            var app = PendingApplications.FirstOrDefault(a => a.Id == id);
            if (app != null && app.Status == "Chờ duyệt")
            {
                try
                {
                    app.Status = "Đã duyệt";
                    // Tự động tạo và lưu thợ mới vào SQL Server
                    var newTech = new Technician
                    {
                        FullName = app.FullName,
                        Phone = app.Phone,
                        WalletBalance = 0
                    };

                    _context.Technicians.Add(newTech);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = $"Duyệt thành công! {app.FullName} đã trở thành đối tác chính thức.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lưu KTV mới vào Database.");
                    app.Status = "Chờ duyệt"; // Hoàn tác trạng thái nếu lưu DB lỗi
                    TempData["ErrorMessage"] = "Lỗi hệ thống khi lưu vào cơ sở dữ liệu. Vui lòng thử lại.";
                }
            }
            return RedirectToAction("ManageApplications");
        }

        [HttpPost]
        public IActionResult RejectApplication(int id)
        {
            var app = PendingApplications.FirstOrDefault(a => a.Id == id);
            if (app != null && app.Status == "Chờ duyệt")
            {
                app.Status = "Từ chối";
                TempData["ErrorMessage"] = $"Đã từ chối đơn ứng tuyển của {app.FullName}.";
            }
            return RedirectToAction("ManageApplications");
        }

        // ==========================================
        // 5. CÁC CHỨC NĂNG XỬ LÝ DỮ LIỆU CŨ (An toàn hóa)
        // ==========================================
        [HttpPost]
        public IActionResult CreateTech(string fullName, string phone, decimal walletBalance)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Tên và Số điện thoại!";
                return RedirectToRefererOrDefault("ManageTechs");
            }

            try
            {
                _context.Technicians.Add(new Technician
                {
                    FullName = fullName.Trim(),
                    Phone = phone.Trim(),
                    WalletBalance = walletBalance >= 0 ? walletBalance : 0
                });
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã thêm kỹ thuật viên {fullName} thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm KTV thủ công.");
                TempData["ErrorMessage"] = "Lỗi Database khi thêm kỹ thuật viên mới!";
            }

            return RedirectToRefererOrDefault("ManageTechs");
        }

        [HttpPost]
        public IActionResult AddFunds(int technicianId, decimal amount)
        {
            var tech = _context.Technicians.Find(technicianId);
            if (tech != null && amount > 0)
            {
                try
                {
                    tech.WalletBalance += amount;
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Đã nạp {amount:N0}đ vào ví của {tech.FullName}!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi nạp tiền.");
                    TempData["ErrorMessage"] = "Lỗi Database khi thực hiện giao dịch nạp tiền!";
                }
            }
            return RedirectToRefererOrDefault("ManageTechs");
        }

        // ==========================================
        // 6. TỐI THƯỢNG: XỬ LÝ PHÂN CÔNG THỢ CHUẨN XÁC VÀ AN TOÀN
        // ==========================================
        [HttpPost]
        public IActionResult AssignTech(int orderId, int technicianId)
        {
            try
            {
                var order = _context.BookingOrders.Find(orderId);
                var tech = _context.Technicians.Find(technicianId);

                // KIỂM TRA CHẶT CHẼ TRƯỚC KHI GÁN FOREIGN KEY
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng này không còn tồn tại trên hệ thống!";
                    return RedirectToRefererOrDefault("ManageOrders");
                }

                if (tech == null)
                {
                    TempData["ErrorMessage"] = "Lỗi: Kỹ thuật viên bạn chọn không tồn tại trong cơ sở dữ liệu (Chưa được duyệt chính thức).";
                    return RedirectToRefererOrDefault("ManageOrders");
                }

                // Tiền hành lưu dữ liệu
                order.Status = 1; // Đang xử lý
                order.TechnicianId = technicianId;
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Phân công đơn #{orderId} cho KTV {tech.FullName} thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi cập nhật DB - AssignTech.");
                TempData["ErrorMessage"] = "Hệ thống SQL từ chối thao tác phân công. Vui lòng kiểm tra lại sự đồng bộ dữ liệu.";
            }

            return RedirectToRefererOrDefault("ManageOrders");
        }

        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.BookingOrders.Find(id);
            if (order != null)
            {
                try
                {
                    _context.BookingOrders.Remove(order);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Đã xóa đơn hàng khỏi hệ thống!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa đơn hàng.");
                    TempData["ErrorMessage"] = "Lỗi Database khi xóa đơn hàng!";
                }
            }
            return RedirectToRefererOrDefault("ManageOrders");
        }

        public IActionResult ExportExcel()
        {
            var orders = _context.BookingOrders.OrderByDescending(o => o.CreatedAt).ToList();
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("STT,Khách Hàng,Số Điện Thoại,Dịch Vụ,Thời Gian Hẹn,Trạng Thái");

            int i = 1;
            foreach (var item in orders)
            {
                string status = item.Status == 0 ? "Chờ thợ nhận" : (item.Status == 1 ? "Đang đi sửa" : "Đã hoàn thành");
                builder.AppendLine($"{i},{item.CustomerName},{item.Phone},\"{item.SelectedServices}\",{item.ServiceDate:dd/MM/yyyy HH:mm},{status}");
                i++;
            }
            return File(System.Text.Encoding.UTF8.GetBytes('\uFEFF' + builder.ToString()), "text/csv", "DanhSachDonHang_BPVStore.csv");
        }

        // ==========================================
        // HELPER: LẤY DỮ LIỆU THỢ (Đã xóa bỏ hoàn toàn dữ liệu ảo)
        // ==========================================
        private List<Technician> GetRealTechnicians()
        {
            // Trả về duy nhất dữ liệu THẬT tồn tại trong SQL, không tạo dữ liệu rác.
            return _context.Technicians.ToList();
        }

        private IActionResult RedirectToRefererOrDefault(string defaultAction)
        {
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction(defaultAction);
        }
    }
}