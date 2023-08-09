using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000015 RID: 21
public class Growup : MonoBehaviour
{
	// Token: 0x06000157 RID: 343 RVA: 0x00009472 File Offset: 0x00007672
	private void Start()
	{
		this.m_baseAI = base.GetComponent<BaseAI>();
		this.m_nview = base.GetComponent<ZNetView>();
		base.InvokeRepeating("GrowUpdate", UnityEngine.Random.Range(10f, 15f), 10f);
	}

	// Token: 0x06000158 RID: 344 RVA: 0x000094AC File Offset: 0x000076AC
	private void GrowUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_baseAI.GetTimeSinceSpawned().TotalSeconds > (double)this.m_growTime)
		{
			Character component = base.GetComponent<Character>();
			Character component2 = UnityEngine.Object.Instantiate<GameObject>(this.GetPrefab(), base.transform.position, base.transform.rotation).GetComponent<Character>();
			if (component && component2)
			{
				if (this.m_inheritTame)
				{
					component2.SetTamed(component.IsTamed());
				}
				component2.SetLevel(component.GetLevel());
			}
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06000159 RID: 345 RVA: 0x0000955C File Offset: 0x0000775C
	private GameObject GetPrefab()
	{
		if (this.m_altGrownPrefabs == null || this.m_altGrownPrefabs.Count == 0)
		{
			return this.m_grownPrefab;
		}
		float num = 0f;
		foreach (Growup.GrownEntry grownEntry in this.m_altGrownPrefabs)
		{
			num += grownEntry.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		for (int i = 0; i < this.m_altGrownPrefabs.Count; i++)
		{
			num3 += this.m_altGrownPrefabs[i].m_weight;
			if (num2 <= num3)
			{
				return this.m_altGrownPrefabs[i].m_prefab;
			}
		}
		return this.m_altGrownPrefabs[0].m_prefab;
	}

	// Token: 0x0400016F RID: 367
	public float m_growTime = 60f;

	// Token: 0x04000170 RID: 368
	public bool m_inheritTame = true;

	// Token: 0x04000171 RID: 369
	public GameObject m_grownPrefab;

	// Token: 0x04000172 RID: 370
	public List<Growup.GrownEntry> m_altGrownPrefabs;

	// Token: 0x04000173 RID: 371
	private BaseAI m_baseAI;

	// Token: 0x04000174 RID: 372
	private ZNetView m_nview;

	// Token: 0x02000016 RID: 22
	[Serializable]
	public class GrownEntry
	{
		// Token: 0x04000175 RID: 373
		public GameObject m_prefab;

		// Token: 0x04000176 RID: 374
		public float m_weight = 1f;
	}
}
