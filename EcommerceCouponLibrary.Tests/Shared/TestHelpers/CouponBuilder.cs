using EcommerceCouponLibrary.Domain.ValueObjects;
using EcommerceCouponLibrary.Domain.Enums;

namespace EcommerceCouponLibrary.Tests.Shared.TestHelpers
{
    /// <summary>
    /// Builder pattern for creating test coupons with fluent API
    /// </summary>
    public class CouponBuilder
    {
        private string _code = "SAVE20";
        private string _name = "20% Off";
        private CouponType _type = CouponType.Percentage;
        private decimal _value = 20.0m;
        private DateRange _validity = DateRange.FromNow(TimeSpan.FromDays(30));
        private Money _minimumOrderAmount = Money.USD(0.00m);
        private Money _maximumDiscount = Money.USD(100.00m);
        private int _globalUsageLimit = 1000;
        private int _customerUsageLimit = 1;
        private bool _isActive = true;
        private string _currencyCode = "USD";

        /// <summary>
        /// Sets the coupon code
        /// </summary>
        public CouponBuilder WithCode(string code)
        {
            _code = code;
            return this;
        }

        /// <summary>
        /// Sets the coupon name
        /// </summary>
        public CouponBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the coupon type
        /// </summary>
        public CouponBuilder WithType(CouponType type)
        {
            _type = type;
            return this;
        }

        /// <summary>
        /// Sets the coupon value
        /// </summary>
        public CouponBuilder WithValue(decimal value)
        {
            _value = value;
            return this;
        }

        /// <summary>
        /// Sets the validity period
        /// </summary>
        public CouponBuilder WithValidity(DateRange validity)
        {
            _validity = validity;
            return this;
        }

        /// <summary>
        /// Sets the minimum order amount
        /// </summary>
        public CouponBuilder WithMinimumOrderAmount(Money minimumOrderAmount)
        {
            _minimumOrderAmount = minimumOrderAmount;
            return this;
        }

        /// <summary>
        /// Sets the maximum discount amount
        /// </summary>
        public CouponBuilder WithMaximumDiscount(Money maximumDiscount)
        {
            _maximumDiscount = maximumDiscount;
            return this;
        }

        /// <summary>
        /// Sets the global usage limit
        /// </summary>
        public CouponBuilder WithGlobalUsageLimit(int globalUsageLimit)
        {
            _globalUsageLimit = globalUsageLimit;
            return this;
        }

        /// <summary>
        /// Sets the customer usage limit
        /// </summary>
        public CouponBuilder WithCustomerUsageLimit(int customerUsageLimit)
        {
            _customerUsageLimit = customerUsageLimit;
            return this;
        }

        /// <summary>
        /// Sets the active status
        /// </summary>
        public CouponBuilder WithActiveStatus(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        /// <summary>
        /// Sets the currency code
        /// </summary>
        public CouponBuilder WithCurrencyCode(string currencyCode)
        {
            _currencyCode = currencyCode;
            return this;
        }

        /// <summary>
        /// Creates an expired coupon
        /// </summary>
        public CouponBuilder AsExpired()
        {
            _validity = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1));
            return this;
        }

        /// <summary>
        /// Creates a future coupon
        /// </summary>
        public CouponBuilder AsFuture()
        {
            _validity = DateRange.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(31));
            return this;
        }

        /// <summary>
        /// Creates a percentage coupon
        /// </summary>
        public CouponBuilder AsPercentage(decimal percentage)
        {
            _type = CouponType.Percentage;
            _value = percentage;
            return this;
        }

        /// <summary>
        /// Creates a fixed amount coupon
        /// </summary>
        public CouponBuilder AsFixedAmount(decimal amount)
        {
            _type = CouponType.FixedAmount;
            _value = amount;
            return this;
        }

        /// <summary>
        /// Creates a free shipping coupon
        /// </summary>
        public CouponBuilder AsFreeShipping()
        {
            _type = CouponType.FreeShipping;
            _value = 0.0m;
            return this;
        }

        /// <summary>
        /// Creates a single-use coupon
        /// </summary>
        public CouponBuilder AsSingleUse()
        {
            _globalUsageLimit = 1;
            _customerUsageLimit = 1;
            return this;
        }

        /// <summary>
        /// Creates a high-value coupon
        /// </summary>
        public CouponBuilder AsHighValue()
        {
            _value = 50.0m;
            _maximumDiscount = Money.USD(500.00m);
            return this;
        }

        /// <summary>
        /// Creates a low-value coupon
        /// </summary>
        public CouponBuilder AsLowValue()
        {
            _value = 5.0m;
            _maximumDiscount = Money.USD(10.00m);
            return this;
        }

        /// <summary>
        /// Builds the coupon with the current configuration
        /// </summary>
        public (CouponCode Code, string Name, CouponType Type, decimal Value, DateRange Validity) Build()
        {
            return (
                CouponCode.Create(_code),
                _name,
                _type,
                _value,
                _validity
            );
        }

        /// <summary>
        /// Creates a default valid coupon
        /// </summary>
        public static CouponBuilder CreateValidCoupon()
        {
            return new CouponBuilder();
        }

        /// <summary>
        /// Creates a default percentage coupon
        /// </summary>
        public static CouponBuilder CreatePercentageCoupon()
        {
            return new CouponBuilder().AsPercentage(20.0m);
        }

        /// <summary>
        /// Creates a default fixed amount coupon
        /// </summary>
        public static CouponBuilder CreateFixedAmountCoupon()
        {
            return new CouponBuilder().AsFixedAmount(10.0m);
        }

        /// <summary>
        /// Creates a default free shipping coupon
        /// </summary>
        public static CouponBuilder CreateFreeShippingCoupon()
        {
            return new CouponBuilder().AsFreeShipping();
        }

        /// <summary>
        /// Creates a default expired coupon
        /// </summary>
        public static CouponBuilder CreateExpiredCoupon()
        {
            return new CouponBuilder().AsExpired();
        }

        /// <summary>
        /// Creates a default future coupon
        /// </summary>
        public static CouponBuilder CreateFutureCoupon()
        {
            return new CouponBuilder().AsFuture();
        }
    }
}
