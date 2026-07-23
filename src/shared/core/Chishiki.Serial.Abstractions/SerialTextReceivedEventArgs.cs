namespace Chishiki.Serial.Abstractions;

/// <summary>Testo ricevuto.</summary>
/// <remarks>Crea un nuovo evento.</remarks>
public class SerialTextReceivedEventArgs(string text) : EventArgs
{
    /// <summary>Testo.</summary>
    public string Text { get; } = text;
}
