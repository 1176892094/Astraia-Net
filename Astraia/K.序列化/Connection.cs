namespace Astraia;

[Serializable]
public abstract class Connection
{
    private readonly Dictionary<int, NetworkWriter> writers = new();
    private readonly NetworkReader reader = new();
    
    public int Count => reader.Count;

    internal void Update()
    {
        foreach (var copied in writers)
        {
            using var writer = MemoryWriter.Pop();
            while (copied.Value.GetBatch(writer))
            {
                SendInternal(writer, copied.Key);
                writer.Reset();
            }
        }
    }

    internal bool AddBatch(ArraySegment<byte> segment)
    {
        return reader.AddBatch(segment);
    }

    internal bool GetMessage(out ArraySegment<byte> segment)
    {
        return reader.GetMessage(out segment);
    }

    public void Send<T>(T message, int pass = Pass.KCP) where T : struct, IMessage
    {
        using var writer = MemoryWriter.Pop();
        writer.Write(NetworkMessage<T>.Id);
        writer.Invoke(message);

        if (writer.position > GetLength(pass))
        {
            Log.Error($"发送消息数量过大！消息大小: {writer.position}");
            return;
        }

        OnSend(message, writer.position);
        AddMessage(writer, pass);
    }

    private void AddMessage(MemoryWriter writer, int pass)
    {
        if (!writers.TryGetValue(pass, out var copied))
        {
            copied = new NetworkWriter(GetLength(pass));
            writers[pass] = copied;
        }

        copied.AddMessage(writer);
        DataInternal(copied, pass);
    }

    public static int GetLength(int pass)
    {
        return pass == Pass.KCP ? Const.KCP_LEN : Const.UDP_LEN;
    }

    internal abstract void SendInternal(MemoryWriter writer, int pass);
    internal abstract void DataInternal(NetworkWriter writer, int pass);
    internal abstract void OnSend<T>(T message, int count) where T : struct, IMessage;
    internal abstract void OnData<T>(T message, int count) where T : struct, IMessage;
    public abstract void Disconnect();
}