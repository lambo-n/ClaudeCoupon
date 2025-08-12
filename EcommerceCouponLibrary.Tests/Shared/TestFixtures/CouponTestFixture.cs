using EcommerceCouponLibrary.Domain.ValueObjects;
using EcommerceCouponLibrary.Domain.Enums;

namespace EcommerceCouponLibrary.Tests.Shared.TestFixtures
{
    /// <summary>
    /// Test fixture providing reusable test data for coupon-related tests
    /// </summary>
    public class CouponTestFixture
    {
        /// <summary>
        /// Creates a valid percentage coupon for testing
        /// </summary>
        public static (CouponCode Code, string Name, CouponType Type, decimal Value, DateRange Validity) CreateValidPercentageCoupon()
        {
            return (
                CouponCode.Create("SAVE20"),
                "20% Off",
                CouponType.Percentage,
                20.0m,
                DateRange.FromNow(TimeSpan.FromDays(30))
            );
        }

        /// <summary>
        /// Creates a valid fixed amount coupon for testing
        /// </summary>
        public static (CouponCode Code, string Name, CouponType Type, decimal Value, DateRange Validity) CreateValidFixedAmountCoupon()
        {
            return (
                CouponCode.Create("SAVE10"),
                "$10 Off",
                CouponType.FixedAmount,
                10.0m,
                DateRange.FromNow(TimeSpan.FromDays(30))
            );
        }

        /// <summary>
        /// Creates an expired coupon for testing
        /// </summary>
        public static (CouponCode Code, string Name, CouponType Type, decimal Value, DateRange Validity) CreateExpiredCoupon()
        {
            return (
                CouponCode.Create("EXPIRED"),
                "Expired Coupon",
                CouponType.Percentage,
                15.0m,
                DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1))
            );
        }

        /// <summary>
        /// Creates a future coupon for testing
        /// </summary>
        public static (CouponCode Code, string Name, CouponType Type, decimal Value, DateRange Validity) CreateFutureCoupon()
        {
            return (
                CouponCode.Create("FUTURE"),
                "Future Coupon",
                CouponType.Percentage,
                25.0m,
                DateRange.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(31))
            );
        }

        /// <summary>
        /// Creates a free shipping coupon for testing
        /// </summary>
        public static (CouponCode Code, string Name, CouponType Type, decimal Value, DateRange Validity) CreateFreeShippingCoupon()
        {
            return (
                CouponCode.Create("FREESHIP"),
                "Free Shipping",
                CouponType.FreeShipping,
                0.0m,
                DateRange.FromNow(TimeSpan.FromDays(30))
            );
        }

        /// <summary>
        /// Creates a list of valid coupon codes for testing
        /// </summary>
        public static IEnumerable<string> GetValidCouponCodes()
        {
            return new[]
            {
                "SAVE20",
                "SAVE10",
                "FREESHIP",
                "WELCOME",
                "HOLIDAY",
                "SUMMER",
                "WINTER",
                "SPRING",
                "FALL",
                "SPECIAL"
            };
        }

        /// <summary>
        /// Creates a list of invalid coupon codes for testing
        /// </summary>
        public static IEnumerable<string> GetInvalidCouponCodes()
        {
            return new[]
            {
                "",           // Empty
                "AB",         // Too short
                null!,        // Null
                "   ",        // Whitespace only
                "A".PadRight(51, 'A'), // Too long
                "SAVE@20",    // Invalid characters
                "SAVE 20"     // Contains space
            };
        }

        /// <summary>
        /// Creates test money amounts for different currencies
        /// </summary>
        public static IEnumerable<(decimal Amount, string Currency)> GetTestMoneyAmounts()
        {
            return new[]
            {
                (100.00m, "USD"),
                (85.50m, "EUR"),
                (15000m, "JPY"),
                (75.25m, "GBP"),
                (125.75m, "CAD"),
                (95.00m, "AUD")
            };
        }

        /// <summary>
        /// Creates test date ranges for different scenarios
        /// </summary>
        public static IEnumerable<DateRange> GetTestDateRanges()
        {
            return new[]
            {
                DateRange.FromNow(TimeSpan.FromDays(30)),                    // Active
                DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1)), // Expired
                DateRange.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(31)),   // Future
                DateRange.FromNow(TimeSpan.FromHours(1)),                    // Short duration
                DateRange.FromNow(TimeSpan.FromDays(365))                    // Long duration
            };
        }
    }
}
