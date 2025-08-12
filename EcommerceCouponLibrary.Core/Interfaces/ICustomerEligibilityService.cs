using System.Threading.Tasks;

namespace EcommerceCouponLibrary.Core.Interfaces
{
    public interface ICustomerEligibilityService
    {
        Task<bool> IsFirstOrderAsync(string customerId);
        Task<bool> IsInAllowedGroupsAsync(string customerId, string[] allowedGroups);
    }
}
