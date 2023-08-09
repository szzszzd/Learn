using System;
using UnityEngine;

// Token: 0x0200026E RID: 622
public class MovementTest : MonoBehaviour
{
	// Token: 0x060017EC RID: 6124 RVA: 0x0009F3F1 File Offset: 0x0009D5F1
	private void Start()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_center = base.transform.position;
	}

	// Token: 0x060017ED RID: 6125 RVA: 0x0009F410 File Offset: 0x0009D610
	private void FixedUpdate()
	{
		this.m_timer += Time.fixedDeltaTime;
		float num = 5f;
		Vector3 vector = this.m_center + new Vector3(Mathf.Sin(this.m_timer * this.m_speed) * num, 0f, Mathf.Cos(this.m_timer * this.m_speed) * num);
		this.m_vel = (vector - this.m_body.position) / Time.fixedDeltaTime;
		this.m_body.position = vector;
		this.m_body.velocity = this.m_vel;
	}

	// Token: 0x04001964 RID: 6500
	public float m_speed = 10f;

	// Token: 0x04001965 RID: 6501
	private float m_timer;

	// Token: 0x04001966 RID: 6502
	private Rigidbody m_body;

	// Token: 0x04001967 RID: 6503
	private Vector3 m_center;

	// Token: 0x04001968 RID: 6504
	private Vector3 m_vel;
}
