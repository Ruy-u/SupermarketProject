using ShoppingCartApp.Models;

namespace ShoppingCartApp.Helpers
{
    public static class SharedCart
    {
        public static List<ItemProjects> CartItems { get; } = new List<ItemProjects>();
    }
}
