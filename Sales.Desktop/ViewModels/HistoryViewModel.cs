using Sales.Desktop.Helpers;
using Sales.Models.Entities;
using Sales.Services.Constracts;
using System.Collections.ObjectModel;
using System.Windows;

namespace Sales.Desktop.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGenericService<Logs> _logService;
        private ObservableCollection<Logs> logs = new();
        private Logs selectedLog = new();
        private int totalRecords;

        public RelayCommand DeleteCommand => new RelayCommand(async (object parameter) => await Delete(parameter), canExecute => true);

        public ObservableCollection<Logs> Logs { get => logs; set { SetProperty(ref logs, value); TotalRecords = logs.Count(); } }
        public Logs SelectedLog { get => selectedLog; set => SetProperty(ref selectedLog, value); }
        public int TotalRecords { get => totalRecords; set => totalRecords = value; }

        public HistoryViewModel(IServiceProvider serviceProvider, IGenericService<Logs> logService) 
        {
            _serviceProvider = serviceProvider;
            _logService = logService;

            Task.Run(async () => await LoadData()).Wait();
        }
        private async Task LoadData()
        {
            try
            {
                var rs = await _logService.GetAllAsync(new Logs());
                if (rs != null)
                {
                    Logs = new(rs.Results!);
                }

                TotalRecords = Logs.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối máy chủ.\nCó lỗi xảy ra: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }        
        
        private async Task Delete(object parameter)
        {
            if (parameter is Logs log) SelectedLog = log;
            if (SelectedLog == null) return;

            var dialog = MessageBox.Show($"Bạn có chắc muốn xóa nhật kí ({SelectedLog.Id})", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialog == MessageBoxResult.Yes)
            {
                var result = await _logService.DeleteAsync(SelectedLog);
                if (result.Success)
                {
                    MessageBox.Show($"Đã xóa thành công nhật kí ({SelectedLog.Id})", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logs.Remove(SelectedLog);
                }
                else
                {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
