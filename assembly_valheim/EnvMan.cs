using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

// Token: 0x020001BF RID: 447
public class EnvMan : MonoBehaviour
{
	// Token: 0x170000BB RID: 187
	// (get) Token: 0x060011B5 RID: 4533 RVA: 0x00074BF6 File Offset: 0x00072DF6
	public static EnvMan instance
	{
		get
		{
			return EnvMan.s_instance;
		}
	}

	// Token: 0x060011B6 RID: 4534 RVA: 0x00074C00 File Offset: 0x00072E00
	private void Awake()
	{
		EnvMan.s_instance = this;
		foreach (EnvSetup env in this.m_environments)
		{
			this.InitializeEnvironment(env);
		}
		foreach (BiomeEnvSetup biome in this.m_biomes)
		{
			this.InitializeBiomeEnvSetup(biome);
		}
		this.m_currentEnv = this.GetDefaultEnv();
	}

	// Token: 0x060011B7 RID: 4535 RVA: 0x00074CA8 File Offset: 0x00072EA8
	private void OnDestroy()
	{
		EnvMan.s_instance = null;
	}

	// Token: 0x060011B8 RID: 4536 RVA: 0x00074CB0 File Offset: 0x00072EB0
	private void InitializeEnvironment(EnvSetup env)
	{
		this.SetParticleArrayEnabled(env.m_psystems, false);
		if (env.m_envObject)
		{
			env.m_envObject.SetActive(false);
		}
	}

	// Token: 0x060011B9 RID: 4537 RVA: 0x00074CD8 File Offset: 0x00072ED8
	private void InitializeBiomeEnvSetup(BiomeEnvSetup biome)
	{
		foreach (EnvEntry envEntry in biome.m_environments)
		{
			envEntry.m_env = this.GetEnv(envEntry.m_environment);
		}
	}

