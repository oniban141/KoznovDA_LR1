using System.Collections.Generic;
using DailyPlanner.Core.Models;

namespace DailyPlanner.Core.Services
{
    /// <summary>Интерфейс сервиса задач.</summary>
    public interface ITaskService
    {
        IEnumerable<TaskItem> GetAll();
        TaskItem GetById(int id);
        void Add(TaskItem task);
        void Complete(int id);
        void Delete(int id);
    }
}
