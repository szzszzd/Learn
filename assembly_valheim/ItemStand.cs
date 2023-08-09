using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000259 RID: 601
public class ItemStand : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06001732 RID: 5938 RVA: 0x0009982C File Offset: 0x00097A2C
	private void Awake()
	{
		this.m_nview = (this.m_netViewOverride ? this.m_netViewOverride : base.gameObject.GetComponent<ZNetView>());
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		this.m_nview.Register("DropItem", new Action<long>(this.RPC_DropItem));
		this.m_nview.Register("RequestOwn", new Action<long>(this.RPC_RequestOwn));
		this.m_nview.Register("DestroyAttachment", new Action<long>(this.RPC_DestroyAttachment));
		this.m_nview.Register<string, int, int>("SetVisualItem", new Action<long, string, int, int>(this.RPC_SetVisualItem));
		base.InvokeRepeating("UpdateVisual", 1f, 4f);
	}

	// Token: 0x06001733 RID: 5939 RVA: 0x00099923 File Offset: 0x00097B23
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			this.DropItem();
		}
	}

	// Token: 0x06001734 RID: 5940 RVA: 0x00099938 File Offset: 0x00097B38
	public string GetHoverText()
	{
		if (!Player.m_localPlayer)
		{
			return "";
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		if (this.HaveAttachment())
		{
			if (this.m_canBeRemoved)
			{
				return Localization.instance.Localize(this.m_name + " ( " + this.m_currentItemName + " )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_take");
			}
			if (!(this.m_guardianPower != null))
			{
				return "";
			}
			if (base.IsInvoking("DelayedPowerActivation"))
			{
				return "";
			}
			if (this.IsGuardianPowerActive(Player.m_localPlayer))
			{
				return "";
			}
			string tooltipString = this.m_guardianPower.GetTooltipString();
			return Localization.instance.Localize(string.Concat(new string[]
			{
				"<color=orange>",
				this.m_guardianPower.m_name,
				"</color>\n",
				tooltipString,
				"\n\n[<color=yellow><b>$KEY_Use</b></color>] $guardianstone_hook_activate"
			}));
		}
		else
		{
			if (this.m_autoAttach && this.m_supportedItems.Count == 1)
			{
				return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_attach");
			}
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>1-8</b></color>] $piece_itemstand_attach");
		}
	}

	// Token: 0x06001735 RID: 5941 RVA: 0x00099A98 File Offset: 0x00097C98
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001736 RID: 5942 RVA: 0x00099AA0 File Offset: 0x00097CA0
	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		if (!this.HaveAttachment())
		{
			if (this.m_autoAttach && this.m_supportedItems.Count == 1)
			{
				ItemDrop.ItemData item = user.GetInventory().GetItem(this.m_supportedItems[0].m_itemData.m_shared.m_name, -1, false);
				if (item != null)
				{
					this.UseItem(user, item);
					return true;
				}
				user.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem", 0, null);
			}
			return false;
		}
		if (this.m_canBeRemoved)
		{
			this.m_nview.InvokeRPC("DropItem", Array.Empty<object>());
			return true;
		}
		if (!(this.m_guardianPower != null))
		{
			return false;
		}
		if (base.IsInvoking("DelayedPowerActivation"))
		{
			return false;
		}
		if (this.IsGuardianPowerActive(user))
		{
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$guardianstone_hook_power_activate ", 0, null);
		this.m_activatePowerEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		this.m_activatePowerEffectsPlayer.Create(user.transform.position, Quaternion.identity, user.transform, 1f, -1);
		base.Invoke("DelayedPowerActivation", this.m_powerActivationDelay);
		return true;
	}

	// Token: 0x06001737 RID: 5943 RVA: 0x00099BEF File Offset: 0x00097DEF
	private bool IsGuardianPowerActive(Humanoid user)
	{
		return (user as Player).GetGuardianPowerName() == this.m_guardianPower.name;
	}

	// Token: 0x06001738 RID: 5944 RVA: 0x00099C0C File Offset: 0x00097E0C
	private void DelayedPowerActivation()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		localPlayer.SetGuardianPower(this.m_guardianPower.name);
	}

	// Token: 0x06001739 RID: 5945 RVA: 0x00099C3C File Offset: 0x00097E3C
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.HaveAttachment())
		{
			return false;
		}
		if (!this.CanAttach(item))
		{
			user.Message(MessageHud.MessageType.Center, "$piece_itemstand_cantattach", 0, null);
			return true;
		}
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.InvokeRPC("RequestOwn", Array.Empty<object>());
		}
		this.m_queuedItem = item;
		base.CancelInvoke("UpdateAttach");
		base.InvokeRepeating("UpdateAttach", 0f, 0.1f);
		return true;
	}

	// Token: 0x0600173A RID: 5946 RVA: 0x00099CB6 File Offset: 0x00097EB6
	private void RPC_DropItem(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_canBeRemoved)
		{
			return;
		}
		this.DropItem();
	}

	// Token: 0x0600173B RID: 5947 RVA: 0x00099CD5 File Offset: 0x00097ED5
	public void DestroyAttachment()
	{
		this.m_nview.InvokeRPC("DestroyAttachment", Array.Empty<object>());
	}

	// Token: 0x0600173C RID: 5948 RVA: 0x00099CEC File Offset: 0x00097EEC
	private void RPC_DestroyAttachment(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.HaveAttachment())
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_item, "");
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetVisualItem", new object[]
		{
			"",
			0,
			0
		});
		this.m_destroyEffects.Create(this.m_dropSpawnPoint.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x0600173D RID: 5949 RVA: 0x00099D84 File Offset: 0x00097F84
	private void DropItem()
	{
		if (!this.HaveAttachment())
		{
			return;
		}
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_item, "");
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(@string);
		if (itemPrefab)
		{
			Vector3 b = Vector3.zero;
			Quaternion rhs = Quaternion.identity;
			Transform transform = itemPrefab.transform.Find("attach");
			if (itemPrefab.transform.Find("attachobj") && transform)
			{
				rhs = transform.transform.localRotation;
				b = transform.transform.localPosition;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(itemPrefab, this.m_dropSpawnPoint.position + b, this.m_dropSpawnPoint.rotation * rhs);
			gameObject.GetComponent<ItemDrop>().LoadFromExternalZDO(this.m_nview.GetZDO());
			gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
			this.m_effects.Create(this.m_dropSpawnPoint.position, Quaternion.identity, null, 1f, -1);
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_item, "");
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetVisualItem", new object[]
		{
			"",
			0,
			0
		});
	}

	// Token: 0x0600173E RID: 5950 RVA: 0x00099EED File Offset: 0x000980ED
	private Transform GetAttach(ItemDrop.ItemData item)
	{
		return this.m_attachOther;
	}

	// Token: 0x0600173F RID: 5951 RVA: 0x00099EF8 File Offset: 0x000980F8
	private void UpdateAttach()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		base.CancelInvoke("UpdateAttach");
		Player localPlayer = Player.m_localPlayer;
		if (this.m_queuedItem != null && localPlayer != null && localPlayer.GetInventory().ContainsItem(this.m_queuedItem) && !this.HaveAttachment())
		{
			ItemDrop.ItemData itemData = this.m_queuedItem.Clone();
			itemData.m_stack = 1;
			this.m_nview.GetZDO().Set(ZDOVars.s_item, this.m_queuedItem.m_dropPrefab.name);
			ItemDrop.SaveToZDO(itemData, this.m_nview.GetZDO());
			localPlayer.UnequipItem(this.m_queuedItem, true);
			localPlayer.GetInventory().RemoveOneItem(this.m_queuedItem);
			this.m_nview.InvokeRPC(ZNetView.Everybody, "SetVisualItem", new object[]
			{
				itemData.m_dropPrefab.name,
				itemData.m_variant,
				itemData.m_quality
			});
			Transform attach = this.GetAttach(this.m_queuedItem);
			this.m_effects.Create(attach.transform.position, Quaternion.identity, null, 1f, -1);
		}
		this.m_queuedItem = null;
	}

	// Token: 0x06001740 RID: 5952 RVA: 0x0009A040 File Offset: 0x00098240
	private void RPC_RequestOwn(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().SetOwner(sender);
	}

	// Token: 0x06001741 RID: 5953 RVA: 0x0009A064 File Offset: 0x00098264
	private void UpdateVisual()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_item, "");
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_variant, 0);
		int int2 = this.m_nview.GetZDO().GetInt(ZDOVars.s_quality, 1);
		this.SetVisualItem(@string, @int, int2);
	}

	// Token: 0x06001742 RID: 5954 RVA: 0x0009A0DF File Offset: 0x000982DF
	private void RPC_SetVisualItem(long sender, string itemName, int variant, int quality)
	{
		this.SetVisualItem(itemName, variant, quality);
	}

	// Token: 0x06001743 RID: 5955 RVA: 0x0009A0EC File Offset: 0x000982EC
	private void SetVisualItem(string itemName, int variant, int quality)
	{
		if (this.m_visualName == itemName && this.m_visualVariant == variant)
		{
			return;
		}
		this.m_visualName = itemName;
		this.m_visualVariant = variant;
		this.m_currentItemName = "";
		if (this.m_visualName == "")
		{
			UnityEngine.Object.Destroy(this.m_visualItem);
			return;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
		if (itemPrefab == null)
		{
			ZLog.LogWarning("Missing item prefab " + itemName);
			return;
		}
		GameObject attachPrefab = this.GetAttachPrefab(itemPrefab);
		if (attachPrefab == null)
		{
			ZLog.LogWarning("Failed to get attach prefab for item " + itemName);
			return;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		this.m_currentItemName = component.m_itemData.m_shared.m_name;
		Transform attach = this.GetAttach(component.m_itemData);
		Transform transform = itemPrefab.transform.Find("attachobj");
		GameObject gameObject = (transform != null) ? transform.gameObject : null;
		this.m_visualItem = UnityEngine.Object.Instantiate<GameObject>((gameObject != null) ? gameObject : attachPrefab, attach.position, attach.rotation, attach);
		this.m_visualItem.transform.localPosition = attachPrefab.transform.localPosition;
		this.m_visualItem.transform.localRotation = attachPrefab.transform.localRotation;
		this.m_visualItem.transform.localScale = Vector3.Scale(attachPrefab.transform.localScale, component.m_itemData.GetScale((float)quality));
		IEquipmentVisual componentInChildren = this.m_visualItem.GetComponentInChildren<IEquipmentVisual>();
		if (componentInChildren != null)
		{
			componentInChildren.Setup(this.m_visualVariant);
		}
	}

	// Token: 0x06001744 RID: 5956 RVA: 0x0009A28C File Offset: 0x0009848C
	private GameObject GetAttachPrefab(GameObject item)
	{
		Transform transform = item.transform.Find("attach");
		if (transform)
		{
			return transform.gameObject;
		}
		return null;
	}

	// Token: 0x06001745 RID: 5957 RVA: 0x0009A2BC File Offset: 0x000984BC
	private bool CanAttach(ItemDrop.ItemData item)
	{
		return !(this.GetAttachPrefab(item.m_dropPrefab) == null) && !this.IsUnsupported(item) && this.IsSupported(item) && (this.m_supportedTypes.Count == 0 || this.m_supportedTypes.Contains(item.m_shared.m_itemType));
	}

	// Token: 0x06001746 RID: 5958 RVA: 0x0009A31C File Offset: 0x0009851C
	private bool IsUnsupported(ItemDrop.ItemData item)
	{
		using (List<ItemDrop>.Enumerator enumerator = this.m_unsupportedItems.GetEnumerator())
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

	// Token: 0x06001747 RID: 5959 RVA: 0x0009A390 File Offset: 0x00098590
	private bool IsSupported(ItemDrop.ItemData item)
	{
		if (this.m_supportedItems.Count == 0)
		{
			return true;
		}
		using (List<ItemDrop>.Enumerator enumerator = this.m_supportedItems.GetEnumerator())
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

	// Token: 0x06001748 RID: 5960 RVA: 0x0009A414 File Offset: 0x00098614
	public bool HaveAttachment()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetString(ZDOVars.s_item, "") != "";
	}

	// Token: 0x06001749 RID: 5961 RVA: 0x0009A449 File Offset: 0x00098649
	public string GetAttachedItem()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		return this.m_nview.GetZDO().GetString(ZDOVars.s_item, "");
	}

	// Token: 0x04001895 RID: 6293
	public ZNetView m_netViewOverride;

	// Token: 0x04001896 RID: 6294
	public string m_name = "";

	// Token: 0x04001897 RID: 6295
	public Transform m_attachOther;

	// Token: 0x04001898 RID: 6296
	public Transform m_dropSpawnPoint;

	// Token: 0x04001899 RID: 6297
	public bool m_canBeRemoved = true;

	// Token: 0x0400189A RID: 6298
	public bool m_autoAttach;

	// Token: 0x0400189B RID: 6299
	public List<ItemDrop.ItemData.ItemType> m_supportedTypes = new List<ItemDrop.ItemData.ItemType>();

	// Token: 0x0400189C RID: 6300
	public List<ItemDrop> m_unsupportedItems = new List<ItemDrop>();

	// Token: 0x0400189D RID: 6301
	public List<ItemDrop> m_supportedItems = new List<ItemDrop>();

	// Token: 0x0400189E RID: 6302
	public EffectList m_effects = new EffectList();

	// Token: 0x0400189F RID: 6303
	public EffectList m_destroyEffects = new EffectList();

	// Token: 0x040018A0 RID: 6304
	[Header("Guardian power")]
	public float m_powerActivationDelay = 2f;

	// Token: 0x040018A1 RID: 6305
	public StatusEffect m_guardianPower;

	// Token: 0x040018A2 RID: 6306
	public EffectList m_activatePowerEffects = new EffectList();

	// Token: 0x040018A3 RID: 6307
	public EffectList m_activatePowerEffectsPlayer = new EffectList();

	// Token: 0x040018A4 RID: 6308
	private string m_visualName = "";

	// Token: 0x040018A5 RID: 6309
	private int m_visualVariant;

	// Token: 0x040018A6 RID: 6310
	private GameObject m_visualItem;

	// Token: 0x040018A7 RID: 6311
	private string m_currentItemName = "";

	// Token: 0x040018A8 RID: 6312
	private ItemDrop.ItemData m_queuedItem;

	// Token: 0x040018A9 RID: 6313
	private ZNetView m_nview;
}
