using System;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Application.DTOs;
using EcommerceCouponLibrary.Application.Interfaces;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Application.Services
{
    public class CouponManagementService : ICouponManagementService
    {
        private readonly ICouponWriteRepository _writeRepository;

        public CouponManagementService(ICouponWriteRepository writeRepository)
        {
            _writeRepository = writeRepository ?? throw new ArgumentNullException(nameof(writeRepository));
        }

        public async Task<Coupon> CreatePercentageCouponAsync(CreatePercentageCouponRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (request.Percentage < 0 || request.Percentage > 1) throw new ArgumentException("Percentage must be between 0 and 1");
            if (request.StartDate >= request.EndDate) throw new ArgumentException("StartDate must be before EndDate");

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = request.Code.Trim().ToUpperInvariant(),
                Name = request.Name,
                Type = CouponType.Percentage,
                Value = request.Percentage,
                CurrencyCode = request.CurrencyCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsCombinable = request.IsCombinable,
                IsActive = true
            };

            return await _writeRepository.CreateAsync(coupon);
        }

        public async Task<Coupon> CreateFixedAmountCouponAsync(CreateFixedAmountCouponRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than zero");
            if (request.StartDate >= request.EndDate) throw new ArgumentException("StartDate must be before EndDate");

            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = request.Code.Trim().ToUpperInvariant(),
                Name = request.Name,
                Type = CouponType.FixedAmount,
                Value = request.Amount,
                CurrencyCode = request.CurrencyCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsCombinable = request.IsCombinable,
                IsActive = true
            };

            return await _writeRepository.CreateAsync(coupon);
        }

        public Task<bool> PauseCouponAsync(Guid couponId) => _writeRepository.PauseAsync(couponId);

        public Task<bool> ResumeCouponAsync(Guid couponId) => _writeRepository.ResumeAsync(couponId);

        public async Task<bool> UpdateScheduleAsync(Guid couponId, DateTime newStart, DateTime newEnd)
        {
            if (newStart >= newEnd) throw new ArgumentException("StartDate must be before EndDate");

            // naive approach: fetch/update via write repo contract
            // since write repo API is minimal, we rely on Pause/Resume/Update
            // We'll simulate by creating a minimal coupon instance that the repo updates by Id
            var placeholder = new Coupon { Id = couponId, StartDate = newStart, EndDate = newEnd };
            return await _writeRepository.UpdateAsync(placeholder);
        }
    }
}
