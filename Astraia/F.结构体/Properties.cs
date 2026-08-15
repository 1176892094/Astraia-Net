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