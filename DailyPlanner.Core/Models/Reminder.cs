using System;

namespace DailyPlanner.Core.Models
{
    /// <summary>Напоминание ежедневника.</summary>
    public class Reminder
    {
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public bool IsShown { get; set; }
    }
}
