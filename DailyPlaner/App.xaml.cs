using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DailyPlaner
{
    public partial class App : Application
    {
        public static bool IsExiting { get; private set; }
        public static System.Windows.Forms.NotifyIcon TrayIcon { get; private set; }

        private const string SettingsRegistryKey = "SOFTWARE\\DailyPlanner";
        private const string AutoStartRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AppName = "DailyPlanner";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupTrayIcon();
            CheckAndCreateDatabase();
            SetAutoStart(LoadAutoStartSetting());
            var loginHost = new System.Windows.Navigation.NavigationWindow
            {
                Content = new Views.LoginWindow(),
                ShowsNavigationUI = false,
                Title = "Ежедневник — вход",
                Width = 1000,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            SetWindowIcon(loginHost);
            loginHost.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (TrayIcon != null)
            {
                TrayIcon.Visible = false;
                TrayIcon.Dispose();
                TrayIcon = null;
            }
            base.OnExit(e);
        }

        public static string FindIconFile()
        {
            var candidates = new List<string>
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icon.jpg"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icon.jpg"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Icon.jpg")
            };
            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        public static void SetWindowIcon(Window window)
        {
            if (window == null)
            {
                return;
            }
            try
            {
                string iconPath = FindIconFile();
                if (iconPath != null)
                {
                    var bitmap = new BitmapImage(new Uri(iconPath));
                    bitmap.Freeze();
                    window.Icon = bitmap;
                }
            }
            catch (Exception)
            {
            }
        }

        public static void RestoreFromTray()
        {
            var window = Current.MainWindow;
            if (window == null)
            {
                return;
            }
            window.Show();
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Activate();
        }

        public static void ExitApp()
        {
            IsExiting = true;
            Current.Shutdown();
        }


        public static bool LoadAutoStartSetting()
        {
            return LoadSetting("AutoStart") == 1;
        }

        public static void SetAutoStart(bool enabled)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AutoStartRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (enabled)
                        {
                            key.SetValue(AppName, Assembly.GetEntryAssembly().Location);
                        }
                        else if (key.GetValue(AppName) != null)
                        {
                            key.DeleteValue(AppName);
                        }
                    }
                }
                SaveSetting("AutoStart", enabled);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось изменить настройки автозапуска: {ex.Message}", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetupTrayIcon()
        {
            if (TrayIcon != null)
            {
                return;
            }
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Открыть", null, (s, args) => RestoreFromTray());
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add("Выход", null, (s, args) => ExitApp());
            TrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = "Ежедневник",
                Icon = LoadTrayIcon(),
                ContextMenuStrip = menu,
                Visible = true
            };
            TrayIcon.DoubleClick += (s, args) => RestoreFromTray();
        }

        private static System.Drawing.Icon LoadTrayIcon()
        {
            try
            {
                string iconPath = FindIconFile();
                if (iconPath != null)
                {
                    using (var bitmap = new System.Drawing.Bitmap(iconPath))
                    {
                        IntPtr handle = bitmap.GetHicon();
                        var icon = System.Drawing.Icon.FromHandle(handle);
                        var clone = (System.Drawing.Icon)icon.Clone();
                        _ = Win32.DestroyIcon(handle);
                        return clone;
                    }
                }
            }
            catch (Exception)
            {
            }
            return System.Drawing.SystemIcons.Application;
        }

        private void CheckAndCreateDatabase()
        {
            try
            {
                new Services.DatabaseService().TestConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к базе данных:\n{ex.Message}\n\nПроверьте, что SQL Server (PCGl1tch) запущен и база DailyPlannerDBV3 создана.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static int LoadSetting(string name)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SettingsRegistryKey, false))
                {
                    if (key != null && key.GetValue(name) is int value)
                    {
                        return value;
                    }
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }

        private static void SaveSetting(string name, bool value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsRegistryKey))
                {
                    if (key != null)
                    {
                        key.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception)
            {
            }
        }


        internal static class Win32
        {
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            internal static extern bool DestroyIcon(IntPtr hIcon);
        }
    }
}
