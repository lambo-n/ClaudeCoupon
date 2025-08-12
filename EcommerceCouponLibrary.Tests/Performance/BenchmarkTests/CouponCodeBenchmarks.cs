using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EcommerceCouponLibrary.Domain.ValueObjects;
using EcommerceCouponLibrary.Tests.Shared.TestFixtures;

namespace EcommerceCouponLibrary.Tests.Performance.BenchmarkTests
{
    /// <summary>
    /// Performance benchmarks for CouponCode operations
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class CouponCodeBenchmarks
    {
        private readonly string[] _testCodes = CouponTestFixture.GetValidCouponCodes().ToArray();

        [Benchmark]
        public void CouponCode_Create_ValidCode()
        {
            var couponCode = CouponCode.Create("SAVE20");
            _ = couponCode.Value;
        }

        [Benchmark]
        public void CouponCode_Create_WithNormalization()
        {
            var couponCode = CouponCode.Create("save20");
            _ = couponCode.Value;
        }

        [Benchmark]
        public void CouponCode_Equality_Comparison()
        {
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("SAVE20");
            _ = code1 == code2;
        }

        [Benchmark]
        public void CouponCode_GetHashCode()
        {
            var couponCode = CouponCode.Create("SAVE20");
            _ = couponCode.GetHashCode();
        }

        [Benchmark]
        public void CouponCode_ToString()
        {
            var couponCode = CouponCode.Create("SAVE20");
            _ = couponCode.ToString();
        }

        [Benchmark]
        public void CouponCode_ImplicitConversion()
        {
            var couponCode = CouponCode.Create("SAVE20");
            string result = couponCode;
            _ = result;
        }

        [Benchmark]
        public void CouponCode_BulkCreation()
        {
            var codes = new CouponCode[_testCodes.Length];
            for (int i = 0; i < _testCodes.Length; i++)
            {
                codes[i] = CouponCode.Create(_testCodes[i]);
            }
            _ = codes;
        }

        [Benchmark]
        public void CouponCode_BulkEqualityChecks()
        {
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("SAVE20");
            var code3 = CouponCode.Create("SAVE10");

            for (int i = 0; i < 1000; i++)
            {
                _ = code1 == code2;
                _ = code1 == code3;
                _ = code1.Equals(code2);
                _ = code1.Equals(code3);
            }
        }

        [Benchmark]
        public void CouponCode_BulkHashCodeGeneration()
        {
            var codes = _testCodes.Select(c => CouponCode.Create(c)).ToArray();
            var hashCodes = new int[codes.Length];

            for (int i = 0; i < codes.Length; i++)
            {
                hashCodes[i] = codes[i].GetHashCode();
            }
            _ = hashCodes;
        }

        [Benchmark]
        public void CouponCode_BulkToString()
        {
            var codes = _testCodes.Select(c => CouponCode.Create(c)).ToArray();
            var strings = new string[codes.Length];

            for (int i = 0; i < codes.Length; i++)
            {
                strings[i] = codes[i].ToString();
            }
            _ = strings;
        }

        [Benchmark]
        public void CouponCode_Validation_ValidCodes()
        {
            var validCodes = new[] { "SAVE20", "WELCOME", "HOLIDAY", "SUMMER", "WINTER" };
            var results = new bool[validCodes.Length];

            for (int i = 0; i < validCodes.Length; i++)
            {
                try
                {
                    var code = CouponCode.Create(validCodes[i]);
                    results[i] = true;
                }
                catch
                {
                    results[i] = false;
                }
            }
            _ = results;
        }

        [Benchmark]
        public void CouponCode_Validation_InvalidCodes()
        {
            var invalidCodes = new[] { "", "AB", "   ", "A".PadRight(51, 'A') };
            var results = new bool[invalidCodes.Length];

            for (int i = 0; i < invalidCodes.Length; i++)
            {
                try
                {
                    var code = CouponCode.Create(invalidCodes[i]);
                    results[i] = true;
                }
                catch
                {
                    results[i] = false;
                }
            }
            _ = results;
        }
    }

