// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 21:08:34
// # Recently: 2026-08-14 21:39:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

internal static class Service
{
    private static readonly Dictionary<string, Lobby> rooms = new Dictionary<string, Lobby>();
    private static readonly Dictionary<int, Lobby> clients = new Dictionary<int, Lobby>();
    private static readonly HashSet<int> connections = new HashSet<int>();
    private static readonly Queue<int> indices = new Queue<int>();
    private static int counter;
    private static Transport Transport => Program.Transport;
    public static List<Lobby> Rooms => rooms.Values.ToList();

    public static void Connect(int clientId)
    {
        connections.Add(clientId);
        using var writer = MemoryWriter.Pop();
        writer.WriteByte((byte)Lobby.Info.身份验证成功);
        Transport.SendToClient(clientId, writer);
    }

    public static void Receive(int clientId, ArraySegment<byte> segment, int channel)
    {
        try
        {
            using var reader = MemoryReader.Pop(segment);
            var opcode = (Lobby.Info)reader.ReadByte();
            if (opcode == Lobby.Info.请求进入大厅)
            {
                if (connections.Remove(clientId))
                {
                    var serverId = reader.ReadString();
                    if (serverId == Program.Setting.ServerId)
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.Info.进入大厅成功);
                        Transport.SendToClient(clientId, writer);
                    }
                }
            }
            else if (opcode == Lobby.Info.请求创建房间)
            {
                Disconnect(clientId);
                string id;
                do
                {
                    id = Seed.Next(0xAAAAAA, 0xFFFFFF).ToString("X6");
                } while (rooms.ContainsKey(id));

                var room = new Lobby
                {
                    Id = id,
                    Host = clientId,
                    Name = reader.ReadString(),
                    Data = reader.ReadString(),
                    Count = reader.ReadInt32(),
                    Type = (Lobby.Room)reader.ReadInt32(),
                    Index = indices.Count > 0 ? indices.Dequeue() : ++counter,
                    Members = new List<int>(),
                };

                rooms.Add(id, room);
                clients.Add(clientId, room);
                Log.Info("客户端 {0} 创建房间。 房间名称: {1} 房间数: {2} 连接数: {3}".Format(clientId, room.Name, rooms.Count, clients.Count));

                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.Info.创建房间成功);
                writer.WriteInt32(room.Index);
                writer.WriteString(room.Id);
                Transport.SendToClient(clientId, writer);
            }
            else if (opcode == Lobby.Info.请求加入房间)
            {
                Disconnect(clientId);
                var roomId = reader.ReadString();
                if (rooms.TryGetValue(roomId, out var room) && room.Members.Count + 1 < room.Count)
                {
                    room.Members.Add(clientId);
                    clients.Add(clientId, room);
                    Log.Info("客户端 {0} 加入房间。 房间名称: {1} 房间数: {2} 连接数: {3}".Format(clientId, room.Name, rooms.Count, clients.Count));

                    using var writer = MemoryWriter.Pop();
                    writer.WriteByte((byte)Lobby.Info.加入房间成功);
                    writer.WriteInt32(clientId);
                    Transport.SendToClient(clientId, writer);
                    Transport.SendToClient(room.Host, writer);
                }
                else
                {
                    using var writer = MemoryWriter.Pop();
                    writer.WriteByte((byte)Lobby.Info.离开房间成功);
                    Transport.SendToClient(clientId, writer);
                }
            }
            else if (opcode == Lobby.Info.更新房间数据)
            {
                if (clients.TryGetValue(clientId, out var room))
                {
                    room.Name = reader.ReadString();
                    room.Data = reader.ReadString();
                    room.Count = reader.ReadInt32();
                    room.Type = (Lobby.Room)reader.ReadInt32();
                    clients[clientId] = room;
                }
            }
            else if (opcode == Lobby.Info.请求离开房间)
            {
                Disconnect(clientId);
            }
            else if (opcode == Lobby.Info.同步网络数据)
            {
                var agentId = reader.ReadInt32();
                var message = reader.ReadArraySegment();
                if (clients.TryGetValue(clientId, out var room))
                {
                    if (message.Count > Connection.GetLength(channel))
                    {
                        Log.Warn(message.Count);
                        Disconnect(clientId);
                        return;
                    }

                    if (room.Host == clientId)
                    {
                        if (room.Members.Contains(agentId))
                        {
                            using var writer = MemoryWriter.Pop();
                            writer.WriteByte((byte)Lobby.Info.同步网络数据);
                            writer.WriteArraySegment(message);
                            Transport.SendToClient(agentId, writer, channel);
                        }
                    }
                    else
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.Info.同步网络数据);
                        writer.WriteArraySegment(message);
                        writer.WriteInt32(clientId);
                        Transport.SendToClient(room.Host, writer, channel);
                    }
                }
            }
            else if (opcode == Lobby.Info.请求移除玩家)
            {
                var agentId = reader.ReadInt32();
                Disconnect(agentId);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            Transport.Disconnect(clientId);
        }
    }

    public static void Disconnect(int clientId)
    {
        if (clients.TryGetValue(clientId, out var room))
        {
            if (room.Host == clientId) // 主机断开
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.Info.离开房间成功);
                foreach (var member in room.Members)
                {
                    Transport.SendToClient(member, writer);
                    clients.Remove(member);
                }

                room.Members.Clear();
                rooms.Remove(room.Id);
                clients.Remove(clientId);
                indices.Enqueue(room.Index);
                return;
            }

            if (room.Members.Remove(clientId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.Info.断开玩家连接);
                writer.WriteInt32(clientId);
                Transport.SendToClient(room.Host, writer);
                clients.Remove(clientId);
            }
        }
    }
}