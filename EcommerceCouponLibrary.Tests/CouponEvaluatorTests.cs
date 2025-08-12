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
            Assert.True(result);
            Assert.Empty(_testOrder.AppliedCoupons);
        }

        [Fact]
        public void RemoveCoupon_NonExistentCoupon_ReturnsFalse()
        {
            // Act
            var result = _evaluator.RemoveCoupon(_testOrder, Guid.NewGuid());

            // Assert
            Assert.False(result);
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
    }
}
