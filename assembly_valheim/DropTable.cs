using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000121 RID: 289
[Serializable]
public class DropTable
{
	// Token: 0x06000B22 RID: 2850 RVA: 0x0005247A File Offset: 0x0005067A
	public DropTable Clone()
	{
		return base.MemberwiseClone() as DropTable;
	}

	// Token: 0x06000B23 RID: 2851 RVA: 0x00052488 File Offset: 0x00050688
	public List<ItemDrop.ItemData> GetDropListItems()
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		if (this.m_drops.Count == 0)
		{
			return list;
		}
		if (UnityEngine.Random.value > this.m_dropChance)
		{
			return list;
		}
		List<DropTable.DropData> list2 = new List<DropTable.DropData>(this.m_drops);
		float num = 0f;
		foreach (DropTable.DropData dropData in list2)
		{
			num += dropData.m_weight;
		}
		int num2 = UnityEngine.Random.Range(this.m_dropMin, this.m_dropMax + 1);
		for (int i = 0; i < num2; i++)
		{
			float num3 = UnityEngine.Random.Range(0f, num);
			bool flag = false;
			float num4 = 0f;
			foreach (DropTable.DropData dropData2 in list2)
			{
				num4 += dropData2.m_weight;
				if (num3 <= num4)
				{
					flag = true;
					this.AddItemToList(list, dropData2);
					if (this.m_oneOfEach)
					{
						list2.Remove(dropData2);
						num -= dropData2.m_weight;
						break;
					}
					break;
				}
			}
			if (!flag && list2.Count > 0)
			{
				this.AddItemToList(list, list2[0]);
			}
		}
		return list;
	}

	// Token: 0x06000B24 RID: 2852 RVA: 0x000525E4 File Offset: 0x000507E4
	private void AddItemToList(List<ItemDrop.ItemData> toDrop, DropTable.DropData data)
	{
		ItemDrop.ItemData itemData = data.m_item.GetComponent<ItemDrop>().m_itemData;
		ItemDrop.ItemData itemData2 = itemData.Clone();
		itemData2.m_dropPrefab = data.m_item;
		int minInclusive = Mathf.Max(1, data.m_stackMin);
		int num = Mathf.Min(itemData.m_shared.m_maxStackSize, data.m_stackMax);
		itemData2.m_stack = UnityEngine.Random.Range(minInclusive, num + 1);
		toDrop.Add(itemData2);
	}

	// Token: 0x06000B25 RID: 2853 RVA: 0x00052650 File Offset: 0x00050850
	public List<GameObject> GetDropList()
	{
		int amount = UnityEngine.Random.Range(this.m_dropMin, this.m_dropMax + 1);
		return this.GetDropList(amount);
	}

	// Token: 0x06000B26 RID: 2854 RVA: 0x00052678 File Offset: 0x00050878
	private List<GameObject> GetDropList(int amount)
	{
		List<GameObject> list = new List<GameObject>();
		if (this.m_drops.Count == 0)
		{
			return list;
		}
		if (UnityEngine.Random.value > this.m_dropChance)
		{
			return list;
		}
		List<DropTable.DropData> list2 = new List<DropTable.DropData>(this.m_drops);
		float num = 0f;
		foreach (DropTable.DropData dropData in list2)
		{
			num += dropData.m_weight;
			if (dropData.m_weight <= 0f && list2.Count > 1)
			{
				ZLog.LogWarning(string.Format("Droptable item '{0}' has a weight of 0 and will not be dropped correctly!", dropData.m_item));
			}
		}
		for (int i = 0; i < amount; i++)
		{
			float num2 = UnityEngine.Random.Range(0f, num);
			bool flag = false;
			float num3 = 0f;
			foreach (DropTable.DropData dropData2 in list2)
			{
				num3 += dropData2.m_weight;
				if (num2 <= num3)
				{
					flag = true;
					int num4 = UnityEngine.Random.Range(dropData2.m_stackMin, dropData2.m_stackMax);
					for (int j = 0; j < num4; j++)
					{
						list.Add(dropData2.m_item);
					}
					if (this.m_oneOfEach)
					{
						list2.Remove(dropData2);
						num -= dropData2.m_weight;
						break;
					}
					break;
				}
			}
			if (!flag && list2.Count > 0)
			{
				list.Add(list2[0].m_item);
			}
		}
		return list;
	}

	// Token: 0x06000B27 RID: 2855 RVA: 0x00052818 File Offset: 0x00050A18
	public bool IsEmpty()
	{
		return this.m_drops.Count == 0;
	}

	// Token: 0x04000D58 RID: 3416
	public List<DropTable.DropData> m_drops = new List<DropTable.DropData>();

	// Token: 0x04000D59 RID: 3417
	public int m_dropMin = 1;

	// Token: 0x04000D5A RID: 3418
	public int m_dropMax = 1;

	// Token: 0x04000D5B RID: 3419
	[Range(0f, 1f)]
	public float m_dropChance = 1f;

	// Token: 0x04000D5C RID: 3420
	public bool m_oneOfEach;

	// Token: 0x02000122 RID: 290
	[Serializable]
	public struct DropData
	{
		// Token: 0x04000D5D RID: 3421
		public GameObject m_item;

		// Token: 0x04000D5E RID: 3422
		public int m_stackMin;

		// Token: 0x04000D5F RID: 3423
		public int m_stackMax;

		// Token: 0x04000D60 RID: 3424
		public float m_weight;
	}
}
