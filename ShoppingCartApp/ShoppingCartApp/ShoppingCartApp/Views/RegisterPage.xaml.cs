using ShoppingCartApp.Models;
namespace ShoppingCartApp.Views;
using ShoppingCartApp.Data;

public partial class RegisterPage : ContentPage
{
    private readonly ApplicationDbContext _dbContext;

    public RegisterPage(ApplicationDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;
        var email = EmailEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Error", "All fields are required.", "OK");
            return;
        }

        _dbContext.UserAccounts.Add(new UserAccountProjects
        {
            Name = username,
            Pass = password,
            Role = "Customer"
        });

        _dbContext.Customers.Add(new CustomerProjects
        {
            Name = username,
            Email = email
        });

        await _dbContext.SaveChangesAsync();

        await DisplayAlert("Success", "Registration complete. Please log in.", "OK");
        await Navigation.PopAsync();
    }
}
