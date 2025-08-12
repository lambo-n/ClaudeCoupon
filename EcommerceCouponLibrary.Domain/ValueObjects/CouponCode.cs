using System.Text.RegularExpressions;

namespace EcommerceCouponLibrary.Domain.ValueObjects
{
    /// <summary>
    /// Represents a coupon code with validation and normalization
    /// </summary>
    public class CouponCode : ValueObject
    {
        private readonly string _value;

        /// <summary>
        /// Gets the normalized coupon code value
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// Initializes a new instance of the CouponCode value object
        /// </summary>
        /// <param name="value">The coupon code value</param>
        public CouponCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Coupon code cannot be null or empty", nameof(value));

            // trim first for validations below
            var trimmed = value.Trim();

            if (trimmed.Length < 3)
                throw new ArgumentException("Coupon code must be at least 3 characters long", nameof(value));

            if (trimmed.Length > 50)
                throw new ArgumentException("Coupon code cannot exceed 50 characters", nameof(value));

            // Reject whitespace inside the code
            if (trimmed.Any(char.IsWhiteSpace))
                throw new ArgumentException("Coupon code cannot contain whitespace characters", nameof(value));

            // Allow only alphanumeric, dash and underscore characters
            if (!Regex.IsMatch(trimmed, "^[A-Za-z0-9_-]+$"))
                throw new ArgumentException("Coupon code contains invalid characters. Allowed: letters, digits, '-' and '_'", nameof(value));

            // Normalize the coupon code (uppercase)
            _value = trimmed.ToUpperInvariant();
        }

        /// <summary>
        /// Creates a CouponCode from a string value
        /// </summary>
        /// <param name="value">The coupon code value</param>
        /// <returns>A new CouponCode instance</returns>
        public static CouponCode Create(string value) => new(value);

        /// <summary>
        /// Implicit conversion from string to CouponCode
        /// </summary>
        public static implicit operator string(CouponCode couponCode) => couponCode.Value;

        /// <summary>
        /// Explicit conversion from string to CouponCode
        /// </summary>
        public static explicit operator CouponCode(string value) => new(value);



        /// <summary>
        /// Formats the coupon code as a string
        /// </summary>
        public override string ToString() => _value;

        /// <summary>
        /// Gets the equality components for value object comparison
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _value;
        }
    }
}
