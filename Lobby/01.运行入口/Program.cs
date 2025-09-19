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
    internal class Program
    {
        public static Setting Setting;
        public static Process Process;

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            Logs.Info = Info;
            Logs.Warn = Warn;
            Logs.Error = Error;
            var transport = new Transport();
            transport.Awake();
            try
            {
                Logs.Info("运行服务器...");
                if (!File.Exists("setting.json"))
                {
                    var contents = JsonConvert.SerializeObject(new Setting(), Formatting.Indented);
                    File.WriteAllText("setting.json", contents);

                    Logs.Warn("请将 setting.json 文件配置正确并重新运行。");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return;
                }

                Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json"));

                Logs.Info("加载程序集...");
                Assembly.LoadFile(Path.GetFullPath("Astraia.dll"));
                Assembly.LoadFile(Path.GetFullPath("Astraia.Kcp.dll"));

                Logs.Info("初始化传输类...");
                Process = new Process(transport);

                transport.port = Setting.RestPort;
                transport.OnServerConnect = Process.ServerConnect;
                transport.OnServerReceive = Process.ServerReceive;
                transport.OnServerDisconnect = Process.ServerDisconnect;
                transport.StartServer();

                Logs.Info("开始进行传输...");
                RoomRequest();
            }
            catch (Exception e)
            {
                Logs.Error(e.ToString());
                Console.ReadKey();
                Environment.Exit(0);
            }


            while (true)
            {
                transport.Update();
                await Task.Delay(Setting.UpdateTime);
            }

            void Info(string message)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[{0}] {1}".Format(DateTime.Now.ToString("MM-dd HH:mm:ss"), message));
            }

            void Warn(string message)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[{0}] {1}".Format(DateTime.Now.ToString("MM-dd HH:mm:ss"), message));
            }

            void Error(string message)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[{0}] {1}".Format(DateTime.Now.ToString("MM-dd HH:mm:ss"), message));
            }
        }

        private void RoomRequest()
        {
            Task.Run(async () =>
            {
                var service = new HttpListener();
                service.Prefixes.Add("http://*:{0}/".Format(Setting.RestPort));
                service.Start();
                while (true)
                {
                    var context = await service.GetContextAsync(); // 异步等待请求
                    HandleRequest(context);
                }
            });
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            Task.Run(async () => // 每个请求单独处理
            {
                var request = context.Request;
                var response = context.Response;
                if (request.HttpMethod == "Get" && request.Url.AbsolutePath == "/api/compressed/servers")
                {
                    if (Setting.RequestRoom)
                    {
                        var json = JsonConvert.SerializeObject(Process.roomData);
                        json = Service.Zip.Compress(json);
                        var buffer = Service.Text.GetBytes(json);
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentType = "text/plain; charset=utf-8";
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    }
                }

                response.OutputStream.Close();
            });
        }
    }
}