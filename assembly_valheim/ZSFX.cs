using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000092 RID: 146
public class ZSFX : MonoBehaviour
{
	// Token: 0x06000644 RID: 1604 RVA: 0x0002FCA0 File Offset: 0x0002DEA0
	public void Awake()
	{
		this.m_delay = UnityEngine.Random.Range(this.m_minDelay, this.m_maxDelay);
		this.m_audioSource = base.GetComponent<AudioSource>();
		this.m_baseSpread = this.m_audioSource.spread;
	}

	// Token: 0x06000645 RID: 1605 RVA: 0x0002FCD6 File Offset: 0x0002DED6
	private void OnEnable()
	{
		ZSFX.Instances.Add(this);
	}

	// Token: 0x06000646 RID: 1606 RVA: 0x0002FCE4 File Offset: 0x0002DEE4
	private void OnDisable()
	{
		if (this.m_playOnAwake && this.m_audioSource.loop)
		{
			this.m_time = 0f;
			this.m_delay = UnityEngine.Random.Range(this.m_minDelay, this.m_maxDelay);
			this.m_audioSource.Stop();
		}
		ZSFX.Instances.Remove(this);
	}

	// Token: 0x06000647 RID: 1607 RVA: 0x0002FD40 File Offset: 0x0002DF40
	public void CustomUpdate(float dt)
	{
		if (this.m_audioSource == null)
		{
			return;
		}
		this.m_time += dt;
		if (this.m_delay >= 0f && this.m_time >= this.m_delay)
		{
			this.m_delay = -1f;
			if (this.m_playOnAwake)
			{
				this.Play();
			}
		}
		if (this.m_audioSource.isPlaying)
		{
			if (this.m_distanceReverb && this.m_audioSource.loop)
			{
				this.m_updateReverbTimer += dt;
				if (this.m_updateReverbTimer > 1f)
				{
					this.m_updateReverbTimer = 0f;
					this.UpdateReverb();
				}
			}
			if (this.m_fadeOutOnAwake && this.m_time > this.m_fadeOutDelay)
			{
				this.m_fadeOutOnAwake = false;
				this.FadeOut();
			}
			if (this.m_fadeOutTimer >= 0f)
			{
				this.m_fadeOutTimer += dt;
				if (this.m_fadeOutTimer >= this.m_fadeOutDuration)
				{
					this.m_audioSource.volume = 0f;
					this.Stop();
					return;
				}
				float num = Mathf.Clamp01(this.m_fadeOutTimer / this.m_fadeOutDuration);
				this.m_audioSource.volume = (1f - num) * this.m_vol;
				return;
			}
			else if (this.m_fadeInTimer >= 0f)
			{
				this.m_fadeInTimer += Time.deltaTime;
				float num2 = Mathf.Clamp01(this.m_fadeInTimer / this.m_fadeInDuration);
				this.m_audioSource.volume = num2 * this.m_vol;
				if (this.m_fadeInTimer > this.m_fadeInDuration)
				{
					this.m_fadeInTimer = -1f;
				}
			}
		}
	}

	// Token: 0x06000648 RID: 1608 RVA: 0x0002FEDD File Offset: 0x0002E0DD
	public void FadeOut()
	{
		if (this.m_fadeOutTimer < 0f)
		{
			this.m_fadeOutTimer = 0f;
		}
	}

	// Token: 0x06000649 RID: 1609 RVA: 0x0002FEF7 File Offset: 0x0002E0F7
	public void Stop()
	{
		if (this.m_audioSource != null)
		{
			this.m_audioSource.Stop();
		}
	}

	// Token: 0x0600064A RID: 1610 RVA: 0x0002FF12 File Offset: 0x0002E112
	public bool IsPlaying()
	{
		return !(this.m_audioSource == null) && this.m_audioSource.isPlaying;
	}

