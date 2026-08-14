namespace Astraia;

public interface INode
{
    Task<State> OnTick(int[] indices, Blackboard<int> root);
}

public static class Nodes
{
    private static readonly Dictionary<Type, Func<Node, Func<Node, Type>, INode>> Func = new();

    static Nodes()
    {
        Func[typeof(Sequence)] = Sequence;
        Func[typeof(Selector)] = Selector;
        Func[typeof(Parallel)] = Parallel;
        Func[typeof(Randomer)] = Randomer;
        Func[typeof(Repeater)] = Repeater;
        Func[typeof(Inverter)] = Inverter;
        Func[typeof(Success)] = Success;
        Func[typeof(Failure)] = Failure;
    }

    private static INode Sequence(Node node, Func<Node, Type> func)
    {
        return new Sequence(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
    }

    private static INode Selector(Node node, Func<Node, Type> func)
    {
        return new Selector(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
    }

    private static INode Parallel(Node node, Func<Node, Type> func)
    {
        return new Parallel(node.Data == "Any", node.Nodes.Select(i => i.Build(func)).ToArray());
    }

    private static INode Randomer(Node node, Func<Node, Type> func)
    {
        return new Randomer(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
    }

    private static INode Repeater(Node node, Func<Node, Type> func)
    {
        return new Repeater(node.Index, int.Parse(node.Data), node.Nodes.Select(i => i.Build(func)).First());
    }

    private static INode Inverter(Node node, Func<Node, Type> func)
    {
        return new Inverter(node.Nodes.Select(i => i.Build(func)).First());
    }

    private static INode Success(Node node, Func<Node, Type> func)
    {
        return new Success(node.Nodes.Select(i => i.Build(func)).First());
    }

    private static INode Failure(Node node, Func<Node, Type> func)
    {
        return new Failure(node.Nodes.Select(i => i.Build(func)).First());
    }

    public static Node Load(string reason, List<Node> nodes)
    {
        if (string.IsNullOrEmpty(reason))
        {
            return default;
        }

        var count = nodes.Count;
        var index = FindFirstBracket(reason);
        if (index < 0)
        {
            var node = new Node(reason, count);
            nodes.Add(node);
            return node;
        }
        else
        {
            var node = new Node(reason.Substring(0, index), count);
            nodes.Add(node);
            foreach (var child in LoadNode(Checked(reason, index)))
            {
                node.Nodes.Add(Load(child, nodes));
            }

            return node;
        }
    }

    private static string Checked(string reason, int index)
    {
        var depth = 0;
        var count = index;
        while (count < reason.Length)
        {
            if (IsLeftBracket(reason[count]))
            {
                depth++;
            }
            else if (IsRightBracket(reason[count]))
            {
                depth--;
            }

            if (depth == 0)
            {
                break;
            }

            count++;
        }

        return reason.Substring(index + 1, count - index - 1);
    }

    private static List<string> LoadNode(string reason)
    {
        var result = new List<string>();
        var depth = 0;
        var index = 0;

        for (var i = 0; i < reason.Length; i++)
        {
            var c = reason[i];
            if (IsLeftBracket(c))
            {
                depth++;
            }
            else if (IsRightBracket(c))
            {
                depth--;
            }
            else if (depth == 0 && IsSeparator(c))
            {
                result.Add(reason.Substring(index, i - index).Trim());
                index = i + 1;
            }
        }

        result.Add(reason.Substring(index).Trim());
        return result;
    }

    private static int FindFirstBracket(string text)
    {
        var englishIndex = text.IndexOf('(');
        var chineseIndex = text.IndexOf('（');

        if (englishIndex < 0)
        {
            return chineseIndex;
        }

        if (chineseIndex < 0)
        {
            return englishIndex;
        }

        return Math.Min(englishIndex, chineseIndex);
    }

    private static int FindColon(string text)
    {
        var englishIndex = text.IndexOf(':');
        var chineseIndex = text.IndexOf('：');

        if (englishIndex < 0)
        {
            return chineseIndex;
        }

        if (chineseIndex < 0)
        {
            return englishIndex;
        }

        return Math.Min(englishIndex, chineseIndex);
    }

    private static bool IsLeftBracket(char c)
    {
        return c is '(' or '（';
    }

    private static bool IsRightBracket(char c)
    {
        return c is ')' or '）';
    }

    private static bool IsSeparator(char c)
    {
        return c is ',' or '，';
    }

    [Serializable]
    public readonly struct Node
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Data;
        public readonly List<Node> Nodes;

        public Node(string name, int index)
        {
            var i = FindColon(name);
            if (i < 0)
            {
                Name = name.Trim();
                Data = string.Empty;
            }
            else
            {
                Name = name.Trim().Substring(0, i);
                Data = name.Trim().Substring(i + 1);
            }

            Index = index;
            Nodes = new List<Node>();
        }

        public INode Build(Func<Node, Type> func)
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new NullReferenceException();
            }

            var reason = func.Invoke(this);
            if (Func.TryGetValue(reason, out var result))
            {
                return result.Invoke(this, func);
            }

            return (INode)Activator.CreateInstance(reason);
        }
    }
}