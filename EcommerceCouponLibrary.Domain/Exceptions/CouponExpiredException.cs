namespace EcommerceCouponLibrary.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a coupon has expired
    /// </summary>
    public class CouponExpiredException : CouponValidationException
    {
        /// <summary>
        /// Gets the expiration date of the coupon
        /// </summary>
        public DateTime ExpirationDate { get; }

        public CouponExpiredException(DateTime expirationDate, string message)
            : base(message, Enums.CouponRejectionReason.Expired)
        {
            ExpirationDate = expirationDate;
        }

        public CouponExpiredException(DateTime expirationDate, string message, Exception innerException)
            : base(message, Enums.CouponRejectionReason.Expired, innerException)
        {
            ExpirationDate = expirationDate;
        }
    }
}
