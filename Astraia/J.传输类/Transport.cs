// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 20:08:13
// # Recently: 2026-08-15 17:54:37
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

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