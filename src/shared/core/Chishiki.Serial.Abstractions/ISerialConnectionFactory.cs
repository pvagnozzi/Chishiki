//

namespace Chishiki.Serial.Abstractions;

/// <summary>
/// Factory interface for creating instances of ISerialConnection.
/// </summary>
public interface ISerialConnectionFactory
{
    /// <summary>
    /// Gets a list of available serial port names on the system.
    /// </summary>
    /// <returns>A read-only list of available serial port names.</returns>
    IReadOnlyList<string> GetAvailablePorts();

    /// <summary>
    /// Creates a new instance of an ISerialConnection.
    /// </summary>
    /// <param name="portName">The name of the serial port (e.g., "COM1", "/dev/ttyUSB0").</param>
    /// <param name="options">Options for configuring the serial connection.</param>
    /// <returns>A new ISerialConnection instance.</returns>
    ISerialConnection CreateConnection(string portName, SerialConnectionOptions options);
}
