// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 15:08:20
// # Recently: 2026-08-14 15:33:20
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

internal unsafe class KcpData : IDisposable
{
    private static readonly Dictionary<int, SendDelegate> methods = new Dictionary<int, SendDelegate>();
    private int key;
    private IKCPCB* kcp;

    public uint State => kcp->state;
    public uint Death => kcp->dead_link;
    public uint Count => kcp->nrcv_buf + kcp->nrcv_que + kcp->nsnd_buf + kcp->nsnd_que;

    public void Build(SendDelegate onSend)
    {
        ReleaseUnmanagedResources();

        int newKey;
        do
        {
            newKey = Seed.Next();
        } while (newKey == 0 || methods.ContainsKey(newKey));

        var newKcp = Kcp.ikcp_create(0, (void*)newKey);
        if (newKcp == null)
        {
            return;
        }

        key = newKey;
        kcp = newKcp;

        kcp->dead_link = Const.DEAD_LINK;
        Kcp.ikcp_setmtu(kcp, Const.MTU_DEF - Const.HEAD_SIZE);
        Kcp.ikcp_nodelay(kcp, 1, Const.STEP_TIME, Const.FAST_SEND, 1);
        Kcp.ikcp_wndsize(kcp, Const.SED_WIN, Const.REV_WIN);
        Kcp.ikcp_setoutput(kcp, &Output);
        methods.Add(newKey, onSend);
    }

    private static int Output(byte* bytes, int count, IKCPCB* kcp, void* user)
    {
        if (methods.TryGetValue((int)user, out var method))
        {
            method(bytes, count);
        }

        return count;
    }

    public int Input(byte[] buffer, int offset, int length)
    {
        fixed (byte* ptr = &buffer[offset])
        {
            return Kcp.ikcp_input(kcp, ptr, length);
        }
    }

    public int Receive(byte[] buffer, int length)
    {
        fixed (byte* ptr = buffer)
        {
            return Kcp.ikcp_recv(kcp, ptr, length);
        }
    }

    public int Send(byte[] buffer, int offset, int length)
    {
        fixed (byte* ptr = &buffer[offset])
        {
            return Kcp.ikcp_send(kcp, ptr, length);
        }
    }

    public void Flush()
    {
        Kcp.ikcp_flush(kcp);
    }

    public void Update(uint current)
    {
        Kcp.ikcp_update(kcp, current);
    }

    public int PeekSize()
    {
        return Kcp.ikcp_peeksize(kcp);
    }

    ~KcpData()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        if (key != 0)
        {
            methods.Remove(key);
            key = 0;
        }

        if (kcp != null)
        {
            Kcp.ikcp_release(kcp);
            kcp = null;
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}