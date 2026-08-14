// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 21:08:17
// # Recently: 2026-08-14 21:39:17
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

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
