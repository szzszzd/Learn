using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Token: 0x020001D2 RID: 466
public class MusicMan : MonoBehaviour
{
	// Token: 0x170000C6 RID: 198
	// (get) Token: 0x0600130D RID: 4877 RVA: 0x0007D88C File Offset: 0x0007BA8C
	public static MusicMan instance
	{
		get
		{
			return MusicMan.m_instance;
		}
	}

	// Token: 0x0600130E RID: 4878 RVA: 0x0007D894 File Offset: 0x0007BA94
	private void Awake()
	{
		if (MusicMan.m_instance)
		{
			return;
		}
		MusicMan.m_instance = this;
		GameObject gameObject = new GameObject("music");
		gameObject.transform.SetParent(base.transform);
		this.m_musicSource = gameObject.AddComponent<AudioSource>();
		this.m_musicSource.loop = true;
		this.m_musicSource.spatialBlend = 0f;
		this.m_musicSource.outputAudioMixerGroup = this.m_musicMixer;
		this.m_musicSource.priority = 0;
		this.m_musicSource.bypassReverbZones = true;
		this.m_randomAmbientInterval = UnityEngine.Random.Range(this.m_randomMusicIntervalMin, this.m_randomMusicIntervalMax);
		MusicMan.m_masterMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
		this.ApplySettings();
		foreach (MusicMan.NamedMusic namedMusic in this.m_music)
		{
			foreach (AudioClip audioClip in namedMusic.m_clips)
			{
				if (audioClip == null || !audioClip)
				{
					namedMusic.m_enabled = false;
					ZLog.LogWarning("Missing audio clip in music " + namedMusic.m_name);
					break;
				}
			}
		}
	}

	// Token: 0x0600130F RID: 4879 RVA: 0x0007D9E4 File Offset: 0x0007BBE4
	public void ApplySettings()
	{
		foreach (MusicMan.NamedMusic namedMusic in this.m_music)
		{
			if (namedMusic.m_ambientMusic)
			{
				namedMusic.m_loop = Settings.ContinousMusic;
				if (!Settings.ContinousMusic && this.GetCurrentMusic() == namedMusic.m_name && this.m_musicSource.loop)
				{
					ZLog.Log("Stopping looping music because continous music is disabled");
					this.StopMusic();
				}
			}
		}
	}

	// Token: 0x06001310 RID: 4880 RVA: 0x0007DA7C File Offset: 0x0007BC7C
	private void OnDestroy()
	{
		if (MusicMan.m_instance == this)
		{
			MusicMan.m_instance = null;
		}
	}

	// Token: 0x06001311 RID: 4881 RVA: 0x0007DA94 File Offset: 0x0007BC94
	private void Update()
	{
		if (MusicMan.m_instance != this)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		this.UpdateCurrentMusic(deltaTime);
		this.UpdateCombatMusic(deltaTime);
		this.m_currentMusicVolMax = MusicVolume.UpdateProximityVolumes(this.m_musicSource);
		this.UpdateMusic(deltaTime);
	}

	// Token: 0x06001312 RID: 4882 RVA: 0x0007DADC File Offset: 0x0007BCDC
	private void UpdateCurrentMusic(float dt)
	{
		string currentMusic = this.GetCurrentMusic();
		if (Game.instance != null)
		{
			if (Player.m_localPlayer == null)
			{
				this.StartMusic("respawn");
				return;
			}
			if (currentMusic == "respawn")
			{
				this.StopMusic();
			}
		}
		if (Player.m_localPlayer && Player.m_localPlayer.InIntro())
		{
			this.StartMusic("intro");
			return;
		}
		if (currentMusic == "intro")
		{
			this.StopMusic();
		}
		if (this.HandleEventMusic(currentMusic))
		{
			return;
		}
		if (this.HandleLocationMusic(currentMusic))
		{
			return;
		}
		if (this.HandleSailingMusic(dt, currentMusic))
		{
			return;
		}
		if (this.HandleTriggerMusic(currentMusic))
		{
			return;
		}
		this.HandleEnvironmentMusic(dt, currentMusic);
	}

