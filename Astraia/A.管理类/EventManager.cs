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

public static class EventManager
{
    internal static readonly Dictionary<Type, IPool> poolData = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Listen<T>(IEvent<T> data) where T : struct, IEvent
    {
        LoadPool<T>().Listen(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove<T>(IEvent<T> data) where T : struct, IEvent
    {
        LoadPool<T>().Remove(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<T>(T data) where T : struct, IEvent
    {
        LoadPool<T>().Invoke(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Pool<T> LoadPool<T>() where T : struct, IEvent
    {
        if (!poolData.TryGetValue(typeof(T), out var pool))
        {
            pool = new Pool<T>(typeof(T));
            poolData.Add(typeof(T), pool);
        }

        return (Pool<T>)pool;
    }

    internal static void Dispose()
    {
        foreach (var item in poolData.Values)
        {
            item.Dispose();
        }

        poolData.Clear();
    }

    private class Pool<T>(Type Type) : IPool where T : struct, IEvent
    {
        private event Action<T> OnExecute;
        public int Acquire { get; private set; }
        public int Release { get; private set; }
        public int Dequeue { get; private set; }
        public int Enqueue { get; private set; }

        public void Listen(IEvent<T> obj)
        {
            Dequeue++;
            Acquire++;
            OnExecute += obj.Execute;
        }

        public void Remove(IEvent<T> obj)
        {
            Enqueue++;
            Acquire--;
            OnExecute -= obj.Execute;
        }

        public void Invoke(T message)
        {
            Release++;
            OnExecute?.Invoke(message);
        }

        void IDisposable.Dispose()
        {
            OnExecute = null;
        }

        Type IPool.Type => Type;
        string IPool.Path => Type.Name;
    }
}

public interface IEvent;

public interface IEvent<in T> where T : struct, IEvent
{
    void Execute(T message);
}