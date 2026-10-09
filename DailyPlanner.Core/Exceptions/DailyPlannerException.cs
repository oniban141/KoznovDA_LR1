using System;

namespace DailyPlanner.Core.Exceptions
{
    /// <summary>Базовое исключение библиотеки DailyPlanner.Core.</summary>
    public class DailyPlannerException : Exception
    {
        public DailyPlannerException() { }
        public DailyPlannerException(string message) : base(message) { }
        public DailyPlannerException(string message, Exception inner)
            : base(message, inner) { }
    }
}
