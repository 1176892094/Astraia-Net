// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 20:08:13
// # Recently: 2026-09-06 15:30:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

[Serializable]
public sealed class Tween : Async
{
    private Fixation progress;
    private Action<Fixation> onUpdate;

    internal static Tween Create(object owner, Fixation duration)
    {
        var item = HeapManager.Dequeue<Tween>();
        item.state = 0;
        item.owner = owner;
        item.progress = 0;
        item.duration = duration;
        TimeManager.Register(item);
        return item;
    }

    protected override void Release()
    {
        TimeManager.UnRegister(this);
        owner = null;
        onUpdate = null;
        HeapManager.Enqueue(this);
    }

    protected override void Update(Fixation elapseTime)
    {
        var startTime = waitTime - duration;
        if (startTime < elapseTime)
        {
            progress = (elapseTime - startTime) / duration;
            if (progress >= 1)
            {
                progress = 1;
            }

            onUpdate(progress);
            if (progress >= 1)
            {
                Interrupt(State.Success);
            }
        }
    }

    public Tween OnUpdate(Action<Fixation> update)
    {
        onUpdate += update;
        return this;
    }
}