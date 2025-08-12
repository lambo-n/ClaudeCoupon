using System;
using System.Threading.Tasks;
using FluentAssertions;
using EcommerceCouponLibrary.Core.Models;
using EcommerceCouponLibrary.Core.Services;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Providers.InMemory;
using Xunit;

namespace EcommerceCouponLibrary.Tests.Unit.Core
{
    public class FakeCustomerEligibilityService : ICustomerEligibilityService
    {
        public bool FirstOrder { get; set; } = true;
        public string[] Groups { get; set; } = Array.Empty<string>();

        public Task<bool> IsFirstOrderAsync(string customerId) => Task.FromResult(FirstOrder);
        public Task<bool> IsInAllowedGroupsAsync(string customerId, string[] allowedGroups)
        {
            foreach (var g in Groups)
            {
                foreach (var a in allowedGroups)
                {
                    if (string.Equals(g, a, StringComparison.OrdinalIgnoreCase)) return Task.FromResult(true);
                }
            }
            return Task.FromResult(allowedGroups.Length == 0);
        }
    }

    public class CouponEligibilityAdvancedTests
    {
        private static (CouponEvaluator evaluator, InMemoryCouponRepository repo, Order order, FakeCustomerEligibilityService elig) CreateSystem()
        {
            var repo = new InMemoryCouponRepository();
            var elig = new FakeCustomerEligibilityService();
            var evaluator = new CouponEvaluator(repo, elig);
            var order = new Order { CurrencyCode = "USD", CountryCode = "US" };
            return (evaluator, repo, order, elig);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task RequireFirstOrder_NonFirstOrder_Rejected_M009()
        {
            var (evaluator, repo, order, elig) = CreateSystem();
            elig.FirstOrder = false;
            var coupon = new Coupon
            {
                Id = Guid.NewGuid(), Code = "FIRST",
                Name = "First Order Only", Type = CouponType.Percentage, Value = 0.10m,
                CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true,
                RequireFirstOrder = true
            };
            repo.SeedCoupons(new[] { coupon });

            var result = await evaluator.ValidateCouponAsync("FIRST", order, "cust-1");
            result.IsSuccess.Should().BeFalse();
            result.RejectionReason.Should().Be(CouponRejectionReason.CustomerNotEligible);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task AllowedGroups_NotMember_Rejected_M009()
        {
            var (evaluator, repo, order, elig) = CreateSystem();
            elig.Groups = new[] { "vip" };
            var coupon = new Coupon
            {
                Id = Guid.NewGuid(), Code = "VIP",
                Name = "VIP Only", Type = CouponType.Percentage, Value = 0.10m,
                CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true,
                AllowedCustomerGroups = new[] { "gold" }
            };
            repo.SeedCoupons(new[] { coupon });

            var result = await evaluator.ValidateCouponAsync("VIP", order, "cust-1");
            result.IsSuccess.Should().BeFalse();
            result.RejectionReason.Should().Be(CouponRejectionReason.CustomerNotEligible);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task GeoRestriction_DisallowedCountry_Rejected_M010()
        {
            var (evaluator, repo, order, _) = CreateSystem();
            order.CountryCode = "CA";
            var coupon = new Coupon
            {
                Id = Guid.NewGuid(), Code = "USONLY",
                Name = "US Only", Type = CouponType.Percentage, Value = 0.10m,
                CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true,
                AllowedCountries = new[] { "US" }
            };
            repo.SeedCoupons(new[] { coupon });

            var result = await evaluator.ValidateCouponAsync("USONLY", order, "cust-1");
            result.IsSuccess.Should().BeFalse();
            result.RejectionReason.Should().Be(CouponRejectionReason.CustomerNotEligible);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task CurrencyRestriction_DisallowedCurrency_Rejected_M010()
        {
            var (evaluator, repo, order, _) = CreateSystem();
            order.CurrencyCode = "EUR";
            var coupon = new Coupon
            {
                Id = Guid.NewGuid(), Code = "USDONLY",
                Name = "USD Only", Type = CouponType.Percentage, Value = 0.10m,
                CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true,
                AllowedCurrencies = new[] { "USD" }
            };
            repo.SeedCoupons(new[] { coupon });

            var result = await evaluator.ValidateCouponAsync("USDONLY", order, "cust-1");
            result.IsSuccess.Should().BeFalse();
            result.RejectionReason.Should().Be(CouponRejectionReason.CurrencyMismatch);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task NonCombinable_WhenExistingCoupon_CannotCombine_M015()
        {
            var (evaluator, repo, order, _) = CreateSystem();
            var c1 = new Coupon { Id = Guid.NewGuid(), Code = "A", Name = "A", Type = CouponType.FixedAmount, Value = 5m, CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true, IsCombinable = false };
            var c2 = new Coupon { Id = Guid.NewGuid(), Code = "B", Name = "B", Type = CouponType.FixedAmount, Value = 5m, CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true, IsCombinable = false };
            repo.SeedCoupons(new[] { c1, c2 });
            order.AddItem(new OrderItem { UnitPrice = Money.USD(50m), Quantity = 1 });

            var r1 = await evaluator.ApplyCouponAsync(order, "A", "cust-1");
            r1.IsSuccess.Should().BeTrue();

            var r2 = await evaluator.ApplyCouponAsync(order, "B", "cust-1");
            r2.IsSuccess.Should().BeFalse();
            r2.RejectionReason.Should().Be(CouponRejectionReason.CannotCombine);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Core")]
        public async Task FreeShippingCoupon_RemovesShipping_M017()
        {
            var (evaluator, repo, order, _) = CreateSystem();
            order.AddItem(new OrderItem { UnitPrice = Money.USD(100m), Quantity = 1 });
            order.ShippingAmount = Money.USD(20m);
            order.TaxAmount = Money.USD(10m);

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(), Code = "FREESHIP",
                Name = "Free Shipping", Type = CouponType.FreeShipping, Value = 0m,
                CurrencyCode = "USD", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), IsActive = true
            };
            repo.SeedCoupons(new[] { coupon });

            var result = await evaluator.ApplyCouponAsync(order, "FREESHIP", "cust-1");
            result.IsSuccess.Should().BeTrue();
            result.DiscountAmount.Amount.Should().Be(20m);
            result.OrderTotals!.FinalTotal.Amount.Should().Be(110m); // 100 + 20 + 10 - 20
        }
    }
}
