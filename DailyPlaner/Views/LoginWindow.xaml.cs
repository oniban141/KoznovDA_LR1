using System.Windows;
using System.Windows.Controls;
using DailyPlaner.ViewModels;

namespace DailyPlaner.Views
{
    public partial class LoginWindow : Page
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (UsernameWatermark != null)
            {
                UsernameWatermark.Visibility = string.IsNullOrEmpty(textBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
            if (PasswordWatermark != null)
            {
                PasswordWatermark.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
