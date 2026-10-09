using System;
using System.Collections.Generic;
using System.Linq;
using DailyPlanner.Core.Exceptions;
using DailyPlanner.Core.Models;

namespace DailyPlanner.Core.Services
{
    /// <summary>Сервис работы с напоминаниями.</summary>
    public class ReminderService : IReminderService
    {
        private readonly List<Reminder> _reminders = new List<Reminder>();

        public void Schedule(Reminder reminder)
        {
            if (reminder == null)
                throw new ValidationException(
                    "Напоминание не задано.", "reminder");
            if (string.IsNullOrWhiteSpace(reminder.Message))
                throw new ValidationException(
                    "Текст напоминания не может быть пустым.", "Message");
            if (reminder.Time < DateTime.Now)
                throw new ValidationException(
                    "Время напоминания не может находиться в прошлом.", "Time");

            _reminders.Add(reminder);
        }

        public IEnumerable<Reminder> GetDue(DateTime now)
        {
            return _reminders.Where(r => !r.IsShown && r.Time <= now)
                             .ToList();
        }
    }
}
