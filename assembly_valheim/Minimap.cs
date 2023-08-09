using System;
using System.Collections.Generic;
using Fishlabs;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Token: 0x020000E5 RID: 229
public class Minimap : MonoBehaviour
{
	// Token: 0x17000052 RID: 82
	// (get) Token: 0x0600093C RID: 2364 RVA: 0x00045E27 File Offset: 0x00044027
	public static Minimap instance
	{
		get
		{
			return Minimap.m_instance;
		}
	}

	// Token: 0x0600093D RID: 2365 RVA: 0x00045E2E File Offset: 0x0004402E
	private void Awake()
	{
		Minimap.m_instance = this;
		this.m_largeRoot.SetActive(false);
		this.m_smallRoot.SetActive(true);
	}

	// Token: 0x0600093E RID: 2366 RVA: 0x00045E4E File Offset: 0x0004404E
	private void OnDestroy()
	{
		Minimap.m_instance = null;
	}

	// Token: 0x0600093F RID: 2367 RVA: 0x00045E56 File Offset: 0x00044056
	public static bool IsOpen()
	{
		return Minimap.m_instance && (Minimap.m_instance.m_largeRoot.activeSelf || Minimap.m_instance.m_hiddenFrames <= 2);
	}

	// Token: 0x06000940 RID: 2368 RVA: 0x00045E89 File Offset: 0x00044089
	public static bool InTextInput()
	{
		return Minimap.m_instance && Minimap.m_instance.m_mode == Minimap.MapMode.Large && Minimap.m_instance.m_wasFocused;
	}

