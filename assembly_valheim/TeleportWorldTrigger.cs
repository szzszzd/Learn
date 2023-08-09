using System;
using UnityEngine;

// Token: 0x020002A1 RID: 673
public class TeleportWorldTrigger : MonoBehaviour
{
	// Token: 0x060019B0 RID: 6576 RVA: 0x000A9E36 File Offset: 0x000A8036
	private void Awake()
	{
		this.m_teleportWorld = base.GetComponentInParent<TeleportWorld>();
	}

	// Token: 0x060019B1 RID: 6577 RVA: 0x000A9E44 File Offset: 0x000A8044
	private void OnTriggerEnter(Collider colliderIn)
	{
		Player component = colliderIn.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		ZLog.Log("Teleportation TRIGGER");
		this.m_teleportWorld.Teleport(component);
	}

	// Token: 0x04001B7C RID: 7036
	private TeleportWorld m_teleportWorld;
}
