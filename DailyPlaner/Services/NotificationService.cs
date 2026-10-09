using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Notifications;

namespace DailyPlaner.Services
{
    public class NotificationService
    {
        private static readonly List<System.Threading.Timer> _timers = new List<System.Threading.Timer>();

        public void ShowNotification(string title, string message)
        {
            try
            {
                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.BeginInvoke((Action)(() => ShowNotification(title, message)));
                    return;
                }
                try
                {
                    new ToastContentBuilder().AddText(title).AddText(message).Show();
                }
                catch
                {
                    ShowTrayFallback(title, message);
                }
            }
            catch (Exception ex)
            {
                ShowTrayFallback(title, message);
                Console.WriteLine($"Ошибка показа уведомления: {ex.Message}");
            }
        }

        public void ShowReminderNotification(string title, string message, DateTime dueDate)
        {
            try
            {
                ShowNotification(title, $"{message}\n\nСрок: {dueDate:yyyy-MM-dd HH:mm}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка показа напоминания: {ex.Message}");
            }
        }

        public void ScheduleNotification(string title, string message, DateTime scheduleTime)
        {
            try
            {
                var delay = scheduleTime - DateTime.Now;
                if (delay < TimeSpan.Zero)
                {
                    ShowNotification(title, message);
                    return;
                }
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer(_ =>
                {
                    try
                    {
                        ShowNotification(title, message);
                    }
                    finally
                    {
                        lock (_timers)
                        {
                            _timers.Remove(timer);
                        }
                        timer?.Dispose();
                    }
                }, null, delay, TimeSpan.FromMilliseconds(-1));
                lock (_timers)
                {
                    _timers.Add(timer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка планирования уведомления: {ex.Message}");
            }
        }

        private static void ShowTrayFallback(string title, string message)
        {
            try
            {
                var icon = App.TrayIcon;
                if (icon == null || !icon.Visible)
                {
                    return;
                }
                icon.BalloonTipTitle = title;
                icon.BalloonTipText = message;
                icon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                icon.ShowBalloonTip(4000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка показа уведомления в трее: {ex.Message}");
            }
        }
    }
}
