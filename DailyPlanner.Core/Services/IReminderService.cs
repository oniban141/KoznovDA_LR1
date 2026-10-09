using System;
using System.Collections.Generic;
using DailyPlanner.Core.Models;

namespace DailyPlanner.Core.Services
{
    /// <summary>Интерфейс сервиса напоминаний.</summary>
    public interface IReminderService
    {
        void Schedule(Reminder reminder);
        IEnumerable<Reminder> GetDue(DateTime now);
    }
}