	// Token: 0x06001313 RID: 4883 RVA: 0x0007DB94 File Offset: 0x0007BD94
	private bool HandleEnvironmentMusic(float dt, string currentMusic)
	{
		if (!EnvMan.instance)
		{
			return false;
		}
		MusicMan.NamedMusic environmentMusic = this.GetEnvironmentMusic();
		string currentMusic2 = this.GetCurrentMusic();
		if (environmentMusic == null || (this.m_currentMusic != null && environmentMusic.m_name != currentMusic2))
		{
			this.StopMusic();
			return true;
		}
		if (environmentMusic.m_name == currentMusic2)
		{
			return true;
		}
		if (!environmentMusic.m_loop)
		{
			if (Time.time - this.m_lastAmbientMusicTime < this.m_randomAmbientInterval)
			{
				return false;
			}
			this.m_randomAmbientInterval = UnityEngine.Random.Range(this.m_randomMusicIntervalMin, this.m_randomMusicIntervalMax);
			this.m_lastAmbientMusicTime = Time.time;
			ZLog.Log("Environment music starting at random ambient interval");
		}
		this.StartMusic(environmentMusic);
		return true;
	}

	// Token: 0x06001314 RID: 4884 RVA: 0x0007DC44 File Offset: 0x0007BE44
	private MusicMan.NamedMusic GetEnvironmentMusic()
	{
		string name;
		if (Player.m_localPlayer && Player.m_localPlayer.IsSafeInHome())
		{
			name = "home";
		}
		else
		{
			name = EnvMan.instance.GetAmbientMusic();
		}
		return this.FindMusic(name);
	}

	// Token: 0x06001315 RID: 4885 RVA: 0x0007DC88 File Offset: 0x0007BE88
	private bool HandleTriggerMusic(string currentMusic)
	{
		if (this.m_triggerMusic != null)
		{
			this.StartMusic(this.m_triggerMusic);
			this.m_triggeredMusic = this.m_triggerMusic;
			this.m_triggerMusic = null;
			return true;
		}
		if (this.m_triggeredMusic != null)
		{
			if (currentMusic == this.m_triggeredMusic)
			{
				return true;
			}
			this.m_triggeredMusic = null;
		}
		return false;
	}

	// Token: 0x06001316 RID: 4886 RVA: 0x0007DCDF File Offset: 0x0007BEDF
	public void LocationMusic(string name)
	{
		this.m_locationMusic = name;
	}

	// Token: 0x06001317 RID: 4887 RVA: 0x0007DCE8 File Offset: 0x0007BEE8
	private bool HandleLocationMusic(string currentMusic)
	{
		if (this.m_lastLocationMusic != null && DateTime.Now > this.m_lastLocationMusicChange + TimeSpan.FromSeconds((double)this.m_repeatLocationMusicResetSeconds))
		{
			this.m_lastLocationMusic = null;
			this.m_lastLocationMusicChange = DateTime.Now;
		}
		if (this.m_locationMusic == null)
		{
			return false;
		}
		if (currentMusic == this.m_locationMusic && !this.m_musicSource.isPlaying)
		{
			this.m_locationMusic = null;
			return false;
		}
		if (currentMusic != this.m_locationMusic)
		{
			this.m_lastLocationMusicChange = DateTime.Now;
		}
		if (this.StartMusic(this.m_locationMusic))
		{
			this.m_lastLocationMusic = this.m_locationMusic;
		}
		else
		{
			ZLog.Log("Location music missing: " + this.m_locationMusic);
			this.m_locationMusic = null;
		}
		return true;
	}

	// Token: 0x06001318 RID: 4888 RVA: 0x0007DDB4 File Offset: 0x0007BFB4
	private bool HandleEventMusic(string currentMusic)
	{
		if (RandEventSystem.instance)
		{
			string musicOverride = RandEventSystem.instance.GetMusicOverride();
			if (musicOverride != null)
			{
				this.StartMusic(musicOverride);
				this.m_randomEventMusic = musicOverride;
				return true;
			}
			if (currentMusic == this.m_randomEventMusic)
			{
				this.m_randomEventMusic = null;
				this.StopMusic();
			}
		}
		return false;
	}

	// Token: 0x06001319 RID: 4889 RVA: 0x0007DE08 File Offset: 0x0007C008
	private bool HandleCombatMusic(string currentMusic)
	{
		if (this.InCombat())
		{
			this.StartMusic("combat");
			return true;
		}
		if (currentMusic == "combat")
		{
			this.StopMusic();
		}
		return false;
	}

	// Token: 0x0600131A RID: 4890 RVA: 0x0007DE34 File Offset: 0x0007C034
	private bool HandleSailingMusic(float dt, string currentMusic)
	{
		if (this.IsSailing())
		{
			this.m_notSailDuration = 0f;
			this.m_sailDuration += dt;
			if (this.m_sailDuration > this.m_sailMusicMinSailTime)
			{
				this.StartMusic("sailing");
				return true;
			}
		}
		else
		{
			this.m_sailDuration = 0f;
			this.m_notSailDuration += dt;
			if (this.m_notSailDuration > this.m_sailMusicMinSailTime / 2f && currentMusic == "sailing")
			{
				this.StopMusic();
			}
		}
		return false;
	}

