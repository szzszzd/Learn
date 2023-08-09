using System;
using System.Collections.Generic;

// Token: 0x02000151 RID: 337
internal class ZDOComparer : IEqualityComparer<ZDO>
{
	// Token: 0x06000CB5 RID: 3253 RVA: 0x0005A042 File Offset: 0x00058242
	public bool Equals(ZDO a, ZDO b)
	{
		return a == b;
	}

	// Token: 0x06000CB6 RID: 3254 RVA: 0x0005A048 File Offset: 0x00058248
	public int GetHashCode(ZDO a)
	{
		return a.GetHashCode();
	}
}
