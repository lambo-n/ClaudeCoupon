# Ecommerce Coupon Library

A C#/.NET library for handling coupon and discount logic in ecommerce applications. This library provides a robust foundation for applying, validating, and managing coupons with support for various discount types, eligibility rules, and usage limits.

## Features

### ✅ Implemented (U-001 - Enter a coupon at checkout)

- **Money Value Type**: Precise financial calculations with currency support and banker's rounding
- **Coupon Domain Model**: Comprehensive coupon representation with types, values, and metadata
- **Order Management**: Order and line item models with automatic total calculations
- **Coupon Application**: Apply percentage and fixed-amount coupons to orders
- **Validation**: Comprehensive validation including expiration, minimum orders, and usage limits
- **Case-Insensitive Codes**: Coupon codes are normalized for reliable user input
- **Detailed Results**: Rich result objects with line-level discount allocations
- **Thread-Safe**: Pure evaluation with no side effects

### 🚧 Planned Features

- Product/category targeting and exclusions
- Customer eligibility rules
- Coupon stacking and conflict resolution
- Usage tracking and limits
- Multi-currency support
- Extensible rule system

## Quick Start

### Installation

```bash
dotnet add package EcommerceCouponLibrary.Core
```

### Basic Usage

```csharp
using EcommerceCouponLibrary.Core.Models;
using EcommerceCouponLibrary.Core.Services;
using EcommerceCouponLibrary.Core.Repositories;

// Create repository and evaluator
var repository = new InMemoryCouponRepository();
var evaluator = new CouponEvaluator(repository);

// Create a coupon
var coupon = Coupon.CreatePercentageCoupon(
    "SAVE10",
    "10% Off",
    0.10m,
    DateTime.UtcNow.AddDays(-1),
    DateTime.UtcNow.AddDays(30)
);

// Add to repository
((InMemoryCouponRepository)repository).AddCoupon(coupon);

// Create an order
var order = new Order
{
    Id = Guid.NewGuid(),
    CustomerId = "customer-123",
    CurrencyCode = "USD"
};

// Add items
order.AddItem(new OrderItem
{
    ProductId = "prod-1",
    ProductName = "Test Product",
    Quantity = 2,
    UnitPrice = Money.USD(25.00m)
});

// Apply coupon
var result = await evaluator.ApplyCouponAsync(order, "SAVE10", "customer-123");

if (result.IsSuccess)
{
    Console.WriteLine($"Coupon applied! You saved {result.DiscountAmount}");
    Console.WriteLine($"Final total: {result.OrderTotals.FinalTotal}");
}
else
{
    Console.WriteLine($"Coupon failed: {result.Message}");
}
```

## Core Components

### Money Value Type

The `Money` struct provides precise financial calculations with currency awareness:

```csharp
var amount = Money.USD(100.50m);
var discount = amount * 0.10m; // 10% discount
var total = amount - discount;
```

Features:
- Currency-specific decimal places (JPY = 0, USD = 2)
- Banker's rounding for consistent calculations
- Type-safe operations (prevents currency mixing)
- Immutable value type

### Coupon Model

```csharp
public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public CouponType Type { get; set; } // Percentage or FixedAmount
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public Money? MinimumOrderAmount { get; set; }
    public Money? MaximumDiscountAmount { get; set; }
    // ... more properties
}
```

### Order Model

```csharp
public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; }
    public string CurrencyCode { get; set; }
    public List<OrderItem> Items { get; set; }
    public List<AppliedCoupon> AppliedCoupons { get; set; }
    
    // Calculated properties
    public Money Subtotal { get; }
    public Money TotalDiscount { get; }
    public Money Total { get; }
}
```

## U-001 Implementation Details

The **U-001: Enter a coupon at checkout** user story is fully implemented with the following acceptance criteria:

### ✅ Given a valid, eligible code, when I apply it, then my order total updates and shows the discount

- Coupon validation checks all eligibility criteria
- Discount calculation handles both percentage and fixed amounts
- Order totals are automatically updated
- Line-level discount allocations are provided

### ✅ Given an invalid or expired code, when I apply it, then I see a clear message explaining why it can't be used

- Comprehensive validation with specific error reasons
- Human-readable error messages
- Support for various rejection scenarios:
  - Invalid/not found codes
  - Expired coupons
  - Not yet active coupons
  - Inactive coupons
  - Below minimum order amounts
  - Currency mismatches
  - Usage limits exceeded

## Validation Rules

The library validates coupons against these criteria:

1. **Existence**: Coupon code must exist in the repository
2. **Active Status**: Coupon must be marked as active
3. **Date Range**: Current time must be within start/end dates
4. **Currency Compatibility**: Fixed-amount coupons must match order currency
5. **Minimum Order**: Order subtotal must meet minimum requirement
6. **Usage Limits**: Global and per-customer limits are respected
7. **Unique Codes**: Single-use codes cannot be reused

## Error Handling

The library provides detailed error information through the `CouponApplicationResult`:

```csharp
public class CouponApplicationResult
{
    public bool IsSuccess { get; set; }
    public Coupon? Coupon { get; set; }
    public Money DiscountAmount { get; set; }
    public CouponRejectionReason? RejectionReason { get; set; }
    public string Message { get; set; }
    public OrderTotals? OrderTotals { get; set; }
    public List<LineDiscount> LineDiscounts { get; set; }
}
```

## Testing

Run the test suite:

```bash
dotnet test
```

The test suite includes comprehensive coverage for:
- Money value type operations and rounding
- Coupon application scenarios
- Validation rules
- Error conditions
- Edge cases

## Architecture

The library follows clean architecture principles:

- **Domain Models**: Core business entities (Money, Coupon, Order)
- **Interfaces**: Abstract contracts for dependencies
- **Services**: Business logic implementation
- **Repositories**: Data access abstractions
- **Results**: Rich result objects with detailed information

### Dependency Injection

The library is designed for dependency injection:

```csharp
services.AddScoped<ICouponRepository, YourCouponRepository>();
services.AddScoped<ICouponEvaluator, CouponEvaluator>();
```

## Contributing

This library is built according to the requirements in `Coupon-Library-Requirements-Combined.md`. When implementing new features:

1. Follow the existing patterns and conventions
2. Add comprehensive tests
3. Update documentation
4. Ensure thread safety and determinism

## License

[Add your license here]
