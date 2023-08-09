using System;
using UnityEngine;

// Token: 0x02000243 RID: 579
public class HitArea : MonoBehaviour, IDestructible
{
	// Token: 0x060016E4 RID: 5860 RVA: 0x0000290F File Offset: 0x00000B0F
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x060016E5 RID: 5861 RVA: 0x000974F3 File Offset: 0x000956F3
	public void Damage(HitData hit)
	{
		if (this.m_onHit != null)
		{
			this.m_onHit(hit, this);
		}
	}

	// Token: 0x04001810 RID: 6160
	public Action<HitData, HitArea> m_onHit;

	// Token: 0x04001811 RID: 6161
	public float m_health = 1f;

	// Token: 0x04001812 RID: 6162
	[NonSerialized]
	public GameObject m_parentObject;
}
