using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using DailyPlaner.Services;

namespace DailyPlaner.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public const string PageOverview = "OverviewPage";
        public const string PageTasks = "TasksPage";
        public const string PageEvents = "EventsPage";
        public const string PageNotes = "NotesPage";
        public const string PageCalendar = "CalendarPage";
        public const string PageAdmin = "AdminPage";

        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private ReminderScheduler _reminderScheduler;
        private User _currentUser;
        private ObservableCollection<Task> _tasks;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Note> _notes;
        private Task _selectedTask;
        private Event _selectedEvent;
        private Note _selectedNote;
        private DateTime _selectedDate;
        private ObservableCollection<User> _users;
        private User _selectedAdminUser;
        private bool _isAdmin;
        private List<User> _allUsers;
        private string _adminSearchText;
        private DateTime? _adminSearchDate;
        private string _adminStatsHeader;
        private int _totalTasks;
        private int _totalEvents;
        private int _totalNotes;
        private int _totalReminders;
        private int _totalUsers;

        public event PropertyChangedEventHandler PropertyChanged;

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                if (_currentUser != null)
                {
                    IsAdmin = _currentUser.RoleId == 2;
                    LoadUserData();
                    StartReminderScheduler();
                }
            }
        }

        public ObservableCollection<Task> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
                OnPropertyChanged(nameof(CompletedTasksCount));
                OnPropertyChanged(nameof(UpcomingTasks));
            }
        }

        public IEnumerable<Task> UpcomingTasks
        {
            get
            {
                return Tasks
                    .OrderBy(t => t.IsCompleted)
                    .ThenBy(t => t.DueDate)
                    .Take(10);
            }
        }

        public ObservableCollection<Event> Events
        {
            get { return _events; }
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
            }
        }

        public ObservableCollection<Note> Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public string ActivePage { get; set; }

        public Task SelectedTask
        {
            get { return _selectedTask; }
            set
            {
                _selectedTask = value;
                OnPropertyChanged(nameof(SelectedTask));
            }
        }

        public Event SelectedEvent
        {
            get { return _selectedEvent; }
            set
            {
                _selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent));
            }
        }

        public Note SelectedNote
        {
            get { return _selectedNote; }
            set
            {
                _selectedNote = value;
                OnPropertyChanged(nameof(SelectedNote));
            }
        }


        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        public int CompletedTasksCount
        {
            get { return Tasks.Count(t => t.IsCompleted); }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        public User SelectedAdminUser
        {
            get { return _selectedAdminUser; }
            set
            {
                _selectedAdminUser = value;
                OnPropertyChanged(nameof(SelectedAdminUser));
                UpdateAdminStats();
            }
        }

        public string AdminSearchText
        {
            get { return _adminSearchText; }
            set
            {
                _adminSearchText = value;
                OnPropertyChanged(nameof(AdminSearchText));
                ApplyAdminSearch();
            }
        }

        public DateTime? AdminSearchDate
        {
            get { return _adminSearchDate; }
            set
            {
                _adminSearchDate = value;
                OnPropertyChanged(nameof(AdminSearchDate));
                ApplyAdminSearch();
            }
        }

        public string AdminStatsHeader
        {
            get { return _adminStatsHeader; }
            set
            {
                _adminStatsHeader = value;
                OnPropertyChanged(nameof(AdminStatsHeader));
            }
        }

        public int TotalTasks
        {
            get { return _totalTasks; }
            set { SetField(ref _totalTasks, value, nameof(TotalTasks)); }
        }

        public int TotalEvents
        {
            get { return _totalEvents; }
            set { SetField(ref _totalEvents, value, nameof(TotalEvents)); }
        }

        public int TotalNotes
        {
            get { return _totalNotes; }
            set { SetField(ref _totalNotes, value, nameof(TotalNotes)); }
        }

        public int TotalReminders
        {
            get { return _totalReminders; }
            set { SetField(ref _totalReminders, value, nameof(TotalReminders)); }
        }

        public int TotalUsers
        {
            get { return _totalUsers; }
            set { SetField(ref _totalUsers, value, nameof(TotalUsers)); }
        }

        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand CompleteTaskCommand { get; }
        public ICommand AddEventCommand { get; }
        public ICommand EditEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand CompleteEventCommand { get; }
        public ICommand AddNoteCommand { get; }
        public ICommand EditNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand ExportToJsonCommand { get; }
        public ICommand ImportFromJsonCommand { get; }
        public ICommand TestNotificationCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ResetUserPasswordCommand { get; }

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            _notificationService = new NotificationService();
            Tasks = new ObservableCollection<Task>();
            Events = new ObservableCollection<Event>();
            Notes = new ObservableCollection<Note>();
            SelectedDate = DateTime.Today;
            AddTaskCommand = new RelayCommand(ExecuteAddTask);
            EditTaskCommand = new RelayCommand(ExecuteEditTask, CanExecuteTaskCommand);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteTaskCommand);
            CompleteTaskCommand = new RelayCommand(ExecuteCompleteTask, CanExecuteTaskCommand);
            AddEventCommand = new RelayCommand(ExecuteAddEvent);
            EditEventCommand = new RelayCommand(ExecuteEditEvent, CanExecuteEventCommand);
            DeleteEventCommand = new RelayCommand(ExecuteDeleteEvent, CanExecuteEventCommand);
            CompleteEventCommand = new RelayCommand(ExecuteCompleteEvent, CanExecuteEventCommand);
            AddNoteCommand = new RelayCommand(ExecuteAddNote);
            EditNoteCommand = new RelayCommand(ExecuteEditNote, CanExecuteNoteCommand);
            DeleteNoteCommand = new RelayCommand(ExecuteDeleteNote, CanExecuteNoteCommand);
            ExportToJsonCommand = new RelayCommand(ExecuteExportToJson);
            ImportFromJsonCommand = new RelayCommand(ExecuteImportFromJson);
            TestNotificationCommand = new RelayCommand(ExecuteTestNotification);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            DeleteUserCommand = new RelayCommand(ExecuteDeleteUser, CanExecuteAdminCommand);
            ResetUserPasswordCommand = new RelayCommand(ExecuteResetUserPassword, CanExecuteAdminCommand);
        }

        private void StartReminderScheduler()
        {
            if (_reminderScheduler == null)
            {
                _reminderScheduler = new ReminderScheduler();
            }
            _reminderScheduler.Start();
        }

        private void LoadUserData()
        {
            try
            {
                Tasks = new ObservableCollection<Task>(_databaseService.GetTasksByUserId(CurrentUser.Id));
                Events = new ObservableCollection<Event>(_databaseService.GetEventsByUserId(CurrentUser.Id));
                Notes = new ObservableCollection<Note>(_databaseService.GetNotesByUserId(CurrentUser.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteTaskCommand(object parameter)
        {
            return GetSelectedTasks(parameter).Count > 0;
        }

        private bool CanExecuteEventCommand(object parameter)
        {
            return GetSelectedEvents(parameter).Count > 0;
        }

        private bool CanExecuteNoteCommand(object parameter)
        {
            return GetSelectedNotes(parameter).Count > 0;
        }

        private List<Task> GetSelectedTasks(object parameter)
        {
            var items = new List<Task>();
            if (parameter is IList list)
            {
                items.AddRange(list.OfType<Task>());
            }
            if (items.Count == 0 && SelectedTask != null)
            {
                items.Add(SelectedTask);
            }
            return items;
        }

        private List<Event> GetSelectedEvents(object parameter)
        {
            var items = new List<Event>();
            if (parameter is IList list)
            {
                items.AddRange(list.OfType<Event>());
            }
            if (items.Count == 0 && SelectedEvent != null)
            {
                items.Add(SelectedEvent);
            }
            return items;
        }

        private List<Note> GetSelectedNotes(object parameter)
        {
            var items = new List<Note>();
            if (parameter is IList list)
            {
                items.AddRange(list.OfType<Note>());
            }
            if (items.Count == 0 && SelectedNote != null)
            {
                items.Add(SelectedNote);
            }
            return items;
        }

        private static string PluralForms(int count, string one, string few, string many)
        {
            if (count % 10 == 1 && count % 100 != 11)
            {
                return one;
            }
            if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 12 || count % 100 > 14))
            {
                return few;
            }
            return many;
        }

        private void ExecuteAddTask(object parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.TaskDialog(string.Empty, string.Empty, DateTime.Today.AddDays(1))
                {
                    Owner = Application.Current.MainWindow
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                var task = new Task
                {
                    UserId = CurrentUser.Id,
                    Title = dialog.TaskTitle,
                    Description = dialog.TaskDescription,
                    DueDate = dialog.TaskDueDate,
                    IsCompleted = false,
                    Priority = dialog.TaskPriority
                };
                if (_databaseService.CreateTask(task))
                {
                    if (dialog.ReminderEnabled && dialog.ReminderTime.HasValue)
                    {
                        _databaseService.CreateReminder(new Reminder
                        {
                            UserId = CurrentUser.Id,
                            TaskId = task.Id,
                            ReminderDate = dialog.ReminderTime.Value,
                            Message = $"Задача '{task.Title}' — срок {dialog.TaskDueDate:dd.MM.yyyy HH:mm}",
                            IsShown = true
                        });
                    }
                    RefreshDataForCurrentPage();
                    _notificationService.ShowNotification("Задача создана", $"Задача '{task.Title}' успешно добавлена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditTask(object parameter)
        {
            try
            {
                if (SelectedTask == null)
                {
                    return;
                }
                var dialog = new Views.Dialogs.TaskDialog(SelectedTask.Title, SelectedTask.Description, SelectedTask.DueDate, SelectedTask.Priority)
                {
                    Owner = Application.Current.MainWindow
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                SelectedTask.Title = dialog.TaskTitle;
                SelectedTask.Description = dialog.TaskDescription;
                SelectedTask.DueDate = dialog.TaskDueDate;
                SelectedTask.Priority = dialog.TaskPriority;
                if (_databaseService.UpdateTask(SelectedTask))
                {
                    RefreshDataForCurrentPage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteTask(object parameter)
        {
            var tasks = GetSelectedTasks(parameter);
            DeleteItems(tasks, t => _databaseService.DeleteTask(t.Id),
                () => PluralForms(tasks.Count, "задачу", "задачи", "задач"), "эту задачу", "задачи",
                () => SelectedTask = null);
        }

        private void ExecuteCompleteTask(object parameter)
        {
            var task = SelectedTask;
            if (task == null)
            {
                return;
            }
            try
            {
                task.IsCompleted = !task.IsCompleted;
                if (_databaseService.UpdateTask(task))
                {
                    string title = task.Title;
                    bool completed = task.IsCompleted;
                    RefreshDataForCurrentPage();
                    _notificationService.ShowNotification("Задача обновлена", $"Задача '{title}' отмечена как {(completed ? "выполненная" : "невыполненная")}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddEvent(object parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.EventDialog(string.Empty, string.Empty, string.Empty, DateTime.Today)
                {
                    Owner = Application.Current.MainWindow
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                var ev = new Event
                {
                    UserId = CurrentUser.Id,
                    Title = dialog.EventTitle,
                    Description = dialog.EventDescription,
                    StartDate = dialog.EventStart,
                    EndDate = dialog.EventEnd,
                    Location = dialog.EventLocation
                };
                if (_databaseService.CreateEvent(ev))
                {
                    if (dialog.ReminderEnabled && dialog.ReminderTime.HasValue)
                    {
                        string message = $"Событие '{ev.Title}' — начало {dialog.EventStart:dd.MM.yyyy HH:mm}" + (string.IsNullOrWhiteSpace(ev.Location) ? "" : $", место: {ev.Location}");
                        _notificationService.ScheduleNotification("Напоминание", message, dialog.ReminderTime.Value);
                    }
                    RefreshDataForCurrentPage();
                    _notificationService.ShowNotification("Событие создано", $"Событие '{ev.Title}' успешно добавлено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditEvent(object parameter)
        {
            try
            {
                if (SelectedEvent == null)
                {
                    return;
                }
                var dialog = new Views.Dialogs.EventDialog(SelectedEvent.Title, SelectedEvent.Description, SelectedEvent.Location, SelectedEvent.StartDate, SelectedEvent.EndDate)
                {
                    Owner = Application.Current.MainWindow
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                SelectedEvent.Title = dialog.EventTitle;
                SelectedEvent.Description = dialog.EventDescription;
                SelectedEvent.Location = dialog.EventLocation;
                SelectedEvent.StartDate = dialog.EventStart;
                SelectedEvent.EndDate = dialog.EventEnd;
                if (_databaseService.UpdateEvent(SelectedEvent))
                {
                    RefreshDataForCurrentPage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteEvent(object parameter)
        {
            var events = GetSelectedEvents(parameter);
            DeleteItems(events, ev => _databaseService.DeleteEvent(ev.Id),
                () => PluralForms(events.Count, "событие", "события", "событий"), "это событие", "события",
                () => SelectedEvent = null);
        }

        private void ExecuteCompleteEvent(object parameter)
        {
            var ev = SelectedEvent;
            if (ev == null)
            {
                return;
            }
            try
            {
                ev.IsCompleted = !ev.IsCompleted;
                if (_databaseService.UpdateEvent(ev))
                {
                    string title = ev.Title;
                    bool completed = ev.IsCompleted;
                    RefreshDataForCurrentPage();
                    _notificationService.ShowNotification("Событие обновлено", $"Событие '{title}' отмечено как {(completed ? "завершённое" : "незавершённое")}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddNote(object parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.NoteDialog(string.Empty, string.Empty)
                {
                    Owner = Application.Current.MainWindow
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                var note = new Note
                {
                    UserId = CurrentUser.Id,
                    Title = dialog.NoteTitle,
                    Content = dialog.NoteContent,
                    CreatedDate = DateTime.Now
                };
                if (_databaseService.CreateNote(note))
                {
                    RefreshDataForCurrentPage();
                    _notificationService.ShowNotification("Заметка создана", $"Заметка '{note.Title}' успешно добавлена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditNote(object parameter)
        {
            try
            {
                if (SelectedNote == null)
                {
                    return;
                }
                var dialog = new Views.Dialogs.NoteDialog(SelectedNote.Title, SelectedNote.Content)
                {
                    Owner = Application.Current.MainWindow
                };
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                SelectedNote.Title = dialog.NoteTitle;
                SelectedNote.Content = dialog.NoteContent;
                if (_databaseService.UpdateNote(SelectedNote))
                {
                    RefreshDataForCurrentPage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteNote(object parameter)
        {
            var notes = GetSelectedNotes(parameter);
            DeleteItems(notes, n => _databaseService.DeleteNote(n.Id),
                () => PluralForms(notes.Count, "заметку", "заметки", "заметок"), "эту заметку", "заметки",
                () => SelectedNote = null);
        }

        private void DeleteItems<T>(List<T> items, Func<T, bool> delete, Func<string> word, string singleText, string errorTitle, Action resetSelection)
        {
            try
            {
                if (items.Count == 0)
                {
                    return;
                }
                string plural = word();
                bool confirmed = items.Count > 1
                    ? MessageBox.Show($"Вы точно хотите удалить {items.Count} {plural}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes
                    : MessageBox.Show($"Вы уверены, что хотите удалить {singleText}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                if (!confirmed)
                {
                    return;
                }
                int deleted = items.Count(item => delete(item));
                if (deleted > 0)
                {
                    RefreshDataForCurrentPage();
                    resetSelection();
                }
                if (deleted < items.Count)
                {
                    MessageBox.Show($"Удалено {deleted} из {items.Count}. Некоторые записи не удалось удалить.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении {errorTitle}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void ResetFiltersAndReload()
        {
            if (CurrentUser == null)
            {
                return;
            }
            if (ActivePage == PageAdmin)
            {
                LoadAdminData();
            }
            else
            {
                LoadUserData();
            }
        }

        private void RefreshDataForCurrentPage()
        {
            if (CurrentUser == null)
            {
                return;
            }
            if (ActivePage == PageAdmin)
            {
                LoadAdminData();
            }
            else
            {
                LoadUserData();
            }
        }

        private void LoadAdminData()
        {
            try
            {
                _allUsers = _databaseService.GetAllUsers();
                ApplyAdminSearch();
                if (SelectedAdminUser == null)
                {
                    UpdateAdminStats();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных администрирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyAdminSearch()
        {
            if (_allUsers == null)
            {
                return;
            }
            string query = AdminSearchText?.Trim() ?? string.Empty;
            var filtered = _allUsers.Where(u =>
                (query.Length == 0 ||
                    (u.Username != null && u.Username.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (u.Email != null && u.Email.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                (AdminSearchDate == null || u.CreatedAt.Date == AdminSearchDate.Value.Date)).ToList();
            Users = new ObservableCollection<User>(filtered);
        }

        private void UpdateAdminStats()
        {
            var user = SelectedAdminUser;
            if (user == null)
            {
                AdminStatsHeader = "Общая статистика по всей базе";
                TotalUsers = _databaseService.CountRows("Users");
                TotalTasks = _databaseService.CountRows("Tasks");
                TotalEvents = _databaseService.CountRows("Events");
                TotalNotes = _databaseService.CountRows("Notes");
                TotalReminders = _databaseService.CountRows("Reminders");
            }
            else
            {
                AdminStatsHeader = $"Данные пользователя \"{user.Username}\"";
                TotalUsers = 1;
                TotalTasks = _databaseService.CountUserRows("Tasks", user.Id);
                TotalEvents = _databaseService.CountUserRows("Events", user.Id);
                TotalNotes = _databaseService.CountUserRows("Notes", user.Id);
                TotalReminders = _databaseService.CountUserRows("Reminders", user.Id);
            }
        }

        private bool CanExecuteAdminCommand(object parameter)
        {
            return SelectedAdminUser != null && SelectedAdminUser.Id != CurrentUser?.Id;
        }

        private void ExecuteDeleteUser(object parameter)
        {
            try
            {
                var user = SelectedAdminUser;
                if (user == null || user.Id == CurrentUser.Id)
                {
                    return;
                }
                var answer = MessageBox.Show($"Вы точно хотите удалить пользователя \"{user.Username}\" со всеми его задачами, событиями, заметками и напоминаниями?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }
                if (_databaseService.DeleteUserWithAllData(user.Id))
                {
                    var name = user.Username;
                    SelectedAdminUser = null;
                    LoadAdminData();
                    MessageBox.Show($"Пользователь \"{name}\" удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось удалить пользователя. Попробуйте ещё раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteResetUserPassword(object parameter)
        {
            try
            {
                var user = SelectedAdminUser;
                if (user == null || user.Id == CurrentUser.Id)
                {
                    return;
                }
                var answer = MessageBox.Show($"Сбросить пароль пользователя \"{user.Username}\" на стандартный (user123)?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }
                if (_databaseService.ResetUserPassword(user.Id, DatabaseService.HashPassword("user123")))
                {
                    MessageBox.Show($"Пароль пользователя \"{user.Username}\" сброшен на user123.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось сбросить пароль. Попробуйте ещё раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowAdminUserDetails(User user)
        {
            if (user == null)
            {
                return;
            }
            try
            {
                int tasks = _databaseService.CountUserRows("Tasks", user.Id);
                int events = _databaseService.CountUserRows("Events", user.Id);
                int notes = _databaseService.CountUserRows("Notes", user.Id);
                int reminders = _databaseService.CountUserRows("Reminders", user.Id);
                string role = user.RoleId == 2 ? "Администратор" : "Пользователь";
                MessageBox.Show(
                    $"Логин: {user.Username}\nEmail: {(string.IsNullOrWhiteSpace(user.Email) ? "—" : user.Email)}\nРоль: {role}\nДата регистрации: {user.CreatedAt:dd.MM.yyyy HH:mm}\n\nЗадач: {tasks}\nСобытий: {events}\nЗаметок: {notes}\nНапоминаний: {reminders}",
                    $"Пользователь \"{user.Username}\"", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке информации о пользователе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExportToJson(object parameter)
        {
            try
            {
                var data = new
                {
                    Tasks = _databaseService.GetTasksByUserId(CurrentUser.Id).Select(t => new
                    {
                        t.Title, t.Description, t.DueDate, t.Priority, t.Status
                    }),
                    Events = _databaseService.GetEventsByUserId(CurrentUser.Id).Select(e => new
                    {
                        e.Title, e.Description, e.StartDate, e.EndDate, e.Location, e.Status
                    }),
                    Notes = _databaseService.GetNotesByUserId(CurrentUser.Id).Select(n => new
                    {
                        n.Title, n.Content, CreatedAt = n.CreatedDate
                    }),
                    Settings = new
                    {
                        AutoStart = App.LoadAutoStartSetting()
                    }
                };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Файлы JSON (*.json)|*.json|Все файлы (*.*)|*.*",
                    DefaultExt = ".json"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("Задачи, события, заметки и настройки приложения успешно экспортированы в JSON!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в JSON: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteImportFromJson(object parameter)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Файлы JSON (*.json)|*.json|Все файлы (*.*)|*.*"
                };
                if (openFileDialog.ShowDialog() != true)
                {
                    return;
                }
                string json = File.ReadAllText(openFileDialog.FileName);
                if (string.IsNullOrWhiteSpace(json))
                {
                    MessageBox.Show("Файл пуст — нечего импортировать.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var data = JsonConvert.DeserializeObject<BackupData>(json);
                if (data != null)
                {
                    int imported = 0;
                    if (data.Tasks != null)
                    {
                        foreach (var task in data.Tasks)
                        {
                            task.UserId = CurrentUser.Id;
                            if (_databaseService.CreateTask(task)) imported++;
                        }
                    }
                    if (data.Events != null)
                    {
                        foreach (var ev in data.Events)
                        {
                            ev.UserId = CurrentUser.Id;
                            if (_databaseService.CreateEvent(ev)) imported++;
                        }
                    }
                    if (data.Notes != null)
                    {
                        foreach (var note in data.Notes)
                        {
                            note.UserId = CurrentUser.Id;
                            if (_databaseService.CreateNote(note)) imported++;
                        }
                    }
                    if (data.Settings != null && data.Settings.AutoStart.HasValue)
                    {
                        App.SetAutoStart(data.Settings.AutoStart.Value);
                    }
                    RefreshDataForCurrentPage();
                    MessageBox.Show($"Данные успешно импортированы из JSON (записей: {imported})!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из JSON: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteTestNotification(object parameter)
        {
            try
            {
                if (_reminderScheduler == null)
                {
                    StartReminderScheduler();
                }
                int pending = _reminderScheduler != null ? _reminderScheduler.GetPendingReminderCount() : -1;
                DateTime? next = _reminderScheduler != null ? _reminderScheduler.GetNextReminderTime() : null;
                string schedulerInfo;
                if (pending < 0)
                {
                    schedulerInfo = "Не удалось получить данные о напоминаниях из базы (проверьте подключение к базе данных).";
                }
                else if (pending == 0)
                {
                    schedulerInfo = "Активных напоминаний в базе нет — напоминания показываются только для существующих записей.";
                }
                else
                {
                    schedulerInfo = $"Активных напоминаний в базе: {pending}. Ближайшее: {next:dd.MM.yyyy HH:mm}.";
                }
                _notificationService.ShowReminderNotification(
                    "Проверка напоминаний",
                    $"Если вы видите это уведомление — напоминания работают.\n\n{schedulerInfo}",
                    DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке напоминаний: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteLogout(object parameter)
        {
            try
            {
                _reminderScheduler?.Stop();
                CurrentUser = null;
                Tasks.Clear();
                Events.Clear();
                Notes.Clear();
                var loginHost = new System.Windows.Navigation.NavigationWindow
                {
                    Content = new Views.LoginWindow(),
                    ShowsNavigationUI = false,
                    Title = "Ежедневник — вход",
                    Width = 1000,
                    Height = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                loginHost.Show();
                Application.Current.MainWindow = loginHost;
                foreach (System.Windows.Window window in Application.Current.Windows)
                {
                    if (window != loginHost)
                    {
                        if (window is MainWindow mainWindow)
                        {
                            mainWindow.AllowClose = true;
                        }
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (Equals(field, value))
            {
                return;
            }
            field = value;
            OnPropertyChanged(propertyName);
        }
    }

    public class BackupData
    {
        public List<Task> Tasks { get; set; }
        public List<Event> Events { get; set; }
        public List<Note> Notes { get; set; }
        public BackupSettings Settings { get; set; }
    }

    public class BackupSettings
    {
        public bool? AutoStart { get; set; }
    }
}
