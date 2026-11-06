using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Webdienlanh.Models;

namespace Webdienlanh.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Tiêm AppDbContext vào để Controller này có thể làm việc với CSDL
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Xử lý thông tin đăng nhập
        [HttpPost]
        public IActionResult Login(string username, string password, string role)
        {
            // 1. Nếu chọn quyền Admin (Dùng tài khoản cố định)
            if (role == "Admin" && username == "admin" && password == "123")
            {
                return RedirectToAction("Index", "Admin");
            }
            // 2. Nếu chọn quyền Thợ (Kiểm tra trong CSDL tài khoản vừa đăng ký)
            else if (role == "Tho")
            {
                // Giữ lại tài khoản mặc định lúc trước để dự phòng test nhanh
                if (username == "tho" && password == "123")
                {
                    return RedirectToAction("Index", "Technician");
                }

                // Kiểm tra tài khoản thợ trong SQL Server
                var validTech = _context.Technicians
                    .FirstOrDefault(t => t.Username == username && t.Password == password);

                if (validTech != null)
                {
                    return RedirectToAction("Index", "Technician");
                }
            }

            // Nếu sai toàn bộ
            TempData["Error"] = "Sai tài khoản, mật khẩu hoặc chọn sai quyền! (Gợi ý: admin/123 hoặc tài khoản Thợ vừa đăng ký)";
            return View();
        }

        // ==========================================
        // PHẦN MỚI THÊM: TÍNH NĂNG ĐĂNG KÝ THỢ
        // ==========================================

        // GET: Hiển thị form đăng ký thợ
        public IActionResult Register()
        {
            return View();
        }

        // POST: Xử lý lưu tài khoản thợ vào Database
        [HttpPost]
        public IActionResult Register(Technician tech)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra xem tên đăng nhập (Username) đã bị ai lấy chưa
                var existingUser = _context.Technicians.FirstOrDefault(t => t.Username == tech.Username);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Tên đăng nhập này đã có người sử dụng. Vui lòng chọn tên khác!";
                    return View(tech);
                }

                // 2. Lưu tài khoản mới vào SQL Server
                tech.CreatedAt = DateTime.Now;
                _context.Technicians.Add(tech);
                _context.SaveChanges();

                // 3. Thông báo bằng SweetAlert2 và chuyển về trang Đăng nhập
                TempData["SuccessMessage"] = $"Chào mừng {tech.FullName}! Bạn đã đăng ký thành công. Vui lòng đăng nhập để nhận đơn.";
                return RedirectToAction("Login");
            }

            return View(tech);
        }
    }
}