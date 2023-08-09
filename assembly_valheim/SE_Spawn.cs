using System;
using UnityEngine;

// Token: 0x02000063 RID: 99
public class SE_Spawn : StatusEffect
{
	// Token: 0x0600050B RID: 1291 RVA: 0x00028B84 File Offset: 0x00026D84
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_spawned)
		{
			return;
		}
		if (this.m_time > this.m_delay)
		{
			this.m_spawned = true;
			Vector3 position = this.m_character.transform.TransformVector(this.m_spawnOffset);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, position, Quaternion.identity);
			Projectile component = gameObject.GetComponent<Projectile>();
			if (component)
			{
				component.Setup(this.m_character, Vector3.zero, -1f, null, null, null);
			}
			this.m_spawnEffect.Create(gameObject.transform.position, gameObject.transform.rotation, null, 1f, -1);
		}
	}

	// Token: 0x040005E8 RID: 1512
	[Header("__SE_Spawn__")]
	public float m_delay = 10f;

	// Token: 0x040005E9 RID: 1513
	public GameObject m_prefab;

	// Token: 0x040005EA RID: 1514
	public Vector3 m_spawnOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040005EB RID: 1515
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x040005EC RID: 1516
	private bool m_spawned;
}
