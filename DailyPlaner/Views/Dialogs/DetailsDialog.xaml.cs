using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class DetailsDialog : Window
    {
        private DetailsDialog()
        {
            InitializeComponent();
        }

        public static void ShowTask(Window owner, Task task)
        {
            var dialog = new DetailsDialog { Owner = owner };

            dialog.KindLabel.Text = "Задача";
            dialog.TitleText.Text = string.IsNullOrWhiteSpace(task.Title) ? "—" : task.Title;
            dialog.DescriptionText.Text = string.IsNullOrWhiteSpace(task.Description) ? "—" : task.Description;
            dialog.TimeLabel.Text = "Срок выполнения";
            dialog.TimeText.Text = task.DueDate.ToString("dd.MM.yyyy HH:mm");
            dialog.ExtraLabel.Text = "Приоритет";
            dialog.ExtraText.Text = string.IsNullOrWhiteSpace(task.Priority) ? "—" : task.Priority;
            dialog.StatusLabel.Text = "Статус";
            dialog.StatusText.Text = task.IsCompleted ? "\u2705 Выполнена" : "\U0001F552 Ожидает выполнения";
            dialog.ShowDialog();
        }

        public static void ShowEvent(Window owner, Event ev)
        {
            var dialog = new DetailsDialog { Owner = owner };

            dialog.KindLabel.Text = "Событие";
            dialog.TitleText.Text = string.IsNullOrWhiteSpace(ev.Title) ? "—" : ev.Title;
            dialog.DescriptionText.Text = string.IsNullOrWhiteSpace(ev.Description) ? "—" : ev.Description;
            dialog.TimeLabel.Text = "Начало и окончание";
            dialog.TimeText.Text = $"{ev.StartDate:dd.MM.yyyy HH:mm} — {ev.EndDate:dd.MM.yyyy HH:mm}";
            dialog.ExtraLabel.Text = "Место";
            dialog.ExtraText.Text = string.IsNullOrWhiteSpace(ev.Location) ? "—" : ev.Location;
            dialog.StatusLabel.Text = "Статус";
            dialog.StatusText.Text = ev.IsCompleted ? "\u2705 Завершено" : "\U0001F552 Запланировано";
            dialog.ShowDialog();
        }

        public static void ShowNote(Window owner, Note note)
        {
            var dialog = new DetailsDialog { Owner = owner };

            dialog.KindLabel.Text = "Заметка";
            dialog.TitleText.Text = string.IsNullOrWhiteSpace(note.Title) ? "—" : note.Title;
            dialog.DescriptionText.Text = string.IsNullOrWhiteSpace(note.Content) ? "—" : note.Content;
            dialog.TimeLabel.Text = "Создано";
            dialog.TimeText.Text = note.CreatedDate.ToString("dd.MM.yyyy HH:mm");
            dialog.ExtraLabel.Visibility = Visibility.Collapsed;
            dialog.ExtraText.Visibility = Visibility.Collapsed;
            dialog.StatusLabel.Visibility = Visibility.Collapsed;
            dialog.StatusText.Visibility = Visibility.Collapsed;
            dialog.ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
