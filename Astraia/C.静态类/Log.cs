// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 20:08:13
// # Recently: 2026-08-15 17:54:36
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

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