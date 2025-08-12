using System;
using System.Collections.Generic;

namespace EcommerceCouponLibrary.Core.Models
{
    /// <summary>
    /// Result of attempting to apply a coupon to an order
    /// </summary>
    public class CouponApplicationResult
    {
        /// <summary>
        /// Whether the coupon was successfully applied
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The coupon that was attempted to be applied
        /// </summary>
        public Coupon? Coupon { get; set; }

        /// <summary>
        /// The discount amount that was applied (if successful)
        /// </summary>
        public Money DiscountAmount { get; set; }

        /// <summary>
        /// The reason why the coupon could not be applied (if unsuccessful)
        /// </summary>
        public CouponRejectionReason? RejectionReason { get; set; }

        /// <summary>
        /// Human-readable message explaining the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Order totals before and after applying the coupon
        /// </summary>
        public OrderTotals? OrderTotals { get; set; }

        /// <summary>
        /// Line-level discount allocations
        /// </summary>
        public List<LineDiscount> LineDiscounts { get; set; } = new();

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="coupon">The coupon that was applied</param>
        /// <param name="discountAmount">The discount amount</param>
        /// <param name="orderTotals">The order totals</param>
        /// <param name="lineDiscounts">Line-level discounts</param>
        /// <param name="customMessage">Optional custom message (if null, default message is used)</param>
        /// <returns>A successful result</returns>
        public static CouponApplicationResult Success(Coupon coupon, Money discountAmount, OrderTotals orderTotals, List<LineDiscount> lineDiscounts, string? customMessage = null)
        {
            return new CouponApplicationResult
            {
                IsSuccess = true,
                Coupon = coupon,
                DiscountAmount = discountAmount,
                OrderTotals = orderTotals,
                LineDiscounts = lineDiscounts,
                Message = customMessage ?? $"Coupon '{coupon.Name}' applied successfully! You saved {discountAmount}."
            };
        }

        /// <summary>
        /// Creates a failure result
        /// </summary>
        /// <param name="coupon">The coupon that failed to apply</param>
        /// <param name="rejectionReason">The reason for rejection</param>
        /// <param name="message">Human-readable message</param>
        /// <returns>A failure result</returns>
        public static CouponApplicationResult Failure(Coupon? coupon, CouponRejectionReason rejectionReason, string message)
        {
            return new CouponApplicationResult
            {
                IsSuccess = false,
                Coupon = coupon,
                RejectionReason = rejectionReason,
                Message = message
            };
        }
    }

    /// <summary>
    /// Order totals before and after applying a coupon
    /// </summary>
    public class OrderTotals
    {
        /// <summary>
        /// Subtotal before any discounts
        /// </summary>
        public Money Subtotal { get; set; }

        /// <summary>
        /// Total discount amount
        /// </summary>
        public Money TotalDiscount { get; set; }

        /// <summary>
        /// Final total after discounts
        /// </summary>
        public Money FinalTotal { get; set; }

        /// <summary>
        /// Currency code for all amounts
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Discount allocation for a specific line item
    /// </summary>
    public class LineDiscount
    {
        /// <summary>
        /// The line item that received the discount
        /// </summary>
        public OrderItem Item { get; set; } = null!;

        /// <summary>
        /// The discount amount applied to this line
        /// </summary>
        public Money DiscountAmount { get; set; }

        /// <summary>
        /// The original price of this line
        /// </summary>
        public Money OriginalPrice { get; set; }

        /// <summary>
        /// The final price of this line after discount
        /// </summary>
        public Money FinalPrice { get; set; }
    }

    /// <summary>
    /// Reasons why a coupon might be rejected
    /// </summary>
    public enum CouponRejectionReason
    {
        /// <summary>
        /// Coupon not found
        /// </summary>
        NotFound,

        /// <summary>
        /// Coupon has expired
        /// </summary>
        Expired,

        /// <summary>
        /// Coupon is not yet active
        /// </summary>
        NotYetActive,

        /// <summary>
        /// Coupon is inactive
        /// </summary>
        Inactive,

        /// <summary>
        /// Order subtotal is below minimum requirement
        /// </summary>
        BelowMinimum,

        /// <summary>
        /// Global usage limit has been reached
        /// </summary>
        GlobalLimitReached,

        /// <summary>
        /// Customer has reached their usage limit for this coupon
        /// </summary>
        CustomerLimitReached,

        /// <summary>
        /// Coupon has already been used
        /// </summary>
        AlreadyUsed,

        /// <summary>
        /// Customer is not eligible for this coupon
        /// </summary>
        CustomerNotEligible,

        /// <summary>
        /// No eligible items in the order
        /// </summary>
        NoEligibleItems,

        /// <summary>
        /// Coupon cannot be combined with existing coupons
        /// </summary>
        CannotCombine,

        /// <summary>
        /// Invalid coupon code format
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// Currency mismatch
        /// </summary>
        CurrencyMismatch,

        /// <summary>
        /// Coupon replacement failed
        /// </summary>
        ReplacementFailed,

        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown
    }
}
