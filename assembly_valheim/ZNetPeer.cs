using System;
using UnityEngine;

// Token: 0x02000165 RID: 357
public class ZNetPeer : IDisposable
{
	// Token: 0x06000DFF RID: 3583 RVA: 0x0006091C File Offset: 0x0005EB1C
	public ZNetPeer(ISocket socket, bool server)
	{
		this.m_socket = socket;
		this.m_rpc = new ZRpc(this.m_socket);
		this.m_server = server;
	}

	// Token: 0x06000E00 RID: 3584 RVA: 0x0006096F File Offset: 0x0005EB6F
	public void Dispose()
	{
		this.m_socket.Dispose();
		this.m_rpc.Dispose();
	}

	// Token: 0x06000E01 RID: 3585 RVA: 0x00060987 File Offset: 0x0005EB87
	public bool IsReady()
	{
		return this.m_uid != 0L;
	}

	// Token: 0x06000E02 RID: 3586 RVA: 0x00060993 File Offset: 0x0005EB93
	public Vector3 GetRefPos()
	{
		return this.m_refPos;
	}

	// Token: 0x04001004 RID: 4100
	public ZRpc m_rpc;

	// Token: 0x04001005 RID: 4101
	public ISocket m_socket;

	// Token: 0x04001006 RID: 4102
	public long m_uid;

	// Token: 0x04001007 RID: 4103
	public bool m_server;

	// Token: 0x04001008 RID: 4104
	public Vector3 m_refPos = Vector3.zero;

	// Token: 0x04001009 RID: 4105
	public bool m_publicRefPos;

	// Token: 0x0400100A RID: 4106
	public ZDOID m_characterID = ZDOID.None;

	// Token: 0x0400100B RID: 4107
	public string m_playerName = "";
}
