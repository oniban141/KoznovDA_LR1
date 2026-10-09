using System;
using System.Collections.Generic;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class EventDialog : Window
    {
        private static readonly int[] ReminderOffsetMinutes = { 5, 10, 15, 30, 60, 120, 1440 };

        public string EventTitle
        {
            get { return TitleBox.Text.Trim(); }
        }

        public string EventDescription
        {
            get { return DescriptionBox.Text.Trim(); }
        }

        public string EventLocation
        {
            get { return LocationBox.Text.Trim(); }
        }

        public DateTime EventStart { get; private set; }
        public DateTime EventEnd { get; private set; }

        public bool ReminderEnabled
        {
            get { return ReminderCheckBox.IsChecked == true; }
        }

        public DateTime? ReminderTime { get; private set; }

        public EventDialog(string title, string description, string location, DateTime start, DateTime? end = null)
        {
            InitializeComponent();
            EventStart = start;
            EventEnd = end ?? start.AddHours(1);
            FillTimeItems(TimeComboBox);
            StartDatePicker.SelectedDate = EventStart.Date;
            SelectTime(TimeComboBox, EventStart.TimeOfDay);
            StartDatePicker.SelectedDateChanged += (s, e) => SyncEndDateToStart();
            FillTimeItems(EndTimeComboBox);
            EndDatePicker.SelectedDate = EventEnd.Date;
            SelectTime(EndTimeComboBox, EventEnd.TimeOfDay);
            ReminderOffsetComboBox.ItemsSource = new[]
            {
                "5 минут", "10 минут", "15 минут", "30 минут", "1 час", "2 часа", "За день"
            };
            ReminderOffsetComboBox.SelectedIndex = 3;
            TitleBox.Text = title ?? string.Empty;
            DescriptionBox.Text = description ?? string.Empty;
            LocationBox.Text = location ?? string.Empty;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void SyncEndDateToStart()
        {
            if (EndDatePicker.SelectedDate == null)
            {
                EndDatePicker.SelectedDate = StartDatePicker.SelectedDate;
            }
        }

        private void FillTimeItems(System.Windows.Controls.ComboBox comboBox)
        {
            var items = new List<string>();
            var start = DateTime.Today;
            for (int i = 0; i < 48; i++)
            {
                items.Add(start.ToString("HH:mm"));
                start = start.AddMinutes(30);
            }
            comboBox.ItemsSource = items;
        }

        private void SelectTime(System.Windows.Controls.ComboBox comboBox, TimeSpan time)
        {
            var rounded = new TimeSpan(time.Hours, time.Minutes >= 30 ? 30 : 0, 0);
            string target = ((int)rounded.TotalHours).ToString("00") + ":" + rounded.Minutes.ToString("00");
            foreach (var item in comboBox.Items)
            {
                if (item.ToString() == target)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
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

        private DateTime GetStartFromControls()
        {
            var date = StartDatePicker.SelectedDate ?? DateTime.Today;
            var time = TimeSpan.Zero;
            if (TimeComboBox.SelectedItem != null && TimeSpan.TryParse(TimeComboBox.SelectedItem.ToString(), out var parsed))
            {
                time = parsed;
            }
            return date.Date + time;
        }

        private DateTime GetEndFromControls()
        {
            var date = EndDatePicker.SelectedDate ?? (StartDatePicker.SelectedDate ?? DateTime.Today);
            var time = TimeSpan.Zero;
            if (EndTimeComboBox.SelectedItem != null && TimeSpan.TryParse(EndTimeComboBox.SelectedItem.ToString(), out var parsed))
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
            var start = GetStartFromControls();
            if (start < DateTime.MinValue.AddMinutes(ReminderOffsetMinutes[index]))
            {
                return;
            }
            ReminderTime = start.AddMinutes(-ReminderOffsetMinutes[index]);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EventTitle))
            {
                MessageBox.Show("Введите название события.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            EventStart = GetStartFromControls();
            EventEnd = GetEndFromControls();
            if (EventEnd < EventStart)
            {
                MessageBox.Show("Окончание события не может быть раньше начала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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

        private void LocationBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (LocationWatermark != null)
            {
                LocationWatermark.Visibility = string.IsNullOrEmpty(LocationBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
