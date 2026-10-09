using System;
using System.Collections.Generic;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class TaskDialog : Window
    {
        private static readonly int[] ReminderOffsetMinutes = { 5, 10, 15, 30, 60, 120, 1440 };

        public string TaskTitle
        {
            get { return TitleBox.Text.Trim(); }
        }

        public string TaskDescription
        {
            get { return DescriptionBox.Text.Trim(); }
        }

        public DateTime TaskDueDate { get; private set; }
        public bool ReminderEnabled
        {
            get { return ReminderCheckBox.IsChecked == true; }
        }

        public DateTime? ReminderTime { get; private set; }
        public string TaskPriority { get; private set; }

        public TaskDialog(string title, string description, DateTime dueDate)
            : this(title, description, dueDate, "Средний")
        {
        }

        public TaskDialog(string title, string description, DateTime dueDate, string priority)
        {
            InitializeComponent();
            TaskDueDate = dueDate;
            var priorityItems = new[] { "Низкий", "Средний", "Высокий" };
            PriorityComboBox.ItemsSource = priorityItems;
            int priorityIndex = Array.IndexOf(priorityItems, string.IsNullOrWhiteSpace(priority) ? "Средний" : priority);
            PriorityComboBox.SelectedIndex = priorityIndex >= 0 ? priorityIndex : 1;
            FillTimeItems(dueDate);
            DueDatePicker.SelectedDate = dueDate.Date;
            SelectTime(dueDate.TimeOfDay);
            ReminderOffsetComboBox.ItemsSource = new[]
            {
                "5 минут", "10 минут", "15 минут", "30 минут", "1 час", "2 часа", "За день"
            };
            ReminderOffsetComboBox.SelectedIndex = 3;
            TitleBox.Text = title ?? string.Empty;
            DescriptionBox.Text = description ?? string.Empty;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void FillTimeItems(DateTime baseTime)
        {
            var items = new List<string>();
            var start = baseTime.Date;
            for (int i = 0; i < 48; i++)
            {
                items.Add(start.ToString("HH:mm"));
                start = start.AddMinutes(30);
            }
            TimeComboBox.ItemsSource = items;
        }

        private void SelectTime(TimeSpan time)
        {
            var rounded = new TimeSpan(time.Hours, time.Minutes >= 30 ? 30 : 0, 0);
            string target = ((int)rounded.TotalHours).ToString("00") + ":" + rounded.Minutes.ToString("00");
            foreach (var item in TimeComboBox.Items)
            {
                if (item.ToString() == target)
                {
                    TimeComboBox.SelectedItem = item;
                    return;
                }
            }
            if (TimeComboBox.Items.Count > 0)
            {
                TimeComboBox.SelectedIndex = 0;
            }
        }

        private void ReminderCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ReminderPanel.Visibility = ReminderCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ReminderOffsetComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ReminderCheckBox.IsChecked == true)
            {
                UpdateReminderTime();
            }
        }

        private DateTime GetDueDateFromControls()
        {
            var date = DueDatePicker.SelectedDate ?? DateTime.Today;
            var time = TimeSpan.Zero;
            if (TimeComboBox.SelectedItem != null && TimeSpan.TryParse(TimeComboBox.SelectedItem.ToString(), out var parsed))
            {
                time = parsed;
            }
            return date.Date + time;
        }

        private void UpdateReminderTime()
        {
            ReminderTime = null;
            if (ReminderCheckBox.IsChecked != true)
            {
                return;
            }
            int index = ReminderOffsetComboBox.SelectedIndex;
            if (index < 0 || index >= ReminderOffsetMinutes.Length)
            {
                return;
            }
            var dueDate = GetDueDateFromControls();
            if (dueDate < DateTime.MinValue.AddMinutes(ReminderOffsetMinutes[index]))
            {
                return;
            }
            ReminderTime = dueDate.AddMinutes(-ReminderOffsetMinutes[index]);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TaskDueDate = GetDueDateFromControls();
            TaskPriority = PriorityComboBox.SelectedItem?.ToString() ?? "Средний";
            if (ReminderEnabled)
            {
                UpdateReminderTime();
            }
            else
            {
                ReminderTime = null;
            }
            DialogResult = true;
        }

        private void TitleBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TitleWatermark != null)
            {
                TitleWatermark.Visibility = string.IsNullOrEmpty(TitleBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void DescriptionBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DescriptionWatermark != null)
            {
                DescriptionWatermark.Visibility = string.IsNullOrEmpty(DescriptionBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
