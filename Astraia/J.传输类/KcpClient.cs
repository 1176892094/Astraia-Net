namespace Astraia;

internal sealed class KcpClient(KcpClientEvent Event, byte[] buffer)
{
    private State state = State.Failure;
    private Socket socket;
    private KcpPeer kcpPeer;
    private EndPoint endPoint;

    public void Connect(string address, ushort port)
    {
        try
        {
            if (state != State.Failure)
            {
                Log.Warn("客户端已经连接!");
                return;
            }

            var addresses = Dns.GetHostAddresses(address);
            if (addresses.Length >= 1)
            {
                Register();
                state = State.Running;
                endPoint = new IPEndPoint(addresses[0], port);
                socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.Blocked();
                socket.Connect(endPoint);
                Log.Info($"客户端连接到: {addresses[0]} : {port}");
                kcpPeer.Handshake(0);
            }
        }
        catch (SocketException e)
        {
            Event.onError(Error.解析失败, $"无法解析主机地址: {address}\n{e}");
            Event.onDisconnect(0);
        }
    }

    public void Send(ArraySegment<byte> segment, int pass)
    {
        if (state != State.Failure)
        {
            kcpPeer.SendData(segment, pass);
            Event.onSend?.Invoke(segment);
        }
    }

    private bool TryReceive(out ArraySegment<byte> segment)
    {
        segment = default;
        try
        {
            if (socket != null && socket.Poll(0, SelectMode.SelectRead))
            {
                var count = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                segment = new ArraySegment<byte>(buffer, 0, count);
                return true;
            }

            return false;
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock)
            {
                Log.Info($"客户端接收消息失败!\n{e}");
                kcpPeer.Disconnect();
            }

            return false;
        }
    }

    public void Disconnect()
    {
        if (state != State.Failure)
        {
            kcpPeer.Disconnect();
        }
    }

    private void Register()
    {
        if (kcpPeer == null)
        {
            var kcpData = new KcpClientEvent();
            kcpPeer = new KcpPeer(kcpData, nameof(KcpClient));
            kcpData.onConnect = OnConnect;
            kcpData.onDisconnect = OnDisconnect;
            kcpData.onError = OnError;
            kcpData.onReceive = OnReceive;
            kcpData.onSend = OnSend;
        }

        kcpPeer.Rebuild();
    }

    private void OnConnect(int serverId)
    {
        Log.Info($"客户端 {serverId} 连接到服务器。");
        state = State.Success;
        Event.onConnect(serverId);
    }

    private void OnDisconnect(int serverId)
    {
        Log.Info($"客户端 {serverId} 从服务器断开。");
        state = State.Failure;
        socket.Close();
        socket = null;
        endPoint = null;
        Event.onDisconnect(serverId);
    }

    private void OnError(Error error, string message)
    {
        Event.onError(error, message);
    }

    private void OnReceive(ArraySegment<byte> segment, int pass)
    {
        Event.onReceive(segment, pass);
    }

    private void OnSend(ArraySegment<byte> segment)
    {
        try
        {
            if (socket != null)
            {
                if (socket.Poll(0, SelectMode.SelectWrite))
                {
                    socket.Send(segment.Array!, segment.Offset, segment.Count, SocketFlags.None);
                }
            }
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock)
            {
                Log.Info($"客户端发送消息失败!\n{e}");
            }
        }
    }

    public void EarlyUpdate()
    {
        if (state != State.Failure)
        {
            while (TryReceive(out var segment))
            {
                kcpPeer.Input(segment);
            }

            kcpPeer.EarlyUpdate();
        }
    }

    public void AfterUpdate()
    {
        if (state != State.Failure)
        {
            kcpPeer.AfterUpdate();
        }
    }
}