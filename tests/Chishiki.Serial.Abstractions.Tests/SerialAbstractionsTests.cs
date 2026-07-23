// -----------------------------------------------------------------------------
// File:        SerialAbstractionsTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers focused tests for the Chishiki.Serial.Abstractions contracts and DTOs.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Text;
using Chishiki.Serial.Abstractions;
using NUnit.Framework;

namespace Chishiki.Serial.Abstractions.Tests;

/// <summary>Provides focused tests for serial abstraction data carriers.</summary>
[TestFixture]
public sealed class SerialAbstractionsTests
{
    [Test]
    public void SerialConnectionOptionsExposeExpectedDefaults()
    {
        var options = new SerialConnectionOptions { PortName = "COM1" };

        Assert.That(options.PortName, Is.EqualTo("COM1"));
        Assert.That(options.BaudRate, Is.EqualTo(9600));
        Assert.That(options.Parity, Is.EqualTo(SerialParity.None));
        Assert.That(options.DataBits, Is.EqualTo(8));
        Assert.That(options.StopBits, Is.EqualTo(SerialStopBits.One));
        Assert.That(options.Handshake, Is.EqualTo(SerialHandshake.None));
        Assert.That(options.ReadTimeout, Is.EqualTo(1000));
        Assert.That(options.WriteTimeout, Is.EqualTo(1000));
        Assert.That(options.DtrEnable, Is.False);
        Assert.That(options.RtsEnable, Is.False);
        Assert.That(options.TextEncoding, Is.EqualTo(Encoding.ASCII));
        Assert.That(options.NewLine, Is.EqualTo("\n"));
    }

    [Test]
    public void SerialEventArgsRetainSuppliedValues()
    {
        var data = new byte[] { 0x01, 0x02 };
        var exception = new InvalidOperationException("failure");

        var dataArgs = new SerialDataReceivedEventArgs(data);
        var errorArgs = new SerialErrorEventArgs("error", exception);
        var signalArgs = new SerialSignalChangedEventArgs(ctsHolding: true, dsrHolding: false, cdHolding: true);
        var textArgs = new SerialTextReceivedEventArgs("ready");

        Assert.That(dataArgs.Data, Is.SameAs(data));
        Assert.That(errorArgs.Message, Is.EqualTo("error"));
        Assert.That(errorArgs.Exception, Is.SameAs(exception));
        Assert.That(signalArgs.CtsHolding, Is.True);
        Assert.That(signalArgs.DsrHolding, Is.False);
        Assert.That(signalArgs.CdHolding, Is.True);
        Assert.That(textArgs.Text, Is.EqualTo("ready"));
    }

    [Test]
    public void SerialEnumsPreserveExpectedContractValues()
    {
        Assert.That((int)SerialHandshake.None, Is.EqualTo(0));
        Assert.That((int)SerialParity.Even, Is.EqualTo(2));
        Assert.That((int)SerialStopBits.OnePointFive, Is.EqualTo(3));
    }
}
