using System;
using System.Threading.Tasks;
using FluentAssertions;
using EcommerceCouponLibrary.Application.Interfaces;
using EcommerceCouponLibrary.Application.Services;
using EcommerceCouponLibrary.Application.DTOs;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;
using EcommerceCouponLibrary.Providers.InMemory;
using Xunit;

namespace EcommerceCouponLibrary.Tests.Unit.Application.Services
{
    public class CouponManagementServiceTests
    {
        private static (ICouponRepository readRepo, ICouponWriteRepository writeRepo, ICouponManagementService service) CreateSystem()
        {
            var inMemoryRepo = new InMemoryCouponRepository();
            ICouponRepository readRepo = inMemoryRepo;
            ICouponWriteRepository writeRepo = inMemoryRepo;
            var service = new CouponManagementService(writeRepo);
            return (readRepo, writeRepo, service);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Application")]
        public async Task CreatePercentageCoupon_SchedulesAndPersists_M001()
        {
            // Arrange
            var (readRepo, _, service) = CreateSystem();
            var request = new CreatePercentageCouponRequest
            {
                Code = "SAVE15",
                Name = "15% Off",
                Percentage = 0.15m,
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                IsCombinable = false,
                CurrencyCode = "USD"
            };

            // Act
            var created = await service.CreatePercentageCouponAsync(request);

            // Assert
            created.Should().NotBeNull();
            created.Code.Should().Be("SAVE15");
            created.Type.Should().Be(CouponType.Percentage);
            created.Value.Should().Be(0.15m);
            created.IsCombinable.Should().BeFalse();
            created.StartDate.Should().BeCloseTo(request.StartDate, TimeSpan.FromSeconds(1));
            created.EndDate.Should().BeCloseTo(request.EndDate, TimeSpan.FromSeconds(1));

            var fetched = await readRepo.GetByCodeAsync("SAVE15");
            fetched.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Application")]
        public async Task CreateFixedAmountCoupon_PersistsWithCurrency_M001()
        {
            // Arrange
            var (readRepo, _, service) = CreateSystem();
            var request = new CreateFixedAmountCouponRequest
            {
                Code = "LESS10",
                Name = "$10 Off",
                Amount = 10m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddMinutes(5),
                EndDate = DateTime.UtcNow.AddDays(3),
                IsCombinable = true
            };

            // Act
            var created = await service.CreateFixedAmountCouponAsync(request);

            // Assert
            created.Should().NotBeNull();
            created.Code.Should().Be("LESS10");
            created.Type.Should().Be(CouponType.FixedAmount);
            created.Value.Should().Be(10m);
            created.CurrencyCode.Should().Be("USD");
            created.IsCombinable.Should().BeTrue();

            var fetched = await readRepo.GetByCodeAsync("LESS10");
            fetched.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Application")]
        public async Task ScheduleAndPauseResume_ValidationReflectsState_M002_M003()
        {
            // Arrange
            var (readRepo, _, service) = CreateSystem();
            var createReq = new CreatePercentageCouponRequest
            {
                Code = "WINTER20",
                Name = "20% Off",
                Percentage = 0.20m,
                StartDate = DateTime.UtcNow.AddMinutes(30),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsCombinable = false,
                CurrencyCode = "USD"
            };
            var created = await service.CreatePercentageCouponAsync(createReq);
            var evaluator = new EcommerceCouponLibrary.Core.Services.CouponEvaluator(readRepo);

            var order = new Order { CurrencyCode = "USD" };

            // Before start -> NotYetActive
            var validateBefore = await evaluator.ValidateCouponAsync("WINTER20", order, "cust-1");
            validateBefore.IsSuccess.Should().BeFalse();
            validateBefore.RejectionReason.Should().Be(CouponRejectionReason.NotYetActive);

            // Pause -> Inactive
            await service.PauseCouponAsync(created.Id);
            var validatePaused = await evaluator.ValidateCouponAsync("WINTER20", order, "cust-1");
            validatePaused.IsSuccess.Should().BeFalse();
            validatePaused.RejectionReason.Should().Be(CouponRejectionReason.Inactive);

            // Resume -> back to not yet active (since future start)
            await service.ResumeCouponAsync(created.Id);
            var validateResumed = await evaluator.ValidateCouponAsync("WINTER20", order, "cust-1");
            validateResumed.IsSuccess.Should().BeFalse();
            validateResumed.RejectionReason.Should().Be(CouponRejectionReason.NotYetActive);

            // Move start earlier -> Success
            var newStart = DateTime.UtcNow.AddMinutes(-1);
            await service.UpdateScheduleAsync(created.Id, newStart, created.EndDate);
            var validateNow = await evaluator.ValidateCouponAsync("WINTER20", order, "cust-1");
            validateNow.IsSuccess.Should().BeTrue();
        }
    }
}
