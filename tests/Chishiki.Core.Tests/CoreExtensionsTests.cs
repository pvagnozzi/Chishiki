// -----------------------------------------------------------------------------
// File:        CoreExtensionsTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers focused unit tests for the Chishiki.Core extension and helper surface.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Security.Claims;
using System.Security.Cryptography;
using Chishiki.Configuration;
using Chishiki.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Chishiki.Core.Tests;

/// <summary>Provides focused tests for core extensions and helpers.</summary>
[TestFixture]
public sealed class CoreExtensionsTests
{
    [Test]
    public void CapitalizeWhenValueIsNullThrowsArgumentNullException()
    {
        string? input = null;

        Assert.That(() => input!.Capitalize(), Throws.ArgumentNullException);
    }

    [Test]
    public void CapitalizeWhenValueIsEmptyThrowsArgumentException()
    {
        Assert.That(() => string.Empty.Capitalize(), Throws.ArgumentException);
    }

    [Test]
    public void CapitalizeWhenValueStartsLowercaseReturnsCapitalizedText()
    {
        var result = "chishiki".Capitalize();

        Assert.That(result, Is.EqualTo("Chishiki"));
    }

    [Test]
    public void ToGuidWhenInputIsInvalidReturnsEmptyGuid()
    {
        var result = "not-a-guid".ToGuid();

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void ToCorrelationIdWhenInputIsInvalidReturnsNonEmptyGuid()
    {
        var result = "not-a-guid".ToCorrelationId();

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void ToClaimDictionaryReturnsClaimValuesByType()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Name, "Ada")
        ]));

        var result = principal.ToClaimDictionary();

        Assert.That(result[ClaimTypes.NameIdentifier], Is.EqualTo("42"));
        Assert.That(result[ClaimTypes.Name], Is.EqualTo("Ada"));
    }

    [Test]
    public void AddMissingWhenClaimTypeAlreadyExistsReturnsOriginalSequence()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "Ada") };

        var result = claims.AddMissing(new Claim(ClaimTypes.Name, "Grace"));

        Assert.That(result, Is.SameAs(claims));
        Assert.That(claims, Has.Count.EqualTo(1));
    }

    [Test]
    public void AddMissingRangeAddsOnlyClaimsWithNewTypes()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "Ada") };

        var result = claims.AddMissingRange(
        [
            new Claim(ClaimTypes.Name, "Grace"),
            new Claim(ClaimTypes.Role, "Admin")
        ]);

        Assert.That(result, Is.SameAs(claims));
        Assert.That(result.Select(c => c.Type), Is.EqualTo(new[] { ClaimTypes.Name, ClaimTypes.Role }));
    }

    [Test]
    public void GetSectionBindsTypedOptionsFromConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sample:Name"] = "Ada",
                ["Sample:Enabled"] = "true"
            })
            .Build();

        var result = configuration.GetSection<SampleOptions>("Sample");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Ada"));
        Assert.That(result.Enabled, Is.True);
    }

    [Test]
    public void GetSectionWhenSectionIsMissingThrowsInvalidOperationException()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        Assert.That(() => configuration.GetSection<SampleOptions>("Missing"), Throws.InvalidOperationException);
    }

    [Test]
    public void GetConfigurationBuildsConfiguredInstanceFromServiceProvider()
    {
        var services = new ServiceCollection();
        _ = services.Configure<SampleOptions>(options =>
        {
            options.Name = "Configured";
            options.Enabled = true;
        });

        using var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.GetConfiguration<SampleOptions>();

        Assert.That(result.Name, Is.EqualTo("Configured"));
        Assert.That(result.Enabled, Is.True);
    }

    [Test]
    public void PagedListMapPreservesMetadataAndTransformsItems()
    {
        IPagedList<int> source = new PagedList<int>([1, 2], totalCount: 5, pageIndex: 1, pageSize: 2);

        var result = source.Map(value => $"#{value}");

        Assert.That(result.Items, Is.EqualTo(new[] { "#1", "#2" }));
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageIndex, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(3));
    }

    [Test]
    public void ReflectionHelpersClassifyTypesCorrectly()
    {
        Assert.That(typeof(List<int>).Implements<IEnumerable<int>>(), Is.True);
        Assert.That(typeof(bool?).IsBoolean(), Is.True);
        Assert.That(typeof(decimal).IsNumeric(), Is.True);
        Assert.That(typeof(DateTimeOffset?).IsDateTime(), Is.True);
        Assert.That(typeof(byte[]).IsBinary(), Is.True);
        Assert.That(typeof(string).IsScalar(), Is.True);
        Assert.That(typeof(List<int>).IsEnumerable(), Is.True);
    }

    [Test]
    public void InvokeStaticMethodInvokesMatchingMethod()
    {
        StaticInvocationTarget.LastValue = null;

        typeof(StaticInvocationTarget).InvokeStaticMethod(nameof(StaticInvocationTarget.SetValue), typeof(string), "done");

        Assert.That(StaticInvocationTarget.LastValue, Is.EqualTo("done"));
    }

    [Test]
    public void InvokeStaticMethodWhenMethodIsMissingThrowsInvalidCastException()
    {
        Assert.That(
            () => typeof(StaticInvocationTarget).InvokeStaticMethod("Missing", typeof(string), "value"),
            Throws.TypeOf<InvalidCastException>());
    }

    [Test]
    public void EncryptAndDecryptRoundTripReturnsOriginalText()
    {
        const string key = "1234567890ABCDEF";
        const string text = "secret";

        var encrypted = text.Encrypt(key);
        var decrypted = encrypted!.Decrypt(key);

        Assert.That(encrypted, Is.Not.Null.And.Not.EqualTo(text));
        Assert.That(decrypted, Is.EqualTo(text));
    }

    [Test]
    public void EncryptWhenTextIsWhitespaceReturnsNullAndDecryptReturnsEmptyString()
    {
        const string key = "1234567890ABCDEF";

        var encrypted = "   ".Encrypt(key);
        var decrypted = string.Empty.Decrypt(key);

        Assert.That(encrypted, Is.Null);
        Assert.That(decrypted, Is.Empty);
    }

    [Test]
    public void EncryptWhenKeyIsInvalidLengthThrowsCryptographicException()
    {
        Assert.That(() => "secret".Encrypt("short"), Throws.TypeOf<CryptographicException>());
    }

    private sealed class SampleOptions
    {
        public string Name { get; set; } = string.Empty;

        public bool Enabled { get; set; }
    }

    private static class StaticInvocationTarget
    {
        public static string? LastValue { get; set; }

        public static void SetValue(string value) => LastValue = value;
    }
}
