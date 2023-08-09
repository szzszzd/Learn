using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000053 RID: 83
public class MonsterAI : BaseAI
{
	// Token: 0x06000482 RID: 1154 RVA: 0x00024B70 File Offset: 0x00022D70
	protected override void Awake()
	{
		base.Awake();
		this.m_despawnInDay = this.m_nview.GetZDO().GetBool(ZDOVars.s_despawnInDay, this.m_despawnInDay);
		this.m_eventCreature = this.m_nview.GetZDO().GetBool(ZDOVars.s_eventCreature, this.m_eventCreature);
		this.m_animator.SetBool(MonsterAI.s_sleeping, this.IsSleeping());
		this.m_interceptTime = UnityEngine.Random.Range(this.m_interceptTimeMin, this.m_interceptTimeMax);
		this.m_pauseTimer = UnityEngine.Random.Range(0f, this.m_circleTargetInterval);
		this.m_updateTargetTimer = UnityEngine.Random.Range(0f, 2f);
		if (this.m_wakeUpDelayMin > 0f || this.m_wakeUpDelayMax > 0f)
		{
			this.m_sleepDelay = UnityEngine.Random.Range(this.m_wakeUpDelayMin, this.m_wakeUpDelayMax);
		}
		if (this.m_enableHuntPlayer)
		{
			base.SetHuntPlayer(true);
		}
	}

	// Token: 0x06000483 RID: 1155 RVA: 0x00024C5D File Offset: 0x00022E5D
	protected override void OnEnable()
	{
		base.OnEnable();
		MonsterAI.Instances.Add(this);
	}

	// Token: 0x06000484 RID: 1156 RVA: 0x00024C70 File Offset: 0x00022E70
	protected override void OnDisable()
	{
		base.OnDisable();
		MonsterAI.Instances.Remove(this);
	}

