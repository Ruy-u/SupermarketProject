using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Data;
using ShoppingCartApp.Helpers;
using ShoppingCartApp.Models;

namespace ShoppingCartApp.Views;

public partial class CartPage : ContentPage
{
    private readonly ApplicationDbContext _dbContext;

    public CartPage(ApplicationDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;
        LoadCart();
    }

    private void LoadCart()
    {
        CartCollectionView.ItemsSource = SharedCart.CartItems;
    }

    private async void OnPurchaseClicked(object sender, EventArgs e)
    {
        if (SharedCart.CartItems == null || !SharedCart.CartItems.Any())
        {
            await DisplayAlert("Error", "Your cart is empty.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(SessionHelper.LoggedInUserName))
        {
            await DisplayAlert("Error", "User information is missing. Please log in again.", "OK");
            Application.Current.MainPage = new NavigationPage(new LoginPage(_dbContext));
            return;
        }

        try
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Step 1: Create the order
            var order = new OrderProjects
            {
                CustName = SessionHelper.LoggedInUserName,
                OrderDate = DateTime.Now,
                Total = SharedCart.CartItems.Sum(i => (int?)i.Price) ?? 0
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Step 2: Add order line items
            foreach (var item in SharedCart.CartItems)
            {
                var orderLine = new OrderLineProjects
                {
                    ItemName = item.Name,
                    ItemQuant = 1, // Default quantity
                    ItemPrice = item.Price,
                    OrderId = order.Id
                };

                _dbContext.OrderLines.Add(orderLine);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Step 3: Retrieve the order details (including line items)
            var savedOrder = await _dbContext.Orders
                .Include(o => o.OrderLineProjects) // Include related order lines
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            // Step 4: Build the order details message
            string orderDetails = $"Order ID: {savedOrder.Id}\n" +
                                  $"Customer: {savedOrder.CustName}\n" +
                                  $"Order Date: {savedOrder.OrderDate}\n" +
                                  $"Total: {savedOrder.Total:C}\n\n" +
                                  $"Items:\n";

            foreach (var line in savedOrder.OrderLineProjects)
            {
                orderDetails += $"- {line.ItemName} (Quantity: {line.ItemQuant}, Price: {line.ItemPrice:C})\n";
            }

            // Step 5: Show the order details in an alert
            await DisplayAlert("Order Confirmation", orderDetails, "OK");

            // Step 6: Clear the cart and refresh the view
            SharedCart.CartItems.Clear();
            LoadCart();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}
