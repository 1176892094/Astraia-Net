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
public readonly record struct Neighbors(int x, int y, int cost)
{
    public static readonly Neighbors[] Data;

    static Neighbors()
    {
        Data = new Neighbors[8];

        Data[0] = new Neighbors(+0, 1, 10);
        Data[1] = new Neighbors(+1, 1, 14);
        Data[2] = new Neighbors(-1, 1, 14);

        Data[3] = new Neighbors(+0, -1, 10);
        Data[4] = new Neighbors(+1, -1, 14);
        Data[5] = new Neighbors(-1, -1, 14);

        Data[6] = new Neighbors(+1, 0, 10);
        Data[7] = new Neighbors(-1, 0, 10);
    }
}