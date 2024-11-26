using Sales.Desktop.Helpers;

namespace Sales.Desktop.Services.Constracts
{
    public interface INavigationService
    {
        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
        public BaseViewModel CurrentViewModel { get; set; }
    }
}
