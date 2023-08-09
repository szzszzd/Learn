using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

// Token: 0x020001B3 RID: 435
public class AudioMan : MonoBehaviour
{
	// Token: 0x170000B9 RID: 185
	// (get) Token: 0x0600117E RID: 4478 RVA: 0x0007065A File Offset: 0x0006E85A
	public static AudioMan instance
	{
		get
		{
			return AudioMan.m_instance;
		}
	}

	// Token: 0x0600117F RID: 4479 RVA: 0x00070664 File Offset: 0x0006E864
	private void Awake()
	{
		if (AudioMan.m_instance != null)
		{
			ZLog.Log("Audioman already exist, destroying self");
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		AudioMan.m_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameObject gameObject = new GameObject("ocean_ambient_loop");
		gameObject.transform.SetParent(base.transform);
		this.m_oceanAmbientSource = gameObject.AddComponent<AudioSource>();
		this.m_oceanAmbientSource.loop = true;
		this.m_oceanAmbientSource.spatialBlend = 0.75f;
		this.m_oceanAmbientSource.outputAudioMixerGroup = this.m_ambientMixer;
		this.m_oceanAmbientSource.maxDistance = 128f;
		this.m_oceanAmbientSource.minDistance = 40f;
		this.m_oceanAmbientSource.spread = 90f;
		this.m_oceanAmbientSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_oceanAmbientSource.clip = this.m_oceanAudio;
		this.m_oceanAmbientSource.bypassReverbZones = true;
		this.m_oceanAmbientSource.dopplerLevel = 0f;
		this.m_oceanAmbientSource.volume = 0f;
		this.m_oceanAmbientSource.priority = 0;
		this.m_oceanAmbientSource.Play();
		GameObject gameObject2 = new GameObject("ambient_loop");
		gameObject2.transform.SetParent(base.transform);
		this.m_ambientLoopSource = gameObject2.AddComponent<AudioSource>();
		this.m_ambientLoopSource.loop = true;
		this.m_ambientLoopSource.spatialBlend = 0f;
		this.m_ambientLoopSource.outputAudioMixerGroup = this.m_ambientMixer;
		this.m_ambientLoopSource.bypassReverbZones = true;
		this.m_ambientLoopSource.priority = 0;
		this.m_ambientLoopSource.volume = 0f;
		GameObject gameObject3 = new GameObject("wind_loop");
		gameObject3.transform.SetParent(base.transform);
		this.m_windLoopSource = gameObject3.AddComponent<AudioSource>();
		this.m_windLoopSource.loop = true;
		this.m_windLoopSource.spatialBlend = 0f;
		this.m_windLoopSource.outputAudioMixerGroup = this.m_ambientMixer;
		this.m_windLoopSource.bypassReverbZones = true;
		this.m_windLoopSource.clip = this.m_windAudio;
		this.m_windLoopSource.volume = 0f;
		this.m_windLoopSource.priority = 0;
		this.m_windLoopSource.Play();
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			AudioListener.volume = 0f;
			return;
		}
		AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", AudioListener.volume);
		AudioMan.SetSFXVolume(PlayerPrefs.GetFloat("SfxVolume", AudioMan.GetSFXVolume()));
	}

	// Token: 0x06001180 RID: 4480 RVA: 0x000708DB File Offset: 0x0006EADB
	private void OnDestroy()
	{
		if (AudioMan.m_instance == this)
		{
			AudioMan.m_instance = null;
		}
	}

