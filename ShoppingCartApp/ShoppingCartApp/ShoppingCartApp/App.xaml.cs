using ShoppingCartApp.Views; // Ensure this namespace matches your LoginPage location
using ShoppingCartApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCartApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Set LoginPage as the starting page
        MainPage = new NavigationPage(new LoginPage(new ApplicationDbContext(GetDbOptions())));
    }

    private static DbContextOptions<ApplicationDbContext> GetDbOptions()
    {
        // Manually create DbContextOptions for ApplicationDbContext
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Data Source=.\\sqlexpress;Initial Catalog=faisal;Integrated Security=True;TrustServerCertificate=True;");
        return optionsBuilder.Options;
    }
}
