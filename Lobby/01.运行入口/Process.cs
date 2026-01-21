// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-10 21:01:31
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace Astraia.Net
{
    internal static class Process
    {
        private static readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private static readonly Dictionary<int, Room> clients = new Dictionary<int, Room>();
        private static readonly HashSet<int> connections = new HashSet<int>();
        private static Transport Transport => Program.Transport;
        public static List<Room> Rooms => rooms.Values.ToList();

        public static void Connect(int clientId)
        {
            connections.Add(clientId);
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.身份验证成功);
            Transport.SendToClient(clientId, writer);
        }

        public static void Disconnect(int clientId)
        {
            var copies = rooms.Values.ToList();
            foreach (var room in copies)
            {
                if (room.clientId == clientId) // 主机断开
                {
                    using var writer = MemoryWriter.Pop();
                    writer.WriteByte((byte)Lobby.离开房间成功);
                    foreach (var client in room.clients)
                    {
                        Transport.SendToClient(client, writer);
                        clients.Remove(client);
                    }

                    room.clients.Clear();
                    rooms.Remove(room.roomId);
                    clients.Remove(clientId);
                    return;
                }

                if (room.clients.Remove(clientId)) // 客户端断开
                {
                    using var writer = MemoryWriter.Pop();
                    writer.WriteByte((byte)Lobby.断开玩家连接);
                    writer.WriteInt(clientId);
                    Transport.SendToClient(room.clientId, writer);
                    clients.Remove(clientId);
                    break;
                }
            }
        }

        public static void Receive(int clientId, ArraySegment<byte> segment, int channel)
        {
            try
            {
                using var reader = MemoryReader.Pop(segment);
                var opcode = (Lobby)reader.ReadByte();
                if (opcode == Lobby.请求进入大厅)
                {
                    if (connections.Contains(clientId))
                    {
                        var serverKey = reader.ReadString();
                        if (serverKey == Program.Setting.ServerId)
                        {
                            using var writer = MemoryWriter.Pop();
                            writer.WriteByte((byte)Lobby.进入大厅成功);
                            Transport.SendToClient(clientId, writer);
                        }

                        connections.Remove(clientId);
                    }
                }
                else if (opcode == Lobby.请求创建房间)
                {
                    Disconnect(clientId);
                    string id;
                    do
                    {
                        var buffer = new char[6];
                        for (int i = 0; i < 6; i++)
                        {
                            buffer[i] = (char)('A' + Service.Seed.Next(26));
                        }

                        id = new string(buffer);
                    } while (rooms.ContainsKey(id));

                    var room = new Room
                    {
                        roomId = id,
                        clientId = clientId,
                        roomName = reader.ReadString(),
                        roomData = reader.ReadString(),
                        maxCount = reader.ReadInt(),
                        roomMode = reader.ReadByte(),
                        clients = new HashSet<int>(),
                    };

                    rooms.Add(id, room);
                    clients.Add(clientId, room);
                    Service.Log.Info("客户端 {0} 创建房间。 房间名称: {1} 房间数: {2} 连接数: {3}".Format(clientId, room.roomName, rooms.Count, clients.Count));

                    using var writer = MemoryWriter.Pop();
                    writer.WriteByte((byte)Lobby.创建房间成功);
                    writer.WriteString(room.roomId);
                    Transport.SendToClient(clientId, writer);
                }
                else if (opcode == Lobby.请求加入房间)
                {
                    Disconnect(clientId);
                    var roomId = reader.ReadString();
                    if (rooms.TryGetValue(roomId, out var room) && room.clients.Count + 1 < room.maxCount)
                    {
                        room.clients.Add(clientId);
                        clients.Add(clientId, room);
                        Service.Log.Info(("客户端 {0} 加入房间。 房间名称: {1} 房间数: {2} 连接数: {3}".Format(clientId, room.roomName, rooms.Count, clients.Count)));

                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.加入房间成功);
                        writer.WriteInt(clientId);
                        Transport.SendToClient(clientId, writer);
                        Transport.SendToClient(room.clientId, writer);
                    }
                    else
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.离开房间成功);
                        Transport.SendToClient(clientId, writer);
                    }
                }
                else if (opcode == Lobby.更新房间数据)
                {
                    if (clients.TryGetValue(clientId, out var room))
                    {
                        room.roomName = reader.ReadString();
                        room.roomData = reader.ReadString();
                        room.roomMode = reader.ReadByte();
                        room.maxCount = reader.ReadInt();
                    }
                }
                else if (opcode == Lobby.请求离开房间)
                {
                    Disconnect(clientId);
                }
                else if (opcode == Lobby.同步网络数据)
                {
                    var message = reader.ReadArraySegment();
                    var targetId = reader.ReadInt();
                    if (clients.TryGetValue(clientId, out var room) && room != null)
                    {
                        if (message.Count > Transport.GetLength(channel))
                        {
                            Service.Log.Warn("接收消息大小过大！消息大小: {0}".Format(message.Count));
                            Disconnect(clientId);
                            return;
                        }

                        if (room.clientId == clientId)
                        {
                            if (room.clients.Contains(targetId))
                            {
                                using var writer = MemoryWriter.Pop();
                                writer.WriteByte((byte)Lobby.同步网络数据);
                                writer.WriteArraySegment(message);
                                Transport.SendToClient(targetId, writer, channel);
                            }
                        }
                        else
                        {
                            using var writer = MemoryWriter.Pop();
                            writer.WriteByte((byte)Lobby.同步网络数据);
                            writer.WriteArraySegment(message);
                            writer.WriteInt(clientId);
                            Transport.SendToClient(room.clientId, writer, channel);
                        }
                    }
                }
                else if (opcode == Lobby.请求移除玩家)
                {
                    var targetId = reader.ReadInt();
                    var copies = rooms.Values.ToList();
                    foreach (var room in copies)
                    {
                        if (room.clientId == targetId) // 踢掉的是主机
                        {
                            using var writer = MemoryWriter.Pop();
                            writer.WriteByte((byte)Lobby.离开房间成功);
                            foreach (var client in room.clients)
                            {
                                Transport.SendToClient(client, writer);
                                clients.Remove(client);
                            }

                            room.clients.Clear();
                            rooms.Remove(room.roomId);
                            clients.Remove(targetId);
                            return;
                        }

                        if (room.clientId == clientId) // 踢掉的是客户端
                        {
                            if (room.clients.Remove(targetId))
                            {
                                using var writer = MemoryWriter.Pop();
                                writer.WriteByte((byte)Lobby.断开玩家连接);
                                writer.WriteInt(targetId);
                                Transport.SendToClient(room.clientId, writer);
                                clients.Remove(targetId);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Service.Log.Error(e.ToString());
                Transport.Disconnect(clientId);
            }
        }
    }
}