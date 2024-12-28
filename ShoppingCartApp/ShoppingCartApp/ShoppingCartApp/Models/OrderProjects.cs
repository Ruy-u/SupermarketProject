namespace ShoppingCartApp.Models
{
    public class OrderProjects
    {
        public int Id { get; set; }
        public string CustName { get; set; }
        public DateTime OrderDate { get; set; }
        public int? Total { get; set; }

        // Navigation property for OrderLineProjects
        public ICollection<OrderLineProjects> OrderLineProjects { get; set; } // Correct plural name for clarity
    }
}
