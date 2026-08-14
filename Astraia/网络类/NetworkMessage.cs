namespace Astraia;

internal delegate void MessageDelegate(Connection client, MemoryReader reader, int pass);

internal static class NetworkMessage
{
    private static readonly Dictionary<ushort, MessageDelegate> clientMethod = new();
    private static readonly Dictionary<ushort, MessageDelegate> serverMethod = new();

    public static void SetValueByClient(ushort id, MessageDelegate message)
    {
        clientMethod[id] = message;
    }

    public static void SetValueByServer(ushort id, MessageDelegate message)
    {
        serverMethod[id] = message;
    }

    public static bool GetValueByClient(ushort id, out MessageDelegate message)
    {
        return clientMethod.TryGetValue(id, out message);
    }

    public static bool GetValueByServer(ushort id, out MessageDelegate message)
    {
        return serverMethod.TryGetValue(id, out message);
    }

    public static ushort Id(string name)
    {
        var result = 23U;
        foreach (var c in name)
        {
            result = result * 31 + c;
        }

        return (ushort)result;
    }
}

public static class NetworkMessage<T> where T : struct, IMessage
{
    public static readonly ushort Id = NetworkMessage.Id(typeof(T).FullName);

    public static void Add(Action<T> onReceive)
    {
        NetworkMessage.SetValueByClient(Id, (client, reader, pass) =>
        {
            try
            {
                var position = reader.position;
                var message = reader.Invoke<T>();
                client.OnData(message, reader.position - position);
                onReceive(message);
            }
            catch (Exception e)
            {
                Log.Error($"{typeof(T).Name} 调用失败。传输通道: {pass}\n{e}");
                client.Disconnect();
            }
        });
    }

    public static void Add(Action<Connection, T> onReceive)
    {
        NetworkMessage.SetValueByServer(Id, (client, reader, pass) =>
        {
            try
            {
                var position = reader.position;
                var message = reader.Invoke<T>();
                client.OnData(message, reader.position - position);
                onReceive(client, message);
            }
            catch (Exception e)
            {
                Log.Error($"{typeof(T).Name} 调用失败。传输通道: {pass}\n{e}");
                client.Disconnect();
            }
        });
    }

    public static void Add(Action<Connection, T, int> onReceive)
    {
        NetworkMessage.SetValueByServer(Id, (client, reader, pass) =>
        {
            try
            {
                var position = reader.position;
                var message = reader.Invoke<T>();
                client.OnData(message, reader.position - position);
                onReceive(client, message, pass);
            }
            catch (Exception e)
            {
                Log.Error($"{typeof(T).Name} 调用失败。传输通道: {pass}\n{e}");
                client.Disconnect();
            }
        });
    }
}

public interface IMessage;