using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;
using MongoDB.Driver;

namespace EcommerceCouponLibrary.Providers.Mongo
{
    public class MongoCouponRepository : ICouponRepository
    {
        private readonly IMongoCollection<Coupon> _coupons;
        private readonly IMongoCollection<CouponUsage> _usages;

        public MongoCouponRepository(IMongoDatabase database, string couponsCollectionName = "coupons", string usagesCollectionName = "coupon_usages")
        {
            _coupons = database.GetCollection<Coupon>(couponsCollectionName);
            _usages = database.GetCollection<CouponUsage>(usagesCollectionName);
            _coupons.Indexes.CreateOne(new CreateIndexModel<Coupon>(Builders<Coupon>.IndexKeys.Ascending(c => c.Code), new CreateIndexOptions { Unique = false }));
            _usages.Indexes.CreateOne(new CreateIndexModel<CouponUsage>(Builders<CouponUsage>.IndexKeys.Ascending(u => new { u.CouponId, u.CustomerId })));
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var normalized = code.Trim().ToUpperInvariant();
            var filter = Builders<Coupon>.Filter.Where(c => c.Code.ToUpper() == normalized);
            return await _coupons.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Coupon?> GetByIdAsync(Guid id)
        {
            return await _coupons.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            var filter = Builders<Coupon>.Filter.Where(c => c.IsValidAt(now));
            return await _coupons.Find(filter).ToListAsync();
        }

        public async Task<int> GetGlobalUsageCountAsync(Guid couponId)
        {
            return (int)await _usages.CountDocumentsAsync(u => u.CouponId == couponId);
        }

        public async Task<int> GetCustomerUsageCountAsync(Guid couponId, string customerId)
        {
            return (int)await _usages.CountDocumentsAsync(u => u.CouponId == couponId && u.CustomerId == customerId);
        }

        public async Task RecordUsageAsync(Guid couponId, string customerId, Guid orderId)
        {
            await _usages.InsertOneAsync(new CouponUsage { CouponId = couponId, CustomerId = customerId, OrderId = orderId });
        }

        public async Task<bool> IsUniqueCodeUsedAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var normalized = code.Trim().ToUpperInvariant();
            var filter = Builders<Coupon>.Filter.Where(c => c.Code.ToUpper() == normalized);
            return await _coupons.Find(filter).AnyAsync();
        }
    }

    public class CouponUsage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CouponId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
