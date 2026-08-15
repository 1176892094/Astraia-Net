// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 20:08:13
// # Recently: 2026-08-15 17:54:37
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

[Serializable]
public readonly record struct Position(Fixation x, Fixation y)
{
    public static readonly Position Zero = new(Fixation.Zero, Fixation.Zero);

    public Fixation sqrMagnitude => x * x + y * y;

    public Fixation magnitude => Fixation.Sqrt(sqrMagnitude);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position operator +(Position a, Position b)
    {
        return new Position(a.x + b.x, a.y + b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position operator -(Position a, Position b)
    {
        return new Position(a.x - b.x, a.y - b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position operator *(Position a, Fixation b)
    {
        return new Position(a.x * b, a.y * b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position operator /(Position a, Fixation b)
    {
        return new Position(a.x / b, a.y / b);
    }

    public static Fixation Dot(Position a, Position b)
    {
        return a.x * b.x + a.y * b.y;
    }

    public static Fixation Cross(Position a, Position b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public static Fixation Distance(Position a, Position b)
    {
        return Fixation.Sqrt((a - b).sqrMagnitude);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Normalize(Position value)
    {
        var sqrMagnitude = value.sqrMagnitude;
        if (sqrMagnitude == Fixation.Zero)
        {
            return Zero;
        }

        var invMagnitude = Fixation.One / Fixation.Sqrt(sqrMagnitude);
        return value * invMagnitude;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position MoveTowards(Position current, Position target, Fixation maxDistanceDelta)
    {
        var delta = target - current;

        var sqrDistance = delta.sqrMagnitude;

        if (sqrDistance == Fixation.Zero)
        {
            return target;
        }

        if (sqrDistance <= maxDistanceDelta * maxDistanceDelta)
        {
            return target;
        }

        return current + Normalize(delta) * maxDistanceDelta;
    }
}