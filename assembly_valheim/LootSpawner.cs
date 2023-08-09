﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200012B RID: 299
public class LootSpawner : MonoBehaviour
{
	// Token: 0x06000BAE RID: 2990 RVA: 0x000562FF File Offset: 0x000544FF
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		base.InvokeRepeating("UpdateSpawner", 10f, 2f);
	}

	// Token: 0x06000BAF RID: 2991 RVA: 0x00056330 File Offset: 0x00054530
	private void UpdateSpawner()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_spawnAtDay && EnvMan.instance.IsDay())
		{
			return;
		}
		if (!this.m_spawnAtNight && EnvMan.instance.IsNight())
		{
			return;
		}
		if (this.m_spawnWhenEnemiesCleared)
		{
			bool flag = LootSpawner.IsMonsterInRange(base.transform.position, this.m_enemiesCheckRange);
			if (flag && !this.m_seenEnemies)
			{
				this.m_seenEnemies = true;
			}
			if (flag || !this.m_seenEnemies)
			{
				return;
			}
		}
		long @long = this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L);
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(@long);
		TimeSpan timeSpan = time - d;
		if (this.m_respawnTimeMinuts <= 0f && @long != 0L)
		{
			return;
		}
		if (timeSpan.TotalMinutes < (double)this.m_respawnTimeMinuts)
		{
			return;
		}
		if (!Player.IsPlayerInRange(base.transform.position, 20f))
		{
			return;
		}
		List<GameObject> dropList = this.m_items.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.3f;
			Vector3 position = base.transform.position + new Vector3(vector.x, 0.3f * (float)i, vector.y);
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			UnityEngine.Object.Instantiate<GameObject>(dropList[i], position, rotation);
		}
		this.m_spawnEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		this.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
		this.m_seenEnemies = false;
	}

	// Token: 0x06000BB0 RID: 2992 RVA: 0x000564FC File Offset: 0x000546FC
	public static bool IsMonsterInRange(Vector3 point, float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		float time = Time.time;
		foreach (Character character in allCharacters)
		{
			if (character.IsMonsterFaction(time) && Vector3.Distance(character.transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000BB1 RID: 2993 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04000E16 RID: 3606
	public DropTable m_items = new DropTable();

	// Token: 0x04000E17 RID: 3607
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x04000E18 RID: 3608
	public float m_respawnTimeMinuts = 10f;

	// Token: 0x04000E19 RID: 3609
	public bool m_spawnAtNight = true;

	// Token: 0x04000E1A RID: 3610
	public bool m_spawnAtDay = true;

	// Token: 0x04000E1B RID: 3611
	public bool m_spawnWhenEnemiesCleared;

	// Token: 0x04000E1C RID: 3612
	public float m_enemiesCheckRange = 30f;

	// Token: 0x04000E1D RID: 3613
	private const float c_TriggerDistance = 20f;

	// Token: 0x04000E1E RID: 3614
	private ZNetView m_nview;

	// Token: 0x04000E1F RID: 3615
	private bool m_seenEnemies;
}
