namespace EcommerceCouponLibrary.Domain.Specifications
{
    /// <summary>
    /// Base interface for coupon specifications using the specification pattern
    /// </summary>
    /// <typeparam name="T">The type of entity to validate</typeparam>
    public interface ICouponSpecification<in T>
    {
        /// <summary>
        /// Determines if the specification is satisfied by the given entity
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>True if the specification is satisfied</returns>
        bool IsSatisfiedBy(T entity);

        /// <summary>
        /// Gets the error message if the specification is not satisfied
        /// </summary>
        /// <param name="entity">The entity that failed validation</param>
        /// <returns>The error message</returns>
        string GetErrorMessage(T entity);

        /// <summary>
        /// Gets the rejection reason for this specification
        /// </summary>
        Enums.CouponRejectionReason RejectionReason { get; }
    }
}
