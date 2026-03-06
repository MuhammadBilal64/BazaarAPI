using E_Commerce_BackendAPI.Utilities;

namespace E_Commerce_BackendAPI.Model
{
    public class Order:BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime OrderDate { get; set; }= DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public Decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();




    }
}
