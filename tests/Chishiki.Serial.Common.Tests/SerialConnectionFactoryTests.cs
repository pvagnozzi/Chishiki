// -----------------------------------------------------------------------------
// File:        SerialConnectionFactoryTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers deterministic factory behavior for the shared serial-common library.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Serial.Abstractions;
using Chishiki.Serial.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Serial.Common.Tests;

/// <summary>Provides deterministic tests for <see cref="SerialConnectionFactory"/>.</summary>
public sealed class SerialConnectionFactoryTests
{
    [Test]
    public void CreateConnectionReturnsClosedConnectionWithSuppliedOptions()
    {
        var loggerFactory = CreateLoggerFactory();
        var options = new SerialConnectionOptions
        {
            PortName = "COM42",
            BaudRate = 115200,
            ReadTimeout = 250,
            WriteTimeout = 500,
            DtrEnable = true,
            RtsEnable = true,
            NewLine = "\r\n"
        };

        var factory = new SerialConnectionFactory(loggerFactory);

        using var connection = factory.CreateConnection(options.PortName, options);

        Assert.That(connection.Options, Is.SameAs(options));
        Assert.That(connection.IsOpen, Is.False);
        loggerFactory.Received(1).CreateLogger(Arg.Any<string>());
    }

    [Test]
    public void CreateConnectionWithUnsupportedParityThrows()
    {
        var factory = new SerialConnectionFactory(CreateLoggerFactory());
        var options = new SerialConnectionOptions
        {
            PortName = "COM42",
            Parity = (SerialParity)999
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => factory.CreateConnection(options.PortName, options));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void CreateConnectionWithUnsupportedStopBitsThrows()
    {
        var factory = new SerialConnectionFactory(CreateLoggerFactory());
        var options = new SerialConnectionOptions
        {
            PortName = "COM42",
            StopBits = (SerialStopBits)999
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => factory.CreateConnection(options.PortName, options));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void CreateConnectionWithUnsupportedHandshakeThrows()
    {
        var factory = new SerialConnectionFactory(CreateLoggerFactory());
        var options = new SerialConnectionOptions
        {
            PortName = "COM42",
            Handshake = (SerialHandshake)999
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => factory.CreateConnection(options.PortName, options));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void AddSerialCommonAcceptsAServiceCollection()
    {
        var services = new ServiceCollection();

        Assert.DoesNotThrow(() => services.AddSerialCommon());
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        return loggerFactory;
    }
}
