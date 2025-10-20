using FluentAssertions;
using System.Globalization;
using Transwextions.App;

namespace Transwextions.Tests;

[TestFixture]
public class HelpersTests
{
    private CultureInfo _originalCulture;

    [SetUp]
    public void SetUp()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
    }

    [TearDown]
    public void TearDown()
    {
        if (_originalCulture is not null)
            CultureInfo.CurrentCulture = _originalCulture;
    }

    [Test]
    public void ConvertCentsToDecimal_Success_ExpectedValues()
    {
        ulong centsSmall = 199;
        ulong centsExact = 12345;
        ulong centsZero = 0;

        // Act
        decimal amountSmall = Helpers.ConvertCentsToDecimal(centsSmall);
        decimal amountExact = Helpers.ConvertCentsToDecimal(centsExact);
        decimal amountZero = Helpers.ConvertCentsToDecimal(centsZero);

        // Assert
        amountSmall.Should().Be(1.99m);
        amountExact.Should().Be(123.45m);
        amountZero.Should().Be(0m);
    }

    [Test]
    public void ConvertDecimalToCurrencyString_FormatsWithDollarSignAndCommasCorrectlyWithExpectedValue()
    {
        // Arrange
        decimal amount = 1234.56m;

        // Act
        string formatted = Helpers.ConvertDecimalToCurrencyString(amount);

        // Assert
        formatted.Should().Be("$1,234.56");
    }

    [Test]
    public void ConvertDecimalToCents_RoundsAwayFromZeroToNearestCent()
    {
        // Arrange
        decimal amountRoundDown = 1.234m;
        decimal amountRoundUp = 1.235m;
        decimal amountExact = 1.23m;
        decimal amountZero = 0m;

        // Act
        ulong centsDown = Helpers.ConvertDecimalToCents(amountRoundDown);
        ulong centsUp = Helpers.ConvertDecimalToCents(amountRoundUp);
        ulong centsExact = Helpers.ConvertDecimalToCents(amountExact);
        ulong centsZero = Helpers.ConvertDecimalToCents(amountZero);

        // Assert
        centsDown.Should().Be(123UL);
        centsUp.Should().Be(124UL);
        centsExact.Should().Be(123UL);
        centsZero.Should().Be(0UL);
    }

    [Test]
    public void ConvertCentsToCurrencyString_FormatsCorrectlyWithExpectedValue()
    {
        // Arrange
        ulong cents = 123456;

        // Act
        string formatted = Helpers.ConvertCentsToCurrencyString(cents);

        // Assert
        formatted.Should().Be("$1,234.56");
    }

    [Test]
    public void ConverTotalCentsToDeciamlUsingExchangeRate_ConvertAndRoundToTwoDecimalsWithExpectedValues()
    {
        // Arrange
        ulong totalCentsA = 10_000;
        decimal rateA = 1.2345m;

        ulong totalCentsB = 199;
        decimal rateB = 0.5m;

        // Act
        decimal usdA = Helpers.ConverTotalCentsToDeciamlUsingExchangeRate(totalCentsA, rateA);
        decimal usdB = Helpers.ConverTotalCentsToDeciamlUsingExchangeRate(totalCentsB, rateB);

        // Assert
        usdA.Should().Be(123.45m);
        usdB.Should().Be(1.00m);
    }
}