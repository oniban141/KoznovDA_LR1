using System;
using System.Collections.Generic;
using System.Linq;
using DailyPlanner.Core.Exceptions;
using DailyPlanner.Core.Models;

namespace DailyPlanner.Core.Services
{
    /// <summary>Сервис работы с задачами.</summary>
    public class TaskService : ITaskService
    {
        private readonly List<TaskItem> _tasks = new List<TaskItem>();
        private int _nextId = 1;

        public IEnumerable<TaskItem> GetAll()
        {
            return _tasks.ToList();
        }

        public TaskItem GetById(int id)
        {
            TaskItem task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                throw new EntryNotFoundException(
                    "Задача с идентификатором " + id + " не найдена.");
            return task;
        }

        public void Add(TaskItem task)
        {
            if (task == null)
                throw new ValidationException(
                    "Задача не задана.", "task");
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ValidationException(
                    "Название задачи не может быть пустым.", "Title");
            if (task.Deadline < DateTime.Now)
                throw new ValidationException(
                    "Срок выполнения задачи не может находиться в прошлом.",
                    "Deadline");
            bool duplicate = _tasks.Any(t =>
                t.Title == task.Title && t.Deadline == task.Deadline);
            if (duplicate)
                throw new DuplicateEntryException(
                    "Задача с таким названием и сроком уже существует.");

            task.Id = _nextId++;
            _tasks.Add(task);
        }

        public void Complete(int id)
        {
            TaskItem task = GetById(id);
            task.IsCompleted = true;
        }

        public void Delete(int id)
        {
            TaskItem task = GetById(id);
            _tasks.Remove(task);
        }
    }
}
