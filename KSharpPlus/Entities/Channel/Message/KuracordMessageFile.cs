namespace KSharpPlus.Entities.Channel.Message; 

/// <summary>
/// Represents the File that should be sent to Kuracord from the <see cref="KuracordMessageBuilder"/>.
/// </summary>
public class KuracordMessageFile {
    internal KuracordMessageFile(string? fileName, Stream stream, long? resetPositionTo, string fileType = null, string contentType = null) {
        FileName = fileName ?? "file";
        FileType = fileType;
        ContentType = contentType;
        Stream = stream;
        ResetPositionTo = resetPositionTo;
    }

    /// <summary>
    /// Gets the FileName of the File.
    /// </summary>
    public string FileName { get; internal set; }

    /// <summary>
    /// Gets the stream of the File.
    /// </summary>
    public Stream Stream { get; internal set; }

    internal string FileType { get; set; }

    internal string ContentType { get; set; }

    /// <summary>
    /// Gets the position the File should be reset to.
    /// </summary>
    internal long? ResetPositionTo { get; set; }
}