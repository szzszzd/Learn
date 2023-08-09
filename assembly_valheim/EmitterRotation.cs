using System;
using UnityEngine;

// Token: 0x02000071 RID: 113
public class EmitterRotation : MonoBehaviour
{
	// Token: 0x0600057F RID: 1407 RVA: 0x0002AE6C File Offset: 0x0002906C
	private void Start()
	{
		this.m_lastPos = base.transform.position;
		this.m_ps = base.GetComponentInChildren<ParticleSystem>();
	}

	// Token: 0x06000580 RID: 1408 RVA: 0x0002AE8C File Offset: 0x0002908C
	private void Update()
	{
		if (!this.m_ps.emission.enabled)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Vector3 vector = position - this.m_lastPos;
		this.m_lastPos = position;
		float t = Mathf.Clamp01(vector.magnitude / Time.deltaTime / this.m_maxSpeed);
		if (vector == Vector3.zero)
		{
			vector = Vector3.up;
		}
		Quaternion a = Quaternion.LookRotation(Vector3.up);
		Quaternion b = Quaternion.LookRotation(vector);
		Quaternion to = Quaternion.Lerp(a, b, t);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * this.m_rotSpeed);
	}

	// Token: 0x04000676 RID: 1654
	public float m_maxSpeed = 10f;

	// Token: 0x04000677 RID: 1655
	public float m_rotSpeed = 90f;

	// Token: 0x04000678 RID: 1656
	private Vector3 m_lastPos;

	// Token: 0x04000679 RID: 1657
	private ParticleSystem m_ps;
}
