using System;
using System.Globalization;

namespace EcommerceCouponLibrary.Core.Models
{
    /// <summary>
    /// Represents a monetary amount with currency and precise decimal handling.
    /// Implements banker's rounding for consistent financial calculations.
    /// </summary>
    public readonly struct Money : IEquatable<Money>, IComparable<Money>
    {
        private readonly decimal _amount;
        private readonly string _currencyCode;

        /// <summary>
        /// Gets the amount in the smallest currency unit (e.g., cents for USD)
        /// </summary>
        public decimal Amount => _amount;

        /// <summary>
        /// Gets the ISO 4217 currency code (e.g., "USD", "EUR", "JPY")
        /// </summary>
        public string CurrencyCode => _currencyCode;

        /// <summary>
        /// Gets the number of decimal places for this currency
        /// </summary>
        public int DecimalPlaces => GetDecimalPlaces(_currencyCode);

        /// <summary>
        /// Initializes a new instance of the Money struct
        /// </summary>
        /// <param name="amount">The monetary amount</param>
        /// <param name="currencyCode">The ISO 4217 currency code</param>
        public Money(decimal amount, string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentException("Currency code cannot be null or empty", nameof(currencyCode));

            _currencyCode = currencyCode.ToUpperInvariant();
            _amount = RoundToCurrencyPrecision(amount, _currencyCode);
        }

        /// <summary>
        /// Creates a Money instance from a decimal amount and currency code
        /// </summary>
        public static Money Create(decimal amount, string currencyCode) => new(amount, currencyCode);

        /// <summary>
        /// Creates a Money instance for USD
        /// </summary>
        public static Money USD(decimal amount) => new(amount, "USD");

        /// <summary>
        /// Creates a Money instance for EUR
        /// </summary>
        public static Money EUR(decimal amount) => new(amount, "EUR");

        /// <summary>
        /// Creates a Money instance for JPY
        /// </summary>
        public static Money JPY(decimal amount) => new(amount, "JPY");

        /// <summary>
        /// Zero amount in the specified currency
        /// </summary>
        public static Money Zero(string currencyCode) => new(0m, currencyCode);

        /// <summary>
        /// Adds two Money instances of the same currency
        /// </summary>
        public static Money operator +(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot add money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return new Money(left.Amount + right.Amount, left.CurrencyCode);
        }

        /// <summary>
        /// Subtracts two Money instances of the same currency
        /// </summary>
        public static Money operator -(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot subtract money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return new Money(left.Amount - right.Amount, left.CurrencyCode);
        }

        /// <summary>
        /// Multiplies Money by a decimal factor
        /// </summary>
        public static Money operator *(Money money, decimal factor)
        {
            return new Money(money.Amount * factor, money.CurrencyCode);
        }

        /// <summary>
        /// Multiplies a decimal factor by Money
        /// </summary>
        public static Money operator *(decimal factor, Money money)
        {
            return money * factor;
        }

        /// <summary>
        /// Divides Money by a decimal factor
        /// </summary>
        public static Money operator /(Money money, decimal factor)
        {
            if (factor == 0)
                throw new DivideByZeroException("Cannot divide Money by zero");

            return new Money(money.Amount / factor, money.CurrencyCode);
        }

        /// <summary>
        /// Compares two Money instances
        /// </summary>
        public static bool operator <(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return left.Amount < right.Amount;
        }

        /// <summary>
        /// Compares two Money instances
        /// </summary>
        public static bool operator >(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return left.Amount > right.Amount;
        }

        /// <summary>
        /// Compares two Money instances
        /// </summary>
        public static bool operator <=(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return left.Amount <= right.Amount;
        }

        /// <summary>
        /// Compares two Money instances
        /// </summary>
        public static bool operator >=(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return left.Amount >= right.Amount;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public static bool operator ==(Money left, Money right) => left.Equals(right);

        /// <summary>
        /// Inequality comparison
        /// </summary>
        public static bool operator !=(Money left, Money right) => !left.Equals(right);

        /// <summary>
        /// Returns the minimum of two Money instances
        /// </summary>
        public static Money Min(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return left.Amount <= right.Amount ? left : right;
        }

        /// <summary>
        /// Returns the maximum of two Money instances
        /// </summary>
        public static Money Max(Money left, Money right)
        {
            if (left.CurrencyCode != right.CurrencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {left.CurrencyCode} and {right.CurrencyCode}");

            return left.Amount >= right.Amount ? left : right;
        }

        /// <summary>
        /// Returns the absolute value of the Money amount
        /// </summary>
        public Money Abs() => new Money(Math.Abs(_amount), _currencyCode);

        /// <summary>
        /// Returns true if the amount is zero
        /// </summary>
        public bool IsZero => _amount == 0;

        /// <summary>
        /// Returns true if the amount is negative
        /// </summary>
        public bool IsNegative => _amount < 0;

        /// <summary>
        /// Returns true if the amount is positive
        /// </summary>
        public bool IsPositive => _amount > 0;

        /// <summary>
        /// Rounds the amount to the currency's precision using banker's rounding
        /// </summary>
        private static decimal RoundToCurrencyPrecision(decimal amount, string currencyCode)
        {
            int decimalPlaces = GetDecimalPlaces(currencyCode);
            return Math.Round(amount, decimalPlaces, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Gets the number of decimal places for a currency
        /// </summary>
        private static int GetDecimalPlaces(string currencyCode)
        {
            switch (currencyCode)
            {
                case "JPY": return 0;  // Japanese Yen has no decimal places
                case "USD": return 2;  // US Dollar has 2 decimal places
                case "EUR": return 2;  // Euro has 2 decimal places
                case "GBP": return 2;  // British Pound has 2 decimal places
                case "CAD": return 2;  // Canadian Dollar has 2 decimal places
                case "AUD": return 2;  // Australian Dollar has 2 decimal places
                default: return 2;     // Default to 2 decimal places for unknown currencies
            }
        }

        /// <summary>
        /// Formats the Money as a string
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0:F" + DecimalPlaces + "} {1}", _amount, _currencyCode);
        }

        /// <summary>
        /// Formats the Money as a string with culture-specific formatting
        /// </summary>
        public string ToString(CultureInfo culture)
        {
            return string.Format(culture, "{0:F" + DecimalPlaces + "} {1}", _amount, _currencyCode);
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public bool Equals(Money other)
        {
            return _amount == other._amount && _currencyCode == other._currencyCode;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Money other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        public override int GetHashCode()
        {
            return _amount.GetHashCode() ^ _currencyCode.GetHashCode();
        }

        /// <summary>
        /// Compares this Money instance to another
        /// </summary>
        public int CompareTo(Money other)
        {
            if (_currencyCode != other._currencyCode)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {_currencyCode} and {other._currencyCode}");

            return _amount.CompareTo(other._amount);
        }
    }
}
