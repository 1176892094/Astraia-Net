namespace Astraia;

public static class Log
{
    private static event Action<string> onInfo = Console.WriteLine;
    private static event Action<string> onWarn = Console.WriteLine;
    private static event Action<string> onError = Console.Error.WriteLine;

    public static void Setup(Action<string> onInfo, Action<string> onWarn, Action<string> onError)
    {
        Log.onInfo = onInfo;
        Log.onWarn = onWarn;
        Log.onError = onError;
    }

    public static void Info(object message)
    {
        onInfo(message.ToString());
    }

    public static void Warn(object message)
    {
        onWarn(message.ToString());
    }

    public static void Error(object message)
    {
        onError(message.ToString());
    }
}