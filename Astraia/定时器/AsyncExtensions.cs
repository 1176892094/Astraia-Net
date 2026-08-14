namespace Astraia;

public static class AsyncExtensions
{
    public static Watch Render(this Watch item)
    {
        return TimeManager.Render(item);
    }

    public static Watch Physic(this Watch item)
    {
        return TimeManager.Physic(item);
    }

    public static Watch Accept(this Watch item)
    {
        return TimeManager.Accept(item);
    }

    public static Tween Render(this Tween item)
    {
        return TimeManager.Render(item);
    }

    public static Tween Physic(this Tween item)
    {
        return TimeManager.Physic(item);
    }

    public static Tween Accept(this Tween item)
    {
        return TimeManager.Accept(item);
    }
}