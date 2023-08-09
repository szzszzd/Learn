using System;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class CreatureSpawner : MonoBehaviour
{
	// Token: 0x0600012B RID: 299 RVA: 0x000081E8 File Offset: 0x000063E8
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		base.InvokeRepeating("UpdateSpawner", UnityEngine.Random.Range(3f, 5f), 5f);
	}

	// Token: 0x0600012C RID: 300 RVA: 0x00008224 File Offset: 0x00006424
	private void UpdateSpawner()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		ZDOConnection connection = this.m_nview.GetZDO().GetConnection();
		bool flag = connection != null && connection.m_type == ZDOExtraData.ConnectionType.Spawned;
		if (this.m_respawnTimeMinuts <= 0f && flag)
		{
			return;
		}
		ZDOID id = (connection != null) ? connection.m_target : ZDOID.None;
		if (!id.IsNone() && ZDOMan.instance.GetZDO(id) != null)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_aliveTime, ZNet.instance.GetTime().Ticks);
			return;
		}
		if (this.m_respawnTimeMinuts > 0f)
		{
			DateTime time = ZNet.instance.GetTime();
			DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_aliveTime, 0L));
			if ((time - d).TotalMinutes < (double)this.m_respawnTimeMinuts)
			{
				return;
			}
		}
		if (!this.m_spawnAtDay && EnvMan.instance.IsDay())
		{
			return;
		}
		if (!this.m_spawnAtNight && EnvMan.instance.IsNight())
		{
			return;
		}
		if (!this.m_spawnInPlayerBase && EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.PlayerBase, 0f))
		{
			return;
		}
		if (this.m_triggerNoise > 0f)
		{
			if (!Player.IsPlayerInRange(base.transform.position, this.m_triggerDistance, this.m_triggerNoise))
			{
				return;
			}
		}
		else if (!Player.IsPlayerInRange(base.transform.position, this.m_triggerDistance))
		{
			return;
		}
		this.Spawn();
	}

	// Token: 0x0600012D RID: 301 RVA: 0x000083B0 File Offset: 0x000065B0
	private bool HasSpawned()
	{
		return !(this.m_nview == null) && this.m_nview.GetZDO() != null && !this.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned).IsNone();
	}

	// Token: 0x0600012E RID: 302 RVA: 0x000083F8 File Offset: 0x000065F8
	private ZNetView Spawn()
	{
		Vector3 position = base.transform.position;
		float y;
		if (ZoneSystem.instance.FindFloor(position, out y))
		{
			position.y = y;
		}
		Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_creaturePrefab, position, rotation);
		ZNetView component = gameObject.GetComponent<ZNetView>();
		BaseAI component2 = gameObject.GetComponent<BaseAI>();
		if (component2 != null && this.m_setPatrolSpawnPoint)
		{
			component2.SetPatrolPoint();
		}
		if (this.m_maxLevel > 1)
		{
			Character component3 = gameObject.GetComponent<Character>();
			if (component3)
			{
				int num = this.m_minLevel;
				while (num < this.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= this.m_levelupChance)
				{
					num++;
				}
				if (num > 1)
				{
					component3.SetLevel(num);
				}
			}
			else
			{
				ItemDrop component4 = gameObject.GetComponent<ItemDrop>();
				if (component4)
				{
					int num2 = this.m_minLevel;
					while (num2 < this.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= this.m_levelupChance)
					{
						num2++;
					}
					if (num2 > 1)
					{
						component4.SetQuality(num2);
					}
				}
			}
		}
		this.m_nview.GetZDO().SetConnection(ZDOExtraData.ConnectionType.Spawned, component.GetZDO().m_uid);
		this.m_nview.GetZDO().Set(ZDOVars.s_aliveTime, ZNet.instance.GetTime().Ticks);
		this.SpawnEffect(gameObject);
		return component;
	}

	// Token: 0x0600012F RID: 303 RVA: 0x0000857C File Offset: 0x0000677C
	private void SpawnEffect(GameObject spawnedObject)
	{
		Character component = spawnedObject.GetComponent<Character>();
		Vector3 basePos = component ? component.GetCenterPoint() : (base.transform.position + Vector3.up * 0.75f);
		this.m_spawnEffects.Create(basePos, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x06000130 RID: 304 RVA: 0x000085D9 File Offset: 0x000067D9
	private float GetRadius()
	{
		return 0.75f;
	}

	// Token: 0x06000131 RID: 305 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x0400010E RID: 270
	private const float m_radius = 0.75f;

	// Token: 0x0400010F RID: 271
	public GameObject m_creaturePrefab;

	// Token: 0x04000110 RID: 272
	[Header("Level")]
	public int m_maxLevel = 1;

	// Token: 0x04000111 RID: 273
	public int m_minLevel = 1;

	// Token: 0x04000112 RID: 274
	public float m_levelupChance = 10f;

	// Token: 0x04000113 RID: 275
	[Header("Spawn settings")]
	public float m_respawnTimeMinuts = 20f;

	// Token: 0x04000114 RID: 276
	public float m_triggerDistance = 60f;

	// Token: 0x04000115 RID: 277
	public float m_triggerNoise;

	// Token: 0x04000116 RID: 278
	public bool m_spawnAtNight = true;

	// Token: 0x04000117 RID: 279
	public bool m_spawnAtDay = true;

	// Token: 0x04000118 RID: 280
	public bool m_requireSpawnArea;

	// Token: 0x04000119 RID: 281
	public bool m_spawnInPlayerBase;

	// Token: 0x0400011A RID: 282
	public bool m_setPatrolSpawnPoint;

	// Token: 0x0400011B RID: 283
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x0400011C RID: 284
	private ZNetView m_nview;
}
