using Microsoft.Extensions.DependencyInjection;
using Sales.Desktop.Helpers;
using Sales.Desktop.Services.Constracts;
using Sales.Desktop.Services.Implementations;
using Sales.Desktop.ViewModels;
using Sales.Desktop.Views;
using Sales.Models;
using Sales.Models.DTOs;
using Sales.Models.Entities;
using Sales.Services.Constracts;
using Sales.Services.Implementations;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Windows;

namespace Sales.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var response = await authService.GetValidAccessTokenAsync();
            if (response.Success && response.Result != null)
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
                mainWindow.Show();
            }
            else
            {
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.DataContext = _serviceProvider.GetRequiredService<LoginViewModel>();
                loginWindow.Show();
            }
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IGenericService<Functions>, GenericService<Functions>>();
            services.AddScoped<IGenericService<Logs>, GenericService<Logs>>();
            services.AddScoped<IGenericService<Permissions>, GenericService<Permissions>>();
            services.AddScoped<IGenericService<Roles>, GenericService<Roles>>();
            services.AddScoped<IGenericService<Users>, GenericService<Users>>();
            services.AddScoped<IGenericService<Functions>, GenericService<Functions>>();
            services.AddScoped<IOtherService, OtherService>();

            services.AddScoped<GenericService<UsersDto>>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<UserViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<UserDialogViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<HomeView>();
            services.AddTransient<UserViewModel>();
            services.AddTransient<HistoryView>();
            services.AddTransient<UserDialogWindow>();

        }
    }

}
