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
public sealed class SpatialHash<T>
{
    private readonly Dictionary<int, HashSet<T>> buckets = new();
    private readonly Dictionary<T, int> objects = new();

    private static int Compress(int x, int y)
    {
        return (x << 16) | (y & 0xFFFF);
    }

    private static int Compress(Position position)
    {
        return Compress(position.x.FloorToInt(), position.y.FloorToInt());
    }

    private void Remove(T item, int node)
    {
        if (buckets.TryGetValue(node, out var items))
        {
            items.Remove(item);
            if (items.Count == 0)
            {
                buckets.Remove(node);
            }
        }
    }

    public void Insert(T item, Position center)
    {
        var node = Compress(center);
        if (!buckets.TryGetValue(node, out var items))
        {
            items = new HashSet<T>();
            buckets.Add(node, items);
        }

        items.Add(item);
        objects[item] = node;
    }

    public void Remove(T item)
    {
        if (objects.TryGetValue(item, out var node))
        {
            Remove(item, node);
            objects.Remove(item);
        }
    }

    public void Update(T item, Position center)
    {
        if (objects.TryGetValue(item, out var oldNode))
        {
            var newNode = Compress(center);
            if (oldNode != newNode)
            {
                Remove(item, oldNode);

                if (!buckets.TryGetValue(newNode, out var newItems))
                {
                    newItems = new HashSet<T>();
                    buckets.Add(newNode, newItems);
                }

                newItems.Add(item);
                objects[item] = newNode;
            }
        }
    }

    public void Query(Position center, int extentX, int extentY, HashSet<T> items)
    {
        items.Clear();
        var centerX = center.x.FloorToInt();
        var centerY = center.y.FloorToInt();

        var minX = centerX - extentX;
        var maxX = centerX + extentX;
        var minY = centerY - extentY;
        var maxY = centerY + extentY;

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var node = Compress(x, y);
                if (buckets.TryGetValue(node, out var copies))
                {
                    foreach (var item in copies)
                    {
                        items.Add(item);
                    }
                }
            }
        }
    }

    public void Clear()
    {
        buckets.Clear();
        objects.Clear();
    }
}