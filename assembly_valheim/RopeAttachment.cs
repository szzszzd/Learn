using System;
using UnityEngine;

// Token: 0x02000287 RID: 647
public class RopeAttachment : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x060018BA RID: 6330 RVA: 0x000A4DF1 File Offset: 0x000A2FF1
	private void Awake()
	{
		this.m_boatBody = base.GetComponentInParent<Rigidbody>();
	}

	// Token: 0x060018BB RID: 6331 RVA: 0x000A4DFF File Offset: 0x000A2FFF
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_puller)
		{
			this.m_puller = null;
			ZLog.Log("Detached rope");
		}
		else
		{
			this.m_puller = character;
			ZLog.Log("Attached rope");
		}
		return true;
	}

	// Token: 0x060018BC RID: 6332 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060018BD RID: 6333 RVA: 0x000A4E38 File Offset: 0x000A3038
	public string GetHoverText()
	{
		return this.m_hoverText;
	}

	// Token: 0x060018BE RID: 6334 RVA: 0x000A4E40 File Offset: 0x000A3040
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060018BF RID: 6335 RVA: 0x000A4E48 File Offset: 0x000A3048
	private void FixedUpdate()
	{
		if (this.m_puller && Vector3.Distance(this.m_puller.transform.position, base.transform.position) > this.m_pullDistance)
		{
			Vector3 position = ((this.m_puller.transform.position - base.transform.position).normalized * this.m_maxPullVel - this.m_boatBody.GetPointVelocity(base.transform.position)) * this.m_pullForce;
			this.m_boatBody.AddForceAtPosition(base.transform.position, position);
		}
	}

	// Token: 0x04001AA6 RID: 6822
	public string m_name = "Rope";

	// Token: 0x04001AA7 RID: 6823
	public string m_hoverText = "Pull";

	// Token: 0x04001AA8 RID: 6824
	public float m_pullDistance = 5f;

	// Token: 0x04001AA9 RID: 6825
	public float m_pullForce = 1f;

	// Token: 0x04001AAA RID: 6826
	public float m_maxPullVel = 1f;

	// Token: 0x04001AAB RID: 6827
	private Rigidbody m_boatBody;

	// Token: 0x04001AAC RID: 6828
	private Character m_puller;
}
