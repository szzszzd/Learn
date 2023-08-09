using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200008C RID: 140
public class SmokeRenderer : MonoBehaviour
{
	// Token: 0x0600062C RID: 1580 RVA: 0x0002F243 File Offset: 0x0002D443
	private void Start()
	{
		this.m_instanceRenderer = base.GetComponent<InstanceRenderer>();
	}

	// Token: 0x0600062D RID: 1581 RVA: 0x0002F251 File Offset: 0x0002D451
	private void Update()
	{
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		this.UpdateInstances();
	}

	// Token: 0x0600062E RID: 1582 RVA: 0x000023E2 File Offset: 0x000005E2
	private void UpdateInstances()
	{
	}

	// Token: 0x04000770 RID: 1904
	private InstanceRenderer m_instanceRenderer;

	// Token: 0x04000771 RID: 1905
	private List<Vector4> tempTransforms = new List<Vector4>();
}
