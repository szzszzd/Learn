using System;
using UnityEngine;

// Token: 0x02000061 RID: 97
public class SE_Shield : StatusEffect
{
	// Token: 0x06000502 RID: 1282 RVA: 0x000275AC File Offset: 0x000257AC
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x06000503 RID: 1283 RVA: 0x000288AC File Offset: 0x00026AAC
	public override bool IsDone()
	{
		if (this.m_damage > this.m_totalAbsorbDamage)
		{
			this.m_breakEffects.Create(this.m_character.GetCenterPoint(), this.m_character.transform.rotation, this.m_character.transform, this.m_character.GetRadius() * 2f, -1);
			if (this.m_levelUpSkillOnBreak != Skills.SkillType.None)
			{
				Skills skills = this.m_character.GetSkills();
				if (skills != null && skills)
				{
					skills.RaiseSkill(this.m_levelUpSkillOnBreak, this.m_levelUpSkillFactor);
					Terminal.Log(string.Format("{0} is leveling up {1} at factor {2}", this.m_name, this.m_levelUpSkillOnBreak, this.m_levelUpSkillFactor));
				}
			}
			return true;
		}
		return base.IsDone();
	}

	// Token: 0x06000504 RID: 1284 RVA: 0x00028974 File Offset: 0x00026B74
	public override void OnDamaged(HitData hit, Character attacker)
	{
		float totalDamage = hit.GetTotalDamage();
		this.m_damage += totalDamage;
		hit.ApplyModifier(0f);
		this.m_hitEffects.Create(hit.m_point, Quaternion.LookRotation(-hit.m_dir), this.m_character.transform, 1f, -1);
	}

	// Token: 0x06000505 RID: 1285 RVA: 0x000289D4 File Offset: 0x00026BD4
	public override void SetLevel(int itemLevel, float skillLevel)
	{
		if (this.m_ttlPerItemLevel > 0)
		{
			this.m_ttl = (float)(this.m_ttlPerItemLevel * itemLevel);
		}
		this.m_totalAbsorbDamage = this.m_absorbDamage + this.m_absorbDamagePerSkillLevel * skillLevel;
		Terminal.Log(string.Format("Shield setting itemlevel: {0} = ttl: {1}, skilllevel: {2} = absorb: {3}", new object[]
		{
			itemLevel,
			this.m_ttl,
			skillLevel,
			this.m_totalAbsorbDamage
		}));
		base.SetLevel(itemLevel, skillLevel);
	}

	// Token: 0x06000506 RID: 1286 RVA: 0x00028A5C File Offset: 0x00026C5C
	public override string GetTooltipString()
	{
		return string.Concat(new string[]
		{
			base.GetTooltipString(),
			"\n$se_shield_ttl <color=orange>",
			this.m_ttl.ToString("0"),
			"</color>\n$se_shield_damage <color=orange>",
			this.m_totalAbsorbDamage.ToString("0"),
			"</color>"
		});
	}

	// Token: 0x040005DC RID: 1500
	[Header("__SE_Shield__")]
	public float m_absorbDamage = 100f;

	// Token: 0x040005DD RID: 1501
	public Skills.SkillType m_levelUpSkillOnBreak;

	// Token: 0x040005DE RID: 1502
	public float m_levelUpSkillFactor = 1f;

	// Token: 0x040005DF RID: 1503
	public int m_ttlPerItemLevel;

	// Token: 0x040005E0 RID: 1504
	public float m_absorbDamagePerSkillLevel;

	// Token: 0x040005E1 RID: 1505
	public EffectList m_breakEffects = new EffectList();

	// Token: 0x040005E2 RID: 1506
	public EffectList m_hitEffects = new EffectList();

	// Token: 0x040005E3 RID: 1507
	private float m_totalAbsorbDamage;

	// Token: 0x040005E4 RID: 1508
	private float m_damage;
}
