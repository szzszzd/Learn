using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000216 RID: 534
public class Beacon : MonoBehaviour
{
	// Token: 0x06001539 RID: 5433 RVA: 0x0008B87A File Offset: 0x00089A7A
	private void Awake()
	{
		Beacon.m_instances.Add(this);
	}

	// Token: 0x0600153A RID: 5434 RVA: 0x0008B887 File Offset: 0x00089A87
	private void OnDestroy()
	{
		Beacon.m_instances.Remove(this);
	}

	// Token: 0x0600153B RID: 5435 RVA: 0x0008B898 File Offset: 0x00089A98
	public static Beacon FindClosestBeaconInRange(Vector3 point)
	{
		Beacon beacon = null;
		float num = 999999f;
		foreach (Beacon beacon2 in Beacon.m_instances)
		{
			float num2 = Vector3.Distance(point, beacon2.transform.position);
			if (num2 < beacon2.m_range && (beacon == null || num2 < num))
			{
				beacon = beacon2;
				num = num2;
			}
		}
		return beacon;
	}

	// Token: 0x0600153C RID: 5436 RVA: 0x0008B91C File Offset: 0x00089B1C
	public static void FindBeaconsInRange(Vector3 point, List<Beacon> becons)
	{
		foreach (Beacon beacon in Beacon.m_instances)
		{
			if (Vector3.Distance(point, beacon.transform.position) < beacon.m_range)
			{
				becons.Add(beacon);
			}
		}
	}

	// Token: 0x04001611 RID: 5649
	public float m_range = 20f;

	// Token: 0x04001612 RID: 5650
	private static List<Beacon> m_instances = new List<Beacon>();
}
