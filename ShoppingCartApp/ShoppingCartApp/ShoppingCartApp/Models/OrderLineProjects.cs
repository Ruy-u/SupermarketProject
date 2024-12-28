using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartApp.Models
{
    public class OrderLineProjects
    {
        public int Id { get; set; }

        public string ItemName { get; set; }

        public int ItemQuant { get; set; }

        public decimal? ItemPrice { get; set; }

        // Foreign key column
        public int OrderId { get; set; }

        // Navigation property
        [ForeignKey("OrderId")] // Explicitly map this navigation to orderid column
        public OrderProjects Order { get; set; }
    }
}
