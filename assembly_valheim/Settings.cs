using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fishlabs;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020000F8 RID: 248
public class Settings : MonoBehaviour
{
	// Token: 0x17000058 RID: 88
	// (get) Token: 0x06000A1C RID: 2588 RVA: 0x0004CE0A File Offset: 0x0004B00A
	public static Settings instance
	{
		get
		{
			return Settings.m_instance;
		}
	}

	// Token: 0x06000A1D RID: 2589 RVA: 0x0004CE14 File Offset: 0x0004B014
	private void Awake()
	{
		Settings.m_instance = this;
		this.m_bindDialog.SetActive(false);
		this.m_resDialog.SetActive(false);
		this.m_resSwitchDialog.SetActive(false);
		this.m_gamepadRoot.SetActive(false);
		this.m_resListBaseSize = this.m_resListRoot.rect.height;
		this.LoadSettings();
		this.SetupKeys();
		foreach (Selectable selectable in this.m_settingsPanel.GetComponentsInChildren<Selectable>())
		{
			if (selectable.enabled)
			{
				this.m_navigationObjects.Add(selectable);
			}
		}
		this.m_tabHandler = base.GetComponentInChildren<TabHandler>();
		this.SetAvailableTabs();
	}

	// Token: 0x06000A1E RID: 2590 RVA: 0x000023E2 File Offset: 0x000005E2
	private void SetAvailableTabs()
	{
	}

	// Token: 0x06000A1F RID: 2591 RVA: 0x0004CEC0 File Offset: 0x0004B0C0
	private void OnDestroy()
	{
		Action settingsPopupDestroyed = this.SettingsPopupDestroyed;
		if (settingsPopupDestroyed != null)
		{
			settingsPopupDestroyed();
		}
		Settings.m_instance = null;
	}

