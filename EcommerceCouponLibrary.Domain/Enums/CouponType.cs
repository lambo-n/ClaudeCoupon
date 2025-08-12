namespace EcommerceCouponLibrary.Domain.Enums
{
    /// <summary>
    /// Types of coupons supported by the system
    /// </summary>
    public enum CouponType
    {
        /// <summary>
        /// Percentage-based discount (e.g., 10% off)
        /// </summary>
        Percentage = 1,

        /// <summary>
        /// Fixed amount discount (e.g., $5 off)
        /// </summary>
        FixedAmount = 2,

        /// <summary>
        /// Free shipping discount
        /// </summary>
        FreeShipping = 3,

        /// <summary>
        /// Buy X Get Y discount
        /// </summary>
        BuyXGetY = 4,

        /// <summary>
        /// Spend X Save Y discount
        /// </summary>
        SpendXSaveY = 5,

        /// <summary>
        /// Gift with purchase
        /// </summary>
        GiftWithPurchase = 6
    }
}
