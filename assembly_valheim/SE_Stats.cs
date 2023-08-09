using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000064 RID: 100
public class SE_Stats : StatusEffect
{
	// Token: 0x0600050D RID: 1293 RVA: 0x00028C6C File Offset: 0x00026E6C
	public override void Setup(Character character)
	{
		base.Setup(character);
		if (this.m_healthOverTime > 0f && this.m_healthOverTimeInterval > 0f)
		{
			if (this.m_healthOverTimeDuration <= 0f)
			{
				this.m_healthOverTimeDuration = this.m_ttl;
			}
			this.m_healthOverTimeTicks = this.m_healthOverTimeDuration / this.m_healthOverTimeInterval;
			this.m_healthOverTimeTickHP = this.m_healthOverTime / this.m_healthOverTimeTicks;
		}
		if (this.m_staminaOverTime > 0f && this.m_staminaOverTimeDuration <= 0f)
		{
			this.m_staminaOverTimeDuration = this.m_ttl;
		}
		if (this.m_eitrOverTime > 0f && this.m_eitrOverTimeDuration <= 0f)
		{
			this.m_eitrOverTimeDuration = this.m_ttl;
		}
	}

	// Token: 0x0600050E RID: 1294 RVA: 0x00028D28 File Offset: 0x00026F28
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_tickInterval > 0f)
		{
			this.m_tickTimer += dt;
			if (this.m_tickTimer >= this.m_tickInterval)
			{
				this.m_tickTimer = 0f;
				if (this.m_character.GetHealthPercentage() >= this.m_healthPerTickMinHealthPercentage)
				{
					if (this.m_healthPerTick > 0f)
					{
						this.m_character.Heal(this.m_healthPerTick, true);
					}
					else
					{
						HitData hitData = new HitData();
						hitData.m_damage.m_damage = -this.m_healthPerTick;
						hitData.m_point = this.m_character.GetTopPoint();
						this.m_character.Damage(hitData);
					}
				}
			}
		}
		if (this.m_healthOverTimeTicks > 0f)
		{
			this.m_healthOverTimeTimer += dt;
			if (this.m_healthOverTimeTimer > this.m_healthOverTimeInterval)
			{
				this.m_healthOverTimeTimer = 0f;
				this.m_healthOverTimeTicks -= 1f;
				this.m_character.Heal(this.m_healthOverTimeTickHP, true);
			}
		}
		if (this.m_staminaOverTime != 0f && this.m_time <= this.m_staminaOverTimeDuration)
		{
			float num = this.m_staminaOverTimeDuration / dt;
			this.m_character.AddStamina(this.m_staminaOverTime / num);
		}
		if (this.m_eitrOverTime != 0f && this.m_time <= this.m_eitrOverTimeDuration)
		{
			float num2 = this.m_eitrOverTimeDuration / dt;
			this.m_character.AddEitr(this.m_eitrOverTime / num2);
		}
		if (this.m_staminaDrainPerSec > 0f)
		{
			this.m_character.UseStamina(this.m_staminaDrainPerSec * dt);
		}
	}

	// Token: 0x0600050F RID: 1295 RVA: 0x00028EC2 File Offset: 0x000270C2
	public override void ModifyHealthRegen(ref float regenMultiplier)
	{
		if (this.m_healthRegenMultiplier > 1f)
		{
			regenMultiplier += this.m_healthRegenMultiplier - 1f;
			return;
		}
		regenMultiplier *= this.m_healthRegenMultiplier;
	}

	// Token: 0x06000510 RID: 1296 RVA: 0x00028EEE File Offset: 0x000270EE
	public override void ModifyStaminaRegen(ref float staminaRegen)
	{
		if (this.m_staminaRegenMultiplier > 1f)
		{
			staminaRegen += this.m_staminaRegenMultiplier - 1f;
			return;
		}
		staminaRegen *= this.m_staminaRegenMultiplier;
	}

	// Token: 0x06000511 RID: 1297 RVA: 0x00028F1A File Offset: 0x0002711A
	public override void ModifyEitrRegen(ref float staminaRegen)
	{
		if (this.m_eitrRegenMultiplier > 1f)
		{
			staminaRegen += this.m_eitrRegenMultiplier - 1f;
			return;
		}
		staminaRegen *= this.m_eitrRegenMultiplier;
	}

	// Token: 0x06000512 RID: 1298 RVA: 0x00028F46 File Offset: 0x00027146
	public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
	{
		modifiers.Apply(this.m_mods);
	}

	// Token: 0x06000513 RID: 1299 RVA: 0x00028F54 File Offset: 0x00027154
	public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
	{
		if (this.m_raiseSkill == Skills.SkillType.None)
		{
			return;
		}
		if (this.m_raiseSkill == Skills.SkillType.All || this.m_raiseSkill == skill)
		{
			value += this.m_raiseSkillModifier;
		}
	}

	// Token: 0x06000514 RID: 1300 RVA: 0x00028F80 File Offset: 0x00027180
	public override void ModifySkillLevel(Skills.SkillType skill, ref float value)
	{
		if (this.m_skillLevel == Skills.SkillType.None)
		{
			return;
		}
		if (this.m_skillLevel == Skills.SkillType.All || this.m_skillLevel == skill)
		{
			value += this.m_skillLevelModifier;
		}
		if (this.m_skillLevel2 == Skills.SkillType.All || this.m_skillLevel2 == skill)
		{
			value += this.m_skillLevelModifier2;
		}
	}

	// Token: 0x06000515 RID: 1301 RVA: 0x00028FD8 File Offset: 0x000271D8
	public override void ModifyNoise(float baseNoise, ref float noise)
	{
		noise += baseNoise * this.m_noiseModifier;
	}

	// Token: 0x06000516 RID: 1302 RVA: 0x00028FE7 File Offset: 0x000271E7
	public override void ModifyStealth(float baseStealth, ref float stealth)
	{
		stealth += baseStealth * this.m_stealthModifier;
	}

	// Token: 0x06000517 RID: 1303 RVA: 0x00028FF6 File Offset: 0x000271F6
	public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
	{
		limit += this.m_addMaxCarryWeight;
		if (limit < 0f)
		{
			limit = 0f;
		}
	}

	// Token: 0x06000518 RID: 1304 RVA: 0x00029013 File Offset: 0x00027213
	public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
	{
		if (skill == this.m_modifyAttackSkill || this.m_modifyAttackSkill == Skills.SkillType.All)
		{
			hitData.m_damage.Modify(this.m_damageModifier);
		}
	}

	// Token: 0x06000519 RID: 1305 RVA: 0x0002903D File Offset: 0x0002723D
	public override void ModifyRunStaminaDrain(float baseDrain, ref float drain)
	{
		drain += baseDrain * this.m_runStaminaDrainModifier;
	}

	// Token: 0x0600051A RID: 1306 RVA: 0x0002904C File Offset: 0x0002724C
	public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
	{
		staminaUse += baseStaminaUse * this.m_jumpStaminaUseModifier;
	}

	// Token: 0x0600051B RID: 1307 RVA: 0x0002905C File Offset: 0x0002725C
	public override void ModifySpeed(float baseSpeed, ref float speed)
	{
		if (this.m_character.IsSwimming())
		{
			speed += baseSpeed * this.m_speedModifier * 0.5f;
		}
		else
		{
			speed += baseSpeed * this.m_speedModifier;
		}
		if (speed < 0f)
		{
			speed = 0f;
		}
	}

	// Token: 0x0600051C RID: 1308 RVA: 0x000290A8 File Offset: 0x000272A8
	public override void ModifyJump(Vector3 baseJump, ref Vector3 jump)
	{
		jump += new Vector3(baseJump.x * this.m_jumpModifier.x, baseJump.y * this.m_jumpModifier.y, baseJump.z * this.m_jumpModifier.z);
	}

	// Token: 0x0600051D RID: 1309 RVA: 0x00029101 File Offset: 0x00027301
	public override void ModifyWalkVelocity(ref Vector3 vel)
	{
		if (this.m_maxMaxFallSpeed > 0f && vel.y < -this.m_maxMaxFallSpeed)
		{
			vel.y = -this.m_maxMaxFallSpeed;
		}
	}

	// Token: 0x0600051E RID: 1310 RVA: 0x0002912C File Offset: 0x0002732C
	public override void ModifyFallDamage(float baseDamage, ref float damage)
	{
		damage += baseDamage * this.m_fallDamageModifier;
		if (damage < 0f)
		{
			damage = 0f;
		}
	}

	// Token: 0x0600051F RID: 1311 RVA: 0x0002914C File Offset: 0x0002734C
	public override string GetTooltipString()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		if (this.m_tooltip.Length > 0)
		{
			stringBuilder.AppendFormat("{0}\n", this.m_tooltip);
		}
		if (this.m_jumpStaminaUseModifier != 0f)
		{
			stringBuilder.AppendFormat("$se_jumpstamina: <color=orange>{0}%</color>\n", (this.m_jumpStaminaUseModifier * 100f).ToString("+0;-0"));
		}
		if (this.m_runStaminaDrainModifier != 0f)
		{
			stringBuilder.AppendFormat("$se_runstamina: <color=orange>{0}%</color>\n", (this.m_runStaminaDrainModifier * 100f).ToString("+0;-0"));
		}
		if (this.m_healthOverTime != 0f)
		{
			stringBuilder.AppendFormat("$se_health: <color=orange>{0}</color>\n", this.m_healthOverTime.ToString());
		}
		if (this.m_staminaOverTime != 0f)
		{
			stringBuilder.AppendFormat("$se_stamina: <color=orange>{0}</color>\n", this.m_staminaOverTime.ToString());
		}
		if (this.m_eitrOverTime != 0f)
		{
			stringBuilder.AppendFormat("$se_eitr: <color=orange>{0}</color>\n", this.m_eitrOverTime.ToString());
		}
		if (this.m_healthRegenMultiplier != 1f)
		{
			stringBuilder.AppendFormat("$se_healthregen: <color=orange>{0}%</color>\n", ((this.m_healthRegenMultiplier - 1f) * 100f).ToString("+0;-0"));
		}
		if (this.m_staminaRegenMultiplier != 1f)
		{
			stringBuilder.AppendFormat("$se_staminaregen: <color=orange>{0}%</color>\n", ((this.m_staminaRegenMultiplier - 1f) * 100f).ToString("+0;-0"));
		}
		if (this.m_eitrRegenMultiplier != 1f)
		{
			stringBuilder.AppendFormat("$se_eitrregen: <color=orange>{0}%</color>\n", ((this.m_eitrRegenMultiplier - 1f) * 100f).ToString("+0;-0"));
		}
		if (this.m_addMaxCarryWeight != 0f)
		{
			stringBuilder.AppendFormat("$se_max_carryweight: <color=orange>{0}</color>\n", this.m_addMaxCarryWeight.ToString("+0;-0"));
		}
		if (this.m_mods.Count > 0)
		{
			stringBuilder.Append(SE_Stats.GetDamageModifiersTooltipString(this.m_mods));
			stringBuilder.Append("\n");
		}
		if (this.m_noiseModifier != 0f)
		{
			stringBuilder.AppendFormat("$se_noisemod: <color=orange>{0}%</color>\n", (this.m_noiseModifier * 100f).ToString("+0;-0"));
		}
		if (this.m_stealthModifier != 0f)
		{
			stringBuilder.AppendFormat("$se_sneakmod: <color=orange>{0}%</color>\n", (this.m_stealthModifier * 100f).ToString("+0;-0"));
		}
		if (this.m_speedModifier != 0f)
		{
			stringBuilder.AppendFormat("$item_movement_modifier: <color=orange>{0}%</color>\n", (this.m_speedModifier * 100f).ToString("+0;-0"));
		}
		if (this.m_maxMaxFallSpeed != 0f)
		{
			stringBuilder.AppendFormat("$item_limitfallspeed: <color=orange>{0}m/s</color>\n", this.m_maxMaxFallSpeed.ToString("0"));
		}
		if (this.m_fallDamageModifier != 0f)
		{
			stringBuilder.AppendFormat("$item_falldamage: <color=orange>{0}%</color>\n", (this.m_fallDamageModifier * 100f).ToString("+0;-0"));
		}
		if (this.m_skillLevel != Skills.SkillType.None)
		{
			stringBuilder.AppendFormat("{0} <color=orange>{1}</color>\n", Localization.instance.Localize("$skill_" + this.m_skillLevel.ToString().ToLower()), this.m_skillLevelModifier.ToString("+0;-0"));
		}
		if (this.m_skillLevel2 != Skills.SkillType.None)
		{
			stringBuilder.AppendFormat("{0} <color=orange>{1}</color>\n", Localization.instance.Localize("$skill_" + this.m_skillLevel2.ToString().ToLower()), this.m_skillLevelModifier2.ToString("+0;-0"));
		}
		return stringBuilder.ToString();
	}

	// Token: 0x06000520 RID: 1312 RVA: 0x000294E8 File Offset: 0x000276E8
	public static string GetDamageModifiersTooltipString(List<HitData.DamageModPair> mods)
	{
		if (mods.Count == 0)
		{
			return "";
		}
		string text = "";
		foreach (HitData.DamageModPair damageModPair in mods)
		{
			if (damageModPair.m_modifier != HitData.DamageModifier.Ignore && damageModPair.m_modifier != HitData.DamageModifier.Normal)
			{
				switch (damageModPair.m_modifier)
				{
				case HitData.DamageModifier.Resistant:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_resistant</color> VS ";
					break;
				case HitData.DamageModifier.Weak:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_weak</color> VS ";
					break;
				case HitData.DamageModifier.Immune:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_immune</color> VS ";
					break;
				case HitData.DamageModifier.VeryResistant:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_veryresistant</color> VS ";
					break;
				case HitData.DamageModifier.VeryWeak:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_veryweak</color> VS ";
					break;
				}
				text += "<color=orange>";
				HitData.DamageType type = damageModPair.m_type;
				if (type <= HitData.DamageType.Fire)
				{
					if (type <= HitData.DamageType.Chop)
					{
						switch (type)
						{
						case HitData.DamageType.Blunt:
							text += "$inventory_blunt";
							break;
						case HitData.DamageType.Slash:
							text += "$inventory_slash";
							break;
						case HitData.DamageType.Blunt | HitData.DamageType.Slash:
							break;
						case HitData.DamageType.Pierce:
							text += "$inventory_pierce";
							break;
						default:
							if (type == HitData.DamageType.Chop)
							{
								text += "$inventory_chop";
							}
							break;
						}
					}
					else if (type != HitData.DamageType.Pickaxe)
					{
						if (type == HitData.DamageType.Fire)
						{
							text += "$inventory_fire";
						}
					}
					else
					{
						text += "$inventory_pickaxe";
					}
				}
				else if (type <= HitData.DamageType.Lightning)
				{
					if (type != HitData.DamageType.Frost)
					{
						if (type == HitData.DamageType.Lightning)
						{
							text += "$inventory_lightning";
						}
					}
					else
					{
						text += "$inventory_frost";
					}
				}
				else if (type != HitData.DamageType.Poison)
				{
					if (type == HitData.DamageType.Spirit)
					{
						text += "$inventory_spirit";
					}
				}
				else
				{
					text += "$inventory_poison";
				}
				text += "</color>";
			}
		}
		return text;
	}

	// Token: 0x040005ED RID: 1517
	[Header("__SE_Stats__")]
	[Header("HP per tick")]
	public float m_tickInterval;

	// Token: 0x040005EE RID: 1518
	public float m_healthPerTickMinHealthPercentage;

	// Token: 0x040005EF RID: 1519
	public float m_healthPerTick;

	// Token: 0x040005F0 RID: 1520
	[Header("Health over time")]
	public float m_healthOverTime;

	// Token: 0x040005F1 RID: 1521
	public float m_healthOverTimeDuration;

	// Token: 0x040005F2 RID: 1522
	public float m_healthOverTimeInterval = 5f;

	// Token: 0x040005F3 RID: 1523
	[Header("Stamina")]
	public float m_staminaOverTime;

	// Token: 0x040005F4 RID: 1524
	public float m_staminaOverTimeDuration;

	// Token: 0x040005F5 RID: 1525
	public float m_staminaDrainPerSec;

	// Token: 0x040005F6 RID: 1526
	public float m_runStaminaDrainModifier;

	// Token: 0x040005F7 RID: 1527
	public float m_jumpStaminaUseModifier;

	// Token: 0x040005F8 RID: 1528
	[Header("Eitr")]
	public float m_eitrOverTime;

	// Token: 0x040005F9 RID: 1529
	public float m_eitrOverTimeDuration;

	// Token: 0x040005FA RID: 1530
	[Header("Regen modifiers")]
	public float m_healthRegenMultiplier = 1f;

	// Token: 0x040005FB RID: 1531
	public float m_staminaRegenMultiplier = 1f;

	// Token: 0x040005FC RID: 1532
	public float m_eitrRegenMultiplier = 1f;

	// Token: 0x040005FD RID: 1533
	[Header("Modify raise skill")]
	public Skills.SkillType m_raiseSkill;

	// Token: 0x040005FE RID: 1534
	public float m_raiseSkillModifier;

	// Token: 0x040005FF RID: 1535
	[Header("Modify skill level")]
	public Skills.SkillType m_skillLevel;

	// Token: 0x04000600 RID: 1536
	public float m_skillLevelModifier;

	// Token: 0x04000601 RID: 1537
	public Skills.SkillType m_skillLevel2;

	// Token: 0x04000602 RID: 1538
	public float m_skillLevelModifier2;

	// Token: 0x04000603 RID: 1539
	[Header("Hit modifier")]
	public List<HitData.DamageModPair> m_mods = new List<HitData.DamageModPair>();

	// Token: 0x04000604 RID: 1540
	[Header("Attack")]
	public Skills.SkillType m_modifyAttackSkill;

	// Token: 0x04000605 RID: 1541
	public float m_damageModifier = 1f;

	// Token: 0x04000606 RID: 1542
	[Header("Sneak")]
	public float m_noiseModifier;

	// Token: 0x04000607 RID: 1543
	public float m_stealthModifier;

	// Token: 0x04000608 RID: 1544
	[Header("Carry weight")]
	public float m_addMaxCarryWeight;

	// Token: 0x04000609 RID: 1545
	[Header("Speed")]
	public float m_speedModifier;

	// Token: 0x0400060A RID: 1546
	public Vector3 m_jumpModifier;

	// Token: 0x0400060B RID: 1547
	[Header("Fall")]
	public float m_maxMaxFallSpeed;

	// Token: 0x0400060C RID: 1548
	public float m_fallDamageModifier;

	// Token: 0x0400060D RID: 1549
	private float m_tickTimer;

	// Token: 0x0400060E RID: 1550
	private float m_healthOverTimeTimer;

	// Token: 0x0400060F RID: 1551
	private float m_healthOverTimeTicks;

	// Token: 0x04000610 RID: 1552
	private float m_healthOverTimeTickHP;
}
