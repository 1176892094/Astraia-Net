// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-10 21:01:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Astraia.Net
{
    internal static class Program
    {
        public static NetworkTransport Transport;
        public static Setting Setting;

        public static void Main(string[] args)
        {
            StartAsync(args).GetAwaiter().GetResult();
        }

        private static async Task StartAsync(string[] args)
        {
            Service.Log.Setup(Info, Warn, Error);
            Transport = new NetworkTransport();
            Transport.Awake();
            try
            {
                Service.Log.Info("运行服务器...");
                if (!File.Exists("setting.json"))
                {
                    var setting = JsonConvert.SerializeObject(new Setting(), Formatting.Indented);
                    File.WriteAllText("setting.json", setting);
                }

                Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json"));

                Service.Log.Info("加载程序集...");
                Assembly.LoadFile(Path.GetFullPath("Astraia.dll"));
                Assembly.LoadFile(Path.GetFullPath("Astraia.Kcp.dll"));

                Service.Log.Info("传输初始化...");
                var port = Setting.ServerPort;
                if (args.Length > 0 && ushort.TryParse(args[0], out var result))
                {
                    port = result;
                }

                Transport.port = port;
                Transport.server.Connect = Common.Connect;
                Transport.server.Receive = Common.Receive;
                Transport.server.Disconnect = Common.Disconnect;
                Transport.StartServer();

                Service.Http.Start(port, HttpThread);
                Service.Log.Info("开始进行传输...");
            }
            catch (Exception e)
            {
                Service.Log.Error(e.ToString());
                Console.ReadKey();
                Environment.Exit(0);
                return;
            }

            while (true)
            {
                try
                {
                    Transport.Update();
                    await Task.Delay(10);
                }
                catch (Exception e)
                {
                    Service.Log.Warn(e.ToString());
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
            if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/api/compressed/servers")
            {
                var readJson = JsonConvert.SerializeObject(Common.Rooms);
                readJson = Service.Zip.Compress(readJson);
                var readBytes = Service.Text.GetBytes(readJson);
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "text/plain; charset=utf-8";
                response.ContentLength64 = readBytes.Length;
                await response.OutputStream.WriteAsync(readBytes, 0, readBytes.Length);
            }
        }
    }
}