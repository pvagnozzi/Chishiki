namespace Chishiki.Serial.Abstractions;

/// <summary>Stato dei segnali hardware della seriale.</summary>
/// <remarks>Crea un nuovo snapshot dei segnali.</remarks>
public class SerialSignalChangedEventArgs(bool ctsHolding, bool dsrHolding, bool cdHolding) : EventArgs
{
    /// <summary>Stato del segnale CTS.</summary>
    public bool CtsHolding { get; } = ctsHolding;

    /// <summary>Stato del segnale DSR.</summary>
    public bool DsrHolding { get; } = dsrHolding;

    /// <summary>Stato del segnale CD.</summary>
    public bool CdHolding { get; } = cdHolding;
}
