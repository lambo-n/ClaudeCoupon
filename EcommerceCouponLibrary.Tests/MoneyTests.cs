using System;
using System.Globalization;
using EcommerceCouponLibrary.Core.Models;
using Xunit;

namespace EcommerceCouponLibrary.Tests
{
    public class MoneyTests
    {
        [Fact]
        public void Create_ValidAmountAndCurrency_CreatesSuccessfully()
        {
            // Act
            var money = new Money(100.50m, "USD");

            // Assert
            Assert.Equal(100.50m, money.Amount);
            Assert.Equal("USD", money.CurrencyCode);
            Assert.Equal(2, money.DecimalPlaces);
        }

        [Fact]
        public void Create_EmptyCurrencyCode_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Money(100.50m, ""));
            Assert.Throws<ArgumentException>(() => new Money(100.50m, null!));
        }

        [Fact]
        public void USD_StaticFactory_CreatesUSDMoney()
        {
            // Act
            var money = Money.USD(100.50m);

            // Assert
            Assert.Equal(100.50m, money.Amount);
            Assert.Equal("USD", money.CurrencyCode);
        }

        [Fact]
        public void EUR_StaticFactory_CreatesEURMoney()
        {
            // Act
            var money = Money.EUR(100.50m);

            // Assert
            Assert.Equal(100.50m, money.Amount);
            Assert.Equal("EUR", money.CurrencyCode);
        }

        [Fact]
        public void JPY_StaticFactory_CreatesJPYMoney()
        {
            // Act
            var money = Money.JPY(1000m);

            // Assert
            Assert.Equal(1000m, money.Amount);
            Assert.Equal("JPY", money.CurrencyCode);
            Assert.Equal(0, money.DecimalPlaces);
        }

        [Fact]
        public void Zero_StaticFactory_CreatesZeroMoney()
        {
            // Act
            var money = Money.Zero("USD");

            // Assert
            Assert.Equal(0m, money.Amount);
            Assert.Equal("USD", money.CurrencyCode);
            Assert.True(money.IsZero);
        }

        [Fact]
        public void Addition_SameCurrency_AddsCorrectly()
        {
            // Arrange
            var money1 = Money.USD(100.50m);
            var money2 = Money.USD(25.25m);

            // Act
            var result = money1 + money2;

            // Assert
            Assert.Equal(125.75m, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Addition_DifferentCurrencies_ThrowsInvalidOperationException()
        {
            // Arrange
            var usd = Money.USD(100.50m);
            var eur = Money.EUR(25.25m);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => usd + eur);
        }

        [Fact]
        public void Subtraction_SameCurrency_SubtractsCorrectly()
        {
            // Arrange
            var money1 = Money.USD(100.50m);
            var money2 = Money.USD(25.25m);

            // Act
            var result = money1 - money2;

            // Assert
            Assert.Equal(75.25m, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Multiplication_ByDecimal_MultipliesCorrectly()
        {
            // Arrange
            var money = Money.USD(100.00m);
            var factor = 0.10m;

            // Act
            var result = money * factor;

            // Assert
            Assert.Equal(10.00m, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Division_ByDecimal_DividesCorrectly()
        {
            // Arrange
            var money = Money.USD(100.00m);
            var divisor = 4m;

            // Act
            var result = money / divisor;

            // Assert
            Assert.Equal(25.00m, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Division_ByZero_ThrowsDivideByZeroException()
        {
            // Arrange
            var money = Money.USD(100.00m);

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() => money / 0m);
        }

        [Fact]
        public void Comparison_SameCurrency_ComparesCorrectly()
        {
            // Arrange
            var money1 = Money.USD(100.00m);
            var money2 = Money.USD(50.00m);
            var money3 = Money.USD(100.00m);

            // Assert
            Assert.True(money1 > money2);
            Assert.True(money2 < money1);
            Assert.True(money1 >= money3);
            Assert.True(money1 <= money3);
            Assert.True(money1 == money3);
            Assert.False(money1 != money3);
        }

        [Fact]
        public void Comparison_DifferentCurrencies_ThrowsInvalidOperationException()
        {
            // Arrange
            var usd = Money.USD(100.00m);
            var eur = Money.EUR(100.00m);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => usd > eur);
            Assert.Throws<InvalidOperationException>(() => usd < eur);
            Assert.Throws<InvalidOperationException>(() => usd >= eur);
            Assert.Throws<InvalidOperationException>(() => usd <= eur);
        }

        [Fact]
        public void Min_SameCurrency_ReturnsMinimum()
        {
            // Arrange
            var money1 = Money.USD(100.00m);
            var money2 = Money.USD(50.00m);

            // Act
            var result = Money.Min(money1, money2);

            // Assert
            Assert.Equal(money2, result);
        }

        [Fact]
        public void Max_SameCurrency_ReturnsMaximum()
        {
            // Arrange
            var money1 = Money.USD(100.00m);
            var money2 = Money.USD(50.00m);

            // Act
            var result = Money.Max(money1, money2);

            // Assert
            Assert.Equal(money1, result);
        }

        [Fact]
        public void Abs_NegativeAmount_ReturnsPositive()
        {
            // Arrange
            var money = new Money(-100.50m, "USD");

            // Act
            var result = money.Abs();

            // Assert
            Assert.Equal(100.50m, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void IsZero_ZeroAmount_ReturnsTrue()
        {
            // Arrange
            var money = Money.Zero("USD");

            // Assert
            Assert.True(money.IsZero);
            Assert.False(money.IsPositive);
            Assert.False(money.IsNegative);
        }

        [Fact]
        public void IsPositive_PositiveAmount_ReturnsTrue()
        {
            // Arrange
            var money = Money.USD(100.50m);

            // Assert
            Assert.True(money.IsPositive);
            Assert.False(money.IsZero);
            Assert.False(money.IsNegative);
        }

        [Fact]
        public void IsNegative_NegativeAmount_ReturnsTrue()
        {
            // Arrange
            var money = new Money(-100.50m, "USD");

            // Assert
            Assert.True(money.IsNegative);
            Assert.False(money.IsZero);
            Assert.False(money.IsPositive);
        }

        [Fact]
        public void Rounding_USD_BankersRounding()
        {
            // Test banker's rounding (round to even)
            Assert.Equal(10.00m, new Money(10.004m, "USD").Amount);
            Assert.Equal(10.00m, new Money(10.005m, "USD").Amount); // 10.005 rounds to 10.00 (banker's rounding)
            Assert.Equal(10.01m, new Money(10.006m, "USD").Amount);
        }

        [Fact]
        public void Rounding_JPY_NoDecimalPlaces()
        {
            // JPY should have no decimal places
            Assert.Equal(1000m, new Money(1000.5m, "JPY").Amount);
            Assert.Equal(1001m, new Money(1000.6m, "JPY").Amount);
        }

        [Fact]
        public void ToString_DefaultFormat_FormatsCorrectly()
        {
            // Arrange
            var money = Money.USD(100.50m);

            // Act
            var result = money.ToString();

            // Assert
            Assert.Equal("100.50 USD", result);
        }

        [Fact]
        public void ToString_WithCulture_FormatsCorrectly()
        {
            // Arrange
            var money = Money.USD(100.50m);
            var culture = new CultureInfo("en-US");

            // Act
            var result = money.ToString(culture);

            // Assert
            Assert.Equal("100.50 USD", result);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            // Arrange
            var money1 = Money.USD(100.50m);
            var money2 = Money.USD(100.50m);

            // Assert
            Assert.True(money1.Equals(money2));
            Assert.True(money1 == money2);
            Assert.False(money1 != money2);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            // Arrange
            var money1 = Money.USD(100.50m);
            var money2 = Money.USD(100.51m);

            // Assert
            Assert.False(money1.Equals(money2));
            Assert.False(money1 == money2);
            Assert.True(money1 != money2);
        }

        [Fact]
        public void GetHashCode_SameValues_SameHashCode()
        {
            // Arrange
            var money1 = Money.USD(100.50m);
            var money2 = Money.USD(100.50m);

            // Assert
            Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
        }

        [Fact]
        public void CompareTo_SameCurrency_ComparesCorrectly()
        {
            // Arrange
            var money1 = Money.USD(100.00m);
            var money2 = Money.USD(50.00m);
            var money3 = Money.USD(100.00m);

            // Assert
            Assert.True(money1.CompareTo(money2) > 0);
            Assert.True(money2.CompareTo(money1) < 0);
            Assert.Equal(0, money1.CompareTo(money3));
        }

        [Fact]
        public void CompareTo_DifferentCurrencies_ThrowsInvalidOperationException()
        {
            // Arrange
            var usd = Money.USD(100.00m);
            var eur = Money.EUR(100.00m);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => usd.CompareTo(eur));
        }

        [Theory]
        [InlineData("USD", 2)]
        [InlineData("EUR", 2)]
        [InlineData("GBP", 2)]
        [InlineData("JPY", 0)]
        [InlineData("CAD", 2)]
        [InlineData("AUD", 2)]
        [InlineData("UNKNOWN", 2)] // Default case
        public void DecimalPlaces_DifferentCurrencies_ReturnsCorrectPlaces(string currencyCode, int expectedPlaces)
        {
            // Arrange
            var money = new Money(100.50m, currencyCode);

            // Assert
            Assert.Equal(expectedPlaces, money.DecimalPlaces);
        }
    }
}
