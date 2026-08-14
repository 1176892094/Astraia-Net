namespace Astraia;

internal static class Text
{
    [ThreadStatic] private static UTF8Encoding encoding;

    private static UTF8Encoding Encoding => encoding ??= new UTF8Encoding(false, true);

    public static byte[] GetBytes(string message)
    {
        return Encoding.GetBytes(message);
    }

    public static int GetBytes(string message, int count, byte[] buffer, int index)
    {
        return Encoding.GetBytes(message, 0, count, buffer, index);
    }

    public static string GetString(byte[] bytes)
    {
        return Encoding.GetString(bytes);
    }

    public static string GetString(byte[] bytes, int index, int count)
    {
        return Encoding.GetString(bytes, index, count);
    }

    public static int GetMaxByteCount(int count)
    {
        return Encoding.GetMaxByteCount(count);
    }
}