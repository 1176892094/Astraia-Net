// *********************************************************************************
// # Project: SQLServer
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-09-03 14:09:26
// # Recently: 2025-02-11 00:02:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia.Net
{
    [Serializable]
    internal struct LoginRequest
    {
        public string username;
        public string password;
        public SettingManager settingManager;
        public PlayerData playerData1;
        public PlayerData playerData2;
        public PlayerData playerData3;
        public PlayerData playerData4;
    }

    [Serializable]
    internal struct LoginResponse
    {
        public long userName;
        public long userData;
        public long codeData;
    }

    [Serializable]
    internal struct SettingManager
    {
        public string deviceData;
        public string version;
        public long userData;
        public long userName;
        public long recordTime;
        public long createTime;
        public long targetTime;
    }

    [Serializable]
    internal struct PlayerData
    {
        public string deviceData;
        public string playerName;
        public int modifyData;
        public int playerType;
        public int experience;
        public int coinCache;
        public int coinCount;
        public int woodCache;
        public int woodCount;
    }
}