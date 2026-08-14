namespace Astraia;

[Serializable]
public abstract class State<T> : IState
{
    public T owner { get; private set; }

    void IState.Acquire(object value)
    {
        owner = (T)value;
    }

    void IState.Release()
    {
        owner = default;
    }

    void IState.OnEnter()
    {
        OnEnter();
    }

    void IState.OnUpdate()
    {
        OnUpdate();
    }

    void IState.OnExit()
    {
        OnExit();
    }

    protected virtual void OnEnter() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnExit() { }
}

internal interface IState
{
    void Acquire(object value);
    void Release();
    void OnEnter();
    void OnUpdate();
    void OnExit();
}