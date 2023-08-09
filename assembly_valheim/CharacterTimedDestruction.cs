using System;
using UnityEngine;

// Token: 0x0200000A RID: 10
public class CharacterTimedDestruction : MonoBehaviour
{
	// Token: 0x06000122 RID: 290 RVA: 0x00007FF5 File Offset: 0x000061F5
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_triggerOnAwake)
		{
			this.Trigger();
		}
	}

	// Token: 0x06000123 RID: 291 RVA: 0x00008011 File Offset: 0x00006211
	public void Trigger()
	{
		base.InvokeRepeating("DestroyNow", UnityEngine.Random.Range(this.m_timeoutMin, this.m_timeoutMax), 1f);
	}

	// Token: 0x06000124 RID: 292 RVA: 0x00008034 File Offset: 0x00006234
	public void Trigger(float timeout)
	{
		base.InvokeRepeating("DestroyNow", timeout, 1f);
	}

	// Token: 0x06000125 RID: 293 RVA: 0x00008048 File Offset: 0x00006248
	private void DestroyNow()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		Character component = base.GetComponent<Character>();
		HitData hitData = new HitData();
		hitData.m_damage.m_damage = 99999f;
		hitData.m_point = base.transform.position;
		component.ApplyDamage(hitData, false, true, HitData.DamageModifier.Normal);
	}

	// Token: 0x04000104 RID: 260
	public float m_timeoutMin = 1f;

	// Token: 0x04000105 RID: 261
	public float m_timeoutMax = 1f;

	// Token: 0x04000106 RID: 262
	public bool m_triggerOnAwake;

	// Token: 0x04000107 RID: 263
	private ZNetView m_nview;

	// Token: 0x04000108 RID: 264
	private Character m_character;
}
