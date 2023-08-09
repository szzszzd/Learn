using System;
using UnityEngine;

// Token: 0x02000078 RID: 120
public class LightFlicker : MonoBehaviour
{
	// Token: 0x06000595 RID: 1429 RVA: 0x0002BB84 File Offset: 0x00029D84
	private void Awake()
	{
		this.m_light = base.GetComponent<Light>();
		this.m_baseIntensity = this.m_light.intensity;
		this.m_basePosition = base.transform.localPosition;
		this.m_flickerOffset = UnityEngine.Random.Range(0f, 10f);
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x0002BBD4 File Offset: 0x00029DD4
	private void OnEnable()
	{
		this.m_time = 0f;
		if (this.m_light)
		{
			this.m_light.intensity = 0f;
		}
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x0002BC00 File Offset: 0x00029E00
	private void Update()
	{
		if (!this.m_light)
		{
			return;
		}
		if (Settings.ReduceFlashingLights)
		{
			if (this.m_flashingLightingsAccessibility == LightFlicker.LightFlashSettings.Off)
			{
				this.m_light.intensity = 0f;
				return;
			}
			if (this.m_flashingLightingsAccessibility == LightFlicker.LightFlashSettings.AlwaysOn)
			{
				this.m_light.intensity = 1f;
				return;
			}
		}
		this.m_time += Time.deltaTime;
		float num = this.m_flickerOffset + Time.time * this.m_flickerSpeed;
		float num2;
		if (Settings.ReduceFlashingLights && this.m_flashingLightingsAccessibility == LightFlicker.LightFlashSettings.OnIncludeFade)
		{
			num2 = 1f;
		}
		else
		{
			num2 = 1f + Mathf.Sin(num) * Mathf.Sin(num * 0.56436f) * Mathf.Cos(num * 0.758348f) * this.m_flickerIntensity;
		}
		if (this.m_fadeInDuration > 0f)
		{
			num2 *= Utils.LerpStep(0f, this.m_fadeInDuration, this.m_time);
		}
		if (this.m_ttl > 0f)
		{
			if (this.m_time > this.m_ttl)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			float l = this.m_ttl - this.m_fadeDuration;
			num2 *= 1f - Utils.LerpStep(l, this.m_ttl, this.m_time);
		}
		this.m_light.intensity = this.m_baseIntensity * num2;
		Vector3 b = new Vector3(Mathf.Sin(num) * Mathf.Sin(num * 0.56436f), Mathf.Sin(num * 0.56436f) * Mathf.Sin(num * 0.688742f), Mathf.Cos(num * 0.758348f) * Mathf.Cos(num * 0.4563696f)) * this.m_movement;
		base.transform.localPosition = this.m_basePosition + b;
	}

	// Token: 0x040006AD RID: 1709
	public float m_flickerIntensity = 0.1f;

	// Token: 0x040006AE RID: 1710
	public float m_flickerSpeed = 10f;

	// Token: 0x040006AF RID: 1711
	public float m_movement = 0.1f;

	// Token: 0x040006B0 RID: 1712
	public float m_ttl;

	// Token: 0x040006B1 RID: 1713
	public float m_fadeDuration = 0.2f;

	// Token: 0x040006B2 RID: 1714
	public float m_fadeInDuration;

	// Token: 0x040006B3 RID: 1715
	public LightFlicker.LightFlashSettings m_flashingLightingsAccessibility;

	// Token: 0x040006B4 RID: 1716
	private Light m_light;

	// Token: 0x040006B5 RID: 1717
	private float m_baseIntensity = 1f;

	// Token: 0x040006B6 RID: 1718
	private Vector3 m_basePosition = Vector3.zero;

	// Token: 0x040006B7 RID: 1719
	private float m_time;

	// Token: 0x040006B8 RID: 1720
	private float m_flickerOffset;

	// Token: 0x02000079 RID: 121
	public enum LightFlashSettings
	{
		// Token: 0x040006BA RID: 1722
		Default,
		// Token: 0x040006BB RID: 1723
		OnIncludeFade,
		// Token: 0x040006BC RID: 1724
		Off,
		// Token: 0x040006BD RID: 1725
		AlwaysOn
	}
}
