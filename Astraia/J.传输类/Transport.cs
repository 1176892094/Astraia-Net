namespace Astraia;

[Serializable]
internal abstract class Transport
{
    public string address = "localhost";
    public ushort port = 20974;

    public KcpClientEvent client = new KcpClientEvent();
    public KcpServerEvent server = new KcpServerEvent();

    public abstract void Start(bool isRemote);
    public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP);
    public abstract void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP);
    public abstract void StartServer();
    public abstract void StopServer();
    public abstract void Disconnect(int clientId);
    public abstract void StartClient();
    public abstract void Disconnect();
    public abstract void ClientEarlyUpdate();
    public abstract void ClientAfterUpdate();
    public abstract void ServerEarlyUpdate();
    public abstract void ServerAfterUpdate();
}

[Serializable]
internal sealed class NetworkTransport : Transport
{
    private KcpClient kcpClient;
    private KcpServer kcpServer;

    public override void Start(bool isRemote)
    {
        kcpClient = new KcpClient(client, new byte[Const.MTU_DEF]);
        kcpServer = new KcpServer(server, new byte[Const.MTU_DEF]);
        if (isRemote)
        {
            server.onError = OnServerError;
        }
        else
        {
            client.onError = OnClientError;
        }
    }

    private static void OnServerError(int clientId, Error error, string message)
    {
        if (error != Error.解析失败 && error != Error.连接超时)
        {
            Log.Warn($"客户端: {clientId}  错误代码: {error}\n{message}");
        }
    }

    private static void OnClientError(Error error, string message)
    {
        Log.Warn($"错误代码: {error}\n{message}");
    }

    public override void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP)
    {
        kcpServer.Send(clientId, segment, pass);
    }

    public override void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP)
    {
        kcpClient.Send(segment, pass);
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

    public override void Disconnect()
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