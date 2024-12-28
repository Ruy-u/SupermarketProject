using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Views;
using ShoppingCartApp.Data;

namespace ShoppingCartApp
{
    using Microsoft.EntityFrameworkCore;
    using ShoppingCartApp.Data;

    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Register DbContext with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Data Source=.\\sqlexpress;Initial Catalog=faisal;Integrated Security=True;TrustServerCertificate=True;")
       .LogTo(Console.WriteLine, LogLevel.Information)); // Logs SQL queries to the console

            // Register AppShell
            builder.Services.AddSingleton<AppShell>();

            // Register Pages
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<CartPage>();

            return builder.Build();
        }
    }
}

