using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000123 RID: 291
public class Inventory
{
	// Token: 0x06000B29 RID: 2857 RVA: 0x00052854 File Offset: 0x00050A54
	public Inventory(string name, Sprite bkg, int w, int h)
	{
		this.m_bkg = bkg;
		this.m_name = name;
		this.m_width = w;
		this.m_height = h;
	}

	// Token: 0x06000B2A RID: 2858 RVA: 0x000528B0 File Offset: 0x00050AB0
	private bool AddItem(ItemDrop.ItemData item, int amount, int x, int y)
	{
		amount = Mathf.Min(amount, item.m_stack);
		if (x < 0 || y < 0 || x >= this.m_width || y >= this.m_height)
		{
			return false;
		}
		ItemDrop.ItemData itemAt = this.GetItemAt(x, y);
		bool result;
		if (itemAt != null)
		{
			if (itemAt.m_shared.m_name != item.m_shared.m_name || (itemAt.m_shared.m_maxQuality > 1 && itemAt.m_quality != item.m_quality))
			{
				return false;
			}
			int num = itemAt.m_shared.m_maxStackSize - itemAt.m_stack;
			if (num <= 0)
			{
				return false;
			}
			int num2 = Mathf.Min(num, amount);
			itemAt.m_stack += num2;
			item.m_stack -= num2;
			result = (num2 == amount);
			ZLog.Log("Added to stack" + itemAt.m_stack.ToString() + " " + item.m_stack.ToString());
		}
		else
		{
			ItemDrop.ItemData itemData = item.Clone();
			itemData.m_stack = amount;
			itemData.m_gridPos = new Vector2i(x, y);
			this.m_inventory.Add(itemData);
			item.m_stack -= amount;
			result = true;
		}
		this.Changed();
		return result;
	}

	// Token: 0x06000B2B RID: 2859 RVA: 0x000529E8 File Offset: 0x00050BE8
	public bool CanAddItem(GameObject prefab, int stack = -1)
	{
		ItemDrop component = prefab.GetComponent<ItemDrop>();
		return !(component == null) && this.CanAddItem(component.m_itemData, stack);
	}

	// Token: 0x06000B2C RID: 2860 RVA: 0x00052A14 File Offset: 0x00050C14
	public bool CanAddItem(ItemDrop.ItemData item, int stack = -1)
	{
		if (this.HaveEmptySlot())
		{
			return true;
		}
		if (stack <= 0)
		{
			stack = item.m_stack;
		}
		return this.FindFreeStackSpace(item.m_shared.m_name) >= stack;
	}

	// Token: 0x06000B2D RID: 2861 RVA: 0x00052A44 File Offset: 0x00050C44
	public bool AddItem(GameObject prefab, int amount)
	{
		ItemDrop.ItemData itemData = prefab.GetComponent<ItemDrop>().m_itemData.Clone();
		itemData.m_dropPrefab = prefab;
		itemData.m_stack = Mathf.Min(amount, itemData.m_shared.m_maxStackSize);
		ZLog.Log("adding " + prefab.name + "  " + itemData.m_stack.ToString());
		return this.AddItem(itemData);
	}

	// Token: 0x06000B2E RID: 2862 RVA: 0x00052AAC File Offset: 0x00050CAC
	public bool AddItem(ItemDrop.ItemData item)
	{
		bool result = true;
		if (item.m_shared.m_maxStackSize > 1)
		{
			int i = 0;
			while (i < item.m_stack)
			{
				ItemDrop.ItemData itemData = this.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
				if (itemData != null)
				{
					itemData.m_stack++;
					i++;
				}
				else
				{
					int stack = item.m_stack - i;
					item.m_stack = stack;
					Vector2i vector2i = this.FindEmptySlot(this.TopFirst(item));
					if (vector2i.x >= 0)
					{
						item.m_gridPos = vector2i;
						this.m_inventory.Add(item);
						break;
					}
					result = false;
					break;
				}
			}
		}
		else
		{
			Vector2i vector2i2 = this.FindEmptySlot(this.TopFirst(item));
			if (vector2i2.x >= 0)
			{
				item.m_gridPos = vector2i2;
				this.m_inventory.Add(item);
			}
			else
			{
				result = false;
			}
		}
		this.Changed();
		return result;
	}

