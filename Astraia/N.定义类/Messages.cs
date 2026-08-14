// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-15 03:08:40
// # Recently: 2026-08-15 03:28:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

public readonly record struct LobbyDisconnect : IEvent;

public readonly record struct LobbyUpdate(List<Lobby> rooms) : IEvent;

public readonly record struct LobbyCreate(int index, string address) : IEvent;

public readonly record struct ServerConnect(int id) : IEvent;

public readonly record struct ServerDisconnect(int id) : IEvent;

public readonly record struct ServerReady(int id) : IEvent;

public readonly record struct ClientConnect : IEvent;

public readonly record struct ClientDisconnect : IEvent;

public readonly record struct ServerLoadScene(string sceneName) : IEvent;

public readonly record struct ServerSceneLoaded(string sceneName) : IEvent;

public readonly record struct ClientLoadScene(string sceneName) : IEvent;

public readonly record struct ClientSceneLoaded(string sceneName) : IEvent;

public readonly record struct ServerResponse(string address, ushort port) : IEvent;

public readonly record struct PingUpdate(double pingTime) : IEvent;



