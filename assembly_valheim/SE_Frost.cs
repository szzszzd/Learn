using System;
using UnityEngine;

// Token: 0x0200005B RID: 91
public class SE_Frost : StatusEffect
{
	// Token: 0x060004E6 RID: 1254 RVA: 0x00027F04 File Offset: 0x00026104
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
	}

	// Token: 0x060004E7 RID: 1255 RVA: 0x00027F10 File Offset: 0x00026110
	public void AddDamage(float damage)
	{
		float num = this.m_character.IsPlayer() ? this.m_freezeTimePlayer : this.m_freezeTimeEnemy;
		float num2 = Mathf.Clamp01(damage / this.m_character.GetMaxHealth()) * num;
		float num3 = this.m_ttl - this.m_time;
		if (num2 > num3)
		{
			this.m_ttl = num2;
			this.ResetTime();
			base.TriggerStartEffects();
		}
	}

	// Token: 0x060004E8 RID: 1256 RVA: 0x00027F74 File Offset: 0x00026174
	public override void ModifySpeed(float baseSpeed, ref float speed)
	{
		float num = Mathf.Clamp01(this.m_time / this.m_ttl);
		num = Mathf.Pow(num, 2f);
		speed -= baseSpeed * Mathf.Lerp(1f - this.m_minSpeedFactor, 0f, num);
		if (speed < 0f)
		{
			speed = 0f;
		}
	}

	// Token: 0x040005B9 RID: 1465
	[Header("SE_Frost")]
	public float m_freezeTimeEnemy = 10f;

	// Token: 0x040005BA RID: 1466
	public float m_freezeTimePlayer = 10f;

	// Token: 0x040005BB RID: 1467
	public float m_minSpeedFactor = 0.1f;
}
