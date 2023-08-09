using System;
using UnityEngine;

// Token: 0x02000280 RID: 640
public class RandomMovement : MonoBehaviour
{
	// Token: 0x0600187E RID: 6270 RVA: 0x000A371E File Offset: 0x000A191E
	private void Start()
	{
		this.m_basePosition = base.transform.localPosition;
	}

	// Token: 0x0600187F RID: 6271 RVA: 0x000A3734 File Offset: 0x000A1934
	private void Update()
	{
		float num = Time.time * this.m_frequency;
		Vector3 b = new Vector3(Mathf.Sin(num) * Mathf.Sin(num * 0.56436f), Mathf.Sin(num * 0.56436f) * Mathf.Sin(num * 0.688742f), Mathf.Cos(num * 0.758348f) * Mathf.Cos(num * 0.4563696f)) * this.m_movement;
		base.transform.localPosition = this.m_basePosition + b;
	}

	// Token: 0x04001A54 RID: 6740
	public float m_frequency = 10f;

	// Token: 0x04001A55 RID: 6741
	public float m_movement = 0.1f;

	// Token: 0x04001A56 RID: 6742
	private Vector3 m_basePosition = Vector3.zero;
}
