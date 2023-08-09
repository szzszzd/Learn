using System;
using UnityEngine;

// Token: 0x0200008F RID: 143
public class Thunder : MonoBehaviour
{
	// Token: 0x06000639 RID: 1593 RVA: 0x0002F4F8 File Offset: 0x0002D6F8
	private void Start()
	{
		this.m_strikeTimer = UnityEngine.Random.Range(this.m_strikeIntervalMin, this.m_strikeIntervalMax);
	}

	// Token: 0x0600063A RID: 1594 RVA: 0x0002F514 File Offset: 0x0002D714
	private void Update()
	{
		if (this.m_strikeTimer > 0f)
		{
			this.m_strikeTimer -= Time.deltaTime;
			if (this.m_strikeTimer <= 0f)
			{
				this.DoFlash();
			}
		}
		if (this.m_thunderTimer > 0f)
		{
			this.m_thunderTimer -= Time.deltaTime;
			if (this.m_thunderTimer <= 0f)
			{
				this.DoThunder();
				this.m_strikeTimer = UnityEngine.Random.Range(this.m_strikeIntervalMin, this.m_strikeIntervalMax);
			}
		}
		if (this.m_spawnThor)
		{
			this.m_thorTimer += Time.deltaTime;
			if (this.m_thorTimer > this.m_thorInterval)
			{
				this.m_thorTimer = 0f;
				if (UnityEngine.Random.value <= this.m_thorChance && (this.m_requiredGlobalKey == "" || ZoneSystem.instance.GetGlobalKey(this.m_requiredGlobalKey)))
				{
					this.SpawnThor();
				}
			}
		}
	}

	// Token: 0x0600063B RID: 1595 RVA: 0x0002F608 File Offset: 0x0002D808
	private void SpawnThor()
	{
		float num = UnityEngine.Random.value * 6.2831855f;
		Vector3 vector = base.transform.position + new Vector3(Mathf.Sin(num), 0f, Mathf.Cos(num)) * this.m_thorSpawnDistance;
		vector.y += UnityEngine.Random.Range(this.m_thorSpawnAltitudeMin, this.m_thorSpawnAltitudeMax);
		float groundHeight = ZoneSystem.instance.GetGroundHeight(vector);
		if (vector.y < groundHeight)
		{
			vector.y = groundHeight + 50f;
		}
		float f = num + 180f + (float)UnityEngine.Random.Range(-45, 45);
		Vector3 vector2 = base.transform.position + new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * this.m_thorSpawnDistance;
		vector2.y += UnityEngine.Random.Range(this.m_thorSpawnAltitudeMin, this.m_thorSpawnAltitudeMax);
		float groundHeight2 = ZoneSystem.instance.GetGroundHeight(vector2);
		if (vector.y < groundHeight2)
		{
			vector.y = groundHeight2 + 50f;
		}
		Vector3 normalized = (vector2 - vector).normalized;
		UnityEngine.Object.Instantiate<GameObject>(this.m_thorPrefab, vector, Quaternion.LookRotation(normalized));
	}

	// Token: 0x0600063C RID: 1596 RVA: 0x0002F744 File Offset: 0x0002D944
	private void DoFlash()
	{
		float f = UnityEngine.Random.value * 6.2831855f;
		float d = UnityEngine.Random.Range(this.m_flashDistanceMin, this.m_flashDistanceMax);
		this.m_flashPos = base.transform.position + new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * d;
		this.m_flashPos.y = this.m_flashPos.y + this.m_flashAltitude;
		Quaternion rotation = Quaternion.LookRotation((base.transform.position - this.m_flashPos).normalized);
		GameObject[] array = this.m_flashEffect.Create(this.m_flashPos, Quaternion.identity, null, 1f, -1);
		for (int i = 0; i < array.Length; i++)
		{
			Light[] componentsInChildren = array[i].GetComponentsInChildren<Light>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].transform.rotation = rotation;
			}
		}
		this.m_thunderTimer = UnityEngine.Random.Range(this.m_thunderDelayMin, this.m_thunderDelayMax);
	}

	// Token: 0x0600063D RID: 1597 RVA: 0x0002F852 File Offset: 0x0002DA52
	private void DoThunder()
	{
		this.m_thunderEffect.Create(this.m_flashPos, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x04000782 RID: 1922
	public float m_strikeIntervalMin = 3f;

	// Token: 0x04000783 RID: 1923
	public float m_strikeIntervalMax = 10f;

	// Token: 0x04000784 RID: 1924
	public float m_thunderDelayMin = 3f;

	// Token: 0x04000785 RID: 1925
	public float m_thunderDelayMax = 5f;

	// Token: 0x04000786 RID: 1926
	public float m_flashDistanceMin = 50f;

	// Token: 0x04000787 RID: 1927
	public float m_flashDistanceMax = 200f;

	// Token: 0x04000788 RID: 1928
	public float m_flashAltitude = 100f;

	// Token: 0x04000789 RID: 1929
	public EffectList m_flashEffect = new EffectList();

	// Token: 0x0400078A RID: 1930
	public EffectList m_thunderEffect = new EffectList();

	// Token: 0x0400078B RID: 1931
	[Header("Thor")]
	public bool m_spawnThor;

	// Token: 0x0400078C RID: 1932
	public string m_requiredGlobalKey = "";

	// Token: 0x0400078D RID: 1933
	public GameObject m_thorPrefab;

	// Token: 0x0400078E RID: 1934
	public float m_thorSpawnDistance = 300f;

	// Token: 0x0400078F RID: 1935
	public float m_thorSpawnAltitudeMax = 100f;

	// Token: 0x04000790 RID: 1936
	public float m_thorSpawnAltitudeMin = 100f;

	// Token: 0x04000791 RID: 1937
	public float m_thorInterval = 10f;

	// Token: 0x04000792 RID: 1938
	public float m_thorChance = 1f;

	// Token: 0x04000793 RID: 1939
	private Vector3 m_flashPos = Vector3.zero;

	// Token: 0x04000794 RID: 1940
	private float m_strikeTimer = -1f;

	// Token: 0x04000795 RID: 1941
	private float m_thunderTimer = -1f;

	// Token: 0x04000796 RID: 1942
	private float m_thorTimer;
}
