using System;
using System.Text.Json.Serialization;

namespace EcommerceCouponLibrary.Core.Models
{
    /// <summary>
    /// Represents a coupon that can be applied to an order
    /// </summary>
    public class Coupon
    {
        /// <summary>
        /// Unique identifier for the coupon
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The coupon code that customers enter
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name for the coupon
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the coupon offer
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of discount this coupon provides
        /// </summary>
        public CouponType Type { get; set; }

        /// <summary>
        /// The discount value (percentage as decimal 0.0-1.0, or fixed amount)
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Currency code for fixed amount coupons
        /// </summary>
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>
        /// When the coupon becomes active
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// When the coupon expires
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Whether the coupon is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Maximum number of times this coupon can be used globally
        /// </summary>
        public int? GlobalUsageLimit { get; set; }

        /// <summary>
        /// Maximum number of times a single customer can use this coupon
        /// </summary>
        public int? PerCustomerUsageLimit { get; set; }

        /// <summary>
        /// Minimum order subtotal required to use this coupon
        /// </summary>
        public Money? MinimumOrderAmount { get; set; }

        /// <summary>
        /// Maximum discount amount that can be applied (caps the discount)
        /// </summary>
        public Money? MaximumDiscountAmount { get; set; }

        /// <summary>
        /// Whether this coupon can be combined with other coupons
        /// </summary>
        public bool IsCombinable { get; set; } = false;

        /// <summary>
        /// Priority for this coupon when multiple coupons could apply
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Whether the coupon applies to shipping costs
        /// </summary>
        public bool AppliesToShipping { get; set; } = false;

        /// <summary>
        /// Whether the coupon applies to taxes
        /// </summary>
        public bool AppliesToTaxes { get; set; } = false;

        /// <summary>
        /// Date when the coupon was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the coupon was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the discount value as a Money object for fixed amount coupons
        /// </summary>
        [JsonIgnore]
        public Money? FixedAmount => Type == CouponType.FixedAmount ? Money.Create(Value, CurrencyCode) : null;

        /// <summary>
        /// Gets the discount percentage as a decimal (0.0-1.0) for percentage coupons
        /// </summary>
        [JsonIgnore]
        public decimal? Percentage => Type == CouponType.Percentage ? Value : null;

        /// <summary>
        /// Checks if the coupon is currently valid based on dates and active status
        /// </summary>
        /// <param name="currentTime">The current time to check against</param>
        /// <returns>True if the coupon is currently valid</returns>
        public bool IsValidAt(DateTime currentTime)
        {
            return IsActive && 
                   currentTime >= StartDate && 
                   currentTime <= EndDate;
        }

        /// <summary>
        /// Creates a new percentage-based coupon
        /// </summary>
        /// <param name="code">The coupon code</param>
        /// <param name="name">The coupon name</param>
        /// <param name="percentage">The discount percentage (0.0-1.0)</param>
        /// <param name="startDate">When the coupon becomes active</param>
        /// <param name="endDate">When the coupon expires</param>
        /// <returns>A new Coupon instance</returns>
        public static Coupon CreatePercentageCoupon(string code, string name, decimal percentage, DateTime startDate, DateTime endDate)
        {
            if (percentage < 0 || percentage > 1)
                throw new ArgumentException("Percentage must be between 0 and 1", nameof(percentage));

            return new Coupon
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Type = CouponType.Percentage,
                Value = percentage,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        /// <summary>
        /// Creates a new fixed amount coupon
        /// </summary>
        /// <param name="code">The coupon code</param>
        /// <param name="name">The coupon name</param>
        /// <param name="amount">The fixed discount amount</param>
        /// <param name="currencyCode">The currency code</param>
        /// <param name="startDate">When the coupon becomes active</param>
        /// <param name="endDate">When the coupon expires</param>
        /// <returns>A new Coupon instance</returns>
        public static Coupon CreateFixedAmountCoupon(string code, string name, decimal amount, string currencyCode, DateTime startDate, DateTime endDate)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            return new Coupon
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Type = CouponType.FixedAmount,
                Value = amount,
                CurrencyCode = currencyCode,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }

    /// <summary>
    /// Types of coupons supported by the system
    /// </summary>
    public enum CouponType
    {
        /// <summary>
        /// Percentage-based discount (e.g., 10% off)
        /// </summary>
        Percentage,

        /// <summary>
        /// Fixed amount discount (e.g., $5 off)
        /// </summary>
        FixedAmount
    }
}
