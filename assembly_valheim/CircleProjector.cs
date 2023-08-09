using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x0200006C RID: 108
public class CircleProjector : MonoBehaviour
{
	// Token: 0x0600056C RID: 1388 RVA: 0x0002A358 File Offset: 0x00028558
	private void Start()
	{
		this.CreateSegments();
	}

	// Token: 0x0600056D RID: 1389 RVA: 0x0002A360 File Offset: 0x00028560
	private void Update()
	{
		this.CreateSegments();
		bool flag = this.m_turns == 1f;
		float num = 6.2831855f * this.m_turns / (float)(this.m_nrOfSegments - (flag ? 0 : 1));
		float num2 = (flag && !this.m_sliceLines) ? (Time.time * this.m_speed) : 0f;
		for (int i = 0; i < this.m_nrOfSegments; i++)
		{
			float f = 0.017453292f * this.m_start + (float)i * num + num2;
			Vector3 vector = base.transform.position + new Vector3(Mathf.Sin(f) * this.m_radius, 0f, Mathf.Cos(f) * this.m_radius);
			GameObject gameObject = this.m_segments[i];
			RaycastHit raycastHit;
			if (Physics.Raycast(vector + Vector3.up * 500f, Vector3.down, out raycastHit, 1000f, this.m_mask.value))
			{
				vector.y = raycastHit.point.y;
			}
			gameObject.transform.position = vector;
		}
		for (int j = 0; j < this.m_nrOfSegments; j++)
		{
			GameObject gameObject2 = this.m_segments[j];
			GameObject gameObject3;
			GameObject gameObject4;
			if (flag)
			{
				gameObject3 = ((j == 0) ? this.m_segments[this.m_nrOfSegments - 1] : this.m_segments[j - 1]);
				gameObject4 = ((j == this.m_nrOfSegments - 1) ? this.m_segments[0] : this.m_segments[j + 1]);
			}
			else
			{
				gameObject3 = ((j == 0) ? gameObject2 : this.m_segments[j - 1]);
				gameObject4 = ((j == this.m_nrOfSegments - 1) ? gameObject2 : this.m_segments[j + 1]);
			}
			Vector3 normalized = (gameObject4.transform.position - gameObject3.transform.position).normalized;
			gameObject2.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
		for (int k = this.m_nrOfSegments; k < this.m_segments.Count; k++)
		{
			Vector3 position = this.m_segments[k].transform.position;
			RaycastHit raycastHit2;
			if (Physics.Raycast(position + Vector3.up * 500f, Vector3.down, out raycastHit2, 1000f, this.m_mask.value))
			{
				position.y = raycastHit2.point.y;
			}
			this.m_segments[k].transform.position = position;
		}
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x0002A61C File Offset: 0x0002881C
	private void CreateSegments()
	{
		if ((!this.m_sliceLines && this.m_segments.Count == this.m_nrOfSegments) || (this.m_sliceLines && this.m_calcStart == this.m_start && this.m_calcTurns == this.m_turns))
		{
			return;
		}
		foreach (GameObject obj in this.m_segments)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_segments.Clear();
		for (int i = 0; i < this.m_nrOfSegments; i++)
		{
			GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform.position, Quaternion.identity, base.transform);
			this.m_segments.Add(item);
		}
		this.m_calcStart = this.m_start;
		this.m_calcTurns = this.m_turns;
		if (this.m_sliceLines)
		{
			float start = this.m_start;
			float angle = this.m_start + 6.2831855f * this.m_turns * 57.29578f;
			float num = 2f * this.m_radius * 3.1415927f * this.m_turns / (float)this.m_nrOfSegments;
			int count = (int)(this.m_radius / num) - 2;
			this.<CreateSegments>g__placeSlices|2_0(start, count);
			this.<CreateSegments>g__placeSlices|2_0(angle, count);
		}
	}

	// Token: 0x06000570 RID: 1392 RVA: 0x0002A7B8 File Offset: 0x000289B8
	[CompilerGenerated]
	private void <CreateSegments>g__placeSlices|2_0(float angle, int count)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform.position, Quaternion.Euler(0f, angle, 0f), base.transform);
			gameObject.transform.position += gameObject.transform.forward * this.m_radius * ((float)(i + 1) / (float)(count + 1));
			this.m_segments.Add(gameObject);
		}
	}

	// Token: 0x04000649 RID: 1609
	public float m_radius = 5f;

	// Token: 0x0400064A RID: 1610
	public int m_nrOfSegments = 20;

	// Token: 0x0400064B RID: 1611
	public float m_speed = 0.1f;

	// Token: 0x0400064C RID: 1612
	public float m_turns = 1f;

	// Token: 0x0400064D RID: 1613
	public float m_start;

	// Token: 0x0400064E RID: 1614
	public bool m_sliceLines;

	// Token: 0x0400064F RID: 1615
	private float m_calcStart;

	// Token: 0x04000650 RID: 1616
	private float m_calcTurns;

	// Token: 0x04000651 RID: 1617
	public GameObject m_prefab;

	// Token: 0x04000652 RID: 1618
	public LayerMask m_mask;

	// Token: 0x04000653 RID: 1619
	private List<GameObject> m_segments = new List<GameObject>();
}