	// Token: 0x0600131B RID: 4891 RVA: 0x0007DEC0 File Offset: 0x0007C0C0
	private bool IsSailing()
	{
		if (!Player.m_localPlayer)
		{
			return false;
		}
		Ship localShip = Ship.GetLocalShip();
		return localShip && localShip.GetSpeed() > this.m_sailMusicShipSpeedThreshold;
	}

	// Token: 0x0600131C RID: 4892 RVA: 0x0007DEFC File Offset: 0x0007C0FC
	private void UpdateMusic(float dt)
	{
		if (this.m_queuedMusic != null || this.m_stopMusic)
		{
			if (!this.m_musicSource.isPlaying || this.m_currentMusicVol <= 0f)
			{
				if (this.m_musicSource.isPlaying && this.m_currentMusic != null && this.m_currentMusic.m_loop && this.m_currentMusic.m_resume)
				{
					this.m_currentMusic.m_lastPlayedTime = Time.time;
					this.m_currentMusic.m_savedPlaybackPos = this.m_musicSource.timeSamples;
					ZLog.Log("Stopped music " + this.m_currentMusic.m_name + " at " + this.m_currentMusic.m_savedPlaybackPos.ToString());
				}
				this.m_musicSource.Stop();
				this.m_stopMusic = false;
				this.m_currentMusic = null;
				if (this.m_queuedMusic != null)
				{
					this.m_musicSource.clip = this.m_queuedMusic.m_clips[UnityEngine.Random.Range(0, this.m_queuedMusic.m_clips.Length)];
					this.m_musicSource.loop = this.m_queuedMusic.m_loop;
					this.m_musicSource.volume = 0f;
					this.m_musicSource.timeSamples = 0;
					this.m_musicSource.Play();
					if (this.m_queuedMusic.m_loop && this.m_queuedMusic.m_resume && Time.time - this.m_queuedMusic.m_lastPlayedTime < this.m_musicSource.clip.length * 2f)
					{
						this.m_musicSource.timeSamples = this.m_queuedMusic.m_savedPlaybackPos;
						ZLog.Log("Resumed music " + this.m_queuedMusic.m_name + " at " + this.m_queuedMusic.m_savedPlaybackPos.ToString());
					}
					this.m_currentMusicVol = 0f;
					this.m_musicVolume = this.m_queuedMusic.m_volume;
					this.m_musicFadeTime = this.m_queuedMusic.m_fadeInTime;
					this.m_alwaysFadeout = this.m_queuedMusic.m_alwaysFadeout;
					this.m_currentMusic = this.m_queuedMusic;
					this.m_queuedMusic = null;
				}
			}
			else
			{
				float num = (this.m_queuedMusic != null) ? Mathf.Min(this.m_queuedMusic.m_fadeInTime, this.m_musicFadeTime) : this.m_musicFadeTime;
				this.m_currentMusicVol = Mathf.MoveTowards(this.m_currentMusicVol, 0f, dt / num);
				this.m_musicSource.volume = Utils.SmoothStep(0f, 1f, this.m_currentMusicVol) * this.m_musicVolume * MusicMan.m_masterMusicVolume;
			}
		}
		else if (this.m_musicSource.isPlaying)
		{
			float num2 = this.m_musicSource.clip.length - this.m_musicSource.time;
			if (this.m_alwaysFadeout && !this.m_musicSource.loop && num2 < this.m_musicFadeTime)
			{
				this.m_currentMusicVol = Mathf.MoveTowards(this.m_currentMusicVol, 0f, dt / this.m_musicFadeTime);
				this.m_musicSource.volume = Utils.SmoothStep(0f, 1f, this.m_currentMusicVol) * this.m_musicVolume * MusicMan.m_masterMusicVolume;
			}
			else
			{
				this.m_currentMusicVol = Mathf.MoveTowards(this.m_currentMusicVol, this.m_currentMusicVolMax, dt / this.m_musicFadeTime);
				this.m_musicSource.volume = Utils.SmoothStep(0f, 1f, this.m_currentMusicVol) * this.m_musicVolume * MusicMan.m_masterMusicVolume;
			}
			if (!Settings.ContinousMusic && num2 < this.m_musicFadeTime)
			{
				this.StopMusic();
				ZLog.Log("Music stopped after finishing, because continous music is disabled");
			}
		}
		else if (this.m_currentMusic != null && !this.m_musicSource.isPlaying)
		{
			this.m_currentMusic = null;
		}
		if (this.m_resetMusicTimer > 0f)
		{
			this.m_resetMusicTimer -= dt;
		}
		if (Terminal.m_showTests)
		{
			Terminal.m_testList["Music current"] = ((this.m_currentMusic == null) ? "NULL" : this.m_currentMusic.m_name);
			Terminal.m_testList["Music last started"] = ((this.m_lastStartedMusic == null) ? "NULL" : this.m_lastStartedMusic.m_name);
			Terminal.m_testList["Music queued"] = ((this.m_queuedMusic == null) ? "NULL" : this.m_queuedMusic.m_name);
			Terminal.m_testList["Music stopping"] = this.m_stopMusic.ToString();
			Terminal.m_testList["Music reset non continous"] = string.Format("{0} / {1}", this.m_resetMusicTimer, this.m_musicResetNonContinous);
			if (Input.GetKeyDown(KeyCode.N) && Input.GetKey(KeyCode.LeftShift) && this.m_musicSource != null && this.m_musicSource.isPlaying)
			{
				this.m_musicSource.time = this.m_musicSource.clip.length - 4f;
			}
		}
	}

