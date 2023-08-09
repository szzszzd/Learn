using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000003 RID: 3
public class Character : MonoBehaviour, IDestructible, Hoverable, IWaterInteractable
{
	// Token: 0x06000004 RID: 4 RVA: 0x000020C8 File Offset: 0x000002C8
	protected virtual void Awake()
	{
		Character.s_characters.Add(this);
		this.m_collider = base.GetComponent<CapsuleCollider>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_zanim = base.GetComponent<ZSyncAnimation>();
		this.m_nview = ((this.m_nViewOverride != null) ? this.m_nViewOverride : base.GetComponent<ZNetView>());
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_animEvent = this.m_animator.GetComponent<CharacterAnimEvent>();
		this.m_baseAI = base.GetComponent<BaseAI>();
		this.m_animator.logWarnings = false;
		this.m_visual = base.transform.Find("Visual").gameObject;
		this.m_lodGroup = this.m_visual.GetComponent<LODGroup>();
		this.m_head = this.m_animator.GetBoneTransform(HumanBodyBones.Head);
		this.m_body.maxDepenetrationVelocity = 2f;
		if (Character.s_smokeRayMask == 0)
		{
			Character.s_smokeRayMask = LayerMask.GetMask(new string[]
			{
				"smoke"
			});
			Character.s_characterLayer = LayerMask.NameToLayer("character");
			Character.s_characterNetLayer = LayerMask.NameToLayer("character_net");
			Character.s_characterGhostLayer = LayerMask.NameToLayer("character_ghost");
			Character.s_groundRayMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain",
				"blocker",
				"vehicle"
			});
		}
		if (this.m_lodGroup)
		{
			this.m_originalLocalRef = this.m_lodGroup.localReferencePoint;
		}
		this.m_seman = new SEMan(this, this.m_nview);
		if (this.m_nview.GetZDO() != null)
		{
			if (!this.IsPlayer())
			{
				this.m_tamed = this.m_nview.GetZDO().GetBool(ZDOVars.s_tamed, this.m_tamed);
				this.m_level = this.m_nview.GetZDO().GetInt(ZDOVars.s_level, 1);
				if (this.m_nview.IsOwner() && this.GetHealth() == this.GetMaxHealth())
				{
					this.SetupMaxHealth();
				}
			}
			this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
			this.m_nview.Register<float, bool>("Heal", new Action<long, float, bool>(this.RPC_Heal));
			this.m_nview.Register<float>("AddNoise", new Action<long, float>(this.RPC_AddNoise));
			this.m_nview.Register<Vector3>("Stagger", new Action<long, Vector3>(this.RPC_Stagger));
			this.m_nview.Register("ResetCloth", new Action<long>(this.RPC_ResetCloth));
			this.m_nview.Register<bool>("SetTamed", new Action<long, bool>(this.RPC_SetTamed));
			this.m_nview.Register<float>("FreezeFrame", new Action<long, float>(this.RPC_FreezeFrame));
			this.m_nview.Register<Vector3, Quaternion, bool>("RPC_TeleportTo", new Action<long, Vector3, Quaternion, bool>(this.RPC_TeleportTo));
		}
	}

	// Token: 0x06000005 RID: 5 RVA: 0x000023C7 File Offset: 0x000005C7
	protected virtual void OnEnable()
	{
		Character.Instances.Add(this);
	}

	// Token: 0x06000006 RID: 6 RVA: 0x000023D4 File Offset: 0x000005D4
	protected virtual void OnDisable()
	{
		Character.Instances.Remove(this);
	}

	// Token: 0x06000007 RID: 7 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void Start()
	{
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000023E4 File Offset: 0x000005E4
	protected virtual void OnDestroy()
	{
		this.m_seman.OnDestroy();
		Character.s_characters.Remove(this);
	}

	// Token: 0x06000009 RID: 9 RVA: 0x00002400 File Offset: 0x00000600
	private void SetupMaxHealth()
	{
		int level = this.GetLevel();
		this.SetMaxHealth(this.m_health * (float)level);
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00002424 File Offset: 0x00000624
	public void SetLevel(int level)
	{
		if (level < 1)
		{
			return;
		}
		this.m_level = level;
		this.m_nview.GetZDO().Set(ZDOVars.s_level, level, false);
		this.SetupMaxHealth();
		if (this.m_onLevelSet != null)
		{
			this.m_onLevelSet(this.m_level);
		}
	}

	// Token: 0x0600000B RID: 11 RVA: 0x00002473 File Offset: 0x00000673
	public int GetLevel()
	{
		return this.m_level;
	}

	// Token: 0x0600000C RID: 12 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsPlayer()
	{
		return false;
	}

	// Token: 0x0600000D RID: 13 RVA: 0x0000247E File Offset: 0x0000067E
	public Character.Faction GetFaction()
	{
		return this.m_faction;
	}

	// Token: 0x0600000E RID: 14 RVA: 0x00002486 File Offset: 0x00000686
	public string GetGroup()
	{
		return this.m_group;
	}

	// Token: 0x0600000F RID: 15 RVA: 0x00002490 File Offset: 0x00000690
	public void CustomFixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateLayer();
		this.UpdateContinousEffects();
		this.UpdateWater(fixedDeltaTime);
		this.UpdateGroundTilt(fixedDeltaTime);
		this.SetVisible(this.m_nview.HasOwner());
		this.UpdateLookTransition(fixedDeltaTime);
		if (this.m_nview.IsOwner())
		{
			this.UpdateGroundContact(fixedDeltaTime);
			this.UpdateNoise(fixedDeltaTime);
			this.m_seman.Update(fixedDeltaTime);
			this.UpdateStagger(fixedDeltaTime);
			this.UpdatePushback(fixedDeltaTime);
			this.UpdateMotion(fixedDeltaTime);
			this.UpdateSmoke(fixedDeltaTime);
			this.UnderWorldCheck(fixedDeltaTime);
			this.SyncVelocity();
			this.CheckDeath();
		}
	}

	// Token: 0x06000010 RID: 16 RVA: 0x0000253C File Offset: 0x0000073C
	private void UpdateLayer()
	{
		if (this.m_collider.gameObject.layer == Character.s_characterLayer || this.m_collider.gameObject.layer == Character.s_characterNetLayer)
		{
			if (this.m_nview.IsOwner())
			{
				this.m_collider.gameObject.layer = (this.IsAttached() ? Character.s_characterNetLayer : Character.s_characterLayer);
			}
			else
			{
				this.m_collider.gameObject.layer = Character.s_characterNetLayer;
			}
		}
		if (this.m_disableWhileSleeping)
		{
			if (this.m_baseAI && this.m_baseAI.IsSleeping())
			{
				this.m_body.isKinematic = true;
				return;
			}
			this.m_body.isKinematic = false;
		}
	}

	// Token: 0x06000011 RID: 17 RVA: 0x000025FC File Offset: 0x000007FC
	private void UnderWorldCheck(float dt)
	{
		if (this.IsDead())
		{
			return;
		}
		this.m_underWorldCheckTimer += dt;
		if (this.m_underWorldCheckTimer > 5f || this.IsPlayer())
		{
			this.m_underWorldCheckTimer = 0f;
			float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
			if (base.transform.position.y < groundHeight - 1f)
			{
				Vector3 position = base.transform.position;
				position.y = groundHeight + 0.5f;
				base.transform.position = position;
				this.m_body.position = position;
				this.m_body.velocity = Vector3.zero;
			}
		}
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000026B4 File Offset: 0x000008B4
	private void UpdateSmoke(float dt)
	{
		if (this.m_tolerateSmoke)
		{
			return;
		}
		this.m_smokeCheckTimer += dt;
		if (this.m_smokeCheckTimer > 2f)
		{
			this.m_smokeCheckTimer = 0f;
			if (Physics.CheckSphere(this.GetTopPoint() + Vector3.up * 0.1f, 0.5f, Character.s_smokeRayMask))
			{
				this.m_seman.AddStatusEffect(Character.s_statusEffectSmoked, true, 0, 0f);
				return;
			}
			this.m_seman.RemoveStatusEffect(Character.s_statusEffectSmoked, true);
		}
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00002748 File Offset: 0x00000948
	private void UpdateContinousEffects()
	{
		this.SetupContinuousEffect(base.transform.position, this.m_sliding, this.m_slideEffects, ref this.m_slideEffects_instances);
		Vector3 position = base.transform.position;
		position.y = this.GetLiquidLevel() + 0.05f;
		EffectList effects = (this.InTar() && this.m_tarEffects.HasEffects()) ? this.m_tarEffects : this.m_waterEffects;
		this.SetupContinuousEffect(position, this.InLiquid(), effects, ref this.m_waterEffects_instances);
		this.SetupContinuousEffect(base.transform.position, this.IsFlying(), this.m_flyingContinuousEffect, ref this.m_flyingEffects_instances);
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000027F4 File Offset: 0x000009F4
	private void SetupContinuousEffect(Vector3 point, bool enabledEffect, EffectList effects, ref GameObject[] instances)
	{
		if (!effects.HasEffects())
		{
			return;
		}
		if (enabledEffect)
		{
			if (instances == null)
			{
				instances = effects.Create(point, Quaternion.identity, base.transform, 1f, -1);
				return;
			}
			foreach (GameObject gameObject in instances)
			{
				if (gameObject)
				{
					gameObject.transform.position = point;
				}
			}
			return;
		}
		else
		{
			if (instances == null)
			{
				return;
			}
			foreach (GameObject gameObject2 in instances)
			{
				if (gameObject2)
				{
					foreach (ParticleSystem particleSystem in gameObject2.GetComponentsInChildren<ParticleSystem>())
					{
						particleSystem.emission.enabled = false;
						particleSystem.Stop();
					}
					CamShaker componentInChildren = gameObject2.GetComponentInChildren<CamShaker>();
					if (componentInChildren)
					{
						UnityEngine.Object.Destroy(componentInChildren);
					}
					ZSFX componentInChildren2 = gameObject2.GetComponentInChildren<ZSFX>();
					if (componentInChildren2)
					{
						componentInChildren2.FadeOut();
					}
					TimedDestruction component = gameObject2.GetComponent<TimedDestruction>();
					if (component)
					{
						component.Trigger();
					}
					else
					{
						UnityEngine.Object.Destroy(gameObject2);
					}
				}
			}
			instances = null;
			return;
		}
	}

	// Token: 0x06000015 RID: 21 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void OnSwimming(Vector3 targetVel, float dt)
	{
	}

	// Token: 0x06000016 RID: 22 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void OnSneaking(float dt)
	{
	}

	// Token: 0x06000017 RID: 23 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void OnJump()
	{
	}

	// Token: 0x06000018 RID: 24 RVA: 0x0000290F File Offset: 0x00000B0F
	protected virtual bool TakeInput()
	{
		return true;
	}

	// Token: 0x06000019 RID: 25 RVA: 0x00002912 File Offset: 0x00000B12
	private float GetSlideAngle()
	{
		if (this.IsPlayer())
		{
			return 38f;
		}
		if (this.HaveRider())
		{
			return 45f;
		}
		return 90f;
	}

	// Token: 0x0600001A RID: 26 RVA: 0x00002935 File Offset: 0x00000B35
	public bool HaveRider()
	{
		return this.m_baseAI && this.m_baseAI.HaveRider();
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00002954 File Offset: 0x00000B54
	private void ApplySlide(float dt, ref Vector3 currentVel, Vector3 bodyVel, bool running)
	{
		bool flag = this.CanWallRun();
		float num = Mathf.Clamp(Mathf.Acos(Mathf.Clamp01(((this.m_groundTilt != Character.GroundTiltType.None) ? this.m_groundTiltNormal : this.m_lastGroundNormal).y)) * 57.29578f, 0f, 90f);
		Vector3 lastGroundNormal = this.m_lastGroundNormal;
		lastGroundNormal.y = 0f;
		lastGroundNormal.Normalize();
		Vector3 velocity = this.m_body.velocity;
		Vector3 rhs = Vector3.Cross(this.m_lastGroundNormal, Vector3.up);
		Vector3 a = Vector3.Cross(this.m_lastGroundNormal, rhs);
		bool flag2 = currentVel.magnitude > 0.1f;
		if (num > this.GetSlideAngle())
		{
			if (running && flag && flag2)
			{
				this.m_slippage = 0f;
				this.m_wallRunning = true;
			}
			else
			{
				this.m_slippage = Mathf.MoveTowards(this.m_slippage, 1f, 1f * dt);
			}
			Vector3 b = a * 5f;
			currentVel = Vector3.Lerp(currentVel, b, this.m_slippage);
			this.m_sliding = (this.m_slippage > 0.5f);
			return;
		}
		this.m_slippage = 0f;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002A7C File Offset: 0x00000C7C
	private void UpdateMotion(float dt)
	{
		this.UpdateBodyFriction();
		this.m_sliding = false;
		this.m_wallRunning = false;
		this.m_running = false;
		this.m_walking = false;
		if (this.IsDead())
		{
			return;
		}
		if (this.IsDebugFlying())
		{
			this.UpdateDebugFly(dt);
			return;
		}
		if (this.InIntro())
		{
			this.m_maxAirAltitude = base.transform.position.y;
			this.m_body.velocity = Vector3.zero;
			this.m_body.angularVelocity = Vector3.zero;
		}
		if (!this.InLiquidSwimDepth() && !this.IsOnGround() && !this.IsAttached())
		{
			float y = base.transform.position.y;
			this.m_maxAirAltitude = Mathf.Max(this.m_maxAirAltitude, y);
			this.m_fallTimer += dt;
			if (this.IsPlayer() && this.m_fallTimer > 0.1f)
			{
				this.m_zanim.SetBool(Character.s_animatorFalling, true);
			}
		}
		else
		{
			this.m_fallTimer = 0f;
			if (this.IsPlayer())
			{
				this.m_zanim.SetBool(Character.s_animatorFalling, false);
			}
		}
		if (this.IsSwimming())
		{
			this.UpdateSwimming(dt);
		}
		else if (this.m_flying)
		{
			this.UpdateFlying(dt);
		}
		else
		{
			this.UpdateWalking(dt);
		}
		this.m_lastGroundTouch += Time.fixedDeltaTime;
		this.m_jumpTimer += Time.fixedDeltaTime;
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00002BE4 File Offset: 0x00000DE4
	private void UpdateDebugFly(float dt)
	{
		float num = this.m_run ? ((float)Character.m_debugFlySpeed * 2.5f) : ((float)Character.m_debugFlySpeed);
		Vector3 b = this.m_moveDir * num;
		if (this.TakeInput())
		{
			if (ZInput.GetButton("Jump") || ZInput.GetButton("JoyJump"))
			{
				b.y = num;
			}
			else if (Input.GetKey(KeyCode.LeftControl) || ZInput.GetButton("JoyCrouch"))
			{
				b.y = -num;
			}
		}
		this.m_currentVel = Vector3.Lerp(this.m_currentVel, b, 0.5f);
		this.m_body.velocity = this.m_currentVel;
		this.m_body.useGravity = false;
		this.m_lastGroundTouch = 0f;
		this.m_maxAirAltitude = base.transform.position.y;
		this.m_body.rotation = Quaternion.RotateTowards(base.transform.rotation, this.m_lookYaw, this.m_turnSpeed * dt);
		this.m_body.angularVelocity = Vector3.zero;
		this.UpdateEyeRotation();
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00002CFC File Offset: 0x00000EFC
	private void UpdateSwimming(float dt)
	{
		bool flag = this.IsOnGround();
		if (Mathf.Max(0f, this.m_maxAirAltitude - base.transform.position.y) > 0.5f && this.m_onLand != null)
		{
			this.m_onLand(new Vector3(base.transform.position.x, this.GetLiquidLevel(), base.transform.position.z));
		}
		this.m_maxAirAltitude = base.transform.position.y;
		float d = this.m_swimSpeed * this.GetAttackSpeedFactorMovement();
		if (this.InMinorActionSlowdown())
		{
			d = 0f;
		}
		this.m_seman.ApplyStatusEffectSpeedMods(ref d);
		Vector3 vector = this.m_moveDir * d;
		if (this.IsPlayer())
		{
			this.m_currentVel = Vector3.Lerp(this.m_currentVel, vector, this.m_swimAcceleration);
		}
		else
		{
			float num = vector.magnitude;
			float magnitude = this.m_currentVel.magnitude;
			if (num > magnitude)
			{
				num = Mathf.MoveTowards(magnitude, num, this.m_swimAcceleration);
				vector = vector.normalized * num;
			}
			this.m_currentVel = Vector3.Lerp(this.m_currentVel, vector, 0.5f);
		}
		if (this.m_currentVel.magnitude > 0.1f)
		{
			this.AddNoise(15f);
		}
		this.AddPushbackForce(ref this.m_currentVel);
		Vector3 force = this.m_currentVel - this.m_body.velocity;
		force.y = 0f;
		if (force.magnitude > 20f)
		{
			force = force.normalized * 20f;
		}
		this.m_body.AddForce(force, ForceMode.VelocityChange);
		float num2 = this.GetLiquidLevel() - this.m_swimDepth;
		if (base.transform.position.y < num2)
		{
			float t = Mathf.Clamp01((num2 - base.transform.position.y) / 2f);
			float target = Mathf.Lerp(0f, 10f, t);
			Vector3 velocity = this.m_body.velocity;
			velocity.y = Mathf.MoveTowards(velocity.y, target, 50f * dt);
			this.m_body.velocity = velocity;
		}
		else
		{
			float t2 = Mathf.Clamp01(-(num2 - base.transform.position.y) / 1f);
			float num3 = Mathf.Lerp(0f, 10f, t2);
			Vector3 velocity2 = this.m_body.velocity;
			velocity2.y = Mathf.MoveTowards(velocity2.y, -num3, 30f * dt);
			this.m_body.velocity = velocity2;
		}
		float target2 = 0f;
		if (this.m_moveDir.magnitude > 0.1f || this.AlwaysRotateCamera())
		{
			float swimTurnSpeed = this.m_swimTurnSpeed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref swimTurnSpeed);
			target2 = this.UpdateRotation(swimTurnSpeed, dt, false);
		}
		this.m_body.angularVelocity = Vector3.zero;
		this.UpdateEyeRotation();
		this.m_body.useGravity = true;
		float value = (this.IsPlayer() || this.HaveRider()) ? Vector3.Dot(this.m_currentVel, base.transform.forward) : Vector3.Dot(this.m_body.velocity, base.transform.forward);
		float value2 = Vector3.Dot(this.m_currentVel, base.transform.right);
		this.m_currentTurnVel = Mathf.SmoothDamp(this.m_currentTurnVel, target2, ref this.m_currentTurnVelChange, 0.5f, 99f);
		this.m_zanim.SetFloat(Character.s_forwardSpeed, value);
		this.m_zanim.SetFloat(Character.s_sidewaySpeed, value2);
		this.m_zanim.SetFloat(Character.s_turnSpeed, this.m_currentTurnVel);
		this.m_zanim.SetBool(Character.s_inWater, !flag);
		this.m_zanim.SetBool(Character.s_onGround, false);
		this.m_zanim.SetBool(Character.s_encumbered, false);
		this.m_zanim.SetBool(Character.s_flying, false);
		if (!flag)
		{
			this.OnSwimming(vector, dt);
		}
	}

	// Token: 0x0600001F RID: 31 RVA: 0x0000311C File Offset: 0x0000131C
	private void UpdateFlying(float dt)
	{
		float d = (this.m_run ? this.m_flyFastSpeed : this.m_flySlowSpeed) * this.GetAttackSpeedFactorMovement();
		Vector3 b = this.CanMove() ? (this.m_moveDir * d) : Vector3.zero;
		this.m_currentVel = Vector3.Lerp(this.m_currentVel, b, this.m_acceleration);
		this.m_maxAirAltitude = base.transform.position.y;
		this.ApplyRootMotion(ref this.m_currentVel);
		this.AddPushbackForce(ref this.m_currentVel);
		Vector3 force = this.m_currentVel - this.m_body.velocity;
		if (force.magnitude > 20f)
		{
			force = force.normalized * 20f;
		}
		this.m_body.AddForce(force, ForceMode.VelocityChange);
		float target = 0f;
		if ((this.m_moveDir.magnitude > 0.1f || this.AlwaysRotateCamera()) && !this.InDodge() && this.CanMove())
		{
			float flyTurnSpeed = this.m_flyTurnSpeed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref flyTurnSpeed);
			target = this.UpdateRotation(flyTurnSpeed, dt, true);
		}
		this.m_body.angularVelocity = Vector3.zero;
		this.UpdateEyeRotation();
		this.m_body.useGravity = false;
		float num = Vector3.Dot(this.m_currentVel, base.transform.forward);
		float value = Vector3.Dot(this.m_currentVel, base.transform.right);
		float num2 = Vector3.Dot(this.m_body.velocity, base.transform.forward);
		this.m_currentTurnVel = Mathf.SmoothDamp(this.m_currentTurnVel, target, ref this.m_currentTurnVelChange, 0.5f, 99f);
		this.m_zanim.SetFloat(Character.s_forwardSpeed, this.IsPlayer() ? num : num2);
		this.m_zanim.SetFloat(Character.s_sidewaySpeed, value);
		this.m_zanim.SetFloat(Character.s_turnSpeed, this.m_currentTurnVel);
		this.m_zanim.SetBool(Character.s_inWater, false);
		this.m_zanim.SetBool(Character.s_onGround, false);
		this.m_zanim.SetBool(Character.s_encumbered, false);
		this.m_zanim.SetBool(Character.s_flying, true);
	}

	// Token: 0x06000020 RID: 32 RVA: 0x00003358 File Offset: 0x00001558
	private void UpdateWalking(float dt)
	{
		Vector3 moveDir = this.m_moveDir;
		bool flag = this.IsCrouching();
		this.m_running = this.CheckRun(moveDir, dt);
		float num = this.m_speed * this.GetJogSpeedFactor();
		if ((this.m_walk || this.InMinorActionSlowdown()) && !flag)
		{
			num = this.m_walkSpeed;
			this.m_walking = (moveDir.magnitude > 0.1f);
		}
		else if (this.m_running)
		{
			num = this.m_runSpeed * this.GetRunSpeedFactor();
			if (this.IsPlayer() && moveDir.magnitude > 0f)
			{
				moveDir.Normalize();
			}
		}
		else if (flag || this.IsEncumbered())
		{
			num = this.m_crouchSpeed;
		}
		this.ApplyLiquidResistance(ref num);
		num *= this.GetAttackSpeedFactorMovement();
		this.m_seman.ApplyStatusEffectSpeedMods(ref num);
		Vector3 vector = this.CanMove() ? (moveDir * num) : Vector3.zero;
		if (vector.magnitude > 0f && this.IsOnGround())
		{
			vector = Vector3.ProjectOnPlane(vector, this.m_lastGroundNormal).normalized * vector.magnitude;
		}
		float num2 = vector.magnitude;
		float magnitude = this.m_currentVel.magnitude;
		if (num2 > magnitude)
		{
			num2 = Mathf.MoveTowards(magnitude, num2, this.m_acceleration);
			vector = vector.normalized * num2;
		}
		else
		{
			num2 = Mathf.MoveTowards(magnitude, num2, this.m_acceleration * 2f);
			vector = ((vector.magnitude > 0f) ? (vector.normalized * num2) : (this.m_currentVel.normalized * num2));
		}
		this.m_currentVel = Vector3.Lerp(this.m_currentVel, vector, 0.5f);
		Vector3 velocity = this.m_body.velocity;
		Vector3 currentVel = this.m_currentVel;
		currentVel.y = velocity.y;
		if (this.IsOnGround() && this.m_lastAttachBody == null)
		{
			this.ApplySlide(dt, ref currentVel, velocity, this.m_running);
			currentVel.y = Mathf.Min(currentVel.y, 3f);
		}
		this.ApplyRootMotion(ref currentVel);
		this.AddPushbackForce(ref currentVel);
		this.ApplyGroundForce(ref currentVel, vector);
		Vector3 vector2 = currentVel - velocity;
		if (!this.IsOnGround())
		{
			if (vector.magnitude > 0.1f)
			{
				vector2 *= this.m_airControl;
			}
			else
			{
				vector2 = Vector3.zero;
			}
		}
		if (this.IsAttached())
		{
			vector2 = Vector3.zero;
		}
		if (vector2.magnitude > 20f)
		{
			vector2 = vector2.normalized * 20f;
		}
		if (vector2.magnitude > 0.01f)
		{
			this.m_body.AddForce(vector2, ForceMode.VelocityChange);
		}
		Vector3 velocity2 = this.m_body.velocity;
		this.m_seman.ModifyWalkVelocity(ref velocity2);
		this.m_body.velocity = velocity2;
		if (this.m_lastGroundBody && this.m_lastGroundBody.gameObject.layer != base.gameObject.layer && this.m_lastGroundBody.mass > this.m_body.mass)
		{
			float d = this.m_body.mass / this.m_lastGroundBody.mass;
			this.m_lastGroundBody.AddForceAtPosition(-vector2 * d, base.transform.position, ForceMode.VelocityChange);
		}
		float target = 0f;
		if ((moveDir.magnitude > 0.1f || this.AlwaysRotateCamera()) && !this.InDodge() && this.CanMove())
		{
			float turnSpeed = this.m_run ? this.m_runTurnSpeed : this.m_turnSpeed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref turnSpeed);
			target = this.UpdateRotation(turnSpeed, dt, false);
		}
		if (this.IsSneaking())
		{
			this.OnSneaking(dt);
		}
		this.UpdateEyeRotation();
		this.m_body.useGravity = true;
		float num3 = Vector3.Dot(this.m_currentVel, Vector3.ProjectOnPlane(base.transform.forward, this.m_lastGroundNormal).normalized);
		float num4 = Vector3.Dot(this.m_body.velocity, this.m_visual.transform.forward);
		if (this.IsRiding())
		{
			num3 = num4;
		}
		else if (!this.IsPlayer() && !this.HaveRider())
		{
			num3 = Mathf.Min(num3, num4);
		}
		float value = Vector3.Dot(this.m_currentVel, Vector3.ProjectOnPlane(base.transform.right, this.m_lastGroundNormal).normalized);
		this.m_currentTurnVel = Mathf.SmoothDamp(this.m_currentTurnVel, target, ref this.m_currentTurnVelChange, 0.5f, 99f);
		this.m_zanim.SetFloat(Character.s_forwardSpeed, num3);
		this.m_zanim.SetFloat(Character.s_sidewaySpeed, value);
		this.m_zanim.SetFloat(Character.s_turnSpeed, this.m_currentTurnVel);
		this.m_zanim.SetBool(Character.s_inWater, false);
		this.m_zanim.SetBool(Character.s_onGround, this.IsOnGround());
		this.m_zanim.SetBool(Character.s_encumbered, this.IsEncumbered());
		this.m_zanim.SetBool(Character.s_flying, false);
		if (this.m_currentVel.magnitude > 0.1f)
		{
			if (this.m_running)
			{
				this.AddNoise(30f);
				return;
			}
			if (!flag)
			{
				this.AddNoise(15f);
			}
		}
	}

	// Token: 0x06000021 RID: 33 RVA: 0x000038B8 File Offset: 0x00001AB8
	public bool IsSneaking()
	{
		return this.IsCrouching() && this.m_currentVel.magnitude > 0.1f && this.IsOnGround();
	}

	// Token: 0x06000022 RID: 34 RVA: 0x000038DC File Offset: 0x00001ADC
	private float GetSlopeAngle()
	{
		if (!this.IsOnGround())
		{
			return 0f;
		}
		float num = Vector3.SignedAngle(base.transform.forward, this.m_lastGroundNormal, base.transform.right);
		return -(90f - -num);
	}

	// Token: 0x06000023 RID: 35 RVA: 0x00003924 File Offset: 0x00001B24
	protected void AddPushbackForce(ref Vector3 velocity)
	{
		if (this.m_pushForce != Vector3.zero)
		{
			Vector3 normalized = this.m_pushForce.normalized;
			float num = Vector3.Dot(normalized, velocity);
			if (num < 20f)
			{
				velocity += normalized * (20f - num);
			}
			if (this.IsSwimming() || this.m_flying)
			{
				velocity *= 0.5f;
			}
		}
	}

	// Token: 0x06000024 RID: 36 RVA: 0x000039A8 File Offset: 0x00001BA8
	private void ApplyPushback(HitData hit)
	{
		this.ApplyPushback(hit.m_dir, hit.m_pushForce);
	}

	// Token: 0x06000025 RID: 37 RVA: 0x000039BC File Offset: 0x00001BBC
	public void ApplyPushback(Vector3 dir, float pushForce)
	{
		if (pushForce != 0f && dir != Vector3.zero)
		{
			float d = pushForce * Mathf.Clamp01(1f + this.GetEquipmentMovementModifier()) / this.m_body.mass * 2.5f;
			dir.y = 0f;
			dir.Normalize();
			Vector3 pushForce2 = dir * d;
			if (this.m_pushForce.magnitude < pushForce2.magnitude)
			{
				this.m_pushForce = pushForce2;
			}
		}
	}

	// Token: 0x06000026 RID: 38 RVA: 0x00003A3A File Offset: 0x00001C3A
	private void UpdatePushback(float dt)
	{
		this.m_pushForce = Vector3.MoveTowards(this.m_pushForce, Vector3.zero, 100f * dt);
	}

	// Token: 0x06000027 RID: 39 RVA: 0x00003A5C File Offset: 0x00001C5C
	private void ApplyGroundForce(ref Vector3 vel, Vector3 targetVel)
	{
		Vector3 vector = Vector3.zero;
		if (this.IsOnGround() && this.m_lastGroundBody)
		{
			vector = this.m_lastGroundBody.GetPointVelocity(base.transform.position);
			vector.y = 0f;
		}
		Ship standingOnShip = this.GetStandingOnShip();
		if (standingOnShip != null)
		{
			if (targetVel.magnitude > 0.01f)
			{
				this.m_lastAttachBody = null;
			}
			else if (this.m_lastAttachBody != this.m_lastGroundBody)
			{
				this.m_lastAttachBody = this.m_lastGroundBody;
				this.m_lastAttachPos = this.m_lastAttachBody.transform.InverseTransformPoint(this.m_body.position);
			}
			if (this.m_lastAttachBody)
			{
				Vector3 vector2 = this.m_lastAttachBody.transform.TransformPoint(this.m_lastAttachPos);
				Vector3 a = vector2 - this.m_body.position;
				if (a.magnitude < 4f)
				{
					Vector3 position = vector2;
					position.y = this.m_body.position.y;
					if (standingOnShip.IsOwner())
					{
						a.y = 0f;
						vector += a * 10f;
					}
					else
					{
						this.m_body.position = position;
					}
				}
				else
				{
					this.m_lastAttachBody = null;
				}
			}
		}
		else
		{
			this.m_lastAttachBody = null;
		}
		vel += vector;
	}

	// Token: 0x06000028 RID: 40 RVA: 0x00003BCC File Offset: 0x00001DCC
	private float UpdateRotation(float turnSpeed, float dt, bool smooth)
	{
		Quaternion quaternion = this.AlwaysRotateCamera() ? this.m_lookYaw : Quaternion.LookRotation(this.m_moveDir);
		float yawDeltaAngle = Utils.GetYawDeltaAngle(base.transform.rotation, quaternion);
		float num = 1f;
		if (!this.IsPlayer())
		{
			num = Mathf.Clamp01(Mathf.Abs(yawDeltaAngle) / 90f);
			num = Mathf.Pow(num, 0.5f);
			float num2 = Mathf.Clamp01(Mathf.Abs(yawDeltaAngle) / 90f);
			num2 = Mathf.Pow(num2, 0.5f);
			if (smooth)
			{
				this.currentRotSpeedFactor = Mathf.MoveTowards(this.currentRotSpeedFactor, num2, dt);
				num = this.currentRotSpeedFactor;
			}
			else
			{
				num = num2;
			}
		}
		float num3 = turnSpeed * this.GetAttackSpeedFactorRotation() * num;
		Quaternion rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, num3 * dt);
		if (Mathf.Abs(yawDeltaAngle) > 0.001f)
		{
			base.transform.rotation = rotation;
		}
		return num3 * Mathf.Sign(yawDeltaAngle) * 0.017453292f;
	}

	// Token: 0x06000029 RID: 41 RVA: 0x00003CC0 File Offset: 0x00001EC0
	private void UpdateGroundTilt(float dt)
	{
		if (this.m_visual == null)
		{
			return;
		}
		if (this.m_baseAI && this.m_baseAI.IsSleeping())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			if (this.m_groundTilt != Character.GroundTiltType.None)
			{
				if (!this.IsFlying() && this.IsOnGround() && !this.IsAttached())
				{
					Vector3 vector = this.m_lastGroundNormal;
					if (this.m_groundTilt == Character.GroundTiltType.PitchRaycast || this.m_groundTilt == Character.GroundTiltType.FullRaycast)
					{
						Vector3 p = base.transform.position + base.transform.forward * this.m_collider.radius;
						Vector3 p2 = base.transform.position - base.transform.forward * this.m_collider.radius;
						float num;
						Vector3 b;
						this.GetGroundHeight(p, out num, out b);
						float num2;
						Vector3 b2;
						this.GetGroundHeight(p2, out num2, out b2);
						vector = (vector + b + b2).normalized;
					}
					Vector3 vector2 = base.transform.InverseTransformVector(vector);
					vector2 = Vector3.RotateTowards(Vector3.up, vector2, 0.87266463f, 1f);
					this.m_groundTiltNormal = Vector3.Lerp(this.m_groundTiltNormal, vector2, 0.05f);
					Vector3 vector3;
					if (this.m_groundTilt == Character.GroundTiltType.Pitch || this.m_groundTilt == Character.GroundTiltType.PitchRaycast)
					{
						Vector3 b3 = Vector3.Project(this.m_groundTiltNormal, Vector3.right);
						vector3 = this.m_groundTiltNormal - b3;
					}
					else
					{
						vector3 = this.m_groundTiltNormal;
					}
					Quaternion to = Quaternion.LookRotation(Vector3.Cross(vector3, Vector3.left), vector3);
					this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, to, dt * this.m_groundTiltSpeed);
				}
				else
				{
					this.m_groundTiltNormal = Vector3.up;
					if (this.IsSwimming())
					{
						this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, Quaternion.identity, dt * this.m_groundTiltSpeed);
					}
					else
					{
						this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, Quaternion.identity, dt * this.m_groundTiltSpeed * 2f);
					}
				}
				this.m_nview.GetZDO().Set(ZDOVars.s_tiltrot, this.m_visual.transform.localRotation);
				return;
			}
			if (this.CanWallRun())
			{
				if (this.m_wallRunning)
				{
					Vector3 vector4 = Vector3.Lerp(Vector3.up, this.m_lastGroundNormal, 0.65f);
					Vector3 forward = Vector3.ProjectOnPlane(base.transform.forward, vector4);
					forward.Normalize();
					Quaternion to2 = Quaternion.LookRotation(forward, vector4);
					this.m_visual.transform.rotation = Quaternion.RotateTowards(this.m_visual.transform.rotation, to2, 30f * dt);
				}
				else
				{
					this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, Quaternion.identity, dt * this.m_groundTiltSpeed * 2f);
				}
				this.m_nview.GetZDO().Set(ZDOVars.s_tiltrot, this.m_visual.transform.localRotation);
				return;
			}
		}
		else if (this.m_groundTilt != Character.GroundTiltType.None || this.CanWallRun())
		{
			Quaternion quaternion = this.m_nview.GetZDO().GetQuaternion(ZDOVars.s_tiltrot, Quaternion.identity);
			this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, quaternion, dt * this.m_groundTiltSpeed);
		}
	}

	// Token: 0x0600002A RID: 42 RVA: 0x00004074 File Offset: 0x00002274
	private bool GetGroundHeight(Vector3 p, out float height, out Vector3 normal)
	{
		p.y += 10f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 20f, Character.s_groundRayMask))
		{
			height = raycastHit.point.y;
			normal = raycastHit.normal;
			return true;
		}
		height = p.y;
		normal = Vector3.zero;
		return false;
	}

	// Token: 0x0600002B RID: 43 RVA: 0x000040DB File Offset: 0x000022DB
	public bool IsWallRunning()
	{
		return this.m_wallRunning;
	}

	// Token: 0x0600002C RID: 44 RVA: 0x0000247B File Offset: 0x0000067B
	private bool IsOnSnow()
	{
		return false;
	}

	// Token: 0x0600002D RID: 45 RVA: 0x000040E4 File Offset: 0x000022E4
	public void Heal(float hp, bool showText = true)
	{
		if (hp <= 0f)
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_Heal(0L, hp, showText);
			return;
		}
		this.m_nview.InvokeRPC("Heal", new object[]
		{
			hp,
			showText
		});
	}

	// Token: 0x0600002E RID: 46 RVA: 0x0000413C File Offset: 0x0000233C
	private void RPC_Heal(long sender, float hp, bool showText)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float health = this.GetHealth();
		if (health <= 0f || this.IsDead())
		{
			return;
		}
		float num = Mathf.Min(health + hp, this.GetMaxHealth());
		if (num > health)
		{
			this.SetHealth(num);
			if (showText)
			{
				Vector3 topPoint = this.GetTopPoint();
				DamageText.instance.ShowText(DamageText.TextType.Heal, topPoint, hp, this.IsPlayer());
			}
		}
	}

	// Token: 0x0600002F RID: 47 RVA: 0x000041A8 File Offset: 0x000023A8
	public Vector3 GetTopPoint()
	{
		return base.transform.TransformPoint(this.m_collider.center) + this.m_visual.transform.up * this.m_collider.height * 0.5f;
	}

	// Token: 0x06000030 RID: 48 RVA: 0x000041FA File Offset: 0x000023FA
	public float GetRadius()
	{
		return this.m_collider.radius;
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00004207 File Offset: 0x00002407
	public float GetHeight()
	{
		return Mathf.Max(this.m_collider.height, this.m_collider.radius * 2f);
	}

	// Token: 0x06000032 RID: 50 RVA: 0x0000422A File Offset: 0x0000242A
	public Vector3 GetHeadPoint()
	{
		return this.m_head.position;
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00004237 File Offset: 0x00002437
	public Vector3 GetEyePoint()
	{
		return this.m_eye.position;
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00004244 File Offset: 0x00002444
	public Vector3 GetCenterPoint()
	{
		return this.m_collider.bounds.center;
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00004264 File Offset: 0x00002464
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Character;
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00004268 File Offset: 0x00002468
	private short FindWeakSpotIndex(Collider c)
	{
		if (c == null || this.m_weakSpots == null || this.m_weakSpots.Length == 0)
		{
			return -1;
		}
		short num = 0;
		while ((int)num < this.m_weakSpots.Length)
		{
			if (this.m_weakSpots[(int)num].m_collider == c)
			{
				return num;
			}
			num += 1;
		}
		return -1;
	}

	// Token: 0x06000037 RID: 55 RVA: 0x000042BD File Offset: 0x000024BD
	private WeakSpot GetWeakSpot(short index)
	{
		if (index < 0 || (int)index >= this.m_weakSpots.Length)
		{
			return null;
		}
		return this.m_weakSpots[(int)index];
	}

	// Token: 0x06000038 RID: 56 RVA: 0x000042D8 File Offset: 0x000024D8
	public void Damage(HitData hit)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		hit.m_weakSpot = this.FindWeakSpotIndex(hit.m_hitCollider);
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit
		});
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00004314 File Offset: 0x00002514
	private void RPC_Damage(long sender, HitData hit)
	{
		if (this.IsDebugFlying())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetHealth() <= 0f || this.IsDead() || this.IsTeleporting() || this.InCutscene())
		{
			return;
		}
		if (hit.m_dodgeable && this.IsDodgeInvincible())
		{
			return;
		}
		Character attacker = hit.GetAttacker();
		if (hit.HaveAttacker() && attacker == null)
		{
			return;
		}
		if (this.IsPlayer() && !this.IsPVPEnabled() && attacker != null && attacker.IsPlayer() && !hit.m_ignorePVP)
		{
			return;
		}
		if (attacker != null && !attacker.IsPlayer())
		{
			float difficultyDamageScalePlayer = Game.instance.GetDifficultyDamageScalePlayer(base.transform.position);
			hit.ApplyModifier(difficultyDamageScalePlayer);
		}
		this.m_seman.OnDamaged(hit, attacker);
		if (this.m_baseAI != null && this.m_baseAI.IsAggravatable() && !this.m_baseAI.IsAggravated() && attacker && attacker.IsPlayer() && hit.GetTotalDamage() > 0f)
		{
			BaseAI.AggravateAllInArea(base.transform.position, 20f, BaseAI.AggravatedReason.Damage);
		}
		if (this.m_baseAI != null && !this.m_baseAI.IsAlerted() && hit.m_backstabBonus > 1f && Time.time - this.m_backstabTime > 300f)
		{
			this.m_backstabTime = Time.time;
			hit.ApplyModifier(hit.m_backstabBonus);
			this.m_backstabHitEffects.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
		}
		if (this.IsStaggering() && !this.IsPlayer())
		{
			hit.ApplyModifier(2f);
			this.m_critHitEffects.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
		}
		if (hit.m_blockable && this.IsBlocking())
		{
			this.BlockAttack(hit, attacker);
		}
		this.ApplyPushback(hit);
		if (hit.m_statusEffectHash != 0)
		{
			StatusEffect statusEffect = this.m_seman.GetStatusEffect(hit.m_statusEffectHash);
			if (statusEffect == null)
			{
				statusEffect = this.m_seman.AddStatusEffect(hit.m_statusEffectHash, false, (int)hit.m_itemLevel, hit.m_skillLevel);
			}
			else
			{
				statusEffect.ResetTime();
				statusEffect.SetLevel((int)hit.m_itemLevel, hit.m_skillLevel);
			}
			if (statusEffect != null && attacker != null)
			{
				statusEffect.SetAttacker(attacker);
			}
		}
		WeakSpot weakSpot = this.GetWeakSpot(hit.m_weakSpot);
		if (weakSpot != null)
		{
			ZLog.Log("HIT Weakspot:" + weakSpot.gameObject.name);
		}
		HitData.DamageModifiers damageModifiers = this.GetDamageModifiers(weakSpot);
		HitData.DamageModifier mod;
		hit.ApplyResistance(damageModifiers, out mod);
		if (this.IsPlayer())
		{
			float bodyArmor = this.GetBodyArmor();
			hit.ApplyArmor(bodyArmor);
			this.DamageArmorDurability(hit);
		}
		float poison = hit.m_damage.m_poison;
		float fire = hit.m_damage.m_fire;
		float spirit = hit.m_damage.m_spirit;
		hit.m_damage.m_poison = 0f;
		hit.m_damage.m_fire = 0f;
		hit.m_damage.m_spirit = 0f;
		this.ApplyDamage(hit, true, true, mod);
		this.AddFireDamage(fire);
		this.AddSpiritDamage(spirit);
		this.AddPoisonDamage(poison);
		this.AddFrostDamage(hit.m_damage.m_frost);
		this.AddLightningDamage(hit.m_damage.m_lightning);
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00004694 File Offset: 0x00002894
	protected HitData.DamageModifier GetDamageModifier(HitData.DamageType damageType)
	{
		return this.GetDamageModifiers(null).GetModifier(damageType);
	}

	// Token: 0x0600003B RID: 59 RVA: 0x000046B4 File Offset: 0x000028B4
	protected HitData.DamageModifiers GetDamageModifiers(WeakSpot weakspot = null)
	{
		HitData.DamageModifiers result = weakspot ? weakspot.m_damageModifiers.Clone() : this.m_damageModifiers.Clone();
		this.ApplyArmorDamageMods(ref result);
		this.m_seman.ApplyDamageMods(ref result);
		return result;
	}

	// Token: 0x0600003C RID: 60 RVA: 0x000046F8 File Offset: 0x000028F8
	public void ApplyDamage(HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal)
	{
		if (this.IsDebugFlying() || this.IsDead() || this.IsTeleporting() || this.InCutscene())
		{
			return;
		}
		float totalDamage = hit.GetTotalDamage();
		if (!this.IsPlayer())
		{
			float difficultyDamageScaleEnemy = Game.instance.GetDifficultyDamageScaleEnemy(base.transform.position);
			hit.ApplyModifier(difficultyDamageScaleEnemy);
		}
		float totalDamage2 = hit.GetTotalDamage();
		if (totalDamage2 <= 0.1f)
		{
			return;
		}
		if (showDamageText && (totalDamage2 > 0f || !this.IsPlayer()))
		{
			DamageText.instance.ShowText(mod, hit.m_point, totalDamage, this.IsPlayer() || this.IsTamed());
		}
		if (!this.InGodMode() && !this.InGhostMode())
		{
			float num = this.GetHealth();
			num -= totalDamage2;
			this.SetHealth(num);
		}
		float totalStaggerDamage = hit.m_damage.GetTotalStaggerDamage();
		this.AddStaggerDamage(totalStaggerDamage * hit.m_staggerMultiplier, hit.m_dir);
		if (triggerEffects && totalDamage2 > this.GetMaxHealth() / 10f)
		{
			this.DoDamageCameraShake(hit);
			if (hit.m_damage.GetTotalPhysicalDamage() > 0f)
			{
				this.m_hitEffects.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
			}
		}
		this.OnDamaged(hit);
		if (this.m_onDamaged != null)
		{
			this.m_onDamaged(totalDamage2, hit.GetAttacker());
		}
		if (Character.s_dpsDebugEnabled)
		{
			Character.AddDPS(totalDamage2, this);
		}
	}

	// Token: 0x0600003D RID: 61 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void DoDamageCameraShake(HitData hit)
	{
	}

	// Token: 0x0600003E RID: 62 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void DamageArmorDurability(HitData hit)
	{
	}

	// Token: 0x0600003F RID: 63 RVA: 0x00004860 File Offset: 0x00002A60
	private void AddFireDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Burning se_Burning = this.m_seman.GetStatusEffect(Character.s_statusEffectBurning) as SE_Burning;
		if (se_Burning == null)
		{
			se_Burning = (this.m_seman.AddStatusEffect(Character.s_statusEffectBurning, false, 0, 0f) as SE_Burning);
		}
		if (!se_Burning.AddFireDamage(damage))
		{
			this.m_seman.RemoveStatusEffect(se_Burning, false);
		}
	}

	// Token: 0x06000040 RID: 64 RVA: 0x000048CC File Offset: 0x00002ACC
	private void AddSpiritDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Burning se_Burning = this.m_seman.GetStatusEffect(Character.s_statusEffectSpirit) as SE_Burning;
		if (se_Burning == null)
		{
			se_Burning = (this.m_seman.AddStatusEffect(Character.s_statusEffectSpirit, false, 0, 0f) as SE_Burning);
		}
		if (!se_Burning.AddSpiritDamage(damage))
		{
			this.m_seman.RemoveStatusEffect(se_Burning, false);
		}
	}

	// Token: 0x06000041 RID: 65 RVA: 0x00004938 File Offset: 0x00002B38
	private void AddPoisonDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Poison se_Poison = this.m_seman.GetStatusEffect(Character.s_statusEffectPoison) as SE_Poison;
		if (se_Poison == null)
		{
			se_Poison = (this.m_seman.AddStatusEffect(Character.s_statusEffectPoison, false, 0, 0f) as SE_Poison);
		}
		se_Poison.AddDamage(damage);
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00004994 File Offset: 0x00002B94
	private void AddFrostDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Frost se_Frost = this.m_seman.GetStatusEffect(Character.s_statusEffectFrost) as SE_Frost;
		if (se_Frost == null)
		{
			se_Frost = (this.m_seman.AddStatusEffect(Character.s_statusEffectFrost, false, 0, 0f) as SE_Frost);
		}
		se_Frost.AddDamage(damage);
	}

	// Token: 0x06000043 RID: 67 RVA: 0x000049ED File Offset: 0x00002BED
	private void AddLightningDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		this.m_seman.AddStatusEffect(Character.s_statusEffectLightning, true, 0, 0f);
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00004A10 File Offset: 0x00002C10
	private static void AddDPS(float damage, Character me)
	{
		if (me == Player.m_localPlayer)
		{
			Character.CalculateDPS("To-you ", Character.s_playerDamage, damage);
			return;
		}
		Character.CalculateDPS("To-others ", Character.s_enemyDamage, damage);
	}

	// Token: 0x06000045 RID: 69 RVA: 0x00004A40 File Offset: 0x00002C40
	private static void CalculateDPS(string name, List<KeyValuePair<float, float>> damages, float damage)
	{
		float time = Time.time;
		if (damages.Count > 0 && Time.time - damages[damages.Count - 1].Key > 5f)
		{
			damages.Clear();
		}
		damages.Add(new KeyValuePair<float, float>(time, damage));
		float num = Time.time - damages[0].Key;
		if (num < 0.01f)
		{
			return;
		}
		float num2 = 0f;
		foreach (KeyValuePair<float, float> keyValuePair in damages)
		{
			num2 += keyValuePair.Value;
		}
		float num3 = num2 / num;
		string text = string.Concat(new string[]
		{
			"DPS ",
			name,
			" ( ",
			damages.Count.ToString(),
			" attacks, ",
			num.ToString("0.0"),
			"s ): ",
			num3.ToString("0.0")
		});
		ZLog.Log(text);
		MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text, 0, null);
	}

	// Token: 0x06000046 RID: 70 RVA: 0x00004B7C File Offset: 0x00002D7C
	public float GetStaggerPercentage()
	{
		return Mathf.Clamp01(this.m_staggerDamage / this.GetStaggerTreshold());
	}

	// Token: 0x06000047 RID: 71 RVA: 0x00004B90 File Offset: 0x00002D90
	private float GetStaggerTreshold()
	{
		return this.GetMaxHealth() * this.m_staggerDamageFactor;
	}

	// Token: 0x06000048 RID: 72 RVA: 0x00004BA0 File Offset: 0x00002DA0
	protected bool AddStaggerDamage(float damage, Vector3 forceDirection)
	{
		if (this.m_staggerDamageFactor <= 0f)
		{
			return false;
		}
		this.m_staggerDamage += damage;
		float staggerTreshold = this.GetStaggerTreshold();
		if (this.m_staggerDamage >= staggerTreshold)
		{
			this.m_staggerDamage = staggerTreshold;
			this.Stagger(forceDirection);
			if (this.IsPlayer())
			{
				Hud.instance.StaggerBarFlash();
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000049 RID: 73 RVA: 0x00004C00 File Offset: 0x00002E00
	private void UpdateStagger(float dt)
	{
		if (this.m_staggerDamageFactor <= 0f && !this.IsPlayer())
		{
			return;
		}
		float num = this.GetMaxHealth() * this.m_staggerDamageFactor;
		this.m_staggerDamage -= num / 5f * dt;
		if (this.m_staggerDamage < 0f)
		{
			this.m_staggerDamage = 0f;
		}
	}

	// Token: 0x0600004A RID: 74 RVA: 0x00004C5F File Offset: 0x00002E5F
	public void Stagger(Vector3 forceDirection)
	{
		if (this.m_nview.IsOwner())
		{
			this.RPC_Stagger(0L, forceDirection);
			return;
		}
		this.m_nview.InvokeRPC("Stagger", new object[]
		{
			forceDirection
		});
	}

	// Token: 0x0600004B RID: 75 RVA: 0x00004C98 File Offset: 0x00002E98
	private void RPC_Stagger(long sender, Vector3 forceDirection)
	{
		if (!this.IsStaggering())
		{
			if (forceDirection.magnitude > 0.01f)
			{
				forceDirection.y = 0f;
				base.transform.rotation = Quaternion.LookRotation(-forceDirection);
			}
			this.m_zanim.SetSpeed(1f);
			this.m_zanim.SetTrigger("stagger");
		}
	}

	// Token: 0x0600004C RID: 76 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
	}

	// Token: 0x0600004D RID: 77 RVA: 0x00004CFD File Offset: 0x00002EFD
	public virtual float GetBodyArmor()
	{
		return 0f;
	}

	// Token: 0x0600004E RID: 78 RVA: 0x0000247B File Offset: 0x0000067B
	protected virtual bool BlockAttack(HitData hit, Character attacker)
	{
		return false;
	}

	// Token: 0x0600004F RID: 79 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void OnDamaged(HitData hit)
	{
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00004D04 File Offset: 0x00002F04
	private void OnCollisionStay(Collision collision)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_jumpTimer < 0.1f)
		{
			return;
		}
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			float num = contactPoint.point.y - base.transform.position.y;
			if (contactPoint.normal.y > 0.1f && num < this.m_collider.radius)
			{
				if (contactPoint.normal.y > this.m_groundContactNormal.y || !this.m_groundContact)
				{
					this.m_groundContact = true;
					this.m_groundContactNormal = contactPoint.normal;
					this.m_groundContactPoint = contactPoint.point;
					this.m_lowestContactCollider = collision.collider;
				}
				else
				{
					Vector3 vector = Vector3.Normalize(this.m_groundContactNormal + contactPoint.normal);
					if (vector.y > this.m_groundContactNormal.y)
					{
						this.m_groundContactNormal = vector;
						this.m_groundContactPoint = (this.m_groundContactPoint + contactPoint.point) * 0.5f;
					}
				}
			}
		}
	}

	// Token: 0x06000051 RID: 81 RVA: 0x00004E4C File Offset: 0x0000304C
	private void UpdateGroundContact(float dt)
	{
		if (!this.m_groundContact)
		{
			return;
		}
		this.m_lastGroundCollider = this.m_lowestContactCollider;
		this.m_lastGroundNormal = this.m_groundContactNormal;
		this.m_lastGroundPoint = this.m_groundContactPoint;
		this.m_lastGroundBody = (this.m_lastGroundCollider ? this.m_lastGroundCollider.attachedRigidbody : null);
		if (!this.IsPlayer() && this.m_lastGroundBody != null && this.m_lastGroundBody.gameObject.layer == base.gameObject.layer)
		{
			this.m_lastGroundCollider = null;
			this.m_lastGroundBody = null;
		}
		float num = Mathf.Max(0f, this.m_maxAirAltitude - base.transform.position.y);
		if (num > 0.8f && this.m_onLand != null)
		{
			Vector3 lastGroundPoint = this.m_lastGroundPoint;
			if (this.InLiquid())
			{
				lastGroundPoint.y = this.GetLiquidLevel();
			}
			this.m_onLand(this.m_lastGroundPoint);
		}
		if (this.IsPlayer() && num > 4f)
		{
			float num2 = Mathf.Clamp01((num - 4f) / 16f) * 100f;
			this.m_seman.ModifyFallDamage(num2, ref num2);
			if (num2 > 0f)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = num2;
				hitData.m_point = this.m_lastGroundPoint;
				hitData.m_dir = this.m_lastGroundNormal;
				this.Damage(hitData);
			}
		}
		this.ResetGroundContact();
		this.m_lastGroundTouch = 0f;
		this.m_maxAirAltitude = base.transform.position.y;
	}

	// Token: 0x06000052 RID: 82 RVA: 0x00004FDE File Offset: 0x000031DE
	private void ResetGroundContact()
	{
		this.m_lowestContactCollider = null;
		this.m_groundContact = false;
		this.m_groundContactNormal = Vector3.zero;
		this.m_groundContactPoint = Vector3.zero;
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00005004 File Offset: 0x00003204
	public Ship GetStandingOnShip()
	{
		if (this.InNumShipVolumes == 0)
		{
			return null;
		}
		if (!this.IsOnGround())
		{
			return null;
		}
		if (this.m_lastGroundBody)
		{
			return this.m_lastGroundBody.GetComponent<Ship>();
		}
		return null;
	}

	// Token: 0x06000054 RID: 84 RVA: 0x00005034 File Offset: 0x00003234
	public bool IsOnGround()
	{
		return this.m_lastGroundTouch < 0.2f || this.m_body.IsSleeping();
	}

	// Token: 0x06000055 RID: 85 RVA: 0x00005050 File Offset: 0x00003250
	private void CheckDeath()
	{
		if (this.IsDead())
		{
			return;
		}
		if (this.GetHealth() <= 0f)
		{
			this.OnDeath();
		}
	}

	// Token: 0x06000056 RID: 86 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void OnRagdollCreated(Ragdoll ragdoll)
	{
	}

	// Token: 0x06000057 RID: 87 RVA: 0x00005070 File Offset: 0x00003270
	protected virtual void OnDeath()
	{
		GameObject[] array = this.m_deathEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		for (int i = 0; i < array.Length; i++)
		{
			Ragdoll component = array[i].GetComponent<Ragdoll>();
			if (component)
			{
				CharacterDrop component2 = base.GetComponent<CharacterDrop>();
				LevelEffects componentInChildren = base.GetComponentInChildren<LevelEffects>();
				Vector3 velocity = this.m_body.velocity;
				if (this.m_pushForce.magnitude * 0.5f > velocity.magnitude)
				{
					velocity = this.m_pushForce * 0.5f;
				}
				float hue = 0f;
				float saturation = 0f;
				float value = 0f;
				if (componentInChildren)
				{
					componentInChildren.GetColorChanges(out hue, out saturation, out value);
				}
				component.Setup(velocity, hue, saturation, value, component2);
				this.OnRagdollCreated(component);
				if (component2)
				{
					component2.SetDropsEnabled(false);
				}
			}
		}
		if (!string.IsNullOrEmpty(this.m_defeatSetGlobalKey))
		{
			ZoneSystem.instance.SetGlobalKey(this.m_defeatSetGlobalKey);
		}
		if (this.m_onDeath != null)
		{
			this.m_onDeath();
		}
		ZNetScene.instance.Destroy(base.gameObject);
		Gogan.LogEvent("Game", "Killed", this.m_name, 0L);
	}

	// Token: 0x06000058 RID: 88 RVA: 0x000051C4 File Offset: 0x000033C4
	public float GetHealth()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null)
		{
			return this.GetMaxHealth();
		}
		return zdo.GetFloat(ZDOVars.s_health, this.GetMaxHealth());
	}

	// Token: 0x06000059 RID: 89 RVA: 0x000051F8 File Offset: 0x000033F8
	public void SetHealth(float health)
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null || !this.m_nview.IsOwner())
		{
			return;
		}
		if (health < 0f)
		{
			health = 0f;
		}
		zdo.Set(ZDOVars.s_health, health);
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00005240 File Offset: 0x00003440
	public void UseHealth(float hp)
	{
		if (hp <= 0f)
		{
			return;
		}
		float num = this.GetHealth();
		num -= hp;
		num = Mathf.Clamp(num, 0f, this.GetMaxHealth());
		this.SetHealth(num);
		if (this.IsPlayer())
		{
			Hud.instance.DamageFlash();
		}
	}

	// Token: 0x0600005B RID: 91 RVA: 0x0000528C File Offset: 0x0000348C
	public float GetHealthPercentage()
	{
		return this.GetHealth() / this.GetMaxHealth();
	}

	// Token: 0x0600005C RID: 92 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsDead()
	{
		return false;
	}

	// Token: 0x0600005D RID: 93 RVA: 0x0000529B File Offset: 0x0000349B
	public void SetMaxHealth(float health)
	{
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_maxHealth, health);
		}
		if (this.GetHealth() > health)
		{
			this.SetHealth(health);
		}
	}

	// Token: 0x0600005E RID: 94 RVA: 0x000052D0 File Offset: 0x000034D0
	public float GetMaxHealth()
	{
		if (this.m_nview.GetZDO() != null)
		{
			return this.m_nview.GetZDO().GetFloat(ZDOVars.s_maxHealth, this.m_health);
		}
		return this.m_health;
	}

	// Token: 0x0600005F RID: 95 RVA: 0x00004CFD File Offset: 0x00002EFD
	public virtual float GetMaxStamina()
	{
		return 0f;
	}

	// Token: 0x06000060 RID: 96 RVA: 0x00004CFD File Offset: 0x00002EFD
	public virtual float GetMaxEitr()
	{
		return 0f;
	}

	// Token: 0x06000061 RID: 97 RVA: 0x00005301 File Offset: 0x00003501
	public virtual float GetEitrPercentage()
	{
		return 1f;
	}

	// Token: 0x06000062 RID: 98 RVA: 0x00005301 File Offset: 0x00003501
	public virtual float GetStaminaPercentage()
	{
		return 1f;
	}

	// Token: 0x06000063 RID: 99 RVA: 0x00005308 File Offset: 0x00003508
	public bool IsBoss()
	{
		return this.m_boss;
	}

	// Token: 0x06000064 RID: 100 RVA: 0x00005310 File Offset: 0x00003510
	public void SetLookDir(Vector3 dir, float transitionTime = 0f)
	{
		if (transitionTime > 0f)
		{
			this.m_lookTransitionTimeTotal = transitionTime;
			this.m_lookTransitionTime = transitionTime;
			this.m_lookTransitionStart = this.GetLookDir();
			this.m_lookTransitionTarget = Vector3.Normalize(dir);
			return;
		}
		if (dir.magnitude <= Mathf.Epsilon)
		{
			dir = base.transform.forward;
		}
		else
		{
			dir.Normalize();
		}
		this.m_lookDir = dir;
		dir.y = 0f;
		this.m_lookYaw = Quaternion.LookRotation(dir);
	}

	// Token: 0x06000065 RID: 101 RVA: 0x00005394 File Offset: 0x00003594
	private void UpdateLookTransition(float dt)
	{
		if (this.m_lookTransitionTime > 0f)
		{
			this.SetLookDir(Vector3.Lerp(this.m_lookTransitionTarget, this.m_lookTransitionStart, Mathf.SmoothStep(0f, 1f, this.m_lookTransitionTime / this.m_lookTransitionTimeTotal)), 0f);
			this.m_lookTransitionTime -= dt;
		}
	}

	// Token: 0x06000066 RID: 102 RVA: 0x000053F4 File Offset: 0x000035F4
	public Vector3 GetLookDir()
	{
		return this.m_eye.forward;
	}

	// Token: 0x06000067 RID: 103 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void OnAttackTrigger()
	{
	}

	// Token: 0x06000068 RID: 104 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void OnStopMoving()
	{
	}

	// Token: 0x06000069 RID: 105 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void OnWeaponTrailStart()
	{
	}

	// Token: 0x0600006A RID: 106 RVA: 0x00005401 File Offset: 0x00003601
	public void SetMoveDir(Vector3 dir)
	{
		this.m_moveDir = dir;
	}

	// Token: 0x0600006B RID: 107 RVA: 0x0000540A File Offset: 0x0000360A
	public void SetRun(bool run)
	{
		this.m_run = run;
	}

	// Token: 0x0600006C RID: 108 RVA: 0x00005413 File Offset: 0x00003613
	public void SetWalk(bool walk)
	{
		this.m_walk = walk;
	}

	// Token: 0x0600006D RID: 109 RVA: 0x0000541C File Offset: 0x0000361C
	public bool GetWalk()
	{
		return this.m_walk;
	}

	// Token: 0x0600006E RID: 110 RVA: 0x00005424 File Offset: 0x00003624
	protected virtual void UpdateEyeRotation()
	{
		this.m_eye.rotation = Quaternion.LookRotation(this.m_lookDir);
	}

	// Token: 0x0600006F RID: 111 RVA: 0x0000543C File Offset: 0x0000363C
	public void OnAutoJump(Vector3 dir, float upVel, float forwardVel)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.IsOnGround() || this.IsDead() || this.InAttack() || this.InDodge() || this.IsKnockedBack())
		{
			return;
		}
		if (Time.time - this.m_lastAutoJumpTime < 0.5f)
		{
			return;
		}
		this.m_lastAutoJumpTime = Time.time;
		if (Vector3.Dot(this.m_moveDir, dir) < 0.5f)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		vector.y = upVel;
		vector += dir * forwardVel;
		this.m_body.velocity = vector;
		this.m_lastGroundTouch = 1f;
		this.m_jumpTimer = 0f;
		this.m_jumpEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		this.SetCrouch(false);
		this.UpdateBodyFriction();
	}

	// Token: 0x06000070 RID: 112 RVA: 0x0000553C File Offset: 0x0000373C
	public void Jump(bool force = false)
	{
		if (this.IsOnGround() && !this.IsDead() && (force || !this.InAttack()) && !this.IsEncumbered() && !this.InDodge() && !this.IsKnockedBack() && !this.IsStaggering())
		{
			bool flag = false;
			if (!this.HaveStamina(this.m_jumpStaminaUsage))
			{
				if (this.IsPlayer())
				{
					Hud.instance.StaminaBarEmptyFlash();
				}
				flag = true;
			}
			float speed = this.m_speed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref speed);
			if (speed <= 0f)
			{
				flag = true;
			}
			float num = 0f;
			Skills skills = this.GetSkills();
			if (skills != null)
			{
				num = skills.GetSkillFactor(Skills.SkillType.Jump);
				if (!flag)
				{
					this.RaiseSkill(Skills.SkillType.Jump, 1f);
				}
			}
			Vector3 vector = this.m_body.velocity;
			Mathf.Acos(Mathf.Clamp01(this.m_lastGroundNormal.y));
			Vector3 normalized = (this.m_lastGroundNormal + Vector3.up).normalized;
			float num2 = 1f + num * 0.4f;
			float num3 = this.m_jumpForce * num2;
			float num4 = Vector3.Dot(normalized, vector);
			if (num4 < num3)
			{
				vector += normalized * (num3 - num4);
			}
			if (this.IsPlayer())
			{
				vector += this.m_moveDir * this.m_jumpForceForward * num2;
			}
			else
			{
				vector += base.transform.forward * this.m_jumpForceForward * num2;
			}
			if (flag)
			{
				vector *= this.m_jumpForceTiredFactor;
			}
			this.m_seman.ApplyStatusEffectJumpMods(ref vector);
			if (vector.x <= 0f && vector.y <= 0f && vector.z <= 0f)
			{
				return;
			}
			this.m_body.WakeUp();
			this.m_body.velocity = vector;
			this.ResetGroundContact();
			this.m_lastGroundTouch = 1f;
			this.m_jumpTimer = 0f;
			this.m_zanim.SetTrigger("jump");
			this.AddNoise(30f);
			this.m_jumpEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
			this.ResetCloth();
			this.OnJump();
			this.SetCrouch(false);
			this.UpdateBodyFriction();
		}
	}

	// Token: 0x06000071 RID: 113 RVA: 0x000057BC File Offset: 0x000039BC
	private void UpdateBodyFriction()
	{
		this.m_collider.material.frictionCombine = PhysicMaterialCombine.Multiply;
		if (this.IsDead())
		{
			this.m_collider.material.staticFriction = 1f;
			this.m_collider.material.dynamicFriction = 1f;
			this.m_collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
			return;
		}
		if (this.IsSwimming())
		{
			this.m_collider.material.staticFriction = 0.2f;
			this.m_collider.material.dynamicFriction = 0.2f;
			return;
		}
		if (!this.IsOnGround())
		{
			this.m_collider.material.staticFriction = 0f;
			this.m_collider.material.dynamicFriction = 0f;
			return;
		}
		if (this.IsFlying())
		{
			this.m_collider.material.staticFriction = 0f;
			this.m_collider.material.dynamicFriction = 0f;
			return;
		}
		if (this.m_moveDir.magnitude < 0.1f)
		{
			this.m_collider.material.staticFriction = 0.8f * (1f - this.m_slippage);
			this.m_collider.material.dynamicFriction = 0.8f * (1f - this.m_slippage);
			this.m_collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
			return;
		}
		this.m_collider.material.staticFriction = 0.4f * (1f - this.m_slippage);
		this.m_collider.material.dynamicFriction = 0.4f * (1f - this.m_slippage);
	}

	// Token: 0x06000072 RID: 114 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool StartAttack(Character target, bool charge)
	{
		return false;
	}

	// Token: 0x06000073 RID: 115 RVA: 0x00005963 File Offset: 0x00003B63
	public virtual float GetTimeSinceLastAttack()
	{
		return 99999f;
	}

	// Token: 0x06000074 RID: 116 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void OnNearFire(Vector3 point)
	{
	}

	// Token: 0x06000075 RID: 117 RVA: 0x0000596A File Offset: 0x00003B6A
	public ZDOID GetZDOID()
	{
		if (!this.m_nview.IsValid())
		{
			return ZDOID.None;
		}
		return this.m_nview.GetZDO().m_uid;
	}

	// Token: 0x06000076 RID: 118 RVA: 0x0000598F File Offset: 0x00003B8F
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x06000077 RID: 119 RVA: 0x000059AB File Offset: 0x00003BAB
	public long GetOwner()
	{
		if (!this.m_nview.IsValid())
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetOwner();
	}

	// Token: 0x06000078 RID: 120 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool UseMeleeCamera()
	{
		return false;
	}

	// Token: 0x06000079 RID: 121 RVA: 0x0000290F File Offset: 0x00000B0F
	protected virtual bool AlwaysRotateCamera()
	{
		return true;
	}

	// Token: 0x0600007A RID: 122 RVA: 0x000059CD File Offset: 0x00003BCD
	public void SetLiquidLevel(float level, LiquidType type, Component liquidObj)
	{
		if (type != LiquidType.Water)
		{
			if (type == LiquidType.Tar)
			{
				this.m_tarLevel = level;
			}
		}
		else
		{
			this.m_waterLevel = level;
		}
		this.m_liquidLevel = Mathf.Max(this.m_waterLevel, this.m_tarLevel);
	}

	// Token: 0x0600007B RID: 123 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsPVPEnabled()
	{
		return false;
	}

	// Token: 0x0600007C RID: 124 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InIntro()
	{
		return false;
	}

	// Token: 0x0600007D RID: 125 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InCutscene()
	{
		return false;
	}

	// Token: 0x0600007E RID: 126 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsCrouching()
	{
		return false;
	}

	// Token: 0x0600007F RID: 127 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InBed()
	{
		return false;
	}

	// Token: 0x06000080 RID: 128 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsAttached()
	{
		return false;
	}

	// Token: 0x06000081 RID: 129 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsAttachedToShip()
	{
		return false;
	}

	// Token: 0x06000082 RID: 130 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsRiding()
	{
		return false;
	}

	// Token: 0x06000083 RID: 131 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void SetCrouch(bool crouch)
	{
	}

	// Token: 0x06000084 RID: 132 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void AttachStart(Transform attachPoint, GameObject colliderRoot, bool hideWeapons, bool isBed, bool onShip, string attachAnimation, Vector3 detachOffset)
	{
	}

	// Token: 0x06000085 RID: 133 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void AttachStop()
	{
	}

	// Token: 0x06000086 RID: 134 RVA: 0x00005A00 File Offset: 0x00003C00
	private void UpdateWater(float dt)
	{
		this.m_swimTimer += dt;
		float depth = this.InLiquidDepth();
		if (this.m_canSwim && this.InLiquidSwimDepth(depth))
		{
			this.m_swimTimer = 0f;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.InLiquidWetDepth(depth))
		{
			return;
		}
		if (this.m_waterLevel > this.m_tarLevel)
		{
			this.m_seman.AddStatusEffect(Character.s_statusEffectWet, true, 0, 0f);
			return;
		}
		if (!this.m_tolerateTar)
		{
			this.m_seman.AddStatusEffect(Character.s_statusEffectTared, true, 0, 0f);
		}
	}

	// Token: 0x06000087 RID: 135 RVA: 0x00005AA0 File Offset: 0x00003CA0
	private void ApplyLiquidResistance(ref float speed)
	{
		float num = this.InLiquidDepth();
		if (num <= 0f)
		{
			return;
		}
		if (this.m_seman.HaveStatusEffect(Character.s_statusEffectTared))
		{
			return;
		}
		float num2 = (this.m_tarLevel > this.m_waterLevel) ? 0.1f : 0.05f;
		float num3 = this.m_collider.height / 3f;
		float num4 = Mathf.Clamp01(num / num3);
		speed -= speed * speed * num4 * num2;
	}

	// Token: 0x06000088 RID: 136 RVA: 0x00005B14 File Offset: 0x00003D14
	public bool IsSwimming()
	{
		return this.m_swimTimer < 0.5f;
	}

	// Token: 0x06000089 RID: 137 RVA: 0x00005B23 File Offset: 0x00003D23
	private bool InLiquidSwimDepth()
	{
		return this.InLiquidDepth() > Mathf.Max(0f, this.m_swimDepth - 0.4f);
	}

	// Token: 0x0600008A RID: 138 RVA: 0x00005B43 File Offset: 0x00003D43
	private bool InLiquidSwimDepth(float depth)
	{
		return depth > Mathf.Max(0f, this.m_swimDepth - 0.4f);
	}

	// Token: 0x0600008B RID: 139 RVA: 0x00005B5E File Offset: 0x00003D5E
	private bool InLiquidKneeDepth()
	{
		return this.InLiquidDepth() > 0.4f;
	}

	// Token: 0x0600008C RID: 140 RVA: 0x00005B6D File Offset: 0x00003D6D
	private bool InLiquidKneeDepth(float depth)
	{
		return depth > 0.4f;
	}

	// Token: 0x0600008D RID: 141 RVA: 0x00005B77 File Offset: 0x00003D77
	public bool InLiquidWetDepth___NotUsed()
	{
		return this.InLiquidSwimDepth() || (this.IsSitting() && this.InLiquidKneeDepth());
	}

	// Token: 0x0600008E RID: 142 RVA: 0x00005B93 File Offset: 0x00003D93
	private bool InLiquidWetDepth(float depth)
	{
		return this.InLiquidSwimDepth(depth) || (this.IsSitting() && this.InLiquidKneeDepth(depth));
	}

	// Token: 0x0600008F RID: 143 RVA: 0x00005BB4 File Offset: 0x00003DB4
	private float InLiquidDepth()
	{
		if (this.m_cashedInLiquidDepthFrame == Time.frameCount)
		{
			return this.m_cashedInLiquidDepth;
		}
		if (this.GetStandingOnShip() != null || this.IsAttachedToShip())
		{
			this.m_cashedInLiquidDepthFrame = Time.frameCount;
			this.m_cashedInLiquidDepth = 0f;
			return this.m_cashedInLiquidDepth;
		}
		this.m_cashedInLiquidDepth = Mathf.Max(0f, this.GetLiquidLevel() - base.transform.position.y);
		this.m_cashedInLiquidDepthFrame = Time.frameCount;
		return this.m_cashedInLiquidDepth;
	}

	// Token: 0x06000090 RID: 144 RVA: 0x00005C40 File Offset: 0x00003E40
	public float GetLiquidLevel()
	{
		return this.m_liquidLevel;
	}

	// Token: 0x06000091 RID: 145 RVA: 0x00005C48 File Offset: 0x00003E48
	public bool InLiquid()
	{
		return this.InLiquidDepth() > 0f;
	}

	// Token: 0x06000092 RID: 146 RVA: 0x00005C57 File Offset: 0x00003E57
	private bool InTar()
	{
		return this.m_tarLevel > this.m_waterLevel && this.InLiquid();
	}

	// Token: 0x06000093 RID: 147 RVA: 0x00005C6F File Offset: 0x00003E6F
	public bool InWater()
	{
		return this.m_waterLevel > this.m_tarLevel && this.InLiquid();
	}

	// Token: 0x06000094 RID: 148 RVA: 0x00005C87 File Offset: 0x00003E87
	protected virtual bool CheckRun(Vector3 moveDir, float dt)
	{
		return this.m_run && moveDir.magnitude >= 0.1f && !this.IsCrouching() && !this.IsEncumbered() && !this.InDodge();
	}

	// Token: 0x06000095 RID: 149 RVA: 0x00005CC0 File Offset: 0x00003EC0
	public bool IsRunning()
	{
		return this.m_running;
	}

	// Token: 0x06000096 RID: 150 RVA: 0x00005CC8 File Offset: 0x00003EC8
	public bool IsWalking()
	{
		return this.m_walking;
	}

	// Token: 0x06000097 RID: 151 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InPlaceMode()
	{
		return false;
	}

	// Token: 0x06000098 RID: 152 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void AddEitr(float v)
	{
	}

	// Token: 0x06000099 RID: 153 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void UseEitr(float eitr)
	{
	}

	// Token: 0x0600009A RID: 154 RVA: 0x0000290F File Offset: 0x00000B0F
	public virtual bool HaveEitr(float amount = 0f)
	{
		return true;
	}

	// Token: 0x0600009B RID: 155 RVA: 0x0000290F File Offset: 0x00000B0F
	public virtual bool HaveStamina(float amount = 0f)
	{
		return true;
	}

	// Token: 0x0600009C RID: 156 RVA: 0x00005CD0 File Offset: 0x00003ED0
	public bool HaveHealth(float amount = 0f)
	{
		return this.GetHealth() >= amount;
	}

	// Token: 0x0600009D RID: 157 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void AddStamina(float v)
	{
	}

	// Token: 0x0600009E RID: 158 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void UseStamina(float stamina)
	{
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00005CDE File Offset: 0x00003EDE
	protected int GetNextOrCurrentAnimHash()
	{
		if (this.m_cachedAnimHashFrame == MonoUpdaters.UpdateCount)
		{
			return this.m_cachedNextOrCurrentAnimHash;
		}
		this.UpdateCachedAnimHashes();
		return this.m_cachedNextOrCurrentAnimHash;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x00005D00 File Offset: 0x00003F00
	protected int GetCurrentAnimHash()
	{
		if (this.m_cachedAnimHashFrame == MonoUpdaters.UpdateCount)
		{
			return this.m_cachedCurrentAnimHash;
		}
		this.UpdateCachedAnimHashes();
		return this.m_cachedCurrentAnimHash;
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x00005D22 File Offset: 0x00003F22
	protected int GetNextAnimHash()
	{
		if (this.m_cachedAnimHashFrame == MonoUpdaters.UpdateCount)
		{
			return this.m_cachedNextAnimHash;
		}
		this.UpdateCachedAnimHashes();
		return this.m_cachedNextAnimHash;
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x00005D44 File Offset: 0x00003F44
	private void UpdateCachedAnimHashes()
	{
		this.m_cachedAnimHashFrame = MonoUpdaters.UpdateCount;
		this.m_cachedCurrentAnimHash = this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash;
		this.m_cachedNextAnimHash = 0;
		this.m_cachedNextOrCurrentAnimHash = this.m_cachedCurrentAnimHash;
		if (this.m_animator.IsInTransition(0))
		{
			this.m_cachedNextAnimHash = this.m_animator.GetNextAnimatorStateInfo(0).tagHash;
			this.m_cachedNextOrCurrentAnimHash = this.m_cachedNextAnimHash;
		}
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x00005DBD File Offset: 0x00003FBD
	public bool IsStaggering()
	{
		return this.GetNextAnimHash() == Character.s_animatorTagStagger || this.GetCurrentAnimHash() == Character.s_animatorTagStagger;
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x00005DDC File Offset: 0x00003FDC
	public virtual bool CanMove()
	{
		if (this.IsStaggering())
		{
			return false;
		}
		int nextOrCurrentAnimHash = this.GetNextOrCurrentAnimHash();
		return nextOrCurrentAnimHash != Character.s_animatorTagFreeze && nextOrCurrentAnimHash != Character.s_animatorTagSitting;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsEncumbered()
	{
		return false;
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsTeleporting()
	{
		return false;
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00005E0F File Offset: 0x0000400F
	private bool CanWallRun()
	{
		return this.IsPlayer();
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00005E17 File Offset: 0x00004017
	public void ShowPickupMessage(ItemDrop.ItemData item, int amount)
	{
		this.Message(MessageHud.MessageType.TopLeft, "$msg_added " + item.m_shared.m_name, amount, item.GetIcon());
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00005E3C File Offset: 0x0000403C
	public void ShowRemovedMessage(ItemDrop.ItemData item, int amount)
	{
		this.Message(MessageHud.MessageType.TopLeft, "$msg_removed " + item.m_shared.m_name, amount, item.GetIcon());
	}

	// Token: 0x060000AA RID: 170 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void Message(MessageHud.MessageType type, string msg, int amount = 0, Sprite icon = null)
	{
	}

	// Token: 0x060000AB RID: 171 RVA: 0x00005E61 File Offset: 0x00004061
	public CapsuleCollider GetCollider()
	{
		return this.m_collider;
	}

	// Token: 0x060000AC RID: 172 RVA: 0x00005301 File Offset: 0x00003501
	public virtual float GetStealthFactor()
	{
		return 1f;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x00005E6C File Offset: 0x0000406C
	private void UpdateNoise(float dt)
	{
		this.m_noiseRange = Mathf.Max(0f, this.m_noiseRange - dt * 4f);
		this.m_syncNoiseTimer += dt;
		if (this.m_syncNoiseTimer > 0.5f)
		{
			this.m_syncNoiseTimer = 0f;
			this.m_nview.GetZDO().Set(ZDOVars.s_noise, this.m_noiseRange);
		}
	}

	// Token: 0x060000AE RID: 174 RVA: 0x00005ED8 File Offset: 0x000040D8
	public void AddNoise(float range)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_AddNoise(0L, range);
			return;
		}
		this.m_nview.InvokeRPC("AddNoise", new object[]
		{
			range
		});
	}

	// Token: 0x060000AF RID: 175 RVA: 0x00005F29 File Offset: 0x00004129
	private void RPC_AddNoise(long sender, float range)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (range > this.m_noiseRange)
		{
			this.m_noiseRange = range;
			this.m_seman.ModifyNoise(this.m_noiseRange, ref this.m_noiseRange);
		}
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x00005F60 File Offset: 0x00004160
	public float GetNoiseRange()
	{
		if (!this.m_nview.IsValid())
		{
			return 0f;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_noiseRange;
		}
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_noise, 0f);
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InGodMode()
	{
		return false;
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InGhostMode()
	{
		return false;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsDebugFlying()
	{
		return false;
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00005FB0 File Offset: 0x000041B0
	public virtual string GetHoverText()
	{
		Tameable component = base.GetComponent<Tameable>();
		if (component)
		{
			return component.GetHoverText();
		}
		return "";
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x00005FD8 File Offset: 0x000041D8
	public virtual string GetHoverName()
	{
		Tameable component = base.GetComponent<Tameable>();
		if (component)
		{
			return component.GetHoverName();
		}
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsDrawingBow()
	{
		return false;
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InAttack()
	{
		return false;
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void StopEmote()
	{
	}

	// Token: 0x060000B9 RID: 185 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InMinorAction()
	{
		return false;
	}

	// Token: 0x060000BA RID: 186 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InMinorActionSlowdown()
	{
		return false;
	}

	// Token: 0x060000BB RID: 187 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InDodge()
	{
		return false;
	}

	// Token: 0x060000BC RID: 188 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsDodgeInvincible()
	{
		return false;
	}

	// Token: 0x060000BD RID: 189 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool InEmote()
	{
		return false;
	}

	// Token: 0x060000BE RID: 190 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsBlocking()
	{
		return false;
	}

	// Token: 0x060000BF RID: 191 RVA: 0x0000600B File Offset: 0x0000420B
	public bool IsFlying()
	{
		return this.m_flying;
	}

	// Token: 0x060000C0 RID: 192 RVA: 0x00006013 File Offset: 0x00004213
	public bool IsKnockedBack()
	{
		return this.m_pushForce != Vector3.zero;
	}

	// Token: 0x060000C1 RID: 193 RVA: 0x00006028 File Offset: 0x00004228
	private void OnDrawGizmosSelected()
	{
		if (this.m_nview != null && this.m_nview.GetZDO() != null)
		{
			float @float = this.m_nview.GetZDO().GetFloat(ZDOVars.s_noise, 0f);
			Gizmos.DrawWireSphere(base.transform.position, @float);
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_swimDepth, new Vector3(1f, 0.05f, 1f));
		if (this.IsOnGround())
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(this.m_lastGroundPoint, this.m_lastGroundPoint + this.m_lastGroundNormal);
		}
	}

	// Token: 0x060000C2 RID: 194 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		return false;
	}

	// Token: 0x060000C3 RID: 195 RVA: 0x000060ED File Offset: 0x000042ED
	protected void RPC_TeleportTo(long sender, Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.TeleportTo(pos, rot, distantTeleport);
	}

	// Token: 0x060000C4 RID: 196 RVA: 0x00006108 File Offset: 0x00004308
	private void SyncVelocity()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_bodyVelocity, this.m_body.velocity);
	}

	// Token: 0x060000C5 RID: 197 RVA: 0x0000612C File Offset: 0x0000432C
	public Vector3 GetVelocity()
	{
		if (!this.m_nview.IsValid())
		{
			return Vector3.zero;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_body.velocity;
		}
		return this.m_nview.GetZDO().GetVec3(ZDOVars.s_bodyVelocity, Vector3.zero);
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x0000617F File Offset: 0x0000437F
	public void AddRootMotion(Vector3 vel)
	{
		if (this.InDodge() || this.InAttack() || this.InEmote())
		{
			this.m_rootMotion += vel;
		}
	}

	// Token: 0x060000C7 RID: 199 RVA: 0x000061AC File Offset: 0x000043AC
	private void ApplyRootMotion(ref Vector3 vel)
	{
		Vector3 vector = this.m_rootMotion * 55f;
		if (vector.magnitude > vel.magnitude)
		{
			vel = vector;
		}
		this.m_rootMotion = Vector3.zero;
	}

	// Token: 0x060000C8 RID: 200 RVA: 0x000061EC File Offset: 0x000043EC
	public static void GetCharactersInRange(Vector3 point, float radius, List<Character> characters)
	{
		float num = radius * radius;
		foreach (Character character in Character.s_characters)
		{
			if (Utils.DistanceSqr(character.transform.position, point) < num)
			{
				characters.Add(character);
			}
		}
	}

	// Token: 0x060000C9 RID: 201 RVA: 0x00006258 File Offset: 0x00004458
	public static List<Character> GetAllCharacters()
	{
		return Character.s_characters;
	}

	// Token: 0x060000CA RID: 202 RVA: 0x00006260 File Offset: 0x00004460
	public static bool IsCharacterInRange(Vector3 point, float range)
	{
		using (List<Character>.Enumerator enumerator = Character.s_characters.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, point) < range)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060000CB RID: 203 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void OnTargeted(bool sensed, bool alerted)
	{
	}

	// Token: 0x060000CC RID: 204 RVA: 0x000062C4 File Offset: 0x000044C4
	public GameObject GetVisual()
	{
		return this.m_visual;
	}

	// Token: 0x060000CD RID: 205 RVA: 0x000062CC File Offset: 0x000044CC
	protected void UpdateLodgroup()
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		Renderer[] componentsInChildren = this.m_visual.GetComponentsInChildren<Renderer>();
		LOD[] lods = this.m_lodGroup.GetLODs();
		lods[0].renderers = componentsInChildren;
		this.m_lodGroup.SetLODs(lods);
	}

	// Token: 0x060000CE RID: 206 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsSitting()
	{
		return false;
	}

	// Token: 0x060000CF RID: 207 RVA: 0x00004CFD File Offset: 0x00002EFD
	public virtual float GetEquipmentMovementModifier()
	{
		return 0f;
	}

	// Token: 0x060000D0 RID: 208 RVA: 0x00005301 File Offset: 0x00003501
	protected virtual float GetJogSpeedFactor()
	{
		return 1f;
	}

	// Token: 0x060000D1 RID: 209 RVA: 0x0000631C File Offset: 0x0000451C
	protected virtual float GetRunSpeedFactor()
	{
		if (this.HaveRider())
		{
			float riderSkill = this.m_baseAI.GetRiderSkill();
			return 1f + riderSkill * 0.25f;
		}
		return 1f;
	}

	// Token: 0x060000D2 RID: 210 RVA: 0x00005301 File Offset: 0x00003501
	protected virtual float GetAttackSpeedFactorMovement()
	{
		return 1f;
	}

	// Token: 0x060000D3 RID: 211 RVA: 0x00005301 File Offset: 0x00003501
	protected virtual float GetAttackSpeedFactorRotation()
	{
		return 1f;
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x00006350 File Offset: 0x00004550
	public virtual void RaiseSkill(Skills.SkillType skill, float value = 1f)
	{
		if (!this.IsTamed())
		{
			return;
		}
		if (!this.m_tameable)
		{
			this.m_tameable = base.GetComponent<Tameable>();
			this.m_tameableMonsterAI = base.GetComponent<MonsterAI>();
		}
		if (!this.m_tameable || !this.m_tameableMonsterAI)
		{
			ZLog.LogWarning(this.m_name + " is tamed but missing tameable or monster AI script!");
			return;
		}
		if (this.m_tameable.m_levelUpOwnerSkill != Skills.SkillType.None)
		{
			GameObject followTarget = this.m_tameableMonsterAI.GetFollowTarget();
			if (followTarget != null && followTarget)
			{
				Character component = followTarget.GetComponent<Character>();
				if (component != null)
				{
					Skills skills = component.GetSkills();
					if (skills != null)
					{
						skills.RaiseSkill(this.m_tameable.m_levelUpOwnerSkill, value * this.m_tameable.m_levelUpFactor);
						Terminal.Log(string.Format("{0} leveling up from '{1}' to master {2} skill '{3}' at factor {4}", new object[]
						{
							base.name,
							skill,
							component.name,
							this.m_tameable.m_levelUpOwnerSkill,
							value * this.m_tameable.m_levelUpFactor
						}));
					}
				}
			}
		}
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x00006475 File Offset: 0x00004675
	public virtual Skills GetSkills()
	{
		return null;
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x00006478 File Offset: 0x00004678
	public float GetSkillLevel(Skills.SkillType skillType)
	{
		Skills skills = this.GetSkills();
		if (skills != null)
		{
			return skills.GetSkillLevel(skillType);
		}
		return 0f;
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x00004CFD File Offset: 0x00002EFD
	public virtual float GetSkillFactor(Skills.SkillType skill)
	{
		return 0f;
	}

	// Token: 0x060000D8 RID: 216 RVA: 0x0000649C File Offset: 0x0000469C
	public virtual float GetRandomSkillFactor(Skills.SkillType skill)
	{
		return Mathf.Pow(UnityEngine.Random.Range(0.75f, 1f), 0.5f) * this.m_nview.GetZDO().GetFloat(ZDOVars.s_randomSkillFactor, 1f);
	}

	// Token: 0x060000D9 RID: 217 RVA: 0x000064D4 File Offset: 0x000046D4
	public bool IsMonsterFaction(float time)
	{
		return !this.IsTamed(time) && (this.m_faction == Character.Faction.ForestMonsters || this.m_faction == Character.Faction.Undead || this.m_faction == Character.Faction.Demon || this.m_faction == Character.Faction.PlainsMonsters || this.m_faction == Character.Faction.MountainMonsters || this.m_faction == Character.Faction.SeaMonsters || this.m_faction == Character.Faction.MistlandsMonsters);
	}

	// Token: 0x060000DA RID: 218 RVA: 0x0000652E File Offset: 0x0000472E
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x060000DB RID: 219 RVA: 0x00006541 File Offset: 0x00004741
	public Collider GetLastGroundCollider()
	{
		return this.m_lastGroundCollider;
	}

	// Token: 0x060000DC RID: 220 RVA: 0x00006549 File Offset: 0x00004749
	public Vector3 GetLastGroundNormal()
	{
		return this.m_groundContactNormal;
	}

	// Token: 0x060000DD RID: 221 RVA: 0x00006551 File Offset: 0x00004751
	public void ResetCloth()
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "ResetCloth", Array.Empty<object>());
	}

	// Token: 0x060000DE RID: 222 RVA: 0x00006570 File Offset: 0x00004770
	private void RPC_ResetCloth(long sender)
	{
		foreach (Cloth cloth in base.GetComponentsInChildren<Cloth>())
		{
			if (cloth.enabled)
			{
				cloth.enabled = false;
				cloth.enabled = true;
			}
		}
	}

	// Token: 0x060000DF RID: 223 RVA: 0x000065AC File Offset: 0x000047AC
	public virtual bool GetRelativePosition(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		relativeVel = Vector3.zero;
		if (this.IsOnGround() && this.m_lastGroundBody)
		{
			ZNetView component = this.m_lastGroundBody.GetComponent<ZNetView>();
			if (component && component.IsValid())
			{
				parent = component.GetZDO().m_uid;
				attachJoint = "";
				relativePos = component.transform.InverseTransformPoint(base.transform.position);
				relativeRot = Quaternion.Inverse(component.transform.rotation) * base.transform.rotation;
				relativeVel = component.transform.InverseTransformVector(this.m_body.velocity - this.m_lastGroundBody.velocity);
				return true;
			}
		}
		parent = ZDOID.None;
		attachJoint = "";
		relativePos = Vector3.zero;
		relativeRot = Quaternion.identity;
		return false;
	}

	// Token: 0x060000E0 RID: 224 RVA: 0x000066B6 File Offset: 0x000048B6
	public Quaternion GetLookYaw()
	{
		return this.m_lookYaw;
	}

	// Token: 0x060000E1 RID: 225 RVA: 0x000066BE File Offset: 0x000048BE
	public Vector3 GetMoveDir()
	{
		return this.m_moveDir;
	}

	// Token: 0x060000E2 RID: 226 RVA: 0x000066C6 File Offset: 0x000048C6
	public BaseAI GetBaseAI()
	{
		return this.m_baseAI;
	}

	// Token: 0x060000E3 RID: 227 RVA: 0x000066CE File Offset: 0x000048CE
	public float GetMass()
	{
		return this.m_body.mass;
	}

	// Token: 0x060000E4 RID: 228 RVA: 0x000066DC File Offset: 0x000048DC
	protected void SetVisible(bool visible)
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		if (this.m_lodVisible == visible)
		{
			return;
		}
		this.m_lodVisible = visible;
		if (this.m_lodVisible)
		{
			this.m_lodGroup.localReferencePoint = this.m_originalLocalRef;
			return;
		}
		this.m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
	}

	// Token: 0x060000E5 RID: 229 RVA: 0x00006742 File Offset: 0x00004942
	public void SetTamed(bool tamed)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_tamed == tamed)
		{
			return;
		}
		this.m_nview.InvokeRPC("SetTamed", new object[]
		{
			tamed
		});
	}

	// Token: 0x060000E6 RID: 230 RVA: 0x0000677B File Offset: 0x0000497B
	private void RPC_SetTamed(long sender, bool tamed)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_tamed == tamed)
		{
			return;
		}
		this.m_tamed = tamed;
		this.m_nview.GetZDO().Set(ZDOVars.s_tamed, this.m_tamed);
	}

	// Token: 0x060000E7 RID: 231 RVA: 0x000067B8 File Offset: 0x000049B8
	private bool IsTamed(float time)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.m_nview.GetZDO().IsOwner() && time - this.m_lastTamedCheck > 1f)
		{
			this.m_lastTamedCheck = time;
			this.m_tamed = this.m_nview.GetZDO().GetBool(ZDOVars.s_tamed, this.m_tamed);
		}
		return this.m_tamed;
	}

	// Token: 0x060000E8 RID: 232 RVA: 0x00006823 File Offset: 0x00004A23
	public bool IsTamed()
	{
		return this.IsTamed(Time.time);
	}

	// Token: 0x060000E9 RID: 233 RVA: 0x00006830 File Offset: 0x00004A30
	public SEMan GetSEMan()
	{
		return this.m_seman;
	}

	// Token: 0x060000EA RID: 234 RVA: 0x00006838 File Offset: 0x00004A38
	public bool InInterior()
	{
		return Character.InInterior(base.transform);
	}

	// Token: 0x060000EB RID: 235 RVA: 0x00006845 File Offset: 0x00004A45
	public static bool InInterior(Transform me)
	{
		return me.position.y > 3000f;
	}

	// Token: 0x060000EC RID: 236 RVA: 0x00006859 File Offset: 0x00004A59
	public static void SetDPSDebug(bool enabled)
	{
		Character.s_dpsDebugEnabled = enabled;
	}

	// Token: 0x060000ED RID: 237 RVA: 0x00006861 File Offset: 0x00004A61
	public static bool IsDPSDebugEnabled()
	{
		return Character.s_dpsDebugEnabled;
	}

	// Token: 0x060000EE RID: 238 RVA: 0x00006868 File Offset: 0x00004A68
	public void TakeOff()
	{
		this.m_flying = true;
		this.m_jumpEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		this.m_animator.SetTrigger("fly_takeoff");
	}

	// Token: 0x060000EF RID: 239 RVA: 0x000068A4 File Offset: 0x00004AA4
	public void Land()
	{
		this.m_flying = false;
		this.m_animator.SetTrigger("fly_land");
	}

	// Token: 0x060000F0 RID: 240 RVA: 0x000068BD File Offset: 0x00004ABD
	public void FreezeFrame(float duration)
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "FreezeFrame", new object[]
		{
			duration
		});
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x000068E3 File Offset: 0x00004AE3
	private void RPC_FreezeFrame(long sender, float duration)
	{
		this.m_animEvent.FreezeFrame(duration);
	}

	// Token: 0x17000001 RID: 1
	// (get) Token: 0x060000F2 RID: 242 RVA: 0x000068F1 File Offset: 0x00004AF1
	// (set) Token: 0x060000F3 RID: 243 RVA: 0x000068F9 File Offset: 0x00004AF9
	public int InNumShipVolumes { get; set; }

	// Token: 0x060000F4 RID: 244 RVA: 0x00006904 File Offset: 0x00004B04
	public int Increment(LiquidType type)
	{
		int[] liquids = this.m_liquids;
		int num = liquids[(int)type] + 1;
		liquids[(int)type] = num;
		return num;
	}

	// Token: 0x060000F5 RID: 245 RVA: 0x00006928 File Offset: 0x00004B28
	public int Decrement(LiquidType type)
	{
		int[] liquids = this.m_liquids;
		int num = liquids[(int)type] - 1;
		liquids[(int)type] = num;
		return num;
	}

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x060000F6 RID: 246 RVA: 0x00006949 File Offset: 0x00004B49
	public static List<Character> Instances { get; } = new List<Character>();

	// Token: 0x04000005 RID: 5
	private float m_underWorldCheckTimer;

	// Token: 0x04000006 RID: 6
	private float currentRotSpeedFactor;

	// Token: 0x04000007 RID: 7
	private Collider m_lowestContactCollider;

	// Token: 0x04000008 RID: 8
	private bool m_groundContact;

	// Token: 0x04000009 RID: 9
	private Vector3 m_groundContactPoint = Vector3.zero;

	// Token: 0x0400000A RID: 10
	private Vector3 m_groundContactNormal = Vector3.zero;

	// Token: 0x0400000B RID: 11
	private int m_cachedCurrentAnimHash;

	// Token: 0x0400000C RID: 12
	private int m_cachedNextAnimHash;

	// Token: 0x0400000D RID: 13
	private int m_cachedNextOrCurrentAnimHash;

	// Token: 0x0400000E RID: 14
	private int m_cachedAnimHashFrame;

	// Token: 0x0400000F RID: 15
	public ZNetView m_nViewOverride;

	// Token: 0x04000010 RID: 16
	public Action<float, Character> m_onDamaged;

	// Token: 0x04000011 RID: 17
	public Action m_onDeath;

	// Token: 0x04000012 RID: 18
	public Action<int> m_onLevelSet;

	// Token: 0x04000013 RID: 19
	public Action<Vector3> m_onLand;

	// Token: 0x04000014 RID: 20
	[Header("Character")]
	public string m_name = "";

	// Token: 0x04000015 RID: 21
	public string m_group = "";

	// Token: 0x04000016 RID: 22
	public Character.Faction m_faction = Character.Faction.AnimalsVeg;

	// Token: 0x04000017 RID: 23
	public bool m_boss;

	// Token: 0x04000018 RID: 24
	public bool m_dontHideBossHud;

	// Token: 0x04000019 RID: 25
	public string m_bossEvent = "";

	// Token: 0x0400001A RID: 26
	public string m_defeatSetGlobalKey = "";

	// Token: 0x0400001B RID: 27
	[Header("Movement & Physics")]
	public float m_crouchSpeed = 2f;

	// Token: 0x0400001C RID: 28
	public float m_walkSpeed = 5f;

	// Token: 0x0400001D RID: 29
	public float m_speed = 10f;

	// Token: 0x0400001E RID: 30
	public float m_turnSpeed = 300f;

	// Token: 0x0400001F RID: 31
	public float m_runSpeed = 20f;

	// Token: 0x04000020 RID: 32
	public float m_runTurnSpeed = 300f;

	// Token: 0x04000021 RID: 33
	public float m_flySlowSpeed = 5f;

	// Token: 0x04000022 RID: 34
	public float m_flyFastSpeed = 12f;

	// Token: 0x04000023 RID: 35
	public float m_flyTurnSpeed = 12f;

	// Token: 0x04000024 RID: 36
	public float m_acceleration = 1f;

	// Token: 0x04000025 RID: 37
	public float m_jumpForce = 10f;

	// Token: 0x04000026 RID: 38
	public float m_jumpForceForward;

	// Token: 0x04000027 RID: 39
	public float m_jumpForceTiredFactor = 0.7f;

	// Token: 0x04000028 RID: 40
	public float m_airControl = 0.1f;

	// Token: 0x04000029 RID: 41
	public bool m_canSwim = true;

	// Token: 0x0400002A RID: 42
	public float m_swimDepth = 2f;

	// Token: 0x0400002B RID: 43
	public float m_swimSpeed = 2f;

	// Token: 0x0400002C RID: 44
	public float m_swimTurnSpeed = 100f;

	// Token: 0x0400002D RID: 45
	public float m_swimAcceleration = 0.05f;

	// Token: 0x0400002E RID: 46
	public Character.GroundTiltType m_groundTilt;

	// Token: 0x0400002F RID: 47
	public float m_groundTiltSpeed = 50f;

	// Token: 0x04000030 RID: 48
	public bool m_flying;

	// Token: 0x04000031 RID: 49
	public float m_jumpStaminaUsage = 10f;

	// Token: 0x04000032 RID: 50
	public bool m_disableWhileSleeping;

	// Token: 0x04000033 RID: 51
	[Header("Bodyparts")]
	public Transform m_eye;

	// Token: 0x04000034 RID: 52
	protected Transform m_head;

	// Token: 0x04000035 RID: 53
	[Header("Effects")]
	public EffectList m_hitEffects = new EffectList();

	// Token: 0x04000036 RID: 54
	public EffectList m_critHitEffects = new EffectList();

	// Token: 0x04000037 RID: 55
	public EffectList m_backstabHitEffects = new EffectList();

	// Token: 0x04000038 RID: 56
	public EffectList m_deathEffects = new EffectList();

	// Token: 0x04000039 RID: 57
	public EffectList m_waterEffects = new EffectList();

	// Token: 0x0400003A RID: 58
	public EffectList m_tarEffects = new EffectList();

	// Token: 0x0400003B RID: 59
	public EffectList m_slideEffects = new EffectList();

	// Token: 0x0400003C RID: 60
	public EffectList m_jumpEffects = new EffectList();

	// Token: 0x0400003D RID: 61
	public EffectList m_flyingContinuousEffect = new EffectList();

	// Token: 0x0400003E RID: 62
	[Header("Health & Damage")]
	public bool m_tolerateWater = true;

	// Token: 0x0400003F RID: 63
	public bool m_tolerateSmoke = true;

	// Token: 0x04000040 RID: 64
	public bool m_tolerateTar;

	// Token: 0x04000041 RID: 65
	public float m_health = 10f;

	// Token: 0x04000042 RID: 66
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x04000043 RID: 67
	public WeakSpot[] m_weakSpots;

	// Token: 0x04000044 RID: 68
	public bool m_staggerWhenBlocked = true;

	// Token: 0x04000045 RID: 69
	public float m_staggerDamageFactor;

	// Token: 0x04000046 RID: 70
	private const float c_MinSlideDegreesPlayer = 38f;

	// Token: 0x04000047 RID: 71
	private const float c_MinSlideDegreesMount = 45f;

	// Token: 0x04000048 RID: 72
	private const float c_MinSlideDegreesMonster = 90f;

	// Token: 0x04000049 RID: 73
	private const float c_RootMotionMultiplier = 55f;

	// Token: 0x0400004A RID: 74
	private const float c_PushForceScale = 2.5f;

	// Token: 0x0400004B RID: 75
	private const float c_ContinuousPushForce = 20f;

	// Token: 0x0400004C RID: 76
	private const float c_PushForceDissipation = 100f;

	// Token: 0x0400004D RID: 77
	private const float c_MaxMoveForce = 20f;

	// Token: 0x0400004E RID: 78
	private const float c_StaggerResetTime = 5f;

	// Token: 0x0400004F RID: 79
	private const float c_BackstabResetTime = 300f;

	// Token: 0x04000050 RID: 80
	private float m_staggerDamage;

	// Token: 0x04000051 RID: 81
	private float m_backstabTime = -99999f;

	// Token: 0x04000052 RID: 82
	private GameObject[] m_waterEffects_instances;

	// Token: 0x04000053 RID: 83
	private GameObject[] m_slideEffects_instances;

	// Token: 0x04000054 RID: 84
	private GameObject[] m_flyingEffects_instances;

	// Token: 0x04000055 RID: 85
	protected Vector3 m_moveDir = Vector3.zero;

	// Token: 0x04000056 RID: 86
	protected Vector3 m_lookDir = Vector3.forward;

	// Token: 0x04000057 RID: 87
	protected Quaternion m_lookYaw = Quaternion.identity;

	// Token: 0x04000058 RID: 88
	protected bool m_run;

	// Token: 0x04000059 RID: 89
	protected bool m_walk;

	// Token: 0x0400005A RID: 90
	private Vector3 m_lookTransitionStart;

	// Token: 0x0400005B RID: 91
	private Vector3 m_lookTransitionTarget;

	// Token: 0x0400005C RID: 92
	protected float m_lookTransitionTime;

	// Token: 0x0400005D RID: 93
	protected float m_lookTransitionTimeTotal;

	// Token: 0x0400005E RID: 94
	protected bool m_attack;

	// Token: 0x0400005F RID: 95
	protected bool m_attackHold;

	// Token: 0x04000060 RID: 96
	protected bool m_secondaryAttack;

	// Token: 0x04000061 RID: 97
	protected bool m_secondaryAttackHold;

	// Token: 0x04000062 RID: 98
	protected bool m_blocking;

	// Token: 0x04000063 RID: 99
	protected GameObject m_visual;

	// Token: 0x04000064 RID: 100
	protected LODGroup m_lodGroup;

	// Token: 0x04000065 RID: 101
	protected Rigidbody m_body;

	// Token: 0x04000066 RID: 102
	protected CapsuleCollider m_collider;

	// Token: 0x04000067 RID: 103
	protected ZNetView m_nview;

	// Token: 0x04000068 RID: 104
	protected ZSyncAnimation m_zanim;

	// Token: 0x04000069 RID: 105
	protected Animator m_animator;

	// Token: 0x0400006A RID: 106
	protected CharacterAnimEvent m_animEvent;

	// Token: 0x0400006B RID: 107
	protected BaseAI m_baseAI;

	// Token: 0x0400006C RID: 108
	private const float c_MaxFallHeight = 20f;

	// Token: 0x0400006D RID: 109
	private const float c_MinFallHeight = 4f;

	// Token: 0x0400006E RID: 110
	private const float c_MaxFallDamage = 100f;

	// Token: 0x0400006F RID: 111
	private const float c_StaggerDamageBonus = 2f;

	// Token: 0x04000070 RID: 112
	private const float c_AutoJumpInterval = 0.5f;

	// Token: 0x04000071 RID: 113
	private float m_jumpTimer;

	// Token: 0x04000072 RID: 114
	private float m_lastAutoJumpTime;

	// Token: 0x04000073 RID: 115
	private float m_lastGroundTouch;

	// Token: 0x04000074 RID: 116
	private Vector3 m_lastGroundNormal = Vector3.up;

	// Token: 0x04000075 RID: 117
	private Vector3 m_lastGroundPoint = Vector3.up;

	// Token: 0x04000076 RID: 118
	private Collider m_lastGroundCollider;

	// Token: 0x04000077 RID: 119
	private Rigidbody m_lastGroundBody;

	// Token: 0x04000078 RID: 120
	private Vector3 m_lastAttachPos = Vector3.zero;

	// Token: 0x04000079 RID: 121
	private Rigidbody m_lastAttachBody;

	// Token: 0x0400007A RID: 122
	protected float m_maxAirAltitude = -10000f;

	// Token: 0x0400007B RID: 123
	private float m_waterLevel = -10000f;

	// Token: 0x0400007C RID: 124
	private float m_tarLevel = -10000f;

	// Token: 0x0400007D RID: 125
	private float m_liquidLevel = -10000f;

	// Token: 0x0400007E RID: 126
	private float m_swimTimer = 999f;

	// Token: 0x0400007F RID: 127
	private float m_fallTimer;

	// Token: 0x04000080 RID: 128
	protected SEMan m_seman;

	// Token: 0x04000081 RID: 129
	private float m_noiseRange;

	// Token: 0x04000082 RID: 130
	private float m_syncNoiseTimer;

	// Token: 0x04000083 RID: 131
	private bool m_tamed;

	// Token: 0x04000084 RID: 132
	private float m_lastTamedCheck;

	// Token: 0x04000085 RID: 133
	private Tameable m_tameable;

	// Token: 0x04000086 RID: 134
	private MonsterAI m_tameableMonsterAI;

	// Token: 0x04000087 RID: 135
	private int m_level = 1;

	// Token: 0x04000088 RID: 136
	private Vector3 m_currentVel = Vector3.zero;

	// Token: 0x04000089 RID: 137
	private float m_currentTurnVel;

	// Token: 0x0400008A RID: 138
	private float m_currentTurnVelChange;

	// Token: 0x0400008B RID: 139
	private Vector3 m_groundTiltNormal = Vector3.up;

	// Token: 0x0400008C RID: 140
	protected Vector3 m_pushForce = Vector3.zero;

	// Token: 0x0400008D RID: 141
	private Vector3 m_rootMotion = Vector3.zero;

	// Token: 0x0400008E RID: 142
	private static readonly int s_forwardSpeed = ZSyncAnimation.GetHash("forward_speed");

	// Token: 0x0400008F RID: 143
	private static readonly int s_sidewaySpeed = ZSyncAnimation.GetHash("sideway_speed");

	// Token: 0x04000090 RID: 144
	private static readonly int s_turnSpeed = ZSyncAnimation.GetHash("turn_speed");

	// Token: 0x04000091 RID: 145
	private static readonly int s_inWater = ZSyncAnimation.GetHash("inWater");

	// Token: 0x04000092 RID: 146
	private static readonly int s_onGround = ZSyncAnimation.GetHash("onGround");

	// Token: 0x04000093 RID: 147
	private static readonly int s_encumbered = ZSyncAnimation.GetHash("encumbered");

	// Token: 0x04000094 RID: 148
	private static readonly int s_flying = ZSyncAnimation.GetHash("flying");

	// Token: 0x04000095 RID: 149
	private float m_slippage;

	// Token: 0x04000096 RID: 150
	protected bool m_wallRunning;

	// Token: 0x04000097 RID: 151
	private bool m_sliding;

	// Token: 0x04000098 RID: 152
	private bool m_running;

	// Token: 0x04000099 RID: 153
	private bool m_walking;

	// Token: 0x0400009A RID: 154
	private Vector3 m_originalLocalRef;

	// Token: 0x0400009B RID: 155
	private bool m_lodVisible = true;

	// Token: 0x0400009C RID: 156
	private static int s_smokeRayMask = 0;

	// Token: 0x0400009D RID: 157
	private float m_smokeCheckTimer;

	// Token: 0x0400009E RID: 158
	private static bool s_dpsDebugEnabled = false;

	// Token: 0x0400009F RID: 159
	private static readonly List<KeyValuePair<float, float>> s_enemyDamage = new List<KeyValuePair<float, float>>();

	// Token: 0x040000A0 RID: 160
	private static readonly List<KeyValuePair<float, float>> s_playerDamage = new List<KeyValuePair<float, float>>();

	// Token: 0x040000A1 RID: 161
	private static readonly List<Character> s_characters = new List<Character>();

	// Token: 0x040000A2 RID: 162
	private static int s_characterLayer = 0;

	// Token: 0x040000A3 RID: 163
	private static int s_characterNetLayer = 0;

	// Token: 0x040000A4 RID: 164
	private static int s_characterGhostLayer = 0;

	// Token: 0x040000A5 RID: 165
	private static int s_groundRayMask = 0;

	// Token: 0x040000A6 RID: 166
	private float m_cashedInLiquidDepth;

	// Token: 0x040000A7 RID: 167
	private int m_cashedInLiquidDepthFrame;

	// Token: 0x040000A8 RID: 168
	protected static readonly int s_animatorTagFreeze = ZSyncAnimation.GetHash("freeze");

	// Token: 0x040000A9 RID: 169
	protected static readonly int s_animatorTagStagger = ZSyncAnimation.GetHash("stagger");

	// Token: 0x040000AA RID: 170
	protected static readonly int s_animatorTagSitting = ZSyncAnimation.GetHash("sitting");

	// Token: 0x040000AB RID: 171
	private static readonly int s_animatorFalling = ZSyncAnimation.GetHash("falling");

	// Token: 0x040000AC RID: 172
	private static readonly int s_statusEffectBurning = "Burning".GetStableHashCode();

	// Token: 0x040000AD RID: 173
	private static readonly int s_statusEffectFrost = "Frost".GetStableHashCode();

	// Token: 0x040000AE RID: 174
	private static readonly int s_statusEffectLightning = "Lightning".GetStableHashCode();

	// Token: 0x040000AF RID: 175
	private static readonly int s_statusEffectPoison = "Poison".GetStableHashCode();

	// Token: 0x040000B0 RID: 176
	private static readonly int s_statusEffectSmoked = "Smoked".GetStableHashCode();

	// Token: 0x040000B1 RID: 177
	private static readonly int s_statusEffectSpirit = "Spirit".GetStableHashCode();

	// Token: 0x040000B2 RID: 178
	private static readonly int s_statusEffectTared = "Tared".GetStableHashCode();

	// Token: 0x040000B3 RID: 179
	private static readonly int s_statusEffectWet = "Wet".GetStableHashCode();

	// Token: 0x040000B4 RID: 180
	public static int m_debugFlySpeed = 20;

	// Token: 0x040000B6 RID: 182
	private readonly int[] m_liquids = new int[2];

	// Token: 0x02000004 RID: 4
	public enum Faction
	{
		// Token: 0x040000B9 RID: 185
		Players,
		// Token: 0x040000BA RID: 186
		AnimalsVeg,
		// Token: 0x040000BB RID: 187
		ForestMonsters,
		// Token: 0x040000BC RID: 188
		Undead,
		// Token: 0x040000BD RID: 189
		Demon,
		// Token: 0x040000BE RID: 190
		MountainMonsters,
		// Token: 0x040000BF RID: 191
		SeaMonsters,
		// Token: 0x040000C0 RID: 192
		PlainsMonsters,
		// Token: 0x040000C1 RID: 193
		Boss,
		// Token: 0x040000C2 RID: 194
		MistlandsMonsters,
		// Token: 0x040000C3 RID: 195
		Dverger
	}

	// Token: 0x02000005 RID: 5
	public enum GroundTiltType
	{
		// Token: 0x040000C5 RID: 197
		None,
		// Token: 0x040000C6 RID: 198
		Pitch,
		// Token: 0x040000C7 RID: 199
		Full,
		// Token: 0x040000C8 RID: 200
		PitchRaycast,
		// Token: 0x040000C9 RID: 201
		FullRaycast
	}
}
