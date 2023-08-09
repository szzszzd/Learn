using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200008B RID: 139
public class Smoke : MonoBehaviour
{
	// Token: 0x06000620 RID: 1568 RVA: 0x0002EEF4 File Offset: 0x0002D0F4
	private void Awake()
	{
		Smoke.s_smoke.Add(this);
		this.m_added = true;
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_mr = base.GetComponent<MeshRenderer>();
		this.m_body.maxDepenetrationVelocity = 1f;
		this.m_vel += Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * this.m_randomVel;
	}

	// Token: 0x06000621 RID: 1569 RVA: 0x0002EF7C File Offset: 0x0002D17C
	private void OnEnable()
	{
		Smoke.Instances.Add(this);
	}

	// Token: 0x06000622 RID: 1570 RVA: 0x0002EF89 File Offset: 0x0002D189
	private void OnDisable()
	{
		Smoke.Instances.Remove(this);
	}

	// Token: 0x06000623 RID: 1571 RVA: 0x0002EF97 File Offset: 0x0002D197
	private void OnDestroy()
	{
		if (this.m_added)
		{
			Smoke.s_smoke.Remove(this);
			this.m_added = false;
		}
	}

	// Token: 0x06000624 RID: 1572 RVA: 0x0002EFB4 File Offset: 0x0002D1B4
	public void StartFadeOut()
	{
		if (this.m_fadeTimer >= 0f)
		{
			return;
		}
		if (this.m_added)
		{
			Smoke.s_smoke.Remove(this);
			this.m_added = false;
		}
		this.m_fadeTimer = 0f;
	}

	// Token: 0x06000625 RID: 1573 RVA: 0x0002EFEA File Offset: 0x0002D1EA
	public static int GetTotalSmoke()
	{
		return Smoke.s_smoke.Count;
	}

	// Token: 0x06000626 RID: 1574 RVA: 0x0002EFF6 File Offset: 0x0002D1F6
	public static void FadeOldest()
	{
		if (Smoke.s_smoke.Count == 0)
		{
			return;
		}
		Smoke.s_smoke[0].StartFadeOut();
	}

	// Token: 0x06000627 RID: 1575 RVA: 0x0002F018 File Offset: 0x0002D218
	public static void FadeMostDistant()
	{
		if (Smoke.s_smoke.Count == 0)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 position = mainCamera.transform.position;
		int num = -1;
		float num2 = 0f;
		for (int i = 0; i < Smoke.s_smoke.Count; i++)
		{
			float num3 = Vector3.Distance(Smoke.s_smoke[i].transform.position, position);
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		if (num != -1)
		{
			Smoke.s_smoke[num].StartFadeOut();
		}
	}

	// Token: 0x06000628 RID: 1576 RVA: 0x0002F0AC File Offset: 0x0002D2AC
	public void CustomUpdate(float deltaTime)
	{
		this.m_time += deltaTime;
		if (this.m_time > this.m_ttl && this.m_fadeTimer < 0f)
		{
			this.StartFadeOut();
		}
		float num = 1f - Mathf.Clamp01(this.m_time / this.m_ttl);
		this.m_body.mass = num * num;
		Vector3 velocity = this.m_body.velocity;
		Vector3 vel = this.m_vel;
		vel.y *= num;
		Vector3 a = vel - velocity;
		this.m_body.AddForce(a * this.m_force * deltaTime, ForceMode.VelocityChange);
		if (this.m_fadeTimer >= 0f)
		{
			this.m_fadeTimer += deltaTime;
			float a2 = 1f - Mathf.Clamp01(this.m_fadeTimer / this.m_fadetime);
			Color color = this.m_mr.material.color;
			color.a = a2;
			this.m_mr.material.color = color;
			if (this.m_fadeTimer >= this.m_fadetime)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x06000629 RID: 1577 RVA: 0x0002F1CF File Offset: 0x0002D3CF
	public static List<Smoke> Instances { get; } = new List<Smoke>();

	// Token: 0x04000764 RID: 1892
	public Vector3 m_vel = Vector3.up;

	// Token: 0x04000765 RID: 1893
	public float m_randomVel = 0.1f;

	// Token: 0x04000766 RID: 1894
	public float m_force = 0.1f;

	// Token: 0x04000767 RID: 1895
	public float m_ttl = 10f;

	// Token: 0x04000768 RID: 1896
	public float m_fadetime = 3f;

	// Token: 0x04000769 RID: 1897
	private Rigidbody m_body;

	// Token: 0x0400076A RID: 1898
	private float m_time;

	// Token: 0x0400076B RID: 1899
	private float m_fadeTimer = -1f;

	// Token: 0x0400076C RID: 1900
	private bool m_added;

	// Token: 0x0400076D RID: 1901
	private MeshRenderer m_mr;

	// Token: 0x0400076E RID: 1902
	private static readonly List<Smoke> s_smoke = new List<Smoke>();
}
