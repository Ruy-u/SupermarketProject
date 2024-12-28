using ShoppingCartApp.Data;
using ShoppingCartApp.Views;

namespace ShoppingCartApp;

public partial class AppShell : Shell
{
    private readonly ApplicationDbContext _dbContext;

    public AppShell(ApplicationDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        // Redirect to LoginPage on Logout
        Application.Current.MainPage = new NavigationPage(new LoginPage(_dbContext));
    }
}
