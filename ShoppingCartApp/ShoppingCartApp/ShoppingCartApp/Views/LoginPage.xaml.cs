using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Data;
using ShoppingCartApp.Helpers;

namespace ShoppingCartApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApplicationDbContext _dbContext;

    public LoginPage(ApplicationDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter both username and password.", "OK");
            return;
        }

        try
        {
            // Query the database to check credentials
            var user = await _dbContext.UserAccounts
                .FirstOrDefaultAsync(u => u.Name == username && u.Pass == password);

            if (user != null)
            {
                // Save user details in SessionHelper
                SessionHelper.LoggedInUserName = user.Name;
                SessionHelper.LoggedInUserId = user.Id;

                await DisplayAlert("Success", "Login successful!", "OK");
                Application.Current.MainPage = new AppShell(_dbContext);
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid username or password.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Navigate to Register Page
        await Navigation.PushAsync(new RegisterPage(_dbContext));
    }
}
