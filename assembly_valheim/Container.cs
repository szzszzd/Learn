using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000221 RID: 545
public class Container : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600158D RID: 5517 RVA: 0x0008D770 File Offset: 0x0008B970
	private void Awake()
	{
		this.m_nview = (this.m_rootObjectOverride ? this.m_rootObjectOverride.GetComponent<ZNetView>() : base.GetComponent<ZNetView>());
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_inventory = new Inventory(this.m_name, this.m_bkg, this.m_width, this.m_height);
		Inventory inventory = this.m_inventory;
		inventory.m_onChanged = (Action)Delegate.Combine(inventory.m_onChanged, new Action(this.OnContainerChanged));
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_nview)
		{
			this.m_nview.Register<long>("RequestOpen", new Action<long, long>(this.RPC_RequestOpen));
			this.m_nview.Register<bool>("OpenRespons", new Action<long, bool>(this.RPC_OpenRespons));
			this.m_nview.Register<long>("RequestTakeAll", new Action<long, long>(this.RPC_RequestTakeAll));
			this.m_nview.Register<bool>("TakeAllRespons", new Action<long, bool>(this.RPC_TakeAllRespons));
		}
		WearNTear wearNTear = this.m_rootObjectOverride ? this.m_rootObjectOverride.GetComponent<WearNTear>() : base.GetComponent<WearNTear>();
		if (wearNTear)
		{
			WearNTear wearNTear2 = wearNTear;
			wearNTear2.m_onDestroyed = (Action)Delegate.Combine(wearNTear2.m_onDestroyed, new Action(this.OnDestroyed));
		}
		Destructible destructible = this.m_rootObjectOverride ? this.m_rootObjectOverride.GetComponent<Destructible>() : base.GetComponent<Destructible>();
		if (destructible)
		{
			Destructible destructible2 = destructible;
			destructible2.m_onDestroyed = (Action)Delegate.Combine(destructible2.m_onDestroyed, new Action(this.OnDestroyed));
		}
		if (this.m_nview.IsOwner() && !this.m_nview.GetZDO().GetBool(ZDOVars.s_addedDefaultItems, false))
		{
			this.AddDefaultItems();
			this.m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, true);
		}
		base.InvokeRepeating("CheckForChanges", 0f, 1f);
	}

	// Token: 0x0600158E RID: 5518 RVA: 0x0008D970 File Offset: 0x0008BB70
	private void AddDefaultItems()
	{
		foreach (ItemDrop.ItemData item in this.m_defaultItems.GetDropListItems())
		{
			this.m_inventory.AddItem(item);
		}
	}

	// Token: 0x0600158F RID: 5519 RVA: 0x0008D9D0 File Offset: 0x0008BBD0
	private void DropAllItems(GameObject lootContainerPrefab)
	{
		while (this.m_inventory.NrOfItems() > 0)
		{
			Vector3 position = base.transform.position + UnityEngine.Random.insideUnitSphere * 1f;
			UnityEngine.Object.Instantiate<GameObject>(lootContainerPrefab, position, UnityEngine.Random.rotation).GetComponent<Container>().GetInventory().MoveAll(this.m_inventory);
		}
	}

	// Token: 0x06001590 RID: 5520 RVA: 0x0008DA30 File Offset: 0x0008BC30
	private void DropAllItems()
	{
		List<ItemDrop.ItemData> allItems = this.m_inventory.GetAllItems();
		int num = 1;
		foreach (ItemDrop.ItemData item in allItems)
		{
			Vector3 position = base.transform.position + Vector3.up * 0.5f + UnityEngine.Random.insideUnitSphere * 0.3f;
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			ItemDrop.DropItem(item, 0, position, rotation);
			num++;
		}
		this.m_inventory.RemoveAll();
		this.Save();
	}

	// Token: 0x06001591 RID: 5521 RVA: 0x0008DAF0 File Offset: 0x0008BCF0
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			if (this.m_destroyedLootPrefab)
			{
				this.DropAllItems(this.m_destroyedLootPrefab);
				return;
			}
			this.DropAllItems();
		}
	}

	// Token: 0x06001592 RID: 5522 RVA: 0x0008DB20 File Offset: 0x0008BD20
	private void CheckForChanges()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.Load();
		this.UpdateUseVisual();
		if (this.m_autoDestroyEmpty && this.m_nview.IsOwner() && !this.IsInUse() && this.m_inventory.NrOfItems() == 0)
		{
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06001593 RID: 5523 RVA: 0x0008DB7C File Offset: 0x0008BD7C
	private void UpdateUseVisual()
	{
		bool flag;
		if (this.m_nview.IsOwner())
		{
			flag = this.m_inUse;
			this.m_nview.GetZDO().Set(ZDOVars.s_inUse, this.m_inUse ? 1 : 0, false);
		}
		else
		{
			flag = (this.m_nview.GetZDO().GetInt(ZDOVars.s_inUse, 0) == 1);
		}
		if (this.m_open)
		{
			this.m_open.SetActive(flag);
		}
		if (this.m_closed)
		{
			this.m_closed.SetActive(!flag);
		}
	}

	// Token: 0x06001594 RID: 5524 RVA: 0x0008DC10 File Offset: 0x0008BE10
	public string GetHoverText()
	{
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		string text;
		if (this.m_inventory.NrOfItems() == 0)
		{
			text = this.m_name + " ( $piece_container_empty )";
		}
		else
		{
			text = this.m_name;
		}
		text += "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open";
		return Localization.instance.Localize(text);
	}

	// Token: 0x06001595 RID: 5525 RVA: 0x0008DC97 File Offset: 0x0008BE97
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001596 RID: 5526 RVA: 0x0008DCA0 File Offset: 0x0008BEA0
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		if (!this.CheckAccess(playerID))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_cantopen", 0, null);
			return true;
		}
		this.m_nview.InvokeRPC("RequestOpen", new object[]
		{
			playerID
		});
		return true;
	}

	// Token: 0x06001597 RID: 5527 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001598 RID: 5528 RVA: 0x0008DD1E File Offset: 0x0008BF1E
	public bool CanBeRemoved()
	{
		return this.m_privacy != Container.PrivacySetting.Private || this.GetInventory().NrOfItems() <= 0;
	}

	// Token: 0x06001599 RID: 5529 RVA: 0x0008DD3C File Offset: 0x0008BF3C
	private bool CheckAccess(long playerID)
	{
		switch (this.m_privacy)
		{
		case Container.PrivacySetting.Private:
			return this.m_piece.GetCreator() == playerID;
		case Container.PrivacySetting.Group:
			return false;
		case Container.PrivacySetting.Public:
			return true;
		default:
			return false;
		}
	}

	// Token: 0x0600159A RID: 5530 RVA: 0x0008DD78 File Offset: 0x0008BF78
	public bool IsOwner()
	{
		return this.m_nview.IsOwner();
	}

	// Token: 0x0600159B RID: 5531 RVA: 0x0008DD85 File Offset: 0x0008BF85
	public bool IsInUse()
	{
		return this.m_inUse;
	}

	// Token: 0x0600159C RID: 5532 RVA: 0x0008DD90 File Offset: 0x0008BF90
	public void SetInUse(bool inUse)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_inUse == inUse)
		{
			return;
		}
		this.m_inUse = inUse;
		this.UpdateUseVisual();
		if (inUse)
		{
			this.m_openEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			return;
		}
		this.m_closeEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x0600159D RID: 5533 RVA: 0x0008DE18 File Offset: 0x0008C018
	public Inventory GetInventory()
	{
		return this.m_inventory;
	}

	// Token: 0x0600159E RID: 5534 RVA: 0x0008DE20 File Offset: 0x0008C020
	private void RPC_RequestOpen(long uid, long playerID)
	{
		ZLog.Log(string.Concat(new string[]
		{
			"Player ",
			uid.ToString(),
			" wants to open ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.GetSessionID().ToString()
		}));
		if (!this.m_nview.IsOwner())
		{
			ZLog.Log("  but im not the owner");
			return;
		}
		if ((this.IsInUse() || (this.m_wagon && this.m_wagon.InUse())) && uid != ZNet.GetUID())
		{
			ZLog.Log("  in use");
			this.m_nview.InvokeRPC(uid, "OpenRespons", new object[]
			{
				false
			});
			return;
		}
		if (!this.CheckAccess(playerID))
		{
			ZLog.Log("  not yours");
			this.m_nview.InvokeRPC(uid, "OpenRespons", new object[]
			{
				false
			});
			return;
		}
		ZDOMan.instance.ForceSendZDO(uid, this.m_nview.GetZDO().m_uid);
		this.m_nview.GetZDO().SetOwner(uid);
		this.m_nview.InvokeRPC(uid, "OpenRespons", new object[]
		{
			true
		});
	}

	// Token: 0x0600159F RID: 5535 RVA: 0x0008DF6A File Offset: 0x0008C16A
	private void RPC_OpenRespons(long uid, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			InventoryGui.instance.Show(this, 1);
			return;
		}
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
	}

	// Token: 0x060015A0 RID: 5536 RVA: 0x0008DF9C File Offset: 0x0008C19C
	public bool TakeAll(Humanoid character)
	{
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return false;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		if (!this.CheckAccess(playerID))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_cantopen", 0, null);
			return false;
		}
		this.m_nview.InvokeRPC("RequestTakeAll", new object[]
		{
			playerID
		});
		return true;
	}

	// Token: 0x060015A1 RID: 5537 RVA: 0x0008E018 File Offset: 0x0008C218
	private void RPC_RequestTakeAll(long uid, long playerID)
	{
		ZLog.Log(string.Concat(new string[]
		{
			"Player ",
			uid.ToString(),
			" wants to takeall from ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.GetSessionID().ToString()
		}));
		if (!this.m_nview.IsOwner())
		{
			ZLog.Log("  but im not the owner");
			return;
		}
		if ((this.IsInUse() || (this.m_wagon && this.m_wagon.InUse())) && uid != ZNet.GetUID())
		{
			ZLog.Log("  in use");
			this.m_nview.InvokeRPC(uid, "TakeAllRespons", new object[]
			{
				false
			});
			return;
		}
		if (!this.CheckAccess(playerID))
		{
			ZLog.Log("  not yours");
			this.m_nview.InvokeRPC(uid, "TakeAllRespons", new object[]
			{
				false
			});
			return;
		}
		if (Time.time - this.m_lastTakeAllTime < 2f)
		{
			return;
		}
		this.m_lastTakeAllTime = Time.time;
		this.m_nview.InvokeRPC(uid, "TakeAllRespons", new object[]
		{
			true
		});
	}

	// Token: 0x060015A2 RID: 5538 RVA: 0x0008E158 File Offset: 0x0008C358
	private void RPC_TakeAllRespons(long uid, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			this.m_nview.ClaimOwnership();
			ZDOMan.instance.ForceSendZDO(uid, this.m_nview.GetZDO().m_uid);
			Player.m_localPlayer.GetInventory().MoveAll(this.m_inventory);
			if (this.m_onTakeAllSuccess != null)
			{
				this.m_onTakeAllSuccess();
				return;
			}
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
		}
	}

	// Token: 0x060015A3 RID: 5539 RVA: 0x0008E1D6 File Offset: 0x0008C3D6
	private void OnContainerChanged()
	{
		if (this.m_loading)
		{
			return;
		}
		if (!this.IsOwner())
		{
			return;
		}
		this.Save();
	}

	// Token: 0x060015A4 RID: 5540 RVA: 0x0008E1F0 File Offset: 0x0008C3F0
	private void Save()
	{
		ZPackage zpackage = new ZPackage();
		this.m_inventory.Save(zpackage);
		string @base = zpackage.GetBase64();
		this.m_nview.GetZDO().Set(ZDOVars.s_items, @base);
		this.m_lastRevision = this.m_nview.GetZDO().DataRevision;
		this.m_lastDataString = @base;
	}

	// Token: 0x060015A5 RID: 5541 RVA: 0x0008E24C File Offset: 0x0008C44C
	private void Load()
	{
		if (this.m_nview.GetZDO().DataRevision == this.m_lastRevision)
		{
			return;
		}
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_items, "");
		if (string.IsNullOrEmpty(@string) || @string == this.m_lastDataString)
		{
			return;
		}
		ZPackage pkg = new ZPackage(@string);
		this.m_loading = true;
		this.m_inventory.Load(pkg);
		this.m_loading = false;
		this.m_lastRevision = this.m_nview.GetZDO().DataRevision;
		this.m_lastDataString = @string;
	}

	// Token: 0x0400167F RID: 5759
	private float m_lastTakeAllTime;

	// Token: 0x04001680 RID: 5760
	public Action m_onTakeAllSuccess;

	// Token: 0x04001681 RID: 5761
	public string m_name = "Container";

	// Token: 0x04001682 RID: 5762
	public Sprite m_bkg;

	// Token: 0x04001683 RID: 5763
	public int m_width = 3;

	// Token: 0x04001684 RID: 5764
	public int m_height = 2;

	// Token: 0x04001685 RID: 5765
	public Container.PrivacySetting m_privacy = Container.PrivacySetting.Public;

	// Token: 0x04001686 RID: 5766
	public bool m_checkGuardStone;

	// Token: 0x04001687 RID: 5767
	public bool m_autoDestroyEmpty;

	// Token: 0x04001688 RID: 5768
	public DropTable m_defaultItems = new DropTable();

	// Token: 0x04001689 RID: 5769
	public GameObject m_open;

	// Token: 0x0400168A RID: 5770
	public GameObject m_closed;

	// Token: 0x0400168B RID: 5771
	public EffectList m_openEffects = new EffectList();

	// Token: 0x0400168C RID: 5772
	public EffectList m_closeEffects = new EffectList();

	// Token: 0x0400168D RID: 5773
	public ZNetView m_rootObjectOverride;

	// Token: 0x0400168E RID: 5774
	public Vagon m_wagon;

	// Token: 0x0400168F RID: 5775
	public GameObject m_destroyedLootPrefab;

	// Token: 0x04001690 RID: 5776
	private Inventory m_inventory;

	// Token: 0x04001691 RID: 5777
	private ZNetView m_nview;

	// Token: 0x04001692 RID: 5778
	private Piece m_piece;

	// Token: 0x04001693 RID: 5779
	private bool m_inUse;

	// Token: 0x04001694 RID: 5780
	private bool m_loading;

	// Token: 0x04001695 RID: 5781
	private uint m_lastRevision = uint.MaxValue;

	// Token: 0x04001696 RID: 5782
	private string m_lastDataString = "";

	// Token: 0x02000222 RID: 546
	public enum PrivacySetting
	{
		// Token: 0x04001698 RID: 5784
		Private,
		// Token: 0x04001699 RID: 5785
		Group,
		// Token: 0x0400169A RID: 5786
		Public
	}
}
