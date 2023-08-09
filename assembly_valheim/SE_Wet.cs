using System;
using UnityEngine;

// Token: 0x02000065 RID: 101
public class SE_Wet : SE_Stats
{
	// Token: 0x06000522 RID: 1314 RVA: 0x00028617 File Offset: 0x00026817
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x06000523 RID: 1315 RVA: 0x00029764 File Offset: 0x00027964
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (!this.m_character.m_tolerateWater)
		{
			this.m_timer += dt;
			if (this.m_timer > this.m_damageInterval)
			{
				this.m_timer = 0f;
				HitData hitData = new HitData();
				hitData.m_point = this.m_character.transform.position;
				hitData.m_damage.m_damage = this.m_waterDamage;
				this.m_character.Damage(hitData);
			}
		}
		if (this.m_character.GetSEMan().HaveStatusEffect("CampFire"))
		{
			this.m_time += dt * 10f;
		}
		if (this.m_character.GetSEMan().HaveStatusEffect("Burning"))
		{
			this.m_time += dt * 50f;
		}
	}

	// Token: 0x04000611 RID: 1553
	[Header("__SE_Wet__")]
	public float m_waterDamage;

	// Token: 0x04000612 RID: 1554
	public float m_damageInterval = 0.5f;

	// Token: 0x04000613 RID: 1555
	private float m_timer;
}