	// Token: 0x0600064B RID: 1611 RVA: 0x0002FF30 File Offset: 0x0002E130
	private void UpdateReverb()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (this.m_distanceReverb && this.m_audioSource.spatialBlend != 0f && mainCamera != null)
		{
			float num = Vector3.Distance(mainCamera.transform.position, base.transform.position);
			bool flag = Mister.InsideMister(base.transform.position, 0f);
			float num2 = this.m_useCustomReverbDistance ? this.m_customReverbDistance : 64f;
			float num3 = Mathf.Clamp01(num / num2);
			float b = Mathf.Clamp01(this.m_audioSource.maxDistance / num2) * Mathf.Clamp01(num / this.m_audioSource.maxDistance);
			float num4 = Mathf.Max(num3, b);
			if (flag)
			{
				num4 = Mathf.Lerp(num4, 0f, num3);
				this.m_audioSource.pitch = this.m_basePitch - this.m_basePitch * 0.5f * num3;
			}
			this.m_audioSource.bypassReverbZones = false;
			this.m_audioSource.reverbZoneMix = num4;
			if (this.m_baseSpread < 120f)
			{
				float a = Mathf.Max(this.m_baseSpread, 45f);
				this.m_audioSource.spread = Mathf.Lerp(a, 120f, num4);
				return;
			}
		}
		else
		{
			this.m_audioSource.bypassReverbZones = true;
		}
	}

	// Token: 0x0600064C RID: 1612 RVA: 0x00030080 File Offset: 0x0002E280
	public void Play()
	{
		if (this.m_audioSource == null)
		{
			return;
		}
		if (this.m_audioClips.Length == 0)
		{
			return;
		}
		if (!this.m_audioSource.gameObject.activeInHierarchy)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, this.m_audioClips.Length);
		this.m_audioSource.clip = this.m_audioClips[num];
		this.m_audioSource.pitch = UnityEngine.Random.Range(this.m_minPitch, this.m_maxPitch);
		this.m_basePitch = this.m_audioSource.pitch;
		if (this.m_randomPan)
		{
			this.m_audioSource.panStereo = UnityEngine.Random.Range(this.m_minPan, this.m_maxPan);
		}
		this.m_vol = UnityEngine.Random.Range(this.m_minVol, this.m_maxVol);
		if (this.m_fadeInDuration > 0f)
		{
			this.m_audioSource.volume = 0f;
			this.m_fadeInTimer = 0f;
		}
		else
		{
			this.m_audioSource.volume = this.m_vol;
		}
		this.UpdateReverb();
		this.m_audioSource.Play();
	}

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x0600064D RID: 1613 RVA: 0x0003018F File Offset: 0x0002E38F
	public static List<ZSFX> Instances { get; } = new List<ZSFX>();

	// Token: 0x040007A6 RID: 1958
	public bool m_playOnAwake = true;

	// Token: 0x040007A7 RID: 1959
	[Header("Clips")]
	public AudioClip[] m_audioClips = new AudioClip[0];

	// Token: 0x040007A8 RID: 1960
	[Header("Random")]
	public float m_maxPitch = 1f;

	// Token: 0x040007A9 RID: 1961
	public float m_minPitch = 1f;

	// Token: 0x040007AA RID: 1962
	public float m_maxVol = 1f;

	// Token: 0x040007AB RID: 1963
	public float m_minVol = 1f;

	// Token: 0x040007AC RID: 1964
	[Header("Fade")]
	public float m_fadeInDuration;

	// Token: 0x040007AD RID: 1965
	public float m_fadeOutDuration;

	// Token: 0x040007AE RID: 1966
	public float m_fadeOutDelay;

	// Token: 0x040007AF RID: 1967
	public bool m_fadeOutOnAwake;

	// Token: 0x040007B0 RID: 1968
	[Header("Pan")]
	public bool m_randomPan;

	// Token: 0x040007B1 RID: 1969
	public float m_minPan = -1f;

	// Token: 0x040007B2 RID: 1970
	public float m_maxPan = 1f;

	// Token: 0x040007B3 RID: 1971
	[Header("Delay")]
	public float m_maxDelay;

	// Token: 0x040007B4 RID: 1972
	public float m_minDelay;

	// Token: 0x040007B5 RID: 1973
	[Header("Reverb")]
	public bool m_distanceReverb = true;

	// Token: 0x040007B6 RID: 1974
	public bool m_useCustomReverbDistance;

	// Token: 0x040007B7 RID: 1975
	public float m_customReverbDistance = 10f;

	// Token: 0x040007B8 RID: 1976
	private const float m_globalReverbDistance = 64f;

	// Token: 0x040007B9 RID: 1977
	private const float m_minReverbSpread = 45f;

	// Token: 0x040007BA RID: 1978
	private const float m_maxReverbSpread = 120f;

	// Token: 0x040007BB RID: 1979
	private float m_delay;

	// Token: 0x040007BC RID: 1980
	private float m_time;

	// Token: 0x040007BD RID: 1981
	private float m_fadeOutTimer = -1f;

	// Token: 0x040007BE RID: 1982
	private float m_fadeInTimer = -1f;

	// Token: 0x040007BF RID: 1983
	private float m_vol = 1f;

	// Token: 0x040007C0 RID: 1984
	private float m_baseSpread;

	// Token: 0x040007C1 RID: 1985
	private float m_basePitch;

	// Token: 0x040007C2 RID: 1986
	private float m_updateReverbTimer;

	// Token: 0x040007C3 RID: 1987
	private AudioSource m_audioSource;
}
