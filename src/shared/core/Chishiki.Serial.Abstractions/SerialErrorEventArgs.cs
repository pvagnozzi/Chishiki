namespace Chishiki.Serial.Abstractions;

/// <summary>Errore associato alla connessione seriale.</summary>
/// <remarks>Crea un nuovo errore seriale.</remarks>
public sealed class SerialErrorEventArgs(string message, Exception? exception = null) : EventArgs
{
    /// <summary>Messaggio di errore.</summary>
    public string Message { get; } = message;

    /// <summary>Eccezione associata all'errore, se presente.</summary>
    public Exception? Exception { get; } = exception;
}
