using System;
using UnityEngine;

// Token: 0x0200000B RID: 11
public class Corpse : MonoBehaviour
{
	// Token: 0x06000127 RID: 295 RVA: 0x000080C4 File Offset: 0x000062C4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_container = base.GetComponent<Container>();
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(ZDOVars.s_timeOfDeath, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_timeOfDeath, ZNet.instance.GetTime().Ticks);
		}
		base.InvokeRepeating("UpdateDespawn", Corpse.m_updateDt, Corpse.m_updateDt);
	}

	// Token: 0x06000128 RID: 296 RVA: 0x0000814C File Offset: 0x0000634C
	private void UpdateDespawn()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_container.IsInUse())
		{
			return;
		}
		if (this.m_container.GetInventory().NrOfItems() <= 0)
		{
			this.m_emptyTimer += Corpse.m_updateDt;
			if (this.m_emptyTimer >= this.m_emptyDespawnDelaySec)
			{
				ZLog.Log("Despawning looted corpse");
				this.m_nview.Destroy();
				return;
			}
		}
		else
		{
			this.m_emptyTimer = 0f;
		}
	}

	// Token: 0x04000109 RID: 265
	private static readonly float m_updateDt = 2f;

	// Token: 0x0400010A RID: 266
	public float m_emptyDespawnDelaySec = 10f;

	// Token: 0x0400010B RID: 267
	private float m_emptyTimer;

	// Token: 0x0400010C RID: 268
	private Container m_container;

	// Token: 0x0400010D RID: 269
	private ZNetView m_nview;
}
