using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DepartmentStaffTerminal.Models;
using DepartmentStaffTerminal.Views;

namespace DepartmentStaffTerminal;

public partial class App : System.Windows.Application
{
    private IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

                services.AddSingleton<IConfiguration>(context.Configuration);
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<RequestDetailsWindow>();
                services.AddTransient<BlacklistReasonWindow>();
            })
            .Build();
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}