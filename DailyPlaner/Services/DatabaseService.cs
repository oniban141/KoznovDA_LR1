using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace DailyPlaner.Services
{
    public class DatabaseService
    {
        public static string GetFriendlyDatabaseError(Exception exception)
        {
            var sqlEx = exception as SqlException;
            if (sqlEx == null)
            {
                return $"Ошибка базы данных: {exception.Message}";
            }
            switch (sqlEx.Number)
            {
                case -1:
                case 2:
                case 53:
                    return "Сервер 'PCGl1tch' недоступен. Проверьте, что SQL Server запущен, и имя сервера указано верно (Server=PCGl1tch).";
                case 4060:
                    return "База данных 'DailyPlanerDBV3' не существует или недоступна.";
                case 18456:
                    return "Ошибка авторизации Windows. Проверьте, что учётная запись Windows имеет доступ к SQL Server.";
                case 18452:
                    return "Неудачная попытка входа. Проверьте режим аутентификации SQL Server (Windows Authentication).";
                default:
                    return $"Ошибка базы данных (код {sqlEx.Number}): {sqlEx.Message}";
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private static Exception GetInnermostException(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        private static bool Save(DbContext context, bool showError = false)
        {
            try
            {
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                if (showError)
                {
                    System.Windows.MessageBox.Show(GetFriendlyDatabaseError(GetInnermostException(ex)), "Ошибка базы данных", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                return false;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    context.Database.Connection.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(GetFriendlyDatabaseError(GetInnermostException(ex)), ex);
            }
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Users.FirstOrDefault(u => u.Username == username);
                }
            }
            catch
            {
                return null;
            }
        }

        public bool UsernameExists(string username)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Users.Any(u => u.Username == username);
                }
            }
            catch
            {
                return false;
            }
        }

        public bool CreateUser(User user)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                context.Users.Add(user);
                return Save(context, true);
            }
        }

        public List<Gender> GetAllGenders()
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Genders.ToList();
                }
            }
            catch
            {
                return new List<Gender>();
            }
        }

        public List<Task> GetTasksByUserId(int userId)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Tasks.Where(t => t.UserId == userId).ToList();
                }
            }
            catch
            {
                return new List<Task>();
            }
        }

        public bool CreateTask(Task task)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                task.Status = task.IsCompleted ? "Выполнена" : "Ожидает";
                if (string.IsNullOrEmpty(task.Priority))
                {
                    task.Priority = "Средний";
                }
                context.Tasks.Add(task);
                return Save(context, true);
            }
        }

        public bool UpdateTask(Task task)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                var dbTask = context.Tasks.Find(task.Id);
                if (dbTask == null)
                {
                    return false;
                }
                dbTask.UserId = task.UserId;
                dbTask.Title = task.Title;
                dbTask.Description = task.Description;
                dbTask.DueDate = task.DueDate;
                dbTask.Priority = string.IsNullOrEmpty(task.Priority) ? "Средний" : task.Priority;
                dbTask.Status = task.IsCompleted ? "Выполнена" : "Ожидает";
                return Save(context);
            }
        }

        public bool DeleteTask(int id)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                context.Reminders.RemoveRange(context.Reminders.Where(r => r.TaskId == id));
                var task = context.Tasks.Find(id);
                if (task != null)
                {
                    context.Tasks.Remove(task);
                }
                return Save(context);
            }
        }

        public List<Event> GetEventsByUserId(int userId)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Events.Where(e => e.UserId == userId).ToList();
                }
            }
            catch
            {
                return new List<Event>();
            }
        }

        public bool CreateEvent(Event ev)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                ev.Status = ev.IsCompleted ? "Завершено" : "Запланировано";
                context.Events.Add(ev);
                return Save(context, true);
            }
        }

        public bool UpdateEvent(Event ev)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                var dbEvent = context.Events.Find(ev.Id);
                if (dbEvent == null)
                {
                    return false;
                }
                dbEvent.UserId = ev.UserId;
                dbEvent.Title = ev.Title;
                dbEvent.Description = ev.Description;
                dbEvent.StartDate = ev.StartDate;
                dbEvent.EndDate = ev.EndDate;
                dbEvent.Location = ev.Location;
                dbEvent.Status = ev.IsCompleted ? "Завершено" : "Запланировано";
                return Save(context);
            }
        }

        public bool DeleteEvent(int id)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                context.Reminders.RemoveRange(context.Reminders.Where(r => r.EventId == id));
                var ev = context.Events.Find(id);
                if (ev != null)
                {
                    context.Events.Remove(ev);
                }
                return Save(context);
            }
        }

        public List<Note> GetNotesByUserId(int userId)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Notes.Where(n => n.UserId == userId).ToList();
                }
            }
            catch
            {
                return new List<Note>();
            }
        }

        public bool CreateNote(Note note)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                if (note.CreatedDate == default(DateTime))
                {
                    note.CreatedDate = DateTime.Now;
                }
                context.Notes.Add(note);
                return Save(context, true);
            }
        }

        public bool UpdateNote(Note note)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                var dbNote = context.Notes.Find(note.Id);
                if (dbNote == null)
                {
                    return false;
                }
                dbNote.UserId = note.UserId;
                dbNote.Title = note.Title;
                dbNote.Content = note.Content;
                return Save(context);
            }
        }

        public bool DeleteNote(int id)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                var note = context.Notes.Find(id);
                if (note != null)
                {
                    context.Notes.Remove(note);
                }
                return Save(context);
            }
        }

        public List<User> GetAllUsers()
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Users.OrderBy(u => u.CreatedAt).ToList();
                }
            }
            catch
            {
                return new List<User>();
            }
        }

        public bool ResetUserPassword(int userId, string passwordHash)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                var user = context.Users.Find(userId);
                if (user == null)
                {
                    return false;
                }
                user.PasswordHash = passwordHash;
                return Save(context, true);
            }
        }

        public bool DeleteUserWithAllData(int userId)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                context.Reminders.RemoveRange(context.Reminders.Where(r => r.UserId == userId));
                context.Tasks.RemoveRange(context.Tasks.Where(t => t.UserId == userId));
                context.Events.RemoveRange(context.Events.Where(e => e.UserId == userId));
                context.Notes.RemoveRange(context.Notes.Where(n => n.UserId == userId));
                var user = context.Users.Find(userId);
                if (user != null)
                {
                    context.Users.Remove(user);
                }
                return Save(context, true);
            }
        }

        public int CountRows(string table)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    switch (table)
                    {
                        case "Users": return context.Users.Count();
                        case "Tasks": return context.Tasks.Count();
                        case "Events": return context.Events.Count();
                        case "Notes": return context.Notes.Count();
                        case "Reminders": return context.Reminders.Count();
                        default: return 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        public int CountUserRows(string table, int userId)
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    switch (table)
                    {
                        case "Tasks": return context.Tasks.Count(t => t.UserId == userId);
                        case "Events": return context.Events.Count(e => e.UserId == userId);
                        case "Notes": return context.Notes.Count(n => n.UserId == userId);
                        case "Reminders": return context.Reminders.Count(r => r.UserId == userId);
                        default: return 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        public List<Reminder> GetAllReminders()
        {
            try
            {
                using (var context = new DailyPlannerDBV3Entities())
                {
                    return context.Reminders.ToList();
                }
            }
            catch
            {
                return new List<Reminder>();
            }
        }

        public bool CreateReminder(Reminder reminder)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                context.Reminders.Add(reminder);
                return Save(context, true);
            }
        }

        public bool UpdateReminder(Reminder reminder)
        {
            using (var context = new DailyPlannerDBV3Entities())
            {
                var dbReminder = context.Reminders.Find(reminder.Id);
                if (dbReminder == null)
                {
                    return false;
                }
                dbReminder.UserId = reminder.UserId;
                dbReminder.TaskId = reminder.TaskId;
                dbReminder.ReminderTime = reminder.ReminderTime;
                dbReminder.Message = reminder.Message;
                dbReminder.IsActive = reminder.IsActive;
                return Save(context);
            }
        }
    }
}
