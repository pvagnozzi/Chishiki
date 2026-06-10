namespace Chishiki.Serial.Abstractions;

/// <summary>Dati ricevuti.</summary>
/// <remarks>Crea un nuovo evento.</remarks>
public sealed class SerialDataReceivedEventArgs(byte[] data) : EventArgs
{
    /// <summary>Dati.</summary>
    public byte[] Data { get; } = data;
}
