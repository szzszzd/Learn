using System;
using UnityEngine;

// Token: 0x0200029F RID: 671
public class TeleportHome : MonoBehaviour
{
	// Token: 0x0600199F RID: 6559 RVA: 0x000A98EC File Offset: 0x000A7AEC
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
		Game.instance.RequestRespawn(0f);
	}
}
