using System;
using UnityEngine;

// Token: 0x02000066 RID: 102
public class StatusEffect : ScriptableObject
{
	// Token: 0x06000525 RID: 1317 RVA: 0x0002984E File Offset: 0x00027A4E
	public StatusEffect Clone()
	{
		return base.MemberwiseClone() as StatusEffect;
	}

	// Token: 0x06000526 RID: 1318 RVA: 0x0000290F File Offset: 0x00000B0F
	public virtual bool CanAdd(Character character)
	{
		return true;
	}

	// Token: 0x06000527 RID: 1319 RVA: 0x0002985B File Offset: 0x00027A5B
	public virtual void Setup(Character character)
	{
		this.m_character = character;
		if (!string.IsNullOrEmpty(this.m_startMessage))
		{
			this.m_character.Message(this.m_startMessageType, this.m_startMessage, 0, null);
		}
		this.TriggerStartEffects();
	}

	// Token: 0x06000528 RID: 1320 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void SetAttacker(Character attacker)
	{
	}

	// Token: 0x06000529 RID: 1321 RVA: 0x00029890 File Offset: 0x00027A90
	public virtual string GetTooltipString()
	{
		return this.m_tooltip;
	}

	// Token: 0x0600052A RID: 1322 RVA: 0x00029898 File Offset: 0x00027A98
	protected virtual void OnApplicationQuit()
	{
		this.m_startEffectInstances = null;
	}

	// Token: 0x0600052B RID: 1323 RVA: 0x000298A1 File Offset: 0x00027AA1
	public virtual void OnDestroy()
	{
		this.RemoveStartEffects();
	}

	// Token: 0x0600052C RID: 1324 RVA: 0x000298AC File Offset: 0x00027AAC
	protected void TriggerStartEffects()
	{
		this.RemoveStartEffects();
		float radius = this.m_character.GetRadius();
		int variant = -1;
		Player player = this.m_character as Player;
		if (player)
		{
			variant = player.GetPlayerModel();
		}
		this.m_startEffectInstances = this.m_startEffects.Create(this.m_character.GetCenterPoint(), this.m_character.transform.rotation, this.m_character.transform, radius * 2f, variant);
	}

	// Token: 0x0600052D RID: 1325 RVA: 0x00029928 File Offset: 0x00027B28
	private void RemoveStartEffects()
	{
		if (this.m_startEffectInstances != null && ZNetScene.instance != null)
		{
			foreach (GameObject gameObject in this.m_startEffectInstances)
			{
				if (gameObject)
				{
					ZNetView component = gameObject.GetComponent<ZNetView>();
					if (component.IsValid())
					{
						component.ClaimOwnership();
						component.Destroy();
					}
				}
			}
			this.m_startEffectInstances = null;
		}
	}

	// Token: 0x0600052E RID: 1326 RVA: 0x00029990 File Offset: 0x00027B90
	public virtual void Stop()
	{
		this.RemoveStartEffects();
		this.m_stopEffects.Create(this.m_character.transform.position, this.m_character.transform.rotation, null, 1f, -1);
		if (!string.IsNullOrEmpty(this.m_stopMessage))
		{
			this.m_character.Message(this.m_stopMessageType, this.m_stopMessage, 0, null);
		}
	}

	// Token: 0x0600052F RID: 1327 RVA: 0x000299FC File Offset: 0x00027BFC
	public virtual void UpdateStatusEffect(float dt)
	{
		this.m_time += dt;
		if (this.m_repeatInterval > 0f && !string.IsNullOrEmpty(this.m_repeatMessage))
		{
			this.m_msgTimer += dt;
			if (this.m_msgTimer > this.m_repeatInterval)
			{
				this.m_msgTimer = 0f;
				this.m_character.Message(this.m_repeatMessageType, this.m_repeatMessage, 0, null);
			}
		}
	}

	// Token: 0x06000530 RID: 1328 RVA: 0x00029A71 File Offset: 0x00027C71
	public virtual bool IsDone()
	{
		return this.m_ttl > 0f && this.m_time > this.m_ttl;
	}

	// Token: 0x06000531 RID: 1329 RVA: 0x00029A91 File Offset: 0x00027C91
	public virtual void ResetTime()
	{
		this.m_time = 0f;
	}

	// Token: 0x06000532 RID: 1330 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void SetLevel(int itemLevel, float skillLevel)
	{
	}

	// Token: 0x06000533 RID: 1331 RVA: 0x00029A9E File Offset: 0x00027C9E
	public float GetDuration()
	{
		return this.m_time;
	}

	// Token: 0x06000534 RID: 1332 RVA: 0x00029AA6 File Offset: 0x00027CA6
	public float GetRemaningTime()
	{
		return this.m_ttl - this.m_time;
	}

	// Token: 0x06000535 RID: 1333 RVA: 0x00029AB5 File Offset: 0x00027CB5
	public virtual string GetIconText()
	{
		if (this.m_ttl > 0f)
		{
			return StatusEffect.GetTimeString(this.m_ttl - this.GetDuration(), false, false);
		}
		return "";
	}

	// Token: 0x06000536 RID: 1334 RVA: 0x00029AE0 File Offset: 0x00027CE0
	public static string GetTimeString(float time, bool sufix = false, bool alwaysShowMinutes = false)
	{
		if (time <= 0f)
		{
			return "";
		}
		int num = Mathf.CeilToInt(time);
		int num2 = (int)((float)num / 60f);
		int num3 = Mathf.Max(0, num - num2 * 60);
		if (sufix)
		{
			if (num2 > 0 || alwaysShowMinutes)
			{
				return num2.ToString() + "m:" + num3.ToString("00") + "s";
			}
			return num3.ToString() + "s";
		}
		else
		{
			if (num2 > 0 || alwaysShowMinutes)
			{
				return num2.ToString() + ":" + num3.ToString("00");
			}
			return num3.ToString();
		}
	}

