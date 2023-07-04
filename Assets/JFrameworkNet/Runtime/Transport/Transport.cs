using System;
using JFramework.Udp;
using UnityEngine;

namespace JFramework.Net
{
    internal abstract class Transport : MonoBehaviour
    {
        public static Transport Instance;

        /// <summary>
        /// 连接地址
        /// </summary>
        public Address address = new Address("localhost", 20974);

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public static Action OnClientConnected;

        /// <summary>
        /// 客户端断开事件
        /// </summary>
        public static Action OnClientDisconnected;

        /// <summary>
        /// 客户端传输事件
        /// </summary>
        protected static Action<ArraySegment<byte>, Channel> OnClientSend;

        /// <summary>
        /// 客户端接收事件
        /// </summary>
        public static Action<ArraySegment<byte>, Channel> OnClientReceive;

        /// <summary>
        /// 客户端连接到服务器的事件
        /// </summary>
        public static Action<int> OnServerConnected;

        /// <summary>
        /// 客户端从服务器断开的事件
        /// </summary>
        public static Action<int> OnServerDisconnected;

        /// <summary>
        /// 服务器向客户端传输的事件
        /// </summary>
        protected static Action<int, ArraySegment<byte>, Channel> OnServerSend;

        /// <summary>
        /// 服务器接收客户端消息的事件
        /// </summary>
        public static Action<int, ArraySegment<byte>, Channel> OnServerReceive;

        /// <summary>
        /// 根据地址连接
        /// </summary>
        /// <param name="address">传入地址(IP + 端口号)</param>
        public abstract void ClientConnect(Address address);

        /// <summary>
        /// 根据Uri连接
        /// </summary>
        /// <param name="uri">传入Uri</param>
        public abstract void ClientConnect(Uri uri);

        /// <summary>
        /// 客户端向服务器传输信息
        /// </summary>
        /// <param name="segment">传入发送的数据</param>
        /// <param name="channel">传入通道</param>
        public abstract void ClientSend(ArraySegment<byte> segment, Channel channel);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public abstract void ClientDisconnect();

        /// <summary>
        /// 当服务器连接
        /// </summary>
        public abstract void ServerStart();

        /// <summary>
        /// 服务器传输信息给客户端
        /// </summary>
        public abstract void ServerSend(int clientId, ArraySegment<byte> segment, Channel channel);

        /// <summary>
        /// 服务器断开指定客户端连接
        /// </summary>
        /// <param name="clientId">传入要断开的客户端Id</param>
        public abstract void ServerDisconnect(int clientId);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public abstract int GetMaxPacketSize(Channel channel = Channel.Reliable);
        
        /// <summary>
        /// 网络消息合批阈值
        /// </summary>
        /// <returns>返回阈值</returns>
        public abstract int GetBatchThreshold();

        /// <summary>
        /// 当服务器停止
        /// </summary>
        public abstract void ServerStop();

        /// <summary>
        /// 客户端Update之前
        /// </summary>
        public abstract void ClientEarlyUpdate();

        /// <summary>
        /// 客户端Update之后
        /// </summary>
        public abstract void ClientAfterUpdate();

        /// <summary>
        /// 服务器Update之前
        /// </summary>
        public abstract void ServerEarlyUpdate();

        /// <summary>
        /// 服务器Update之后
        /// </summary>
        public abstract void ServerAfterUpdate();

        /// <summary>
        /// 运行初始化
        /// </summary>
        public static void RuntimeInitializeOnLoad()
        {
            OnClientConnected = null;
            OnClientDisconnected = null;
            OnClientSend = null;
            OnClientReceive = null;
            OnServerConnected = null;
            OnServerDisconnected = null;
            OnServerReceive = null;
            OnServerSend = null;
        }
    }
}