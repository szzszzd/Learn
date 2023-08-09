using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001D4 RID: 468
public class NavmeshTest : MonoBehaviour
{
	// Token: 0x0600132B RID: 4907 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Awake()
	{
	}

	// Token: 0x0600132C RID: 4908 RVA: 0x0007E6F8 File Offset: 0x0007C8F8
	private void Update()
	{
		if (Pathfinding.instance.GetPath(base.transform.position, this.m_target.position, this.m_path, this.m_agentType, false, this.m_cleanPath, false))
		{
			this.m_havePath = true;
			return;
		}
		this.m_havePath = false;
	}

	// Token: 0x0600132D RID: 4909 RVA: 0x0007E74C File Offset: 0x0007C94C
	private void OnDrawGizmos()
	{
		if (this.m_target == null)
		{
			return;
		}
		if (this.m_havePath)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < this.m_path.Count - 1; i++)
			{
				Vector3 a = this.m_path[i];
				Vector3 a2 = this.m_path[i + 1];
				Gizmos.DrawLine(a + Vector3.up * 0.2f, a2 + Vector3.up * 0.2f);
			}
			foreach (Vector3 a3 in this.m_path)
			{
				Gizmos.DrawSphere(a3 + Vector3.up * 0.2f, 0.1f);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(base.transform.position, 0.3f);
			Gizmos.DrawSphere(this.m_target.position, 0.3f);
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position + Vector3.up * 0.2f, this.m_target.position + Vector3.up * 0.2f);
		Gizmos.DrawSphere(base.transform.position, 0.3f);
		Gizmos.DrawSphere(this.m_target.position, 0.3f);
	}

	// Token: 0x04001417 RID: 5143
	public Transform m_target;

	// Token: 0x04001418 RID: 5144
	public Pathfinding.AgentType m_agentType = Pathfinding.AgentType.Humanoid;

	// Token: 0x04001419 RID: 5145
	public bool m_cleanPath = true;

	// Token: 0x0400141A RID: 5146
	private List<Vector3> m_path = new List<Vector3>();

	// Token: 0x0400141B RID: 5147
	private bool m_havePath;
}
