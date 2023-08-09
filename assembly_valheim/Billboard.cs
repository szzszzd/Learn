using System;
using UnityEngine;

// Token: 0x02000219 RID: 537
public class Billboard : MonoBehaviour
{
	// Token: 0x0600155F RID: 5471 RVA: 0x0008C35F File Offset: 0x0008A55F
	private void Awake()
	{
		this.m_normal = base.transform.up;
	}

	// Token: 0x06001560 RID: 5472 RVA: 0x0008C374 File Offset: 0x0008A574
	private void LateUpdate()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 vector = mainCamera.transform.position;
		if (this.m_invert)
		{
			vector = base.transform.position - (vector - base.transform.position);
		}
		if (this.m_vertical)
		{
			vector.y = base.transform.position.y;
			base.transform.LookAt(vector, this.m_normal);
			return;
		}
		base.transform.LookAt(vector);
	}

	// Token: 0x0400162E RID: 5678
	public bool m_vertical = true;

	// Token: 0x0400162F RID: 5679
	public bool m_invert;

	// Token: 0x04001630 RID: 5680
	private Vector3 m_normal;
}
