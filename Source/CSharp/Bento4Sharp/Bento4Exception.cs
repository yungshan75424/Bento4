namespace Bento4Sharp;

public sealed class Bento4Exception : Exception
{
    public Bento4Exception(int resultCode)
        : base($"Bento4 decrypt failed with result code {resultCode} ({GetResultName(resultCode)}).")
    {
        ResultCode = resultCode;
        Result = Enum.IsDefined(typeof(Bento4Result), resultCode)
            ? (Bento4Result)resultCode
            : Bento4Result.Failure;
    }

    public int ResultCode { get; }

    public Bento4Result Result { get; }

    private static string GetResultName(int resultCode)
    {
        return Enum.IsDefined(typeof(Bento4Result), resultCode)
            ? ((Bento4Result)resultCode).ToString()
            : "Unknown";
    }
}
