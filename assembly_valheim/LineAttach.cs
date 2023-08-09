using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007A RID: 122
[ExecuteInEditMode]
public class LineAttach : MonoBehaviour
{
	// Token: 0x06000599 RID: 1433 RVA: 0x0002BE0D File Offset: 0x0002A00D
	private void Start()
	{
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x0002BE1C File Offset: 0x0002A01C
	private void LateUpdate()
	{
		for (int i = 0; i < this.m_attachments.Count; i++)
		{
			Transform transform = this.m_attachments[i];
			if (transform)
			{
				this.m_lineRenderer.SetPosition(i, base.transform.InverseTransformPoint(transform.position));
			}
		}
	}

	// Token: 0x040006BE RID: 1726
	public List<Transform> m_attachments = new List<Transform>();

	// Token: 0x040006BF RID: 1727
	private LineRenderer m_lineRenderer;
}
