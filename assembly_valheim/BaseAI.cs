using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000051 RID: 81
public class BaseAI : MonoBehaviour
{
	// Token: 0x06000427 RID: 1063 RVA: 0x00021C38 File Offset: 0x0001FE38
	protected virtual void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_character = base.GetComponent<Character>();
		this.m_animator = base.GetComponent<ZSyncAnimation>();
		base.GetComponent<Rigidbody>();
		this.m_tamable = base.GetComponent<Tameable>();
		if (BaseAI.m_solidRayMask == 0)
		{
			BaseAI.m_solidRayMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain",
				"vehicle"
			});
			BaseAI.m_viewBlockMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain",
				"viewblock",
				"vehicle"
			});
			BaseAI.m_monsterTargetRayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"piece_nonsolid",
				"Default",
				"static_solid",
				"Default_small",
				"vehicle"
			});
		}
		Character character = this.m_character;
		character.m_onDamaged = (Action<float, Character>)Delegate.Combine(character.m_onDamaged, new Action<float, Character>(this.OnDamaged));
		Character character2 = this.m_character;
		character2.m_onDeath = (Action)Delegate.Combine(character2.m_onDeath, new Action(this.OnDeath));
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
			if (!string.IsNullOrEmpty(this.m_spawnMessage))
			{
				MessageHud.instance.MessageAll(MessageHud.MessageType.Center, this.m_spawnMessage);
			}
		}
		this.m_randomMoveUpdateTimer = UnityEngine.Random.Range(0f, this.m_randomMoveInterval);
		this.m_nview.Register("Alert", new Action<long>(this.RPC_Alert));
		this.m_nview.Register<Vector3, float, ZDOID>("OnNearProjectileHit", new Action<long, Vector3, float, ZDOID>(this.RPC_OnNearProjectileHit));
		this.m_nview.Register<bool, int>("SetAggravated", new Action<long, bool, int>(this.RPC_SetAggravated));
		this.m_huntPlayer = this.m_nview.GetZDO().GetBool(ZDOVars.s_huntPlayer, this.m_huntPlayer);
		this.m_spawnPoint = this.m_nview.GetZDO().GetVec3(ZDOVars.s_spawnPoint, base.transform.position);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnPoint, this.m_spawnPoint);
		}
		base.InvokeRepeating("DoIdleSound", this.m_idleSoundInterval, this.m_idleSoundInterval);
	}

	// Token: 0x06000428 RID: 1064 RVA: 0x00021EFA File Offset: 0x000200FA
	protected virtual void OnEnable()
	{
		BaseAI.Instances.Add(this);
	}

	// Token: 0x06000429 RID: 1065 RVA: 0x00021F07 File Offset: 0x00020107
	protected virtual void OnDisable()
	{
		BaseAI.Instances.Remove(this);
	}

	// Token: 0x0600042A RID: 1066 RVA: 0x00021F15 File Offset: 0x00020115
	public void SetPatrolPoint()
	{
		this.SetPatrolPoint(base.transform.position);
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x00021F28 File Offset: 0x00020128
	private void SetPatrolPoint(Vector3 point)
	{
		this.m_patrol = true;
		this.m_patrolPoint = point;
		this.m_nview.GetZDO().Set(ZDOVars.s_patrolPoint, point);
		this.m_nview.GetZDO().Set(ZDOVars.s_patrol, true);
	}

	// Token: 0x0600042C RID: 1068 RVA: 0x00021F64 File Offset: 0x00020164
	public void ResetPatrolPoint()
	{
		this.m_patrol = false;
		this.m_nview.GetZDO().Set(ZDOVars.s_patrol, false);
	}

	// Token: 0x0600042D RID: 1069 RVA: 0x00021F84 File Offset: 0x00020184
	protected bool GetPatrolPoint(out Vector3 point)
	{
		if (Time.time - this.m_patrolPointUpdateTime > 1f)
		{
			this.m_patrolPointUpdateTime = Time.time;
			this.m_patrol = this.m_nview.GetZDO().GetBool(ZDOVars.s_patrol, false);
			if (this.m_patrol)
			{
				this.m_patrolPoint = this.m_nview.GetZDO().GetVec3(ZDOVars.s_patrolPoint, this.m_patrolPoint);
			}
		}
		point = this.m_patrolPoint;
		return this.m_patrol;
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x00022008 File Offset: 0x00020208
	public void UpdateAI(float dt)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			this.m_alerted = this.m_nview.GetZDO().GetBool(ZDOVars.s_alert, false);
			return;
		}
		this.UpdateTakeoffLanding(dt);
		if (this.m_jumpInterval > 0f)
		{
			this.m_jumpTimer += dt;
		}
		if (this.m_randomMoveUpdateTimer > 0f)
		{
			this.m_randomMoveUpdateTimer -= dt;
		}
		this.UpdateRegeneration(dt);
		this.m_timeSinceHurt += dt;
	}

	// Token: 0x0600042F RID: 1071 RVA: 0x000220A0 File Offset: 0x000202A0
	private void UpdateRegeneration(float dt)
	{
		this.m_regenTimer += dt;
		if (this.m_regenTimer <= 2f)
		{
			return;
		}
		this.m_regenTimer = 0f;
		if (this.m_tamable && this.m_character.IsTamed() && this.m_tamable.IsHungry())
		{
			return;
		}
		float worldTimeDelta = this.GetWorldTimeDelta();
		float num = this.m_character.GetMaxHealth() / 3600f;
		this.m_character.Heal(num * worldTimeDelta, this.m_tamable && this.m_character.IsTamed());
	}

	// Token: 0x06000430 RID: 1072 RVA: 0x0002213E File Offset: 0x0002033E
	protected bool IsTakingOff()
	{
		return this.m_randomFly && this.m_character.IsFlying() && this.m_randomFlyTimer < this.m_takeoffTime;
	}

	// Token: 0x06000431 RID: 1073 RVA: 0x00022168 File Offset: 0x00020368
	private void UpdateTakeoffLanding(float dt)
	{
		if (!this.m_randomFly)
		{
			return;
		}
		this.m_randomFlyTimer += dt;
		if (this.m_character.InAttack() || this.m_character.IsStaggering())
		{
			return;
		}
		if (this.m_character.IsFlying())
		{
			if (this.m_randomFlyTimer > this.m_airDuration && this.GetAltitude() < this.m_maxLandAltitude)
			{
				this.m_randomFlyTimer = 0f;
				if (UnityEngine.Random.value <= this.m_chanceToLand)
				{
					this.m_character.Land();
					return;
				}
			}
		}
		else if (this.m_randomFlyTimer > this.m_groundDuration)
		{
			this.m_randomFlyTimer = 0f;
			if (UnityEngine.Random.value <= this.m_chanceToTakeoff)
			{
				this.m_character.TakeOff();
			}
		}
	}

	// Token: 0x06000432 RID: 1074 RVA: 0x00022228 File Offset: 0x00020428
	private float GetWorldTimeDelta()
	{
		DateTime time = ZNet.instance.GetTime();
		long @long = this.m_nview.GetZDO().GetLong(ZDOVars.s_worldTimeHash, 0L);
		if (@long == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_worldTimeHash, time.Ticks);
			return 0f;
		}
		DateTime d = new DateTime(@long);
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set(ZDOVars.s_worldTimeHash, time.Ticks);
		return (float)timeSpan.TotalSeconds;
	}

	// Token: 0x06000433 RID: 1075 RVA: 0x000222B4 File Offset: 0x000204B4
	public TimeSpan GetTimeSinceSpawned()
	{
		if (!this.m_nview || !this.m_nview.IsValid())
		{
			return TimeSpan.Zero;
		}
		long num = this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L);
		if (num == 0L)
		{
			num = ZNet.instance.GetTime().Ticks;
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, num);
		}
		DateTime d = new DateTime(num);
		return ZNet.instance.GetTime() - d;
	}

	// Token: 0x06000434 RID: 1076 RVA: 0x0002233D File Offset: 0x0002053D
	private void DoIdleSound()
	{
		if (this.IsSleeping())
		{
			return;
		}
		if (UnityEngine.Random.value > this.m_idleSoundChance)
		{
			return;
		}
		this.m_idleSound.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x06000435 RID: 1077 RVA: 0x0002237C File Offset: 0x0002057C
	protected void Follow(GameObject go, float dt)
	{
		float num = Vector3.Distance(go.transform.position, base.transform.position);
		bool run = num > 10f;
		if (num < 3f)
		{
			this.StopMoving();
			return;
		}
		this.MoveTo(dt, go.transform.position, 0f, run);
	}

	// Token: 0x06000436 RID: 1078 RVA: 0x000223D4 File Offset: 0x000205D4
	protected void MoveToWater(float dt, float maxRange)
	{
		float num = this.m_haveWaterPosition ? 2f : 0.5f;
		if (Time.time - this.m_lastMoveToWaterUpdate > num)
		{
			this.m_lastMoveToWaterUpdate = Time.time;
			Vector3 vector = base.transform.position;
			for (int i = 0; i < 10; i++)
			{
				Vector3 b = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(4f, maxRange);
				Vector3 vector2 = base.transform.position + b;
				vector2.y = ZoneSystem.instance.GetSolidHeight(vector2);
				if (vector2.y < vector.y)
				{
					vector = vector2;
				}
			}
			if (vector.y < ZoneSystem.instance.m_waterLevel)
			{
				this.m_moveToWaterPosition = vector;
				this.m_haveWaterPosition = true;
			}
			else
			{
				this.m_haveWaterPosition = false;
			}
		}
		if (this.m_haveWaterPosition)
		{
			this.MoveTowards(this.m_moveToWaterPosition - base.transform.position, true);
		}
	}

	// Token: 0x06000437 RID: 1079 RVA: 0x000224E8 File Offset: 0x000206E8
	protected void MoveAwayAndDespawn(float dt, bool run)
	{
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 40f);
		if (closestPlayer != null)
		{
			Vector3 normalized = (closestPlayer.transform.position - base.transform.position).normalized;
			this.MoveTo(dt, base.transform.position - normalized * 5f, 0f, run);
			return;
		}
		this.m_nview.Destroy();
	}

	// Token: 0x06000438 RID: 1080 RVA: 0x00022570 File Offset: 0x00020770
	protected void IdleMovement(float dt)
	{
		Vector3 centerPoint = (this.m_character.IsTamed() || this.HuntPlayer()) ? base.transform.position : this.m_spawnPoint;
		Vector3 vector;
		if (this.GetPatrolPoint(out vector))
		{
			centerPoint = vector;
		}
		this.RandomMovement(dt, centerPoint, true);
	}

	// Token: 0x06000439 RID: 1081 RVA: 0x000225BC File Offset: 0x000207BC
	protected void RandomMovement(float dt, Vector3 centerPoint, bool snapToGround = false)
	{
		if (this.m_randomMoveUpdateTimer <= 0f)
		{
			float y;
			if (snapToGround && ZoneSystem.instance.GetSolidHeight(this.m_randomMoveTarget, out y, 1000))
			{
				centerPoint.y = y;
			}
			if (Utils.DistanceXZ(centerPoint, base.transform.position) > this.m_randomMoveRange * 2f)
			{
				Vector3 vector = centerPoint - base.transform.position;
				vector.y = 0f;
				vector.Normalize();
				vector = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(-30, 30), 0f) * vector;
				this.m_randomMoveTarget = base.transform.position + vector * this.m_randomMoveRange * 2f;
			}
			else
			{
				Vector3 b = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * base.transform.forward * UnityEngine.Random.Range(this.m_randomMoveRange * 0.7f, this.m_randomMoveRange);
				this.m_randomMoveTarget = centerPoint + b;
			}
			if (this.m_character.IsFlying())
			{
				this.m_randomMoveTarget.y = this.m_randomMoveTarget.y + UnityEngine.Random.Range(this.m_flyAltitudeMin, this.m_flyAltitudeMax);
			}
			if (!this.IsValidRandomMovePoint(this.m_randomMoveTarget))
			{
				return;
			}
			this.m_reachedRandomMoveTarget = false;
			this.m_randomMoveUpdateTimer = UnityEngine.Random.Range(this.m_randomMoveInterval, this.m_randomMoveInterval + this.m_randomMoveInterval / 2f);
			if (this.m_avoidWater && this.m_character.IsSwimming())
			{
				this.m_randomMoveUpdateTimer /= 4f;
			}
		}
		if (!this.m_reachedRandomMoveTarget)
		{
			bool flag = this.IsAlerted() || Utils.DistanceXZ(base.transform.position, centerPoint) > this.m_randomMoveRange * 2f;
			if (this.MoveTo(dt, this.m_randomMoveTarget, 0f, flag))
			{
				this.m_reachedRandomMoveTarget = true;
				if (flag)
				{
					this.m_randomMoveUpdateTimer = 0f;
					return;
				}
			}
		}
		else
		{
			this.StopMoving();
		}
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x000227DE File Offset: 0x000209DE
	public void ResetRandomMovement()
	{
		this.m_reachedRandomMoveTarget = true;
		this.m_randomMoveUpdateTimer = UnityEngine.Random.Range(this.m_randomMoveInterval, this.m_randomMoveInterval + this.m_randomMoveInterval / 2f);
	}

	// Token: 0x0600043B RID: 1083 RVA: 0x0002280C File Offset: 0x00020A0C
	protected bool Flee(float dt, Vector3 from)
	{
		float time = Time.time;
		if (time - this.m_fleeTargetUpdateTime > 2f)
		{
			this.m_fleeTargetUpdateTime = time;
			Vector3 point = -(from - base.transform.position);
			point.y = 0f;
			point.Normalize();
			bool flag = false;
			for (int i = 0; i < 4; i++)
			{
				this.m_fleeTarget = base.transform.position + Quaternion.Euler(0f, UnityEngine.Random.Range(-45f, 45f), 0f) * point * 25f;
				if (this.HavePath(this.m_fleeTarget) && (!this.m_avoidWater || this.m_character.IsSwimming() || ZoneSystem.instance.GetSolidHeight(this.m_fleeTarget) >= ZoneSystem.instance.m_waterLevel))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.m_fleeTarget = base.transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * 25f;
			}
		}
		return this.MoveTo(dt, this.m_fleeTarget, 1f, this.IsAlerted());
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x00022960 File Offset: 0x00020B60
	protected bool AvoidFire(float dt, Character moveToTarget, bool superAfraid)
	{
		if (this.m_character.IsTamed())
		{
			return false;
		}
		if (superAfraid)
		{
			EffectArea effectArea = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Fire, 3f);
			if (effectArea)
			{
				this.m_nearFireTime = Time.time;
				this.m_nearFireArea = effectArea;
			}
			if (Time.time - this.m_nearFireTime < 6f && this.m_nearFireArea)
			{
				this.SetAlerted(true);
				this.Flee(dt, this.m_nearFireArea.transform.position);
				return true;
			}
		}
		else
		{
			EffectArea effectArea2 = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Fire, 3f);
			if (effectArea2)
			{
				if (moveToTarget != null && EffectArea.IsPointInsideArea(moveToTarget.transform.position, EffectArea.Type.Fire, 0f))
				{
					this.RandomMovementArroundPoint(dt, effectArea2.transform.position, effectArea2.GetRadius() + 3f + 1f, this.IsAlerted());
					return true;
				}
				this.RandomMovementArroundPoint(dt, effectArea2.transform.position, (effectArea2.GetRadius() + 3f) * 1.5f, this.IsAlerted());
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x00022A98 File Offset: 0x00020C98
	protected void RandomMovementArroundPoint(float dt, Vector3 point, float distance, bool run)
	{
		float time = Time.time;
		if (time - this.aroundPointUpdateTime > this.m_randomCircleInterval)
		{
			this.aroundPointUpdateTime = time;
			Vector3 point2 = base.transform.position - point;
			point2.y = 0f;
			point2.Normalize();
			float num;
			if (Vector3.Distance(base.transform.position, point) < distance / 2f)
			{
				num = (float)(((double)UnityEngine.Random.value > 0.5) ? 90 : -90);
			}
			else
			{
				num = (float)(((double)UnityEngine.Random.value > 0.5) ? 40 : -40);
			}
			Vector3 a = Quaternion.Euler(0f, num, 0f) * point2;
			this.arroundPointTarget = point + a * distance;
			if (Vector3.Dot(base.transform.forward, this.arroundPointTarget - base.transform.position) < 0f)
			{
				a = Quaternion.Euler(0f, -num, 0f) * point2;
				this.arroundPointTarget = point + a * distance;
				if (this.m_serpentMovement && Vector3.Distance(point, base.transform.position) > distance / 2f && Vector3.Dot(base.transform.forward, this.arroundPointTarget - base.transform.position) < 0f)
				{
					this.arroundPointTarget = point - a * distance;
				}
			}
			if (this.m_character.IsFlying())
			{
				this.arroundPointTarget.y = this.arroundPointTarget.y + UnityEngine.Random.Range(this.m_flyAltitudeMin, this.m_flyAltitudeMax);
			}
		}
		if (this.MoveTo(dt, this.arroundPointTarget, 0f, run))
		{
			if (run)
			{
				this.aroundPointUpdateTime = 0f;
			}
			if (!this.m_serpentMovement && !run)
			{
				this.LookAt(point);
			}
		}
	}

	// Token: 0x0600043E RID: 1086 RVA: 0x00022C8C File Offset: 0x00020E8C
	private bool GetSolidHeight(Vector3 p, float maxUp, float maxDown, out float height)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * maxUp, Vector3.down, out raycastHit, maxDown, BaseAI.m_solidRayMask))
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x0600043F RID: 1087 RVA: 0x00022CD8 File Offset: 0x00020ED8
	protected bool IsValidRandomMovePoint(Vector3 point)
	{
		if (this.m_character.IsFlying())
		{
			return true;
		}
		float num;
		if (this.m_avoidWater && this.GetSolidHeight(point, 20f, 100f, out num))
		{
			if (this.m_character.IsSwimming())
			{
				float num2;
				if (this.GetSolidHeight(base.transform.position, 20f, 100f, out num2) && num < num2)
				{
					return false;
				}
			}
			else if (num < ZoneSystem.instance.m_waterLevel)
			{
				return false;
			}
		}
		return (!this.m_afraidOfFire && !this.m_avoidFire) || !EffectArea.IsPointInsideArea(point, EffectArea.Type.Fire, 0f);
	}

	// Token: 0x06000440 RID: 1088 RVA: 0x00022D78 File Offset: 0x00020F78
	protected virtual void OnDamaged(float damage, Character attacker)
	{
		this.m_timeSinceHurt = 0f;
	}

	// Token: 0x06000441 RID: 1089 RVA: 0x00022D85 File Offset: 0x00020F85
	protected virtual void OnDeath()
	{
		if (!string.IsNullOrEmpty(this.m_deathMessage))
		{
			MessageHud.instance.MessageAll(MessageHud.MessageType.Center, this.m_deathMessage);
		}
	}

	// Token: 0x06000442 RID: 1090 RVA: 0x00022DA5 File Offset: 0x00020FA5
	public bool CanSenseTarget(Character target)
	{
		return BaseAI.CanSenseTarget(base.transform, this.m_character.m_eye.position, this.m_hearRange, this.m_viewRange, this.m_viewAngle, this.IsAlerted(), this.m_mistVision, target);
	}

	// Token: 0x06000443 RID: 1091 RVA: 0x00022DE1 File Offset: 0x00020FE1
	public static bool CanSenseTarget(Transform me, Vector3 eyePoint, float hearRange, float viewRange, float viewAngle, bool alerted, bool mistVision, Character target)
	{
		return BaseAI.CanHearTarget(me, hearRange, target) || BaseAI.CanSeeTarget(me, eyePoint, viewRange, viewAngle, alerted, mistVision, target);
	}

	// Token: 0x06000444 RID: 1092 RVA: 0x00022E05 File Offset: 0x00021005
	public bool CanHearTarget(Character target)
	{
		return BaseAI.CanHearTarget(base.transform, this.m_hearRange, target);
	}

	// Token: 0x06000445 RID: 1093 RVA: 0x00022E1C File Offset: 0x0002101C
	public static bool CanHearTarget(Transform me, float hearRange, Character target)
	{
		if (target.IsPlayer())
		{
			Player player = target as Player;
			if (player.InDebugFlyMode() || player.InGhostMode())
			{
				return false;
			}
		}
		float num = Vector3.Distance(target.transform.position, me.position);
		if (Character.InInterior(me))
		{
			hearRange = Mathf.Min(12f, hearRange);
		}
		return num <= hearRange && num < target.GetNoiseRange();
	}

	// Token: 0x06000446 RID: 1094 RVA: 0x00022E88 File Offset: 0x00021088
	public bool CanSeeTarget(Character target)
	{
		return BaseAI.CanSeeTarget(base.transform, this.m_character.m_eye.position, this.m_viewRange, this.m_viewAngle, this.IsAlerted(), this.m_mistVision, target);
	}

	// Token: 0x06000447 RID: 1095 RVA: 0x00022EC0 File Offset: 0x000210C0
	public static bool CanSeeTarget(Transform me, Vector3 eyePoint, float viewRange, float viewAngle, bool alerted, bool mistVision, Character target)
	{
		if (target.IsPlayer())
		{
			Player player = target as Player;
			if (player.InDebugFlyMode() || player.InGhostMode())
			{
				return false;
			}
		}
		float num = Vector3.Distance(target.transform.position, me.position);
		if (num > viewRange)
		{
			return false;
		}
		float num2 = num / viewRange;
		float stealthFactor = target.GetStealthFactor();
		float num3 = viewRange * stealthFactor;
		if (num > num3)
		{
			return false;
		}
		if (!alerted && Vector3.Angle(target.transform.position - me.position, me.forward) > viewAngle)
		{
			return false;
		}
		Vector3 vector = target.IsCrouching() ? target.GetCenterPoint() : target.m_eye.position;
		Vector3 vector2 = vector - eyePoint;
		return !Physics.Raycast(eyePoint, vector2.normalized, vector2.magnitude, BaseAI.m_viewBlockMask) && (mistVision || !ParticleMist.IsMistBlocked(eyePoint, vector));
	}

	// Token: 0x06000448 RID: 1096 RVA: 0x00022FA8 File Offset: 0x000211A8
	protected bool CanSeeTarget(StaticTarget target)
	{
		Vector3 center = target.GetCenter();
		if (Vector3.Distance(center, base.transform.position) > this.m_viewRange)
		{
			return false;
		}
		Vector3 rhs = center - this.m_character.m_eye.position;
		if (this.m_viewRange > 0f && !this.IsAlerted() && Vector3.Dot(base.transform.forward, rhs) < 0f)
		{
			return false;
		}
		List<Collider> allColliders = target.GetAllColliders();
		int num = Physics.RaycastNonAlloc(this.m_character.m_eye.position, rhs.normalized, BaseAI.s_tempRaycastHits, rhs.magnitude, BaseAI.m_viewBlockMask);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = BaseAI.s_tempRaycastHits[i];
			if (!allColliders.Contains(raycastHit.collider))
			{
				return false;
			}
		}
		return this.m_mistVision || !ParticleMist.IsMistBlocked(this.m_character.m_eye.position, center);
	}

	// Token: 0x06000449 RID: 1097 RVA: 0x000230A4 File Offset: 0x000212A4
	private void MoveTowardsSwoop(Vector3 dir, bool run, float distance)
	{
		dir = dir.normalized;
		float num = Mathf.Clamp01(Vector3.Dot(dir, this.m_character.transform.forward));
		num *= num;
		float num2 = Mathf.Clamp01(distance / this.m_serpentTurnRadius);
		float num3 = 1f - (1f - num2) * (1f - num);
		num3 = num3 * 0.9f + 0.1f;
		Vector3 moveDir = base.transform.forward * num3;
		this.LookTowards(dir);
		this.m_character.SetMoveDir(moveDir);
		this.m_character.SetRun(run);
	}

	// Token: 0x0600044A RID: 1098 RVA: 0x00023140 File Offset: 0x00021340
	public void MoveTowards(Vector3 dir, bool run)
	{
		dir = dir.normalized;
		this.LookTowards(dir);
		if (this.m_smoothMovement)
		{
			float num = Vector3.Angle(new Vector3(dir.x, 0f, dir.z), base.transform.forward);
			float d = 1f - Mathf.Clamp01(num / this.m_moveMinAngle);
			Vector3 moveDir = base.transform.forward * d;
			moveDir.y = dir.y;
			this.m_character.SetMoveDir(moveDir);
			this.m_character.SetRun(run);
			if (this.m_jumpInterval > 0f && this.m_jumpTimer >= this.m_jumpInterval)
			{
				this.m_jumpTimer = 0f;
				this.m_character.Jump(false);
				return;
			}
		}
		else if (this.IsLookingTowards(dir, this.m_moveMinAngle))
		{
			this.m_character.SetMoveDir(dir);
			this.m_character.SetRun(run);
			if (this.m_jumpInterval > 0f && this.m_jumpTimer >= this.m_jumpInterval)
			{
				this.m_jumpTimer = 0f;
				this.m_character.Jump(false);
				return;
			}
		}
		else
		{
			this.StopMoving();
		}
	}

	// Token: 0x0600044B RID: 1099 RVA: 0x00023270 File Offset: 0x00021470
	protected void LookAt(Vector3 point)
	{
		Vector3 vector = point - this.m_character.m_eye.position;
		if (Utils.LengthXZ(vector) < 0.01f)
		{
			return;
		}
		vector.Normalize();
		this.LookTowards(vector);
	}

	// Token: 0x0600044C RID: 1100 RVA: 0x000232B0 File Offset: 0x000214B0
	public void LookTowards(Vector3 dir)
	{
		this.m_character.SetLookDir(dir, 0f);
	}

	// Token: 0x0600044D RID: 1101 RVA: 0x000232C4 File Offset: 0x000214C4
	protected bool IsLookingAt(Vector3 point, float minAngle)
	{
		return this.IsLookingTowards((point - base.transform.position).normalized, minAngle);
	}

	// Token: 0x0600044E RID: 1102 RVA: 0x000232F4 File Offset: 0x000214F4
	public bool IsLookingTowards(Vector3 dir, float minAngle)
	{
		dir.y = 0f;
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		return Vector3.Angle(dir, forward) < minAngle;
	}

	// Token: 0x0600044F RID: 1103 RVA: 0x0002332F File Offset: 0x0002152F
	public void StopMoving()
	{
		this.m_character.SetMoveDir(Vector3.zero);
	}

	// Token: 0x06000450 RID: 1104 RVA: 0x00023341 File Offset: 0x00021541
	protected bool HavePath(Vector3 target)
	{
		return this.m_character.IsFlying() || Pathfinding.instance.HavePath(base.transform.position, target, this.m_pathAgentType);
	}

	// Token: 0x06000451 RID: 1105 RVA: 0x00023370 File Offset: 0x00021570
	protected bool FindPath(Vector3 target)
	{
		float time = Time.time;
		float num = time - this.m_lastFindPathTime;
		if (num < 1f)
		{
			return this.m_lastFindPathResult;
		}
		if (Vector3.Distance(target, this.m_lastFindPathTarget) < 1f && num < 5f)
		{
			return this.m_lastFindPathResult;
		}
		this.m_lastFindPathTarget = target;
		this.m_lastFindPathTime = time;
		this.m_lastFindPathResult = Pathfinding.instance.GetPath(base.transform.position, target, this.m_path, this.m_pathAgentType, false, true, false);
		return this.m_lastFindPathResult;
	}

	// Token: 0x06000452 RID: 1106 RVA: 0x000233FC File Offset: 0x000215FC
	protected bool FoundPath()
	{
		return this.m_lastFindPathResult;
	}

	// Token: 0x06000453 RID: 1107 RVA: 0x00023404 File Offset: 0x00021604
	protected bool MoveTo(float dt, Vector3 point, float dist, bool run)
	{
		if (this.m_character.m_flying)
		{
			dist = Mathf.Max(dist, 1f);
			float num;
			if (this.GetSolidHeight(point, 0f, this.m_flyAltitudeMin * 2f, out num))
			{
				point.y = Mathf.Max(point.y, num + this.m_flyAltitudeMin);
			}
			return this.MoveAndAvoid(dt, point, dist, run);
		}
		float num2 = run ? 1f : 0.5f;
		if (this.m_serpentMovement)
		{
			num2 = 3f;
		}
		if (Utils.DistanceXZ(point, base.transform.position) < Mathf.Max(dist, num2))
		{
			this.StopMoving();
			return true;
		}
		if (!this.FindPath(point))
		{
			this.StopMoving();
			return true;
		}
		if (this.m_path.Count == 0)
		{
			this.StopMoving();
			return true;
		}
		Vector3 vector = this.m_path[0];
		if (Utils.DistanceXZ(vector, base.transform.position) < num2)
		{
			this.m_path.RemoveAt(0);
			if (this.m_path.Count == 0)
			{
				this.StopMoving();
				return true;
			}
		}
		else if (this.m_serpentMovement)
		{
			float distance = Vector3.Distance(vector, base.transform.position);
			Vector3 normalized = (vector - base.transform.position).normalized;
			this.MoveTowardsSwoop(normalized, run, distance);
		}
		else
		{
			Vector3 normalized2 = (vector - base.transform.position).normalized;
			this.MoveTowards(normalized2, run);
		}
		return false;
	}

	// Token: 0x06000454 RID: 1108 RVA: 0x00023580 File Offset: 0x00021780
	protected bool MoveAndAvoid(float dt, Vector3 point, float dist, bool run)
	{
		Vector3 vector = point - base.transform.position;
		if (this.m_character.IsFlying())
		{
			if (vector.magnitude < dist)
			{
				this.StopMoving();
				return true;
			}
		}
		else
		{
			vector.y = 0f;
			if (vector.magnitude < dist)
			{
				this.StopMoving();
				return true;
			}
		}
		vector.Normalize();
		float radius = this.m_character.GetRadius();
		float num = radius + 1f;
		if (!this.m_character.InAttack())
		{
			this.m_getOutOfCornerTimer -= dt;
			if (this.m_getOutOfCornerTimer > 0f)
			{
				Vector3 dir = Quaternion.Euler(0f, this.m_getOutOfCornerAngle, 0f) * -vector;
				this.MoveTowards(dir, run);
				return false;
			}
			this.m_stuckTimer += Time.fixedDeltaTime;
			if (this.m_stuckTimer > 1.5f)
			{
				if (Vector3.Distance(base.transform.position, this.m_lastPosition) < 0.2f)
				{
					this.m_getOutOfCornerTimer = 4f;
					this.m_getOutOfCornerAngle = UnityEngine.Random.Range(-20f, 20f);
					this.m_stuckTimer = 0f;
					return false;
				}
				this.m_stuckTimer = 0f;
				this.m_lastPosition = base.transform.position;
			}
		}
		if (this.CanMove(vector, radius, num))
		{
			this.MoveTowards(vector, run);
		}
		else
		{
			Vector3 forward = base.transform.forward;
			if (this.m_character.IsFlying())
			{
				forward.y = 0.2f;
				forward.Normalize();
			}
			Vector3 b = base.transform.right * radius * 0.75f;
			float num2 = num * 1.5f;
			Vector3 centerPoint = this.m_character.GetCenterPoint();
			float num3 = this.Raycast(centerPoint - b, forward, num2, 0.1f);
			float num4 = this.Raycast(centerPoint + b, forward, num2, 0.1f);
			if (num3 >= num2 && num4 >= num2)
			{
				this.MoveTowards(forward, run);
			}
			else
			{
				Vector3 dir2 = Quaternion.Euler(0f, -20f, 0f) * forward;
				Vector3 dir3 = Quaternion.Euler(0f, 20f, 0f) * forward;
				if (num3 > num4)
				{
					this.MoveTowards(dir2, run);
				}
				else
				{
					this.MoveTowards(dir3, run);
				}
			}
		}
		return false;
	}

	// Token: 0x06000455 RID: 1109 RVA: 0x000237F0 File Offset: 0x000219F0
	private bool CanMove(Vector3 dir, float checkRadius, float distance)
	{
		Vector3 centerPoint = this.m_character.GetCenterPoint();
		Vector3 right = base.transform.right;
		return this.Raycast(centerPoint, dir, distance, 0.1f) >= distance && this.Raycast(centerPoint - right * (checkRadius - 0.1f), dir, distance, 0.1f) >= distance && this.Raycast(centerPoint + right * (checkRadius - 0.1f), dir, distance, 0.1f) >= distance;
	}

	// Token: 0x06000456 RID: 1110 RVA: 0x00023874 File Offset: 0x00021A74
	public float Raycast(Vector3 p, Vector3 dir, float distance, float radius)
	{
		if (radius == 0f)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(p, dir, out raycastHit, distance, BaseAI.m_solidRayMask))
			{
				return raycastHit.distance;
			}
			return distance;
		}
		else
		{
			RaycastHit raycastHit2;
			if (Physics.SphereCast(p, radius, dir, out raycastHit2, distance, BaseAI.m_solidRayMask))
			{
				return raycastHit2.distance;
			}
			return distance;
		}
	}

	// Token: 0x06000457 RID: 1111 RVA: 0x000238C4 File Offset: 0x00021AC4
	public void SetAggravated(bool aggro, BaseAI.AggravatedReason reason)
	{
		if (!this.m_aggravatable)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_aggravated == aggro)
		{
			return;
		}
		this.m_nview.InvokeRPC("SetAggravated", new object[]
		{
			aggro,
			(int)reason
		});
	}

	// Token: 0x06000458 RID: 1112 RVA: 0x0002391C File Offset: 0x00021B1C
	private void RPC_SetAggravated(long sender, bool aggro, int reason)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_aggravated == aggro)
		{
			return;
		}
		this.m_aggravated = aggro;
		this.m_nview.GetZDO().Set(ZDOVars.s_aggravated, this.m_aggravated);
		if (this.m_onBecameAggravated != null)
		{
			this.m_onBecameAggravated((BaseAI.AggravatedReason)reason);
		}
	}

	// Token: 0x06000459 RID: 1113 RVA: 0x00023977 File Offset: 0x00021B77
	public bool IsAggravatable()
	{
		return this.m_aggravatable;
	}

	// Token: 0x0600045A RID: 1114 RVA: 0x00023980 File Offset: 0x00021B80
	public bool IsAggravated()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.m_aggravatable)
		{
			return false;
		}
		if (Time.time - this.m_lastAggravatedCheck > 1f)
		{
			this.m_lastAggravatedCheck = Time.time;
			this.m_aggravated = this.m_nview.GetZDO().GetBool(ZDOVars.s_aggravated, this.m_aggravated);
		}
		return this.m_aggravated;
	}

	// Token: 0x0600045B RID: 1115 RVA: 0x000239EB File Offset: 0x00021BEB
	public bool IsEnemy(Character other)
	{
		return BaseAI.IsEnemy(this.m_character, other);
	}

	// Token: 0x0600045C RID: 1116 RVA: 0x000239FC File Offset: 0x00021BFC
	public static bool IsEnemy(Character a, Character b)
	{
		if (a == b)
		{
			return false;
		}
		string group = a.GetGroup();
		if (group.Length > 0 && group == b.GetGroup())
		{
			return false;
		}
		Character.Faction faction = a.GetFaction();
		Character.Faction faction2 = b.GetFaction();
		bool flag = a.IsTamed();
		bool flag2 = b.IsTamed();
		bool flag3 = a.GetBaseAI() && a.GetBaseAI().IsAggravated();
		bool flag4 = b.GetBaseAI() && b.GetBaseAI().IsAggravated();
		if (flag || flag2)
		{
			return (!flag || !flag2) && (!flag || faction2 != Character.Faction.Players) && (!flag2 || faction != Character.Faction.Players) && (!flag || faction2 != Character.Faction.Dverger || flag4) && (!flag2 || faction != Character.Faction.Dverger || flag3);
		}
		if ((flag3 || flag4) && ((flag3 && faction2 == Character.Faction.Players) || (flag4 && faction == Character.Faction.Players)))
		{
			return true;
		}
		if (faction == faction2)
		{
			return false;
		}
		switch (faction)
		{
		case Character.Faction.Players:
			return faction2 != Character.Faction.Dverger;
		case Character.Faction.AnimalsVeg:
			return true;
		case Character.Faction.ForestMonsters:
			return faction2 != Character.Faction.AnimalsVeg && faction2 != Character.Faction.Boss;
		case Character.Faction.Undead:
			return faction2 != Character.Faction.Demon && faction2 != Character.Faction.Boss;
		case Character.Faction.Demon:
			return faction2 != Character.Faction.Undead && faction2 != Character.Faction.Boss;
		case Character.Faction.MountainMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.SeaMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.PlainsMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.Boss:
			return faction2 == Character.Faction.Players;
		case Character.Faction.MistlandsMonsters:
			return faction2 != Character.Faction.AnimalsVeg && faction2 != Character.Faction.Boss;
		case Character.Faction.Dverger:
			return faction2 != Character.Faction.AnimalsVeg && faction2 != Character.Faction.Boss && faction2 > Character.Faction.Players;
		default:
			return false;
		}
	}

	// Token: 0x0600045D RID: 1117 RVA: 0x00023B84 File Offset: 0x00021D84
	protected StaticTarget FindRandomStaticTarget(float maxDistance)
	{
		float radius = this.m_character.GetRadius();
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius + maxDistance, BaseAI.m_monsterTargetRayMask);
		if (array.Length == 0)
		{
			return null;
		}
		List<StaticTarget> list = new List<StaticTarget>();
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			StaticTarget componentInParent = array2[i].GetComponentInParent<StaticTarget>();
			if (!(componentInParent == null) && componentInParent.IsRandomTarget() && this.CanSeeTarget(componentInParent))
			{
				list.Add(componentInParent);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x0600045E RID: 1118 RVA: 0x00023C24 File Offset: 0x00021E24
	protected StaticTarget FindClosestStaticPriorityTarget()
	{
		float num = (this.m_viewRange > 0f) ? this.m_viewRange : this.m_hearRange;
		Collider[] array = Physics.OverlapSphere(base.transform.position, num, BaseAI.m_monsterTargetRayMask);
		if (array.Length == 0)
		{
			return null;
		}
		StaticTarget result = null;
		float num2 = num;
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			StaticTarget componentInParent = array2[i].GetComponentInParent<StaticTarget>();
			if (!(componentInParent == null) && componentInParent.IsPriorityTarget())
			{
				float num3 = Vector3.Distance(base.transform.position, componentInParent.GetCenter());
				if (num3 < num2 && this.CanSeeTarget(componentInParent))
				{
					result = componentInParent;
					num2 = num3;
				}
			}
		}
		return result;
	}

	// Token: 0x0600045F RID: 1119 RVA: 0x00023CD4 File Offset: 0x00021ED4
	protected void HaveFriendsInRange(float range, out Character hurtFriend, out Character friend)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		friend = this.HaveFriendInRange(allCharacters, range);
		hurtFriend = this.HaveHurtFriendInRange(allCharacters, range);
	}

	// Token: 0x06000460 RID: 1120 RVA: 0x00023CFC File Offset: 0x00021EFC
	private Character HaveFriendInRange(List<Character> characters, float range)
	{
		foreach (Character character in characters)
		{
			if (!(character == this.m_character) && !BaseAI.IsEnemy(this.m_character, character) && Vector3.Distance(character.transform.position, base.transform.position) <= range)
			{
				return character;
			}
		}
		return null;
	}

	// Token: 0x06000461 RID: 1121 RVA: 0x00023D84 File Offset: 0x00021F84
	protected Character HaveFriendInRange(float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		return this.HaveFriendInRange(allCharacters, range);
	}

	// Token: 0x06000462 RID: 1122 RVA: 0x00023DA0 File Offset: 0x00021FA0
	private Character HaveHurtFriendInRange(List<Character> characters, float range)
	{
		foreach (Character character in characters)
		{
			if (!BaseAI.IsEnemy(this.m_character, character) && Vector3.Distance(character.transform.position, base.transform.position) <= range && character.GetHealth() < character.GetMaxHealth())
			{
				return character;
			}
		}
		return null;
	}

	// Token: 0x06000463 RID: 1123 RVA: 0x00023E28 File Offset: 0x00022028
	protected float StandStillDuration(float distanceTreshold)
	{
		if (Vector3.Distance(base.transform.position, this.m_lastMovementCheck) > distanceTreshold)
		{
			this.m_lastMovementCheck = base.transform.position;
			this.m_lastMoveTime = Time.time;
		}
		return Time.time - this.m_lastMoveTime;
	}

	// Token: 0x06000464 RID: 1124 RVA: 0x00023E78 File Offset: 0x00022078
	protected Character HaveHurtFriendInRange(float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		return this.HaveHurtFriendInRange(allCharacters, range);
	}

	// Token: 0x06000465 RID: 1125 RVA: 0x00023E94 File Offset: 0x00022094
	protected Character FindEnemy()
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		Character character = null;
		float num = 99999f;
		foreach (Character character2 in allCharacters)
		{
			if (BaseAI.IsEnemy(this.m_character, character2) && !character2.IsDead())
			{
				BaseAI baseAI = character2.GetBaseAI();
				if ((!(baseAI != null) || !baseAI.IsSleeping()) && this.CanSenseTarget(character2))
				{
					float num2 = Vector3.Distance(character2.transform.position, base.transform.position);
					if (num2 < num || character == null)
					{
						character = character2;
						num = num2;
					}
				}
			}
		}
		if (!(character == null) || !this.HuntPlayer())
		{
			return character;
		}
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 200f);
		if (closestPlayer && (closestPlayer.InDebugFlyMode() || closestPlayer.InGhostMode()))
		{
			return null;
		}
		return closestPlayer;
	}

	// Token: 0x06000466 RID: 1126 RVA: 0x00023F9C File Offset: 0x0002219C
	public static Character FindClosestCreature(Transform me, Vector3 eyePoint, float hearRange, float viewRange, float viewAngle, bool alerted, bool mistVision, bool includePlayers = true, bool includeTamed = true, List<Character> onlyTargets = null)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		Character character = null;
		float num = 99999f;
		foreach (Character character2 in allCharacters)
		{
			if ((includePlayers || !(character2 is Player)) && (includeTamed || !character2.IsTamed()))
			{
				if (onlyTargets != null && onlyTargets.Count > 0)
				{
					bool flag = false;
					foreach (Character character3 in onlyTargets)
					{
						if (character2.m_name == character3.m_name)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				if (!character2.IsDead())
				{
					BaseAI baseAI = character2.GetBaseAI();
					if ((!(baseAI != null) || !baseAI.IsSleeping()) && BaseAI.CanSenseTarget(me, eyePoint, hearRange, viewRange, viewAngle, alerted, mistVision, character2))
					{
						float num2 = Vector3.Distance(character2.transform.position, me.position);
						if (num2 < num || character == null)
						{
							character = character2;
							num = num2;
						}
					}
				}
			}
		}
		return character;
	}

	// Token: 0x06000467 RID: 1127 RVA: 0x000240DC File Offset: 0x000222DC
	public void SetHuntPlayer(bool hunt)
	{
		if (this.m_huntPlayer == hunt)
		{
			return;
		}
		this.m_huntPlayer = hunt;
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_huntPlayer, this.m_huntPlayer);
		}
	}

	// Token: 0x06000468 RID: 1128 RVA: 0x00024117 File Offset: 0x00022317
	public virtual bool HuntPlayer()
	{
		return this.m_huntPlayer;
	}

	// Token: 0x06000469 RID: 1129 RVA: 0x00024120 File Offset: 0x00022320
	protected bool HaveAlertedCreatureInRange(float range)
	{
		foreach (BaseAI baseAI in BaseAI.Instances)
		{
			if (Vector3.Distance(base.transform.position, baseAI.transform.position) < range && baseAI.IsAlerted())
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x00024198 File Offset: 0x00022398
	public static void DoProjectileHitNoise(Vector3 center, float range, Character attacker)
	{
		foreach (BaseAI baseAI in BaseAI.Instances)
		{
			if ((!attacker || baseAI.IsEnemy(attacker)) && Vector3.Distance(baseAI.transform.position, center) < range && baseAI.m_nview && baseAI.m_nview.IsValid())
			{
				baseAI.m_nview.InvokeRPC("OnNearProjectileHit", new object[]
				{
					center,
					range,
					attacker ? attacker.GetZDOID() : ZDOID.None
				});
			}
		}
	}

	// Token: 0x0600046B RID: 1131 RVA: 0x00024270 File Offset: 0x00022470
	protected virtual void RPC_OnNearProjectileHit(long sender, Vector3 center, float range, ZDOID attacker)
	{
		this.Alert();
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x00024278 File Offset: 0x00022478
	public void Alert()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.IsAlerted())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.SetAlerted(true);
			return;
		}
		this.m_nview.InvokeRPC("Alert", Array.Empty<object>());
	}

	// Token: 0x0600046D RID: 1133 RVA: 0x000242C6 File Offset: 0x000224C6
	private void RPC_Alert(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.SetAlerted(true);
	}

	// Token: 0x0600046E RID: 1134 RVA: 0x000242E0 File Offset: 0x000224E0
	protected virtual void SetAlerted(bool alert)
	{
		if (this.m_alerted == alert)
		{
			return;
		}
		this.m_alerted = alert;
		this.m_animator.SetBool("alert", this.m_alerted);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_alert, this.m_alerted);
		}
		if (this.m_alerted)
		{
			this.m_alertedEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		}
		if (alert && this.m_alertedMessage.Length > 0 && !this.m_nview.GetZDO().GetBool(ZDOVars.s_shownAlertMessage, false))
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_shownAlertMessage, true);
			MessageHud.instance.MessageAll(MessageHud.MessageType.Center, this.m_alertedMessage);
		}
	}

	// Token: 0x0600046F RID: 1135 RVA: 0x000243B8 File Offset: 0x000225B8
	public static bool InStealthRange(Character me)
	{
		bool result = false;
		foreach (BaseAI baseAI in BaseAI.Instances)
		{
			if (BaseAI.IsEnemy(me, baseAI.m_character))
			{
				float num = Vector3.Distance(me.transform.position, baseAI.transform.position);
				if (num < baseAI.m_viewRange || num < 10f)
				{
					if (baseAI.IsAlerted())
					{
						return false;
					}
					result = true;
				}
			}
		}
		return result;
	}

	// Token: 0x06000470 RID: 1136 RVA: 0x00024454 File Offset: 0x00022654
	public static bool HaveEnemyInRange(Character me, Vector3 point, float range)
	{
		foreach (Character character in Character.GetAllCharacters())
		{
			if (BaseAI.IsEnemy(me, character) && Vector3.Distance(character.transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000471 RID: 1137 RVA: 0x000244C4 File Offset: 0x000226C4
	public static Character FindClosestEnemy(Character me, Vector3 point, float maxDistance)
	{
		Character character = null;
		float num = maxDistance;
		foreach (Character character2 in Character.GetAllCharacters())
		{
			if (BaseAI.IsEnemy(me, character2))
			{
				float num2 = Vector3.Distance(character2.transform.position, point);
				if (character == null || num2 < num)
				{
					character = character2;
					num = num2;
				}
			}
		}
		return character;
	}

	// Token: 0x06000472 RID: 1138 RVA: 0x00024544 File Offset: 0x00022744
	public static Character FindRandomEnemy(Character me, Vector3 point, float maxDistance)
	{
		List<Character> list = new List<Character>();
		foreach (Character character in Character.GetAllCharacters())
		{
			if (BaseAI.IsEnemy(me, character) && Vector3.Distance(character.transform.position, point) < maxDistance)
			{
				list.Add(character);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x06000473 RID: 1139 RVA: 0x000245D8 File Offset: 0x000227D8
	public bool IsAlerted()
	{
		return this.m_alerted;
	}

	// Token: 0x06000474 RID: 1140 RVA: 0x000245E0 File Offset: 0x000227E0
	protected void SetTargetInfo(ZDOID targetID)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_haveTargetHash, !targetID.IsNone());
	}

	// Token: 0x06000475 RID: 1141 RVA: 0x00024601 File Offset: 0x00022801
	public bool HaveTarget()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(ZDOVars.s_haveTargetHash, false);
	}

	// Token: 0x06000476 RID: 1142 RVA: 0x00024628 File Offset: 0x00022828
	private float GetAltitude()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 1000f, BaseAI.m_solidRayMask))
		{
			return this.m_character.transform.position.y - raycastHit.point.y;
		}
		return 1000f;
	}

	// Token: 0x06000477 RID: 1143 RVA: 0x00024680 File Offset: 0x00022880
	protected virtual void OnDrawGizmosSelected()
	{
		if (this.m_lastFindPathResult)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < this.m_path.Count - 1; i++)
			{
				Vector3 a = this.m_path[i];
				Vector3 a2 = this.m_path[i + 1];
				Gizmos.DrawLine(a + Vector3.up * 0.1f, a2 + Vector3.up * 0.1f);
			}
			Gizmos.color = Color.cyan;
			foreach (Vector3 a3 in this.m_path)
			{
				Gizmos.DrawSphere(a3 + Vector3.up * 0.1f, 0.1f);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawLine(base.transform.position, this.m_lastFindPathTarget);
			Gizmos.DrawSphere(this.m_lastFindPathTarget, 0.2f);
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, this.m_lastFindPathTarget);
		Gizmos.DrawSphere(this.m_lastFindPathTarget, 0.2f);
	}

	// Token: 0x06000478 RID: 1144 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsSleeping()
	{
		return false;
	}

	// Token: 0x06000479 RID: 1145 RVA: 0x000247CC File Offset: 0x000229CC
	public bool HasZDOOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().HasOwner();
	}

	// Token: 0x0600047A RID: 1146 RVA: 0x000247F0 File Offset: 0x000229F0
	public bool CanUseAttack(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_aiInDungeonOnly && !this.m_character.InInterior())
		{
			return false;
		}
		if (item.m_shared.m_aiMaxHealthPercentage < 1f && this.m_character.GetHealthPercentage() > item.m_shared.m_aiMaxHealthPercentage)
		{
			return false;
		}
		bool flag = this.m_character.IsFlying();
		bool flag2 = this.m_character.IsSwimming();
		if (item.m_shared.m_aiWhenFlying && flag)
		{
			float altitude = this.GetAltitude();
			return altitude > item.m_shared.m_aiWhenFlyingAltitudeMin && altitude < item.m_shared.m_aiWhenFlyingAltitudeMax;
		}
		return (!item.m_shared.m_aiInMistOnly || ParticleMist.IsInMist(this.m_character.GetCenterPoint())) && ((item.m_shared.m_aiWhenWalking && !flag && !flag2) || (item.m_shared.m_aiWhenSwiming && flag2));
	}

	// Token: 0x0600047B RID: 1147 RVA: 0x00006475 File Offset: 0x00004675
	public virtual Character GetTargetCreature()
	{
		return null;
	}

	// Token: 0x0600047C RID: 1148 RVA: 0x000248DA File Offset: 0x00022ADA
	public bool HaveRider()
	{
		return this.m_tamable && this.m_tamable.HaveRider();
	}

	// Token: 0x0600047D RID: 1149 RVA: 0x000248F6 File Offset: 0x00022AF6
	public float GetRiderSkill()
	{
		if (this.m_tamable)
		{
			return this.m_tamable.GetRiderSkill();
		}
		return 0f;
	}

	// Token: 0x0600047E RID: 1150 RVA: 0x00024918 File Offset: 0x00022B18
	public static void AggravateAllInArea(Vector3 point, float radius, BaseAI.AggravatedReason reason)
	{
		foreach (BaseAI baseAI in BaseAI.Instances)
		{
			if (baseAI.IsAggravatable() && Vector3.Distance(point, baseAI.transform.position) <= radius)
			{
				baseAI.SetAggravated(true, reason);
				baseAI.Alert();
			}
		}
	}

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x0600047F RID: 1151 RVA: 0x00024990 File Offset: 0x00022B90
	public static List<BaseAI> Instances { get; } = new List<BaseAI>();

	// Token: 0x040004D7 RID: 1239
	private float m_lastMoveToWaterUpdate;

	// Token: 0x040004D8 RID: 1240
	private bool m_haveWaterPosition;

	// Token: 0x040004D9 RID: 1241
	private Vector3 m_moveToWaterPosition = Vector3.zero;

	// Token: 0x040004DA RID: 1242
	private float m_fleeTargetUpdateTime;

	// Token: 0x040004DB RID: 1243
	private Vector3 m_fleeTarget = Vector3.zero;

	// Token: 0x040004DC RID: 1244
	private float m_nearFireTime;

	// Token: 0x040004DD RID: 1245
	private EffectArea m_nearFireArea;

	// Token: 0x040004DE RID: 1246
	private float aroundPointUpdateTime;

	// Token: 0x040004DF RID: 1247
	private Vector3 arroundPointTarget = Vector3.zero;

	// Token: 0x040004E0 RID: 1248
	private Vector3 m_lastMovementCheck;

	// Token: 0x040004E1 RID: 1249
	private float m_lastMoveTime;

	// Token: 0x040004E2 RID: 1250
	private const bool m_debugDraw = false;

	// Token: 0x040004E3 RID: 1251
	public Action<BaseAI.AggravatedReason> m_onBecameAggravated;

	// Token: 0x040004E4 RID: 1252
	public float m_viewRange = 50f;

	// Token: 0x040004E5 RID: 1253
	public float m_viewAngle = 90f;

	// Token: 0x040004E6 RID: 1254
	public float m_hearRange = 9999f;

	// Token: 0x040004E7 RID: 1255
	public bool m_mistVision;

	// Token: 0x040004E8 RID: 1256
	private const float m_interiorMaxHearRange = 12f;

	// Token: 0x040004E9 RID: 1257
	private const float m_despawnDistance = 80f;

	// Token: 0x040004EA RID: 1258
	private const float m_regenAllHPTime = 3600f;

	// Token: 0x040004EB RID: 1259
	public EffectList m_alertedEffects = new EffectList();

	// Token: 0x040004EC RID: 1260
	public EffectList m_idleSound = new EffectList();

	// Token: 0x040004ED RID: 1261
	public float m_idleSoundInterval = 5f;

	// Token: 0x040004EE RID: 1262
	public float m_idleSoundChance = 0.5f;

	// Token: 0x040004EF RID: 1263
	public Pathfinding.AgentType m_pathAgentType = Pathfinding.AgentType.Humanoid;

	// Token: 0x040004F0 RID: 1264
	public float m_moveMinAngle = 10f;

	// Token: 0x040004F1 RID: 1265
	public bool m_smoothMovement = true;

	// Token: 0x040004F2 RID: 1266
	public bool m_serpentMovement;

	// Token: 0x040004F3 RID: 1267
	public float m_serpentTurnRadius = 20f;

	// Token: 0x040004F4 RID: 1268
	public float m_jumpInterval;

	// Token: 0x040004F5 RID: 1269
	[Header("Random circle")]
	public float m_randomCircleInterval = 2f;

	// Token: 0x040004F6 RID: 1270
	[Header("Random movement")]
	public float m_randomMoveInterval = 5f;

	// Token: 0x040004F7 RID: 1271
	public float m_randomMoveRange = 4f;

	// Token: 0x040004F8 RID: 1272
	[Header("Fly behaviour")]
	public bool m_randomFly;

	// Token: 0x040004F9 RID: 1273
	public float m_chanceToTakeoff = 1f;

	// Token: 0x040004FA RID: 1274
	public float m_chanceToLand = 1f;

	// Token: 0x040004FB RID: 1275
	public float m_groundDuration = 10f;

	// Token: 0x040004FC RID: 1276
	public float m_airDuration = 10f;

	// Token: 0x040004FD RID: 1277
	public float m_maxLandAltitude = 5f;

	// Token: 0x040004FE RID: 1278
	public float m_takeoffTime = 5f;

	// Token: 0x040004FF RID: 1279
	public float m_flyAltitudeMin = 3f;

	// Token: 0x04000500 RID: 1280
	public float m_flyAltitudeMax = 10f;

	// Token: 0x04000501 RID: 1281
	public bool m_limitMaxAltitude;

	// Token: 0x04000502 RID: 1282
	[Header("Other")]
	public bool m_avoidFire;

	// Token: 0x04000503 RID: 1283
	public bool m_afraidOfFire;

	// Token: 0x04000504 RID: 1284
	public bool m_avoidWater = true;

	// Token: 0x04000505 RID: 1285
	public bool m_aggravatable;

	// Token: 0x04000506 RID: 1286
	public string m_spawnMessage = "";

	// Token: 0x04000507 RID: 1287
	public string m_deathMessage = "";

	// Token: 0x04000508 RID: 1288
	public string m_alertedMessage = "";

	// Token: 0x04000509 RID: 1289
	private bool m_patrol;

	// Token: 0x0400050A RID: 1290
	private Vector3 m_patrolPoint = Vector3.zero;

	// Token: 0x0400050B RID: 1291
	private float m_patrolPointUpdateTime;

	// Token: 0x0400050C RID: 1292
	protected ZNetView m_nview;

	// Token: 0x0400050D RID: 1293
	protected Character m_character;

	// Token: 0x0400050E RID: 1294
	protected ZSyncAnimation m_animator;

	// Token: 0x0400050F RID: 1295
	protected Tameable m_tamable;

	// Token: 0x04000510 RID: 1296
	private static int m_solidRayMask = 0;

	// Token: 0x04000511 RID: 1297
	private static int m_viewBlockMask = 0;

	// Token: 0x04000512 RID: 1298
	private static int m_monsterTargetRayMask = 0;

	// Token: 0x04000513 RID: 1299
	private Vector3 m_randomMoveTarget = Vector3.zero;

	// Token: 0x04000514 RID: 1300
	private float m_randomMoveUpdateTimer;

	// Token: 0x04000515 RID: 1301
	private bool m_reachedRandomMoveTarget = true;

	// Token: 0x04000516 RID: 1302
	private float m_jumpTimer;

	// Token: 0x04000517 RID: 1303
	private float m_randomFlyTimer;

	// Token: 0x04000518 RID: 1304
	private float m_regenTimer;

	// Token: 0x04000519 RID: 1305
	private bool m_alerted;

	// Token: 0x0400051A RID: 1306
	private bool m_huntPlayer;

	// Token: 0x0400051B RID: 1307
	private bool m_aggravated;

	// Token: 0x0400051C RID: 1308
	private float m_lastAggravatedCheck;

	// Token: 0x0400051D RID: 1309
	protected Vector3 m_spawnPoint = Vector3.zero;

	// Token: 0x0400051E RID: 1310
	private const float m_getOfOfCornerMaxAngle = 20f;

	// Token: 0x0400051F RID: 1311
	private float m_getOutOfCornerTimer;

	// Token: 0x04000520 RID: 1312
	private float m_getOutOfCornerAngle;

	// Token: 0x04000521 RID: 1313
	private Vector3 m_lastPosition = Vector3.zero;

	// Token: 0x04000522 RID: 1314
	private float m_stuckTimer;

	// Token: 0x04000523 RID: 1315
	protected float m_timeSinceHurt = 99999f;

	// Token: 0x04000524 RID: 1316
	private Vector3 m_lastFindPathTarget = new Vector3(-999999f, -999999f, -999999f);

	// Token: 0x04000525 RID: 1317
	private float m_lastFindPathTime;

	// Token: 0x04000526 RID: 1318
	private bool m_lastFindPathResult;

	// Token: 0x04000527 RID: 1319
	private readonly List<Vector3> m_path = new List<Vector3>();

	// Token: 0x04000528 RID: 1320
	private static readonly RaycastHit[] s_tempRaycastHits = new RaycastHit[128];

	// Token: 0x02000052 RID: 82
	public enum AggravatedReason
	{
		// Token: 0x0400052B RID: 1323
		Damage,
		// Token: 0x0400052C RID: 1324
		Building,
		// Token: 0x0400052D RID: 1325
		Theif
	}
}