	// Token: 0x0600131D RID: 4893 RVA: 0x0007E3F4 File Offset: 0x0007C5F4
	private void UpdateCombatMusic(float dt)
	{
		if (this.m_combatTimer > 0f)
		{
			this.m_combatTimer -= Time.deltaTime;
		}
	}

	// Token: 0x0600131E RID: 4894 RVA: 0x0007E415 File Offset: 0x0007C615
	public void ResetCombatTimer()
	{
		this.m_combatTimer = this.m_combatMusicTimeout;
	}

	// Token: 0x0600131F RID: 4895 RVA: 0x0007E423 File Offset: 0x0007C623
	private bool InCombat()
	{
		return this.m_combatTimer > 0f;
	}

	// Token: 0x06001320 RID: 4896 RVA: 0x0007E432 File Offset: 0x0007C632
	public void TriggerMusic(string name)
	{
		this.m_triggerMusic = name;
	}

	// Token: 0x06001321 RID: 4897 RVA: 0x0007E43C File Offset: 0x0007C63C
	private bool StartMusic(string name)
	{
		if (this.GetCurrentMusic() == name)
		{
			return true;
		}
		MusicMan.NamedMusic music = this.FindMusic(name);
		return this.StartMusic(music);
	}

	// Token: 0x06001322 RID: 4898 RVA: 0x0007E468 File Offset: 0x0007C668
	private bool StartMusic(MusicMan.NamedMusic music)
	{
		if (music != null && this.GetCurrentMusic() == music.m_name)
		{
			return true;
		}
		if (music == this.m_lastStartedMusic && !Settings.ContinousMusic && this.m_resetMusicTimer > 0f)
		{
			return false;
		}
		this.m_lastStartedMusic = music;
		this.m_resetMusicTimer = this.m_musicResetNonContinous + ((music != null && music.m_clips.Length != 0) ? music.m_clips[0].length : 0f);
		if (music != null)
		{
			this.m_queuedMusic = music;
			this.m_stopMusic = false;
			ZLog.Log("Starting music " + music.m_name);
			return true;
		}
		this.StopMusic();
		return false;
	}

	// Token: 0x06001323 RID: 4899 RVA: 0x0007E510 File Offset: 0x0007C710
	private MusicMan.NamedMusic FindMusic(string name)
	{
		if (name == null || name.Length == 0)
		{
			return null;
		}
		foreach (MusicMan.NamedMusic namedMusic in this.m_music)
		{
			if (namedMusic.m_name == name && namedMusic.m_enabled && namedMusic.m_clips.Length != 0 && namedMusic.m_clips[0])
			{
				return namedMusic;
			}
		}
		return null;
	}

	// Token: 0x06001324 RID: 4900 RVA: 0x0007E5A0 File Offset: 0x0007C7A0
	public bool IsPlaying()
	{
		return this.m_musicSource.isPlaying;
	}

	// Token: 0x06001325 RID: 4901 RVA: 0x0007E5AD File Offset: 0x0007C7AD
	private string GetCurrentMusic()
	{
		if (this.m_stopMusic)
		{
			return "";
		}
		if (this.m_queuedMusic != null)
		{
			return this.m_queuedMusic.m_name;
		}
		if (this.m_currentMusic != null)
		{
			return this.m_currentMusic.m_name;
		}
		return "";
	}

	// Token: 0x06001326 RID: 4902 RVA: 0x0007E5EA File Offset: 0x0007C7EA
	private void StopMusic()
	{
		this.m_queuedMusic = null;
		this.m_stopMusic = true;
	}

