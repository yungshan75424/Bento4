using System.Reflection;
using System.Runtime.InteropServices;

namespace Bento4Sharp;

internal static class NativeMethods
{
    private static readonly object SyncRoot = new();
    private static bool s_resolverRegistered;
    private static string? s_nativeLibraryPath;

    public static void SetNativeLibraryPath(string nativeLibraryPath)
    {
        if (string.IsNullOrWhiteSpace(nativeLibraryPath))
        {
            throw new ArgumentException("Native library path is required.", nameof(nativeLibraryPath));
        }

        var fullPath = Path.GetFullPath(nativeLibraryPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Bento4 native library not found.", fullPath);
        }

        lock (SyncRoot)
        {
            s_nativeLibraryPath = fullPath;
            RegisterResolver();
        }
    }

    static NativeMethods()
    {
        RegisterResolver();
    }

    private static void RegisterResolver()
    {
        if (s_resolverRegistered)
        {
            return;
        }

        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(),
            (libraryName, assembly, searchPath) =>
            {
                if (!IsBento4Library(libraryName))
                {
                    return IntPtr.Zero;
                }

                var explicitPath = s_nativeLibraryPath;
                if (!string.IsNullOrWhiteSpace(explicitPath))
                {
                    return NativeLibrary.Load(explicitPath);
                }

                foreach (var candidate in GetDefaultCandidates())
                {
                    if (File.Exists(candidate))
                    {
                        return NativeLibrary.Load(candidate);
                    }
                }

                return NativeLibrary.Load(libraryName, assembly, searchPath);
            });

        s_resolverRegistered = true;
    }

    private static bool IsBento4Library(string libraryName)
    {
        return string.Equals(libraryName, "Bento4CDll", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(libraryName, "Bento4CDll.dll", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(libraryName, "Bento4C", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(libraryName, "libBento4C.so", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetDefaultCandidates()
    {
        var appDir = AppContext.BaseDirectory;

        yield return Path.Combine(appDir, "Bento4CDll.dll");
        yield return Path.Combine(appDir, "runtimes", "win-x64", "native", "Bento4CDll.dll");
        yield return Path.Combine(appDir, "libBento4C.so");
        yield return Path.Combine(appDir, "runtimes", "linux-x64", "native", "libBento4C.so");
        yield return Path.GetFullPath(Path.Combine(
            appDir,
            "..", "..", "..", "..", "..",
            "Build", "Targets", "x86_64-microsoft-win32-vs2019", "Bento4CDll", "x64", "Release", "Bento4CDll.dll"));
    }

    public static int DecryptFile(
        string inputFilename,
        string outputFilename,
        string[] keys,
        string? fragmentsInfoFilename,
        int showProgress)
    {
        if (OperatingSystem.IsWindows())
        {
            return AP4_DecryptFileW(inputFilename, outputFilename, keys, (uint)keys.Length, fragmentsInfoFilename, showProgress);
        }

        return AP4_DecryptFileUtf8(inputFilename, outputFilename, keys, (uint)keys.Length, fragmentsInfoFilename, showProgress);
    }

    [DllImport("Bento4CDll", EntryPoint = "AP4_DecryptFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int AP4_DecryptFileW(
        string inputFilename,
        string outputFilename,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 3)]
        string[] keys,
        uint keyCount,
        string? fragmentsInfoFilename,
        int showProgress);

    [DllImport("Bento4C", EntryPoint = "AP4_DecryptFile", CallingConvention = CallingConvention.Cdecl)]
    private static extern int AP4_DecryptFileUtf8(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string inputFilename,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string outputFilename,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str, SizeParamIndex = 3)]
        string[] keys,
        uint keyCount,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? fragmentsInfoFilename,
        int showProgress);
}
