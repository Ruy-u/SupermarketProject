namespace SupermarketProject.Models
{
    public class ItemProjects
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Discount { get; set; } // Nullable to match database behavior
        public int? Category { get; set; }
        public int? Quantity { get; set; } = 0;
        public string? Imgfile { get; set; }
    }
}