	// Token: 0x06000537 RID: 1335 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
	{
	}

	// Token: 0x06000538 RID: 1336 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyHealthRegen(ref float regenMultiplier)
	{
	}

	// Token: 0x06000539 RID: 1337 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyStaminaRegen(ref float staminaRegen)
	{
	}

	// Token: 0x0600053A RID: 1338 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyEitrRegen(ref float eitrRegen)
	{
	}

	// Token: 0x0600053B RID: 1339 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
	{
	}

	// Token: 0x0600053C RID: 1340 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
	{
	}

	// Token: 0x0600053D RID: 1341 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifySkillLevel(Skills.SkillType skill, ref float level)
	{
	}

	// Token: 0x0600053E RID: 1342 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifySpeed(float baseSpeed, ref float speed)
	{
	}

	// Token: 0x0600053F RID: 1343 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyJump(Vector3 baseJump, ref Vector3 jump)
	{
	}

	// Token: 0x06000540 RID: 1344 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyWalkVelocity(ref Vector3 vel)
	{
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyFallDamage(float baseDamage, ref float damage)
	{
	}

	// Token: 0x06000542 RID: 1346 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyNoise(float baseNoise, ref float noise)
	{
	}

	// Token: 0x06000543 RID: 1347 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyStealth(float baseStealth, ref float stealth)
	{
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyMaxCarryWeight(float baseLimit, ref float limit)
	{
	}

	// Token: 0x06000545 RID: 1349 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyRunStaminaDrain(float baseDrain, ref float drain)
	{
	}

	// Token: 0x06000546 RID: 1350 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
	{
	}

	// Token: 0x06000547 RID: 1351 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void OnDamaged(HitData hit, Character attacker)
	{
	}

	// Token: 0x06000548 RID: 1352 RVA: 0x00029B89 File Offset: 0x00027D89
	public bool HaveAttribute(StatusEffect.StatusAttribute value)
	{
		return (this.m_attributes & value) > StatusEffect.StatusAttribute.None;
	}

	// Token: 0x06000549 RID: 1353 RVA: 0x00029B96 File Offset: 0x00027D96
	public int NameHash()
	{
		if (this.m_nameHash == 0)
		{
			this.m_nameHash = base.name.GetStableHashCode();
		}
		return this.m_nameHash;
	}

	// Token: 0x04000614 RID: 1556
	[Header("__Common__")]
	public string m_name = "";

	// Token: 0x04000615 RID: 1557
	public string m_category = "";

	// Token: 0x04000616 RID: 1558
	public Sprite m_icon;

	// Token: 0x04000617 RID: 1559
	public bool m_flashIcon;

	// Token: 0x04000618 RID: 1560
	public bool m_cooldownIcon;

	// Token: 0x04000619 RID: 1561
	[TextArea]
	public string m_tooltip = "";

	// Token: 0x0400061A RID: 1562
	[BitMask(typeof(StatusEffect.StatusAttribute))]
	public StatusEffect.StatusAttribute m_attributes;

	// Token: 0x0400061B RID: 1563
	public MessageHud.MessageType m_startMessageType = MessageHud.MessageType.TopLeft;

	// Token: 0x0400061C RID: 1564
	public string m_startMessage = "";

	// Token: 0x0400061D RID: 1565
	public MessageHud.MessageType m_stopMessageType = MessageHud.MessageType.TopLeft;

	// Token: 0x0400061E RID: 1566
	public string m_stopMessage = "";

	// Token: 0x0400061F RID: 1567
	public MessageHud.MessageType m_repeatMessageType = MessageHud.MessageType.TopLeft;

	// Token: 0x04000620 RID: 1568
	public string m_repeatMessage = "";

	// Token: 0x04000621 RID: 1569
	public float m_repeatInterval;

	// Token: 0x04000622 RID: 1570
	public float m_ttl;

	// Token: 0x04000623 RID: 1571
	public EffectList m_startEffects = new EffectList();

	// Token: 0x04000624 RID: 1572
	public EffectList m_stopEffects = new EffectList();

	// Token: 0x04000625 RID: 1573
	[Header("__Guardian power__")]
	public float m_cooldown;

	// Token: 0x04000626 RID: 1574
	public string m_activationAnimation = "gpower";

	// Token: 0x04000627 RID: 1575
	[NonSerialized]
	public bool m_isNew = true;

	// Token: 0x04000628 RID: 1576
	private float m_msgTimer;

	// Token: 0x04000629 RID: 1577
	protected Character m_character;

	// Token: 0x0400062A RID: 1578
	protected float m_time;

	// Token: 0x0400062B RID: 1579
	protected GameObject[] m_startEffectInstances;

	// Token: 0x0400062C RID: 1580
	private int m_nameHash;

	// Token: 0x02000067 RID: 103
	public enum StatusAttribute
	{
		// Token: 0x0400062E RID: 1582
		None,
		// Token: 0x0400062F RID: 1583
		ColdResistance,
		// Token: 0x04000630 RID: 1584
		DoubleImpactDamage,
		// Token: 0x04000631 RID: 1585
		SailingPower = 4
	}
}
