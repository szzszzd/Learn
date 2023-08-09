using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200026C RID: 620
public class Mister : MonoBehaviour
{
	// Token: 0x060017DC RID: 6108 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Awake()
	{
	}

	// Token: 0x060017DD RID: 6109 RVA: 0x0009F07A File Offset: 0x0009D27A
	private void OnEnable()
	{
		Mister.m_instances.Add(this);
	}

	// Token: 0x060017DE RID: 6110 RVA: 0x0009F087 File Offset: 0x0009D287
	private void OnDisable()
	{
		Mister.m_instances.Remove(this);
	}

	// Token: 0x060017DF RID: 6111 RVA: 0x0009F095 File Offset: 0x0009D295
	public static List<Mister> GetMisters()
	{
		return Mister.m_instances;
	}

	// Token: 0x060017E0 RID: 6112 RVA: 0x0009F09C File Offset: 0x0009D29C
	public static List<Mister> GetDemistersSorted(Vector3 refPoint)
	{
		foreach (Mister mister in Mister.m_instances)
		{
			mister.m_tempDistance = Vector3.Distance(mister.transform.position, refPoint);
		}
		Mister.m_instances.Sort((Mister a, Mister b) => a.m_tempDistance.CompareTo(b.m_tempDistance));
		return Mister.m_instances;
	}

	// Token: 0x060017E1 RID: 6113 RVA: 0x0009F12C File Offset: 0x0009D32C
	public static Mister FindMister(Vector3 p)
	{
		foreach (Mister mister in Mister.m_instances)
		{
			if (Vector3.Distance(mister.transform.position, p) < mister.m_radius)
			{
				return mister;
			}
		}
		return null;
	}

	// Token: 0x060017E2 RID: 6114 RVA: 0x0009F198 File Offset: 0x0009D398
	public static bool InsideMister(Vector3 p, float radius = 0f)
	{
		foreach (Mister mister in Mister.m_instances)
		{
			if (Vector3.Distance(mister.transform.position, p) < mister.m_radius + radius && p.y - radius < mister.transform.position.y + mister.m_height)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060017E3 RID: 6115 RVA: 0x0009F228 File Offset: 0x0009D428
	public bool IsCompletelyInsideOtherMister(float thickness)
	{
		Vector3 position = base.transform.position;
		foreach (Mister mister in Mister.m_instances)
		{
			if (!(mister == this) && Vector3.Distance(position, mister.transform.position) + this.m_radius + thickness < mister.m_radius && position.y + this.m_height < mister.transform.position.y + mister.m_height)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060017E4 RID: 6116 RVA: 0x0009F2D8 File Offset: 0x0009D4D8
	public bool Inside(Vector3 p, float radius)
	{
		return Vector3.Distance(p, base.transform.position) < radius && p.y - radius < base.transform.position.y + this.m_height;
	}

	// Token: 0x060017E5 RID: 6117 RVA: 0x0009F314 File Offset: 0x0009D514
	public static bool IsInsideOtherMister(Vector3 p, Mister ignore)
	{
		foreach (Mister mister in Mister.m_instances)
		{
			if (!(mister == ignore) && Vector3.Distance(p, mister.transform.position) < mister.m_radius && p.y < mister.transform.position.y + mister.m_height)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060017E6 RID: 6118 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmosSelected()
	{
	}

	// Token: 0x0400195E RID: 6494
	public float m_radius = 50f;

	// Token: 0x0400195F RID: 6495
	public float m_height = 10f;

	// Token: 0x04001960 RID: 6496
	private float m_tempDistance;

	// Token: 0x04001961 RID: 6497
	private static List<Mister> m_instances = new List<Mister>();
}
