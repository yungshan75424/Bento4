namespace Bento4Sharp;

/// <summary>
/// Provides high-level C# methods for decrypting MP4 files through the Bento4 native library.
/// </summary>
public static class Bento4Decryptor
{
    /// <summary>
    /// Decrypts an MP4 file synchronously.
    /// </summary>
    /// <param name="request">The decrypt request that contains input path, output path, keys, and optional settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when required request values are missing or invalid.</exception>
    /// <exception cref="FileNotFoundException">Thrown when <see cref="DecryptRequest.NativeLibraryPath" /> is set but the DLL cannot be found.</exception>
    /// <exception cref="Bento4Exception">Thrown when Bento4 returns a non-success result code.</exception>
    public static void Decrypt(DecryptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        if (!string.IsNullOrWhiteSpace(request.NativeLibraryPath))
        {
            NativeMethods.SetNativeLibraryPath(request.NativeLibraryPath);
        }

        var keys = request.Keys.ToArray();
        var result = NativeMethods.DecryptFile(
            request.InputPath,
            request.OutputPath,
            keys,
            string.IsNullOrWhiteSpace(request.FragmentsInfoPath) ? null : request.FragmentsInfoPath,
            request.ShowProgress ? 1 : 0);

        if (result != 0)
        {
            throw new Bento4Exception(result);
        }
    }

    /// <summary>
    /// Decrypts an MP4 file on a background thread.
    /// </summary>
    /// <param name="request">The decrypt request that contains input path, output path, keys, and optional settings.</param>
    /// <param name="cancellationToken">A token used to cancel before the native decrypt operation starts.</param>
    /// <returns>A task that completes when decryption finishes.</returns>
    /// <remarks>
    /// Cancellation is cooperative before the native call starts. Once Bento4 is actively decrypting, the current
    /// native API does not expose an interruption callback yet.
    /// </remarks>
    public static Task DecryptAsync(DecryptRequest request, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Decrypt(request);
        }, cancellationToken);
    }

    private static void Validate(DecryptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InputPath))
        {
            throw new ArgumentException("InputPath is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            throw new ArgumentException("OutputPath is required.", nameof(request));
        }

        if (request.Keys.Count == 0)
        {
            throw new ArgumentException("At least one key is required.", nameof(request));
        }

        if (request.Keys.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Keys cannot contain empty values.", nameof(request));
        }
    }
}
