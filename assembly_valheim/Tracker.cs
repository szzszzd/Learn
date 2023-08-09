using System;
using UnityEngine;

// Token: 0x020002AD RID: 685
public class Tracker : MonoBehaviour
{
	// Token: 0x060019FD RID: 6653 RVA: 0x000AC2B8 File Offset: 0x000AA4B8
	private void Awake()
	{
		ZNetView component = base.GetComponent<ZNetView>();
		if (component && component.IsOwner())
		{
			this.m_active = true;
			ZNet.instance.SetReferencePosition(base.transform.position);
		}
	}

	// Token: 0x060019FE RID: 6654 RVA: 0x000AC2F8 File Offset: 0x000AA4F8
	public void SetActive(bool active)
	{
		this.m_active = active;
	}

	// Token: 0x060019FF RID: 6655 RVA: 0x000AC301 File Offset: 0x000AA501
	private void OnDestroy()
	{
		this.m_active = false;
	}

	// Token: 0x06001A00 RID: 6656 RVA: 0x000AC30A File Offset: 0x000AA50A
	private void FixedUpdate()
	{
		if (this.m_active)
		{
			ZNet.instance.SetReferencePosition(base.transform.position);
		}
	}

	// Token: 0x04001BD5 RID: 7125
	private bool m_active;
}
