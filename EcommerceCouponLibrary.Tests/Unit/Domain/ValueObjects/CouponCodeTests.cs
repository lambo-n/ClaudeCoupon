using FluentAssertions;
using EcommerceCouponLibrary.Domain.ValueObjects;
using EcommerceCouponLibrary.Domain.Enums;
using EcommerceCouponLibrary.Tests.Shared.TestFixtures;
using EcommerceCouponLibrary.Tests.Shared.TestHelpers;

namespace EcommerceCouponLibrary.Tests.Unit.Domain.ValueObjects
{
    /// <summary>
    /// Unit tests for CouponCode value object
    /// </summary>
    public class CouponCodeTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_ValidCode_CreatesSuccessfully()
        {
            // Arrange
            var code = "SAVE20";

            // Act
            var couponCode = CouponCode.Create(code);

            // Assert
            couponCode.Value.Should().Be("SAVE20");
            couponCode.ToString().Should().Be("SAVE20");
        }

        [Theory]
        [InlineData("")]
        [InlineData("AB")]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_InvalidCode_ThrowsArgumentException(string invalidCode)
        {
            // Act & Assert
            Action act = () => CouponCode.Create(invalidCode);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [MemberData(nameof(GetValidCouponCodes))]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_ValidCodesFromFixture_CreateSuccessfully(string validCode)
        {
            // Act
            var couponCode = CouponCode.Create(validCode);

            // Assert
            couponCode.Value.Should().Be(validCode.ToUpperInvariant());
        }

        [Theory]
        [MemberData(nameof(GetInvalidCouponCodes))]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_InvalidCodesFromFixture_ThrowArgumentException(string invalidCode)
        {
            // Act & Assert
            Action act = () => CouponCode.Create(invalidCode);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("save20", "SAVE20")]
        [InlineData("Save20", "SAVE20")]
        [InlineData(" SAVE20 ", "SAVE20")]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_NormalizesInput_Correctly(string input, string expected)
        {
            // Act
            var couponCode = CouponCode.Create(input);

            // Assert
            couponCode.Value.Should().Be(expected);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_ImplicitConversionToString_WorksCorrectly()
        {
            // Arrange
            var couponCode = CouponCode.Create("SAVE20");

            // Act
            string result = couponCode;

            // Assert
            result.Should().Be("SAVE20");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_ExplicitConversionFromString_WorksCorrectly()
        {
            // Arrange
            var code = "SAVE20";

            // Act
            var couponCode = (CouponCode)code;

            // Assert
            couponCode.Value.Should().Be("SAVE20");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_Equality_SameValues_ReturnsTrue()
        {
            // Arrange
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("SAVE20");

            // Act & Assert
            code1.Should().Be(code2);
            (code1 == code2).Should().BeTrue();
            (code1 != code2).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_Equality_DifferentValues_ReturnsFalse()
        {
            // Arrange
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("SAVE10");

            // Act & Assert
            code1.Should().NotBe(code2);
            (code1 == code2).Should().BeFalse();
            (code1 != code2).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_Equality_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("save20");

            // Act & Assert
            code1.Should().Be(code2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_GetHashCode_SameValues_SameHashCode()
        {
            // Arrange
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("SAVE20");

            // Act & Assert
            code1.GetHashCode().Should().Be(code2.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_GetHashCode_DifferentValues_DifferentHashCode()
        {
            // Arrange
            var code1 = CouponCode.Create("SAVE20");
            var code2 = CouponCode.Create("SAVE10");

            // Act & Assert
            code1.GetHashCode().Should().NotBe(code2.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_Equals_Null_ReturnsFalse()
        {
            // Arrange
            var couponCode = CouponCode.Create("SAVE20");

            // Act & Assert
            couponCode.Equals(null).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var couponCode = CouponCode.Create("SAVE20");
            var differentObject = "SAVE20";

            // Act & Assert
            couponCode.Equals(differentObject).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_ToString_ReturnsNormalizedValue()
        {
            // Arrange
            var couponCode = CouponCode.Create("save20");

            // Act
            var result = couponCode.ToString();

            // Assert
            result.Should().Be("SAVE20");
        }

        [Theory]
        [InlineData("SAVE20", 6)]
        [InlineData("WELCOME", 7)]
        [InlineData("HOLIDAY2023", 11)]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_Length_ReturnsCorrectLength(string code, int expectedLength)
        {
            // Arrange
            var couponCode = CouponCode.Create(code);

            // Act & Assert
            couponCode.Value.Length.Should().Be(expectedLength);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_BuilderPattern_WorksCorrectly()
        {
            // Arrange
            var builder = CouponBuilder.CreateValidCoupon()
                .WithCode("CUSTOM")
                .WithName("Custom Coupon");

            // Act
            var (code, name, type, value, validity) = builder.Build();

            // Assert
            code.Value.Should().Be("CUSTOM");
            name.Should().Be("Custom Coupon");
            type.Should().Be(CouponType.Percentage);
            value.Should().Be(20.0m);
            validity.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Domain")]
        public void CouponCode_BuilderPattern_ChainedMethods_WorkCorrectly()
        {
            // Arrange & Act
            var (code, name, type, value, validity) = CouponBuilder
                .CreateValidCoupon()
                .WithCode("SPECIAL")
                .AsPercentage(25.0m)
                .AsHighValue()
                .Build();

            // Assert
            code.Value.Should().Be("SPECIAL");
            name.Should().Be("20% Off"); // Default name
            type.Should().Be(CouponType.Percentage);
            value.Should().Be(50.0m); // High value overrides percentage
        }

        public static IEnumerable<object[]> GetValidCouponCodes()
        {
            return CouponTestFixture.GetValidCouponCodes()
                .Select(code => new object[] { code });
        }

        public static IEnumerable<object[]> GetInvalidCouponCodes()
        {
            return CouponTestFixture.GetInvalidCouponCodes()
                .Where(code => code != null) // Filter out null for MemberData
                .Select(code => new object[] { code });
        }
    }
}
