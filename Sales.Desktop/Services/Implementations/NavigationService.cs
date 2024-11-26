using Microsoft.Extensions.DependencyInjection;
using Sales.Desktop.Helpers;
using Sales.Desktop.Services.Constracts;
using System.ComponentModel;

namespace Sales.Desktop.Services.Implementations
{
    public class NavigationService : INavigationService, INotifyPropertyChanged
    {
        private readonly IServiceProvider? _serviceProvider;
        private BaseViewModel _currentViewModel = new();
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            CurrentViewModel = _serviceProvider!.GetService<TViewModel>()!;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
