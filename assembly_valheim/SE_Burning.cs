using System;
using UnityEngine;

// Token: 0x02000057 RID: 87
public class SE_Burning : StatusEffect
{
	// Token: 0x060004D2 RID: 1234 RVA: 0x000275AC File Offset: 0x000257AC
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x000275B8 File Offset: 0x000257B8
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_fireDamageLeft > 0f && this.m_character.GetSEMan().HaveStatusEffect("Wet"))
		{
			this.m_time += dt * 5f;
		}
		this.m_timer -= dt;
		if (this.m_timer <= 0f)
		{
			this.m_timer = this.m_damageInterval;
			HitData hitData = new HitData();
			hitData.m_point = this.m_character.GetCenterPoint();
			hitData.m_damage.m_fire = this.m_fireDamagePerHit;
			hitData.m_damage.m_spirit = this.m_spiritDamagePerHit;
			this.m_fireDamageLeft = Mathf.Max(0f, this.m_fireDamageLeft - this.m_fireDamagePerHit);
			this.m_spiritDamageLeft = Mathf.Max(0f, this.m_spiritDamageLeft - this.m_spiritDamagePerHit);
			this.m_character.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
		}
	}

	// Token: 0x060004D4 RID: 1236 RVA: 0x000276B0 File Offset: 0x000258B0
	public bool AddFireDamage(float damage)
	{
		int num = (int)(this.m_ttl / this.m_damageInterval);
		if (damage / (float)num < 0.2f && this.m_fireDamageLeft == 0f)
		{
			return false;
		}
		this.m_fireDamageLeft += damage;
		this.m_fireDamagePerHit = this.m_fireDamageLeft / (float)num;
		this.ResetTime();
		return true;
	}

	// Token: 0x060004D5 RID: 1237 RVA: 0x0002770C File Offset: 0x0002590C
	public bool AddSpiritDamage(float damage)
	{
		int num = (int)(this.m_ttl / this.m_damageInterval);
		if (damage / (float)num < 0.2f && this.m_spiritDamageLeft == 0f)
		{
			return false;
		}
		this.m_spiritDamageLeft += damage;
		this.m_spiritDamagePerHit = this.m_spiritDamageLeft / (float)num;
		this.ResetTime();
		return true;
	}

	// Token: 0x04000592 RID: 1426
	[Header("SE_Burning")]
	public float m_damageInterval = 1f;

	// Token: 0x04000593 RID: 1427
	private float m_timer;

	// Token: 0x04000594 RID: 1428
	private float m_fireDamageLeft;

	// Token: 0x04000595 RID: 1429
	private float m_fireDamagePerHit;

	// Token: 0x04000596 RID: 1430
	private float m_spiritDamageLeft;

	// Token: 0x04000597 RID: 1431
	private float m_spiritDamagePerHit;

	// Token: 0x04000598 RID: 1432
	private const float m_minimumDamageTick = 0.2f;
}
