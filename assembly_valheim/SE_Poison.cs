using System;
using UnityEngine;

// Token: 0x0200005E RID: 94
public class SE_Poison : StatusEffect
{
	// Token: 0x060004F2 RID: 1266 RVA: 0x0002846C File Offset: 0x0002666C
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_timer -= dt;
		if (this.m_timer <= 0f)
		{
			this.m_timer = this.m_damageInterval;
			HitData hitData = new HitData();
			hitData.m_point = this.m_character.GetCenterPoint();
			hitData.m_damage.m_poison = this.m_damagePerHit;
			this.m_damageLeft -= this.m_damagePerHit;
			this.m_character.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
		}
	}

	// Token: 0x060004F3 RID: 1267 RVA: 0x000284F4 File Offset: 0x000266F4
	public void AddDamage(float damage)
	{
		if (damage >= this.m_damageLeft)
		{
			this.m_damageLeft = damage;
			float num = this.m_character.IsPlayer() ? this.m_TTLPerDamagePlayer : this.m_TTLPerDamage;
			this.m_ttl = this.m_baseTTL + Mathf.Pow(this.m_damageLeft * num, this.m_TTLPower);
			int num2 = (int)(this.m_ttl / this.m_damageInterval);
			this.m_damagePerHit = this.m_damageLeft / (float)num2;
			ZLog.Log(string.Concat(new string[]
			{
				"Poison damage: ",
				this.m_damageLeft.ToString(),
				" ttl:",
				this.m_ttl.ToString(),
				" hits:",
				num2.ToString(),
				" dmg perhit:",
				this.m_damagePerHit.ToString()
			}));
			this.ResetTime();
		}
	}

	// Token: 0x040005CD RID: 1485
	[Header("SE_Poison")]
	public float m_damageInterval = 1f;

	// Token: 0x040005CE RID: 1486
	public float m_baseTTL = 2f;

	// Token: 0x040005CF RID: 1487
	public float m_TTLPerDamagePlayer = 2f;

	// Token: 0x040005D0 RID: 1488
	public float m_TTLPerDamage = 2f;

	// Token: 0x040005D1 RID: 1489
	public float m_TTLPower = 0.5f;

	// Token: 0x040005D2 RID: 1490
	private float m_timer;

	// Token: 0x040005D3 RID: 1491
	private float m_damageLeft;

	// Token: 0x040005D4 RID: 1492
	private float m_damagePerHit;
}
