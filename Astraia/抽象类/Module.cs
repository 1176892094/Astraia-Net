namespace Astraia;

[Serializable]
public abstract class Module<T> : IModule
{
    public T owner { get; private set; }

    void IModule.Acquire(object value)
    {
        owner = (T)value;
    }

    void IModule.Release()
    {
        owner = default;
    }

    void IModule.Dequeue()
    {
        Dequeue();
    }

    void IModule.Enqueue()
    {
        Enqueue();
    }

    void IModule.OnShow()
    {
        OnShow();
    }

    void IModule.OnHide()
    {
        OnHide();
    }

    protected virtual void Dequeue() { }
    protected virtual void Enqueue() { }
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}

internal interface IModule
{
    void Acquire(object value);
    void Release();
    void Dequeue();
    void Enqueue();
    void OnShow();
    void OnHide();
}