// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 20:08:13
// # Recently: 2026-08-15 17:54:36
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

public readonly struct Sequence(int index, INode[] nodes) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        var current = indices[index];
        while (current < nodes.Length)
        {
            var state = await nodes[current].OnTick(indices, root);
            if (state == State.Running)
            {
                return State.Running;
            }

            if (state == State.Failure)
            {
                indices[index] = 0;
                return State.Failure;
            }

            current++;
            indices[index] = current;
        }

        indices[index] = 0;
        return State.Success;
    }
}

public readonly struct Selector(int index, INode[] nodes) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        var current = indices[index];
        while (current < nodes.Length)
        {
            var state = await nodes[current].OnTick(indices, root);
            if (state == State.Running)
            {
                return State.Running;
            }

            if (state == State.Success)
            {
                indices[index] = 0;
                return State.Success;
            }

            current++;
            indices[index] = current;
        }

        indices[index] = 0;
        return State.Failure;
    }
}

public readonly struct Parallel(bool isAny, INode[] nodes) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        if (isAny)
        {
            foreach (var node in nodes)
            {
                var state = await node.OnTick(indices, root);
                if (state == State.Success)
                {
                    return State.Success;
                }

                if (state == State.Failure)
                {
                    return State.Failure;
                }
            }

            return State.Running;
        }

        var isAll = true;
        foreach (var node in nodes)
        {
            var state = await node.OnTick(indices, root);
            if (state == State.Failure)
            {
                return State.Failure;
            }

            if (state == State.Running)
            {
                isAll = false;
            }
        }

        return isAll ? State.Success : State.Running;
    }
}

public readonly struct Randomer(int index, INode[] nodes) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        if (indices[index] == 0)
        {
            indices[index] = Seed.Next(nodes.Length) + 1;
        }

        var state = await nodes[indices[index] - 1].OnTick(indices, root);
        if (state == State.Running)
        {
            return State.Running;
        }

        indices[index] = 0;
        return state;
    }
}

public readonly struct Repeater(int index, int count, INode node) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        var state = await node.OnTick(indices, root);
        if (state == State.Running)
        {
            return State.Running;
        }

        indices[index]++;
        if (count < 0 || indices[index] < count)
        {
            return State.Running;
        }

        indices[index] = 0;
        return State.Success;
    }
}

public readonly struct Inverter(INode node) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        var state = await node.OnTick(indices, root);
        switch (state)
        {
            case State.Success: return State.Failure;
            case State.Failure: return State.Success;
        }

        return State.Running;
    }
}

public readonly struct Success(INode node) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        var state = await node.OnTick(indices, root);
        return state == State.Running ? State.Running : State.Success;
    }
}

public readonly struct Failure(INode nodes) : INode
{
    public async Task<State> OnTick(int[] indices, Blackboard<int> root)
    {
        var state = await nodes.OnTick(indices, root);
        return state == State.Running ? State.Running : State.Failure;
    }
}