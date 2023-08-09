using System;
using UnityEngine;

// Token: 0x020002BF RID: 703
public class WayStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001AA1 RID: 6817 RVA: 0x000B0ECB File Offset: 0x000AF0CB
	private void Awake()
	{
		this.m_activeObject.SetActive(false);
	}

	// Token: 0x06001AA2 RID: 6818 RVA: 0x000B0ED9 File Offset: 0x000AF0D9
	public string GetHoverText()
	{
		if (this.m_activeObject.activeSelf)
		{
			return "Activated waystone";
		}
		return Localization.instance.Localize("Waystone\n[<color=yellow><b>$KEY_Use</b></color>] Activate");
	}

	// Token: 0x06001AA3 RID: 6819 RVA: 0x000B0EFD File Offset: 0x000AF0FD
	public string GetHoverName()
	{
		return "Waystone";
	}

	// Token: 0x06001AA4 RID: 6820 RVA: 0x000B0F04 File Offset: 0x000AF104
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!this.m_activeObject.activeSelf)
		{
			character.Message(MessageHud.MessageType.Center, this.m_activateMessage, 0, null);
			this.m_activeObject.SetActive(true);
			this.m_activeEffect.Create(base.gameObject.transform.position, base.gameObject.transform.rotation, null, 1f, -1);
		}
		return true;
	}

	// Token: 0x06001AA5 RID: 6821 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001AA6 RID: 6822 RVA: 0x000B0F74 File Offset: 0x000AF174
	private void FixedUpdate()
	{
		if (this.m_activeObject.activeSelf && Game.instance != null)
		{
			Vector3 forward = this.GetSpawnPoint() - base.transform.position;
			forward.y = 0f;
			forward.Normalize();
			this.m_activeObject.transform.rotation = Quaternion.LookRotation(forward);
		}
	}

	// Token: 0x06001AA7 RID: 6823 RVA: 0x000B0FDC File Offset: 0x000AF1DC
	private Vector3 GetSpawnPoint()
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		if (playerProfile.HaveCustomSpawnPoint())
		{
			return playerProfile.GetCustomSpawnPoint();
		}
		return playerProfile.GetHomePoint();
	}

	// Token: 0x04001CB9 RID: 7353
	[TextArea]
	public string m_activateMessage = "You touch the cold stone surface and you think of home.";

	// Token: 0x04001CBA RID: 7354
	public GameObject m_activeObject;

	// Token: 0x04001CBB RID: 7355
	public EffectList m_activeEffect;
}
