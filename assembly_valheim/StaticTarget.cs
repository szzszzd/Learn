using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000299 RID: 665
public class StaticTarget : MonoBehaviour
{
	// Token: 0x06001971 RID: 6513 RVA: 0x000A90F3 File Offset: 0x000A72F3
	public virtual bool IsPriorityTarget()
	{
		return this.m_primaryTarget;
	}

	// Token: 0x06001972 RID: 6514 RVA: 0x000A90FB File Offset: 0x000A72FB
	public virtual bool IsRandomTarget()
	{
		return this.m_randomTarget;
	}

	// Token: 0x06001973 RID: 6515 RVA: 0x000A9104 File Offset: 0x000A7304
	public Vector3 GetCenter()
	{
		if (!this.m_haveCenter)
		{
			List<Collider> allColliders = this.GetAllColliders();
			this.m_localCenter = Vector3.zero;
			foreach (Collider collider in allColliders)
			{
				if (collider)
				{
					this.m_localCenter += collider.bounds.center;
				}
			}
			this.m_localCenter /= (float)this.m_colliders.Count;
			this.m_localCenter = base.transform.InverseTransformPoint(this.m_localCenter);
			this.m_haveCenter = true;
		}
		return base.transform.TransformPoint(this.m_localCenter);
	}

	// Token: 0x06001974 RID: 6516 RVA: 0x000A91DC File Offset: 0x000A73DC
	public List<Collider> GetAllColliders()
	{
		if (this.m_colliders == null)
		{
			Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
			this.m_colliders = new List<Collider>();
			this.m_colliders.Capacity = componentsInChildren.Length;
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger)
				{
					this.m_colliders.Add(collider);
				}
			}
		}
		return this.m_colliders;
	}

	// Token: 0x06001975 RID: 6517 RVA: 0x000A9254 File Offset: 0x000A7454
	public Vector3 FindClosestPoint(Vector3 point)
	{
		List<Collider> allColliders = this.GetAllColliders();
		if (allColliders.Count == 0)
		{
			return base.transform.position;
		}
		float num = 9999999f;
		Vector3 result = Vector3.zero;
		foreach (Collider collider in allColliders)
		{
			if (collider)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				Vector3 vector = (meshCollider && !meshCollider.convex) ? collider.ClosestPointOnBounds(point) : collider.ClosestPoint(point);
				float num2 = Vector3.Distance(point, vector);
				if (num2 < num)
				{
					result = vector;
					num = num2;
				}
			}
		}
		return result;
	}

	// Token: 0x04001B5A RID: 7002
	[Header("Static target")]
	public bool m_primaryTarget;

	// Token: 0x04001B5B RID: 7003
	public bool m_randomTarget = true;

	// Token: 0x04001B5C RID: 7004
	private List<Collider> m_colliders;

	// Token: 0x04001B5D RID: 7005
	private Vector3 m_localCenter;

	// Token: 0x04001B5E RID: 7006
	private bool m_haveCenter;
}
