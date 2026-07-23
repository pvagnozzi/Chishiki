// -----------------------------------------------------------------------------
// File:        CoreInfrastructureTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers disposable, reflection-constructor, resource, dictionary, and exception helpers in Chishiki.Core.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Configuration;
using Chishiki.Exceptions;
using Chishiki.Logging;
using Chishiki.Reflection;
using Chishiki.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Core.Tests;

/// <summary>Provides focused tests for core infrastructure helpers and base classes.</summary>
[TestFixture]
public sealed class CoreInfrastructureTests
{
    [Test]
    public void DictionaryGetValueReturnsStoredValueOrProvidedDefault()
    {
        IDictionary<string, string> dictionary = new Dictionary<string, string>
        {
            ["present"] = "value",
        };

        Assert.Multiple(() =>
        {
            Assert.That(dictionary.GetValue("present", "fallback"), Is.EqualTo("value"));
            Assert.That(dictionary.GetValue("missing", "fallback"), Is.EqualTo("fallback"));
            Assert.That(dictionary.GetValue("missing"), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void ClaimHelpersCreateDictionaryAndAddMissingClaimTypes()
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.Name, "Ada"),
        };

        var added = claims.AddMissing(new(System.Security.Claims.ClaimTypes.Role, "Admin"));
        var dictionary = added.ToDictionary();

        Assert.Multiple(() =>
        {
            Assert.That(added, Is.Not.SameAs(claims));
            Assert.That(dictionary[System.Security.Claims.ClaimTypes.Name], Is.EqualTo("Ada"));
            Assert.That(dictionary[System.Security.Claims.ClaimTypes.Role], Is.EqualTo("Admin"));
        });
    }

    [Test]
    public void StreamExtensionsRoundTripTextThroughBytesAndMemoryStream()
    {
        var bytes = "hello".AsBytes(System.Text.Encoding.UTF8);
        using var stream = new MemoryStream(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(bytes.AsString(System.Text.Encoding.UTF8), Is.EqualTo("hello"));
            Assert.That(stream.AsString(System.Text.Encoding.UTF8), Is.EqualTo("hello"));
        });
    }

    [Test]
    public void DomainAndConfigurationExceptionsPreserveSuppliedState()
    {
        var correlationId = Guid.NewGuid();
        var innerException = new InvalidOperationException("boom");

        var domainException = new DomainException("domain failure", correlationId, innerException);
        var notFoundException = new NotFoundDomainException("missing", correlationId, innerException);
        var configurationException = new ConfigurationException("configuration failure", innerException);

        Assert.Multiple(() =>
        {
            Assert.That(domainException.Message, Is.EqualTo("domain failure"));
            Assert.That(domainException.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(domainException.InnerException, Is.SameAs(innerException));
            Assert.That(notFoundException.Message, Is.EqualTo("missing"));
            Assert.That(notFoundException.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(notFoundException.InnerException, Is.SameAs(innerException));
            Assert.That(configurationException.Message, Is.EqualTo("configuration failure"));
            Assert.That(configurationException.InnerException, Is.SameAs(innerException));
        });
    }

    [Test]
    public void DeepCopyCreatesIndependentClone()
    {
        var original = new DeepCopyModel
        {
            Name = "Ada",
            Tags = ["math", "logic"],
        };

        var copy = original.DeepCopy();
        copy.Tags.Add("poetry");

        Assert.Multiple(() =>
        {
            Assert.That(copy, Is.Not.SameAs(original));
            Assert.That(copy.Name, Is.EqualTo(original.Name));
            Assert.That(original.Tags, Is.EqualTo(["math", "logic"]));
            Assert.That(copy.Tags, Is.EqualTo(["math", "logic", "poetry"]));
        });
    }

    [Test]
    public void LoggableUsesSuppliedLoggerOrCreatesOneFromFactory()
    {
        var suppliedLogger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var factoryLogger = Substitute.For<ILogger>();
#pragma warning disable CA2263 // Prefer generic overload - Testing non-generic overload behavior is the purpose of this test
        _ = loggerFactory.CreateLogger(typeof(TestLoggable)).Returns(factoryLogger);
#pragma warning restore CA2263

        var withLogger = new TestLoggable(suppliedLogger, loggerFactory);
        var withFactory = new TestLoggable(null, loggerFactory);
        var withoutAnything = new TestLoggable();

        Assert.Multiple(() =>
        {
            Assert.That(withLogger.Logger, Is.SameAs(suppliedLogger));
            Assert.That(withFactory.Logger, Is.SameAs(factoryLogger));
            Assert.That(withoutAnything.Logger, Is.SameAs(NullLogger.Instance));
        });

#pragma warning disable CA2263 // Prefer generic overload - Testing non-generic overload behavior is the purpose of this test
        _ = loggerFactory.Received(1).CreateLogger(typeof(TestLoggable));
#pragma warning restore CA2263
    }

    [Test]
    public async Task AsyncDisposableDisposeAsyncInvokesManagedCleanupOnceAndSetsDisposedState()
    {
        var sut = new TestAsyncDisposable();

        await sut.DisposeAsync();
        await sut.DisposeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(sut.ManagedDisposeCallCount, Is.EqualTo(1));
            Assert.That(sut.IsDisposedForTest, Is.True);
            Assert.That(() => sut.EnsureNotDisposed(), Throws.TypeOf<ObjectDisposedException>());
        });
    }

