using System;
using UnityEngine;

// Token: 0x020002AC RID: 684
public class ToggleSwitch : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x060019F7 RID: 6647 RVA: 0x000AC240 File Offset: 0x000AA440
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_onUse != null)
		{
			this.m_onUse(this, character);
		}
		return true;
	}

	// Token: 0x060019F8 RID: 6648 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060019F9 RID: 6649 RVA: 0x000AC25D File Offset: 0x000AA45D
	public string GetHoverText()
	{
		return this.m_hoverText;
	}

	// Token: 0x060019FA RID: 6650 RVA: 0x000AC265 File Offset: 0x000AA465
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060019FB RID: 6651 RVA: 0x000AC26D File Offset: 0x000AA46D
	public void SetState(bool enabled)
	{
		this.m_state = enabled;
		this.m_renderer.material = (this.m_state ? this.m_enableMaterial : this.m_disableMaterial);
	}

	// Token: 0x04001BCE RID: 7118
	public MeshRenderer m_renderer;

	// Token: 0x04001BCF RID: 7119
	public Material m_enableMaterial;

	// Token: 0x04001BD0 RID: 7120
	public Material m_disableMaterial;

	// Token: 0x04001BD1 RID: 7121
	public Action<ToggleSwitch, Humanoid> m_onUse;

	// Token: 0x04001BD2 RID: 7122
	public string m_hoverText = "";

	// Token: 0x04001BD3 RID: 7123
	public string m_name = "";

	// Token: 0x04001BD4 RID: 7124
	private bool m_state;
}
