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