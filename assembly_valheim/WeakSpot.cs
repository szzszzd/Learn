using System;
using UnityEngine;

// Token: 0x0200003F RID: 63
public class WeakSpot : MonoBehaviour
{
	// Token: 0x060003B8 RID: 952 RVA: 0x0001C770 File Offset: 0x0001A970
	private void Awake()
	{
		this.m_collider = base.GetComponent<Collider>();
	}

	// Token: 0x040003BB RID: 955
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x040003BC RID: 956
	[NonSerialized]
	public Collider m_collider;
}
