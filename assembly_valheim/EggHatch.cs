using System;
using UnityEngine;

// Token: 0x0200000E RID: 14
public class EggHatch : MonoBehaviour
{
	// Token: 0x0600013A RID: 314 RVA: 0x000089EF File Offset: 0x00006BEF
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (UnityEngine.Random.value <= this.m_chanceToHatch)
		{
			base.InvokeRepeating("CheckSpawn", UnityEngine.Random.Range(1f, 2f), 1f);
		}
	}

	// Token: 0x0600013B RID: 315 RVA: 0x00008A2C File Offset: 0x00006C2C
	private void CheckSpawn()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_triggerDistance);
		if (closestPlayer && !closestPlayer.InGhostMode())
		{
			this.Hatch();
		}
	}

	// Token: 0x0600013C RID: 316 RVA: 0x00008A84 File Offset: 0x00006C84
	private void Hatch()
	{
		this.m_hatchEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		UnityEngine.Object.Instantiate<GameObject>(this.m_spawnPrefab, base.transform.TransformPoint(this.m_spawnOffset), Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f));
		this.m_nview.Destroy();
	}

	// Token: 0x04000129 RID: 297
	public float m_triggerDistance = 5f;

	// Token: 0x0400012A RID: 298
	[Range(0f, 1f)]
	public float m_chanceToHatch = 1f;

	// Token: 0x0400012B RID: 299
	public Vector3 m_spawnOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x0400012C RID: 300
	public GameObject m_spawnPrefab;

	// Token: 0x0400012D RID: 301
	public EffectList m_hatchEffect;

	// Token: 0x0400012E RID: 302
	private ZNetView m_nview;
}
