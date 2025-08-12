using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Providers.Dapper
{
    public class DapperCouponRepository : ICouponRepository
    {
        private readonly IDbConnection _connection;
        public DapperCouponRepository(IDbConnection connection) => _connection = connection;

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            const string sql = @"SELECT * FROM Coupons WHERE UPPER(Code) = UPPER(@Code) LIMIT 1";
            return (await _connection.QueryAsync<Coupon>(sql, new { Code = code.Trim() })).FirstOrDefault();
        }

        public async Task<Coupon?> GetByIdAsync(Guid id)
        {
            const string sql = @"SELECT * FROM Coupons WHERE Id = @Id LIMIT 1";
            return (await _connection.QueryAsync<Coupon>(sql, new { Id = id })).FirstOrDefault();
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            const string sql = @"SELECT * FROM Coupons WHERE IsActive = 1 AND @Now BETWEEN StartDate AND EndDate";
            return await _connection.QueryAsync<Coupon>(sql, new { Now = now });
        }

        public async Task<int> GetGlobalUsageCountAsync(Guid couponId)
        {
            const string sql = @"SELECT COUNT(1) FROM CouponUsages WHERE CouponId = @CouponId";
            return await _connection.ExecuteScalarAsync<int>(sql, new { CouponId = couponId });
        }

        public async Task<int> GetCustomerUsageCountAsync(Guid couponId, string customerId)
        {
            const string sql = @"SELECT COUNT(1) FROM CouponUsages WHERE CouponId = @CouponId AND CustomerId = @CustomerId";
            return await _connection.ExecuteScalarAsync<int>(sql, new { CouponId = couponId, CustomerId = customerId });
        }

        public async Task RecordUsageAsync(Guid couponId, string customerId, Guid orderId)
        {
            const string sql = @"INSERT INTO CouponUsages (Id, CouponId, CustomerId, OrderId, UsedAtUtc) VALUES (@Id, @CouponId, @CustomerId, @OrderId, @UsedAtUtc)";
            await _connection.ExecuteAsync(sql, new { Id = Guid.NewGuid(), CouponId = couponId, CustomerId = customerId, OrderId = orderId, UsedAtUtc = DateTime.UtcNow });
        }

        public async Task<bool> IsUniqueCodeUsedAsync(string code)
        {
            const string sql = @"SELECT EXISTS (SELECT 1 FROM Coupons WHERE UPPER(Code) = UPPER(@Code))";
            return await _connection.ExecuteScalarAsync<bool>(sql, new { Code = code.Trim() });
        }
    }
}
