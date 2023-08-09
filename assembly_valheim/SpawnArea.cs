using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001FB RID: 507
public class SpawnArea : MonoBehaviour
{
	// Token: 0x06001459 RID: 5209 RVA: 0x0008462F File Offset: 0x0008282F
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		base.InvokeRepeating("UpdateSpawn", 2f, 2f);
	}

	// Token: 0x0600145A RID: 5210 RVA: 0x00084654 File Offset: 0x00082854
	private void UpdateSpawn()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (ZNetScene.instance.OutsideActiveArea(base.transform.position))
		{
			return;
		}
		if (!Player.IsPlayerInRange(base.transform.position, this.m_triggerDistance))
		{
			return;
		}
		this.m_spawnTimer += 2f;
		if (this.m_spawnTimer > this.m_spawnIntervalSec)
		{
			this.m_spawnTimer = 0f;
			this.SpawnOne();
		}
	}

	// Token: 0x0600145B RID: 5211 RVA: 0x000846D4 File Offset: 0x000828D4
	private bool SpawnOne()
	{
		int num;
		int num2;
		this.GetInstances(out num, out num2);
		if (num >= this.m_maxNear || num2 >= this.m_maxTotal)
		{
			return false;
		}
		SpawnArea.SpawnData spawnData = this.SelectWeightedPrefab();
		if (spawnData == null)
		{
			return false;
		}
		Vector3 position;
		if (!this.FindSpawnPoint(spawnData.m_prefab, out position))
		{
			return false;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(spawnData.m_prefab, position, Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f));
		if (this.m_setPatrolSpawnPoint)
		{
			BaseAI component = gameObject.GetComponent<BaseAI>();
			if (component != null)
			{
				component.SetPatrolPoint();
			}
		}
		Character component2 = gameObject.GetComponent<Character>();
		if (spawnData.m_maxLevel > 1)
		{
			int num3 = spawnData.m_minLevel;
			while (num3 < spawnData.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= this.m_levelupChance)
			{
				num3++;
			}
			if (num3 > 1)
			{
				component2.SetLevel(num3);
			}
		}
		Vector3 centerPoint = component2.GetCenterPoint();
		this.m_spawnEffects.Create(centerPoint, Quaternion.identity, null, 1f, -1);
		return true;
	}

	// Token: 0x0600145C RID: 5212 RVA: 0x000847E0 File Offset: 0x000829E0
	private bool FindSpawnPoint(GameObject prefab, out Vector3 point)
	{
		prefab.GetComponent<BaseAI>();
		for (int i = 0; i < 10; i++)
		{
			Vector3 vector = base.transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(0f, this.m_spawnRadius);
			float num;
			if (ZoneSystem.instance.FindFloor(vector, out num) && (!this.m_onGroundOnly || !ZoneSystem.instance.IsBlocked(vector)))
			{
				vector.y = num + 0.1f;
				point = vector;
				return true;
			}
		}
		point = Vector3.zero;
		return false;
	}

	// Token: 0x0600145D RID: 5213 RVA: 0x0008489C File Offset: 0x00082A9C
	private SpawnArea.SpawnData SelectWeightedPrefab()
	{
		if (this.m_prefabs.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (SpawnArea.SpawnData spawnData in this.m_prefabs)
		{
			num += spawnData.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (SpawnArea.SpawnData spawnData2 in this.m_prefabs)
		{
			num3 += spawnData2.m_weight;
			if (num2 <= num3)
			{
				return spawnData2;
			}
		}
		return this.m_prefabs[this.m_prefabs.Count - 1];
	}

	// Token: 0x0600145E RID: 5214 RVA: 0x00084988 File Offset: 0x00082B88
	private void GetInstances(out int near, out int total)
	{
		near = 0;
		total = 0;
		Vector3 position = base.transform.position;
		foreach (BaseAI baseAI in BaseAI.Instances)
		{
			if (this.IsSpawnPrefab(baseAI.gameObject))
			{
				float num = Utils.DistanceXZ(baseAI.transform.position, position);
				if (num < this.m_nearRadius)
				{
					near++;
				}
				if (num < this.m_farRadius)
				{
					total++;
				}
			}
		}
	}

	// Token: 0x0600145F RID: 5215 RVA: 0x00084A24 File Offset: 0x00082C24
	private bool IsSpawnPrefab(GameObject go)
	{
		string name = go.name;
		Character component = go.GetComponent<Character>();
		foreach (SpawnArea.SpawnData spawnData in this.m_prefabs)
		{
			if (name.CustomStartsWith(spawnData.m_prefab.name) && (!component || !component.IsTamed()))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001460 RID: 5216 RVA: 0x00084AAC File Offset: 0x00082CAC
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, this.m_spawnRadius);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, this.m_nearRadius);
	}

	// Token: 0x040014F5 RID: 5365
	private const float dt = 2f;

	// Token: 0x040014F6 RID: 5366
	public List<SpawnArea.SpawnData> m_prefabs = new List<SpawnArea.SpawnData>();

	// Token: 0x040014F7 RID: 5367
	public float m_levelupChance = 15f;

	// Token: 0x040014F8 RID: 5368
	public float m_spawnIntervalSec = 30f;

	// Token: 0x040014F9 RID: 5369
	public float m_triggerDistance = 256f;

	// Token: 0x040014FA RID: 5370
	public bool m_setPatrolSpawnPoint = true;

	// Token: 0x040014FB RID: 5371
	public float m_spawnRadius = 2f;

	// Token: 0x040014FC RID: 5372
	public float m_nearRadius = 10f;

	// Token: 0x040014FD RID: 5373
	public float m_farRadius = 1000f;

	// Token: 0x040014FE RID: 5374
	public int m_maxNear = 3;

	// Token: 0x040014FF RID: 5375
	public int m_maxTotal = 20;

	// Token: 0x04001500 RID: 5376
	public bool m_onGroundOnly;

	// Token: 0x04001501 RID: 5377
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x04001502 RID: 5378
	private ZNetView m_nview;

	// Token: 0x04001503 RID: 5379
	private float m_spawnTimer;

	// Token: 0x020001FC RID: 508
	[Serializable]
	public class SpawnData
	{
		// Token: 0x04001504 RID: 5380
		public GameObject m_prefab;

		// Token: 0x04001505 RID: 5381
		public float m_weight;

		// Token: 0x04001506 RID: 5382
		[Header("Level")]
		public int m_maxLevel = 1;

		// Token: 0x04001507 RID: 5383
		public int m_minLevel = 1;
	}
}