	// Token: 0x06000941 RID: 2369 RVA: 0x00045EB0 File Offset: 0x000440B0
	private void Start()
	{
		this.m_mapTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RGB24, false);
		this.m_mapTexture.name = "_Minimap m_mapTexture";
		this.m_mapTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_forestMaskTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RGBA32, false);
		this.m_forestMaskTexture.name = "_Minimap m_forestMaskTexture";
		this.m_forestMaskTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_heightTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RFloat, false);
		this.m_heightTexture.name = "_Minimap m_heightTexture";
		this.m_heightTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_fogTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RGBA32, false);
		this.m_fogTexture.name = "_Minimap m_fogTexture";
		this.m_fogTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_explored = new bool[this.m_textureSize * this.m_textureSize];
		this.m_exploredOthers = new bool[this.m_textureSize * this.m_textureSize];
		this.m_mapImageLarge.material = UnityEngine.Object.Instantiate<Material>(this.m_mapImageLarge.material);
		this.m_mapImageSmall.material = UnityEngine.Object.Instantiate<Material>(this.m_mapImageSmall.material);
		this.m_mapSmallShader = this.m_mapImageSmall.material;
		this.m_mapLargeShader = this.m_mapImageLarge.material;
		this.m_mapLargeShader.SetTexture("_MainTex", this.m_mapTexture);
		this.m_mapLargeShader.SetTexture("_MaskTex", this.m_forestMaskTexture);
		this.m_mapLargeShader.SetTexture("_HeightTex", this.m_heightTexture);
		this.m_mapLargeShader.SetTexture("_FogTex", this.m_fogTexture);
		this.m_mapSmallShader.SetTexture("_MainTex", this.m_mapTexture);
		this.m_mapSmallShader.SetTexture("_MaskTex", this.m_forestMaskTexture);
		this.m_mapSmallShader.SetTexture("_HeightTex", this.m_heightTexture);
		this.m_mapSmallShader.SetTexture("_FogTex", this.m_fogTexture);
		this.m_nameInput.gameObject.SetActive(false);
		UIInputHandler component = this.m_mapImageLarge.GetComponent<UIInputHandler>();
		component.m_onRightClick = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightClick, new Action<UIInputHandler>(this.OnMapRightClick));
		component.m_onMiddleClick = (Action<UIInputHandler>)Delegate.Combine(component.m_onMiddleClick, new Action<UIInputHandler>(this.OnMapMiddleClick));
		component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(this.OnMapLeftDown));
		component.m_onLeftUp = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftUp, new Action<UIInputHandler>(this.OnMapLeftUp));
		this.m_visibleIconTypes = new bool[Enum.GetValues(typeof(Minimap.PinType)).Length];
		for (int i = 0; i < this.m_visibleIconTypes.Length; i++)
		{
			this.m_visibleIconTypes[i] = true;
		}
		this.m_selectedIcons[Minimap.PinType.Death] = this.m_selectedIconDeath;
		this.m_selectedIcons[Minimap.PinType.Boss] = this.m_selectedIconBoss;
		this.m_selectedIcons[Minimap.PinType.Icon0] = this.m_selectedIcon0;
		this.m_selectedIcons[Minimap.PinType.Icon1] = this.m_selectedIcon1;
		this.m_selectedIcons[Minimap.PinType.Icon2] = this.m_selectedIcon2;
		this.m_selectedIcons[Minimap.PinType.Icon3] = this.m_selectedIcon3;
		this.m_selectedIcons[Minimap.PinType.Icon4] = this.m_selectedIcon4;
		this.SelectIcon(Minimap.PinType.Icon0);
		this.Reset();
	}

	// Token: 0x06000942 RID: 2370 RVA: 0x00046234 File Offset: 0x00044434
	public void Reset()
	{
		Color32[] array = new Color32[this.m_textureSize * this.m_textureSize];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		this.m_fogTexture.SetPixels32(array);
		this.m_fogTexture.Apply();
		for (int j = 0; j < this.m_explored.Length; j++)
		{
			this.m_explored[j] = false;
			this.m_exploredOthers[j] = false;
		}
		this.m_sharedMapHint.gameObject.SetActive(false);
	}

	// Token: 0x06000943 RID: 2371 RVA: 0x000462D0 File Offset: 0x000444D0
	public void ResetSharedMapData()
	{
		Color[] pixels = this.m_fogTexture.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i].g = 255f;
		}
		this.m_fogTexture.SetPixels(pixels);
		this.m_fogTexture.Apply();
		for (int j = 0; j < this.m_exploredOthers.Length; j++)
		{
			this.m_exploredOthers[j] = false;
		}
		for (int k = this.m_pins.Count - 1; k >= 0; k--)
		{
			Minimap.PinData pinData = this.m_pins[k];
			if (pinData.m_ownerID != 0L)
			{
				this.DestroyPinMarker(pinData);
				this.m_pins.RemoveAt(k);
			}
		}
		this.m_sharedMapHint.gameObject.SetActive(false);
	}

	// Token: 0x06000944 RID: 2372 RVA: 0x0004638F File Offset: 0x0004458F
	public void ForceRegen()
	{
		if (WorldGenerator.instance != null)
		{
			this.GenerateWorldMap();
		}
	}

	// Token: 0x06000945 RID: 2373 RVA: 0x000463A0 File Offset: 0x000445A0
	private void Update()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			return;
		}
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		if (!this.m_hasGenerated)
		{
			if (WorldGenerator.instance == null)
			{
				return;
			}
			this.GenerateWorldMap();
			this.LoadMapData();
			this.m_hasGenerated = true;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		this.UpdateExplore(deltaTime, localPlayer);
		if (localPlayer.IsDead())
		{
			this.SetMapMode(Minimap.MapMode.None);
			return;
		}
		if (this.m_mode == Minimap.MapMode.None)
		{
			this.SetMapMode(Minimap.MapMode.Small);
		}
		if (this.m_mode == Minimap.MapMode.Large)
		{
			this.m_hiddenFrames = 0;
		}
		else
		{
			this.m_hiddenFrames++;
		}
		bool flag = (Chat.instance == null || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible() && !InventoryGui.IsVisible();
		if (flag)
		{
			if (Minimap.InTextInput())
			{
				if (ZInput.GetKeyDown(KeyCode.Escape) || ZInput.GetButton("JoyButtonB"))
				{
					this.m_namePin = null;
				}
			}
			else if (ZInput.GetButtonDown("Map") || (ZInput.GetButtonDown("JoyMap") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper")) && !ZInput.GetButton("JoyAltKeys")) || (this.m_mode == Minimap.MapMode.Large && (ZInput.GetKeyDown(KeyCode.Escape) || (ZInput.GetButtonDown("JoyMap") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper"))) || ZInput.GetButtonDown("JoyButtonB"))))
			{
				switch (this.m_mode)
				{
				case Minimap.MapMode.None:
					this.SetMapMode(Minimap.MapMode.Small);
					break;
				case Minimap.MapMode.Small:
					this.SetMapMode(Minimap.MapMode.Large);
					break;
				case Minimap.MapMode.Large:
					this.SetMapMode(Minimap.MapMode.Small);
					break;
				}
			}
		}
		if (this.m_mode == Minimap.MapMode.Large)
		{
			this.m_publicPosition.isOn = ZNet.instance.IsReferencePositionPublic();
			this.m_gamepadCrosshair.gameObject.SetActive(ZInput.IsGamepadActive());
		}
		if (this.m_showSharedMapData && this.m_sharedMapDataFade < 1f)
		{
			this.m_sharedMapDataFade = Mathf.Min(1f, this.m_sharedMapDataFade + this.m_sharedMapDataFadeRate * deltaTime);
			this.m_mapSmallShader.SetFloat("_SharedFade", this.m_sharedMapDataFade);
			this.m_mapLargeShader.SetFloat("_SharedFade", this.m_sharedMapDataFade);
		}
		else if (!this.m_showSharedMapData && this.m_sharedMapDataFade > 0f)
		{
			this.m_sharedMapDataFade = Mathf.Max(0f, this.m_sharedMapDataFade - this.m_sharedMapDataFadeRate * deltaTime);
			this.m_mapSmallShader.SetFloat("_SharedFade", this.m_sharedMapDataFade);
			this.m_mapLargeShader.SetFloat("_SharedFade", this.m_sharedMapDataFade);
		}
		this.UpdateMap(localPlayer, deltaTime, flag);
		this.UpdateDynamicPins(deltaTime);
		this.UpdatePins();
		this.UpdateBiome(localPlayer);
		this.UpdateNameInput();
	}

	// Token: 0x06000946 RID: 2374 RVA: 0x0004667C File Offset: 0x0004487C
	private void ShowPinNameInput(Vector3 pos)
	{
		this.m_namePin = this.AddPin(pos, this.m_selectedType, "", true, false, 0L);
		this.m_nameInput.text = "";
		this.m_nameInput.gameObject.SetActive(true);
		this.m_nameInput.ActivateInputField();
		if (ZInput.IsGamepadActive())
		{
			this.m_nameInput.gameObject.transform.localPosition = new Vector3(0f, -30f, 0f);
		}
		else
		{
			Vector2 vector;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_nameInput.gameObject.transform.parent.GetComponent<RectTransform>(), ZInput.mousePosition, null, out vector);
			this.m_nameInput.gameObject.transform.localPosition = new Vector3(vector.x, vector.y - 30f);
		}
		this.m_wasFocused = true;
	}

	// Token: 0x06000947 RID: 2375 RVA: 0x00046763 File Offset: 0x00044963
	private void UpdateNameInput()
	{
		if (this.m_delayTextInput < 0f)
		{
			return;
		}
		this.m_delayTextInput -= Time.deltaTime;
		this.m_wasFocused = (this.m_delayTextInput > 0f);
	}

	// Token: 0x06000948 RID: 2376 RVA: 0x00046798 File Offset: 0x00044998
	private void CreateMapNamePin(Minimap.PinData namePin, RectTransform root)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_pinNamePrefab, root);
		TMP_Text componentInChildren = gameObject.GetComponentInChildren<TMP_Text>();
		namePin.m_NamePinData.SetTextAndGameObject(gameObject, componentInChildren);
		namePin.m_NamePinData.PinNameRectTransform.SetParent(root);
	}

	// Token: 0x06000949 RID: 2377 RVA: 0x000467D8 File Offset: 0x000449D8
	public void OnPinTextEntered(string t)
	{
		string text = this.m_nameInput.text;
		if (text.Length > 0 && this.m_namePin != null)
		{
			text = text.Replace('$', ' ');
			text = text.Replace('<', ' ');
			text = text.Replace('>', ' ');
			this.m_namePin.m_name = text;
			if (!string.IsNullOrEmpty(text) && this.m_namePin.m_NamePinData == null)
			{
				this.m_namePin.m_NamePinData = new Minimap.PinNameData(this.m_namePin);
				if (this.m_namePin.m_NamePinData.PinNameGameObject == null)
				{
					this.CreateMapNamePin(this.m_namePin, this.m_pinNameRootLarge);
				}
			}
		}
		this.m_namePin = null;
		this.m_nameInput.text = "";
		this.m_nameInput.gameObject.SetActive(false);
		this.m_delayTextInput = 0.5f;
	}

	// Token: 0x0600094A RID: 2378 RVA: 0x000468C0 File Offset: 0x00044AC0
	private void UpdateMap(Player player, float dt, bool takeInput)
	{
		if (takeInput)
		{
			if (this.m_mode == Minimap.MapMode.Large)
			{
				float num = 0f;
				num += ZInput.GetAxis("Mouse ScrollWheel") * this.m_largeZoom * 2f;
				if (ZInput.GetButton("JoyButtonX"))
				{
					Vector3 viewCenterWorldPoint = this.GetViewCenterWorldPoint();
					Chat.instance.SendPing(viewCenterWorldPoint);
				}
				if (ZInput.GetButton("JoyLTrigger"))
				{
					num -= this.m_largeZoom * dt * 2f;
				}
				if (ZInput.GetButton("JoyRTrigger"))
				{
					num += this.m_largeZoom * dt * 2f;
				}
				if (ZInput.GetButtonDown("JoyDPadUp"))
				{
					Minimap.PinType pinType = Minimap.PinType.None;
					using (Dictionary<Minimap.PinType, Image>.Enumerator enumerator = this.m_selectedIcons.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<Minimap.PinType, Image> keyValuePair = enumerator.Current;
							if (keyValuePair.Key == this.m_selectedType && pinType != Minimap.PinType.None)
							{
								this.SelectIcon(pinType);
								break;
							}
							pinType = keyValuePair.Key;
						}
						goto IL_153;
					}
				}
				if (ZInput.GetButtonDown("JoyDPadDown"))
				{
					bool flag = false;
					foreach (KeyValuePair<Minimap.PinType, Image> keyValuePair2 in this.m_selectedIcons)
					{
						if (flag)
						{
							this.SelectIcon(keyValuePair2.Key);
							break;
						}
						if (keyValuePair2.Key == this.m_selectedType)
						{
							flag = true;
						}
					}
				}
				IL_153:
				if (ZInput.GetButtonDown("JoyDPadRight"))
				{
					this.ToggleIconFilter(this.m_selectedType);
				}
				if (ZInput.GetButtonUp("JoyButtonA"))
				{
					this.ShowPinNameInput(this.ScreenToWorldPoint(new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2))));
				}
				if (ZInput.GetButtonDown("JoyTabRight"))
				{
					Vector3 pos = this.ScreenToWorldPoint(new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2)));
					this.RemovePin(pos, this.m_removeRadius * (this.m_largeZoom * 2f));
					this.m_namePin = null;
				}
				if (ZInput.GetButtonDown("JoyTabLeft"))
				{
					Vector3 pos2 = this.ScreenToWorldPoint(new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2)));
					Minimap.PinData closestPin = this.GetClosestPin(pos2, this.m_removeRadius * (this.m_largeZoom * 2f));
					if (closestPin != null)
					{
						if (closestPin.m_ownerID != 0L)
						{
							closestPin.m_ownerID = 0L;
						}
						else
						{
							closestPin.m_checked = !closestPin.m_checked;
						}
					}
				}
				if (ZInput.GetButtonDown("MapZoomOut") && !Minimap.InTextInput())
				{
					num -= this.m_largeZoom * 0.5f;
				}
				if (ZInput.GetButtonDown("MapZoomIn") && !Minimap.InTextInput())
				{
					num += this.m_largeZoom * 0.5f;
				}
				this.m_largeZoom = Mathf.Clamp(this.m_largeZoom - num, this.m_minZoom, this.m_maxZoom);
			}
			else
			{
				float num2 = 0f;
				if (ZInput.GetButtonDown("MapZoomOut"))
				{
					num2 -= this.m_smallZoom * 0.5f;
				}
				if (ZInput.GetButtonDown("MapZoomIn"))
				{
					num2 += this.m_smallZoom * 0.5f;
				}
				this.m_smallZoom = Mathf.Clamp(this.m_smallZoom - num2, this.m_minZoom, this.m_maxZoom);
			}
		}
		if (this.m_mode == Minimap.MapMode.Large)
		{
			if (this.m_leftDownTime != 0f && this.m_leftDownTime > this.m_clickDuration && !this.m_dragView)
			{
				this.m_dragWorldPos = this.ScreenToWorldPoint(ZInput.mousePosition);
				this.m_dragView = true;
				this.m_namePin = null;
			}
			this.m_mapOffset.x = this.m_mapOffset.x + ZInput.GetJoyLeftStickX(true) * dt * 50000f * this.m_largeZoom * this.m_gamepadMoveSpeed;
			this.m_mapOffset.z = this.m_mapOffset.z - ZInput.GetJoyLeftStickY(true) * dt * 50000f * this.m_largeZoom * this.m_gamepadMoveSpeed;
			if (this.m_dragView)
			{
				Vector3 b = this.ScreenToWorldPoint(ZInput.mousePosition) - this.m_dragWorldPos;
				this.m_mapOffset -= b;
				this.CenterMap(player.transform.position + this.m_mapOffset);
				this.m_dragWorldPos = this.ScreenToWorldPoint(ZInput.mousePosition);
			}
			else
			{
				this.CenterMap(player.transform.position + this.m_mapOffset);
			}
		}
		else
		{
			this.CenterMap(player.transform.position);
		}
		this.UpdateWindMarker();
		this.UpdatePlayerMarker(player, Utils.GetMainCamera().transform.rotation);
	}

	// Token: 0x0600094B RID: 2379 RVA: 0x00046D54 File Offset: 0x00044F54
	public void SetMapMode(Minimap.MapMode mode)
	{
		if (mode == this.m_mode)
		{
			return;
		}
		if (Player.m_localPlayer != null && (PlayerPrefs.GetFloat("mapenabled_" + Player.m_localPlayer.GetPlayerName(), 1f) == 0f || ZoneSystem.instance.GetGlobalKey("nomap")))
		{
			mode = Minimap.MapMode.None;
		}
		this.m_mode = mode;
		switch (mode)
		{
		case Minimap.MapMode.None:
			this.m_largeRoot.SetActive(false);
			this.m_smallRoot.SetActive(false);
			return;
		case Minimap.MapMode.Small:
			this.m_largeRoot.SetActive(false);
			this.m_smallRoot.SetActive(true);
			return;
		case Minimap.MapMode.Large:
		{
			this.m_largeRoot.SetActive(true);
			this.m_smallRoot.SetActive(false);
			bool active = PlayerPrefs.GetInt("KeyHints", 1) == 1;
			foreach (GameObject gameObject in this.m_hints)
			{
				gameObject.SetActive(active);
			}
			this.m_dragView = false;
			this.m_mapOffset = Vector3.zero;
			this.m_namePin = null;
			return;
		}
		default:
			return;
		}
	}

	// Token: 0x0600094C RID: 2380 RVA: 0x00046E84 File Offset: 0x00045084
	private void CenterMap(Vector3 centerPoint)
	{
		float x;
		float y;
		this.WorldToMapPoint(centerPoint, out x, out y);
		Rect uvRect = this.m_mapImageSmall.uvRect;
		uvRect.width = this.m_smallZoom;
		uvRect.height = this.m_smallZoom;
		uvRect.center = new Vector2(x, y);
		this.m_mapImageSmall.uvRect = uvRect;
		RectTransform rectTransform = this.m_mapImageLarge.transform as RectTransform;
		float num = rectTransform.rect.width / rectTransform.rect.height;
		Rect uvRect2 = this.m_mapImageSmall.uvRect;
		uvRect2.width = this.m_largeZoom * num;
		uvRect2.height = this.m_largeZoom;
		uvRect2.center = new Vector2(x, y);
		this.m_mapImageLarge.uvRect = uvRect2;
		if (this.m_mode == Minimap.MapMode.Large)
		{
			this.m_mapLargeShader.SetFloat("_zoom", this.m_largeZoom);
			this.m_mapLargeShader.SetFloat("_pixelSize", 200f / this.m_largeZoom);
			this.m_mapLargeShader.SetVector("_mapCenter", centerPoint);
			return;
		}
		this.m_mapSmallShader.SetFloat("_zoom", this.m_smallZoom);
		this.m_mapSmallShader.SetFloat("_pixelSize", 200f / this.m_smallZoom);
		this.m_mapSmallShader.SetVector("_mapCenter", centerPoint);
	}

	// Token: 0x0600094D RID: 2381 RVA: 0x00046FED File Offset: 0x000451ED
	private void UpdateDynamicPins(float dt)
	{
		this.UpdateProfilePins();
		this.UpdateShoutPins();
		this.UpdatePingPins();
		this.UpdatePlayerPins(dt);
		this.UpdateLocationPins(dt);
		this.UpdateEventPin(dt);
	}

	// Token: 0x0600094E RID: 2382 RVA: 0x00047018 File Offset: 0x00045218
	private void UpdateProfilePins()
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		playerProfile.HaveDeathPoint();
		if (this.m_deathPin != null)
		{
			this.RemovePin(this.m_deathPin);
			this.m_deathPin = null;
		}
		if (playerProfile.HaveCustomSpawnPoint())
		{
			if (this.m_spawnPointPin == null)
			{
				this.m_spawnPointPin = this.AddPin(playerProfile.GetCustomSpawnPoint(), Minimap.PinType.Bed, "", false, false, 0L);
			}
			this.m_spawnPointPin.m_pos = playerProfile.GetCustomSpawnPoint();
			return;
		}
		if (this.m_spawnPointPin != null)
		{
			this.RemovePin(this.m_spawnPointPin);
			this.m_spawnPointPin = null;
		}
	}

	// Token: 0x0600094F RID: 2383 RVA: 0x000470AC File Offset: 0x000452AC
	private void UpdateEventPin(float dt)
	{
		if (Time.time - this.m_updateEventTime < 1f)
		{
			return;
		}
		this.m_updateEventTime = Time.time;
		RandomEvent currentRandomEvent = RandEventSystem.instance.GetCurrentRandomEvent();
		if (currentRandomEvent != null)
		{
			if (this.m_randEventAreaPin == null)
			{
				this.m_randEventAreaPin = this.AddPin(currentRandomEvent.m_pos, Minimap.PinType.EventArea, "", false, false, 0L);
				this.m_randEventAreaPin.m_worldSize = RandEventSystem.instance.m_randomEventRange * 2f;
				this.m_randEventAreaPin.m_worldSize *= 0.9f;
			}
			if (this.m_randEventPin == null)
			{
				this.m_randEventPin = this.AddPin(currentRandomEvent.m_pos, Minimap.PinType.RandomEvent, "", false, false, 0L);
				this.m_randEventPin.m_animate = true;
				this.m_randEventPin.m_doubleSize = true;
			}
			this.m_randEventAreaPin.m_pos = currentRandomEvent.m_pos;
			this.m_randEventPin.m_pos = currentRandomEvent.m_pos;
			this.m_randEventPin.m_name = Localization.instance.Localize(currentRandomEvent.GetHudText());
			return;
		}
		if (this.m_randEventPin != null)
		{
			this.RemovePin(this.m_randEventPin);
			this.m_randEventPin = null;
		}
		if (this.m_randEventAreaPin != null)
		{
			this.RemovePin(this.m_randEventAreaPin);
			this.m_randEventAreaPin = null;
		}
	}

	// Token: 0x06000950 RID: 2384 RVA: 0x000471F4 File Offset: 0x000453F4
	private void UpdateLocationPins(float dt)
	{
		this.m_updateLocationsTimer -= dt;
		if (this.m_updateLocationsTimer <= 0f)
		{
			this.m_updateLocationsTimer = 5f;
			Dictionary<Vector3, string> dictionary = new Dictionary<Vector3, string>();
			ZoneSystem.instance.GetLocationIcons(dictionary);
			bool flag = false;
			while (!flag)
			{
				flag = true;
				foreach (KeyValuePair<Vector3, Minimap.PinData> keyValuePair in this.m_locationPins)
				{
					if (!dictionary.ContainsKey(keyValuePair.Key))
					{
						ZLog.DevLog("Minimap: Removing location " + keyValuePair.Value.m_name);
						this.RemovePin(keyValuePair.Value);
						this.m_locationPins.Remove(keyValuePair.Key);
						flag = false;
						break;
					}
				}
			}
			foreach (KeyValuePair<Vector3, string> keyValuePair2 in dictionary)
			{
				if (!this.m_locationPins.ContainsKey(keyValuePair2.Key))
				{
					Sprite locationIcon = this.GetLocationIcon(keyValuePair2.Value);
					if (locationIcon)
					{
						Minimap.PinData pinData = this.AddPin(keyValuePair2.Key, Minimap.PinType.None, "", false, false, 0L);
						pinData.m_icon = locationIcon;
						pinData.m_doubleSize = true;
						this.m_locationPins.Add(keyValuePair2.Key, pinData);
						ZLog.Log("Minimap: Adding unique location " + keyValuePair2.Key.ToString());
					}
				}
			}
		}
	}

	// Token: 0x06000951 RID: 2385 RVA: 0x000473A4 File Offset: 0x000455A4
	private Sprite GetLocationIcon(string name)
	{
		foreach (Minimap.LocationSpriteData locationSpriteData in this.m_locationIcons)
		{
			if (locationSpriteData.m_name == name)
			{
				return locationSpriteData.m_icon;
			}
		}
		return null;
	}

	// Token: 0x06000952 RID: 2386 RVA: 0x0004740C File Offset: 0x0004560C
	private void UpdatePlayerPins(float dt)
	{
		this.m_tempPlayerInfo.Clear();
		ZNet.instance.GetOtherPublicPlayers(this.m_tempPlayerInfo);
		if (this.m_playerPins.Count != this.m_tempPlayerInfo.Count)
		{
			foreach (Minimap.PinData pin in this.m_playerPins)
			{
				this.RemovePin(pin);
			}
			this.m_playerPins.Clear();
			foreach (ZNet.PlayerInfo playerInfo in this.m_tempPlayerInfo)
			{
				Minimap.PinData item = this.AddPin(Vector3.zero, Minimap.PinType.Player, "", false, false, 0L);
				this.m_playerPins.Add(item);
			}
		}
		for (int i = 0; i < this.m_tempPlayerInfo.Count; i++)
		{
			Minimap.PinData pinData = this.m_playerPins[i];
			ZNet.PlayerInfo playerInfo2 = this.m_tempPlayerInfo[i];
			if (pinData.m_name == playerInfo2.m_name)
			{
				pinData.m_pos = Vector3.MoveTowards(pinData.m_pos, playerInfo2.m_position, 200f * dt);
			}
			else
			{
				pinData.m_name = playerInfo2.m_name;
				pinData.m_pos = playerInfo2.m_position;
				if (pinData.m_NamePinData == null)
				{
					pinData.m_NamePinData = new Minimap.PinNameData(pinData);
					this.CreateMapNamePin(pinData, this.m_pinNameRootLarge);
				}
			}
		}
	}

	// Token: 0x06000953 RID: 2387 RVA: 0x000475B8 File Offset: 0x000457B8
	private void UpdatePingPins()
	{
		this.m_tempShouts.Clear();
		Chat.instance.GetPingWorldTexts(this.m_tempShouts);
		if (this.m_pingPins.Count != this.m_tempShouts.Count)
		{
			foreach (Minimap.PinData pin in this.m_pingPins)
			{
				this.RemovePin(pin);
			}
			this.m_pingPins.Clear();
			foreach (Chat.WorldTextInstance worldTextInstance in this.m_tempShouts)
			{
				Minimap.PinData pinData = this.AddPin(Vector3.zero, Minimap.PinType.Ping, worldTextInstance.m_name + ": " + worldTextInstance.m_text, false, false, 0L);
				pinData.m_doubleSize = true;
				pinData.m_animate = true;
				this.m_pingPins.Add(pinData);
			}
		}
		for (int i = 0; i < this.m_tempShouts.Count; i++)
		{
			Minimap.PinData pinData2 = this.m_pingPins[i];
			Chat.WorldTextInstance worldTextInstance2 = this.m_tempShouts[i];
			pinData2.m_pos = worldTextInstance2.m_position;
			pinData2.m_name = worldTextInstance2.m_name + ": " + worldTextInstance2.m_text;
		}
	}

	// Token: 0x06000954 RID: 2388 RVA: 0x00047730 File Offset: 0x00045930
	private void UpdateShoutPins()
	{
		this.m_tempShouts.Clear();
		Chat.instance.GetShoutWorldTexts(this.m_tempShouts);
		if (this.m_shoutPins.Count != this.m_tempShouts.Count)
		{
			foreach (Minimap.PinData pin in this.m_shoutPins)
			{
				this.RemovePin(pin);
			}
			this.m_shoutPins.Clear();
			foreach (Chat.WorldTextInstance worldTextInstance in this.m_tempShouts)
			{
				Minimap.PinData pinData = this.AddPin(Vector3.zero, Minimap.PinType.Shout, worldTextInstance.m_name + ": " + worldTextInstance.m_text, false, false, 0L);
				pinData.m_doubleSize = true;
				pinData.m_animate = true;
				this.m_shoutPins.Add(pinData);
			}
		}
		for (int i = 0; i < this.m_tempShouts.Count; i++)
		{
			Minimap.PinData pinData2 = this.m_shoutPins[i];
			Chat.WorldTextInstance worldTextInstance2 = this.m_tempShouts[i];
			pinData2.m_pos = worldTextInstance2.m_position;
			pinData2.m_name = worldTextInstance2.m_name + ": " + worldTextInstance2.m_text;
		}
	}

	// Token: 0x06000955 RID: 2389 RVA: 0x000478A4 File Offset: 0x00045AA4
	private void UpdatePins()
	{
		RawImage rawImage = (this.m_mode == Minimap.MapMode.Large) ? this.m_mapImageLarge : this.m_mapImageSmall;
		float num = (this.m_mode == Minimap.MapMode.Large) ? this.m_pinSizeLarge : this.m_pinSizeSmall;
		if (this.m_mode != Minimap.MapMode.Large)
		{
			float smallZoom = this.m_smallZoom;
		}
		else
		{
			float largeZoom = this.m_largeZoom;
		}
		Color color = new Color(0.7f, 0.7f, 0.7f, 0.8f * this.m_sharedMapDataFade);
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			RectTransform rectTransform = (this.m_mode == Minimap.MapMode.Large) ? this.m_pinRootLarge : this.m_pinRootSmall;
			RectTransform root = (this.m_mode == Minimap.MapMode.Large) ? this.m_pinNameRootLarge : this.m_pinNameRootSmall;
			if (this.IsPointVisible(pinData.m_pos, rawImage) && this.m_visibleIconTypes[(int)pinData.m_type] && (this.m_sharedMapDataFade > 0f || pinData.m_ownerID == 0L))
			{
				if (pinData.m_uiElement == null || pinData.m_uiElement.parent != rectTransform)
				{
					this.DestroyPinMarker(pinData);
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_pinPrefab);
					pinData.m_iconElement = gameObject.GetComponent<Image>();
					pinData.m_iconElement.sprite = pinData.m_icon;
					pinData.m_uiElement = (gameObject.transform as RectTransform);
					pinData.m_uiElement.SetParent(rectTransform);
					float size = pinData.m_doubleSize ? (num * 2f) : num;
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
					pinData.m_checkedElement = gameObject.transform.Find("Checked").gameObject;
				}
				if (pinData.m_NamePinData != null && pinData.m_NamePinData.PinNameGameObject == null)
				{
					this.CreateMapNamePin(pinData, root);
				}
				if (pinData.m_ownerID != 0L && this.m_sharedMapHint != null)
				{
					this.m_sharedMapHint.gameObject.SetActive(true);
				}
				pinData.m_iconElement.color = ((pinData.m_ownerID != 0L) ? color : Color.white);
				if (pinData.m_NamePinData != null)
				{
					pinData.m_NamePinData.PinNameText.color = ((pinData.m_ownerID != 0L) ? color : Color.white);
				}
				float mx;
				float my;
				this.WorldToMapPoint(pinData.m_pos, out mx, out my);
				Vector2 anchoredPosition = this.MapPointToLocalGuiPos(mx, my, rawImage);
				pinData.m_uiElement.anchoredPosition = anchoredPosition;
				if (pinData.m_NamePinData != null)
				{
					pinData.m_NamePinData.PinNameRectTransform.anchoredPosition = anchoredPosition;
				}
				if (pinData.m_animate)
				{
					float num2 = pinData.m_doubleSize ? (num * 2f) : num;
					num2 *= 0.8f + Mathf.Sin(Time.time * 5f) * 0.2f;
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
				}
				if (pinData.m_worldSize > 0f)
				{
					Vector2 size2 = new Vector2(pinData.m_worldSize / this.m_pixelSize / (float)this.m_textureSize, pinData.m_worldSize / this.m_pixelSize / (float)this.m_textureSize);
					Vector2 vector = this.MapSizeToLocalGuiSize(size2, rawImage);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vector.x);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vector.y);
				}
				pinData.m_checkedElement.SetActive(pinData.m_checked);
				if (pinData.m_name.Length > 0 && this.m_mode == Minimap.MapMode.Large && this.m_largeZoom < this.m_showNamesZoom && pinData.m_NamePinData != null)
				{
					pinData.m_NamePinData.PinNameGameObject.SetActive(true);
				}
				else if (pinData.m_NamePinData != null)
				{
					pinData.m_NamePinData.PinNameGameObject.SetActive(false);
				}
			}
			else
			{
				this.DestroyPinMarker(pinData);
			}
		}
	}

	// Token: 0x06000956 RID: 2390 RVA: 0x00047CE0 File Offset: 0x00045EE0
	private void DestroyPinMarker(Minimap.PinData pin)
	{
		if (pin.m_uiElement != null)
		{
			UnityEngine.Object.Destroy(pin.m_uiElement.gameObject);
			pin.m_uiElement = null;
		}
		if (pin.m_NamePinData != null)
		{
			pin.m_NamePinData.DestroyMapMarker();
		}
	}

	// Token: 0x06000957 RID: 2391 RVA: 0x00047D1C File Offset: 0x00045F1C
	private void UpdateWindMarker()
	{
		Quaternion quaternion = Quaternion.LookRotation(EnvMan.instance.GetWindDir());
		this.m_windMarker.rotation = Quaternion.Euler(0f, 0f, -quaternion.eulerAngles.y);
	}

	// Token: 0x06000958 RID: 2392 RVA: 0x00047D60 File Offset: 0x00045F60
	private void UpdatePlayerMarker(Player player, Quaternion playerRot)
	{
		Vector3 position = player.transform.position;
		Vector3 eulerAngles = playerRot.eulerAngles;
		this.m_smallMarker.rotation = Quaternion.Euler(0f, 0f, -eulerAngles.y);
		if (this.m_mode == Minimap.MapMode.Large && this.IsPointVisible(position, this.m_mapImageLarge))
		{
			this.m_largeMarker.gameObject.SetActive(true);
			this.m_largeMarker.rotation = this.m_smallMarker.rotation;
			float mx;
			float my;
			this.WorldToMapPoint(position, out mx, out my);
			Vector2 anchoredPosition = this.MapPointToLocalGuiPos(mx, my, this.m_mapImageLarge);
			this.m_largeMarker.anchoredPosition = anchoredPosition;
		}
		else
		{
			this.m_largeMarker.gameObject.SetActive(false);
		}
		Ship controlledShip = player.GetControlledShip();
		if (controlledShip)
		{
			this.m_smallShipMarker.gameObject.SetActive(true);
			Vector3 eulerAngles2 = controlledShip.transform.rotation.eulerAngles;
			this.m_smallShipMarker.rotation = Quaternion.Euler(0f, 0f, -eulerAngles2.y);
			if (this.m_mode == Minimap.MapMode.Large)
			{
				this.m_largeShipMarker.gameObject.SetActive(true);
				Vector3 position2 = controlledShip.transform.position;
				float mx2;
				float my2;
				this.WorldToMapPoint(position2, out mx2, out my2);
				Vector2 anchoredPosition2 = this.MapPointToLocalGuiPos(mx2, my2, this.m_mapImageLarge);
				this.m_largeShipMarker.anchoredPosition = anchoredPosition2;
				this.m_largeShipMarker.rotation = this.m_smallShipMarker.rotation;
				return;
			}
		}
		else
		{
			this.m_smallShipMarker.gameObject.SetActive(false);
			this.m_largeShipMarker.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000959 RID: 2393 RVA: 0x00047F08 File Offset: 0x00046108
	private Vector2 MapPointToLocalGuiPos(float mx, float my, RawImage img)
	{
		Vector2 result = default(Vector2);
		result.x = (mx - img.uvRect.xMin) / img.uvRect.width;
		result.y = (my - img.uvRect.yMin) / img.uvRect.height;
		result.x *= img.rectTransform.rect.width;
		result.y *= img.rectTransform.rect.height;
		return result;
	}

	// Token: 0x0600095A RID: 2394 RVA: 0x00047FA8 File Offset: 0x000461A8
	private Vector2 MapSizeToLocalGuiSize(Vector2 size, RawImage img)
	{
		size.x /= img.uvRect.width;
		size.y /= img.uvRect.height;
		return new Vector2(size.x * img.rectTransform.rect.width, size.y * img.rectTransform.rect.height);
	}

	// Token: 0x0600095B RID: 2395 RVA: 0x00048020 File Offset: 0x00046220
	private bool IsPointVisible(Vector3 p, RawImage map)
	{
		float num;
		float num2;
		this.WorldToMapPoint(p, out num, out num2);
		return num > map.uvRect.xMin && num < map.uvRect.xMax && num2 > map.uvRect.yMin && num2 < map.uvRect.yMax;
	}

	// Token: 0x0600095C RID: 2396 RVA: 0x00048080 File Offset: 0x00046280
	public void ExploreAll()
	{
		for (int i = 0; i < this.m_textureSize; i++)
		{
			for (int j = 0; j < this.m_textureSize; j++)
			{
				this.Explore(j, i);
			}
		}
		this.m_fogTexture.Apply();
	}

	// Token: 0x0600095D RID: 2397 RVA: 0x000480C4 File Offset: 0x000462C4
	private void WorldToMapPoint(Vector3 p, out float mx, out float my)
	{
		int num = this.m_textureSize / 2;
		mx = p.x / this.m_pixelSize + (float)num;
		my = p.z / this.m_pixelSize + (float)num;
		mx /= (float)this.m_textureSize;
		my /= (float)this.m_textureSize;
	}

	// Token: 0x0600095E RID: 2398 RVA: 0x00048118 File Offset: 0x00046318
	private Vector3 MapPointToWorld(float mx, float my)
	{
		int num = this.m_textureSize / 2;
		mx *= (float)this.m_textureSize;
		my *= (float)this.m_textureSize;
		mx -= (float)num;
		my -= (float)num;
		mx *= this.m_pixelSize;
		my *= this.m_pixelSize;
		return new Vector3(mx, 0f, my);
	}

	// Token: 0x0600095F RID: 2399 RVA: 0x00048170 File Offset: 0x00046370
	private void WorldToPixel(Vector3 p, out int px, out int py)
	{
		int num = this.m_textureSize / 2;
		px = Mathf.RoundToInt(p.x / this.m_pixelSize + (float)num);
		py = Mathf.RoundToInt(p.z / this.m_pixelSize + (float)num);
	}

	// Token: 0x06000960 RID: 2400 RVA: 0x000481B4 File Offset: 0x000463B4
	private void UpdateExplore(float dt, Player player)
	{
		this.m_exploreTimer += Time.deltaTime;
		if (this.m_exploreTimer > this.m_exploreInterval)
		{
			this.m_exploreTimer = 0f;
			this.Explore(player.transform.position, this.m_exploreRadius);
		}
	}

	// Token: 0x06000961 RID: 2401 RVA: 0x00048204 File Offset: 0x00046404
	private void Explore(Vector3 p, float radius)
	{
		int num = (int)Mathf.Ceil(radius / this.m_pixelSize);
		bool flag = false;
		int num2;
		int num3;
		this.WorldToPixel(p, out num2, out num3);
		for (int i = num3 - num; i <= num3 + num; i++)
		{
			for (int j = num2 - num; j <= num2 + num; j++)
			{
				if (j >= 0 && i >= 0 && j < this.m_textureSize && i < this.m_textureSize && new Vector2((float)(j - num2), (float)(i - num3)).magnitude <= (float)num && this.Explore(j, i))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.m_fogTexture.Apply();
		}
	}

	// Token: 0x06000962 RID: 2402 RVA: 0x000482AC File Offset: 0x000464AC
	private bool Explore(int x, int y)
	{
		if (this.m_explored[y * this.m_textureSize + x])
		{
			return false;
		}
		Color pixel = this.m_fogTexture.GetPixel(x, y);
		pixel.r = 0f;
		this.m_fogTexture.SetPixel(x, y, pixel);
		this.m_explored[y * this.m_textureSize + x] = true;
		return true;
	}

	// Token: 0x06000963 RID: 2403 RVA: 0x0004830C File Offset: 0x0004650C
	private bool ExploreOthers(int x, int y)
	{
		if (this.m_exploredOthers[y * this.m_textureSize + x])
		{
			return false;
		}
		Color pixel = this.m_fogTexture.GetPixel(x, y);
		pixel.g = 0f;
		this.m_fogTexture.SetPixel(x, y, pixel);
		this.m_exploredOthers[y * this.m_textureSize + x] = true;
		if (this.m_sharedMapHint != null)
		{
			this.m_sharedMapHint.gameObject.SetActive(true);
		}
		return true;
	}

	// Token: 0x06000964 RID: 2404 RVA: 0x00048388 File Offset: 0x00046588
	private bool IsExplored(Vector3 worldPos)
	{
		int num;
		int num2;
		this.WorldToPixel(worldPos, out num, out num2);
		return num >= 0 && num < this.m_textureSize && num2 >= 0 && num2 < this.m_textureSize && (this.m_explored[num2 * this.m_textureSize + num] || this.m_exploredOthers[num2 * this.m_textureSize + num]);
	}

	// Token: 0x06000965 RID: 2405 RVA: 0x000483E2 File Offset: 0x000465E2
	private float GetHeight(int x, int y)
	{
		return this.m_heightTexture.GetPixel(x, y).r;
	}

	// Token: 0x06000966 RID: 2406 RVA: 0x000483F8 File Offset: 0x000465F8
	private void GenerateWorldMap()
	{
		int num = this.m_textureSize / 2;
		float num2 = this.m_pixelSize / 2f;
		Color32[] array = new Color32[this.m_textureSize * this.m_textureSize];
		Color32[] array2 = new Color32[this.m_textureSize * this.m_textureSize];
		Color[] array3 = new Color[this.m_textureSize * this.m_textureSize];
		for (int i = 0; i < this.m_textureSize; i++)
		{
			for (int j = 0; j < this.m_textureSize; j++)
			{
				float wx = (float)(j - num) * this.m_pixelSize + num2;
				float wy = (float)(i - num) * this.m_pixelSize + num2;
				Heightmap.Biome biome = WorldGenerator.instance.GetBiome(wx, wy);
				Color color;
				float biomeHeight = WorldGenerator.instance.GetBiomeHeight(biome, wx, wy, out color, false);
				array[i * this.m_textureSize + j] = this.GetPixelColor(biome);
				array2[i * this.m_textureSize + j] = this.GetMaskColor(wx, wy, biomeHeight, biome);
				array3[i * this.m_textureSize + j] = new Color(biomeHeight, 0f, 0f);
			}
		}
		this.m_forestMaskTexture.SetPixels32(array2);
		this.m_forestMaskTexture.Apply();
		this.m_mapTexture.SetPixels32(array);
		this.m_mapTexture.Apply();
		this.m_heightTexture.SetPixels(array3);
		this.m_heightTexture.Apply();
	}

	// Token: 0x06000967 RID: 2407 RVA: 0x00048580 File Offset: 0x00046780
	private Color GetMaskColor(float wx, float wy, float height, Heightmap.Biome biome)
	{
		if (height < ZoneSystem.instance.m_waterLevel)
		{
			return this.noForest;
		}
		if (biome == Heightmap.Biome.Meadows)
		{
			if (!WorldGenerator.InForest(new Vector3(wx, 0f, wy)))
			{
				return this.noForest;
			}
			return this.forest;
		}
		else if (biome == Heightmap.Biome.Plains)
		{
			if (WorldGenerator.GetForestFactor(new Vector3(wx, 0f, wy)) >= 0.8f)
			{
				return this.noForest;
			}
			return this.forest;
		}
		else
		{
			if (biome == Heightmap.Biome.BlackForest)
			{
				return this.forest;
			}
			if (biome == Heightmap.Biome.Mistlands)
			{
				float forestFactor = WorldGenerator.GetForestFactor(new Vector3(wx, 0f, wy));
				return new Color(0f, 1f - Utils.SmoothStep(1.1f, 1.3f, forestFactor), 0f, 0f);
			}
			return this.noForest;
		}
	}

	// Token: 0x06000968 RID: 2408 RVA: 0x0004864C File Offset: 0x0004684C
	private Color GetPixelColor(Heightmap.Biome biome)
	{
		if (biome <= Heightmap.Biome.Plains)
		{
			switch (biome)
			{
			case Heightmap.Biome.Meadows:
				return this.m_meadowsColor;
			case Heightmap.Biome.Swamp:
				return this.m_swampColor;
			case Heightmap.Biome.Meadows | Heightmap.Biome.Swamp:
				break;
			case Heightmap.Biome.Mountain:
				return this.m_mountainColor;
			default:
				if (biome == Heightmap.Biome.BlackForest)
				{
					return this.m_blackforestColor;
				}
				if (biome == Heightmap.Biome.Plains)
				{
					return this.m_heathColor;
				}
				break;
			}
		}
		else if (biome <= Heightmap.Biome.DeepNorth)
		{
			if (biome == Heightmap.Biome.AshLands)
			{
				return this.m_ashlandsColor;
			}
			if (biome == Heightmap.Biome.DeepNorth)
			{
				return this.m_deepnorthColor;
			}
		}
		else
		{
			if (biome == Heightmap.Biome.Ocean)
			{
				return Color.white;
			}
			if (biome == Heightmap.Biome.Mistlands)
			{
				return this.m_mistlandsColor;
			}
		}
		return Color.white;
	}

	// Token: 0x06000969 RID: 2409 RVA: 0x000486FC File Offset: 0x000468FC
	private void LoadMapData()
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		if (playerProfile.GetMapData() != null)
		{
			this.SetMapData(playerProfile.GetMapData());
		}
	}

	// Token: 0x0600096A RID: 2410 RVA: 0x00048728 File Offset: 0x00046928
	public void SaveMapData()
	{
		Game.instance.GetPlayerProfile().SetMapData(this.GetMapData());
	}

	// Token: 0x0600096B RID: 2411 RVA: 0x00048740 File Offset: 0x00046940
	private byte[] GetMapData()
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(Minimap.MAPVERSION);
		ZPackage zpackage2 = new ZPackage();
		zpackage2.Write(this.m_textureSize);
		for (int i = 0; i < this.m_explored.Length; i++)
		{
			zpackage2.Write(this.m_explored[i]);
		}
		for (int j = 0; j < this.m_explored.Length; j++)
		{
			zpackage2.Write(this.m_exploredOthers[j]);
		}
		int num = 0;
		using (List<Minimap.PinData>.Enumerator enumerator = this.m_pins.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_save)
				{
					num++;
				}
			}
		}
		zpackage2.Write(num);
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_save)
			{
				zpackage2.Write(pinData.m_name);
				zpackage2.Write(pinData.m_pos);
				zpackage2.Write((int)pinData.m_type);
				zpackage2.Write(pinData.m_checked);
				zpackage2.Write(pinData.m_ownerID);
			}
		}
		zpackage2.Write(ZNet.instance.IsReferencePositionPublic());
		ZLog.Log("Uncompressed size " + zpackage2.Size().ToString());
		zpackage.WriteCompressed(zpackage2);
		ZLog.Log("Compressed size " + zpackage.Size().ToString());
		return zpackage.GetArray();
	}

	// Token: 0x0600096C RID: 2412 RVA: 0x000488EC File Offset: 0x00046AEC
	private void SetMapData(byte[] data)
	{
		ZPackage zpackage = new ZPackage(data);
		int num = zpackage.ReadInt();
		if (num >= 7)
		{
			ZLog.Log("Unpacking compressed mapdata " + zpackage.Size().ToString());
			zpackage = zpackage.ReadCompressedPackage();
		}
		int num2 = zpackage.ReadInt();
		if (this.m_textureSize != num2)
		{
			string str = "Missmatching mapsize ";
			Texture2D mapTexture = this.m_mapTexture;
			ZLog.LogWarning(str + ((mapTexture != null) ? mapTexture.ToString() : null) + " vs " + num2.ToString());
			return;
		}
		this.Reset();
		for (int i = 0; i < this.m_explored.Length; i++)
		{
			if (zpackage.ReadBool())
			{
				int x = i % num2;
				int y = i / num2;
				this.Explore(x, y);
			}
		}
		if (num >= 5)
		{
			for (int j = 0; j < this.m_exploredOthers.Length; j++)
			{
				if (zpackage.ReadBool())
				{
					int x2 = j % num2;
					int y2 = j / num2;
					this.ExploreOthers(x2, y2);
				}
			}
		}
		if (num >= 2)
		{
			int num3 = zpackage.ReadInt();
			this.ClearPins();
			for (int k = 0; k < num3; k++)
			{
				string name = zpackage.ReadString();
				Vector3 pos = zpackage.ReadVector3();
				Minimap.PinType type = (Minimap.PinType)zpackage.ReadInt();
				bool isChecked = num >= 3 && zpackage.ReadBool();
				long ownerID = (num >= 6) ? zpackage.ReadLong() : 0L;
				this.AddPin(pos, type, name, true, isChecked, ownerID);
			}
		}
		if (num >= 4)
		{
			bool publicReferencePosition = zpackage.ReadBool();
			ZNet.instance.SetPublicReferencePosition(publicReferencePosition);
		}
		this.m_fogTexture.Apply();
	}

	// Token: 0x0600096D RID: 2413 RVA: 0x00048A74 File Offset: 0x00046C74
	public bool RemovePin(Vector3 pos, float radius)
	{
		Minimap.PinData closestPin = this.GetClosestPin(pos, radius);
		if (closestPin != null)
		{
			this.RemovePin(closestPin);
			return true;
		}
		return false;
	}

	// Token: 0x0600096E RID: 2414 RVA: 0x00048A98 File Offset: 0x00046C98
	private bool HavePinInRange(Vector3 pos, float radius)
	{
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_save && Utils.DistanceXZ(pos, pinData.m_pos) < radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600096F RID: 2415 RVA: 0x00048B04 File Offset: 0x00046D04
	private Minimap.PinData GetClosestPin(Vector3 pos, float radius)
	{
		Minimap.PinData pinData = null;
		float num = 999999f;
		foreach (Minimap.PinData pinData2 in this.m_pins)
		{
			if (pinData2.m_save && pinData2.m_uiElement && pinData2.m_uiElement.gameObject.activeInHierarchy)
			{
				float num2 = Utils.DistanceXZ(pos, pinData2.m_pos);
				if (num2 < radius && (num2 < num || pinData == null))
				{
					pinData = pinData2;
					num = num2;
				}
			}
		}
		return pinData;
	}

	// Token: 0x06000970 RID: 2416 RVA: 0x00048BA0 File Offset: 0x00046DA0
	public void RemovePin(Minimap.PinData pin)
	{
		this.DestroyPinMarker(pin);
		this.m_pins.Remove(pin);
	}

	// Token: 0x06000971 RID: 2417 RVA: 0x00048BB6 File Offset: 0x00046DB6
	public void ShowPointOnMap(Vector3 point)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		this.SetMapMode(Minimap.MapMode.Large);
		this.m_mapOffset = point - Player.m_localPlayer.transform.position;
	}

	// Token: 0x06000972 RID: 2418 RVA: 0x00048BE8 File Offset: 0x00046DE8
	public bool DiscoverLocation(Vector3 pos, Minimap.PinType type, string name, bool showMap)
	{
		if (Player.m_localPlayer == null)
		{
			return false;
		}
		if (this.HaveSimilarPin(pos, type, name, true))
		{
			if (showMap)
			{
				Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_pin_exist", 0, null);
				this.ShowPointOnMap(pos);
			}
			return false;
		}
		Sprite sprite = this.GetSprite(type);
		this.AddPin(pos, type, name, true, false, 0L);
		if (showMap)
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_pin_added: " + name, 0, sprite);
			this.ShowPointOnMap(pos);
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_pin_added: " + name, 0, sprite);
		}
		return true;
	}

	// Token: 0x06000973 RID: 2419 RVA: 0x00048C84 File Offset: 0x00046E84
	private bool HaveSimilarPin(Vector3 pos, Minimap.PinType type, string name, bool save)
	{
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_name == name && pinData.m_type == type && pinData.m_save == save && Utils.DistanceXZ(pos, pinData.m_pos) < 1f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000974 RID: 2420 RVA: 0x00048D0C File Offset: 0x00046F0C
	public Minimap.PinData AddPin(Vector3 pos, Minimap.PinType type, string name, bool save, bool isChecked, long ownerID = 0L)
	{
		if (type >= (Minimap.PinType)this.m_visibleIconTypes.Length || type < Minimap.PinType.Icon0)
		{
			ZLog.LogWarning(string.Format("Trying to add invalid pin type: {0}", type));
			type = Minimap.PinType.Icon3;
		}
		if (name == null)
		{
			name = "";
		}
		Minimap.PinData pinData = new Minimap.PinData();
		pinData.m_type = type;
		pinData.m_name = name;
		pinData.m_pos = pos;
		pinData.m_icon = this.GetSprite(type);
		pinData.m_save = save;
		pinData.m_checked = isChecked;
		pinData.m_ownerID = ownerID;
		if (!string.IsNullOrEmpty(pinData.m_name))
		{
			pinData.m_NamePinData = new Minimap.PinNameData(pinData);
		}
		this.m_pins.Add(pinData);
		if (type < (Minimap.PinType)this.m_visibleIconTypes.Length && !this.m_visibleIconTypes[(int)type])
		{
			this.ToggleIconFilter(type);
		}
		return pinData;
	}

	// Token: 0x06000975 RID: 2421 RVA: 0x00048DCC File Offset: 0x00046FCC
	private Sprite GetSprite(Minimap.PinType type)
	{
		if (type == Minimap.PinType.None)
		{
			return null;
		}
		return this.m_icons.Find((Minimap.SpriteData x) => x.m_name == type).m_icon;
	}

	// Token: 0x06000976 RID: 2422 RVA: 0x00048E10 File Offset: 0x00047010
	private Vector3 GetViewCenterWorldPoint()
	{
		Rect uvRect = this.m_mapImageLarge.uvRect;
		float mx = uvRect.xMin + 0.5f * uvRect.width;
		float my = uvRect.yMin + 0.5f * uvRect.height;
		return this.MapPointToWorld(mx, my);
	}

	// Token: 0x06000977 RID: 2423 RVA: 0x00048E60 File Offset: 0x00047060
	private Vector3 ScreenToWorldPoint(Vector3 mousePos)
	{
		Vector2 screenPoint = mousePos;
		RectTransform rectTransform = this.m_mapImageLarge.transform as RectTransform;
		Vector2 point;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out point))
		{
			Vector2 vector = Rect.PointToNormalized(rectTransform.rect, point);
			Rect uvRect = this.m_mapImageLarge.uvRect;
			float mx = uvRect.xMin + vector.x * uvRect.width;
			float my = uvRect.yMin + vector.y * uvRect.height;
			return this.MapPointToWorld(mx, my);
		}
		return Vector3.zero;
	}

	// Token: 0x06000978 RID: 2424 RVA: 0x00048EEC File Offset: 0x000470EC
	private void OnMapLeftDown(UIInputHandler handler)
	{
		if (Time.time - this.m_leftClickTime < 0.3f)
		{
			this.OnMapDblClick();
			this.m_leftClickTime = 0f;
			this.m_leftDownTime = 0f;
			return;
		}
		this.m_leftClickTime = Time.time;
		this.m_leftDownTime = Time.time;
	}

	// Token: 0x06000979 RID: 2425 RVA: 0x00048F3F File Offset: 0x0004713F
	private void OnMapLeftUp(UIInputHandler handler)
	{
		if (this.m_leftDownTime != 0f)
		{
			if (Time.time - this.m_leftDownTime < this.m_clickDuration)
			{
				this.OnMapLeftClick();
			}
			this.m_leftDownTime = 0f;
		}
		this.m_dragView = false;
	}

	// Token: 0x0600097A RID: 2426 RVA: 0x00048F7A File Offset: 0x0004717A
	public void OnMapDblClick()
	{
		if (this.m_selectedType == Minimap.PinType.Death)
		{
			return;
		}
		this.ShowPinNameInput(this.ScreenToWorldPoint(ZInput.mousePosition));
	}

	// Token: 0x0600097B RID: 2427 RVA: 0x00048F98 File Offset: 0x00047198
	public void OnMapLeftClick()
	{
		ZLog.Log("Left click");
		Vector3 pos = this.ScreenToWorldPoint(ZInput.mousePosition);
		Minimap.PinData closestPin = this.GetClosestPin(pos, this.m_removeRadius * (this.m_largeZoom * 2f));
		if (closestPin != null)
		{
			if (closestPin.m_ownerID != 0L)
			{
				closestPin.m_ownerID = 0L;
				return;
			}
			closestPin.m_checked = !closestPin.m_checked;
		}
	}

	// Token: 0x0600097C RID: 2428 RVA: 0x00048FFC File Offset: 0x000471FC
	public void OnMapMiddleClick(UIInputHandler handler)
	{
		Vector3 vector = this.ScreenToWorldPoint(ZInput.mousePosition);
		Chat.instance.SendPing(vector);
		if (Player.m_debugMode && global::Console.instance != null && global::Console.instance.IsCheatsEnabled() && ZInput.GetKey(KeyCode.LeftControl))
		{
			Vector3 vector2 = new Vector3(vector.x, Player.m_localPlayer.transform.position.y, vector.z);
			float val;
			Heightmap.GetHeight(vector2, out val);
			vector2.y = Math.Max(0f, val);
			Player.m_localPlayer.TeleportTo(vector2, Player.m_localPlayer.transform.rotation, true);
		}
	}

	// Token: 0x0600097D RID: 2429 RVA: 0x000490B0 File Offset: 0x000472B0
	public void OnMapRightClick(UIInputHandler handler)
	{
		ZLog.Log("Right click");
		Vector3 pos = this.ScreenToWorldPoint(ZInput.mousePosition);
		this.RemovePin(pos, this.m_removeRadius * (this.m_largeZoom * 2f));
		this.m_namePin = null;
	}

	// Token: 0x0600097E RID: 2430 RVA: 0x000490F5 File Offset: 0x000472F5
	public void OnPressedIcon0()
	{
		this.SelectIcon(Minimap.PinType.Icon0);
	}

	// Token: 0x0600097F RID: 2431 RVA: 0x000490FE File Offset: 0x000472FE
	public void OnPressedIcon1()
	{
		this.SelectIcon(Minimap.PinType.Icon1);
	}

	// Token: 0x06000980 RID: 2432 RVA: 0x00049107 File Offset: 0x00047307
	public void OnPressedIcon2()
	{
		this.SelectIcon(Minimap.PinType.Icon2);
	}

	// Token: 0x06000981 RID: 2433 RVA: 0x00049110 File Offset: 0x00047310
	public void OnPressedIcon3()
	{
		this.SelectIcon(Minimap.PinType.Icon3);
	}

	// Token: 0x06000982 RID: 2434 RVA: 0x00049119 File Offset: 0x00047319
	public void OnPressedIcon4()
	{
		this.SelectIcon(Minimap.PinType.Icon4);
	}

	// Token: 0x06000983 RID: 2435 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnPressedIconDeath()
	{
	}

	// Token: 0x06000984 RID: 2436 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnPressedIconBoss()
	{
	}

	// Token: 0x06000985 RID: 2437 RVA: 0x00049122 File Offset: 0x00047322
	public void OnAltPressedIcon0()
	{
		this.ToggleIconFilter(Minimap.PinType.Icon0);
	}

	// Token: 0x06000986 RID: 2438 RVA: 0x0004912B File Offset: 0x0004732B
	public void OnAltPressedIcon1()
	{
		this.ToggleIconFilter(Minimap.PinType.Icon1);
	}

	// Token: 0x06000987 RID: 2439 RVA: 0x00049134 File Offset: 0x00047334
	public void OnAltPressedIcon2()
	{
		this.ToggleIconFilter(Minimap.PinType.Icon2);
	}

	// Token: 0x06000988 RID: 2440 RVA: 0x0004913D File Offset: 0x0004733D
	public void OnAltPressedIcon3()
	{
		this.ToggleIconFilter(Minimap.PinType.Icon3);
	}

	// Token: 0x06000989 RID: 2441 RVA: 0x00049146 File Offset: 0x00047346
	public void OnAltPressedIcon4()
	{
		this.ToggleIconFilter(Minimap.PinType.Icon4);
	}

	// Token: 0x0600098A RID: 2442 RVA: 0x0004914F File Offset: 0x0004734F
	public void OnAltPressedIconDeath()
	{
		this.ToggleIconFilter(Minimap.PinType.Death);
	}

	// Token: 0x0600098B RID: 2443 RVA: 0x00049158 File Offset: 0x00047358
	public void OnAltPressedIconBoss()
	{
		this.ToggleIconFilter(Minimap.PinType.Boss);
	}

	// Token: 0x0600098C RID: 2444 RVA: 0x00049162 File Offset: 0x00047362
	public void OnTogglePublicPosition()
	{
		ZNet.instance.SetPublicReferencePosition(this.m_publicPosition.isOn);
	}

	// Token: 0x0600098D RID: 2445 RVA: 0x00049179 File Offset: 0x00047379
	public void OnToggleSharedMapData()
	{
		this.m_showSharedMapData = !this.m_showSharedMapData;
	}

	// Token: 0x0600098E RID: 2446 RVA: 0x0004918C File Offset: 0x0004738C
	private void SelectIcon(Minimap.PinType type)
	{
		this.m_selectedType = type;
		foreach (KeyValuePair<Minimap.PinType, Image> keyValuePair in this.m_selectedIcons)
		{
			keyValuePair.Value.enabled = (keyValuePair.Key == type);
		}
	}

	// Token: 0x0600098F RID: 2447 RVA: 0x000491F8 File Offset: 0x000473F8
	private void ToggleIconFilter(Minimap.PinType type)
	{
		this.m_visibleIconTypes[(int)type] = !this.m_visibleIconTypes[(int)type];
		foreach (KeyValuePair<Minimap.PinType, Image> keyValuePair in this.m_selectedIcons)
		{
			keyValuePair.Value.transform.parent.GetComponent<Image>().color = (this.m_visibleIconTypes[(int)keyValuePair.Key] ? Color.white : Color.gray);
		}
	}

	// Token: 0x06000990 RID: 2448 RVA: 0x00049290 File Offset: 0x00047490
	private void ClearPins()
	{
		foreach (Minimap.PinData pin in this.m_pins)
		{
			this.DestroyPinMarker(pin);
		}
		this.m_pins.Clear();
		this.m_deathPin = null;
	}

	// Token: 0x06000991 RID: 2449 RVA: 0x000492F8 File Offset: 0x000474F8
	private void UpdateBiome(Player player)
	{
		if (this.m_mode != Minimap.MapMode.Large)
		{
			Heightmap.Biome currentBiome = player.GetCurrentBiome();
			if (currentBiome != this.m_biome)
			{
				this.m_biome = currentBiome;
				string text = Localization.instance.Localize("$biome_" + currentBiome.ToString().ToLower());
				this.m_biomeNameSmall.text = text;
				this.m_biomeNameLarge.text = text;
				this.m_biomeNameSmall.GetComponent<Animator>().SetTrigger("pulse");
			}
			return;
		}
		Vector3 vector = this.ScreenToWorldPoint(ZInput.IsMouseActive() ? ZInput.mousePosition : new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2)));
		if (this.IsExplored(vector))
		{
			Heightmap.Biome biome = WorldGenerator.instance.GetBiome(vector);
			string text2 = Localization.instance.Localize("$biome_" + biome.ToString().ToLower());
			this.m_biomeNameLarge.text = text2;
			return;
		}
		this.m_biomeNameLarge.text = "";
	}

	// Token: 0x06000992 RID: 2450 RVA: 0x00049404 File Offset: 0x00047604
	public byte[] GetSharedMapData(byte[] oldMapData)
	{
		List<bool> list = null;
		if (oldMapData != null)
		{
			ZPackage zpackage = new ZPackage(oldMapData);
			int version = zpackage.ReadInt();
			list = this.ReadExploredArray(zpackage, version);
		}
		ZPackage zpackage2 = new ZPackage();
		zpackage2.Write(2);
		zpackage2.Write(this.m_explored.Length);
		for (int i = 0; i < this.m_explored.Length; i++)
		{
			bool flag = this.m_exploredOthers[i] || this.m_explored[i];
			if (list != null)
			{
				flag |= list[i];
			}
			zpackage2.Write(flag);
		}
		int num = 0;
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_save && pinData.m_type != Minimap.PinType.Death)
			{
				num++;
			}
		}
		long playerID = Player.m_localPlayer.GetPlayerID();
		zpackage2.Write(num);
		foreach (Minimap.PinData pinData2 in this.m_pins)
		{
			if (pinData2.m_save && pinData2.m_type != Minimap.PinType.Death)
			{
				long data = (pinData2.m_ownerID != 0L) ? pinData2.m_ownerID : playerID;
				zpackage2.Write(data);
				zpackage2.Write(pinData2.m_name);
				zpackage2.Write(pinData2.m_pos);
				zpackage2.Write((int)pinData2.m_type);
				zpackage2.Write(pinData2.m_checked);
			}
		}
		return zpackage2.GetArray();
	}

	// Token: 0x06000993 RID: 2451 RVA: 0x000495A8 File Offset: 0x000477A8
	private List<bool> ReadExploredArray(ZPackage pkg, int version)
	{
		int num = pkg.ReadInt();
		if (num != this.m_explored.Length)
		{
			ZLog.LogWarning("Map exploration array size missmatch:" + num.ToString() + " VS " + this.m_explored.Length.ToString());
			return null;
		}
		List<bool> list = new List<bool>();
		for (int i = 0; i < this.m_textureSize; i++)
		{
			for (int j = 0; j < this.m_textureSize; j++)
			{
				bool item = pkg.ReadBool();
				list.Add(item);
			}
		}
		return list;
	}

	// Token: 0x06000994 RID: 2452 RVA: 0x00049634 File Offset: 0x00047834
	public bool AddSharedMapData(byte[] dataArray)
	{
		ZPackage zpackage = new ZPackage(dataArray);
		int num = zpackage.ReadInt();
		List<bool> list = this.ReadExploredArray(zpackage, num);
		if (list == null)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < this.m_textureSize; i++)
		{
			for (int j = 0; j < this.m_textureSize; j++)
			{
				int num2 = i * this.m_textureSize + j;
				bool flag2 = list[num2];
				bool flag3 = this.m_exploredOthers[num2] || this.m_explored[num2];
				if (flag2 != flag3 && flag2 && this.ExploreOthers(j, i))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.m_fogTexture.Apply();
		}
		bool flag4 = false;
		if (num >= 2)
		{
			long playerID = Player.m_localPlayer.GetPlayerID();
			int num3 = zpackage.ReadInt();
			for (int k = 0; k < num3; k++)
			{
				long num4 = zpackage.ReadLong();
				string name = zpackage.ReadString();
				Vector3 pos = zpackage.ReadVector3();
				Minimap.PinType type = (Minimap.PinType)zpackage.ReadInt();
				bool isChecked = zpackage.ReadBool();
				if (num4 == playerID)
				{
					num4 = 0L;
				}
				if (!this.HavePinInRange(pos, 1f))
				{
					this.AddPin(pos, type, name, true, isChecked, num4);
					flag4 = true;
				}
			}
		}
		return flag || flag4;
	}

	// Token: 0x04000B19 RID: 2841
	private Color forest = new Color(1f, 0f, 0f, 0f);

	// Token: 0x04000B1A RID: 2842
	private Color noForest = new Color(0f, 0f, 0f, 0f);

	// Token: 0x04000B1B RID: 2843
	private static int MAPVERSION = 7;

	// Token: 0x04000B1C RID: 2844
	private const int sharedMapDataVersion = 2;

	// Token: 0x04000B1D RID: 2845
	private static Minimap m_instance;

	// Token: 0x04000B1E RID: 2846
	public GameObject m_smallRoot;

	// Token: 0x04000B1F RID: 2847
	public GameObject m_largeRoot;

	// Token: 0x04000B20 RID: 2848
	public RawImage m_mapImageSmall;

	// Token: 0x04000B21 RID: 2849
	public RawImage m_mapImageLarge;

	// Token: 0x04000B22 RID: 2850
	public RectTransform m_pinRootSmall;

	// Token: 0x04000B23 RID: 2851
	public RectTransform m_pinRootLarge;

	// Token: 0x04000B24 RID: 2852
	public RectTransform m_pinNameRootSmall;

	// Token: 0x04000B25 RID: 2853
	public RectTransform m_pinNameRootLarge;

	// Token: 0x04000B26 RID: 2854
	public TMP_Text m_biomeNameSmall;

	// Token: 0x04000B27 RID: 2855
	public TMP_Text m_biomeNameLarge;

	// Token: 0x04000B28 RID: 2856
	public RectTransform m_smallShipMarker;

	// Token: 0x04000B29 RID: 2857
	public RectTransform m_largeShipMarker;

	// Token: 0x04000B2A RID: 2858
	public RectTransform m_smallMarker;

	// Token: 0x04000B2B RID: 2859
	public RectTransform m_largeMarker;

	// Token: 0x04000B2C RID: 2860
	public RectTransform m_windMarker;

	// Token: 0x04000B2D RID: 2861
	public RectTransform m_gamepadCrosshair;

	// Token: 0x04000B2E RID: 2862
	public Toggle m_publicPosition;

	// Token: 0x04000B2F RID: 2863
	public Image m_selectedIcon0;

	// Token: 0x04000B30 RID: 2864
	public Image m_selectedIcon1;

	// Token: 0x04000B31 RID: 2865
	public Image m_selectedIcon2;

	// Token: 0x04000B32 RID: 2866
	public Image m_selectedIcon3;

	// Token: 0x04000B33 RID: 2867
	public Image m_selectedIcon4;

	// Token: 0x04000B34 RID: 2868
	public Image m_selectedIconDeath;

	// Token: 0x04000B35 RID: 2869
	public Image m_selectedIconBoss;

	// Token: 0x04000B36 RID: 2870
	private Dictionary<Minimap.PinType, Image> m_selectedIcons = new Dictionary<Minimap.PinType, Image>();

	// Token: 0x04000B37 RID: 2871
	private bool[] m_visibleIconTypes;

	// Token: 0x04000B38 RID: 2872
	private bool m_showSharedMapData = true;

	// Token: 0x04000B39 RID: 2873
	public float m_sharedMapDataFadeRate = 2f;

	// Token: 0x04000B3A RID: 2874
	private float m_sharedMapDataFade;

	// Token: 0x04000B3B RID: 2875
	public GameObject m_mapSmall;

	// Token: 0x04000B3C RID: 2876
	public GameObject m_mapLarge;

	// Token: 0x04000B3D RID: 2877
	private Material m_mapSmallShader;

	// Token: 0x04000B3E RID: 2878
	private Material m_mapLargeShader;

	// Token: 0x04000B3F RID: 2879
	public GameObject m_pinPrefab;

	// Token: 0x04000B40 RID: 2880
	[SerializeField]
	private GameObject m_pinNamePrefab;

	// Token: 0x04000B41 RID: 2881
	public GuiInputField m_nameInput;

	// Token: 0x04000B42 RID: 2882
	public int m_textureSize = 256;

	// Token: 0x04000B43 RID: 2883
	public float m_pixelSize = 64f;

	// Token: 0x04000B44 RID: 2884
	public float m_minZoom = 0.01f;

	// Token: 0x04000B45 RID: 2885
	public float m_maxZoom = 1f;

	// Token: 0x04000B46 RID: 2886
	public float m_showNamesZoom = 0.5f;

	// Token: 0x04000B47 RID: 2887
	public float m_exploreInterval = 2f;

	// Token: 0x04000B48 RID: 2888
	public float m_exploreRadius = 100f;

	// Token: 0x04000B49 RID: 2889
	public float m_removeRadius = 128f;

	// Token: 0x04000B4A RID: 2890
	public float m_pinSizeSmall = 32f;

	// Token: 0x04000B4B RID: 2891
	public float m_pinSizeLarge = 48f;

	// Token: 0x04000B4C RID: 2892
	public float m_clickDuration = 0.25f;

	// Token: 0x04000B4D RID: 2893
	public List<Minimap.SpriteData> m_icons = new List<Minimap.SpriteData>();

	// Token: 0x04000B4E RID: 2894
	public List<Minimap.LocationSpriteData> m_locationIcons = new List<Minimap.LocationSpriteData>();

	// Token: 0x04000B4F RID: 2895
	public Color m_meadowsColor = new Color(0.45f, 1f, 0.43f);

	// Token: 0x04000B50 RID: 2896
	public Color m_ashlandsColor = new Color(1f, 0.2f, 0.2f);

	// Token: 0x04000B51 RID: 2897
	public Color m_blackforestColor = new Color(0f, 0.7f, 0f);

	// Token: 0x04000B52 RID: 2898
	public Color m_deepnorthColor = new Color(1f, 1f, 1f);

	// Token: 0x04000B53 RID: 2899
	public Color m_heathColor = new Color(1f, 1f, 0.2f);

	// Token: 0x04000B54 RID: 2900
	public Color m_swampColor = new Color(0.6f, 0.5f, 0.5f);

	// Token: 0x04000B55 RID: 2901
	public Color m_mountainColor = new Color(1f, 1f, 1f);

	// Token: 0x04000B56 RID: 2902
	private Color m_mistlandsColor = new Color(0.2f, 0.2f, 0.2f);

	// Token: 0x04000B57 RID: 2903
	private Minimap.PinData m_namePin;

	// Token: 0x04000B58 RID: 2904
	private Minimap.PinType m_selectedType;

	// Token: 0x04000B59 RID: 2905
	private Minimap.PinData m_deathPin;

	// Token: 0x04000B5A RID: 2906
	private Minimap.PinData m_spawnPointPin;

	// Token: 0x04000B5B RID: 2907
	private Dictionary<Vector3, Minimap.PinData> m_locationPins = new Dictionary<Vector3, Minimap.PinData>();

	// Token: 0x04000B5C RID: 2908
	private float m_updateLocationsTimer;

	// Token: 0x04000B5D RID: 2909
	private List<Minimap.PinData> m_pingPins = new List<Minimap.PinData>();

	// Token: 0x04000B5E RID: 2910
	private List<Minimap.PinData> m_shoutPins = new List<Minimap.PinData>();

	// Token: 0x04000B5F RID: 2911
	private List<Chat.WorldTextInstance> m_tempShouts = new List<Chat.WorldTextInstance>();

	// Token: 0x04000B60 RID: 2912
	private List<Minimap.PinData> m_playerPins = new List<Minimap.PinData>();

	// Token: 0x04000B61 RID: 2913
	private List<ZNet.PlayerInfo> m_tempPlayerInfo = new List<ZNet.PlayerInfo>();

	// Token: 0x04000B62 RID: 2914
	private Minimap.PinData m_randEventPin;

	// Token: 0x04000B63 RID: 2915
	private Minimap.PinData m_randEventAreaPin;

	// Token: 0x04000B64 RID: 2916
	private float m_updateEventTime;

	// Token: 0x04000B65 RID: 2917
	private bool[] m_explored;

	// Token: 0x04000B66 RID: 2918
	private bool[] m_exploredOthers;

	// Token: 0x04000B67 RID: 2919
	public GameObject m_sharedMapHint;

	// Token: 0x04000B68 RID: 2920
	public List<GameObject> m_hints;

	// Token: 0x04000B69 RID: 2921
	private List<Minimap.PinData> m_pins = new List<Minimap.PinData>();

	// Token: 0x04000B6A RID: 2922
	private Texture2D m_forestMaskTexture;

	// Token: 0x04000B6B RID: 2923
	private Texture2D m_mapTexture;

	// Token: 0x04000B6C RID: 2924
	private Texture2D m_heightTexture;

	// Token: 0x04000B6D RID: 2925
	private Texture2D m_fogTexture;

	// Token: 0x04000B6E RID: 2926
	private float m_largeZoom = 0.1f;

	// Token: 0x04000B6F RID: 2927
	private float m_smallZoom = 0.01f;

	// Token: 0x04000B70 RID: 2928
	private Heightmap.Biome m_biome;

	// Token: 0x04000B71 RID: 2929
	[HideInInspector]
	public Minimap.MapMode m_mode;

	// Token: 0x04000B72 RID: 2930
	public float m_nomapPingDistance = 50f;

	// Token: 0x04000B73 RID: 2931
	private float m_exploreTimer;

	// Token: 0x04000B74 RID: 2932
	private bool m_hasGenerated;

	// Token: 0x04000B75 RID: 2933
	private bool m_dragView = true;

	// Token: 0x04000B76 RID: 2934
	private Vector3 m_mapOffset = Vector3.zero;

	// Token: 0x04000B77 RID: 2935
	private float m_leftDownTime;

	// Token: 0x04000B78 RID: 2936
	private float m_leftClickTime;

	// Token: 0x04000B79 RID: 2937
	private Vector3 m_dragWorldPos = Vector3.zero;

	// Token: 0x04000B7A RID: 2938
	private bool m_wasFocused;

	// Token: 0x04000B7B RID: 2939
	private float m_delayTextInput;

	// Token: 0x04000B7C RID: 2940
	private const bool m_enableLastDeathAutoPin = false;

	// Token: 0x04000B7D RID: 2941
	private int m_hiddenFrames;

	// Token: 0x04000B7E RID: 2942
	[SerializeField]
	private float m_gamepadMoveSpeed = 0.33f;

	// Token: 0x020000E6 RID: 230
	public enum MapMode
	{
		// Token: 0x04000B80 RID: 2944
		None,
		// Token: 0x04000B81 RID: 2945
		Small,
		// Token: 0x04000B82 RID: 2946
		Large
	}

	// Token: 0x020000E7 RID: 231
	public enum PinType
	{
		// Token: 0x04000B84 RID: 2948
		Icon0,
		// Token: 0x04000B85 RID: 2949
		Icon1,
		// Token: 0x04000B86 RID: 2950
		Icon2,
		// Token: 0x04000B87 RID: 2951
		Icon3,
		// Token: 0x04000B88 RID: 2952
		Death,
		// Token: 0x04000B89 RID: 2953
		Bed,
		// Token: 0x04000B8A RID: 2954
		Icon4,
		// Token: 0x04000B8B RID: 2955
		Shout,
		// Token: 0x04000B8C RID: 2956
		None,
		// Token: 0x04000B8D RID: 2957
		Boss,
		// Token: 0x04000B8E RID: 2958
		Player,
		// Token: 0x04000B8F RID: 2959
		RandomEvent,
		// Token: 0x04000B90 RID: 2960
		Ping,
		// Token: 0x04000B91 RID: 2961
		EventArea
	}

	// Token: 0x020000E8 RID: 232
	public class PinData
	{
		// Token: 0x04000B92 RID: 2962
		public string m_name;

		// Token: 0x04000B93 RID: 2963
		public Minimap.PinType m_type;

		// Token: 0x04000B94 RID: 2964
		public Sprite m_icon;

		// Token: 0x04000B95 RID: 2965
		public Vector3 m_pos;

		// Token: 0x04000B96 RID: 2966
		public bool m_save;

		// Token: 0x04000B97 RID: 2967
		public long m_ownerID;

		// Token: 0x04000B98 RID: 2968
		public bool m_checked;

		// Token: 0x04000B99 RID: 2969
		public bool m_doubleSize;

		// Token: 0x04000B9A RID: 2970
		public bool m_animate;

		// Token: 0x04000B9B RID: 2971
		public float m_worldSize;

		// Token: 0x04000B9C RID: 2972
		public RectTransform m_uiElement;

		// Token: 0x04000B9D RID: 2973
		public GameObject m_checkedElement;

		// Token: 0x04000B9E RID: 2974
		public Image m_iconElement;

		// Token: 0x04000B9F RID: 2975
		public Minimap.PinNameData m_NamePinData;
	}

	// Token: 0x020000E9 RID: 233
	public class PinNameData
	{
		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000999 RID: 2457 RVA: 0x000499DC File Offset: 0x00047BDC
		// (set) Token: 0x06000998 RID: 2456 RVA: 0x000499D3 File Offset: 0x00047BD3
		public TMP_Text PinNameText { get; private set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600099B RID: 2459 RVA: 0x000499ED File Offset: 0x00047BED
		// (set) Token: 0x0600099A RID: 2458 RVA: 0x000499E4 File Offset: 0x00047BE4
		public GameObject PinNameGameObject { get; private set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600099D RID: 2461 RVA: 0x000499FE File Offset: 0x00047BFE
		// (set) Token: 0x0600099C RID: 2460 RVA: 0x000499F5 File Offset: 0x00047BF5
		public RectTransform PinNameRectTransform { get; private set; }

		// Token: 0x0600099E RID: 2462 RVA: 0x00049A06 File Offset: 0x00047C06
		public PinNameData(Minimap.PinData pin)
		{
			this.ParentPin = pin;
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x00049A15 File Offset: 0x00047C15
		internal void SetTextAndGameObject(GameObject text, TMP_Text textComponent)
		{
			this.PinNameGameObject = text;
			this.PinNameText = textComponent;
			this.PinNameText.text = Localization.instance.Localize(this.ParentPin.m_name);
			this.PinNameRectTransform = text.GetComponent<RectTransform>();
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x00049A51 File Offset: 0x00047C51
		internal void DestroyMapMarker()
		{
			UnityEngine.Object.Destroy(this.PinNameGameObject);
			this.PinNameGameObject = null;
		}

		// Token: 0x04000BA0 RID: 2976
		public readonly Minimap.PinData ParentPin;
	}

	// Token: 0x020000EA RID: 234
	[Serializable]
	public struct SpriteData
	{
		// Token: 0x04000BA4 RID: 2980
		public Minimap.PinType m_name;

		// Token: 0x04000BA5 RID: 2981
		public Sprite m_icon;
	}

	// Token: 0x020000EB RID: 235
	[Serializable]
	public struct LocationSpriteData
	{
		// Token: 0x04000BA6 RID: 2982
		public string m_name;

		// Token: 0x04000BA7 RID: 2983
		public Sprite m_icon;
	}
}