	// Token: 0x060011BA RID: 4538 RVA: 0x00074D38 File Offset: 0x00072F38
	private void SetParticleArrayEnabled(GameObject[] psystems, bool enabled)
	{
		foreach (GameObject gameObject in psystems)
		{
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].emission.enabled = enabled;
			}
			MistEmitter componentInChildren = gameObject.GetComponentInChildren<MistEmitter>();
			if (componentInChildren)
			{
				componentInChildren.enabled = enabled;
			}
		}
	}

	// Token: 0x060011BB RID: 4539 RVA: 0x00074DA0 File Offset: 0x00072FA0
	private float RescaleDayFraction(float fraction)
	{
		if (fraction >= 0.15f && fraction <= 0.85f)
		{
			float num = (fraction - 0.15f) / 0.7f;
			fraction = 0.25f + num * 0.5f;
		}
		else if (fraction < 0.5f)
		{
			fraction = fraction / 0.15f * 0.25f;
		}
		else
		{
			float num2 = (fraction - 0.85f) / 0.15f;
			fraction = 0.75f + num2 * 0.25f;
		}
		return fraction;
	}

	// Token: 0x060011BC RID: 4540 RVA: 0x00074E14 File Offset: 0x00073014
	private void Update()
	{
		Vector3 windForce = EnvMan.instance.GetWindForce();
		this.m_cloudOffset += windForce * Time.deltaTime * 0.01f;
		Shader.SetGlobalVector(EnvMan.s_cloudOffset, this.m_cloudOffset);
		Shader.SetGlobalVector(EnvMan.s_netRefPos, ZNet.instance.GetReferencePosition());
	}

	// Token: 0x060011BD RID: 4541 RVA: 0x00074E80 File Offset: 0x00073080
	private void FixedUpdate()
	{
		this.UpdateTimeSkip(Time.fixedDeltaTime);
		this.m_totalSeconds = ZNet.instance.GetTimeSeconds();
		long num = (long)this.m_totalSeconds;
		double num2 = this.m_totalSeconds * 1000.0;
		long num3 = this.m_dayLengthSec * 1000L;
		float num4 = Mathf.Clamp01((float)(num2 % (double)num3 / 1000.0) / (float)this.m_dayLengthSec);
		num4 = this.RescaleDayFraction(num4);
		float smoothDayFraction = this.m_smoothDayFraction;
		float t = Mathf.LerpAngle(this.m_smoothDayFraction * 360f, num4 * 360f, 0.01f);
		this.m_smoothDayFraction = Mathf.Repeat(t, 360f) / 360f;
		if (this.m_debugTimeOfDay)
		{
			this.m_smoothDayFraction = this.m_debugTime;
		}
		float num5 = Mathf.Pow(Mathf.Max(1f - Mathf.Clamp01(this.m_smoothDayFraction / 0.25f), Mathf.Clamp01((this.m_smoothDayFraction - 0.75f) / 0.25f)), 0.5f);
		float num6 = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(this.m_smoothDayFraction - 0.5f) / 0.25f), 0.5f);
		float num7 = Mathf.Min(Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.26f) / -this.m_sunHorizonTransitionL), Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.26f) / this.m_sunHorizonTransitionH));
		float num8 = Mathf.Min(Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.74f) / -this.m_sunHorizonTransitionH), Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.74f) / this.m_sunHorizonTransitionL));
		float num9 = 1f / (num5 + num6 + num7 + num8);
		num5 *= num9;
		num6 *= num9;
		num7 *= num9;
		num8 *= num9;
		Heightmap.Biome biome = this.GetBiome();
		this.UpdateTriggers(smoothDayFraction, this.m_smoothDayFraction, biome, Time.fixedDeltaTime);
		this.UpdateEnvironment(num, biome);
		this.InterpolateEnvironment(Time.fixedDeltaTime);
		this.UpdateWind(num, Time.fixedDeltaTime);
		if (!string.IsNullOrEmpty(this.m_forceEnv))
		{
			EnvSetup env = this.GetEnv(this.m_forceEnv);
			if (env != null)
			{
				this.SetEnv(env, num6, num5, num7, num8, Time.fixedDeltaTime);
				return;
			}
		}
		else
		{
			this.SetEnv(this.m_currentEnv, num6, num5, num7, num8, Time.fixedDeltaTime);
		}
	}

	// Token: 0x060011BE RID: 4542 RVA: 0x000750F3 File Offset: 0x000732F3
	private int GetCurrentDay()
	{
		return (int)(this.m_totalSeconds / (double)this.m_dayLengthSec);
	}

	// Token: 0x060011BF RID: 4543 RVA: 0x00075104 File Offset: 0x00073304
	private void UpdateTriggers(float oldDayFraction, float newDayFraction, Heightmap.Biome biome, float dt)
	{
		if (Player.m_localPlayer == null || biome == Heightmap.Biome.None)
		{
			return;
		}
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		if (currentEnvironment == null)
		{
			return;
		}
		this.UpdateAmbientMusic(biome, currentEnvironment, dt);
		if (oldDayFraction > 0.2f && oldDayFraction < 0.25f && newDayFraction > 0.25f && newDayFraction < 0.3f)
		{
			this.OnMorning(biome, currentEnvironment);
		}
		if (oldDayFraction > 0.7f && oldDayFraction < 0.75f && newDayFraction > 0.75f && newDayFraction < 0.8f)
		{
			this.OnEvening(biome, currentEnvironment);
		}
	}

	// Token: 0x060011C0 RID: 4544 RVA: 0x00075188 File Offset: 0x00073388
	private void UpdateAmbientMusic(Heightmap.Biome biome, EnvSetup currentEnv, float dt)
	{
		this.m_ambientMusicTimer += dt;
		if (this.m_ambientMusicTimer > 2f)
		{
			this.m_ambientMusicTimer = 0f;
			this.m_ambientMusic = null;
			BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
			if (this.IsDay())
			{
				if (currentEnv.m_musicDay.Length > 0)
				{
					this.m_ambientMusic = currentEnv.m_musicDay;
					return;
				}
				if (biomeEnvSetup.m_musicDay.Length > 0)
				{
					this.m_ambientMusic = biomeEnvSetup.m_musicDay;
					return;
				}
			}
			else
			{
				if (currentEnv.m_musicNight.Length > 0)
				{
					this.m_ambientMusic = currentEnv.m_musicNight;
					return;
				}
				if (biomeEnvSetup.m_musicNight.Length > 0)
				{
					this.m_ambientMusic = biomeEnvSetup.m_musicNight;
				}
			}
		}
	}

	// Token: 0x060011C1 RID: 4545 RVA: 0x00075240 File Offset: 0x00073440
	public string GetAmbientMusic()
	{
		return this.m_ambientMusic;
	}

	// Token: 0x060011C2 RID: 4546 RVA: 0x00075248 File Offset: 0x00073448
	private void OnMorning(Heightmap.Biome biome, EnvSetup currentEnv)
	{
		string name = "morning";
		if (currentEnv.m_musicMorning.Length > 0)
		{
			name = currentEnv.m_musicMorning;
		}
		else
		{
			BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
			if (biomeEnvSetup.m_musicMorning.Length > 0)
			{
				name = biomeEnvSetup.m_musicMorning;
			}
		}
		MusicMan.instance.TriggerMusic(name);
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_newday", new string[]
		{
			this.GetCurrentDay().ToString()
		}), 0, null);
	}

	// Token: 0x060011C3 RID: 4547 RVA: 0x000752D0 File Offset: 0x000734D0
	private void OnEvening(Heightmap.Biome biome, EnvSetup currentEnv)
	{
		string name = "evening";
		if (currentEnv.m_musicEvening.Length > 0)
		{
			name = currentEnv.m_musicEvening;
		}
		else
		{
			BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
			if (biomeEnvSetup.m_musicEvening.Length > 0)
			{
				name = biomeEnvSetup.m_musicEvening;
			}
		}
		MusicMan.instance.TriggerMusic(name);
	}

	// Token: 0x060011C4 RID: 4548 RVA: 0x00075324 File Offset: 0x00073524
	public void SetForceEnvironment(string env)
	{
		if (this.m_forceEnv == env)
		{
			return;
		}
		ZLog.Log("Setting forced environment " + env);
		this.m_forceEnv = env;
		this.FixedUpdate();
		if (ReflectionUpdate.instance)
		{
			ReflectionUpdate.instance.UpdateReflection();
		}
	}

	// Token: 0x060011C5 RID: 4549 RVA: 0x00075374 File Offset: 0x00073574
	private EnvSetup SelectWeightedEnvironment(List<EnvEntry> environments)
	{
		float num = 0f;
		foreach (EnvEntry envEntry in environments)
		{
			num += envEntry.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (EnvEntry envEntry2 in environments)
		{
			num3 += envEntry2.m_weight;
			if (num3 >= num2)
			{
				return envEntry2.m_env;
			}
		}
		return environments[environments.Count - 1].m_env;
	}

	// Token: 0x060011C6 RID: 4550 RVA: 0x00075444 File Offset: 0x00073644
	private string GetEnvironmentOverride()
	{
		if (!string.IsNullOrEmpty(this.m_debugEnv))
		{
			return this.m_debugEnv;
		}
		if (Player.m_localPlayer != null && Player.m_localPlayer.InIntro())
		{
			return this.m_introEnvironment;
		}
		string envOverride = RandEventSystem.instance.GetEnvOverride();
		if (!string.IsNullOrEmpty(envOverride))
		{
			return envOverride;
		}
		string environment = EnvZone.GetEnvironment();
		if (!string.IsNullOrEmpty(environment))
		{
			return environment;
		}
		return null;
	}

	// Token: 0x060011C7 RID: 4551 RVA: 0x000754AC File Offset: 0x000736AC
	private void UpdateEnvironment(long sec, Heightmap.Biome biome)
	{
		string environmentOverride = this.GetEnvironmentOverride();
		if (!string.IsNullOrEmpty(environmentOverride))
		{
			this.m_environmentPeriod = -1L;
			this.m_currentBiome = this.GetBiome();
			this.QueueEnvironment(environmentOverride);
			return;
		}
		long num = sec / this.m_environmentDuration;
		if (this.m_environmentPeriod != num || this.m_currentBiome != biome)
		{
			this.m_environmentPeriod = num;
			this.m_currentBiome = biome;
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)num);
			List<EnvEntry> availableEnvironments = this.GetAvailableEnvironments(biome);
			if (availableEnvironments != null && availableEnvironments.Count > 0)
			{
				EnvSetup env = this.SelectWeightedEnvironment(availableEnvironments);
				this.QueueEnvironment(env);
			}
			UnityEngine.Random.state = state;
		}
	}

	// Token: 0x060011C8 RID: 4552 RVA: 0x00075544 File Offset: 0x00073744
	private BiomeEnvSetup GetBiomeEnvSetup(Heightmap.Biome biome)
	{
		foreach (BiomeEnvSetup biomeEnvSetup in this.m_biomes)
		{
			if (biomeEnvSetup.m_biome == biome)
			{
				return biomeEnvSetup;
			}
		}
		return null;
	}

	// Token: 0x060011C9 RID: 4553 RVA: 0x000755A0 File Offset: 0x000737A0
	private List<EnvEntry> GetAvailableEnvironments(Heightmap.Biome biome)
	{
		BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
		if (biomeEnvSetup != null)
		{
			return biomeEnvSetup.m_environments;
		}
		return null;
	}

	// Token: 0x060011CA RID: 4554 RVA: 0x000755C0 File Offset: 0x000737C0
	private Heightmap.Biome GetBiome()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return Heightmap.Biome.None;
		}
		Vector3 position = mainCamera.transform.position;
		if (this.m_cachedHeightmap == null || !this.m_cachedHeightmap.IsPointInside(position, 0f))
		{
			this.m_cachedHeightmap = Heightmap.FindHeightmap(position);
		}
		if (this.m_cachedHeightmap)
		{
			return this.m_cachedHeightmap.GetBiome(position);
		}
		return Heightmap.Biome.None;
	}

	// Token: 0x060011CB RID: 4555 RVA: 0x00075634 File Offset: 0x00073834
	private void InterpolateEnvironment(float dt)
	{
		if (this.m_nextEnv != null)
		{
			this.m_transitionTimer += dt;
			float num = Mathf.Clamp01(this.m_transitionTimer / this.m_transitionDuration);
			this.m_currentEnv = this.InterpolateEnvironment(this.m_prevEnv, this.m_nextEnv, num);
			if (num >= 1f)
			{
				this.m_currentEnv = this.m_nextEnv;
				this.m_prevEnv = null;
				this.m_nextEnv = null;
			}
		}
	}

	// Token: 0x060011CC RID: 4556 RVA: 0x000756A8 File Offset: 0x000738A8
	private void QueueEnvironment(string name)
	{
		if (this.m_currentEnv.m_name == name)
		{
			return;
		}
		if (this.m_nextEnv != null && this.m_nextEnv.m_name == name)
		{
			return;
		}
		EnvSetup env = this.GetEnv(name);
		if (env != null)
		{
			this.QueueEnvironment(env);
		}
	}

	// Token: 0x060011CD RID: 4557 RVA: 0x000756F7 File Offset: 0x000738F7
	private void QueueEnvironment(EnvSetup env)
	{
		if (this.m_firstEnv)
		{
			this.m_firstEnv = false;
			this.m_currentEnv = env;
			return;
		}
		this.m_prevEnv = this.m_currentEnv.Clone();
		this.m_nextEnv = env;
		this.m_transitionTimer = 0f;
	}

	// Token: 0x060011CE RID: 4558 RVA: 0x00075734 File Offset: 0x00073934
	private EnvSetup InterpolateEnvironment(EnvSetup a, EnvSetup b, float i)
	{
		EnvSetup envSetup = a.Clone();
		envSetup.m_name = b.m_name;
		if (i >= 0.5f)
		{
			envSetup.m_isFreezingAtNight = b.m_isFreezingAtNight;
			envSetup.m_isFreezing = b.m_isFreezing;
			envSetup.m_isCold = b.m_isCold;
			envSetup.m_isColdAtNight = b.m_isColdAtNight;
			envSetup.m_isColdAtNight = b.m_isColdAtNight;
		}
		envSetup.m_ambColorDay = Color.Lerp(a.m_ambColorDay, b.m_ambColorDay, i);
		envSetup.m_ambColorNight = Color.Lerp(a.m_ambColorNight, b.m_ambColorNight, i);
		envSetup.m_fogColorDay = Color.Lerp(a.m_fogColorDay, b.m_fogColorDay, i);
		envSetup.m_fogColorEvening = Color.Lerp(a.m_fogColorEvening, b.m_fogColorEvening, i);
		envSetup.m_fogColorMorning = Color.Lerp(a.m_fogColorMorning, b.m_fogColorMorning, i);
		envSetup.m_fogColorNight = Color.Lerp(a.m_fogColorNight, b.m_fogColorNight, i);
		envSetup.m_fogColorSunDay = Color.Lerp(a.m_fogColorSunDay, b.m_fogColorSunDay, i);
		envSetup.m_fogColorSunEvening = Color.Lerp(a.m_fogColorSunEvening, b.m_fogColorSunEvening, i);
		envSetup.m_fogColorSunMorning = Color.Lerp(a.m_fogColorSunMorning, b.m_fogColorSunMorning, i);
		envSetup.m_fogColorSunNight = Color.Lerp(a.m_fogColorSunNight, b.m_fogColorSunNight, i);
		envSetup.m_fogDensityDay = Mathf.Lerp(a.m_fogDensityDay, b.m_fogDensityDay, i);
		envSetup.m_fogDensityEvening = Mathf.Lerp(a.m_fogDensityEvening, b.m_fogDensityEvening, i);
		envSetup.m_fogDensityMorning = Mathf.Lerp(a.m_fogDensityMorning, b.m_fogDensityMorning, i);
		envSetup.m_fogDensityNight = Mathf.Lerp(a.m_fogDensityNight, b.m_fogDensityNight, i);
		envSetup.m_sunColorDay = Color.Lerp(a.m_sunColorDay, b.m_sunColorDay, i);
		envSetup.m_sunColorEvening = Color.Lerp(a.m_sunColorEvening, b.m_sunColorEvening, i);
		envSetup.m_sunColorMorning = Color.Lerp(a.m_sunColorMorning, b.m_sunColorMorning, i);
		envSetup.m_sunColorNight = Color.Lerp(a.m_sunColorNight, b.m_sunColorNight, i);
		envSetup.m_lightIntensityDay = Mathf.Lerp(a.m_lightIntensityDay, b.m_lightIntensityDay, i);
		envSetup.m_lightIntensityNight = Mathf.Lerp(a.m_lightIntensityNight, b.m_lightIntensityNight, i);
		envSetup.m_sunAngle = Mathf.Lerp(a.m_sunAngle, b.m_sunAngle, i);
		envSetup.m_windMin = Mathf.Lerp(a.m_windMin, b.m_windMin, i);
		envSetup.m_windMax = Mathf.Lerp(a.m_windMax, b.m_windMax, i);
		envSetup.m_rainCloudAlpha = Mathf.Lerp(a.m_rainCloudAlpha, b.m_rainCloudAlpha, i);
		envSetup.m_ambientLoop = ((i > 0.75f) ? b.m_ambientLoop : a.m_ambientLoop);
		envSetup.m_ambientVol = ((i > 0.75f) ? b.m_ambientVol : a.m_ambientVol);
		envSetup.m_musicEvening = b.m_musicEvening;
		envSetup.m_musicMorning = b.m_musicMorning;
		envSetup.m_musicDay = b.m_musicDay;
		envSetup.m_musicNight = b.m_musicNight;
		return envSetup;
	}

	// Token: 0x060011CF RID: 4559 RVA: 0x00075A44 File Offset: 0x00073C44
	private void SetEnv(EnvSetup env, float dayInt, float nightInt, float morningInt, float eveningInt, float dt)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		this.m_dirLight.transform.rotation = Quaternion.Euler(-90f + env.m_sunAngle, 0f, 0f) * Quaternion.Euler(0f, -90f, 0f) * Quaternion.Euler(-90f + 360f * this.m_smoothDayFraction, 0f, 0f);
		Vector3 v = -this.m_dirLight.transform.forward;
		this.m_dirLight.intensity = env.m_lightIntensityDay * dayInt;
		this.m_dirLight.intensity += env.m_lightIntensityNight * nightInt;
		if (nightInt > 0f)
		{
			this.m_dirLight.transform.rotation = this.m_dirLight.transform.rotation * Quaternion.Euler(180f, 0f, 0f);
		}
		this.m_dirLight.transform.position = mainCamera.transform.position - this.m_dirLight.transform.forward * 3000f;
		this.m_dirLight.color = new Color(0f, 0f, 0f, 0f);
		this.m_dirLight.color += env.m_sunColorNight * nightInt;
		if (dayInt > 0f)
		{
			this.m_dirLight.color += env.m_sunColorDay * dayInt;
			this.m_dirLight.color += env.m_sunColorMorning * morningInt;
			this.m_dirLight.color += env.m_sunColorEvening * eveningInt;
		}
		RenderSettings.fogColor = new Color(0f, 0f, 0f, 0f);
		RenderSettings.fogColor += env.m_fogColorNight * nightInt;
		RenderSettings.fogColor += env.m_fogColorDay * dayInt;
		RenderSettings.fogColor += env.m_fogColorMorning * morningInt;
		RenderSettings.fogColor += env.m_fogColorEvening * eveningInt;
		this.m_sunFogColor = new Color(0f, 0f, 0f, 0f);
		this.m_sunFogColor += env.m_fogColorSunNight * nightInt;
		if (dayInt > 0f)
		{
			this.m_sunFogColor += env.m_fogColorSunDay * dayInt;
			this.m_sunFogColor += env.m_fogColorSunMorning * morningInt;
			this.m_sunFogColor += env.m_fogColorSunEvening * eveningInt;
		}
		this.m_sunFogColor = Color.Lerp(RenderSettings.fogColor, this.m_sunFogColor, Mathf.Clamp01(Mathf.Max(nightInt, dayInt) * 3f));
		RenderSettings.fogDensity = 0f;
		RenderSettings.fogDensity += env.m_fogDensityNight * nightInt;
		RenderSettings.fogDensity += env.m_fogDensityDay * dayInt;
		RenderSettings.fogDensity += env.m_fogDensityMorning * morningInt;
		RenderSettings.fogDensity += env.m_fogDensityEvening * eveningInt;
		RenderSettings.ambientMode = AmbientMode.Flat;
		RenderSettings.ambientLight = Color.Lerp(env.m_ambColorNight, env.m_ambColorDay, dayInt);
		SunShafts component = mainCamera.GetComponent<SunShafts>();
		if (component)
		{
			component.sunColor = this.m_dirLight.color;
		}
		if (env.m_envObject != this.m_currentEnvObject)
		{
			if (this.m_currentEnvObject)
			{
				this.m_currentEnvObject.SetActive(false);
				this.m_currentEnvObject = null;
			}
			if (env.m_envObject)
			{
				this.m_currentEnvObject = env.m_envObject;
				this.m_currentEnvObject.SetActive(true);
			}
		}
		if (env.m_psystems != this.m_currentPSystems)
		{
			if (this.m_currentPSystems != null)
			{
				this.SetParticleArrayEnabled(this.m_currentPSystems, false);
				this.m_currentPSystems = null;
			}
			if (env.m_psystems != null && (!env.m_psystemsOutsideOnly || (Player.m_localPlayer && !Player.m_localPlayer.InShelter())))
			{
				this.SetParticleArrayEnabled(env.m_psystems, true);
				this.m_currentPSystems = env.m_psystems;
			}
		}
		this.m_clouds.material.SetFloat(EnvMan.s_rain, env.m_rainCloudAlpha);
		if (env.m_ambientLoop)
		{
			AudioMan.instance.QueueAmbientLoop(env.m_ambientLoop, env.m_ambientVol);
		}
		else
		{
			AudioMan.instance.StopAmbientLoop();
		}
		Shader.SetGlobalVector(EnvMan.s_skyboxSunDir, v);
		Shader.SetGlobalVector(EnvMan.s_skyboxSunDir, v);
		Shader.SetGlobalVector(EnvMan.s_sunDir, -this.m_dirLight.transform.forward);
		Shader.SetGlobalColor(EnvMan.s_sunFogColor, this.m_sunFogColor);
		Shader.SetGlobalColor(EnvMan.s_sunColor, this.m_dirLight.color * this.m_dirLight.intensity);
		Shader.SetGlobalColor(EnvMan.s_ambientColor, RenderSettings.ambientLight);
		float num = Shader.GetGlobalFloat(EnvMan.s_wet);
		num = Mathf.MoveTowards(num, env.m_isWet ? 1f : 0f, dt / this.m_wetTransitionDuration);
		Shader.SetGlobalFloat(EnvMan.s_wet, num);
	}

	// Token: 0x060011D0 RID: 4560 RVA: 0x00075FF4 File Offset: 0x000741F4
	public float GetDayFraction()
	{
		return this.m_smoothDayFraction;
	}

	// Token: 0x060011D1 RID: 4561 RVA: 0x00075FFC File Offset: 0x000741FC
	public int GetDay(double time)
	{
		return (int)(time / (double)this.m_dayLengthSec);
	}

	// Token: 0x060011D2 RID: 4562 RVA: 0x00076008 File Offset: 0x00074208
	public double GetMorningStartSec(int day)
	{
		return (double)((float)((long)day * this.m_dayLengthSec) + (float)this.m_dayLengthSec * 0.15f);
	}

	// Token: 0x060011D3 RID: 4563 RVA: 0x00076024 File Offset: 0x00074224
	private void UpdateTimeSkip(float dt)
	{
		if (!ZNet.instance.IsServer())
		{
			return;
		}
		if (this.m_skipTime)
		{
			double num = ZNet.instance.GetTimeSeconds();
			num += (double)dt * this.m_timeSkipSpeed;
			if (num >= this.m_skipToTime)
			{
				num = this.m_skipToTime;
				this.m_skipTime = false;
			}
			ZNet.instance.SetNetTime(num);
		}
	}

	// Token: 0x060011D4 RID: 4564 RVA: 0x0007607F File Offset: 0x0007427F
	public bool IsTimeSkipping()
	{
		return this.m_skipTime;
	}

	// Token: 0x060011D5 RID: 4565 RVA: 0x00076088 File Offset: 0x00074288
	public void SkipToMorning()
	{
		double timeSeconds = ZNet.instance.GetTimeSeconds();
		double time = timeSeconds - (double)((float)this.m_dayLengthSec * 0.15f);
		int day = this.GetDay(time);
		double morningStartSec = this.GetMorningStartSec(day + 1);
		this.m_skipTime = true;
		this.m_skipToTime = morningStartSec;
		double num = morningStartSec - timeSeconds;
		this.m_timeSkipSpeed = num / 12.0;
		ZLog.Log(string.Concat(new string[]
		{
			"Time ",
			timeSeconds.ToString(),
			", day:",
			day.ToString(),
			"    nextm:",
			morningStartSec.ToString(),
			"  skipspeed:",
			this.m_timeSkipSpeed.ToString()
		}));
	}

	// Token: 0x060011D6 RID: 4566 RVA: 0x00076144 File Offset: 0x00074344
	public bool CanSleep()
	{
		return (EnvMan.instance.IsAfternoon() || EnvMan.instance.IsNight()) && (Player.m_localPlayer == null || DateTime.Now > Player.m_localPlayer.m_wakeupTime + TimeSpan.FromSeconds(this.m_sleepCooldownSeconds));
	}

	// Token: 0x060011D7 RID: 4567 RVA: 0x000761A0 File Offset: 0x000743A0
	public bool IsDay()
	{
		float dayFraction = this.GetDayFraction();
		return dayFraction >= 0.25f && dayFraction <= 0.75f;
	}

	// Token: 0x060011D8 RID: 4568 RVA: 0x000761CC File Offset: 0x000743CC
	public bool IsAfternoon()
	{
		float dayFraction = this.GetDayFraction();
		return dayFraction >= 0.5f && dayFraction <= 0.75f;
	}

	// Token: 0x060011D9 RID: 4569 RVA: 0x000761F8 File Offset: 0x000743F8
	public bool IsNight()
	{
		float dayFraction = this.GetDayFraction();
		return dayFraction <= 0.25f || dayFraction >= 0.75f;
	}

	// Token: 0x060011DA RID: 4570 RVA: 0x00076224 File Offset: 0x00074424
	public bool IsDaylight()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return (currentEnvironment == null || !currentEnvironment.m_alwaysDark) && this.IsDay();
	}

	// Token: 0x060011DB RID: 4571 RVA: 0x0007624B File Offset: 0x0007444B
	public Heightmap.Biome GetCurrentBiome()
	{
		return this.m_currentBiome;
	}

	// Token: 0x060011DC RID: 4572 RVA: 0x00076253 File Offset: 0x00074453
	public bool IsEnvironment(string name)
	{
		return this.GetCurrentEnvironment().m_name == name;
	}

	// Token: 0x060011DD RID: 4573 RVA: 0x00076268 File Offset: 0x00074468
	public bool IsEnvironment(List<string> names)
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return names.Contains(currentEnvironment.m_name);
	}

	// Token: 0x060011DE RID: 4574 RVA: 0x00076288 File Offset: 0x00074488
	public EnvSetup GetCurrentEnvironment()
	{
		if (!string.IsNullOrEmpty(this.m_forceEnv))
		{
			EnvSetup env = this.GetEnv(this.m_forceEnv);
			if (env != null)
			{
				return env;
			}
		}
		return this.m_currentEnv;
	}

	// Token: 0x060011DF RID: 4575 RVA: 0x000762BC File Offset: 0x000744BC
	public bool IsFreezing()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return currentEnvironment != null && (currentEnvironment.m_isFreezing || (currentEnvironment.m_isFreezingAtNight && !this.IsDay()));
	}

	// Token: 0x060011E0 RID: 4576 RVA: 0x000762F4 File Offset: 0x000744F4
	public bool IsCold()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return currentEnvironment != null && (currentEnvironment.m_isCold || (currentEnvironment.m_isColdAtNight && !this.IsDay()));
	}

	// Token: 0x060011E1 RID: 4577 RVA: 0x0007632C File Offset: 0x0007452C
	public bool IsWet()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return currentEnvironment != null && currentEnvironment.m_isWet;
	}

	// Token: 0x060011E2 RID: 4578 RVA: 0x0007634B File Offset: 0x0007454B
	public Color GetSunFogColor()
	{
		return this.m_sunFogColor;
	}

	// Token: 0x060011E3 RID: 4579 RVA: 0x00076353 File Offset: 0x00074553
	public Vector3 GetSunDirection()
	{
		return this.m_dirLight.transform.forward;
	}

	// Token: 0x060011E4 RID: 4580 RVA: 0x00076368 File Offset: 0x00074568
	private EnvSetup GetEnv(string name)
	{
		foreach (EnvSetup envSetup in this.m_environments)
		{
			if (envSetup.m_name == name)
			{
				return envSetup;
			}
		}
		return null;
	}

	// Token: 0x060011E5 RID: 4581 RVA: 0x000763CC File Offset: 0x000745CC
	private EnvSetup GetDefaultEnv()
	{
		foreach (EnvSetup envSetup in this.m_environments)
		{
			if (envSetup.m_default)
			{
				return envSetup;
			}
		}
		return null;
	}

	// Token: 0x060011E6 RID: 4582 RVA: 0x00076428 File Offset: 0x00074628
	public void SetDebugWind(float angle, float intensity)
	{
		this.m_debugWind = true;
		this.m_debugWindAngle = angle;
		this.m_debugWindIntensity = Mathf.Clamp01(intensity);
	}

	// Token: 0x060011E7 RID: 4583 RVA: 0x00076444 File Offset: 0x00074644
	public void ResetDebugWind()
	{
		this.m_debugWind = false;
	}

	// Token: 0x060011E8 RID: 4584 RVA: 0x0007644D File Offset: 0x0007464D
	public Vector3 GetWindForce()
	{
		return this.GetWindDir() * this.m_wind.w;
	}

	// Token: 0x060011E9 RID: 4585 RVA: 0x00076465 File Offset: 0x00074665
	public Vector3 GetWindDir()
	{
		return new Vector3(this.m_wind.x, this.m_wind.y, this.m_wind.z);
	}

	// Token: 0x060011EA RID: 4586 RVA: 0x0007648D File Offset: 0x0007468D
	public float GetWindIntensity()
	{
		return this.m_wind.w;
	}

	// Token: 0x060011EB RID: 4587 RVA: 0x0007649C File Offset: 0x0007469C
	private void UpdateWind(long timeSec, float dt)
	{
		if (this.m_debugWind)
		{
			float f = 0.017453292f * this.m_debugWindAngle;
			Vector3 dir = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
			this.SetTargetWind(dir, this.m_debugWindIntensity);
		}
		else
		{
			EnvSetup currentEnvironment = this.GetCurrentEnvironment();
			if (currentEnvironment != null)
			{
				UnityEngine.Random.State state = UnityEngine.Random.state;
				float f2 = 0f;
				float num = 0.5f;
				this.AddWindOctave(timeSec, 1, ref f2, ref num);
				this.AddWindOctave(timeSec, 2, ref f2, ref num);
				this.AddWindOctave(timeSec, 4, ref f2, ref num);
				this.AddWindOctave(timeSec, 8, ref f2, ref num);
				UnityEngine.Random.state = state;
				Vector3 dir2 = new Vector3(Mathf.Sin(f2), 0f, Mathf.Cos(f2));
				num = Mathf.Lerp(currentEnvironment.m_windMin, currentEnvironment.m_windMax, num);
				if (Player.m_localPlayer && !Player.m_localPlayer.InInterior())
				{
					float num2 = Utils.LengthXZ(Player.m_localPlayer.transform.position);
					if (num2 > 10500f - this.m_edgeOfWorldWidth)
					{
						float num3 = Utils.LerpStep(10500f - this.m_edgeOfWorldWidth, 10500f, num2);
						num3 = 1f - Mathf.Pow(1f - num3, 2f);
						dir2 = Player.m_localPlayer.transform.position.normalized;
						num = Mathf.Lerp(num, 1f, num3);
					}
					else
					{
						Ship localShip = Ship.GetLocalShip();
						if (localShip && localShip.IsWindControllActive())
						{
							dir2 = localShip.transform.forward;
						}
					}
				}
				this.SetTargetWind(dir2, num);
			}
		}
		this.UpdateWindTransition(dt);
	}

	// Token: 0x060011EC RID: 4588 RVA: 0x00076645 File Offset: 0x00074845
	private void AddWindOctave(long timeSec, int octave, ref float angle, ref float intensity)
	{
		UnityEngine.Random.InitState((int)(timeSec / (this.m_windPeriodDuration / (long)octave)));
		angle += UnityEngine.Random.value * (6.2831855f / (float)octave);
		intensity += -(0.5f / (float)octave) + UnityEngine.Random.value / (float)octave;
	}

	// Token: 0x060011ED RID: 4589 RVA: 0x00076684 File Offset: 0x00074884
	private void SetTargetWind(Vector3 dir, float intensity)
	{
		if (this.m_windTransitionTimer >= 0f)
		{
			return;
		}
		intensity = Mathf.Clamp(intensity, 0.05f, 1f);
		if (Mathf.Approximately(dir.x, this.m_windDir1.x) && Mathf.Approximately(dir.y, this.m_windDir1.y) && Mathf.Approximately(dir.z, this.m_windDir1.z) && Mathf.Approximately(intensity, this.m_windDir1.w))
		{
			return;
		}
		this.m_windTransitionTimer = 0f;
		this.m_windDir2 = new Vector4(dir.x, dir.y, dir.z, intensity);
	}

	// Token: 0x060011EE RID: 4590 RVA: 0x00076738 File Offset: 0x00074938
	private void UpdateWindTransition(float dt)
	{
		if (this.m_windTransitionTimer >= 0f)
		{
			this.m_windTransitionTimer += dt;
			float num = Mathf.Clamp01(this.m_windTransitionTimer / this.m_windTransitionDuration);
			Shader.SetGlobalVector(EnvMan.s_globalWind1, this.m_windDir1);
			Shader.SetGlobalVector(EnvMan.s_globalWind2, this.m_windDir2);
			Shader.SetGlobalFloat(EnvMan.s_globalWindAlpha, num);
			this.m_wind = Vector4.Lerp(this.m_windDir1, this.m_windDir2, num);
			if (num >= 1f)
			{
				this.m_windDir1 = this.m_windDir2;
				this.m_windTransitionTimer = -1f;
			}
		}
		else
		{
			Shader.SetGlobalVector(EnvMan.s_globalWind1, this.m_windDir1);
			Shader.SetGlobalFloat(EnvMan.s_globalWindAlpha, 0f);
			this.m_wind = this.m_windDir1;
		}
		Shader.SetGlobalVector(EnvMan.s_globalWindForce, this.GetWindForce());
	}

	// Token: 0x060011EF RID: 4591 RVA: 0x0007681C File Offset: 0x00074A1C
	public void GetWindData(out Vector4 wind1, out Vector4 wind2, out float alpha)
	{
		wind1 = this.m_windDir1;
		wind2 = this.m_windDir2;
		if (this.m_windTransitionTimer >= 0f)
		{
			alpha = Mathf.Clamp01(this.m_windTransitionTimer / this.m_windTransitionDuration);
			return;
		}
		alpha = 0f;
	}

	// Token: 0x060011F0 RID: 4592 RVA: 0x0007686C File Offset: 0x00074A6C
	public void AppendEnvironment(EnvSetup env)
	{
		EnvSetup env2 = this.GetEnv(env.m_name);
		if (env2 != null)
		{
			this.m_environments.Remove(env2);
		}
		this.m_environments.Add(env);
		this.InitializeEnvironment(env);
	}

	// Token: 0x060011F1 RID: 4593 RVA: 0x000768AC File Offset: 0x00074AAC
	public void AppendBiomeSetup(BiomeEnvSetup biomeEnv)
	{
		BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biomeEnv.m_biome);
		if (biomeEnvSetup != null)
		{
			this.m_biomes.Remove(biomeEnvSetup);
		}
		this.m_biomes.Add(biomeEnv);
		this.InitializeBiomeEnvSetup(biomeEnv);
	}

	// Token: 0x060011F2 RID: 4594 RVA: 0x000768EC File Offset: 0x00074AEC
	public bool CheckInteriorBuildingOverride()
	{
		string b = this.GetCurrentEnvironment().m_name.ToLower();
		using (List<string>.Enumerator enumerator = this.m_interiorBuildingOverrideEnvironments.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.ToLower() == b)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x04001297 RID: 4759
	private static EnvMan s_instance;

	// Token: 0x04001298 RID: 4760
	public Light m_dirLight;

	// Token: 0x04001299 RID: 4761
	public bool m_debugTimeOfDay;

	// Token: 0x0400129A RID: 4762
	[Range(0f, 1f)]
	public float m_debugTime = 0.5f;

	// Token: 0x0400129B RID: 4763
	public string m_debugEnv = "";

	// Token: 0x0400129C RID: 4764
	public bool m_debugWind;

	// Token: 0x0400129D RID: 4765
	[Range(0f, 360f)]
	public float m_debugWindAngle;

	// Token: 0x0400129E RID: 4766
	[Range(0f, 1f)]
	public float m_debugWindIntensity = 1f;

	// Token: 0x0400129F RID: 4767
	public float m_sunHorizonTransitionH = 0.08f;

	// Token: 0x040012A0 RID: 4768
	public float m_sunHorizonTransitionL = 0.02f;

	// Token: 0x040012A1 RID: 4769
	public long m_dayLengthSec = 1200L;

	// Token: 0x040012A2 RID: 4770
	public float m_transitionDuration = 2f;

	// Token: 0x040012A3 RID: 4771
	public long m_environmentDuration = 20L;

	// Token: 0x040012A4 RID: 4772
	public long m_windPeriodDuration = 10L;

	// Token: 0x040012A5 RID: 4773
	public float m_windTransitionDuration = 5f;

	// Token: 0x040012A6 RID: 4774
	public List<EnvSetup> m_environments = new List<EnvSetup>();

	// Token: 0x040012A7 RID: 4775
	public List<string> m_interiorBuildingOverrideEnvironments = new List<string>();

	// Token: 0x040012A8 RID: 4776
	public List<BiomeEnvSetup> m_biomes = new List<BiomeEnvSetup>();

	// Token: 0x040012A9 RID: 4777
	public string m_introEnvironment = "ThunderStorm";

	// Token: 0x040012AA RID: 4778
	public float m_edgeOfWorldWidth = 500f;

	// Token: 0x040012AB RID: 4779
	[Header("Music")]
	public float m_randomMusicIntervalMin = 60f;

	// Token: 0x040012AC RID: 4780
	public float m_randomMusicIntervalMax = 200f;

	// Token: 0x040012AD RID: 4781
	[Header("Other")]
	public MeshRenderer m_clouds;

	// Token: 0x040012AE RID: 4782
	public MeshRenderer m_rainClouds;

	// Token: 0x040012AF RID: 4783
	public MeshRenderer m_rainCloudsDownside;

	// Token: 0x040012B0 RID: 4784
	public float m_wetTransitionDuration = 15f;

	// Token: 0x040012B1 RID: 4785
	public double m_sleepCooldownSeconds = 30.0;

	// Token: 0x040012B2 RID: 4786
	private bool m_skipTime;

	// Token: 0x040012B3 RID: 4787
	private double m_skipToTime;

	// Token: 0x040012B4 RID: 4788
	private double m_timeSkipSpeed = 1.0;

	// Token: 0x040012B5 RID: 4789
	private const double c_TimeSkipDuration = 12.0;

	// Token: 0x040012B6 RID: 4790
	private double m_totalSeconds;

	// Token: 0x040012B7 RID: 4791
	private float m_smoothDayFraction;

	// Token: 0x040012B8 RID: 4792
	private Color m_sunFogColor = Color.white;

	// Token: 0x040012B9 RID: 4793
	private GameObject[] m_currentPSystems;

	// Token: 0x040012BA RID: 4794
	private GameObject m_currentEnvObject;

	// Token: 0x040012BB RID: 4795
	private const float c_MorningL = 0.15f;

	// Token: 0x040012BC RID: 4796
	private Vector4 m_windDir1 = new Vector4(0f, 0f, -1f, 0f);

	// Token: 0x040012BD RID: 4797
	private Vector4 m_windDir2 = new Vector4(0f, 0f, -1f, 0f);

	// Token: 0x040012BE RID: 4798
	private Vector4 m_wind = new Vector4(0f, 0f, -1f, 0f);

	// Token: 0x040012BF RID: 4799
	private float m_windTransitionTimer = -1f;

	// Token: 0x040012C0 RID: 4800
	private Vector3 m_cloudOffset = Vector3.zero;

	// Token: 0x040012C1 RID: 4801
	private string m_forceEnv = "";

	// Token: 0x040012C2 RID: 4802
	private EnvSetup m_currentEnv;

	// Token: 0x040012C3 RID: 4803
	private EnvSetup m_prevEnv;

	// Token: 0x040012C4 RID: 4804
	private EnvSetup m_nextEnv;

	// Token: 0x040012C5 RID: 4805
	private string m_ambientMusic;

	// Token: 0x040012C6 RID: 4806
	private float m_ambientMusicTimer;

	// Token: 0x040012C7 RID: 4807
	private Heightmap m_cachedHeightmap;

	// Token: 0x040012C8 RID: 4808
	private Heightmap.Biome m_currentBiome;

	// Token: 0x040012C9 RID: 4809
	private long m_environmentPeriod;

	// Token: 0x040012CA RID: 4810
	private float m_transitionTimer;

	// Token: 0x040012CB RID: 4811
	private bool m_firstEnv = true;

	// Token: 0x040012CC RID: 4812
	private static readonly int s_netRefPos = Shader.PropertyToID("_NetRefPos");

	// Token: 0x040012CD RID: 4813
	private static readonly int s_skyboxSunDir = Shader.PropertyToID("_SkyboxSunDir");

	// Token: 0x040012CE RID: 4814
	private static readonly int s_sunDir = Shader.PropertyToID("_SunDir");

	// Token: 0x040012CF RID: 4815
	private static readonly int s_sunFogColor = Shader.PropertyToID("_SunFogColor");

	// Token: 0x040012D0 RID: 4816
	private static readonly int s_wet = Shader.PropertyToID("_Wet");

	// Token: 0x040012D1 RID: 4817
	private static readonly int s_sunColor = Shader.PropertyToID("_SunColor");

	// Token: 0x040012D2 RID: 4818
	private static readonly int s_ambientColor = Shader.PropertyToID("_AmbientColor");

	// Token: 0x040012D3 RID: 4819
	private static readonly int s_globalWind1 = Shader.PropertyToID("_GlobalWind1");

	// Token: 0x040012D4 RID: 4820
	private static readonly int s_globalWind2 = Shader.PropertyToID("_GlobalWind2");

	// Token: 0x040012D5 RID: 4821
	private static readonly int s_globalWindAlpha = Shader.PropertyToID("_GlobalWindAlpha");

	// Token: 0x040012D6 RID: 4822
	private static readonly int s_cloudOffset = Shader.PropertyToID("_CloudOffset");

	// Token: 0x040012D7 RID: 4823
	private static readonly int s_globalWindForce = Shader.PropertyToID("_GlobalWindForce");

	// Token: 0x040012D8 RID: 4824
	private static readonly int s_rain = Shader.PropertyToID("_Rain");
}
