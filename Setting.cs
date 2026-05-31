using System;
using System.Collections.Generic;

namespace Astraia.Net;

[Serializable]
internal class Setting
{
    /// <summary>
    /// 服务器密钥
    /// </summary>
    public string ServerId = Guid.NewGuid().ToString();

    /// <summary>
    /// 服务器端口
    /// </summary>
    public ushort ServerPort = 8080;
}

[Serializable]
internal class Room
{
    /// <summary>
    /// 房间所有者
    /// </summary>
    public int Host;

    /// <summary>
    /// 房间最大人数
    /// </summary>
    public int Count;

    /// <summary>
    /// 是否显示
    /// </summary>
    public int State;

    /// <summary>
    /// 房间Id
    /// </summary>
    public string Id;

    /// <summary>
    /// 房间名称
    /// </summary>
    public string Name;

    /// <summary>
    /// 额外房间数据
    /// </summary>
    public string Data;

    /// <summary>
    /// 客户端数量
    /// </summary>
    public List<int> Members;
}