	// Token: 0x06001181 RID: 4481 RVA: 0x000708F0 File Offset: 0x0006EAF0
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		this.UpdateAmbientLoop(deltaTime);
		this.UpdateRandomAmbient(deltaTime);
		this.UpdateSnapshots(deltaTime);
	}

	// Token: 0x06001182 RID: 4482 RVA: 0x00070918 File Offset: 0x0006EB18
	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateOceanAmbiance(fixedDeltaTime);
		this.UpdateWindAmbience(fixedDeltaTime);
	}

	// Token: 0x06001183 RID: 4483 RVA: 0x0007093C File Offset: 0x0006EB3C
	public static float GetSFXVolume()
	{
		if (AudioMan.m_instance == null)
		{
			return 1f;
		}
		float num;
		AudioMan.m_instance.m_masterMixer.GetFloat("SfxVol", out num);
		if (num <= -80f)
		{
			return 0f;
		}
		return Mathf.Pow(10f, num / 10f);
	}

	// Token: 0x06001184 RID: 4484 RVA: 0x00070994 File Offset: 0x0006EB94
	public static void SetSFXVolume(float vol)
	{
		if (AudioMan.m_instance == null)
		{
			return;
		}
		float value = (vol > 0f) ? (Mathf.Log10(Mathf.Clamp(vol, 0.001f, 1f)) * 10f) : -80f;
		AudioMan.m_instance.m_masterMixer.SetFloat("SfxVol", value);
	}

	// Token: 0x06001185 RID: 4485 RVA: 0x000709F0 File Offset: 0x0006EBF0
	private void UpdateRandomAmbient(float dt)
	{
		if (this.InMenu())
		{
			return;
		}
		this.m_randomAmbientTimer += dt;
		if (this.m_randomAmbientTimer > this.m_randomAmbientInterval)
		{
			this.m_randomAmbientTimer = 0f;
			if (UnityEngine.Random.value <= this.m_randomAmbientChance)
			{
				float num = 0f;
				AudioClip audioClip;
				if (this.SelectRandomAmbientClip(out audioClip, out num))
				{
					Vector3 randomAmbiencePoint = this.GetRandomAmbiencePoint();
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_randomAmbientPrefab, randomAmbiencePoint, Quaternion.identity, base.transform);
					ZSFX component = gameObject.GetComponent<ZSFX>();
					component.m_audioClips = new AudioClip[]
					{
						audioClip
					};
					component.Play();
					TimedDestruction component2 = gameObject.GetComponent<TimedDestruction>();
					if (num > 0f)
					{
						component.m_fadeOutDelay = 0f;
						component.m_fadeOutDuration = num;
						component.m_fadeOutOnAwake = true;
						component2.m_timeout = num + 2f;
					}
					else
					{
						component.m_fadeOutDelay = audioClip.length - 1f;
						component.m_fadeOutDuration = 1f;
						component.m_fadeOutOnAwake = true;
						component2.m_timeout = audioClip.length * 1.5f;
					}
					component2.Trigger();
				}
			}
		}
	}

	// Token: 0x06001186 RID: 4486 RVA: 0x00070B14 File Offset: 0x0006ED14
	private Vector3 GetRandomAmbiencePoint()
	{
		Vector3 a = Vector3.zero;
		Camera mainCamera = Utils.GetMainCamera();
		if (Player.m_localPlayer)
		{
			a = Player.m_localPlayer.transform.position;
		}
		else if (mainCamera)
		{
			a = mainCamera.transform.position;
		}
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = UnityEngine.Random.Range(this.m_randomMinDistance, this.m_randomMaxDistance);
		return a + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
	}

	// Token: 0x06001187 RID: 4487 RVA: 0x00070BA4 File Offset: 0x0006EDA4
	private bool SelectRandomAmbientClip(out AudioClip clip, out float fadeoutDuration)
	{
		fadeoutDuration = 0f;
		clip = null;
		if (EnvMan.instance == null)
		{
			return false;
		}
		EnvSetup currentEnvironment = EnvMan.instance.GetCurrentEnvironment();
		AudioMan.BiomeAmbients biomeAmbients;
		if (currentEnvironment != null && !string.IsNullOrEmpty(currentEnvironment.m_ambientList))
		{
			biomeAmbients = this.GetAmbients(currentEnvironment.m_ambientList);
		}
		else
		{
			biomeAmbients = this.GetBiomeAmbients(EnvMan.instance.GetCurrentBiome());
		}
		if (biomeAmbients == null)
		{
			return false;
		}
		fadeoutDuration = biomeAmbients.m_forceFadeout;
		List<AudioClip> list = new List<AudioClip>(biomeAmbients.m_randomAmbientClips);
		List<AudioClip> collection = EnvMan.instance.IsDaylight() ? biomeAmbients.m_randomAmbientClipsDay : biomeAmbients.m_randomAmbientClipsNight;
		list.AddRange(collection);
		if (list.Count == 0)
		{
			return false;
		}
		clip = list[UnityEngine.Random.Range(0, list.Count)];
		return true;
	}

	// Token: 0x06001188 RID: 4488 RVA: 0x00070C64 File Offset: 0x0006EE64
	private void UpdateAmbientLoop(float dt)
	{
		if (EnvMan.instance == null)
		{
			this.m_ambientLoopSource.Stop();
			return;
		}
		if (this.m_queuedAmbientLoop || this.m_stopAmbientLoop)
		{
			if (this.m_ambientLoopSource.isPlaying && this.m_ambientLoopSource.volume > 0f)
			{
				this.m_ambientLoopSource.volume = Mathf.MoveTowards(this.m_ambientLoopSource.volume, 0f, dt / this.m_ambientFadeTime);
				return;
			}
			this.m_ambientLoopSource.Stop();
			this.m_stopAmbientLoop = false;
			if (this.m_queuedAmbientLoop)
			{
				this.m_ambientLoopSource.clip = this.m_queuedAmbientLoop;
				this.m_ambientLoopSource.volume = 0f;
				this.m_ambientLoopSource.Play();
				this.m_ambientVol = this.m_queuedAmbientVol;
				this.m_queuedAmbientLoop = null;
				return;
			}
		}
		else if (this.m_ambientLoopSource.isPlaying)
		{
			this.m_ambientLoopSource.volume = Mathf.MoveTowards(this.m_ambientLoopSource.volume, this.m_ambientVol, dt / this.m_ambientFadeTime);
		}
	}

	// Token: 0x06001189 RID: 4489 RVA: 0x00070D82 File Offset: 0x0006EF82
	public void SetIndoor(bool indoor)
	{
		this.m_indoor = indoor;
	}

	// Token: 0x0600118A RID: 4490 RVA: 0x00070D8B File Offset: 0x0006EF8B
	private bool InMenu()
	{
		return FejdStartup.instance != null || Menu.IsVisible() || (Game.instance && Game.instance.WaitingForRespawn()) || TextViewer.IsShowingIntro();
	}

	// Token: 0x0600118B RID: 4491 RVA: 0x00070DC0 File Offset: 0x0006EFC0
	private void UpdateSnapshots(float dt)
	{
		if (this.InMenu())
		{
			this.SetSnapshot(AudioMan.Snapshot.Menu);
			return;
		}
		if (this.m_indoor)
		{
			this.SetSnapshot(AudioMan.Snapshot.Indoor);
			return;
		}
		this.SetSnapshot(AudioMan.Snapshot.Default);
	}

	// Token: 0x0600118C RID: 4492 RVA: 0x00070DEC File Offset: 0x0006EFEC
	private void SetSnapshot(AudioMan.Snapshot snapshot)
	{
		if (this.m_currentSnapshot == snapshot)
		{
			return;
		}
		this.m_currentSnapshot = snapshot;
		switch (snapshot)
		{
		case AudioMan.Snapshot.Default:
			this.m_masterMixer.FindSnapshot("Default").TransitionTo(this.m_snapshotTransitionTime);
			return;
		case AudioMan.Snapshot.Menu:
			this.m_masterMixer.FindSnapshot("Menu").TransitionTo(this.m_snapshotTransitionTime);
			return;
		case AudioMan.Snapshot.Indoor:
			this.m_masterMixer.FindSnapshot("Indoor").TransitionTo(this.m_snapshotTransitionTime);
			return;
		default:
			return;
		}
	}

	// Token: 0x0600118D RID: 4493 RVA: 0x00070E70 File Offset: 0x0006F070
	public void StopAmbientLoop()
	{
		this.m_queuedAmbientLoop = null;
		this.m_stopAmbientLoop = true;
	}

	// Token: 0x0600118E RID: 4494 RVA: 0x00070E80 File Offset: 0x0006F080
	public void QueueAmbientLoop(AudioClip clip, float vol)
	{
		if (this.m_queuedAmbientLoop == clip && this.m_queuedAmbientVol == vol)
		{
			return;
		}
		if (this.m_queuedAmbientLoop == null && this.m_ambientLoopSource.clip == clip && this.m_ambientVol == vol)
		{
			return;
		}
		this.m_queuedAmbientLoop = clip;
		this.m_queuedAmbientVol = vol;
		this.m_stopAmbientLoop = false;
	}

	// Token: 0x0600118F RID: 4495 RVA: 0x00070EE8 File Offset: 0x0006F0E8
	private void UpdateWindAmbience(float dt)
	{
		if (ZoneSystem.instance == null)
		{
			this.m_windLoopSource.volume = 0f;
			return;
		}
		float num = EnvMan.instance.GetWindIntensity();
		num = Mathf.Pow(num, this.m_windIntensityPower);
		num += num * Mathf.Sin(Time.time) * Mathf.Sin(Time.time * 1.54323f) * Mathf.Sin(Time.time * 2.31237f) * this.m_windVariation;
		this.m_windLoopSource.volume = Mathf.Lerp(this.m_windMinVol, this.m_windMaxVol, num);
		this.m_windLoopSource.pitch = Mathf.Lerp(this.m_windMinPitch, this.m_windMaxPitch, num);
	}

	// Token: 0x06001190 RID: 4496 RVA: 0x00070FA0 File Offset: 0x0006F1A0
	private void UpdateOceanAmbiance(float dt)
	{
		if (ZoneSystem.instance == null)
		{
			this.m_oceanAmbientSource.volume = 0f;
			return;
		}
		this.m_oceanUpdateTimer += dt;
		if (this.m_oceanUpdateTimer > 2f)
		{
			this.m_oceanUpdateTimer = 0f;
			this.m_haveOcean = this.FindAverageOceanPoint(out this.m_avgOceanPoint);
		}
		if (this.m_haveOcean)
		{
			float windIntensity = EnvMan.instance.GetWindIntensity();
			float target = Mathf.Lerp(this.m_oceanVolumeMin, this.m_oceanVolumeMax, windIntensity);
			this.m_oceanAmbientSource.volume = Mathf.MoveTowards(this.m_oceanAmbientSource.volume, target, this.m_oceanFadeSpeed * dt);
			this.m_oceanAmbientSource.transform.position = Vector3.Lerp(this.m_oceanAmbientSource.transform.position, this.m_avgOceanPoint, this.m_oceanMoveSpeed);
			return;
		}
		this.m_oceanAmbientSource.volume = Mathf.MoveTowards(this.m_oceanAmbientSource.volume, 0f, this.m_oceanFadeSpeed * dt);
	}

	// Token: 0x06001191 RID: 4497 RVA: 0x000710A8 File Offset: 0x0006F2A8
	private bool FindAverageOceanPoint(out Vector3 point)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			point = Vector3.zero;
			return false;
		}
		Vector3 vector = Vector3.zero;
		int num = 0;
		Vector3 position = mainCamera.transform.position;
		Vector2i zone = ZoneSystem.instance.GetZone(position);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Vector2i id = zone;
				id.x += j;
				id.y += i;
				Vector3 zonePos = ZoneSystem.instance.GetZonePos(id);
				if (this.IsOceanZone(zonePos))
				{
					num++;
					vector += zonePos;
				}
			}
		}
		if (num > 0)
		{
			vector /= (float)num;
			point = vector;
			point.y = ZoneSystem.instance.m_waterLevel;
			return true;
		}
		point = Vector3.zero;
		return false;
	}

	// Token: 0x06001192 RID: 4498 RVA: 0x00071188 File Offset: 0x0006F388
	private bool IsOceanZone(Vector3 centerPos)
	{
		float groundHeight = ZoneSystem.instance.GetGroundHeight(centerPos);
		return ZoneSystem.instance.m_waterLevel - groundHeight > this.m_oceanDepthTreshold;
	}

	// Token: 0x06001193 RID: 4499 RVA: 0x000711B8 File Offset: 0x0006F3B8
	private AudioMan.BiomeAmbients GetAmbients(string name)
	{
		foreach (AudioMan.BiomeAmbients biomeAmbients in this.m_randomAmbients)
		{
			if (biomeAmbients.m_name == name)
			{
				return biomeAmbients;
			}
		}
		return null;
	}

	// Token: 0x06001194 RID: 4500 RVA: 0x0007121C File Offset: 0x0006F41C
	private AudioMan.BiomeAmbients GetBiomeAmbients(Heightmap.Biome biome)
	{
		foreach (AudioMan.BiomeAmbients biomeAmbients in this.m_randomAmbients)
		{
			if ((biomeAmbients.m_biome & biome) != Heightmap.Biome.None)
			{
				return biomeAmbients;
			}
		}
		return null;
	}

	// Token: 0x04001212 RID: 4626
	private static AudioMan m_instance;

	// Token: 0x04001213 RID: 4627
	[Header("Mixers")]
	public AudioMixerGroup m_ambientMixer;

	// Token: 0x04001214 RID: 4628
	public AudioMixer m_masterMixer;

	// Token: 0x04001215 RID: 4629
	public float m_snapshotTransitionTime = 2f;

	// Token: 0x04001216 RID: 4630
	[Header("Wind")]
	public AudioClip m_windAudio;

	// Token: 0x04001217 RID: 4631
	public float m_windMinVol;

	// Token: 0x04001218 RID: 4632
	public float m_windMaxVol = 1f;

	// Token: 0x04001219 RID: 4633
	public float m_windMinPitch = 0.5f;

	// Token: 0x0400121A RID: 4634
	public float m_windMaxPitch = 1.5f;

	// Token: 0x0400121B RID: 4635
	public float m_windVariation = 0.2f;

	// Token: 0x0400121C RID: 4636
	public float m_windIntensityPower = 1.5f;

	// Token: 0x0400121D RID: 4637
	[Header("Ocean")]
	public AudioClip m_oceanAudio;

	// Token: 0x0400121E RID: 4638
	public float m_oceanVolumeMax = 1f;

	// Token: 0x0400121F RID: 4639
	public float m_oceanVolumeMin = 1f;

	// Token: 0x04001220 RID: 4640
	public float m_oceanFadeSpeed = 0.1f;

	// Token: 0x04001221 RID: 4641
	public float m_oceanMoveSpeed = 0.1f;

	// Token: 0x04001222 RID: 4642
	public float m_oceanDepthTreshold = 10f;

	// Token: 0x04001223 RID: 4643
	[Header("Random ambients")]
	public float m_ambientFadeTime = 2f;

	// Token: 0x04001224 RID: 4644
	public float m_randomAmbientInterval = 5f;

	// Token: 0x04001225 RID: 4645
	public float m_randomAmbientChance = 0.5f;

	// Token: 0x04001226 RID: 4646
	public float m_randomMinDistance = 5f;

	// Token: 0x04001227 RID: 4647
	public float m_randomMaxDistance = 20f;

	// Token: 0x04001228 RID: 4648
	public List<AudioMan.BiomeAmbients> m_randomAmbients = new List<AudioMan.BiomeAmbients>();

	// Token: 0x04001229 RID: 4649
	public GameObject m_randomAmbientPrefab;

	// Token: 0x0400122A RID: 4650
	private AudioSource m_oceanAmbientSource;

	// Token: 0x0400122B RID: 4651
	private AudioSource m_ambientLoopSource;

	// Token: 0x0400122C RID: 4652
	private AudioSource m_windLoopSource;

	// Token: 0x0400122D RID: 4653
	private AudioClip m_queuedAmbientLoop;

	// Token: 0x0400122E RID: 4654
	private float m_queuedAmbientVol;

	// Token: 0x0400122F RID: 4655
	private float m_ambientVol;

	// Token: 0x04001230 RID: 4656
	private float m_randomAmbientTimer;

	// Token: 0x04001231 RID: 4657
	private bool m_stopAmbientLoop;

	// Token: 0x04001232 RID: 4658
	private bool m_indoor;

	// Token: 0x04001233 RID: 4659
	private float m_oceanUpdateTimer;

	// Token: 0x04001234 RID: 4660
	private bool m_haveOcean;

	// Token: 0x04001235 RID: 4661
	private Vector3 m_avgOceanPoint = Vector3.zero;

	// Token: 0x04001236 RID: 4662
	private AudioMan.Snapshot m_currentSnapshot;

	// Token: 0x020001B4 RID: 436
	[Serializable]
	public class BiomeAmbients
	{
		// Token: 0x04001237 RID: 4663
		public string m_name = "";

		// Token: 0x04001238 RID: 4664
		public float m_forceFadeout = 3f;

		// Token: 0x04001239 RID: 4665
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x0400123A RID: 4666
		public List<AudioClip> m_randomAmbientClips = new List<AudioClip>();

		// Token: 0x0400123B RID: 4667
		public List<AudioClip> m_randomAmbientClipsDay = new List<AudioClip>();

		// Token: 0x0400123C RID: 4668
		public List<AudioClip> m_randomAmbientClipsNight = new List<AudioClip>();
	}

	// Token: 0x020001B5 RID: 437
	private enum Snapshot
	{
		// Token: 0x0400123E RID: 4670
		Default,
		// Token: 0x0400123F RID: 4671
		Menu,
		// Token: 0x04001240 RID: 4672
		Indoor
	}
}
