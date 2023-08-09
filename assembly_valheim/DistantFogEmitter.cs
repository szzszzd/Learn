using System;
using UnityEngine;

// Token: 0x0200006D RID: 109
public class DistantFogEmitter : MonoBehaviour
{
	// Token: 0x06000571 RID: 1393 RVA: 0x0002A844 File Offset: 0x00028A44
	public void SetEmit(bool emit)
	{
		this.m_emit = emit;
	}

	// Token: 0x06000572 RID: 1394 RVA: 0x0002A850 File Offset: 0x00028A50
	private void Update()
	{
		if (!this.m_emit)
		{
			return;
		}
		if (WorldGenerator.instance == null)
		{
			return;
		}
		this.m_placeTimer += Time.deltaTime;
		if (this.m_placeTimer > this.m_interval)
		{
			this.m_placeTimer = 0f;
			int num = Mathf.Max(0, this.m_particles - this.TotalNrOfParticles());
			num /= 4;
			for (int i = 0; i < num; i++)
			{
				this.PlaceOne();
			}
		}
	}

	// Token: 0x06000573 RID: 1395 RVA: 0x0002A8C4 File Offset: 0x00028AC4
	private int TotalNrOfParticles()
	{
		int num = 0;
		foreach (ParticleSystem particleSystem in this.m_psystems)
		{
			num += particleSystem.particleCount;
		}
		return num;
	}

	// Token: 0x06000574 RID: 1396 RVA: 0x0002A8F8 File Offset: 0x00028AF8
	private void PlaceOne()
	{
		Vector3 a;
		if (this.GetRandomPoint(base.transform.position, out a))
		{
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = a + Vector3.up * this.m_placeOffset;
			this.m_psystems[UnityEngine.Random.Range(0, this.m_psystems.Length)].Emit(emitParams, 1);
		}
	}

	// Token: 0x06000575 RID: 1397 RVA: 0x0002A95C File Offset: 0x00028B5C
	private bool GetRandomPoint(Vector3 center, out Vector3 p)
	{
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = Mathf.Sqrt(UnityEngine.Random.value) * (this.m_maxRadius - this.m_minRadius) + this.m_minRadius;
		p = center + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
		p.y = WorldGenerator.instance.GetHeight(p.x, p.z);
		if (p.y < ZoneSystem.instance.m_waterLevel)
		{
			if (this.m_skipWater)
			{
				return false;
			}
			if (UnityEngine.Random.value > this.m_waterSpawnChance)
			{
				return false;
			}
			p.y = ZoneSystem.instance.m_waterLevel;
		}
		else if (p.y > this.m_mountainLimit)
		{
			if (UnityEngine.Random.value > this.m_mountainSpawnChance)
			{
				return false;
			}
		}
		else if (UnityEngine.Random.value > this.m_landSpawnChance)
		{
			return false;
		}
		return true;
	}

	// Token: 0x04000654 RID: 1620
	public float m_interval = 1f;

	// Token: 0x04000655 RID: 1621
	public float m_minRadius = 100f;

	// Token: 0x04000656 RID: 1622
	public float m_maxRadius = 500f;

	// Token: 0x04000657 RID: 1623
	public float m_mountainSpawnChance = 1f;

	// Token: 0x04000658 RID: 1624
	public float m_landSpawnChance = 0.5f;

	// Token: 0x04000659 RID: 1625
	public float m_waterSpawnChance = 0.25f;

	// Token: 0x0400065A RID: 1626
	public float m_mountainLimit = 120f;

	// Token: 0x0400065B RID: 1627
	public float m_emitStep = 10f;

	// Token: 0x0400065C RID: 1628
	public int m_emitPerStep = 10;

	// Token: 0x0400065D RID: 1629
	public int m_particles = 100;

	// Token: 0x0400065E RID: 1630
	public float m_placeOffset = 1f;

	// Token: 0x0400065F RID: 1631
	public ParticleSystem[] m_psystems;

	// Token: 0x04000660 RID: 1632
	public bool m_skipWater;

	// Token: 0x04000661 RID: 1633
	private float m_placeTimer;

	// Token: 0x04000662 RID: 1634
	private bool m_emit = true;

	// Token: 0x04000663 RID: 1635
	private Vector3 m_lastPosition = Vector3.zero;
}
