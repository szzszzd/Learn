using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200027D RID: 637
public class Radiator : MonoBehaviour
{
	// Token: 0x0600186C RID: 6252 RVA: 0x000A2D87 File Offset: 0x000A0F87
	private void Start()
	{
		this.m_nview = base.GetComponentInParent<ZNetView>();
	}

	// Token: 0x0600186D RID: 6253 RVA: 0x00084517 File Offset: 0x00082717
	private void OnEnable()
	{
		base.StartCoroutine("UpdateLoop");
	}

	// Token: 0x0600186E RID: 6254 RVA: 0x000A2D95 File Offset: 0x000A0F95
	private IEnumerator UpdateLoop()
	{
		for (;;)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(this.m_rateMin, this.m_rateMax));
			if (this.m_nview.IsValid() && this.m_nview.IsOwner())
			{
				Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
				Vector3 position = base.transform.position;
				if (onUnitSphere.y < 0f)
				{
					onUnitSphere.y = -onUnitSphere.y;
				}
				if (this.m_emitFrom)
				{
					position = this.m_emitFrom.ClosestPoint(this.m_emitFrom.transform.position + onUnitSphere * 1000f) + onUnitSphere * this.m_offset;
				}
				UnityEngine.Object.Instantiate<GameObject>(this.m_projectile, position, Quaternion.LookRotation(onUnitSphere, Vector3.up)).GetComponent<Projectile>().Setup(null, onUnitSphere * this.m_velocity, 0f, null, null, null);
			}
		}
		yield break;
	}

	// Token: 0x04001A2A RID: 6698
	public GameObject m_projectile;

	// Token: 0x04001A2B RID: 6699
	public Collider m_emitFrom;

	// Token: 0x04001A2C RID: 6700
	public float m_rateMin = 2f;

	// Token: 0x04001A2D RID: 6701
	public float m_rateMax = 5f;

	// Token: 0x04001A2E RID: 6702
	public float m_velocity = 10f;

	// Token: 0x04001A2F RID: 6703
	public float m_offset = 0.1f;

	// Token: 0x04001A30 RID: 6704
	private ZNetView m_nview;
}
