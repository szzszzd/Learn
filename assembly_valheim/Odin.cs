using System;
using UnityEngine;

// Token: 0x02000271 RID: 625
public class Odin : MonoBehaviour
{
	// Token: 0x06001808 RID: 6152 RVA: 0x000A024E File Offset: 0x0009E44E
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
	}

	// Token: 0x06001809 RID: 6153 RVA: 0x000A025C File Offset: 0x0009E45C
	private void Update()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_despawnFarDistance);
		if (closestPlayer == null)
		{
			this.m_despawn.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			this.m_nview.Destroy();
			ZLog.Log("No player in range, despawning");
			return;
		}
		Vector3 forward = closestPlayer.transform.position - base.transform.position;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation(forward);
		if (Vector3.Distance(closestPlayer.transform.position, base.transform.position) < this.m_despawnCloseDistance)
		{
			this.m_despawn.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			this.m_nview.Destroy();
			ZLog.Log("Player go too close,despawning");
			return;
		}
		this.m_time += Time.deltaTime;
		if (this.m_time > this.m_ttl)
		{
			this.m_despawn.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			this.m_nview.Destroy();
			ZLog.Log("timeout " + this.m_time.ToString() + " , despawning");
			return;
		}
	}

	// Token: 0x0400198A RID: 6538
	public float m_despawnCloseDistance = 20f;

	// Token: 0x0400198B RID: 6539
	public float m_despawnFarDistance = 50f;

	// Token: 0x0400198C RID: 6540
	public EffectList m_despawn = new EffectList();

	// Token: 0x0400198D RID: 6541
	public float m_ttl = 300f;

	// Token: 0x0400198E RID: 6542
	private float m_time;

	// Token: 0x0400198F RID: 6543
	private ZNetView m_nview;
}
