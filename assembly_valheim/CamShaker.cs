using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000069 RID: 105
public class CamShaker : MonoBehaviour
{
	// Token: 0x06000552 RID: 1362 RVA: 0x00029EFC File Offset: 0x000280FC
	private void Start()
	{
		if (this.m_continous)
		{
			if (this.m_delay <= 0f)
			{
				base.StartCoroutine("TriggerContinous");
				return;
			}
			base.Invoke("DelayedTriggerContinous", this.m_delay);
			return;
		}
		else
		{
			if (this.m_delay <= 0f)
			{
				this.Trigger();
				return;
			}
			base.Invoke("Trigger", this.m_delay);
			return;
		}
	}

	// Token: 0x06000553 RID: 1363 RVA: 0x00029F62 File Offset: 0x00028162
	private void DelayedTriggerContinous()
	{
		base.StartCoroutine("TriggerContinous");
	}

	// Token: 0x06000554 RID: 1364 RVA: 0x00029F70 File Offset: 0x00028170
	private IEnumerator TriggerContinous()
	{
		float t = 0f;
		for (;;)
		{
			this.Trigger();
			t += Time.deltaTime;
			if (this.m_continousDuration > 0f && t > this.m_continousDuration)
			{
				break;
			}
			yield return null;
		}
		yield break;
		yield break;
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x00029F80 File Offset: 0x00028180
	private void Trigger()
	{
		if (GameCamera.instance)
		{
			if (this.m_localOnly)
			{
				ZNetView component = base.GetComponent<ZNetView>();
				if (component && component.IsValid() && !component.IsOwner())
				{
					return;
				}
			}
			GameCamera.instance.AddShake(base.transform.position, this.m_range, this.m_strength, this.m_continous);
		}
	}

	// Token: 0x04000636 RID: 1590
	public float m_strength = 1f;

	// Token: 0x04000637 RID: 1591
	public float m_range = 50f;

	// Token: 0x04000638 RID: 1592
	public float m_delay;

	// Token: 0x04000639 RID: 1593
	public bool m_continous;

	// Token: 0x0400063A RID: 1594
	public float m_continousDuration;

	// Token: 0x0400063B RID: 1595
	public bool m_localOnly;
}
