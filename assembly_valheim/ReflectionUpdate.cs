using System;
using UnityEngine;

// Token: 0x020001E0 RID: 480
public class ReflectionUpdate : MonoBehaviour
{
	// Token: 0x170000CA RID: 202
	// (get) Token: 0x060013B9 RID: 5049 RVA: 0x00081D34 File Offset: 0x0007FF34
	public static ReflectionUpdate instance
	{
		get
		{
			return ReflectionUpdate.m_instance;
		}
	}

	// Token: 0x060013BA RID: 5050 RVA: 0x00081D3B File Offset: 0x0007FF3B
	private void Start()
	{
		ReflectionUpdate.m_instance = this;
		this.m_current = this.m_probe1;
	}

	// Token: 0x060013BB RID: 5051 RVA: 0x00081D4F File Offset: 0x0007FF4F
	private void OnDestroy()
	{
		ReflectionUpdate.m_instance = null;
	}

	// Token: 0x060013BC RID: 5052 RVA: 0x00081D58 File Offset: 0x0007FF58
	public void UpdateReflection()
	{
		Vector3 vector = ZNet.instance.GetReferencePosition();
		vector += Vector3.up * this.m_reflectionHeight;
		this.m_current = ((this.m_current == this.m_probe1) ? this.m_probe2 : this.m_probe1);
		this.m_current.transform.position = vector;
		this.m_renderID = this.m_current.RenderProbe();
	}

	// Token: 0x060013BD RID: 5053 RVA: 0x00081DD0 File Offset: 0x0007FFD0
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		this.m_updateTimer += deltaTime;
		if (this.m_updateTimer > this.m_interval)
		{
			this.m_updateTimer = 0f;
			this.UpdateReflection();
		}
		if (this.m_current.IsFinishedRendering(this.m_renderID))
		{
			float num = Mathf.Clamp01(this.m_updateTimer / this.m_transitionDuration);
			num = Mathf.Pow(num, this.m_power);
			if (this.m_probe1 == this.m_current)
			{
				this.m_probe1.importance = 1;
				this.m_probe2.importance = 0;
				Vector3 size = this.m_probe1.size;
				size.x = 2000f * num;
				size.y = 1000f * num;
				size.z = 2000f * num;
				this.m_probe1.size = size;
				this.m_probe2.size = new Vector3(2001f, 1001f, 2001f);
				return;
			}
			this.m_probe1.importance = 0;
			this.m_probe2.importance = 1;
			Vector3 size2 = this.m_probe2.size;
			size2.x = 2000f * num;
			size2.y = 1000f * num;
			size2.z = 2000f * num;
			this.m_probe2.size = size2;
			this.m_probe1.size = new Vector3(2001f, 1001f, 2001f);
		}
	}

	// Token: 0x04001498 RID: 5272
	private static ReflectionUpdate m_instance;

	// Token: 0x04001499 RID: 5273
	public ReflectionProbe m_probe1;

	// Token: 0x0400149A RID: 5274
	public ReflectionProbe m_probe2;

	// Token: 0x0400149B RID: 5275
	public float m_interval = 3f;

	// Token: 0x0400149C RID: 5276
	public float m_reflectionHeight = 5f;

	// Token: 0x0400149D RID: 5277
	public float m_transitionDuration = 3f;

	// Token: 0x0400149E RID: 5278
	public float m_power = 1f;

	// Token: 0x0400149F RID: 5279
	private ReflectionProbe m_current;

	// Token: 0x040014A0 RID: 5280
	private int m_renderID;

	// Token: 0x040014A1 RID: 5281
	private float m_updateTimer;
}
