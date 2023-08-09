using System;
using UnityEngine;

// Token: 0x020001D1 RID: 465
public class LocationProxy : MonoBehaviour
{
	// Token: 0x06001309 RID: 4873 RVA: 0x0007D79A File Offset: 0x0007B99A
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.SpawnLocation();
	}

	// Token: 0x0600130A RID: 4874 RVA: 0x0007D7B0 File Offset: 0x0007B9B0
	public void SetLocation(string location, int seed, bool spawnNow)
	{
		int stableHashCode = location.GetStableHashCode();
		this.m_nview.GetZDO().Set(ZDOVars.s_location, stableHashCode, false);
		this.m_nview.GetZDO().Set(ZDOVars.s_seed, seed, false);
		if (spawnNow)
		{
			this.SpawnLocation();
		}
	}

	// Token: 0x0600130B RID: 4875 RVA: 0x0007D7FC File Offset: 0x0007B9FC
	private bool SpawnLocation()
	{
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_location, 0);
		int int2 = this.m_nview.GetZDO().GetInt(ZDOVars.s_seed, 0);
		if (@int == 0)
		{
			return false;
		}
		this.m_instance = ZoneSystem.instance.SpawnProxyLocation(@int, int2, base.transform.position, base.transform.rotation);
		if (this.m_instance == null)
		{
			return false;
		}
		this.m_instance.transform.SetParent(base.transform, true);
		return true;
	}

	// Token: 0x040013E9 RID: 5097
	private GameObject m_instance;

	// Token: 0x040013EA RID: 5098
	private ZNetView m_nview;
}
