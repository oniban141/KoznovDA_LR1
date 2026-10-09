namespace DailyPlanner.Core.Exceptions
{
    /// <summary>Попытка добавить повторяющуюся запись.</summary>
    public class DuplicateEntryException : DailyPlannerException
    {
        public DuplicateEntryException(string message) : base(message) { }
    }
}
