using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DailyPlanner.Core.Exceptions;
using DailyPlanner.Core.Models;
using DailyPlanner.Core.Services;

namespace DailyPlanner.Core.Tests
{
    /// <summary>Интеграционные сценарии с обработкой исключений.</summary>
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void AddTask_SeveralErrors_AllHandled()
        {
            ITaskService service = new TaskService();
            int handled = 0;

            // Пустое название – перехват ValidationException
            try
            {
                service.Add(new TaskItem
                {
                    Title = "",
                    Deadline = DateTime.Now.AddHours(1)
                });
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual("Title", ex.ParamName);
                handled++;
            }
            finally
            {
                Assert.AreEqual(0, service.GetAll().Count());
            }

            // Повтор задачи – перехват DuplicateEntryException
            TaskItem task = new TaskItem
            {
                Title = "Купить хлеб",
                Deadline = DateTime.Now.AddHours(3)
            };
            try
            {
                service.Add(task);
                service.Add(new TaskItem
                {
                    Title = "Купить хлеб",
                    Deadline = task.Deadline
                });
                Assert.Fail("Повторная задача не должна добавляться.");
            }
            catch (DuplicateEntryException)
            {
                handled++;
            }
            finally
            {
                Assert.AreEqual(1, service.GetAll().Count());
            }

            // Неизвестный идентификатор – перехват EntryNotFoundException
            try
            {
                service.GetById(42);
            }
            catch (EntryNotFoundException)
            {
                handled++;
            }
            finally
            {
                Assert.AreEqual(1, service.GetAll().Count());
            }

            Assert.AreEqual(3, handled);
        }

        [TestMethod]
        public void Reminders_OnlyDueReturned()
        {
            IReminderService service = new ReminderService();
            DateTime now = DateTime.Now;

            service.Schedule(new Reminder
            {
                Message = "Позвонить мастеру",
                Time = now.AddSeconds(10)
            });

            Assert.AreEqual(0, service.GetDue(now).Count());
            Assert.AreEqual(1, service.GetDue(now.AddMinutes(1)).Count());
        }
    }
}
