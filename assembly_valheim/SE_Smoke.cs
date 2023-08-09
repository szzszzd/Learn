using System;
using UnityEngine;

// Token: 0x02000062 RID: 98
public class SE_Smoke : StatusEffect
{
	// Token: 0x06000508 RID: 1288 RVA: 0x00028AEF File Offset: 0x00026CEF
	public override bool CanAdd(Character character)
	{
		return !character.m_tolerateSmoke && base.CanAdd(character);
	}

	// Token: 0x06000509 RID: 1289 RVA: 0x00028B04 File Offset: 0x00026D04
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_timer += dt;
		if (this.m_timer > this.m_damageInterval)
		{
			this.m_timer = 0f;
			HitData hitData = new HitData();
			hitData.m_point = this.m_character.GetCenterPoint();
			hitData.m_damage = this.m_damage;
			this.m_character.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
		}
	}

	// Token: 0x040005E5 RID: 1509
	[Header("SE_Burning")]
	public HitData.DamageTypes m_damage;

	// Token: 0x040005E6 RID: 1510
	public float m_damageInterval = 1f;

	// Token: 0x040005E7 RID: 1511
	private float m_timer;
}
