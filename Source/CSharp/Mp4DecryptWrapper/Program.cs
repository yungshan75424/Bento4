using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args.Length == 0 || HasArg(args, "--help") || HasArg(args, "-h"))
{
    PrintUsage();
    return 1;
}

try
{
    var request = ParseRequest(args);
    var dllPath = ResolveBento4Dll(request.DllPath);
    NativeMethods.LoadFrom(dllPath);

    var keys = request.Keys.ToArray();
    var result = NativeMethods.AP4_DecryptFileW(
        request.Input,
        request.Output,
        keys,
        (uint)keys.Length,
        string.IsNullOrWhiteSpace(request.FragmentsInfo) ? null : request.FragmentsInfo,
        request.ShowProgress ? 1 : 0);

    if (result != 0)
    {
        Console.Error.WriteLine($"ERROR: AP4_DecryptFileW failed with result {result}.");
    }

    return result == 0 ? 0 : 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine("ERROR: " + exception.Message);
    return 1;
}

static Mp4DecryptRequest ParseRequest(string[] args)
{
    var jsonBase64 = GetValue(args, "--JsonBase64") ?? GetValue(args, "--json-base64");
    if (jsonBase64 != null)
    {
        var json = DecodeBase64Utf8(jsonBase64);
        var jsonRequest = JsonSerializer.Deserialize<Mp4DecryptRequest>(json, CreateJsonOptions());
        if (jsonRequest == null)
        {
            throw new ArgumentException("invalid JSON request.");
        }

        ValidateRequest(jsonRequest);
        return jsonRequest;
    }

    var input = GetValue(args, "--input");
    var output = GetValue(args, "--output");
    var key = GetValue(args, "--key");
    if (input == null || output == null || key == null)
    {
        throw new ArgumentException("use --JsonBase64, or provide --input, --output and --key.");
    }

    var cliRequest = new Mp4DecryptRequest
    {
        Input = input,
        Output = output,
        Keys = [key],
        FragmentsInfo = GetValue(args, "--fragments-info"),
        ShowProgress = HasArg(args, "--show-progress"),
        DllPath = GetValue(args, "--dll")
    };

    ValidateRequest(cliRequest);
    return cliRequest;
}

static void ValidateRequest(Mp4DecryptRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Input))
    {
        throw new ArgumentException("input is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Output))
    {
        throw new ArgumentException("output is required.");
    }

    if (request.Keys.Count == 0)
    {
        throw new ArgumentException("at least one key is required.");
    }

    if (request.Keys.Any(string.IsNullOrWhiteSpace))
    {
        throw new ArgumentException("key cannot be empty.");
    }
}

static string ResolveBento4Dll(string? explicitPath)
{
    if (!string.IsNullOrWhiteSpace(explicitPath))
    {
        return RequireFile(explicitPath);
    }

    var envPath = Environment.GetEnvironmentVariable("BENTO4_DLL");
    if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
    {
        return envPath;
    }

    var appDir = AppContext.BaseDirectory;
    var candidates = new[]
    {
        Path.Combine(appDir, "Bento4CDll.dll"),
        Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "..", "..", "Build", "Targets", "x86_64-microsoft-win32-vs2019", "Bento4CDll", "x64", "Release", "Bento4CDll.dll"))
    };

    foreach (var candidate in candidates)
    {
        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new FileNotFoundException("Bento4CDll.dll not found. Use --dll or set BENTO4_DLL.");
}

static string RequireFile(string path)
{
    var fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
        throw new FileNotFoundException("file not found: " + fullPath);
    }

    return fullPath;
}

static string DecodeBase64Utf8(string value)
{
    value = value.Trim();
    value = value.Replace('-', '+').Replace('_', '/');
    switch (value.Length % 4)
    {
        case 2:
            value += "==";
            break;
        case 3:
            value += "=";
            break;
    }

    return Encoding.UTF8.GetString(Convert.FromBase64String(value));
}

static string? GetValue(string[] args, string name)
{
    for (var index = 0; index < args.Length; index++)
    {
        if (!string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (index + 1 >= args.Length)
        {
            throw new ArgumentException(name + " requires a value.");
        }

        return args[index + 1];
    }

    return null;
}

static bool HasArg(string[] args, string name)
{
    return args.Any(arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
}

static void PrintUsage()
{
    Console.Error.WriteLine("usage:");
    Console.Error.WriteLine("  Mp4DecryptSharp --JsonBase64 <base64-utf8-json>");
    Console.Error.WriteLine("  Mp4DecryptSharp --input <file> --output <file> --key <id:key> [--dll <Bento4CDll.dll>]");
    Console.Error.WriteLine();
    Console.Error.WriteLine("json fields:");
    Console.Error.WriteLine("  input, output, keys, fragmentsInfo, showProgress, dllPath");
}

static JsonSerializerOptions CreateJsonOptions()
{
    return new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}

sealed class Mp4DecryptRequest
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = "";

    [JsonPropertyName("output")]
    public string Output { get; set; } = "";

    [JsonPropertyName("keys")]
    public List<string> Keys { get; set; } = [];

    [JsonPropertyName("fragmentsInfo")]
    public string? FragmentsInfo { get; set; }

    [JsonPropertyName("showProgress")]
    public bool ShowProgress { get; set; }

    [JsonPropertyName("dllPath")]
    public string? DllPath { get; set; }
}

static class NativeMethods
{
    private static string? s_dllPath;

    public static void LoadFrom(string dllPath)
    {
        s_dllPath = dllPath;
        NativeLibrary.SetDllImportResolver(
            Assembly.GetExecutingAssembly(),
            (libraryName, _, _) =>
            {
                if (string.Equals(libraryName, "Bento4CDll", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(libraryName, "Bento4CDll.dll", StringComparison.OrdinalIgnoreCase))
                {
                    return NativeLibrary.Load(s_dllPath);
                }

                return IntPtr.Zero;
            });
    }

    [DllImport("Bento4CDll", EntryPoint = "AP4_DecryptFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int AP4_DecryptFileW(
        string inputFilename,
        string outputFilename,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 3)]
        string[] keys,
        uint keyCount,
        string? fragmentsInfoFilename,
        int showProgress);
}