	// Token: 0x06001327 RID: 4903 RVA: 0x0007E5FA File Offset: 0x0007C7FA
	public void Reset()
	{
		this.StopMusic();
		this.m_combatTimer = 0f;
		this.m_randomEventMusic = null;
		this.m_triggerMusic = null;
		this.m_locationMusic = null;
	}

	// Token: 0x040013EB RID: 5099
	private string m_triggeredMusic = "";

	// Token: 0x040013EC RID: 5100
	private static MusicMan m_instance;

	// Token: 0x040013ED RID: 5101
	public static float m_masterMusicVolume = 1f;

	// Token: 0x040013EE RID: 5102
	public AudioMixerGroup m_musicMixer;

	// Token: 0x040013EF RID: 5103
	public List<MusicMan.NamedMusic> m_music = new List<MusicMan.NamedMusic>();

	// Token: 0x040013F0 RID: 5104
	public float m_musicResetNonContinous = 120f;

	// Token: 0x040013F1 RID: 5105
	[Header("Combat")]
	public float m_combatMusicTimeout = 4f;

	// Token: 0x040013F2 RID: 5106
	[Header("Sailing")]
	public float m_sailMusicShipSpeedThreshold = 3f;

	// Token: 0x040013F3 RID: 5107
	public float m_sailMusicMinSailTime = 20f;

	// Token: 0x040013F4 RID: 5108
	[Header("Ambient music")]
	public float m_randomMusicIntervalMin = 300f;

	// Token: 0x040013F5 RID: 5109
	public float m_randomMusicIntervalMax = 500f;

	// Token: 0x040013F6 RID: 5110
	private MusicMan.NamedMusic m_queuedMusic;

	// Token: 0x040013F7 RID: 5111
	private MusicMan.NamedMusic m_currentMusic;

	// Token: 0x040013F8 RID: 5112
	private MusicMan.NamedMusic m_lastStartedMusic;

	// Token: 0x040013F9 RID: 5113
	private float m_musicVolume = 1f;

	// Token: 0x040013FA RID: 5114
	private float m_musicFadeTime = 3f;

	// Token: 0x040013FB RID: 5115
	private bool m_alwaysFadeout;

	// Token: 0x040013FC RID: 5116
	private bool m_stopMusic;

	// Token: 0x040013FD RID: 5117
	private string m_randomEventMusic;

	// Token: 0x040013FE RID: 5118
	private float m_lastAmbientMusicTime;

	// Token: 0x040013FF RID: 5119
	private float m_randomAmbientInterval;

	// Token: 0x04001400 RID: 5120
	private string m_triggerMusic;

	// Token: 0x04001401 RID: 5121
	private string m_locationMusic;

	// Token: 0x04001402 RID: 5122
	public string m_lastLocationMusic;

	// Token: 0x04001403 RID: 5123
	private DateTime m_lastLocationMusicChange;

	// Token: 0x04001404 RID: 5124
	public int m_repeatLocationMusicResetSeconds = 300;

	// Token: 0x04001405 RID: 5125
	private float m_combatTimer;

	// Token: 0x04001406 RID: 5126
	private float m_resetMusicTimer;

	// Token: 0x04001407 RID: 5127
	private AudioSource m_musicSource;

	// Token: 0x04001408 RID: 5128
	private float m_currentMusicVol;

	// Token: 0x04001409 RID: 5129
	public float m_currentMusicVolMax = 1f;

	// Token: 0x0400140A RID: 5130
	private float m_sailDuration;

	// Token: 0x0400140B RID: 5131
	private float m_notSailDuration;

	// Token: 0x020001D3 RID: 467
	[Serializable]
	public class NamedMusic
	{
		// Token: 0x0400140C RID: 5132
		public string m_name = "";

		// Token: 0x0400140D RID: 5133
		public AudioClip[] m_clips;

		// Token: 0x0400140E RID: 5134
		public float m_volume = 1f;

		// Token: 0x0400140F RID: 5135
		public float m_fadeInTime = 3f;

		// Token: 0x04001410 RID: 5136
		public bool m_alwaysFadeout;

		// Token: 0x04001411 RID: 5137
		public bool m_loop;

		// Token: 0x04001412 RID: 5138
		public bool m_resume;

		// Token: 0x04001413 RID: 5139
		public bool m_enabled = true;

		// Token: 0x04001414 RID: 5140
		public bool m_ambientMusic;

		// Token: 0x04001415 RID: 5141
		[NonSerialized]
		public int m_savedPlaybackPos;

		// Token: 0x04001416 RID: 5142
		[NonSerialized]
		public float m_lastPlayedTime;
	}
}
