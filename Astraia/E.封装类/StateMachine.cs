namespace Astraia;

[Serializable]
public sealed class StateMachine
{
    private Dictionary<int, IState> states = new();
    private int index;
    private IState state;
    private IState other;

    public void Create<TState>(object owner, int value)
    {
        var item = (IState)Activator.CreateInstance<TState>();
        item.Acquire(owner);
        states[value] = item;
    }

    public void Switch(int value)
    {
        state?.OnExit();
        states.TryGetValue(value, out state);
        state?.OnEnter();
    }

    public void Update()
    {
        state?.OnUpdate();
    }

    public void Update(int value)
    {
        if (other == state)
        {
            return;
        }

        if (index != value)
        {
            other?.OnExit();
        }

        states.TryGetValue(value, out other);

        if (index != value)
        {
            other?.OnEnter();
        }

        index = value;
        other?.OnUpdate();
    }

    public void Clear()
    {
        foreach (var item in states.Values)
        {
            item.Release();
        }

        other = null;
        state = null;
        states.Clear();
    }
}