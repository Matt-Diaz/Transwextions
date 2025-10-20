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
        // page 1
        var p1 = new TreasuryReportingResponse
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

        // page 2 (duplicate currency)
        var p2 = new TreasuryReportingResponse
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

        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;

        var mock = new MockHttpMessageHandler();

        // Expect Page 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(p1));

        // Expect Page 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=2") == true)
            .Respond("application/json", Serialize(p2));

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        var result = await svc.GetCurrenciesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Object.Should().HaveCount(2);
        result.Object.Should().Equal("Dollar", "Peso");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_NoCurrencies()
    {
        // page 1
        var p1 = new TreasuryReportingResponse
        {
            Data = null,
            Links = new() { Next = "&page%5Bnumber%5D=2" }
        };

        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;

        var mock = new MockHttpMessageHandler();

        // Expect Page 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(p1));

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync();

        result.IsSuccess.Should().BeFalse();
        result.Object.Should().BeNull();
        result.ErrorMessage.Should().Contain("response contained no data");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_CouldNotDeserialize()
    {
        var baseUrl = TreasuryReportingRatesAPIEndpoints.GetAllCurrenciesEndpoint;

        var mock = new MockHttpMessageHandler();

        var badJson = "{ this is : not valid json ";

        // First response with bad JSON
        mock.Expect($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D") == false)
            .Respond("application/json", badJson);

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);
        var result = await svc.GetCurrenciesAsync();

        result.IsSuccess.Should().BeFalse();
        result.Object.Should().BeNull();
        result.ErrorMessage.Should().Contain("Failed to deserialize Treasury response");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_CancelRequest()
    {
        var mock = new MockHttpMessageHandler();
        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await svc.GetCurrenciesAsync(cts.Token);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Operation canceled");
    }

    [Test]
    public async Task GetCurrenciesAsync_Failure_PageLoopLimit()
    {
        // page 1
        var p1 = new TreasuryReportingResponse
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

        var mock = new MockHttpMessageHandler();

        // Page 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(p1));

        // Page 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=1") == true)
            .Respond("application/json", Serialize(p1));

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        var result = await svc.GetCurrenciesAsync();

        result.IsSuccess.Should().BeFalse();
        result.Object.Should().BeNull();
        result.ErrorMessage.Should().Contain("Exceeded maximum page loop limit");
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Success_MultiplePages()
    {
        // page 1
        var p1 = new TreasuryReportingResponse
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

        // page 2
        var p2 = new TreasuryReportingResponse
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

        var mock = new MockHttpMessageHandler();

        // Expect Page 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(p1));

        // Expect Page 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=2") == true)
            .Respond("application/json", Serialize(p2));

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        result.IsSuccess.Should().BeTrue();
        result.Object.Should().HaveCount(3);
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Failure_MinDateCannotBeGreaterThanMaxDate()
    {
        var maxDate = new DateTime(2025, 10, 01);

        // Min Date is greater than Max Date.
        var minDate = maxDate.Date.AddMonths(1);

        var baseUrl = string.Format(
            TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint,
            minDate.Date.ToString("yyyy-MM-dd"),
            maxDate.Date.ToString("yyyy-MM-dd"));

        var mock = new MockHttpMessageHandler();

        // Expect Page 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", string.Empty);

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("MinDate cannot be greater than MaxDate");
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Failure_PageLoopLimitExceeded()
    {
        // page 1
        var p1 = new TreasuryReportingResponse
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

        // page 2
        var p2 = new TreasuryReportingResponse
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

        var mock = new MockHttpMessageHandler();

        // Expect Page 1
        mock.Expect($"{baseUrl}")
            .Respond("application/json", Serialize(p1));

        // Expect Page 2
        mock.When($"{baseUrl}")
            .With(req => req.RequestUri!.Query.Contains("page%5Bnumber%5D=2") == true)
            .Respond("application/json", Serialize(p2));

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Exceeded maximum page loop limit");
    }

    [Test]
    public async Task GetExchangeRatesByDateRangeAsync_Failure_CouldNotDeserialize()
    {
        var minDate = new DateTime(2025, 04, 01);
        var maxDate = new DateTime(2025, 10, 01);

        var baseUrl = string.Format(
            TreasuryReportingRatesAPIEndpoints.GetAllRatesWithinDateRangeEndpoint,
            minDate.Date.ToString("yyyy-MM-dd"),
            maxDate.Date.ToString("yyyy-MM-dd"));

        var mock = new MockHttpMessageHandler();
        var badJson = "{ this is : not valid json ";

        // Expect Bad Json.
        mock.Expect($"{baseUrl}")
            .Respond("application/json", badJson);

        var http = new HttpClient(mock);
        var svc = new TreasuryReportingRatesService(http);

        var result = await svc.GetExchangeRatesByDateRangeAsync(minDate, maxDate);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to deserialize Treasury response");
    }
}