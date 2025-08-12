namespace EcommerceCouponLibrary.Domain.Enums
{
    /// <summary>
    /// Reasons why a coupon might be rejected
    /// </summary>
    public enum CouponRejectionReason
    {
        /// <summary>
        /// Coupon not found
        /// </summary>
        NotFound = 1,

        /// <summary>
        /// Coupon has expired
        /// </summary>
        Expired = 2,

        /// <summary>
        /// Coupon is not yet active
        /// </summary>
        NotYetActive = 3,

        /// <summary>
        /// Coupon is inactive
        /// </summary>
        Inactive = 4,

        /// <summary>
        /// Order subtotal is below minimum requirement
        /// </summary>
        BelowMinimum = 5,

        /// <summary>
        /// Global usage limit has been reached
        /// </summary>
        GlobalLimitReached = 6,

        /// <summary>
        /// Customer has reached their usage limit for this coupon
        /// </summary>
        CustomerLimitReached = 7,

        /// <summary>
        /// Coupon has already been used
        /// </summary>
        AlreadyUsed = 8,

        /// <summary>
        /// Customer is not eligible for this coupon
        /// </summary>
        CustomerNotEligible = 9,

        /// <summary>
        /// No eligible items in the order
        /// </summary>
        NoEligibleItems = 10,

        /// <summary>
        /// Coupon cannot be combined with existing coupons
        /// </summary>
        CannotCombine = 11,

        /// <summary>
        /// Invalid coupon code format
        /// </summary>
        InvalidFormat = 12,

        /// <summary>
        /// Currency mismatch
        /// </summary>
        CurrencyMismatch = 13,

        /// <summary>
        /// Coupon replacement failed
        /// </summary>
        ReplacementFailed = 14,

        /// <summary>
        /// Geographic restriction
        /// </summary>
        GeographicRestriction = 15,

        /// <summary>
        /// Product/category restriction
        /// </summary>
        ProductRestriction = 16,

        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown = 99
    }
}
