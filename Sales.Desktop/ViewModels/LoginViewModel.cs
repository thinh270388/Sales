using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sales.Desktop.Helpers;
using Sales.Desktop.Views;
using Sales.Models.DTOs;
using Sales.Services.Constracts;
using System.Windows;

namespace Sales.Desktop.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;

        private Sales.Models.Requests.LoginRequest loginRequest = new();
        private string errorMessage = string.Empty;
        private bool remember = false;
        RegistryHelper registry = new();
        public RelayCommand LoginCommand => new RelayCommand(cmd => Login(), canExecute => !string.IsNullOrEmpty(LoginRequest.Email) && !string.IsNullOrEmpty(LoginRequest.Password));
        public RelayCommand CancelCommand => new RelayCommand(cmd => Cancel(), canExecute => true);

        public LoginViewModel(IServiceProvider serviceProvider, IAuthService authService)
        {
            _serviceProvider = serviceProvider;
            _authService = authService;

            string s = registry.Read(ConstantHelper.REGISTRY_SUBKEY, ConstantHelper.REGISTRY_LOGIN, false)!;
            if (!string.IsNullOrEmpty(s))
            {
                SaveLoginDto isSave = JsonConvert.DeserializeObject<SaveLoginDto>(s)!;
                LoginRequest.Email = isSave.Email;
                LoginRequest.Password = isSave.Password;
                Remember = isSave.IsRemember;
            }
        }
        private async void Login()
        {
            try
            {
                var result = await _authService.LoginAsync(LoginRequest);
                if (result.Success)
                {
                    if (Remember)
                    {
                        registry.Write(
                            ConstantHelper.REGISTRY_SUBKEY,
                            ConstantHelper.REGISTRY_LOGIN,
                            JsonConvert.SerializeObject(new SaveLoginDto() { IsRemember = Remember, Email = LoginRequest.Email, Password = LoginRequest.Password }), false);
                    }
                    else
                    {
                        registry.Write(
                            ConstantHelper.REGISTRY_SUBKEY,
                            ConstantHelper.REGISTRY_LOGIN,
                            JsonConvert.SerializeObject(new SaveLoginDto()), false);
                    }

                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
                    // viewModel.User = result.Result!.UserLogin!;
                    mainWindow.DataContext = viewModel;
                    mainWindow.Show();

                    Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()!.Close();
                }
                else
                {
                    ErrorMessage = result.Message!;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
            }
            
        }
        private void Cancel()
        {
            Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()!.Close();
        }
        public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }
        public Sales.Models.Requests.LoginRequest LoginRequest { get => loginRequest; set => SetProperty(ref loginRequest, value); }
        public bool Remember { get => remember; set => SetProperty(ref remember, value); }
    }
}
