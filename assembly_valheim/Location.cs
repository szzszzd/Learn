using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000262 RID: 610
public class Location : MonoBehaviour
{
	// Token: 0x06001794 RID: 6036 RVA: 0x0009CDBC File Offset: 0x0009AFBC
	private void Awake()
	{
		Location.m_allLocations.Add(this);
		if (this.m_hasInterior)
		{
			Vector3 zoneCenter = this.GetZoneCenter();
			Vector3 position = new Vector3(zoneCenter.x, base.transform.position.y + 5000f, zoneCenter.z);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_interiorPrefab, position, Quaternion.identity, base.transform);
			gameObject.transform.localScale = new Vector3(ZoneSystem.instance.m_zoneSize, 500f, ZoneSystem.instance.m_zoneSize);
			gameObject.GetComponent<EnvZone>().m_environment = this.m_interiorEnvironment;
		}
	}

	// Token: 0x06001795 RID: 6037 RVA: 0x0009CE60 File Offset: 0x0009B060
	private Vector3 GetZoneCenter()
	{
		Vector2i zone = ZoneSystem.instance.GetZone(base.transform.position);
		return ZoneSystem.instance.GetZonePos(zone);
	}

	// Token: 0x06001796 RID: 6038 RVA: 0x0009CE8E File Offset: 0x0009B08E
	private void OnDestroy()
	{
		Location.m_allLocations.Remove(this);
	}

	// Token: 0x06001797 RID: 6039 RVA: 0x0009CE9C File Offset: 0x0009B09C
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position + new Vector3(0f, -0.01f, 0f), Quaternion.identity, new Vector3(1f, 0.001f, 1f));
		Gizmos.DrawSphere(Vector3.zero, this.m_exteriorRadius);
		Gizmos.matrix = Matrix4x4.identity;
		Utils.DrawGizmoCircle(base.transform.position, this.m_exteriorRadius, 32);
		if (this.m_hasInterior)
		{
			Utils.DrawGizmoCircle(base.transform.position + new Vector3(0f, 5000f, 0f), this.m_interiorRadius, 32);
			Utils.DrawGizmoCircle(base.transform.position, this.m_interiorRadius, 32);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position + new Vector3(0f, 5000f, 0f), Quaternion.identity, new Vector3(1f, 0.001f, 1f));
			Gizmos.DrawSphere(Vector3.zero, this.m_interiorRadius);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	// Token: 0x06001798 RID: 6040 RVA: 0x0009CFF5 File Offset: 0x0009B1F5
	public float GetMaxRadius()
	{
		if (!this.m_hasInterior)
		{
			return this.m_exteriorRadius;
		}
		return Mathf.Max(this.m_exteriorRadius, this.m_interiorRadius);
	}

	// Token: 0x06001799 RID: 6041 RVA: 0x0009D018 File Offset: 0x0009B218
	public bool IsInside(Vector3 point, float radius)
	{
		float maxRadius = this.GetMaxRadius();
		return Utils.DistanceXZ(base.transform.position, point) < maxRadius + radius;
	}

	// Token: 0x0600179A RID: 6042 RVA: 0x0009D044 File Offset: 0x0009B244
	public static bool IsInsideLocation(Vector3 point, float distance)
	{
		using (List<Location>.Enumerator enumerator = Location.m_allLocations.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsInside(point, distance))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600179B RID: 6043 RVA: 0x0009D0A0 File Offset: 0x0009B2A0
	public static Location GetLocation(Vector3 point)
	{
		foreach (Location location in Location.m_allLocations)
		{
			if (location.IsInside(point, 0f))
			{
				return location;
			}
		}
		return null;
	}

	// Token: 0x0600179C RID: 6044 RVA: 0x0009D100 File Offset: 0x0009B300
	public static bool IsInsideNoBuildLocation(Vector3 point)
	{
		foreach (Location location in Location.m_allLocations)
		{
			if (location.m_noBuild && location.IsInside(point, 0f))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x04001901 RID: 6401
	[FormerlySerializedAs("m_radius")]
	public float m_exteriorRadius = 20f;

	// Token: 0x04001902 RID: 6402
	public bool m_noBuild = true;

	// Token: 0x04001903 RID: 6403
	public bool m_clearArea = true;

	// Token: 0x04001904 RID: 6404
	[Header("Other")]
	public bool m_applyRandomDamage;

	// Token: 0x04001905 RID: 6405
	[Header("Interior")]
	public bool m_hasInterior;

	// Token: 0x04001906 RID: 6406
	public float m_interiorRadius = 20f;

	// Token: 0x04001907 RID: 6407
	public string m_interiorEnvironment = "";

	// Token: 0x04001908 RID: 6408
	public Transform m_interiorTransform;

	// Token: 0x04001909 RID: 6409
	public bool m_useCustomInteriorTransform;

	// Token: 0x0400190A RID: 6410
	public DungeonGenerator m_generator;

	// Token: 0x0400190B RID: 6411
	public GameObject m_interiorPrefab;

	// Token: 0x0400190C RID: 6412
	private static List<Location> m_allLocations = new List<Location>();
}
