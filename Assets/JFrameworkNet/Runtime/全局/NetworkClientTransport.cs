using System;
using JFramework.Udp;
using UnityEngine;

namespace JFramework.Net
{
    public static partial class NetworkClient
    {
        /// <summary>
        /// 添加传输事件
        /// </summary>
        private static void RegisterTransport()
        {
            UnRegisterTransport();
            Transport.OnClientConnected += OnClientConnected;
            Transport.OnClientDisconnected += OnClientDisconnected;
            Transport.OnClientReceive += OnClientReceive;
        }

        /// <summary>
        /// 移除传输事件
        /// </summary>
        private static void UnRegisterTransport()
        {
            Transport.OnClientConnected -= OnClientConnected;
            Transport.OnClientDisconnected -= OnClientDisconnected;
            Transport.OnClientReceive -= OnClientReceive;
        }

        /// <summary>
        /// 当客户端连接
        /// </summary>
        private static void OnClientConnected()
        {
            if (connection == null)
            {
                Debug.LogError("Skipped connect message handling because server is null.");
                return;
            }

            NetworkTime.RuntimeInitializeOnLoad();
            state = ConnectState.Connected;
            NetworkTime.UpdateClient();
            OnConnected?.Invoke();
        }

        /// <summary>
        /// 当客户端断开连接
        /// </summary>
        private static void OnClientDisconnected()
        {
            if (state != ConnectState.Disconnected)
            {
                state = ConnectState.Disconnected;
                connection = null;
                isReady = false;
                OnDisconnected?.Invoke();
                UnRegisterTransport();
            }
        }

        /// <summary>
        /// 当客户端从服务器接收消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="channel"></param>
        internal static void OnClientReceive(ArraySegment<byte> data, Channel channel)
        {
            if (connection == null)
            {
                Debug.LogError("Skipped message handling because server is null.");
                return;
            }

            if (!readers.ReadEnqueue(data))
            {
                Debug.LogWarning($"Failed to add batch.");
                connection.Disconnect();
                return;
            }

            while (!isLoadScene && readers.ReadDequeue(out var reader, out double timestamp))
            {
                if (reader.Residue >= NetworkConst.MessageSize)
                {
                    connection.timestamp = timestamp;
                    if (!TryInvoke(reader, channel))
                    {
                        Debug.LogWarning($"Failed to unpack and invoke message.");
                        connection.Disconnect();
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning($"messages should start with message id.");
                    connection.Disconnect();
                    return;
                }
            }

            if (!isLoadScene && readers.Count > 0)
            {
                Debug.LogError($"Still had {readers.Count} batches remaining after processing.\n");
            }
        }

        /// <summary>
        /// 尝试读取并调用从服务器接收的委托
        /// </summary>
        /// <param name="reader">网络读取器</param>
        /// <param name="channel">传输通道</param>
        /// <returns>返回是否读取成功</returns>
        private static bool TryInvoke(NetworkReader reader, Channel channel)
        {
            if (NetworkEvent.ReadEvent(reader, out ushort id))
            {
                if (events.TryGetValue(id, out EventDelegate handle))
                {
                    handle.Invoke(connection, reader, channel);
                    return true;
                }

                Debug.LogWarning($"Unknown message id: {id}.");
                return false;
            }

            Debug.LogWarning("Invalid message header.");
            return false;
        }
    }
}