using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001D5 RID: 469
public class ObjectDB : MonoBehaviour
{
	// Token: 0x170000C7 RID: 199
	// (get) Token: 0x0600132F RID: 4911 RVA: 0x0007E909 File Offset: 0x0007CB09
	public static ObjectDB instance
	{
		get
		{
			return ObjectDB.m_instance;
		}
	}

	// Token: 0x06001330 RID: 4912 RVA: 0x0007E910 File Offset: 0x0007CB10
	private void Awake()
	{
		ObjectDB.m_instance = this;
		this.UpdateItemHashes();
	}

	// Token: 0x06001331 RID: 4913 RVA: 0x0007E91E File Offset: 0x0007CB1E
	public void CopyOtherDB(ObjectDB other)
	{
		this.m_items = other.m_items;
		this.m_recipes = other.m_recipes;
		this.m_StatusEffects = other.m_StatusEffects;
		this.UpdateItemHashes();
	}

	// Token: 0x06001332 RID: 4914 RVA: 0x0007E94C File Offset: 0x0007CB4C
	private void UpdateItemHashes()
	{
		this.m_itemByHash.Clear();
		foreach (GameObject gameObject in this.m_items)
		{
			this.m_itemByHash.Add(gameObject.name.GetStableHashCode(), gameObject);
		}
	}

	// Token: 0x06001333 RID: 4915 RVA: 0x0007E9BC File Offset: 0x0007CBBC
	public StatusEffect GetStatusEffect(string name)
	{
		foreach (StatusEffect statusEffect in this.m_StatusEffects)
		{
			if (statusEffect.name == name)
			{
				return statusEffect;
			}
		}
		return null;
	}

	// Token: 0x06001334 RID: 4916 RVA: 0x0007EA20 File Offset: 0x0007CC20
	public StatusEffect GetStatusEffect(int nameHash)
	{
		foreach (StatusEffect statusEffect in this.m_StatusEffects)
		{
			if (statusEffect.name.GetStableHashCode() == nameHash)
			{
				return statusEffect;
			}
		}
		return null;
	}

	// Token: 0x06001335 RID: 4917 RVA: 0x0007EA84 File Offset: 0x0007CC84
	public GameObject GetItemPrefab(string name)
	{
		foreach (GameObject gameObject in this.m_items)
		{
			if (gameObject.name == name)
			{
				return gameObject;
			}
		}
		return null;
	}

	// Token: 0x06001336 RID: 4918 RVA: 0x0007EAE8 File Offset: 0x0007CCE8
	public GameObject GetItemPrefab(int hash)
	{
		GameObject result;
		if (this.m_itemByHash.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001337 RID: 4919 RVA: 0x000643B2 File Offset: 0x000625B2
	public int GetPrefabHash(GameObject prefab)
	{
		return prefab.name.GetStableHashCode();
	}

	// Token: 0x06001338 RID: 4920 RVA: 0x0007EB08 File Offset: 0x0007CD08
	public List<ItemDrop> GetAllItems(ItemDrop.ItemData.ItemType type, string startWith)
	{
		List<ItemDrop> list = new List<ItemDrop>();
		foreach (GameObject gameObject in this.m_items)
		{
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			if (component.m_itemData.m_shared.m_itemType == type && component.gameObject.name.CustomStartsWith(startWith))
			{
				list.Add(component);
			}
		}
		return list;
	}

	// Token: 0x06001339 RID: 4921 RVA: 0x0007EB90 File Offset: 0x0007CD90
	public Recipe GetRecipe(ItemDrop.ItemData item)
	{
		foreach (Recipe recipe in this.m_recipes)
		{
			if (!(recipe.m_item == null) && recipe.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name)
			{
				return recipe;
			}
		}
		return null;
	}

	// Token: 0x0400141C RID: 5148
	private static ObjectDB m_instance;

	// Token: 0x0400141D RID: 5149
	public List<StatusEffect> m_StatusEffects = new List<StatusEffect>();

	// Token: 0x0400141E RID: 5150
	public List<GameObject> m_items = new List<GameObject>();

	// Token: 0x0400141F RID: 5151
	public List<Recipe> m_recipes = new List<Recipe>();

	// Token: 0x04001420 RID: 5152
	private Dictionary<int, GameObject> m_itemByHash = new Dictionary<int, GameObject>();
}
