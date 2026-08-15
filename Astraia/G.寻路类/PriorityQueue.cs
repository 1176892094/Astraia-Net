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
internal sealed class PriorityQueue(int[] cost)
{
    private readonly List<int> heap = new();

    public int Count => heap.Count;

    public void Enqueue(int index)
    {
        heap.Add(index);

        var i = heap.Count - 1;

        while (i > 0)
        {
            var parent = (i - 1) >> 1;

            if (cost[heap[parent]] <= cost[heap[i]])
            {
                break;
            }

            (heap[parent], heap[i]) = (heap[i], heap[parent]);
            i = parent;
        }
    }

    public int Dequeue()
    {
        var root = heap[0];

        var last = heap[^1];
        heap.RemoveAt(heap.Count - 1);

        if (heap.Count == 0)
        {
            return root;
        }

        heap[0] = last;

        var i = 0;

        while (true)
        {
            var left = i * 2 + 1;

            if (left >= heap.Count)
            {
                break;
            }

            var right = left + 1;

            var smallest = left;

            if (right < heap.Count && cost[heap[right]] < cost[heap[left]])
            {
                smallest = right;
            }

            if (cost[heap[i]] <= cost[heap[smallest]])
            {
                break;
            }

            (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
            i = smallest;
        }

        return root;
    }

    public void Clear()
    {
        heap.Clear();
    }
}