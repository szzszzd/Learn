using System;
using UnityEngine;

// Token: 0x02000215 RID: 533
public class AutoJumpLedge : MonoBehaviour
{
	// Token: 0x06001537 RID: 5431 RVA: 0x0008B81C File Offset: 0x00089A1C
	private void OnTriggerStay(Collider collider)
	{
		Character component = collider.GetComponent<Character>();
		if (component)
		{
			component.OnAutoJump(base.transform.forward, this.m_upVel, this.m_forwardVel);
		}
	}

	// Token: 0x0400160E RID: 5646
	public bool m_forwardOnly = true;

	// Token: 0x0400160F RID: 5647
	public float m_upVel = 1f;

	// Token: 0x04001610 RID: 5648
	public float m_forwardVel = 1f;
}
