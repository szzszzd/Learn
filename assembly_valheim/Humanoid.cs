using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000017 RID: 23
public class Humanoid : Character
{
	// Token: 0x0600015C RID: 348 RVA: 0x00009670 File Offset: 0x00007870
	protected override void Awake()
	{
		base.Awake();
		this.m_visEquipment = base.GetComponent<VisEquipment>();
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_seed = this.m_nview.GetZDO().GetInt(ZDOVars.s_seed, 0);
		if (this.m_seed == 0)
		{
			this.m_seed = this.m_nview.GetZDO().m_uid.GetHashCode();
			this.m_nview.GetZDO().Set(ZDOVars.s_seed, this.m_seed, true);
		}
	}

	// Token: 0x0600015D RID: 349 RVA: 0x000096FE File Offset: 0x000078FE
	protected override void OnEnable()
	{
		base.OnEnable();
		Humanoid.Instances.Add(this);
	}

	// Token: 0x0600015E RID: 350 RVA: 0x00009711 File Offset: 0x00007911
	protected override void OnDisable()
	{
		base.OnDisable();
		Humanoid.Instances.Remove(this);
	}

	// Token: 0x0600015F RID: 351 RVA: 0x00009725 File Offset: 0x00007925
	protected override void Start()
	{
		if (!this.IsPlayer())
		{
			this.GiveDefaultItems();
		}
	}

