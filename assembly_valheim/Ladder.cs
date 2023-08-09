using System;
using UnityEngine;

// Token: 0x0200025A RID: 602
public class Ladder : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x0600174B RID: 5963 RVA: 0x0009A50C File Offset: 0x0009870C
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!this.InUseDistance(character))
		{
			return false;
		}
		character.transform.position = this.m_targetPos.position;
		character.transform.rotation = this.m_targetPos.rotation;
		character.SetLookDir(this.m_targetPos.forward, 0f);
		return false;
	}

	// Token: 0x0600174C RID: 5964 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x0600174D RID: 5965 RVA: 0x0009A56C File Offset: 0x0009876C
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x0600174E RID: 5966 RVA: 0x0009A5A5 File Offset: 0x000987A5
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x0600174F RID: 5967 RVA: 0x0009A5AD File Offset: 0x000987AD
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, base.transform.position) < this.m_useDistance;
	}

	// Token: 0x040018AA RID: 6314
	public Transform m_targetPos;

	// Token: 0x040018AB RID: 6315
	public string m_name = "Ladder";

	// Token: 0x040018AC RID: 6316
	public float m_useDistance = 2f;
}
