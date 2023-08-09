using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200029A RID: 666
public class StationExtension : MonoBehaviour, Hoverable
{
	// Token: 0x06001977 RID: 6519 RVA: 0x000A9324 File Offset: 0x000A7524
	private void Awake()
	{
		if (base.GetComponent<ZNetView>().GetZDO() == null)
		{
			return;
		}
		this.m_piece = base.GetComponent<Piece>();
		StationExtension.m_allExtensions.Add(this);
		if (this.m_continousConnection)
		{
			base.InvokeRepeating("UpdateConnection", 1f, 4f);
		}
	}

	// Token: 0x06001978 RID: 6520 RVA: 0x000A9373 File Offset: 0x000A7573
	private void OnDestroy()
	{
		if (this.m_connection)
		{
			UnityEngine.Object.Destroy(this.m_connection);
			this.m_connection = null;
		}
		StationExtension.m_allExtensions.Remove(this);
	}

	// Token: 0x06001979 RID: 6521 RVA: 0x000A93A0 File Offset: 0x000A75A0
	public string GetHoverText()
	{
		if (!this.m_continousConnection)
		{
			this.PokeEffect(1f);
		}
		return Localization.instance.Localize(this.m_piece.m_name);
	}

	// Token: 0x0600197A RID: 6522 RVA: 0x000A93CA File Offset: 0x000A75CA
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_piece.m_name);
	}

	// Token: 0x0600197B RID: 6523 RVA: 0x000A93E1 File Offset: 0x000A75E1
	private string GetExtensionName()
	{
		return this.m_piece.m_name;
	}

	// Token: 0x0600197C RID: 6524 RVA: 0x000A93F0 File Offset: 0x000A75F0
	public static void FindExtensions(CraftingStation station, Vector3 pos, List<StationExtension> extensions)
	{
		foreach (StationExtension stationExtension in StationExtension.m_allExtensions)
		{
			if (Vector3.Distance(stationExtension.transform.position, pos) < stationExtension.m_maxStationDistance && stationExtension.m_craftingStation.m_name == station.m_name && (stationExtension.m_stack || !StationExtension.ExtensionInList(extensions, stationExtension)))
			{
				extensions.Add(stationExtension);
			}
		}
	}

	// Token: 0x0600197D RID: 6525 RVA: 0x000A9484 File Offset: 0x000A7684
	private static bool ExtensionInList(List<StationExtension> extensions, StationExtension extension)
	{
		using (List<StationExtension>.Enumerator enumerator = extensions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetExtensionName() == extension.GetExtensionName())
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600197E RID: 6526 RVA: 0x000A94E4 File Offset: 0x000A76E4
	public bool OtherExtensionInRange(float radius)
	{
		foreach (StationExtension stationExtension in StationExtension.m_allExtensions)
		{
			if (!(stationExtension == this) && Vector3.Distance(stationExtension.transform.position, base.transform.position) < radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600197F RID: 6527 RVA: 0x000A9560 File Offset: 0x000A7760
	public List<CraftingStation> FindStationsInRange(Vector3 center)
	{
		List<CraftingStation> list = new List<CraftingStation>();
		CraftingStation.FindStationsInRange(this.m_craftingStation.m_name, center, this.m_maxStationDistance, list);
		return list;
	}

	// Token: 0x06001980 RID: 6528 RVA: 0x000A958C File Offset: 0x000A778C
	public CraftingStation FindClosestStationInRange(Vector3 center)
	{
		return CraftingStation.FindClosestStationInRange(this.m_craftingStation.m_name, center, this.m_maxStationDistance);
	}

	// Token: 0x06001981 RID: 6529 RVA: 0x000A95A5 File Offset: 0x000A77A5
	private void UpdateConnection()
	{
		this.PokeEffect(5f);
	}

	// Token: 0x06001982 RID: 6530 RVA: 0x000A95B4 File Offset: 0x000A77B4
	private void PokeEffect(float timeout = 1f)
	{
		CraftingStation craftingStation = this.FindClosestStationInRange(base.transform.position);
		if (craftingStation)
		{
			this.StartConnectionEffect(craftingStation, timeout);
		}
	}

	// Token: 0x06001983 RID: 6531 RVA: 0x000A95E3 File Offset: 0x000A77E3
	public void StartConnectionEffect(CraftingStation station, float timeout = 1f)
	{
		this.StartConnectionEffect(station.GetConnectionEffectPoint(), timeout);
	}

	// Token: 0x06001984 RID: 6532 RVA: 0x000A95F4 File Offset: 0x000A77F4
	public void StartConnectionEffect(Vector3 targetPos, float timeout = 1f)
	{
		Vector3 connectionPoint = this.GetConnectionPoint();
		if (this.m_connection == null)
		{
			this.m_connection = UnityEngine.Object.Instantiate<GameObject>(this.m_connectionPrefab, connectionPoint, Quaternion.identity);
		}
		Vector3 vector = targetPos - connectionPoint;
		Quaternion rotation = Quaternion.LookRotation(vector.normalized);
		this.m_connection.transform.position = connectionPoint;
		this.m_connection.transform.rotation = rotation;
		this.m_connection.transform.localScale = new Vector3(1f, 1f, vector.magnitude);
		base.CancelInvoke("StopConnectionEffect");
		base.Invoke("StopConnectionEffect", timeout);
	}

	// Token: 0x06001985 RID: 6533 RVA: 0x000A96A1 File Offset: 0x000A78A1
	public void StopConnectionEffect()
	{
		if (this.m_connection)
		{
			UnityEngine.Object.Destroy(this.m_connection);
			this.m_connection = null;
		}
	}

	// Token: 0x06001986 RID: 6534 RVA: 0x000A96C2 File Offset: 0x000A78C2
	private Vector3 GetConnectionPoint()
	{
		return base.transform.TransformPoint(this.m_connectionOffset);
	}

	// Token: 0x06001987 RID: 6535 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04001B5F RID: 7007
	public CraftingStation m_craftingStation;

	// Token: 0x04001B60 RID: 7008
	public float m_maxStationDistance = 5f;

	// Token: 0x04001B61 RID: 7009
	public bool m_stack;

	// Token: 0x04001B62 RID: 7010
	public GameObject m_connectionPrefab;

	// Token: 0x04001B63 RID: 7011
	public Vector3 m_connectionOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x04001B64 RID: 7012
	public bool m_continousConnection;

	// Token: 0x04001B65 RID: 7013
	private GameObject m_connection;

	// Token: 0x04001B66 RID: 7014
	private Piece m_piece;

	// Token: 0x04001B67 RID: 7015
	private static List<StationExtension> m_allExtensions = new List<StationExtension>();
}
