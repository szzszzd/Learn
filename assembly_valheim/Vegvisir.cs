using System;
using UnityEngine;

// Token: 0x020002BC RID: 700
public class Vegvisir : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001A7C RID: 6780 RVA: 0x000AFFFA File Offset: 0x000AE1FA
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + " " + this.m_pinName + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_register_location ");
	}

	// Token: 0x06001A7D RID: 6781 RVA: 0x000B0021 File Offset: 0x000AE221
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001A7E RID: 6782 RVA: 0x000B002C File Offset: 0x000AE22C
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		Game.instance.DiscoverClosestLocation(this.m_locationName, base.transform.position, this.m_pinName, (int)this.m_pinType, true);
		Gogan.LogEvent("Game", "Vegvisir", this.m_locationName, 0L);
		return true;
	}

	// Token: 0x06001A7F RID: 6783 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x04001C9B RID: 7323
	public string m_name = "$piece_vegvisir";

	// Token: 0x04001C9C RID: 7324
	public string m_locationName = "";

	// Token: 0x04001C9D RID: 7325
	public string m_pinName = "Pin";

	// Token: 0x04001C9E RID: 7326
	public Minimap.PinType m_pinType;
}
