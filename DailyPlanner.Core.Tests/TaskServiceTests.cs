using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DailyPlanner.Core.Exceptions;
using DailyPlanner.Core.Models;
using DailyPlanner.Core.Services;

namespace DailyPlanner.Core.Tests
{
    [TestClass]
    public class TaskServiceTests
    {
        private static TaskItem ValidTask()
        {
            return new TaskItem
            {
                Title = "Подготовить отчёт",
                Description = "Отчёт по лабораторной работе",
                Deadline = DateTime.Now.AddHours(2)
            };
        }

        [TestMethod]
        public void Add_ValidTask_Succeeds()
        {
            ITaskService service = new TaskService();
            TaskItem task = ValidTask();

            service.Add(task);

            Assert.AreEqual(1, service.GetAll().Count());
            Assert.IsTrue(task.Id > 0);
        }

        [TestMethod]
        public void Add_NullTask_ThrowsValidationException()
        {
            ITaskService service = new TaskService();

            Assert.ThrowsException<ValidationException>(
                () => service.Add(null));
        }

        [TestMethod]
        public void Add_EmptyTitle_ThrowsValidationException()
        {
            ITaskService service = new TaskService();
            TaskItem task = ValidTask();
            task.Title = "   ";

            Assert.ThrowsException<ValidationException>(
                () => service.Add(task));
        }

        [TestMethod]
        public void Add_PastDeadline_ThrowsValidationException()
        {
            ITaskService service = new TaskService();
            TaskItem task = ValidTask();
            task.Deadline = DateTime.Now.AddHours(-1);

            Assert.ThrowsException<ValidationException>(
                () => service.Add(task));
        }

        [TestMethod]
        public void Add_DuplicateTask_ThrowsDuplicateEntryException()
        {
            ITaskService service = new TaskService();
            service.Add(ValidTask());

            Assert.ThrowsException<DuplicateEntryException>(
                () => service.Add(ValidTask()));
        }

        [TestMethod]
        public void GetById_UnknownId_ThrowsEntryNotFoundException()
        {
            ITaskService service = new TaskService();

            Assert.ThrowsException<EntryNotFoundException>(
                () => service.GetById(999));
        }

        [TestMethod]
        public void Complete_SetsFlag()
        {
            ITaskService service = new TaskService();
            TaskItem task = ValidTask();
            service.Add(task);

            service.Complete(task.Id);

            Assert.IsTrue(service.GetById(task.Id).IsCompleted);
        }

        [TestMethod]
        public void Delete_RemovesTask()
        {
            ITaskService service = new TaskService();
            TaskItem task = ValidTask();
            service.Add(task);

            service.Delete(task.Id);

            Assert.AreEqual(0, service.GetAll().Count());
        }
    }
}
