// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.API.Web.UnitTests;

[TestFixture, Category("Unit")]
public class WeatherForecastTests
{
    [Test]
    public void TemperatureF_At0Celsius_Returns32Fahrenheit()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 0, null);

        Assert.That(forecast.TemperatureF, Is.EqualTo(32));
    }

    [Test]
    public void TemperatureF_At100Celsius_ReturnsExpectedFahrenheit()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 100, "Hot");

        // 32 + (int)(100 / 0.5556) = 32 + 179 = 211
        Assert.That(forecast.TemperatureF, Is.EqualTo(211));
    }

    [Test]
    public void TemperatureF_At20Celsius_ReturnsExpectedFahrenheit()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 20, "Mild");

        // 32 + (int)(20 / 0.5556) = 32 + 35 = 67
        Assert.That(forecast.TemperatureF, Is.EqualTo(67));
    }

    [Test]
    public void TemperatureF_AtNegative10Celsius_ReturnsExpectedFahrenheit()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), -10, "Freezing");

        // 32 + (int)(-10 / 0.5556) = 32 + (-17) = 15
        Assert.That(forecast.TemperatureF, Is.EqualTo(15));
    }

    [Test]
    public void Summary_CanBeNull()
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 20, null);

        Assert.That(forecast.Summary, Is.Null);
    }

    [Test]
    public void RecordEquality_SameValues_AreEqual()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        var a = new WeatherForecast(date, 25, "Warm");
        var b = new WeatherForecast(date, 25, "Warm");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void RecordEquality_DifferentTemperature_AreNotEqual()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        var a = new WeatherForecast(date, 25, "Warm");
        var b = new WeatherForecast(date, 30, "Warm");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void RecordEquality_DifferentDate_AreNotEqual()
    {
        var a = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 20, "Mild");
        var b = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 20, "Mild");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void WithExpression_OverridesSelectedProperty()
    {
        var original = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 20, "Mild");
        var modified = original with { TemperatureC = 30 };

        Assert.Multiple(() =>
        {
            Assert.That(modified.TemperatureC, Is.EqualTo(30));
            Assert.That(modified.Date, Is.EqualTo(original.Date));
            Assert.That(modified.Summary, Is.EqualTo(original.Summary));
        });
    }
}
