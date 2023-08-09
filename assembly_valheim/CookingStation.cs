using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000223 RID: 547
public class CookingStation : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x060015A7 RID: 5543 RVA: 0x0008E34C File Offset: 0x0008C54C
	private void Awake()
	{
		this.m_nview = base.gameObject.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_ps = new ParticleSystem[this.m_slots.Length];
		this.m_as = new AudioSource[this.m_slots.Length];
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			this.m_ps[i] = this.m_slots[i].GetComponent<ParticleSystem>();
			this.m_as[i] = this.m_slots[i].GetComponent<AudioSource>();
		}
		this.m_nview.Register<Vector3>("RemoveDoneItem", new Action<long, Vector3>(this.RPC_RemoveDoneItem));
		this.m_nview.Register<string>("AddItem", new Action<long, string>(this.RPC_AddItem));
		this.m_nview.Register("AddFuel", new Action<long>(this.RPC_AddFuel));
		this.m_nview.Register<int, string>("SetSlotVisual", new Action<long, int, string>(this.RPC_SetSlotVisual));
		if (this.m_addFoodSwitch)
		{
			this.m_addFoodSwitch.m_onUse = new Switch.Callback(this.OnAddFoodSwitch);
			this.m_addFoodSwitch.m_hoverText = this.HoverText();
		}
		if (this.m_addFuelSwitch)
		{
			this.m_addFuelSwitch.m_onUse = new Switch.Callback(this.OnAddFuelSwitch);
			this.m_addFuelSwitch.m_onHover = new Switch.TooltipCallback(this.OnHoverFuelSwitch);
		}
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		base.InvokeRepeating("UpdateCooking", 0f, 1f);
	}

	// Token: 0x060015A8 RID: 5544 RVA: 0x0008E504 File Offset: 0x0008C704
	private void DropAllItems()
	{
		if (this.m_fuelItem != null)
		{
			float fuel = this.GetFuel();
			for (int i = 0; i < (int)fuel; i++)
			{
				this.<DropAllItems>g__drop|1_0(this.m_fuelItem);
			}
			this.SetFuel(0f);
		}
		for (int j = 0; j < this.m_slots.Length; j++)
		{
			string text;
			float num;
			CookingStation.Status status;
			this.GetSlot(j, out text, out num, out status);
			if (text != "")
			{
				if (status == CookingStation.Status.Done)
				{
					this.<DropAllItems>g__drop|1_0(this.GetItemConversion(text).m_to);
				}
				else if (status == CookingStation.Status.Burnt)
				{
					this.<DropAllItems>g__drop|1_0(this.m_overCookedItem);
				}
				else if (status == CookingStation.Status.NotDone)
				{
					GameObject prefab = ZNetScene.instance.GetPrefab(text);
					if (prefab != null)
					{
						ItemDrop component = prefab.GetComponent<ItemDrop>();
						if (component)
						{
							this.<DropAllItems>g__drop|1_0(component);
						}
					}
				}
				this.SetSlot(j, "", 0f, CookingStation.Status.NotDone);
			}
		}
	}

	// Token: 0x060015A9 RID: 5545 RVA: 0x0008E5F0 File Offset: 0x0008C7F0
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			this.DropAllItems();
		}
	}

	// Token: 0x060015AA RID: 5546 RVA: 0x0008E608 File Offset: 0x0008C808
	private void UpdateCooking()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		bool flag = (this.m_requireFire && this.IsFireLit()) || (this.m_useFuel && this.GetFuel() > 0f);
		if (this.m_nview.IsOwner())
		{
			float deltaTime = this.GetDeltaTime();
			if (flag)
			{
				this.UpdateFuel(deltaTime);
				for (int i = 0; i < this.m_slots.Length; i++)
				{
					string text;
					float num;
					CookingStation.Status status;
					this.GetSlot(i, out text, out num, out status);
					if (text != "" && status != CookingStation.Status.Burnt)
					{
						CookingStation.ItemConversion itemConversion = this.GetItemConversion(text);
						if (itemConversion == null)
						{
							this.SetSlot(i, "", 0f, CookingStation.Status.NotDone);
						}
						else
						{
							num += deltaTime;
							if (num > itemConversion.m_cookTime * 2f)
							{
								this.m_overcookedEffect.Create(this.m_slots[i].position, Quaternion.identity, null, 1f, -1);
								this.SetSlot(i, this.m_overCookedItem.name, num, CookingStation.Status.Burnt);
							}
							else if (num > itemConversion.m_cookTime && text == itemConversion.m_from.name)
							{
								this.m_doneEffect.Create(this.m_slots[i].position, Quaternion.identity, null, 1f, -1);
								this.SetSlot(i, itemConversion.m_to.name, num, CookingStation.Status.Done);
							}
							else
							{
								this.SetSlot(i, text, num, status);
							}
						}
					}
				}
			}
		}
		this.UpdateVisual(flag);
	}

	// Token: 0x060015AB RID: 5547 RVA: 0x0008E798 File Offset: 0x0008C998
	private float GetDeltaTime()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_startTime, time.Ticks));
		float totalSeconds = (float)(time - d).TotalSeconds;
		this.m_nview.GetZDO().Set(ZDOVars.s_startTime, time.Ticks);
		return totalSeconds;
	}

	// Token: 0x060015AC RID: 5548 RVA: 0x0008E800 File Offset: 0x0008CA00
	private void UpdateFuel(float dt)
	{
		if (!this.m_useFuel)
		{
			return;
		}
		float num = dt / (float)this.m_secPerFuel;
		float num2 = this.GetFuel();
		num2 -= num;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		this.SetFuel(num2);
	}

	// Token: 0x060015AD RID: 5549 RVA: 0x0008E840 File Offset: 0x0008CA40
	private void UpdateVisual(bool fireLit)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			string item;
			float num;
			CookingStation.Status status;
			this.GetSlot(i, out item, out num, out status);
			this.SetSlotVisual(i, item, fireLit, status);
		}
		if (this.m_useFuel)
		{
			bool active = this.GetFuel() > 0f;
			if (this.m_haveFireObject)
			{
				this.m_haveFireObject.SetActive(fireLit);
			}
			if (this.m_haveFuelObject)
			{
				this.m_haveFuelObject.SetActive(active);
			}
		}
	}

	// Token: 0x060015AE RID: 5550 RVA: 0x0008E8C1 File Offset: 0x0008CAC1
	private void RPC_SetSlotVisual(long sender, int slot, string item)
	{
		this.SetSlotVisual(slot, item, false, CookingStation.Status.NotDone);
	}

	// Token: 0x060015AF RID: 5551 RVA: 0x0008E8D0 File Offset: 0x0008CAD0
	private void SetSlotVisual(int i, string item, bool fireLit, CookingStation.Status status)
	{
		if (item == "")
		{
			this.m_ps[i].emission.enabled = false;
			if (this.m_burntPS.Length != 0)
			{
				this.m_burntPS[i].emission.enabled = false;
			}
			if (this.m_donePS.Length != 0)
			{
				this.m_donePS[i].emission.enabled = false;
			}
			this.m_as[i].mute = true;
			if (this.m_slots[i].childCount > 0)
			{
				UnityEngine.Object.Destroy(this.m_slots[i].GetChild(0).gameObject);
				return;
			}
		}
		else
		{
			this.m_ps[i].emission.enabled = (fireLit && status != CookingStation.Status.Burnt);
			if (this.m_burntPS.Length != 0)
			{
				this.m_burntPS[i].emission.enabled = (fireLit && status == CookingStation.Status.Burnt);
			}
			if (this.m_donePS.Length != 0)
			{
				this.m_donePS[i].emission.enabled = (fireLit && status == CookingStation.Status.Done);
			}
			this.m_as[i].mute = !fireLit;
			if (this.m_slots[i].childCount == 0 || this.m_slots[i].GetChild(0).name != item)
			{
				if (this.m_slots[i].childCount > 0)
				{
					UnityEngine.Object.Destroy(this.m_slots[i].GetChild(0).gameObject);
				}
				Component component = ObjectDB.instance.GetItemPrefab(item).transform.Find("attach");
				Transform transform = this.m_slots[i];
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(component.gameObject, transform.position, transform.rotation, transform);
				gameObject.name = item;
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].shadowCastingMode = ShadowCastingMode.Off;
				}
			}
		}
	}

	// Token: 0x060015B0 RID: 5552 RVA: 0x0008EAC4 File Offset: 0x0008CCC4
	private void RPC_RemoveDoneItem(long sender, Vector3 userPoint)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			string text;
			float num;
			CookingStation.Status status;
			this.GetSlot(i, out text, out num, out status);
			if (text != "" && this.IsItemDone(text))
			{
				this.SpawnItem(text, i, userPoint);
				this.SetSlot(i, "", 0f, CookingStation.Status.NotDone);
				this.m_nview.InvokeRPC(ZNetView.Everybody, "SetSlotVisual", new object[]
				{
					i,
					""
				});
				return;
			}
		}
	}

	// Token: 0x060015B1 RID: 5553 RVA: 0x0008EB50 File Offset: 0x0008CD50
	private bool HaveDoneItem()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			string text;
			float num;
			CookingStation.Status status;
			this.GetSlot(i, out text, out num, out status);
			if (text != "" && this.IsItemDone(text))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060015B2 RID: 5554 RVA: 0x0008EB98 File Offset: 0x0008CD98
	private bool IsItemDone(string itemName)
	{
		if (itemName == this.m_overCookedItem.name)
		{
			return true;
		}
		CookingStation.ItemConversion itemConversion = this.GetItemConversion(itemName);
		return itemConversion != null && itemName == itemConversion.m_to.name;
	}

	// Token: 0x060015B3 RID: 5555 RVA: 0x0008EBE0 File Offset: 0x0008CDE0
	private void SpawnItem(string name, int slot, Vector3 userPoint)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		Vector3 vector;
		Vector3 a;
		if (this.m_spawnPoint != null)
		{
			vector = this.m_spawnPoint.position;
			a = this.m_spawnPoint.forward;
		}
		else
		{
			Vector3 position = this.m_slots[slot].position;
			Vector3 vector2 = userPoint - position;
			vector2.y = 0f;
			vector2.Normalize();
			vector = position + vector2 * 0.5f;
			a = vector2;
		}
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		UnityEngine.Object.Instantiate<GameObject>(itemPrefab, vector, rotation).GetComponent<Rigidbody>().velocity = a * this.m_spawnForce;
		this.m_pickEffector.Create(vector, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x060015B4 RID: 5556 RVA: 0x0008ECB2 File Offset: 0x0008CEB2
	public string GetHoverText()
	{
		if (this.m_addFoodSwitch != null)
		{
			return "";
		}
		return Localization.instance.Localize(this.HoverText());
	}

	// Token: 0x060015B5 RID: 5557 RVA: 0x0008ECD8 File Offset: 0x0008CED8
	private string HoverText()
	{
		return string.Concat(new string[]
		{
			this.m_name,
			"\n[<color=yellow><b>$KEY_Use</b></color>] ",
			this.m_addItemTooltip,
			"\n[<color=yellow><b>1-8</b></color>] ",
			this.m_addItemTooltip
		});
	}

	// Token: 0x060015B6 RID: 5558 RVA: 0x0008ED10 File Offset: 0x0008CF10
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060015B7 RID: 5559 RVA: 0x0008ED18 File Offset: 0x0008CF18
	private bool OnAddFuelSwitch(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (item != null && item.m_shared.m_name != this.m_fuelItem.m_itemData.m_shared.m_name)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wrongitem", 0, null);
			return false;
		}
		if (this.GetFuel() > (float)(this.m_maxFuel - 1))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull", 0, null);
			return false;
		}
		if (!user.GetInventory().HaveItem(this.m_fuelItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_donthaveany " + this.m_fuelItem.m_itemData.m_shared.m_name, 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + this.m_fuelItem.m_itemData.m_shared.m_name, 0, null);
		user.GetInventory().RemoveItem(this.m_fuelItem.m_itemData.m_shared.m_name, 1, -1);
		this.m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
		return true;
	}

	// Token: 0x060015B8 RID: 5560 RVA: 0x0008EE2C File Offset: 0x0008D02C
	private void RPC_AddFuel(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		ZLog.Log("Add fuel");
		float fuel = this.GetFuel();
		this.SetFuel(fuel + 1f);
		this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
	}

	// Token: 0x060015B9 RID: 5561 RVA: 0x0008EE94 File Offset: 0x0008D094
	private string OnHoverFuelSwitch()
	{
		float fuel = this.GetFuel();
		return Localization.instance.Localize(string.Format("{0} ({1} {2}/{3})\n[<color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add {4}", new object[]
		{
			this.m_name,
			this.m_fuelItem.m_itemData.m_shared.m_name,
			Mathf.Ceil(fuel),
			this.m_maxFuel,
			this.m_fuelItem.m_itemData.m_shared.m_name
		}));
	}

	// Token: 0x060015BA RID: 5562 RVA: 0x0008EF17 File Offset: 0x0008D117
	private bool OnAddFoodSwitch(Switch caller, Humanoid user, ItemDrop.ItemData item)
	{
		ZLog.Log("add food switch");
		if (item != null)
		{
			return this.OnUseItem(user, item);
		}
		return this.OnInteract(user);
	}

	// Token: 0x060015BB RID: 5563 RVA: 0x0008EF36 File Offset: 0x0008D136
	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		return !hold && !(this.m_addFoodSwitch != null) && this.OnInteract(user);
	}

	// Token: 0x060015BC RID: 5564 RVA: 0x0008EF54 File Offset: 0x0008D154
	private bool OnInteract(Humanoid user)
	{
		if (this.HaveDoneItem())
		{
			this.m_nview.InvokeRPC("RemoveDoneItem", new object[]
			{
				user.transform.position
			});
			return true;
		}
		ItemDrop.ItemData itemData = this.FindCookableItem(user.GetInventory());
		if (itemData == null)
		{
			CookingStation.ItemMessage itemMessage = this.FindIncompatibleItem(user.GetInventory());
			if (itemMessage != null)
			{
				user.Message(MessageHud.MessageType.Center, itemMessage.m_message + " " + itemMessage.m_item.m_itemData.m_shared.m_name, 0, null);
			}
			else
			{
				user.Message(MessageHud.MessageType.Center, "$msg_nocookitems", 0, null);
			}
			return false;
		}
		return this.OnUseItem(user, itemData);
	}

	// Token: 0x060015BD RID: 5565 RVA: 0x0008EFFC File Offset: 0x0008D1FC
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return !(this.m_addFoodSwitch != null) && this.OnUseItem(user, item);
	}

	// Token: 0x060015BE RID: 5566 RVA: 0x0008F018 File Offset: 0x0008D218
	private bool OnUseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.m_requireFire && !this.IsFireLit())
		{
			user.Message(MessageHud.MessageType.Center, "$msg_needfire", 0, null);
			return false;
		}
		if (this.GetFreeSlot() == -1)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_nocookroom", 0, null);
			return false;
		}
		return this.CookItem(user, item);
	}

	// Token: 0x060015BF RID: 5567 RVA: 0x0008F068 File Offset: 0x0008D268
	private bool IsFireLit()
	{
		if (this.m_fireCheckPoints != null && this.m_fireCheckPoints.Length != 0)
		{
			Transform[] fireCheckPoints = this.m_fireCheckPoints;
			for (int i = 0; i < fireCheckPoints.Length; i++)
			{
				if (!EffectArea.IsPointInsideArea(fireCheckPoints[i].position, EffectArea.Type.Burning, this.m_fireCheckRadius))
				{
					return false;
				}
			}
			return true;
		}
		return EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Burning, this.m_fireCheckRadius);
	}

	// Token: 0x060015C0 RID: 5568 RVA: 0x0008F0D8 File Offset: 0x0008D2D8
	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (CookingStation.ItemConversion itemConversion in this.m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name, -1, false);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	// Token: 0x060015C1 RID: 5569 RVA: 0x0008F14C File Offset: 0x0008D34C
	private CookingStation.ItemMessage FindIncompatibleItem(Inventory inventory)
	{
		foreach (CookingStation.ItemMessage itemMessage in this.m_incompatibleItems)
		{
			if (inventory.GetItem(itemMessage.m_item.m_itemData.m_shared.m_name, -1, false) != null)
			{
				return itemMessage;
			}
		}
		return null;
	}

	// Token: 0x060015C2 RID: 5570 RVA: 0x0008F1C0 File Offset: 0x0008D3C0
	private bool CookItem(Humanoid user, ItemDrop.ItemData item)
	{
		string name = item.m_dropPrefab.name;
		if (!this.m_nview.HasOwner())
		{
			this.m_nview.ClaimOwnership();
		}
		foreach (CookingStation.ItemMessage itemMessage in this.m_incompatibleItems)
		{
			if (itemMessage.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name)
			{
				user.Message(MessageHud.MessageType.Center, itemMessage.m_message + " " + itemMessage.m_item.m_itemData.m_shared.m_name, 0, null);
				return true;
			}
		}
		if (!this.IsItemAllowed(item))
		{
			return false;
		}
		if (this.GetFreeSlot() == -1)
		{
			return false;
		}
		user.GetInventory().RemoveOneItem(item);
		this.m_nview.InvokeRPC("AddItem", new object[]
		{
			name
		});
		return true;
	}

	// Token: 0x060015C3 RID: 5571 RVA: 0x0008F2CC File Offset: 0x0008D4CC
	private void RPC_AddItem(long sender, string itemName)
	{
		if (!this.IsItemAllowed(itemName))
		{
			return;
		}
		int freeSlot = this.GetFreeSlot();
		if (freeSlot == -1)
		{
			return;
		}
		this.SetSlot(freeSlot, itemName, 0f, CookingStation.Status.NotDone);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetSlotVisual", new object[]
		{
			freeSlot,
			itemName
		});
		this.m_addEffect.Create(this.m_slots[freeSlot].position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x060015C4 RID: 5572 RVA: 0x0008F34C File Offset: 0x0008D54C
	private void SetSlot(int slot, string itemName, float cookedTime, CookingStation.Status status)
	{
		this.m_nview.GetZDO().Set("slot" + slot.ToString(), itemName);
		this.m_nview.GetZDO().Set("slot" + slot.ToString(), cookedTime);
		this.m_nview.GetZDO().Set("slotstatus" + slot.ToString(), (int)status);
	}

	// Token: 0x060015C5 RID: 5573 RVA: 0x0008F3C0 File Offset: 0x0008D5C0
	private void GetSlot(int slot, out string itemName, out float cookedTime, out CookingStation.Status status)
	{
		itemName = this.m_nview.GetZDO().GetString("slot" + slot.ToString(), "");
		cookedTime = this.m_nview.GetZDO().GetFloat("slot" + slot.ToString(), 0f);
		status = (CookingStation.Status)this.m_nview.GetZDO().GetInt("slotstatus" + slot.ToString(), 0);
	}

	// Token: 0x060015C6 RID: 5574 RVA: 0x0008F444 File Offset: 0x0008D644
	private bool IsEmpty()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if (this.m_nview.GetZDO().GetString("slot" + i.ToString(), "") != "")
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060015C7 RID: 5575 RVA: 0x0008F49C File Offset: 0x0008D69C
	private int GetFreeSlot()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if (this.m_nview.GetZDO().GetString("slot" + i.ToString(), "") == "")
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060015C8 RID: 5576 RVA: 0x0008F4F1 File Offset: 0x0008D6F1
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x060015C9 RID: 5577 RVA: 0x0008F504 File Offset: 0x0008D704
	private bool IsItemAllowed(string itemName)
	{
		using (List<CookingStation.ItemConversion>.Enumerator enumerator = this.m_conversion.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_from.gameObject.name == itemName)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060015CA RID: 5578 RVA: 0x0008F570 File Offset: 0x0008D770
	private CookingStation.ItemConversion GetItemConversion(string itemName)
	{
		foreach (CookingStation.ItemConversion itemConversion in this.m_conversion)
		{
			if (itemConversion.m_from.gameObject.name == itemName || itemConversion.m_to.gameObject.name == itemName)
			{
				return itemConversion;
			}
		}
		return null;
	}

	// Token: 0x060015CB RID: 5579 RVA: 0x0008F5F4 File Offset: 0x0008D7F4
	private void SetFuel(float fuel)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_fuel, fuel);
	}

	// Token: 0x060015CC RID: 5580 RVA: 0x0008F60C File Offset: 0x0008D80C
	private float GetFuel()
	{
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
	}

	// Token: 0x060015CD RID: 5581 RVA: 0x0008F628 File Offset: 0x0008D828
	private void OnDrawGizmosSelected()
	{
		if (this.m_requireFire)
		{
			if (this.m_fireCheckPoints != null && this.m_fireCheckPoints.Length != 0)
			{
				foreach (Transform transform in this.m_fireCheckPoints)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere(transform.position, this.m_fireCheckRadius);
				}
				return;
			}
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, this.m_fireCheckRadius);
		}
	}

	// Token: 0x060015CF RID: 5583 RVA: 0x0008F74C File Offset: 0x0008D94C
	[CompilerGenerated]
	private void <DropAllItems>g__drop|1_0(ItemDrop item)
	{
		Vector3 position = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		UnityEngine.Object.Instantiate<GameObject>(item.gameObject, position, rotation);
	}

	// Token: 0x0400169B RID: 5787
	public Switch m_addFoodSwitch;

	// Token: 0x0400169C RID: 5788
	public Switch m_addFuelSwitch;

	// Token: 0x0400169D RID: 5789
	public EffectList m_addEffect = new EffectList();

	// Token: 0x0400169E RID: 5790
	public EffectList m_doneEffect = new EffectList();

	// Token: 0x0400169F RID: 5791
	public EffectList m_overcookedEffect = new EffectList();

	// Token: 0x040016A0 RID: 5792
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x040016A1 RID: 5793
	public string m_addItemTooltip = "$piece_cstand_cook";

	// Token: 0x040016A2 RID: 5794
	public Transform m_spawnPoint;

	// Token: 0x040016A3 RID: 5795
	public float m_spawnForce = 5f;

	// Token: 0x040016A4 RID: 5796
	public ItemDrop m_overCookedItem;

	// Token: 0x040016A5 RID: 5797
	public List<CookingStation.ItemConversion> m_conversion = new List<CookingStation.ItemConversion>();

	// Token: 0x040016A6 RID: 5798
	public List<CookingStation.ItemMessage> m_incompatibleItems = new List<CookingStation.ItemMessage>();

	// Token: 0x040016A7 RID: 5799
	public Transform[] m_slots;

	// Token: 0x040016A8 RID: 5800
	public ParticleSystem[] m_donePS;

	// Token: 0x040016A9 RID: 5801
	public ParticleSystem[] m_burntPS;

	// Token: 0x040016AA RID: 5802
	public string m_name = "";

	// Token: 0x040016AB RID: 5803
	public bool m_requireFire = true;

	// Token: 0x040016AC RID: 5804
	public Transform[] m_fireCheckPoints;

	// Token: 0x040016AD RID: 5805
	public float m_fireCheckRadius = 0.25f;

	// Token: 0x040016AE RID: 5806
	public bool m_useFuel;

	// Token: 0x040016AF RID: 5807
	public ItemDrop m_fuelItem;

	// Token: 0x040016B0 RID: 5808
	public int m_maxFuel = 10;

	// Token: 0x040016B1 RID: 5809
	public int m_secPerFuel = 5000;

	// Token: 0x040016B2 RID: 5810
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x040016B3 RID: 5811
	public GameObject m_haveFuelObject;

	// Token: 0x040016B4 RID: 5812
	public GameObject m_haveFireObject;

	// Token: 0x040016B5 RID: 5813
	private ZNetView m_nview;

	// Token: 0x040016B6 RID: 5814
	private ParticleSystem[] m_ps;

	// Token: 0x040016B7 RID: 5815
	private AudioSource[] m_as;

	// Token: 0x02000224 RID: 548
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x040016B8 RID: 5816
		public ItemDrop m_from;

		// Token: 0x040016B9 RID: 5817
		public ItemDrop m_to;

		// Token: 0x040016BA RID: 5818
		public float m_cookTime = 10f;
	}

	// Token: 0x02000225 RID: 549
	[Serializable]
	public class ItemMessage
	{
		// Token: 0x040016BB RID: 5819
		public ItemDrop m_item;

		// Token: 0x040016BC RID: 5820
		public string m_message;
	}

	// Token: 0x02000226 RID: 550
	private enum Status
	{
		// Token: 0x040016BE RID: 5822
		NotDone,
		// Token: 0x040016BF RID: 5823
		Done,
		// Token: 0x040016C0 RID: 5824
		Burnt
	}
}
