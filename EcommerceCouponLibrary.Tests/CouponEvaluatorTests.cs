using System;
using System.Linq;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;
using EcommerceCouponLibrary.Core.Repositories;
using EcommerceCouponLibrary.Core.Services;
using Xunit;

namespace EcommerceCouponLibrary.Tests
{
    public class CouponEvaluatorTests
    {
        private readonly ICouponRepository _repository;
        private readonly ICouponEvaluator _evaluator;
        private readonly Order _testOrder;

        public CouponEvaluatorTests()
        {
            _repository = new InMemoryCouponRepository();
            _evaluator = new CouponEvaluator(_repository);
            
            // Create a test order
            _testOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = "test-customer-123",
                CurrencyCode = "USD"
            };

            // Add some test items
            _testOrder.AddItem(new OrderItem
            {
                ProductId = "prod-1",
                ProductName = "Test Product 1",
                Quantity = 2,
                UnitPrice = Money.USD(25.00m)
            });

            _testOrder.AddItem(new OrderItem
            {
                ProductId = "prod-2",
                ProductName = "Test Product 2",
                Quantity = 1,
                UnitPrice = Money.USD(50.00m)
            });
        }

        [Fact]
        public async Task ApplyCoupon_ValidPercentageCoupon_AppliesSuccessfully()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE10", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(10.00m), result.DiscountAmount); // 10% of $100
            Assert.Equal("Coupon '10% Off' applied successfully! You saved 10.00 USD.", result.Message);
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(100.00m), result.OrderTotals.Subtotal);
            Assert.Equal(Money.USD(10.00m), result.OrderTotals.TotalDiscount);
            Assert.Equal(Money.USD(90.00m), result.OrderTotals.FinalTotal);
            Assert.Equal(2, result.LineDiscounts.Count); // 2 items in the test order
        }

        [Fact]
        public async Task ApplyCoupon_ValidFixedAmountCoupon_AppliesSuccessfully()
        {
            // Arrange
            var coupon = Coupon.CreateFixedAmountCoupon(
                "SAVE5",
                "$5 Off",
                5.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE5", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(5.00m), result.DiscountAmount);
            Assert.Equal("Coupon '$5 Off' applied successfully! You saved 5.00 USD.", result.Message);
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(100.00m), result.OrderTotals.Subtotal);
            Assert.Equal(Money.USD(5.00m), result.OrderTotals.TotalDiscount);
            Assert.Equal(Money.USD(95.00m), result.OrderTotals.FinalTotal);
        }

        [Fact]
        public async Task ApplyCoupon_InvalidCode_ReturnsNotFoundError()
        {
            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "INVALID", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Coupon);
            Assert.Equal(CouponRejectionReason.NotFound, result.RejectionReason);
            Assert.Equal("Invalid coupon code. Please check the code and try again.", result.Message);
        }

        [Fact]
        public async Task ApplyCoupon_ExpiredCoupon_ReturnsExpiredError()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "EXPIRED",
                "Expired Coupon",
                0.10m,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(-1)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "EXPIRED", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(CouponRejectionReason.Expired, result.RejectionReason);
            Assert.Equal("This coupon has expired.", result.Message);
        }

        [Fact]
        public async Task ApplyCoupon_NotYetActiveCoupon_ReturnsNotYetActiveError()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "FUTURE",
                "Future Coupon",
                0.10m,
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "FUTURE", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(CouponRejectionReason.NotYetActive, result.RejectionReason);
            Assert.Contains("This coupon will be active starting", result.Message);
        }

        [Fact]
        public async Task ApplyCoupon_InactiveCoupon_ReturnsInactiveError()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "INACTIVE",
                "Inactive Coupon",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            coupon.IsActive = false;
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "INACTIVE", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(CouponRejectionReason.Inactive, result.RejectionReason);
            Assert.Equal("This coupon is currently inactive.", result.Message);
        }

        [Fact]
        public async Task ApplyCoupon_BelowMinimumOrder_ReturnsBelowMinimumError()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "MIN100",
                "Minimum $100",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            coupon.MinimumOrderAmount = Money.USD(150.00m);
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "MIN100", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(CouponRejectionReason.BelowMinimum, result.RejectionReason);
            Assert.Contains("Order must be at least 150.00 USD to use this coupon", result.Message);
            Assert.Contains("Add 50.00 USD more to your cart", result.Message);
        }

        [Fact]
        public async Task ApplyCoupon_CurrencyMismatch_ReturnsCurrencyMismatchError()
        {
            // Arrange
            var coupon = Coupon.CreateFixedAmountCoupon(
                "EURO5",
                "€5 Off",
                5.00m,
                "EUR",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "EURO5", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(CouponRejectionReason.CurrencyMismatch, result.RejectionReason);
            Assert.Equal("This coupon is only valid for EUR orders.", result.Message);
        }

        [Fact]
        public async Task ApplyCoupon_CaseInsensitiveCode_AppliesSuccessfully()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "save10", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(10.00m), result.DiscountAmount);
        }

        [Fact]
        public async Task ApplyCoupon_WhitespaceInCode_AppliesSuccessfully()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "  SAVE10  ", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(10.00m), result.DiscountAmount);
        }

        [Fact]
        public async Task ApplyCoupon_FixedAmountExceedsSubtotal_CapsAtSubtotal()
        {
            // Arrange
            var coupon = Coupon.CreateFixedAmountCoupon(
                "SAVE200",
                "$200 Off",
                200.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE200", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(100.00m), result.DiscountAmount); // Capped at subtotal
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(0.00m), result.OrderTotals.FinalTotal); // Total becomes zero
        }

        [Fact]
        public async Task ApplyCoupon_MaximumDiscountCap_RespectsCap()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE50",
                "50% Off",
                0.50m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            coupon.MaximumDiscountAmount = Money.USD(25.00m); // Cap at $25
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE50", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(25.00m), result.DiscountAmount); // Capped at $25 instead of $50
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(75.00m), result.OrderTotals.FinalTotal);
        }

        [Fact]
        public async Task ValidateCoupon_ValidCoupon_ReturnsSuccess()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ValidateCouponAsync("SAVE10", _testOrder, "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(0.00m), result.DiscountAmount); // No discount applied during validation
        }





        [Fact]
        public void GetOrderTotals_NoCoupons_ReturnsCorrectTotals()
        {
            // Act
            var totals = _evaluator.GetOrderTotals(_testOrder);

            // Assert
            Assert.Equal(Money.USD(100.00m), totals.Subtotal);
            Assert.Equal(Money.USD(0.00m), totals.TotalDiscount);
            Assert.Equal(Money.USD(100.00m), totals.FinalTotal);
            Assert.Equal("USD", totals.CurrencyCode);
        }

        [Fact]
        public void GetOrderTotals_WithAppliedCoupon_ReturnsCorrectTotals()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            _testOrder.ApplyCoupon(coupon, Money.USD(10.00m));

            // Act
            var totals = _evaluator.GetOrderTotals(_testOrder);

            // Assert
            Assert.Equal(Money.USD(100.00m), totals.Subtotal);
            Assert.Equal(Money.USD(10.00m), totals.TotalDiscount);
            Assert.Equal(Money.USD(90.00m), totals.FinalTotal);
        }

        // U-002 Tests: Change or remove a coupon
        [Fact]
        public void RemoveCoupon_ExistingCoupon_RemovesSuccessfully()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            _testOrder.ApplyCoupon(coupon, Money.USD(10.00m));

            // Act
            var result = _evaluator.RemoveCoupon(_testOrder, coupon.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(coupon, result.Coupon);
            Assert.Equal(Money.USD(0.00m), result.DiscountAmount);
            Assert.Equal("Coupon '10% Off' has been removed. Your total has been updated.", result.Message);
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(100.00m), result.OrderTotals.Subtotal);
            Assert.Equal(Money.USD(0.00m), result.OrderTotals.TotalDiscount);
            Assert.Equal(Money.USD(100.00m), result.OrderTotals.FinalTotal);
            Assert.Empty(_testOrder.AppliedCoupons);
        }

        [Fact]
        public void RemoveCoupon_NonExistentCoupon_ReturnsFailure()
        {
            // Act
            var result = _evaluator.RemoveCoupon(_testOrder, Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Coupon);
            Assert.Equal(CouponRejectionReason.NotFound, result.RejectionReason);
            Assert.Equal("The specified coupon was not found on this order.", result.Message);
        }

        [Fact]
        public async Task ReplaceCoupon_ValidReplacement_ReplacesSuccessfully()
        {
            // Arrange
            var originalCoupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            var newCoupon = Coupon.CreateFixedAmountCoupon(
                "SAVE15",
                "$15 Off",
                15.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            
            _testOrder.ApplyCoupon(originalCoupon, Money.USD(10.00m));
            ((InMemoryCouponRepository)_repository).AddCoupon(newCoupon);

            // Act
            var result = await _evaluator.ReplaceCouponAsync(_testOrder, originalCoupon.Id, "SAVE15", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newCoupon, result.Coupon);
            Assert.Equal(Money.USD(15.00m), result.DiscountAmount);
            Assert.Contains("Coupon '10% Off' was replaced with '$15 Off'", result.Message);
            Assert.Contains("New savings: 15.00 USD", result.Message);
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(100.00m), result.OrderTotals.Subtotal);
            Assert.Equal(Money.USD(15.00m), result.OrderTotals.TotalDiscount);
            Assert.Equal(Money.USD(85.00m), result.OrderTotals.FinalTotal);
            Assert.Single(_testOrder.AppliedCoupons);
            Assert.Equal(newCoupon.Id, _testOrder.AppliedCoupons[0].Coupon.Id);
        }

        [Fact]
        public async Task ReplaceCoupon_InvalidNewCoupon_RestoresOriginalCoupon()
        {
            // Arrange
            var originalCoupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            _testOrder.ApplyCoupon(originalCoupon, Money.USD(10.00m));

            // Act
            var result = await _evaluator.ReplaceCouponAsync(_testOrder, originalCoupon.Id, "INVALID", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CouponRejectionReason.NotFound, result.RejectionReason);
            Assert.Contains("Failed to apply new coupon", result.Message);
            Assert.Contains("Original coupon has been restored", result.Message);
            Assert.Single(_testOrder.AppliedCoupons);
            Assert.Equal(originalCoupon.Id, _testOrder.AppliedCoupons[0].Coupon.Id);
            Assert.Equal(Money.USD(10.00m), _testOrder.AppliedCoupons[0].DiscountAmount);
        }

        [Fact]
        public async Task ReplaceCoupon_NonExistentOriginalCoupon_ReturnsFailure()
        {
            // Arrange
            var newCoupon = Coupon.CreateFixedAmountCoupon(
                "SAVE15",
                "$15 Off",
                15.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(newCoupon);

            // Act
            var result = await _evaluator.ReplaceCouponAsync(_testOrder, Guid.NewGuid(), "SAVE15", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Coupon);
            Assert.Equal(CouponRejectionReason.NotFound, result.RejectionReason);
            Assert.Equal("The specified coupon was not found on this order.", result.Message);
            Assert.Empty(_testOrder.AppliedCoupons);
        }

        [Fact]
        public async Task ReplaceCoupon_ExpiredNewCoupon_RestoresOriginalCoupon()
        {
            // Arrange
            var originalCoupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            var expiredCoupon = Coupon.CreateFixedAmountCoupon(
                "EXPIRED",
                "Expired Coupon",
                15.00m,
                "USD",
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(-1)
            );
            
            _testOrder.ApplyCoupon(originalCoupon, Money.USD(10.00m));
            ((InMemoryCouponRepository)_repository).AddCoupon(expiredCoupon);

            // Act
            var result = await _evaluator.ReplaceCouponAsync(_testOrder, originalCoupon.Id, "EXPIRED", "test-customer-123");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expiredCoupon, result.Coupon);
            Assert.Equal(CouponRejectionReason.Expired, result.RejectionReason);
            Assert.Contains("Failed to apply new coupon", result.Message);
            Assert.Contains("Original coupon has been restored", result.Message);
            Assert.Single(_testOrder.AppliedCoupons);
            Assert.Equal(originalCoupon.Id, _testOrder.AppliedCoupons[0].Coupon.Id);
        }

        [Fact]
        public async Task ReplaceCoupon_BetterDiscount_AppliesNewCoupon()
        {
            // Arrange
            var originalCoupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            var betterCoupon = Coupon.CreatePercentageCoupon(
                "SAVE20",
                "20% Off",
                0.20m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            
            _testOrder.ApplyCoupon(originalCoupon, Money.USD(10.00m));
            ((InMemoryCouponRepository)_repository).AddCoupon(betterCoupon);

            // Act
            var result = await _evaluator.ReplaceCouponAsync(_testOrder, originalCoupon.Id, "SAVE20", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(betterCoupon, result.Coupon);
            Assert.Equal(Money.USD(20.00m), result.DiscountAmount);
            Assert.Contains("Coupon '10% Off' was replaced with '20% Off'", result.Message);
            Assert.Contains("New savings: 20.00 USD", result.Message);
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(80.00m), result.OrderTotals.FinalTotal);
        }

        [Fact]
        public async Task ReplaceCoupon_WorseDiscount_StillAppliesNewCoupon()
        {
            // Arrange
            var originalCoupon = Coupon.CreatePercentageCoupon(
                "SAVE20",
                "20% Off",
                0.20m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            var worseCoupon = Coupon.CreateFixedAmountCoupon(
                "SAVE5",
                "$5 Off",
                5.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            
            _testOrder.ApplyCoupon(originalCoupon, Money.USD(20.00m));
            ((InMemoryCouponRepository)_repository).AddCoupon(worseCoupon);

            // Act
            var result = await _evaluator.ReplaceCouponAsync(_testOrder, originalCoupon.Id, "SAVE5", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(worseCoupon, result.Coupon);
            Assert.Equal(Money.USD(5.00m), result.DiscountAmount);
            Assert.Contains("Coupon '20% Off' was replaced with '$5 Off'", result.Message);
            Assert.Contains("New savings: 5.00 USD", result.Message);
            Assert.NotNull(result.OrderTotals);
            Assert.Equal(Money.USD(95.00m), result.OrderTotals.FinalTotal);
        }

        // U-003 Tests: See what the coupon affected
        [Fact]
        public async Task ApplyCoupon_AllItemsEligible_ShowsDiscountForAllItems()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE10", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.LineDiscounts.Count); // Both items should have discounts
            
            // First item: $50 (2 * $25) should get $5 discount (10% of $50)
            var firstItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-1");
            Assert.Equal(Money.USD(5.00m), firstItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(50.00m), firstItemDiscount.OriginalPrice);
            Assert.Equal(Money.USD(45.00m), firstItemDiscount.FinalPrice);
            
            // Second item: $50 should get $5 discount (10% of $50)
            var secondItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-2");
            Assert.Equal(Money.USD(5.00m), secondItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(50.00m), secondItemDiscount.OriginalPrice);
            Assert.Equal(Money.USD(45.00m), secondItemDiscount.FinalPrice);
        }

        [Fact]
        public async Task ApplyCoupon_FixedAmountDiscount_AllocatesProportionally()
        {
            // Arrange
            var coupon = Coupon.CreateFixedAmountCoupon(
                "SAVE10",
                "$10 Off",
                10.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE10", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.LineDiscounts.Count);
            
            // Both items have equal value ($50 each), so they should get equal discount ($5 each)
            var firstItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-1");
            var secondItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-2");
            
            Assert.Equal(Money.USD(5.00m), firstItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(5.00m), secondItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(45.00m), firstItemDiscount.FinalPrice);
            Assert.Equal(Money.USD(45.00m), secondItemDiscount.FinalPrice);
        }

        [Fact]
        public async Task ApplyCoupon_UnequalItemValues_AllocatesProportionally()
        {
            // Arrange - Create order with unequal item values
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = "test-customer-123",
                CurrencyCode = "USD"
            };

            // Add items with different values
            order.AddItem(new OrderItem
            {
                ProductId = "prod-1",
                ProductName = "Cheap Item",
                Quantity = 1,
                UnitPrice = Money.USD(20.00m) // $20
            });

            order.AddItem(new OrderItem
            {
                ProductId = "prod-2",
                ProductName = "Expensive Item",
                Quantity = 1,
                UnitPrice = Money.USD(80.00m) // $80
            });

            var coupon = Coupon.CreateFixedAmountCoupon(
                "SAVE10",
                "$10 Off",
                10.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(order, "SAVE10", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.LineDiscounts.Count);
            
            // $20 item should get $2 discount (20% of $10)
            var cheapItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-1");
            Assert.Equal(Money.USD(2.00m), cheapItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(18.00m), cheapItemDiscount.FinalPrice);
            
            // $80 item should get $8 discount (80% of $10)
            var expensiveItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-2");
            Assert.Equal(Money.USD(8.00m), expensiveItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(72.00m), expensiveItemDiscount.FinalPrice);
        }

        [Fact]
        public async Task ApplyCoupon_ZeroDiscount_NoLineDiscountsCreated()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE0",
                "0% Off",
                0.00m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE0", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.LineDiscounts);
            Assert.Equal(Money.USD(0.00m), result.DiscountAmount);
        }

        [Fact]
        public async Task ApplyCoupon_DiscountExceedsSubtotal_CapsAtSubtotal()
        {
            // Arrange
            var coupon = Coupon.CreateFixedAmountCoupon(
                "SAVE200",
                "$200 Off",
                200.00m,
                "USD",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE200", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.LineDiscounts.Count);
            
            // Discount should be capped at subtotal ($100), allocated proportionally
            var firstItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-1");
            var secondItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-2");
            
            Assert.Equal(Money.USD(50.00m), firstItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(50.00m), secondItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(0.00m), firstItemDiscount.FinalPrice);
            Assert.Equal(Money.USD(0.00m), secondItemDiscount.FinalPrice);
        }

        [Fact]
        public async Task ApplyCoupon_MaximumDiscountCap_RespectsCapInLineDiscounts()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE50",
                "50% Off",
                0.50m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            coupon.MaximumDiscountAmount = Money.USD(25.00m); // Cap at $25
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE50", "test-customer-123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.LineDiscounts.Count);
            
            // Total discount should be capped at $25, allocated proportionally
            var totalLineDiscounts = result.LineDiscounts.Sum(ld => ld.DiscountAmount.Amount);
            Assert.Equal(25.00m, totalLineDiscounts);
            
            // Each item should get $12.50 discount (50% of $25 cap)
            var firstItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-1");
            var secondItemDiscount = result.LineDiscounts.First(ld => ld.Item.ProductId == "prod-2");
            
            Assert.Equal(Money.USD(12.50m), firstItemDiscount.DiscountAmount);
            Assert.Equal(Money.USD(12.50m), secondItemDiscount.DiscountAmount);
        }

        [Fact]
        public async Task GetDiscountBreakdown_WithAppliedCoupon_ReturnsLineDiscounts()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);
            
            var result = await _evaluator.ApplyCouponAsync(_testOrder, "SAVE10", "test-customer-123");
            Assert.True(result.IsSuccess);

            // Act
            var breakdown = _evaluator.GetDiscountBreakdown(_testOrder);

            // Assert
            Assert.NotNull(breakdown);
            Assert.Equal(2, breakdown.LineDiscounts.Count);
            Assert.Equal(Money.USD(10.00m), breakdown.TotalDiscount);
            
            var firstItem = breakdown.LineDiscounts.First(ld => ld.Item.ProductId == "prod-1");
            var secondItem = breakdown.LineDiscounts.First(ld => ld.Item.ProductId == "prod-2");
            
            Assert.Equal(Money.USD(5.00m), firstItem.DiscountAmount);
            Assert.Equal(Money.USD(5.00m), secondItem.DiscountAmount);
        }

        [Fact]
        public void GetDiscountBreakdown_NoAppliedCoupons_ReturnsEmptyList()
        {
            // Act
            var breakdown = _evaluator.GetDiscountBreakdown(_testOrder);

            // Assert
            Assert.NotNull(breakdown);
            Assert.Empty(breakdown.LineDiscounts);
            Assert.Equal(Money.USD(0.00m), breakdown.TotalDiscount);
        }

        [Fact]
        public void GetEligibleItems_AllItemsEligible_ReturnsAllItems()
        {
            // Arrange
            var coupon = Coupon.CreatePercentageCoupon(
                "SAVE10",
                "10% Off",
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30)
            );
            ((InMemoryCouponRepository)_repository).AddCoupon(coupon);

            // Act
            var eligibleItems = _evaluator.GetEligibleItems(_testOrder, "SAVE10");

            // Assert
            Assert.Equal(2, eligibleItems.Count());
            Assert.Contains(eligibleItems, item => item.ProductId == "prod-1");
            Assert.Contains(eligibleItems, item => item.ProductId == "prod-2");
        }

        [Fact]
        public void GetEligibleItems_InvalidCouponCode_ReturnsEmptyList()
        {
            // Act
            var eligibleItems = _evaluator.GetEligibleItems(_testOrder, "INVALID");

            // Assert
            Assert.Empty(eligibleItems);
        }
    }
}
