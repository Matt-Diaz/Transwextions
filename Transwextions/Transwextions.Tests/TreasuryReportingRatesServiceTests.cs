using FluentAssertions;
using RichardSzalay.MockHttp;
using System.Text.Json;
using Transwextions.App.Services;
using Transwextions.Data.Constants;
using Transwextions.Data.Payloads;

namespace Transwextions.Tests;

[TestFixture]
public class TreasuryReportingRatesServiceTests
{
    private static string Serialize(object o) => JsonSerializer.Serialize(o, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    [Test]
    public async Task GetCurrenciesAsync_Success_MultiplePages()
    {
        // Arrange
        var response1 = new TreasuryReportingResponse
        {
            Data = new()
            {
                new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Dollar",
                },
                 new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Dollar"
                },
            },
            Links = new() { Next = "&page%5Bnumber%5D=2" }
        };

        var response2 = new TreasuryReportingResponse
        {
            Data = new()
            {
                 new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Peso",
                    ExchangeRate = 17.5m,
                    RecordDate = new DateOnly(2025, 06, 30)
                }
            },
            Links = new() { Next = null }
        };

        // Mock
        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;
        var mock = new MockHttpMessageHandler();

        // Response 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(response1));

        // Response 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=2") == true)
            .Respond("application/json", Serialize(response2));

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Object.Should().HaveCount(2);
        result.Object.Should().Equal("Dollar", "Peso");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_NoCurrencies()
    {
        // Arrange
        var response1 = new TreasuryReportingResponse
        {
            Data = null,
            Links = new() { Next = "&page%5Bnumber%5D=2" }
        };

        // Mock
        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;
        var mock = new MockHttpMessageHandler();

        // Repsonse 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(response1));


        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Object.Should().BeNull();
        result.ErrorMessage.Should().Contain("response contained no data");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_CouldNotDeserialize()
    {
        // Mock
        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;
        var mock = new MockHttpMessageHandler();
        var badJson = "{ this is : not valid json ";

        // First response with bad JSON
        mock.Expect($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D") == false)
            .Respond("application/json", badJson);

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Object.Should().BeNull();
        result.ErrorMessage.Should().Contain("Failed to deserialize Treasury response");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_CancelRequest()
    {
        // Mock
        var mock = new MockHttpMessageHandler();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync(cts.Token);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Operation canceled");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_PageLoopLimit()
    {
        // Arrange
        var response1 = new TreasuryReportingResponse
        {
            Data = new()
            {
                new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Dollar"
                },
                new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Peso"
                }
            },
            Links = new() { Next = "&page%5Bnumber%5D=1" }
        };

        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;

        // Mock
        var mock = new MockHttpMessageHandler();

        // Response 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(response1));

        // Response 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=1") == true)
            .Respond("application/json", Serialize(response1));

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Object.Should().BeNull();
        result.ErrorMessage.Should().Contain("Exceeded maximum page loop limit");
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Success_MultiplePages()
    {
        // Arrange
        var response1 = new TreasuryReportingResponse
        {
            Data = new()
            {
                new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Dollar",
                    ExchangeRate = 1.0m,
                    RecordDate = new DateOnly(2025, 06, 30)
                },
                 new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Other",
                    ExchangeRate = 2.0m,
                    RecordDate = new DateOnly(2025, 06, 28)
                },
            },
            Links = new() { Next = "&page%5Bnumber%5D=2" }
        };

        var response2 = new TreasuryReportingResponse
        {
            Data = new()
            {
                 new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Peso",
                    ExchangeRate = 17.5m,
                    RecordDate = new DateOnly(2025, 06, 30)
                }
            },
            Links = new() { Next = null }
        };

        var minDate = new DateTime(2025, 04, 01);
        var maxDate = new DateTime(2025, 10, 01);

        var baseUrl = string.Format(
            TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint,
            minDate.Date.ToString("yyyy-MM-dd"),
            maxDate.Date.ToString("yyyy-MM-dd"));

        // Mock
        var mock = new MockHttpMessageHandler();

        // Response 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(response1));

        // Response 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=2") == true)
            .Respond("application/json", Serialize(response2));


        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Object.Should().HaveCount(3);
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Failure_MinDateCannotBeGreaterThanMaxDate()
    {
        // Arrange
        var maxDate = new DateTime(2025, 10, 01);

        // Min Date is greater than Max Date.
        var minDate = maxDate.Date.AddMonths(1);

        var baseUrl = string.Format(
            TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint,
            minDate.Date.ToString("yyyy-MM-dd"),
            maxDate.Date.ToString("yyyy-MM-dd"));

        // Mock
        var mock = new MockHttpMessageHandler();

        // Response
        mock.Expect($"{baseUrl}")
            .Respond("application/json", string.Empty);

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("MinDate cannot be greater than MaxDate");
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Failure_PageLoopLimitExceeded()
    {
        // Arrange
        var response1 = new TreasuryReportingResponse
        {
            Data = new()
            {
                new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Dollar",
                    ExchangeRate = 1.0m,
                    RecordDate = new DateOnly(2025, 06, 30)
                },
                 new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Other",
                    ExchangeRate = 2.0m,
                    RecordDate = new DateOnly(2025, 06, 28)
                },
            },
            Links = new() { Next = "&page%5Bnumber%5D=2" }
        };

        var responseLoop = new TreasuryReportingResponse
        {
            Data = new()
            {
                 new TreasuryReportingResponse.RateOfExchangeObject
                {
                    Currency = "Peso",
                    ExchangeRate = 17.5m,
                    RecordDate = new DateOnly(2025, 06, 30)
                }
            },
            Links = new() { Next = "&page%5Bnumber%5D=2" }
        };

        var minDate = new DateTime(2025, 04, 01);
        var maxDate = new DateTime(2025, 10, 01);

        var baseUrl = string.Format(
            TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint,
            minDate.Date.ToString("yyyy-MM-dd"),
            maxDate.Date.ToString("yyyy-MM-dd"));

        // Mock
        var mock = new MockHttpMessageHandler();

        // Response 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(response1));

        // Response Loop
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=2") == true)
            .Respond("application/json", Serialize(responseLoop));

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Exceeded maximum page loop limit");
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Failure_CouldNotDeserialize()
    {
        // Arrange
        var minDate = new DateTime(2025, 04, 01);
        var maxDate = new DateTime(2025, 10, 01);

        var baseUrl = string.Format(
            TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint,
            minDate.Date.ToString("yyyy-MM-dd"),
            maxDate.Date.ToString("yyyy-MM-dd"));

        // Mock
        var mock = new MockHttpMessageHandler();
        var badJson = "{ this is : not valid json ";

        // Mock Bad Json
        mock.Expect($"{baseUrl}")
            .Respond("application/json", badJson);

        // Act
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to deserialize Treasury response");
    }
}