using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000089 RID: 137
public class ParticleMist : MonoBehaviour
{
	// Token: 0x1700001D RID: 29
	// (get) Token: 0x0600060F RID: 1551 RVA: 0x0002E4A1 File Offset: 0x0002C6A1
	public static ParticleMist instance
	{
		get
		{
			return ParticleMist.m_instance;
		}
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x0002E4A8 File Offset: 0x0002C6A8
	private void Awake()
	{
		ParticleMist.m_instance = this;
		this.m_ps = base.GetComponent<ParticleSystem>();
		this.m_lastUpdatePos = base.transform.position;
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x0002E4CD File Offset: 0x0002C6CD
	private void OnDestroy()
	{
		if (ParticleMist.m_instance == this)
		{
			ParticleMist.m_instance = null;
		}
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x0002E4E4 File Offset: 0x0002C6E4
	private void Update()
	{
		if (!this.m_ps.emission.enabled)
		{
			return;
		}
		this.m_accumulator += Time.fixedDeltaTime;
		if (this.m_accumulator < 0.1f)
		{
			return;
		}
		this.m_accumulator -= 0.1f;
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		List<Mister> demistersSorted = Mister.GetDemistersSorted(localPlayer.transform.position);
		if (demistersSorted.Count == 0)
		{
			return;
		}
		this.m_haveActiveMist = (demistersSorted.Count > 0);
		this.GetAllForcefields(this.fields);
		this.m_inMistAreaTimer += 0.1f;
		float value = Vector3.Distance(base.transform.position, this.m_lastUpdatePos);
		this.m_combinedMovement += Mathf.Clamp(value, 0f, 10f);
		this.m_lastUpdatePos = base.transform.position;
		float minAlt;
		float num;
		this.FindMaxMistAlltitude(50f, out minAlt, out num);
		int num2 = (int)(this.m_combinedMovement * (float)this.m_localEmissionPerUnit);
		if (num2 > 0)
		{
			this.m_combinedMovement = Mathf.Max(0f, this.m_combinedMovement - (float)num2 / (float)this.m_localEmissionPerUnit);
		}
		int toEmit = (int)((float)this.m_localEmission * 0.1f) + num2;
		this.Emit(base.transform.position, 0f, this.m_localRange, toEmit, this.fields, null, minAlt);
		foreach (Demister demister in this.fields)
		{
			float endRange = demister.m_forceField.endRange;
			float num3 = Mathf.Max(0f, Vector3.Distance(demister.transform.position, base.transform.position) - endRange);
			if (num3 <= this.m_maxDistance)
			{
				float num4 = 12.566371f * (endRange * endRange);
				float num5 = Mathf.Lerp(this.m_emissionMax, 0f, Utils.LerpStep(this.m_minDistance, this.m_maxDistance, num3));
				int num6 = (int)(num4 * num5 * 0.1f);
				float movedDistance = demister.GetMovedDistance();
				num6 += (int)(movedDistance * this.m_emissionPerUnit);
				this.Emit(demister.transform.position, endRange, 0f, num6, this.fields, demister, minAlt);
			}
		}
		foreach (Mister mister in demistersSorted)
		{
			if (!mister.Inside(base.transform.position, 0f))
			{
				this.MisterEmit(mister, demistersSorted, this.fields, minAlt, 0.1f);
			}
		}
	}

	// Token: 0x06000613 RID: 1555 RVA: 0x0002E7C0 File Offset: 0x0002C9C0
	private void Emit(Vector3 center, float radius, float thickness, int toEmit, List<Demister> fields, Demister pf, float minAlt)
	{
		if (!Mister.InsideMister(center, radius + thickness))
		{
			return;
		}
		if (this.IsInsideOtherDemister(fields, center, radius + thickness, pf))
		{
			return;
		}
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		for (int i = 0; i < toEmit; i++)
		{
			Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
			Vector3 vector = center + onUnitSphere * (radius + 0.1f + UnityEngine.Random.Range(0f, thickness));
			if (vector.y >= minAlt && !this.IsInsideOtherDemister(fields, vector, 0f, pf) && Mister.InsideMister(vector, 0f))
			{
				float num = Vector3.Distance(base.transform.position, vector);
				if (num <= this.m_maxDistance)
				{
					emitParams.startSize = Mathf.Lerp(this.m_minSize, this.m_maxSize, Utils.LerpStep(this.m_minDistance, this.m_maxDistance, num));
					emitParams.position = vector;
					this.m_ps.Emit(emitParams, 1);
				}
			}
		}
	}

	// Token: 0x06000614 RID: 1556 RVA: 0x0002E8B4 File Offset: 0x0002CAB4
	private void MisterEmit(Mister mister, List<Mister> allMisters, List<Demister> fields, float minAlt, float dt)
	{
		Vector3 position = mister.transform.position;
		float radius = mister.m_radius;
		float num = Mathf.Max(0f, Vector3.Distance(mister.transform.position, base.transform.position) - radius);
		if (num > this.m_distantMaxRange)
		{
			return;
		}
		if (mister.IsCompletelyInsideOtherMister(this.m_distantThickness))
		{
			return;
		}
		float num2 = 12.566371f * (radius * radius);
		float num3 = Mathf.Lerp(this.m_distantEmissionMax, 0f, Utils.LerpStep(0f, this.m_distantMaxRange, num));
		int num4 = (int)(num2 * num3 * dt);
		float num5 = mister.transform.position.y + mister.m_height;
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		for (int i = 0; i < num4; i++)
		{
			Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
			Vector3 vector = position + onUnitSphere * (radius + 0.1f + UnityEngine.Random.Range(0f, this.m_distantThickness));
			if (vector.y >= minAlt)
			{
				if (vector.y > num5)
				{
					vector.y = num5;
				}
				if (!Mister.IsInsideOtherMister(vector, mister) && !this.IsInsideOtherDemister(fields, vector, 0f, null))
				{
					float num6 = Vector3.Distance(base.transform.position, vector);
					if (num6 <= this.m_distantMaxRange)
					{
						emitParams.startSize = Mathf.Lerp(this.m_distantMinSize, this.m_distantMaxSize, Utils.LerpStep(0f, this.m_distantMaxRange, num6));
						emitParams.position = vector;
						Vector3 velocity = onUnitSphere * UnityEngine.Random.Range(0f, this.m_distantEmissionMaxVel);
						velocity.y = 0f;
						emitParams.velocity = velocity;
						this.m_ps.Emit(emitParams, 1);
					}
				}
			}
		}
	}

	// Token: 0x06000615 RID: 1557 RVA: 0x0002EA80 File Offset: 0x0002CC80
	private bool IsInsideOtherDemister(List<Demister> fields, Vector3 p, float radius, Demister ignore)
	{
		foreach (Demister demister in fields)
		{
			if (!(demister == ignore) && Vector3.Distance(demister.transform.position, p) + radius < demister.m_forceField.endRange)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000616 RID: 1558 RVA: 0x0002EAF8 File Offset: 0x0002CCF8
	public static bool IsInMist(Vector3 p0)
	{
		return !(ParticleMist.m_instance == null) && ParticleMist.m_instance.m_haveActiveMist && Mister.InsideMister(p0, 0f) && !ParticleMist.m_instance.InsideDemister(p0);
	}

	// Token: 0x06000617 RID: 1559 RVA: 0x0002EB34 File Offset: 0x0002CD34
	public static bool IsMistBlocked(Vector3 p0, Vector3 p1)
	{
		return !(ParticleMist.m_instance == null) && ParticleMist.m_instance.IsMistBlocked_internal(p0, p1);
	}

	// Token: 0x06000618 RID: 1560 RVA: 0x0002EB54 File Offset: 0x0002CD54
	private bool IsMistBlocked_internal(Vector3 p0, Vector3 p1)
	{
		if (!this.m_haveActiveMist)
		{
			return false;
		}
		if (Vector3.Distance(p0, p1) < 10f)
		{
			return false;
		}
		Vector3 p2 = (p0 + p1) * 0.5f;
		return (Mister.InsideMister(p0, 0f) && !this.InsideDemister(p0)) || (Mister.InsideMister(p1, 0f) && !this.InsideDemister(p1)) || (Mister.InsideMister(p2, 0f) && !this.InsideDemister(p2));
	}

	// Token: 0x06000619 RID: 1561 RVA: 0x0002EBD8 File Offset: 0x0002CDD8
	private bool InsideDemister(Vector3 p)
	{
		foreach (Demister demister in Demister.GetDemisters())
		{
			if (Vector3.Distance(demister.transform.position, p) < demister.m_forceField.endRange)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600061A RID: 1562 RVA: 0x0002EC48 File Offset: 0x0002CE48
	private void GetAllForcefields(List<Demister> fields)
	{
		List<Demister> demisters = Demister.GetDemisters();
		this.sortList.Clear();
		foreach (Demister demister in demisters)
		{
			this.sortList.Add(new KeyValuePair<Demister, float>(demister, Vector3.Distance(base.transform.position, demister.transform.position)));
		}
		this.sortList.Sort((KeyValuePair<Demister, float> a, KeyValuePair<Demister, float> b) => a.Value.CompareTo(b.Value));
		fields.Clear();
		foreach (KeyValuePair<Demister, float> keyValuePair in this.sortList)
		{
			fields.Add(keyValuePair.Key);
		}
	}

	// Token: 0x0600061B RID: 1563 RVA: 0x0002ED44 File Offset: 0x0002CF44
	private void FindMaxMistAlltitude(float testRange, out float minMistHeight, out float maxMistHeight)
	{
		Vector3 position = base.transform.position;
		float num = 0f;
		int num2 = 20;
		minMistHeight = 99999f;
		for (int i = 0; i < num2; i++)
		{
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			Vector3 p = position + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * testRange;
			float groundHeight = ZoneSystem.instance.GetGroundHeight(p);
			num += groundHeight;
			if (groundHeight < minMistHeight)
			{
				minMistHeight = groundHeight;
			}
		}
		float num3 = num / (float)num2;
		maxMistHeight = num3 + this.m_maxMistAltitude;
	}

	// Token: 0x04000747 RID: 1863
	private List<Heightmap> tempHeightmaps = new List<Heightmap>();

	// Token: 0x04000748 RID: 1864
	private List<Demister> fields = new List<Demister>();

	// Token: 0x04000749 RID: 1865
	private List<KeyValuePair<Demister, float>> sortList = new List<KeyValuePair<Demister, float>>();

	// Token: 0x0400074A RID: 1866
	private static ParticleMist m_instance;

	// Token: 0x0400074B RID: 1867
	private ParticleSystem m_ps;

	// Token: 0x0400074C RID: 1868
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome = Heightmap.Biome.Mistlands;

	// Token: 0x0400074D RID: 1869
	public float m_localRange = 10f;

	// Token: 0x0400074E RID: 1870
	public int m_localEmission = 50;

	// Token: 0x0400074F RID: 1871
	public int m_localEmissionPerUnit = 50;

	// Token: 0x04000750 RID: 1872
	public float m_maxMistAltitude = 50f;

	// Token: 0x04000751 RID: 1873
	[Header("Misters")]
	public float m_distantMaxRange = 100f;

	// Token: 0x04000752 RID: 1874
	public float m_distantMinSize = 5f;

	// Token: 0x04000753 RID: 1875
	public float m_distantMaxSize = 20f;

	// Token: 0x04000754 RID: 1876
	public float m_distantEmissionMax = 0.1f;

	// Token: 0x04000755 RID: 1877
	public float m_distantEmissionMaxVel = 1f;

	// Token: 0x04000756 RID: 1878
	public float m_distantThickness = 4f;

	// Token: 0x04000757 RID: 1879
	[Header("Demisters")]
	public float m_minDistance = 10f;

	// Token: 0x04000758 RID: 1880
	public float m_maxDistance = 50f;

	// Token: 0x04000759 RID: 1881
	public float m_emissionMax = 0.2f;

	// Token: 0x0400075A RID: 1882
	public float m_emissionPerUnit = 20f;

	// Token: 0x0400075B RID: 1883
	public float m_minSize = 2f;

	// Token: 0x0400075C RID: 1884
	public float m_maxSize = 10f;

	// Token: 0x0400075D RID: 1885
	private float m_inMistAreaTimer;

	// Token: 0x0400075E RID: 1886
	private float m_accumulator;

	// Token: 0x0400075F RID: 1887
	private float m_combinedMovement;

	// Token: 0x04000760 RID: 1888
	private Vector3 m_lastUpdatePos;

	// Token: 0x04000761 RID: 1889
	private bool m_haveActiveMist;
}
