using System;
using UnityEngine;

// Token: 0x02000298 RID: 664
public class SpawnPrefab : MonoBehaviour
{
	// Token: 0x0600196D RID: 6509 RVA: 0x000A8FE0 File Offset: 0x000A71E0
	private void Start()
	{
		this.m_nview = base.GetComponentInParent<ZNetView>();
		if (this.m_nview == null)
		{
			ZLog.LogWarning("SpawnerPrefab cant find netview " + base.gameObject.name);
			return;
		}
		base.InvokeRepeating("TrySpawn", 1f, 1f);
	}

	// Token: 0x0600196E RID: 6510 RVA: 0x000A9038 File Offset: 0x000A7238
	private void TrySpawn()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		string name = "HasSpawned_" + base.gameObject.name;
		if (!this.m_nview.GetZDO().GetBool(name, false))
		{
			ZLog.Log("SpawnPrefab " + base.gameObject.name + " SPAWNING " + this.m_prefab.name);
			UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform.position, base.transform.rotation);
			this.m_nview.GetZDO().Set(name, true);
		}
		base.CancelInvoke("TrySpawn");
	}

	// Token: 0x0600196F RID: 6511 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04001B58 RID: 7000
	public GameObject m_prefab;

	// Token: 0x04001B59 RID: 7001
	private ZNetView m_nview;
}
