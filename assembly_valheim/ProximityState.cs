using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200027C RID: 636
public class ProximityState : MonoBehaviour
{
	// Token: 0x06001868 RID: 6248 RVA: 0x000A2C3C File Offset: 0x000A0E3C
	private void Start()
	{
		this.m_animator.SetBool("near", false);
	}

	// Token: 0x06001869 RID: 6249 RVA: 0x000A2C50 File Offset: 0x000A0E50
	private void OnTriggerEnter(Collider other)
	{
		if (this.m_playerOnly)
		{
			Character component = other.GetComponent<Character>();
			if (!component || !component.IsPlayer())
			{
				return;
			}
		}
		if (this.m_near.Contains(other))
		{
			return;
		}
		this.m_near.Add(other);
		if (!this.m_animator.GetBool("near"))
		{
			this.m_animator.SetBool("near", true);
			this.m_movingClose.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		}
	}

	// Token: 0x0600186A RID: 6250 RVA: 0x000A2CE4 File Offset: 0x000A0EE4
	private void OnTriggerExit(Collider other)
	{
		this.m_near.Remove(other);
		if (this.m_near.Count == 0 && this.m_animator.GetBool("near"))
		{
			this.m_animator.SetBool("near", false);
			this.m_movingAway.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		}
	}

	// Token: 0x04001A25 RID: 6693
	public bool m_playerOnly = true;

	// Token: 0x04001A26 RID: 6694
	public Animator m_animator;

	// Token: 0x04001A27 RID: 6695
	public EffectList m_movingClose = new EffectList();

	// Token: 0x04001A28 RID: 6696
	public EffectList m_movingAway = new EffectList();

	// Token: 0x04001A29 RID: 6697
	private List<Collider> m_near = new List<Collider>();
}
