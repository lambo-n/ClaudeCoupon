namespace EcommerceCouponLibrary.Domain.Exceptions
{
    /// <summary>
    /// Base exception for coupon validation errors
    /// </summary>
    public abstract class CouponValidationException : Exception
    {
        /// <summary>
        /// Gets the rejection reason for this validation failure
        /// </summary>
        public Enums.CouponRejectionReason RejectionReason { get; }

        protected CouponValidationException(string message, Enums.CouponRejectionReason rejectionReason)
            : base(message)
        {
            RejectionReason = rejectionReason;
        }

        protected CouponValidationException(string message, Enums.CouponRejectionReason rejectionReason, Exception innerException)
            : base(message, innerException)
        {
            RejectionReason = rejectionReason;
        }
    }
}
