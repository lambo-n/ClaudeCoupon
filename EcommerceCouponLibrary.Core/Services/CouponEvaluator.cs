using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Core.Services
{
    /// <summary>
    /// Service for evaluating and applying coupons to orders
    /// </summary>
    public class CouponEvaluator : ICouponEvaluator
    {
        private readonly ICouponRepository _couponRepository;

        public CouponEvaluator(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository ?? throw new ArgumentNullException(nameof(couponRepository));
        }

        /// <summary>
        /// Applies a coupon to an order
        /// </summary>
        /// <param name="order">The order to apply the coupon to</param>
        /// <param name="couponCode">The coupon code to apply</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The result of the coupon application</returns>
        public async Task<CouponApplicationResult> ApplyCouponAsync(Order order, string couponCode, string customerId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (string.IsNullOrWhiteSpace(couponCode))
                throw new ArgumentException("Coupon code cannot be null or empty", nameof(couponCode));

            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

            // Normalize the coupon code (case-insensitive)
            var normalizedCode = couponCode.Trim().ToUpperInvariant();

            // Get the coupon from the repository
            var coupon = await _couponRepository.GetByCodeAsync(normalizedCode);
            if (coupon == null)
            {
                return CouponApplicationResult.Failure(
                    null,
                    CouponRejectionReason.NotFound,
                    "Invalid coupon code. Please check the code and try again."
                );
            }

            // Validate the coupon
            var validationResult = await ValidateCouponInternalAsync(coupon, order, customerId);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // Calculate the discount
            var discountCalculation = CalculateDiscount(coupon, order);
            if (discountCalculation.DiscountAmount.IsZero)
            {
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.NoEligibleItems,
                    "This coupon doesn't apply to any items in your cart."
                );
            }

            // Apply the coupon to the order
            order.ApplyCoupon(coupon, discountCalculation.DiscountAmount);

            // Get updated order totals
            var orderTotals = GetOrderTotals(order);

            return CouponApplicationResult.Success(
                coupon,
                discountCalculation.DiscountAmount,
                orderTotals,
                discountCalculation.LineDiscounts
            );
        }

        /// <summary>
        /// Validates a coupon code without applying it
        /// </summary>
        /// <param name="couponCode">The coupon code to validate</param>
        /// <param name="order">The order to validate against</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The validation result</returns>
        public async Task<CouponApplicationResult> ValidateCouponAsync(string couponCode, Order order, string customerId)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                throw new ArgumentException("Coupon code cannot be null or empty", nameof(couponCode));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

            // Normalize the coupon code (case-insensitive)
            var normalizedCode = couponCode.Trim().ToUpperInvariant();

            // Get the coupon from the repository
            var coupon = await _couponRepository.GetByCodeAsync(normalizedCode);
            if (coupon == null)
            {
                return CouponApplicationResult.Failure(
                    null,
                    CouponRejectionReason.NotFound,
                    "Invalid coupon code. Please check the code and try again."
                );
            }

            return await ValidateCouponInternalAsync(coupon, order, customerId);
        }

        /// <summary>
        /// Removes a coupon from an order
        /// </summary>
        /// <param name="order">The order to remove the coupon from</param>
        /// <param name="couponId">The ID of the coupon to remove</param>
        /// <returns>The result of the coupon removal</returns>
        public CouponApplicationResult RemoveCoupon(Order order, Guid couponId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var appliedCoupon = order.AppliedCoupons.FirstOrDefault(ac => ac.Coupon.Id == couponId);
            if (appliedCoupon == null)
            {
                return CouponApplicationResult.Failure(
                    null,
                    CouponRejectionReason.NotFound,
                    "The specified coupon was not found on this order."
                );
            }

            // Store coupon info before removal
            var removedCoupon = appliedCoupon.Coupon;
            var removedDiscount = appliedCoupon.DiscountAmount;

            // Remove the coupon
            order.RemoveCoupon(couponId);

            // Get updated order totals
            var orderTotals = GetOrderTotals(order);

            return CouponApplicationResult.Success(
                removedCoupon,
                Money.Zero(order.CurrencyCode), // No discount after removal
                orderTotals,
                new List<LineDiscount>(),
                $"Coupon '{removedCoupon.Name}' has been removed. Your total has been updated."
            );
        }

        /// <summary>
        /// Replaces an existing coupon with a new one
        /// </summary>
        /// <param name="order">The order to replace the coupon on</param>
        /// <param name="existingCouponId">The ID of the existing coupon to remove</param>
        /// <param name="newCouponCode">The new coupon code to apply</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The result of the coupon replacement</returns>
        public async Task<CouponApplicationResult> ReplaceCouponAsync(Order order, Guid existingCouponId, string newCouponCode, string customerId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (string.IsNullOrWhiteSpace(newCouponCode))
                throw new ArgumentException("New coupon code cannot be null or empty", nameof(newCouponCode));

            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

            // First, remove the existing coupon
            var existingCoupon = order.AppliedCoupons.FirstOrDefault(ac => ac.Coupon.Id == existingCouponId);
            if (existingCoupon == null)
            {
                return CouponApplicationResult.Failure(
                    null,
                    CouponRejectionReason.NotFound,
                    "The specified coupon was not found on this order."
                );
            }

            // Store the existing coupon info for the result
            var removedCoupon = existingCoupon.Coupon;
            var removedDiscount = existingCoupon.DiscountAmount;

            // Remove the existing coupon
            order.RemoveCoupon(existingCouponId);

            // Now apply the new coupon
            var newCouponResult = await ApplyCouponAsync(order, newCouponCode, customerId);

            // If the new coupon application failed, we need to restore the original coupon
            if (!newCouponResult.IsSuccess)
            {
                // Restore the original coupon
                order.ApplyCoupon(removedCoupon, removedDiscount);

                // Return a failure result with information about both the removal and the failed application
                return CouponApplicationResult.Failure(
                    newCouponResult.Coupon,
                    newCouponResult.RejectionReason ?? CouponRejectionReason.Unknown,
                    $"Failed to apply new coupon: {newCouponResult.Message}. Original coupon has been restored."
                );
            }

            // Success - return the new coupon result with additional context
            return CouponApplicationResult.Success(
                newCouponResult.Coupon!,
                newCouponResult.DiscountAmount,
                newCouponResult.OrderTotals!,
                newCouponResult.LineDiscounts,
                $"Coupon '{removedCoupon.Name}' was replaced with '{newCouponResult.Coupon!.Name}'. New savings: {newCouponResult.DiscountAmount}"
            );
        }

        /// <summary>
        /// Gets the current order totals after all applied coupons
        /// </summary>
        /// <param name="order">The order</param>
        /// <returns>The order totals</returns>
        public OrderTotals GetOrderTotals(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return new OrderTotals
            {
                Subtotal = order.Subtotal,
                TotalDiscount = order.TotalDiscount,
                FinalTotal = order.Total,
                CurrencyCode = order.CurrencyCode
            };
        }

        /// <summary>
        /// Internal validation method
        /// </summary>
        private async Task<CouponApplicationResult> ValidateCouponInternalAsync(Coupon coupon, Order order, string customerId)
        {
            var currentTime = DateTime.UtcNow;

            // Check if coupon is active
            if (!coupon.IsActive)
            {
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.Inactive,
                    "This coupon is currently inactive."
                );
            }

            // Check if coupon is within its active window
            if (currentTime < coupon.StartDate)
            {
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.NotYetActive,
                    $"This coupon will be active starting {coupon.StartDate:g}."
                );
            }

            if (currentTime > coupon.EndDate)
            {
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.Expired,
                    "This coupon has expired."
                );
            }

            // Check currency compatibility
            if (coupon.Type == CouponType.FixedAmount && coupon.CurrencyCode != order.CurrencyCode)
            {
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.CurrencyMismatch,
                    $"This coupon is only valid for {coupon.CurrencyCode} orders."
                );
            }

            // Check minimum order amount
            if (coupon.MinimumOrderAmount.HasValue && order.Subtotal < coupon.MinimumOrderAmount.Value)
            {
                var remaining = coupon.MinimumOrderAmount.Value - order.Subtotal;
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.BelowMinimum,
                    $"Order must be at least {coupon.MinimumOrderAmount.Value} to use this coupon. Add {remaining} more to your cart."
                );
            }

            // Check global usage limit
            if (coupon.GlobalUsageLimit.HasValue)
            {
                var globalUsage = await _couponRepository.GetGlobalUsageCountAsync(coupon.Id);
                if (globalUsage >= coupon.GlobalUsageLimit.Value)
                {
                    return CouponApplicationResult.Failure(
                        coupon,
                        CouponRejectionReason.GlobalLimitReached,
                        "This coupon has reached its usage limit."
                    );
                }
            }

            // Check per-customer usage limit
            if (coupon.PerCustomerUsageLimit.HasValue)
            {
                var customerUsage = await _couponRepository.GetCustomerUsageCountAsync(coupon.Id, customerId);
                if (customerUsage >= coupon.PerCustomerUsageLimit.Value)
                {
                    return CouponApplicationResult.Failure(
                        coupon,
                        CouponRejectionReason.CustomerLimitReached,
                        "You have already used this coupon the maximum number of times."
                    );
                }
            }

            // Check if this is a unique code that has already been used
            if (await _couponRepository.IsUniqueCodeUsedAsync(coupon.Code))
            {
                return CouponApplicationResult.Failure(
                    coupon,
                    CouponRejectionReason.AlreadyUsed,
                    "This coupon code has already been used."
                );
            }

            return CouponApplicationResult.Success(
                coupon,
                Money.Zero(order.CurrencyCode),
                GetOrderTotals(order),
                new List<LineDiscount>()
            );
        }

        /// <summary>
        /// Calculates the discount for a coupon on an order
        /// </summary>
        private DiscountCalculation CalculateDiscount(Coupon coupon, Order order)
        {
            var eligibleItems = GetEligibleItems(coupon, order);
            if (!eligibleItems.Any())
            {
                return new DiscountCalculation
                {
                    DiscountAmount = Money.Zero(order.CurrencyCode),
                    LineDiscounts = new List<LineDiscount>()
                };
            }

            var eligibleSubtotal = eligibleItems.Sum(item => item.TotalPrice.Amount);
            var discountAmount = CalculateDiscountAmount(coupon, eligibleSubtotal);

            // Apply maximum discount cap if specified
            if (coupon.MaximumDiscountAmount.HasValue)
            {
                discountAmount = Money.Min(discountAmount, coupon.MaximumDiscountAmount.Value);
            }

            // Allocate the discount across eligible items proportionally
            var lineDiscounts = AllocateDiscountToItems(eligibleItems, discountAmount);

            return new DiscountCalculation
            {
                DiscountAmount = discountAmount,
                LineDiscounts = lineDiscounts
            };
        }

        /// <summary>
        /// Gets items that are eligible for the coupon
        /// </summary>
        private IEnumerable<OrderItem> GetEligibleItems(Coupon coupon, Order order)
        {
            return order.Items.Where(item => IsItemEligible(coupon, item));
        }

        /// <summary>
        /// Checks if an item is eligible for a coupon
        /// </summary>
        private bool IsItemEligible(Coupon coupon, OrderItem item)
        {
            // For now, all items are eligible
            // This will be extended in future epics for product/category targeting
            return true;
        }

        /// <summary>
        /// Calculates the discount amount for a coupon
        /// </summary>
        private Money CalculateDiscountAmount(Coupon coupon, decimal eligibleSubtotal)
        {
            var eligibleSubtotalMoney = Money.Create(eligibleSubtotal, coupon.CurrencyCode);
            
            return coupon.Type switch
            {
                CouponType.Percentage => eligibleSubtotalMoney * coupon.Value,
                CouponType.FixedAmount => Money.Min(coupon.FixedAmount!.Value, eligibleSubtotalMoney),
                _ => throw new InvalidOperationException($"Unknown coupon type: {coupon.Type}")
            };
        }

        /// <summary>
        /// Allocates a discount amount across eligible items proportionally
        /// </summary>
        private List<LineDiscount> AllocateDiscountToItems(IEnumerable<OrderItem> eligibleItems, Money totalDiscount)
        {
            var lineDiscounts = new List<LineDiscount>();
            var items = eligibleItems.ToList();

            if (!items.Any() || totalDiscount.IsZero)
                return lineDiscounts;

            var totalEligibleAmount = items.Sum(item => item.TotalPrice.Amount);
            var remainingDiscount = totalDiscount;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var isLastItem = i == items.Count - 1;

                Money itemDiscount;
                if (isLastItem)
                {
                    // Give remaining discount to last item to avoid rounding issues
                    itemDiscount = remainingDiscount;
                }
                else
                {
                    // Calculate proportional discount
                    var proportion = item.TotalPrice.Amount / totalEligibleAmount;
                    itemDiscount = totalDiscount * proportion;
                    remainingDiscount -= itemDiscount;
                }

                lineDiscounts.Add(new LineDiscount
                {
                    Item = item,
                    DiscountAmount = itemDiscount,
                    OriginalPrice = item.TotalPrice,
                    FinalPrice = item.TotalPrice - itemDiscount
                });
            }

            return lineDiscounts;
        }

        /// <summary>
        /// Internal class for discount calculation results
        /// </summary>
        private class DiscountCalculation
        {
            public Money DiscountAmount { get; set; }
            public List<LineDiscount> LineDiscounts { get; set; } = new();
        }
    }
}
