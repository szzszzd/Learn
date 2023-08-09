using System;
using UnityEngine;

// Token: 0x020002AB RID: 683
public class TimedDestruction : MonoBehaviour
{
	// Token: 0x060019F2 RID: 6642 RVA: 0x000AC1A5 File Offset: 0x000AA3A5
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_triggerOnAwake)
		{
			this.Trigger();
		}
	}

	// Token: 0x060019F3 RID: 6643 RVA: 0x000AC1C1 File Offset: 0x000AA3C1
	public void Trigger()
	{
		base.InvokeRepeating("DestroyNow", this.m_timeout, 1f);
	}

	// Token: 0x060019F4 RID: 6644 RVA: 0x00008034 File Offset: 0x00006234
	public void Trigger(float timeout)
	{
		base.InvokeRepeating("DestroyNow", timeout, 1f);
	}

	// Token: 0x060019F5 RID: 6645 RVA: 0x000AC1DC File Offset: 0x000AA3DC
	private void DestroyNow()
	{
		if (!this.m_nview)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x04001BCB RID: 7115
	public float m_timeout = 1f;

	// Token: 0x04001BCC RID: 7116
	public bool m_triggerOnAwake;

	// Token: 0x04001BCD RID: 7117
	private ZNetView m_nview;
}
