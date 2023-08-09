using System;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x0200021C RID: 540
public class Chair : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001571 RID: 5489 RVA: 0x0008C7C4 File Offset: 0x0008A9C4
	public string GetHoverText()
	{
		if (Time.time - Chair.m_lastSitTime < 2f)
		{
			return "";
		}
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06001572 RID: 5490 RVA: 0x0008C820 File Offset: 0x0008AA20
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001573 RID: 5491 RVA: 0x0008C828 File Offset: 0x0008AA28
	public bool Interact(Humanoid human, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		Player player = human as Player;
		if (!this.InUseDistance(player))
		{
			return false;
		}
		if (Time.time - Chair.m_lastSitTime < 2f)
		{
			return false;
		}
		if (player)
		{
			if (player.IsEncumbered())
			{
				return false;
			}
			player.AttachStart(this.m_attachPoint, null, false, false, this.m_inShip, this.m_attachAnimation, this.m_detachOffset);
			Chair.m_lastSitTime = Time.time;
		}
		return false;
	}

	// Token: 0x06001574 RID: 5492 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001575 RID: 5493 RVA: 0x0008C89E File Offset: 0x0008AA9E
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, this.m_attachPoint.position) < this.m_useDistance;
	}

	// Token: 0x04001642 RID: 5698
	public string m_name = "Chair";

	// Token: 0x04001643 RID: 5699
	public float m_useDistance = 2f;

	// Token: 0x04001644 RID: 5700
	public Transform m_attachPoint;

	// Token: 0x04001645 RID: 5701
	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x04001646 RID: 5702
	public string m_attachAnimation = "attach_chair";

	// Token: 0x04001647 RID: 5703
	[FormerlySerializedAs("m_onShip")]
	public bool m_inShip;

	// Token: 0x04001648 RID: 5704
	private const float m_minSitDelay = 2f;

	// Token: 0x04001649 RID: 5705
	private static float m_lastSitTime;
}
