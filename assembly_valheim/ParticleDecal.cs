using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000088 RID: 136
[ExecuteAlways]
public class ParticleDecal : MonoBehaviour
{
	// Token: 0x0600060C RID: 1548 RVA: 0x0002E36E File Offset: 0x0002C56E
	private void Awake()
	{
		this.part = base.GetComponent<ParticleSystem>();
		this.collisionEvents = new List<ParticleCollisionEvent>();
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x0002E388 File Offset: 0x0002C588
	private void OnParticleCollision(GameObject other)
	{
		if (this.m_chance < 100f && UnityEngine.Random.Range(0f, 100f) > this.m_chance)
		{
			return;
		}
		int num = this.part.GetCollisionEvents(other, this.collisionEvents);
		for (int i = 0; i < num; i++)
		{
			ParticleCollisionEvent particleCollisionEvent = this.collisionEvents[i];
			Vector3 eulerAngles = Quaternion.LookRotation(particleCollisionEvent.normal).eulerAngles;
			eulerAngles.x = -eulerAngles.x + 180f;
			eulerAngles.y = -eulerAngles.y;
			eulerAngles.z = (float)UnityEngine.Random.Range(0, 360);
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = particleCollisionEvent.intersection;
			emitParams.rotation3D = eulerAngles;
			emitParams.velocity = -particleCollisionEvent.normal * 0.001f;
			this.m_decalSystem.Emit(emitParams, 1);
		}
	}

	// Token: 0x04000743 RID: 1859
	public ParticleSystem m_decalSystem;

	// Token: 0x04000744 RID: 1860
	[Range(0f, 100f)]
	public float m_chance = 100f;

	// Token: 0x04000745 RID: 1861
	private ParticleSystem part;

	// Token: 0x04000746 RID: 1862
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
}
