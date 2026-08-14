using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;

namespace Astraia;

internal static class Program
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
    public static Transport Transport;
    public static Setting Setting;

    public static void Main(string[] args)
    {
        StartAsync(args).GetAwaiter().GetResult();
    }

    private static async Task StartAsync(string[] args)
    {
        Log.Setup(Info, Warn, Error);
        Transport = new NetworkTransport();
        Transport.Start(true);
        try
        {
            Log.Info("运行服务器...");
            if (!File.Exists("setting.json"))
            {
                var setting = JsonSerializer.Serialize(new Setting(), Options);
                await File.WriteAllTextAsync("setting.json", setting);
            }

            var readText = await File.ReadAllTextAsync("setting.json");
            Setting = JsonSerializer.Deserialize<Setting>(readText, Options);

            Log.Info("服务器密钥：" + Setting.ServerId);

            Assembly.LoadFile(Path.GetFullPath("Astraia.dll"));
            Log.Info("加载程序集...");

            var port = Setting.ServerPort;
            if (args.Length > 0 && ushort.TryParse(args[0], out var result))
            {
                port = result;
            }

            Transport.port = port;
            Transport.server.onConnect = Service.Connect;
            Transport.server.onReceive = Service.Receive;
            Transport.server.onDisconnect = Service.Disconnect;
            Transport.StartServer();
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
                Transport.ServerEarlyUpdate();
                Transport.ServerAfterUpdate();
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
            var serializer = new XmlSerializer(Service.Rooms.GetType());
            await using var writer = new StringWriter();
            serializer.Serialize(writer, Service.Rooms);

            var xml = Zip.Compress(writer.ToString());
            var readBytes = Text.GetBytes(xml);

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/xml; charset=utf-8";
            response.ContentLength64 = readBytes.Length;

            await response.OutputStream.WriteAsync(readBytes, 0, readBytes.Length);
        }
    }
}