using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000162 RID: 354
public static class ZDOPool
{
	// Token: 0x06000DF0 RID: 3568 RVA: 0x0005FE39 File Offset: 0x0005E039
	public static ZDO Create(ZDOID id, Vector3 position)
	{
		ZDO zdo = ZDOPool.Get();
		zdo.Initialize(id, position);
		return zdo;
	}

	// Token: 0x06000DF1 RID: 3569 RVA: 0x0005FE48 File Offset: 0x0005E048
	public static ZDO Create()
	{
		return ZDOPool.Get();
	}

	// Token: 0x06000DF2 RID: 3570 RVA: 0x0005FE50 File Offset: 0x0005E050
	public static void Release(Dictionary<ZDOID, ZDO> objects)
	{
		foreach (ZDO zdo in objects.Values)
		{
			ZDOPool.Release(zdo);
		}
	}

	// Token: 0x06000DF3 RID: 3571 RVA: 0x0005FEA0 File Offset: 0x0005E0A0
	public static void Release(ZDO zdo)
	{
		zdo.Reset();
		ZDOPool.s_free.Push(zdo);
		ZDOPool.s_active--;
	}

	// Token: 0x06000DF4 RID: 3572 RVA: 0x0005FEC0 File Offset: 0x0005E0C0
	private static ZDO Get()
	{
		if (ZDOPool.s_free.Count <= 0)
		{
			for (int i = 0; i < 64; i++)
			{
				ZDO item = new ZDO();
				ZDOPool.s_free.Push(item);
			}
		}
		ZDOPool.s_active++;
		ZDO zdo = ZDOPool.s_free.Pop();
		zdo.Init();
		return zdo;
	}

	// Token: 0x06000DF5 RID: 3573 RVA: 0x0005FF14 File Offset: 0x0005E114
	public static int GetPoolSize()
	{
		return ZDOPool.s_free.Count;
	}

	// Token: 0x06000DF6 RID: 3574 RVA: 0x0005FF20 File Offset: 0x0005E120
	public static int GetPoolActive()
	{
		return ZDOPool.s_active;
	}

	// Token: 0x06000DF7 RID: 3575 RVA: 0x0005FF27 File Offset: 0x0005E127
	public static int GetPoolTotal()
	{
		return ZDOPool.s_active + ZDOPool.s_free.Count;
	}

	// Token: 0x04000F5F RID: 3935
	private const int c_BatchSize = 64;

	// Token: 0x04000F60 RID: 3936
	private static readonly Stack<ZDO> s_free = new Stack<ZDO>();

	// Token: 0x04000F61 RID: 3937
	private static int s_active;
}
