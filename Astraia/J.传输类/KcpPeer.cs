namespace Astraia;

internal sealed class KcpPeer(KcpClientEvent Event, string userName)
{
    private readonly byte[] rawSendBuffer = new byte[Const.MTU_DEF];
    private readonly byte[] kcpSendBuffer = new byte[Const.KCP_LEN + 1];
    private readonly byte[] kcpDataBuffer = new byte[Const.KCP_LEN + 1];
    private readonly KcpData kcpData = new();
    private readonly Stopwatch watch = new();

    private State state;
    private int pingTime;
    private int nextTime;
    private int waitTime;
    private int userData;

    public unsafe void Rebuild()
    {
        pingTime = 0;
        nextTime = 0;
        waitTime = Const.WAIT_TIME;
        kcpData.Build(SendReliable);
        state = State.Running;
        watch.Restart();
    }

    private unsafe void SendReliable(byte* bytes, int count) // pass(1) + userData(4) + header(24) + opcode(1) + data
    {
        rawSendBuffer[0] = Pass.KCP;
        Common.Encode(rawSendBuffer, 1, userData);
        fixed (byte* dest = &rawSendBuffer[Const.HEAD_SIZE])
        {
            Buffer.MemoryCopy(bytes, dest, count, count);
        }

        Event.onSend(new ArraySegment<byte>(rawSendBuffer, 0, Const.HEAD_SIZE + count));
    }

    public void Handshake(int userData)
    {
        this.userData = userData;
        SendReliable(Opcode.握手, BitConverter.GetBytes(userData));
    }

    private bool TryReceive(out Opcode message, out ArraySegment<byte> segment)
    {
        segment = default;
        message = Opcode.断连;
        var count = kcpData.PeekSize();
        if (count <= 0)
        {
            return false;
        }

        if (count > kcpDataBuffer.Length)
        {
            Event.onError(Error.无效接收, $"{userName}接收网络消息过大。消息大小: {kcpDataBuffer.Length} < {count}。");
            Disconnect();
            return false;
        }

        if (kcpData.Receive(kcpDataBuffer, count) < 0)
        {
            Event.onError(Error.无效接收, $"{userName}接收网络消息失败。");
            Disconnect();
            return false;
        }

        message = (Opcode)kcpDataBuffer[0];
        segment = new ArraySegment<byte>(kcpDataBuffer, 1, count - 1);
        nextTime = (int)watch.ElapsedMilliseconds;
        return true;
    }

    public void Input(ArraySegment<byte> segment)
    {
        if (segment.Count <= Const.HEAD_SIZE)
        {
            return;
        }

        var pass = segment.Array![segment.Offset];
        var readData = Common.Decode(segment.Array, segment.Offset + 1);
        if (state == State.Success && readData != userData)
        {
            Log.Warn($"{userName}数据校验失败。旧: {userData} 新: {readData}");
            return;
        }

        var message = new ArraySegment<byte>(segment.Array, segment.Offset + Const.HEAD_SIZE, segment.Count - Const.HEAD_SIZE);
        if (pass == Pass.KCP)
        {
            if (kcpData.Input(message.Array, message.Offset, message.Count) != 0)
            {
                Log.Warn($"{userName}发送可靠消息失败。消息大小: {message.Count - 1}");
            }
        }
        else if (pass == Pass.UDP)
        {
            if (state == State.Success)
            {
                Event.onReceive(message, Pass.UDP);
                nextTime = (int)watch.ElapsedMilliseconds;
            }
        }
    }

    private void SendReliable(Opcode message, ArraySegment<byte> segment = default)
    {
        if (segment.Count > Const.KCP_LEN)
        {
            Event.onError(Error.无效发送, $"{userName}发送网络消息过大。消息大小: {segment.Count} < {Const.KCP_LEN}");
            return;
        }

        kcpSendBuffer[0] = (byte)message;
        if (segment.Count > 0)
        {
            Buffer.BlockCopy(segment.Array!, segment.Offset, kcpSendBuffer, 1, segment.Count);
        }

        if (kcpData.Send(kcpSendBuffer, 0, segment.Count + 1) < 0)
        {
            Event.onError(Error.无效发送, $"{userName}发送网络消息失败。消息大小: {segment.Count}。");
        }
    }

    private void SendUnreliable(ArraySegment<byte> segment)
    {
        if (segment.Count > Const.UDP_LEN)
        {
            Event.onError(Error.无效发送, $"{userName}发送网络消息过大。消息大小: {segment.Count} < {Const.UDP_LEN}");
            return;
        }

        rawSendBuffer[0] = Pass.UDP;
        Common.Encode(rawSendBuffer, 1, userData);
        if (segment.Count > 0)
        {
            Buffer.BlockCopy(segment.Array!, segment.Offset, rawSendBuffer, Const.HEAD_SIZE, segment.Count);
        }

        Event.onSend(new ArraySegment<byte>(rawSendBuffer, 0, segment.Count + Const.HEAD_SIZE));
    }

