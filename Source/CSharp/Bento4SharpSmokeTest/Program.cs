using Bento4Sharp;

try
{
    Bento4Decryptor.Decrypt(new DecryptRequest
    {
        InputPath = @"C:\中文目錄\不存在.mp4",
        OutputPath = @"C:\中文目錄\out.mp4",
        Keys = ["1:00112233445566778899aabbccddeeff"]
    });

    Console.WriteLine("OK");
}
catch (Bento4Exception exception)
{
    Console.WriteLine($"Bento4Error={exception.ResultCode}");
}