	// Token: 0x06000B2F RID: 2863 RVA: 0x00052B88 File Offset: 0x00050D88
	private bool TopFirst(ItemDrop.ItemData item)
	{
		return item.IsWeapon() || (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Misc);
	}

	// Token: 0x06000B30 RID: 2864 RVA: 0x00052BE0 File Offset: 0x00050DE0
	public void MoveAll(Inventory fromInventory)
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
		List<ItemDrop.ItemData> list2 = new List<ItemDrop.ItemData>();
		foreach (ItemDrop.ItemData itemData in list)
		{
			if (this.AddItem(itemData, itemData.m_stack, itemData.m_gridPos.x, itemData.m_gridPos.y))
			{
				fromInventory.RemoveItem(itemData);
			}
			else
			{
				list2.Add(itemData);
			}
		}
		foreach (ItemDrop.ItemData item in list2)
		{
			if (this.AddItem(item))
			{
				fromInventory.RemoveItem(item);
			}
		}
		this.Changed();
		fromInventory.Changed();
	}

	// Token: 0x06000B31 RID: 2865 RVA: 0x00052CC0 File Offset: 0x00050EC0
	public void MoveItemToThis(Inventory fromInventory, ItemDrop.ItemData item)
	{
		if (this.AddItem(item))
		{
			fromInventory.RemoveItem(item);
		}
		this.Changed();
		fromInventory.Changed();
	}

	// Token: 0x06000B32 RID: 2866 RVA: 0x00052CDF File Offset: 0x00050EDF
	public bool MoveItemToThis(Inventory fromInventory, ItemDrop.ItemData item, int amount, int x, int y)
	{
		bool result = this.AddItem(item, amount, x, y);
		if (item.m_stack == 0)
		{
			fromInventory.RemoveItem(item);
			return result;
		}
		fromInventory.Changed();
		return result;
	}

	// Token: 0x06000B33 RID: 2867 RVA: 0x00052D04 File Offset: 0x00050F04
	public bool RemoveItem(int index)
	{
		if (index < 0 || index >= this.m_inventory.Count)
		{
			return false;
		}
		this.m_inventory.RemoveAt(index);
		this.Changed();
		return true;
	}

	// Token: 0x06000B34 RID: 2868 RVA: 0x00052D2D File Offset: 0x00050F2D
	public bool ContainsItem(ItemDrop.ItemData item)
	{
		return this.m_inventory.Contains(item);
	}

	// Token: 0x06000B35 RID: 2869 RVA: 0x00052D3C File Offset: 0x00050F3C
	public bool RemoveOneItem(ItemDrop.ItemData item)
	{
		if (!this.m_inventory.Contains(item))
		{
			return false;
		}
		if (item.m_stack > 1)
		{
			item.m_stack--;
			this.Changed();
		}
		else
		{
			this.m_inventory.Remove(item);
			this.Changed();
		}
		return true;
	}

	// Token: 0x06000B36 RID: 2870 RVA: 0x00052D8C File Offset: 0x00050F8C
	public bool RemoveItem(ItemDrop.ItemData item)
	{
		if (!this.m_inventory.Contains(item))
		{
			ZLog.Log("Item is not in this container");
			return false;
		}
		this.m_inventory.Remove(item);
		this.Changed();
		return true;
	}

	// Token: 0x06000B37 RID: 2871 RVA: 0x00052DBC File Offset: 0x00050FBC
	public bool RemoveItem(ItemDrop.ItemData item, int amount)
	{
		amount = Mathf.Min(item.m_stack, amount);
		if (amount == item.m_stack)
		{
			return this.RemoveItem(item);
		}
		if (!this.m_inventory.Contains(item))
		{
			return false;
		}
		item.m_stack -= amount;
		this.Changed();
		return true;
	}

	// Token: 0x06000B38 RID: 2872 RVA: 0x00052E10 File Offset: 0x00051010
	public void RemoveItem(string name, int amount, int itemQuality = -1)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name && (itemQuality < 0 || itemData.m_quality == itemQuality))
			{
				int num = Mathf.Min(itemData.m_stack, amount);
				itemData.m_stack -= num;
				amount -= num;
				if (amount <= 0)
				{
					break;
				}
			}
		}
		this.m_inventory.RemoveAll((ItemDrop.ItemData x) => x.m_stack <= 0);
		this.Changed();
	}

	// Token: 0x06000B39 RID: 2873 RVA: 0x00052ED4 File Offset: 0x000510D4
	public bool HaveItem(string name)
	{
		using (List<ItemDrop.ItemData>.Enumerator enumerator = this.m_inventory.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_shared.m_name == name)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000B3A RID: 2874 RVA: 0x00052F38 File Offset: 0x00051138
	public void GetAllPieceTables(List<PieceTable> tables)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_buildPieces != null && !tables.Contains(itemData.m_shared.m_buildPieces))
			{
				tables.Add(itemData.m_shared.m_buildPieces);
			}
		}
	}

	// Token: 0x06000B3B RID: 2875 RVA: 0x00052FBC File Offset: 0x000511BC
	public int CountItems(string name, int quality = -1)
	{
		int num = 0;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name && (quality < 0 || quality == itemData.m_quality))
			{
				num += itemData.m_stack;
			}
		}
		return num;
	}

	// Token: 0x06000B3C RID: 2876 RVA: 0x00053034 File Offset: 0x00051234
	public ItemDrop.ItemData GetItem(int index)
	{
		return this.m_inventory[index];
	}

	// Token: 0x06000B3D RID: 2877 RVA: 0x00053044 File Offset: 0x00051244
	public ItemDrop.ItemData GetItem(string name, int quality = -1, bool isPrefabName = false)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (((isPrefabName && itemData.m_dropPrefab.name == name) || (!isPrefabName && itemData.m_shared.m_name == name)) && (quality < 0 || quality == itemData.m_quality))
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x06000B3E RID: 2878 RVA: 0x000530D0 File Offset: 0x000512D0
	public ItemDrop.ItemData GetAmmoItem(string ammoName, string matchPrefabName = null)
	{
		int num = 0;
		ItemDrop.ItemData itemData = null;
		foreach (ItemDrop.ItemData itemData2 in this.m_inventory)
		{
			if ((itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable || itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable) && itemData2.m_shared.m_ammoType == ammoName && (matchPrefabName == null || itemData2.m_dropPrefab.name == matchPrefabName))
			{
				int num2 = itemData2.m_gridPos.y * this.m_width + itemData2.m_gridPos.x;
				if (num2 < num || itemData == null)
				{
					num = num2;
					itemData = itemData2;
				}
			}
		}
		return itemData;
	}

	// Token: 0x06000B3F RID: 2879 RVA: 0x000531AC File Offset: 0x000513AC
	public int FindFreeStackSpace(string name)
	{
		int num = 0;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name && itemData.m_stack < itemData.m_shared.m_maxStackSize)
			{
				num += itemData.m_shared.m_maxStackSize - itemData.m_stack;
			}
		}
		return num;
	}

	// Token: 0x06000B40 RID: 2880 RVA: 0x00053238 File Offset: 0x00051438
	private ItemDrop.ItemData FindFreeStackItem(string name, int quality)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name && itemData.m_quality == quality && itemData.m_stack < itemData.m_shared.m_maxStackSize)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x06000B41 RID: 2881 RVA: 0x000532BC File Offset: 0x000514BC
	public int NrOfItems()
	{
		return this.m_inventory.Count;
	}

	// Token: 0x06000B42 RID: 2882 RVA: 0x000532CC File Offset: 0x000514CC
	public int NrOfItemsIncludingStacks()
	{
		int num = 0;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			num += itemData.m_stack;
		}
		return num;
	}

	// Token: 0x06000B43 RID: 2883 RVA: 0x00053324 File Offset: 0x00051524
	public float SlotsUsedPercentage()
	{
		return (float)this.m_inventory.Count / (float)(this.m_width * this.m_height) * 100f;
	}

	// Token: 0x06000B44 RID: 2884 RVA: 0x00053348 File Offset: 0x00051548
	public void Print()
	{
		for (int i = 0; i < this.m_inventory.Count; i++)
		{
			ItemDrop.ItemData itemData = this.m_inventory[i];
			ZLog.Log(string.Concat(new string[]
			{
				i.ToString(),
				": ",
				itemData.m_shared.m_name,
				"  ",
				itemData.m_stack.ToString(),
				" / ",
				itemData.m_shared.m_maxStackSize.ToString()
			}));
		}
	}

	// Token: 0x06000B45 RID: 2885 RVA: 0x000533D9 File Offset: 0x000515D9
	public int GetEmptySlots()
	{
		return this.m_height * this.m_width - this.m_inventory.Count;
	}

	// Token: 0x06000B46 RID: 2886 RVA: 0x000533F4 File Offset: 0x000515F4
	public bool HaveEmptySlot()
	{
		return this.m_inventory.Count < this.m_width * this.m_height;
	}

	// Token: 0x06000B47 RID: 2887 RVA: 0x00053410 File Offset: 0x00051610
	private Vector2i FindEmptySlot(bool topFirst)
	{
		if (topFirst)
		{
			for (int i = 0; i < this.m_height; i++)
			{
				for (int j = 0; j < this.m_width; j++)
				{
					if (this.GetItemAt(j, i) == null)
					{
						return new Vector2i(j, i);
					}
				}
			}
		}
		else
		{
			for (int k = this.m_height - 1; k >= 0; k--)
			{
				for (int l = 0; l < this.m_width; l++)
				{
					if (this.GetItemAt(l, k) == null)
					{
						return new Vector2i(l, k);
					}
				}
			}
		}
		return new Vector2i(-1, -1);
	}

	// Token: 0x06000B48 RID: 2888 RVA: 0x00053494 File Offset: 0x00051694
	public ItemDrop.ItemData GetOtherItemAt(int x, int y, ItemDrop.ItemData oldItem)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData != oldItem && itemData.m_gridPos.x == x && itemData.m_gridPos.y == y)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x06000B49 RID: 2889 RVA: 0x00053508 File Offset: 0x00051708
	public ItemDrop.ItemData GetItemAt(int x, int y)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_gridPos.x == x && itemData.m_gridPos.y == y)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x06000B4A RID: 2890 RVA: 0x00053578 File Offset: 0x00051778
	public List<ItemDrop.ItemData> GetEquippedItems()
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_equipped)
			{
				list.Add(itemData);
			}
		}
		return list;
	}

	// Token: 0x06000B4B RID: 2891 RVA: 0x000535DC File Offset: 0x000517DC
	public void GetWornItems(List<ItemDrop.ItemData> worn)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_useDurability && itemData.m_durability < itemData.GetMaxDurability())
			{
				worn.Add(itemData);
			}
		}
	}

	// Token: 0x06000B4C RID: 2892 RVA: 0x0005364C File Offset: 0x0005184C
	public void GetValuableItems(List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_value > 0)
			{
				items.Add(itemData);
			}
		}
	}

	// Token: 0x06000B4D RID: 2893 RVA: 0x000536B0 File Offset: 0x000518B0
	public List<ItemDrop.ItemData> GetAllItems()
	{
		return this.m_inventory;
	}

	// Token: 0x06000B4E RID: 2894 RVA: 0x000536B8 File Offset: 0x000518B8
	public void GetAllItems(string name, List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name)
			{
				items.Add(itemData);
			}
		}
	}

	// Token: 0x06000B4F RID: 2895 RVA: 0x00053720 File Offset: 0x00051920
	public void GetAllItems(ItemDrop.ItemData.ItemType type, List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_itemType == type)
			{
				items.Add(itemData);
			}
		}
	}

	// Token: 0x06000B50 RID: 2896 RVA: 0x00053784 File Offset: 0x00051984
	public int GetWidth()
	{
		return this.m_width;
	}

	// Token: 0x06000B51 RID: 2897 RVA: 0x0005378C File Offset: 0x0005198C
	public int GetHeight()
	{
		return this.m_height;
	}

	// Token: 0x06000B52 RID: 2898 RVA: 0x00053794 File Offset: 0x00051994
	public string GetName()
	{
		return this.m_name;
	}

	// Token: 0x06000B53 RID: 2899 RVA: 0x0005379C File Offset: 0x0005199C
	public Sprite GetBkg()
	{
		return this.m_bkg;
	}

	// Token: 0x06000B54 RID: 2900 RVA: 0x000537A4 File Offset: 0x000519A4
	public void Save(ZPackage pkg)
	{
		pkg.Write(this.currentVersion);
		pkg.Write(this.m_inventory.Count);
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_dropPrefab == null)
			{
				ZLog.Log("Item missing prefab " + itemData.m_shared.m_name);
				pkg.Write("");
			}
			else
			{
				pkg.Write(itemData.m_dropPrefab.name);
			}
			pkg.Write(itemData.m_stack);
			pkg.Write(itemData.m_durability);
			pkg.Write(itemData.m_gridPos);
			pkg.Write(itemData.m_equipped);
			pkg.Write(itemData.m_quality);
			pkg.Write(itemData.m_variant);
			pkg.Write(itemData.m_crafterID);
			pkg.Write(itemData.m_crafterName);
			pkg.Write(itemData.m_customData.Count);
			foreach (KeyValuePair<string, string> keyValuePair in itemData.m_customData)
			{
				pkg.Write(keyValuePair.Key);
				pkg.Write(keyValuePair.Value);
			}
		}
	}

	// Token: 0x06000B55 RID: 2901 RVA: 0x00053938 File Offset: 0x00051B38
	public void Load(ZPackage pkg)
	{
		int num = pkg.ReadInt();
		int num2 = pkg.ReadInt();
		this.m_inventory.Clear();
		for (int i = 0; i < num2; i++)
		{
			string text = pkg.ReadString();
			int stack = pkg.ReadInt();
			float durability = pkg.ReadSingle();
			Vector2i pos = pkg.ReadVector2i();
			bool equiped = pkg.ReadBool();
			int quality = 1;
			if (num >= 101)
			{
				quality = pkg.ReadInt();
			}
			int variant = 0;
			if (num >= 102)
			{
				variant = pkg.ReadInt();
			}
			long crafterID = 0L;
			string crafterName = "";
			if (num >= 103)
			{
				crafterID = pkg.ReadLong();
				crafterName = pkg.ReadString();
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (num >= 104)
			{
				int num3 = pkg.ReadInt();
				for (int j = 0; j < num3; j++)
				{
					string key = pkg.ReadString();
					string value = pkg.ReadString();
					dictionary[key] = value;
				}
			}
			if (text != "")
			{
				this.AddItem(text, stack, durability, pos, equiped, quality, variant, crafterID, crafterName, dictionary);
			}
		}
		this.Changed();
	}

	// Token: 0x06000B56 RID: 2902 RVA: 0x00053A44 File Offset: 0x00051C44
	public ItemDrop.ItemData AddItem(string name, int stack, int quality, int variant, long crafterID, string crafterName)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		if (itemPrefab == null)
		{
			ZLog.Log("Failed to find item prefab " + name);
			return null;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component == null)
		{
			ZLog.Log("Invalid item " + name);
			return null;
		}
		if (component.m_itemData.m_shared.m_maxStackSize <= 1 && this.FindEmptySlot(this.TopFirst(component.m_itemData)).x == -1)
		{
			return null;
		}
		ItemDrop.ItemData result = null;
		int i = stack;
		while (i > 0)
		{
			ZNetView.m_forceDisableInit = true;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(itemPrefab);
			ZNetView.m_forceDisableInit = false;
			ItemDrop component2 = gameObject.GetComponent<ItemDrop>();
			if (component2 == null)
			{
				ZLog.Log("Missing itemdrop in " + name);
				UnityEngine.Object.Destroy(gameObject);
				return null;
			}
			int num = Mathf.Min(i, component2.m_itemData.m_shared.m_maxStackSize);
			i -= num;
			component2.m_itemData.m_stack = num;
			component2.SetQuality(quality);
			component2.m_itemData.m_variant = variant;
			component2.m_itemData.m_durability = component2.m_itemData.GetMaxDurability();
			component2.m_itemData.m_crafterID = crafterID;
			component2.m_itemData.m_crafterName = crafterName;
			if (!this.AddItem(component2.m_itemData))
			{
				UnityEngine.Object.Destroy(gameObject);
				return null;
			}
			result = component2.m_itemData;
			UnityEngine.Object.Destroy(gameObject);
		}
		return result;
	}

	// Token: 0x06000B57 RID: 2903 RVA: 0x00053BB8 File Offset: 0x00051DB8
	private bool AddItem(string name, int stack, float durability, Vector2i pos, bool equiped, int quality, int variant, long crafterID, string crafterName, Dictionary<string, string> customData)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		if (itemPrefab == null)
		{
			ZLog.Log("Failed to find item prefab " + name);
			return false;
		}
		ZNetView.m_forceDisableInit = true;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(itemPrefab);
		ZNetView.m_forceDisableInit = false;
		ItemDrop component = gameObject.GetComponent<ItemDrop>();
		if (component == null)
		{
			ZLog.Log("Missing itemdrop in " + name);
			UnityEngine.Object.Destroy(gameObject);
			return false;
		}
		component.m_itemData.m_stack = Mathf.Min(stack, component.m_itemData.m_shared.m_maxStackSize);
		component.m_itemData.m_durability = durability;
		component.m_itemData.m_equipped = equiped;
		component.SetQuality(quality);
		component.m_itemData.m_variant = variant;
		component.m_itemData.m_crafterID = crafterID;
		component.m_itemData.m_crafterName = crafterName;
		component.m_itemData.m_customData = customData;
		this.AddItem(component.m_itemData, component.m_itemData.m_stack, pos.x, pos.y);
		UnityEngine.Object.Destroy(gameObject);
		return true;
	}

	// Token: 0x06000B58 RID: 2904 RVA: 0x00053CCC File Offset: 0x00051ECC
	public void MoveInventoryToGrave(Inventory original)
	{
		this.m_inventory.Clear();
		this.m_width = original.m_width;
		this.m_height = original.m_height;
		foreach (ItemDrop.ItemData itemData in original.m_inventory)
		{
			if (!itemData.m_shared.m_questItem && !itemData.m_equipped)
			{
				this.m_inventory.Add(itemData);
			}
		}
		original.m_inventory.RemoveAll((ItemDrop.ItemData x) => !x.m_shared.m_questItem && !x.m_equipped);
		original.Changed();
		this.Changed();
	}

	// Token: 0x06000B59 RID: 2905 RVA: 0x00053D94 File Offset: 0x00051F94
	private void Changed()
	{
		this.UpdateTotalWeight();
		if (this.m_onChanged != null)
		{
			this.m_onChanged();
		}
	}

	// Token: 0x06000B5A RID: 2906 RVA: 0x00053DAF File Offset: 0x00051FAF
	public void RemoveAll()
	{
		this.m_inventory.Clear();
		this.Changed();
	}

	// Token: 0x06000B5B RID: 2907 RVA: 0x00053DC4 File Offset: 0x00051FC4
	private void UpdateTotalWeight()
	{
		this.m_totalWeight = 0f;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			this.m_totalWeight += itemData.GetWeight();
		}
	}

	// Token: 0x06000B5C RID: 2908 RVA: 0x00053E30 File Offset: 0x00052030
	public float GetTotalWeight()
	{
		return this.m_totalWeight;
	}

	// Token: 0x06000B5D RID: 2909 RVA: 0x00053E38 File Offset: 0x00052038
	public void GetBoundItems(List<ItemDrop.ItemData> bound)
	{
		bound.Clear();
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_gridPos.y == 0)
			{
				bound.Add(itemData);
			}
		}
	}

	// Token: 0x06000B5E RID: 2910 RVA: 0x00053EA0 File Offset: 0x000520A0
	public bool IsTeleportable()
	{
		using (List<ItemDrop.ItemData>.Enumerator enumerator = this.m_inventory.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.m_shared.m_teleportable)
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x04000D61 RID: 3425
	private int currentVersion = 104;

	// Token: 0x04000D62 RID: 3426
	public Action m_onChanged;

	// Token: 0x04000D63 RID: 3427
	private string m_name = "";

	// Token: 0x04000D64 RID: 3428
	private Sprite m_bkg;

	// Token: 0x04000D65 RID: 3429
	private List<ItemDrop.ItemData> m_inventory = new List<ItemDrop.ItemData>();

	// Token: 0x04000D66 RID: 3430
	private int m_width = 4;

	// Token: 0x04000D67 RID: 3431
	private int m_height = 4;

	// Token: 0x04000D68 RID: 3432
	private float m_totalWeight;
}
