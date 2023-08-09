using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000125 RID: 293
public class ItemDrop : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000B63 RID: 2915 RVA: 0x00053F34 File Offset: 0x00052134
	private void Awake()
	{
		if (!string.IsNullOrEmpty(base.name))
		{
			this.m_nameHash = base.name.GetStableHashCode();
		}
		this.m_myIndex = ItemDrop.s_instances.Count;
		ItemDrop.s_instances.Add(this);
		string prefabName = this.GetPrefabName(base.gameObject.name);
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
		this.m_itemData.m_dropPrefab = itemPrefab;
		if (Application.isEditor)
		{
			this.m_itemData.m_shared = itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared;
		}
		this.m_floating = base.GetComponent<Floating>();
		this.m_body = base.GetComponent<Rigidbody>();
		if (this.m_body)
		{
			this.m_body.maxDepenetrationVelocity = 1f;
		}
		this.m_spawnTime = Time.time;
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.IsValid())
		{
			if (this.m_nview.IsOwner())
			{
				DateTime dateTime = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L));
				if (dateTime.Ticks == 0L)
				{
					this.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
				}
			}
			this.m_nview.Register("RequestOwn", new Action<long>(this.RPC_RequestOwn));
			this.Load();
			base.InvokeRepeating("SlowUpdate", UnityEngine.Random.Range(1f, 2f), 10f);
		}
		this.SetQuality(this.m_itemData.m_quality);
	}

	// Token: 0x06000B64 RID: 2916 RVA: 0x000540E0 File Offset: 0x000522E0
	private void OnDestroy()
	{
		ItemDrop.s_instances[this.m_myIndex] = ItemDrop.s_instances[ItemDrop.s_instances.Count - 1];
		ItemDrop.s_instances[this.m_myIndex].m_myIndex = this.m_myIndex;
		ItemDrop.s_instances.RemoveAt(ItemDrop.s_instances.Count - 1);
	}

	// Token: 0x06000B65 RID: 2917 RVA: 0x00054144 File Offset: 0x00052344
	private void Start()
	{
		this.Save();
		IEquipmentVisual componentInChildren = base.gameObject.GetComponentInChildren<IEquipmentVisual>();
		if (componentInChildren != null)
		{
			componentInChildren.Setup(this.m_itemData.m_variant);
		}
	}

	// Token: 0x06000B66 RID: 2918 RVA: 0x00054178 File Offset: 0x00052378
	private double GetTimeSinceSpawned()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L));
		return (ZNet.instance.GetTime() - d).TotalSeconds;
	}

	// Token: 0x06000B67 RID: 2919 RVA: 0x000541BC File Offset: 0x000523BC
	private void SlowUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.TerrainCheck();
		if (this.m_autoDestroy)
		{
			this.TimedDestruction();
		}
		if (ItemDrop.s_instances.Count > 200)
		{
			this.AutoStackItems();
		}
	}

	// Token: 0x06000B68 RID: 2920 RVA: 0x00054210 File Offset: 0x00052410
	private void TerrainCheck()
	{
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		if (base.transform.position.y - groundHeight < -0.5f)
		{
			Vector3 position = base.transform.position;
			position.y = groundHeight + 0.5f;
			base.transform.position = position;
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.velocity = Vector3.zero;
			}
		}
	}

	// Token: 0x06000B69 RID: 2921 RVA: 0x0005428C File Offset: 0x0005248C
	private void TimedDestruction()
	{
		if (this.GetTimeSinceSpawned() < 3600.0)
		{
			return;
		}
		if (this.IsInsideBase())
		{
			return;
		}
		if (Player.IsPlayerInRange(base.transform.position, 25f))
		{
			return;
		}
		if (this.InTar())
		{
			return;
		}
		this.m_nview.Destroy();
	}

	// Token: 0x06000B6A RID: 2922 RVA: 0x000542E0 File Offset: 0x000524E0
	private bool IsInsideBase()
	{
		return base.transform.position.y > ZoneSystem.instance.m_waterLevel + -2f && EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.PlayerBase, 0f);
	}

	// Token: 0x06000B6B RID: 2923 RVA: 0x00054330 File Offset: 0x00052530
	private void AutoStackItems()
	{
		if (this.m_itemData.m_shared.m_maxStackSize <= 1 || this.m_itemData.m_stack >= this.m_itemData.m_shared.m_maxStackSize)
		{
			return;
		}
		if (this.m_haveAutoStacked)
		{
			return;
		}
		this.m_haveAutoStacked = true;
		if (ItemDrop.s_itemMask == 0)
		{
			ItemDrop.s_itemMask = LayerMask.GetMask(new string[]
			{
				"item"
			});
		}
		bool flag = false;
		foreach (Collider collider in Physics.OverlapSphere(base.transform.position, 4f, ItemDrop.s_itemMask))
		{
			if (collider.attachedRigidbody)
			{
				ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
				if (!(component == null) && !(component == this) && component.m_itemData.m_shared.m_autoStack && !(component.m_nview == null) && component.m_nview.IsValid() && component.m_nview.IsOwner() && !(component.m_itemData.m_shared.m_name != this.m_itemData.m_shared.m_name) && component.m_itemData.m_quality == this.m_itemData.m_quality)
				{
					int num = this.m_itemData.m_shared.m_maxStackSize - this.m_itemData.m_stack;
					if (num == 0)
					{
						break;
					}
					if (component.m_itemData.m_stack <= num)
					{
						this.m_itemData.m_stack += component.m_itemData.m_stack;
						flag = true;
						component.m_nview.Destroy();
					}
				}
			}
		}
		if (flag)
		{
			this.Save();
		}
	}

	// Token: 0x06000B6C RID: 2924 RVA: 0x000544FC File Offset: 0x000526FC
	public string GetHoverText()
	{
		this.Load();
		string str = this.m_itemData.m_shared.m_name;
		if (this.m_itemData.m_quality > 1)
		{
			str = str + "[" + this.m_itemData.m_quality.ToString() + "] ";
		}
		if (this.m_itemData.m_stack > 1)
		{
			str = str + " x" + this.m_itemData.m_stack.ToString();
		}
		return Localization.instance.Localize(str + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	// Token: 0x06000B6D RID: 2925 RVA: 0x0005458E File Offset: 0x0005278E
	public string GetHoverName()
	{
		return this.m_itemData.m_shared.m_name;
	}

	// Token: 0x06000B6E RID: 2926 RVA: 0x000545A0 File Offset: 0x000527A0
	private string GetPrefabName(string name)
	{
		char[] anyOf = new char[]
		{
			'(',
			' '
		};
		int num = name.IndexOfAny(anyOf);
		string result;
		if (num >= 0)
		{
			result = name.Substring(0, num);
		}
		else
		{
			result = name;
		}
		return result;
	}

	// Token: 0x06000B6F RID: 2927 RVA: 0x000545D8 File Offset: 0x000527D8
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (this.InTar())
		{
			character.Message(MessageHud.MessageType.Center, "$hud_itemstucktar", 0, null);
			return true;
		}
		this.Pickup(character);
		return true;
	}

	// Token: 0x06000B70 RID: 2928 RVA: 0x00054600 File Offset: 0x00052800
	public bool InTar()
	{
		if (this.m_body == null)
		{
			return false;
		}
		if (this.m_floating != null)
		{
			return this.m_floating.IsInTar();
		}
		Vector3 worldCenterOfMass = this.m_body.worldCenterOfMass;
		float liquidLevel = Floating.GetLiquidLevel(worldCenterOfMass, 1f, LiquidType.Tar);
		return worldCenterOfMass.y < liquidLevel;
	}

	// Token: 0x06000B71 RID: 2929 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000B72 RID: 2930 RVA: 0x00054658 File Offset: 0x00052858
	public void SetStack(int stack)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_itemData.m_stack = stack;
		if (this.m_itemData.m_stack > this.m_itemData.m_shared.m_maxStackSize)
		{
			this.m_itemData.m_stack = this.m_itemData.m_shared.m_maxStackSize;
		}
		this.Save();
	}

	// Token: 0x06000B73 RID: 2931 RVA: 0x000546CC File Offset: 0x000528CC
	public void Pickup(Humanoid character)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.CanPickup(true))
		{
			this.Load();
			character.Pickup(base.gameObject, true, true);
			this.Save();
			return;
		}
		this.m_pickupRequester = character;
		base.CancelInvoke("PickupUpdate");
		float num = 0.05f;
		base.InvokeRepeating("PickupUpdate", num, num);
		this.RequestOwn();
	}

	// Token: 0x06000B74 RID: 2932 RVA: 0x00054738 File Offset: 0x00052938
	public void RequestOwn()
	{
		if (Time.time - this.m_lastOwnerRequest < this.m_ownerRetryTimeout)
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			return;
		}
		this.m_lastOwnerRequest = Time.time;
		this.m_ownerRetryTimeout = Mathf.Min(0.2f * Mathf.Pow(2f, (float)this.m_ownerRetryCounter), 30f);
		this.m_ownerRetryCounter++;
		this.m_nview.InvokeRPC("RequestOwn", Array.Empty<object>());
	}

	// Token: 0x06000B75 RID: 2933 RVA: 0x000547C0 File Offset: 0x000529C0
	public bool RemoveOne()
	{
		if (!this.CanPickup(true))
		{
			this.RequestOwn();
			return false;
		}
		if (this.m_itemData.m_stack <= 1)
		{
			this.m_nview.Destroy();
			return true;
		}
		this.m_itemData.m_stack--;
		this.Save();
		return true;
	}

	// Token: 0x06000B76 RID: 2934 RVA: 0x00054813 File Offset: 0x00052A13
	public void OnPlayerDrop()
	{
		this.m_autoPickup = false;
	}

	// Token: 0x06000B77 RID: 2935 RVA: 0x0005481C File Offset: 0x00052A1C
	public bool CanPickup(bool autoPickupDelay = true)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return true;
		}
		if (autoPickupDelay && (double)(Time.time - this.m_spawnTime) < 0.5)
		{
			return false;
		}
		if (this.m_nview.IsOwner())
		{
			this.m_ownerRetryCounter = 0;
			this.m_ownerRetryTimeout = 0f;
		}
		return this.m_nview.IsOwner();
	}

	// Token: 0x06000B78 RID: 2936 RVA: 0x00054890 File Offset: 0x00052A90
	private void RPC_RequestOwn(long uid)
	{
		ZLog.Log(string.Concat(new string[]
		{
			"Player ",
			uid.ToString(),
			" wants to pickup ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.GetSessionID().ToString()
		}));
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().SetOwner(uid);
			return;
		}
		if (this.m_nview.GetZDO().GetOwner() == uid)
		{
			ZLog.Log("  but they are already the owner");
			return;
		}
		ZLog.Log("  but neither I nor the requesting player are the owners");
	}

	// Token: 0x06000B79 RID: 2937 RVA: 0x00054934 File Offset: 0x00052B34
	private void PickupUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.CanPickup(true))
		{
			ZLog.Log("Im finally the owner");
			base.CancelInvoke("PickupUpdate");
			this.Load();
			(this.m_pickupRequester as Player).Pickup(base.gameObject, true, true);
			this.Save();
			return;
		}
		ZLog.Log("Im still nto the owner");
	}

	// Token: 0x06000B7A RID: 2938 RVA: 0x000549A0 File Offset: 0x00052BA0
	private void Save()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			ItemDrop.SaveToZDO(this.m_itemData, this.m_nview.GetZDO());
		}
	}

	// Token: 0x06000B7B RID: 2939 RVA: 0x000549EC File Offset: 0x00052BEC
	public void Load()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo.DataRevision == this.m_loadedRevision)
		{
			return;
		}
		this.m_loadedRevision = zdo.DataRevision;
		ItemDrop.LoadFromZDO(this.m_itemData, zdo);
		this.SetQuality(this.m_itemData.m_quality);
	}

	// Token: 0x06000B7C RID: 2940 RVA: 0x00054A59 File Offset: 0x00052C59
	public void LoadFromExternalZDO(ZDO zdo)
	{
		ItemDrop.LoadFromZDO(this.m_itemData, zdo);
		ItemDrop.SaveToZDO(this.m_itemData, this.m_nview.GetZDO());
		this.SetQuality(this.m_itemData.m_quality);
	}

	// Token: 0x06000B7D RID: 2941 RVA: 0x00054A90 File Offset: 0x00052C90
	public static void SaveToZDO(ItemDrop.ItemData itemData, ZDO zdo)
	{
		zdo.Set(ZDOVars.s_durability, itemData.m_durability);
		zdo.Set(ZDOVars.s_stack, itemData.m_stack, false);
		zdo.Set(ZDOVars.s_quality, itemData.m_quality, false);
		zdo.Set(ZDOVars.s_variant, itemData.m_variant, false);
		zdo.Set(ZDOVars.s_crafterID, itemData.m_crafterID);
		zdo.Set(ZDOVars.s_crafterName, itemData.m_crafterName);
		zdo.Set(ZDOVars.s_dataCount, itemData.m_customData.Count, false);
		int num = 0;
		foreach (KeyValuePair<string, string> keyValuePair in itemData.m_customData)
		{
			zdo.Set(string.Format("data_{0}", num), keyValuePair.Key);
			zdo.Set(string.Format("data__{0}", num++), keyValuePair.Value);
		}
	}

	// Token: 0x06000B7E RID: 2942 RVA: 0x00054B9C File Offset: 0x00052D9C
	private static void LoadFromZDO(ItemDrop.ItemData itemData, ZDO zdo)
	{
		itemData.m_durability = zdo.GetFloat(ZDOVars.s_durability, itemData.m_durability);
		itemData.m_stack = zdo.GetInt(ZDOVars.s_stack, itemData.m_stack);
		itemData.m_quality = zdo.GetInt(ZDOVars.s_quality, itemData.m_quality);
		itemData.m_variant = zdo.GetInt(ZDOVars.s_variant, itemData.m_variant);
		itemData.m_crafterID = zdo.GetLong(ZDOVars.s_crafterID, itemData.m_crafterID);
		itemData.m_crafterName = zdo.GetString(ZDOVars.s_crafterName, itemData.m_crafterName);
		int @int = zdo.GetInt(ZDOVars.s_dataCount, 0);
		itemData.m_customData.Clear();
		for (int i = 0; i < @int; i++)
		{
			itemData.m_customData[zdo.GetString(string.Format("data_{0}", i), "")] = zdo.GetString(string.Format("data__{0}", i), "");
		}
	}

	// Token: 0x06000B7F RID: 2943 RVA: 0x00054C98 File Offset: 0x00052E98
	public static void SaveToZDO(int index, ItemDrop.ItemData itemData, ZDO zdo)
	{
		zdo.Set(index.ToString() + "_durability", itemData.m_durability);
		zdo.Set(index.ToString() + "_stack", itemData.m_stack);
		zdo.Set(index.ToString() + "_quality", itemData.m_quality);
		zdo.Set(index.ToString() + "_variant", itemData.m_variant);
		zdo.Set(index.ToString() + "_crafterID", itemData.m_crafterID);
		zdo.Set(index.ToString() + "_crafterName", itemData.m_crafterName);
		zdo.Set(index.ToString() + "_dataCount", itemData.m_customData.Count);
		int num = 0;
		foreach (KeyValuePair<string, string> keyValuePair in itemData.m_customData)
		{
			zdo.Set(string.Format("{0}_data_{1}", index, num), keyValuePair.Key);
			zdo.Set(string.Format("{0}_data__{1}", index, num++), keyValuePair.Value);
		}
	}

	// Token: 0x06000B80 RID: 2944 RVA: 0x00054E00 File Offset: 0x00053000
	public static void LoadFromZDO(int index, ItemDrop.ItemData itemData, ZDO zdo)
	{
		itemData.m_durability = zdo.GetFloat(index.ToString() + "_durability", itemData.m_durability);
		itemData.m_stack = zdo.GetInt(index.ToString() + "_stack", itemData.m_stack);
		itemData.m_quality = zdo.GetInt(index.ToString() + "_quality", itemData.m_quality);
		itemData.m_variant = zdo.GetInt(index.ToString() + "_variant", itemData.m_variant);
		itemData.m_crafterID = zdo.GetLong(index.ToString() + "_crafterID", itemData.m_crafterID);
		itemData.m_crafterName = zdo.GetString(index.ToString() + "_crafterName", itemData.m_crafterName);
		int @int = zdo.GetInt(index.ToString() + "_dataCount", 0);
		for (int i = 0; i < @int; i++)
		{
			itemData.m_customData[zdo.GetString(string.Format("{0}_data_{1}", index, i), "")] = zdo.GetString(string.Format("{0}_data__{1}", index, i), "");
		}
	}

	// Token: 0x06000B81 RID: 2945 RVA: 0x00054F54 File Offset: 0x00053154
	public static ItemDrop DropItem(ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation)
	{
		ItemDrop component = UnityEngine.Object.Instantiate<GameObject>(item.m_dropPrefab, position, rotation).GetComponent<ItemDrop>();
		component.m_itemData = item.Clone();
		if (component.m_itemData.m_quality > 1)
		{
			component.SetQuality(component.m_itemData.m_quality);
		}
		if (amount > 0)
		{
			component.m_itemData.m_stack = amount;
		}
		if (component.m_onDrop != null)
		{
			component.m_onDrop(component);
		}
		component.Save();
		return component;
	}

	// Token: 0x06000B82 RID: 2946 RVA: 0x00054FCA File Offset: 0x000531CA
	public void SetQuality(int quality)
	{
		this.m_itemData.m_quality = quality;
		base.transform.localScale = this.m_itemData.GetScale();
	}

	// Token: 0x06000B83 RID: 2947 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x06000B84 RID: 2948 RVA: 0x00054FEE File Offset: 0x000531EE
	public int NameHash()
	{
		return this.m_nameHash;
	}

	// Token: 0x04000D6C RID: 3436
	private static readonly List<ItemDrop> s_instances = new List<ItemDrop>();

	// Token: 0x04000D6D RID: 3437
	private int m_myIndex = -1;

	// Token: 0x04000D6E RID: 3438
	public bool m_autoPickup = true;

	// Token: 0x04000D6F RID: 3439
	public bool m_autoDestroy = true;

	// Token: 0x04000D70 RID: 3440
	public ItemDrop.ItemData m_itemData = new ItemDrop.ItemData();

	// Token: 0x04000D71 RID: 3441
	[HideInInspector]
	public Action<ItemDrop> m_onDrop;

	// Token: 0x04000D72 RID: 3442
	private int m_nameHash;

	// Token: 0x04000D73 RID: 3443
	private Floating m_floating;

	// Token: 0x04000D74 RID: 3444
	private Rigidbody m_body;

	// Token: 0x04000D75 RID: 3445
	private ZNetView m_nview;

	// Token: 0x04000D76 RID: 3446
	private Character m_pickupRequester;

	// Token: 0x04000D77 RID: 3447
	private float m_lastOwnerRequest;

	// Token: 0x04000D78 RID: 3448
	private int m_ownerRetryCounter;

	// Token: 0x04000D79 RID: 3449
	private float m_ownerRetryTimeout;

	// Token: 0x04000D7A RID: 3450
	private float m_spawnTime;

	// Token: 0x04000D7B RID: 3451
	private uint m_loadedRevision = uint.MaxValue;

	// Token: 0x04000D7C RID: 3452
	private const double c_AutoDestroyTimeout = 3600.0;

	// Token: 0x04000D7D RID: 3453
	private const double c_AutoPickupDelay = 0.5;

	// Token: 0x04000D7E RID: 3454
	private const float c_AutoDespawnBaseMinAltitude = -2f;

	// Token: 0x04000D7F RID: 3455
	private const int c_AutoStackThreshold = 200;

	// Token: 0x04000D80 RID: 3456
	private const float c_AutoStackRange = 4f;

	// Token: 0x04000D81 RID: 3457
	private bool m_haveAutoStacked;

	// Token: 0x04000D82 RID: 3458
	private static int s_itemMask = 0;

	// Token: 0x02000126 RID: 294
	[Serializable]
	public class ItemData
	{
		// Token: 0x06000B88 RID: 2952 RVA: 0x00055085 File Offset: 0x00053285
		public ItemDrop.ItemData Clone()
		{
			ItemDrop.ItemData itemData = base.MemberwiseClone() as ItemDrop.ItemData;
			itemData.m_customData = new Dictionary<string, string>(this.m_customData);
			return itemData;
		}

		// Token: 0x06000B89 RID: 2953 RVA: 0x000550A4 File Offset: 0x000532A4
		public bool IsEquipable()
		{
			return this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility;
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x00055180 File Offset: 0x00053380
		public bool IsWeapon()
		{
			return this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch;
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x000551D8 File Offset: 0x000533D8
		public bool IsTwoHanded()
		{
			return this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow;
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x00055208 File Offset: 0x00053408
		public bool HavePrimaryAttack()
		{
			return !string.IsNullOrEmpty(this.m_shared.m_attack.m_attackAnimation);
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x00055222 File Offset: 0x00053422
		public bool HaveSecondaryAttack()
		{
			return !string.IsNullOrEmpty(this.m_shared.m_secondaryAttack.m_attackAnimation);
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0005523C File Offset: 0x0005343C
		public float GetArmor()
		{
			return this.GetArmor(this.m_quality);
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0005524A File Offset: 0x0005344A
		public float GetArmor(int quality)
		{
			return this.m_shared.m_armor + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_armorPerLevel;
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x0005526E File Offset: 0x0005346E
		public int GetValue()
		{
			return this.m_shared.m_value * this.m_stack;
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x00055284 File Offset: 0x00053484
		public float GetWeight()
		{
			float num = this.m_shared.m_weight * (float)this.m_stack;
			if (this.m_shared.m_scaleWeightByQuality != 0f && this.m_quality != 1)
			{
				num += num * (float)(this.m_quality - 1) * this.m_shared.m_scaleWeightByQuality;
			}
			return num;
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x000552DB File Offset: 0x000534DB
		public HitData.DamageTypes GetDamage()
		{
			return this.GetDamage(this.m_quality);
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x000552EC File Offset: 0x000534EC
		public float GetDurabilityPercentage()
		{
			float maxDurability = this.GetMaxDurability();
			if (maxDurability == 0f)
			{
				return 1f;
			}
			return Mathf.Clamp01(this.m_durability / maxDurability);
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x0005531B File Offset: 0x0005351B
		public float GetMaxDurability()
		{
			return this.GetMaxDurability(this.m_quality);
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x00055329 File Offset: 0x00053529
		public float GetMaxDurability(int quality)
		{
			return this.m_shared.m_maxDurability + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_durabilityPerLevel;
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x00055350 File Offset: 0x00053550
		public HitData.DamageTypes GetDamage(int quality)
		{
			HitData.DamageTypes damages = this.m_shared.m_damages;
			if (quality > 1)
			{
				damages.Add(this.m_shared.m_damagesPerLevel, quality - 1);
			}
			return damages;
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x00055383 File Offset: 0x00053583
		public float GetBaseBlockPower()
		{
			return this.GetBaseBlockPower(this.m_quality);
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x00055391 File Offset: 0x00053591
		public float GetBaseBlockPower(int quality)
		{
			return this.m_shared.m_blockPower + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_blockPowerPerLevel;
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x000553B5 File Offset: 0x000535B5
		public float GetBlockPower(float skillFactor)
		{
			return this.GetBlockPower(this.m_quality, skillFactor);
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x000553C4 File Offset: 0x000535C4
		public float GetBlockPower(int quality, float skillFactor)
		{
			float baseBlockPower = this.GetBaseBlockPower(quality);
			return baseBlockPower + baseBlockPower * skillFactor * 0.5f;
		}

		// Token: 0x06000B9B RID: 2971 RVA: 0x000553D8 File Offset: 0x000535D8
		public float GetBlockPowerTooltip(int quality)
		{
			if (Player.m_localPlayer == null)
			{
				return 0f;
			}
			float skillFactor = Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking);
			return this.GetBlockPower(quality, skillFactor);
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x0005540C File Offset: 0x0005360C
		public float GetDrawStaminaDrain()
		{
			if (this.m_shared.m_attack.m_drawStaminaDrain <= 0f)
			{
				return 0f;
			}
			float drawStaminaDrain = this.m_shared.m_attack.m_drawStaminaDrain;
			float skillFactor = Player.m_localPlayer.GetSkillFactor(this.m_shared.m_skillType);
			return drawStaminaDrain - drawStaminaDrain * 0.33f * skillFactor;
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x00055468 File Offset: 0x00053668
		public float GetWeaponLoadingTime()
		{
			if (this.m_shared.m_attack.m_requiresReload)
			{
				float skillFactor = Player.m_localPlayer.GetSkillFactor(this.m_shared.m_skillType);
				return Mathf.Lerp(this.m_shared.m_attack.m_reloadTime, this.m_shared.m_attack.m_reloadTime * 0.5f, skillFactor);
			}
			return 1f;
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x000554CF File Offset: 0x000536CF
		public float GetDeflectionForce()
		{
			return this.GetDeflectionForce(this.m_quality);
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x000554DD File Offset: 0x000536DD
		public float GetDeflectionForce(int quality)
		{
			return this.m_shared.m_deflectionForce + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_deflectionForcePerLevel;
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x00055501 File Offset: 0x00053701
		public Vector3 GetScale()
		{
			return this.GetScale((float)this.m_quality);
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x00055510 File Offset: 0x00053710
		public Vector3 GetScale(float quality)
		{
			float num = 1f + (quality - 1f) * this.m_shared.m_scaleByQuality;
			return new Vector3(num, num, num);
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x00055532 File Offset: 0x00053732
		public string GetTooltip()
		{
			return ItemDrop.ItemData.GetTooltip(this, this.m_quality, false);
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x00055541 File Offset: 0x00053741
		public Sprite GetIcon()
		{
			return this.m_shared.m_icons[this.m_variant];
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x00055558 File Offset: 0x00053758
		private static void AddHandedTip(ItemDrop.ItemData item, StringBuilder text)
		{
			ItemDrop.ItemData.ItemType itemType = item.m_shared.m_itemType;
			if (itemType <= ItemDrop.ItemData.ItemType.TwoHandedWeapon)
			{
				switch (itemType)
				{
				case ItemDrop.ItemData.ItemType.OneHandedWeapon:
				case ItemDrop.ItemData.ItemType.Shield:
					break;
				case ItemDrop.ItemData.ItemType.Bow:
					goto IL_48;
				default:
					if (itemType != ItemDrop.ItemData.ItemType.TwoHandedWeapon)
					{
						return;
					}
					goto IL_48;
				}
			}
			else if (itemType != ItemDrop.ItemData.ItemType.Torch)
			{
				if (itemType != ItemDrop.ItemData.ItemType.Tool && itemType != ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
				{
					return;
				}
				goto IL_48;
			}
			text.Append("\n$item_onehanded");
			return;
			IL_48:
			text.Append("\n$item_twohanded");
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x000555BC File Offset: 0x000537BC
		private static void AddBlockTooltip(ItemDrop.ItemData item, int qualityLevel, StringBuilder text)
		{
			text.AppendFormat("\n$item_blockarmor: <color=orange>{0}</color> <color=yellow>({1})</color>", item.GetBaseBlockPower(qualityLevel), item.GetBlockPowerTooltip(qualityLevel).ToString("0"));
			text.AppendFormat("\n$item_blockforce: <color=orange>{0}</color>", item.GetDeflectionForce(qualityLevel));
			if (item.m_shared.m_timedBlockBonus > 1f)
			{
				text.AppendFormat("\n$item_parrybonus: <color=orange>{0}x</color>", item.m_shared.m_timedBlockBonus);
			}
			string damageModifiersTooltipString = SE_Stats.GetDamageModifiersTooltipString(item.m_shared.m_damageModifiers);
			if (damageModifiersTooltipString.Length > 0)
			{
				text.Append(damageModifiersTooltipString);
			}
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x00055660 File Offset: 0x00053860
		public static string GetTooltip(ItemDrop.ItemData item, int qualityLevel, bool crafting)
		{
			Player localPlayer = Player.m_localPlayer;
			ItemDrop.ItemData.m_stringBuilder.Clear();
			ItemDrop.ItemData.m_stringBuilder.Append(item.m_shared.m_description);
			ItemDrop.ItemData.m_stringBuilder.Append("\n");
			if (item.m_shared.m_dlc.Length > 0)
			{
				ItemDrop.ItemData.m_stringBuilder.Append("\n<color=#00FFFF>$item_dlc</color>");
			}
			ItemDrop.ItemData.AddHandedTip(item, ItemDrop.ItemData.m_stringBuilder);
			if (item.m_crafterID != 0L)
			{
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_crafter: <color=orange>{0}</color>", item.m_crafterName);
			}
			if (!item.m_shared.m_teleportable)
			{
				ItemDrop.ItemData.m_stringBuilder.Append("\n<color=orange>$item_noteleport</color>");
			}
			if (item.m_shared.m_value > 0)
			{
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_value: <color=orange>{0}  ({1})</color>", item.GetValue(), item.m_shared.m_value);
			}
			ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_weight: <color=orange>{0}</color>", item.GetWeight().ToString("0.0"));
			if (item.m_shared.m_maxQuality > 1 && !crafting)
			{
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_quality: <color=orange>{0}</color>", qualityLevel);
			}
			if (item.m_shared.m_useDurability)
			{
				if (crafting)
				{
					float maxDurability = item.GetMaxDurability(qualityLevel);
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_durability: <color=orange>{0}</color>", maxDurability);
				}
				else
				{
					float maxDurability2 = item.GetMaxDurability(qualityLevel);
					float durability = item.m_durability;
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_durability: <color=orange>{0}%</color> <color=yellow>({1}/{2})</color>", (item.GetDurabilityPercentage() * 100f).ToString("0"), durability.ToString("0"), maxDurability2.ToString("0"));
				}
				if (item.m_shared.m_canBeReparied && !crafting)
				{
					Recipe recipe = ObjectDB.instance.GetRecipe(item);
					if (recipe != null)
					{
						int minStationLevel = recipe.m_minStationLevel;
						ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_repairlevel: <color=orange>{0}</color>", minStationLevel.ToString());
					}
				}
			}
			switch (item.m_shared.m_itemType)
			{
			case ItemDrop.ItemData.ItemType.Consumable:
				if (item.m_shared.m_food > 0f || item.m_shared.m_foodStamina > 0f || item.m_shared.m_foodEitr > 0f)
				{
					float maxHealth = localPlayer.GetMaxHealth();
					float maxStamina = localPlayer.GetMaxStamina();
					float maxEitr = localPlayer.GetMaxEitr();
					if (item.m_shared.m_food > 0f)
					{
						ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_food_health: <color=#ff8080ff>{0}</color>  ($item_current:<color=yellow>{1}</color>)", item.m_shared.m_food, maxHealth.ToString("0"));
					}
					if (item.m_shared.m_foodStamina > 0f)
					{
						ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_food_stamina: <color=#ffff80ff>{0}</color>  ($item_current:<color=yellow>{1}</color>)", item.m_shared.m_foodStamina, maxStamina.ToString("0"));
					}
					if (item.m_shared.m_foodEitr > 0f)
					{
						ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_food_eitr: <color=#9090ffff>{0}</color>  ($item_current:<color=yellow>{1}</color>)", item.m_shared.m_foodEitr, maxEitr.ToString("0"));
					}
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_food_duration: <color=orange>{0}</color>", ItemDrop.ItemData.GetDurationString(item.m_shared.m_foodBurnTime));
					if (item.m_shared.m_foodRegen > 0f)
					{
						ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_food_regen: <color=orange>{0} hp/tick</color>", item.m_shared.m_foodRegen);
					}
				}
				break;
			case ItemDrop.ItemData.ItemType.OneHandedWeapon:
			case ItemDrop.ItemData.ItemType.Bow:
			case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
			case ItemDrop.ItemData.ItemType.Torch:
			case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
			{
				ItemDrop.ItemData.m_stringBuilder.Append(item.GetDamage(qualityLevel).GetTooltipString(item.m_shared.m_skillType));
				if (item.m_shared.m_attack.m_attackStamina > 0f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_staminause: <color=orange>{0}</color>", item.m_shared.m_attack.m_attackStamina);
				}
				if (item.m_shared.m_attack.m_attackEitr > 0f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_eitruse: <color=orange>{0}</color>", item.m_shared.m_attack.m_attackEitr);
				}
				if (item.m_shared.m_attack.m_attackHealth > 0f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_healthuse: <color=orange>{0}</color>", item.m_shared.m_attack.m_attackHealth);
				}
				if (item.m_shared.m_attack.m_attackHealthPercentage > 0f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_healthuse: <color=orange>{0}%</color>", item.m_shared.m_attack.m_attackHealthPercentage.ToString("0.0"));
				}
				if (item.m_shared.m_attack.m_drawStaminaDrain > 0f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_staminahold: <color=orange>{0}</color>/s", item.m_shared.m_attack.m_drawStaminaDrain);
				}
				ItemDrop.ItemData.AddBlockTooltip(item, qualityLevel, ItemDrop.ItemData.m_stringBuilder);
				if (item.m_shared.m_attackForce > 0f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
				}
				if (item.m_shared.m_backstabBonus > 1f)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_backstab: <color=orange>{0}x</color>", item.m_shared.m_backstabBonus);
				}
				if (item.m_shared.m_tamedOnly)
				{
					ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n<color=orange>$item_tamedonly</color>", Array.Empty<object>());
				}
				string projectileTooltip = item.GetProjectileTooltip(qualityLevel);
				if (projectileTooltip.Length > 0 && item.m_shared.m_projectileToolTip)
				{
					ItemDrop.ItemData.m_stringBuilder.Append("\n\n");
					ItemDrop.ItemData.m_stringBuilder.Append(projectileTooltip);
				}
				break;
			}
			case ItemDrop.ItemData.ItemType.Shield:
				ItemDrop.ItemData.AddBlockTooltip(item, qualityLevel, ItemDrop.ItemData.m_stringBuilder);
				break;
			case ItemDrop.ItemData.ItemType.Helmet:
			case ItemDrop.ItemData.ItemType.Chest:
			case ItemDrop.ItemData.ItemType.Legs:
			case ItemDrop.ItemData.ItemType.Shoulder:
			{
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_armor: <color=orange>{0}</color>", item.GetArmor(qualityLevel));
				string damageModifiersTooltipString = SE_Stats.GetDamageModifiersTooltipString(item.m_shared.m_damageModifiers);
				if (damageModifiersTooltipString.Length > 0)
				{
					ItemDrop.ItemData.m_stringBuilder.Append(damageModifiersTooltipString);
				}
				break;
			}
			case ItemDrop.ItemData.ItemType.Ammo:
			case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
				ItemDrop.ItemData.m_stringBuilder.Append(item.GetDamage(qualityLevel).GetTooltipString(item.m_shared.m_skillType));
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
				break;
			}
			float skillLevel = Player.m_localPlayer.GetSkillLevel(item.m_shared.m_skillType);
			string statusEffectTooltip = item.GetStatusEffectTooltip(qualityLevel, skillLevel);
			if (statusEffectTooltip.Length > 0)
			{
				ItemDrop.ItemData.m_stringBuilder.Append("\n\n");
				ItemDrop.ItemData.m_stringBuilder.Append(statusEffectTooltip);
			}
			if (item.m_shared.m_eitrRegenModifier > 0f && localPlayer != null)
			{
				float equipmentEitrRegenModifier = localPlayer.GetEquipmentEitrRegenModifier();
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_eitrregen_modifier: <color=orange>{0}%</color> ($item_total:<color=yellow>{1}%</color>)", (item.m_shared.m_eitrRegenModifier * 100f).ToString("+0;-0"), (equipmentEitrRegenModifier * 100f).ToString("+0;-0"));
			}
			if (item.m_shared.m_movementModifier != 0f && localPlayer != null)
			{
				float equipmentMovementModifier = localPlayer.GetEquipmentMovementModifier();
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_movement_modifier: <color=orange>{0}%</color> ($item_total:<color=yellow>{1}%</color>)", (item.m_shared.m_movementModifier * 100f).ToString("+0;-0"), (equipmentMovementModifier * 100f).ToString("+0;-0"));
			}
			string setStatusEffectTooltip = item.GetSetStatusEffectTooltip(qualityLevel, skillLevel);
			if (setStatusEffectTooltip.Length > 0)
			{
				ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n\n$item_seteffect (<color=orange>{0}</color> $item_parts):<color=orange>{1}</color>\n{2}", item.m_shared.m_setSize, item.m_shared.m_setStatusEffect.m_name, setStatusEffectTooltip);
			}
			return ItemDrop.ItemData.m_stringBuilder.ToString();
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x00055E6C File Offset: 0x0005406C
		public static string GetDurationString(float time)
		{
			int num = Mathf.CeilToInt(time);
			int num2 = (int)((float)num / 60f);
			int num3 = Mathf.Max(0, num - num2 * 60);
			if (num2 > 0 && num3 > 0)
			{
				return num2.ToString() + "m " + num3.ToString() + "s";
			}
			if (num2 > 0)
			{
				return num2.ToString() + "m ";
			}
			return num3.ToString() + "s";
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x00055EE4 File Offset: 0x000540E4
		private string GetStatusEffectTooltip(int quality, float skillLevel)
		{
			if (this.m_shared.m_attackStatusEffect)
			{
				this.m_shared.m_attackStatusEffect.SetLevel(quality, skillLevel);
				return "<color=orange>" + this.m_shared.m_attackStatusEffect.m_name + "</color>\n" + this.m_shared.m_attackStatusEffect.GetTooltipString();
			}
			if (this.m_shared.m_consumeStatusEffect)
			{
				this.m_shared.m_consumeStatusEffect.SetLevel(quality, skillLevel);
				return "<color=orange>" + this.m_shared.m_consumeStatusEffect.m_name + "</color>\n" + this.m_shared.m_consumeStatusEffect.GetTooltipString();
			}
			if (this.m_shared.m_equipStatusEffect)
			{
				this.m_shared.m_equipStatusEffect.SetLevel(quality, skillLevel);
				return "<color=orange>" + this.m_shared.m_equipStatusEffect.m_name + "</color>\n" + this.m_shared.m_equipStatusEffect.GetTooltipString();
			}
			return "";
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x00055FF4 File Offset: 0x000541F4
		private string GetEquipStatusEffectTooltip(int quality, float skillLevel)
		{
			if (this.m_shared.m_equipStatusEffect)
			{
				StatusEffect equipStatusEffect = this.m_shared.m_equipStatusEffect;
				this.m_shared.m_equipStatusEffect.SetLevel(quality, skillLevel);
				if (equipStatusEffect != null)
				{
					return equipStatusEffect.GetTooltipString();
				}
			}
			return "";
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x00056048 File Offset: 0x00054248
		private string GetSetStatusEffectTooltip(int quality, float skillLevel)
		{
			if (this.m_shared.m_setStatusEffect)
			{
				StatusEffect setStatusEffect = this.m_shared.m_setStatusEffect;
				this.m_shared.m_setStatusEffect.SetLevel(quality, skillLevel);
				if (setStatusEffect != null)
				{
					return setStatusEffect.GetTooltipString();
				}
			}
			return "";
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x0005609C File Offset: 0x0005429C
		private string GetProjectileTooltip(int itemQuality)
		{
			string text = "";
			if (this.m_shared.m_attack.m_attackProjectile)
			{
				IProjectile component = this.m_shared.m_attack.m_attackProjectile.GetComponent<IProjectile>();
				if (component != null)
				{
					text += component.GetTooltipString(itemQuality);
				}
			}
			if (this.m_shared.m_spawnOnHit)
			{
				IProjectile component2 = this.m_shared.m_spawnOnHit.GetComponent<IProjectile>();
				if (component2 != null)
				{
					text += component2.GetTooltipString(itemQuality);
				}
			}
			return text;
		}

		// Token: 0x04000D83 RID: 3459
		private static StringBuilder m_stringBuilder = new StringBuilder(256);

		// Token: 0x04000D84 RID: 3460
		public int m_stack = 1;

		// Token: 0x04000D85 RID: 3461
		public float m_durability = 100f;

		// Token: 0x04000D86 RID: 3462
		public int m_quality = 1;

		// Token: 0x04000D87 RID: 3463
		public int m_variant;

		// Token: 0x04000D88 RID: 3464
		public ItemDrop.ItemData.SharedData m_shared;

		// Token: 0x04000D89 RID: 3465
		[NonSerialized]
		public long m_crafterID;

		// Token: 0x04000D8A RID: 3466
		[NonSerialized]
		public string m_crafterName = "";

		// Token: 0x04000D8B RID: 3467
		public Dictionary<string, string> m_customData = new Dictionary<string, string>();

		// Token: 0x04000D8C RID: 3468
		[NonSerialized]
		public Vector2i m_gridPos = Vector2i.zero;

		// Token: 0x04000D8D RID: 3469
		[NonSerialized]
		public bool m_equipped;

		// Token: 0x04000D8E RID: 3470
		[NonSerialized]
		public GameObject m_dropPrefab;

		// Token: 0x04000D8F RID: 3471
		[NonSerialized]
		public float m_lastAttackTime;

		// Token: 0x04000D90 RID: 3472
		[NonSerialized]
		public GameObject m_lastProjectile;

		// Token: 0x02000127 RID: 295
		public enum ItemType
		{
			// Token: 0x04000D92 RID: 3474
			None,
			// Token: 0x04000D93 RID: 3475
			Material,
			// Token: 0x04000D94 RID: 3476
			Consumable,
			// Token: 0x04000D95 RID: 3477
			OneHandedWeapon,
			// Token: 0x04000D96 RID: 3478
			Bow,
			// Token: 0x04000D97 RID: 3479
			Shield,
			// Token: 0x04000D98 RID: 3480
			Helmet,
			// Token: 0x04000D99 RID: 3481
			Chest,
			// Token: 0x04000D9A RID: 3482
			Ammo = 9,
			// Token: 0x04000D9B RID: 3483
			Customization,
			// Token: 0x04000D9C RID: 3484
			Legs,
			// Token: 0x04000D9D RID: 3485
			Hands,
			// Token: 0x04000D9E RID: 3486
			Trophy,
			// Token: 0x04000D9F RID: 3487
			TwoHandedWeapon,
			// Token: 0x04000DA0 RID: 3488
			Torch,
			// Token: 0x04000DA1 RID: 3489
			Misc,
			// Token: 0x04000DA2 RID: 3490
			Shoulder,
			// Token: 0x04000DA3 RID: 3491
			Utility,
			// Token: 0x04000DA4 RID: 3492
			Tool,
			// Token: 0x04000DA5 RID: 3493
			Attach_Atgeir,
			// Token: 0x04000DA6 RID: 3494
			Fish,
			// Token: 0x04000DA7 RID: 3495
			TwoHandedWeaponLeft,
			// Token: 0x04000DA8 RID: 3496
			AmmoNonEquipable
		}

		// Token: 0x02000128 RID: 296
		public enum AnimationState
		{
			// Token: 0x04000DAA RID: 3498
			Unarmed,
			// Token: 0x04000DAB RID: 3499
			OneHanded,
			// Token: 0x04000DAC RID: 3500
			TwoHandedClub,
			// Token: 0x04000DAD RID: 3501
			Bow,
			// Token: 0x04000DAE RID: 3502
			Shield,
			// Token: 0x04000DAF RID: 3503
			Torch,
			// Token: 0x04000DB0 RID: 3504
			LeftTorch,
			// Token: 0x04000DB1 RID: 3505
			Atgeir,
			// Token: 0x04000DB2 RID: 3506
			TwoHandedAxe,
			// Token: 0x04000DB3 RID: 3507
			FishingRod,
			// Token: 0x04000DB4 RID: 3508
			Crossbow,
			// Token: 0x04000DB5 RID: 3509
			Knives,
			// Token: 0x04000DB6 RID: 3510
			Staves,
			// Token: 0x04000DB7 RID: 3511
			Greatsword,
			// Token: 0x04000DB8 RID: 3512
			MagicItem
		}

		// Token: 0x02000129 RID: 297
		public enum AiTarget
		{
			// Token: 0x04000DBA RID: 3514
			Enemy,
			// Token: 0x04000DBB RID: 3515
			FriendHurt,
			// Token: 0x04000DBC RID: 3516
			Friend
		}

		// Token: 0x0200012A RID: 298
		[Serializable]
		public class SharedData
		{
			// Token: 0x04000DBD RID: 3517
			public string m_name = "";

			// Token: 0x04000DBE RID: 3518
			public string m_dlc = "";

			// Token: 0x04000DBF RID: 3519
			public ItemDrop.ItemData.ItemType m_itemType = ItemDrop.ItemData.ItemType.Misc;

			// Token: 0x04000DC0 RID: 3520
			public Sprite[] m_icons = Array.Empty<Sprite>();

			// Token: 0x04000DC1 RID: 3521
			public ItemDrop.ItemData.ItemType m_attachOverride;

			// Token: 0x04000DC2 RID: 3522
			[TextArea]
			public string m_description = "";

			// Token: 0x04000DC3 RID: 3523
			public int m_maxStackSize = 1;

			// Token: 0x04000DC4 RID: 3524
			public bool m_autoStack = true;

			// Token: 0x04000DC5 RID: 3525
			public int m_maxQuality = 1;

			// Token: 0x04000DC6 RID: 3526
			public float m_scaleByQuality;

			// Token: 0x04000DC7 RID: 3527
			public float m_weight = 1f;

			// Token: 0x04000DC8 RID: 3528
			public float m_scaleWeightByQuality;

			// Token: 0x04000DC9 RID: 3529
			public int m_value;

			// Token: 0x04000DCA RID: 3530
			public bool m_teleportable = true;

			// Token: 0x04000DCB RID: 3531
			public bool m_questItem;

			// Token: 0x04000DCC RID: 3532
			public float m_equipDuration = 1f;

			// Token: 0x04000DCD RID: 3533
			public int m_variants;

			// Token: 0x04000DCE RID: 3534
			public Vector2Int m_trophyPos = Vector2Int.zero;

			// Token: 0x04000DCF RID: 3535
			public PieceTable m_buildPieces;

			// Token: 0x04000DD0 RID: 3536
			public bool m_centerCamera;

			// Token: 0x04000DD1 RID: 3537
			public string m_setName = "";

			// Token: 0x04000DD2 RID: 3538
			public int m_setSize;

			// Token: 0x04000DD3 RID: 3539
			public StatusEffect m_setStatusEffect;

			// Token: 0x04000DD4 RID: 3540
			public StatusEffect m_equipStatusEffect;

			// Token: 0x04000DD5 RID: 3541
			[Header("Stat modifiers")]
			public float m_movementModifier;

			// Token: 0x04000DD6 RID: 3542
			public float m_eitrRegenModifier;

			// Token: 0x04000DD7 RID: 3543
			[Header("Food settings")]
			public float m_food;

			// Token: 0x04000DD8 RID: 3544
			public float m_foodStamina;

			// Token: 0x04000DD9 RID: 3545
			public float m_foodEitr;

			// Token: 0x04000DDA RID: 3546
			public float m_foodBurnTime;

			// Token: 0x04000DDB RID: 3547
			public float m_foodRegen;

			// Token: 0x04000DDC RID: 3548
			[Header("Armor settings")]
			public Material m_armorMaterial;

			// Token: 0x04000DDD RID: 3549
			public bool m_helmetHideHair = true;

			// Token: 0x04000DDE RID: 3550
			public bool m_helmetHideBeard;

			// Token: 0x04000DDF RID: 3551
			public float m_armor = 10f;

			// Token: 0x04000DE0 RID: 3552
			public float m_armorPerLevel = 1f;

			// Token: 0x04000DE1 RID: 3553
			public List<HitData.DamageModPair> m_damageModifiers = new List<HitData.DamageModPair>();

			// Token: 0x04000DE2 RID: 3554
			[Header("Shield settings")]
			public float m_blockPower = 10f;

			// Token: 0x04000DE3 RID: 3555
			public float m_blockPowerPerLevel;

			// Token: 0x04000DE4 RID: 3556
			public float m_deflectionForce;

			// Token: 0x04000DE5 RID: 3557
			public float m_deflectionForcePerLevel;

			// Token: 0x04000DE6 RID: 3558
			public float m_timedBlockBonus = 1.5f;

			// Token: 0x04000DE7 RID: 3559
			[Header("Weapon")]
			public ItemDrop.ItemData.AnimationState m_animationState = ItemDrop.ItemData.AnimationState.OneHanded;

			// Token: 0x04000DE8 RID: 3560
			public Skills.SkillType m_skillType = Skills.SkillType.Swords;

			// Token: 0x04000DE9 RID: 3561
			public int m_toolTier;

			// Token: 0x04000DEA RID: 3562
			public HitData.DamageTypes m_damages;

			// Token: 0x04000DEB RID: 3563
			public HitData.DamageTypes m_damagesPerLevel;

			// Token: 0x04000DEC RID: 3564
			public float m_attackForce = 30f;

			// Token: 0x04000DED RID: 3565
			public float m_backstabBonus = 4f;

			// Token: 0x04000DEE RID: 3566
			public bool m_dodgeable;

			// Token: 0x04000DEF RID: 3567
			public bool m_blockable;

			// Token: 0x04000DF0 RID: 3568
			public bool m_tamedOnly;

			// Token: 0x04000DF1 RID: 3569
			public bool m_alwaysRotate;

			// Token: 0x04000DF2 RID: 3570
			public StatusEffect m_attackStatusEffect;

			// Token: 0x04000DF3 RID: 3571
			public GameObject m_spawnOnHit;

			// Token: 0x04000DF4 RID: 3572
			public GameObject m_spawnOnHitTerrain;

			// Token: 0x04000DF5 RID: 3573
			public bool m_projectileToolTip = true;

			// Token: 0x04000DF6 RID: 3574
			[Header("Ammo")]
			public string m_ammoType = "";

			// Token: 0x04000DF7 RID: 3575
			[Header("Attacks")]
			public Attack m_attack;

			// Token: 0x04000DF8 RID: 3576
			public Attack m_secondaryAttack;

			// Token: 0x04000DF9 RID: 3577
			[Header("Durability")]
			public bool m_useDurability;

			// Token: 0x04000DFA RID: 3578
			public bool m_destroyBroken = true;

			// Token: 0x04000DFB RID: 3579
			public bool m_canBeReparied = true;

			// Token: 0x04000DFC RID: 3580
			public float m_maxDurability = 100f;

			// Token: 0x04000DFD RID: 3581
			public float m_durabilityPerLevel = 50f;

			// Token: 0x04000DFE RID: 3582
			public float m_useDurabilityDrain = 1f;

			// Token: 0x04000DFF RID: 3583
			public float m_durabilityDrain;

			// Token: 0x04000E00 RID: 3584
			[Header("AI")]
			public float m_aiAttackRange = 2f;

			// Token: 0x04000E01 RID: 3585
			public float m_aiAttackRangeMin;

			// Token: 0x04000E02 RID: 3586
			public float m_aiAttackInterval = 2f;

			// Token: 0x04000E03 RID: 3587
			public float m_aiAttackMaxAngle = 5f;

			// Token: 0x04000E04 RID: 3588
			public bool m_aiWhenFlying = true;

			// Token: 0x04000E05 RID: 3589
			public float m_aiWhenFlyingAltitudeMin;

			// Token: 0x04000E06 RID: 3590
			public float m_aiWhenFlyingAltitudeMax = 999999f;

			// Token: 0x04000E07 RID: 3591
			public bool m_aiWhenWalking = true;

			// Token: 0x04000E08 RID: 3592
			public bool m_aiWhenSwiming = true;

			// Token: 0x04000E09 RID: 3593
			public bool m_aiPrioritized;

			// Token: 0x04000E0A RID: 3594
			public bool m_aiInDungeonOnly;

			// Token: 0x04000E0B RID: 3595
			public bool m_aiInMistOnly;

			// Token: 0x04000E0C RID: 3596
			[Range(0f, 1f)]
			public float m_aiMaxHealthPercentage = 1f;

			// Token: 0x04000E0D RID: 3597
			public ItemDrop.ItemData.AiTarget m_aiTargetType;

			// Token: 0x04000E0E RID: 3598
			[Header("Effects")]
			public EffectList m_hitEffect = new EffectList();

			// Token: 0x04000E0F RID: 3599
			public EffectList m_hitTerrainEffect = new EffectList();

			// Token: 0x04000E10 RID: 3600
			public EffectList m_blockEffect = new EffectList();

			// Token: 0x04000E11 RID: 3601
			public EffectList m_startEffect = new EffectList();

			// Token: 0x04000E12 RID: 3602
			public EffectList m_holdStartEffect = new EffectList();

			// Token: 0x04000E13 RID: 3603
			public EffectList m_triggerEffect = new EffectList();

			// Token: 0x04000E14 RID: 3604
			public EffectList m_trailStartEffect = new EffectList();

			// Token: 0x04000E15 RID: 3605
			[Header("Consumable")]
			public StatusEffect m_consumeStatusEffect;
		}
	}
}
