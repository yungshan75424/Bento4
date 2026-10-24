namespace Bento4Sharp;

/// <summary>
/// Represents the input parameters for decrypting an MP4 file with Bento4.
/// </summary>
public sealed class DecryptRequest
{
    /// <summary>
    /// Gets the full path of the encrypted input MP4 file.
    /// Chinese and other Unicode paths are supported.
    /// </summary>
    /// <example>C:\影片\encrypted.mp4</example>
    public required string InputPath { get; init; }

    /// <summary>
    /// Gets the full path where the decrypted MP4 file should be written.
    /// The target directory must already exist.
    /// </summary>
    /// <example>C:\影片\decrypted.mp4</example>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Gets the decryption keys in Bento4 format.
    /// Each item must use the same format accepted by mp4decrypt: <c>trackId:keyHex</c> or <c>kidHex:keyHex</c>.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// <list type="bullet">
    /// <item><description><c>1:00112233445566778899aabbccddeeff</c> uses track ID 1.</description></item>
    /// <item><description><c>0123456789abcdef0123456789abcdef:00112233445566778899aabbccddeeff</c> uses a 128-bit KID.</description></item>
    /// </list>
    /// Multiple keys can be provided when the file has multiple encrypted tracks.
    /// </remarks>
    public required IReadOnlyList<string> Keys { get; init; }

    /// <summary>
    /// Gets the optional fragments info file path.
    /// Use this only when decrypting fragmented MP4 data that needs track information from another file.
    /// </summary>
    /// <remarks>
    /// This maps to Bento4's <c>--fragments-info</c> option.
    /// Most normal MP4 decrypt operations can leave this as <see langword="null" />.
    /// </remarks>
    public string? FragmentsInfoPath { get; init; }

    /// <summary>
    /// Gets a value indicating whether Bento4 should enable progress reporting internally.
    /// </summary>
    /// <remarks>
    /// The current native DLL API does not expose progress callbacks yet, so this is kept for API compatibility
    /// with the original mp4decrypt option and for future extension.
    /// </remarks>
    public bool ShowProgress { get; init; }

    /// <summary>
    /// Gets the optional full path to <c>Bento4CDll.dll</c>.
    /// </summary>
    /// <remarks>
    /// Leave this as <see langword="null" /> when <c>Bento4CDll.dll</c> is copied next to the application
    /// or under <c>runtimes\win-x64\native</c>. Set it only when the native DLL is stored somewhere else.
    /// </remarks>
    public string? NativeLibraryPath { get; init; }
}
