using System;
using UnityEngine;

// Token: 0x0200029B RID: 667
public class Switch : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x0600198A RID: 6538 RVA: 0x000A9710 File Offset: 0x000A7910
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			if (this.m_holdRepeatInterval <= 0f)
			{
				return false;
			}
			if (Time.time - this.m_lastUseTime < this.m_holdRepeatInterval)
			{
				return false;
			}
		}
		this.m_lastUseTime = Time.time;
		return this.m_onUse != null && this.m_onUse(this, character, null);
	}

	// Token: 0x0600198B RID: 6539 RVA: 0x000A9768 File Offset: 0x000A7968
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return this.m_onUse != null && this.m_onUse(this, user, item);
	}

	// Token: 0x0600198C RID: 6540 RVA: 0x000A9782 File Offset: 0x000A7982
	public string GetHoverText()
	{
		if (this.m_onHover != null)
		{
			return this.m_onHover();
		}
		return Localization.instance.Localize(this.m_hoverText);
	}

	// Token: 0x0600198D RID: 6541 RVA: 0x000A97A8 File Offset: 0x000A79A8
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x04001B68 RID: 7016
	public Switch.Callback m_onUse;

	// Token: 0x04001B69 RID: 7017
	public Switch.TooltipCallback m_onHover;

	// Token: 0x04001B6A RID: 7018
	[TextArea(3, 20)]
	public string m_hoverText = "";

	// Token: 0x04001B6B RID: 7019
	public string m_name = "";

	// Token: 0x04001B6C RID: 7020
	public float m_holdRepeatInterval = -1f;

	// Token: 0x04001B6D RID: 7021
	private float m_lastUseTime;

	// Token: 0x0200029C RID: 668
	// (Invoke) Token: 0x06001990 RID: 6544
	public delegate bool Callback(Switch caller, Humanoid user, ItemDrop.ItemData item);

	// Token: 0x0200029D RID: 669
	// (Invoke) Token: 0x06001994 RID: 6548
	public delegate string TooltipCallback();
}
