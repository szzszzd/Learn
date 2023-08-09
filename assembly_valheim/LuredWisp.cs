using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000264 RID: 612
public class LuredWisp : MonoBehaviour
{
	// Token: 0x060017A2 RID: 6050 RVA: 0x0009D264 File Offset: 0x0009B464
	private void Awake()
	{
		LuredWisp.m_wisps.Add(this);
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_targetPoint = base.transform.position;
		this.m_time = (float)UnityEngine.Random.Range(0, 1000);
		base.InvokeRepeating("UpdateTarget", UnityEngine.Random.Range(0f, 2f), 2f);
	}

	// Token: 0x060017A3 RID: 6051 RVA: 0x0009D2CA File Offset: 0x0009B4CA
	private void OnDestroy()
	{
		LuredWisp.m_wisps.Remove(this);
	}

	// Token: 0x060017A4 RID: 6052 RVA: 0x0009D2D8 File Offset: 0x0009B4D8
	private void UpdateTarget()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_despawnTimer > 0f)
		{
			return;
		}
		WispSpawner bestSpawner = WispSpawner.GetBestSpawner(base.transform.position, this.m_maxLureDistance);
		if (bestSpawner == null || (this.m_despawnInDaylight && EnvMan.instance.IsDaylight()))
		{
			this.m_despawnTimer = 3f;
			this.m_targetPoint = base.transform.position + Quaternion.Euler(-20f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * 100f;
			return;
		}
		this.m_despawnTimer = 0f;
		this.m_targetPoint = bestSpawner.m_spawnPoint.position;
	}

	// Token: 0x060017A5 RID: 6053 RVA: 0x0009D3AF File Offset: 0x0009B5AF
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateMovement(this.m_targetPoint, Time.fixedDeltaTime);
	}

	// Token: 0x060017A6 RID: 6054 RVA: 0x0009D3E0 File Offset: 0x0009B5E0
	private void UpdateMovement(Vector3 targetPos, float dt)
	{
		if (this.m_despawnTimer > 0f)
		{
			this.m_despawnTimer -= dt;
			if (this.m_despawnTimer <= 0f)
			{
				this.m_despawnEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				this.m_nview.Destroy();
				return;
			}
		}
		this.m_time += dt;
		float num = this.m_time * this.m_noiseSpeed;
		targetPos += new Vector3(Mathf.Sin(num * 4f), Mathf.Sin(num * 2f) * this.m_noiseDistanceYScale, Mathf.Cos(num * 5f)) * this.m_noiseDistance;
		Vector3 normalized = (targetPos - base.transform.position).normalized;
		this.m_ballVel += normalized * this.m_acceleration * dt;
		if (this.m_ballVel.magnitude > this.m_maxSpeed)
		{
			this.m_ballVel = this.m_ballVel.normalized * this.m_maxSpeed;
		}
		this.m_ballVel -= this.m_ballVel * this.m_friction;
		base.transform.position = base.transform.position + this.m_ballVel * dt;
	}

	// Token: 0x060017A7 RID: 6055 RVA: 0x0009D560 File Offset: 0x0009B760
	public static int GetWispsInArea(Vector3 p, float r)
	{
		float num = r * r;
		int num2 = 0;
		foreach (LuredWisp luredWisp in LuredWisp.m_wisps)
		{
			if (Utils.DistanceSqr(p, luredWisp.transform.position) < num)
			{
				num2++;
			}
		}
		return num2;
	}

	// Token: 0x04001910 RID: 6416
	public bool m_despawnInDaylight = true;

	// Token: 0x04001911 RID: 6417
	public float m_maxLureDistance = 20f;

	// Token: 0x04001912 RID: 6418
	public float m_acceleration = 6f;

	// Token: 0x04001913 RID: 6419
	public float m_noiseDistance = 1.5f;

	// Token: 0x04001914 RID: 6420
	public float m_noiseDistanceYScale = 0.2f;

	// Token: 0x04001915 RID: 6421
	public float m_noiseSpeed = 0.5f;

	// Token: 0x04001916 RID: 6422
	public float m_maxSpeed = 40f;

	// Token: 0x04001917 RID: 6423
	public float m_friction = 0.03f;

	// Token: 0x04001918 RID: 6424
	public EffectList m_despawnEffects = new EffectList();

	// Token: 0x04001919 RID: 6425
	private static List<LuredWisp> m_wisps = new List<LuredWisp>();

	// Token: 0x0400191A RID: 6426
	private Vector3 m_ballVel = Vector3.zero;

	// Token: 0x0400191B RID: 6427
	private ZNetView m_nview;

	// Token: 0x0400191C RID: 6428
	private Vector3 m_targetPoint;

	// Token: 0x0400191D RID: 6429
	private float m_despawnTimer;

	// Token: 0x0400191E RID: 6430
	private float m_time;
}
