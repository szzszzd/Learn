using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000237 RID: 567
public class Fish : MonoBehaviour, IWaterInteractable, Hoverable, Interactable
{
	// Token: 0x0600163B RID: 5691 RVA: 0x000921AC File Offset: 0x000903AC
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_itemDrop = base.GetComponent<ItemDrop>();
		this.m_lodGroup = base.GetComponent<LODGroup>();
		if (this.m_itemDrop)
		{
			if (this.m_itemDrop.m_itemData.m_quality > 1)
			{
				this.m_itemDrop.SetQuality(this.m_itemDrop.m_itemData.m_quality);
			}
			ItemDrop itemDrop = this.m_itemDrop;
			itemDrop.m_onDrop = (Action<ItemDrop>)Delegate.Combine(itemDrop.m_onDrop, new Action<ItemDrop>(this.onDrop));
			if (this.m_pickupItem == null)
			{
				this.m_pickupItem = base.gameObject;
			}
		}
		this.m_waterWaveCount = UnityEngine.Random.Range(0, 1);
		if (this.m_lodGroup)
		{
			this.m_originalLocalRef = this.m_lodGroup.localReferencePoint;
		}
	}

	// Token: 0x0600163C RID: 5692 RVA: 0x00092290 File Offset: 0x00090490
	private void Start()
	{
		this.m_spawnPoint = this.m_nview.GetZDO().GetVec3(ZDOVars.s_spawnPoint, base.transform.position);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnPoint, this.m_spawnPoint);
		}
		if (this.m_nview.IsOwner())
		{
			this.RandomizeWaypoint(true, DateTime.Now);
		}
		if (this.m_nview && this.m_nview.IsValid())
		{
			this.m_nview.Register("RequestPickup", new Action<long>(this.RPC_RequestPickup));
			this.m_nview.Register("Pickup", new Action<long>(this.RPC_Pickup));
		}
		if (this.m_waterVolume != null)
		{
			this.m_waterDepth = this.m_waterVolume.Depth(base.transform.position);
			this.m_waterWave = this.m_waterVolume.CalcWave(base.transform.position, this.m_waterDepth, Fish.s_wrappedTimeSeconds, 1f);
		}
	}

	// Token: 0x0600163D RID: 5693 RVA: 0x000923AD File Offset: 0x000905AD
	private void OnEnable()
	{
		Fish.Instances.Add(this);
	}

	// Token: 0x0600163E RID: 5694 RVA: 0x000923BA File Offset: 0x000905BA
	private void OnDisable()
	{
		Fish.Instances.Remove(this);
	}

	// Token: 0x0600163F RID: 5695 RVA: 0x000923C8 File Offset: 0x000905C8
	public string GetHoverText()
	{
		string text = this.m_name;
		if (this.IsOutOfWater())
		{
			if (this.m_itemDrop)
			{
				return this.m_itemDrop.GetHoverText();
			}
			text += "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup";
		}
		return Localization.instance.Localize(text);
	}

	// Token: 0x06001640 RID: 5696 RVA: 0x00092414 File Offset: 0x00090614
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001641 RID: 5697 RVA: 0x0009241C File Offset: 0x0009061C
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		return !repeat && this.IsOutOfWater() && this.Pickup(character);
	}

	// Token: 0x06001642 RID: 5698 RVA: 0x0009243C File Offset: 0x0009063C
	public bool Pickup(Humanoid character)
	{
		if (this.m_itemDrop)
		{
			this.m_itemDrop.Pickup(character);
			return true;
		}
		if (this.m_pickupItem == null)
		{
			return false;
		}
		if (!character.GetInventory().CanAddItem(this.m_pickupItem, this.m_pickupItemStackSize))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_noroom", 0, null);
			return false;
		}
		this.m_nview.InvokeRPC("RequestPickup", Array.Empty<object>());
		return true;
	}

	// Token: 0x06001643 RID: 5699 RVA: 0x000924B3 File Offset: 0x000906B3
	private void RPC_RequestPickup(long uid)
	{
		if (Time.time - this.m_pickupTime > 2f)
		{
			this.m_pickupTime = Time.time;
			this.m_nview.InvokeRPC(uid, "Pickup", Array.Empty<object>());
		}
	}

	// Token: 0x06001644 RID: 5700 RVA: 0x000924E9 File Offset: 0x000906E9
	private void RPC_Pickup(long uid)
	{
		if (Player.m_localPlayer && Player.m_localPlayer.PickupPrefab(this.m_pickupItem, this.m_pickupItemStackSize, true) != null)
		{
			this.m_nview.ClaimOwnership();
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06001645 RID: 5701 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001646 RID: 5702 RVA: 0x00092528 File Offset: 0x00090728
	public void SetLiquidLevel(float level, LiquidType type, Component liquidObj)
	{
		if (type == LiquidType.Water)
		{
			this.m_inWater = level;
		}
		this.m_liquidSurface = null;
		this.m_waterVolume = null;
		WaterVolume waterVolume = liquidObj as WaterVolume;
		if (waterVolume != null)
		{
			this.m_waterVolume = waterVolume;
			return;
		}
		LiquidSurface liquidSurface = liquidObj as LiquidSurface;
		if (liquidSurface != null)
		{
			this.m_liquidSurface = liquidSurface;
		}
	}

	// Token: 0x06001647 RID: 5703 RVA: 0x0000652E File Offset: 0x0000472E
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x06001648 RID: 5704 RVA: 0x00092570 File Offset: 0x00090770
	public bool IsOutOfWater()
	{
		return this.m_inWater < base.transform.position.y - this.m_height;
	}

	// Token: 0x06001649 RID: 5705 RVA: 0x00092594 File Offset: 0x00090794
	public void CustomFixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (Time.frameCount != Fish.s_updatedFrame)
		{
			Vector4 a;
			Vector4 b;
			float num;
			EnvMan.instance.GetWindData(out a, out b, out num);
			Fish.s_wind = a + b;
			Fish.s_wrappedTimeSeconds = ZNet.instance.GetWrappedDayTimeSeconds();
			Fish.s_now = DateTime.Now;
			Fish.s_deltaTime = Time.fixedDeltaTime;
			Fish.s_time = Time.time;
			Fish.s_dawnDusk = 1f - Mathf.Abs(Mathf.Abs(EnvMan.instance.GetDayFraction() * 2f - 1f) - 0.5f) * 2f;
			Fish.s_updatedFrame = Time.frameCount;
		}
		Vector3 position = base.transform.position;
		bool flag = this.IsOutOfWater();
		if (this.m_waterVolume != null)
		{
			int num2 = this.m_waterWaveCount + 1;
			this.m_waterWaveCount = num2;
			if ((num2 & 1) == 1)
			{
				this.m_waterDepth = this.m_waterVolume.Depth(position);
			}
			else
			{
				this.m_waterWave = this.m_waterVolume.CalcWave(position, this.m_waterDepth, Fish.s_wrappedTimeSeconds, 1f);
			}
		}
		this.SetVisible(this.m_nview.HasOwner());
		if (this.m_lastOwner != this.m_nview.GetZDO().GetOwner())
		{
			this.m_lastOwner = this.m_nview.GetZDO().GetOwner();
			this.m_body.WakeUp();
		}
		if (!flag && UnityEngine.Random.value > 0.975f && this.m_nview.GetZDO().GetInt(ZDOVars.s_hooked, 0) == 1 && this.m_nview.GetZDO().GetFloat(ZDOVars.s_escape, 0f) > 0f)
		{
			this.m_jumpEffects.Create(position, Quaternion.identity, base.transform, 1f, -1);
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		FishingFloat fishingFloat = FishingFloat.FindFloat(this);
		if (fishingFloat)
		{
			Utils.Pull(this.m_body, fishingFloat.transform.position, 1f, this.m_hookForce, 1f, 0.5f, false, false, 1f);
		}
		if (this.m_isColliding && flag)
		{
			this.ConsiderJump(Fish.s_now);
		}
		if (this.m_escapeTime > 0f)
		{
			this.m_body.rotation *= Quaternion.AngleAxis(Mathf.Sin(this.m_escapeTime * 40f) * 12f, Vector3.up);
			this.m_escapeTime -= Fish.s_deltaTime;
			if (this.m_escapeTime <= 0f)
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_escape, 0, false);
				this.m_nextEscape = Fish.s_now + TimeSpan.FromSeconds((double)UnityEngine.Random.Range(this.m_escapeWaitMin, this.m_escapeWaitMax));
			}
		}
		else if (Fish.s_now > this.m_nextEscape && this.IsHooked())
		{
			this.Escape();
		}
		if (this.m_inWater <= -10000f || this.m_inWater < position.y + this.m_height)
		{
			this.m_body.useGravity = true;
			if (flag)
			{
				if (this.m_isJumping)
				{
					Vector3 velocity = this.m_body.velocity;
					if (!this.m_jumpedFromLand && velocity != Vector3.zero)
					{
						velocity.y *= 1.6f;
						this.m_body.rotation = Quaternion.RotateTowards(this.m_body.rotation, Quaternion.LookRotation(velocity), 5f);
					}
				}
				return;
			}
		}
		if (this.m_isJumping)
		{
			if (this.m_body.velocity.y < 0f)
			{
				this.m_jumpEffects.Create(position, Quaternion.identity, null, 1f, -1);
				this.m_isJumping = false;
				this.m_body.rotation = Quaternion.Euler(0f, this.m_body.rotation.eulerAngles.y, 0f);
				this.RandomizeWaypoint(true, Fish.s_now);
			}
		}
		else if (this.m_waterWave >= this.m_minDepth && this.m_waterWave < this.m_minDepth + this.m_maxJumpDepthOffset)
		{
			this.ConsiderJump(Fish.s_now);
		}
		this.m_JumpHeightStrength = 1f;
		this.m_body.useGravity = false;
		this.m_fast = false;
		bool flag2 = Fish.s_now > this.m_blockChange;
		Player playerNoiseRange = Player.GetPlayerNoiseRange(position, 100f);
		if (playerNoiseRange)
		{
			if (Vector3.Distance(position, playerNoiseRange.transform.position) > this.m_avoidRange / 2f && !this.IsHooked())
			{
				if (flag2 || Fish.s_now > this.m_lastCollision + TimeSpan.FromSeconds((double)this.m_collisionFleeTimeout))
				{
					Vector3 normalized = (position - playerNoiseRange.transform.position).normalized;
					this.SwimDirection(normalized, true, true, Fish.s_deltaTime);
				}
				return;
			}
			this.m_fast = true;
			if (this.m_swimTimer > 0.5f)
			{
				this.m_swimTimer = 0.5f;
			}
		}
		this.m_swimTimer -= Fish.s_deltaTime;
		if (this.m_swimTimer <= 0f && flag2)
		{
			this.RandomizeWaypoint(!this.m_fast, Fish.s_now);
		}
		if (this.m_haveWaypoint)
		{
			if (this.m_waypointFF)
			{
				this.m_waypoint = this.m_waypointFF.transform.position + Vector3.down;
			}
			if (Vector2.Distance(this.m_waypoint, position) < 0.2f || (this.m_escapeTime < 0f && this.IsHooked()))
			{
				if (!this.m_waypointFF)
				{
					this.m_haveWaypoint = false;
					return;
				}
				if (Fish.s_time - this.m_lastNibbleTime > 1f && this.m_failedBait != this.m_waypointFF)
				{
					this.m_lastNibbleTime = Fish.s_time;
					bool flag3 = this.TestBate(this.m_waypointFF);
					this.m_waypointFF.Nibble(this, flag3);
					if (!flag3)
					{
						this.m_failedBait = this.m_waypointFF;
					}
				}
			}
			Vector3 dir = Vector3.Normalize(this.m_waypoint - position);
			this.SwimDirection(dir, this.m_fast, false, Fish.s_deltaTime);
		}
		else
		{
			this.Stop(Fish.s_deltaTime);
		}
		if (!flag && this.m_waterVolume != null)
		{
			this.m_body.MovePosition(this.m_body.position + new Vector3(0f, this.m_waterWave - this.m_lastWave, 0f));
			this.m_lastWave = this.m_waterWave;
			if (this.m_waterWave > 0f)
			{
				this.m_body.AddForce(Fish.s_wind * this.m_waveFollowDirection * this.m_waterWave);
			}
		}
	}

	// Token: 0x0600164A RID: 5706 RVA: 0x00092C9C File Offset: 0x00090E9C
	private void Stop(float dt)
	{
		if (this.m_inWater < base.transform.position.y + this.m_height)
		{
			return;
		}
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Quaternion to = Quaternion.LookRotation(forward, Vector3.up);
		Quaternion rot = Quaternion.RotateTowards(this.m_body.rotation, to, this.m_turnRate * dt);
		this.m_body.MoveRotation(rot);
		Vector3 force = -this.m_body.velocity * this.m_acceleration;
		this.m_body.AddForce(force, ForceMode.VelocityChange);
	}

	// Token: 0x0600164B RID: 5707 RVA: 0x00092D44 File Offset: 0x00090F44
	private void SwimDirection(Vector3 dir, bool fast, bool avoidLand, float dt)
	{
		Vector3 vector = dir;
		vector.y = 0f;
		if (vector == Vector3.zero)
		{
			ZLog.LogWarning("Invalid swim direction");
			return;
		}
		vector.Normalize();
		float num = this.m_turnRate;
		if (fast)
		{
			num *= this.m_avoidSpeedScale;
		}
		Quaternion to = Quaternion.LookRotation(vector, Vector3.up);
		Quaternion rotation = Quaternion.RotateTowards(base.transform.rotation, to, num * dt);
		if (this.m_isJumping && this.m_body.velocity.y > 0f)
		{
			return;
		}
		if (!this.m_isJumping)
		{
			this.m_body.rotation = rotation;
		}
		float num2 = this.m_speed;
		if (fast)
		{
			num2 *= this.m_avoidSpeedScale;
		}
		if (avoidLand && this.GetPointDepth(base.transform.position + base.transform.forward) < this.m_minDepth)
		{
			num2 = 0f;
		}
		if (fast && Vector3.Dot(dir, base.transform.forward) < 0f)
		{
			num2 = 0f;
		}
		Vector3 forward = base.transform.forward;
		forward.y = dir.y;
		Vector3 vector2 = forward * num2 - this.m_body.velocity;
		if (this.m_inWater < base.transform.position.y + this.m_height && vector2.y > 0f)
		{
			vector2.y = 0f;
		}
		this.m_body.AddForce(vector2 * this.m_acceleration, ForceMode.VelocityChange);
	}

	// Token: 0x0600164C RID: 5708 RVA: 0x00092EDC File Offset: 0x000910DC
	private FishingFloat FindFloat()
	{
		foreach (FishingFloat fishingFloat in FishingFloat.GetAllInstances())
		{
			if (fishingFloat.IsInWater() && Vector3.Distance(base.transform.position, fishingFloat.transform.position) <= fishingFloat.m_range && !(fishingFloat.GetCatch() != null))
			{
				float baseHookChance = this.m_baseHookChance;
				if (UnityEngine.Random.value < baseHookChance)
				{
					return fishingFloat;
				}
			}
		}
		return null;
	}

	// Token: 0x0600164D RID: 5709 RVA: 0x00092F78 File Offset: 0x00091178
	private bool TestBate(FishingFloat ff)
	{
		string bait = ff.GetBait();
		foreach (Fish.BaitSetting baitSetting in this.m_baits)
		{
			if (baitSetting.m_bait.name == bait && UnityEngine.Random.value < baitSetting.m_chance)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600164E RID: 5710 RVA: 0x00092FF4 File Offset: 0x000911F4
	private bool RandomizeWaypoint(bool canHook, DateTime now)
	{
		if (this.m_isJumping)
		{
			return false;
		}
		Vector2 vector = UnityEngine.Random.insideUnitCircle * this.m_swimRange;
		this.m_waypoint = this.m_spawnPoint + new Vector3(vector.x, 0f, vector.y);
		this.m_waypointFF = null;
		if (canHook)
		{
			FishingFloat fishingFloat = this.FindFloat();
			if (fishingFloat && fishingFloat != this.m_failedBait)
			{
				this.m_waypointFF = fishingFloat;
				this.m_waypoint = fishingFloat.transform.position + Vector3.down;
			}
		}
		float pointDepth = this.GetPointDepth(this.m_waypoint);
		if (pointDepth < this.m_minDepth)
		{
			return false;
		}
		Vector3 p = (this.m_waypoint + base.transform.position) * 0.5f;
		if (this.GetPointDepth(p) < this.m_minDepth)
		{
			return false;
		}
		float maxInclusive = Mathf.Min(this.m_maxDepth, pointDepth - this.m_height);
		float waterLevel = this.GetWaterLevel(this.m_waypoint);
		this.m_waypoint.y = waterLevel - UnityEngine.Random.Range(this.m_minDepth, maxInclusive);
		this.m_haveWaypoint = true;
		this.m_swimTimer = UnityEngine.Random.Range(this.m_wpDurationMin, this.m_wpDurationMax);
		this.m_blockChange = now + TimeSpan.FromSeconds((double)UnityEngine.Random.Range(this.m_blockChangeDurationMin, this.m_blockChangeDurationMax));
		return true;
	}

	// Token: 0x0600164F RID: 5711 RVA: 0x0009315C File Offset: 0x0009135C
	private void Escape()
	{
		this.m_escapeTime = UnityEngine.Random.Range(this.m_escapeMin, this.m_escapeMax + (float)(this.m_itemDrop ? this.m_itemDrop.m_itemData.m_quality : 1) * this.m_escapeMaxPerLevel);
		this.m_nview.GetZDO().Set(ZDOVars.s_escape, this.m_escapeTime);
	}

	// Token: 0x06001650 RID: 5712 RVA: 0x000931C4 File Offset: 0x000913C4
	private float GetPointDepth(Vector3 p)
	{
		float num;
		if (ZoneSystem.instance && ZoneSystem.instance.GetSolidHeight(p, out num, (this.m_waterVolume != null) ? 0 : 1000))
		{
			return this.GetWaterLevel(p) - num;
		}
		return 0f;
	}

	// Token: 0x06001651 RID: 5713 RVA: 0x00093211 File Offset: 0x00091411
	private float GetWaterLevel(Vector3 point)
	{
		if (!(this.m_waterVolume != null))
		{
			return ZoneSystem.instance.m_waterLevel;
		}
		return this.m_waterVolume.GetWaterSurface(point, 1f);
	}

	// Token: 0x06001652 RID: 5714 RVA: 0x0009323D File Offset: 0x0009143D
	private bool DangerNearby()
	{
		return Player.GetPlayerNoiseRange(base.transform.position, 100f) != null;
	}

	// Token: 0x06001653 RID: 5715 RVA: 0x0009325A File Offset: 0x0009145A
	public ZDOID GetZDOID()
	{
		return this.m_nview.GetZDO().m_uid;
	}

	// Token: 0x06001654 RID: 5716 RVA: 0x0009326C File Offset: 0x0009146C
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_height, new Vector3(1f, 0.02f, 1f));
	}

	// Token: 0x06001655 RID: 5717 RVA: 0x000932BC File Offset: 0x000914BC
	private void OnCollisionEnter(Collision collision)
	{
		this.m_isColliding = true;
		this.onCollision();
	}

	// Token: 0x06001656 RID: 5718 RVA: 0x000932CB File Offset: 0x000914CB
	private void OnCollisionStay(Collision collision)
	{
		if (DateTime.Now > this.m_lastCollision + TimeSpan.FromSeconds(0.5))
		{
			this.onCollision();
		}
		if (this.m_isJumping)
		{
			this.m_isJumping = false;
		}
	}

	// Token: 0x06001657 RID: 5719 RVA: 0x00093307 File Offset: 0x00091507
	private void OnCollisionExit(Collision collision)
	{
		this.m_isColliding = false;
	}

	// Token: 0x06001658 RID: 5720 RVA: 0x00093310 File Offset: 0x00091510
	private void onCollision()
	{
		this.m_lastCollision = DateTime.Now;
		if (!this.m_nview || !this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		int num = 0;
		while (num < 10 && !this.RandomizeWaypoint(!this.m_fast, DateTime.Now))
		{
			num++;
		}
	}

	// Token: 0x06001659 RID: 5721 RVA: 0x00093373 File Offset: 0x00091573
	private void onDrop(ItemDrop item)
	{
		this.m_JumpHeightStrength = 0f;
	}

	// Token: 0x0600165A RID: 5722 RVA: 0x00093380 File Offset: 0x00091580
	private void ConsiderJump(DateTime now)
	{
		if (this.m_itemDrop && (float)this.m_itemDrop.m_itemData.m_quality > this.m_jumpMaxLevel)
		{
			return;
		}
		if (this.m_JumpHeightStrength > 0f && now > this.m_lastJumpCheck + TimeSpan.FromSeconds((double)this.m_jumpFrequencySeconds))
		{
			this.m_lastJumpCheck = now;
			if (this.IsOutOfWater())
			{
				if (UnityEngine.Random.Range(0f, 1f) < this.m_jumpOnLandChance * this.m_JumpHeightStrength)
				{
					this.Jump();
					return;
				}
			}
			else if (UnityEngine.Random.Range(0f, 1f) < (this.m_jumpChance + Mathf.Min(0f, this.m_lastWave) * this.m_waveJumpMultiplier) * Fish.s_dawnDusk)
			{
				this.Jump();
			}
		}
	}

	// Token: 0x0600165B RID: 5723 RVA: 0x00093454 File Offset: 0x00091654
	private void Jump()
	{
		if (this.m_isJumping)
		{
			return;
		}
		this.m_isJumping = true;
		if (this.IsOutOfWater())
		{
			this.m_jumpedFromLand = true;
			this.m_JumpHeightStrength *= this.m_jumpOnLandDecay;
			float jumpOnLandRotation = this.m_jumpOnLandRotation;
			this.m_body.AddForce(new Vector3(0f, this.m_JumpHeightStrength * this.m_jumpHeightLand * base.transform.localScale.y, 0f), ForceMode.Impulse);
			this.m_body.AddTorque(UnityEngine.Random.Range(-jumpOnLandRotation, jumpOnLandRotation), UnityEngine.Random.Range(-jumpOnLandRotation, jumpOnLandRotation), UnityEngine.Random.Range(-jumpOnLandRotation, jumpOnLandRotation), ForceMode.Impulse);
			return;
		}
		this.m_jumpedFromLand = false;
		this.m_jumpEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		this.m_body.AddForce(new Vector3(0f, this.m_jumpHeight * base.transform.localScale.y, 0f), ForceMode.Impulse);
		this.m_body.AddForce(base.transform.forward * this.m_jumpForwardStrength * base.transform.localScale.y, ForceMode.Impulse);
	}

	// Token: 0x0600165C RID: 5724 RVA: 0x0009358C File Offset: 0x0009178C
	public void OnHooked(FishingFloat ff)
	{
		if (this.m_nview && this.m_nview.IsValid())
		{
			this.m_nview.ClaimOwnership();
		}
		this.m_fishingFloat = ff;
		if (this.m_nview.IsValid())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_hooked, (ff != null) ? 1 : 0, false);
			this.Escape();
		}
	}

	// Token: 0x0600165D RID: 5725 RVA: 0x000935FB File Offset: 0x000917FB
	public bool IsHooked()
	{
		return this.m_fishingFloat != null;
	}

	// Token: 0x0600165E RID: 5726 RVA: 0x00093609 File Offset: 0x00091809
	public bool IsEscaping()
	{
		return this.m_escapeTime > 0f && this.IsHooked();
	}

	// Token: 0x0600165F RID: 5727 RVA: 0x00093620 File Offset: 0x00091820
	public float GetStaminaUse()
	{
		if (!this.IsEscaping())
		{
			return this.m_staminaUse;
		}
		return this.m_escapeStaminaUse;
	}

	// Token: 0x06001660 RID: 5728 RVA: 0x00093638 File Offset: 0x00091838
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

	// Token: 0x06001661 RID: 5729 RVA: 0x000936A0 File Offset: 0x000918A0
	public int Increment(LiquidType type)
	{
		int[] liquids = this.m_liquids;
		int num = liquids[(int)type] + 1;
		liquids[(int)type] = num;
		return num;
	}

	// Token: 0x06001662 RID: 5730 RVA: 0x000936C4 File Offset: 0x000918C4
	public int Decrement(LiquidType type)
	{
		int[] liquids = this.m_liquids;
		int num = liquids[(int)type] - 1;
		liquids[(int)type] = num;
		return num;
	}

	// Token: 0x170000E5 RID: 229
	// (get) Token: 0x06001663 RID: 5731 RVA: 0x000936E5 File Offset: 0x000918E5
	public static List<Fish> Instances { get; } = new List<Fish>();

	// Token: 0x0400173C RID: 5948
	public string m_name = "Fish";

	// Token: 0x0400173D RID: 5949
	public float m_swimRange = 20f;

	// Token: 0x0400173E RID: 5950
	public float m_minDepth = 1f;

	// Token: 0x0400173F RID: 5951
	public float m_maxDepth = 4f;

	// Token: 0x04001740 RID: 5952
	public float m_speed = 10f;

	// Token: 0x04001741 RID: 5953
	public float m_acceleration = 5f;

	// Token: 0x04001742 RID: 5954
	public float m_turnRate = 10f;

	// Token: 0x04001743 RID: 5955
	public float m_wpDurationMin = 4f;

	// Token: 0x04001744 RID: 5956
	public float m_wpDurationMax = 4f;

	// Token: 0x04001745 RID: 5957
	public float m_avoidSpeedScale = 2f;

	// Token: 0x04001746 RID: 5958
	public float m_avoidRange = 5f;

	// Token: 0x04001747 RID: 5959
	public float m_height = 0.2f;

	// Token: 0x04001748 RID: 5960
	public float m_hookForce = 4f;

	// Token: 0x04001749 RID: 5961
	public float m_staminaUse = 1f;

	// Token: 0x0400174A RID: 5962
	public float m_escapeStaminaUse = 2f;

	// Token: 0x0400174B RID: 5963
	public float m_escapeMin = 0.5f;

	// Token: 0x0400174C RID: 5964
	public float m_escapeMax = 3f;

	// Token: 0x0400174D RID: 5965
	public float m_escapeWaitMin = 0.75f;

	// Token: 0x0400174E RID: 5966
	public float m_escapeWaitMax = 4f;

	// Token: 0x0400174F RID: 5967
	public float m_escapeMaxPerLevel = 1.5f;

	// Token: 0x04001750 RID: 5968
	public float m_baseHookChance = 0.5f;

	// Token: 0x04001751 RID: 5969
	public GameObject m_pickupItem;

	// Token: 0x04001752 RID: 5970
	public int m_pickupItemStackSize = 1;

	// Token: 0x04001753 RID: 5971
	private float m_escapeTime;

	// Token: 0x04001754 RID: 5972
	private DateTime m_nextEscape;

	// Token: 0x04001755 RID: 5973
	private Vector3 m_spawnPoint;

	// Token: 0x04001756 RID: 5974
	private bool m_fast;

	// Token: 0x04001757 RID: 5975
	private DateTime m_lastCollision;

	// Token: 0x04001758 RID: 5976
	private DateTime m_blockChange;

	// Token: 0x04001759 RID: 5977
	[global::Tooltip("Fish aren't smart enough to change their mind too often (and makes reactions/collisions feel less artificial)")]
	public float m_blockChangeDurationMin = 0.1f;

	// Token: 0x0400175A RID: 5978
	public float m_blockChangeDurationMax = 0.6f;

	// Token: 0x0400175B RID: 5979
	public float m_collisionFleeTimeout = 1.5f;

	// Token: 0x0400175C RID: 5980
	private Vector3 m_waypoint;

	// Token: 0x0400175D RID: 5981
	private FishingFloat m_waypointFF;

	// Token: 0x0400175E RID: 5982
	private FishingFloat m_failedBait;

	// Token: 0x0400175F RID: 5983
	private bool m_haveWaypoint;

	// Token: 0x04001760 RID: 5984
	[Header("Baits")]
	public List<Fish.BaitSetting> m_baits = new List<Fish.BaitSetting>();

	// Token: 0x04001761 RID: 5985
	public DropTable m_extraDrops = new DropTable();

	// Token: 0x04001762 RID: 5986
	[Header("Jumping")]
	public float m_jumpSpeed = 3f;

	// Token: 0x04001763 RID: 5987
	public float m_jumpHeight = 14f;

	// Token: 0x04001764 RID: 5988
	public float m_jumpForwardStrength = 16f;

	// Token: 0x04001765 RID: 5989
	public float m_jumpHeightLand = 3f;

	// Token: 0x04001766 RID: 5990
	public float m_jumpChance = 0.25f;

	// Token: 0x04001767 RID: 5991
	public float m_jumpOnLandChance = 0.5f;

	// Token: 0x04001768 RID: 5992
	public float m_jumpOnLandDecay = 0.5f;

	// Token: 0x04001769 RID: 5993
	public float m_maxJumpDepthOffset = 0.5f;

	// Token: 0x0400176A RID: 5994
	public float m_jumpFrequencySeconds = 0.1f;

	// Token: 0x0400176B RID: 5995
	public float m_jumpOnLandRotation = 2f;

	// Token: 0x0400176C RID: 5996
	public float m_waveJumpMultiplier = 0.05f;

	// Token: 0x0400176D RID: 5997
	public float m_jumpMaxLevel = 2f;

	// Token: 0x0400176E RID: 5998
	public EffectList m_jumpEffects = new EffectList();

	// Token: 0x0400176F RID: 5999
	private float m_JumpHeightStrength;

	// Token: 0x04001770 RID: 6000
	private bool m_jumpedFromLand;

	// Token: 0x04001771 RID: 6001
	private bool m_isColliding;

	// Token: 0x04001772 RID: 6002
	private bool m_isJumping;

	// Token: 0x04001773 RID: 6003
	private DateTime m_lastJumpCheck;

	// Token: 0x04001774 RID: 6004
	private float m_swimTimer;

	// Token: 0x04001775 RID: 6005
	private float m_lastNibbleTime;

	// Token: 0x04001776 RID: 6006
	[Header("Waves")]
	public float m_waveFollowDirection = 7f;

	// Token: 0x04001777 RID: 6007
	private float m_lastWave;

	// Token: 0x04001778 RID: 6008
	private float m_inWater = -10000f;

	// Token: 0x04001779 RID: 6009
	private WaterVolume m_waterVolume;

	// Token: 0x0400177A RID: 6010
	private LiquidSurface m_liquidSurface;

	// Token: 0x0400177B RID: 6011
	private FishingFloat m_fishingFloat;

	// Token: 0x0400177C RID: 6012
	private float m_pickupTime;

	// Token: 0x0400177D RID: 6013
	private long m_lastOwner = -1L;

	// Token: 0x0400177E RID: 6014
	private Vector3 m_originalLocalRef;

	// Token: 0x0400177F RID: 6015
	private bool m_lodVisible = true;

	// Token: 0x04001780 RID: 6016
	private ZNetView m_nview;

	// Token: 0x04001781 RID: 6017
	private Rigidbody m_body;

	// Token: 0x04001782 RID: 6018
	private ItemDrop m_itemDrop;

	// Token: 0x04001783 RID: 6019
	private LODGroup m_lodGroup;

	// Token: 0x04001784 RID: 6020
	private static Vector4 s_wind;

	// Token: 0x04001785 RID: 6021
	private static float s_wrappedTimeSeconds;

	// Token: 0x04001786 RID: 6022
	private static DateTime s_now;

	// Token: 0x04001787 RID: 6023
	private static float s_deltaTime;

	// Token: 0x04001788 RID: 6024
	private static float s_time;

	// Token: 0x04001789 RID: 6025
	private static float s_dawnDusk;

	// Token: 0x0400178A RID: 6026
	private static int s_updatedFrame;

	// Token: 0x0400178B RID: 6027
	private float m_waterDepth;

	// Token: 0x0400178C RID: 6028
	private float m_waterWave;

	// Token: 0x0400178D RID: 6029
	private int m_waterWaveCount;

	// Token: 0x0400178E RID: 6030
	private readonly int[] m_liquids = new int[2];

	// Token: 0x02000238 RID: 568
	[Serializable]
	public class BaitSetting
	{
		// Token: 0x04001790 RID: 6032
		public ItemDrop m_bait;

		// Token: 0x04001791 RID: 6033
		[Range(0f, 1f)]
		public float m_chance;
	}
}
