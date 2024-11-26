using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Sales.Desktop.Helpers;
using Sales.Desktop.Views;
using Sales.Models.DTOs;
using Sales.Models.Entities;
using Sales.Services.Constracts;
using Sales.Services.Implementations;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;

namespace Sales.Desktop.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly IGenericService<Users> _userService;
        private readonly IGenericService<Functions> _functionService;
        private readonly IGenericService<Roles> _roleService;
        private readonly IOtherService _otherService;
        private ObservableCollection<UsersDto> users = new();
        private ObservableCollection<Functions> functions = new();
        private ObservableCollection<Roles> roles = new();
        private UsersDto selectedUser = new();
        private Functions selectedFunction = new();
        private Roles selectedRole = new();
        private int totalRecords;

        public RelayCommand AddUserCommand => new RelayCommand(cmd => Add(), canExecute => true);
        public RelayCommand EditUserCommand => new RelayCommand((object parameter) => Edit(parameter), (object parameter) => CanEdit(parameter));
        public RelayCommand DeleteUserCommand => new RelayCommand(async (object parameter) => await Delete(parameter), (object parameter) => CanDelete(parameter));

        public ObservableCollection<UsersDto> Users { get => users; set => SetProperty(ref users, value); }
        public UsersDto SelectedUser { get => selectedUser; set => SetProperty(ref selectedUser, value); }
        public int TotalRecords { get => totalRecords; set => totalRecords = value; }
        public ObservableCollection<Functions> Functions { get => functions; set => SetProperty(ref functions, value); }
        public ObservableCollection<Roles> Roles { get => roles; set => SetProperty(ref roles, value); }
        public Functions SelectedFunction { get => selectedFunction; set => SetProperty(ref selectedFunction, value); }
        public Roles SelectedRole { get => selectedRole; set => SetProperty(ref selectedRole, value); }

        public UserViewModel(
            IServiceProvider serviceProvider, 
            IMapper mapper, 
            IGenericService<Users> userService, 
            IGenericService<Functions> functionsService,
            IGenericService<Roles> roleService,
            IOtherService otherService) 
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _userService = userService;
            _functionService = functionsService;
            _roleService = roleService;
            _otherService = otherService;

            Task.Run(async () => await LoadData()).Wait();
        }
        private async Task LoadData()
        {
            try
            {
                var rsUsers = await _otherService.GetFullUserAsync();
                if (rsUsers != null)
                {
                    Users = new(rsUsers.Results!);
                }
                var rsFunctions = await _functionService.GetAllAsync(new Models.Entities.Functions());
                if (rsFunctions != null)
                {
                    Functions = new(rsFunctions.Results!);
                }
                var rsRoles = await _roleService.GetAllAsync(new Models.Entities.Roles());
                if (rsRoles != null)
                {
                    Roles = new(rsRoles.Results!);
                }

                TotalRecords = Users.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối máy chủ.\nCó lỗi xảy ra: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }
        private void Add()
        {
            var newUser = new UsersDto();

            var dialog = _serviceProvider.GetRequiredService<UserDialogWindow>();
            var viewModel = _serviceProvider.GetRequiredService<UserDialogViewModel>();
            viewModel.User = newUser;
            dialog.DataContext = viewModel;

            if (dialog.ShowDialog() == true)
            {
                Users.Add(newUser);
                SelectedUser = newUser;
            }
        }
        private void Edit(object parameter)
        {
            if (parameter is UsersDto userResponse)
            {
                SelectedUser = userResponse;
                var updateUser = SelectedUser.Clone();

                var dialog = _serviceProvider.GetRequiredService<UserDialogWindow>();
                var viewModel = _serviceProvider.GetRequiredService<UserDialogViewModel>();
                viewModel.User = updateUser;
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true)
                {
                    var index = Users.IndexOf(SelectedUser);
                    Users[index] = updateUser;
                    SelectedUser = updateUser;
                }
            }
        }
        private bool CanEdit(object parameter)
        {
            if (parameter is UsersDto usersDto)
            {
                return usersDto.IsSystem == null || (bool)!usersDto.IsSystem;
            }
            return false;
        }
        private async Task Delete(object parameter)
        {
            if (parameter is UsersDto userResponse) SelectedUser = userResponse;
            if (SelectedUser == null) return;

            if ((bool)SelectedUser.IsSystem!)
            {
                MessageBox.Show($"Bạn không thể xóa người dùng hệ thống ({SelectedUser.Email})", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = MessageBox.Show($"Bạn có chắc muốn xóa người dùng ({SelectedUser.Email})", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialog == MessageBoxResult.Yes)
            {
                var deleteUser = _mapper.Map<Users>(SelectedUser);
                var result = await _userService.DeleteAsync(deleteUser);
                if (result.Success)
                {
                    MessageBox.Show($"Xóa người dùng ({SelectedUser.Email}) thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Users.Remove(SelectedUser);
                }
                else
                {
                    MessageBox.Show($"Xóa người dùng ({SelectedUser.Email}) thất bại", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool CanDelete(object parameter)
        {
            if (parameter is UsersDto usersDto)
            {
                return usersDto.IsSystem == null || (bool)!usersDto.IsSystem;
            }
            return false;
        }
    }
}
