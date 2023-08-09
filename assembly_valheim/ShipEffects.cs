using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000290 RID: 656
public class ShipEffects : MonoBehaviour
{
	// Token: 0x06001921 RID: 6433 RVA: 0x000A7448 File Offset: 0x000A5648
	private void Awake()
	{
		ZNetView componentInParent = base.GetComponentInParent<ZNetView>();
		if (componentInParent && componentInParent.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		this.m_body = base.GetComponentInParent<Rigidbody>();
		this.m_ship = base.GetComponentInParent<Ship>();
		if (this.m_speedWakeRoot)
		{
			this.m_wakeParticles = this.m_speedWakeRoot.GetComponentsInChildren<ParticleSystem>();
		}
		if (this.m_wakeSoundRoot)
		{
			foreach (AudioSource audioSource in this.m_wakeSoundRoot.GetComponentsInChildren<AudioSource>())
			{
				audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				this.m_wakeSounds.Add(new KeyValuePair<AudioSource, float>(audioSource, audioSource.volume));
			}
		}
		if (this.m_inWaterSoundRoot)
		{
			foreach (AudioSource audioSource2 in this.m_inWaterSoundRoot.GetComponentsInChildren<AudioSource>())
			{
				audioSource2.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				this.m_inWaterSounds.Add(new KeyValuePair<AudioSource, float>(audioSource2, audioSource2.volume));
			}
		}
		if (this.m_sailSound)
		{
			this.m_sailBaseVol = this.m_sailSound.volume;
			this.m_sailSound.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
		}
	}

	// Token: 0x06001922 RID: 6434 RVA: 0x000A7594 File Offset: 0x000A5794
	private void OnEnable()
	{
		ShipEffects.Instances.Add(this);
	}

	// Token: 0x06001923 RID: 6435 RVA: 0x000A75A1 File Offset: 0x000A57A1
	private void OnDisable()
	{
		ShipEffects.Instances.Remove(this);
	}

	// Token: 0x06001924 RID: 6436 RVA: 0x000A75B0 File Offset: 0x000A57B0
	public void CustomLateUpdate()
	{
		float waterLevel = Floating.GetWaterLevel(base.transform.position, ref this.m_previousWaterVolume);
		ref Vector3 position = base.transform.position;
		float deltaTime = Time.deltaTime;
		if (position.y > waterLevel)
		{
			this.m_shadow.gameObject.SetActive(false);
			this.SetWake(false, deltaTime);
			this.FadeSounds(this.m_inWaterSounds, false, deltaTime);
			return;
		}
		this.m_shadow.gameObject.SetActive(true);
		bool enabled = this.m_body.velocity.magnitude > this.m_minimumWakeVel;
		this.FadeSounds(this.m_inWaterSounds, true, deltaTime);
		this.SetWake(enabled, deltaTime);
		if (this.m_sailSound)
		{
			float target = this.m_ship.IsSailUp() ? this.m_sailBaseVol : 0f;
			ShipEffects.FadeSound(this.m_sailSound, target, this.m_sailFadeDuration, deltaTime);
		}
		if (this.m_splashEffects != null)
		{
			this.m_splashEffects.SetActive(this.m_ship.HasPlayerOnboard());
		}
	}

	// Token: 0x06001925 RID: 6437 RVA: 0x000A76BC File Offset: 0x000A58BC
	private void SetWake(bool enabled, float dt)
	{
		ParticleSystem[] wakeParticles = this.m_wakeParticles;
		for (int i = 0; i < wakeParticles.Length; i++)
		{
			wakeParticles[i].emission.enabled = enabled;
		}
		this.FadeSounds(this.m_wakeSounds, enabled, dt);
	}

	// Token: 0x06001926 RID: 6438 RVA: 0x000A7700 File Offset: 0x000A5900
	private void FadeSounds(List<KeyValuePair<AudioSource, float>> sources, bool enabled, float dt)
	{
		foreach (KeyValuePair<AudioSource, float> keyValuePair in sources)
		{
			if (enabled)
			{
				ShipEffects.FadeSound(keyValuePair.Key, keyValuePair.Value, this.m_audioFadeDuration, dt);
			}
			else
			{
				ShipEffects.FadeSound(keyValuePair.Key, 0f, this.m_audioFadeDuration, dt);
			}
		}
	}

	// Token: 0x06001927 RID: 6439 RVA: 0x000A7780 File Offset: 0x000A5980
	private static void FadeSound(AudioSource source, float target, float fadeDuration, float dt)
	{
		float maxDelta = dt / fadeDuration;
		if (target > 0f)
		{
			if (!source.isPlaying)
			{
				source.Play();
			}
			source.volume = Mathf.MoveTowards(source.volume, target, maxDelta);
			return;
		}
		if (source.isPlaying)
		{
			source.volume = Mathf.MoveTowards(source.volume, 0f, maxDelta);
			if (source.volume <= 0f)
			{
				source.Stop();
			}
		}
	}

	// Token: 0x170000F3 RID: 243
	// (get) Token: 0x06001928 RID: 6440 RVA: 0x000A77ED File Offset: 0x000A59ED
	public static List<ShipEffects> Instances { get; } = new List<ShipEffects>();

	// Token: 0x04001B13 RID: 6931
	public Transform m_shadow;

	// Token: 0x04001B14 RID: 6932
	public float m_offset = 0.01f;

	// Token: 0x04001B15 RID: 6933
	public float m_minimumWakeVel = 5f;

	// Token: 0x04001B16 RID: 6934
	public GameObject m_speedWakeRoot;

	// Token: 0x04001B17 RID: 6935
	public GameObject m_wakeSoundRoot;

	// Token: 0x04001B18 RID: 6936
	public GameObject m_inWaterSoundRoot;

	// Token: 0x04001B19 RID: 6937
	public float m_audioFadeDuration = 2f;

	// Token: 0x04001B1A RID: 6938
	public AudioSource m_sailSound;

	// Token: 0x04001B1B RID: 6939
	public float m_sailFadeDuration = 1f;

	// Token: 0x04001B1C RID: 6940
	public GameObject m_splashEffects;

	// Token: 0x04001B1D RID: 6941
	private ParticleSystem[] m_wakeParticles;

	// Token: 0x04001B1E RID: 6942
	private float m_sailBaseVol = 1f;

	// Token: 0x04001B1F RID: 6943
	private readonly List<KeyValuePair<AudioSource, float>> m_wakeSounds = new List<KeyValuePair<AudioSource, float>>();

	// Token: 0x04001B20 RID: 6944
	private readonly List<KeyValuePair<AudioSource, float>> m_inWaterSounds = new List<KeyValuePair<AudioSource, float>>();

	// Token: 0x04001B21 RID: 6945
	private WaterVolume m_previousWaterVolume;

	// Token: 0x04001B22 RID: 6946
	private Rigidbody m_body;

	// Token: 0x04001B23 RID: 6947
	private Ship m_ship;
}
