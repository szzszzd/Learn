using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002BD RID: 701
public class WaterTrigger : MonoBehaviour
{
	// Token: 0x06001A81 RID: 6785 RVA: 0x000B00A7 File Offset: 0x000AE2A7
	private void Start()
	{
		this.m_cooldownTimer = UnityEngine.Random.Range(0f, 2f);
	}

	// Token: 0x06001A82 RID: 6786 RVA: 0x000B00BE File Offset: 0x000AE2BE
	private void OnEnable()
	{
		WaterTrigger.Instances.Add(this);
	}

	// Token: 0x06001A83 RID: 6787 RVA: 0x000B00CB File Offset: 0x000AE2CB
	private void OnDisable()
	{
		WaterTrigger.Instances.Remove(this);
	}

	// Token: 0x06001A84 RID: 6788 RVA: 0x000B00DC File Offset: 0x000AE2DC
	public void CustomUpdate(float deltaTime)
	{
		this.m_cooldownTimer += deltaTime;
		if (this.m_cooldownTimer <= this.m_cooldownDelay)
		{
			return;
		}
		Transform transform = base.transform;
		Vector3 position = transform.position;
		float waterLevel = Floating.GetWaterLevel(position, ref this.m_previousAndOut);
		if (position.y < waterLevel)
		{
			this.m_effects.Create(position, transform.rotation, transform, 1f, -1);
			this.m_cooldownTimer = 0f;
		}
	}

	// Token: 0x170000F8 RID: 248
	// (get) Token: 0x06001A85 RID: 6789 RVA: 0x000B014F File Offset: 0x000AE34F
	public static List<WaterTrigger> Instances { get; } = new List<WaterTrigger>();

	// Token: 0x04001C9F RID: 7327
	public EffectList m_effects = new EffectList();

	// Token: 0x04001CA0 RID: 7328
	public float m_cooldownDelay = 2f;

	// Token: 0x04001CA1 RID: 7329
	private float m_cooldownTimer;

	// Token: 0x04001CA2 RID: 7330
	private WaterVolume m_previousAndOut;
}
