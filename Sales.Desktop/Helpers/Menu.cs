namespace Sales.Desktop.Helpers
{
    public interface IMenuControl
    {
        string? ItemText { get; set; }
        string? ItemIcon { get; set; }
        string? IconText { get; set; }
        bool CanExecute { get; set; }        
        RelayCommand? OnClicked { get; set; }
    }
    public class MenuControl : IMenuControl
    {
        public string? ItemText { get; set; }
        public string? ItemIcon { get; set; }
        public string? IconText { get; set; }
        public bool CanExecute { get; set; }
        public RelayCommand? OnClicked { get; set; }
    }
}