    /// <summary>
    /// Performance benchmarks for Money operations
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class MoneyBenchmarks
    {
        private readonly Money _usd100 = Money.USD(100.00m);
        private readonly Money _usd50 = Money.USD(50.00m);
        private readonly Money _eur75 = Money.EUR(75.00m);

        [Benchmark]
        public void Money_Create_USD()
        {
            var money = Money.USD(100.00m);
            _ = money.Amount;
        }

        [Benchmark]
        public void Money_Create_EUR()
        {
            var money = Money.EUR(85.50m);
            _ = money.Amount;
        }

        [Benchmark]
        public void Money_Create_JPY()
        {
            var money = Money.JPY(15000m);
            _ = money.Amount;
        }

        [Benchmark]
        public void Money_Addition_SameCurrency()
        {
            var result = _usd100 + _usd50;
            _ = result.Amount;
        }

        [Benchmark]
        public void Money_Subtraction_SameCurrency()
        {
            var result = _usd100 - _usd50;
            _ = result.Amount;
        }

        [Benchmark]
        public void Money_Multiplication()
        {
            var result = _usd100 * 0.2m;
            _ = result.Amount;
        }

        [Benchmark]
        public void Money_Division()
        {
            var result = _usd100 / 2m;
            _ = result.Amount;
        }

        [Benchmark]
        public void Money_Comparison()
        {
            _ = _usd100 > _usd50;
            _ = _usd100 < _usd50;
            _ = _usd100 == _usd50;
        }

        [Benchmark]
        public void Money_ToString()
        {
            _ = _usd100.ToString();
        }

        [Benchmark]
        public void Money_ToString_WithCulture()
        {
            _ = _usd100.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        [Benchmark]
        public void Money_IsZero()
        {
            _ = _usd100.IsZero;
        }

        [Benchmark]
        public void Money_IsPositive()
        {
            _ = _usd100.IsPositive;
        }

        [Benchmark]
        public void Money_IsNegative()
        {
            _ = _usd100.IsNegative;
        }

        [Benchmark]
        public void Money_Abs()
        {
            var negative = Money.USD(-100.00m);
            var result = negative.Abs();
            _ = result.Amount;
        }

        [Benchmark]
        public void Money_Min()
        {
            var result = Money.Min(_usd100, _usd50);
            _ = result.Amount;
        }

        [Benchmark]
        public void Money_Max()
        {
            var result = Money.Max(_usd100, _usd50);
            _ = result.Amount;
        }
    }

    /// <summary>
    /// Performance benchmarks for DateRange operations
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class DateRangeBenchmarks
    {
        private readonly DateRange _activeRange = DateRange.FromNow(TimeSpan.FromDays(30));
        private readonly DateRange _expiredRange = DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1));
        private readonly DateRange _futureRange = DateRange.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(31));

        [Benchmark]
        public void DateRange_Create_FromDates()
        {
            var range = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
            _ = range.StartDate;
        }

        [Benchmark]
        public void DateRange_Create_FromNow()
        {
            var range = DateRange.FromNow(TimeSpan.FromDays(30));
            _ = range.StartDate;
        }

        [Benchmark]
        public void DateRange_Contains_CurrentDate()
        {
            _ = _activeRange.Contains(DateTime.UtcNow);
        }

        [Benchmark]
        public void DateRange_IsActive()
        {
            _ = _activeRange.IsActive();
        }

        [Benchmark]
        public void DateRange_HasStarted()
        {
            _ = _activeRange.HasStarted();
        }

        [Benchmark]
        public void DateRange_HasEnded()
        {
            _ = _expiredRange.HasEnded();
        }

        [Benchmark]
        public void DateRange_Duration()
        {
            _ = _activeRange.Duration;
        }

        [Benchmark]
        public void DateRange_TimeUntilStart()
        {
            _ = _futureRange.TimeUntilStart();
        }

        [Benchmark]
        public void DateRange_TimeUntilEnd()
        {
            _ = _activeRange.TimeUntilEnd();
        }

        [Benchmark]
        public void DateRange_ToString()
        {
            _ = _activeRange.ToString();
        }

        [Benchmark]
        public void DateRange_Equality()
        {
            var range1 = DateRange.FromNow(TimeSpan.FromDays(30));
            var range2 = DateRange.FromNow(TimeSpan.FromDays(30));
            _ = range1 == range2;
        }
    }
}
