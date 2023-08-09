using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000040 RID: 64
public class Aoe : MonoBehaviour, IProjectile
{
	// Token: 0x060003BA RID: 954 RVA: 0x0001C780 File Offset: 0x0001A980
	private void Awake()
	{
		this.m_nview = base.GetComponentInParent<ZNetView>();
		this.m_rayMask = 0;
		if (this.m_hitCharacters)
		{
			this.m_rayMask |= LayerMask.GetMask(new string[]
			{
				"character",
				"character_net",
				"character_ghost"
			});
		}
		if (this.m_hitProps)
		{
			this.m_rayMask |= LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"hitbox",
				"character_noenv",
				"vehicle"
			});
		}
		if (!string.IsNullOrEmpty(this.m_statusEffect))
		{
			this.m_statusEffectHash = this.m_statusEffect.GetStableHashCode();
		}
		if (!string.IsNullOrEmpty(this.m_statusEffectIfBoss))
		{
			this.m_statusEffectIfBossHash = this.m_statusEffectIfBoss.GetStableHashCode();
		}
		if (!string.IsNullOrEmpty(this.m_statusEffectIfPlayer))
		{
			this.m_statusEffectIfPlayerHash = this.m_statusEffectIfPlayer.GetStableHashCode();
		}
	}

	// Token: 0x060003BB RID: 955 RVA: 0x0001C88A File Offset: 0x0001AA8A
	private HitData.DamageTypes GetDamage()
	{
		return this.GetDamage(this.m_level);
	}

	// Token: 0x060003BC RID: 956 RVA: 0x0001C898 File Offset: 0x0001AA98
	private HitData.DamageTypes GetDamage(int itemQuality)
	{
		if (itemQuality <= 1)
		{
			return this.m_damage;
		}
		HitData.DamageTypes damage = this.m_damage;
		damage.Add(this.m_damagePerLevel, itemQuality - 1);
		return damage;
	}

	// Token: 0x060003BD RID: 957 RVA: 0x0001C8C8 File Offset: 0x0001AAC8
	public string GetTooltipString(int itemQuality)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append("AOE");
		stringBuilder.Append(this.GetDamage(itemQuality).GetTooltipString());
		stringBuilder.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", this.m_attackForce);
		stringBuilder.AppendFormat("\n$item_backstab: <color=orange>{0}x</color>", this.m_backstabBonus);
		return stringBuilder.ToString();
	}

	// Token: 0x060003BE RID: 958 RVA: 0x0001C934 File Offset: 0x0001AB34
	private void Start()
	{
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		if (!this.m_useTriggers && this.m_hitInterval <= 0f)
		{
			this.CheckHits();
		}
	}

	// Token: 0x060003BF RID: 959 RVA: 0x0001C988 File Offset: 0x0001AB88
	private void FixedUpdate()
	{
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		if (this.m_hitInterval > 0f)
		{
			this.m_hitTimer -= Time.fixedDeltaTime;
			if (this.m_hitTimer <= 0f)
			{
				this.m_hitTimer = this.m_hitInterval;
				if (this.m_useTriggers)
				{
					this.m_hitList.Clear();
				}
				else
				{
					this.CheckHits();
				}
			}
		}
		if (this.m_owner != null && this.m_attachToCaster)
		{
			base.transform.position = this.m_owner.transform.TransformPoint(this.m_offset);
			base.transform.rotation = this.m_owner.transform.rotation * this.m_localRot;
		}
		if (this.m_ttl > 0f)
		{
			this.m_ttl -= Time.fixedDeltaTime;
			if (this.m_ttl <= 0f && ZNetScene.instance)
			{
				ZNetScene.instance.Destroy(base.gameObject);
			}
		}
	}

	// Token: 0x060003C0 RID: 960 RVA: 0x0001CAB8 File Offset: 0x0001ACB8
	private void CheckHits()
	{
		this.m_hitList.Clear();
		foreach (Collider collider in (this.m_useCollider != null) ? Physics.OverlapBox(base.transform.position + this.m_useCollider.center, this.m_useCollider.size / 2f, base.transform.rotation, this.m_rayMask) : Physics.OverlapSphere(base.transform.position, this.m_radius, this.m_rayMask))
		{
			this.OnHit(collider, collider.transform.position);
		}
	}

	// Token: 0x060003C1 RID: 961 RVA: 0x0001CB68 File Offset: 0x0001AD68
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		this.m_owner = owner;
		if (item != null)
		{
			this.m_level = item.m_quality;
		}
		if (this.m_attachToCaster && owner != null)
		{
			this.m_offset = owner.transform.InverseTransformPoint(base.transform.position);
			this.m_localRot = Quaternion.Inverse(owner.transform.rotation) * base.transform.rotation;
		}
		if (hitData != null && this.m_useAttackSettings)
		{
			this.m_damage = hitData.m_damage;
			this.m_blockable = hitData.m_blockable;
			this.m_dodgeable = hitData.m_dodgeable;
			this.m_attackForce = hitData.m_pushForce;
			this.m_backstabBonus = hitData.m_backstabBonus;
			if (this.m_statusEffectHash != hitData.m_statusEffectHash)
			{
				this.m_statusEffectHash = hitData.m_statusEffectHash;
				this.m_statusEffect = "<changed>";
			}
			this.m_toolTier = (int)hitData.m_toolTier;
			this.m_skill = hitData.m_skill;
		}
	}

	// Token: 0x060003C2 RID: 962 RVA: 0x0001CC74 File Offset: 0x0001AE74
	private void OnCollisionEnter(Collision collision)
	{
		if (!this.m_triggerEnterOnly)
		{
			return;
		}
		if (!this.m_useTriggers)
		{
			ZLog.LogWarning("AOE got OnTriggerStay but trigger damage is disabled in " + base.gameObject.name);
			return;
		}
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		this.OnHit(collision.collider, collision.collider.transform.position);
	}

	// Token: 0x060003C3 RID: 963 RVA: 0x0001CCF4 File Offset: 0x0001AEF4
	private void OnCollisionStay(Collision collision)
	{
		if (this.m_triggerEnterOnly)
		{
			return;
		}
		if (!this.m_useTriggers)
		{
			ZLog.LogWarning("AOE got OnTriggerStay but trigger damage is disabled in " + base.gameObject.name);
			return;
		}
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		this.OnHit(collision.collider, collision.collider.transform.position);
	}

	// Token: 0x060003C4 RID: 964 RVA: 0x0001CD74 File Offset: 0x0001AF74
	private void OnTriggerEnter(Collider collider)
	{
		if (!this.m_triggerEnterOnly)
		{
			return;
		}
		if (!this.m_useTriggers)
		{
			ZLog.LogWarning("AOE got OnTriggerStay but trigger damage is disabled in " + base.gameObject.name);
			return;
		}
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		this.OnHit(collider, collider.transform.position);
	}

	// Token: 0x060003C5 RID: 965 RVA: 0x0001CDEC File Offset: 0x0001AFEC
	private void OnTriggerStay(Collider collider)
	{
		if (this.m_triggerEnterOnly)
		{
			return;
		}
		if (!this.m_useTriggers)
		{
			ZLog.LogWarning("AOE got OnTriggerStay but trigger damage is disabled in " + base.gameObject.name);
			return;
		}
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		this.OnHit(collider, collider.transform.position);
	}

	// Token: 0x060003C6 RID: 966 RVA: 0x0001CE64 File Offset: 0x0001B064
	private bool OnHit(Collider collider, Vector3 hitPoint)
	{
		GameObject gameObject = Projectile.FindHitObject(collider);
		if (this.m_hitList.Contains(gameObject))
		{
			return false;
		}
		this.m_hitList.Add(gameObject);
		float num = 1f;
		if (this.m_owner && this.m_owner.IsPlayer() && this.m_skill != Skills.SkillType.None)
		{
			num = this.m_owner.GetRandomSkillFactor(this.m_skill);
		}
		bool result = false;
		bool flag = false;
		IDestructible component = gameObject.GetComponent<IDestructible>();
		if (component != null)
		{
			if (!this.m_hitParent && base.gameObject.transform.parent != null && gameObject == base.gameObject.transform.parent.gameObject)
			{
				return false;
			}
			Character character = component as Character;
			if (character)
			{
				if (this.m_nview == null && !character.IsOwner())
				{
					return false;
				}
				if (this.m_owner != null)
				{
					if (!this.m_hitOwner && character == this.m_owner)
					{
						return false;
					}
					if (!this.m_hitSame && character.m_name == this.m_owner.m_name)
					{
						return false;
					}
					bool flag2 = BaseAI.IsEnemy(this.m_owner, character) || (character.GetBaseAI() && character.GetBaseAI().IsAggravatable() && this.m_owner.IsPlayer());
					if (!this.m_hitFriendly && !flag2)
					{
						return false;
					}
					if (!this.m_hitEnemy && flag2)
					{
						return false;
					}
				}
				if (!this.m_hitCharacters)
				{
					return false;
				}
				if (this.m_dodgeable && character.IsDodgeInvincible())
				{
					return false;
				}
				flag = true;
			}
			else if (!this.m_hitProps)
			{
				return false;
			}
			Vector3 dir = this.m_attackForceForward ? base.transform.forward : (hitPoint - base.transform.position).normalized;
			HitData hitData = new HitData();
			hitData.m_hitCollider = collider;
			hitData.m_damage = this.GetDamage();
			hitData.m_pushForce = this.m_attackForce * num;
			hitData.m_backstabBonus = this.m_backstabBonus;
			hitData.m_point = hitPoint;
			hitData.m_dir = dir;
			hitData.m_statusEffectHash = this.GetStatusEffect(character);
			HitData hitData2 = hitData;
			Character owner = this.m_owner;
			hitData2.m_skillLevel = ((owner != null) ? owner.GetSkillLevel(this.m_skill) : 0f);
			hitData.m_itemLevel = (short)this.m_level;
			hitData.m_dodgeable = this.m_dodgeable;
			hitData.m_blockable = this.m_blockable;
			hitData.m_ranged = true;
			hitData.m_ignorePVP = (this.m_owner == character || this.m_ignorePVP);
			hitData.m_toolTier = (short)this.m_toolTier;
			hitData.SetAttacker(this.m_owner);
			hitData.m_damage.Modify(num);
			component.Damage(hitData);
			if (this.m_damageSelf > 0f)
			{
				IDestructible componentInParent = base.GetComponentInParent<IDestructible>();
				if (componentInParent != null)
				{
					HitData hitData3 = new HitData();
					hitData3.m_damage.m_damage = this.m_damageSelf;
					hitData3.m_point = hitPoint;
					hitData3.m_blockable = false;
					hitData3.m_dodgeable = false;
					componentInParent.Damage(hitData3);
				}
			}
			result = true;
		}
		this.m_hitEffects.Create(hitPoint, Quaternion.identity, null, 1f, -1);
		if (!this.m_gaveSkill && this.m_owner && this.m_skill > Skills.SkillType.None && flag && this.m_canRaiseSkill)
		{
			this.m_owner.RaiseSkill(this.m_skill, 1f);
			this.m_gaveSkill = true;
		}
		return result;
	}

	// Token: 0x060003C7 RID: 967 RVA: 0x0001D20B File Offset: 0x0001B40B
	private int GetStatusEffect(Character character)
	{
		if (character)
		{
			if (character.IsBoss() && this.m_statusEffectIfBossHash != 0)
			{
				return this.m_statusEffectIfBossHash;
			}
			if (character.IsPlayer() && this.m_statusEffectIfPlayerHash != 0)
			{
				return this.m_statusEffectIfPlayerHash;
			}
		}
		return this.m_statusEffectHash;
	}

	// Token: 0x060003C8 RID: 968 RVA: 0x0001D249 File Offset: 0x0001B449
	private void OnDrawGizmos()
	{
		bool useTriggers = this.m_useTriggers;
	}

	// Token: 0x040003BD RID: 957
	[Header("Attack (overridden by item )")]
	public bool m_useAttackSettings = true;

	// Token: 0x040003BE RID: 958
	public HitData.DamageTypes m_damage;

	// Token: 0x040003BF RID: 959
	public bool m_dodgeable;

	// Token: 0x040003C0 RID: 960
	public bool m_blockable;

	// Token: 0x040003C1 RID: 961
	public int m_toolTier;

	// Token: 0x040003C2 RID: 962
	public float m_attackForce;

	// Token: 0x040003C3 RID: 963
	public float m_backstabBonus = 4f;

	// Token: 0x040003C4 RID: 964
	public string m_statusEffect = "";

	// Token: 0x040003C5 RID: 965
	public string m_statusEffectIfBoss = "";

	// Token: 0x040003C6 RID: 966
	public string m_statusEffectIfPlayer = "";

	// Token: 0x040003C7 RID: 967
	private int m_statusEffectHash;

	// Token: 0x040003C8 RID: 968
	private int m_statusEffectIfBossHash;

	// Token: 0x040003C9 RID: 969
	private int m_statusEffectIfPlayerHash;

	// Token: 0x040003CA RID: 970
	[Header("Attack (other)")]
	public HitData.DamageTypes m_damagePerLevel;

	// Token: 0x040003CB RID: 971
	public bool m_attackForceForward;

	// Token: 0x040003CC RID: 972
	[Header("Damage self")]
	public float m_damageSelf;

	// Token: 0x040003CD RID: 973
	[Header("Ignore targets")]
	public bool m_hitOwner;

	// Token: 0x040003CE RID: 974
	public bool m_hitParent = true;

	// Token: 0x040003CF RID: 975
	public bool m_hitSame;

	// Token: 0x040003D0 RID: 976
	public bool m_hitFriendly = true;

	// Token: 0x040003D1 RID: 977
	public bool m_hitEnemy = true;

	// Token: 0x040003D2 RID: 978
	public bool m_hitCharacters = true;

	// Token: 0x040003D3 RID: 979
	public bool m_hitProps = true;

	// Token: 0x040003D4 RID: 980
	public bool m_ignorePVP;

	// Token: 0x040003D5 RID: 981
	[Header("Other")]
	public Skills.SkillType m_skill;

	// Token: 0x040003D6 RID: 982
	public bool m_canRaiseSkill = true;

	// Token: 0x040003D7 RID: 983
	public bool m_useTriggers;

	// Token: 0x040003D8 RID: 984
	public bool m_triggerEnterOnly;

	// Token: 0x040003D9 RID: 985
	public BoxCollider m_useCollider;

	// Token: 0x040003DA RID: 986
	public float m_radius = 4f;

	// Token: 0x040003DB RID: 987
	public float m_ttl = 4f;

	// Token: 0x040003DC RID: 988
	public float m_hitInterval = 1f;

	// Token: 0x040003DD RID: 989
	public EffectList m_hitEffects = new EffectList();

	// Token: 0x040003DE RID: 990
	public bool m_attachToCaster;

	// Token: 0x040003DF RID: 991
	private ZNetView m_nview;

	// Token: 0x040003E0 RID: 992
	private Character m_owner;

	// Token: 0x040003E1 RID: 993
	private readonly List<GameObject> m_hitList = new List<GameObject>();

	// Token: 0x040003E2 RID: 994
	private float m_hitTimer;

	// Token: 0x040003E3 RID: 995
	private Vector3 m_offset = Vector3.zero;

	// Token: 0x040003E4 RID: 996
	private Quaternion m_localRot = Quaternion.identity;

	// Token: 0x040003E5 RID: 997
	private int m_level;

	// Token: 0x040003E6 RID: 998
	private int m_rayMask;

	// Token: 0x040003E7 RID: 999
	private bool m_gaveSkill;
}
