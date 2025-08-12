using System.Collections.Generic;
using System.Linq;

namespace EcommerceCouponLibrary.Core.Models
{
    /// <summary>
    /// Represents a breakdown of discounts applied to an order
    /// </summary>
    public class DiscountBreakdown
    {
        /// <summary>
        /// List of line-level discounts
        /// </summary>
        public List<LineDiscount> LineDiscounts { get; set; } = new();

        /// <summary>
        /// Total discount amount across all items
        /// </summary>
        public Money TotalDiscount => CalculateTotalDiscount();

        /// <summary>
        /// Number of items that received discounts
        /// </summary>
        public int DiscountedItemCount => LineDiscounts.Count;

        /// <summary>
        /// Number of items that did not receive discounts
        /// </summary>
        public int NonDiscountedItemCount { get; set; }

        /// <summary>
        /// Currency code for all amounts
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets items that received discounts
        /// </summary>
        /// <returns>List of items with discounts</returns>
        public IEnumerable<OrderItem> GetDiscountedItems()
        {
            return LineDiscounts.Select(ld => ld.Item);
        }

        /// <summary>
        /// Gets items that did not receive discounts
        /// </summary>
        /// <param name="allOrderItems">All items in the order</param>
        /// <returns>List of items without discounts</returns>
        public IEnumerable<OrderItem> GetNonDiscountedItems(IEnumerable<OrderItem> allOrderItems)
        {
            var discountedItemIds = LineDiscounts.Select(ld => ld.Item.Id).ToHashSet();
            return allOrderItems.Where(item => !discountedItemIds.Contains(item.Id));
        }

        /// <summary>
        /// Calculates the total discount amount
        /// </summary>
        /// <returns>Total discount amount</returns>
        private Money CalculateTotalDiscount()
        {
            if (!LineDiscounts.Any())
                return Money.Zero(CurrencyCode);

            var total = LineDiscounts.Sum(ld => ld.DiscountAmount.Amount);
            return Money.Create(total, CurrencyCode);
        }
    }
}
