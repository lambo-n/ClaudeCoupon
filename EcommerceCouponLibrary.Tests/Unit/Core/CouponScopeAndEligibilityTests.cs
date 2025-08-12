using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using EcommerceCouponLibrary.Core.Models;
using EcommerceCouponLibrary.Core.Services;
using EcommerceCouponLibrary.Providers.InMemory;
using Xunit;

namespace EcommerceCouponLibrary.Tests.Unit.Core
{
    public class CouponScopeAndEligibilityTests
    {
        private static (CouponEvaluator evaluator, InMemoryCouponRepository repo, Order order) CreateSystem()
        {
            var repo = new InMemoryCouponRepository();
            var evaluator = new CouponEvaluator(repo);
            var order = new Order { CurrencyCode = "USD" };
            return (evaluator, repo, order);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task ApplyCoupon_Scope_MerchandiseOnly_DoesNotDiscountShippingOrTaxes_M006()
        {
            // Arrange
            var (evaluator, repo, order) = CreateSystem();
            order.AddItem(new OrderItem { UnitPrice = Money.USD(100m), Quantity = 1 });
            order.AddItem(new OrderItem { UnitPrice = Money.USD(50m), Quantity = 2 }); // subtotal 200
            order.ShippingAmount = Money.USD(20m);
            order.TaxAmount = Money.USD(10m);

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "MERCH10",
                Name = "10% Off Merchandise",
                Type = CouponType.Percentage,
                Value = 0.10m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                IsActive = true,
                // Scope: MerchandiseOnly (default expected)
            };
            repo.SeedCoupons(new[] { coupon });

            // Act
            var result = await evaluator.ApplyCouponAsync(order, "MERCH10", "cust-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.DiscountAmount.Amount.Should().BeApproximately(20m, 0.01m); // 10% of 200
            result.OrderTotals!.FinalTotal.Amount.Should().BeApproximately(210m, 0.01m); // 200 + 20 + 10 - 20
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task ApplyCoupon_Scope_OrderTotal_IncludesShippingAndTaxes_M006()
        {
            // Arrange
            var (evaluator, repo, order) = CreateSystem();
            order.AddItem(new OrderItem { UnitPrice = Money.USD(100m), Quantity = 1 }); // subtotal 100
            order.ShippingAmount = Money.USD(20m);
            order.TaxAmount = Money.USD(10m);

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "TOTAL10",
                Name = "10% Off Order Total",
                Type = CouponType.Percentage,
                Value = 0.10m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                IsActive = true,
                Scope = DiscountScope.OrderTotal
            };
            repo.SeedCoupons(new[] { coupon });

            // Act
            var result = await evaluator.ApplyCouponAsync(order, "TOTAL10", "cust-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.DiscountAmount.Amount.Should().BeApproximately(13m, 0.01m); // 10% of 130
            result.OrderTotals!.FinalTotal.Amount.Should().BeApproximately(117m, 0.01m); // 100+20+10-13
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task ApplyCoupon_IncludedCategories_OnlyEligibleItemsDiscounted_M007()
        {
            // Arrange
            var (evaluator, repo, order) = CreateSystem();
            order.AddItem(new OrderItem { ProductId = "A", Category = "Shoes", UnitPrice = Money.USD(100m), Quantity = 1 });
            order.AddItem(new OrderItem { ProductId = "B", Category = "Bags", UnitPrice = Money.USD(50m), Quantity = 2 }); // subtotal 200

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "SHOES20",
                Name = "20% Off Shoes",
                Type = CouponType.Percentage,
                Value = 0.20m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                IsActive = true,
                IncludedCategories = new[] { "Shoes" }
            };
            repo.SeedCoupons(new[] { coupon });

            // Act
            var result = await evaluator.ApplyCouponAsync(order, "SHOES20", "cust-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.DiscountAmount.Amount.Should().BeApproximately(20m, 0.01m); // 20% of Shoes (100)
            // Ensure only 1 line discounted
            result.LineDiscounts.Count(ld => ld.Item.Category == "Shoes").Should().Be(1);
            result.LineDiscounts.Count(ld => ld.Item.Category == "Bags").Should().Be(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task ApplyCoupon_ExcludeSaleAndGiftCards_M008()
        {
            // Arrange
            var (evaluator, repo, order) = CreateSystem();
            order.AddItem(new OrderItem { ProductId = "A", Category = "Apparel", UnitPrice = Money.USD(80m), Quantity = 1, IsOnSale = true });
            order.AddItem(new OrderItem { ProductId = "B", Category = "Gift", UnitPrice = Money.USD(50m), Quantity = 1, IsGiftCard = true });
            order.AddItem(new OrderItem { ProductId = "C", Category = "Apparel", UnitPrice = Money.USD(70m), Quantity = 1 }); // only eligible

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "SAVE10",
                Name = "10% Off",
                Type = CouponType.Percentage,
                Value = 0.10m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                IsActive = true,
                ExcludeSaleItems = true,
                ExcludeGiftCards = true
            };
            repo.SeedCoupons(new[] { coupon });

            // Act
            var result = await evaluator.ApplyCouponAsync(order, "SAVE10", "cust-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.DiscountAmount.Amount.Should().BeApproximately(7m, 0.01m); // 10% of only C (70)
            result.LineDiscounts.Count.Should().Be(1);
            result.LineDiscounts.First().Item.ProductId.Should().Be("C");
        }
    }
}
