using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class TriggerSpawner : MonoBehaviour
{
	// Token: 0x0600037E RID: 894 RVA: 0x0001A665 File Offset: 0x00018865
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_nview.Register("Trigger", new Action<long>(this.RPC_Trigger));
		TriggerSpawner.m_allSpawners.Add(this);
	}

	// Token: 0x0600037F RID: 895 RVA: 0x0001A69A File Offset: 0x0001889A
	private void OnDestroy()
	{
		TriggerSpawner.m_allSpawners.Remove(this);
	}

	// Token: 0x06000380 RID: 896 RVA: 0x0001A6A8 File Offset: 0x000188A8
	public static void TriggerAllInRange(Vector3 p, float range)
	{
		ZLog.Log("Trigging spawners in range");
		foreach (TriggerSpawner triggerSpawner in TriggerSpawner.m_allSpawners)
		{
			if (Vector3.Distance(triggerSpawner.transform.position, p) < range)
			{
				triggerSpawner.Trigger();
			}
		}
	}

	// Token: 0x06000381 RID: 897 RVA: 0x0001A718 File Offset: 0x00018918
	private void Trigger()
	{
		this.m_nview.InvokeRPC("Trigger", Array.Empty<object>());
	}

	// Token: 0x06000382 RID: 898 RVA: 0x0001A72F File Offset: 0x0001892F
	private void RPC_Trigger(long sender)
	{
		ZLog.Log("Trigging " + base.gameObject.name);
		this.TrySpawning();
	}

	// Token: 0x06000383 RID: 899 RVA: 0x0001A754 File Offset: 0x00018954
	private void TrySpawning()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_minSpawnInterval > 0f)
		{
			DateTime time = ZNet.instance.GetTime();
			DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L));
			TimeSpan timeSpan = time - d;
			if (timeSpan.TotalMinutes < (double)this.m_minSpawnInterval)
			{
				string str = "Not enough time passed ";
				TimeSpan timeSpan2 = timeSpan;
				ZLog.Log(str + timeSpan2.ToString());
				return;
			}
		}
		if (UnityEngine.Random.Range(0f, 100f) > this.m_spawnChance)
		{
			ZLog.Log("Spawn chance fail " + this.m_spawnChance.ToString());
			return;
		}
		this.Spawn();
	}

	// Token: 0x06000384 RID: 900 RVA: 0x0001A814 File Offset: 0x00018A14
	private bool Spawn()
	{
		Vector3 position = base.transform.position;
		float y;
		if (ZoneSystem.instance.FindFloor(position, out y))
		{
			position.y = y;
		}
		GameObject gameObject = this.m_creaturePrefabs[UnityEngine.Random.Range(0, this.m_creaturePrefabs.Length)];
		int num = this.m_maxSpawned + (int)(this.m_maxExtraPerPlayer * (float)Game.instance.GetPlayerDifficulty(base.transform.position));
		if (num > 0 && SpawnSystem.GetNrOfInstances(gameObject, base.transform.position, this.m_maxSpawnedRange, false, false) >= num)
		{
			return false;
		}
		Quaternion rotation = this.m_useSpawnerRotation ? base.transform.rotation : Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, position, rotation);
		gameObject2.GetComponent<ZNetView>();
		BaseAI component = gameObject2.GetComponent<BaseAI>();
		if (component != null)
		{
			if (this.m_setPatrolSpawnPoint)
			{
				component.SetPatrolPoint();
			}
			if (this.m_setHuntPlayer)
			{
				component.SetHuntPlayer(true);
			}
		}
		if (this.m_maxLevel > 1)
		{
			Character component2 = gameObject2.GetComponent<Character>();
			if (component2)
			{
				int num2 = this.m_minLevel;
				while (num2 < this.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= this.m_levelupChance)
				{
					num2++;
				}
				if (num2 > 1)
				{
					component2.SetLevel(num2);
				}
			}
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
		this.m_spawnEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		return true;
	}

	// Token: 0x06000385 RID: 901 RVA: 0x000085D9 File Offset: 0x000067D9
	private float GetRadius()
	{
		return 0.75f;
	}

	// Token: 0x06000386 RID: 902 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x0400035B RID: 859
	private const float m_radius = 0.75f;

	// Token: 0x0400035C RID: 860
	public GameObject[] m_creaturePrefabs;

	// Token: 0x0400035D RID: 861
	[Header("Level")]
	public int m_maxLevel = 1;

	// Token: 0x0400035E RID: 862
	public int m_minLevel = 1;

	// Token: 0x0400035F RID: 863
	public float m_levelupChance = 10f;

	// Token: 0x04000360 RID: 864
	[Range(0f, 100f)]
	[Header("Spawn settings")]
	public float m_spawnChance = 100f;

	// Token: 0x04000361 RID: 865
	public float m_minSpawnInterval = 10f;

	// Token: 0x04000362 RID: 866
	public int m_maxSpawned = 10;

	// Token: 0x04000363 RID: 867
	public float m_maxExtraPerPlayer;

	// Token: 0x04000364 RID: 868
	public float m_maxSpawnedRange = 30f;

	// Token: 0x04000365 RID: 869
	public bool m_setHuntPlayer;

	// Token: 0x04000366 RID: 870
	public bool m_setPatrolSpawnPoint;

	// Token: 0x04000367 RID: 871
	public bool m_useSpawnerRotation;

	// Token: 0x04000368 RID: 872
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x04000369 RID: 873
	private ZNetView m_nview;

	// Token: 0x0400036A RID: 874
	private static List<TriggerSpawner> m_allSpawners = new List<TriggerSpawner>();
}
