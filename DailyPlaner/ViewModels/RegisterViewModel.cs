using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DailyPlaner.Services;

namespace DailyPlaner.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _email;
        private int _selectedGenderId;
        private List<Gender> _genders;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public int SelectedGenderId
        {
            get { return _selectedGenderId; }
            set
            {
                _selectedGenderId = value;
                OnPropertyChanged(nameof(SelectedGenderId));
            }
        }

        public List<Gender> Genders
        {
            get { return _genders; }
            set
            {
                _genders = value;
                OnPropertyChanged(nameof(Genders));
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public RegisterViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            CancelCommand = new RelayCommand(ExecuteCancel);
            LoadGenders();
        }

        private static string LocalizeGender(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            var lowered = name.Trim().ToLowerInvariant();
            if (lowered == "male" || lowered == "мужской" || lowered == "м" || lowered == "мужчина")
            {
                return "Мужской";
            }
            if (lowered == "female" || lowered == "женский" || lowered == "ж" || lowered == "женщина")
            {
                return "Женский";
            }
            return name.Trim();
        }

        private void LoadGenders()
        {
            try
            {
                var allGenders = _databaseService.GetAllGenders();
                Genders = allGenders
                    .Where(g => g.Name != null &&
                                g.Name.Trim().ToLowerInvariant() != "other" &&
                                g.Name.Trim().ToLowerInvariant() != "другое")
                    .Select(g =>
                    {
                        g.Name = LocalizeGender(g.Name);
                        return g;
                    })
                    .ToList();
                SelectedGenderId = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка полов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password == ConfirmPassword &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   SelectedGenderId > 0;
        }

        private void ExecuteRegister(object parameter)
        {
            try
            {
                if (Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать не менее 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (_databaseService.UsernameExists(Username))
                {
                    MessageBox.Show($"Имя пользователя \"{Username}\" уже занято. Выберите другое.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var user = new User
                {
                    Username = Username,
                    PasswordHash = DatabaseService.HashPassword(Password),
                    Email = Email,
                    GenderId = SelectedGenderId,
                    RoleId = 1,
                    CreatedAt = DateTime.Now
                };
                if (_databaseService.CreateUser(user))
                {
                    MessageBox.Show("Вы успешно зарегистрировались!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseRegisterWindow();
                }
                else
                {
                    MessageBox.Show("Не удалось зарегистрироваться. Попробуйте ещё раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            try
            {
                var window = parameter as Window;
                if (window != null)
                {
                    window.Close();
                }
                else
                {
                    CloseRegisterWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseRegisterWindow()
        {
            Application.Current.Windows.OfType<Views.RegisterWindow>().FirstOrDefault()?.Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
