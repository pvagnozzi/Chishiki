// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chishiki.API.Web.IntegrationTests;

[TestFixture, Category("Integration")]
public class WeatherForecastApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetWeatherForecast_Returns200Ok()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetWeatherForecast_Returns5Forecasts()
    {
        var response = await _client.GetAsync("/weatherforecast");
        var body = await response.Content.ReadAsStringAsync();
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(body);

        Assert.That(forecasts, Has.Length.EqualTo(5));
    }

    [Test]
    public async Task GetWeatherForecast_TemperatureCIsWithinExpectedRange()
    {
        var response = await _client.GetAsync("/weatherforecast");
        var body = await response.Content.ReadAsStringAsync();
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(body, opts);

        Assert.That(forecasts, Is.Not.Null);
        foreach (var forecast in forecasts!)
        {
            var temp = forecast.GetProperty("temperatureC").GetInt32();
            Assert.That(temp, Is.InRange(-20, 55));
        }
    }

    [Test]
    public async Task GetWeatherForecast_TemperatureFIsConsistentWithTemperatureC()
    {
        var response = await _client.GetAsync("/weatherforecast");
        var body = await response.Content.ReadAsStringAsync();
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(body, opts);

        Assert.That(forecasts, Is.Not.Null);
        foreach (var forecast in forecasts!)
        {
            var c = forecast.GetProperty("temperatureC").GetInt32();
            var f = forecast.GetProperty("temperatureF").GetInt32();
            var expected = 32 + (int)(c / 0.5556);
            Assert.That(f, Is.EqualTo(expected));
        }
    }

    [Test]
    public async Task GetWeatherForecast_EachForecastHasDateInFuture()
    {
        var response = await _client.GetAsync("/weatherforecast");
        var body = await response.Content.ReadAsStringAsync();
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(body, opts);

        Assert.That(forecasts, Is.Not.Null);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var forecast in forecasts!)
        {
            var date = DateOnly.Parse(forecast.GetProperty("date").GetString()!);
            Assert.That(date, Is.GreaterThan(today));
        }
    }

    [Test]
    public async Task GetWeatherForecast_ContentTypeIsApplicationJson()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.That(
            response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo("application/json"));
    }
}