    public void SendData(ArraySegment<byte> segment, int pass)
    {
        if (segment.Count == 0)
        {
            Event.onError(Error.无效发送, $"{userName}尝试发送空消息。");
            Disconnect();
            return;
        }

        switch (pass)
        {
            case Pass.KCP:
                SendReliable(Opcode.数据, segment);
                break;
            case Pass.UDP:
                SendUnreliable(segment);
                break;
        }
    }

    public void Disconnect()
    {
        if (state == State.Failure)
        {
            return;
        }

        try
        {
            SendReliable(Opcode.断连);
            kcpData.Flush();
        }
        finally
        {
            state = State.Failure;
            Event.onDisconnect(userData);
        }
    }

    private void BeforeReceive()
    {
        var sinceTime = (int)watch.ElapsedMilliseconds;
        if (sinceTime >= nextTime + waitTime)
        {
            Event.onError(Error.连接超时, $"{userName}在{waitTime / 1000}秒内没有收到任何消息后的连接超时！");
            Disconnect();
            return;
        }

        if (kcpData.State == unchecked((uint)-1))
        {
            Event.onError(Error.连接超时, $"{userName}网络消息被重传了{kcpData.Death}次而没有得到确认！");
            Disconnect();
            return;
        }

        if (sinceTime >= pingTime + Const.PING_TIME)
        {
            SendReliable(Opcode.心跳);
            pingTime = sinceTime;
        }

        if (kcpData.Count >= 10000)
        {
            Event.onError(Error.网络拥塞, $"{userName}断开连接，因为它处理数据的速度不够快！");
            Disconnect();
        }
    }

    private void UpdateConnect()
    {
        BeforeReceive();
        if (TryReceive(out var message, out var segment))
        {
            switch (message)
            {
                case Opcode.握手 when segment.Count != 4:
                    Event.onError(Error.无效接收, $"{userName}接收无效的网络消息。消息类型: {message}");
                    Disconnect();
                    return;
                case Opcode.握手:
                    state = State.Success;
                    userData = Common.Decode(segment.Array, segment.Offset);
                    Event.onConnect(userData);
                    break;
                case Opcode.数据:
                    Event.onError(Error.无效接收, $"{userName}接收无效的网络消息。消息类型: {message}");
                    Disconnect();
                    break;
                case Opcode.断连:
                    Disconnect();
                    break;
            }
        }
    }

    private void UpdateConnected()
    {
        BeforeReceive();
        while (TryReceive(out var message, out var segment))
        {
            switch (message)
            {
                case Opcode.握手:
                    Event.onError(Error.无效接收, $"{userName}接收无效的网络消息。消息类型: {message}");
                    Disconnect();
                    break;
                case Opcode.数据 when segment.Count == 0:
                    Event.onError(Error.无效接收, $"{userName}接收无效的网络消息。消息类型: {message}");
                    Disconnect();
                    break;
                case Opcode.数据:
                    Event.onReceive(segment, Pass.KCP);
                    break;
                case Opcode.断连:
                    Disconnect();
                    break;
            }
        }
    }

    public void EarlyUpdate()
    {
        try
        {
            switch (state)
            {
                case State.Running:
                    UpdateConnect();
                    break;
                case State.Success:
                    UpdateConnected();
                    break;
            }
        }
        catch (SocketException e)
        {
            Event.onError(Error.连接关闭, $"{userName}网络发生异常，断开连接。\n{e}");
            Disconnect();
        }
        catch (ObjectDisposedException e)
        {
            Event.onError(Error.连接关闭, $"{userName}网络发生异常，断开连接。\n{e}");
            Disconnect();
        }
        catch (Exception e)
        {
            Event.onError(Error.未知异常, $"{userName}网络发生异常，断开连接。\n{e}");
            Disconnect();
        }
    }

    public void AfterUpdate()
    {
        try
        {
            if (state != State.Failure)
            {
                kcpData.Update((uint)watch.ElapsedMilliseconds);
            }
        }
        catch (SocketException e)
        {
            Event.onError(Error.连接关闭, $"{userName}网络发生异常，断开连接。\n{e}");
            Disconnect();
        }
        catch (ObjectDisposedException e)
        {
            Event.onError(Error.连接关闭, $"{userName}网络发生异常，断开连接。\n{e}");
            Disconnect();
        }
        catch (Exception e)
        {
            Event.onError(Error.未知异常, $"{userName}网络发生异常，断开连接。\n{e}");
            Disconnect();
        }
    }
}