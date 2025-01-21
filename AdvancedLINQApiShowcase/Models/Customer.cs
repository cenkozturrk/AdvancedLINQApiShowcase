namespace AdvancedLINQApiShowcase.Models
{
    public class Customer : BaseClass
    {
        public string Name { get; set; }
        public string Email { get; set; }

        // One-to-many relationship: One customer can have many orders
        public ICollection<Order>? Orders { get; set; }
    }
}
