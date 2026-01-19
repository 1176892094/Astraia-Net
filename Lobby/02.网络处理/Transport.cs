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
using Astraia.Common;

namespace Astraia
{
    internal abstract class Transport
    {
        public string address = "localhost";
        public ushort port = 20974;

        public readonly Client.Delegate client = new Client.Delegate();
        public readonly Server.Delegate server = new Server.Delegate();

        public abstract uint GetLength(int channel);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void Disconnect(int clientId);
        public abstract void StartClient();
        public abstract void StartClient(Uri uri);
        public abstract void Disconnect();
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

        private Client clientAgent;
        private Server serverAgent;

        public void Awake()
        {
            var setting = new Setting(MAX_MTU, OVER_TIME, INTERVAL, DEAD_LINK, FAST_RESEND, SEND_WIN, RECEIVE_WIN);
            clientAgent = new Client(setting, client);
            serverAgent = new Server(setting, server);
            server.Error = (clientId, error, message) =>
            {
                if (error != Error.解析失败 && error != Error.连接超时)
                {
                    Service.Log.Warn("客户端: {0}  错误代码: {1}\n{2}".Format(clientId, error, message));
                }
            };
        }

        public void Update()
        {
            serverAgent.EarlyUpdate();
            serverAgent.AfterUpdate();
        }

        public override uint GetLength(int channel)
        {
            return channel == Channel.Reliable ? Peer.KcpLength(MAX_MTU, RECEIVE_WIN) : Peer.UdpLength(MAX_MTU);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            serverAgent.Send(clientId, segment, channel);
            server.Send?.Invoke(clientId, segment, channel);
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            clientAgent.Send(segment, channel);
            client.onSend?.Invoke(segment, channel);
        }

        public override void StartServer()
        {
            serverAgent.Connect(port);
        }

        public override void StopServer()
        {
            serverAgent.StopServer();
        }

        public override void Disconnect(int clientId)
        {
            serverAgent.Disconnect(clientId);
        }

        public override void StartClient()
        {
            clientAgent.Connect(address, port);
        }

        public override void StartClient(Uri uri)
        {
            clientAgent.Connect(uri.Host, (ushort)(uri.IsDefaultPort ? port : uri.Port));
        }

        public override void Disconnect()
        {
            clientAgent.Disconnect();
        }

        public override void ClientEarlyUpdate()
        {
            clientAgent.EarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            clientAgent.AfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            serverAgent.EarlyUpdate();
        }

        public override void ServerAfterUpdate()
        {
            serverAgent.AfterUpdate();
        }
    }

}