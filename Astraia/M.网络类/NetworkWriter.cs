namespace Astraia;

internal sealed class NetworkWriter(int capacity)
{
    private readonly Queue<MemoryWriter> writers = new();
    private MemoryWriter writer;

    public void AddMessage(ArraySegment<byte> segment)
    {
        var count = Compress.Length((ulong)segment.Count);
        if (writer == null)
        {
            writer = MemoryWriter.Pop();
        }
        else if (writer.position + count + segment.Count > capacity)
        {
            writers.Enqueue(writer);
            writer = MemoryWriter.Pop();
        }

        Compress.EncodeUInt32(writer, (uint)segment.Count);
        writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
    }

    public bool GetBatch(MemoryWriter result)
    {
        if (result.position != 0)
        {
            throw new ArgumentException("拷贝目标不是空的!", nameof(result));
        }

        MemoryWriter copied;
        if (writers.Count > 0)
        {
            copied = writers.Dequeue();
        }
        else if (writer != null)
        {
            copied = writer;
            writer = null;
        }
        else
        {
            return false;
        }

        result.WriteBytes(copied.buffer, 0, copied.position);
        MemoryWriter.Push(copied);
        return true;
    }
}