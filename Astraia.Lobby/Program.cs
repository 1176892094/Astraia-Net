using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;

namespace Astraia;

internal static class Program
{
    private static readonly Dictionary<string, Lobby> rooms = new Dictionary<string, Lobby>();
    private static readonly Dictionary<int, Lobby> clients = new Dictionary<int, Lobby>();
    private static readonly HashSet<int> connections = new HashSet<int>();
    private static readonly Queue<int> indices = new Queue<int>();

    private static Transport connection;
    private static Setting setting;
    private static int counter;

    public static void Main(string[] args)
    {
        StartAsync(args).GetAwaiter().GetResult();
    }

    private static async Task StartAsync(string[] args)
    {
        Log.Setup(Info, Warn, Error);
        try
        {
            connection = new NetworkTransport();
            connection.Start(true);
            Log.Info("运行服务器...");

            var option = new JsonSerializerOptions { IncludeFields = true };
            if (!File.Exists("setting.json"))
            {
                var saveText = JsonSerializer.Serialize(new Setting(), option);
                await File.WriteAllTextAsync("setting.json", saveText);
            }

            var readText = await File.ReadAllTextAsync("setting.json");
            setting = JsonSerializer.Deserialize<Setting>(readText, option);

            Log.Info("服务器密钥：" + setting.ServerId);

            Assembly.LoadFile(Path.GetFullPath("Astraia.dll"));
            Log.Info("加载程序集...");

            var port = setting.ServerPort;
            if (args.Length > 0 && ushort.TryParse(args[0], out var result))
            {
                port = result;
            }

            connection.port = port;
            connection.server.onConnect = Connect;
            connection.server.onReceive = Receive;
            connection.server.onDisconnect = Disconnect;
            connection.StartServer();
            Log.Info("传输初始化...");

            Host.Start("http://*:{0}/".Format(port), HttpThread);
            Log.Info("开始进行传输...");
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            Console.ReadKey();
            Environment.Exit(0);
            return;
        }

        while (true)
        {
            try
            {
                connection.ServerEarlyUpdate();
                connection.ServerAfterUpdate();
                await Task.Delay(10);
            }
            catch (Exception e)
            {
                Log.Warn(e.ToString());
            }
        }
    }

    private static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[{0}] {1}".Format(DateTime.Now.ToString("MM-dd HH:mm:ss"), message));
    }

    private static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[{0}] {1}".Format(DateTime.Now.ToString("MM-dd HH:mm:ss"), message));
    }

    private static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[{0}] {1}".Format(DateTime.Now.ToString("MM-dd HH:mm:ss"), message));
    }

    private static async Task HttpThread(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (request.HttpMethod == "GET" && request.Url!.AbsolutePath == "/api/compressed/servers")
        {
            var serializer = new XmlSerializer(Rooms.GetType());
            await using var writer = new StringWriter();
            serializer.Serialize(writer, Rooms);

            var xml = Zip.Compress(writer.ToString());
            var readBytes = Text.GetBytes(xml);

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/xml; charset=utf-8";
            response.ContentLength64 = readBytes.Length;

            await response.OutputStream.WriteAsync(readBytes, 0, readBytes.Length);
        }
    }

    private class Setting
    {
        public string ServerId = Guid.NewGuid().ToString();
        public ushort ServerPort = 8080;
    }

    public static List<Lobby> Rooms => rooms.Values.ToList();

    public static void Connect(int clientId)
    {
        connections.Add(clientId);
        using var writer = MemoryWriter.Pop();
        writer.WriteByte((byte)Lobby.Info.身份验证成功);
        connection.SendToClient(clientId, writer);
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
                    if (serverId == setting.ServerId)
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.Info.进入大厅成功);
                        connection.SendToClient(clientId, writer);
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
                connection.SendToClient(clientId, writer);
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
                    connection.SendToClient(clientId, writer);
                    connection.SendToClient(room.Host, writer);
                }
                else
                {
                    using var writer = MemoryWriter.Pop();
                    writer.WriteByte((byte)Lobby.Info.离开房间成功);
                    connection.SendToClient(clientId, writer);
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
                            connection.SendToClient(agentId, writer, channel);
                        }
                    }
                    else
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.Info.同步网络数据);
                        writer.WriteArraySegment(message);
                        writer.WriteInt32(clientId);
                        connection.SendToClient(room.Host, writer, channel);
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
            connection.Disconnect(clientId);
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
                    connection.SendToClient(member, writer);
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
                connection.SendToClient(room.Host, writer);
                clients.Remove(clientId);
            }
        }
    }
}