using System;
using UnityEngine;

// Token: 0x02000087 RID: 135
public class MistEmitter : MonoBehaviour
{
	// Token: 0x06000606 RID: 1542 RVA: 0x0002E108 File Offset: 0x0002C308
	public void SetEmit(bool emit)
	{
		this.m_emit = emit;
	}

	// Token: 0x06000607 RID: 1543 RVA: 0x0002E111 File Offset: 0x0002C311
	private void Update()
	{
		if (!this.m_emit)
		{
			return;
		}
		this.m_placeTimer += Time.deltaTime;
		if (this.m_placeTimer > this.m_interval)
		{
			this.m_placeTimer = 0f;
			this.PlaceOne();
		}
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x0002E150 File Offset: 0x0002C350
	private void PlaceOne()
	{
		Vector3 vector;
		if (MistEmitter.GetRandomPoint(base.transform.position, this.m_totalRadius, out vector))
		{
			int num = 0;
			float num2 = 6.2831855f / (float)this.m_rays;
			for (int i = 0; i < this.m_rays; i++)
			{
				float angle = (float)i * num2;
				if ((double)MistEmitter.GetPointOnEdge(vector, angle, this.m_testRadius).y < (double)vector.y - 0.1)
				{
					num++;
				}
			}
			if (num > this.m_rays / 4)
			{
				return;
			}
			if (EffectArea.IsPointInsideArea(vector, EffectArea.Type.Fire, this.m_testRadius))
			{
				return;
			}
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = vector + Vector3.up * this.m_placeOffset;
			this.m_psystem.Emit(emitParams, 1);
		}
	}

	// Token: 0x06000609 RID: 1545 RVA: 0x0002E224 File Offset: 0x0002C424
	private static bool GetRandomPoint(Vector3 center, float radius, out Vector3 p)
	{
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = UnityEngine.Random.Range(0f, radius);
		p = center + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
		float num2;
		if (!ZoneSystem.instance.GetGroundHeight(p, out num2))
		{
			return false;
		}
		if (num2 < ZoneSystem.instance.m_waterLevel)
		{
			return false;
		}
		float liquidLevel = Floating.GetLiquidLevel(p, 1f, LiquidType.All);
		if (num2 < liquidLevel)
		{
			return false;
		}
		p.y = num2;
		return true;
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x0002E2BC File Offset: 0x0002C4BC
	private static Vector3 GetPointOnEdge(Vector3 center, float angle, float radius)
	{
		Vector3 vector = center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
		vector.y = ZoneSystem.instance.GetGroundHeight(vector);
		if (vector.y < ZoneSystem.instance.m_waterLevel)
		{
			vector.y = ZoneSystem.instance.m_waterLevel;
		}
		return vector;
	}

	// Token: 0x0400073B RID: 1851
	public float m_interval = 1f;

	// Token: 0x0400073C RID: 1852
	public float m_totalRadius = 30f;

	// Token: 0x0400073D RID: 1853
	public float m_testRadius = 5f;

	// Token: 0x0400073E RID: 1854
	public int m_rays = 10;

	// Token: 0x0400073F RID: 1855
	public float m_placeOffset = 1f;

	// Token: 0x04000740 RID: 1856
	public ParticleSystem m_psystem;

	// Token: 0x04000741 RID: 1857
	private float m_placeTimer;

	// Token: 0x04000742 RID: 1858
	private bool m_emit = true;
}
