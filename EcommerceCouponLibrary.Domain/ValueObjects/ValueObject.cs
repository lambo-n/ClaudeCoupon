namespace EcommerceCouponLibrary.Domain.ValueObjects
{
    /// <summary>
    /// Base class for all value objects
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Gets the components that make up the value object for equality comparison
        /// </summary>
        /// <returns>Array of objects to compare for equality</returns>
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            return EqualOperator(left, right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return NotEqualOperator(left, right);
        }

        protected static bool EqualOperator(ValueObject? left, ValueObject? right)
        {
            if (left is null ^ right is null)
                return false;

            return left?.Equals(right) != false;
        }

        protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
        {
            return !EqualOperator(left, right);
        }
    }
}
