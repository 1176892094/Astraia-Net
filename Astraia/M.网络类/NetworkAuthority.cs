// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-15 00:08:32
// # Recently: 2026-08-15 00:06:32
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

[Serializable]
internal sealed class NetworkAuthority : Transport
{
    public static Transport Instance;

    private readonly Dictionary<int, int> clients = new Dictionary<int, int>();
    private readonly Dictionary<int, int> players = new Dictionary<int, int>();

    internal State state = State.Failure;
    internal int serverId;
    internal int objectId;
    internal int maxPlayer;
    internal bool isClient;
    internal bool isServer;
    internal bool isRemote;
    internal string roomName;
    internal string roomData;
    internal string roomGuid;
    internal Lobby.Room roomMode;

    public bool isRunner => isServer || isClient;
    public bool isActive => state == State.Success;
    public bool isSaloon => state != State.Failure;

    public override void Start(bool isRemote)
    {
        this.isRemote = true;
        Instance.client.onConnect -= Connect;
        Instance.client.onDisconnect -= Disconnected;
        Instance.client.onReceive -= Receive;
        Instance.client.onConnect += Connect;
        Instance.client.onDisconnect += Disconnected;
        Instance.client.onReceive += Receive;
        Instance.port = port;
        Instance.address = address;
        Instance.StartClient();
    }

    internal async void Update()
    {
        var texts = await Host.Http.GetStringAsync("http://{0}:{1}/api/compressed/servers".Format(address, port));
        var xml = Zip.Decompress(texts);
        var serializer = new XmlSerializer(typeof(List<Lobby>));
        using var reader = new StringReader(xml);
        var rooms = (List<Lobby>)serializer.Deserialize(reader);
        EventManager.Invoke(new LobbyUpdate(rooms));
        Log.Info($"房间信息: {xml}");
    }

    internal void Submit()
    {
        using var writer = MemoryWriter.Pop();
        writer.WriteByte((byte)Lobby.Info.更新房间数据);
        writer.WriteString(roomName);
        writer.WriteString(roomData);
        writer.WriteInt32(maxPlayer);
        writer.WriteInt32((byte)roomMode);
        Instance.SendToServer(writer);
    }

    private void Connect(int serverId)
    {
        state = State.Running;
        this.serverId = serverId;
    }

    internal void Disconnected(int serverId)
    {
        if (state != State.Failure)
        {
            objectId = 0;
            clients.Clear();
            players.Clear();
            isServer = false;
            isClient = false;
            state = State.Failure;
            Instance.Disconnect();
            EventManager.Invoke(new LobbyDisconnect());
        }

        isRemote = false;
    }

    private void Receive(ArraySegment<byte> segment, int pass)
    {
        try
        {
            using var reader = MemoryReader.Pop(segment);
            var opcode = (Lobby.Info)reader.ReadByte();
            if (opcode == Lobby.Info.身份验证成功)
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.Info.请求进入大厅);
                writer.WriteString(roomGuid);
                Instance.SendToServer(writer);
            }
            else if (opcode == Lobby.Info.进入大厅成功)
            {
                state = State.Success;
                Update();
            }
            else if (opcode == Lobby.Info.创建房间成功)
            {
                var index = reader.ReadInt32();
                Instance.address = reader.ReadString();
                EventManager.Invoke(new LobbyCreate(index, Instance.address));
            }
            else if (opcode == Lobby.Info.加入房间成功)
            {
                if (isServer)
                {
                    objectId++;
                    var clientId = reader.ReadInt32();
                    clients.Add(clientId, objectId);
                    players.Add(objectId, clientId);
                    server.onConnect(objectId);
                }

                if (isClient)
                {
                    client.onConnect(serverId);
                }
            }
            else if (opcode == Lobby.Info.离开房间成功)
            {
                if (isClient)
                {
                    isClient = false;
                    client.onDisconnect(serverId);
                }
            }
            else if (opcode == Lobby.Info.同步网络数据)
            {
                var message = reader.ReadArraySegment();
                if (isServer)
                {
                    var clientId = reader.ReadInt32();
                    if (clients.TryGetValue(clientId, out var playerId))
                    {
                        server.onReceive(playerId, message, pass);
                    }
                }

                if (isClient)
                {
                    client.onReceive(message, pass);
                }
            }
            else if (opcode == Lobby.Info.断开玩家连接)
            {
                if (isServer)
                {
                    var clientId = reader.ReadInt32();
                    if (clients.TryGetValue(clientId, out var playerId))
                    {
                        server.onDisconnect(playerId);
                        clients.Remove(clientId);
                        players.Remove(playerId);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Warn(e.Message);
        }
    }

    public override void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP)
    {
        if (players.TryGetValue(clientId, out var playerId))
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.Info.同步网络数据);
            writer.WriteInt32(playerId);
            writer.WriteArraySegment(segment);
            Instance.SendToServer(writer);
        }
    }

    public override void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP)
    {
        using var writer = MemoryWriter.Pop();
        writer.WriteByte((byte)Lobby.Info.同步网络数据);
        writer.WriteInt32(0);
        writer.WriteArraySegment(segment);
        Instance.SendToServer(writer);
    }

    public override void StartServer()
    {
        isServer = true;
        using var writer = MemoryWriter.Pop();
        writer.WriteByte((byte)Lobby.Info.请求创建房间);
        writer.WriteString(roomName);
        writer.WriteString(roomData);
        writer.WriteInt32(maxPlayer);
        writer.WriteInt32((byte)roomMode);
        Instance.SendToServer(writer);
    }

    public override void StopServer()
    {
        if (isServer)
        {
            isServer = false;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.Info.请求离开房间);
            Instance.SendToServer(writer);
        }
    }

    public override void Disconnect(int clientId)
    {
        if (players.TryGetValue(clientId, out var playerId))
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.Info.请求移除玩家);
            writer.WriteInt32(playerId);
            Instance.SendToServer(writer);
        }
    }

    public override void StartClient()
    {
        isClient = true;
        using var writer = MemoryWriter.Pop();
        writer.WriteByte((byte)Lobby.Info.请求加入房间);
        writer.WriteString(Instance.address);
        Instance.SendToServer(writer);
    }

    public override void Disconnect()
    {
        if (state != State.Failure)
        {
            isClient = false;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.Info.请求离开房间);
            Instance.SendToServer(writer);
        }
    }

    public override void ClientEarlyUpdate()
    {
        Instance.ClientEarlyUpdate();
    }

    public override void ClientAfterUpdate()
    {
        Instance.ClientAfterUpdate();
    }

    public override void ServerEarlyUpdate() { }

    public override void ServerAfterUpdate() { }
}

[Serializable]
public struct Lobby
{
    public int Host;
    public int Count;
    public int Index;
    public Room Type;
    public string Id;
    public string Name;
    public string Data;
    public List<int> Members;

    public enum Room : byte
    {
        公开,
        私有,
        锁定,
    }

    internal enum Info : byte
    {
        身份验证成功 = 1,
        请求进入大厅 = 2,
        进入大厅成功 = 3,
        请求创建房间 = 4,
        创建房间成功 = 5,
        请求加入房间 = 6,
        加入房间成功 = 7,
        请求离开房间 = 8,
        离开房间成功 = 9,
        请求移除玩家 = 10,
        断开玩家连接 = 11,
        更新房间数据 = 12,
        同步网络数据 = 13,
    }
}