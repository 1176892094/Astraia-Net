// *********************************************************************************
// # Project: SQLServer
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-25 18:11:14
// # Recently: 2025-02-11 00:02:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia.Net
{
    internal class LoginTable
    {
        [Key] [Column] public string deviceData { get; set; }

        [Column] public long userName { get; set; }

        [Column] public long userData { get; set; }

        [Column] public DateTime recordTime { get; set; }

        [Column] public DateTime updateTime { get; set; }

        [Column] public DateTime createTime { get; set; }

        [Column] public string settingManager { get; set; }

        [Column] public string playerData1 { get; set; }

        [Column] public string playerData2 { get; set; }

        [Column] public string playerData3 { get; set; }

        [Column] public string playerData4 { get; set; }
    }
}