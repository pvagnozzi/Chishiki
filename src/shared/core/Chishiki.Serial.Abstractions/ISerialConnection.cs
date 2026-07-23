namespace Chishiki.Serial.Abstractions;

/// <summary>Cross-platform serial connection.</summary>
public interface ISerialConnection : IDisposable
{
    /// <summary>Raised when binary data is received.</summary>
    event EventHandler<SerialDataReceivedEventArgs>? DataReceived;

    /// <summary>Raised when text data is received.</summary>
    event EventHandler<SerialTextReceivedEventArgs>? TextReceived;

    /// <summary>Raised when an error is received.</summary>
    event EventHandler<SerialErrorEventArgs>? ErrorReceived;

    /// <summary>Raised when a signal changes.</summary>
    event EventHandler<SerialSignalChangedEventArgs>? SignalChanged;

    /// <summary>Gets the options used to configure the serial connection.</summary>
    SerialConnectionOptions Options { get; }

    /// <summary>Gets a value indicating whether the port is open.</summary>
    bool IsOpen { get; }

    /// <summary>Gets or sets the DTR (Data Terminal Ready) signal state.</summary>
    bool DtrEnable { get; set; }

    /// <summary>Gets or sets the RTS (Request To Send) signal state.</summary>
    bool RtsEnable { get; set; }

    /// <summary>Gets the CTS (Clear To Send) signal state.</summary>
    bool CtsHolding { get; }

    /// <summary>Gets the DSR (Data Set Ready) signal state.</summary>
    bool DsrHolding { get; }

    /// <summary>Gets the CD (Carrier Detect) signal state.</summary>
    bool CdHolding { get; }

    /// <summary>Gets the number of bytes available to read.</summary>
    int BytesToRead { get; }

    /// <summary>Gets the number of bytes waiting to be written.</summary>
    int BytesToWrite { get; }

    /// <summary>Opens the port.</summary>
    void Open();

    /// <summary>Asynchronously opens the port.</summary>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Closes the port.</summary>
    void Close();

    /// <summary>Asynchronously closes the port.</summary>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads binary data from the port.</summary>
    int Read(byte[] buffer, int offset, int count);

    /// <summary>Asynchronously reads binary data from the port.</summary>
    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

    /// <summary>Reads all available text from the port.</summary>
    string ReadExistingText();

    /// <summary>Asynchronously reads all available text from the port.</summary>
    Task<string> ReadExistingTextAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads a line of text from the port.</summary>
    string ReadLine();

    /// <summary>Asynchronously reads a line of text from the port.</summary>
    Task<string> ReadLineAsync(CancellationToken cancellationToken = default);

    /// <summary>Writes binary data to the port.</summary>
    void Write(byte[] buffer, int offset, int count);

    /// <summary>Asynchronously writes binary data to the port.</summary>
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

    /// <summary>Writes text to the port.</summary>
    void WriteText(string text);

    /// <summary>Asynchronously writes text to the port.</summary>
    Task WriteTextAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Writes a line of text to the port.</summary>
    void WriteLine(string text);

    /// <summary>Asynchronously writes a line of text to the port.</summary>
    Task WriteLineAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Flushes all buffered data to the port.</summary>
    void Flush();

    /// <summary>Asynchronously flushes all buffered data to the port.</summary>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears the input buffer.</summary>
    void DiscardInBuffer();

    /// <summary>Clears the output buffer.</summary>
    void DiscardOutBuffer();

    /// <summary>Annulla le operazioni.</summary>
    void CancelPendingOperations();
}
