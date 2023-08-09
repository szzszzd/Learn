using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000030 RID: 48
public class Skills : MonoBehaviour
{
	// Token: 0x0600031B RID: 795 RVA: 0x00017FC8 File Offset: 0x000161C8
	public void Awake()
	{
		this.m_player = base.GetComponent<Player>();
	}

	// Token: 0x0600031C RID: 796 RVA: 0x00017FD8 File Offset: 0x000161D8
	public void Save(ZPackage pkg)
	{
		pkg.Write(2);
		pkg.Write(this.m_skillData.Count);
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			pkg.Write((int)keyValuePair.Value.m_info.m_skill);
			pkg.Write(keyValuePair.Value.m_level);
			pkg.Write(keyValuePair.Value.m_accumulator);
		}
	}

	// Token: 0x0600031D RID: 797 RVA: 0x00018078 File Offset: 0x00016278
	public void Load(ZPackage pkg)
	{
		int num = pkg.ReadInt();
		this.m_skillData.Clear();
		int num2 = pkg.ReadInt();
		for (int i = 0; i < num2; i++)
		{
			Skills.SkillType skillType = (Skills.SkillType)pkg.ReadInt();
			float level = pkg.ReadSingle();
			float accumulator = (num >= 2) ? pkg.ReadSingle() : 0f;
			if (Skills.IsSkillValid(skillType))
			{
				Skills.Skill skill = this.GetSkill(skillType);
				skill.m_level = level;
				skill.m_accumulator = accumulator;
			}
		}
	}

	// Token: 0x0600031E RID: 798 RVA: 0x000180EA File Offset: 0x000162EA
	private static bool IsSkillValid(Skills.SkillType type)
	{
		return Enum.IsDefined(typeof(Skills.SkillType), type);
	}

	// Token: 0x0600031F RID: 799 RVA: 0x00018101 File Offset: 0x00016301
	public float GetSkillFactor(Skills.SkillType skillType)
	{
		if (skillType == Skills.SkillType.None)
		{
			return 0f;
		}
		return Mathf.Clamp01(this.GetSkillLevel(skillType) / 100f);
	}

	// Token: 0x06000320 RID: 800 RVA: 0x00018120 File Offset: 0x00016320
	public float GetSkillLevel(Skills.SkillType skillType)
	{
		if (skillType == Skills.SkillType.None)
		{
			return 0f;
		}
		float level = this.GetSkill(skillType).m_level;
		this.m_player.GetSEMan().ModifySkillLevel(skillType, ref level);
		return level;
	}

	// Token: 0x06000321 RID: 801 RVA: 0x00018158 File Offset: 0x00016358
	public void GetRandomSkillRange(out float min, out float max, Skills.SkillType skillType)
	{
		float skillFactor = this.GetSkillFactor(skillType);
		float num = Mathf.Lerp(0.4f, 1f, skillFactor);
		min = Mathf.Clamp01(num - 0.15f);
		max = Mathf.Clamp01(num + 0.15f);
	}

	// Token: 0x06000322 RID: 802 RVA: 0x0001819C File Offset: 0x0001639C
	public float GetRandomSkillFactor(Skills.SkillType skillType)
	{
		float skillFactor = this.GetSkillFactor(skillType);
		float num = Mathf.Lerp(0.4f, 1f, skillFactor);
		float a = Mathf.Clamp01(num - 0.15f);
		float b = Mathf.Clamp01(num + 0.15f);
		return Mathf.Lerp(a, b, UnityEngine.Random.value);
	}

	// Token: 0x06000323 RID: 803 RVA: 0x000181E8 File Offset: 0x000163E8
	public void CheatRaiseSkill(string name, float value, bool showMessage = true)
	{
		if (name.ToLower() == "all")
		{
			foreach (Skills.SkillType skillType in Skills.s_allSkills)
			{
				if (skillType != Skills.SkillType.All)
				{
					this.CheatRaiseSkill(skillType.ToString(), value, false);
				}
			}
			if (showMessage)
			{
				this.m_player.Message(MessageHud.MessageType.TopLeft, string.Format("All skills increased by {0}", value), 0, null);
				global::Console.instance.Print(string.Format("All skills increased by {0}", value));
			}
			return;
		}
		Skills.SkillType[] array = Skills.s_allSkills;
		int i = 0;
		while (i < array.Length)
		{
			Skills.SkillType skillType2 = array[i];
			if (skillType2.ToString().ToLower() == name.ToLower() && skillType2 != Skills.SkillType.All && skillType2 != Skills.SkillType.None)
			{
				Skills.Skill skill = this.GetSkill(skillType2);
				skill.m_level += value;
				skill.m_level = Mathf.Clamp(skill.m_level, 0f, 100f);
				if (this.m_useSkillCap)
				{
					this.RebalanceSkills(skillType2);
				}
				if (skill.m_info == null)
				{
					return;
				}
				if (showMessage)
				{
					this.m_player.Message(MessageHud.MessageType.TopLeft, "Skill increased " + skill.m_info.m_skill.ToString() + ": " + ((int)skill.m_level).ToString(), 0, skill.m_info.m_icon);
					global::Console.instance.Print("Skill " + skillType2.ToString() + " = " + skill.m_level.ToString());
				}
				return;
			}
			else
			{
				i++;
			}
		}
		global::Console.instance.Print("Skill not found " + name);
	}

	// Token: 0x06000324 RID: 804 RVA: 0x000183B8 File Offset: 0x000165B8
	public void CheatResetSkill(string name)
	{
		foreach (Skills.SkillType skillType in Skills.s_allSkills)
		{
			if (skillType.ToString().ToLower() == name.ToLower())
			{
				this.ResetSkill(skillType);
				global::Console.instance.Print("Skill " + skillType.ToString() + " reset");
				return;
			}
		}
		global::Console.instance.Print("Skill not found " + name);
	}

	// Token: 0x06000325 RID: 805 RVA: 0x0001843F File Offset: 0x0001663F
	public void ResetSkill(Skills.SkillType skillType)
	{
		this.m_skillData.Remove(skillType);
	}

	// Token: 0x06000326 RID: 806 RVA: 0x00018450 File Offset: 0x00016650
	public void RaiseSkill(Skills.SkillType skillType, float factor = 1f)
	{
		if (skillType == Skills.SkillType.None)
		{
			return;
		}
		Skills.Skill skill = this.GetSkill(skillType);
		float level = skill.m_level;
		if (skill.Raise(factor))
		{
			if (this.m_useSkillCap)
			{
				this.RebalanceSkills(skillType);
			}
			this.m_player.OnSkillLevelup(skillType, skill.m_level);
			MessageHud.MessageType type = ((int)level == 0) ? MessageHud.MessageType.Center : MessageHud.MessageType.TopLeft;
			this.m_player.Message(type, "$msg_skillup $skill_" + skill.m_info.m_skill.ToString().ToLower() + ": " + ((int)skill.m_level).ToString(), 0, skill.m_info.m_icon);
			Gogan.LogEvent("Game", "Levelup", skillType.ToString(), (long)((int)skill.m_level));
		}
	}

	// Token: 0x06000327 RID: 807 RVA: 0x0001851C File Offset: 0x0001671C
	private void RebalanceSkills(Skills.SkillType skillType)
	{
		if (this.GetTotalSkill() < this.m_totalSkillCap)
		{
			return;
		}
		float level = this.GetSkill(skillType).m_level;
		float num = this.m_totalSkillCap - level;
		float num2 = 0f;
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			if (keyValuePair.Key != skillType)
			{
				num2 += keyValuePair.Value.m_level;
			}
		}
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair2 in this.m_skillData)
		{
			if (keyValuePair2.Key != skillType)
			{
				keyValuePair2.Value.m_level = keyValuePair2.Value.m_level / num2 * num;
			}
		}
	}

	// Token: 0x06000328 RID: 808 RVA: 0x00018610 File Offset: 0x00016810
	public void Clear()
	{
		this.m_skillData.Clear();
	}

	// Token: 0x06000329 RID: 809 RVA: 0x0001861D File Offset: 0x0001681D
	public void OnDeath()
	{
		this.LowerAllSkills(this.m_DeathLowerFactor);
	}

	// Token: 0x0600032A RID: 810 RVA: 0x0001862C File Offset: 0x0001682C
	public void LowerAllSkills(float factor)
	{
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			float num = keyValuePair.Value.m_level * factor;
			keyValuePair.Value.m_level -= num;
			keyValuePair.Value.m_accumulator = 0f;
		}
		this.m_player.Message(MessageHud.MessageType.TopLeft, "$msg_skills_lowered", 0, null);
	}

	// Token: 0x0600032B RID: 811 RVA: 0x000186C0 File Offset: 0x000168C0
	private Skills.Skill GetSkill(Skills.SkillType skillType)
	{
		Skills.Skill skill;
		if (this.m_skillData.TryGetValue(skillType, out skill))
		{
			return skill;
		}
		skill = new Skills.Skill(this.GetSkillDef(skillType));
		this.m_skillData.Add(skillType, skill);
		return skill;
	}

	// Token: 0x0600032C RID: 812 RVA: 0x000186FC File Offset: 0x000168FC
	private Skills.SkillDef GetSkillDef(Skills.SkillType type)
	{
		foreach (Skills.SkillDef skillDef in this.m_skills)
		{
			if (skillDef.m_skill == type)
			{
				return skillDef;
			}
		}
		return null;
	}

	// Token: 0x0600032D RID: 813 RVA: 0x00018758 File Offset: 0x00016958
	public List<Skills.Skill> GetSkillList()
	{
		List<Skills.Skill> list = new List<Skills.Skill>();
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			list.Add(keyValuePair.Value);
		}
		return list;
	}

	// Token: 0x0600032E RID: 814 RVA: 0x000187B8 File Offset: 0x000169B8
	public float GetTotalSkill()
	{
		float num = 0f;
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			num += keyValuePair.Value.m_level;
		}
		return num;
	}

	// Token: 0x0600032F RID: 815 RVA: 0x0001881C File Offset: 0x00016A1C
	public float GetTotalSkillCap()
	{
		return this.m_totalSkillCap;
	}

	// Token: 0x040002F1 RID: 753
	private const int c_SaveFileDataVersion = 2;

	// Token: 0x040002F2 RID: 754
	private const float c_RandomSkillRange = 0.15f;

	// Token: 0x040002F3 RID: 755
	private const float c_RandomSkillMin = 0.4f;

	// Token: 0x040002F4 RID: 756
	public const float c_MaxSkillLevel = 100f;

	// Token: 0x040002F5 RID: 757
	public float m_DeathLowerFactor = 0.25f;

	// Token: 0x040002F6 RID: 758
	public bool m_useSkillCap;

	// Token: 0x040002F7 RID: 759
	public float m_totalSkillCap = 600f;

	// Token: 0x040002F8 RID: 760
	public List<Skills.SkillDef> m_skills = new List<Skills.SkillDef>();

	// Token: 0x040002F9 RID: 761
	private readonly Dictionary<Skills.SkillType, Skills.Skill> m_skillData = new Dictionary<Skills.SkillType, Skills.Skill>();

	// Token: 0x040002FA RID: 762
	private Player m_player;

	// Token: 0x040002FB RID: 763
	private static readonly Skills.SkillType[] s_allSkills = (Skills.SkillType[])Enum.GetValues(typeof(Skills.SkillType));

	// Token: 0x02000031 RID: 49
	public enum SkillType
	{
		// Token: 0x040002FD RID: 765
		None,
		// Token: 0x040002FE RID: 766
		Swords,
		// Token: 0x040002FF RID: 767
		Knives,
		// Token: 0x04000300 RID: 768
		Clubs,
		// Token: 0x04000301 RID: 769
		Polearms,
		// Token: 0x04000302 RID: 770
		Spears,
		// Token: 0x04000303 RID: 771
		Blocking,
		// Token: 0x04000304 RID: 772
		Axes,
		// Token: 0x04000305 RID: 773
		Bows,
		// Token: 0x04000306 RID: 774
		ElementalMagic,
		// Token: 0x04000307 RID: 775
		BloodMagic,
		// Token: 0x04000308 RID: 776
		Unarmed,
		// Token: 0x04000309 RID: 777
		Pickaxes,
		// Token: 0x0400030A RID: 778
		WoodCutting,
		// Token: 0x0400030B RID: 779
		Crossbows,
		// Token: 0x0400030C RID: 780
		Jump = 100,
		// Token: 0x0400030D RID: 781
		Sneak,
		// Token: 0x0400030E RID: 782
		Run,
		// Token: 0x0400030F RID: 783
		Swim,
		// Token: 0x04000310 RID: 784
		Fishing,
		// Token: 0x04000311 RID: 785
		Ride = 110,
		// Token: 0x04000312 RID: 786
		All = 999
	}

	// Token: 0x02000032 RID: 50
	[Serializable]
	public class SkillDef
	{
		// Token: 0x04000313 RID: 787
		public Skills.SkillType m_skill = Skills.SkillType.Swords;

		// Token: 0x04000314 RID: 788
		public Sprite m_icon;

		// Token: 0x04000315 RID: 789
		public string m_description = "";

		// Token: 0x04000316 RID: 790
		public float m_increseStep = 1f;
	}

	// Token: 0x02000033 RID: 51
	public class Skill
	{
		// Token: 0x06000333 RID: 819 RVA: 0x00018898 File Offset: 0x00016A98
		public Skill(Skills.SkillDef info)
		{
			this.m_info = info;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x000188A8 File Offset: 0x00016AA8
		public bool Raise(float factor)
		{
			if (this.m_level >= 100f)
			{
				return false;
			}
			float num = this.m_info.m_increseStep * factor;
			this.m_accumulator += num;
			float nextLevelRequirement = this.GetNextLevelRequirement();
			if (this.m_accumulator >= nextLevelRequirement)
			{
				this.m_level += 1f;
				this.m_level = Mathf.Clamp(this.m_level, 0f, 100f);
				this.m_accumulator = 0f;
				return true;
			}
			return false;
		}

		// Token: 0x06000335 RID: 821 RVA: 0x0001892B File Offset: 0x00016B2B
		private float GetNextLevelRequirement()
		{
			return Mathf.Pow(this.m_level + 1f, 1.5f) * 0.5f + 0.5f;
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00018950 File Offset: 0x00016B50
		public float GetLevelPercentage()
		{
			if (this.m_level >= 100f)
			{
				return 0f;
			}
			float nextLevelRequirement = this.GetNextLevelRequirement();
			return Mathf.Clamp01(this.m_accumulator / nextLevelRequirement);
		}

		// Token: 0x04000317 RID: 791
		public Skills.SkillDef m_info;

		// Token: 0x04000318 RID: 792
		public float m_level;

		// Token: 0x04000319 RID: 793
		public float m_accumulator;
	}
}
