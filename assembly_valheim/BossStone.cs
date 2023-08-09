using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200021A RID: 538
public class BossStone : MonoBehaviour
{
	// Token: 0x06001562 RID: 5474 RVA: 0x0008C414 File Offset: 0x0008A614
	private void Start()
	{
		if (this.m_mesh.materials[this.m_emissiveMaterialIndex].HasProperty("_EmissionColor"))
		{
			this.m_mesh.materials[this.m_emissiveMaterialIndex].SetColor("_EmissionColor", Color.black);
		}
		if (this.m_activeEffect)
		{
			this.m_activeEffect.SetActive(false);
		}
		this.SetActivated(this.m_itemStand.HaveAttachment(), false);
		base.InvokeRepeating("UpdateVisual", 1f, 1f);
	}

	// Token: 0x06001563 RID: 5475 RVA: 0x0008C4A0 File Offset: 0x0008A6A0
	private void UpdateVisual()
	{
		this.SetActivated(this.m_itemStand.HaveAttachment(), true);
	}

	// Token: 0x06001564 RID: 5476 RVA: 0x0008C4B4 File Offset: 0x0008A6B4
	private void SetActivated(bool active, bool triggerEffect)
	{
		if (active == this.m_active)
		{
			return;
		}
		this.m_active = active;
		if (triggerEffect && active)
		{
			base.Invoke("DelayedAttachEffects_Step1", 1f);
			base.Invoke("DelayedAttachEffects_Step2", 5f);
			base.Invoke("DelayedAttachEffects_Step3", 11f);
			return;
		}
		if (this.m_activeEffect)
		{
			this.m_activeEffect.SetActive(active);
		}
		base.StopCoroutine("FadeEmission");
		base.StartCoroutine("FadeEmission");
	}

	// Token: 0x06001565 RID: 5477 RVA: 0x0008C538 File Offset: 0x0008A738
	private void DelayedAttachEffects_Step1()
	{
		this.m_activateStep1.Create(this.m_itemStand.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x06001566 RID: 5478 RVA: 0x0008C568 File Offset: 0x0008A768
	private void DelayedAttachEffects_Step2()
	{
		this.m_activateStep2.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x06001567 RID: 5479 RVA: 0x0008C594 File Offset: 0x0008A794
	private void DelayedAttachEffects_Step3()
	{
		if (this.m_activeEffect)
		{
			this.m_activeEffect.SetActive(true);
		}
		this.m_activateStep3.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		base.StopCoroutine("FadeEmission");
		base.StartCoroutine("FadeEmission");
		Player.MessageAllInRange(base.transform.position, 20f, MessageHud.MessageType.Center, this.m_completedMessage, null);
	}

	// Token: 0x06001568 RID: 5480 RVA: 0x0008C617 File Offset: 0x0008A817
	private IEnumerator FadeEmission()
	{
		if (this.m_mesh && this.m_mesh.materials[this.m_emissiveMaterialIndex].HasProperty("_EmissionColor"))
		{
			Color startColor = this.m_mesh.materials[this.m_emissiveMaterialIndex].GetColor("_EmissionColor");
			Color targetColor = this.m_active ? this.m_activeEmissiveColor : Color.black;
			for (float t = 0f; t < 1f; t += Time.deltaTime)
			{
				Color value = Color.Lerp(startColor, targetColor, t / 1f);
				this.m_mesh.materials[this.m_emissiveMaterialIndex].SetColor("_EmissionColor", value);
				yield return null;
			}
			startColor = default(Color);
			targetColor = default(Color);
		}
		ZLog.Log("Done fading color");
		yield break;
	}

	// Token: 0x06001569 RID: 5481 RVA: 0x0008C626 File Offset: 0x0008A826
	public bool IsActivated()
	{
		return this.m_active;
	}

	// Token: 0x04001631 RID: 5681
	public ItemStand m_itemStand;

	// Token: 0x04001632 RID: 5682
	public GameObject m_activeEffect;

	// Token: 0x04001633 RID: 5683
	public EffectList m_activateStep1 = new EffectList();

	// Token: 0x04001634 RID: 5684
	public EffectList m_activateStep2 = new EffectList();

	// Token: 0x04001635 RID: 5685
	public EffectList m_activateStep3 = new EffectList();

	// Token: 0x04001636 RID: 5686
	public string m_completedMessage = "";

	// Token: 0x04001637 RID: 5687
	public MeshRenderer m_mesh;

	// Token: 0x04001638 RID: 5688
	public int m_emissiveMaterialIndex;

	// Token: 0x04001639 RID: 5689
	public Color m_activeEmissiveColor = Color.white;

	// Token: 0x0400163A RID: 5690
	private bool m_active;

	// Token: 0x0400163B RID: 5691
	private ZNetView m_nview;
}
