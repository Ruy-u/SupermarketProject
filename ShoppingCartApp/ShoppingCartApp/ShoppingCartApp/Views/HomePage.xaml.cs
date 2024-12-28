using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Data;
using ShoppingCartApp.Models;
using System.Collections.ObjectModel;

namespace ShoppingCartApp.Views;

public partial class HomePage : ContentPage
{
    private readonly ApplicationDbContext _dbContext;

    public ObservableCollection<ItemProjects> Products { get; set; }

    public HomePage(ApplicationDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;

        // Initialize product collection
        Products = new ObservableCollection<ItemProjects>();

        LoadProducts();
        BindingContext = this;
    }

    private async void LoadProducts()
    {
        try
        {
            // Fetch items from the database
            var products = await _dbContext.Items.ToListAsync();

            if (products.Any())
            {
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            else
            {
                await DisplayAlert("Notice", "No products available.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
            await DisplayAlert("Error", "Failed to load products.", "OK");
        }
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        try
        {
            var button = (Button)sender;
            var selectedItem = (ItemProjects)button.CommandParameter;

            if (selectedItem != null && selectedItem.Quantity > 0)
            {
                selectedItem.Quantity -= 1; // Reduce available quantity

                // Save updated quantity to the database
                _dbContext.Items.Update(selectedItem);
                await _dbContext.SaveChangesAsync();

                DisplayAlert("Success", $"{selectedItem.Name} has been added to your cart.", "OK");
            }
            else
            {
                DisplayAlert("Error", "Item is out of stock.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding to cart: {ex.Message}");
            await DisplayAlert("Error", "Failed to add item to cart.", "OK");
        }
    }
}
