using System;
using UnityEngine;

// Token: 0x0200004F RID: 79
public class TriggerSpawnAbility : MonoBehaviour, IProjectile
{
	// Token: 0x0600041B RID: 1051 RVA: 0x00021A1B File Offset: 0x0001FC1B
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		this.m_owner = owner;
		TriggerSpawner.TriggerAllInRange(base.transform.position, this.m_range);
	}

	// Token: 0x0600041C RID: 1052 RVA: 0x0000C988 File Offset: 0x0000AB88
	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	// Token: 0x040004CD RID: 1229
	[Header("Spawn")]
	public float m_range = 10f;

	// Token: 0x040004CE RID: 1230
	private Character m_owner;
}
