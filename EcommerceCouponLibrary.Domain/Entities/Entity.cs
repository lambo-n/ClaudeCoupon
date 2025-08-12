namespace EcommerceCouponLibrary.Domain.Entities
{
    /// <summary>
    /// Base class for all domain entities
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Date when the entity was last modified
        /// </summary>
        public DateTime ModifiedAt { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        protected Entity(Guid id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the modified timestamp
        /// </summary>
        protected void UpdateModifiedTimestamp()
        {
            ModifiedAt = DateTime.UtcNow;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Entity? left, Entity? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Entity? left, Entity? right)
        {
            return !Equals(left, right);
        }
    }
}
