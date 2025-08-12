using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Core.Interfaces
{
    /// <summary>
    /// Repository interface for coupon data access
    /// </summary>
    public interface ICouponRepository
    {
        /// <summary>
        /// Gets a coupon by its code
        /// </summary>
        /// <param name="code">The coupon code</param>
        /// <returns>The coupon if found, null otherwise</returns>
        Task<Coupon?> GetByCodeAsync(string code);

        /// <summary>
        /// Gets a coupon by its ID
        /// </summary>
        /// <param name="id">The coupon ID</param>
        /// <returns>The coupon if found, null otherwise</returns>
        Task<Coupon?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all active coupons
        /// </summary>
        /// <returns>List of active coupons</returns>
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync();

        /// <summary>
        /// Gets the usage count for a coupon globally
        /// </summary>
        /// <param name="couponId">The coupon ID</param>
        /// <returns>The number of times the coupon has been used</returns>
        Task<int> GetGlobalUsageCountAsync(Guid couponId);

        /// <summary>
        /// Gets the usage count for a coupon by a specific customer
        /// </summary>
        /// <param name="couponId">The coupon ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The number of times the customer has used this coupon</returns>
        Task<int> GetCustomerUsageCountAsync(Guid couponId, string customerId);

        /// <summary>
        /// Records a coupon usage
        /// </summary>
        /// <param name="couponId">The coupon ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="orderId">The order ID</param>
        /// <returns>Task</returns>
        Task RecordUsageAsync(Guid couponId, string customerId, Guid orderId);

        /// <summary>
        /// Checks if a unique code has already been used
        /// </summary>
        /// <param name="code">The unique code</param>
        /// <returns>True if the code has been used, false otherwise</returns>
        Task<bool> IsUniqueCodeUsedAsync(string code);
    }
}
