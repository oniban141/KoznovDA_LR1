using System;

namespace DailyPlanner.Core.Models
{
    /// <summary>Задача ежедневника.</summary>
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }
    }
}
