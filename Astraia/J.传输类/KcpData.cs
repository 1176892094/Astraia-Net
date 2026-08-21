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

internal sealed unsafe class KcpData : IDisposable
{
    private IKCPCB* kcp;
    private SendDelegate output;

    public uint State => kcp->state;
    public uint Death => kcp->dead_link;
    public uint Count => kcp->nrcv_buf + kcp->nrcv_que + kcp->nsnd_buf + kcp->nsnd_que;

    public void Build(SendDelegate callback)
    {
        Release();

        var newKcp = Kcp.ikcp_create(0, null);
        if (newKcp == null)
        {
            return;
        }

        kcp = newKcp;
        output = callback;
        kcp->dead_link = Const.DEAD_LINK;
        Kcp.ikcp_setmtu(kcp, Const.MTU_DEF - Const.HEAD_SIZE);
        Kcp.ikcp_nodelay(kcp, 1, Const.STEP_TIME, Const.FAST_SEND, 1);
        Kcp.ikcp_wndsize(kcp, Const.SED_WIN, Const.REV_WIN);
    }

    public int Input(byte[] buffer, int offset, int count)
    {
        fixed (byte* ptr = &buffer[offset])
        {
            return Kcp.ikcp_input(kcp, ptr, count);
        }
    }

    public int Receive(byte[] buffer, int count)
    {
        fixed (byte* ptr = buffer)
        {
            return Kcp.ikcp_recv(kcp, ptr, count);
        }
    }

    public int Send(byte[] buffer, int offset, int count)
    {
        fixed (byte* ptr = &buffer[offset])
        {
            return Kcp.ikcp_send(kcp, ptr, count);
        }
    }

    public void Flush()
    {
        Kcp.ikcp_flush(kcp, output);
    }

    public void Update(uint current)
    {
        Kcp.ikcp_update(kcp, current, output);
    }

    public int PeekSize()
    {
        return Kcp.ikcp_peeksize(kcp);
    }

    private void Release()
    {
        if (kcp != null)
        {
            Kcp.ikcp_release(kcp);
            kcp = null;
        }

        output = null;
    }

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~KcpData()
    {
        Release();
    }
}