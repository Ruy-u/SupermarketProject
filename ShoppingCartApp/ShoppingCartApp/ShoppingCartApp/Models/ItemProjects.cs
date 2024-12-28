using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartApp.Models
{
    public class ItemProjects
    {
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("discount")]
        public string Discount { get; set; }

        [Column("category")]
        public int? Category { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("imgfile")]
        public string Imgfile { get; set; }
    }
}