	// Token: 0x06000160 RID: 352 RVA: 0x00009735 File Offset: 0x00007935
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	// Token: 0x06000161 RID: 353 RVA: 0x00009740 File Offset: 0x00007940
	public void GiveDefaultItems()
	{
		foreach (GameObject prefab in this.m_defaultItems)
		{
			this.GiveDefaultItem(prefab);
		}
		if (this.m_randomWeapon.Length != 0 || this.m_randomArmor.Length != 0 || this.m_randomShield.Length != 0 || this.m_randomSets.Length != 0)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(this.m_seed);
			if (this.m_randomShield.Length != 0)
			{
				GameObject gameObject = this.m_randomShield[UnityEngine.Random.Range(0, this.m_randomShield.Length)];
				if (gameObject)
				{
					this.GiveDefaultItem(gameObject);
				}
			}
			if (this.m_randomWeapon.Length != 0)
			{
				GameObject gameObject2 = this.m_randomWeapon[UnityEngine.Random.Range(0, this.m_randomWeapon.Length)];
				if (gameObject2)
				{
					this.GiveDefaultItem(gameObject2);
				}
			}
			if (this.m_randomArmor.Length != 0)
			{
				GameObject gameObject3 = this.m_randomArmor[UnityEngine.Random.Range(0, this.m_randomArmor.Length)];
				if (gameObject3)
				{
					this.GiveDefaultItem(gameObject3);
				}
			}
			if (this.m_randomSets.Length != 0)
			{
				foreach (GameObject prefab2 in this.m_randomSets[UnityEngine.Random.Range(0, this.m_randomSets.Length)].m_items)
				{
					this.GiveDefaultItem(prefab2);
				}
			}
			UnityEngine.Random.state = state;
		}
	}

	// Token: 0x06000162 RID: 354 RVA: 0x00009880 File Offset: 0x00007A80
	private void GiveDefaultItem(GameObject prefab)
	{
		ItemDrop.ItemData itemData = this.PickupPrefab(prefab, 0, false);
		if (itemData != null && !itemData.IsWeapon())
		{
			this.EquipItem(itemData, false);
		}
	}

	// Token: 0x06000163 RID: 355 RVA: 0x000098AC File Offset: 0x00007AAC
	public new void CustomFixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview == null || this.m_nview.IsOwner())
		{
			this.UpdateAttack(Time.fixedDeltaTime);
			this.UpdateEquipment(Time.fixedDeltaTime);
			this.UpdateBlock(Time.fixedDeltaTime);
		}
	}

	// Token: 0x06000164 RID: 356 RVA: 0x00009903 File Offset: 0x00007B03
	public override bool InAttack()
	{
		return base.GetNextAnimHash() == Humanoid.s_animatorTagAttack || base.GetCurrentAnimHash() == Humanoid.s_animatorTagAttack;
	}

	// Token: 0x06000165 RID: 357 RVA: 0x00009924 File Offset: 0x00007B24
	public override bool StartAttack(Character target, bool secondaryAttack)
	{
		if ((this.InAttack() && !this.HaveQueuedChain()) || this.InDodge() || !this.CanMove() || base.IsKnockedBack() || base.IsStaggering() || this.InMinorAction())
		{
			return false;
		}
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return false;
		}
		if (secondaryAttack && !currentWeapon.HaveSecondaryAttack())
		{
			return false;
		}
		if (!secondaryAttack && !currentWeapon.HavePrimaryAttack())
		{
			return false;
		}
		if (this.m_currentAttack != null)
		{
			this.m_currentAttack.Stop();
			this.m_previousAttack = this.m_currentAttack;
			this.m_currentAttack = null;
		}
		Attack attack = secondaryAttack ? currentWeapon.m_shared.m_secondaryAttack.Clone() : currentWeapon.m_shared.m_attack.Clone();
		if (attack.Start(this, this.m_body, this.m_zanim, this.m_animEvent, this.m_visEquipment, currentWeapon, this.m_previousAttack, this.m_timeSinceLastAttack, this.GetAttackDrawPercentage()))
		{
			this.ClearActionQueue();
			this.m_currentAttack = attack;
			this.m_currentAttackIsSecondary = secondaryAttack;
			this.m_lastCombatTimer = 0f;
			return true;
		}
		return false;
	}

	// Token: 0x06000166 RID: 358 RVA: 0x00009A37 File Offset: 0x00007C37
	public override float GetTimeSinceLastAttack()
	{
		return this.m_timeSinceLastAttack;
	}

	// Token: 0x06000167 RID: 359 RVA: 0x00009A40 File Offset: 0x00007C40
	public float GetAttackDrawPercentage()
	{
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		if (currentWeapon == null || !currentWeapon.m_shared.m_attack.m_bowDraw || this.m_attackDrawTime <= 0f)
		{
			return 0f;
		}
		float skillFactor = this.GetSkillFactor(currentWeapon.m_shared.m_skillType);
		float num = Mathf.Lerp(currentWeapon.m_shared.m_attack.m_drawDurationMin, currentWeapon.m_shared.m_attack.m_drawDurationMin * 0.2f, skillFactor);
		if (num <= 0f)
		{
			return 1f;
		}
		return Mathf.Clamp01(this.m_attackDrawTime / num);
	}

	// Token: 0x06000168 RID: 360 RVA: 0x00009AD8 File Offset: 0x00007CD8
	private void UpdateEquipment(float dt)
	{
		if (!this.IsPlayer())
		{
			return;
		}
		if (base.IsSwimming() && !base.IsOnGround())
		{
			this.HideHandItems();
		}
		if (this.m_rightItem != null && this.m_rightItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_rightItem, dt);
		}
		if (this.m_leftItem != null && this.m_leftItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_leftItem, dt);
		}
		if (this.m_chestItem != null && this.m_chestItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_chestItem, dt);
		}
		if (this.m_legItem != null && this.m_legItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_legItem, dt);
		}
		if (this.m_helmetItem != null && this.m_helmetItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_helmetItem, dt);
		}
		if (this.m_shoulderItem != null && this.m_shoulderItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_shoulderItem, dt);
		}
		if (this.m_utilityItem != null && this.m_utilityItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_utilityItem, dt);
		}
	}

	// Token: 0x06000169 RID: 361 RVA: 0x00009C18 File Offset: 0x00007E18
	private void DrainEquipedItemDurability(ItemDrop.ItemData item, float dt)
	{
		item.m_durability -= item.m_shared.m_durabilityDrain * dt;
		if (item.m_durability > 0f)
		{
			return;
		}
		this.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_broke", new string[]
		{
			item.m_shared.m_name
		}), 0, item.GetIcon());
		this.UnequipItem(item, false);
		if (item.m_shared.m_destroyBroken)
		{
			this.m_inventory.RemoveItem(item);
		}
	}

	// Token: 0x0600016A RID: 362 RVA: 0x00009CA2 File Offset: 0x00007EA2
	protected override void OnDamaged(HitData hit)
	{
		this.SetCrouch(false);
	}

	// Token: 0x0600016B RID: 363 RVA: 0x00009CAC File Offset: 0x00007EAC
	public ItemDrop.ItemData GetCurrentWeapon()
	{
		if (this.m_rightItem != null && this.m_rightItem.IsWeapon())
		{
			return this.m_rightItem;
		}
		if (this.m_leftItem != null && this.m_leftItem.IsWeapon() && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Torch)
		{
			return this.m_leftItem;
		}
		if (this.m_unarmedWeapon)
		{
			return this.m_unarmedWeapon.m_itemData;
		}
		return null;
	}

	// Token: 0x0600016C RID: 364 RVA: 0x00009D1F File Offset: 0x00007F1F
	private ItemDrop.ItemData GetCurrentBlocker()
	{
		if (this.m_leftItem != null)
		{
			return this.m_leftItem;
		}
		return this.GetCurrentWeapon();
	}

	// Token: 0x0600016D RID: 365 RVA: 0x00009D38 File Offset: 0x00007F38
	private void UpdateAttack(float dt)
	{
		this.m_lastCombatTimer += dt;
		if (this.m_currentAttack != null && this.GetCurrentWeapon() != null)
		{
			this.m_currentAttack.Update(dt);
		}
		if (this.InAttack())
		{
			this.m_timeSinceLastAttack = 0f;
			return;
		}
		this.m_timeSinceLastAttack += dt;
	}

	// Token: 0x0600016E RID: 366 RVA: 0x00009D91 File Offset: 0x00007F91
	protected override float GetAttackSpeedFactorMovement()
	{
		if (!this.InAttack() || this.m_currentAttack == null)
		{
			return 1f;
		}
		if (!base.IsFlying() && !base.IsOnGround())
		{
			return 1f;
		}
		return this.m_currentAttack.m_speedFactor;
	}

	// Token: 0x0600016F RID: 367 RVA: 0x00009DCA File Offset: 0x00007FCA
	protected override float GetAttackSpeedFactorRotation()
	{
		if (this.InAttack() && this.m_currentAttack != null)
		{
			return this.m_currentAttack.m_speedFactorRotation;
		}
		return 1f;
	}

	// Token: 0x06000170 RID: 368 RVA: 0x0000247B File Offset: 0x0000067B
	protected virtual bool HaveQueuedChain()
	{
		return false;
	}

	// Token: 0x06000171 RID: 369 RVA: 0x00009DED File Offset: 0x00007FED
	public override void OnWeaponTrailStart()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner() && this.m_currentAttack != null && this.GetCurrentWeapon() != null)
		{
			this.m_currentAttack.OnTrailStart();
		}
	}

	// Token: 0x06000172 RID: 370 RVA: 0x00009E24 File Offset: 0x00008024
	public override void OnAttackTrigger()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_currentAttack != null && this.GetCurrentWeapon() != null)
		{
			this.m_currentAttack.OnAttackTrigger();
		}
	}

	// Token: 0x06000173 RID: 371 RVA: 0x00009E5C File Offset: 0x0000805C
	public override void OnStopMoving()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_currentAttack != null)
		{
			if (!this.InAttack())
			{
				return;
			}
			if (this.GetCurrentWeapon() != null)
			{
				this.m_currentAttack.m_speedFactor = 0f;
				this.m_currentAttack.m_speedFactorRotation = 0f;
			}
		}
	}

	// Token: 0x06000174 RID: 372 RVA: 0x00009EBD File Offset: 0x000080BD
	public virtual Vector3 GetAimDir(Vector3 fromPoint)
	{
		return base.GetLookDir();
	}

	// Token: 0x06000175 RID: 373 RVA: 0x00009EC8 File Offset: 0x000080C8
	public ItemDrop.ItemData PickupPrefab(GameObject prefab, int stackSize = 0, bool autoequip = true)
	{
		ZNetView.m_forceDisableInit = true;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
		ZNetView.m_forceDisableInit = false;
		if (stackSize > 0)
		{
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			component.m_itemData.m_stack = Mathf.Clamp(stackSize, 1, component.m_itemData.m_shared.m_maxStackSize);
		}
		if (this.Pickup(gameObject, autoequip, true))
		{
			return gameObject.GetComponent<ItemDrop>().m_itemData;
		}
		UnityEngine.Object.Destroy(gameObject);
		return null;
	}

	// Token: 0x06000176 RID: 374 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool HaveUniqueKey(string name)
	{
		return false;
	}

	// Token: 0x06000177 RID: 375 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void AddUniqueKey(string name)
	{
	}

	// Token: 0x06000178 RID: 376 RVA: 0x00009F34 File Offset: 0x00008134
	public bool Pickup(GameObject go, bool autoequip = true, bool autoPickupDelay = true)
	{
		if (this.IsTeleporting())
		{
			return false;
		}
		ItemDrop component = go.GetComponent<ItemDrop>();
		if (component == null)
		{
			return false;
		}
		component.Load();
		if (this.IsPlayer() && (component.m_itemData.m_shared.m_icons == null || component.m_itemData.m_shared.m_icons.Length == 0 || component.m_itemData.m_variant >= component.m_itemData.m_shared.m_icons.Length))
		{
			return false;
		}
		if (!component.CanPickup(autoPickupDelay))
		{
			return false;
		}
		if (this.m_inventory.ContainsItem(component.m_itemData))
		{
			return false;
		}
		if (component.m_itemData.m_shared.m_questItem && this.HaveUniqueKey(component.m_itemData.m_shared.m_name))
		{
			this.Message(MessageHud.MessageType.Center, "$msg_cantpickup", 0, null);
			return false;
		}
		int stack = component.m_itemData.m_stack;
		bool flag = this.m_inventory.AddItem(component.m_itemData);
		if (this.m_nview.GetZDO() == null)
		{
			UnityEngine.Object.Destroy(go);
			return true;
		}
		if (!flag)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_noroom", 0, null);
			return false;
		}
		if (component.m_itemData.m_shared.m_questItem)
		{
			this.AddUniqueKey(component.m_itemData.m_shared.m_name);
		}
		ZNetScene.instance.Destroy(go);
		if (autoequip && flag && this.IsPlayer() && component.m_itemData.IsWeapon() && this.m_rightItem == null && this.m_hiddenRightItem == null && (this.m_leftItem == null || !this.m_leftItem.IsTwoHanded()) && (this.m_hiddenLeftItem == null || !this.m_hiddenLeftItem.IsTwoHanded()))
		{
			this.EquipItem(component.m_itemData, true);
		}
		this.m_pickupEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		if (this.IsPlayer())
		{
			base.ShowPickupMessage(component.m_itemData, stack);
		}
		return flag;
	}

	// Token: 0x06000179 RID: 377 RVA: 0x0000A120 File Offset: 0x00008320
	public void EquipBestWeapon(Character targetCreature, StaticTarget targetStatic, Character hurtFriend, Character friend)
	{
		List<ItemDrop.ItemData> allItems = this.m_inventory.GetAllItems();
		if (allItems.Count == 0)
		{
			return;
		}
		if (this.InAttack())
		{
			return;
		}
		float num = 0f;
		if (targetCreature)
		{
			float radius = targetCreature.GetRadius();
			num = Vector3.Distance(targetCreature.transform.position, base.transform.position) - radius;
		}
		else if (targetStatic)
		{
			num = Vector3.Distance(targetStatic.transform.position, base.transform.position);
		}
		float time = Time.time;
		base.IsFlying();
		base.IsSwimming();
		Humanoid.optimalWeapons.Clear();
		Humanoid.outofRangeWeapons.Clear();
		Humanoid.allWeapons.Clear();
		foreach (ItemDrop.ItemData itemData in allItems)
		{
			if (itemData.IsWeapon() && this.m_baseAI.CanUseAttack(itemData))
			{
				if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Enemy)
				{
					if (num >= itemData.m_shared.m_aiAttackRangeMin)
					{
						Humanoid.allWeapons.Add(itemData);
						if ((!(targetCreature == null) || !(targetStatic == null)) && time - itemData.m_lastAttackTime >= itemData.m_shared.m_aiAttackInterval)
						{
							if (num > itemData.m_shared.m_aiAttackRange)
							{
								Humanoid.outofRangeWeapons.Add(itemData);
							}
							else
							{
								if (itemData.m_shared.m_aiPrioritized)
								{
									this.EquipItem(itemData, true);
									return;
								}
								Humanoid.optimalWeapons.Add(itemData);
							}
						}
					}
				}
				else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt)
				{
					if (!(hurtFriend == null) && time - itemData.m_lastAttackTime >= itemData.m_shared.m_aiAttackInterval)
					{
						if (itemData.m_shared.m_aiPrioritized)
						{
							this.EquipItem(itemData, true);
							return;
						}
						Humanoid.optimalWeapons.Add(itemData);
					}
				}
				else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Friend && !(friend == null) && time - itemData.m_lastAttackTime >= itemData.m_shared.m_aiAttackInterval)
				{
					if (itemData.m_shared.m_aiPrioritized)
					{
						this.EquipItem(itemData, true);
						return;
					}
					Humanoid.optimalWeapons.Add(itemData);
				}
			}
		}
		if (Humanoid.optimalWeapons.Count > 0)
		{
			this.EquipItem(Humanoid.optimalWeapons[UnityEngine.Random.Range(0, Humanoid.optimalWeapons.Count)], true);
			return;
		}
		if (Humanoid.outofRangeWeapons.Count > 0)
		{
			this.EquipItem(Humanoid.outofRangeWeapons[UnityEngine.Random.Range(0, Humanoid.outofRangeWeapons.Count)], true);
			return;
		}
		if (Humanoid.allWeapons.Count > 0)
		{
			this.EquipItem(Humanoid.allWeapons[UnityEngine.Random.Range(0, Humanoid.allWeapons.Count)], true);
			return;
		}
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		this.UnequipItem(currentWeapon, false);
	}

	// Token: 0x0600017A RID: 378 RVA: 0x0000A448 File Offset: 0x00008648
	public bool DropItem(Inventory inventory, ItemDrop.ItemData item, int amount)
	{
		if (amount == 0)
		{
			return false;
		}
		if (item.m_shared.m_questItem)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_cantdrop", 0, null);
			return false;
		}
		if (amount > item.m_stack)
		{
			amount = item.m_stack;
		}
		this.RemoveEquipAction(item);
		this.UnequipItem(item, false);
		if (this.m_hiddenLeftItem == item)
		{
			this.m_hiddenLeftItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.m_hiddenRightItem == item)
		{
			this.m_hiddenRightItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (amount == item.m_stack)
		{
			ZLog.Log("drop all " + amount.ToString() + "  " + item.m_stack.ToString());
			if (!inventory.RemoveItem(item))
			{
				ZLog.Log("Was not removed");
				return false;
			}
		}
		else
		{
			ZLog.Log("drop some " + amount.ToString() + "  " + item.m_stack.ToString());
			inventory.RemoveItem(item, amount);
		}
		ItemDrop itemDrop = ItemDrop.DropItem(item, amount, base.transform.position + base.transform.forward + base.transform.up, base.transform.rotation);
		if (this.IsPlayer())
		{
			itemDrop.OnPlayerDrop();
		}
		itemDrop.GetComponent<Rigidbody>().velocity = (base.transform.forward + Vector3.up) * 5f;
		this.m_zanim.SetTrigger("interact");
		this.m_dropEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		this.Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + itemDrop.m_itemData.m_shared.m_name, itemDrop.m_itemData.m_stack, itemDrop.m_itemData.GetIcon());
		return true;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void SetPlaceMode(PieceTable buildPieces)
	{
	}

	// Token: 0x0600017C RID: 380 RVA: 0x0000A625 File Offset: 0x00008825
	public Inventory GetInventory()
	{
		return this.m_inventory;
	}

	// Token: 0x0600017D RID: 381 RVA: 0x0000A630 File Offset: 0x00008830
	public void UseItem(Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui)
	{
		if (inventory == null)
		{
			inventory = this.m_inventory;
		}
		if (!inventory.ContainsItem(item))
		{
			return;
		}
		GameObject hoverObject = this.GetHoverObject();
		Hoverable hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
		if (hoverable != null && !fromInventoryGui)
		{
			Interactable componentInParent = hoverObject.GetComponentInParent<Interactable>();
			if (componentInParent != null && componentInParent.UseItem(this, item))
			{
				this.DoInteractAnimation(hoverObject.transform.position);
				return;
			}
		}
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
		{
			if (this.ConsumeItem(inventory, item))
			{
				this.m_consumeItemEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f, -1);
				this.m_zanim.SetTrigger("eat");
			}
			return;
		}
		if (inventory == this.m_inventory && this.ToggleEquipped(item))
		{
			return;
		}
		if (!fromInventoryGui)
		{
			if (hoverable != null)
			{
				this.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantuseon", new string[]
				{
					item.m_shared.m_name,
					hoverable.GetHoverName()
				}), 0, null);
				return;
			}
			this.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_useonwhat", new string[]
			{
				item.m_shared.m_name
			}), 0, null);
		}
	}

	// Token: 0x0600017E RID: 382 RVA: 0x0000A764 File Offset: 0x00008964
	protected void DoInteractAnimation(Vector3 target)
	{
		Vector3 forward = target - base.transform.position;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation(forward);
		this.m_zanim.SetTrigger("interact");
	}

	// Token: 0x0600017F RID: 383 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual void ClearActionQueue()
	{
	}

	// Token: 0x06000180 RID: 384 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void RemoveEquipAction(ItemDrop.ItemData item)
	{
	}

	// Token: 0x06000181 RID: 385 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void ResetLoadedWeapon()
	{
	}

	// Token: 0x06000182 RID: 386 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsWeaponLoaded()
	{
		return false;
	}

	// Token: 0x06000183 RID: 387 RVA: 0x0000A7B7 File Offset: 0x000089B7
	protected virtual bool ToggleEquipped(ItemDrop.ItemData item)
	{
		if (!item.IsEquipable())
		{
			return false;
		}
		if (this.InAttack())
		{
			return true;
		}
		if (this.IsItemEquiped(item))
		{
			this.UnequipItem(item, true);
		}
		else
		{
			this.EquipItem(item, true);
		}
		return true;
	}

	// Token: 0x06000184 RID: 388 RVA: 0x0000A7EA File Offset: 0x000089EA
	public virtual bool CanConsumeItem(ItemDrop.ItemData item)
	{
		return item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable;
	}

	// Token: 0x06000185 RID: 389 RVA: 0x0000A7FA File Offset: 0x000089FA
	public virtual bool ConsumeItem(Inventory inventory, ItemDrop.ItemData item)
	{
		this.CanConsumeItem(item);
		return false;
	}

	// Token: 0x06000186 RID: 390 RVA: 0x0000A808 File Offset: 0x00008A08
	public bool EquipItem(ItemDrop.ItemData item, bool triggerEquipEffects = true)
	{
		if (this.IsItemEquiped(item))
		{
			return false;
		}
		if (!this.m_inventory.ContainsItem(item))
		{
			return false;
		}
		if (this.InAttack() || this.InDodge())
		{
			return false;
		}
		if (this.IsPlayer() && !this.IsDead() && base.IsSwimming() && !base.IsOnGround())
		{
			return false;
		}
		if (item.m_shared.m_useDurability && item.m_durability <= 0f)
		{
			return false;
		}
		if (item.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(item.m_shared.m_dlc))
		{
			this.Message(MessageHud.MessageType.Center, "$msg_dlcrequired", 0, null);
			return false;
		}
		if (Application.isEditor)
		{
			item.m_shared = item.m_dropPrefab.GetComponent<ItemDrop>().m_itemData.m_shared;
		}
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool)
		{
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.m_rightItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch)
		{
			if (this.m_rightItem != null && this.m_leftItem == null && this.m_rightItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon)
			{
				this.m_leftItem = item;
			}
			else
			{
				this.UnequipItem(this.m_rightItem, triggerEquipEffects);
				if (this.m_leftItem != null && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
				{
					this.UnequipItem(this.m_leftItem, triggerEquipEffects);
				}
				this.m_rightItem = item;
			}
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon)
		{
			if (this.m_rightItem != null && this.m_rightItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch && this.m_leftItem == null)
			{
				ItemDrop.ItemData rightItem = this.m_rightItem;
				this.UnequipItem(this.m_rightItem, triggerEquipEffects);
				this.m_leftItem = rightItem;
				this.m_leftItem.m_equipped = true;
			}
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			if (this.m_leftItem != null && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Torch)
			{
				this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			}
			this.m_rightItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			if (this.m_rightItem != null && this.m_rightItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon && this.m_rightItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Torch)
			{
				this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			}
			this.m_leftItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.m_leftItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.m_rightItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.m_leftItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest)
		{
			this.UnequipItem(this.m_chestItem, triggerEquipEffects);
			this.m_chestItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs)
		{
			this.UnequipItem(this.m_legItem, triggerEquipEffects);
			this.m_legItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable)
		{
			this.UnequipItem(this.m_ammoItem, triggerEquipEffects);
			this.m_ammoItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet)
		{
			this.UnequipItem(this.m_helmetItem, triggerEquipEffects);
			this.m_helmetItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder)
		{
			this.UnequipItem(this.m_shoulderItem, triggerEquipEffects);
			this.m_shoulderItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility)
		{
			this.UnequipItem(this.m_utilityItem, triggerEquipEffects);
			this.m_utilityItem = item;
		}
		if (this.IsItemEquiped(item))
		{
			item.m_equipped = true;
		}
		this.SetupEquipment();
		if (triggerEquipEffects)
		{
			this.TriggerEquipEffect(item);
		}
		return true;
	}

	// Token: 0x06000187 RID: 391 RVA: 0x0000ACB4 File Offset: 0x00008EB4
	public void UnequipItem(ItemDrop.ItemData item, bool triggerEquipEffects = true)
	{
		if (item == null)
		{
			return;
		}
		if (this.m_hiddenLeftItem == item)
		{
			this.m_hiddenLeftItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.m_hiddenRightItem == item)
		{
			this.m_hiddenRightItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.IsItemEquiped(item))
		{
			if (item.IsWeapon())
			{
				if (this.m_currentAttack != null && this.m_currentAttack.GetWeapon() == item)
				{
					this.m_currentAttack.Stop();
					this.m_previousAttack = this.m_currentAttack;
					this.m_currentAttack = null;
				}
				if (!string.IsNullOrEmpty(item.m_shared.m_attack.m_drawAnimationState))
				{
					this.m_zanim.SetBool(item.m_shared.m_attack.m_drawAnimationState, false);
				}
				this.m_attackDrawTime = 0f;
				this.ResetLoadedWeapon();
			}
			if (this.m_rightItem == item)
			{
				this.m_rightItem = null;
			}
			else if (this.m_leftItem == item)
			{
				this.m_leftItem = null;
			}
			else if (this.m_chestItem == item)
			{
				this.m_chestItem = null;
			}
			else if (this.m_legItem == item)
			{
				this.m_legItem = null;
			}
			else if (this.m_ammoItem == item)
			{
				this.m_ammoItem = null;
			}
			else if (this.m_helmetItem == item)
			{
				this.m_helmetItem = null;
			}
			else if (this.m_shoulderItem == item)
			{
				this.m_shoulderItem = null;
			}
			else if (this.m_utilityItem == item)
			{
				this.m_utilityItem = null;
			}
			item.m_equipped = false;
			this.SetupEquipment();
			if (triggerEquipEffects)
			{
				this.TriggerEquipEffect(item);
			}
		}
	}

	// Token: 0x06000188 RID: 392 RVA: 0x0000AE30 File Offset: 0x00009030
	private void TriggerEquipEffect(ItemDrop.ItemData item)
	{
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (MonoUpdaters.UpdateCount == this.m_lastEquipEffectFrame)
		{
			return;
		}
		this.m_lastEquipEffectFrame = MonoUpdaters.UpdateCount;
		this.m_equipEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x06000189 RID: 393 RVA: 0x0000AE87 File Offset: 0x00009087
	public override bool IsAttached()
	{
		return (this.m_currentAttack != null && this.InAttack() && this.m_currentAttack.IsAttached() && !this.m_currentAttack.IsDone()) || base.IsAttached();
	}

	// Token: 0x0600018A RID: 394 RVA: 0x0000AEBC File Offset: 0x000090BC
	public override bool GetRelativePosition(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		if (this.m_currentAttack != null && this.InAttack() && this.m_currentAttack.IsAttached() && !this.m_currentAttack.IsDone())
		{
			return this.m_currentAttack.GetAttachData(out parent, out attachJoint, out relativePos, out relativeRot, out relativeVel);
		}
		return base.GetRelativePosition(out parent, out attachJoint, out relativePos, out relativeRot, out relativeVel);
	}

	// Token: 0x0600018B RID: 395 RVA: 0x0000AF14 File Offset: 0x00009114
	public void UnequipAllItems()
	{
		this.UnequipItem(this.m_rightItem, false);
		this.UnequipItem(this.m_leftItem, false);
		this.UnequipItem(this.m_chestItem, false);
		this.UnequipItem(this.m_legItem, false);
		this.UnequipItem(this.m_helmetItem, false);
		this.UnequipItem(this.m_ammoItem, false);
		this.UnequipItem(this.m_shoulderItem, false);
		this.UnequipItem(this.m_utilityItem, false);
	}

	// Token: 0x0600018C RID: 396 RVA: 0x0000AF8C File Offset: 0x0000918C
	protected override void OnRagdollCreated(Ragdoll ragdoll)
	{
		VisEquipment component = ragdoll.GetComponent<VisEquipment>();
		if (component)
		{
			this.SetupVisEquipment(component, true);
		}
	}

	// Token: 0x0600018D RID: 397 RVA: 0x0000AFB0 File Offset: 0x000091B0
	protected virtual void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
	{
		if (!isRagdoll)
		{
			visEq.SetLeftItem((this.m_leftItem != null) ? this.m_leftItem.m_dropPrefab.name : "", (this.m_leftItem != null) ? this.m_leftItem.m_variant : 0);
			visEq.SetRightItem((this.m_rightItem != null) ? this.m_rightItem.m_dropPrefab.name : "");
			if (this.IsPlayer())
			{
				visEq.SetLeftBackItem((this.m_hiddenLeftItem != null) ? this.m_hiddenLeftItem.m_dropPrefab.name : "", (this.m_hiddenLeftItem != null) ? this.m_hiddenLeftItem.m_variant : 0);
				visEq.SetRightBackItem((this.m_hiddenRightItem != null) ? this.m_hiddenRightItem.m_dropPrefab.name : "");
			}
		}
		visEq.SetChestItem((this.m_chestItem != null) ? this.m_chestItem.m_dropPrefab.name : "");
		visEq.SetLegItem((this.m_legItem != null) ? this.m_legItem.m_dropPrefab.name : "");
		visEq.SetHelmetItem((this.m_helmetItem != null) ? this.m_helmetItem.m_dropPrefab.name : "");
		visEq.SetShoulderItem((this.m_shoulderItem != null) ? this.m_shoulderItem.m_dropPrefab.name : "", (this.m_shoulderItem != null) ? this.m_shoulderItem.m_variant : 0);
		visEq.SetUtilityItem((this.m_utilityItem != null) ? this.m_utilityItem.m_dropPrefab.name : "");
		if (this.IsPlayer())
		{
			visEq.SetBeardItem(this.m_beardItem);
			visEq.SetHairItem(this.m_hairItem);
		}
	}

	// Token: 0x0600018E RID: 398 RVA: 0x0000B17C File Offset: 0x0000937C
	private void SetupEquipment()
	{
		if (this.m_visEquipment && (this.m_nview.GetZDO() == null || this.m_nview.IsOwner()))
		{
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.m_nview.GetZDO() != null)
		{
			this.UpdateEquipmentStatusEffects();
			if (this.m_rightItem != null && this.m_rightItem.m_shared.m_buildPieces)
			{
				this.SetPlaceMode(this.m_rightItem.m_shared.m_buildPieces);
			}
			else
			{
				this.SetPlaceMode(null);
			}
			this.SetupAnimationState();
		}
	}

	// Token: 0x0600018F RID: 399 RVA: 0x0000B214 File Offset: 0x00009414
	private void SetupAnimationState()
	{
		if (this.m_leftItem != null)
		{
			if (this.m_leftItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch)
			{
				this.SetAnimationState(ItemDrop.ItemData.AnimationState.LeftTorch);
				return;
			}
			this.SetAnimationState(this.m_leftItem.m_shared.m_animationState);
			return;
		}
		else
		{
			if (this.m_rightItem != null)
			{
				this.SetAnimationState(this.m_rightItem.m_shared.m_animationState);
				return;
			}
			if (this.m_unarmedWeapon != null)
			{
				this.SetAnimationState(this.m_unarmedWeapon.m_itemData.m_shared.m_animationState);
			}
			return;
		}
	}

	// Token: 0x06000190 RID: 400 RVA: 0x0000B2A4 File Offset: 0x000094A4
	private void SetAnimationState(ItemDrop.ItemData.AnimationState state)
	{
		this.m_zanim.SetFloat(Humanoid.s_statef, (float)state);
		this.m_zanim.SetInt(Humanoid.s_statei, (int)state);
	}

	// Token: 0x06000191 RID: 401 RVA: 0x0000B2C9 File Offset: 0x000094C9
	public override bool IsSitting()
	{
		return base.GetCurrentAnimHash() == Character.s_animatorTagSitting;
	}

	// Token: 0x06000192 RID: 402 RVA: 0x0000B2D8 File Offset: 0x000094D8
	private void UpdateEquipmentStatusEffects()
	{
		HashSet<StatusEffect> hashSet = new HashSet<StatusEffect>();
		if (this.m_leftItem != null && this.m_leftItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_leftItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_rightItem != null && this.m_rightItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_rightItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_chestItem != null && this.m_chestItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_chestItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_legItem != null && this.m_legItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_legItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_helmetItem != null && this.m_helmetItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_helmetItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_shoulderItem != null && this.m_shoulderItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_shoulderItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_utilityItem != null && this.m_utilityItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_utilityItem.m_shared.m_equipStatusEffect);
		}
		if (this.HaveSetEffect(this.m_leftItem))
		{
			hashSet.Add(this.m_leftItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_rightItem))
		{
			hashSet.Add(this.m_rightItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_chestItem))
		{
			hashSet.Add(this.m_chestItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_legItem))
		{
			hashSet.Add(this.m_legItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_helmetItem))
		{
			hashSet.Add(this.m_helmetItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_shoulderItem))
		{
			hashSet.Add(this.m_shoulderItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_utilityItem))
		{
			hashSet.Add(this.m_utilityItem.m_shared.m_setStatusEffect);
		}
		foreach (StatusEffect statusEffect in this.m_equipmentStatusEffects)
		{
			if (!hashSet.Contains(statusEffect))
			{
				this.m_seman.RemoveStatusEffect(statusEffect.NameHash(), false);
			}
		}
		foreach (StatusEffect statusEffect2 in hashSet)
		{
			if (!this.m_equipmentStatusEffects.Contains(statusEffect2))
			{
				this.m_seman.AddStatusEffect(statusEffect2, false, 0, 0f);
			}
		}
		this.m_equipmentStatusEffects.Clear();
		this.m_equipmentStatusEffects.UnionWith(hashSet);
	}

	// Token: 0x06000193 RID: 403 RVA: 0x0000B634 File Offset: 0x00009834
	private bool HaveSetEffect(ItemDrop.ItemData item)
	{
		return item != null && !(item.m_shared.m_setStatusEffect == null) && item.m_shared.m_setName.Length != 0 && item.m_shared.m_setSize > 1 && this.GetSetCount(item.m_shared.m_setName) >= item.m_shared.m_setSize;
	}

	// Token: 0x06000194 RID: 404 RVA: 0x0000B69C File Offset: 0x0000989C
	private int GetSetCount(string setName)
	{
		int num = 0;
		if (this.m_leftItem != null && this.m_leftItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_rightItem != null && this.m_rightItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_chestItem != null && this.m_chestItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_legItem != null && this.m_legItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_helmetItem != null && this.m_helmetItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_shoulderItem != null && this.m_shoulderItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_utilityItem != null && this.m_utilityItem.m_shared.m_setName == setName)
		{
			num++;
		}
		return num;
	}

	// Token: 0x06000195 RID: 405 RVA: 0x0000B7A8 File Offset: 0x000099A8
	public void SetBeard(string name)
	{
		this.m_beardItem = name;
		this.SetupEquipment();
	}

	// Token: 0x06000196 RID: 406 RVA: 0x0000B7B7 File Offset: 0x000099B7
	public string GetBeard()
	{
		return this.m_beardItem;
	}

	// Token: 0x06000197 RID: 407 RVA: 0x0000B7BF File Offset: 0x000099BF
	public void SetHair(string hair)
	{
		this.m_hairItem = hair;
		this.SetupEquipment();
	}

	// Token: 0x06000198 RID: 408 RVA: 0x0000B7CE File Offset: 0x000099CE
	public string GetHair()
	{
		return this.m_hairItem;
	}

	// Token: 0x06000199 RID: 409 RVA: 0x0000B7D8 File Offset: 0x000099D8
	public bool IsItemEquiped(ItemDrop.ItemData item)
	{
		return this.m_rightItem == item || this.m_leftItem == item || this.m_chestItem == item || this.m_legItem == item || this.m_ammoItem == item || this.m_helmetItem == item || this.m_shoulderItem == item || this.m_utilityItem == item;
	}

	// Token: 0x0600019A RID: 410 RVA: 0x0000B83E File Offset: 0x00009A3E
	protected ItemDrop.ItemData GetRightItem()
	{
		return this.m_rightItem;
	}

	// Token: 0x0600019B RID: 411 RVA: 0x0000B846 File Offset: 0x00009A46
	protected ItemDrop.ItemData GetLeftItem()
	{
		return this.m_leftItem;
	}

	// Token: 0x0600019C RID: 412 RVA: 0x0000B84E File Offset: 0x00009A4E
	protected override bool CheckRun(Vector3 moveDir, float dt)
	{
		return !this.IsDrawingBow() && base.CheckRun(moveDir, dt) && !this.IsBlocking();
	}

	// Token: 0x0600019D RID: 413 RVA: 0x0000B870 File Offset: 0x00009A70
	public override bool IsDrawingBow()
	{
		if (this.m_attackDrawTime <= 0f)
		{
			return false;
		}
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		return currentWeapon != null && currentWeapon.m_shared.m_attack.m_bowDraw;
	}

	// Token: 0x0600019E RID: 414 RVA: 0x0000B8A8 File Offset: 0x00009AA8
	protected override bool BlockAttack(HitData hit, Character attacker)
	{
		if (Vector3.Dot(hit.m_dir, base.transform.forward) > 0f)
		{
			return false;
		}
		ItemDrop.ItemData currentBlocker = this.GetCurrentBlocker();
		if (currentBlocker == null)
		{
			return false;
		}
		bool flag = currentBlocker.m_shared.m_timedBlockBonus > 1f && this.m_blockTimer != -1f && this.m_blockTimer < 0.25f;
		float skillFactor = this.GetSkillFactor(Skills.SkillType.Blocking);
		float num = currentBlocker.GetBlockPower(skillFactor);
		if (flag)
		{
			num *= currentBlocker.m_shared.m_timedBlockBonus;
		}
		if (currentBlocker.m_shared.m_damageModifiers.Count > 0)
		{
			HitData.DamageModifiers modifiers = default(HitData.DamageModifiers);
			modifiers.Apply(currentBlocker.m_shared.m_damageModifiers);
			HitData.DamageModifier damageModifier;
			hit.ApplyResistance(modifiers, out damageModifier);
		}
		HitData.DamageTypes damageTypes = hit.m_damage.Clone();
		damageTypes.ApplyArmor(num);
		float totalBlockableDamage = hit.GetTotalBlockableDamage();
		float totalBlockableDamage2 = damageTypes.GetTotalBlockableDamage();
		float num2 = totalBlockableDamage - totalBlockableDamage2;
		float num3 = Mathf.Clamp01(num2 / num);
		float stamina = flag ? this.m_blockStaminaDrain : (this.m_blockStaminaDrain * num3);
		this.UseStamina(stamina);
		float totalStaggerDamage = damageTypes.GetTotalStaggerDamage();
		bool flag2 = base.AddStaggerDamage(totalStaggerDamage, hit.m_dir);
		bool flag3 = this.HaveStamina(0f);
		bool flag4 = flag3 && !flag2;
		if (flag3 && !flag2)
		{
			hit.m_statusEffectHash = 0;
			hit.BlockDamage(num2);
			DamageText.instance.ShowText(DamageText.TextType.Blocked, hit.m_point + Vector3.up * 0.5f, num2, false);
		}
		if (currentBlocker.m_shared.m_useDurability)
		{
			float num4 = currentBlocker.m_shared.m_useDurabilityDrain * (totalBlockableDamage / num);
			currentBlocker.m_durability -= num4;
		}
		this.RaiseSkill(Skills.SkillType.Blocking, flag ? 2f : 1f);
		currentBlocker.m_shared.m_blockEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
		if (attacker && flag && flag4)
		{
			this.m_perfectBlockEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
			if (attacker.m_staggerWhenBlocked)
			{
				attacker.Stagger(-hit.m_dir);
			}
			this.UseStamina(this.m_blockStaminaDrain);
		}
		if (flag4)
		{
			hit.m_pushForce *= num3;
			if (attacker && !hit.m_ranged)
			{
				float num5 = 1f - Mathf.Clamp01(num3 * 0.5f);
				HitData hitData = new HitData();
				hitData.m_pushForce = currentBlocker.GetDeflectionForce() * num5;
				hitData.m_dir = attacker.transform.position - base.transform.position;
				hitData.m_dir.y = 0f;
				hitData.m_dir.Normalize();
				attacker.Damage(hitData);
			}
		}
		return true;
	}

	// Token: 0x0600019F RID: 415 RVA: 0x0000BB80 File Offset: 0x00009D80
	public override bool IsBlocking()
	{
		if (this.m_nview.IsValid() && !this.m_nview.IsOwner())
		{
			return this.m_nview.GetZDO().GetBool(ZDOVars.s_isBlockingHash, false);
		}
		return this.m_blocking && !this.InAttack() && !this.InDodge() && !this.InPlaceMode() && !this.IsEncumbered() && !this.InMinorAction() && !base.IsStaggering();
	}

	// Token: 0x060001A0 RID: 416 RVA: 0x0000BBFC File Offset: 0x00009DFC
	private void UpdateBlock(float dt)
	{
		if (!this.IsBlocking())
		{
			if (this.m_internalBlockingState)
			{
				this.m_internalBlockingState = false;
				this.m_nview.GetZDO().Set(ZDOVars.s_isBlockingHash, false);
				this.m_zanim.SetBool(Humanoid.s_blocking, false);
			}
			this.m_blockTimer = -1f;
			return;
		}
		if (!this.m_internalBlockingState)
		{
			this.m_internalBlockingState = true;
			this.m_nview.GetZDO().Set(ZDOVars.s_isBlockingHash, true);
			this.m_zanim.SetBool(Humanoid.s_blocking, true);
		}
		if (this.m_blockTimer < 0f)
		{
			this.m_blockTimer = 0f;
			return;
		}
		this.m_blockTimer += dt;
	}

	// Token: 0x060001A1 RID: 417 RVA: 0x0000BCB0 File Offset: 0x00009EB0
	protected void HideHandItems()
	{
		if (this.m_leftItem == null && this.m_rightItem == null)
		{
			return;
		}
		ItemDrop.ItemData leftItem = this.m_leftItem;
		ItemDrop.ItemData rightItem = this.m_rightItem;
		this.UnequipItem(this.m_leftItem, true);
		this.UnequipItem(this.m_rightItem, true);
		this.m_hiddenLeftItem = leftItem;
		this.m_hiddenRightItem = rightItem;
		this.SetupVisEquipment(this.m_visEquipment, false);
		this.m_zanim.SetTrigger("equip_hip");
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x0000BD24 File Offset: 0x00009F24
	protected void ShowHandItems()
	{
		ItemDrop.ItemData hiddenLeftItem = this.m_hiddenLeftItem;
		ItemDrop.ItemData hiddenRightItem = this.m_hiddenRightItem;
		if (hiddenLeftItem == null && hiddenRightItem == null)
		{
			return;
		}
		this.m_hiddenLeftItem = null;
		this.m_hiddenRightItem = null;
		if (hiddenLeftItem != null)
		{
			this.EquipItem(hiddenLeftItem, true);
		}
		if (hiddenRightItem != null)
		{
			this.EquipItem(hiddenRightItem, true);
		}
		this.m_zanim.SetTrigger("equip_hip");
	}

	// Token: 0x060001A3 RID: 419 RVA: 0x0000BD7C File Offset: 0x00009F7C
	public ItemDrop.ItemData GetAmmoItem()
	{
		return this.m_ammoItem;
	}

	// Token: 0x060001A4 RID: 420 RVA: 0x00006475 File Offset: 0x00004675
	public virtual GameObject GetHoverObject()
	{
		return null;
	}

	// Token: 0x060001A5 RID: 421 RVA: 0x0000BD84 File Offset: 0x00009F84
	public bool IsTeleportable()
	{
		return this.m_inventory.IsTeleportable();
	}

	// Token: 0x060001A6 RID: 422 RVA: 0x0000BD94 File Offset: 0x00009F94
	public override bool UseMeleeCamera()
	{
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		return currentWeapon != null && currentWeapon.m_shared.m_centerCamera;
	}

	// Token: 0x060001A7 RID: 423 RVA: 0x0000BDB8 File Offset: 0x00009FB8
	public float GetEquipmentWeight()
	{
		float num = 0f;
		if (this.m_rightItem != null)
		{
			num += this.m_rightItem.m_shared.m_weight;
		}
		if (this.m_leftItem != null)
		{
			num += this.m_leftItem.m_shared.m_weight;
		}
		if (this.m_chestItem != null)
		{
			num += this.m_chestItem.m_shared.m_weight;
		}
		if (this.m_legItem != null)
		{
			num += this.m_legItem.m_shared.m_weight;
		}
		if (this.m_helmetItem != null)
		{
			num += this.m_helmetItem.m_shared.m_weight;
		}
		if (this.m_shoulderItem != null)
		{
			num += this.m_shoulderItem.m_shared.m_weight;
		}
		if (this.m_utilityItem != null)
		{
			num += this.m_utilityItem.m_shared.m_weight;
		}
		return num;
	}

	// Token: 0x17000005 RID: 5
	// (get) Token: 0x060001A8 RID: 424 RVA: 0x0000BE89 File Offset: 0x0000A089
	public new static List<Humanoid> Instances { get; } = new List<Humanoid>();

	// Token: 0x04000177 RID: 375
	private static List<ItemDrop.ItemData> optimalWeapons = new List<ItemDrop.ItemData>();

	// Token: 0x04000178 RID: 376
	private static List<ItemDrop.ItemData> outofRangeWeapons = new List<ItemDrop.ItemData>();

	// Token: 0x04000179 RID: 377
	private static List<ItemDrop.ItemData> allWeapons = new List<ItemDrop.ItemData>();

	// Token: 0x0400017A RID: 378
	[Header("Humanoid")]
	public float m_equipStaminaDrain = 10f;

	// Token: 0x0400017B RID: 379
	public float m_blockStaminaDrain = 25f;

	// Token: 0x0400017C RID: 380
	[Header("Default items")]
	public GameObject[] m_defaultItems;

	// Token: 0x0400017D RID: 381
	public GameObject[] m_randomWeapon;

	// Token: 0x0400017E RID: 382
	public GameObject[] m_randomArmor;

	// Token: 0x0400017F RID: 383
	public GameObject[] m_randomShield;

	// Token: 0x04000180 RID: 384
	public Humanoid.ItemSet[] m_randomSets;

	// Token: 0x04000181 RID: 385
	public ItemDrop m_unarmedWeapon;

	// Token: 0x04000182 RID: 386
	[Header("Effects")]
	public EffectList m_pickupEffects = new EffectList();

	// Token: 0x04000183 RID: 387
	public EffectList m_dropEffects = new EffectList();

	// Token: 0x04000184 RID: 388
	public EffectList m_consumeItemEffects = new EffectList();

	// Token: 0x04000185 RID: 389
	public EffectList m_equipEffects = new EffectList();

	// Token: 0x04000186 RID: 390
	public EffectList m_perfectBlockEffect = new EffectList();

	// Token: 0x04000187 RID: 391
	protected readonly Inventory m_inventory = new Inventory("Inventory", null, 8, 4);

	// Token: 0x04000188 RID: 392
	protected ItemDrop.ItemData m_rightItem;

	// Token: 0x04000189 RID: 393
	protected ItemDrop.ItemData m_leftItem;

	// Token: 0x0400018A RID: 394
	protected ItemDrop.ItemData m_chestItem;

	// Token: 0x0400018B RID: 395
	protected ItemDrop.ItemData m_legItem;

	// Token: 0x0400018C RID: 396
	protected ItemDrop.ItemData m_ammoItem;

	// Token: 0x0400018D RID: 397
	protected ItemDrop.ItemData m_helmetItem;

	// Token: 0x0400018E RID: 398
	protected ItemDrop.ItemData m_shoulderItem;

	// Token: 0x0400018F RID: 399
	protected ItemDrop.ItemData m_utilityItem;

	// Token: 0x04000190 RID: 400
	protected string m_beardItem = "";

	// Token: 0x04000191 RID: 401
	protected string m_hairItem = "";

	// Token: 0x04000192 RID: 402
	protected Attack m_currentAttack;

	// Token: 0x04000193 RID: 403
	protected bool m_currentAttackIsSecondary;

	// Token: 0x04000194 RID: 404
	protected float m_attackDrawTime;

	// Token: 0x04000195 RID: 405
	protected float m_lastCombatTimer = 999f;

	// Token: 0x04000196 RID: 406
	protected VisEquipment m_visEquipment;

	// Token: 0x04000197 RID: 407
	private Attack m_previousAttack;

	// Token: 0x04000198 RID: 408
	private ItemDrop.ItemData m_hiddenLeftItem;

	// Token: 0x04000199 RID: 409
	private ItemDrop.ItemData m_hiddenRightItem;

	// Token: 0x0400019A RID: 410
	private int m_lastEquipEffectFrame;

	// Token: 0x0400019B RID: 411
	private float m_timeSinceLastAttack;

	// Token: 0x0400019C RID: 412
	private bool m_internalBlockingState;

	// Token: 0x0400019D RID: 413
	private float m_blockTimer = 9999f;

	// Token: 0x0400019E RID: 414
	private const float m_perfectBlockInterval = 0.25f;

	// Token: 0x0400019F RID: 415
	private readonly HashSet<StatusEffect> m_equipmentStatusEffects = new HashSet<StatusEffect>();

	// Token: 0x040001A0 RID: 416
	private int m_seed;

	// Token: 0x040001A1 RID: 417
	private static readonly int s_statef = ZSyncAnimation.GetHash("statef");

	// Token: 0x040001A2 RID: 418
	private static readonly int s_statei = ZSyncAnimation.GetHash("statei");

	// Token: 0x040001A3 RID: 419
	private static readonly int s_blocking = ZSyncAnimation.GetHash("blocking");

	// Token: 0x040001A4 RID: 420
	protected static readonly int s_animatorTagAttack = ZSyncAnimation.GetHash("attack");

	// Token: 0x02000018 RID: 24
	[Serializable]
	public class ItemSet
	{
		// Token: 0x040001A6 RID: 422
		public string m_name = "";

		// Token: 0x040001A7 RID: 423
		public GameObject[] m_items = Array.Empty<GameObject>();
	}
}
