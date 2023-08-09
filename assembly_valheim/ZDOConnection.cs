using System;

// Token: 0x02000158 RID: 344
public class ZDOConnection
{
	// Token: 0x06000D77 RID: 3447 RVA: 0x0005C9A7 File Offset: 0x0005ABA7
	public ZDOConnection(ZDOExtraData.ConnectionType type, ZDOID target)
	{
		this.m_type = type;
		this.m_target = target;
	}

	// Token: 0x04000F20 RID: 3872
	public readonly ZDOExtraData.ConnectionType m_type;

	// Token: 0x04000F21 RID: 3873
	public readonly ZDOID m_target = ZDOID.None;
}
