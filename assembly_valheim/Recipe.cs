using System;
using UnityEngine;

// Token: 0x02000130 RID: 304
public class Recipe : ScriptableObject
{
	// Token: 0x06000BDE RID: 3038 RVA: 0x000575FB File Offset: 0x000557FB
	public int GetRequiredStationLevel(int quality)
	{
		return Mathf.Max(1, this.m_minStationLevel) + (quality - 1);
	}

	// Token: 0x06000BDF RID: 3039 RVA: 0x0005760D File Offset: 0x0005580D
	public CraftingStation GetRequiredStation(int quality)
	{
		if (this.m_craftingStation)
		{
			return this.m_craftingStation;
		}
		if (quality > 1)
		{
			return this.m_repairStation;
		}
		return null;
	}

	// Token: 0x06000BE0 RID: 3040 RVA: 0x00057630 File Offset: 0x00055830
	public int GetAmount(int quality, out int need, out ItemDrop.ItemData singleReqItem)
	{
		int num = this.m_amount;
		need = 0;
		singleReqItem = null;
		if (this.m_requireOnlyOneIngredient)
		{
			int num2;
			singleReqItem = Player.m_localPlayer.GetFirstRequiredItem(Player.m_localPlayer.GetInventory(), this, quality, out need, out num2);
			num += (int)Mathf.Ceil((float)((singleReqItem.m_quality - 1) * num) * this.m_qualityResultAmountMultiplier) + num2;
		}
		return num;
	}

	// Token: 0x04000E42 RID: 3650
	public ItemDrop m_item;

	// Token: 0x04000E43 RID: 3651
	public int m_amount = 1;

	// Token: 0x04000E44 RID: 3652
	public bool m_enabled = true;

	// Token: 0x04000E45 RID: 3653
	[global::Tooltip("Only supported when using m_requireOnlyOneIngredient")]
	public float m_qualityResultAmountMultiplier = 1f;

	// Token: 0x04000E46 RID: 3654
	[Header("Requirements")]
	public CraftingStation m_craftingStation;

	// Token: 0x04000E47 RID: 3655
	public CraftingStation m_repairStation;

	// Token: 0x04000E48 RID: 3656
	public int m_minStationLevel = 1;

	// Token: 0x04000E49 RID: 3657
	public bool m_requireOnlyOneIngredient;

	// Token: 0x04000E4A RID: 3658
	public Piece.Requirement[] m_resources = new Piece.Requirement[0];
}
