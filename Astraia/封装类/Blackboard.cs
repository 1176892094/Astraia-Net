namespace Astraia;

[Serializable]
public class Blackboard<T>
{
    private Dictionary<Type, IDictionary> properties = new();

    public void Set<TValue>(T key, TValue value)
    {
        if (!properties.TryGetValue(typeof(TValue), out var items))
        {
            items = new Dictionary<T, TValue>();
            properties.Add(typeof(TValue), items);
        }

        ((Dictionary<T, TValue>)items)[key] = value;
    }

    public TValue Get<TValue>(T key)
    {
        if (!properties.TryGetValue(typeof(TValue), out var items))
        {
            items = new Dictionary<T, TValue>();
            properties.Add(typeof(TValue), items);
        }

        return ((Dictionary<T, TValue>)items).GetValueOrDefault(key);
    }

    public void Clear()
    {
        foreach (var child in properties.Values)
        {
            child.Clear();
        }

        properties.Clear();
    }
}