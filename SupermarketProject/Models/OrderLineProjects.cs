namespace SupermarketProject.Models
{
    public class OrderLineProjects
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int ItemQuant { get; set; }
        public decimal? ItemPrice { get; set; }
        public int OrderId { get; set; }

        // Navigation property for OrderProjects
        public OrderProjects Order { get; set; }
    }
}
