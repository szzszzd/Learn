using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000056 RID: 86
public class SEMan
{
	// Token: 0x060004B0 RID: 1200 RVA: 0x000269B8 File Offset: 0x00024BB8
	public SEMan(Character character, ZNetView nview)
	{
		this.m_character = character;
		this.m_nview = nview;
		this.m_nview.Register<int, bool, int, float>("RPC_AddStatusEffect", new RoutedMethod<int, bool, int, float>.Method(this.RPC_AddStatusEffect));
	}

	// Token: 0x060004B1 RID: 1201 RVA: 0x00026A14 File Offset: 0x00024C14
	public void OnDestroy()
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.OnDestroy();
		}
		this.m_statusEffects.Clear();
	}

	// Token: 0x060004B2 RID: 1202 RVA: 0x00026A70 File Offset: 0x00024C70
	public void ApplyStatusEffectSpeedMods(ref float speed)
	{
		float baseSpeed = speed;
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifySpeed(baseSpeed, ref speed);
		}
	}

	// Token: 0x060004B3 RID: 1203 RVA: 0x00026AC8 File Offset: 0x00024CC8
	public void ApplyStatusEffectJumpMods(ref Vector3 jump)
	{
		Vector3 baseJump = jump;
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyJump(baseJump, ref jump);
		}
	}

	// Token: 0x060004B4 RID: 1204 RVA: 0x00026B24 File Offset: 0x00024D24
	public void ApplyDamageMods(ref HitData.DamageModifiers mods)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyDamageMods(ref mods);
		}
	}

	// Token: 0x060004B5 RID: 1205 RVA: 0x00026B78 File Offset: 0x00024D78
	public void Update(float dt)
	{
		this.m_statusEffectAttributes = 0;
		int count = this.m_statusEffects.Count;
		for (int i = 0; i < count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			statusEffect.UpdateStatusEffect(dt);
			if (statusEffect.IsDone())
			{
				this.m_removeStatusEffects.Add(statusEffect);
			}
			else
			{
				this.m_statusEffectAttributes |= (int)statusEffect.m_attributes;
			}
		}
		if (this.m_removeStatusEffects.Count > 0)
		{
			foreach (StatusEffect statusEffect2 in this.m_removeStatusEffects)
			{
				statusEffect2.Stop();
				this.m_statusEffects.Remove(statusEffect2);
			}
			this.m_removeStatusEffects.Clear();
		}
		if (this.m_statusEffectAttributes != this.m_statusEffectAttributesOld)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_seAttrib, this.m_statusEffectAttributes, false);
			this.m_statusEffectAttributesOld = this.m_statusEffectAttributes;
		}
	}

	// Token: 0x060004B6 RID: 1206 RVA: 0x00026C88 File Offset: 0x00024E88
	public StatusEffect AddStatusEffect(int nameHash, bool resetTime = false, int itemLevel = 0, float skillLevel = 0f)
	{
		if (nameHash == 0)
		{
			return null;
		}
		if (this.m_nview.IsOwner())
		{
			return this.Internal_AddStatusEffect(nameHash, resetTime, itemLevel, skillLevel);
		}
		this.m_nview.InvokeRPC("RPC_AddStatusEffect", new object[]
		{
			nameHash,
			resetTime,
			itemLevel,
			skillLevel
		});
		return null;
	}

	// Token: 0x060004B7 RID: 1207 RVA: 0x00026CEF File Offset: 0x00024EEF
	private void RPC_AddStatusEffect(long sender, int nameHash, bool resetTime, int itemLevel, float skillLevel)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.Internal_AddStatusEffect(nameHash, resetTime, itemLevel, skillLevel);
	}

	// Token: 0x060004B8 RID: 1208 RVA: 0x00026D0C File Offset: 0x00024F0C
	private StatusEffect Internal_AddStatusEffect(int nameHash, bool resetTime, int itemLevel, float skillLevel)
	{
		StatusEffect statusEffect = this.GetStatusEffect(nameHash);
		if (statusEffect)
		{
			if (resetTime)
			{
				statusEffect.ResetTime();
				statusEffect.SetLevel(itemLevel, skillLevel);
			}
			return null;
		}
		StatusEffect statusEffect2 = ObjectDB.instance.GetStatusEffect(nameHash);
		if (statusEffect2 == null)
		{
			return null;
		}
		return this.AddStatusEffect(statusEffect2, false, itemLevel, skillLevel);
	}

	// Token: 0x060004B9 RID: 1209 RVA: 0x00026D60 File Offset: 0x00024F60
	public StatusEffect AddStatusEffect(StatusEffect statusEffect, bool resetTime = false, int itemLevel = 0, float skillLevel = 0f)
	{
		StatusEffect statusEffect2 = this.GetStatusEffect(statusEffect.NameHash());
		if (statusEffect2)
		{
			if (resetTime)
			{
				statusEffect2.ResetTime();
				statusEffect2.SetLevel(itemLevel, skillLevel);
			}
			return null;
		}
		if (!statusEffect.CanAdd(this.m_character))
		{
			return null;
		}
		StatusEffect statusEffect3 = statusEffect.Clone();
		this.m_statusEffects.Add(statusEffect3);
		statusEffect3.Setup(this.m_character);
		statusEffect3.SetLevel(itemLevel, skillLevel);
		if (this.m_character.IsPlayer())
		{
			Gogan.LogEvent("Game", "StatusEffect", statusEffect.name, 0L);
		}
		return statusEffect3;
	}

	// Token: 0x060004BA RID: 1210 RVA: 0x00026DF3 File Offset: 0x00024FF3
	public bool RemoveStatusEffect(StatusEffect se, bool quiet = false)
	{
		return this.RemoveStatusEffect(se.NameHash(), quiet);
	}

	// Token: 0x060004BB RID: 1211 RVA: 0x00026E04 File Offset: 0x00025004
	public bool RemoveStatusEffect(int nameHash, bool quiet = false)
	{
		if (nameHash == 0)
		{
			return false;
		}
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (statusEffect.NameHash() == nameHash)
			{
				if (quiet)
				{
					statusEffect.m_stopMessage = "";
				}
				statusEffect.Stop();
				this.m_statusEffects.Remove(statusEffect);
				return true;
			}
		}
		return false;
	}

	// Token: 0x060004BC RID: 1212 RVA: 0x00026E68 File Offset: 0x00025068
	public void RemoveAllStatusEffects(bool quiet = false)
	{
		for (int i = this.m_statusEffects.Count - 1; i >= 0; i--)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (quiet)
			{
				statusEffect.m_stopMessage = "";
			}
			statusEffect.Stop();
			this.m_statusEffects.Remove(statusEffect);
		}
	}

	// Token: 0x060004BD RID: 1213 RVA: 0x00026EBC File Offset: 0x000250BC
	public bool HaveStatusEffectCategory(string cat)
	{
		if (cat.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (statusEffect.m_category.Length > 0 && statusEffect.m_category == cat)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060004BE RID: 1214 RVA: 0x00026F18 File Offset: 0x00025118
	public bool HaveStatusAttribute(StatusEffect.StatusAttribute value)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.IsOwner())
		{
			return (this.m_statusEffectAttributes & (int)value) != 0;
		}
		return (this.m_nview.GetZDO().GetInt(ZDOVars.s_seAttrib, 0) & (int)value) != 0;
	}

	// Token: 0x060004BF RID: 1215 RVA: 0x00026F68 File Offset: 0x00025168
	public bool HaveStatusEffect(string name)
	{
		using (List<StatusEffect>.Enumerator enumerator = this.m_statusEffects.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.name == name)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060004C0 RID: 1216 RVA: 0x00026FC8 File Offset: 0x000251C8
	public bool HaveStatusEffect(int nameHash)
	{
		if (nameHash == 0)
		{
			return false;
		}
		using (List<StatusEffect>.Enumerator enumerator = this.m_statusEffects.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.NameHash() == nameHash)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060004C1 RID: 1217 RVA: 0x00027028 File Offset: 0x00025228
	public List<StatusEffect> GetStatusEffects()
	{
		return this.m_statusEffects;
	}

	// Token: 0x060004C2 RID: 1218 RVA: 0x00027030 File Offset: 0x00025230
	public StatusEffect GetStatusEffect(int nameHash)
	{
		if (nameHash == 0)
		{
			return null;
		}
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			if (statusEffect.NameHash() == nameHash)
			{
				return statusEffect;
			}
		}
		return null;
	}

	// Token: 0x060004C3 RID: 1219 RVA: 0x00027094 File Offset: 0x00025294
	public void GetHUDStatusEffects(List<StatusEffect> effects)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			if (statusEffect.m_icon)
			{
				effects.Add(statusEffect);
			}
		}
	}

	// Token: 0x060004C4 RID: 1220 RVA: 0x000270F4 File Offset: 0x000252F4
	public void ModifyFallDamage(float baseDamage, ref float damage)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyFallDamage(baseDamage, ref damage);
		}
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x00027148 File Offset: 0x00025348
	public void ModifyWalkVelocity(ref Vector3 vel)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyWalkVelocity(ref vel);
		}
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x0002719C File Offset: 0x0002539C
	public void ModifyNoise(float baseNoise, ref float noise)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyNoise(baseNoise, ref noise);
		}
	}

	// Token: 0x060004C7 RID: 1223 RVA: 0x000271F0 File Offset: 0x000253F0
	public void ModifySkillLevel(Skills.SkillType skill, ref float level)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifySkillLevel(skill, ref level);
		}
	}

	// Token: 0x060004C8 RID: 1224 RVA: 0x00027244 File Offset: 0x00025444
	public void ModifyRaiseSkill(Skills.SkillType skill, ref float multiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyRaiseSkill(skill, ref multiplier);
		}
	}

	// Token: 0x060004C9 RID: 1225 RVA: 0x00027298 File Offset: 0x00025498
	public void ModifyStaminaRegen(ref float staminaMultiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyStaminaRegen(ref staminaMultiplier);
		}
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x000272EC File Offset: 0x000254EC
	public void ModifyEitrRegen(ref float eitrMultiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyEitrRegen(ref eitrMultiplier);
		}
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x00027340 File Offset: 0x00025540
	public void ModifyHealthRegen(ref float regenMultiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyHealthRegen(ref regenMultiplier);
		}
	}

	// Token: 0x060004CC RID: 1228 RVA: 0x00027394 File Offset: 0x00025594
	public void ModifyMaxCarryWeight(float baseLimit, ref float limit)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyMaxCarryWeight(baseLimit, ref limit);
		}
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x000273E8 File Offset: 0x000255E8
	public void ModifyStealth(float baseStealth, ref float stealth)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyStealth(baseStealth, ref stealth);
		}
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x0002743C File Offset: 0x0002563C
	public void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyAttack(skill, ref hitData);
		}
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x00027490 File Offset: 0x00025690
	public void ModifyRunStaminaDrain(float baseDrain, ref float drain)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyRunStaminaDrain(baseDrain, ref drain);
		}
		if (drain < 0f)
		{
			drain = 0f;
		}
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x000274F4 File Offset: 0x000256F4
	public void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyJumpStaminaUsage(baseStaminaUse, ref staminaUse);
		}
		if (staminaUse < 0f)
		{
			staminaUse = 0f;
		}
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x00027558 File Offset: 0x00025758
	public void OnDamaged(HitData hit, Character attacker)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.OnDamaged(hit, attacker);
		}
	}

	// Token: 0x0400058C RID: 1420
	private List<StatusEffect> m_statusEffects = new List<StatusEffect>();

	// Token: 0x0400058D RID: 1421
	private List<StatusEffect> m_removeStatusEffects = new List<StatusEffect>();

	// Token: 0x0400058E RID: 1422
	private int m_statusEffectAttributes;

	// Token: 0x0400058F RID: 1423
	private int m_statusEffectAttributesOld = -1;

	// Token: 0x04000590 RID: 1424
	private Character m_character;

	// Token: 0x04000591 RID: 1425
	private ZNetView m_nview;
}
