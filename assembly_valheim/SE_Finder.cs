using System;
using UnityEngine;

// Token: 0x0200005A RID: 90
public class SE_Finder : StatusEffect
{
	// Token: 0x060004E4 RID: 1252 RVA: 0x00027CAC File Offset: 0x00025EAC
	public override void UpdateStatusEffect(float dt)
	{
		this.m_updateBeaconTimer += dt;
		if (this.m_updateBeaconTimer > 1f)
		{
			this.m_updateBeaconTimer = 0f;
			Beacon beacon = Beacon.FindClosestBeaconInRange(this.m_character.transform.position);
			if (beacon != this.m_beacon)
			{
				this.m_beacon = beacon;
				if (this.m_beacon)
				{
					this.m_lastDistance = Utils.DistanceXZ(this.m_character.transform.position, this.m_beacon.transform.position);
					this.m_pingTimer = 0f;
				}
			}
		}
		if (this.m_beacon != null)
		{
			float num = Utils.DistanceXZ(this.m_character.transform.position, this.m_beacon.transform.position);
			float num2 = Mathf.Clamp01(num / this.m_beacon.m_range);
			float num3 = Mathf.Lerp(this.m_closeFrequency, this.m_distantFrequency, num2);
			this.m_pingTimer += dt;
			if (this.m_pingTimer > num3)
			{
				this.m_pingTimer = 0f;
				if (num2 < 0.2f)
				{
					this.m_pingEffectNear.Create(this.m_character.transform.position, this.m_character.transform.rotation, this.m_character.transform, 1f, -1);
				}
				else if (num2 < 0.6f)
				{
					this.m_pingEffectMed.Create(this.m_character.transform.position, this.m_character.transform.rotation, this.m_character.transform, 1f, -1);
				}
				else
				{
					this.m_pingEffectFar.Create(this.m_character.transform.position, this.m_character.transform.rotation, this.m_character.transform, 1f, -1);
				}
				this.m_lastDistance = num;
			}
		}
	}

	// Token: 0x040005AE RID: 1454
	[Header("SE_Finder")]
	public EffectList m_pingEffectNear = new EffectList();

	// Token: 0x040005AF RID: 1455
	public EffectList m_pingEffectMed = new EffectList();

	// Token: 0x040005B0 RID: 1456
	public EffectList m_pingEffectFar = new EffectList();

	// Token: 0x040005B1 RID: 1457
	public float m_closerTriggerDistance = 2f;

	// Token: 0x040005B2 RID: 1458
	public float m_furtherTriggerDistance = 4f;

	// Token: 0x040005B3 RID: 1459
	public float m_closeFrequency = 1f;

	// Token: 0x040005B4 RID: 1460
	public float m_distantFrequency = 5f;

	// Token: 0x040005B5 RID: 1461
	private float m_updateBeaconTimer;

	// Token: 0x040005B6 RID: 1462
	private float m_pingTimer;

	// Token: 0x040005B7 RID: 1463
	private Beacon m_beacon;

	// Token: 0x040005B8 RID: 1464
	private float m_lastDistance;
}