    [Test]
    public Task AsyncDisposableDisposeAsyncSwallowsManagedCleanupExceptionsAndMarksDisposed()
    {
        var sut = new TestAsyncDisposable(throwOnManagedDispose: true);

        Assert.DoesNotThrowAsync(async () => await sut.DisposeAsync());

        Assert.Multiple(() =>
        {
            Assert.That(sut.ManagedDisposeCallCount, Is.EqualTo(1));
            Assert.That(sut.IsDisposedForTest, Is.True);
        });
        return Task.CompletedTask;
    }

    [Test]
    public async Task AsyncDisposableDisposeCoreFalseInvokesUnmanagedCleanupPath()
    {
        var sut = new TestAsyncDisposable();

        await sut.InvokeDisposeCoreAsync(disposing: false);

        Assert.Multiple(() =>
        {
            Assert.That(sut.UnmanagedDisposeCallCount, Is.EqualTo(1));
            Assert.That(sut.ManagedDisposeCallCount, Is.EqualTo(0));
            Assert.That(sut.IsDisposedForTest, Is.True);
        });
    }

    [Test]
    public async Task DefaultAsyncDisposableDisposeCoreCoversBaseManagedAndUnmanagedImplementations()
    {
        var managed = new DefaultAsyncDisposable();
        await managed.InvokeDisposeCoreAsync(disposing: true);

        var unmanaged = new DefaultAsyncDisposable();
        await unmanaged.InvokeDisposeCoreAsync(disposing: false);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var canceled = new DefaultAsyncDisposable();
        await canceled.InvokeDisposeCoreAsync(disposing: true, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(managed.IsDisposedForTest, Is.True);
            Assert.That(unmanaged.IsDisposedForTest, Is.True);
            Assert.That(canceled.IsDisposedForTest, Is.True);
        });
    }

    [Test]
    public void DisposableDisposeInvokesManagedCleanupOnceAndSetsDisposedState()
    {
        var sut = new TestDisposable();

        ((IDisposable)sut).Dispose();
        ((IDisposable)sut).Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(sut.ManagedDisposeCallCount, Is.EqualTo(1));
            Assert.That(sut.DisposedValueForTest, Is.True);
            Assert.That(() => sut.EnsureNotDisposed(), Throws.TypeOf<ObjectDisposedException>());
        });
    }

    [Test]
    public void DisposableDisposeSwallowsManagedCleanupExceptionsAndMarksDisposed()
    {
        var sut = new TestDisposable(throwOnManagedDispose: true);

        Assert.DoesNotThrow(() => ((IDisposable)sut).Dispose());

        Assert.Multiple(() =>
        {
            Assert.That(sut.ManagedDisposeCallCount, Is.EqualTo(1));
            Assert.That(sut.DisposedValueForTest, Is.True);
        });
    }

    [Test]
    public void DisposableDisposeCoreFalseInvokesUnmanagedCleanupPath()
    {
        var sut = new TestDisposable();

        sut.InvokeDisposeCore(disposing: false);

        Assert.Multiple(() =>
        {
            Assert.That(sut.UnmanagedDisposeCallCount, Is.EqualTo(1));
            Assert.That(sut.ManagedDisposeCallCount, Is.EqualTo(0));
            Assert.That(sut.DisposedValueForTest, Is.True);
        });
    }

    [Test]
    public void DefaultDisposableDisposeCoreCoversBaseManagedAndUnmanagedImplementations()
    {
        var managed = new DefaultDisposable();
        managed.InvokeDisposeCore(disposing: true);

        var unmanaged = new DefaultDisposable();
        unmanaged.InvokeDisposeCore(disposing: false);

        Assert.Multiple(() =>
        {
            Assert.That(managed.DisposedValueForTest, Is.True);
            Assert.That(unmanaged.DisposedValueForTest, Is.True);
        });
    }

    [Test]
    public void ServiceImplementsServiceContractAndExposesLogger()
    {
        IService service = new TestService();

        Assert.Multiple(() =>
        {
            Assert.That(service, Is.InstanceOf<IAsyncDisposable>());
            Assert.That(service.Logger, Is.Not.Null);
        });
    }

    [Test]
    public void ConstructorHelpersCreateInstancesForArityOneToFive()
    {
        var one = new Constructor<string, OneArgModel>().Create("Ada");
        var two = new Constructor<string, int, TwoArgModel>().Create("Ada", 42);
        var three = new Constructor<string, int, bool, ThreeArgModel>().Create("Ada", 42, true);
        var four = new Constructor<string, int, bool, double, FourArgModel>().Create("Ada", 42, true, 3.14d);
        var five = new Constructor<string, int, bool, double, long, FiveArgModel>().Create("Ada", 42, true, 3.14d, 7L);

        Assert.Multiple(() =>
        {
            Assert.That(one.Name, Is.EqualTo("Ada"));
            Assert.That(two.Age, Is.EqualTo(42));
            Assert.That(three.Enabled, Is.True);
            Assert.That(four.Score, Is.EqualTo(3.14d));
            Assert.That(five.Count, Is.EqualTo(7L));
        });
    }

    [Test]
    public void ConstructorHelpersThrowWhenMatchingConstructorIsMissing()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => new Constructor<string, MissingOneArgModel>(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => new Constructor<string, int, MissingTwoArgModel>(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => new Constructor<string, int, bool, MissingThreeArgModel>(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => new Constructor<string, int, bool, double, MissingFourArgModel>(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => new Constructor<string, int, bool, double, long, MissingFiveArgModel>(), Throws.TypeOf<InvalidOperationException>());
        });
    }

    [Test]
    public void ReflectionGetTypesImplementingReturnsMatchingTypes()
    {
        var assembly = typeof(CoreInfrastructureTests).Assembly;

        var genericMatches = assembly.GetTypesImplementing<ITestContract>();
#pragma warning disable CA2263 // Prefer generic overload - Testing non-generic overload behavior is the purpose of this test
        var runtimeMatches = assembly.GetTypesImplementing(typeof(ITestContract));
#pragma warning restore CA2263

        Assert.Multiple(() =>
        {
            Assert.That(genericMatches, Does.Contain(typeof(TestContractImplementation)));
            Assert.That(runtimeMatches, Does.Contain(typeof(TestContractImplementation)));
            Assert.That(typeof(TestContractImplementation).Implements(typeof(ITestContract)), Is.True);
        });
    }

    [Test]
    public void ResourceExtensionsListResourcesAndReadEmbeddedResourceText()
    {
        var assembly = typeof(CoreInfrastructureTests).Assembly;
        var expectedResourceName = "Chishiki.Core.Tests.Resources.SampleResource.txt";

        var resources = assembly.ListResources("Chishiki.Core.Tests.Resources");
        var content = assembly.GetResourceText(expectedResourceName);

        Assert.Multiple(() =>
        {
            Assert.That(resources, Does.Contain(expectedResourceName));
            Assert.That(content.Trim(), Is.EqualTo("sample-resource-content"));
        });
    }

    [Test]
    public void ResourceExtensionsGetResourceTextWhenMissingThrows()
    {
        var assembly = typeof(CoreInfrastructureTests).Assembly;

        Assert.That(
            () => assembly.GetResourceText("missing.resource"),
            Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void PrettyPrintJsonFormatsCompactJson()
    {
#pragma warning disable JSON002 // Rilevata probabile stringa JSON
        var result = "{\"name\":\"Ada\",\"age\":42}".PrettyPrintJson();
#pragma warning restore JSON002 // Rilevata probabile stringa JSON

        Assert.That(result, Does.Contain(Environment.NewLine));
        Assert.That(result, Does.Contain("  \"name\": \"Ada\""));
    }

    [Test]
    [TestCase("hello", ConsoleColor.Cyan)]
    [TestCase("green", ConsoleColor.Green)]
    [TestCase("red", ConsoleColor.Red)]
    [TestCase("yellow", ConsoleColor.Yellow)]
    public void ColoredConsoleWritersWriteExpectedText(string text, ConsoleColor color)
    {
        using var writer = new StringWriter();
        var originalOut = Console.Out;
        var originalColor = Console.ForegroundColor;

        try
        {
            Console.SetOut(writer);

            switch (color)
            {
                case ConsoleColor.Green:
                    text.GreenWriteLine();
                    break;
                case ConsoleColor.Red:
                    text.RedWriteLine();
                    break;
                case ConsoleColor.Yellow:
                    text.YellowWriteLine();
                    break;
                case ConsoleColor.Black:
                    break;
                case ConsoleColor.DarkBlue:
                    break;
                case ConsoleColor.DarkGreen:
                    break;
                case ConsoleColor.DarkCyan:
                    break;
                case ConsoleColor.DarkRed:
                    break;
                case ConsoleColor.DarkMagenta:
                    break;
                case ConsoleColor.DarkYellow:
                    break;
                case ConsoleColor.Gray:
                    break;
                case ConsoleColor.DarkGray:
                    break;
                case ConsoleColor.Blue:
                    break;
                case ConsoleColor.Cyan:
                    break;
                case ConsoleColor.Magenta:
                    break;
                case ConsoleColor.White:
                    break;
                default:
                    text.ColoredWriteLine(color);
                    break;
            }
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.ForegroundColor = originalColor;
        }

        Assert.That(writer.ToString(), Is.EqualTo($"{text}{Environment.NewLine}"));
    }

    private sealed class DeepCopyModel
    {
        public string Name { get; init; } = string.Empty;

        public List<string> Tags { get; init; } = [];
    }

    private sealed class TestLoggable(ILogger? logger = null, ILoggerFactory? loggerFactory = null) : Loggable(logger, loggerFactory);

    private sealed class TestService(ILogger? logger = null, ILoggerFactory? loggerFactory = null) : Service(logger, loggerFactory);

    private sealed class TestAsyncDisposable(bool throwOnManagedDispose = false) : AsyncDisposable()
    {
        public int ManagedDisposeCallCount { get; private set; }

        public int UnmanagedDisposeCallCount { get; private set; }

        public bool IsDisposedForTest => IsDisposed;

        public void EnsureNotDisposed() => CheckDisposed();

        public ValueTask InvokeDisposeCoreAsync(bool disposing, CancellationToken cancellationToken = default) => DisposeAsync(disposing, cancellationToken);

        protected override ValueTask DisposeManagedAsync(CancellationToken cancellationToken = default)
        {
            ManagedDisposeCallCount++;
            cancellationToken.ThrowIfCancellationRequested();
            return throwOnManagedDispose ? throw new InvalidOperationException("managed failure") : ValueTask.CompletedTask;
        }

        protected override void DisposeUnmanaged() => UnmanagedDisposeCallCount++;
    }

    private sealed class TestDisposable(bool throwOnManagedDispose = false) : Disposable()
    {
        public int ManagedDisposeCallCount { get; private set; }

        public int UnmanagedDisposeCallCount { get; private set; }

        public bool DisposedValueForTest => DisposedValue;

        public void EnsureNotDisposed() => CheckDisposed();

        public void InvokeDisposeCore(bool disposing) => Dispose(disposing);

        protected override void DisposeManaged()
        {
            ManagedDisposeCallCount++;
            if (throwOnManagedDispose)
            {
                throw new InvalidOperationException("managed failure");
            }
        }

        protected override void DisposeUnmanaged() => UnmanagedDisposeCallCount++;
    }

    private sealed class DefaultAsyncDisposable() : AsyncDisposable
    {
        public bool IsDisposedForTest => IsDisposed;

        public ValueTask InvokeDisposeCoreAsync(bool disposing, CancellationToken cancellationToken = default) => DisposeAsync(disposing, cancellationToken);
    }

    private sealed class DefaultDisposable() : Disposable
    {
        public bool DisposedValueForTest => DisposedValue;

        public void InvokeDisposeCore(bool disposing) => Dispose(disposing);
    }

    private interface ITestContract;

    private sealed class TestContractImplementation : ITestContract;

    private sealed class OneArgModel(string name)
    {
        public string Name { get; } = name;
    }

    private sealed class TwoArgModel(string name, int age)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;
    }

    private sealed class ThreeArgModel(string name, int age, bool enabled)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;

        public bool Enabled { get; } = enabled;
    }

    private sealed class FourArgModel(string name, int age, bool enabled, double score)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;

        public bool Enabled { get; } = enabled;

        public double Score { get; } = score;
    }

    private sealed class FiveArgModel(string name, int age, bool enabled, double score, long count)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;

        public bool Enabled { get; } = enabled;

        public double Score { get; } = score;

        public long Count { get; } = count;
    }

    private sealed class MissingOneArgModel;

    private sealed class MissingTwoArgModel(string name)
    {
        public string Name { get; } = name;
    }

    private sealed class MissingThreeArgModel(string name, int age)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;
    }

    private sealed class MissingFourArgModel(string name, int age, bool enabled)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;

        public bool Enabled { get; } = enabled;
    }

    private sealed class MissingFiveArgModel(string name, int age, bool enabled, double score)
    {
        public string Name { get; } = name;

        public int Age { get; } = age;

        public bool Enabled { get; } = enabled;

        public double Score { get; } = score;
    }
}
