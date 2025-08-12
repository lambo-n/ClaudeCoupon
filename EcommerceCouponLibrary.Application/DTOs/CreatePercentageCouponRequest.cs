using System;

namespace EcommerceCouponLibrary.Application.DTOs
{
    public class CreatePercentageCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCombinable { get; set; }
    }
}
