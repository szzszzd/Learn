using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000244 RID: 580
public class HitData
{
	// Token: 0x060016E8 RID: 5864 RVA: 0x0009757C File Offset: 0x0009577C
	public HitData Clone()
	{
		return (HitData)base.MemberwiseClone();
	}

	// Token: 0x060016E9 RID: 5865 RVA: 0x0009758C File Offset: 0x0009578C
	public void Serialize(ref ZPackage pkg)
	{
		HitData.HitDefaults.SerializeFlags serializeFlags = HitData.HitDefaults.SerializeFlags.None;
		serializeFlags |= ((!this.m_damage.m_damage.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.Damage : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_blunt.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageBlunt : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_slash.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageSlash : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_pierce.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamagePierce : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_chop.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageChop : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_pickaxe.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamagePickaxe : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_fire.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageFire : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_frost.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageFrost : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_lightning.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageLightning : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_poison.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamagePoison : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_damage.m_spirit.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.DamageSpirit : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_pushForce.Equals(0f)) ? HitData.HitDefaults.SerializeFlags.PushForce : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_backstabBonus.Equals(1f)) ? HitData.HitDefaults.SerializeFlags.BackstabBonus : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_staggerMultiplier.Equals(1f)) ? HitData.HitDefaults.SerializeFlags.StaggerMultiplier : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((this.m_attacker != ZDOID.None) ? HitData.HitDefaults.SerializeFlags.Attacker : HitData.HitDefaults.SerializeFlags.None);
		serializeFlags |= ((!this.m_skillRaiseAmount.Equals(1f)) ? HitData.HitDefaults.SerializeFlags.SkillRaiseAmount : HitData.HitDefaults.SerializeFlags.None);
		pkg.Write((ushort)serializeFlags);
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.Damage) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_damage);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageBlunt) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_blunt);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageSlash) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_slash);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamagePierce) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_pierce);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageChop) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_chop);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamagePickaxe) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_pickaxe);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageFire) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_fire);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageFrost) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_frost);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageLightning) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_lightning);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamagePoison) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_poison);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageSpirit) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_damage.m_spirit);
		}
		pkg.Write(this.m_toolTier);
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.PushForce) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_pushForce);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.BackstabBonus) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_backstabBonus);
		}
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.StaggerMultiplier) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_staggerMultiplier);
		}
		byte b = 0;
		if (this.m_dodgeable)
		{
			b |= 1;
		}
		if (this.m_blockable)
		{
			b |= 2;
		}
		if (this.m_ranged)
		{
			b |= 4;
		}
		if (this.m_ignorePVP)
		{
			b |= 8;
		}
		pkg.Write(b);
		pkg.Write(this.m_point);
		pkg.Write(this.m_dir);
		pkg.Write(this.m_statusEffectHash);
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.Attacker) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_attacker);
		}
		pkg.Write((short)this.m_skill);
		if ((serializeFlags & HitData.HitDefaults.SerializeFlags.SkillRaiseAmount) != HitData.HitDefaults.SerializeFlags.None)
		{
			pkg.Write(this.m_skillRaiseAmount);
		}
		pkg.Write((char)this.m_weakSpot);
		pkg.Write(this.m_skillLevel);
		pkg.Write(this.m_itemLevel);
	}

	// Token: 0x060016EA RID: 5866 RVA: 0x000979B8 File Offset: 0x00095BB8
	public void Deserialize(ref ZPackage pkg)
	{
		HitData.HitDefaults.SerializeFlags serializeFlags = (HitData.HitDefaults.SerializeFlags)pkg.ReadUShort();
		this.m_damage.m_damage = (((serializeFlags & HitData.HitDefaults.SerializeFlags.Damage) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_blunt = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageBlunt) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_slash = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageSlash) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_pierce = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamagePierce) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_chop = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageChop) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_pickaxe = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamagePickaxe) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_fire = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageFire) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_frost = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageFrost) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_lightning = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageLightning) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_poison = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamagePoison) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_damage.m_spirit = (((serializeFlags & HitData.HitDefaults.SerializeFlags.DamageSpirit) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_toolTier = pkg.ReadShort();
		this.m_pushForce = (((serializeFlags & HitData.HitDefaults.SerializeFlags.PushForce) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 0f);
		this.m_backstabBonus = (((serializeFlags & HitData.HitDefaults.SerializeFlags.BackstabBonus) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 1f);
		this.m_staggerMultiplier = (((serializeFlags & HitData.HitDefaults.SerializeFlags.StaggerMultiplier) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 1f);
		byte b = pkg.ReadByte();
		this.m_dodgeable = ((b & 1) > 0);
		this.m_blockable = ((b & 2) > 0);
		this.m_ranged = ((b & 4) > 0);
		this.m_ignorePVP = ((b & 8) > 0);
		this.m_point = pkg.ReadVector3();
		this.m_dir = pkg.ReadVector3();
		this.m_statusEffectHash = pkg.ReadInt();
		this.m_attacker = (((serializeFlags & HitData.HitDefaults.SerializeFlags.Attacker) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadZDOID() : HitData.HitDefaults.s_attackerDefault);
		this.m_skill = (Skills.SkillType)pkg.ReadShort();
		this.m_skillRaiseAmount = (((serializeFlags & HitData.HitDefaults.SerializeFlags.SkillRaiseAmount) != HitData.HitDefaults.SerializeFlags.None) ? pkg.ReadSingle() : 1f);
		this.m_weakSpot = (short)pkg.ReadChar();
		this.m_skillLevel = pkg.ReadSingle();
		this.m_itemLevel = pkg.ReadShort();
	}

	// Token: 0x060016EB RID: 5867 RVA: 0x00097C5E File Offset: 0x00095E5E
	public float GetTotalPhysicalDamage()
	{
		return this.m_damage.GetTotalPhysicalDamage();
	}

	// Token: 0x060016EC RID: 5868 RVA: 0x00097C6B File Offset: 0x00095E6B
	public float GetTotalElementalDamage()
	{
		return this.m_damage.GetTotalElementalDamage();
	}

	// Token: 0x060016ED RID: 5869 RVA: 0x00097C78 File Offset: 0x00095E78
	public float GetTotalDamage()
	{
		return this.m_damage.GetTotalDamage();
	}

	// Token: 0x060016EE RID: 5870 RVA: 0x00097C88 File Offset: 0x00095E88
	private float ApplyModifier(float baseDamage, HitData.DamageModifier mod, ref float normalDmg, ref float resistantDmg, ref float weakDmg, ref float immuneDmg)
	{
		if (mod == HitData.DamageModifier.Ignore)
		{
			return 0f;
		}
		float num = baseDamage;
		switch (mod)
		{
		case HitData.DamageModifier.Resistant:
			num /= 2f;
			resistantDmg += baseDamage;
			return num;
		case HitData.DamageModifier.Weak:
			num *= 1.5f;
			weakDmg += baseDamage;
			return num;
		case HitData.DamageModifier.Immune:
			num = 0f;
			immuneDmg += baseDamage;
			return num;
		case HitData.DamageModifier.VeryResistant:
			num /= 4f;
			resistantDmg += baseDamage;
			return num;
		case HitData.DamageModifier.VeryWeak:
			num *= 2f;
			weakDmg += baseDamage;
			return num;
		}
		normalDmg += baseDamage;
		return num;
	}

	// Token: 0x060016EF RID: 5871 RVA: 0x00097D24 File Offset: 0x00095F24
	public void ApplyResistance(HitData.DamageModifiers modifiers, out HitData.DamageModifier significantModifier)
	{
		float damage = this.m_damage.m_damage;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		this.m_damage.m_blunt = this.ApplyModifier(this.m_damage.m_blunt, modifiers.m_blunt, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_slash = this.ApplyModifier(this.m_damage.m_slash, modifiers.m_slash, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_pierce = this.ApplyModifier(this.m_damage.m_pierce, modifiers.m_pierce, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_chop = this.ApplyModifier(this.m_damage.m_chop, modifiers.m_chop, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_pickaxe = this.ApplyModifier(this.m_damage.m_pickaxe, modifiers.m_pickaxe, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_fire = this.ApplyModifier(this.m_damage.m_fire, modifiers.m_fire, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_frost = this.ApplyModifier(this.m_damage.m_frost, modifiers.m_frost, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_lightning = this.ApplyModifier(this.m_damage.m_lightning, modifiers.m_lightning, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_poison = this.ApplyModifier(this.m_damage.m_poison, modifiers.m_poison, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_spirit = this.ApplyModifier(this.m_damage.m_spirit, modifiers.m_spirit, ref damage, ref num, ref num2, ref num3);
		significantModifier = HitData.DamageModifier.Immune;
		if (num3 >= num && num3 >= num2 && num3 >= damage)
		{
			significantModifier = HitData.DamageModifier.Immune;
		}
		if (damage >= num && damage >= num2 && damage >= num3)
		{
			significantModifier = HitData.DamageModifier.Normal;
		}
		if (num >= num2 && num >= num3 && num >= damage)
		{
			significantModifier = HitData.DamageModifier.Resistant;
		}
		if (num2 >= num && num2 >= num3 && num2 >= damage)
		{
			significantModifier = HitData.DamageModifier.Weak;
		}
	}

	// Token: 0x060016F0 RID: 5872 RVA: 0x00097F32 File Offset: 0x00096132
	public void ApplyArmor(float ac)
	{
		this.m_damage.ApplyArmor(ac);
	}

	// Token: 0x060016F1 RID: 5873 RVA: 0x00097F40 File Offset: 0x00096140
	public void ApplyModifier(float multiplier)
	{
		this.m_damage.m_blunt = this.m_damage.m_blunt * multiplier;
		this.m_damage.m_slash = this.m_damage.m_slash * multiplier;
		this.m_damage.m_pierce = this.m_damage.m_pierce * multiplier;
		this.m_damage.m_chop = this.m_damage.m_chop * multiplier;
		this.m_damage.m_pickaxe = this.m_damage.m_pickaxe * multiplier;
		this.m_damage.m_fire = this.m_damage.m_fire * multiplier;
		this.m_damage.m_frost = this.m_damage.m_frost * multiplier;
		this.m_damage.m_lightning = this.m_damage.m_lightning * multiplier;
		this.m_damage.m_poison = this.m_damage.m_poison * multiplier;
		this.m_damage.m_spirit = this.m_damage.m_spirit * multiplier;
	}

	// Token: 0x060016F2 RID: 5874 RVA: 0x00097FED File Offset: 0x000961ED
	public float GetTotalBlockableDamage()
	{
		return this.m_damage.GetTotalBlockableDamage();
	}

	// Token: 0x060016F3 RID: 5875 RVA: 0x00097FFC File Offset: 0x000961FC
	public void BlockDamage(float damage)
	{
		float totalBlockableDamage = this.GetTotalBlockableDamage();
		float num = Mathf.Max(0f, totalBlockableDamage - damage);
		if (totalBlockableDamage <= 0f)
		{
			return;
		}
		float num2 = num / totalBlockableDamage;
		this.m_damage.m_blunt = this.m_damage.m_blunt * num2;
		this.m_damage.m_slash = this.m_damage.m_slash * num2;
		this.m_damage.m_pierce = this.m_damage.m_pierce * num2;
		this.m_damage.m_fire = this.m_damage.m_fire * num2;
		this.m_damage.m_frost = this.m_damage.m_frost * num2;
		this.m_damage.m_lightning = this.m_damage.m_lightning * num2;
		this.m_damage.m_poison = this.m_damage.m_poison * num2;
		this.m_damage.m_spirit = this.m_damage.m_spirit * num2;
	}

	// Token: 0x060016F4 RID: 5876 RVA: 0x000980AB File Offset: 0x000962AB
	public bool HaveAttacker()
	{
		return !this.m_attacker.IsNone();
	}

	// Token: 0x060016F5 RID: 5877 RVA: 0x000980BC File Offset: 0x000962BC
	public Character GetAttacker()
	{
		if (this.m_attacker.IsNone())
		{
			return null;
		}
		if (ZNetScene.instance == null)
		{
			return null;
		}
		GameObject gameObject = ZNetScene.instance.FindInstance(this.m_attacker);
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<Character>();
	}

	// Token: 0x060016F6 RID: 5878 RVA: 0x00098109 File Offset: 0x00096309
	public void SetAttacker(Character attacker)
	{
		if (attacker)
		{
			this.m_attacker = attacker.GetZDOID();
			return;
		}
		this.m_attacker = ZDOID.None;
	}

	// Token: 0x04001813 RID: 6163
	public HitData.DamageTypes m_damage;

	// Token: 0x04001814 RID: 6164
	public bool m_dodgeable;

	// Token: 0x04001815 RID: 6165
	public bool m_blockable;

	// Token: 0x04001816 RID: 6166
	public bool m_ranged;

	// Token: 0x04001817 RID: 6167
	public bool m_ignorePVP;

	// Token: 0x04001818 RID: 6168
	public short m_toolTier;

	// Token: 0x04001819 RID: 6169
	public float m_pushForce;

	// Token: 0x0400181A RID: 6170
	public float m_backstabBonus = 1f;

	// Token: 0x0400181B RID: 6171
	public float m_staggerMultiplier = 1f;

	// Token: 0x0400181C RID: 6172
	public Vector3 m_point = Vector3.zero;

	// Token: 0x0400181D RID: 6173
	public Vector3 m_dir = Vector3.zero;

	// Token: 0x0400181E RID: 6174
	public int m_statusEffectHash;

	// Token: 0x0400181F RID: 6175
	public ZDOID m_attacker = ZDOID.None;

	// Token: 0x04001820 RID: 6176
	public Skills.SkillType m_skill;

	// Token: 0x04001821 RID: 6177
	public float m_skillRaiseAmount = 1f;

	// Token: 0x04001822 RID: 6178
	public float m_skillLevel;

	// Token: 0x04001823 RID: 6179
	public short m_itemLevel;

	// Token: 0x04001824 RID: 6180
	public short m_weakSpot = -1;

	// Token: 0x04001825 RID: 6181
	public Collider m_hitCollider;

	// Token: 0x02000245 RID: 581
	private struct HitDefaults
	{
		// Token: 0x04001826 RID: 6182
		public const float c_DamageDefault = 0f;

		// Token: 0x04001827 RID: 6183
		public const float c_PushForceDefault = 0f;

		// Token: 0x04001828 RID: 6184
		public const float c_BackstabBonusDefault = 1f;

		// Token: 0x04001829 RID: 6185
		public const float c_StaggerMultiplierDefault = 1f;

		// Token: 0x0400182A RID: 6186
		public static readonly ZDOID s_attackerDefault = ZDOID.None;

		// Token: 0x0400182B RID: 6187
		public const float c_SkillRaiseAmountDefault = 1f;

		// Token: 0x02000246 RID: 582
		[Flags]
		public enum SerializeFlags
		{
			// Token: 0x0400182D RID: 6189
			None = 0,
			// Token: 0x0400182E RID: 6190
			Damage = 1,
			// Token: 0x0400182F RID: 6191
			DamageBlunt = 2,
			// Token: 0x04001830 RID: 6192
			DamageSlash = 4,
			// Token: 0x04001831 RID: 6193
			DamagePierce = 8,
			// Token: 0x04001832 RID: 6194
			DamageChop = 16,
			// Token: 0x04001833 RID: 6195
			DamagePickaxe = 32,
			// Token: 0x04001834 RID: 6196
			DamageFire = 64,
			// Token: 0x04001835 RID: 6197
			DamageFrost = 128,
			// Token: 0x04001836 RID: 6198
			DamageLightning = 256,
			// Token: 0x04001837 RID: 6199
			DamagePoison = 512,
			// Token: 0x04001838 RID: 6200
			DamageSpirit = 1024,
			// Token: 0x04001839 RID: 6201
			PushForce = 2048,
			// Token: 0x0400183A RID: 6202
			BackstabBonus = 4096,
			// Token: 0x0400183B RID: 6203
			StaggerMultiplier = 8192,
			// Token: 0x0400183C RID: 6204
			Attacker = 16384,
			// Token: 0x0400183D RID: 6205
			SkillRaiseAmount = 32768
		}
	}

	// Token: 0x02000247 RID: 583
	[Flags]
	public enum DamageType
	{
		// Token: 0x0400183F RID: 6207
		Blunt = 1,
		// Token: 0x04001840 RID: 6208
		Slash = 2,
		// Token: 0x04001841 RID: 6209
		Pierce = 4,
		// Token: 0x04001842 RID: 6210
		Chop = 8,
		// Token: 0x04001843 RID: 6211
		Pickaxe = 16,
		// Token: 0x04001844 RID: 6212
		Fire = 32,
		// Token: 0x04001845 RID: 6213
		Frost = 64,
		// Token: 0x04001846 RID: 6214
		Lightning = 128,
		// Token: 0x04001847 RID: 6215
		Poison = 256,
		// Token: 0x04001848 RID: 6216
		Spirit = 512,
		// Token: 0x04001849 RID: 6217
		Physical = 31,
		// Token: 0x0400184A RID: 6218
		Elemental = 224
	}

	// Token: 0x02000248 RID: 584
	public enum DamageModifier
	{
		// Token: 0x0400184C RID: 6220
		Normal,
		// Token: 0x0400184D RID: 6221
		Resistant,
		// Token: 0x0400184E RID: 6222
		Weak,
		// Token: 0x0400184F RID: 6223
		Immune,
		// Token: 0x04001850 RID: 6224
		Ignore,
		// Token: 0x04001851 RID: 6225
		VeryResistant,
		// Token: 0x04001852 RID: 6226
		VeryWeak
	}

	// Token: 0x02000249 RID: 585
	[Serializable]
	public struct DamageModPair
	{
		// Token: 0x04001853 RID: 6227
		public HitData.DamageType m_type;

		// Token: 0x04001854 RID: 6228
		public HitData.DamageModifier m_modifier;
	}

	// Token: 0x0200024A RID: 586
	[Serializable]
	public struct DamageModifiers
	{
		// Token: 0x060016F8 RID: 5880 RVA: 0x00098137 File Offset: 0x00096337
		public HitData.DamageModifiers Clone()
		{
			return (HitData.DamageModifiers)base.MemberwiseClone();
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x00098150 File Offset: 0x00096350
		public void Apply(List<HitData.DamageModPair> modifiers)
		{
			foreach (HitData.DamageModPair damageModPair in modifiers)
			{
				HitData.DamageType type = damageModPair.m_type;
				if (type <= HitData.DamageType.Fire)
				{
					if (type <= HitData.DamageType.Chop)
					{
						switch (type)
						{
						case HitData.DamageType.Blunt:
							this.ApplyIfBetter(ref this.m_blunt, damageModPair.m_modifier);
							break;
						case HitData.DamageType.Slash:
							this.ApplyIfBetter(ref this.m_slash, damageModPair.m_modifier);
							break;
						case HitData.DamageType.Blunt | HitData.DamageType.Slash:
							break;
						case HitData.DamageType.Pierce:
							this.ApplyIfBetter(ref this.m_pierce, damageModPair.m_modifier);
							break;
						default:
							if (type == HitData.DamageType.Chop)
							{
								this.ApplyIfBetter(ref this.m_chop, damageModPair.m_modifier);
							}
							break;
						}
					}
					else if (type != HitData.DamageType.Pickaxe)
					{
						if (type == HitData.DamageType.Fire)
						{
							this.ApplyIfBetter(ref this.m_fire, damageModPair.m_modifier);
						}
					}
					else
					{
						this.ApplyIfBetter(ref this.m_pickaxe, damageModPair.m_modifier);
					}
				}
				else if (type <= HitData.DamageType.Lightning)
				{
					if (type != HitData.DamageType.Frost)
					{
						if (type == HitData.DamageType.Lightning)
						{
							this.ApplyIfBetter(ref this.m_lightning, damageModPair.m_modifier);
						}
					}
					else
					{
						this.ApplyIfBetter(ref this.m_frost, damageModPair.m_modifier);
					}
				}
				else if (type != HitData.DamageType.Poison)
				{
					if (type == HitData.DamageType.Spirit)
					{
						this.ApplyIfBetter(ref this.m_spirit, damageModPair.m_modifier);
					}
				}
				else
				{
					this.ApplyIfBetter(ref this.m_poison, damageModPair.m_modifier);
				}
			}
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x000982FC File Offset: 0x000964FC
		public HitData.DamageModifier GetModifier(HitData.DamageType type)
		{
			if (type <= HitData.DamageType.Fire)
			{
				if (type <= HitData.DamageType.Chop)
				{
					switch (type)
					{
					case HitData.DamageType.Blunt:
						return this.m_blunt;
					case HitData.DamageType.Slash:
						return this.m_slash;
					case HitData.DamageType.Blunt | HitData.DamageType.Slash:
						break;
					case HitData.DamageType.Pierce:
						return this.m_pierce;
					default:
						if (type == HitData.DamageType.Chop)
						{
							return this.m_chop;
						}
						break;
					}
				}
				else
				{
					if (type == HitData.DamageType.Pickaxe)
					{
						return this.m_pickaxe;
					}
					if (type == HitData.DamageType.Fire)
					{
						return this.m_fire;
					}
				}
			}
			else if (type <= HitData.DamageType.Lightning)
			{
				if (type == HitData.DamageType.Frost)
				{
					return this.m_frost;
				}
				if (type == HitData.DamageType.Lightning)
				{
					return this.m_lightning;
				}
			}
			else
			{
				if (type == HitData.DamageType.Poison)
				{
					return this.m_poison;
				}
				if (type == HitData.DamageType.Spirit)
				{
					return this.m_spirit;
				}
			}
			return HitData.DamageModifier.Normal;
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x000983AC File Offset: 0x000965AC
		private void ApplyIfBetter(ref HitData.DamageModifier original, HitData.DamageModifier mod)
		{
			if (this.ShouldOverride(original, mod))
			{
				original = mod;
			}
		}

		// Token: 0x060016FC RID: 5884 RVA: 0x000983BC File Offset: 0x000965BC
		private bool ShouldOverride(HitData.DamageModifier a, HitData.DamageModifier b)
		{
			return a != HitData.DamageModifier.Ignore && (b == HitData.DamageModifier.Immune || ((a != HitData.DamageModifier.VeryResistant || b != HitData.DamageModifier.Resistant) && (a != HitData.DamageModifier.VeryWeak || b != HitData.DamageModifier.Weak) && ((a != HitData.DamageModifier.Resistant && a != HitData.DamageModifier.VeryResistant && a != HitData.DamageModifier.Immune) || (b != HitData.DamageModifier.Weak && b != HitData.DamageModifier.VeryWeak))));
		}

		// Token: 0x060016FD RID: 5885 RVA: 0x000983F8 File Offset: 0x000965F8
		public void Print()
		{
			ZLog.Log("m_blunt " + this.m_blunt.ToString());
			ZLog.Log("m_slash " + this.m_slash.ToString());
			ZLog.Log("m_pierce " + this.m_pierce.ToString());
			ZLog.Log("m_chop " + this.m_chop.ToString());
			ZLog.Log("m_pickaxe " + this.m_pickaxe.ToString());
			ZLog.Log("m_fire " + this.m_fire.ToString());
			ZLog.Log("m_frost " + this.m_frost.ToString());
			ZLog.Log("m_lightning " + this.m_lightning.ToString());
			ZLog.Log("m_poison " + this.m_poison.ToString());
			ZLog.Log("m_spirit " + this.m_spirit.ToString());
		}

		// Token: 0x04001855 RID: 6229
		public HitData.DamageModifier m_blunt;

		// Token: 0x04001856 RID: 6230
		public HitData.DamageModifier m_slash;

		// Token: 0x04001857 RID: 6231
		public HitData.DamageModifier m_pierce;

		// Token: 0x04001858 RID: 6232
		public HitData.DamageModifier m_chop;

		// Token: 0x04001859 RID: 6233
		public HitData.DamageModifier m_pickaxe;

		// Token: 0x0400185A RID: 6234
		public HitData.DamageModifier m_fire;

		// Token: 0x0400185B RID: 6235
		public HitData.DamageModifier m_frost;

		// Token: 0x0400185C RID: 6236
		public HitData.DamageModifier m_lightning;

		// Token: 0x0400185D RID: 6237
		public HitData.DamageModifier m_poison;

		// Token: 0x0400185E RID: 6238
		public HitData.DamageModifier m_spirit;
	}

	// Token: 0x0200024B RID: 587
	[Serializable]
	public struct DamageTypes
	{
		// Token: 0x060016FE RID: 5886 RVA: 0x00098548 File Offset: 0x00096748
		public bool HaveDamage()
		{
			return this.m_damage > 0f || this.m_blunt > 0f || this.m_slash > 0f || this.m_pierce > 0f || this.m_chop > 0f || this.m_pickaxe > 0f || this.m_fire > 0f || this.m_frost > 0f || this.m_lightning > 0f || this.m_poison > 0f || this.m_spirit > 0f;
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x000985E9 File Offset: 0x000967E9
		public float GetTotalPhysicalDamage()
		{
			return this.m_blunt + this.m_slash + this.m_pierce;
		}

		// Token: 0x06001700 RID: 5888 RVA: 0x000985FF File Offset: 0x000967FF
		public float GetTotalStaggerDamage()
		{
			return this.m_blunt + this.m_slash + this.m_pierce + this.m_lightning;
		}

		// Token: 0x06001701 RID: 5889 RVA: 0x0009861C File Offset: 0x0009681C
		public float GetTotalBlockableDamage()
		{
			return this.m_blunt + this.m_slash + this.m_pierce + this.m_fire + this.m_frost + this.m_lightning + this.m_poison + this.m_spirit;
		}

		// Token: 0x06001702 RID: 5890 RVA: 0x00098655 File Offset: 0x00096855
		public float GetTotalElementalDamage()
		{
			return this.m_fire + this.m_frost + this.m_lightning;
		}

		// Token: 0x06001703 RID: 5891 RVA: 0x0009866C File Offset: 0x0009686C
		public float GetTotalDamage()
		{
			return this.m_damage + this.m_blunt + this.m_slash + this.m_pierce + this.m_chop + this.m_pickaxe + this.m_fire + this.m_frost + this.m_lightning + this.m_poison + this.m_spirit;
		}

		// Token: 0x06001704 RID: 5892 RVA: 0x000986C5 File Offset: 0x000968C5
		public HitData.DamageTypes Clone()
		{
			return (HitData.DamageTypes)base.MemberwiseClone();
		}

		// Token: 0x06001705 RID: 5893 RVA: 0x000986DC File Offset: 0x000968DC
		public void Add(HitData.DamageTypes other, int multiplier = 1)
		{
			this.m_damage += other.m_damage * (float)multiplier;
			this.m_blunt += other.m_blunt * (float)multiplier;
			this.m_slash += other.m_slash * (float)multiplier;
			this.m_pierce += other.m_pierce * (float)multiplier;
			this.m_chop += other.m_chop * (float)multiplier;
			this.m_pickaxe += other.m_pickaxe * (float)multiplier;
			this.m_fire += other.m_fire * (float)multiplier;
			this.m_frost += other.m_frost * (float)multiplier;
			this.m_lightning += other.m_lightning * (float)multiplier;
			this.m_poison += other.m_poison * (float)multiplier;
			this.m_spirit += other.m_spirit * (float)multiplier;
		}

		// Token: 0x06001706 RID: 5894 RVA: 0x000987DC File Offset: 0x000969DC
		public void Modify(float multiplier)
		{
			this.m_damage *= multiplier;
			this.m_blunt *= multiplier;
			this.m_slash *= multiplier;
			this.m_pierce *= multiplier;
			this.m_chop *= multiplier;
			this.m_pickaxe *= multiplier;
			this.m_fire *= multiplier;
			this.m_frost *= multiplier;
			this.m_lightning *= multiplier;
			this.m_poison *= multiplier;
			this.m_spirit *= multiplier;
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x00098884 File Offset: 0x00096A84
		public static float ApplyArmor(float dmg, float ac)
		{
			float result = Mathf.Clamp01(dmg / (ac * 4f)) * dmg;
			if (ac < dmg / 2f)
			{
				result = dmg - ac;
			}
			return result;
		}

		// Token: 0x06001708 RID: 5896 RVA: 0x000988B4 File Offset: 0x00096AB4
		public void ApplyArmor(float ac)
		{
			if (ac <= 0f)
			{
				return;
			}
			float num = this.m_blunt + this.m_slash + this.m_pierce + this.m_fire + this.m_frost + this.m_lightning + this.m_poison + this.m_spirit;
			if (num <= 0f)
			{
				return;
			}
			float num2 = HitData.DamageTypes.ApplyArmor(num, ac) / num;
			this.m_blunt *= num2;
			this.m_slash *= num2;
			this.m_pierce *= num2;
			this.m_fire *= num2;
			this.m_frost *= num2;
			this.m_lightning *= num2;
			this.m_poison *= num2;
			this.m_spirit *= num2;
		}

		// Token: 0x06001709 RID: 5897 RVA: 0x00098988 File Offset: 0x00096B88
		private string DamageRange(float damage, float minFactor, float maxFactor)
		{
			int num = Mathf.RoundToInt(damage * minFactor);
			int num2 = Mathf.RoundToInt(damage * maxFactor);
			return string.Concat(new string[]
			{
				"<color=orange>",
				Mathf.RoundToInt(damage).ToString(),
				"</color> <color=yellow>(",
				num.ToString(),
				"-",
				num2.ToString(),
				") </color>"
			});
		}

		// Token: 0x0600170A RID: 5898 RVA: 0x000989F8 File Offset: 0x00096BF8
		public string GetTooltipString(Skills.SkillType skillType = Skills.SkillType.None)
		{
			if (Player.m_localPlayer == null)
			{
				return "";
			}
			float minFactor;
			float maxFactor;
			Player.m_localPlayer.GetSkills().GetRandomSkillRange(out minFactor, out maxFactor, skillType);
			string text = "";
			if (this.m_damage != 0f)
			{
				text = text + "\n$inventory_damage: " + this.DamageRange(this.m_damage, minFactor, maxFactor);
			}
			if (this.m_blunt != 0f)
			{
				text = text + "\n$inventory_blunt: " + this.DamageRange(this.m_blunt, minFactor, maxFactor);
			}
			if (this.m_slash != 0f)
			{
				text = text + "\n$inventory_slash: " + this.DamageRange(this.m_slash, minFactor, maxFactor);
			}
			if (this.m_pierce != 0f)
			{
				text = text + "\n$inventory_pierce: " + this.DamageRange(this.m_pierce, minFactor, maxFactor);
			}
			if (this.m_fire != 0f)
			{
				text = text + "\n$inventory_fire: " + this.DamageRange(this.m_fire, minFactor, maxFactor);
			}
			if (this.m_frost != 0f)
			{
				text = text + "\n$inventory_frost: " + this.DamageRange(this.m_frost, minFactor, maxFactor);
			}
			if (this.m_lightning != 0f)
			{
				text = text + "\n$inventory_lightning: " + this.DamageRange(this.m_lightning, minFactor, maxFactor);
			}
			if (this.m_poison != 0f)
			{
				text = text + "\n$inventory_poison: " + this.DamageRange(this.m_poison, minFactor, maxFactor);
			}
			if (this.m_spirit != 0f)
			{
				text = text + "\n$inventory_spirit: " + this.DamageRange(this.m_spirit, minFactor, maxFactor);
			}
			return text;
		}

		// Token: 0x0600170B RID: 5899 RVA: 0x00098B94 File Offset: 0x00096D94
		public string GetTooltipString()
		{
			string text = "";
			if (this.m_damage != 0f)
			{
				text = text + "\n$inventory_damage: <color=yellow>" + this.m_damage.ToString() + "</color>";
			}
			if (this.m_blunt != 0f)
			{
				text = text + "\n$inventory_blunt: <color=yellow>" + this.m_blunt.ToString() + "</color>";
			}
			if (this.m_slash != 0f)
			{
				text = text + "\n$inventory_slash: <color=yellow>" + this.m_slash.ToString() + "</color>";
			}
			if (this.m_pierce != 0f)
			{
				text = text + "\n$inventory_pierce: <color=yellow>" + this.m_pierce.ToString() + "</color>";
			}
			if (this.m_fire != 0f)
			{
				text = text + "\n$inventory_fire: <color=yellow>" + this.m_fire.ToString() + "</color>";
			}
			if (this.m_frost != 0f)
			{
				text = text + "\n$inventory_frost: <color=yellow>" + this.m_frost.ToString() + "</color>";
			}
			if (this.m_lightning != 0f)
			{
				text = text + "\n$inventory_lightning: <color=yellow>" + this.m_frost.ToString() + "</color>";
			}
			if (this.m_poison != 0f)
			{
				text = text + "\n$inventory_poison: <color=yellow>" + this.m_poison.ToString() + "</color>";
			}
			if (this.m_spirit != 0f)
			{
				text = text + "\n$inventory_spirit: <color=yellow>" + this.m_spirit.ToString() + "</color>";
			}
			return text;
		}

		// Token: 0x0400185F RID: 6239
		public float m_damage;

		// Token: 0x04001860 RID: 6240
		public float m_blunt;

		// Token: 0x04001861 RID: 6241
		public float m_slash;

		// Token: 0x04001862 RID: 6242
		public float m_pierce;

		// Token: 0x04001863 RID: 6243
		public float m_chop;

		// Token: 0x04001864 RID: 6244
		public float m_pickaxe;

		// Token: 0x04001865 RID: 6245
		public float m_fire;

		// Token: 0x04001866 RID: 6246
		public float m_frost;

		// Token: 0x04001867 RID: 6247
		public float m_lightning;

		// Token: 0x04001868 RID: 6248
		public float m_poison;

		// Token: 0x04001869 RID: 6249
		public float m_spirit;
	}
}
