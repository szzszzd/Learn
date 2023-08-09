using System;
using UnityEngine;

// Token: 0x02000046 RID: 70
public interface IProjectile
{
	// Token: 0x060003F3 RID: 1011
	void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo);

	// Token: 0x060003F4 RID: 1012
	string GetTooltipString(int itemQuality);
}
