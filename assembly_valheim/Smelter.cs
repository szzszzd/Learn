using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000294 RID: 660
public class Smelter : MonoBehaviour
{
	// Token: 0x0600193C RID: 6460 RVA: 0x000A7C7C File Offset: 0x000A5E7C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.m_addOreSwitch)
		{
			Switch addOreSwitch = this.m_addOreSwitch;
			addOreSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addOreSwitch.m_onUse, new Switch.Callback(this.OnAddOre));
			this.m_addOreSwitch.m_onHover = new Switch.TooltipCallback(this.OnHoverAddOre);
		}
		if (this.m_addWoodSwitch)
		{
			Switch addWoodSwitch = this.m_addWoodSwitch;
			addWoodSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addWoodSwitch.m_onUse, new Switch.Callback(this.OnAddFuel));
			this.m_addWoodSwitch.m_onHover = new Switch.TooltipCallback(this.OnHoverAddFuel);
		}
		if (this.m_emptyOreSwitch)
		{
			Switch emptyOreSwitch = this.m_emptyOreSwitch;
			emptyOreSwitch.m_onUse = (Switch.Callback)Delegate.Combine(emptyOreSwitch.m_onUse, new Switch.Callback(this.OnEmpty));
			Switch emptyOreSwitch2 = this.m_emptyOreSwitch;
			emptyOreSwitch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(emptyOreSwitch2.m_onHover, new Switch.TooltipCallback(this.OnHoverEmptyOre));
		}
		this.m_nview.Register<string>("AddOre", new Action<long, string>(this.RPC_AddOre));
		this.m_nview.Register("AddFuel", new Action<long>(this.RPC_AddFuel));
		this.m_nview.Register("EmptyProcessed", new Action<long>(this.RPC_EmptyProcessed));
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		base.InvokeRepeating("UpdateSmelter", 1f, 1f);
	}

	// Token: 0x0600193D RID: 6461 RVA: 0x000A7E3C File Offset: 0x000A603C
	private void DropAllItems()
	{
		this.SpawnProcessed();
		if (this.m_fuelItem != null)
		{
			float @float = this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
			for (int i = 0; i < (int)@float; i++)
			{
				Vector3 position = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
				Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				UnityEngine.Object.Instantiate<GameObject>(this.m_fuelItem.gameObject, position, rotation);
			}
		}
		while (this.GetQueueSize() > 0)
		{
			string queuedOre = this.GetQueuedOre();
			this.RemoveOneOre();
			Smelter.ItemConversion itemConversion = this.GetItemConversion(queuedOre);
			if (itemConversion != null)
			{
				Vector3 position2 = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
				Quaternion rotation2 = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				UnityEngine.Object.Instantiate<GameObject>(itemConversion.m_from.gameObject, position2, rotation2);
			}
		}
	}

	// Token: 0x0600193E RID: 6462 RVA: 0x000A7F69 File Offset: 0x000A6169
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			this.DropAllItems();
		}
	}

	// Token: 0x0600193F RID: 6463 RVA: 0x000A7F7E File Offset: 0x000A617E
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x06001940 RID: 6464 RVA: 0x000A7F94 File Offset: 0x000A6194
	private bool IsItemAllowed(string itemName)
	{
		using (List<Smelter.ItemConversion>.Enumerator enumerator = this.m_conversion.GetEnumerator())
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

	// Token: 0x06001941 RID: 6465 RVA: 0x000A8000 File Offset: 0x000A6200
	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (Smelter.ItemConversion itemConversion in this.m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name, -1, false);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	// Token: 0x06001942 RID: 6466 RVA: 0x000A8074 File Offset: 0x000A6274
	private bool OnAddOre(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (item == null)
		{
			item = this.FindCookableItem(user.GetInventory());
			if (item == null)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems", 0, null);
				return false;
			}
		}
		if (!this.IsItemAllowed(item.m_dropPrefab.name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wontwork", 0, null);
			return false;
		}
		ZLog.Log("trying to add " + item.m_shared.m_name);
		if (this.GetQueueSize() >= this.m_maxOre)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull", 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name, 0, null);
		user.GetInventory().RemoveItem(item, 1);
		this.m_nview.InvokeRPC("AddOre", new object[]
		{
			item.m_dropPrefab.name
		});
		this.m_addedOreTime = Time.time;
		if (this.m_addOreAnimationDuration > 0f)
		{
			this.SetAnimation(true);
		}
		return true;
	}

	// Token: 0x06001943 RID: 6467 RVA: 0x000A8170 File Offset: 0x000A6370
	private float GetBakeTimer()
	{
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_bakeTimer, 0f);
	}

	// Token: 0x06001944 RID: 6468 RVA: 0x000A818C File Offset: 0x000A638C
	private void SetBakeTimer(float t)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_bakeTimer, t);
	}

	// Token: 0x06001945 RID: 6469 RVA: 0x000A81A4 File Offset: 0x000A63A4
	private float GetFuel()
	{
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
	}

	// Token: 0x06001946 RID: 6470 RVA: 0x000A81C0 File Offset: 0x000A63C0
	private void SetFuel(float fuel)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_fuel, fuel);
	}

	// Token: 0x06001947 RID: 6471 RVA: 0x000A81D8 File Offset: 0x000A63D8
	private int GetQueueSize()
	{
		return this.m_nview.GetZDO().GetInt(ZDOVars.s_queued, 0);
	}

	// Token: 0x06001948 RID: 6472 RVA: 0x000A81F0 File Offset: 0x000A63F0
	private void RPC_AddOre(long sender, string name)
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
		this.QueueOre(name);
		this.m_oreAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		ZLog.Log("Added ore " + name);
	}

	// Token: 0x06001949 RID: 6473 RVA: 0x000A8268 File Offset: 0x000A6468
	private void QueueOre(string name)
	{
		int queueSize = this.GetQueueSize();
		this.m_nview.GetZDO().Set("item" + queueSize.ToString(), name);
		this.m_nview.GetZDO().Set(ZDOVars.s_queued, queueSize + 1, false);
	}

	// Token: 0x0600194A RID: 6474 RVA: 0x000A82B7 File Offset: 0x000A64B7
	private string GetQueuedOre()
	{
		if (this.GetQueueSize() == 0)
		{
			return "";
		}
		return this.m_nview.GetZDO().GetString(ZDOVars.s_item0, "");
	}

	// Token: 0x0600194B RID: 6475 RVA: 0x000A82E4 File Offset: 0x000A64E4
	private void RemoveOneOre()
	{
		int queueSize = this.GetQueueSize();
		if (queueSize == 0)
		{
			return;
		}
		for (int i = 0; i < queueSize; i++)
		{
			string @string = this.m_nview.GetZDO().GetString("item" + (i + 1).ToString(), "");
			this.m_nview.GetZDO().Set("item" + i.ToString(), @string);
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_queued, queueSize - 1, false);
	}

	// Token: 0x0600194C RID: 6476 RVA: 0x000A836E File Offset: 0x000A656E
	private bool OnEmpty(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (this.GetProcessedQueueSize() <= 0)
		{
			return false;
		}
		this.m_nview.InvokeRPC("EmptyProcessed", Array.Empty<object>());
		return true;
	}

	// Token: 0x0600194D RID: 6477 RVA: 0x000A8391 File Offset: 0x000A6591
	private void RPC_EmptyProcessed(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.SpawnProcessed();
	}

	// Token: 0x0600194E RID: 6478 RVA: 0x000A83A8 File Offset: 0x000A65A8
	private bool OnAddFuel(Switch sw, Humanoid user, ItemDrop.ItemData item)
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

	// Token: 0x0600194F RID: 6479 RVA: 0x000A84BC File Offset: 0x000A66BC
	private void RPC_AddFuel(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float fuel = this.GetFuel();
		this.SetFuel(fuel + 1f);
		this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
	}

	// Token: 0x06001950 RID: 6480 RVA: 0x000A851C File Offset: 0x000A671C
	private double GetDeltaTime()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_startTime, time.Ticks));
		double totalSeconds = (time - d).TotalSeconds;
		this.m_nview.GetZDO().Set(ZDOVars.s_startTime, time.Ticks);
		return totalSeconds;
	}

	// Token: 0x06001951 RID: 6481 RVA: 0x000A8582 File Offset: 0x000A6782
	private float GetAccumulator()
	{
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_accTime, 0f);
	}

	// Token: 0x06001952 RID: 6482 RVA: 0x000A859E File Offset: 0x000A679E
	private void SetAccumulator(float t)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_accTime, t);
	}

	// Token: 0x06001953 RID: 6483 RVA: 0x000A85B6 File Offset: 0x000A67B6
	private void UpdateRoof()
	{
		if (this.m_requiresRoof)
		{
			this.m_haveRoof = Cover.IsUnderRoof(this.m_roofCheckPoint.position);
		}
	}

	// Token: 0x06001954 RID: 6484 RVA: 0x000A85D6 File Offset: 0x000A67D6
	private void UpdateSmoke()
	{
		if (this.m_smokeSpawner != null)
		{
			this.m_blockedSmoke = this.m_smokeSpawner.IsBlocked();
			return;
		}
		this.m_blockedSmoke = false;
	}

	// Token: 0x06001955 RID: 6485 RVA: 0x000A8600 File Offset: 0x000A6800
	private void UpdateSmelter()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateRoof();
		this.UpdateSmoke();
		this.UpdateState();
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		double deltaTime = this.GetDeltaTime();
		float num = this.GetAccumulator();
		num += (float)deltaTime;
		if (num > 3600f)
		{
			num = 3600f;
		}
		float num2 = this.m_windmill ? this.m_windmill.GetPowerOutput() : 1f;
		while (num >= 1f)
		{
			num -= 1f;
			float num3 = this.GetFuel();
			string queuedOre = this.GetQueuedOre();
			if ((this.m_maxFuel == 0 || num3 > 0f) && (this.m_maxOre == 0 || queuedOre != "") && this.m_secPerProduct > 0f && (!this.m_requiresRoof || this.m_haveRoof) && !this.m_blockedSmoke)
			{
				float num4 = 1f * num2;
				if (this.m_maxFuel > 0)
				{
					float num5 = this.m_secPerProduct / (float)this.m_fuelPerProduct;
					num3 -= num4 / num5;
					if (num3 < 0.0001f)
					{
						num3 = 0f;
					}
					this.SetFuel(num3);
				}
				if (queuedOre != "")
				{
					float num6 = this.GetBakeTimer();
					num6 += num4;
					this.SetBakeTimer(num6);
					if (num6 >= this.m_secPerProduct)
					{
						this.SetBakeTimer(0f);
						this.RemoveOneOre();
						this.QueueProcessed(queuedOre);
					}
				}
			}
		}
		if (this.GetQueuedOre() == "" || ((float)this.m_maxFuel > 0f && this.GetFuel() == 0f))
		{
			this.SpawnProcessed();
		}
		this.SetAccumulator(num);
	}

	// Token: 0x06001956 RID: 6486 RVA: 0x000A87C0 File Offset: 0x000A69C0
	private void QueueProcessed(string ore)
	{
		if (!this.m_spawnStack)
		{
			this.Spawn(ore, 1);
			return;
		}
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_spawnOre, "");
		int num = this.m_nview.GetZDO().GetInt(ZDOVars.s_spawnAmount, 0);
		if (@string.Length <= 0)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnOre, ore);
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 1, false);
			return;
		}
		if (@string != ore)
		{
			this.SpawnProcessed();
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnOre, ore);
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 1, false);
			return;
		}
		num++;
		Smelter.ItemConversion itemConversion = this.GetItemConversion(ore);
		if (itemConversion == null || num >= itemConversion.m_to.m_itemData.m_shared.m_maxStackSize)
		{
			this.Spawn(ore, num);
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnOre, "");
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 0, false);
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, num, false);
	}

	// Token: 0x06001957 RID: 6487 RVA: 0x000A8900 File Offset: 0x000A6B00
	private void SpawnProcessed()
	{
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_spawnAmount, 0);
		if (@int > 0)
		{
			string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_spawnOre, "");
			this.Spawn(@string, @int);
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnOre, "");
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 0, false);
		}
	}

	// Token: 0x06001958 RID: 6488 RVA: 0x000A897C File Offset: 0x000A6B7C
	private int GetProcessedQueueSize()
	{
		return this.m_nview.GetZDO().GetInt(ZDOVars.s_spawnAmount, 0);
	}

	// Token: 0x06001959 RID: 6489 RVA: 0x000A8994 File Offset: 0x000A6B94
	private void Spawn(string ore, int stack)
	{
		Smelter.ItemConversion itemConversion = this.GetItemConversion(ore);
		if (itemConversion != null)
		{
			this.m_produceEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			UnityEngine.Object.Instantiate<GameObject>(itemConversion.m_to.gameObject, this.m_outputPoint.position, this.m_outputPoint.rotation).GetComponent<ItemDrop>().m_itemData.m_stack = stack;
		}
	}

	// Token: 0x0600195A RID: 6490 RVA: 0x000A8A0C File Offset: 0x000A6C0C
	private Smelter.ItemConversion GetItemConversion(string itemName)
	{
		foreach (Smelter.ItemConversion itemConversion in this.m_conversion)
		{
			if (itemConversion.m_from.gameObject.name == itemName)
			{
				return itemConversion;
			}
		}
		return null;
	}

	// Token: 0x0600195B RID: 6491 RVA: 0x000A8A78 File Offset: 0x000A6C78
	private void UpdateState()
	{
		bool flag = this.IsActive();
		this.m_enabledObject.SetActive(flag);
		if (this.m_disabledObject)
		{
			this.m_disabledObject.SetActive(!flag);
		}
		if (this.m_haveFuelObject)
		{
			this.m_haveFuelObject.SetActive(this.GetFuel() > 0f);
		}
		if (this.m_haveOreObject)
		{
			this.m_haveOreObject.SetActive(this.GetQueueSize() > 0);
		}
		if (this.m_noOreObject)
		{
			this.m_noOreObject.SetActive(this.GetQueueSize() == 0);
		}
		if (this.m_addOreAnimationDuration > 0f && Time.time - this.m_addedOreTime < this.m_addOreAnimationDuration)
		{
			flag = true;
		}
		this.SetAnimation(flag);
	}

	// Token: 0x0600195C RID: 6492 RVA: 0x000A8B48 File Offset: 0x000A6D48
	private void SetAnimation(bool active)
	{
		foreach (Animator animator in this.m_animators)
		{
			if (animator.gameObject.activeInHierarchy)
			{
				animator.SetBool("active", active);
				animator.SetFloat("activef", active ? 1f : 0f);
			}
		}
	}

	// Token: 0x0600195D RID: 6493 RVA: 0x000A8BA4 File Offset: 0x000A6DA4
	public bool IsActive()
	{
		return (this.m_maxFuel == 0 || this.GetFuel() > 0f) && (this.m_maxOre == 0 || this.GetQueueSize() > 0) && (!this.m_requiresRoof || this.m_haveRoof) && !this.m_blockedSmoke;
	}

	// Token: 0x0600195E RID: 6494 RVA: 0x000A8BF4 File Offset: 0x000A6DF4
	private string OnHoverAddFuel()
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

	// Token: 0x0600195F RID: 6495 RVA: 0x000A8C78 File Offset: 0x000A6E78
	private string OnHoverEmptyOre()
	{
		int processedQueueSize = this.GetProcessedQueueSize();
		return Localization.instance.Localize(string.Format("{0} ({1} $piece_smelter_ready) \n[<color=yellow><b>$KEY_Use</b></color>] {2}", this.m_name, processedQueueSize, this.m_emptyOreTooltip));
	}

	// Token: 0x06001960 RID: 6496 RVA: 0x000A8CB4 File Offset: 0x000A6EB4
	private string OnHoverAddOre()
	{
		this.m_sb.Clear();
		int queueSize = this.GetQueueSize();
		this.m_sb.Append(string.Format("{0} ({1}/{2}) ", this.m_name, queueSize, this.m_maxOre));
		if (this.m_requiresRoof && !this.m_haveRoof && Mathf.Sin(Time.time * 10f) > 0f)
		{
			this.m_sb.Append(" <color=yellow>$piece_smelter_reqroof</color>");
		}
		this.m_sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] " + this.m_addOreTooltip);
		return Localization.instance.Localize(this.m_sb.ToString());
	}

	// Token: 0x04001B31 RID: 6961
	public string m_name = "Smelter";

	// Token: 0x04001B32 RID: 6962
	public string m_addOreTooltip = "$piece_smelter_additem";

	// Token: 0x04001B33 RID: 6963
	public string m_emptyOreTooltip = "$piece_smelter_empty";

	// Token: 0x04001B34 RID: 6964
	public Switch m_addWoodSwitch;

	// Token: 0x04001B35 RID: 6965
	public Switch m_addOreSwitch;

	// Token: 0x04001B36 RID: 6966
	public Switch m_emptyOreSwitch;

	// Token: 0x04001B37 RID: 6967
	public Transform m_outputPoint;

	// Token: 0x04001B38 RID: 6968
	public Transform m_roofCheckPoint;

	// Token: 0x04001B39 RID: 6969
	public GameObject m_enabledObject;

	// Token: 0x04001B3A RID: 6970
	public GameObject m_disabledObject;

	// Token: 0x04001B3B RID: 6971
	public GameObject m_haveFuelObject;

	// Token: 0x04001B3C RID: 6972
	public GameObject m_haveOreObject;

	// Token: 0x04001B3D RID: 6973
	public GameObject m_noOreObject;

	// Token: 0x04001B3E RID: 6974
	public Animator[] m_animators;

	// Token: 0x04001B3F RID: 6975
	public ItemDrop m_fuelItem;

	// Token: 0x04001B40 RID: 6976
	public int m_maxOre = 10;

	// Token: 0x04001B41 RID: 6977
	public int m_maxFuel = 10;

	// Token: 0x04001B42 RID: 6978
	public int m_fuelPerProduct = 4;

	// Token: 0x04001B43 RID: 6979
	public float m_secPerProduct = 10f;

	// Token: 0x04001B44 RID: 6980
	public bool m_spawnStack;

	// Token: 0x04001B45 RID: 6981
	public bool m_requiresRoof;

	// Token: 0x04001B46 RID: 6982
	public Windmill m_windmill;

	// Token: 0x04001B47 RID: 6983
	public SmokeSpawner m_smokeSpawner;

	// Token: 0x04001B48 RID: 6984
	public float m_addOreAnimationDuration;

	// Token: 0x04001B49 RID: 6985
	public List<Smelter.ItemConversion> m_conversion = new List<Smelter.ItemConversion>();

	// Token: 0x04001B4A RID: 6986
	public EffectList m_oreAddedEffects = new EffectList();

	// Token: 0x04001B4B RID: 6987
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x04001B4C RID: 6988
	public EffectList m_produceEffects = new EffectList();

	// Token: 0x04001B4D RID: 6989
	private ZNetView m_nview;

	// Token: 0x04001B4E RID: 6990
	private bool m_haveRoof;

	// Token: 0x04001B4F RID: 6991
	private bool m_blockedSmoke;

	// Token: 0x04001B50 RID: 6992
	private float m_addedOreTime = -1000f;

	// Token: 0x04001B51 RID: 6993
	private StringBuilder m_sb = new StringBuilder();

	// Token: 0x02000295 RID: 661
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x04001B52 RID: 6994
		public ItemDrop m_from;

		// Token: 0x04001B53 RID: 6995
		public ItemDrop m_to;
	}
}
