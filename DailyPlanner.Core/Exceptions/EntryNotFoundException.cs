namespace DailyPlanner.Core.Exceptions
{
    /// <summary>Запись не найдена.</summary>
    public class EntryNotFoundException : DailyPlannerException
    {
        public EntryNotFoundException(string message) : base(message) { }
    }
}
