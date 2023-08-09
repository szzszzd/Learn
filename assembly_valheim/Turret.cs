using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x020002B6 RID: 694
public class Turret : MonoBehaviour, Hoverable, Interactable, IPieceMarker
{
	// Token: 0x06001A3D RID: 6717 RVA: 0x000AD8B0 File Offset: 0x000ABAB0
	protected void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview)
		{
			this.m_nview.Register<string>("RPC_AddAmmo", new Action<long, string>(this.RPC_AddAmmo));
			this.m_nview.Register<ZDOID>("RPC_SetTarget", new Action<long, ZDOID>(this.RPC_SetTarget));
		}
		this.m_updateTargetTimer = UnityEngine.Random.Range(0f, this.m_updateTargetIntervalNear);
		this.m_baseBodyRotation = this.m_turretBody.transform.localRotation;
		this.m_baseNeckRotation = this.m_turretNeck.transform.localRotation;
		WearNTear component = base.GetComponent<WearNTear>();
		if (component != null)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		if (this.m_marker)
		{
			this.m_marker.m_radius = this.m_viewDistance;
			this.m_marker.gameObject.SetActive(false);
		}
		foreach (Turret.AmmoType ammoType in this.m_allowedAmmo)
		{
			ammoType.m_visual.SetActive(false);
		}
		if (this.m_nview && this.m_nview.IsValid())
		{
			this.UpdateVisualBolt();
		}
		this.ReadTargets();
	}

	// Token: 0x06001A3E RID: 6718 RVA: 0x000ADA1C File Offset: 0x000ABC1C
	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateReloadState();
		this.UpdateMarker(fixedDeltaTime);
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateTurretRotation();
		this.UpdateVisualBolt();
		if (!this.m_nview.IsOwner())
		{
			if (this.m_nview.IsValid() && this.m_lastUpdateTargetRevision != this.m_nview.GetZDO().DataRevision)
			{
				this.m_lastUpdateTargetRevision = this.m_nview.GetZDO().DataRevision;
				this.ReadTargets();
			}
			return;
		}
		this.UpdateTarget(fixedDeltaTime);
		this.UpdateAttack(fixedDeltaTime);
	}

	// Token: 0x06001A3F RID: 6719 RVA: 0x000ADAB4 File Offset: 0x000ABCB4
	private void UpdateTurretRotation()
	{
		if (this.IsCoolingDown())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		bool flag = this.m_target && this.HasAmmo();
		Vector3 forward;
		if (flag)
		{
			if (this.m_lastAmmo == null)
			{
				this.m_lastAmmo = this.GetAmmoItem();
			}
			if (this.m_lastAmmo == null)
			{
				ZLog.LogWarning("Turret had invalid ammo, resetting ammo");
				this.m_nview.GetZDO().Set(ZDOVars.s_ammo, 0, false);
				return;
			}
			float d = Vector2.Distance(this.m_target.transform.position, this.m_eye.transform.position) / this.m_lastAmmo.m_shared.m_attack.m_projectileVel;
			Vector3 b = this.m_target.GetVelocity() * d * this.m_predictionModifier;
			forward = this.m_target.transform.position + b - this.m_turretBody.transform.position;
			float y = forward.y;
			CapsuleCollider componentInChildren = this.m_target.GetComponentInChildren<CapsuleCollider>();
			forward.y = y + ((componentInChildren != null) ? (componentInChildren.height / 2f) : 1f);
		}
		else if (!this.HasAmmo())
		{
			forward = base.transform.forward + new Vector3(0f, -0.3f, 0f);
		}
		else
		{
			this.m_scan += fixedDeltaTime;
			if (this.m_scan > this.m_noTargetScanRate * 2f)
			{
				this.m_scan = 0f;
			}
			forward = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y + (float)((this.m_scan - this.m_noTargetScanRate > 0f) ? 1 : -1) * this.m_horizontalAngle, 0f) * Vector3.forward;
		}
		forward.Normalize();
		Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
		Vector3 eulerAngles = quaternion.eulerAngles;
		float y2 = base.transform.rotation.eulerAngles.y;
		eulerAngles.y -= y2;
		if (this.m_horizontalAngle >= 0f)
		{
			float num = eulerAngles.y;
			if (num > 180f)
			{
				num -= 360f;
			}
			else if (num < -180f)
			{
				num += 360f;
			}
			if (num > this.m_horizontalAngle)
			{
				eulerAngles = new Vector3(eulerAngles.x, this.m_horizontalAngle + y2, eulerAngles.z);
				quaternion.eulerAngles = eulerAngles;
			}
			else if (num < -this.m_horizontalAngle)
			{
				eulerAngles = new Vector3(eulerAngles.x, -this.m_horizontalAngle + y2, eulerAngles.z);
				quaternion.eulerAngles = eulerAngles;
			}
		}
		Quaternion quaternion2 = Utils.RotateTorwardsSmooth(this.m_turretBody.transform.rotation, quaternion, this.m_lastRotation, this.m_turnRate * fixedDeltaTime, this.m_lookAcceleration, this.m_lookDeacceleration, this.m_lookMinDegreesDelta);
		this.m_lastRotation = this.m_turretBody.transform.rotation;
		this.m_turretBody.transform.rotation = this.m_baseBodyRotation * quaternion2;
		this.m_turretNeck.transform.rotation = this.m_baseNeckRotation * Quaternion.Euler(0f, this.m_turretBody.transform.rotation.eulerAngles.y, this.m_turretBody.transform.rotation.eulerAngles.z);
		this.m_aimDiffToTarget = (flag ? Quaternion.Dot(quaternion2, quaternion) : -1f);
	}

	// Token: 0x06001A40 RID: 6720 RVA: 0x000ADE78 File Offset: 0x000AC078
	private void UpdateTarget(float dt)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.HasAmmo())
		{
			if (this.m_haveTarget)
			{
				this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", new object[]
				{
					ZDOID.None
				});
			}
			return;
		}
		this.m_updateTargetTimer -= dt;
		if (this.m_updateTargetTimer <= 0f)
		{
			this.m_updateTargetTimer = (Character.IsCharacterInRange(base.transform.position, 40f) ? this.m_updateTargetIntervalNear : this.m_updateTargetIntervalFar);
			Character character = BaseAI.FindClosestCreature(base.transform, this.m_eye.transform.position, 0f, this.m_viewDistance, this.m_horizontalAngle, false, false, this.m_targetPlayers, (this.m_targetItems.Count > 0) ? this.m_targetTamedConfig : this.m_targetTamed, this.m_targetCharacters);
			if (character != this.m_target)
			{
				if (character)
				{
					this.m_newTargetEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				}
				else
				{
					this.m_lostTargetEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				}
				this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", new object[]
				{
					character ? character.GetZDOID() : ZDOID.None
				});
			}
		}
		if (this.m_haveTarget && (!this.m_target || this.m_target.IsDead()))
		{
			ZLog.Log("Target is gone");
			this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", new object[]
			{
				ZDOID.None
			});
			this.m_lostTargetEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		}
	}

	// Token: 0x06001A41 RID: 6721 RVA: 0x000AE08E File Offset: 0x000AC28E
	private void UpdateAttack(float dt)
	{
		if (!this.m_target)
		{
			return;
		}
		if (this.m_aimDiffToTarget < this.m_shootWhenAimDiff)
		{
			return;
		}
		if (!this.HasAmmo())
		{
			return;
		}
		if (this.IsCoolingDown())
		{
			return;
		}
		this.ShootProjectile();
	}

	// Token: 0x06001A42 RID: 6722 RVA: 0x000AE0C8 File Offset: 0x000AC2C8
	public void ShootProjectile()
	{
		Transform transform = this.m_eye.transform;
		this.m_shootEffect.Create(transform.position, transform.rotation, null, 1f, -1);
		this.m_nview.GetZDO().Set(ZDOVars.s_lastAttack, (float)ZNet.instance.GetTimeSeconds());
		this.m_lastAmmo = this.GetAmmoItem();
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_ammo, 0);
		int num = Mathf.Min(1, (this.m_maxAmmo == 0) ? this.m_lastAmmo.m_shared.m_attack.m_projectiles : Mathf.Min(@int, this.m_lastAmmo.m_shared.m_attack.m_projectiles));
		if (this.m_maxAmmo > 0)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_ammo, @int - num, false);
		}
		ZLog.Log(string.Format("Turret '{0}' is shooting {1} projectiles, ammo: {2}/{3}", new object[]
		{
			base.name,
			num,
			@int,
			this.m_maxAmmo
		}));
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = transform.forward;
			Vector3 axis = Vector3.Cross(vector, Vector3.up);
			float projectileAccuracy = this.m_lastAmmo.m_shared.m_attack.m_projectileAccuracy;
			Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-projectileAccuracy, projectileAccuracy), Vector3.up);
			vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-projectileAccuracy, projectileAccuracy), axis) * vector;
			vector = rotation * vector;
			this.m_lastProjectile = UnityEngine.Object.Instantiate<GameObject>(this.m_lastAmmo.m_shared.m_attack.m_attackProjectile, transform.position, transform.rotation);
			HitData hitData = new HitData();
			hitData.m_toolTier = (short)this.m_lastAmmo.m_shared.m_toolTier;
			hitData.m_pushForce = this.m_lastAmmo.m_shared.m_attackForce;
			hitData.m_backstabBonus = this.m_lastAmmo.m_shared.m_backstabBonus;
			hitData.m_staggerMultiplier = this.m_lastAmmo.m_shared.m_attack.m_staggerMultiplier;
			hitData.m_damage.Add(this.m_lastAmmo.GetDamage(), 1);
			hitData.m_statusEffectHash = (this.m_lastAmmo.m_shared.m_attackStatusEffect ? this.m_lastAmmo.m_shared.m_attackStatusEffect.NameHash() : 0);
			hitData.m_blockable = this.m_lastAmmo.m_shared.m_blockable;
			hitData.m_dodgeable = this.m_lastAmmo.m_shared.m_dodgeable;
			hitData.m_skill = this.m_lastAmmo.m_shared.m_skillType;
			if (this.m_lastAmmo.m_shared.m_attackStatusEffect != null)
			{
				hitData.m_statusEffectHash = this.m_lastAmmo.m_shared.m_attackStatusEffect.NameHash();
			}
			IProjectile component = this.m_lastProjectile.GetComponent<IProjectile>();
			if (component != null)
			{
				component.Setup(null, vector * this.m_lastAmmo.m_shared.m_attack.m_projectileVel, this.m_hitNoise, hitData, null, this.m_lastAmmo);
			}
		}
	}

	// Token: 0x06001A43 RID: 6723 RVA: 0x000AE3FA File Offset: 0x000AC5FA
	public bool IsCoolingDown()
	{
		return this.m_nview.IsValid() && (double)(this.m_nview.GetZDO().GetFloat(ZDOVars.s_lastAttack, 0f) + this.m_attackCooldown) > ZNet.instance.GetTimeSeconds();
	}

	// Token: 0x06001A44 RID: 6724 RVA: 0x000AE439 File Offset: 0x000AC639
	public bool HasAmmo()
	{
		return this.m_maxAmmo == 0 || this.GetAmmo() > 0;
	}

	// Token: 0x06001A45 RID: 6725 RVA: 0x000AE44E File Offset: 0x000AC64E
	public int GetAmmo()
	{
		return this.m_nview.GetZDO().GetInt(ZDOVars.s_ammo, 0);
	}

	// Token: 0x06001A46 RID: 6726 RVA: 0x000AE466 File Offset: 0x000AC666
	public string GetAmmoType()
	{
		if (!this.m_defaultAmmo)
		{
			return this.m_nview.GetZDO().GetString(ZDOVars.s_ammoType, "");
		}
		return this.m_defaultAmmo.name;
	}

	// Token: 0x06001A47 RID: 6727 RVA: 0x000AE49C File Offset: 0x000AC69C
	public void UpdateReloadState()
	{
		bool flag = this.IsCoolingDown();
		if (!this.m_turretBodyArmed.activeInHierarchy && !flag)
		{
			this.m_reloadEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		}
		this.m_turretBodyArmed.SetActive(!flag);
		this.m_turretBodyUnarmed.SetActive(flag);
	}

	// Token: 0x06001A48 RID: 6728 RVA: 0x000AE504 File Offset: 0x000AC704
	private ItemDrop.ItemData GetAmmoItem()
	{
		string ammoType = this.GetAmmoType();
		GameObject prefab = ZNetScene.instance.GetPrefab(ammoType);
		if (!prefab)
		{
			ZLog.LogWarning("Turret '" + base.name + "' is trying to fire but has no ammo or default ammo!");
			return null;
		}
		return prefab.GetComponent<ItemDrop>().m_itemData;
	}

	// Token: 0x06001A49 RID: 6729 RVA: 0x000AE554 File Offset: 0x000AC754
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		this.sb.Clear();
		this.sb.Append((!this.HasAmmo()) ? (this.m_name + " ($piece_turret_noammo)") : string.Format("{0} ({1} / {2})", this.m_name, this.GetAmmo(), this.m_maxAmmo));
		if (this.m_targetCharacters.Count == 0)
		{
			this.sb.Append(" $piece_turret_target $piece_turret_target_everything");
		}
		else
		{
			this.sb.Append(" $piece_turret_target ");
			this.sb.Append(this.m_targetsText);
		}
		this.sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_turret_addammo\n[<color=yellow><b>1-8</b></color>] $piece_turret_target_set");
		return Localization.instance.Localize(this.sb.ToString());
	}

	// Token: 0x06001A4A RID: 6730 RVA: 0x000AE669 File Offset: 0x000AC869
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001A4B RID: 6731 RVA: 0x000AE671 File Offset: 0x000AC871
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			if (this.m_holdRepeatInterval <= 0f)
			{
				return false;
			}
			if (Time.time - this.m_lastUseTime < this.m_holdRepeatInterval)
			{
				return false;
			}
		}
		this.m_lastUseTime = Time.time;
		return this.UseItem(character, null);
	}

	// Token: 0x06001A4C RID: 6732 RVA: 0x000AE6B0 File Offset: 0x000AC8B0
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (item == null)
		{
			item = this.FindAmmoItem(user.GetInventory(), true);
			if (item == null)
			{
				if (this.GetAmmo() > 0 && this.FindAmmoItem(user.GetInventory(), false) != null)
				{
					ItemDrop component = ZNetScene.instance.GetPrefab(this.GetAmmoType()).GetComponent<ItemDrop>();
					user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_turretotherammo") + Localization.instance.Localize(component.m_itemData.m_shared.m_name), 0, null);
					return false;
				}
				user.Message(MessageHud.MessageType.Center, "$msg_noturretammo", 0, null);
				return false;
			}
		}
		foreach (Turret.TrophyTarget trophyTarget in this.m_configTargets)
		{
			if (item.m_shared.m_name == trophyTarget.m_item.m_itemData.m_shared.m_name)
			{
				if (this.m_targetItems.Contains(trophyTarget.m_item))
				{
					this.m_targetItems.Remove(trophyTarget.m_item);
				}
				else
				{
					if (this.m_targetItems.Count >= this.m_maxConfigTargets)
					{
						this.m_targetItems.RemoveAt(0);
					}
					this.m_targetItems.Add(trophyTarget.m_item);
				}
				this.SetTargets();
				Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_turret_target_set_msg " + ((this.m_targetCharacters.Count == 0) ? "$piece_turret_target_everything" : this.m_targetsText)), 0, null);
				this.m_setTargetEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				return true;
			}
		}
		if (!this.IsItemAllowed(item.m_dropPrefab.name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wontwork", 0, null);
			return false;
		}
		if (this.GetAmmo() > 0 && this.GetAmmoType() != item.m_dropPrefab.name)
		{
			ItemDrop component2 = ZNetScene.instance.GetPrefab(this.GetAmmoType()).GetComponent<ItemDrop>();
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_turretotherammo") + Localization.instance.Localize(component2.m_itemData.m_shared.m_name), 0, null);
			return false;
		}
		ZLog.Log("trying to add ammo " + item.m_shared.m_name);
		if (this.GetAmmo() >= this.m_maxAmmo)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull", 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name, 0, null);
		user.GetInventory().RemoveItem(item, 1);
		this.m_nview.InvokeRPC("RPC_AddAmmo", new object[]
		{
			item.m_dropPrefab.name
		});
		return true;
	}

	// Token: 0x06001A4D RID: 6733 RVA: 0x000AE9B0 File Offset: 0x000ACBB0
	private void RPC_AddAmmo(long sender, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.IsItemAllowed(name))
		{
			ZLog.Log("Item not allowed " + name);
			return;
		}
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_ammo, 0);
		this.m_nview.GetZDO().Set(ZDOVars.s_ammo, @int + 1, false);
		this.m_nview.GetZDO().Set(ZDOVars.s_ammoType, name);
		this.m_addAmmoEffect.Create(this.m_turretBody.transform.position, this.m_turretBody.transform.rotation, null, 1f, -1);
		this.UpdateVisualBolt();
		ZLog.Log("Added ammo " + name);
	}

	// Token: 0x06001A4E RID: 6734 RVA: 0x000AEA74 File Offset: 0x000ACC74
	private void RPC_SetTarget(long sender, ZDOID character)
	{
		GameObject gameObject = ZNetScene.instance.FindInstance(character);
		if (gameObject)
		{
			Character component = gameObject.GetComponent<Character>();
			if (component != null)
			{
				this.m_target = component;
				this.m_haveTarget = true;
				return;
			}
		}
		this.m_target = null;
		this.m_haveTarget = false;
		this.m_scan = 0f;
	}

	// Token: 0x06001A4F RID: 6735 RVA: 0x000AEAC8 File Offset: 0x000ACCC8
	private void UpdateVisualBolt()
	{
		if (this.HasAmmo())
		{
			bool flag = !this.IsCoolingDown();
		}
		string ammoType = this.GetAmmoType();
		bool flag2 = this.HasAmmo() && !this.IsCoolingDown();
		foreach (Turret.AmmoType ammoType2 in this.m_allowedAmmo)
		{
			bool flag3 = ammoType2.m_ammo.name == ammoType;
			ammoType2.m_visual.SetActive(flag3 && flag2);
		}
	}

	// Token: 0x06001A50 RID: 6736 RVA: 0x000AEB64 File Offset: 0x000ACD64
	private bool IsItemAllowed(string itemName)
	{
		using (List<Turret.AmmoType>.Enumerator enumerator = this.m_allowedAmmo.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_ammo.name == itemName)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06001A51 RID: 6737 RVA: 0x000AEBC8 File Offset: 0x000ACDC8
	private ItemDrop.ItemData FindAmmoItem(Inventory inventory, bool onlyCurrentlyLoadableType)
	{
		if (onlyCurrentlyLoadableType && this.HasAmmo())
		{
			return inventory.GetAmmoItem(this.m_ammoType, this.GetAmmoType());
		}
		return inventory.GetAmmoItem(this.m_ammoType, null);
	}

	// Token: 0x06001A52 RID: 6738 RVA: 0x000AEBF8 File Offset: 0x000ACDF8
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner() && this.m_returnAmmoOnDestroy)
		{
			int ammo = this.GetAmmo();
			string ammoType = this.GetAmmoType();
			GameObject prefab = ZNetScene.instance.GetPrefab(ammoType);
			for (int i = 0; i < ammo; i++)
			{
				Vector3 position = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
				Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
			}
		}
	}

	// Token: 0x06001A53 RID: 6739 RVA: 0x000AEC96 File Offset: 0x000ACE96
	public void ShowHoverMarker()
	{
		this.ShowBuildMarker();
	}

	// Token: 0x06001A54 RID: 6740 RVA: 0x000AEC9E File Offset: 0x000ACE9E
	public void ShowBuildMarker()
	{
		if (this.m_marker)
		{
			this.m_marker.gameObject.SetActive(true);
			base.CancelInvoke("HideMarker");
			base.Invoke("HideMarker", this.m_markerHideTime);
		}
	}

	// Token: 0x06001A55 RID: 6741 RVA: 0x000AECDC File Offset: 0x000ACEDC
	private void UpdateMarker(float dt)
	{
		if (this.m_marker && this.m_marker.isActiveAndEnabled)
		{
			this.m_marker.m_start = base.transform.rotation.eulerAngles.y - this.m_horizontalAngle;
			this.m_marker.m_turns = this.m_horizontalAngle * 2f / 360f;
		}
	}

	// Token: 0x06001A56 RID: 6742 RVA: 0x000AED4A File Offset: 0x000ACF4A
	private void HideMarker()
	{
		if (this.m_marker)
		{
			this.m_marker.gameObject.SetActive(false);
		}
	}

	// Token: 0x06001A57 RID: 6743 RVA: 0x000AED6C File Offset: 0x000ACF6C
	private void SetTargets()
	{
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.ClaimOwnership();
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_targets, this.m_targetItems.Count, false);
		for (int i = 0; i < this.m_targetItems.Count; i++)
		{
			this.m_nview.GetZDO().Set("target" + i.ToString(), this.m_targetItems[i].m_itemData.m_shared.m_name);
		}
		this.ReadTargets();
	}

	// Token: 0x06001A58 RID: 6744 RVA: 0x000AEE0C File Offset: 0x000AD00C
	private void ReadTargets()
	{
		if (!this.m_nview || !this.m_nview.IsValid())
		{
			return;
		}
		this.m_targetItems.Clear();
		this.m_targetCharacters.Clear();
		this.m_targetsText = "";
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_targets, 0);
		for (int i = 0; i < @int; i++)
		{
			string @string = this.m_nview.GetZDO().GetString("target" + i.ToString(), "");
			foreach (Turret.TrophyTarget trophyTarget in this.m_configTargets)
			{
				if (trophyTarget.m_item.m_itemData.m_shared.m_name == @string)
				{
					this.m_targetItems.Add(trophyTarget.m_item);
					this.m_targetCharacters.AddRange(trophyTarget.m_targets);
					if (this.m_targetsText.Length > 0)
					{
						this.m_targetsText += ", ";
					}
					if (!string.IsNullOrEmpty(trophyTarget.m_nameOverride))
					{
						this.m_targetsText += trophyTarget.m_nameOverride;
						break;
					}
					for (int j = 0; j < trophyTarget.m_targets.Count; j++)
					{
						this.m_targetsText += trophyTarget.m_targets[j].m_name;
						if (j + 1 < trophyTarget.m_targets.Count)
						{
							this.m_targetsText += ", ";
						}
					}
					break;
				}
			}
		}
	}

	// Token: 0x04001C28 RID: 7208
	public string m_name = "Turret";

	// Token: 0x04001C29 RID: 7209
	[Header("Turret")]
	public GameObject m_turretBody;

	// Token: 0x04001C2A RID: 7210
	public GameObject m_turretBodyArmed;

	// Token: 0x04001C2B RID: 7211
	public GameObject m_turretBodyUnarmed;

	// Token: 0x04001C2C RID: 7212
	public GameObject m_turretNeck;

	// Token: 0x04001C2D RID: 7213
	public GameObject m_eye;

	// Token: 0x04001C2E RID: 7214
	[Header("Look & Scan")]
	public float m_turnRate = 10f;

	// Token: 0x04001C2F RID: 7215
	public float m_horizontalAngle = 25f;

	// Token: 0x04001C30 RID: 7216
	public float m_verticalAngle = 20f;

	// Token: 0x04001C31 RID: 7217
	public float m_viewDistance = 10f;

	// Token: 0x04001C32 RID: 7218
	public float m_noTargetScanRate = 10f;

	// Token: 0x04001C33 RID: 7219
	public float m_lookAcceleration = 1.2f;

	// Token: 0x04001C34 RID: 7220
	public float m_lookDeacceleration = 0.05f;

	// Token: 0x04001C35 RID: 7221
	public float m_lookMinDegreesDelta = 0.005f;

	// Token: 0x04001C36 RID: 7222
	[Header("Attack Settings (rest in projectile)")]
	public ItemDrop m_defaultAmmo;

	// Token: 0x04001C37 RID: 7223
	public float m_attackCooldown = 1f;

	// Token: 0x04001C38 RID: 7224
	public float m_attackWarmup = 1f;

	// Token: 0x04001C39 RID: 7225
	public float m_hitNoise = 10f;

	// Token: 0x04001C3A RID: 7226
	public float m_shootWhenAimDiff = 0.9f;

	// Token: 0x04001C3B RID: 7227
	public float m_predictionModifier = 1f;

	// Token: 0x04001C3C RID: 7228
	public float m_updateTargetIntervalNear = 1f;

	// Token: 0x04001C3D RID: 7229
	public float m_updateTargetIntervalFar = 10f;

	// Token: 0x04001C3E RID: 7230
	[Header("Ammo")]
	public int m_maxAmmo;

	// Token: 0x04001C3F RID: 7231
	public string m_ammoType = "$ammo_turretbolt";

	// Token: 0x04001C40 RID: 7232
	public List<Turret.AmmoType> m_allowedAmmo = new List<Turret.AmmoType>();

	// Token: 0x04001C41 RID: 7233
	public bool m_returnAmmoOnDestroy = true;

	// Token: 0x04001C42 RID: 7234
	public float m_holdRepeatInterval = 0.2f;

	// Token: 0x04001C43 RID: 7235
	[Header("Target mode: Everything")]
	public bool m_targetPlayers = true;

	// Token: 0x04001C44 RID: 7236
	public bool m_targetTamed = true;

	// Token: 0x04001C45 RID: 7237
	[Header("Target mode: Configured")]
	public bool m_targetTamedConfig;

	// Token: 0x04001C46 RID: 7238
	public List<Turret.TrophyTarget> m_configTargets = new List<Turret.TrophyTarget>();

	// Token: 0x04001C47 RID: 7239
	public int m_maxConfigTargets = 1;

	// Token: 0x04001C48 RID: 7240
	[Header("Effects")]
	public CircleProjector m_marker;

	// Token: 0x04001C49 RID: 7241
	public float m_markerHideTime = 0.5f;

	// Token: 0x04001C4A RID: 7242
	public EffectList m_shootEffect;

	// Token: 0x04001C4B RID: 7243
	public EffectList m_addAmmoEffect;

	// Token: 0x04001C4C RID: 7244
	public EffectList m_reloadEffect;

	// Token: 0x04001C4D RID: 7245
	public EffectList m_warmUpStartEffect;

	// Token: 0x04001C4E RID: 7246
	public EffectList m_newTargetEffect;

	// Token: 0x04001C4F RID: 7247
	public EffectList m_lostTargetEffect;

	// Token: 0x04001C50 RID: 7248
	public EffectList m_setTargetEffect;

	// Token: 0x04001C51 RID: 7249
	private ZNetView m_nview;

	// Token: 0x04001C52 RID: 7250
	private GameObject m_lastProjectile;

	// Token: 0x04001C53 RID: 7251
	private ItemDrop.ItemData m_lastAmmo;

	// Token: 0x04001C54 RID: 7252
	private Character m_target;

	// Token: 0x04001C55 RID: 7253
	private bool m_haveTarget;

	// Token: 0x04001C56 RID: 7254
	private Quaternion m_baseBodyRotation;

	// Token: 0x04001C57 RID: 7255
	private Quaternion m_baseNeckRotation;

	// Token: 0x04001C58 RID: 7256
	private Quaternion m_lastRotation;

	// Token: 0x04001C59 RID: 7257
	private float m_aimDiffToTarget;

	// Token: 0x04001C5A RID: 7258
	private float m_updateTargetTimer;

	// Token: 0x04001C5B RID: 7259
	private float m_lastUseTime;

	// Token: 0x04001C5C RID: 7260
	private float m_scan;

	// Token: 0x04001C5D RID: 7261
	private readonly List<ItemDrop> m_targetItems = new List<ItemDrop>();

	// Token: 0x04001C5E RID: 7262
	private readonly List<Character> m_targetCharacters = new List<Character>();

	// Token: 0x04001C5F RID: 7263
	private string m_targetsText;

	// Token: 0x04001C60 RID: 7264
	private readonly StringBuilder sb = new StringBuilder();

	// Token: 0x04001C61 RID: 7265
	private uint m_lastUpdateTargetRevision = uint.MaxValue;

	// Token: 0x020002B7 RID: 695
	[Serializable]
	public struct AmmoType
	{
		// Token: 0x04001C62 RID: 7266
		public ItemDrop m_ammo;

		// Token: 0x04001C63 RID: 7267
		public GameObject m_visual;
	}

	// Token: 0x020002B8 RID: 696
	[Serializable]
	public struct TrophyTarget
	{
		// Token: 0x04001C64 RID: 7268
		public string m_nameOverride;

		// Token: 0x04001C65 RID: 7269
		public ItemDrop m_item;

		// Token: 0x04001C66 RID: 7270
		public List<Character> m_targets;
	}
}
