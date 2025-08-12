using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCouponLibrary.Providers.EfCore
{
    public class EfCoreCouponRepository : ICouponRepository
    {
        private readonly CouponDbContext _db;
        public EfCoreCouponRepository(CouponDbContext db) => _db = db;

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var normalized = code.Trim();
            return await _db.Coupons.FirstOrDefaultAsync(c => c.Code.ToLower() == normalized.ToLower());
        }

        public Task<Coupon?> GetByIdAsync(Guid id)
        {
            return _db.Coupons.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            return await _db.Coupons.Where(c => c.IsValidAt(now)).ToListAsync();
        }

        public async Task<int> GetGlobalUsageCountAsync(Guid couponId)
        {
            return await _db.CouponUsages.CountAsync(u => u.CouponId == couponId);
        }

        public async Task<int> GetCustomerUsageCountAsync(Guid couponId, string customerId)
        {
            return await _db.CouponUsages.CountAsync(u => u.CouponId == couponId && u.CustomerId == customerId);
        }

        public async Task RecordUsageAsync(Guid couponId, string customerId, Guid orderId)
        {
            _db.CouponUsages.Add(new CouponUsage { CouponId = couponId, CustomerId = customerId, OrderId = orderId });
            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsUniqueCodeUsedAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            // Assuming unique codes are stored as coupons with unique Code, or a separate table could track single-use codes.
            return await _db.Coupons.AnyAsync(c => c.Code == code.Trim());
        }
    }
}
