using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;
using EcommerceCouponLibrary.Application.Interfaces;

namespace EcommerceCouponLibrary.Providers.InMemory
{
    public class InMemoryCouponRepository : ICouponRepository, ICouponWriteRepository
    {
        private readonly ConcurrentDictionary<Guid, Coupon> _coupons = new();
        private readonly ConcurrentDictionary<Guid, int> _globalUsage = new();
        private readonly ConcurrentDictionary<(Guid CouponId, string CustomerId), int> _customerUsage = new();
        private readonly ConcurrentDictionary<string, bool> _uniqueCodeUsage = new(StringComparer.OrdinalIgnoreCase);

        public Task<Coupon?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return Task.FromResult<Coupon?>(null);
            var normalized = code.Trim();
            var coupon = _coupons.Values.FirstOrDefault(c => string.Equals(c.Code, normalized, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(coupon);
        }

        public Task<Coupon?> GetByIdAsync(Guid id)
        {
            _coupons.TryGetValue(id, out var coupon);
            return Task.FromResult<Coupon?>(coupon);
        }

        public Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            var active = _coupons.Values.Where(c => c.IsValidAt(now));
            return Task.FromResult<IEnumerable<Coupon>>(active.ToList());
        }

        public Task<int> GetGlobalUsageCountAsync(Guid couponId)
        {
            _globalUsage.TryGetValue(couponId, out var count);
            return Task.FromResult(count);
        }

        public Task<int> GetCustomerUsageCountAsync(Guid couponId, string customerId)
        {
            _customerUsage.TryGetValue((couponId, customerId), out var count);
            return Task.FromResult(count);
        }

        public Task RecordUsageAsync(Guid couponId, string customerId, Guid orderId)
        {
            _globalUsage.AddOrUpdate(couponId, 1, (_, v) => v + 1);
            _customerUsage.AddOrUpdate((couponId, customerId), 1, (_, v) => v + 1);
            return Task.CompletedTask;
        }

        public Task<bool> IsUniqueCodeUsedAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return Task.FromResult(false);
            var used = _uniqueCodeUsage.ContainsKey(code.Trim());
            return Task.FromResult(used);
        }

        // Provider-specific convenience methods
        public void SeedCoupons(IEnumerable<Coupon> coupons)
        {
            foreach (var coupon in coupons)
            {
                _coupons[coupon.Id] = coupon;
            }
        }

        public void MarkUniqueCodeAsUsed(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                _uniqueCodeUsage.TryAdd(code.Trim(), true);
            }
        }

        // Write operations
        public Task<Coupon> CreateAsync(Coupon coupon)
        {
            _coupons[coupon.Id] = coupon;
            return Task.FromResult(coupon);
        }

        public Task<bool> UpdateAsync(Coupon coupon)
        {
            if (!_coupons.TryGetValue(coupon.Id, out var existing)) return Task.FromResult(false);
            // update only provided fields (schedule updates in tests)
            if (coupon.StartDate != default) existing.StartDate = coupon.StartDate;
            if (coupon.EndDate != default) existing.EndDate = coupon.EndDate;
            _coupons[coupon.Id] = existing;
            return Task.FromResult(true);
        }

        public Task<bool> PauseAsync(Guid couponId)
        {
            if (!_coupons.TryGetValue(couponId, out var existing)) return Task.FromResult(false);
            existing.IsActive = false;
            _coupons[couponId] = existing;
            return Task.FromResult(true);
        }

        public Task<bool> ResumeAsync(Guid couponId)
        {
            if (!_coupons.TryGetValue(couponId, out var existing)) return Task.FromResult(false);
            existing.IsActive = true;
            _coupons[couponId] = existing;
            return Task.FromResult(true);
        }
    }
}
