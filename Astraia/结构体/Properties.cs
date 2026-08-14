namespace Astraia;

[Serializable]
public readonly record struct Properties<T>(Fixation[] properties) where T : unmanaged, Enum
{
    public float Get(T key)
    {
        return properties[key.Index()];
    }

    public void Set(T key, float value)
    {
        properties[key.Index()] = value;
    }

    public void Add(T key, float value)
    {
        properties[key.Index()] += value;
    }

    public void Sub(T key, float value)
    {
        properties[key.Index()] -= value;
    }

    public void Clear()
    {
        Array.Clear(properties, 0, properties.Length);
    }

    public static Properties<T> Create()
    {
        return new Properties<T>(new Fixation[Seed.Count<T>()]);
    }
}