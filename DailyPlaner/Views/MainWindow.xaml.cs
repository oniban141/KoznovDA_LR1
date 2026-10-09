using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DailyPlaner.ViewModels;

namespace DailyPlaner
{
    public partial class MainWindow : Window
    {
        public bool AllowClose { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            App.SetWindowIcon(this);
            DataContext = new MainViewModel { ActivePage = MainViewModel.PageOverview };
            AutoStartSidebarCheckBox.IsChecked = App.LoadAutoStartSetting();
            Loaded += (s, e) => RefreshDayLists();
        }

        public MainWindow(User user)
        {
            InitializeComponent();
            App.SetWindowIcon(this);
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            viewModel.CurrentUser = user;
            viewModel.ActivePage = MainViewModel.PageOverview;
            NavAdminButton.Visibility = viewModel.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            AutoStartSidebarCheckBox.IsChecked = App.LoadAutoStartSetting();
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedDate) || e.PropertyName == nameof(MainViewModel.Tasks) || e.PropertyName == nameof(MainViewModel.Events))
                {
                    RefreshDayLists();
                }
            };
            Loaded += (s, e) => RefreshDayLists();
        }

        private void SidebarNav_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is string pageName))
            {
                return;
            }
            ShowPage(pageName);
            if (pageName == MainViewModel.PageCalendar)
            {
                RefreshDayLists();
            }
        }

        private void ShowPage(string pageName)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ActivePage = pageName;
                vm.ResetFiltersAndReload();
            }
            OverviewPage.Visibility = pageName == MainViewModel.PageOverview ? Visibility.Visible : Visibility.Collapsed;
            TasksPage.Visibility = pageName == MainViewModel.PageTasks ? Visibility.Visible : Visibility.Collapsed;
            EventsPage.Visibility = pageName == MainViewModel.PageEvents ? Visibility.Visible : Visibility.Collapsed;
            NotesPage.Visibility = pageName == MainViewModel.PageNotes ? Visibility.Visible : Visibility.Collapsed;
            CalendarPage.Visibility = pageName == MainViewModel.PageCalendar ? Visibility.Visible : Visibility.Collapsed;
            AdminPage.Visibility = pageName == MainViewModel.PageAdmin ? Visibility.Visible : Visibility.Collapsed;
        }


        private void AutoStartSidebar_Changed(object sender, RoutedEventArgs e)
        {
            App.SetAutoStart(AutoStartSidebarCheckBox.IsChecked == true);
        }

        private void RefreshDayLists()
        {
            if (!(DataContext is MainViewModel vm) || vm.CurrentUser == null)
            {
                return;
            }
            var date = vm.SelectedDate.Date;
            DayEventsList.ItemsSource = vm.Events.Where(ev => ev.StartDate.Date == date).OrderBy(ev => ev.StartDate).ToList();
            DayTasksList.ItemsSource = vm.Tasks.Where(t => t.DueDate.Date == date).OrderBy(t => t.DueDate).ToList();
        }

        private void MainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDayLists();
        }

        private void DayItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (sender as ListView)?.SelectedItem;
            if (item is Event ev)
            {
                ShowEventDetails(ev);
            }
            else if (item is Task task)
            {
                ShowTaskDetails(task);
            }
        }

        private void TasksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Task task)
            {
                ShowTaskDetails(task);
            }
        }

        private void EventsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Event ev)
            {
                ShowEventDetails(ev);
            }
        }

        private void NotesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Note note)
            {
                ShowNoteDetails(note);
            }
        }

        private void ShowNoteDetails(Note note)
        {
            Views.Dialogs.DetailsDialog.ShowNote(this, note);
        }

        private void AdminUsersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is User user && DataContext is MainViewModel vm)
            {
                vm.ShowAdminUserDetails(user);
            }
        }

        private void AdminClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AdminSearchText = string.Empty;
            }
        }

        private void AdminClearDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AdminSearchDate = null;
            }
        }

        private void AdminCalendarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            AdminSearchDatePicker.IsDropDownOpen = true;
        }

        private void ShowTaskDetails(Task task)
        {
            Views.Dialogs.DetailsDialog.ShowTask(this, task);
        }

        private void ShowEventDetails(Event ev)
        {
            Views.Dialogs.DetailsDialog.ShowEvent(this, ev);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowClose && !App.IsExiting)
            {
                e.Cancel = true;
                Hide();
                App.TrayIcon?.ShowBalloonTip(3000, "Ежедневник",
                    "Приложение свернуто в трей и продолжает работу. Дважды щёлкните по значку, чтобы открыть.",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
            base.OnClosing(e);
        }
    }
}
