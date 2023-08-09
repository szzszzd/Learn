using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000255 RID: 597
public class ItemSets : MonoBehaviour
{
	// Token: 0x170000ED RID: 237
	// (get) Token: 0x06001729 RID: 5929 RVA: 0x00099508 File Offset: 0x00097708
	public static ItemSets instance
	{
		get
		{
			return ItemSets.m_instance;
		}
	}

	// Token: 0x0600172A RID: 5930 RVA: 0x0009950F File Offset: 0x0009770F
	public void Awake()
	{
		ItemSets.m_instance = this;
	}

	// Token: 0x0600172B RID: 5931 RVA: 0x00099518 File Offset: 0x00097718
	public bool TryGetSet(string name, bool dropCurrentItems = false)
	{
		if (Player.m_localPlayer == null)
		{
			return false;
		}
		ItemSets.ItemSet itemSet;
		if (this.GetSetDictionary().TryGetValue(name, out itemSet))
		{
			Skills skills = Player.m_localPlayer.GetSkills();
			if (dropCurrentItems)
			{
				Player.m_localPlayer.CreateTombStone();
				Player.m_localPlayer.ClearFood();
				Player.m_localPlayer.ClearHardDeath();
				Player.m_localPlayer.GetSEMan().RemoveAllStatusEffects(false);
				foreach (Skills.SkillDef skillDef in skills.m_skills)
				{
					skills.CheatResetSkill(skillDef.m_skill.ToString());
				}
			}
			Inventory inventory = Player.m_localPlayer.GetInventory();
			InventoryGui.instance.m_playerGrid.UpdateInventory(inventory, Player.m_localPlayer, null);
			foreach (ItemSets.SetItem setItem in itemSet.m_items)
			{
				if (!(setItem.m_item == null))
				{
					int amount = Math.Max(1, setItem.m_stack);
					ItemDrop.ItemData itemData = inventory.AddItem(setItem.m_item.gameObject.name, Math.Max(1, setItem.m_stack), Math.Max(1, setItem.m_quality), 0, 0L, "Thor");
					if (itemData != null)
					{
						if (setItem.m_use)
						{
							Player.m_localPlayer.UseItem(inventory, itemData, false);
						}
						if (setItem.m_hotbarSlot > 0)
						{
							InventoryGui.instance.m_playerGrid.DropItem(inventory, itemData, amount, new Vector2i(setItem.m_hotbarSlot - 1, 0));
						}
					}
				}
			}
			foreach (ItemSets.SetSkill setSkill in itemSet.m_skills)
			{
				skills.CheatResetSkill(setSkill.m_skill.ToString());
				Player.m_localPlayer.GetSkills().CheatRaiseSkill(setSkill.m_skill.ToString(), (float)setSkill.m_level, true);
			}
			return true;
		}
		return false;
	}

	// Token: 0x0600172C RID: 5932 RVA: 0x00099768 File Offset: 0x00097968
	public List<string> GetSetNames()
	{
		return this.GetSetDictionary().Keys.ToList<string>();
	}

	// Token: 0x0600172D RID: 5933 RVA: 0x0009977C File Offset: 0x0009797C
	public Dictionary<string, ItemSets.ItemSet> GetSetDictionary()
	{
		Dictionary<string, ItemSets.ItemSet> dictionary = new Dictionary<string, ItemSets.ItemSet>();
		foreach (ItemSets.ItemSet itemSet in this.m_sets)
		{
			dictionary[itemSet.m_name] = itemSet;
		}
		return dictionary;
	}

	// Token: 0x04001889 RID: 6281
	private static ItemSets m_instance;

	// Token: 0x0400188A RID: 6282
	public List<ItemSets.ItemSet> m_sets = new List<ItemSets.ItemSet>();

	// Token: 0x02000256 RID: 598
	[Serializable]
	public class ItemSet
	{
		// Token: 0x0400188B RID: 6283
		public string m_name;

		// Token: 0x0400188C RID: 6284
		public List<ItemSets.SetItem> m_items = new List<ItemSets.SetItem>();

		// Token: 0x0400188D RID: 6285
		public List<ItemSets.SetSkill> m_skills = new List<ItemSets.SetSkill>();
	}

	// Token: 0x02000257 RID: 599
	[Serializable]
	public class SetItem
	{
		// Token: 0x0400188E RID: 6286
		public ItemDrop m_item;

		// Token: 0x0400188F RID: 6287
		public int m_quality = 1;

		// Token: 0x04001890 RID: 6288
		public int m_stack = 1;

		// Token: 0x04001891 RID: 6289
		public bool m_use = true;

		// Token: 0x04001892 RID: 6290
		public int m_hotbarSlot;
	}

	// Token: 0x02000258 RID: 600
	[Serializable]
	public class SetSkill
	{
		// Token: 0x04001893 RID: 6291
		public Skills.SkillType m_skill;

		// Token: 0x04001894 RID: 6292
		public int m_level;
	}
}
