namespace Webdienlanh.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int BookingOrderId { get; set; }
        public string SenderRole { get; set; } // 'Customer' hoặc 'Technician'
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết với đơn hàng
        public BookingOrder? BookingOrder { get; set; }
    }
}