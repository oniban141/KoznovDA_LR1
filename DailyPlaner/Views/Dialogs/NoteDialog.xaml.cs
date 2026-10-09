using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class NoteDialog : Window
    {
        public string NoteTitle
        {
            get { return TitleBox.Text.Trim(); }
        }

        public string NoteContent
        {
            get { return ContentBox.Text.Trim(); }
        }

        public NoteDialog(string title, string content)
        {
            InitializeComponent();
            TitleBox.Text = title ?? string.Empty;
            ContentBox.Text = content ?? string.Empty;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoteTitle))
            {
                MessageBox.Show("Введите название заметки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
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

        private void ContentBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ContentWatermark != null)
            {
               ContentWatermark.Visibility = string.IsNullOrEmpty(ContentBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
