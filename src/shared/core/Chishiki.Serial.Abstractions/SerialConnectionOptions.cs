using System.Text;

namespace Chishiki.Serial.Abstractions;

/// <summary>Opzioni della connessione seriale.</summary>
public sealed class SerialConnectionOptions
{
    /// <summary>Nome porta.</summary>
    public required string PortName { get; init; }

    /// <summary>Baud rate.</summary>
    public int BaudRate { get; init; } = 9600;

    /// <summary>Parita.</summary>
    public SerialParity Parity { get; init; } = SerialParity.None;

    /// <summary>Bit dati.</summary>
    public int DataBits { get; init; } = 8;

    /// <summary>Bit di stop.</summary>
    public SerialStopBits StopBits { get; init; } = SerialStopBits.One;

    /// <summary>Handshake.</summary>
    public SerialHandshake Handshake { get; init; } = SerialHandshake.None;

    /// <summary>Timeout lettura.</summary>
    public int ReadTimeout { get; init; } = 1000;

    /// <summary>Timeout scrittura.</summary>
    public int WriteTimeout { get; init; } = 1000;

    /// <summary>DTR.</summary>
    public bool DtrEnable { get; init; }

    /// <summary>RTS.</summary>
    public bool RtsEnable { get; init; }

    /// <summary>Encoding testo.</summary>
    public Encoding TextEncoding { get; init; } = Encoding.ASCII;

    /// <summary>Fine riga.</summary>
    public string NewLine { get; init; } = "\n";
}
