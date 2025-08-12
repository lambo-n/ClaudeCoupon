using System;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceCouponLibrary.Core.Models
{
    /// <summary>
    /// Represents a customer's order that can have coupons applied
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique identifier for the order
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Customer identifier
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Currency code for this order
        /// </summary>
        public string CurrencyCode { get; set; } = "USD";

        /// <summary>
        /// Shipping amount for this order
        /// </summary>
        public Money ShippingAmount { get; set; } = Money.USD(0m);

        /// <summary>
        /// Tax amount for this order
        /// </summary>
        public Money TaxAmount { get; set; } = Money.USD(0m);

        /// <summary>
        /// ISO Country code for the order/shipping destination
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Items in the order
        /// </summary>
        public List<OrderItem> Items { get; set; } = new();

        /// <summary>
        /// Applied coupons
        /// </summary>
        public List<AppliedCoupon> AppliedCoupons { get; set; } = new();

        /// <summary>
        /// Subtotal before any discounts
        /// </summary>
        public Money Subtotal => CalculateSubtotal();

        /// <summary>
        /// Total discount amount from all applied coupons
        /// </summary>
        public Money TotalDiscount => CalculateTotalDiscount();

        /// <summary>
        /// Final total after all discounts
        /// </summary>
        public Money Total => Money.Create(Subtotal.Amount + ShippingAmount.Amount + TaxAmount.Amount, CurrencyCode) - TotalDiscount;

        /// <summary>
        /// Date when the order was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Adds an item to the order
        /// </summary>
        /// <param name="item">The item to add</param>
        public void AddItem(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            Items.Add(item);
        }

        /// <summary>
        /// Removes an item from the order
        /// </summary>
        /// <param name="itemId">The ID of the item to remove</param>
        public void RemoveItem(Guid itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        /// <summary>
        /// Applies a coupon to the order
        /// </summary>
        /// <param name="coupon">The coupon to apply</param>
        /// <param name="discountAmount">The calculated discount amount</param>
        public void ApplyCoupon(Coupon coupon, Money discountAmount)
        {
            if (coupon == null)
                throw new ArgumentNullException(nameof(coupon));

            var appliedCoupon = new AppliedCoupon
            {
                Coupon = coupon,
                DiscountAmount = discountAmount,
                AppliedAt = DateTime.UtcNow
            };

            AppliedCoupons.Add(appliedCoupon);
        }

        /// <summary>
        /// Removes a coupon from the order
        /// </summary>
        /// <param name="couponId">The ID of the coupon to remove</param>
        public void RemoveCoupon(Guid couponId)
        {
            var appliedCoupon = AppliedCoupons.FirstOrDefault(ac => ac.Coupon.Id == couponId);
            if (appliedCoupon != null)
            {
                AppliedCoupons.Remove(appliedCoupon);
            }
        }

        /// <summary>
        /// Clears all applied coupons from the order
        /// </summary>
        public void ClearCoupons()
        {
            AppliedCoupons.Clear();
        }

        /// <summary>
        /// Calculates the subtotal of all items
        /// </summary>
        /// <returns>The subtotal amount</returns>
        private Money CalculateSubtotal()
        {
            if (!Items.Any())
                return Money.Zero(CurrencyCode);

            var total = Items.Sum(item => item.TotalPrice.Amount);
            return Money.Create(total, CurrencyCode);
        }

        /// <summary>
        /// Calculates the total discount from all applied coupons
        /// </summary>
        /// <returns>The total discount amount</returns>
        private Money CalculateTotalDiscount()
        {
            if (!AppliedCoupons.Any())
                return Money.Zero(CurrencyCode);

            var total = AppliedCoupons.Sum(ac => ac.DiscountAmount.Amount);
            return Money.Create(total, CurrencyCode);
        }
    }

    /// <summary>
    /// Represents an item in an order
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Unique identifier for the item
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Product identifier
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Product SKU
        /// </summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Quantity of this item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price of the item
        /// </summary>
        public Money UnitPrice { get; set; }

        /// <summary>
        /// Total price for this item (unit price * quantity)
        /// </summary>
        public Money TotalPrice => UnitPrice * Quantity;

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Product brand
        /// </summary>
        public string Brand { get; set; } = string.Empty;

        /// <summary>
        /// Whether this item is on sale
        /// </summary>
        public bool IsOnSale { get; set; } = false;

        /// <summary>
        /// Whether this item is a gift card
        /// </summary>
        public bool IsGiftCard { get; set; } = false;

        /// <summary>
        /// Whether this item is MAP-protected (Minimum Advertised Price)
        /// </summary>
        public bool IsMapProtected { get; set; } = false;

        /// <summary>
        /// Custom flags for this item
        /// </summary>
        public Dictionary<string, bool> Flags { get; set; } = new();
    }

    /// <summary>
    /// Represents a coupon that has been applied to an order
    /// </summary>
    public class AppliedCoupon
    {
        /// <summary>
        /// The coupon that was applied
        /// </summary>
        public Coupon Coupon { get; set; } = null!;

        /// <summary>
        /// The discount amount that was applied
        /// </summary>
        public Money DiscountAmount { get; set; }

        /// <summary>
        /// When the coupon was applied
        /// </summary>
        public DateTime AppliedAt { get; set; }
    }
}
