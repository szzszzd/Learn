using System;

// Token: 0x02000159 RID: 345
public class ZDOConnectionHashData
{
	// Token: 0x06000D78 RID: 3448 RVA: 0x0005C9C8 File Offset: 0x0005ABC8
	public ZDOConnectionHashData(ZDOExtraData.ConnectionType type, int hash)
	{
		this.m_type = type;
		this.m_hash = hash;
	}

	// Token: 0x04000F22 RID: 3874
	public readonly ZDOExtraData.ConnectionType m_type;

	// Token: 0x04000F23 RID: 3875
	public readonly int m_hash;
}
