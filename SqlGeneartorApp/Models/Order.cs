// Entity classes
namespace SqlGeneratorApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}