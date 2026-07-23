using System.IO.Ports;
using Chishiki.Serial.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chishiki.Serial.Common;

/// <summary>
/// SerialConnectionFactory is responsible for creating instances of ISerialConnection and providing a list of available serial ports.
/// </summary>
/// <param name="loggerFactory">The logger factory used to create loggers for serial connections.</param>
public class SerialConnectionFactory(ILoggerFactory loggerFactory) : ISerialConnectionFactory
{
    /// <summary>
    /// Creates a new instance of an ISerialConnection.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    /// <summary>
    /// Creates a new instance of an ISerialConnection.
    /// </summary>
    /// <param name="portName">The name of the serial port (e.g., "COM1", "/dev/ttyUSB0").</param>
    /// <param name="options">Options for configuring the serial connection.</param>
    /// <returns>A new instance of an ISerialConnection.</returns>    
    public ISerialConnection CreateConnection(string portName, SerialConnectionOptions options)
    {
        var logger = _loggerFactory.CreateLogger<SerialConnection>();
        return new SerialConnection(portName, options, logger);
    }

    /// <summary>
    /// Gets a list of available serial port names on the system.
    /// </summary>
    /// <returns>A read-only list of available serial port names.</returns>
    public IReadOnlyList<string> GetAvailablePorts() => SerialPort.GetPortNames();
}
