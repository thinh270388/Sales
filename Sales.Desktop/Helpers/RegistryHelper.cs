using Microsoft.Win32;
using System.Windows;

namespace Sales.Desktop.Helpers
{
    public class RegistryHelper
    {
        public string? Read(string subKey, string keyName, bool showError = false)
        {
            RegistryKey? registryKey = ConstantHelper.REGISTRY_KEY.OpenSubKey(@"SOFTWARE\" + subKey);
            if (registryKey == null)
            {
                return null;
            }
            else
            {
                try
                {
                    return (string)registryKey.GetValue(keyName)!;
                }
                catch (Exception ex)
                {
                    if (showError) ShowErrorMessage(ex, "Reading registry " + keyName);
                    return null;
                }
            }
        }
        public bool Write(string subKey, string keyName, object value, bool showError = false)
        {
            try
            {
                RegistryKey registryKey = ConstantHelper.REGISTRY_KEY.CreateSubKey(@"SOFTWARE\" + subKey);
                registryKey.SetValue(keyName, value);

                return true;
            }
            catch (Exception ex)
            {
                if (showError) ShowErrorMessage(ex, "Writing registry " + keyName);
                return false;
            }
        }
        private void ShowErrorMessage(Exception ex, string title)
        {
            MessageBox.Show(ex.Message, title);
        }
    }
}
