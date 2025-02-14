namespace SqlGeneratorApp.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
    }
}