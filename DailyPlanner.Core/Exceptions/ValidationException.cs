namespace DailyPlanner.Core.Exceptions
{
    /// <summary>Недопустимые параметры операции.</summary>
    public class ValidationException : DailyPlannerException
    {
        public string ParamName { get; }

        public ValidationException(string message, string paramName)
            : base(message)
        {
            ParamName = paramName;
        }
    }
}