	// Token: 0x06000A20 RID: 2592 RVA: 0x0004CEDC File Offset: 0x0004B0DC
	private void Update()
	{
		if (this.m_bindDialog.activeSelf)
		{
			this.UpdateBinding();
			return;
		}
		this.UpdateResSwitch(Time.unscaledDeltaTime);
		AudioListener.volume = this.m_volumeSlider.value;
		MusicMan.m_masterMusicVolume = this.m_musicVolumeSlider.value;
		AudioMan.SetSFXVolume(this.m_sfxVolumeSlider.value);
		this.SetQualityText(this.m_shadowQualityText, this.GetQualityText((int)this.m_shadowQuality.value));
		this.SetQualityText(this.m_lodText, this.GetQualityText((int)this.m_lod.value));
		this.SetQualityText(this.m_lightsText, this.GetQualityText((int)this.m_lights.value));
		this.SetQualityText(this.m_vegetationText, this.GetQualityText((int)this.m_vegetation.value));
		int pointLightLimit = Settings.GetPointLightLimit((int)this.m_pointLights.value);
		int pointLightShadowLimit = Settings.GetPointLightShadowLimit((int)this.m_pointLightShadows.value);
		this.SetQualityText(this.m_pointLightsText, this.GetQualityText((int)this.m_pointLights.value) + " (" + ((pointLightLimit < 0) ? Localization.instance.Localize("$settings_infinite") : pointLightLimit.ToString()) + ")");
		this.SetQualityText(this.m_pointLightShadowsText, this.GetQualityText((int)this.m_pointLightShadows.value) + " (" + ((pointLightShadowLimit < 0) ? Localization.instance.Localize("$settings_infinite") : pointLightShadowLimit.ToString()) + ")");
		this.SetQualityText(this.m_fpsLimitText, (this.m_fpsLimit.value < 30f) ? Localization.instance.Localize("$settings_infinite") : this.m_fpsLimit.value.ToString());
		if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
		{
			this.m_resButtonText.text = string.Format("{0}x{1} {2}hz", this.m_selectedRes.width, this.m_selectedRes.height, this.m_selectedRes.refreshRate);
		}
		else
		{
			this.m_resButtonText.text = string.Format("{0}x{1}", this.m_selectedRes.width, this.m_selectedRes.height);
		}
		this.m_guiScaleText.text = this.m_guiScaleSlider.value.ToString() + "%";
		GuiScaler.SetScale(this.m_guiScaleSlider.value / 100f);
		this.SetQualityText(this.m_autoBackupsText, (this.m_autoBackups.value == 1f) ? "0" : this.m_autoBackups.value.ToString());
		this.UpdateGamepad();
		if (!this.m_navigationEnabled && !this.m_gamepadRoot.gameObject.activeInHierarchy && !this.m_resDialog.activeInHierarchy && !this.m_resSwitchDialog.activeInHierarchy)
		{
			this.ToggleNavigation(true);
		}
		if (this.m_toggleNavKeyPressed != KeyCode.None && ZInput.instance.GetPressedKey() == KeyCode.None)
		{
			this.m_toggleNavKeyPressed = KeyCode.None;
			this.ToggleNavigation(true);
		}
		bool flag = true;
		if (this.m_gamepadRootWasVisible && this.m_gamepadRoot.gameObject.activeInHierarchy && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB")))
		{
			this.HideGamepadMap();
			flag = false;
		}
		this.m_gamepadRootWasVisible = this.m_gamepadRoot.activeInHierarchy;
		if (this.m_gamepadRoot.gameObject.activeInHierarchy)
		{
			Settings.UpdateGamepadMap(this.m_gamepadRoot, this.m_alternativeGlyphs.isOn, ZInput.InputLayout, true);
		}
		if (Input.GetKeyDown(KeyCode.Escape) && flag)
		{
			this.OnBack();
		}
	}

	// Token: 0x06000A21 RID: 2593 RVA: 0x0004D28D File Offset: 0x0004B48D
	public void ShowGamepadMap()
	{
		this.m_gamepadRoot.SetActive(true);
		this.m_gamepadRoot.GetComponentInChildren<Button>().Select();
		this.ToggleNavigation(false);
	}

	// Token: 0x06000A22 RID: 2594 RVA: 0x0004D2B2 File Offset: 0x0004B4B2
	public void HideGamepadMap()
	{
		this.m_gamepadRoot.SetActive(false);
	}

	// Token: 0x06000A23 RID: 2595 RVA: 0x0004D2C0 File Offset: 0x0004B4C0
	private void UpdateGamepad()
	{
		if (this.m_resDialog.activeInHierarchy)
		{
			if (ZInput.GetButtonDown("JoyBack") || ZInput.GetButtonDown("JoyButtonB"))
			{
				this.OnResCancel();
			}
			if (this.m_resObjects.Count > 1)
			{
				if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown") || Input.GetKeyDown(KeyCode.DownArrow))
				{
					if (this.m_selectedResIndex < this.m_resObjects.Count - 1)
					{
						this.m_selectedResIndex++;
					}
					this.<UpdateGamepad>g__updateResScroll|8_0();
				}
				else if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp") || Input.GetKeyDown(KeyCode.UpArrow))
				{
					if (this.m_selectedResIndex > 0)
					{
						this.m_selectedResIndex--;
					}
					this.<UpdateGamepad>g__updateResScroll|8_0();
				}
			}
		}
		if (this.m_resSwitchDialog.activeInHierarchy && (ZInput.GetButtonDown("JoyBack") || ZInput.GetButtonDown("JoyButtonB")))
		{
			this.RevertMode();
			this.m_resSwitchDialog.SetActive(false);
			this.ToggleNavigation(true);
		}
	}

	// Token: 0x06000A24 RID: 2596 RVA: 0x0004D3DC File Offset: 0x0004B5DC
	public static void UpdateGamepadMap(GameObject gamepadRoot, bool altGlyphs, InputLayout layout, bool showUI = false)
	{
		GamepadMapController gamepadMapController = gamepadRoot.GetComponent<GamepadMapController>();
		if (gamepadMapController == null)
		{
			gamepadMapController = gamepadRoot.GetComponentInChildren<GamepadMapController>();
		}
		if (gamepadMapController != null)
		{
			GamepadMapType type;
			if (altGlyphs)
			{
				if (Settings.IsSteamRunningOnSteamDeck())
				{
					type = GamepadMapType.SteamPS;
				}
				else
				{
					type = GamepadMapType.PS;
				}
			}
			else if (Settings.IsSteamRunningOnSteamDeck())
			{
				type = GamepadMapType.SteamXbox;
			}
			else
			{
				type = GamepadMapType.Default;
			}
			gamepadMapController.SetGamepadMap(type, layout, showUI);
		}
	}

	// Token: 0x06000A25 RID: 2597 RVA: 0x0004D433 File Offset: 0x0004B633
	private void SetQualityText(Text text, string str)
	{
		text.text = Localization.instance.Localize(str);
	}

	// Token: 0x06000A26 RID: 2598 RVA: 0x0004D446 File Offset: 0x0004B646
	private string GetQualityText(int level)
	{
		switch (level)
		{
		default:
			return "[$settings_low]";
		case 1:
			return "[$settings_medium]";
		case 2:
			return "[$settings_high]";
		case 3:
			return "[$settings_veryhigh]";
		}
	}

	// Token: 0x06000A27 RID: 2599 RVA: 0x0004D475 File Offset: 0x0004B675
	public void OnBack()
	{
		this.RevertMode();
		this.LoadSettings();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000A28 RID: 2600 RVA: 0x0004D48E File Offset: 0x0004B68E
	public void OnOk()
	{
		this.SaveSettings();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000A29 RID: 2601 RVA: 0x0004D4A4 File Offset: 0x0004B6A4
	public static void SetPlatformDefaultPrefs()
	{
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			ZLog.Log("Running on Steam Deck!");
		}
		else
		{
			ZLog.Log("Using default prefs");
		}
		PlatformPrefs.PlatformDefaults[] array = new PlatformPrefs.PlatformDefaults[1];
		array[0] = new PlatformPrefs.PlatformDefaults("deck_", () => Settings.IsSteamRunningOnSteamDeck(), new Dictionary<string, PlatformPrefs>
		{
			{
				"GuiScale",
				1.15f
			},
			{
				"DOF",
				0
			},
			{
				"VSync",
				0
			},
			{
				"Bloom",
				1
			},
			{
				"SSAO",
				1
			},
			{
				"SunShafts",
				1
			},
			{
				"AntiAliasing",
				0
			},
			{
				"ChromaticAberration",
				1
			},
			{
				"MotionBlur",
				0
			},
			{
				"SoftPart",
				1
			},
			{
				"Tesselation",
				0
			},
			{
				"DistantShadows",
				1
			},
			{
				"ShadowQuality",
				0
			},
			{
				"LodBias",
				1
			},
			{
				"Lights",
				1
			},
			{
				"ClutterQuality",
				1
			},
			{
				"PointLights",
				1
			},
			{
				"PointLightShadows",
				1
			},
			{
				"FPSLimit",
				60
			}
		});
		PlatformPrefs.SetDefaults(array);
	}

	// Token: 0x06000A2A RID: 2602 RVA: 0x0004D654 File Offset: 0x0004B854
	public static bool IsSteamRunningOnSteamDeck()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SteamDeck");
		return !string.IsNullOrEmpty(environmentVariable) && environmentVariable != "0";
	}

	// Token: 0x06000A2B RID: 2603 RVA: 0x0004D684 File Offset: 0x0004B884
	private void SaveSettings()
	{
		PlatformPrefs.SetFloat("MasterVolume", this.m_volumeSlider.value);
		PlatformPrefs.SetFloat("MouseSensitivity", this.m_sensitivitySlider.value);
		PlatformPrefs.SetFloat("GamepadSensitivity", this.m_gamepadSensitivitySlider.value);
		PlatformPrefs.SetFloat("MusicVolume", this.m_musicVolumeSlider.value);
		PlatformPrefs.SetFloat("SfxVolume", this.m_sfxVolumeSlider.value);
		PlatformPrefs.SetInt("ContinousMusic", this.m_continousMusic.isOn ? 1 : 0);
		PlatformPrefs.SetInt("InvertMouse", this.m_invertMouse.isOn ? 1 : 0);
		PlatformPrefs.SetFloat("GuiScale", this.m_guiScaleSlider.value / 100f);
		PlatformPrefs.SetInt("AutoBackups", (int)this.m_autoBackups.value);
		PlatformPrefs.SetInt("CameraShake", this.m_cameraShake.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ShipCameraTilt", this.m_shipCameraTilt.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ReduceBackgroundUsage", this.m_reduceBGUsage.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ReduceFlashingLights", this.m_reduceFlashingLights.isOn ? 1 : 0);
		PlatformPrefs.SetInt("QuickPieceSelect", this.m_quickPieceSelect.isOn ? 1 : 0);
		PlatformPrefs.SetInt("TutorialsEnabled", this.m_tutorialsEnabled.isOn ? 1 : 0);
		PlatformPrefs.SetInt("KeyHints", this.m_showKeyHints.isOn ? 1 : 0);
		PlatformPrefs.SetInt("DOF", this.m_dofToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("VSync", this.m_vsyncToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("Bloom", this.m_bloomToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("SSAO", this.m_ssaoToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("SunShafts", this.m_sunshaftsToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("AntiAliasing", this.m_aaToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ChromaticAberration", this.m_caToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("MotionBlur", this.m_motionblurToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("SoftPart", this.m_softPartToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("Tesselation", this.m_tesselationToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("DistantShadows", this.m_distantShadowsToggle.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ShadowQuality", (int)this.m_shadowQuality.value);
		PlatformPrefs.SetInt("LodBias", (int)this.m_lod.value);
		PlatformPrefs.SetInt("Lights", (int)this.m_lights.value);
		PlatformPrefs.SetInt("ClutterQuality", (int)this.m_vegetation.value);
		PlatformPrefs.SetInt("PointLights", (int)this.m_pointLights.value);
		PlatformPrefs.SetInt("PointLightShadows", (int)this.m_pointLightShadows.value);
		PlatformPrefs.SetInt("FPSLimit", (int)this.m_fpsLimit.value);
		ZInput.SetGamepadEnabled(this.m_gamepadEnabled.isOn);
		PlatformPrefs.SetInt("AltGlyphs", this.m_alternativeGlyphs.isOn ? 1 : 0);
		ZInput.PlayStationGlyphs = this.m_alternativeGlyphs.isOn;
		PlatformPrefs.SetInt("SwapTriggers", this.m_swapTriggers.isOn ? 1 : 0);
		ZInput.SwapTriggers = this.m_swapTriggers.isOn;
		Settings.ContinousMusic = this.m_continousMusic.isOn;
		Settings.ReduceBackgroundUsage = this.m_reduceBGUsage.isOn;
		Settings.ReduceFlashingLights = this.m_reduceFlashingLights.isOn;
		Raven.m_tutorialsEnabled = this.m_tutorialsEnabled.isOn;
		ZInput.instance.Save();
		ZInput.instance.Reset();
		ZInput.instance.Load();
		if (GameCamera.instance)
		{
			GameCamera.instance.ApplySettings();
		}
		if (CameraEffects.instance)
		{
			CameraEffects.instance.ApplySettings();
		}
		if (ClutterSystem.instance)
		{
			ClutterSystem.instance.ApplySettings();
		}
		if (MusicMan.instance)
		{
			MusicMan.instance.ApplySettings();
		}
		if (GameCamera.instance)
		{
			GameCamera.instance.ApplySettings();
		}
		if (KeyHints.instance)
		{
			KeyHints.instance.ApplySettings();
		}
		Settings.ApplyQualitySettings();
		this.ApplyMode();
		PlayerController.m_mouseSens = this.m_sensitivitySlider.value;
		PlayerController.m_gamepadSens = this.m_gamepadSensitivitySlider.value;
		PlayerController.m_invertMouse = this.m_invertMouse.isOn;
		Localization.instance.SetLanguage(this.m_languageKey);
		GuiScaler.LoadGuiScale();
		PlayerPrefs.Save();
	}

	// Token: 0x06000A2C RID: 2604 RVA: 0x0004DB80 File Offset: 0x0004BD80
	public static void ApplyStartupSettings()
	{
		Settings.ReduceBackgroundUsage = (PlatformPrefs.GetInt("ReduceBackgroundUsage", 0) == 1);
		Settings.ContinousMusic = (PlatformPrefs.GetInt("ContinousMusic", 1) == 1);
		Settings.ReduceFlashingLights = (PlatformPrefs.GetInt("ReduceFlashingLights", 0) == 1);
		Raven.m_tutorialsEnabled = (PlatformPrefs.GetInt("TutorialsEnabled", 1) == 1);
		Settings.ApplyQualitySettings();
	}

	// Token: 0x06000A2D RID: 2605 RVA: 0x0004DBE4 File Offset: 0x0004BDE4
	private static void ApplyQualitySettings()
	{
		QualitySettings.vSyncCount = ((PlatformPrefs.GetInt("VSync", 0) == 1) ? 1 : 0);
		QualitySettings.softParticles = (PlatformPrefs.GetInt("SoftPart", 1) == 1);
		if (PlatformPrefs.GetInt("Tesselation", 1) == 1)
		{
			Shader.EnableKeyword("TESSELATION_ON");
		}
		else
		{
			Shader.DisableKeyword("TESSELATION_ON");
		}
		switch (PlatformPrefs.GetInt("LodBias", 2))
		{
		case 0:
			QualitySettings.lodBias = 1f;
			break;
		case 1:
			QualitySettings.lodBias = 1.5f;
			break;
		case 2:
			QualitySettings.lodBias = 2f;
			break;
		case 3:
			QualitySettings.lodBias = 5f;
			break;
		}
		switch (PlatformPrefs.GetInt("Lights", 2))
		{
		case 0:
			QualitySettings.pixelLightCount = 2;
			break;
		case 1:
			QualitySettings.pixelLightCount = 4;
			break;
		case 2:
			QualitySettings.pixelLightCount = 8;
			break;
		}
		LightLod.m_lightLimit = Settings.GetPointLightLimit(PlatformPrefs.GetInt("PointLights", 3));
		LightLod.m_shadowLimit = Settings.GetPointLightShadowLimit(PlatformPrefs.GetInt("PointLightShadows", 2));
		Settings.FPSLimit = PlatformPrefs.GetInt("FPSLimit", -1);
		Settings.ApplyShadowQuality();
	}

	// Token: 0x06000A2E RID: 2606 RVA: 0x0004DD10 File Offset: 0x0004BF10
	private static int GetPointLightLimit(int level)
	{
		switch (level)
		{
		case 0:
			return 4;
		case 1:
			return 15;
		case 3:
			return -1;
		}
		return 40;
	}

	// Token: 0x06000A2F RID: 2607 RVA: 0x0004DD33 File Offset: 0x0004BF33
	private static int GetPointLightShadowLimit(int level)
	{
		switch (level)
		{
		case 0:
			return 0;
		case 1:
			return 1;
		case 3:
			return -1;
		}
		return 3;
	}

	// Token: 0x06000A30 RID: 2608 RVA: 0x0004DD54 File Offset: 0x0004BF54
	private static void ApplyShadowQuality()
	{
		int @int = PlatformPrefs.GetInt("ShadowQuality", 2);
		int int2 = PlatformPrefs.GetInt("DistantShadows", 1);
		switch (@int)
		{
		case 0:
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 80f;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			break;
		case 1:
			QualitySettings.shadowCascades = 3;
			QualitySettings.shadowDistance = 120f;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			break;
		case 2:
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 150f;
			QualitySettings.shadowResolution = ShadowResolution.High;
			break;
		}
		Heightmap.EnableDistantTerrainShadows = (int2 == 1);
	}

	// Token: 0x06000A31 RID: 2609 RVA: 0x0004DDDC File Offset: 0x0004BFDC
	private void LoadSettings()
	{
		ZInput.instance.Load();
		AudioListener.volume = PlatformPrefs.GetFloat("MasterVolume", AudioListener.volume);
		MusicMan.m_masterMusicVolume = PlatformPrefs.GetFloat("MusicVolume", 1f);
		AudioMan.SetSFXVolume(PlatformPrefs.GetFloat("SfxVolume", 1f));
		Settings.ContinousMusic = (this.m_continousMusic.isOn = ((PlatformPrefs.GetInt("ContinousMusic", 1) == 1) ? true : false));
		PlayerController.m_mouseSens = PlatformPrefs.GetFloat("MouseSensitivity", PlayerController.m_mouseSens);
		PlayerController.m_gamepadSens = PlatformPrefs.GetFloat("GamepadSensitivity", PlayerController.m_gamepadSens);
		PlayerController.m_invertMouse = (PlatformPrefs.GetInt("InvertMouse", 0) == 1);
		float @float = PlatformPrefs.GetFloat("GuiScale", 1f);
		this.m_volumeSlider.value = AudioListener.volume;
		this.m_sensitivitySlider.value = PlayerController.m_mouseSens;
		this.m_gamepadSensitivitySlider.value = PlayerController.m_gamepadSens;
		this.m_sfxVolumeSlider.value = AudioMan.GetSFXVolume();
		this.m_musicVolumeSlider.value = MusicMan.m_masterMusicVolume;
		this.m_guiScaleSlider.value = @float * 100f;
		this.m_autoBackups.value = (float)PlatformPrefs.GetInt("AutoBackups", 4);
		this.m_invertMouse.isOn = PlayerController.m_invertMouse;
		this.m_gamepadEnabled.isOn = ZInput.IsGamepadEnabled();
		this.m_alternativeGlyphs.isOn = (ZInput.PlayStationGlyphs = (PlatformPrefs.GetInt("AltGlyphs", 0) == 1));
		this.m_swapTriggers.isOn = (ZInput.SwapTriggers = (PlatformPrefs.GetInt("SwapTriggers", 0) == 1));
		this.m_languageKey = Localization.instance.GetSelectedLanguage();
		this.m_language.text = Localization.instance.Localize("$language_" + this.m_languageKey.ToLower());
		this.m_cameraShake.isOn = (PlatformPrefs.GetInt("CameraShake", 1) == 1);
		this.m_shipCameraTilt.isOn = (PlatformPrefs.GetInt("ShipCameraTilt", 1) == 1);
		Settings.ReduceBackgroundUsage = (this.m_reduceBGUsage.isOn = ((PlatformPrefs.GetInt("ReduceBackgroundUsage", 0) == 1) ? true : false));
		Settings.ReduceFlashingLights = (this.m_reduceFlashingLights.isOn = ((PlatformPrefs.GetInt("ReduceFlashingLights", 0) == 1) ? true : false));
		this.m_quickPieceSelect.isOn = (PlatformPrefs.GetInt("QuickPieceSelect", 0) == 1);
		Raven.m_tutorialsEnabled = (this.m_tutorialsEnabled.isOn = ((PlatformPrefs.GetInt("TutorialsEnabled", 1) == 1) ? true : false));
		this.m_showKeyHints.isOn = (PlatformPrefs.GetInt("KeyHints", 1) == 1);
		this.m_dofToggle.isOn = (PlatformPrefs.GetInt("DOF", 1) == 1);
		this.m_vsyncToggle.isOn = (PlatformPrefs.GetInt("VSync", 0) == 1);
		this.m_bloomToggle.isOn = (PlatformPrefs.GetInt("Bloom", 1) == 1);
		this.m_ssaoToggle.isOn = (PlatformPrefs.GetInt("SSAO", 1) == 1);
		this.m_sunshaftsToggle.isOn = (PlatformPrefs.GetInt("SunShafts", 1) == 1);
		this.m_aaToggle.isOn = (PlatformPrefs.GetInt("AntiAliasing", 1) == 1);
		this.m_caToggle.isOn = (PlatformPrefs.GetInt("ChromaticAberration", 1) == 1);
		this.m_motionblurToggle.isOn = (PlatformPrefs.GetInt("MotionBlur", 1) == 1);
		this.m_softPartToggle.isOn = (PlatformPrefs.GetInt("SoftPart", 1) == 1);
		this.m_tesselationToggle.isOn = (PlatformPrefs.GetInt("Tesselation", 1) == 1);
		this.m_distantShadowsToggle.isOn = (PlatformPrefs.GetInt("DistantShadows", 1) == 1);
		this.m_shadowQuality.value = (float)PlatformPrefs.GetInt("ShadowQuality", 2);
		this.m_lod.value = (float)PlatformPrefs.GetInt("LodBias", 2);
		this.m_lights.value = (float)PlatformPrefs.GetInt("Lights", 2);
		this.m_vegetation.value = (float)PlatformPrefs.GetInt("ClutterQuality", 2);
		this.m_pointLights.value = (float)PlatformPrefs.GetInt("PointLights", 3);
		this.m_pointLightShadows.value = (float)PlatformPrefs.GetInt("PointLightShadows", 2);
		this.m_fpsLimit.value = (float)PlatformPrefs.GetInt("FPSLimit", -1);
		this.m_fpsLimit.minValue = 29f;
		this.m_fullscreenToggle.isOn = Screen.fullScreen;
		this.m_oldFullscreen = this.m_fullscreenToggle.isOn;
		this.m_oldRes = Screen.currentResolution;
		this.m_oldRes.width = Screen.width;
		this.m_oldRes.height = Screen.height;
		this.m_selectedRes = this.m_oldRes;
		ZLog.Log(string.Concat(new string[]
		{
			"Current res ",
			Screen.currentResolution.width.ToString(),
			"x",
			Screen.currentResolution.height.ToString(),
			"     ",
			Screen.width.ToString(),
			"x",
			Screen.height.ToString()
		}));
	}

	// Token: 0x06000A32 RID: 2610 RVA: 0x0004E360 File Offset: 0x0004C560
	private void SetupKeys()
	{
		foreach (Settings.KeySetting keySetting in this.m_keys)
		{
			keySetting.m_keyTransform.GetComponentInChildren<Button>().onClick.AddListener(new UnityAction(this.OnKeySet));
		}
		this.UpdateBindings();
	}

	// Token: 0x06000A33 RID: 2611 RVA: 0x0004E3D4 File Offset: 0x0004C5D4
	private void UpdateBindings()
	{
		foreach (Settings.KeySetting keySetting in this.m_keys)
		{
			keySetting.m_keyTransform.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = Localization.instance.GetBoundKeyString(keySetting.m_keyName, true);
		}
		Settings.UpdateGamepadMap(this.m_gamepadRoot, this.m_alternativeGlyphs.isOn, ZInput.InputLayout, true);
	}

	// Token: 0x06000A34 RID: 2612 RVA: 0x0004E464 File Offset: 0x0004C664
	private void OnKeySet()
	{
		foreach (Settings.KeySetting keySetting in this.m_keys)
		{
			if (keySetting.m_keyTransform.GetComponentInChildren<Button>().gameObject == EventSystem.current.currentSelectedGameObject)
			{
				this.OpenBindDialog(keySetting.m_keyName);
				return;
			}
		}
		ZLog.Log("NOT FOUND");
	}

	// Token: 0x06000A35 RID: 2613 RVA: 0x0004E4EC File Offset: 0x0004C6EC
	private void OpenBindDialog(string keyName)
	{
		ZLog.Log("Binding key " + keyName);
		this.ToggleNavigation(false);
		ZInput.instance.StartBindKey(keyName);
		this.m_bindDialog.SetActive(true);
	}

	// Token: 0x06000A36 RID: 2614 RVA: 0x0004E51C File Offset: 0x0004C71C
	private void UpdateBinding()
	{
		if (this.m_bindDialog.activeSelf && ZInput.instance.EndBindKey())
		{
			this.m_bindDialog.SetActive(false);
			this.ToggleNavigation(true);
			this.UpdateBindings();
		}
	}

	// Token: 0x06000A37 RID: 2615 RVA: 0x0004E550 File Offset: 0x0004C750
	public void ResetBindings()
	{
		ZInput.instance.Reset();
		this.UpdateBindings();
	}

	// Token: 0x06000A38 RID: 2616 RVA: 0x0004E564 File Offset: 0x0004C764
	public void OnLanguageLeft()
	{
		this.m_languageKey = Localization.instance.GetPrevLanguage(this.m_languageKey);
		this.m_language.text = Localization.instance.Localize("$language_" + this.m_languageKey.ToLower());
	}

	// Token: 0x06000A39 RID: 2617 RVA: 0x0004E5B4 File Offset: 0x0004C7B4
	public void OnLanguageRight()
	{
		this.m_languageKey = Localization.instance.GetNextLanguage(this.m_languageKey);
		this.m_language.text = Localization.instance.Localize("$language_" + this.m_languageKey.ToLower());
	}

	// Token: 0x06000A3A RID: 2618 RVA: 0x0004E601 File Offset: 0x0004C801
	public void OnShowResList()
	{
		this.m_resDialog.SetActive(true);
		this.ToggleNavigation(false);
		this.FillResList();
	}

	// Token: 0x06000A3B RID: 2619 RVA: 0x0004E61C File Offset: 0x0004C81C
	private void UpdateValidResolutions()
	{
		Resolution[] array = Screen.resolutions;
		if (array.Length == 0)
		{
			array = new Resolution[]
			{
				this.m_oldRes
			};
		}
		this.m_resolutions.Clear();
		foreach (Resolution item in array)
		{
			if ((item.width >= this.m_minResWidth && item.height >= this.m_minResHeight) || item.width == this.m_oldRes.width || item.height == this.m_oldRes.height)
			{
				this.m_resolutions.Add(item);
			}
		}
		if (this.m_resolutions.Count == 0)
		{
			Resolution item2 = default(Resolution);
			item2.width = 1280;
			item2.height = 720;
			item2.refreshRate = 60;
			this.m_resolutions.Add(item2);
		}
	}

	// Token: 0x06000A3C RID: 2620 RVA: 0x0004E700 File Offset: 0x0004C900
	private void FillResList()
	{
		foreach (GameObject obj in this.m_resObjects)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_resObjects.Clear();
		this.m_selectedResIndex = 0;
		this.UpdateValidResolutions();
		List<string> list = new List<string>();
		float num = 0f;
		using (List<Resolution>.Enumerator enumerator2 = this.m_resolutions.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				Resolution res = enumerator2.Current;
				string text = string.Format("{0}x{1}", res.width, res.height);
				if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen || !list.Contains(text))
				{
					list.Add(text);
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_resListElement, this.m_resListRoot.transform);
					gameObject.SetActive(true);
					gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
					{
						this.OnResClick(res);
					});
					(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, num * -this.m_resListSpace);
					Text componentInChildren = gameObject.GetComponentInChildren<Text>();
					if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
					{
						componentInChildren.text = string.Format("{0} {1}hz", text, res.refreshRate);
					}
					else
					{
						componentInChildren.text = text;
					}
					this.m_resObjects.Add(gameObject);
					num += 1f;
				}
			}
		}
		float size = Mathf.Max(this.m_resListBaseSize, num * this.m_resListSpace);
		this.m_resListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		this.m_resListScroll.value = 1f;
	}

	// Token: 0x06000A3D RID: 2621 RVA: 0x0004E914 File Offset: 0x0004CB14
	private void ToggleNavigation(bool enabled)
	{
		if (!enabled && EventSystem.current.currentSelectedGameObject != null)
		{
			this.m_lastSelected = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
		}
		this.m_backButton.SetActive(enabled);
		this.m_okButton.SetActive(enabled);
		KeyCode pressedKey = ZInput.instance.GetPressedKey();
		if (enabled && pressedKey != KeyCode.None)
		{
			this.m_toggleNavKeyPressed = pressedKey;
			return;
		}
		this.m_navigationEnabled = enabled;
		foreach (Selectable selectable in this.m_navigationObjects)
		{
			selectable.interactable = enabled;
		}
		this.m_tabHandler.m_gamepadInput = enabled;
		if (enabled && this.m_lastSelected != null)
		{
			this.m_lastSelected.Select();
		}
	}

	// Token: 0x06000A3E RID: 2622 RVA: 0x0004E9F0 File Offset: 0x0004CBF0
	public void OnResCancel()
	{
		this.m_resDialog.SetActive(false);
		this.ToggleNavigation(true);
	}

	// Token: 0x06000A3F RID: 2623 RVA: 0x0004EA05 File Offset: 0x0004CC05
	private void OnResClick(Resolution res)
	{
		this.m_selectedRes = res;
		this.m_resDialog.SetActive(false);
		this.ToggleNavigation(true);
	}

	// Token: 0x06000A40 RID: 2624 RVA: 0x0004EA21 File Offset: 0x0004CC21
	public void OnApplyMode()
	{
		this.ApplyMode();
		this.ShowResSwitchCountdown();
	}

	// Token: 0x06000A41 RID: 2625 RVA: 0x0004EA30 File Offset: 0x0004CC30
	private void ApplyMode()
	{
		if (Screen.width == this.m_selectedRes.width && Screen.height == this.m_selectedRes.height && this.m_fullscreenToggle.isOn == Screen.fullScreen)
		{
			return;
		}
		Screen.SetResolution(this.m_selectedRes.width, this.m_selectedRes.height, this.m_fullscreenToggle.isOn, this.m_selectedRes.refreshRate);
		this.m_modeApplied = true;
	}

	// Token: 0x06000A42 RID: 2626 RVA: 0x0004EAAC File Offset: 0x0004CCAC
	private void RevertMode()
	{
		if (!this.m_modeApplied)
		{
			return;
		}
		this.m_modeApplied = false;
		this.m_selectedRes = this.m_oldRes;
		this.m_fullscreenToggle.isOn = this.m_oldFullscreen;
		Screen.SetResolution(this.m_oldRes.width, this.m_oldRes.height, this.m_oldFullscreen, this.m_oldRes.refreshRate);
	}

	// Token: 0x06000A43 RID: 2627 RVA: 0x0004EB12 File Offset: 0x0004CD12
	private void ShowResSwitchCountdown()
	{
		this.m_resSwitchDialog.SetActive(true);
		this.m_resCountdownTimer = 5f;
		this.m_resSwitchDialog.GetComponentInChildren<Button>().Select();
		this.ToggleNavigation(false);
	}

	// Token: 0x06000A44 RID: 2628 RVA: 0x0004EB42 File Offset: 0x0004CD42
	public void OnResSwitchOK()
	{
		this.m_resSwitchDialog.SetActive(false);
		this.ToggleNavigation(true);
	}

	// Token: 0x06000A45 RID: 2629 RVA: 0x0004EB58 File Offset: 0x0004CD58
	private void UpdateResSwitch(float dt)
	{
		if (this.m_resSwitchDialog.activeSelf)
		{
			this.m_resCountdownTimer -= dt;
			this.m_resSwitchCountdown.text = Mathf.CeilToInt(this.m_resCountdownTimer).ToString();
			if (this.m_resCountdownTimer <= 0f)
			{
				this.RevertMode();
				this.m_resSwitchDialog.SetActive(false);
				this.ToggleNavigation(true);
			}
		}
	}

	// Token: 0x06000A46 RID: 2630 RVA: 0x0004EBC4 File Offset: 0x0004CDC4
	public void OnResetTutorial()
	{
		Player.ResetSeenTutorials();
	}

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x06000A47 RID: 2631 RVA: 0x0004EBCC File Offset: 0x0004CDCC
	// (remove) Token: 0x06000A48 RID: 2632 RVA: 0x0004EC04 File Offset: 0x0004CE04
	public event Action SettingsPopupDestroyed;

	// Token: 0x06000A4B RID: 2635 RVA: 0x0004ECEC File Offset: 0x0004CEEC
	[CompilerGenerated]
	private void <UpdateGamepad>g__updateResScroll|8_0()
	{
		Debug.Log("Res index " + this.m_selectedResIndex.ToString());
		if (this.m_selectedResIndex >= this.m_resObjects.Count)
		{
			this.m_selectedResIndex = this.m_resObjects.Count - 1;
		}
		this.m_resObjects[this.m_selectedResIndex].GetComponentInChildren<Button>().Select();
		this.m_resListScroll.value = 1f - (float)this.m_selectedResIndex / (float)(this.m_resObjects.Count - 1);
	}

	// Token: 0x04000C1D RID: 3101
	private static Settings m_instance;

	// Token: 0x04000C1E RID: 3102
	public static int FPSLimit = -1;

	// Token: 0x04000C1F RID: 3103
	public static bool ReduceBackgroundUsage = false;

	// Token: 0x04000C20 RID: 3104
	public static bool ContinousMusic = true;

	// Token: 0x04000C21 RID: 3105
	public static bool ReduceFlashingLights = false;

	// Token: 0x04000C22 RID: 3106
	public GameObject m_settingsPanel;

	// Token: 0x04000C23 RID: 3107
	private List<Selectable> m_navigationObjects = new List<Selectable>();

	// Token: 0x04000C24 RID: 3108
	private TabHandler m_tabHandler;

	// Token: 0x04000C25 RID: 3109
	private bool m_navigationEnabled = true;

	// Token: 0x04000C26 RID: 3110
	[Header("Inout")]
	public GameObject m_backButton;

	// Token: 0x04000C27 RID: 3111
	public GameObject m_okButton;

	// Token: 0x04000C28 RID: 3112
	public Slider m_sensitivitySlider;

	// Token: 0x04000C29 RID: 3113
	public Slider m_gamepadSensitivitySlider;

	// Token: 0x04000C2A RID: 3114
	public Toggle m_invertMouse;

	// Token: 0x04000C2B RID: 3115
	public Toggle m_gamepadEnabled;

	// Token: 0x04000C2C RID: 3116
	public Toggle m_alternativeGlyphs;

	// Token: 0x04000C2D RID: 3117
	public Toggle m_swapTriggers;

	// Token: 0x04000C2E RID: 3118
	public GameObject m_bindDialog;

	// Token: 0x04000C2F RID: 3119
	public List<Settings.KeySetting> m_keys = new List<Settings.KeySetting>();

	// Token: 0x04000C30 RID: 3120
	[Header("Gamepad")]
	public GameObject m_gamepadRoot;

	// Token: 0x04000C31 RID: 3121
	private bool m_gamepadRootWasVisible;

	// Token: 0x04000C32 RID: 3122
	[Header("Misc")]
	public Toggle m_cameraShake;

	// Token: 0x04000C33 RID: 3123
	public Toggle m_shipCameraTilt;

	// Token: 0x04000C34 RID: 3124
	public Toggle m_reduceBGUsage;

	// Token: 0x04000C35 RID: 3125
	public Toggle m_reduceFlashingLights;

	// Token: 0x04000C36 RID: 3126
	public Toggle m_quickPieceSelect;

	// Token: 0x04000C37 RID: 3127
	public Toggle m_tutorialsEnabled;

	// Token: 0x04000C38 RID: 3128
	public Toggle m_showKeyHints;

	// Token: 0x04000C39 RID: 3129
	public Slider m_guiScaleSlider;

	// Token: 0x04000C3A RID: 3130
	public Text m_guiScaleText;

	// Token: 0x04000C3B RID: 3131
	public Slider m_autoBackups;

	// Token: 0x04000C3C RID: 3132
	public Text m_autoBackupsText;

	// Token: 0x04000C3D RID: 3133
	public Text m_language;

	// Token: 0x04000C3E RID: 3134
	public Button m_resetTutorial;

	// Token: 0x04000C3F RID: 3135
	[Header("Audio")]
	public Slider m_volumeSlider;

	// Token: 0x04000C40 RID: 3136
	public Slider m_sfxVolumeSlider;

	// Token: 0x04000C41 RID: 3137
	public Slider m_musicVolumeSlider;

	// Token: 0x04000C42 RID: 3138
	public Toggle m_continousMusic;

	// Token: 0x04000C43 RID: 3139
	public AudioMixer m_masterMixer;

	// Token: 0x04000C44 RID: 3140
	[Header("Graphics")]
	public Toggle m_dofToggle;

	// Token: 0x04000C45 RID: 3141
	public Toggle m_vsyncToggle;

	// Token: 0x04000C46 RID: 3142
	public Toggle m_bloomToggle;

	// Token: 0x04000C47 RID: 3143
	public Toggle m_ssaoToggle;

	// Token: 0x04000C48 RID: 3144
	public Toggle m_sunshaftsToggle;

	// Token: 0x04000C49 RID: 3145
	public Toggle m_aaToggle;

	// Token: 0x04000C4A RID: 3146
	public Toggle m_caToggle;

	// Token: 0x04000C4B RID: 3147
	public Toggle m_motionblurToggle;

	// Token: 0x04000C4C RID: 3148
	public Toggle m_tesselationToggle;

	// Token: 0x04000C4D RID: 3149
	public Toggle m_distantShadowsToggle;

	// Token: 0x04000C4E RID: 3150
	public Toggle m_softPartToggle;

	// Token: 0x04000C4F RID: 3151
	public Toggle m_fullscreenToggle;

	// Token: 0x04000C50 RID: 3152
	public Slider m_shadowQuality;

	// Token: 0x04000C51 RID: 3153
	public Text m_shadowQualityText;

	// Token: 0x04000C52 RID: 3154
	public Slider m_lod;

	// Token: 0x04000C53 RID: 3155
	public Text m_lodText;

	// Token: 0x04000C54 RID: 3156
	public Slider m_lights;

	// Token: 0x04000C55 RID: 3157
	public Text m_lightsText;

	// Token: 0x04000C56 RID: 3158
	public Slider m_vegetation;

	// Token: 0x04000C57 RID: 3159
	public Text m_vegetationText;

	// Token: 0x04000C58 RID: 3160
	public Slider m_pointLights;

	// Token: 0x04000C59 RID: 3161
	public Text m_pointLightsText;

	// Token: 0x04000C5A RID: 3162
	public Slider m_pointLightShadows;

	// Token: 0x04000C5B RID: 3163
	public Text m_pointLightShadowsText;

	// Token: 0x04000C5C RID: 3164
	public Slider m_fpsLimit;

	// Token: 0x04000C5D RID: 3165
	public Text m_fpsLimitText;

	// Token: 0x04000C5E RID: 3166
	public static int[] m_fpsLimits = new int[]
	{
		30,
		60,
		75,
		90,
		100,
		120,
		144,
		165,
		200,
		240,
		-1
	};

	// Token: 0x04000C5F RID: 3167
	public Text m_resButtonText;

	// Token: 0x04000C60 RID: 3168
	public GameObject m_resDialog;

	// Token: 0x04000C61 RID: 3169
	public GameObject m_resListElement;

	// Token: 0x04000C62 RID: 3170
	public RectTransform m_resListRoot;

	// Token: 0x04000C63 RID: 3171
	public Scrollbar m_resListScroll;

	// Token: 0x04000C64 RID: 3172
	public float m_resListSpace = 20f;

	// Token: 0x04000C65 RID: 3173
	public GameObject m_resSwitchDialog;

	// Token: 0x04000C66 RID: 3174
	public Text m_resSwitchCountdown;

	// Token: 0x04000C67 RID: 3175
	public int m_minResWidth = 1280;

	// Token: 0x04000C68 RID: 3176
	public int m_minResHeight = 720;

	// Token: 0x04000C69 RID: 3177
	private string m_languageKey = "";

	// Token: 0x04000C6A RID: 3178
	private bool m_oldFullscreen;

	// Token: 0x04000C6B RID: 3179
	private Resolution m_oldRes;

	// Token: 0x04000C6C RID: 3180
	private Resolution m_selectedRes;

	// Token: 0x04000C6D RID: 3181
	private KeyCode m_toggleNavKeyPressed;

	// Token: 0x04000C6E RID: 3182
	private Selectable m_lastSelected;

	// Token: 0x04000C6F RID: 3183
	private List<GameObject> m_resObjects = new List<GameObject>();

	// Token: 0x04000C70 RID: 3184
	private List<Resolution> m_resolutions = new List<Resolution>();

	// Token: 0x04000C71 RID: 3185
	private float m_resListBaseSize;

	// Token: 0x04000C72 RID: 3186
	private int m_selectedResIndex;

	// Token: 0x04000C73 RID: 3187
	private bool m_modeApplied;

	// Token: 0x04000C74 RID: 3188
	private float m_resCountdownTimer = 1f;

	// Token: 0x04000C75 RID: 3189
	[Header("Tabs")]
	public Button ControlsTab;

	// Token: 0x04000C76 RID: 3190
	public Button AudioTab;

	// Token: 0x04000C77 RID: 3191
	public Button GraphicsTab;

	// Token: 0x04000C78 RID: 3192
	public Button MiscTab;

	// Token: 0x04000C79 RID: 3193
	public Button ConsoleGameplayTab;

	// Token: 0x04000C7A RID: 3194
	public Button ConsoleControlsTab;

	// Token: 0x04000C7B RID: 3195
	public Button ConsoleAudioTab;

	// Token: 0x04000C7C RID: 3196
	public Button ConsoleGraphicsTab;

	// Token: 0x04000C7D RID: 3197
	public Button ConsoleAccessabilityTab;

	// Token: 0x04000C7E RID: 3198
	public RectTransform ControlsPage;

	// Token: 0x04000C7F RID: 3199
	public RectTransform AudioPage;

	// Token: 0x04000C80 RID: 3200
	public RectTransform GraphicsPage;

	// Token: 0x04000C81 RID: 3201
	public RectTransform MiscPage;

	// Token: 0x04000C82 RID: 3202
	public RectTransform ConsoleGameplayPage;

	// Token: 0x04000C83 RID: 3203
	public RectTransform ConsoleControlsPage;

	// Token: 0x04000C84 RID: 3204
	public RectTransform ConsoleAudioPage;

	// Token: 0x04000C85 RID: 3205
	public RectTransform ConsoleGraphicsPage;

	// Token: 0x04000C86 RID: 3206
	public RectTransform ConsoleAccessabilityPage;

	// Token: 0x020000F9 RID: 249
	[Serializable]
	public class KeySetting
	{
		// Token: 0x04000C88 RID: 3208
		public string m_keyName = "";

		// Token: 0x04000C89 RID: 3209
		public RectTransform m_keyTransform;
	}
}
