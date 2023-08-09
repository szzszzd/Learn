using System;
using UnityEngine;

// Token: 0x02000232 RID: 562
public class EventZone : MonoBehaviour
{
	// Token: 0x0600160B RID: 5643 RVA: 0x00090BC4 File Offset: 0x0008EDC4
	private void OnTriggerStay(Collider collider)
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
		EventZone.m_triggered = this;
	}

	// Token: 0x0600160C RID: 5644 RVA: 0x00090BF8 File Offset: 0x0008EDF8
	private void OnTriggerExit(Collider collider)
	{
		if (EventZone.m_triggered != this)
		{
			return;
		}
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		EventZone.m_triggered = null;
	}

	// Token: 0x0600160D RID: 5645 RVA: 0x00090C38 File Offset: 0x0008EE38
	public static string GetEvent()
	{
		if (EventZone.m_triggered && EventZone.m_triggered.m_event.Length > 0)
		{
			return EventZone.m_triggered.m_event;
		}
		return null;
	}

	// Token: 0x04001707 RID: 5895
	public string m_event = "";

	// Token: 0x04001708 RID: 5896
	private static EventZone m_triggered;
}
