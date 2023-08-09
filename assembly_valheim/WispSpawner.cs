using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002C6 RID: 710
public class WispSpawner : MonoBehaviour, Hoverable
{
	// Token: 0x06001AD8 RID: 6872 RVA: 0x000B2B24 File Offset: 0x000B0D24
	private void Start()
	{
		WispSpawner.s_spawners.Add(this);
		this.m_nview = base.GetComponentInParent<ZNetView>();
		base.InvokeRepeating("TrySpawn", 10f, 10f);
		base.InvokeRepeating("UpdateDemister", UnityEngine.Random.Range(0f, 2f), 2f);
	}

	// Token: 0x06001AD9 RID: 6873 RVA: 0x000B2B7C File Offset: 0x000B0D7C
	private void OnDestroy()
	{
		WispSpawner.s_spawners.Remove(this);
	}

	// Token: 0x06001ADA RID: 6874 RVA: 0x000B2B8C File Offset: 0x000B0D8C
	public string GetHoverText()
	{
		switch (this.GetStatus())
		{
		case WispSpawner.Status.NoSpace:
			return Localization.instance.Localize(this.m_name + " ( $piece_wisplure_nospace )");
		case WispSpawner.Status.TooBright:
			return Localization.instance.Localize(this.m_name + " ( $piece_wisplure_light )");
		case WispSpawner.Status.Full:
			return Localization.instance.Localize(this.m_name + " ( $piece_wisplure_full )");
		case WispSpawner.Status.Ok:
			return Localization.instance.Localize(this.m_name + " ( $piece_wisplure_ok )");
		default:
			return "";
		}
	}

	// Token: 0x06001ADB RID: 6875 RVA: 0x000B2C29 File Offset: 0x000B0E29
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001ADC RID: 6876 RVA: 0x000B2C34 File Offset: 0x000B0E34
	private void UpdateDemister()
	{
		if (this.m_wispsNearbyObject)
		{
			int wispsInArea = LuredWisp.GetWispsInArea(this.m_spawnPoint.position, this.m_nearbyTreshold);
			this.m_wispsNearbyObject.SetActive(wispsInArea > 0);
		}
	}

	// Token: 0x06001ADD RID: 6877 RVA: 0x000B2C74 File Offset: 0x000B0E74
	private WispSpawner.Status GetStatus()
	{
		if (Time.time - this.m_lastStatusUpdate < 4f)
		{
			return this.m_status;
		}
		this.m_lastStatusUpdate = Time.time;
		this.m_status = WispSpawner.Status.Ok;
		if (!this.HaveFreeSpace())
		{
			this.m_status = WispSpawner.Status.NoSpace;
		}
		else if (this.m_onlySpawnAtNight && EnvMan.instance.IsDaylight())
		{
			this.m_status = WispSpawner.Status.TooBright;
		}
		else if (LuredWisp.GetWispsInArea(this.m_spawnPoint.position, this.m_maxSpawnedArea) >= this.m_maxSpawned)
		{
			this.m_status = WispSpawner.Status.Full;
		}
		return this.m_status;
	}

	// Token: 0x06001ADE RID: 6878 RVA: 0x000B2D08 File Offset: 0x000B0F08
	private void TrySpawn()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_lastSpawn, 0L));
		if ((time - d).TotalSeconds < (double)this.m_spawnInterval)
		{
			return;
		}
		if (UnityEngine.Random.value > this.m_spawnChance)
		{
			return;
		}
		if (this.GetStatus() != WispSpawner.Status.Ok)
		{
			return;
		}
		Vector3 position = this.m_spawnPoint.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * this.m_spawnDistance;
		UnityEngine.Object.Instantiate<GameObject>(this.m_wispPrefab, position, Quaternion.identity);
		this.m_nview.GetZDO().Set(ZDOVars.s_lastSpawn, ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x06001ADF RID: 6879 RVA: 0x000B2E04 File Offset: 0x000B1004
	private bool HaveFreeSpace()
	{
		if (this.m_maxCover <= 0f)
		{
			return true;
		}
		float num;
		bool flag;
		Cover.GetCoverForPoint(this.m_coverPoint.position, out num, out flag, 0.5f);
		return num < this.m_maxCover;
	}

	// Token: 0x06001AE0 RID: 6880 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x06001AE1 RID: 6881 RVA: 0x000B2E44 File Offset: 0x000B1044
	public static WispSpawner GetBestSpawner(Vector3 p, float maxRange)
	{
		WispSpawner wispSpawner = null;
		float num = 0f;
		foreach (WispSpawner wispSpawner2 in WispSpawner.s_spawners)
		{
			float num2 = Vector3.Distance(wispSpawner2.m_spawnPoint.position, p);
			if (num2 <= maxRange)
			{
				WispSpawner.Status status = wispSpawner2.GetStatus();
				if (status != WispSpawner.Status.NoSpace && status != WispSpawner.Status.TooBright && (status != WispSpawner.Status.Full || num2 <= wispSpawner2.m_maxSpawnedArea) && (num2 < num || wispSpawner == null))
				{
					num = num2;
					wispSpawner = wispSpawner2;
				}
			}
		}
		return wispSpawner;
	}

	// Token: 0x04001D0F RID: 7439
	public string m_name = "$pieces_wisplure";

	// Token: 0x04001D10 RID: 7440
	public float m_spawnInterval = 5f;

	// Token: 0x04001D11 RID: 7441
	[Range(0f, 1f)]
	public float m_spawnChance = 0.5f;

	// Token: 0x04001D12 RID: 7442
	public int m_maxSpawned = 3;

	// Token: 0x04001D13 RID: 7443
	public bool m_onlySpawnAtNight = true;

	// Token: 0x04001D14 RID: 7444
	public bool m_dontSpawnInCover = true;

	// Token: 0x04001D15 RID: 7445
	[Range(0f, 1f)]
	public float m_maxCover = 0.6f;

	// Token: 0x04001D16 RID: 7446
	public GameObject m_wispPrefab;

	// Token: 0x04001D17 RID: 7447
	public GameObject m_wispsNearbyObject;

	// Token: 0x04001D18 RID: 7448
	public float m_nearbyTreshold = 5f;

	// Token: 0x04001D19 RID: 7449
	public Transform m_spawnPoint;

	// Token: 0x04001D1A RID: 7450
	public Transform m_coverPoint;

	// Token: 0x04001D1B RID: 7451
	public float m_spawnDistance = 20f;

	// Token: 0x04001D1C RID: 7452
	public float m_maxSpawnedArea = 10f;

	// Token: 0x04001D1D RID: 7453
	private ZNetView m_nview;

	// Token: 0x04001D1E RID: 7454
	private WispSpawner.Status m_status = WispSpawner.Status.Ok;

	// Token: 0x04001D1F RID: 7455
	private float m_lastStatusUpdate = -1000f;

	// Token: 0x04001D20 RID: 7456
	private static readonly List<WispSpawner> s_spawners = new List<WispSpawner>();

	// Token: 0x020002C7 RID: 711
	public enum Status
	{
		// Token: 0x04001D22 RID: 7458
		NoSpace,
		// Token: 0x04001D23 RID: 7459
		TooBright,
		// Token: 0x04001D24 RID: 7460
		Full,
		// Token: 0x04001D25 RID: 7461
		Ok
	}
}
