namespace EcommerceCouponLibrary.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a coupon code is invalid
    /// </summary>
    public class InvalidCouponCodeException : CouponValidationException
    {
        /// <summary>
        /// Gets the invalid coupon code that caused the exception
        /// </summary>
        public string CouponCode { get; }

        public InvalidCouponCodeException(string couponCode, string message)
            : base(message, Enums.CouponRejectionReason.InvalidFormat)
        {
            CouponCode = couponCode;
        }

        public InvalidCouponCodeException(string couponCode, string message, Exception innerException)
            : base(message, Enums.CouponRejectionReason.InvalidFormat, innerException)
        {
            CouponCode = couponCode;
        }
    }
}
