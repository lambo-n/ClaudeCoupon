using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Core.Repositories
{
    /// <summary>
    /// In-memory implementation of the coupon repository for testing and development
    /// </summary>
    public class InMemoryCouponRepository : ICouponRepository
    {
        private readonly ConcurrentDictionary<string, Coupon> _couponsByCode = new();
        private readonly ConcurrentDictionary<Guid, Coupon> _couponsById = new();
        private readonly ConcurrentDictionary<Guid, int> _globalUsageCounts = new();
        private readonly ConcurrentDictionary<string, int> _customerUsageCounts = new();
        private readonly ConcurrentDictionary<string, bool> _usedUniqueCodes = new();

        /// <summary>
        /// Adds a coupon to the repository
        /// </summary>
        /// <param name="coupon">The coupon to add</param>
        public void AddCoupon(Coupon coupon)
        {
            if (coupon == null)
                throw new ArgumentNullException(nameof(coupon));

            _couponsByCode[coupon.Code.ToUpperInvariant()] = coupon;
            _couponsById[coupon.Id] = coupon;
        }

        /// <summary>
        /// Removes a coupon from the repository
        /// </summary>
        /// <param name="couponId">The ID of the coupon to remove</param>
        public void RemoveCoupon(Guid couponId)
        {
            if (_couponsById.TryRemove(couponId, out var coupon))
            {
                _couponsByCode.TryRemove(coupon.Code.ToUpperInvariant(), out _);
            }
        }

        /// <summary>
        /// Gets a coupon by its code
        /// </summary>
        /// <param name="code">The coupon code</param>
        /// <returns>The coupon if found, null otherwise</returns>
        public Task<Coupon?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Task.FromResult<Coupon?>(null);

            var normalizedCode = code.Trim().ToUpperInvariant();
            _couponsByCode.TryGetValue(normalizedCode, out var coupon);
            return Task.FromResult(coupon);
        }

        /// <summary>
        /// Gets a coupon by its ID
        /// </summary>
        /// <param name="id">The coupon ID</param>
        /// <returns>The coupon if found, null otherwise</returns>
        public Task<Coupon?> GetByIdAsync(Guid id)
        {
            _couponsById.TryGetValue(id, out var coupon);
            return Task.FromResult(coupon);
        }

        /// <summary>
        /// Gets all active coupons
        /// </summary>
        /// <returns>List of active coupons</returns>
        public Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            var activeCoupons = _couponsById.Values
                .Where(c => c.IsActive && c.IsValidAt(DateTime.UtcNow))
                .ToList();

            return Task.FromResult<IEnumerable<Coupon>>(activeCoupons);
        }

        /// <summary>
        /// Gets the usage count for a coupon globally
        /// </summary>
        /// <param name="couponId">The coupon ID</param>
        /// <returns>The number of times the coupon has been used</returns>
        public Task<int> GetGlobalUsageCountAsync(Guid couponId)
        {
            _globalUsageCounts.TryGetValue(couponId, out var count);
            return Task.FromResult(count);
        }

        /// <summary>
        /// Gets the usage count for a coupon by a specific customer
        /// </summary>
        /// <param name="couponId">The coupon ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The number of times the customer has used this coupon</returns>
        public Task<int> GetCustomerUsageCountAsync(Guid couponId, string customerId)
        {
            var key = $"{couponId}:{customerId}";
            _customerUsageCounts.TryGetValue(key, out var count);
            return Task.FromResult(count);
        }

        /// <summary>
        /// Records a coupon usage
        /// </summary>
        /// <param name="couponId">The coupon ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="orderId">The order ID</param>
        /// <returns>Task</returns>
        public Task RecordUsageAsync(Guid couponId, string customerId, Guid orderId)
        {
            // Increment global usage count
            _globalUsageCounts.AddOrUpdate(couponId, 1, (key, oldValue) => oldValue + 1);

            // Increment customer usage count
            var customerKey = $"{couponId}:{customerId}";
            _customerUsageCounts.AddOrUpdate(customerKey, 1, (key, oldValue) => oldValue + 1);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if a unique code has already been used
        /// </summary>
        /// <param name="code">The unique code</param>
        /// <returns>True if the code has been used, false otherwise</returns>
        public Task<bool> IsUniqueCodeUsedAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Task.FromResult(false);

            var normalizedCode = code.Trim().ToUpperInvariant();
            return Task.FromResult(_usedUniqueCodes.ContainsKey(normalizedCode));
        }

        /// <summary>
        /// Marks a unique code as used
        /// </summary>
        /// <param name="code">The unique code</param>
        public void MarkUniqueCodeAsUsed(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                var normalizedCode = code.Trim().ToUpperInvariant();
                _usedUniqueCodes[normalizedCode] = true;
            }
        }

        /// <summary>
        /// Clears all data from the repository
        /// </summary>
        public void Clear()
        {
            _couponsByCode.Clear();
            _couponsById.Clear();
            _globalUsageCounts.Clear();
            _customerUsageCounts.Clear();
            _usedUniqueCodes.Clear();
        }
    }
}
