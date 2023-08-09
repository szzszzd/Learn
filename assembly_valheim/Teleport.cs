using System;
using UnityEngine;

// Token: 0x0200029E RID: 670
public class Teleport : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001997 RID: 6551 RVA: 0x000A97E3 File Offset: 0x000A79E3
	public string GetHoverText()
	{
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + this.m_hoverText);
	}

	// Token: 0x06001998 RID: 6552 RVA: 0x0000C988 File Offset: 0x0000AB88
	public string GetHoverName()
	{
		return "";
	}

	// Token: 0x06001999 RID: 6553 RVA: 0x000A9800 File Offset: 0x000A7A00
	private void OnTriggerEnter(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		this.Interact(component, false, false);
	}

	// Token: 0x0600199A RID: 6554 RVA: 0x000A9838 File Offset: 0x000A7A38
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_targetPoint == null)
		{
			return false;
		}
		if (character.TeleportTo(this.m_targetPoint.GetTeleportPoint(), this.m_targetPoint.transform.rotation, false))
		{
			if (this.m_enterText.Length > 0)
			{
				MessageHud.instance.ShowBiomeFoundMsg(this.m_enterText, false);
			}
			return true;
		}
		return false;
	}

	// Token: 0x0600199B RID: 6555 RVA: 0x000A98A0 File Offset: 0x000A7AA0
	private Vector3 GetTeleportPoint()
	{
		return base.transform.position + base.transform.forward - base.transform.up;
	}

	// Token: 0x0600199C RID: 6556 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x0600199D RID: 6557 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04001B6E RID: 7022
	public string m_hoverText = "$location_enter";

	// Token: 0x04001B6F RID: 7023
	public string m_enterText = "";

	// Token: 0x04001B70 RID: 7024
	public Teleport m_targetPoint;
}
