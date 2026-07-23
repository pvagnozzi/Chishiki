// -----------------------------------------------------------------------------
// File:        SerialConnectionReflectionTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers internal SerialConnection configuration helpers and deterministic failure paths.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.IO.Ports;
using System.Reflection;
using Chishiki.Serial.Abstractions;
using Chishiki.Serial.Common;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Chishiki.Serial.Common.Tests;

/// <summary>Provides reflection-based tests for internal SerialConnection helpers and deterministic closed-port behavior.</summary>
public sealed class SerialConnectionReflectionTests
{
    [Test]
    public void GetAvailablePortsReturnsNonNullCollection()
    {
        var factory = new SerialConnectionFactory(NullLoggerFactory.Instance);

        var ports = factory.GetAvailablePorts();

        Assert.That(ports, Is.Not.Null);
    }

    [Test]
    public void CreateAndConfigureSerialPortMapsOptionsToSystemSerialPort()
    {
        var options = new SerialConnectionOptions
        {
            PortName = "COM42",
            BaudRate = 57600,
            DataBits = 7,
            Parity = SerialParity.Even,
            StopBits = SerialStopBits.Two,
            Handshake = SerialHandshake.RequestToSend,
            ReadTimeout = 123,
            WriteTimeout = 456,
            DtrEnable = true,
            RtsEnable = true,
            NewLine = "\r\n",
            TextEncoding = System.Text.Encoding.ASCII,
        };

        using var serialPort = InvokeCreateAndConfigureSerialPort("COM42", options);

        Assert.Multiple(() =>
        {
            Assert.That(serialPort.PortName, Is.EqualTo("COM42"));
            Assert.That(serialPort.BaudRate, Is.EqualTo(57600));
            Assert.That(serialPort.DataBits, Is.EqualTo(7));
            Assert.That(serialPort.Parity, Is.EqualTo(Parity.Even));
            Assert.That(serialPort.StopBits, Is.EqualTo(StopBits.Two));
            Assert.That(serialPort.Handshake, Is.EqualTo(Handshake.RequestToSend));
            Assert.That(serialPort.ReadTimeout, Is.EqualTo(123));
            Assert.That(serialPort.WriteTimeout, Is.EqualTo(456));
            Assert.That(serialPort.DtrEnable, Is.True);
            Assert.That(serialPort.RtsEnable, Is.True);
            Assert.That(serialPort.NewLine, Is.EqualTo("\r\n"));
            Assert.That(serialPort.Encoding.WebName, Is.EqualTo(System.Text.Encoding.ASCII.WebName));
        });
    }

    [Test]
    public void ConstructorWithNullArgumentsThrowsArgumentNullException()
    {
        var constructor = GetInternalConstructor();
        var validOptions = new SerialConnectionOptions { PortName = "COM42" };
        var validLogger = NullLogger<SerialConnection>.Instance;

        Assert.Multiple(() =>
        {
            Assert.That(
                () => constructor.Invoke([null!, validOptions, validLogger]),
                Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ArgumentNullException>());
            Assert.That(
                () => constructor.Invoke(["COM42", null!, validLogger]),
                Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ArgumentNullException>());
            Assert.That(
                () => constructor.Invoke(["COM42", validOptions, null!]),
                Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ArgumentNullException>());
        });
    }

    [Test]
    public async Task ClosedPortOperationsThrowConsistentlyForReadWriteOpenAndFlushPaths()
    {
        using var connection = CreateConnection();
        var buffer = new byte[4];

        Assert.Multiple(() =>
        {
            Assert.That(() => connection.Open(), Throws.Exception);
            Assert.That(() => connection.Close(), Throws.Nothing);
            Assert.That(() => connection.Read(buffer, 0, buffer.Length), Throws.Exception);
            Assert.That(() => connection.ReadExistingText(), Throws.Exception);
            Assert.That(() => connection.ReadLine(), Throws.Exception);
            Assert.That(() => connection.Write(buffer, 0, buffer.Length), Throws.Exception);
            Assert.That(() => connection.WriteText("hello"), Throws.Exception);
            Assert.That(() => connection.WriteLine("hello"), Throws.Exception);
            Assert.That(() => connection.Flush(), Throws.Exception);
            Assert.That(() => connection.DiscardInBuffer(), Throws.Exception);
            Assert.That(() => connection.DiscardOutBuffer(), Throws.Exception);
            Assert.That(() => connection.CancelPendingOperations(), Throws.Exception);
        });

        Assert.Multiple(async () =>
        {
            Assert.That(async () => await connection.OpenAsync(), Throws.Exception);
            Assert.That(async () => await connection.CloseAsync(), Throws.Nothing);
            Assert.That(async () => await connection.ReadAsync(buffer, 0, buffer.Length), Throws.Exception);
            Assert.That(async () => await connection.ReadExistingTextAsync(), Throws.Exception);
            Assert.That(async () => await connection.ReadLineAsync(), Throws.Exception);
            Assert.That(async () => await connection.WriteAsync(buffer, 0, buffer.Length), Throws.Exception);
            Assert.That(async () => await connection.WriteTextAsync("hello"), Throws.Exception);
            Assert.That(async () => await connection.WriteLineAsync("hello"), Throws.Exception);
            Assert.That(async () => await connection.FlushAsync(), Throws.Exception);
        });
    }

    [Test]
    public void NullBuffersAndTextThrowArgumentNullExceptionBeforePortAccess()
    {
        using var connection = CreateConnection();

        Assert.Multiple(() =>
        {
            Assert.That(() => connection.Read(null!, 0, 1), Throws.ArgumentNullException);
            Assert.That(() => connection.Write(null!, 0, 1), Throws.ArgumentNullException);
            Assert.That(() => connection.WriteText(null!), Throws.ArgumentNullException);
            Assert.That(() => connection.WriteLine(null!), Throws.ArgumentNullException);
        });
    }

    private static SerialConnection CreateConnection()
    {
        var constructor = GetInternalConstructor();
        var options = new SerialConnectionOptions { PortName = "COM42" };
        return (SerialConnection)constructor.Invoke(["COM42", options, NullLogger<SerialConnection>.Instance]);
    }

    private static ConstructorInfo GetInternalConstructor() =>
        typeof(SerialConnection).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(string), typeof(SerialConnectionOptions), typeof(Microsoft.Extensions.Logging.ILogger<SerialConnection>)],
            modifiers: null) ?? throw new InvalidOperationException("SerialConnection internal constructor not found.");

    private static SerialPort InvokeCreateAndConfigureSerialPort(string portName, SerialConnectionOptions options)
    {
        var method = typeof(SerialConnection).GetMethod(
            "CreateAndConfigureSerialPort",
            BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException("CreateAndConfigureSerialPort method not found.");

        return (SerialPort)method.Invoke(null, [portName, options])!;
    }
}
