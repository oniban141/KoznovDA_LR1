using System.Windows;
using System.Windows.Controls;
using DailyPlaner.ViewModels;

namespace DailyPlaner.Views
{
    public partial class RegisterWindow : Window
    {
        private RegisterViewModel ViewModel
        {
            get { return (RegisterViewModel)DataContext; }
        }

        public RegisterWindow()
        {
            InitializeComponent();
            App.SetWindowIcon(this);
            DataContext = new RegisterViewModel(new Services.DatabaseService());
            GenderComboBox.SelectionChanged += GenderComboBox_SelectionChanged;
        }

        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenderWatermark != null)
            {
                GenderWatermark.Visibility = GenderComboBox.SelectedItem != null
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
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

        private void EmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (EmailWatermark != null)
            {
                EmailWatermark.Visibility = string.IsNullOrEmpty(textBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (ViewModel != null)
            {
                ViewModel.Password = passwordBox.Password;
            }
            if (PasswordWatermark != null)
            {
                PasswordWatermark.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (ViewModel != null)
            {
                ViewModel.ConfirmPassword = passwordBox.Password;
            }
            if (ConfirmPasswordWatermark != null)
            {
                ConfirmPasswordWatermark.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
