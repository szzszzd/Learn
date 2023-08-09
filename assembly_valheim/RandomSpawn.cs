using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000282 RID: 642
public class RandomSpawn : MonoBehaviour
{
	// Token: 0x06001883 RID: 6275 RVA: 0x000A38E4 File Offset: 0x000A1AE4
	public void Randomize()
	{
		bool spawned = UnityEngine.Random.Range(0f, 100f) <= this.m_chanceToSpawn;
		this.SetSpawned(spawned);
	}

	// Token: 0x06001884 RID: 6276 RVA: 0x000A3913 File Offset: 0x000A1B13
	public void Reset()
	{
		this.SetSpawned(true);
	}

	// Token: 0x06001885 RID: 6277 RVA: 0x000A391C File Offset: 0x000A1B1C
	private void SetSpawned(bool doSpawn)
	{
		if (!doSpawn)
		{
			base.gameObject.SetActive(false);
			using (List<ZNetView>.Enumerator enumerator = this.m_childNetViews.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZNetView znetView = enumerator.Current;
					znetView.gameObject.SetActive(false);
				}
				goto IL_62;
			}
		}
		if (this.m_nview == null)
		{
			base.gameObject.SetActive(true);
		}
		IL_62:
		if (this.m_OffObject != null)
		{
			this.m_OffObject.SetActive(!doSpawn);
		}
	}

	// Token: 0x06001886 RID: 6278 RVA: 0x000A39B8 File Offset: 0x000A1BB8
	public void Prepare()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_childNetViews = new List<ZNetView>();
		foreach (ZNetView znetView in base.gameObject.GetComponentsInChildren<ZNetView>(true))
		{
			if (Utils.IsEnabledInheirarcy(znetView.gameObject, base.gameObject))
			{
				this.m_childNetViews.Add(znetView);
			}
		}
	}

	// Token: 0x04001A5D RID: 6749
	public GameObject m_OffObject;

	// Token: 0x04001A5E RID: 6750
	[Range(0f, 100f)]
	public float m_chanceToSpawn = 50f;

	// Token: 0x04001A5F RID: 6751
	private List<ZNetView> m_childNetViews;

	// Token: 0x04001A60 RID: 6752
	private ZNetView m_nview;
}
