namespace EcommerceCouponLibrary.Domain.Enums
{
    /// <summary>
    /// Defines what the discount applies to
    /// </summary>
    public enum DiscountScope
    {
        /// <summary>
        /// Discount applies only to merchandise (excludes shipping, taxes, etc.)
        /// </summary>
        MerchandiseOnly = 1,

        /// <summary>
        /// Discount applies to the entire order total
        /// </summary>
        OrderTotal = 2,

        /// <summary>
        /// Discount applies to shipping only
        /// </summary>
        ShippingOnly = 3,

        /// <summary>
        /// Discount applies to taxes only
        /// </summary>
        TaxesOnly = 4,

        /// <summary>
        /// Discount applies to merchandise and shipping
        /// </summary>
        MerchandiseAndShipping = 5,

        /// <summary>
        /// Discount applies to specific items only
        /// </summary>
        SpecificItems = 6
    }
}
