using System;
using UnityEngine;

// Token: 0x02000267 RID: 615
public class MenuShipMovement : MonoBehaviour
{
	// Token: 0x060017B6 RID: 6070 RVA: 0x0009DB39 File Offset: 0x0009BD39
	private void Start()
	{
		this.m_time = (float)UnityEngine.Random.Range(0, 10);
	}

	// Token: 0x060017B7 RID: 6071 RVA: 0x0009DB4C File Offset: 0x0009BD4C
	private void Update()
	{
		this.m_time += Time.deltaTime;
		base.transform.rotation = Quaternion.Euler(Mathf.Sin(this.m_time * this.m_freq) * this.m_xAngle, 0f, Mathf.Sin(this.m_time * 1.5341234f * this.m_freq) * this.m_zAngle);
	}

	// Token: 0x0400192B RID: 6443
	public float m_freq = 1f;

	// Token: 0x0400192C RID: 6444
	public float m_xAngle = 5f;

	// Token: 0x0400192D RID: 6445
	public float m_zAngle = 5f;

	// Token: 0x0400192E RID: 6446
	private float m_time;
}
