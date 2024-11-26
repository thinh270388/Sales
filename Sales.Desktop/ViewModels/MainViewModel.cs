using Microsoft.Extensions.DependencyInjection;
using Sales.APIs.Repositories.Constracts;
using Sales.Desktop.Helpers;
using Sales.Desktop.Services.Constracts;
using Sales.Desktop.Views;
using Sales.Models;
using Sales.Models.DTOs;
using Sales.Models.Entities;
using Sales.Models.Responses;
using Sales.Services.Constracts;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Sales.Desktop.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        private string imgPath = "/Sales.Desktop;component/Resources/Images/";
        private string logo = "/Sales.Desktop;component/Resources/Images/logo.png";

        private readonly INavigationService _navigationService;
        public INavigationService NavigationService => _navigationService;
        private readonly IServiceProvider _serviceProvider;
        private ObservableCollection<MenuControl> menuControls = new();
        private readonly IGenericService<Roles> _rolesService;
        private readonly IAuthService _authService;
        private bool isMenuExpanded = true;
        private int menuWidth = 200;
        private string currentTime = string.Empty;
        private string version = string.Empty;
        private LoginResponse loginResponse = new();

        public RelayCommand ToggleMenuCommand => new RelayCommand(cmd => ToggleMenu(), canExecute => true);
        public RelayCommand HomeCommand => new RelayCommand(cmd => NavigationService.NavigateTo<HomeViewModel>(), canExecute => true);
        public RelayCommand UserCommand => new RelayCommand(cmd => NavigationService.NavigateTo<UserViewModel>(), canExecute => true);
        public RelayCommand HistoryCommand => new RelayCommand(cmd => NavigationService.NavigateTo<HistoryViewModel>(), canExecute => true);
        public RelayCommand LogoutCommand => new RelayCommand(cmd => LogoutClick(), canExecute => true);

        public MainViewModel(
            INavigationService navigationService, 
            IServiceProvider serviceProvider, 
            IGenericService<Roles> rolesService,
            IAuthService authService) 
        {
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            _rolesService = rolesService;
            _authService = authService;

            MenuControls = new()
            {                
                new MenuControl() { ItemIcon = imgPath + "homes.png", ItemText = "Trang chủ", IconText = "", OnClicked = HomeCommand },
                new MenuControl() { ItemIcon = imgPath + "users.png", ItemText = "Người dùng", IconText = "", OnClicked = UserCommand },
                new MenuControl() { ItemIcon = imgPath + "users.png", ItemText = "Nhật kí", IconText = "", OnClicked = HistoryCommand }
            };

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();

            NavigationService.NavigateTo<HomeViewModel>();

            Version = $"v{Assembly.GetExecutingAssembly()!.GetName()!.Version}";
            Task.Run(async () => await LoadData()).Wait();
        }
        private async Task LoadData()
        {
            var rs = await _authService.GetValidAccessTokenAsync();
            if (rs != null)
            {
                LoginResponse = rs.Result!;
            }
        }
        private void ToggleMenu()
        {
            IsMenuExpanded = !IsMenuExpanded;
            MenuWidth = IsMenuExpanded ? 200 : 60;
        }
        private void LogoutClick()
        {
            // Xóa thông tin lưu trữ
            using (var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (isoStore.FileExists("loginResponse.dat"))
                {
                    isoStore.DeleteFile("loginResponse.dat");
                }
            }

            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            var viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            loginWindow.DataContext = viewModel;
            loginWindow.Show();

            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()!.Close();
        }
        public ObservableCollection<MenuControl> MenuControls { get => menuControls; set => SetProperty(ref menuControls, value); }
        public string Logo { get => logo; set => SetProperty(ref logo, value); }
        public bool IsMenuExpanded { get => isMenuExpanded; set => SetProperty(ref isMenuExpanded, value); }
        public string CurrentTime { get => currentTime; set => SetProperty(ref currentTime, value); }
        public int MenuWidth { get => menuWidth; set => SetProperty(ref menuWidth, value); }
        public string Version { get => version; set => SetProperty(ref version, value); }
        public LoginResponse LoginResponse { get => loginResponse; set => SetProperty(ref loginResponse, value); }
    }
}
