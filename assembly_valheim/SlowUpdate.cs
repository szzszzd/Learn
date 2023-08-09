using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001F8 RID: 504
public class SlowUpdate : MonoBehaviour
{
	// Token: 0x0600144A RID: 5194 RVA: 0x00084468 File Offset: 0x00082668
	public virtual void Awake()
	{
		SlowUpdate.m_allInstances.Add(this);
		this.m_myIndex = SlowUpdate.m_allInstances.Count - 1;
	}

	// Token: 0x0600144B RID: 5195 RVA: 0x00084488 File Offset: 0x00082688
	public virtual void OnDestroy()
	{
		if (this.m_myIndex != -1)
		{
			SlowUpdate.m_allInstances[this.m_myIndex] = SlowUpdate.m_allInstances[SlowUpdate.m_allInstances.Count - 1];
			SlowUpdate.m_allInstances[this.m_myIndex].m_myIndex = this.m_myIndex;
			SlowUpdate.m_allInstances.RemoveAt(SlowUpdate.m_allInstances.Count - 1);
		}
	}

	// Token: 0x0600144C RID: 5196 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual void SUpdate()
	{
	}

	// Token: 0x0600144D RID: 5197 RVA: 0x000844F5 File Offset: 0x000826F5
	public static List<SlowUpdate> GetAllInstaces()
	{
		return SlowUpdate.m_allInstances;
	}

	// Token: 0x040014EE RID: 5358
	private static List<SlowUpdate> m_allInstances = new List<SlowUpdate>();

	// Token: 0x040014EF RID: 5359
	private int m_myIndex = -1;
}
