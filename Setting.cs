namespace Astraia.Net;

[Serializable]
internal class Setting
{
    /// <summary>
    /// 服务器密钥
    /// </summary>
    public string ServerId = "5bcff8ae-d6b7-4f06-a9ff-04fce17f2331";

    /// <summary>
    /// 服务器端口
    /// </summary>
    public ushort ServerPort = 8080;
}

[Serializable]
internal class Room
{
    /// <summary>
    /// 房间拥有者
    /// </summary>
    public int clientId;

    /// <summary>
    /// 是否显示
    /// </summary>
    public byte roomMode;

    /// <summary>
    /// 房间最大人数
    /// </summary>
    public int maxCount;

    /// <summary>
    /// 额外房间数据
    /// </summary>
    public string roomData;

    /// <summary>
    /// 房间Id
    /// </summary>
    public string roomId;

    /// <summary>
    /// 房间名称
    /// </summary>
    public string roomName;

    /// <summary>
    /// 客户端数量
    /// </summary>
    public HashSet<int> clients;
}