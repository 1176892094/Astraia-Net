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
using System.Reflection;
using Newtonsoft.Json;

namespace Astraia.Net
{
    internal class Program
    {
        private static Setting Setting;

        private static void Main(string[] args)
        {
            new Program().StartServer();
        }

        private void StartServer()
        {
            Logs.Info = Info;
            Logs.Warn = Warn;
            Logs.Error = Error;
            try
            {
                Logs.Info("运行服务器...");
                if (!File.Exists("service.json"))
                {
                    var contents = JsonConvert.SerializeObject(new Setting(), Formatting.Indented);
                    File.WriteAllText("service.json", contents);
                    Logs.Warn("请将 service.json 文件配置正确并重新运行。");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return;
                }

                Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("service.json"));
                Logs.Info("加载程序集...");
                Assembly.LoadFile(Path.GetFullPath("Astraia.dll"));
                Assembly.LoadFile(Path.GetFullPath("Astraia.Kcp.dll"));

                Logs.Info("开始进行传输...");
                if (Setting.UseEndPoint)
                {
                    Logs.Info("开启REST服务...");
                    if (!RestUtility.StartServer(Setting.RestPort))
                    {
                        Logs.Error("请以管理员身份运行或检查端口是否被占用。");
                    }
                }

                Console.ReadKey();
            }
            catch (Exception e)
            {
                Logs.Error(e.ToString());
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

        internal static string Login(LoginRequest request)
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
                    Logs.Info("用户 {0} 数据更新成功。耗时 {1} 秒".Format(response.userName, watch.ElapsedMilliseconds / 1000F));
                }

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception e)
            {
                Logs.Error(e.Message);
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
                if (dataTable.recordTime.Ticks > request.settingManager.recordTime)
                {
                    Logs.Error("用户 {0} 数据更新失败！".Format(dataTable.userName));
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
                Logs.Warn(request.settingManager.deviceData);
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
    }
}