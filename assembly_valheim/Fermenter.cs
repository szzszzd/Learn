using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x02000233 RID: 563
public class Fermenter : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001610 RID: 5648 RVA: 0x00090C78 File Offset: 0x0008EE78
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_fermentingObject.SetActive(false);
		this.m_readyObject.SetActive(false);
		this.m_topObject.SetActive(true);
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<string>("AddItem", new Action<long, string>(this.RPC_AddItem));
		this.m_nview.Register("Tap", new Action<long>(this.RPC_Tap));
		if (this.GetStatus() == Fermenter.Status.Fermenting)
		{
			base.InvokeRepeating("SlowUpdate", 2f, 2f);
		}
		else
		{
			base.InvokeRepeating("SlowUpdate", 0f, 2f);
		}
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
	}

	// Token: 0x06001611 RID: 5649 RVA: 0x00090D70 File Offset: 0x0008EF70
	private void DropAllItems()
	{
		Fermenter.Status status = this.GetStatus();
		string content = this.GetContent();
		if (!string.IsNullOrEmpty(content))
		{
			if (status == Fermenter.Status.Ready)
			{
				Fermenter.ItemConversion itemConversion = this.GetItemConversion(content);
				if (itemConversion != null)
				{
					for (int i = 0; i < itemConversion.m_producedItems; i++)
					{
						this.<DropAllItems>g__drop|2_0(itemConversion.m_to);
					}
				}
			}
			else
			{
				GameObject prefab = ZNetScene.instance.GetPrefab(content);
				if (prefab != null)
				{
					ItemDrop component = prefab.GetComponent<ItemDrop>();
					if (component)
					{
						this.<DropAllItems>g__drop|2_0(component);
					}
				}
			}
			this.m_nview.GetZDO().Set(ZDOVars.s_content, "");
			this.m_nview.GetZDO().Set(ZDOVars.s_startTime, 0, false);
		}
	}

	// Token: 0x06001612 RID: 5650 RVA: 0x00090E26 File Offset: 0x0008F026
	private void OnDestroyed()
	{
		this.m_nview.IsOwner();
	}

	// Token: 0x06001613 RID: 5651 RVA: 0x00090E34 File Offset: 0x0008F034
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001614 RID: 5652 RVA: 0x00090E3C File Offset: 0x0008F03C
	public string GetHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		switch (this.GetStatus())
		{
		case Fermenter.Status.Empty:
		{
			string text = "$piece_container_empty";
			if (this.m_exposed)
			{
				text += ", $piece_fermenter_exposed";
			}
			return Localization.instance.Localize(this.m_name + " ( " + text + " )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_add");
		}
		case Fermenter.Status.Fermenting:
		{
			string contentName = this.GetContentName();
			if (this.m_exposed)
			{
				return Localization.instance.Localize(this.m_name + " ( " + contentName + ", $piece_fermenter_exposed )");
			}
			return Localization.instance.Localize(this.m_name + " ( " + contentName + ", $piece_fermenter_fermenting )");
		}
		case Fermenter.Status.Ready:
		{
			string contentName2 = this.GetContentName();
			return Localization.instance.Localize(this.m_name + " ( " + contentName2 + ", $piece_fermenter_ready )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_tap");
		}
		}
		return this.m_name;
	}

	// Token: 0x06001615 RID: 5653 RVA: 0x00090F5C File Offset: 0x0008F15C
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
		Fermenter.Status status = this.GetStatus();
		if (status == Fermenter.Status.Empty)
		{
			if (this.m_exposed)
			{
				user.Message(MessageHud.MessageType.Center, "$piece_fermenter_exposed", 0, null);
				return false;
			}
			ItemDrop.ItemData itemData = this.FindCookableItem(user.GetInventory());
			if (itemData == null)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems", 0, null);
				return false;
			}
			this.AddItem(user, itemData);
			return true;
		}
		else
		{
			if (status == Fermenter.Status.Ready)
			{
				this.m_nview.InvokeRPC("Tap", Array.Empty<object>());
				return true;
			}
			return false;
		}
	}

	// Token: 0x06001616 RID: 5654 RVA: 0x00090FF2 File Offset: 0x0008F1F2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return PrivateArea.CheckAccess(base.transform.position, 0f, true, false) && this.AddItem(user, item);
	}

	// Token: 0x06001617 RID: 5655 RVA: 0x00091018 File Offset: 0x0008F218
	private void SlowUpdate()
	{
		this.UpdateCover(2f);
		switch (this.GetStatus())
		{
		case Fermenter.Status.Empty:
			this.m_fermentingObject.SetActive(false);
			this.m_readyObject.SetActive(false);
			this.m_topObject.SetActive(false);
			return;
		case Fermenter.Status.Fermenting:
			this.m_readyObject.SetActive(false);
			this.m_topObject.SetActive(true);
			this.m_fermentingObject.SetActive(!this.m_exposed);
			return;
		case Fermenter.Status.Exposed:
			break;
		case Fermenter.Status.Ready:
			this.m_fermentingObject.SetActive(false);
			this.m_readyObject.SetActive(true);
			this.m_topObject.SetActive(true);
			break;
		default:
			return;
		}
	}

	// Token: 0x06001618 RID: 5656 RVA: 0x000910C4 File Offset: 0x0008F2C4
	private Fermenter.Status GetStatus()
	{
		if (string.IsNullOrEmpty(this.GetContent()))
		{
			return Fermenter.Status.Empty;
		}
		if (this.GetFermentationTime() > (double)this.m_fermentationDuration)
		{
			return Fermenter.Status.Ready;
		}
		return Fermenter.Status.Fermenting;
	}

	// Token: 0x06001619 RID: 5657 RVA: 0x000910E8 File Offset: 0x0008F2E8
	private bool AddItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.GetStatus() != Fermenter.Status.Empty)
		{
			return false;
		}
		if (!this.IsItemAllowed(item))
		{
			return false;
		}
		if (!user.GetInventory().RemoveOneItem(item))
		{
			return false;
		}
		this.m_nview.InvokeRPC("AddItem", new object[]
		{
			item.m_dropPrefab.name
		});
		return true;
	}

	// Token: 0x0600161A RID: 5658 RVA: 0x00091140 File Offset: 0x0008F340
	private void RPC_AddItem(long sender, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetStatus() != Fermenter.Status.Empty)
		{
			return;
		}
		if (!this.IsItemAllowed(name))
		{
			ZLog.DevLog("Item not allowed");
			return;
		}
		this.m_addedEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		this.m_nview.GetZDO().Set(ZDOVars.s_content, name);
		this.m_nview.GetZDO().Set(ZDOVars.s_startTime, ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x0600161B RID: 5659 RVA: 0x000911E0 File Offset: 0x0008F3E0
	private void RPC_Tap(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetStatus() != Fermenter.Status.Ready)
		{
			return;
		}
		this.m_delayedTapItem = this.GetContent();
		base.Invoke("DelayedTap", this.m_tapDelay);
		this.m_tapEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		this.m_nview.GetZDO().Set(ZDOVars.s_content, "");
		this.m_nview.GetZDO().Set(ZDOVars.s_startTime, 0, false);
	}

	// Token: 0x0600161C RID: 5660 RVA: 0x0009127C File Offset: 0x0008F47C
	private void DelayedTap()
	{
		this.m_spawnEffects.Create(this.m_outputPoint.transform.position, Quaternion.identity, null, 1f, -1);
		Fermenter.ItemConversion itemConversion = this.GetItemConversion(this.m_delayedTapItem);
		if (itemConversion != null)
		{
			float d = 0.3f;
			for (int i = 0; i < itemConversion.m_producedItems; i++)
			{
				Vector3 position = this.m_outputPoint.position + Vector3.up * d;
				UnityEngine.Object.Instantiate<ItemDrop>(itemConversion.m_to, position, Quaternion.identity);
			}
		}
	}

	// Token: 0x0600161D RID: 5661 RVA: 0x00091308 File Offset: 0x0008F508
	private void ResetFermentationTimer()
	{
		if (this.GetStatus() == Fermenter.Status.Fermenting)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_startTime, ZNet.instance.GetTime().Ticks);
		}
	}

	// Token: 0x0600161E RID: 5662 RVA: 0x00091348 File Offset: 0x0008F548
	private double GetFermentationTime()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_startTime, 0L));
		if (d.Ticks == 0L)
		{
			return -1.0;
		}
		return (ZNet.instance.GetTime() - d).TotalSeconds;
	}

	// Token: 0x0600161F RID: 5663 RVA: 0x000913A0 File Offset: 0x0008F5A0
	private string GetContentName()
	{
		string content = this.GetContent();
		if (string.IsNullOrEmpty(content))
		{
			return "";
		}
		Fermenter.ItemConversion itemConversion = this.GetItemConversion(content);
		if (itemConversion == null)
		{
			return "Invalid";
		}
		return itemConversion.m_from.m_itemData.m_shared.m_name;
	}

	// Token: 0x06001620 RID: 5664 RVA: 0x000913E8 File Offset: 0x0008F5E8
	private string GetContent()
	{
		return this.m_nview.GetZDO().GetString(ZDOVars.s_content, "");
	}

	// Token: 0x06001621 RID: 5665 RVA: 0x00091404 File Offset: 0x0008F604
	private void UpdateCover(float dt)
	{
		this.m_updateCoverTimer -= dt;
		if (this.m_updateCoverTimer <= 0f)
		{
			this.m_updateCoverTimer = 10f;
			float num;
			bool flag;
			Cover.GetCoverForPoint(this.m_roofCheckPoint.position, out num, out flag, 0.5f);
			this.m_exposed = (!flag || num < 0.7f);
			if (this.m_exposed && this.m_nview.IsOwner())
			{
				this.ResetFermentationTimer();
			}
		}
	}

	// Token: 0x06001622 RID: 5666 RVA: 0x0009147F File Offset: 0x0008F67F
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x06001623 RID: 5667 RVA: 0x00091494 File Offset: 0x0008F694
	private bool IsItemAllowed(string itemName)
	{
		using (List<Fermenter.ItemConversion>.Enumerator enumerator = this.m_conversion.GetEnumerator())
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

	// Token: 0x06001624 RID: 5668 RVA: 0x00091500 File Offset: 0x0008F700
	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (Fermenter.ItemConversion itemConversion in this.m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name, -1, false);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	// Token: 0x06001625 RID: 5669 RVA: 0x00091574 File Offset: 0x0008F774
	private Fermenter.ItemConversion GetItemConversion(string itemName)
	{
		foreach (Fermenter.ItemConversion itemConversion in this.m_conversion)
		{
			if (itemConversion.m_from.gameObject.name == itemName)
			{
				return itemConversion;
			}
		}
		return null;
	}

	// Token: 0x06001627 RID: 5671 RVA: 0x0009164C File Offset: 0x0008F84C
	[CompilerGenerated]
	private void <DropAllItems>g__drop|2_0(ItemDrop item)
	{
		Vector3 position = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		UnityEngine.Object.Instantiate<GameObject>(item.gameObject, position, rotation);
	}

	// Token: 0x04001709 RID: 5897
	private const float updateDT = 2f;

	// Token: 0x0400170A RID: 5898
	public string m_name = "Fermentation barrel";

	// Token: 0x0400170B RID: 5899
	public float m_fermentationDuration = 2400f;

	// Token: 0x0400170C RID: 5900
	public GameObject m_fermentingObject;

	// Token: 0x0400170D RID: 5901
	public GameObject m_readyObject;

	// Token: 0x0400170E RID: 5902
	public GameObject m_topObject;

	// Token: 0x0400170F RID: 5903
	public EffectList m_addedEffects = new EffectList();

	// Token: 0x04001710 RID: 5904
	public EffectList m_tapEffects = new EffectList();

	// Token: 0x04001711 RID: 5905
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x04001712 RID: 5906
	public Switch m_addSwitch;

	// Token: 0x04001713 RID: 5907
	public Switch m_tapSwitch;

	// Token: 0x04001714 RID: 5908
	public float m_tapDelay = 1.5f;

	// Token: 0x04001715 RID: 5909
	public Transform m_outputPoint;

	// Token: 0x04001716 RID: 5910
	public Transform m_roofCheckPoint;

	// Token: 0x04001717 RID: 5911
	public List<Fermenter.ItemConversion> m_conversion = new List<Fermenter.ItemConversion>();

	// Token: 0x04001718 RID: 5912
	private ZNetView m_nview;

	// Token: 0x04001719 RID: 5913
	private float m_updateCoverTimer;

	// Token: 0x0400171A RID: 5914
	private bool m_exposed;

	// Token: 0x0400171B RID: 5915
	private string m_delayedTapItem = "";

	// Token: 0x02000234 RID: 564
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x0400171C RID: 5916
		public ItemDrop m_from;

		// Token: 0x0400171D RID: 5917
		public ItemDrop m_to;

		// Token: 0x0400171E RID: 5918
		public int m_producedItems = 4;
	}

	// Token: 0x02000235 RID: 565
	private enum Status
	{
		// Token: 0x04001720 RID: 5920
		Empty,
		// Token: 0x04001721 RID: 5921
		Fermenting,
		// Token: 0x04001722 RID: 5922
		Exposed,
		// Token: 0x04001723 RID: 5923
		Ready
	}
}