	// Token: 0x06000485 RID: 1157 RVA: 0x00024C84 File Offset: 0x00022E84
	private void Start()
	{
		if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			Humanoid humanoid = this.m_character as Humanoid;
			if (humanoid)
			{
				humanoid.EquipBestWeapon(null, null, null, null);
			}
		}
	}

	// Token: 0x06000486 RID: 1158 RVA: 0x00024CD6 File Offset: 0x00022ED6
	protected override void OnDamaged(float damage, Character attacker)
	{
		base.OnDamaged(damage, attacker);
		this.Wakeup();
		this.SetAlerted(true);
		this.SetTarget(attacker);
	}

	// Token: 0x06000487 RID: 1159 RVA: 0x00024CF4 File Offset: 0x00022EF4
	private void SetTarget(Character attacker)
	{
		if (attacker != null && this.m_targetCreature == null)
		{
			if (attacker.IsPlayer() && this.m_character.IsTamed())
			{
				return;
			}
			this.m_targetCreature = attacker;
			this.m_lastKnownTargetPos = attacker.transform.position;
			this.m_beenAtLastPos = false;
			this.m_targetStatic = null;
		}
	}

	// Token: 0x06000488 RID: 1160 RVA: 0x00024D54 File Offset: 0x00022F54
	protected override void RPC_OnNearProjectileHit(long sender, Vector3 center, float range, ZDOID attackerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.SetAlerted(true);
		if (this.m_fleeIfNotAlerted)
		{
			return;
		}
		GameObject gameObject = ZNetScene.instance.FindInstance(attackerID);
		if (gameObject != null)
		{
			Character component = gameObject.GetComponent<Character>();
			if (component)
			{
				this.SetTarget(component);
			}
		}
	}

	// Token: 0x06000489 RID: 1161 RVA: 0x00024DAB File Offset: 0x00022FAB
	public void MakeTame()
	{
		this.m_character.SetTamed(true);
		this.SetAlerted(false);
		this.m_targetCreature = null;
		this.m_targetStatic = null;
	}

	// Token: 0x0600048A RID: 1162 RVA: 0x00024DD0 File Offset: 0x00022FD0
	private void UpdateTarget(Humanoid humanoid, float dt, out bool canHearTarget, out bool canSeeTarget)
	{
		this.m_unableToAttackTargetTimer -= dt;
		this.m_updateTargetTimer -= dt;
		if (this.m_updateTargetTimer <= 0f && !this.m_character.InAttack())
		{
			this.m_updateTargetTimer = (Player.IsPlayerInRange(base.transform.position, 50f) ? 2f : 6f);
			Character character = base.FindEnemy();
			if (character)
			{
				this.m_targetCreature = character;
				this.m_targetStatic = null;
			}
			bool flag = this.m_targetCreature != null && this.m_targetCreature.IsPlayer();
			bool flag2 = this.m_targetCreature != null && this.m_unableToAttackTargetTimer > 0f && !base.HavePath(this.m_targetCreature.transform.position);
			if (this.m_attackPlayerObjects && (!this.m_aggravatable || base.IsAggravated()) && (this.m_targetCreature == null || flag2) && !this.m_character.IsTamed())
			{
				StaticTarget staticTarget = base.FindClosestStaticPriorityTarget();
				if (staticTarget)
				{
					this.m_targetStatic = staticTarget;
					this.m_targetCreature = null;
				}
				bool flag3 = false;
				if (this.m_targetStatic != null)
				{
					Vector3 target = this.m_targetStatic.FindClosestPoint(this.m_character.transform.position);
					flag3 = base.HavePath(target);
				}
				if ((this.m_targetStatic == null || !flag3) && base.IsAlerted() && flag)
				{
					StaticTarget staticTarget2 = base.FindRandomStaticTarget(10f);
					if (staticTarget2)
					{
						this.m_targetStatic = staticTarget2;
						this.m_targetCreature = null;
					}
				}
			}
		}
		if (this.m_targetCreature && this.m_character.IsTamed())
		{
			Vector3 b;
			if (base.GetPatrolPoint(out b))
			{
				if (Vector3.Distance(this.m_targetCreature.transform.position, b) > this.m_alertRange)
				{
					this.m_targetCreature = null;
				}
			}
			else if (this.m_follow && Vector3.Distance(this.m_targetCreature.transform.position, this.m_follow.transform.position) > this.m_alertRange)
			{
				this.m_targetCreature = null;
			}
		}
		if (this.m_targetCreature && this.m_targetCreature.IsDead())
		{
			this.m_targetCreature = null;
		}
		if (this.m_targetCreature && !base.IsEnemy(this.m_targetCreature))
		{
			this.m_targetCreature = null;
		}
		canHearTarget = false;
		canSeeTarget = false;
		if (this.m_targetCreature)
		{
			canHearTarget = base.CanHearTarget(this.m_targetCreature);
			canSeeTarget = base.CanSeeTarget(this.m_targetCreature);
			if (canSeeTarget | canHearTarget)
			{
				this.m_timeSinceSensedTargetCreature = 0f;
			}
			if (this.m_targetCreature.IsPlayer())
			{
				this.m_targetCreature.OnTargeted(canSeeTarget | canHearTarget, base.IsAlerted());
			}
			base.SetTargetInfo(this.m_targetCreature.GetZDOID());
		}
		else
		{
			base.SetTargetInfo(ZDOID.None);
		}
		this.m_timeSinceSensedTargetCreature += dt;
		if (base.IsAlerted() || this.m_targetCreature != null)
		{
			this.m_timeSinceAttacking += dt;
			float num = 60f;
			float num2 = Vector3.Distance(this.m_spawnPoint, base.transform.position);
			bool flag4 = this.HuntPlayer() && this.m_targetCreature && this.m_targetCreature.IsPlayer();
			if (this.m_timeSinceSensedTargetCreature > 30f || (!flag4 && (this.m_timeSinceAttacking > num || (this.m_maxChaseDistance > 0f && this.m_timeSinceSensedTargetCreature > 1f && num2 > this.m_maxChaseDistance))))
			{
				this.SetAlerted(false);
				this.m_targetCreature = null;
				this.m_targetStatic = null;
				this.m_timeSinceAttacking = 0f;
				this.m_updateTargetTimer = 5f;
			}
		}
	}

	// Token: 0x0600048B RID: 1163 RVA: 0x000251D8 File Offset: 0x000233D8
	public new void UpdateAI(float dt)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.IsSleeping())
		{
			this.UpdateSleep(dt);
			return;
		}
		Humanoid humanoid = this.m_character as Humanoid;
		if (this.HuntPlayer())
		{
			this.SetAlerted(true);
		}
		bool flag;
		bool flag2;
		this.UpdateTarget(humanoid, dt, out flag, out flag2);
		if (this.m_tamable && this.m_tamable.m_saddle && this.m_tamable.m_saddle.UpdateRiding(dt))
		{
			return;
		}
		if (this.m_avoidLand && !this.m_character.IsSwimming())
		{
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Move to water";
			}
			base.MoveToWater(dt, 20f);
			return;
		}
		if (this.DespawnInDay() && EnvMan.instance.IsDay() && (this.m_targetCreature == null || !flag2))
		{
			base.MoveAwayAndDespawn(dt, true);
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Trying to despawn ";
			}
			return;
		}
		if (this.IsEventCreature() && !RandEventSystem.HaveActiveEvent())
		{
			base.SetHuntPlayer(false);
			if (this.m_targetCreature == null && !base.IsAlerted())
			{
				base.MoveAwayAndDespawn(dt, false);
				if (this.m_aiStatus != null)
				{
					this.m_aiStatus = "Trying to despawn ";
				}
				return;
			}
		}
		if (this.m_fleeIfNotAlerted && !this.HuntPlayer() && this.m_targetCreature && !base.IsAlerted() && Vector3.Distance(this.m_targetCreature.transform.position, base.transform.position) - this.m_targetCreature.GetRadius() > this.m_alertRange)
		{
			base.Flee(dt, this.m_targetCreature.transform.position);
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Avoiding conflict";
			}
			return;
		}
		if (this.m_fleeIfLowHealth > 0f && this.m_character.GetHealthPercentage() < this.m_fleeIfLowHealth && this.m_timeSinceHurt < 20f && this.m_targetCreature != null)
		{
			base.Flee(dt, this.m_targetCreature.transform.position);
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Low health, flee";
			}
			return;
		}
		if ((this.m_afraidOfFire || this.m_avoidFire) && base.AvoidFire(dt, this.m_targetCreature, this.m_afraidOfFire))
		{
			if (this.m_afraidOfFire)
			{
				this.m_targetStatic = null;
				this.m_targetCreature = null;
			}
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Avoiding fire";
			}
			return;
		}
		if (!this.m_character.IsTamed())
		{
			if (this.m_targetCreature != null)
			{
				if (EffectArea.IsPointInsideArea(this.m_targetCreature.transform.position, EffectArea.Type.NoMonsters, 0f))
				{
					base.Flee(dt, this.m_targetCreature.transform.position);
					if (this.m_aiStatus != null)
					{
						this.m_aiStatus = "Avoid no-monster area";
					}
					return;
				}
			}
			else
			{
				EffectArea effectArea = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.NoMonsters, 15f);
				if (effectArea != null)
				{
					base.Flee(dt, effectArea.transform.position);
					if (this.m_aiStatus != null)
					{
						this.m_aiStatus = "Avoid no-monster area";
					}
					return;
				}
			}
		}
		if (this.m_fleeIfHurtWhenTargetCantBeReached && this.m_targetCreature != null && this.m_timeSinceAttacking > 30f && this.m_timeSinceHurt < 20f)
		{
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Hide from unreachable target";
			}
			base.Flee(dt, this.m_targetCreature.transform.position);
			this.m_lastKnownTargetPos = base.transform.position;
			this.m_updateTargetTimer = 1f;
			return;
		}
		if ((!base.IsAlerted() || (this.m_targetStatic == null && this.m_targetCreature == null)) && this.UpdateConsumeItem(humanoid, dt))
		{
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Consume item";
			}
			return;
		}
		if (this.m_circleTargetInterval > 0f && this.m_targetCreature)
		{
			this.m_pauseTimer += dt;
			if (this.m_pauseTimer > this.m_circleTargetInterval)
			{
				if (this.m_pauseTimer > this.m_circleTargetInterval + this.m_circleTargetDuration)
				{
					this.m_pauseTimer = UnityEngine.Random.Range(0f, this.m_circleTargetInterval / 10f);
				}
				base.RandomMovementArroundPoint(dt, this.m_targetCreature.transform.position, this.m_circleTargetDistance, base.IsAlerted());
				if (this.m_aiStatus != null)
				{
					this.m_aiStatus = "Attack pause";
				}
				return;
			}
		}
		ItemDrop.ItemData itemData = this.SelectBestAttack(humanoid, dt);
		bool flag3 = itemData != null && Time.time - itemData.m_lastAttackTime > itemData.m_shared.m_aiAttackInterval && this.m_character.GetTimeSinceLastAttack() >= this.m_minAttackInterval && !base.IsTakingOff();
		if ((this.m_character.IsFlying() ? this.m_circulateWhileChargingFlying : this.m_circulateWhileCharging) && (this.m_targetStatic != null || this.m_targetCreature != null) && itemData != null && !flag3 && !this.m_character.InAttack())
		{
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Move around target weapon ready:" + flag3.ToString();
			}
			if (itemData != null && this.m_aiStatus != null)
			{
				this.m_aiStatus = this.m_aiStatus + " Weapon:" + itemData.m_shared.m_name;
			}
			Vector3 point = this.m_targetCreature ? this.m_targetCreature.transform.position : this.m_targetStatic.transform.position;
			base.RandomMovementArroundPoint(dt, point, this.m_randomMoveRange, base.IsAlerted());
			return;
		}
		if ((this.m_targetStatic == null && this.m_targetCreature == null) || itemData == null)
		{
			if (this.m_follow)
			{
				base.Follow(this.m_follow, dt);
				if (this.m_aiStatus != null)
				{
					this.m_aiStatus = "Follow";
					return;
				}
			}
			else
			{
				if (this.m_aiStatus != null)
				{
					string[] array = new string[7];
					array[0] = "Random movement (weapon: ";
					array[1] = ((itemData != null) ? itemData.m_shared.m_name : "none");
					array[2] = ") (targetpiece: ";
					int num = 3;
					StaticTarget targetStatic = this.m_targetStatic;
					array[num] = ((targetStatic != null) ? targetStatic.ToString() : null);
					array[4] = ") (target: ";
					array[5] = (this.m_targetCreature ? this.m_targetCreature.gameObject.name : "none");
					array[6] = ")";
					this.m_aiStatus = string.Concat(array);
				}
				base.IdleMovement(dt);
			}
			return;
		}
		if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Enemy)
		{
			if (this.m_targetStatic)
			{
				Vector3 vector = this.m_targetStatic.FindClosestPoint(base.transform.position);
				if (Vector3.Distance(vector, base.transform.position) >= itemData.m_shared.m_aiAttackRange || !base.CanSeeTarget(this.m_targetStatic))
				{
					if (this.m_aiStatus != null)
					{
						this.m_aiStatus = "Move to static target";
					}
					base.MoveTo(dt, vector, 0f, base.IsAlerted());
					return;
				}
				base.LookAt(this.m_targetStatic.GetCenter());
				if (base.IsLookingAt(this.m_targetStatic.GetCenter(), itemData.m_shared.m_aiAttackMaxAngle) && flag3)
				{
					if (this.m_aiStatus != null)
					{
						this.m_aiStatus = "Attacking piece";
					}
					this.DoAttack(null, false);
					return;
				}
				base.StopMoving();
				return;
			}
			else if (this.m_targetCreature)
			{
				if (flag || flag2 || (this.HuntPlayer() && this.m_targetCreature.IsPlayer()))
				{
					this.m_beenAtLastPos = false;
					this.m_lastKnownTargetPos = this.m_targetCreature.transform.position;
					float num2 = Vector3.Distance(this.m_lastKnownTargetPos, base.transform.position) - this.m_targetCreature.GetRadius();
					float num3 = this.m_alertRange * this.m_targetCreature.GetStealthFactor();
					if (flag2 && num2 < num3)
					{
						this.SetAlerted(true);
					}
					bool flag4 = num2 < itemData.m_shared.m_aiAttackRange;
					if (!flag4 || !flag2 || itemData.m_shared.m_aiAttackRangeMin < 0f || !base.IsAlerted())
					{
						if (this.m_aiStatus != null)
						{
							this.m_aiStatus = "Move closer";
						}
						Vector3 velocity = this.m_targetCreature.GetVelocity();
						Vector3 vector2 = velocity * this.m_interceptTime;
						Vector3 vector3 = this.m_lastKnownTargetPos;
						if (num2 > vector2.magnitude / 4f)
						{
							vector3 += velocity * this.m_interceptTime;
						}
						base.MoveTo(dt, vector3, 0f, base.IsAlerted());
						if (this.m_timeSinceAttacking > 15f)
						{
							this.m_unableToAttackTargetTimer = 15f;
						}
					}
					else
					{
						base.StopMoving();
					}
					if (flag4 && flag2 && base.IsAlerted())
					{
						if (this.m_aiStatus != null)
						{
							this.m_aiStatus = "In attack range";
						}
						base.LookAt(this.m_targetCreature.GetTopPoint());
						if (flag3 && base.IsLookingAt(this.m_lastKnownTargetPos, itemData.m_shared.m_aiAttackMaxAngle))
						{
							if (this.m_aiStatus != null)
							{
								this.m_aiStatus = "Attacking creature";
							}
							this.DoAttack(this.m_targetCreature, false);
							return;
						}
					}
				}
				else
				{
					if (this.m_aiStatus != null)
					{
						this.m_aiStatus = "Searching for target";
					}
					if (this.m_beenAtLastPos)
					{
						base.RandomMovement(dt, this.m_lastKnownTargetPos, false);
						if (this.m_timeSinceAttacking > 15f)
						{
							this.m_unableToAttackTargetTimer = 15f;
							return;
						}
					}
					else if (base.MoveTo(dt, this.m_lastKnownTargetPos, 0f, base.IsAlerted()))
					{
						this.m_beenAtLastPos = true;
						return;
					}
				}
			}
		}
		else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt || itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Friend)
		{
			if (this.m_aiStatus != null)
			{
				this.m_aiStatus = "Helping friend";
			}
			Character character = (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt) ? base.HaveHurtFriendInRange(this.m_viewRange) : base.HaveFriendInRange(this.m_viewRange);
			if (character)
			{
				if (Vector3.Distance(character.transform.position, base.transform.position) >= itemData.m_shared.m_aiAttackRange)
				{
					base.MoveTo(dt, character.transform.position, 0f, base.IsAlerted());
					return;
				}
				if (flag3)
				{
					base.StopMoving();
					base.LookAt(character.transform.position);
					this.DoAttack(character, true);
					return;
				}
				base.RandomMovement(dt, character.transform.position, false);
				return;
			}
			else
			{
				base.RandomMovement(dt, base.transform.position, true);
			}
		}
	}

	// Token: 0x0600048C RID: 1164 RVA: 0x00025CB4 File Offset: 0x00023EB4
	private bool UpdateConsumeItem(Humanoid humanoid, float dt)
	{
		if (this.m_consumeItems == null || this.m_consumeItems.Count == 0)
		{
			return false;
		}
		this.m_consumeSearchTimer += dt;
		if (this.m_consumeSearchTimer > this.m_consumeSearchInterval)
		{
			this.m_consumeSearchTimer = 0f;
			if (this.m_tamable && !this.m_tamable.IsHungry())
			{
				return false;
			}
			this.m_consumeTarget = this.FindClosestConsumableItem(this.m_consumeSearchRange);
		}
		if (this.m_consumeTarget)
		{
			if (base.MoveTo(dt, this.m_consumeTarget.transform.position, this.m_consumeRange, false))
			{
				base.LookAt(this.m_consumeTarget.transform.position);
				if (base.IsLookingAt(this.m_consumeTarget.transform.position, 20f) && this.m_consumeTarget.RemoveOne())
				{
					if (this.m_onConsumedItem != null)
					{
						this.m_onConsumedItem(this.m_consumeTarget);
					}
					humanoid.m_consumeItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
					this.m_animator.SetTrigger("consume");
					this.m_consumeTarget = null;
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x0600048D RID: 1165 RVA: 0x00025DF8 File Offset: 0x00023FF8
	private ItemDrop FindClosestConsumableItem(float maxRange)
	{
		if (MonsterAI.m_itemMask == 0)
		{
			MonsterAI.m_itemMask = LayerMask.GetMask(new string[]
			{
				"item"
			});
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, maxRange, MonsterAI.m_itemMask);
		ItemDrop itemDrop = null;
		float num = 999999f;
		foreach (Collider collider in array)
		{
			if (collider.attachedRigidbody)
			{
				ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
				if (!(component == null) && component.GetComponent<ZNetView>().IsValid() && this.CanConsume(component.m_itemData))
				{
					float num2 = Vector3.Distance(component.transform.position, base.transform.position);
					if (itemDrop == null || num2 < num)
					{
						itemDrop = component;
						num = num2;
					}
				}
			}
		}
		if (itemDrop && base.HavePath(itemDrop.transform.position))
		{
			return itemDrop;
		}
		return null;
	}

	// Token: 0x0600048E RID: 1166 RVA: 0x00025EEC File Offset: 0x000240EC
	private bool CanConsume(ItemDrop.ItemData item)
	{
		using (List<ItemDrop>.Enumerator enumerator = this.m_consumeItems.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_itemData.m_shared.m_name == item.m_shared.m_name)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600048F RID: 1167 RVA: 0x00025F60 File Offset: 0x00024160
	private ItemDrop.ItemData SelectBestAttack(Humanoid humanoid, float dt)
	{
		if (this.m_targetCreature || this.m_targetStatic)
		{
			this.m_updateWeaponTimer -= dt;
			if (this.m_updateWeaponTimer <= 0f && !this.m_character.InAttack())
			{
				this.m_updateWeaponTimer = 1f;
				Character hurtFriend;
				Character friend;
				base.HaveFriendsInRange(this.m_viewRange, out hurtFriend, out friend);
				humanoid.EquipBestWeapon(this.m_targetCreature, this.m_targetStatic, hurtFriend, friend);
			}
		}
		return humanoid.GetCurrentWeapon();
	}

	// Token: 0x06000490 RID: 1168 RVA: 0x00025FE4 File Offset: 0x000241E4
	private bool DoAttack(Character target, bool isFriend)
	{
		ItemDrop.ItemData currentWeapon = (this.m_character as Humanoid).GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return false;
		}
		if (!base.CanUseAttack(currentWeapon))
		{
			return false;
		}
		bool flag = this.m_character.StartAttack(target, false);
		if (flag)
		{
			this.m_timeSinceAttacking = 0f;
		}
		return flag;
	}

	// Token: 0x06000491 RID: 1169 RVA: 0x0002602D File Offset: 0x0002422D
	public void SetDespawnInDay(bool despawn)
	{
		this.m_despawnInDay = despawn;
		this.m_nview.GetZDO().Set(ZDOVars.s_despawnInDay, despawn);
	}

	// Token: 0x06000492 RID: 1170 RVA: 0x0002604C File Offset: 0x0002424C
	public bool DespawnInDay()
	{
		if (Time.time - this.m_lastDespawnInDayCheck > 4f)
		{
			this.m_lastDespawnInDayCheck = Time.time;
			this.m_despawnInDay = this.m_nview.GetZDO().GetBool(ZDOVars.s_despawnInDay, this.m_despawnInDay);
		}
		return this.m_despawnInDay;
	}

	// Token: 0x06000493 RID: 1171 RVA: 0x0002609E File Offset: 0x0002429E
	public void SetEventCreature(bool despawn)
	{
		this.m_eventCreature = despawn;
		this.m_nview.GetZDO().Set(ZDOVars.s_eventCreature, despawn);
	}

	// Token: 0x06000494 RID: 1172 RVA: 0x000260C0 File Offset: 0x000242C0
	public bool IsEventCreature()
	{
		if (Time.time - this.m_lastEventCreatureCheck > 4f)
		{
			this.m_lastEventCreatureCheck = Time.time;
			this.m_eventCreature = this.m_nview.GetZDO().GetBool(ZDOVars.s_eventCreature, this.m_eventCreature);
		}
		return this.m_eventCreature;
	}

	// Token: 0x06000495 RID: 1173 RVA: 0x00026112 File Offset: 0x00024312
	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
	}

	// Token: 0x06000496 RID: 1174 RVA: 0x0002611A File Offset: 0x0002431A
	public override Character GetTargetCreature()
	{
		return this.m_targetCreature;
	}

	// Token: 0x06000497 RID: 1175 RVA: 0x00026122 File Offset: 0x00024322
	public StaticTarget GetStaticTarget()
	{
		return this.m_targetStatic;
	}

	// Token: 0x06000498 RID: 1176 RVA: 0x0002612C File Offset: 0x0002432C
	private void UpdateSleep(float dt)
	{
		if (!this.IsSleeping())
		{
			return;
		}
		this.m_sleepTimer += dt;
		if (this.m_sleepTimer < this.m_sleepDelay)
		{
			return;
		}
		if (this.HuntPlayer())
		{
			this.Wakeup();
			return;
		}
		if (this.m_wakeupRange > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_wakeupRange);
			if (closestPlayer && !closestPlayer.InGhostMode() && !closestPlayer.IsDebugFlying())
			{
				this.Wakeup();
				return;
			}
		}
		if (this.m_noiseWakeup)
		{
			Player playerNoiseRange = Player.GetPlayerNoiseRange(base.transform.position, this.m_maxNoiseWakeupRange);
			if (playerNoiseRange && !playerNoiseRange.InGhostMode() && !playerNoiseRange.IsDebugFlying())
			{
				this.Wakeup();
				return;
			}
		}
	}

	// Token: 0x06000499 RID: 1177 RVA: 0x000261F0 File Offset: 0x000243F0
	public void OnPrivateAreaAttacked(Character attacker, bool destroyed)
	{
		if (attacker.IsPlayer() && base.IsAggravatable() && !base.IsAggravated())
		{
			this.m_privateAreaAttacks++;
			if (this.m_privateAreaAttacks > this.m_privateAreaTriggerTreshold || destroyed)
			{
				base.SetAggravated(true, BaseAI.AggravatedReason.Damage);
			}
		}
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x00026240 File Offset: 0x00024440
	private void Wakeup()
	{
		if (!this.IsSleeping())
		{
			return;
		}
		this.m_animator.SetBool(MonsterAI.s_sleeping, false);
		this.m_nview.GetZDO().Set(ZDOVars.s_sleeping, false);
		this.m_wakeupEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x0600049B RID: 1179 RVA: 0x000262A6 File Offset: 0x000244A6
	public override bool IsSleeping()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(ZDOVars.s_sleeping, this.m_sleeping);
	}

	// Token: 0x0600049C RID: 1180 RVA: 0x000262D2 File Offset: 0x000244D2
	protected override void SetAlerted(bool alert)
	{
		if (alert)
		{
			this.m_timeSinceSensedTargetCreature = 0f;
		}
		base.SetAlerted(alert);
	}

	// Token: 0x0600049D RID: 1181 RVA: 0x000262E9 File Offset: 0x000244E9
	public override bool HuntPlayer()
	{
		return base.HuntPlayer() && (!this.IsEventCreature() || RandEventSystem.InEvent()) && (!this.DespawnInDay() || !EnvMan.instance.IsDay());
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x0002631D File Offset: 0x0002451D
	public GameObject GetFollowTarget()
	{
		return this.m_follow;
	}

	// Token: 0x0600049F RID: 1183 RVA: 0x00026325 File Offset: 0x00024525
	public void SetFollowTarget(GameObject go)
	{
		this.m_follow = go;
	}

	// Token: 0x1700000D RID: 13
	// (get) Token: 0x060004A0 RID: 1184 RVA: 0x0002632E File Offset: 0x0002452E
	public new static List<MonsterAI> Instances { get; } = new List<MonsterAI>();

	// Token: 0x0400052E RID: 1326
	private float m_lastDespawnInDayCheck = -9999f;

	// Token: 0x0400052F RID: 1327
	private float m_lastEventCreatureCheck = -9999f;

	// Token: 0x04000530 RID: 1328
	public Action<ItemDrop> m_onConsumedItem;

	// Token: 0x04000531 RID: 1329
	private const float m_giveUpTime = 30f;

	// Token: 0x04000532 RID: 1330
	private const float m_updateTargetFarRange = 50f;

	// Token: 0x04000533 RID: 1331
	private const float m_updateTargetIntervalNear = 2f;

	// Token: 0x04000534 RID: 1332
	private const float m_updateTargetIntervalFar = 6f;

	// Token: 0x04000535 RID: 1333
	private const float m_updateWeaponInterval = 1f;

	// Token: 0x04000536 RID: 1334
	private const float m_unableToAttackTargetDuration = 15f;

	// Token: 0x04000537 RID: 1335
	[Header("Monster AI")]
	public float m_alertRange = 9999f;

	// Token: 0x04000538 RID: 1336
	public bool m_fleeIfHurtWhenTargetCantBeReached = true;

	// Token: 0x04000539 RID: 1337
	public bool m_fleeIfNotAlerted;

	// Token: 0x0400053A RID: 1338
	public float m_fleeIfLowHealth;

	// Token: 0x0400053B RID: 1339
	public bool m_circulateWhileCharging;

	// Token: 0x0400053C RID: 1340
	public bool m_circulateWhileChargingFlying;

	// Token: 0x0400053D RID: 1341
	public bool m_enableHuntPlayer;

	// Token: 0x0400053E RID: 1342
	public bool m_attackPlayerObjects = true;

	// Token: 0x0400053F RID: 1343
	public int m_privateAreaTriggerTreshold = 4;

	// Token: 0x04000540 RID: 1344
	public float m_interceptTimeMax;

	// Token: 0x04000541 RID: 1345
	public float m_interceptTimeMin;

	// Token: 0x04000542 RID: 1346
	public float m_maxChaseDistance;

	// Token: 0x04000543 RID: 1347
	public float m_minAttackInterval;

	// Token: 0x04000544 RID: 1348
	[Header("Circle target")]
	public float m_circleTargetInterval;

	// Token: 0x04000545 RID: 1349
	public float m_circleTargetDuration = 5f;

	// Token: 0x04000546 RID: 1350
	public float m_circleTargetDistance = 10f;

	// Token: 0x04000547 RID: 1351
	[Header("Sleep")]
	public bool m_sleeping;

	// Token: 0x04000548 RID: 1352
	public float m_wakeupRange = 5f;

	// Token: 0x04000549 RID: 1353
	public bool m_noiseWakeup;

	// Token: 0x0400054A RID: 1354
	public float m_maxNoiseWakeupRange = 50f;

	// Token: 0x0400054B RID: 1355
	public EffectList m_wakeupEffects = new EffectList();

	// Token: 0x0400054C RID: 1356
	public float m_wakeUpDelayMin;

	// Token: 0x0400054D RID: 1357
	public float m_wakeUpDelayMax;

	// Token: 0x0400054E RID: 1358
	[Header("Other")]
	public bool m_avoidLand;

	// Token: 0x0400054F RID: 1359
	[Header("Consume items")]
	public List<ItemDrop> m_consumeItems;

	// Token: 0x04000550 RID: 1360
	public float m_consumeRange = 2f;

	// Token: 0x04000551 RID: 1361
	public float m_consumeSearchRange = 5f;

	// Token: 0x04000552 RID: 1362
	public float m_consumeSearchInterval = 10f;

	// Token: 0x04000553 RID: 1363
	private ItemDrop m_consumeTarget;

	// Token: 0x04000554 RID: 1364
	private float m_consumeSearchTimer;

	// Token: 0x04000555 RID: 1365
	private static int m_itemMask = 0;

	// Token: 0x04000556 RID: 1366
	private string m_aiStatus;

	// Token: 0x04000557 RID: 1367
	private bool m_despawnInDay;

	// Token: 0x04000558 RID: 1368
	private bool m_eventCreature;

	// Token: 0x04000559 RID: 1369
	private Character m_targetCreature;

	// Token: 0x0400055A RID: 1370
	private Vector3 m_lastKnownTargetPos = Vector3.zero;

	// Token: 0x0400055B RID: 1371
	private bool m_beenAtLastPos;

	// Token: 0x0400055C RID: 1372
	private StaticTarget m_targetStatic;

	// Token: 0x0400055D RID: 1373
	private float m_timeSinceAttacking;

	// Token: 0x0400055E RID: 1374
	private float m_timeSinceSensedTargetCreature;

	// Token: 0x0400055F RID: 1375
	private float m_updateTargetTimer;

	// Token: 0x04000560 RID: 1376
	private float m_updateWeaponTimer;

	// Token: 0x04000561 RID: 1377
	private float m_interceptTime;

	// Token: 0x04000562 RID: 1378
	private float m_sleepDelay = 0.5f;

	// Token: 0x04000563 RID: 1379
	private float m_pauseTimer;

	// Token: 0x04000564 RID: 1380
	private float m_sleepTimer;

	// Token: 0x04000565 RID: 1381
	private float m_unableToAttackTargetTimer;

	// Token: 0x04000566 RID: 1382
	private GameObject m_follow;

	// Token: 0x04000567 RID: 1383
	private int m_privateAreaAttacks;

	// Token: 0x04000568 RID: 1384
	private static readonly int s_sleeping = ZSyncAnimation.GetHash("sleeping");
}
