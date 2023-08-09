using System;
using UnityEngine;

// Token: 0x020001E1 RID: 481
public class RenderGroupSubscriber : MonoBehaviour
{
	// Token: 0x060013BF RID: 5055 RVA: 0x00081F80 File Offset: 0x00080180
	private void OnEnable()
	{
		if (this.m_renderer == null)
		{
			this.m_renderer = base.GetComponent<MeshRenderer>();
		}
		if (this.m_renderer == null)
		{
			ZLog.LogError("RenderGroup script requires a mesh renderer!");
		}
		RenderGroupSystem.Register(this.m_group, new RenderGroupSystem.GroupChangedHandler(this.OnGroupChanged));
	}

	// Token: 0x060013C0 RID: 5056 RVA: 0x00081FD6 File Offset: 0x000801D6
	private void OnDisable()
	{
		RenderGroupSystem.Unregister(this.m_group, new RenderGroupSystem.GroupChangedHandler(this.OnGroupChanged));
	}

	// Token: 0x060013C1 RID: 5057 RVA: 0x00081FEF File Offset: 0x000801EF
	private void OnGroupChanged(bool shouldRender)
	{
		this.m_renderer.enabled = shouldRender;
	}

	// Token: 0x040014A2 RID: 5282
	private MeshRenderer m_renderer;

	// Token: 0x040014A3 RID: 5283
	[SerializeField]
	public RenderGroup m_group;
}
