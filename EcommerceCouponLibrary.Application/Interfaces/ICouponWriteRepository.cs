using System;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Application.Interfaces
{
    public interface ICouponWriteRepository
    {
        Task<Coupon> CreateAsync(Coupon coupon);
        Task<bool> UpdateAsync(Coupon coupon);
        Task<bool> PauseAsync(Guid couponId);
        Task<bool> ResumeAsync(Guid couponId);
    }
}
