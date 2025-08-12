using System;
using System.Threading.Tasks;
using EcommerceCouponLibrary.Application.DTOs;
using EcommerceCouponLibrary.Core.Models;

namespace EcommerceCouponLibrary.Application.Interfaces
{
    public interface ICouponManagementService
    {
        Task<Coupon> CreatePercentageCouponAsync(CreatePercentageCouponRequest request);
        Task<Coupon> CreateFixedAmountCouponAsync(CreateFixedAmountCouponRequest request);
        Task<bool> PauseCouponAsync(Guid couponId);
        Task<bool> ResumeCouponAsync(Guid couponId);
        Task<bool> UpdateScheduleAsync(Guid couponId, DateTime newStart, DateTime newEnd);
    }
}
