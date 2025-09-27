// *********************************************************************************
// # Project: SQLServer
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-09-01 20:09:11
// # Recently: 2025-02-11 00:02:13
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace Astraia.Net
{
    internal class Program
    {
        private static Setting Setting;
        private static Timer cleanupTimer;

        private static void Main(string[] args)
        {
            new Program().StartServer();
        }

        private void StartServer()
        {
            Log.Setup(Info, Warn, Error);
            try
            {
                Log.Info("运行服务器...");
                if (!File.Exists("service.json"))
                {
                    var contents = JsonConvert.SerializeObject(new Setting(), Formatting.Indented);
                    File.WriteAllText("service.json", contents);
                    Log.Warn("请将 service.json 文件配置正确并重新运行。");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return;
                }

                Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("service.json"));
                Log.Info("加载程序集...");
                Assembly.LoadFile(Path.GetFullPath("Astraia.dll"));
                Assembly.LoadFile(Path.GetFullPath("Astraia.Kcp.dll"));

                Log.Info("开始进行传输...");
                SetupDailyCleanup();
                CleanupInactivePlayers();
                HttpServer.StartServer(Setting.HttpPort, HttpThread);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                Console.ReadKey();
                Environment.Exit(0);
            }

            return;

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

        private static async Task HttpThread(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/server/login")
            {
                byte[] readBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(memoryStream);
                    readBytes = memoryStream.ToArray();
                }

                readBytes = Service.Xor.Decrypt(readBytes);
                var readJson = Service.Text.GetString(readBytes);
                readJson = Service.Zip.Decompress(readJson);
                var result = JsonConvert.DeserializeObject<LoginRequest>(readJson);
                readJson = Service.Zip.Compress(Login(result));
                readBytes = Service.Text.GetBytes(readJson);
                readBytes = Service.Xor.Encrypt(readBytes);
                response.ContentType = "application/octet-stream";
                response.ContentLength64 = readBytes.Length;
                await response.OutputStream.WriteAsync(readBytes, 0, readBytes.Length);
            }
        }

        private static string Login(LoginRequest request)
        {
            var watch = new Stopwatch();
            watch.Start();
            var database = Setting.GetConnection(request.username, request.password);
            var response = new LoginResponse();
            if (string.IsNullOrEmpty(database))
            {
                return JsonConvert.SerializeObject(response);
            }

            try
            {
                var connection = new Command(database);
                if (!string.IsNullOrEmpty(request.settingManager.deviceData))
                {
                    response = UpdateLoginTime(connection, request, response);
                }

                if (response.codeData == 0)
                {
                    if (response.userData < 0)
                    {
                        response.userData = 0;
                    }
                    else if (response.userData < request.settingManager.userData)
                    {
                        response.userData = request.settingManager.userData;
                    }

                    var userName = UpdateOrInsert(connection, request, response.userData);
                    if (userName != 0)
                    {
                        response.userName = userName;
                    }

                    watch.Stop();
                    Log.Info("用户 {0} 数据更新成功。耗时 {1} 秒".Format(response.userName, watch.ElapsedMilliseconds / 1000F));
                }

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                response.codeData = 2;
                return JsonConvert.SerializeObject(response);
            }
        }

        private static LoginResponse UpdateLoginTime(Command connection, LoginRequest request, LoginResponse response)
        {
            var parameter = new Dictionary<string, object> { { "@deviceData", request.settingManager.deviceData } };
            var dataTables = Process.Select<LoginTable>(connection, "deviceData = @deviceData", parameter);
            foreach (var dataTable in dataTables)
            {
                if (dataTable.recordTime > DateTime.Parse(request.settingManager.recordTime))
                {
                    Log.Error("用户 {0} 数据更新失败！".Format(dataTable.userName));
                    response.codeData = 1;
                }

                response.userName = dataTable.userName;
                response.userData = dataTable.userData;
                break;
            }


            return response;
        }

        private static long UpdateOrInsert(Command connection, LoginRequest request, long userData)
        {
            var parameters = new Dictionary<string, object> { { "@deviceData", request.settingManager.deviceData } };
            var dataTables = Process.Select<LoginTable>(connection, "deviceData = @deviceData", parameters);
            if (dataTables.Count == 0)
            {
                Log.Warn(request.settingManager.deviceData);
                Process.Insert<LoginTable>(connection, new Dictionary<string, object>
                {
                    { "deviceData", request.settingManager.deviceData },
                    { "userData", userData },
                    { "settingManager", JsonConvert.SerializeObject(request.settingManager) },
                    { "playerData1", JsonConvert.SerializeObject(request.playerData1) },
                    { "playerData2", JsonConvert.SerializeObject(request.playerData2) },
                    { "playerData3", JsonConvert.SerializeObject(request.playerData3) },
                    { "playerData4", JsonConvert.SerializeObject(request.playerData4) },
                });
                dataTables = Process.Select<LoginTable>(connection, "deviceData = @deviceData", parameters);
                return dataTables.Select(dataTable => dataTable.userName).FirstOrDefault();
            }

            var parameter = new KeyValuePair<string, object>("deviceData", request.settingManager.deviceData);
            Process.Update<LoginTable>(connection, parameter, new Dictionary<string, object>
            {
                { "userData", userData },
                { "settingManager", JsonConvert.SerializeObject(request.settingManager) },
                { "playerData1", JsonConvert.SerializeObject(request.playerData1) },
                { "playerData2", JsonConvert.SerializeObject(request.playerData2) },
                { "playerData3", JsonConvert.SerializeObject(request.playerData3) },
                { "playerData4", JsonConvert.SerializeObject(request.playerData4) },
                { "updateTime", DateTime.Now },
            });
            return 0;
        }


        private void SetupDailyCleanup()
        {
            cleanupTimer = new Timer();
            cleanupTimer.AutoReset = true;

            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var initialInterval = (nextMidnight - now).TotalMilliseconds;

            cleanupTimer.Interval = initialInterval;
            cleanupTimer.Elapsed += (sender, e) =>
            {
                CleanupInactivePlayers();
                cleanupTimer.Interval = 24 * 60 * 60 * 1000;
            };
            cleanupTimer.Start();
        }

        private static void CleanupInactivePlayers()
        {
            try
            {
                var database = Setting.GetConnection(Setting.Username, Setting.Password);
                var connection = new Command(database);

                var parameters = new Dictionary<string, object> { { "@threshold", DateTime.Now.AddDays(-7) } };
                var inactivePlayers = Process.Select<LoginTable>(connection, "recordTime < @threshold", parameters);

                var deletedCount = 0;
                foreach (var player in inactivePlayers)
                {
                    Process.Delete<LoginTable>(connection, player.deviceData);
                    deletedCount++;
                }

                Log.Info("清理 {0} 名 7 天未登录玩家。".Format(deletedCount));
            }
            catch (Exception e)
            {
                Log.Error("清理未登录玩家失败：{0}".Format(e));
            }
        }
    }
}