using System;
using UnityEngine;

// Token: 0x0200008D RID: 141
public class SmokeSpawner : MonoBehaviour
{
	// Token: 0x06000630 RID: 1584 RVA: 0x0002F27A File Offset: 0x0002D47A
	private void Start()
	{
		this.m_time = UnityEngine.Random.Range(0f, this.m_interval);
	}

	// Token: 0x06000631 RID: 1585 RVA: 0x0002F292 File Offset: 0x0002D492
	private void Update()
	{
		this.m_time += Time.deltaTime;
		if (this.m_time > this.m_interval)
		{
			this.m_time = 0f;
			this.Spawn();
		}
	}

	// Token: 0x06000632 RID: 1586 RVA: 0x0002F2C8 File Offset: 0x0002D4C8
	private void Spawn()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || Vector3.Distance(localPlayer.transform.position, base.transform.position) > 64f)
		{
			this.m_lastSpawnTime = Time.time;
			return;
		}
		if (this.TestBlocked())
		{
			return;
		}
		if (Smoke.GetTotalSmoke() > 100)
		{
			Smoke.FadeOldest();
		}
		UnityEngine.Object.Instantiate<GameObject>(this.m_smokePrefab, base.transform.position, UnityEngine.Random.rotation);
		this.m_lastSpawnTime = Time.time;
	}

	// Token: 0x06000633 RID: 1587 RVA: 0x0002F350 File Offset: 0x0002D550
	private bool TestBlocked()
	{
		return Physics.CheckSphere(base.transform.position, this.m_testRadius, this.m_testMask.value);
	}

	// Token: 0x06000634 RID: 1588 RVA: 0x0002F378 File Offset: 0x0002D578
	public bool IsBlocked()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return this.TestBlocked();
		}
		return Time.time - this.m_lastSpawnTime > 4f;
	}

	// Token: 0x04000772 RID: 1906
	private const float m_minPlayerDistance = 64f;

	// Token: 0x04000773 RID: 1907
	private const int m_maxGlobalSmoke = 100;

	// Token: 0x04000774 RID: 1908
	private const float m_blockedMinTime = 4f;

	// Token: 0x04000775 RID: 1909
	public GameObject m_smokePrefab;

	// Token: 0x04000776 RID: 1910
	public float m_interval = 0.5f;

	// Token: 0x04000777 RID: 1911
	public LayerMask m_testMask;

	// Token: 0x04000778 RID: 1912
	public float m_testRadius = 0.5f;

	// Token: 0x04000779 RID: 1913
	private float m_lastSpawnTime;

	// Token: 0x0400077A RID: 1914
	private float m_time;
}
