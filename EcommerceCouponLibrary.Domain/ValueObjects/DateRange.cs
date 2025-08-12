namespace EcommerceCouponLibrary.Domain.ValueObjects
{
    /// <summary>
    /// Represents a date range with validation
    /// </summary>
    public class DateRange : ValueObject
    {
        /// <summary>
        /// Gets the start date of the range
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        /// Gets the end date of the range
        /// </summary>
        public DateTime EndDate { get; }

        /// <summary>
        /// Initializes a new instance of the DateRange value object
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        public DateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date", nameof(startDate));

            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>
        /// Creates a DateRange from start and end dates
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>A new DateRange instance</returns>
        public static DateRange Create(DateTime startDate, DateTime endDate) => new(startDate, endDate);

        /// <summary>
        /// Creates a DateRange that starts now and ends after the specified duration
        /// </summary>
        /// <param name="duration">The duration from now</param>
        /// <returns>A new DateRange instance</returns>
        public static DateRange FromNow(TimeSpan duration)
        {
            var startDate = DateTime.UtcNow;
            var endDate = startDate.Add(duration);
            return new DateRange(startDate, endDate);
        }

        /// <summary>
        /// Creates a DateRange that starts after the specified duration and ends after another duration
        /// </summary>
        /// <param name="startAfter">Duration from now to start</param>
        /// <param name="duration">Duration from start to end</param>
        /// <returns>A new DateRange instance</returns>
        public static DateRange FromNow(TimeSpan startAfter, TimeSpan duration)
        {
            var startDate = DateTime.UtcNow.Add(startAfter);
            var endDate = startDate.Add(duration);
            return new DateRange(startDate, endDate);
        }

        /// <summary>
        /// Checks if the specified date is within this range
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if the date is within the range</returns>
        public bool Contains(DateTime date)
        {
            return date >= StartDate && date <= EndDate;
        }

        /// <summary>
        /// Checks if the current date is within this range
        /// </summary>
        /// <returns>True if the current date is within the range</returns>
        public bool IsActive()
        {
            return Contains(DateTime.UtcNow);
        }

        /// <summary>
        /// Checks if the range has started
        /// </summary>
        /// <returns>True if the range has started</returns>
        public bool HasStarted()
        {
            return DateTime.UtcNow >= StartDate;
        }

        /// <summary>
        /// Checks if the range has ended
        /// </summary>
        /// <returns>True if the range has ended</returns>
        public bool HasEnded()
        {
            return DateTime.UtcNow > EndDate;
        }

        /// <summary>
        /// Gets the duration of the range
        /// </summary>
        /// <returns>The duration</returns>
        public TimeSpan Duration => EndDate - StartDate;

        /// <summary>
        /// Gets the remaining time until the range starts
        /// </summary>
        /// <returns>The remaining time, or TimeSpan.Zero if already started</returns>
        public TimeSpan TimeUntilStart()
        {
            var now = DateTime.UtcNow;
            return now < StartDate ? StartDate - now : TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the remaining time until the range ends
        /// </summary>
        /// <returns>The remaining time, or TimeSpan.Zero if already ended</returns>
        public TimeSpan TimeUntilEnd()
        {
            var now = DateTime.UtcNow;
            return now < EndDate ? EndDate - now : TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the equality components for value object comparison
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartDate;
            yield return EndDate;
        }

        /// <summary>
        /// Formats the date range as a string
        /// </summary>
        public override string ToString()
        {
            return $"{StartDate:g} - {EndDate:g}";
        }
    }
}
