namespace DomuWave.Services.Helpers;

public static class PaymentCodeGenerator
{
    private static readonly char[] Chars =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); // no I/O/1/0 ambigui

    public static string Generate()
    {
        var datePart   = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = RandomString(6);
        return $"DW{datePart}{randomPart}";   // es. DW202506041A3XK7
    }

    private static string RandomString(int length)
    {
        var rng    = Random.Shared;
        var result = new char[length];
        for (var i = 0; i < length; i++)
            result[i] = Chars[rng.Next(Chars.Length)];
        return new string(result);
    }
}
