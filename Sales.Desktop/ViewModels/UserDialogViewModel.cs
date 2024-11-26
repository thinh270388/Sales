using AutoMapper;
using Sales.APIs.Helpers;
using Sales.Desktop.Helpers;
using Sales.Desktop.Views;
using Sales.Models;
using Sales.Models.DTOs;
using Sales.Models.Entities;
using Sales.Services.Constracts;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Sales.Desktop.ViewModels
{
    public class UserDialogViewModel : BaseViewModel
    {
        private readonly IMapper _mapper;
        private readonly IGenericService<Roles> _rolesService;
        private readonly IGenericService<Users> _usersService;
        private readonly Encryption encryption = new();
        private UsersDto user = new();
        private ObservableCollection<Roles> roles = new();
        private string password = string.Empty;
        private string confirmPassword = string.Empty;
        public ObservableCollection<string> Errors { get; set; } = new();


        public RelayCommand SaveCommand => new RelayCommand(async (parametter) => await Save(), canExecute => true);
        public RelayCommand CancelCommand => new RelayCommand(cmd => Cancel(), canExecute => true);

        public UsersDto User { get => user; set => SetProperty(ref user, value); }
        public ObservableCollection<Roles> Roles { get => roles; set => SetProperty(ref roles, value); }
        public string Password { get => password; set => SetProperty(ref password, value); }
        public string ConfirmPassword { get => confirmPassword; set => SetProperty(ref confirmPassword, value); }

        public UserDialogViewModel(IMapper mapper, IGenericService<Roles> rolesService, IGenericService<Users> usersService)
        {
            _mapper = mapper;
            _rolesService = rolesService;
            _usersService = usersService;

            Task.Run(async () => await LoadRoles()).Wait();
        }

        private async Task LoadRoles()
        {
            var rs = await _rolesService.GetAllAsync(new Roles());
            if (rs.Success && rs.Results != null)
            {
                Roles = new(rs.Results);
            }
        }

        private async Task Save()
        {
            DtoResult<Users> result = new();

            if (Validate()) return;

            // Kiểm tra trước khi thêm/cập nhật
            if (string.IsNullOrEmpty(User.FullName) || string.IsNullOrEmpty(User.UserName) || string.IsNullOrEmpty(User.Email) || User.RoleId == null)
            {
                MessageBox.Show($"Thông tin chưa đầy đủ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User.PasswordHash = encryption.Encrypt(Password);
            User.RoleName = Roles.FirstOrDefault(x => x.Id == User.RoleId)!.Name!.ToString();
            var usersMapper = _mapper.Map<Users>(User);

            if (User.Id == null)
            {
                result = await _usersService.AddAsync(usersMapper);
                if (result.Success)
                {
                    User.Id = result.Result!.Id;
                    MessageBox.Show($"Thêm người dùng ({User.Email}) thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Thêm người dùng ({User.Email}) thất bại\nLỗi: {result.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }    
            }
            else
            {
                result = await _usersService.UpdateAsync(usersMapper);

                if (result.Success)
                {
                    MessageBox.Show($"Cập nhật người dùng ({User.Email}) thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Cập nhật người dùng ({User.Email}) thất bại\nLỗi: {result.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (result.Success)
            {
                var window = Application.Current.Windows.OfType<UserDialogWindow>().FirstOrDefault();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }
        private void Cancel()
        {
            var window = Application.Current.Windows.OfType<UserDialogWindow>().FirstOrDefault();
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        public bool Validate()
        {
            var context = new ValidationContext(User);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(User, context, results, true);

            Errors.Clear();
            foreach (var result in results)
            {
                Errors.Add(result.ErrorMessage!);
            }
            return isValid;
        }
    }
}
