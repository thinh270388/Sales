using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sales.Desktop.Helpers
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Phương thức dùng để thông báo rằng một thuộc tính đã thay đổi
        public void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Phương thức để đặt giá trị và tự động thông báo nếu giá trị thay đổi
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null!)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
