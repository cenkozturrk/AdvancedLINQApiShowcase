namespace AdvancedLINQApiShowcase.Models
{
    public class Order : BaseClass
    {
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }

        // Foreign key to Customer (Many-to-one relationship)
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

    }
}
