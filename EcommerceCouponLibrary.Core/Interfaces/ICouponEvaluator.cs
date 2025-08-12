using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Core.Interfaces
{
    /// <summary>
    /// Interface for evaluating and applying coupons to orders
    /// </summary>
    public interface ICouponEvaluator
    {
        /// <summary>
        /// Applies a coupon to an order
        /// </summary>
        /// <param name="order">The order to apply the coupon to</param>
        /// <param name="couponCode">The coupon code to apply</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The result of the coupon application</returns>
        Task<CouponApplicationResult> ApplyCouponAsync(Order order, string couponCode, string customerId);

        /// <summary>
        /// Validates a coupon code without applying it
        /// </summary>
        /// <param name="couponCode">The coupon code to validate</param>
        /// <param name="order">The order to validate against</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The validation result</returns>
        Task<CouponApplicationResult> ValidateCouponAsync(string couponCode, Order order, string customerId);

        /// <summary>
        /// Removes a coupon from an order
        /// </summary>
        /// <param name="order">The order to remove the coupon from</param>
        /// <param name="couponId">The ID of the coupon to remove</param>
        /// <returns>The result of the coupon removal</returns>
        CouponApplicationResult RemoveCoupon(Order order, System.Guid couponId);

        /// <summary>
        /// Replaces an existing coupon with a new one
        /// </summary>
        /// <param name="order">The order to replace the coupon on</param>
        /// <param name="existingCouponId">The ID of the existing coupon to remove</param>
        /// <param name="newCouponCode">The new coupon code to apply</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The result of the coupon replacement</returns>
        Task<CouponApplicationResult> ReplaceCouponAsync(Order order, System.Guid existingCouponId, string newCouponCode, string customerId);

        /// <summary>
        /// Gets the current order totals after all applied coupons
        /// </summary>
        /// <param name="order">The order</param>
        /// <returns>The order totals</returns>
        OrderTotals GetOrderTotals(Order order);
    }
}
