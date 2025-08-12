using System;
using Microsoft.EntityFrameworkCore;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Providers.EfCore
{
    public class CouponDbContext : DbContext
    {
        public CouponDbContext(DbContextOptions<CouponDbContext> options) : base(options) { }

        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Code).IsRequired().HasMaxLength(64);
                b.Property(c => c.Name).IsRequired().HasMaxLength(128);
                b.HasIndex(c => c.Code).IsUnique();
            });

            modelBuilder.Entity<CouponUsage>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => new { u.CouponId, u.CustomerId });
            });
        }
    }

    public class CouponUsage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CouponId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
