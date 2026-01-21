// *********************************************************************************
// # Project: Astraia.Lobby
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-08-28 20:08:49
// # Recently: 2024-12-23 00:12:10
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    internal abstract class Transport
    {
        public string address = "localhost";
        public ushort port = 20974;

        public readonly KcpClient.Event client = new KcpClient.Event();
        public readonly KcpServer.Event server = new KcpServer.Event();

        public abstract uint GetLength(int channel);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void Disconnect(int clientId);
        public abstract void StartClient();
        public abstract void StartClient(Uri uri);
        public abstract void StopClient();
        public abstract void ClientEarlyUpdate();
        public abstract void ClientAfterUpdate();
        public abstract void ServerEarlyUpdate();
        public abstract void ServerAfterUpdate();
    }


    internal sealed class NetworkTransport : Transport
    {
        private const uint MAX_MTU = 1200;
        private const uint OVER_TIME = 10000;
        private const uint INTERVAL = 10;
        private const uint DEAD_LINK = 40;
        private const uint FAST_RESEND = 2;
        private const uint SEND_WIN = 1024 * 4;
        private const uint RECEIVE_WIN = 1024 * 4;

        private KcpClient kcpClient;
        private KcpServer kcpServer;

        public void Awake()
        {
            var setting = new Setting(MAX_MTU, OVER_TIME, INTERVAL, DEAD_LINK, FAST_RESEND, SEND_WIN, RECEIVE_WIN);
            kcpClient = new KcpClient(setting, client);
            kcpServer = new KcpServer(setting, server);
            server.Error = OnError;
        }

        private static void OnError(int clientId, Error error, string message)
        {
            if (error != Error.解析失败 && error != Error.连接超时)
            {
                Service.Log.Warn("客户端: {0}  错误代码: {1}\n{2}".Format(clientId, error, message));
            }
        }

        public void Update()
        {
            kcpServer.EarlyUpdate();
            kcpServer.AfterUpdate();
        }

        public override uint GetLength(int channel)
        {
            return channel == Channel.Reliable ? KcpPeer.KcpLength(MAX_MTU, RECEIVE_WIN) : KcpPeer.UdpLength(MAX_MTU);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            kcpServer.Send(clientId, segment, channel);
            server.Send?.Invoke(clientId, segment);
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            kcpClient.Send(segment, channel);
            client.Send?.Invoke(segment);
        }

        public override void StartServer()
        {
            kcpServer.Connect(port);
        }

        public override void StopServer()
        {
            kcpServer.StopServer();
        }

        public override void Disconnect(int clientId)
        {
            kcpServer.Disconnect(clientId);
        }

        public override void StartClient()
        {
            kcpClient.Connect(address, port);
        }

        public override void StartClient(Uri uri)
        {
            kcpClient.Connect(uri.Host, (ushort)(uri.IsDefaultPort ? port : uri.Port));
        }

        public override void StopClient()
        {
            kcpClient.Disconnect();
        }

        public override void ClientEarlyUpdate()
        {
            kcpClient.EarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            kcpClient.AfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            kcpServer.EarlyUpdate();
        }

        public override void ServerAfterUpdate()
        {
            kcpServer.AfterUpdate();
        }
    }
}