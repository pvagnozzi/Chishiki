namespace Chishiki.Serial.Abstractions;

/// <summary>Handshake supportati.</summary>
public enum SerialHandshake
{
    /// <summary>Nessuno.</summary>
    None,

    /// <summary>XOn/XOff.</summary>
    XOnXOff,

    /// <summary>Request To Send.</summary>
    RequestToSend,

    /// <summary>Request To Send con XOn/XOff.</summary>
    RequestToSendXOnXOff
}
