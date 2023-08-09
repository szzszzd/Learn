using System;
using UnityEngine;

// Token: 0x0200006E RID: 110
public class EffectFade : MonoBehaviour
{
	// Token: 0x06000577 RID: 1399 RVA: 0x0002AAE0 File Offset: 0x00028CE0
	private void Awake()
	{
		this.m_particles = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		this.m_light = base.gameObject.GetComponentInChildren<Light>();
		this.m_audioSource = base.gameObject.GetComponentInChildren<AudioSource>();
		if (this.m_light)
		{
			this.m_lightBaseIntensity = this.m_light.intensity;
			this.m_light.intensity = 0f;
		}
		if (this.m_audioSource)
		{
			this.m_baseVolume = this.m_audioSource.volume;
			this.m_audioSource.volume = 0f;
		}
		this.SetActive(false);
	}

	// Token: 0x06000578 RID: 1400 RVA: 0x0002AB84 File Offset: 0x00028D84
	private void Update()
	{
		this.m_intensity = Mathf.MoveTowards(this.m_intensity, this.m_active ? 1f : 0f, Time.deltaTime / this.m_fadeDuration);
		if (this.m_light)
		{
			this.m_light.intensity = this.m_intensity * this.m_lightBaseIntensity;
			this.m_light.enabled = (this.m_light.intensity > 0f);
		}
		if (this.m_audioSource)
		{
			this.m_audioSource.volume = this.m_intensity * this.m_baseVolume;
		}
	}

	// Token: 0x06000579 RID: 1401 RVA: 0x0002AC2C File Offset: 0x00028E2C
	public void SetActive(bool active)
	{
		if (this.m_active == active)
		{
			return;
		}
		this.m_active = active;
		ParticleSystem[] particles = this.m_particles;
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].emission.enabled = active;
		}
	}

	// Token: 0x04000664 RID: 1636
	public float m_fadeDuration = 1f;

	// Token: 0x04000665 RID: 1637
	private ParticleSystem[] m_particles;

	// Token: 0x04000666 RID: 1638
	private Light m_light;

	// Token: 0x04000667 RID: 1639
	private AudioSource m_audioSource;

	// Token: 0x04000668 RID: 1640
	private float m_baseVolume;

	// Token: 0x04000669 RID: 1641
	private float m_lightBaseIntensity;

	// Token: 0x0400066A RID: 1642
	private bool m_active = true;

	// Token: 0x0400066B RID: 1643
	private float m_intensity;
}
