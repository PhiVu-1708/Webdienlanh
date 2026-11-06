using Microsoft.AspNetCore.Mvc;
using Webdienlanh.Models;

namespace Webdienlanh.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Chat/GetMessages?orderId=1
        // Lấy danh sách tin nhắn của một đơn hàng (sắp xếp theo thời gian gửi)
        [HttpGet]
        public IActionResult GetMessages(int orderId)
        {
            var messages = _context.Messages
                .Where(m => m.BookingOrderId == orderId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.BookingOrderId,
                    m.SenderRole,
                    m.MessageText,
                    SentAt = m.SentAt.ToString("HH:mm dd/MM")
                })
                .ToList();

            return Json(messages);
        }

        // POST: /Chat/SendMessage
        // Xử lý gửi tin nhắn mới từ Khách hàng hoặc Thợ
        [HttpPost]
        public IActionResult SendMessage(int orderId, string senderRole, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return BadRequest(new { success = false, message = "Nội dung tin nhắn không được để trống!" });
            }

            var message = new Message
            {
                BookingOrderId = orderId,
                SenderRole = senderRole, // "Customer" hoặc "Technician"
                MessageText = messageText.Trim(),
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}