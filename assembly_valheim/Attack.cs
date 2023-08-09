using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000041 RID: 65
[Serializable]
public class Attack
{
	// Token: 0x060003CA RID: 970 RVA: 0x0001D311 File Offset: 0x0001B511
	public bool StartDraw(Humanoid character, ItemDrop.ItemData weapon)
	{
		if (!Attack.HaveAmmo(character, weapon))
		{
			return false;
		}
		Attack.EquipAmmoItem(character, weapon);
		return true;
	}

	// Token: 0x060003CB RID: 971 RVA: 0x0001D328 File Offset: 0x0001B528
	public bool Start(Humanoid character, Rigidbody body, ZSyncAnimation zanim, CharacterAnimEvent animEvent, VisEquipment visEquipment, ItemDrop.ItemData weapon, Attack previousAttack, float timeSinceLastAttack, float attackDrawPercentage)
	{
		if (this.m_attackAnimation == "")
		{
			return false;
		}
		this.m_character = character;
		this.m_baseAI = this.m_character.GetComponent<BaseAI>();
		this.m_body = body;
		this.m_zanim = zanim;
		this.m_animEvent = animEvent;
		this.m_visEquipment = visEquipment;
		this.m_weapon = weapon;
		this.m_attackDrawPercentage = attackDrawPercentage;
		if (Attack.m_attackMask == 0)
		{
			Attack.m_attackMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"piece_nonsolid",
				"character",
				"character_net",
				"character_ghost",
				"hitbox",
				"character_noenv",
				"vehicle"
			});
			Attack.m_attackMaskTerrain = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"piece_nonsolid",
				"terrain",
				"character",
				"character_net",
				"character_ghost",
				"hitbox",
				"character_noenv",
				"vehicle"
			});
		}
		if (this.m_requiresReload && (!this.m_character.IsWeaponLoaded() || this.m_character.InMinorAction()))
		{
			return false;
		}
		float attackStamina = this.GetAttackStamina();
		if (attackStamina > 0f && !character.HaveStamina(attackStamina + 0.1f))
		{
			if (character.IsPlayer())
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
			return false;
		}
		float attackEitr = this.GetAttackEitr();
		if (attackEitr > 0f)
		{
			if (character.GetMaxEitr() == 0f)
			{
				character.Message(MessageHud.MessageType.Center, "$hud_eitrrequired", 0, null);
				return false;
			}
			if (!character.HaveEitr(attackEitr + 0.1f))
			{
				if (character.IsPlayer())
				{
					Hud.instance.EitrBarEmptyFlash();
				}
				return false;
			}
		}
		float attackHealth = this.GetAttackHealth();
		if (attackHealth > 0f && !character.HaveHealth(attackHealth + 0.1f))
		{
			if (character.IsPlayer())
			{
				Hud.instance.FlashHealthBar();
			}
			return false;
		}
		if (!Attack.HaveAmmo(character, this.m_weapon))
		{
			return false;
		}
		Attack.EquipAmmoItem(character, this.m_weapon);
		if (this.m_attackChainLevels > 1)
		{
			if (previousAttack != null && previousAttack.m_attackAnimation == this.m_attackAnimation)
			{
				this.m_currentAttackCainLevel = previousAttack.m_nextAttackChainLevel;
			}
			if (this.m_currentAttackCainLevel >= this.m_attackChainLevels || timeSinceLastAttack > 0.2f)
			{
				this.m_currentAttackCainLevel = 0;
			}
			this.m_zanim.SetTrigger(this.m_attackAnimation + this.m_currentAttackCainLevel.ToString());
		}
		else if (this.m_attackRandomAnimations >= 2)
		{
			int num = UnityEngine.Random.Range(0, this.m_attackRandomAnimations);
			this.m_zanim.SetTrigger(this.m_attackAnimation + num.ToString());
		}
		else
		{
			this.m_zanim.SetTrigger(this.m_attackAnimation);
		}
		if (character.IsPlayer() && this.m_attackType != Attack.AttackType.None && this.m_currentAttackCainLevel == 0)
		{
			if (ZInput.IsMouseActive() || this.m_attackType == Attack.AttackType.Projectile)
			{
				character.transform.rotation = character.GetLookYaw();
				this.m_body.rotation = character.transform.rotation;
			}
			else if (ZInput.IsGamepadActive() && !character.IsBlocking() && character.GetMoveDir().magnitude > 0.3f)
			{
				character.transform.rotation = Quaternion.LookRotation(character.GetMoveDir());
				this.m_body.rotation = character.transform.rotation;
			}
		}
		weapon.m_lastAttackTime = Time.time;
		this.m_animEvent.ResetChain();
		return true;
	}

	// Token: 0x060003CC RID: 972 RVA: 0x0001D6E8 File Offset: 0x0001B8E8
	private float GetAttackStamina()
	{
		if (this.m_attackStamina <= 0f)
		{
			return 0f;
		}
		float attackStamina = this.m_attackStamina;
		float skillFactor = this.m_character.GetSkillFactor(this.m_weapon.m_shared.m_skillType);
		return attackStamina - attackStamina * 0.33f * skillFactor;
	}

	// Token: 0x060003CD RID: 973 RVA: 0x0001D734 File Offset: 0x0001B934
	private float GetAttackEitr()
	{
		if (this.m_attackEitr <= 0f)
		{
			return 0f;
		}
		float attackEitr = this.m_attackEitr;
		float skillFactor = this.m_character.GetSkillFactor(this.m_weapon.m_shared.m_skillType);
		return attackEitr - attackEitr * 0.33f * skillFactor;
	}

	// Token: 0x060003CE RID: 974 RVA: 0x0001D780 File Offset: 0x0001B980
	private float GetAttackHealth()
	{
		if (this.m_attackHealth <= 0f && this.m_attackHealthPercentage <= 0f)
		{
			return 0f;
		}
		float num = this.m_attackHealth + this.m_character.GetHealth() * this.m_attackHealthPercentage / 100f;
		float skillFactor = this.m_character.GetSkillFactor(this.m_weapon.m_shared.m_skillType);
		return num - num * 0.33f * skillFactor;
	}

	// Token: 0x060003CF RID: 975 RVA: 0x0001D7F4 File Offset: 0x0001B9F4
	public void Update(float dt)
	{
		if (this.m_attackDone)
		{
			return;
		}
		this.m_time += dt;
		bool flag = this.m_character.InAttack();
		if (flag)
		{
			if (!this.m_wasInAttack)
			{
				if (this.m_attackType != Attack.AttackType.Projectile || !this.m_perBurstResourceUsage)
				{
					this.m_character.UseStamina(this.GetAttackStamina());
					this.m_character.UseEitr(this.GetAttackEitr());
					this.m_character.UseHealth(this.GetAttackHealth());
				}
				Transform attackOrigin = this.GetAttackOrigin();
				this.m_weapon.m_shared.m_startEffect.Create(attackOrigin.position, this.m_character.transform.rotation, attackOrigin, 1f, -1);
				this.m_startEffect.Create(attackOrigin.position, this.m_character.transform.rotation, attackOrigin, 1f, -1);
				this.m_character.AddNoise(this.m_attackStartNoise);
				this.m_nextAttackChainLevel = this.m_currentAttackCainLevel + 1;
				if (this.m_nextAttackChainLevel >= this.m_attackChainLevels)
				{
					this.m_nextAttackChainLevel = 0;
				}
				this.m_wasInAttack = true;
			}
			if (this.m_isAttached)
			{
				this.UpdateAttach(dt);
			}
		}
		this.UpdateProjectile(dt);
		if ((!flag && this.m_wasInAttack) || this.m_abortAttack)
		{
			this.Stop();
		}
	}

	// Token: 0x060003D0 RID: 976 RVA: 0x0001D944 File Offset: 0x0001BB44
	public bool IsDone()
	{
		return this.m_attackDone;
	}

	// Token: 0x060003D1 RID: 977 RVA: 0x0001D94C File Offset: 0x0001BB4C
	public void Stop()
	{
		if (this.m_attackDone)
		{
			return;
		}
		if (this.m_loopingAttack)
		{
			this.m_zanim.SetTrigger("attack_abort");
		}
		if (this.m_isAttached)
		{
			this.m_zanim.SetTrigger("detach");
			this.m_isAttached = false;
			this.m_attachTarget = null;
		}
		if (this.m_wasInAttack)
		{
			if (this.m_visEquipment)
			{
				this.m_visEquipment.SetWeaponTrails(false);
			}
			this.m_wasInAttack = false;
		}
		this.m_attackDone = true;
	}

	// Token: 0x060003D2 RID: 978 RVA: 0x0001D9CF File Offset: 0x0001BBCF
	public void Abort()
	{
		this.m_abortAttack = true;
	}

	// Token: 0x060003D3 RID: 979 RVA: 0x0001D9D8 File Offset: 0x0001BBD8
	public void OnAttackTrigger()
	{
		if (!this.UseAmmo(out this.m_lastUsedAmmo))
		{
			return;
		}
		switch (this.m_attackType)
		{
		case Attack.AttackType.Horizontal:
		case Attack.AttackType.Vertical:
			this.DoMeleeAttack();
			break;
		case Attack.AttackType.Projectile:
			this.ProjectileAttackTriggered();
			break;
		case Attack.AttackType.None:
			this.DoNonAttack();
			break;
		case Attack.AttackType.Area:
			this.DoAreaAttack();
			break;
		}
		if (this.m_toggleFlying)
		{
			if (this.m_character.IsFlying())
			{
				this.m_character.Land();
			}
			else
			{
				this.m_character.TakeOff();
			}
		}
		if (this.m_recoilPushback != 0f)
		{
			this.m_character.ApplyPushback(-this.m_character.transform.forward, this.m_recoilPushback);
		}
		if (this.m_selfDamage > 0)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = (float)this.m_selfDamage;
			this.m_character.Damage(hitData);
		}
		if (this.m_consumeItem)
		{
			this.ConsumeItem();
		}
		if (this.m_requiresReload)
		{
			this.m_character.ResetLoadedWeapon();
		}
	}

	// Token: 0x060003D4 RID: 980 RVA: 0x0001DAE4 File Offset: 0x0001BCE4
	private void ConsumeItem()
	{
		if (this.m_weapon.m_shared.m_maxStackSize > 1 && this.m_weapon.m_stack > 1)
		{
			this.m_weapon.m_stack--;
			return;
		}
		this.m_character.UnequipItem(this.m_weapon, false);
		this.m_character.GetInventory().RemoveItem(this.m_weapon);
	}

	// Token: 0x060003D5 RID: 981 RVA: 0x0001DB50 File Offset: 0x0001BD50
	private static ItemDrop.ItemData FindAmmo(Humanoid character, ItemDrop.ItemData weapon)
	{
		if (string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
		{
			return null;
		}
		ItemDrop.ItemData itemData = character.GetAmmoItem();
		if (itemData != null && (!character.GetInventory().ContainsItem(itemData) || itemData.m_shared.m_ammoType != weapon.m_shared.m_ammoType))
		{
			itemData = null;
		}
		if (itemData == null)
		{
			itemData = character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType, null);
		}
		return itemData;
	}

	// Token: 0x060003D6 RID: 982 RVA: 0x0001DBC4 File Offset: 0x0001BDC4
	private static bool EquipAmmoItem(Humanoid character, ItemDrop.ItemData weapon)
	{
		Attack.FindAmmo(character, weapon);
		if (!string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
		{
			ItemDrop.ItemData ammoItem = character.GetAmmoItem();
			if (ammoItem != null && character.GetInventory().ContainsItem(ammoItem) && ammoItem.m_shared.m_ammoType == weapon.m_shared.m_ammoType)
			{
				return true;
			}
			ItemDrop.ItemData ammoItem2 = character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType, null);
			if (ammoItem2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || ammoItem2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable)
			{
				return character.EquipItem(ammoItem2, true);
			}
		}
		return true;
	}

	// Token: 0x060003D7 RID: 983 RVA: 0x0001DC64 File Offset: 0x0001BE64
	private static bool HaveAmmo(Humanoid character, ItemDrop.ItemData weapon)
	{
		if (string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
		{
			return true;
		}
		ItemDrop.ItemData itemData = character.GetAmmoItem();
		if (itemData != null && (!character.GetInventory().ContainsItem(itemData) || itemData.m_shared.m_ammoType != weapon.m_shared.m_ammoType))
		{
			itemData = null;
		}
		if (itemData == null)
		{
			itemData = character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType, null);
		}
		if (itemData == null)
		{
			character.Message(MessageHud.MessageType.Center, "$msg_outof " + weapon.m_shared.m_ammoType, 0, null);
			return false;
		}
		return itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable || character.CanConsumeItem(itemData);
	}

	// Token: 0x060003D8 RID: 984 RVA: 0x0001DD14 File Offset: 0x0001BF14
	private bool UseAmmo(out ItemDrop.ItemData ammoItem)
	{
		this.m_ammoItem = null;
		ammoItem = null;
		if (string.IsNullOrEmpty(this.m_weapon.m_shared.m_ammoType))
		{
			return true;
		}
		ammoItem = this.m_character.GetAmmoItem();
		if (ammoItem != null && (!this.m_character.GetInventory().ContainsItem(ammoItem) || ammoItem.m_shared.m_ammoType != this.m_weapon.m_shared.m_ammoType))
		{
			ammoItem = null;
		}
		if (ammoItem == null)
		{
			ammoItem = this.m_character.GetInventory().GetAmmoItem(this.m_weapon.m_shared.m_ammoType, null);
		}
		if (ammoItem == null)
		{
			this.m_character.Message(MessageHud.MessageType.Center, "$msg_outof " + this.m_weapon.m_shared.m_ammoType, 0, null);
			return false;
		}
		if (ammoItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
		{
			bool flag = this.m_character.ConsumeItem(this.m_character.GetInventory(), ammoItem);
			if (flag)
			{
				this.m_ammoItem = ammoItem;
			}
			return flag;
		}
		this.m_character.GetInventory().RemoveItem(ammoItem, 1);
		this.m_ammoItem = ammoItem;
		return true;
	}

	// Token: 0x060003D9 RID: 985 RVA: 0x0001DE38 File Offset: 0x0001C038
	private void ProjectileAttackTriggered()
	{
		Vector3 basePos;
		Vector3 forward;
		this.GetProjectileSpawnPoint(out basePos, out forward);
		this.m_weapon.m_shared.m_triggerEffect.Create(basePos, Quaternion.LookRotation(forward), null, 1f, -1);
		this.m_triggerEffect.Create(basePos, Quaternion.LookRotation(forward), null, 1f, -1);
		if (this.m_weapon.m_shared.m_useDurability && this.m_character.IsPlayer())
		{
			this.m_weapon.m_durability -= this.m_weapon.m_shared.m_useDurabilityDrain;
		}
		if (this.m_projectileBursts == 1)
		{
			this.FireProjectileBurst();
			return;
		}
		this.m_projectileAttackStarted = true;
	}

	// Token: 0x060003DA RID: 986 RVA: 0x0001DEE8 File Offset: 0x0001C0E8
	private void UpdateProjectile(float dt)
	{
		if (this.m_projectileAttackStarted && this.m_projectileBurstsFired < this.m_projectileBursts)
		{
			this.m_projectileFireTimer -= dt;
			if (this.m_projectileFireTimer <= 0f)
			{
				this.m_projectileFireTimer = this.m_burstInterval;
				this.FireProjectileBurst();
				this.m_projectileBurstsFired++;
			}
		}
	}

	// Token: 0x060003DB RID: 987 RVA: 0x0001DF46 File Offset: 0x0001C146
	private Transform GetAttackOrigin()
	{
		if (this.m_attackOriginJoint.Length > 0)
		{
			return Utils.FindChild(this.m_character.GetVisual().transform, this.m_attackOriginJoint);
		}
		return this.m_character.transform;
	}

	// Token: 0x060003DC RID: 988 RVA: 0x0001DF80 File Offset: 0x0001C180
	private void GetProjectileSpawnPoint(out Vector3 spawnPoint, out Vector3 aimDir)
	{
		Transform attackOrigin = this.GetAttackOrigin();
		Transform transform = this.m_character.transform;
		spawnPoint = attackOrigin.position + transform.up * this.m_attackHeight + transform.forward * this.m_attackRange + transform.right * this.m_attackOffset;
		aimDir = this.m_character.GetAimDir(spawnPoint);
		if (this.m_baseAI)
		{
			Character targetCreature = this.m_baseAI.GetTargetCreature();
			if (targetCreature)
			{
				Vector3 normalized = (targetCreature.GetCenterPoint() - spawnPoint).normalized;
				aimDir = Vector3.RotateTowards(this.m_character.transform.forward, normalized, 1.5707964f, 1f);
			}
		}
		if (this.m_useCharacterFacing)
		{
			Vector3 forward = Vector3.forward;
			if (this.m_useCharacterFacingYAim)
			{
				forward.y = aimDir.y;
			}
			aimDir = transform.TransformDirection(forward);
		}
	}

	// Token: 0x060003DD RID: 989 RVA: 0x0001E09C File Offset: 0x0001C29C
	private void FireProjectileBurst()
	{
		if (this.m_perBurstResourceUsage)
		{
			float attackStamina = this.GetAttackStamina();
			if (attackStamina > 0f)
			{
				if (!this.m_character.HaveStamina(attackStamina))
				{
					this.Stop();
					return;
				}
				this.m_character.UseStamina(attackStamina);
			}
			float attackEitr = this.GetAttackEitr();
			if (attackEitr > 0f)
			{
				if (!this.m_character.HaveEitr(attackEitr))
				{
					this.Stop();
					return;
				}
				this.m_character.UseEitr(attackEitr);
			}
			float attackHealth = this.GetAttackHealth();
			if (attackHealth > 0f)
			{
				if (!this.m_character.HaveHealth(attackHealth))
				{
					this.Stop();
					return;
				}
				this.m_character.UseHealth(attackHealth);
			}
		}
		ItemDrop.ItemData ammoItem = this.m_ammoItem;
		GameObject attackProjectile = this.m_attackProjectile;
		float num = this.m_projectileVel;
		float num2 = this.m_projectileVelMin;
		float num3 = this.m_projectileAccuracy;
		float num4 = this.m_projectileAccuracyMin;
		float num5 = this.m_attackHitNoise;
		if (ammoItem != null && ammoItem.m_shared.m_attack.m_attackProjectile)
		{
			attackProjectile = ammoItem.m_shared.m_attack.m_attackProjectile;
			num += ammoItem.m_shared.m_attack.m_projectileVel;
			num2 += ammoItem.m_shared.m_attack.m_projectileVelMin;
			num3 += ammoItem.m_shared.m_attack.m_projectileAccuracy;
			num4 += ammoItem.m_shared.m_attack.m_projectileAccuracyMin;
			num5 += ammoItem.m_shared.m_attack.m_attackHitNoise;
		}
		float num6 = this.m_character.GetRandomSkillFactor(this.m_weapon.m_shared.m_skillType);
		if (this.m_bowDraw)
		{
			num3 = Mathf.Lerp(num4, num3, Mathf.Pow(this.m_attackDrawPercentage, 0.5f));
			num6 *= this.m_attackDrawPercentage;
			num = Mathf.Lerp(num2, num, this.m_attackDrawPercentage);
		}
		else if (this.m_skillAccuracy)
		{
			float skillFactor = this.m_character.GetSkillFactor(this.m_weapon.m_shared.m_skillType);
			num3 = Mathf.Lerp(num4, num3, skillFactor);
		}
		Vector3 vector;
		Vector3 vector2;
		this.GetProjectileSpawnPoint(out vector, out vector2);
		if (this.m_launchAngle != 0f)
		{
			Vector3 axis = Vector3.Cross(Vector3.up, vector2);
			vector2 = Quaternion.AngleAxis(this.m_launchAngle, axis) * vector2;
		}
		if (this.m_burstEffect.HasEffects())
		{
			this.m_burstEffect.Create(vector, Quaternion.LookRotation(vector2), null, 1f, -1);
		}
		for (int i = 0; i < this.m_projectiles; i++)
		{
			if (this.m_destroyPreviousProjectile && this.m_weapon.m_lastProjectile)
			{
				ZNetScene.instance.Destroy(this.m_weapon.m_lastProjectile);
				this.m_weapon.m_lastProjectile = null;
			}
			Vector3 vector3 = vector2;
			Vector3 axis2 = Vector3.Cross(vector3, Vector3.up);
			Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-num3, num3), Vector3.up);
			vector3 = Quaternion.AngleAxis(UnityEngine.Random.Range(-num3, num3), axis2) * vector3;
			vector3 = rotation * vector3;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(attackProjectile, vector, Quaternion.LookRotation(vector3));
			HitData hitData = new HitData();
			hitData.m_toolTier = (short)this.m_weapon.m_shared.m_toolTier;
			hitData.m_pushForce = this.m_weapon.m_shared.m_attackForce * this.m_forceMultiplier;
			hitData.m_backstabBonus = this.m_weapon.m_shared.m_backstabBonus;
			hitData.m_staggerMultiplier = this.m_staggerMultiplier;
			hitData.m_damage.Add(this.m_weapon.GetDamage(), 1);
			hitData.m_statusEffectHash = (this.m_weapon.m_shared.m_attackStatusEffect ? this.m_weapon.m_shared.m_attackStatusEffect.NameHash() : 0);
			hitData.m_skillLevel = this.m_character.GetSkillLevel(this.m_weapon.m_shared.m_skillType);
			hitData.m_itemLevel = (short)this.m_weapon.m_quality;
			hitData.m_blockable = this.m_weapon.m_shared.m_blockable;
			hitData.m_dodgeable = this.m_weapon.m_shared.m_dodgeable;
			hitData.m_skill = this.m_weapon.m_shared.m_skillType;
			hitData.m_skillRaiseAmount = this.m_raiseSkillAmount;
			hitData.SetAttacker(this.m_character);
			if (ammoItem != null)
			{
				hitData.m_damage.Add(ammoItem.GetDamage(), 1);
				hitData.m_pushForce += ammoItem.m_shared.m_attackForce;
				if (ammoItem.m_shared.m_attackStatusEffect != null)
				{
					hitData.m_statusEffectHash = ammoItem.m_shared.m_attackStatusEffect.NameHash();
				}
				if (!ammoItem.m_shared.m_blockable)
				{
					hitData.m_blockable = false;
				}
				if (!ammoItem.m_shared.m_dodgeable)
				{
					hitData.m_dodgeable = false;
				}
			}
			hitData.m_pushForce *= num6;
			hitData.m_damage.Modify(this.m_damageMultiplier);
			hitData.m_damage.Modify(num6);
			hitData.m_damage.Modify(this.GetLevelDamageFactor());
			this.m_character.GetSEMan().ModifyAttack(this.m_weapon.m_shared.m_skillType, ref hitData);
			IProjectile component = gameObject.GetComponent<IProjectile>();
			if (component != null)
			{
				component.Setup(this.m_character, vector3 * num, num5, hitData, this.m_weapon, this.m_lastUsedAmmo);
			}
			this.m_weapon.m_lastProjectile = gameObject;
		}
	}

	// Token: 0x060003DE RID: 990 RVA: 0x0001E624 File Offset: 0x0001C824
	private void DoNonAttack()
	{
		if (this.m_weapon.m_shared.m_useDurability && this.m_character.IsPlayer())
		{
			this.m_weapon.m_durability -= this.m_weapon.m_shared.m_useDurabilityDrain;
		}
		Transform attackOrigin = this.GetAttackOrigin();
		this.m_weapon.m_shared.m_triggerEffect.Create(attackOrigin.position, this.m_character.transform.rotation, attackOrigin, 1f, -1);
		this.m_triggerEffect.Create(attackOrigin.position, this.m_character.transform.rotation, attackOrigin, 1f, -1);
		if (this.m_weapon.m_shared.m_consumeStatusEffect)
		{
			this.m_character.GetSEMan().AddStatusEffect(this.m_weapon.m_shared.m_consumeStatusEffect, true, 0, 0f);
		}
		this.m_character.AddNoise(this.m_attackHitNoise);
	}

	// Token: 0x060003DF RID: 991 RVA: 0x0001E725 File Offset: 0x0001C925
	private float GetLevelDamageFactor()
	{
		return 1f + (float)Mathf.Max(0, this.m_character.GetLevel() - 1) * 0.5f;
	}

	// Token: 0x060003E0 RID: 992 RVA: 0x0001E748 File Offset: 0x0001C948
	private void DoAreaAttack()
	{
		Transform transform = this.m_character.transform;
		Transform attackOrigin = this.GetAttackOrigin();
		Vector3 vector = attackOrigin.position + Vector3.up * this.m_attackHeight + transform.forward * this.m_attackRange + transform.right * this.m_attackOffset;
		this.m_weapon.m_shared.m_triggerEffect.Create(vector, transform.rotation, attackOrigin, 1f, -1);
		this.m_triggerEffect.Create(vector, transform.rotation, attackOrigin, 1f, -1);
		int num = 0;
		Vector3 vector2 = Vector3.zero;
		bool flag = false;
		float randomSkillFactor = this.m_character.GetRandomSkillFactor(this.m_weapon.m_shared.m_skillType);
		int layerMask = this.m_hitTerrain ? Attack.m_attackMaskTerrain : Attack.m_attackMask;
		Collider[] array = Physics.OverlapSphere(vector, this.m_attackRayWidth, layerMask, QueryTriggerInteraction.UseGlobal);
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		foreach (Collider collider in array)
		{
			if (!(collider.gameObject == this.m_character.gameObject))
			{
				GameObject gameObject = Projectile.FindHitObject(collider);
				if (!(gameObject == this.m_character.gameObject) && !hashSet.Contains(gameObject))
				{
					hashSet.Add(gameObject);
					Vector3 vector3;
					if (collider is MeshCollider)
					{
						vector3 = collider.ClosestPointOnBounds(vector);
					}
					else
					{
						vector3 = collider.ClosestPoint(vector);
					}
					IDestructible component = gameObject.GetComponent<IDestructible>();
					if (component != null)
					{
						Vector3 vector4 = vector3 - vector;
						vector4.y = 0f;
						Vector3 vector5 = vector3 - transform.position;
						if (Vector3.Dot(vector5, vector4) < 0f)
						{
							vector4 = vector5;
						}
						vector4.Normalize();
						HitData hitData = new HitData();
						hitData.m_toolTier = (short)this.m_weapon.m_shared.m_toolTier;
						hitData.m_statusEffectHash = (this.m_weapon.m_shared.m_attackStatusEffect ? this.m_weapon.m_shared.m_attackStatusEffect.NameHash() : 0);
						hitData.m_skillLevel = this.m_character.GetSkillLevel(this.m_weapon.m_shared.m_skillType);
						hitData.m_itemLevel = (short)this.m_weapon.m_quality;
						hitData.m_pushForce = this.m_weapon.m_shared.m_attackForce * randomSkillFactor * this.m_forceMultiplier;
						hitData.m_backstabBonus = this.m_weapon.m_shared.m_backstabBonus;
						hitData.m_staggerMultiplier = this.m_staggerMultiplier;
						hitData.m_dodgeable = this.m_weapon.m_shared.m_dodgeable;
						hitData.m_blockable = this.m_weapon.m_shared.m_blockable;
						hitData.m_skill = this.m_weapon.m_shared.m_skillType;
						hitData.m_skillRaiseAmount = this.m_raiseSkillAmount;
						hitData.m_damage.Add(this.m_weapon.GetDamage(), 1);
						hitData.m_point = vector3;
						hitData.m_dir = vector4;
						hitData.m_hitCollider = collider;
						hitData.SetAttacker(this.m_character);
						hitData.m_damage.Modify(this.m_damageMultiplier);
						hitData.m_damage.Modify(randomSkillFactor);
						hitData.m_damage.Modify(this.GetLevelDamageFactor());
						if (this.m_attackChainLevels > 1 && this.m_currentAttackCainLevel == this.m_attackChainLevels - 1 && this.m_lastChainDamageMultiplier > 1f)
						{
							hitData.m_damage.Modify(this.m_lastChainDamageMultiplier);
							hitData.m_pushForce *= 1.2f;
						}
						this.m_character.GetSEMan().ModifyAttack(this.m_weapon.m_shared.m_skillType, ref hitData);
						Character character = component as Character;
						if (character)
						{
							bool flag2 = BaseAI.IsEnemy(this.m_character, character) || (character.GetBaseAI() && character.GetBaseAI().IsAggravatable() && this.m_character.IsPlayer());
							if ((!this.m_character.IsPlayer() && !flag2) || (!this.m_weapon.m_shared.m_tamedOnly && this.m_character.IsPlayer() && !this.m_character.IsPVPEnabled() && !flag2) || (this.m_weapon.m_shared.m_tamedOnly && !character.IsTamed()))
							{
								goto IL_4C5;
							}
							if (hitData.m_dodgeable && character.IsDodgeInvincible())
							{
								goto IL_4C5;
							}
						}
						else if (this.m_weapon.m_shared.m_tamedOnly)
						{
							goto IL_4C5;
						}
						component.Damage(hitData);
						if ((component.GetDestructibleType() & this.m_skillHitType) != DestructibleType.None)
						{
							flag = true;
						}
					}
					num++;
					vector2 += vector3;
				}
			}
			IL_4C5:;
		}
		if (num > 0)
		{
			vector2 /= (float)num;
			this.m_weapon.m_shared.m_hitEffect.Create(vector2, Quaternion.identity, null, 1f, -1);
			this.m_hitEffect.Create(vector2, Quaternion.identity, null, 1f, -1);
			if (this.m_weapon.m_shared.m_useDurability && this.m_character.IsPlayer())
			{
				this.m_weapon.m_durability -= 1f;
			}
			this.m_character.AddNoise(this.m_attackHitNoise);
			if (flag)
			{
				this.m_character.RaiseSkill(this.m_weapon.m_shared.m_skillType, this.m_raiseSkillAmount);
			}
		}
		if (this.m_spawnOnTrigger)
		{
			IProjectile component2 = UnityEngine.Object.Instantiate<GameObject>(this.m_spawnOnTrigger, vector, Quaternion.identity).GetComponent<IProjectile>();
			if (component2 != null)
			{
				component2.Setup(this.m_character, this.m_character.transform.forward, -1f, null, null, this.m_lastUsedAmmo);
			}
		}
	}

	// Token: 0x060003E1 RID: 993 RVA: 0x0001ED3C File Offset: 0x0001CF3C
	private void GetMeleeAttackDir(out Transform originJoint, out Vector3 attackDir)
	{
		originJoint = this.GetAttackOrigin();
		Vector3 forward = this.m_character.transform.forward;
		Vector3 aimDir = this.m_character.GetAimDir(originJoint.position);
		aimDir.x = forward.x;
		aimDir.z = forward.z;
		aimDir.Normalize();
		attackDir = Vector3.RotateTowards(this.m_character.transform.forward, aimDir, 0.017453292f * this.m_maxYAngle, 10f);
	}

	// Token: 0x060003E2 RID: 994 RVA: 0x0001EDC4 File Offset: 0x0001CFC4
	private void AddHitPoint(List<Attack.HitPoint> list, GameObject go, Collider collider, Vector3 point, float distance, bool multiCollider)
	{
		Attack.HitPoint hitPoint = null;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if ((!multiCollider && list[i].go == go) || (multiCollider && list[i].collider == collider))
			{
				hitPoint = list[i];
				break;
			}
		}
		if (hitPoint == null)
		{
			hitPoint = new Attack.HitPoint();
			hitPoint.go = go;
			hitPoint.collider = collider;
			hitPoint.firstPoint = point;
			list.Add(hitPoint);
		}
		hitPoint.avgPoint += point;
		hitPoint.count++;
		if (distance < hitPoint.closestDistance)
		{
			hitPoint.closestPoint = point;
			hitPoint.closestDistance = distance;
		}
	}

	// Token: 0x060003E3 RID: 995 RVA: 0x0001EE84 File Offset: 0x0001D084
	private void DoMeleeAttack()
	{
		Transform transform;
		Vector3 vector;
		this.GetMeleeAttackDir(out transform, out vector);
		Vector3 point = this.m_character.transform.InverseTransformDirection(vector);
		Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
		this.m_weapon.m_shared.m_triggerEffect.Create(transform.position, quaternion, transform, 1f, -1);
		this.m_triggerEffect.Create(transform.position, quaternion, transform, 1f, -1);
		Vector3 vector2 = transform.position + Vector3.up * this.m_attackHeight + this.m_character.transform.right * this.m_attackOffset;
		float num = this.m_attackAngle / 2f;
		float num2 = 4f;
		float attackRange = this.m_attackRange;
		List<Attack.HitPoint> list = new List<Attack.HitPoint>();
		HashSet<Skills.SkillType> hashSet = new HashSet<Skills.SkillType>();
		int layerMask = this.m_hitTerrain ? Attack.m_attackMaskTerrain : Attack.m_attackMask;
		for (float num3 = -num; num3 <= num; num3 += num2)
		{
			Quaternion rotation = Quaternion.identity;
			if (this.m_attackType == Attack.AttackType.Horizontal)
			{
				rotation = Quaternion.Euler(0f, -num3, 0f);
			}
			else if (this.m_attackType == Attack.AttackType.Vertical)
			{
				rotation = Quaternion.Euler(num3, 0f, 0f);
			}
			Vector3 vector3 = this.m_character.transform.TransformDirection(rotation * point);
			Debug.DrawLine(vector2, vector2 + vector3 * attackRange);
			RaycastHit[] array;
			if (this.m_attackRayWidth > 0f)
			{
				array = Physics.SphereCastAll(vector2, this.m_attackRayWidth, vector3, Mathf.Max(0f, attackRange - this.m_attackRayWidth), layerMask, QueryTriggerInteraction.Ignore);
			}
			else
			{
				array = Physics.RaycastAll(vector2, vector3, attackRange, layerMask, QueryTriggerInteraction.Ignore);
			}
			Array.Sort<RaycastHit>(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
			foreach (RaycastHit raycastHit in array)
			{
				if (!(raycastHit.collider.gameObject == this.m_character.gameObject))
				{
					Vector3 vector4 = raycastHit.point;
					if (raycastHit.distance < 1E-45f)
					{
						if (raycastHit.collider is MeshCollider)
						{
							vector4 = vector2 + vector3 * attackRange;
						}
						else
						{
							vector4 = raycastHit.collider.ClosestPoint(vector2);
						}
					}
					if ((raycastHit.normal == -vector3 && vector4 == Vector3.zero) || this.m_attackAngle >= 180f || Vector3.Dot(vector4 - vector2, vector) > 0f)
					{
						GameObject gameObject = Projectile.FindHitObject(raycastHit.collider);
						if (!(gameObject == this.m_character.gameObject))
						{
							Vagon component = gameObject.GetComponent<Vagon>();
							if (!component || !component.IsAttached(this.m_character))
							{
								Character component2 = gameObject.GetComponent<Character>();
								if (component2 != null)
								{
									bool flag = BaseAI.IsEnemy(this.m_character, component2) || (component2.GetBaseAI() && component2.GetBaseAI().IsAggravatable() && this.m_character.IsPlayer());
									if ((!this.m_character.IsPlayer() && !flag) || (!this.m_weapon.m_shared.m_tamedOnly && this.m_character.IsPlayer() && !this.m_character.IsPVPEnabled() && !flag) || (this.m_weapon.m_shared.m_tamedOnly && !component2.IsTamed()))
									{
										goto IL_412;
									}
									if (this.m_weapon.m_shared.m_dodgeable && component2.IsDodgeInvincible())
									{
										goto IL_412;
									}
								}
								else if (this.m_weapon.m_shared.m_tamedOnly)
								{
									goto IL_412;
								}
								bool multiCollider = this.m_pickaxeSpecial && (gameObject.GetComponent<MineRock5>() || gameObject.GetComponent<MineRock>());
								this.AddHitPoint(list, gameObject, raycastHit.collider, vector4, raycastHit.distance, multiCollider);
								if (!this.m_hitThroughWalls)
								{
									break;
								}
							}
						}
					}
				}
				IL_412:;
			}
		}
		int num4 = 0;
		Vector3 vector5 = Vector3.zero;
		bool flag2 = false;
		Character character = null;
		bool flag3 = false;
		foreach (Attack.HitPoint hitPoint in list)
		{
			GameObject go = hitPoint.go;
			Vector3 vector6 = hitPoint.avgPoint / (float)hitPoint.count;
			Vector3 vector7 = vector6;
			switch (this.m_hitPointtype)
			{
			case Attack.HitPointType.Closest:
				vector7 = hitPoint.closestPoint;
				break;
			case Attack.HitPointType.Average:
				vector7 = vector6;
				break;
			case Attack.HitPointType.First:
				vector7 = hitPoint.firstPoint;
				break;
			}
			num4++;
			vector5 += vector6;
			this.m_weapon.m_shared.m_hitEffect.Create(vector7, Quaternion.identity, null, 1f, -1);
			this.m_hitEffect.Create(vector7, Quaternion.identity, null, 1f, -1);
			IDestructible component3 = go.GetComponent<IDestructible>();
			if (component3 != null)
			{
				DestructibleType destructibleType = component3.GetDestructibleType();
				Skills.SkillType skillType = this.m_weapon.m_shared.m_skillType;
				if (this.m_specialHitSkill != Skills.SkillType.None && (destructibleType & this.m_specialHitType) != DestructibleType.None)
				{
					skillType = this.m_specialHitSkill;
					hashSet.Add(this.m_specialHitSkill);
				}
				else if ((destructibleType & this.m_skillHitType) != DestructibleType.None)
				{
					hashSet.Add(skillType);
				}
				float num5 = this.m_character.GetRandomSkillFactor(skillType);
				if (this.m_multiHit && this.m_lowerDamagePerHit && list.Count > 1)
				{
					num5 /= (float)list.Count * 0.75f;
				}
				HitData hitData = new HitData();
				hitData.m_toolTier = (short)this.m_weapon.m_shared.m_toolTier;
				hitData.m_statusEffectHash = (this.m_weapon.m_shared.m_attackStatusEffect ? this.m_weapon.m_shared.m_attackStatusEffect.NameHash() : 0);
				hitData.m_skillLevel = this.m_character.GetSkillLevel(this.m_weapon.m_shared.m_skillType);
				hitData.m_itemLevel = (short)this.m_weapon.m_quality;
				hitData.m_pushForce = this.m_weapon.m_shared.m_attackForce * num5 * this.m_forceMultiplier;
				hitData.m_backstabBonus = this.m_weapon.m_shared.m_backstabBonus;
				hitData.m_staggerMultiplier = this.m_staggerMultiplier;
				hitData.m_dodgeable = this.m_weapon.m_shared.m_dodgeable;
				hitData.m_blockable = this.m_weapon.m_shared.m_blockable;
				hitData.m_skill = skillType;
				hitData.m_skillRaiseAmount = this.m_raiseSkillAmount;
				hitData.m_damage = this.m_weapon.GetDamage();
				hitData.m_point = vector7;
				hitData.m_dir = (vector7 - vector2).normalized;
				hitData.m_hitCollider = hitPoint.collider;
				hitData.SetAttacker(this.m_character);
				hitData.m_damage.Modify(this.m_damageMultiplier);
				hitData.m_damage.Modify(num5);
				hitData.m_damage.Modify(this.GetLevelDamageFactor());
				if (this.m_attackChainLevels > 1 && this.m_currentAttackCainLevel == this.m_attackChainLevels - 1)
				{
					hitData.m_damage.Modify(2f);
					hitData.m_pushForce *= 1.2f;
				}
				this.m_character.GetSEMan().ModifyAttack(skillType, ref hitData);
				if (component3 is Character)
				{
					character = (component3 as Character);
				}
				component3.Damage(hitData);
				if ((destructibleType & this.m_resetChainIfHit) != DestructibleType.None)
				{
					this.m_nextAttackChainLevel = 0;
				}
				if (!this.m_multiHit)
				{
					break;
				}
			}
			if (go.GetComponent<Heightmap>() != null && !flag2 && (!this.m_pickaxeSpecial || !flag3))
			{
				flag2 = true;
				this.m_weapon.m_shared.m_hitTerrainEffect.Create(vector7, quaternion, null, 1f, -1);
				this.m_hitTerrainEffect.Create(vector7, quaternion, null, 1f, -1);
				if (this.m_weapon.m_shared.m_spawnOnHitTerrain)
				{
					this.SpawnOnHitTerrain(vector7, this.m_weapon.m_shared.m_spawnOnHitTerrain);
				}
				if (!this.m_multiHit)
				{
					break;
				}
				if (this.m_pickaxeSpecial)
				{
					break;
				}
			}
			else
			{
				flag3 = true;
			}
		}
		if (num4 > 0)
		{
			vector5 /= (float)num4;
			if (this.m_weapon.m_shared.m_useDurability && this.m_character.IsPlayer())
			{
				this.m_weapon.m_durability -= this.m_weapon.m_shared.m_useDurabilityDrain;
			}
			this.m_character.AddNoise(this.m_attackHitNoise);
			this.m_character.FreezeFrame(0.15f);
			if (this.m_weapon.m_shared.m_spawnOnHit)
			{
				IProjectile component4 = UnityEngine.Object.Instantiate<GameObject>(this.m_weapon.m_shared.m_spawnOnHit, vector5, quaternion).GetComponent<IProjectile>();
				if (component4 != null)
				{
					component4.Setup(this.m_character, Vector3.zero, this.m_attackHitNoise, null, this.m_weapon, this.m_lastUsedAmmo);
				}
			}
			foreach (Skills.SkillType skill in hashSet)
			{
				this.m_character.RaiseSkill(skill, this.m_raiseSkillAmount * ((character != null) ? 1.5f : 1f));
			}
			if (this.m_attach && !this.m_isAttached && character)
			{
				this.TryAttach(character, vector5);
			}
		}
		if (this.m_spawnOnTrigger)
		{
			IProjectile component5 = UnityEngine.Object.Instantiate<GameObject>(this.m_spawnOnTrigger, vector2, Quaternion.identity).GetComponent<IProjectile>();
			if (component5 != null)
			{
				component5.Setup(this.m_character, this.m_character.transform.forward, -1f, null, this.m_weapon, this.m_lastUsedAmmo);
			}
		}
	}

	// Token: 0x060003E4 RID: 996 RVA: 0x0001F8FC File Offset: 0x0001DAFC
	private bool TryAttach(Character hitCharacter, Vector3 hitPoint)
	{
		if (hitCharacter.IsDodgeInvincible())
		{
			return false;
		}
		if (hitCharacter.IsBlocking())
		{
			Vector3 lhs = hitCharacter.transform.position - this.m_character.transform.position;
			lhs.y = 0f;
			lhs.Normalize();
			if (Vector3.Dot(lhs, hitCharacter.transform.forward) < 0f)
			{
				return false;
			}
		}
		this.m_isAttached = true;
		this.m_attachTarget = hitCharacter.transform;
		float num = hitCharacter.GetRadius() + this.m_character.GetRadius() + 0.1f;
		Vector3 a = hitCharacter.transform.position - this.m_character.transform.position;
		a.y = 0f;
		a.Normalize();
		this.m_attachDistance = num;
		Vector3 position = hitCharacter.GetCenterPoint() - a * num;
		this.m_attachOffset = this.m_attachTarget.InverseTransformPoint(position);
		hitPoint.y = Mathf.Clamp(hitPoint.y, hitCharacter.transform.position.y + hitCharacter.GetRadius(), hitCharacter.transform.position.y + hitCharacter.GetHeight() - hitCharacter.GetRadius() * 1.5f);
		this.m_attachHitPoint = this.m_attachTarget.InverseTransformPoint(hitPoint);
		this.m_zanim.SetTrigger("attach");
		return true;
	}

	// Token: 0x060003E5 RID: 997 RVA: 0x0001FA68 File Offset: 0x0001DC68
	private void UpdateAttach(float dt)
	{
		if (this.m_attachTarget)
		{
			Character component = this.m_attachTarget.GetComponent<Character>();
			if (component != null)
			{
				if (component.IsDead())
				{
					this.Stop();
					return;
				}
				this.m_detachTimer += dt;
				if (this.m_detachTimer > 0.3f)
				{
					this.m_detachTimer = 0f;
					if (component.IsDodgeInvincible())
					{
						this.Stop();
						return;
					}
				}
			}
			Vector3 b = this.m_attachTarget.TransformPoint(this.m_attachOffset);
			Vector3 a = this.m_attachTarget.TransformPoint(this.m_attachHitPoint);
			Vector3 b2 = Vector3.Lerp(this.m_character.transform.position, b, 0.1f);
			Vector3 vector = a - b2;
			vector.Normalize();
			Quaternion rotation = Quaternion.LookRotation(vector);
			Vector3 position = a - vector * this.m_character.GetRadius();
			this.m_character.transform.position = position;
			this.m_character.transform.rotation = rotation;
			this.m_character.GetComponent<Rigidbody>().velocity = Vector3.zero;
			return;
		}
		this.Stop();
	}

	// Token: 0x060003E6 RID: 998 RVA: 0x0001FB8C File Offset: 0x0001DD8C
	public bool IsAttached()
	{
		return this.m_isAttached;
	}

	// Token: 0x060003E7 RID: 999 RVA: 0x0001FB94 File Offset: 0x0001DD94
	public bool GetAttachData(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		attachJoint = "";
		parent = ZDOID.None;
		relativePos = Vector3.zero;
		relativeRot = Quaternion.identity;
		relativeVel = Vector3.zero;
		if (!this.m_isAttached || !this.m_attachTarget)
		{
			return false;
		}
		ZNetView component = this.m_attachTarget.GetComponent<ZNetView>();
		if (!component)
		{
			return false;
		}
		parent = component.GetZDO().m_uid;
		relativePos = component.transform.InverseTransformPoint(this.m_character.transform.position);
		relativeRot = Quaternion.Inverse(component.transform.rotation) * this.m_character.transform.rotation;
		relativeVel = Vector3.zero;
		return true;
	}

	// Token: 0x060003E8 RID: 1000 RVA: 0x0001FC70 File Offset: 0x0001DE70
	private void SpawnOnHitTerrain(Vector3 hitPoint, GameObject prefab)
	{
		TerrainModifier componentInChildren = prefab.GetComponentInChildren<TerrainModifier>();
		if (componentInChildren)
		{
			if (!PrivateArea.CheckAccess(hitPoint, componentInChildren.GetRadius(), true, false))
			{
				return;
			}
			if (Location.IsInsideNoBuildLocation(hitPoint))
			{
				return;
			}
		}
		TerrainOp componentInChildren2 = prefab.GetComponentInChildren<TerrainOp>();
		if (componentInChildren2)
		{
			if (!PrivateArea.CheckAccess(hitPoint, componentInChildren2.GetRadius(), true, false))
			{
				return;
			}
			if (Location.IsInsideNoBuildLocation(hitPoint))
			{
				return;
			}
		}
		TerrainModifier.SetTriggerOnPlaced(true);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, hitPoint, Quaternion.LookRotation(this.m_character.transform.forward));
		TerrainModifier.SetTriggerOnPlaced(false);
		IProjectile component = gameObject.GetComponent<IProjectile>();
		if (component != null)
		{
			component.Setup(this.m_character, Vector3.zero, this.m_attackHitNoise, null, this.m_weapon, this.m_lastUsedAmmo);
		}
	}

	// Token: 0x060003E9 RID: 1001 RVA: 0x0001FD24 File Offset: 0x0001DF24
	public Attack Clone()
	{
		return base.MemberwiseClone() as Attack;
	}

	// Token: 0x060003EA RID: 1002 RVA: 0x0001FD31 File Offset: 0x0001DF31
	public ItemDrop.ItemData GetWeapon()
	{
		return this.m_weapon;
	}

	// Token: 0x060003EB RID: 1003 RVA: 0x0001FD39 File Offset: 0x0001DF39
	public bool CanStartChainAttack()
	{
		return this.m_nextAttackChainLevel > 0 && this.m_animEvent.CanChain();
	}

	// Token: 0x060003EC RID: 1004 RVA: 0x0001FD54 File Offset: 0x0001DF54
	public void OnTrailStart()
	{
		if (this.m_attackType == Attack.AttackType.Projectile)
		{
			Transform attackOrigin = this.GetAttackOrigin();
			this.m_weapon.m_shared.m_trailStartEffect.Create(attackOrigin.position, this.m_character.transform.rotation, this.m_character.transform, 1f, -1);
			this.m_trailStartEffect.Create(attackOrigin.position, this.m_character.transform.rotation, this.m_character.transform, 1f, -1);
			return;
		}
		Transform transform;
		Vector3 forward;
		this.GetMeleeAttackDir(out transform, out forward);
		Quaternion baseRot = Quaternion.LookRotation(forward, Vector3.up);
		this.m_weapon.m_shared.m_trailStartEffect.Create(transform.position, baseRot, this.m_character.transform, 1f, -1);
		this.m_trailStartEffect.Create(transform.position, baseRot, this.m_character.transform, 1f, -1);
	}

	// Token: 0x040003E8 RID: 1000
	[Header("Common")]
	public Attack.AttackType m_attackType;

	// Token: 0x040003E9 RID: 1001
	public string m_attackAnimation = "";

	// Token: 0x040003EA RID: 1002
	public int m_attackRandomAnimations;

	// Token: 0x040003EB RID: 1003
	public int m_attackChainLevels;

	// Token: 0x040003EC RID: 1004
	public bool m_loopingAttack;

	// Token: 0x040003ED RID: 1005
	public bool m_consumeItem;

	// Token: 0x040003EE RID: 1006
	public bool m_hitTerrain = true;

	// Token: 0x040003EF RID: 1007
	public float m_attackStamina = 20f;

	// Token: 0x040003F0 RID: 1008
	public float m_attackEitr;

	// Token: 0x040003F1 RID: 1009
	public float m_attackHealth;

	// Token: 0x040003F2 RID: 1010
	[Range(0f, 100f)]
	public float m_attackHealthPercentage;

	// Token: 0x040003F3 RID: 1011
	public float m_speedFactor = 0.2f;

	// Token: 0x040003F4 RID: 1012
	public float m_speedFactorRotation = 0.2f;

	// Token: 0x040003F5 RID: 1013
	public float m_attackStartNoise = 10f;

	// Token: 0x040003F6 RID: 1014
	public float m_attackHitNoise = 30f;

	// Token: 0x040003F7 RID: 1015
	public float m_damageMultiplier = 1f;

	// Token: 0x040003F8 RID: 1016
	public float m_forceMultiplier = 1f;

	// Token: 0x040003F9 RID: 1017
	public float m_staggerMultiplier = 1f;

	// Token: 0x040003FA RID: 1018
	public float m_recoilPushback;

	// Token: 0x040003FB RID: 1019
	public int m_selfDamage;

	// Token: 0x040003FC RID: 1020
	[Header("Misc")]
	public string m_attackOriginJoint = "";

	// Token: 0x040003FD RID: 1021
	public float m_attackRange = 1.5f;

	// Token: 0x040003FE RID: 1022
	public float m_attackHeight = 0.6f;

	// Token: 0x040003FF RID: 1023
	public float m_attackOffset;

	// Token: 0x04000400 RID: 1024
	public GameObject m_spawnOnTrigger;

	// Token: 0x04000401 RID: 1025
	public bool m_toggleFlying;

	// Token: 0x04000402 RID: 1026
	public bool m_attach;

	// Token: 0x04000403 RID: 1027
	[Header("Loading")]
	public bool m_requiresReload;

	// Token: 0x04000404 RID: 1028
	public string m_reloadAnimation = "";

	// Token: 0x04000405 RID: 1029
	public float m_reloadTime = 2f;

	// Token: 0x04000406 RID: 1030
	public float m_reloadStaminaDrain;

	// Token: 0x04000407 RID: 1031
	[Header("Draw")]
	public bool m_bowDraw;

	// Token: 0x04000408 RID: 1032
	public float m_drawDurationMin;

	// Token: 0x04000409 RID: 1033
	public float m_drawStaminaDrain;

	// Token: 0x0400040A RID: 1034
	public string m_drawAnimationState = "";

	// Token: 0x0400040B RID: 1035
	[Header("Melee/AOE")]
	public float m_attackAngle = 90f;

	// Token: 0x0400040C RID: 1036
	public float m_attackRayWidth;

	// Token: 0x0400040D RID: 1037
	public float m_maxYAngle;

	// Token: 0x0400040E RID: 1038
	public bool m_lowerDamagePerHit = true;

	// Token: 0x0400040F RID: 1039
	public Attack.HitPointType m_hitPointtype;

	// Token: 0x04000410 RID: 1040
	public bool m_hitThroughWalls;

	// Token: 0x04000411 RID: 1041
	public bool m_multiHit = true;

	// Token: 0x04000412 RID: 1042
	public bool m_pickaxeSpecial;

	// Token: 0x04000413 RID: 1043
	public float m_lastChainDamageMultiplier = 2f;

	// Token: 0x04000414 RID: 1044
	[BitMask(typeof(DestructibleType))]
	public DestructibleType m_resetChainIfHit;

	// Token: 0x04000415 RID: 1045
	[Header("Skill settings")]
	public float m_raiseSkillAmount = 1f;

	// Token: 0x04000416 RID: 1046
	[BitMask(typeof(DestructibleType))]
	public DestructibleType m_skillHitType = DestructibleType.Character;

	// Token: 0x04000417 RID: 1047
	public Skills.SkillType m_specialHitSkill;

	// Token: 0x04000418 RID: 1048
	[BitMask(typeof(DestructibleType))]
	public DestructibleType m_specialHitType;

	// Token: 0x04000419 RID: 1049
	[Header("Projectile")]
	public GameObject m_attackProjectile;

	// Token: 0x0400041A RID: 1050
	public float m_projectileVel = 10f;

	// Token: 0x0400041B RID: 1051
	public float m_projectileVelMin = 2f;

	// Token: 0x0400041C RID: 1052
	public float m_projectileAccuracy = 10f;

	// Token: 0x0400041D RID: 1053
	public float m_projectileAccuracyMin = 20f;

	// Token: 0x0400041E RID: 1054
	public bool m_skillAccuracy;

	// Token: 0x0400041F RID: 1055
	public bool m_useCharacterFacing;

	// Token: 0x04000420 RID: 1056
	public bool m_useCharacterFacingYAim;

	// Token: 0x04000421 RID: 1057
	[FormerlySerializedAs("m_useCharacterFacingAngle")]
	public float m_launchAngle;

	// Token: 0x04000422 RID: 1058
	public int m_projectiles = 1;

	// Token: 0x04000423 RID: 1059
	public int m_projectileBursts = 1;

	// Token: 0x04000424 RID: 1060
	public float m_burstInterval;

	// Token: 0x04000425 RID: 1061
	public bool m_destroyPreviousProjectile;

	// Token: 0x04000426 RID: 1062
	public bool m_perBurstResourceUsage;

	// Token: 0x04000427 RID: 1063
	[Header("Attack-Effects")]
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000428 RID: 1064
	public EffectList m_hitTerrainEffect = new EffectList();

	// Token: 0x04000429 RID: 1065
	public EffectList m_startEffect = new EffectList();

	// Token: 0x0400042A RID: 1066
	public EffectList m_triggerEffect = new EffectList();

	// Token: 0x0400042B RID: 1067
	public EffectList m_trailStartEffect = new EffectList();

	// Token: 0x0400042C RID: 1068
	public EffectList m_burstEffect = new EffectList();

	// Token: 0x0400042D RID: 1069
	protected static int m_attackMask;

	// Token: 0x0400042E RID: 1070
	protected static int m_attackMaskTerrain;

	// Token: 0x0400042F RID: 1071
	private Humanoid m_character;

	// Token: 0x04000430 RID: 1072
	private BaseAI m_baseAI;

	// Token: 0x04000431 RID: 1073
	private Rigidbody m_body;

	// Token: 0x04000432 RID: 1074
	private ZSyncAnimation m_zanim;

	// Token: 0x04000433 RID: 1075
	private CharacterAnimEvent m_animEvent;

	// Token: 0x04000434 RID: 1076
	[NonSerialized]
	private ItemDrop.ItemData m_weapon;

	// Token: 0x04000435 RID: 1077
	private VisEquipment m_visEquipment;

	// Token: 0x04000436 RID: 1078
	[NonSerialized]
	private ItemDrop.ItemData m_lastUsedAmmo;

	// Token: 0x04000437 RID: 1079
	private float m_attackDrawPercentage;

	// Token: 0x04000438 RID: 1080
	private const float m_freezeFrameDuration = 0.15f;

	// Token: 0x04000439 RID: 1081
	private const float m_chainAttackMaxTime = 0.2f;

	// Token: 0x0400043A RID: 1082
	private int m_nextAttackChainLevel;

	// Token: 0x0400043B RID: 1083
	private int m_currentAttackCainLevel;

	// Token: 0x0400043C RID: 1084
	private bool m_wasInAttack;

	// Token: 0x0400043D RID: 1085
	private float m_time;

	// Token: 0x0400043E RID: 1086
	private bool m_abortAttack;

	// Token: 0x0400043F RID: 1087
	private bool m_projectileAttackStarted;

	// Token: 0x04000440 RID: 1088
	private float m_projectileFireTimer = -1f;

	// Token: 0x04000441 RID: 1089
	private int m_projectileBurstsFired;

	// Token: 0x04000442 RID: 1090
	[NonSerialized]
	private ItemDrop.ItemData m_ammoItem;

	// Token: 0x04000443 RID: 1091
	private bool m_attackDone;

	// Token: 0x04000444 RID: 1092
	private bool m_isAttached;

	// Token: 0x04000445 RID: 1093
	private Transform m_attachTarget;

	// Token: 0x04000446 RID: 1094
	private Vector3 m_attachOffset;

	// Token: 0x04000447 RID: 1095
	private float m_attachDistance;

	// Token: 0x04000448 RID: 1096
	private Vector3 m_attachHitPoint;

	// Token: 0x04000449 RID: 1097
	private float m_detachTimer;

	// Token: 0x02000042 RID: 66
	private class HitPoint
	{
		// Token: 0x0400044A RID: 1098
		public GameObject go;

		// Token: 0x0400044B RID: 1099
		public Vector3 avgPoint = Vector3.zero;

		// Token: 0x0400044C RID: 1100
		public int count;

		// Token: 0x0400044D RID: 1101
		public Vector3 firstPoint;

		// Token: 0x0400044E RID: 1102
		public Collider collider;

		// Token: 0x0400044F RID: 1103
		public Dictionary<Collider, Vector3> allHits = new Dictionary<Collider, Vector3>();

		// Token: 0x04000450 RID: 1104
		public Vector3 closestPoint;

		// Token: 0x04000451 RID: 1105
		public float closestDistance = 999999f;
	}

	// Token: 0x02000043 RID: 67
	public enum AttackType
	{
		// Token: 0x04000453 RID: 1107
		Horizontal,
		// Token: 0x04000454 RID: 1108
		Vertical,
		// Token: 0x04000455 RID: 1109
		Projectile,
		// Token: 0x04000456 RID: 1110
		None,
		// Token: 0x04000457 RID: 1111
		Area,
		// Token: 0x04000458 RID: 1112
		TriggerProjectile
	}

	// Token: 0x02000044 RID: 68
	public enum HitPointType
	{
		// Token: 0x0400045A RID: 1114
		Closest,
		// Token: 0x0400045B RID: 1115
		Average,
		// Token: 0x0400045C RID: 1116
		First
	}
}
