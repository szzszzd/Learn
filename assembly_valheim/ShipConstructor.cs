using System;
using UnityEngine;

// Token: 0x0200028D RID: 653
public class ShipConstructor : MonoBehaviour
{
	// Token: 0x06001907 RID: 6407 RVA: 0x000A6F20 File Offset: 0x000A5120
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
		}
		base.InvokeRepeating("UpdateConstruction", 5f, 1f);
		if (this.IsBuilt())
		{
			this.m_hideWhenConstructed.SetActive(false);
			return;
		}
	}

	// Token: 0x06001908 RID: 6408 RVA: 0x000A6FCC File Offset: 0x000A51CC
	private bool IsBuilt()
	{
		return this.m_nview.GetZDO().GetBool(ZDOVars.s_done, false);
	}

	// Token: 0x06001909 RID: 6409 RVA: 0x000A6FE4 File Offset: 0x000A51E4
	private void UpdateConstruction()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.IsBuilt())
		{
			this.m_hideWhenConstructed.SetActive(false);
			return;
		}
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L));
		if ((time - d).TotalMinutes > (double)this.m_constructionTimeMinutes)
		{
			this.m_hideWhenConstructed.SetActive(false);
			UnityEngine.Object.Instantiate<GameObject>(this.m_shipPrefab, this.m_spawnPoint.position, this.m_spawnPoint.rotation);
			this.m_nview.GetZDO().Set(ZDOVars.s_done, true);
		}
	}

	// Token: 0x04001B07 RID: 6919
	public GameObject m_shipPrefab;

	// Token: 0x04001B08 RID: 6920
	public GameObject m_hideWhenConstructed;

	// Token: 0x04001B09 RID: 6921
	public Transform m_spawnPoint;

	// Token: 0x04001B0A RID: 6922
	public long m_constructionTimeMinutes = 1L;

	// Token: 0x04001B0B RID: 6923
	private ZNetView m_nview;
}
