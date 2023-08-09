using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000B2 RID: 178
public class Hud : MonoBehaviour
{
	// Token: 0x06000773 RID: 1907 RVA: 0x000394C0 File Offset: 0x000376C0
	private void OnDestroy()
	{
		Hud.m_instance = null;
		PlayerProfile.SavingStarted = (Action)Delegate.Remove(PlayerProfile.SavingStarted, new Action(this.SaveStarted));
		PlayerProfile.SavingFinished = (Action)Delegate.Remove(PlayerProfile.SavingFinished, new Action(this.SaveFinished));
	}

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x06000774 RID: 1908 RVA: 0x00039513 File Offset: 0x00037713
	public static Hud instance
	{
		get
		{
			return Hud.m_instance;
		}
	}

	// Token: 0x06000775 RID: 1909 RVA: 0x0003951C File Offset: 0x0003771C
	private void Awake()
	{
		Hud.m_instance = this;
		this.m_pieceSelectionWindow.SetActive(false);
		this.m_loadingScreen.gameObject.SetActive(false);
		this.m_statusEffectTemplate.gameObject.SetActive(false);
		this.m_eventBar.SetActive(false);
		this.m_gpRoot.gameObject.SetActive(false);
		this.m_betaText.SetActive(false);
		UIInputHandler closePieceSelectionButton = this.m_closePieceSelectionButton;
		closePieceSelectionButton.m_onLeftClick = (Action<UIInputHandler>)Delegate.Combine(closePieceSelectionButton.m_onLeftClick, new Action<UIInputHandler>(this.OnClosePieceSelection));
		UIInputHandler closePieceSelectionButton2 = this.m_closePieceSelectionButton;
		closePieceSelectionButton2.m_onRightClick = (Action<UIInputHandler>)Delegate.Combine(closePieceSelectionButton2.m_onRightClick, new Action<UIInputHandler>(this.OnClosePieceSelection));
		if (SteamManager.APP_ID == 1223920U)
		{
			this.m_betaText.SetActive(true);
		}
		foreach (GameObject gameObject in this.m_pieceCategoryTabs)
		{
			this.m_buildCategoryNames.Add(gameObject.transform.Find("Text").GetComponent<Text>().text);
			UIInputHandler component = gameObject.GetComponent<UIInputHandler>();
			component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(this.OnLeftClickCategory));
		}
		PlayerProfile.SavingStarted = (Action)Delegate.Remove(PlayerProfile.SavingStarted, new Action(this.SaveStarted));
		PlayerProfile.SavingStarted = (Action)Delegate.Combine(PlayerProfile.SavingStarted, new Action(this.SaveStarted));
		PlayerProfile.SavingFinished = (Action)Delegate.Remove(PlayerProfile.SavingFinished, new Action(this.SaveFinished));
		PlayerProfile.SavingFinished = (Action)Delegate.Combine(PlayerProfile.SavingFinished, new Action(this.SaveFinished));
	}

	// Token: 0x06000776 RID: 1910 RVA: 0x000396D1 File Offset: 0x000378D1
	private void SaveStarted()
	{
		Debug.Log("saving started");
		this.m_savingTriggered = true;
		this.m_saveIconTimer = 3f;
	}

	// Token: 0x06000777 RID: 1911 RVA: 0x000396EF File Offset: 0x000378EF
	private void SaveFinished()
	{
		Debug.Log("saving finished");
		this.m_savingTriggered = false;
	}

	// Token: 0x06000778 RID: 1912 RVA: 0x00039704 File Offset: 0x00037904
	private void SetVisible(bool visible)
	{
		if (visible == this.IsVisible())
		{
			return;
		}
		if (visible)
		{
			this.m_rootObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			this.m_rootObject.transform.localPosition = new Vector3(10000f, 0f, 0f);
		}
		if (Menu.instance && (visible || (Player.m_localPlayer && !Player.m_localPlayer.InCutscene())))
		{
			Menu.instance.transform.localPosition = this.m_rootObject.transform.localPosition;
		}
	}

	// Token: 0x06000779 RID: 1913 RVA: 0x000397AE File Offset: 0x000379AE
	private bool IsVisible()
	{
		return this.m_rootObject.transform.localPosition.x < 1000f;
	}

	// Token: 0x0600077A RID: 1914 RVA: 0x000397CC File Offset: 0x000379CC
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		bool flag = ZNet.instance != null && ZNet.instance.IsSaving();
		if (this.m_savingTriggered || flag || this.m_saveIconTimer > 0f)
		{
			this.m_saveIcon.SetActive(true);
			this.m_saveIconTimer -= Time.unscaledDeltaTime;
			Color color = this.m_saveIconImage.color;
			float a = Mathf.PingPong(this.m_saveIconTimer * 2f, 1f);
			this.m_saveIconImage.color = new Color(color.r, color.g, color.b, a);
			this.m_badConnectionIcon.SetActive(false);
		}
		else
		{
			this.m_saveIcon.SetActive(false);
			this.m_badConnectionIcon.SetActive(ZNet.instance != null && ZNet.instance.HasBadConnection() && Mathf.Sin(Time.time * 10f) > 0f);
		}
		Player localPlayer = Player.m_localPlayer;
		this.UpdateDamageFlash(deltaTime);
		if (localPlayer)
		{
			if (Input.GetKeyDown(KeyCode.F3) && Input.GetKey(KeyCode.LeftControl))
			{
				this.m_userHidden = !this.m_userHidden;
			}
			this.SetVisible(!this.m_userHidden && !localPlayer.InCutscene());
			this.UpdateBuild(localPlayer, false);
			this.m_tempStatusEffects.Clear();
			localPlayer.GetSEMan().GetHUDStatusEffects(this.m_tempStatusEffects);
			this.UpdateStatusEffects(this.m_tempStatusEffects);
			this.UpdateGuardianPower(localPlayer);
			float attackDrawPercentage = localPlayer.GetAttackDrawPercentage();
			this.UpdateFood(localPlayer);
			this.UpdateHealth(localPlayer);
			this.UpdateStamina(localPlayer, deltaTime);
			this.UpdateEitr(localPlayer, deltaTime);
			this.UpdateStealth(localPlayer, attackDrawPercentage);
			this.UpdateCrosshair(localPlayer, attackDrawPercentage);
			this.UpdateEvent(localPlayer);
			this.UpdateActionProgress(localPlayer);
			this.UpdateStagger(localPlayer, deltaTime);
			this.UpdateMount(localPlayer, deltaTime);
		}
	}

	// Token: 0x0600077B RID: 1915 RVA: 0x000399B8 File Offset: 0x00037BB8
	private void LateUpdate()
	{
		this.UpdateBlackScreen(Player.m_localPlayer, Time.deltaTime);
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			this.UpdateShipHud(localPlayer, Time.deltaTime);
		}
	}

	// Token: 0x0600077C RID: 1916 RVA: 0x000399EF File Offset: 0x00037BEF
	private float GetFadeDuration(Player player)
	{
		if (player != null)
		{
			if (player.IsDead())
			{
				return 9.5f;
			}
			if (player.IsSleeping())
			{
				return 3f;
			}
		}
		return 1f;
	}

	// Token: 0x0600077D RID: 1917 RVA: 0x00039A1C File Offset: 0x00037C1C
	private void UpdateBlackScreen(Player player, float dt)
	{
		if (!(player == null) && !player.IsDead() && !player.IsTeleporting() && !Game.instance.IsShuttingDown() && !player.IsSleeping())
		{
			this.m_haveSetupLoadScreen = false;
			float fadeDuration = this.GetFadeDuration(player);
			float num = this.m_loadingScreen.alpha;
			num = Mathf.MoveTowards(num, 0f, dt / fadeDuration);
			this.m_loadingScreen.alpha = num;
			if (this.m_loadingScreen.alpha <= 0f)
			{
				this.m_loadingScreen.gameObject.SetActive(false);
			}
			return;
		}
		this.m_loadingScreen.gameObject.SetActive(true);
		float num2 = this.m_loadingScreen.alpha;
		float fadeDuration2 = this.GetFadeDuration(player);
		num2 = Mathf.MoveTowards(num2, 1f, dt / fadeDuration2);
		if (Game.instance.IsShuttingDown())
		{
			num2 = 1f;
		}
		this.m_loadingScreen.alpha = num2;
		if (player != null && player.IsSleeping())
		{
			this.m_sleepingProgress.SetActive(true);
			this.m_loadingProgress.SetActive(false);
			this.m_teleportingProgress.SetActive(false);
			return;
		}
		if (player != null && player.ShowTeleportAnimation())
		{
			this.m_loadingProgress.SetActive(false);
			this.m_sleepingProgress.SetActive(false);
			this.m_teleportingProgress.SetActive(true);
			return;
		}
		if (Game.instance && Game.instance.WaitingForRespawn())
		{
			if (!this.m_haveSetupLoadScreen)
			{
				this.m_haveSetupLoadScreen = true;
				string text = this.m_loadingTips[UnityEngine.Random.Range(0, this.m_loadingTips.Count)];
				ZLog.Log("tip:" + text);
				this.m_loadingTip.text = Localization.instance.Localize(text);
			}
			this.m_loadingProgress.SetActive(true);
			this.m_sleepingProgress.SetActive(false);
			this.m_teleportingProgress.SetActive(false);
			return;
		}
		this.m_loadingProgress.SetActive(false);
		this.m_sleepingProgress.SetActive(false);
		this.m_teleportingProgress.SetActive(false);
	}

	// Token: 0x0600077E RID: 1918 RVA: 0x00039C2C File Offset: 0x00037E2C
	private void UpdateShipHud(Player player, float dt)
	{
		Ship controlledShip = player.GetControlledShip();
		if (controlledShip == null)
		{
			this.m_shipHudRoot.gameObject.SetActive(false);
			return;
		}
		Ship.Speed speedSetting = controlledShip.GetSpeedSetting();
		float rudder = controlledShip.GetRudder();
		float rudderValue = controlledShip.GetRudderValue();
		this.m_shipHudRoot.SetActive(true);
		this.m_rudderSlow.SetActive(speedSetting == Ship.Speed.Slow);
		this.m_rudderForward.SetActive(speedSetting == Ship.Speed.Half);
		this.m_rudderFastForward.SetActive(speedSetting == Ship.Speed.Full);
		this.m_rudderBackward.SetActive(speedSetting == Ship.Speed.Back);
		this.m_rudderLeft.SetActive(false);
		this.m_rudderRight.SetActive(false);
		this.m_fullSail.SetActive(speedSetting == Ship.Speed.Full);
		this.m_halfSail.SetActive(speedSetting == Ship.Speed.Half);
		this.m_rudder.SetActive(speedSetting == Ship.Speed.Slow || speedSetting == Ship.Speed.Back || (speedSetting == Ship.Speed.Stop && Mathf.Abs(rudderValue) > 0.2f));
		if ((rudder > 0f && rudderValue < 1f) || (rudder < 0f && rudderValue > -1f))
		{
			this.m_shipRudderIcon.transform.Rotate(new Vector3(0f, 0f, 200f * -rudder * dt));
		}
		if (Mathf.Abs(rudderValue) < 0.02f)
		{
			this.m_shipRudderIndicator.gameObject.SetActive(false);
		}
		else
		{
			this.m_shipRudderIndicator.gameObject.SetActive(true);
			if (rudderValue > 0f)
			{
				this.m_shipRudderIndicator.fillClockwise = true;
				this.m_shipRudderIndicator.fillAmount = rudderValue * 0.25f;
			}
			else
			{
				this.m_shipRudderIndicator.fillClockwise = false;
				this.m_shipRudderIndicator.fillAmount = -rudderValue * 0.25f;
			}
		}
		float shipYawAngle = controlledShip.GetShipYawAngle();
		this.m_shipWindIndicatorRoot.localRotation = Quaternion.Euler(0f, 0f, shipYawAngle);
		float windAngle = controlledShip.GetWindAngle();
		this.m_shipWindIconRoot.localRotation = Quaternion.Euler(0f, 0f, windAngle);
		float windAngleFactor = controlledShip.GetWindAngleFactor();
		this.m_shipWindIcon.color = Color.Lerp(new Color(0.2f, 0.2f, 0.2f, 1f), Color.white, windAngleFactor);
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		this.m_shipControlsRoot.transform.position = mainCamera.WorldToScreenPoint(controlledShip.m_controlGuiPos.position);
	}

	// Token: 0x0600077F RID: 1919 RVA: 0x00039E8C File Offset: 0x0003808C
	private void UpdateStagger(Player player, float dt)
	{
		float staggerPercentage = player.GetStaggerPercentage();
		this.m_staggerProgress.SetValue(staggerPercentage);
		if (staggerPercentage > 0f)
		{
			this.m_staggerHideTimer = 0f;
		}
		else
		{
			this.m_staggerHideTimer += dt;
		}
		this.m_staggerAnimator.SetBool("Visible", this.m_staggerHideTimer < 1f);
	}

	// Token: 0x06000780 RID: 1920 RVA: 0x00039EEC File Offset: 0x000380EC
	public void StaggerBarFlash()
	{
		this.m_staggerAnimator.SetTrigger("Flash");
	}

	// Token: 0x06000781 RID: 1921 RVA: 0x00039F00 File Offset: 0x00038100
	private void UpdateActionProgress(Player player)
	{
		string text;
		float value;
		player.GetActionProgress(out text, out value);
		if (!string.IsNullOrEmpty(text))
		{
			this.m_actionBarRoot.SetActive(true);
			this.m_actionProgress.SetValue(value);
			this.m_actionName.text = Localization.instance.Localize(text);
			return;
		}
		this.m_actionBarRoot.SetActive(false);
	}

	// Token: 0x06000782 RID: 1922 RVA: 0x00039F5C File Offset: 0x0003815C
	private void UpdateCrosshair(Player player, float bowDrawPercentage)
	{
		GameObject hoverObject = player.GetHoverObject();
		Hoverable hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
		if (hoverable != null && !TextViewer.instance.IsVisible())
		{
			string text = hoverable.GetHoverText();
			if (ZInput.IsGamepadActive())
			{
				text = text.Replace("[<color=yellow><b><sprite=", "<sprite=");
				text = text.Replace("\"></b></color>]", "\">");
			}
			this.m_hoverName.text = text;
			this.m_crosshair.color = ((this.m_hoverName.text.Length > 0) ? Color.yellow : new Color(1f, 1f, 1f, 0.5f));
		}
		else
		{
			this.m_crosshair.color = new Color(1f, 1f, 1f, 0.5f);
			this.m_hoverName.text = "";
		}
		Piece hoveringPiece = player.GetHoveringPiece();
		if (hoveringPiece)
		{
			WearNTear component = hoveringPiece.GetComponent<WearNTear>();
			if (component)
			{
				this.m_pieceHealthRoot.gameObject.SetActive(true);
				this.m_pieceHealthBar.SetValue(component.GetHealthPercentage());
			}
			else
			{
				this.m_pieceHealthRoot.gameObject.SetActive(false);
			}
		}
		else
		{
			this.m_pieceHealthRoot.gameObject.SetActive(false);
		}
		if (bowDrawPercentage > 0f)
		{
			float num = Mathf.Lerp(1f, 0.15f, bowDrawPercentage);
			this.m_crosshairBow.gameObject.SetActive(true);
			this.m_crosshairBow.transform.localScale = new Vector3(num, num, num);
			this.m_crosshairBow.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.yellow, bowDrawPercentage);
			return;
		}
		this.m_crosshairBow.gameObject.SetActive(false);
	}

	// Token: 0x06000783 RID: 1923 RVA: 0x0003A134 File Offset: 0x00038334
	private void FixedUpdate()
	{
		this.UpdatePieceBar(Time.fixedDeltaTime);
	}

	// Token: 0x06000784 RID: 1924 RVA: 0x0003A144 File Offset: 0x00038344
	private void UpdateStealth(Player player, float bowDrawPercentage)
	{
		float stealthFactor = player.GetStealthFactor();
		if ((player.IsCrouching() || stealthFactor < 1f) && bowDrawPercentage == 0f)
		{
			if (player.IsSensed())
			{
				this.m_targetedAlert.SetActive(true);
				this.m_targeted.SetActive(false);
				this.m_hidden.SetActive(false);
			}
			else if (player.IsTargeted())
			{
				this.m_targetedAlert.SetActive(false);
				this.m_targeted.SetActive(true);
				this.m_hidden.SetActive(false);
			}
			else
			{
				this.m_targetedAlert.SetActive(false);
				this.m_targeted.SetActive(false);
				this.m_hidden.SetActive(true);
			}
			this.m_stealthBar.gameObject.SetActive(true);
			this.m_stealthBar.SetValue(stealthFactor);
			return;
		}
		this.m_targetedAlert.SetActive(false);
		this.m_hidden.SetActive(false);
		this.m_targeted.SetActive(false);
		this.m_stealthBar.gameObject.SetActive(false);
	}

	// Token: 0x06000785 RID: 1925 RVA: 0x0003A24C File Offset: 0x0003844C
	private void SetHealthBarSize(float size)
	{
		size = Mathf.Ceil(size);
		Mathf.Max(size + 56f, 138f);
		this.m_healthBarRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		this.m_healthBarSlow.SetWidth(size);
		this.m_healthBarFast.SetWidth(size);
	}

	// Token: 0x06000786 RID: 1926 RVA: 0x0003A298 File Offset: 0x00038498
	private void SetStaminaBarSize(float size)
	{
		this.m_staminaBar2Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size + this.m_staminaBarBorderBuffer);
		this.m_staminaBar2Slow.SetWidth(size);
		this.m_staminaBar2Fast.SetWidth(size);
	}

	// Token: 0x06000787 RID: 1927 RVA: 0x0003A2C6 File Offset: 0x000384C6
	private void SetEitrBarSize(float size)
	{
		this.m_eitrBarRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size + this.m_staminaBarBorderBuffer);
		this.m_eitrBarSlow.SetWidth(size);
		this.m_eitrBarFast.SetWidth(size);
	}

	// Token: 0x06000788 RID: 1928 RVA: 0x0003A2F4 File Offset: 0x000384F4
	private void UpdateFood(Player player)
	{
		List<Player.Food> foods = player.GetFoods();
		float size = player.GetBaseFoodHP() / 25f * 32f;
		this.m_foodBaseBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		for (int i = 0; i < this.m_foodBars.Length; i++)
		{
			Image image = this.m_foodBars[i];
			Image image2 = this.m_foodIcons[i];
			Text text = this.m_foodTime[i];
			if (i < foods.Count)
			{
				image.gameObject.SetActive(true);
				Player.Food food = foods[i];
				image2.gameObject.SetActive(true);
				image2.sprite = food.m_item.GetIcon();
				if (food.CanEatAgain())
				{
					image2.color = new Color(1f, 1f, 1f, 0.7f + Mathf.Sin(Time.time * 5f) * 0.3f);
				}
				else
				{
					image2.color = Color.white;
				}
				text.gameObject.SetActive(true);
				if (food.m_time >= 60f)
				{
					text.text = Mathf.CeilToInt(food.m_time / 60f).ToString() + "m";
					text.color = Color.white;
				}
				else
				{
					text.text = Mathf.FloorToInt(food.m_time).ToString() + "s";
					text.color = new Color(1f, 1f, 1f, 0.4f + Mathf.Sin(Time.time * 10f) * 0.6f);
				}
			}
			else
			{
				image.gameObject.SetActive(false);
				image2.gameObject.SetActive(false);
				text.gameObject.SetActive(false);
			}
		}
		float size2 = Mathf.Ceil(player.GetMaxHealth() / 25f * 32f);
		this.m_foodBarRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size2);
	}

	// Token: 0x06000789 RID: 1929 RVA: 0x0003A4F4 File Offset: 0x000386F4
	private void UpdateMount(Player player, float dt)
	{
		Sadle sadle = player.GetDoodadController() as Sadle;
		if (sadle == null)
		{
			this.m_mountPanel.SetActive(false);
			return;
		}
		Character character = sadle.GetCharacter();
		this.m_mountPanel.SetActive(true);
		this.m_mountHealthBarSlow.SetValue(character.GetHealthPercentage());
		this.m_mountHealthBarFast.SetValue(character.GetHealthPercentage());
		this.m_mountHealthText.text = Mathf.CeilToInt(character.GetHealth()).ToString();
		float stamina = sadle.GetStamina();
		float maxStamina = sadle.GetMaxStamina();
		this.m_mountStaminaBar.SetValue(stamina / maxStamina);
		this.m_mountStaminaText.text = Mathf.CeilToInt(stamina).ToString();
		this.m_mountNameText.text = character.GetHoverName() + " (" + Localization.instance.Localize(sadle.GetTameable().GetStatusString()) + " )";
	}

	// Token: 0x0600078A RID: 1930 RVA: 0x0003A5E4 File Offset: 0x000387E4
	private void UpdateHealth(Player player)
	{
		float maxHealth = player.GetMaxHealth();
		this.SetHealthBarSize(maxHealth / 25f * 32f);
		float health = player.GetHealth();
		this.m_healthBarFast.SetMaxValue(maxHealth);
		this.m_healthBarFast.SetValue(health);
		this.m_healthBarSlow.SetMaxValue(maxHealth);
		this.m_healthBarSlow.SetValue(health);
		string text = Mathf.CeilToInt(player.GetHealth()).ToString();
		this.m_healthText.text = text.ToString();
	}

	// Token: 0x0600078B RID: 1931 RVA: 0x0003A668 File Offset: 0x00038868
	private void UpdateStamina(Player player, float dt)
	{
		float stamina = player.GetStamina();
		float maxStamina = player.GetMaxStamina();
		if (stamina < maxStamina)
		{
			this.m_staminaHideTimer = 0f;
		}
		else
		{
			this.m_staminaHideTimer += dt;
		}
		this.m_staminaAnimator.SetBool("Visible", this.m_staminaHideTimer < 1f);
		this.m_staminaText.text = Mathf.CeilToInt(stamina).ToString();
		this.SetStaminaBarSize(maxStamina / 25f * 32f);
		RectTransform rectTransform = this.m_staminaBar2Root.transform as RectTransform;
		if (this.m_buildHud.activeSelf || this.m_shipHudRoot.activeSelf)
		{
			rectTransform.anchoredPosition = new Vector2(0f, 320f);
		}
		else
		{
			rectTransform.anchoredPosition = new Vector2(0f, 130f);
		}
		this.m_staminaBar2Slow.SetValue(stamina / maxStamina);
		this.m_staminaBar2Fast.SetValue(stamina / maxStamina);
	}

	// Token: 0x0600078C RID: 1932 RVA: 0x0003A760 File Offset: 0x00038960
	private void UpdateEitr(Player player, float dt)
	{
		float eitr = player.GetEitr();
		float maxEitr = player.GetMaxEitr();
		if (eitr < maxEitr)
		{
			this.m_eitrHideTimer = 0f;
		}
		else
		{
			this.m_eitrHideTimer += dt;
		}
		this.m_eitrAnimator.SetBool("Visible", this.m_eitrHideTimer < 1f);
		this.m_eitrText.text = Mathf.CeilToInt(eitr).ToString();
		this.SetEitrBarSize(maxEitr / 25f * 32f);
		RectTransform rectTransform = this.m_eitrBarRoot.transform as RectTransform;
		if (this.m_buildHud.activeSelf || this.m_shipHudRoot.activeSelf)
		{
			rectTransform.anchoredPosition = new Vector2(0f, 285f);
		}
		else
		{
			rectTransform.anchoredPosition = new Vector2(0f, 130f);
		}
		this.m_eitrBarSlow.SetValue(eitr / maxEitr);
		this.m_eitrBarFast.SetValue(eitr / maxEitr);
	}

	// Token: 0x0600078D RID: 1933 RVA: 0x0003A858 File Offset: 0x00038A58
	public void DamageFlash()
	{
		Color color = this.m_damageScreen.color;
		color.a = 1f;
		this.m_damageScreen.color = color;
		this.m_damageScreen.gameObject.SetActive(true);
	}

	// Token: 0x0600078E RID: 1934 RVA: 0x0003A89C File Offset: 0x00038A9C
	private void UpdateDamageFlash(float dt)
	{
		Color color = this.m_damageScreen.color;
		color.a = Mathf.MoveTowards(color.a, 0f, dt * 4f);
		this.m_damageScreen.color = color;
		if (color.a <= 0f)
		{
			this.m_damageScreen.gameObject.SetActive(false);
		}
	}

	// Token: 0x0600078F RID: 1935 RVA: 0x0003A900 File Offset: 0x00038B00
	private void UpdatePieceList(Player player, Vector2Int selectedNr, Piece.PieceCategory category, bool updateAllBuildStatuses)
	{
		List<Piece> buildPieces = player.GetBuildPieces();
		int num = 15;
		int num2 = 6;
		if (buildPieces.Count <= 1)
		{
			num = 1;
			num2 = 1;
		}
		if (this.m_pieceIcons.Count != num * num2)
		{
			foreach (Hud.PieceIconData pieceIconData in this.m_pieceIcons)
			{
				UnityEngine.Object.Destroy(pieceIconData.m_go);
			}
			this.m_pieceIcons.Clear();
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_pieceIconPrefab, this.m_pieceListRoot);
					(gameObject.transform as RectTransform).anchoredPosition = new Vector2((float)j * this.m_pieceIconSpacing, (float)(-(float)i) * this.m_pieceIconSpacing);
					Hud.PieceIconData pieceIconData2 = new Hud.PieceIconData();
					pieceIconData2.m_go = gameObject;
					pieceIconData2.m_tooltip = gameObject.GetComponent<UITooltip>();
					pieceIconData2.m_icon = gameObject.transform.Find("icon").GetComponent<Image>();
					pieceIconData2.m_marker = gameObject.transform.Find("selected").gameObject;
					pieceIconData2.m_upgrade = gameObject.transform.Find("upgrade").gameObject;
					pieceIconData2.m_icon.color = new Color(1f, 0f, 1f, 0f);
					UIInputHandler component = gameObject.GetComponent<UIInputHandler>();
					component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(this.OnLeftClickPiece));
					component.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightDown, new Action<UIInputHandler>(this.OnRightClickPiece));
					component.m_onPointerEnter = (Action<UIInputHandler>)Delegate.Combine(component.m_onPointerEnter, new Action<UIInputHandler>(this.OnHoverPiece));
					component.m_onPointerExit = (Action<UIInputHandler>)Delegate.Combine(component.m_onPointerExit, new Action<UIInputHandler>(this.OnHoverPieceExit));
					this.m_pieceIcons.Add(pieceIconData2);
				}
			}
		}
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num3 = k * num + l;
				Hud.PieceIconData pieceIconData3 = this.m_pieceIcons[num3];
				pieceIconData3.m_marker.SetActive(new Vector2Int(l, k) == selectedNr);
				if (num3 < buildPieces.Count)
				{
					Piece piece = buildPieces[num3];
					pieceIconData3.m_icon.sprite = piece.m_icon;
					pieceIconData3.m_icon.enabled = true;
					pieceIconData3.m_tooltip.m_text = piece.m_name;
					pieceIconData3.m_upgrade.SetActive(piece.m_isUpgrade);
				}
				else
				{
					pieceIconData3.m_icon.enabled = false;
					pieceIconData3.m_tooltip.m_text = "";
					pieceIconData3.m_upgrade.SetActive(false);
				}
			}
		}
		this.UpdatePieceBuildStatus(buildPieces, player);
		if (updateAllBuildStatuses)
		{
			this.UpdatePieceBuildStatusAll(buildPieces, player);
		}
		if (this.m_lastPieceCategory != category)
		{
			this.m_lastPieceCategory = category;
			this.m_pieceBarPosX = this.m_pieceBarTargetPosX;
			this.UpdatePieceBuildStatusAll(buildPieces, player);
		}
	}

	// Token: 0x06000790 RID: 1936 RVA: 0x0003AC4C File Offset: 0x00038E4C
	private void OnLeftClickCategory(UIInputHandler ih)
	{
		for (int i = 0; i < this.m_pieceCategoryTabs.Length; i++)
		{
			if (this.m_pieceCategoryTabs[i] == ih.gameObject)
			{
				Player.m_localPlayer.SetBuildCategory(i);
				return;
			}
		}
	}

	// Token: 0x06000791 RID: 1937 RVA: 0x0003AC8D File Offset: 0x00038E8D
	private void OnLeftClickPiece(UIInputHandler ih)
	{
		this.SelectPiece(ih);
		Hud.HidePieceSelection();
	}

	// Token: 0x06000792 RID: 1938 RVA: 0x0003AC9B File Offset: 0x00038E9B
	private void OnRightClickPiece(UIInputHandler ih)
	{
		if (this.IsQuickPieceSelectEnabled())
		{
			this.SelectPiece(ih);
			Hud.HidePieceSelection();
		}
	}

	// Token: 0x06000793 RID: 1939 RVA: 0x0003ACB4 File Offset: 0x00038EB4
	private void OnHoverPiece(UIInputHandler ih)
	{
		Vector2Int selectedGrid = this.GetSelectedGrid(ih);
		if (selectedGrid.x != -1)
		{
			this.m_hoveredPiece = Player.m_localPlayer.GetPiece(selectedGrid);
		}
	}

	// Token: 0x06000794 RID: 1940 RVA: 0x0003ACE4 File Offset: 0x00038EE4
	private void OnHoverPieceExit(UIInputHandler ih)
	{
		this.m_hoveredPiece = null;
	}

	// Token: 0x06000795 RID: 1941 RVA: 0x0003ACED File Offset: 0x00038EED
	public bool IsQuickPieceSelectEnabled()
	{
		return PlayerPrefs.GetInt("QuickPieceSelect", 0) == 1;
	}

	// Token: 0x06000796 RID: 1942 RVA: 0x0003AD00 File Offset: 0x00038F00
	private Vector2Int GetSelectedGrid(UIInputHandler ih)
	{
		int num = 15;
		int num2 = 6;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int index = i * num + j;
				if (this.m_pieceIcons[index].m_go == ih.gameObject)
				{
					return new Vector2Int(j, i);
				}
			}
		}
		return new Vector2Int(-1, -1);
	}

	// Token: 0x06000797 RID: 1943 RVA: 0x0003AD60 File Offset: 0x00038F60
	private void SelectPiece(UIInputHandler ih)
	{
		Vector2Int selectedGrid = this.GetSelectedGrid(ih);
		if (selectedGrid.x != -1)
		{
			Player.m_localPlayer.SetSelectedPiece(selectedGrid);
			this.m_selectItemEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		}
	}

	// Token: 0x06000798 RID: 1944 RVA: 0x0003ADB0 File Offset: 0x00038FB0
	private void UpdatePieceBuildStatus(List<Piece> pieces, Player player)
	{
		if (this.m_pieceIcons.Count == 0)
		{
			return;
		}
		if (this.m_pieceIconUpdateIndex >= this.m_pieceIcons.Count)
		{
			this.m_pieceIconUpdateIndex = 0;
		}
		Hud.PieceIconData pieceIconData = this.m_pieceIcons[this.m_pieceIconUpdateIndex];
		if (this.m_pieceIconUpdateIndex < pieces.Count)
		{
			Piece piece = pieces[this.m_pieceIconUpdateIndex];
			bool flag = player.HaveRequirements(piece, Player.RequirementMode.CanBuild);
			pieceIconData.m_icon.color = (flag ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 0f, 1f, 0f));
		}
		this.m_pieceIconUpdateIndex++;
	}

	// Token: 0x06000799 RID: 1945 RVA: 0x0003AE6C File Offset: 0x0003906C
	private void UpdatePieceBuildStatusAll(List<Piece> pieces, Player player)
	{
		for (int i = 0; i < this.m_pieceIcons.Count; i++)
		{
			Hud.PieceIconData pieceIconData = this.m_pieceIcons[i];
			if (i < pieces.Count)
			{
				Piece piece = pieces[i];
				bool flag = player.HaveRequirements(piece, Player.RequirementMode.CanBuild);
				pieceIconData.m_icon.color = (flag ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 0f, 1f, 0f));
			}
			else
			{
				pieceIconData.m_icon.color = Color.white;
			}
		}
		this.m_pieceIconUpdateIndex = 0;
	}

	// Token: 0x0600079A RID: 1946 RVA: 0x0003AF14 File Offset: 0x00039114
	private void UpdatePieceBar(float dt)
	{
		this.m_pieceBarPosX = Mathf.Lerp(this.m_pieceBarPosX, this.m_pieceBarTargetPosX, 0.1f);
		this.m_pieceListRoot.anchoredPosition.x = Mathf.Round(this.m_pieceBarPosX);
	}

	// Token: 0x0600079B RID: 1947 RVA: 0x0003AF60 File Offset: 0x00039160
	public void TogglePieceSelection()
	{
		this.m_hoveredPiece = null;
		if (this.m_pieceSelectionWindow.activeSelf)
		{
			this.m_pieceSelectionWindow.SetActive(false);
			return;
		}
		this.m_pieceSelectionWindow.SetActive(true);
		this.UpdateBuild(Player.m_localPlayer, true);
	}

	// Token: 0x0600079C RID: 1948 RVA: 0x0003AF9B File Offset: 0x0003919B
	private void OnClosePieceSelection(UIInputHandler ih)
	{
		Hud.HidePieceSelection();
	}

	// Token: 0x0600079D RID: 1949 RVA: 0x0003AFA2 File Offset: 0x000391A2
	public static void HidePieceSelection()
	{
		if (Hud.m_instance == null)
		{
			return;
		}
		Hud.m_instance.m_closePieceSelection = 2;
	}

	// Token: 0x0600079E RID: 1950 RVA: 0x0003AFBD File Offset: 0x000391BD
	public static bool IsPieceSelectionVisible()
	{
		return !(Hud.m_instance == null) && Hud.m_instance.m_buildHud.activeSelf && Hud.m_instance.m_pieceSelectionWindow.activeSelf;
	}

	// Token: 0x0600079F RID: 1951 RVA: 0x0003AFF0 File Offset: 0x000391F0
	private void UpdateBuild(Player player, bool forceUpdateAllBuildStatuses)
	{
		if (!player.InPlaceMode())
		{
			this.m_hoveredPiece = null;
			this.m_buildHud.SetActive(false);
			this.m_pieceSelectionWindow.SetActive(false);
			return;
		}
		if (this.m_closePieceSelection > 0)
		{
			this.m_closePieceSelection--;
			if (this.m_closePieceSelection <= 0 && this.m_pieceSelectionWindow.activeSelf)
			{
				this.m_hoveredPiece = null;
				this.m_pieceSelectionWindow.SetActive(false);
			}
		}
		Piece piece;
		Vector2Int selectedNr;
		int num;
		Piece.PieceCategory pieceCategory;
		bool flag;
		player.GetBuildSelection(out piece, out selectedNr, out num, out pieceCategory, out flag);
		this.m_buildHud.SetActive(true);
		if (this.m_pieceSelectionWindow.activeSelf)
		{
			this.UpdatePieceList(player, selectedNr, pieceCategory, forceUpdateAllBuildStatuses);
			this.m_pieceCategoryRoot.SetActive(flag);
			if (flag)
			{
				for (int i = 0; i < this.m_pieceCategoryTabs.Length; i++)
				{
					GameObject gameObject = this.m_pieceCategoryTabs[i];
					Transform transform = gameObject.transform.Find("Selected");
					string text = this.m_buildCategoryNames[i] + " [<color=yellow>" + player.GetAvailableBuildPiecesInCategory((Piece.PieceCategory)i).ToString() + "</color>]";
					if (i == (int)pieceCategory)
					{
						transform.gameObject.SetActive(true);
						transform.GetComponentInChildren<Text>().text = text;
					}
					else
					{
						transform.gameObject.SetActive(false);
						gameObject.GetComponentInChildren<Text>().text = text;
					}
				}
			}
			Localization.instance.Localize(this.m_buildHud.transform);
		}
		if (this.m_hoveredPiece && (ZInput.IsGamepadActive() || !player.IsPieceAvailable(this.m_hoveredPiece)))
		{
			this.m_hoveredPiece = null;
		}
		if (this.m_hoveredPiece)
		{
			this.SetupPieceInfo(this.m_hoveredPiece);
			return;
		}
		this.SetupPieceInfo(piece);
	}

	// Token: 0x060007A0 RID: 1952 RVA: 0x0003B1B8 File Offset: 0x000393B8
	private void SetupPieceInfo(Piece piece)
	{
		if (piece == null)
		{
			this.m_buildSelection.text = Localization.instance.Localize("$hud_nothingtobuild");
			this.m_pieceDescription.text = "";
			this.m_buildIcon.enabled = false;
			this.m_snappingIcon.enabled = false;
			for (int i = 0; i < this.m_requirementItems.Length; i++)
			{
				this.m_requirementItems[i].SetActive(false);
			}
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		this.m_buildSelection.text = Localization.instance.Localize(piece.m_name);
		this.m_pieceDescription.text = Localization.instance.Localize(piece.m_description);
		this.m_buildIcon.enabled = true;
		this.m_buildIcon.sprite = piece.m_icon;
		Sprite snappingIconForPiece = this.GetSnappingIconForPiece(piece);
		this.m_snappingIcon.sprite = snappingIconForPiece;
		this.m_snappingIcon.enabled = (snappingIconForPiece != null && (piece.m_category == Piece.PieceCategory.Building || piece.m_groundPiece || piece.m_waterPiece));
		for (int j = 0; j < this.m_requirementItems.Length; j++)
		{
			if (j < piece.m_resources.Length)
			{
				Piece.Requirement req = piece.m_resources[j];
				this.m_requirementItems[j].SetActive(true);
				InventoryGui.SetupRequirement(this.m_requirementItems[j].transform, req, localPlayer, false, 0);
			}
			else
			{
				this.m_requirementItems[j].SetActive(false);
			}
		}
		if (piece.m_craftingStation)
		{
			CraftingStation craftingStation = CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, localPlayer.transform.position);
			GameObject gameObject = this.m_requirementItems[piece.m_resources.Length];
			gameObject.SetActive(true);
			Image component = gameObject.transform.Find("res_icon").GetComponent<Image>();
			Text component2 = gameObject.transform.Find("res_name").GetComponent<Text>();
			Text component3 = gameObject.transform.Find("res_amount").GetComponent<Text>();
			UITooltip component4 = gameObject.GetComponent<UITooltip>();
			component.sprite = piece.m_craftingStation.m_icon;
			component2.text = Localization.instance.Localize(piece.m_craftingStation.m_name);
			component4.m_text = piece.m_craftingStation.m_name;
			if (craftingStation != null)
			{
				craftingStation.ShowAreaMarker();
				component.color = Color.white;
				component3.text = "";
				component3.color = Color.white;
				return;
			}
			component.color = Color.gray;
			component3.text = "None";
			component3.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : Color.white);
		}
	}

	// Token: 0x060007A1 RID: 1953 RVA: 0x0003B474 File Offset: 0x00039674
	private Sprite GetSnappingIconForPiece(Piece piece)
	{
		if (piece.m_groundPiece)
		{
			if (!Player.m_localPlayer.AlternativePlacementActive)
			{
				return null;
			}
			return this.m_hoeSnappingIcon;
		}
		else if (piece.m_waterPiece)
		{
			if (!Player.m_localPlayer.AlternativePlacementActive)
			{
				return null;
			}
			return this.m_shipSnappingIcon;
		}
		else
		{
			if (!Player.m_localPlayer.AlternativePlacementActive)
			{
				return null;
			}
			return this.m_buildSnappingIcon;
		}
	}

	// Token: 0x060007A2 RID: 1954 RVA: 0x0003B4D0 File Offset: 0x000396D0
	private void UpdateGuardianPower(Player player)
	{
		StatusEffect statusEffect;
		float num;
		player.GetGuardianPowerHUD(out statusEffect, out num);
		if (!statusEffect)
		{
			this.m_gpRoot.gameObject.SetActive(false);
			return;
		}
		this.m_gpRoot.gameObject.SetActive(true);
		this.m_gpIcon.sprite = statusEffect.m_icon;
		this.m_gpIcon.color = ((num <= 0f) ? Color.white : new Color(1f, 0f, 1f, 0f));
		this.m_gpName.text = Localization.instance.Localize(statusEffect.m_name);
		if (num > 0f)
		{
			this.m_gpCooldown.text = StatusEffect.GetTimeString(num, false, false);
			return;
		}
		this.m_gpCooldown.text = Localization.instance.Localize("$hud_ready");
	}

	// Token: 0x060007A3 RID: 1955 RVA: 0x0003B5AC File Offset: 0x000397AC
	private void UpdateStatusEffects(List<StatusEffect> statusEffects)
	{
		if (this.m_statusEffects.Count != statusEffects.Count)
		{
			foreach (RectTransform rectTransform in this.m_statusEffects)
			{
				UnityEngine.Object.Destroy(rectTransform.gameObject);
			}
			this.m_statusEffects.Clear();
			for (int i = 0; i < statusEffects.Count; i++)
			{
				RectTransform rectTransform2 = UnityEngine.Object.Instantiate<RectTransform>(this.m_statusEffectTemplate, this.m_statusEffectListRoot);
				rectTransform2.gameObject.SetActive(true);
				rectTransform2.anchoredPosition = new Vector3(-4f - (float)i * this.m_statusEffectSpacing, 0f, 0f);
				this.m_statusEffects.Add(rectTransform2);
			}
		}
		for (int j = 0; j < statusEffects.Count; j++)
		{
			StatusEffect statusEffect = statusEffects[j];
			RectTransform rectTransform3 = this.m_statusEffects[j];
			Image component = rectTransform3.Find("Icon").GetComponent<Image>();
			component.sprite = statusEffect.m_icon;
			if (statusEffect.m_flashIcon)
			{
				component.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? new Color(1f, 0.5f, 0.5f, 1f) : Color.white);
			}
			else
			{
				component.color = Color.white;
			}
			rectTransform3.Find("Cooldown").gameObject.SetActive(statusEffect.m_cooldownIcon);
			rectTransform3.GetComponentInChildren<Text>().text = Localization.instance.Localize(statusEffect.m_name);
			Text component2 = rectTransform3.Find("TimeText").GetComponent<Text>();
			string iconText = statusEffect.GetIconText();
			if (!string.IsNullOrEmpty(iconText))
			{
				component2.gameObject.SetActive(true);
				component2.text = iconText;
			}
			else
			{
				component2.gameObject.SetActive(false);
			}
			if (statusEffect.m_isNew)
			{
				statusEffect.m_isNew = false;
				rectTransform3.GetComponentInChildren<Animator>().SetTrigger("flash");
			}
		}
	}

	// Token: 0x060007A4 RID: 1956 RVA: 0x0003B7D0 File Offset: 0x000399D0
	private void UpdateEvent(Player player)
	{
		RandomEvent activeEvent = RandEventSystem.instance.GetActiveEvent();
		if (activeEvent != null && !EnemyHud.instance.ShowingBossHud() && activeEvent.GetTime() > 3f)
		{
			this.m_eventBar.SetActive(true);
			this.m_eventName.text = Localization.instance.Localize(activeEvent.GetHudText());
			return;
		}
		this.m_eventBar.SetActive(false);
	}

	// Token: 0x060007A5 RID: 1957 RVA: 0x0003B838 File Offset: 0x00039A38
	public void ToggleBetaTextVisible()
	{
		this.m_betaText.SetActive(!this.m_betaText.activeSelf);
	}

	// Token: 0x060007A6 RID: 1958 RVA: 0x0003B853 File Offset: 0x00039A53
	public void FlashHealthBar()
	{
		this.m_healthAnimator.SetTrigger("Flash");
	}

	// Token: 0x060007A7 RID: 1959 RVA: 0x0003B865 File Offset: 0x00039A65
	public void StaminaBarUppgradeFlash()
	{
		this.m_staminaAnimator.SetTrigger("Flash");
	}

	// Token: 0x060007A8 RID: 1960 RVA: 0x0003B878 File Offset: 0x00039A78
	public void StaminaBarEmptyFlash()
	{
		this.m_staminaHideTimer = 0f;
		if (this.m_staminaAnimator.GetCurrentAnimatorStateInfo(0).IsTag("nostamina"))
		{
			return;
		}
		this.m_staminaAnimator.SetTrigger("NoStamina");
	}

	// Token: 0x060007A9 RID: 1961 RVA: 0x0003B8BC File Offset: 0x00039ABC
	public void EitrBarEmptyFlash()
	{
		this.m_eitrHideTimer = 0f;
		if (this.m_eitrAnimator.GetCurrentAnimatorStateInfo(0).IsTag("nostamina"))
		{
			return;
		}
		this.m_eitrAnimator.SetTrigger("NoStamina");
	}

	// Token: 0x060007AA RID: 1962 RVA: 0x0003B900 File Offset: 0x00039B00
	public void EitrBarUppgradeFlash()
	{
		this.m_eitrAnimator.SetTrigger("Flash");
	}

	// Token: 0x060007AB RID: 1963 RVA: 0x0003B912 File Offset: 0x00039B12
	public static bool IsUserHidden()
	{
		return Hud.m_instance && Hud.m_instance.m_userHidden;
	}

	// Token: 0x04000918 RID: 2328
	private static Hud m_instance;

	// Token: 0x04000919 RID: 2329
	public GameObject m_rootObject;

	// Token: 0x0400091A RID: 2330
	public Text m_buildSelection;

	// Token: 0x0400091B RID: 2331
	public Text m_pieceDescription;

	// Token: 0x0400091C RID: 2332
	public Image m_buildIcon;

	// Token: 0x0400091D RID: 2333
	[SerializeField]
	private Image m_snappingIcon;

	// Token: 0x0400091E RID: 2334
	[SerializeField]
	private Sprite m_buildSnappingIcon;

	// Token: 0x0400091F RID: 2335
	[SerializeField]
	private Sprite m_shipSnappingIcon;

	// Token: 0x04000920 RID: 2336
	[SerializeField]
	private Sprite m_hoeSnappingIcon;

	// Token: 0x04000921 RID: 2337
	public GameObject m_buildHud;

	// Token: 0x04000922 RID: 2338
	public GameObject m_saveIcon;

	// Token: 0x04000923 RID: 2339
	public Image m_saveIconImage;

	// Token: 0x04000924 RID: 2340
	public GameObject m_badConnectionIcon;

	// Token: 0x04000925 RID: 2341
	public GameObject m_betaText;

	// Token: 0x04000926 RID: 2342
	[Header("Piece")]
	public GameObject[] m_requirementItems = new GameObject[0];

	// Token: 0x04000927 RID: 2343
	public GameObject[] m_pieceCategoryTabs = new GameObject[0];

	// Token: 0x04000928 RID: 2344
	public GameObject m_pieceSelectionWindow;

	// Token: 0x04000929 RID: 2345
	public GameObject m_pieceCategoryRoot;

	// Token: 0x0400092A RID: 2346
	public RectTransform m_pieceListRoot;

	// Token: 0x0400092B RID: 2347
	public RectTransform m_pieceListMask;

	// Token: 0x0400092C RID: 2348
	public GameObject m_pieceIconPrefab;

	// Token: 0x0400092D RID: 2349
	public UIInputHandler m_closePieceSelectionButton;

	// Token: 0x0400092E RID: 2350
	public EffectList m_selectItemEffect = new EffectList();

	// Token: 0x0400092F RID: 2351
	public float m_pieceIconSpacing = 64f;

	// Token: 0x04000930 RID: 2352
	private float m_pieceBarPosX;

	// Token: 0x04000931 RID: 2353
	private float m_pieceBarTargetPosX;

	// Token: 0x04000932 RID: 2354
	private Piece.PieceCategory m_lastPieceCategory = Piece.PieceCategory.Max;

	// Token: 0x04000933 RID: 2355
	[Header("Health")]
	public RectTransform m_healthBarRoot;

	// Token: 0x04000934 RID: 2356
	public RectTransform m_healthPanel;

	// Token: 0x04000935 RID: 2357
	private const float m_healthPanelBuffer = 56f;

	// Token: 0x04000936 RID: 2358
	private const float m_healthPanelMinSize = 138f;

	// Token: 0x04000937 RID: 2359
	public Animator m_healthAnimator;

	// Token: 0x04000938 RID: 2360
	public GuiBar m_healthBarFast;

	// Token: 0x04000939 RID: 2361
	public GuiBar m_healthBarSlow;

	// Token: 0x0400093A RID: 2362
	public Text m_healthText;

	// Token: 0x0400093B RID: 2363
	[Header("Food")]
	public Image[] m_foodBars;

	// Token: 0x0400093C RID: 2364
	public Image[] m_foodIcons;

	// Token: 0x0400093D RID: 2365
	public Text[] m_foodTime;

	// Token: 0x0400093E RID: 2366
	public RectTransform m_foodBarRoot;

	// Token: 0x0400093F RID: 2367
	public RectTransform m_foodBaseBar;

	// Token: 0x04000940 RID: 2368
	public Image m_foodIcon;

	// Token: 0x04000941 RID: 2369
	public Color m_foodColorHungry = Color.white;

	// Token: 0x04000942 RID: 2370
	public Color m_foodColorFull = Color.white;

	// Token: 0x04000943 RID: 2371
	public Text m_foodText;

	// Token: 0x04000944 RID: 2372
	[Header("Action bar")]
	public GameObject m_actionBarRoot;

	// Token: 0x04000945 RID: 2373
	public GuiBar m_actionProgress;

	// Token: 0x04000946 RID: 2374
	public Text m_actionName;

	// Token: 0x04000947 RID: 2375
	[Header("Stagger bar")]
	public Animator m_staggerAnimator;

	// Token: 0x04000948 RID: 2376
	public GuiBar m_staggerProgress;

	// Token: 0x04000949 RID: 2377
	[Header("Guardian power")]
	public RectTransform m_gpRoot;

	// Token: 0x0400094A RID: 2378
	public Text m_gpName;

	// Token: 0x0400094B RID: 2379
	public Text m_gpCooldown;

	// Token: 0x0400094C RID: 2380
	public Image m_gpIcon;

	// Token: 0x0400094D RID: 2381
	[Header("Stamina")]
	public RectTransform m_staminaBar2Root;

	// Token: 0x0400094E RID: 2382
	public Animator m_staminaAnimator;

	// Token: 0x0400094F RID: 2383
	public GuiBar m_staminaBar2Fast;

	// Token: 0x04000950 RID: 2384
	public GuiBar m_staminaBar2Slow;

	// Token: 0x04000951 RID: 2385
	public Text m_staminaText;

	// Token: 0x04000952 RID: 2386
	private float m_staminaBarBorderBuffer = 16f;

	// Token: 0x04000953 RID: 2387
	[Header("Eitr")]
	public RectTransform m_eitrBarRoot;

	// Token: 0x04000954 RID: 2388
	public Animator m_eitrAnimator;

	// Token: 0x04000955 RID: 2389
	public GuiBar m_eitrBarFast;

	// Token: 0x04000956 RID: 2390
	public GuiBar m_eitrBarSlow;

	// Token: 0x04000957 RID: 2391
	public Text m_eitrText;

	// Token: 0x04000958 RID: 2392
	[Header("Mount")]
	public GameObject m_mountPanel;

	// Token: 0x04000959 RID: 2393
	public GuiBar m_mountHealthBarFast;

	// Token: 0x0400095A RID: 2394
	public GuiBar m_mountHealthBarSlow;

	// Token: 0x0400095B RID: 2395
	public TextMeshProUGUI m_mountHealthText;

	// Token: 0x0400095C RID: 2396
	public GuiBar m_mountStaminaBar;

	// Token: 0x0400095D RID: 2397
	public TextMeshProUGUI m_mountStaminaText;

	// Token: 0x0400095E RID: 2398
	public TextMeshProUGUI m_mountNameText;

	// Token: 0x0400095F RID: 2399
	[Header("Loading")]
	public CanvasGroup m_loadingScreen;

	// Token: 0x04000960 RID: 2400
	public GameObject m_loadingProgress;

	// Token: 0x04000961 RID: 2401
	public GameObject m_sleepingProgress;

	// Token: 0x04000962 RID: 2402
	public GameObject m_teleportingProgress;

	// Token: 0x04000963 RID: 2403
	public Image m_loadingImage;

	// Token: 0x04000964 RID: 2404
	public Text m_loadingTip;

	// Token: 0x04000965 RID: 2405
	public List<string> m_loadingTips = new List<string>();

	// Token: 0x04000966 RID: 2406
	[Header("Crosshair")]
	public Image m_crosshair;

	// Token: 0x04000967 RID: 2407
	public Image m_crosshairBow;

	// Token: 0x04000968 RID: 2408
	public TextMeshProUGUI m_hoverName;

	// Token: 0x04000969 RID: 2409
	public RectTransform m_pieceHealthRoot;

	// Token: 0x0400096A RID: 2410
	public GuiBar m_pieceHealthBar;

	// Token: 0x0400096B RID: 2411
	public Image m_damageScreen;

	// Token: 0x0400096C RID: 2412
	[Header("Target")]
	public GameObject m_targetedAlert;

	// Token: 0x0400096D RID: 2413
	public GameObject m_targeted;

	// Token: 0x0400096E RID: 2414
	public GameObject m_hidden;

	// Token: 0x0400096F RID: 2415
	public GuiBar m_stealthBar;

	// Token: 0x04000970 RID: 2416
	[Header("Status effect")]
	public RectTransform m_statusEffectListRoot;

	// Token: 0x04000971 RID: 2417
	public RectTransform m_statusEffectTemplate;

	// Token: 0x04000972 RID: 2418
	public float m_statusEffectSpacing = 55f;

	// Token: 0x04000973 RID: 2419
	private List<RectTransform> m_statusEffects = new List<RectTransform>();

	// Token: 0x04000974 RID: 2420
	[Header("Ship hud")]
	public GameObject m_shipHudRoot;

	// Token: 0x04000975 RID: 2421
	public GameObject m_shipControlsRoot;

	// Token: 0x04000976 RID: 2422
	public GameObject m_rudderLeft;

	// Token: 0x04000977 RID: 2423
	public GameObject m_rudderRight;

	// Token: 0x04000978 RID: 2424
	public GameObject m_rudderSlow;

	// Token: 0x04000979 RID: 2425
	public GameObject m_rudderForward;

	// Token: 0x0400097A RID: 2426
	public GameObject m_rudderFastForward;

	// Token: 0x0400097B RID: 2427
	public GameObject m_rudderBackward;

	// Token: 0x0400097C RID: 2428
	public GameObject m_halfSail;

	// Token: 0x0400097D RID: 2429
	public GameObject m_fullSail;

	// Token: 0x0400097E RID: 2430
	public GameObject m_rudder;

	// Token: 0x0400097F RID: 2431
	public RectTransform m_shipWindIndicatorRoot;

	// Token: 0x04000980 RID: 2432
	public Image m_shipWindIcon;

	// Token: 0x04000981 RID: 2433
	public RectTransform m_shipWindIconRoot;

	// Token: 0x04000982 RID: 2434
	public Image m_shipRudderIndicator;

	// Token: 0x04000983 RID: 2435
	public Image m_shipRudderIcon;

	// Token: 0x04000984 RID: 2436
	[Header("Event")]
	public GameObject m_eventBar;

	// Token: 0x04000985 RID: 2437
	public Text m_eventName;

	// Token: 0x04000986 RID: 2438
	[NonSerialized]
	public bool m_userHidden;

	// Token: 0x04000987 RID: 2439
	private CraftingStation m_currentCraftingStation;

	// Token: 0x04000988 RID: 2440
	private List<string> m_buildCategoryNames = new List<string>();

	// Token: 0x04000989 RID: 2441
	private List<StatusEffect> m_tempStatusEffects = new List<StatusEffect>();

	// Token: 0x0400098A RID: 2442
	private List<Hud.PieceIconData> m_pieceIcons = new List<Hud.PieceIconData>();

	// Token: 0x0400098B RID: 2443
	private int m_pieceIconUpdateIndex;

	// Token: 0x0400098C RID: 2444
	private bool m_haveSetupLoadScreen;

	// Token: 0x0400098D RID: 2445
	private float m_staggerHideTimer = 99999f;

	// Token: 0x0400098E RID: 2446
	private float m_staminaHideTimer = 99999f;

	// Token: 0x0400098F RID: 2447
	private float m_eitrHideTimer = 99999f;

	// Token: 0x04000990 RID: 2448
	private int m_closePieceSelection;

	// Token: 0x04000991 RID: 2449
	private Piece m_hoveredPiece;

	// Token: 0x04000992 RID: 2450
	private const float minimumSaveIconDisplayTime = 3f;

	// Token: 0x04000993 RID: 2451
	private float m_saveIconTimer;

	// Token: 0x04000994 RID: 2452
	private bool m_savingTriggered;

	// Token: 0x020000B3 RID: 179
	private class PieceIconData
	{
		// Token: 0x04000995 RID: 2453
		public GameObject m_go;

		// Token: 0x04000996 RID: 2454
		public Image m_icon;

		// Token: 0x04000997 RID: 2455
		public GameObject m_marker;

		// Token: 0x04000998 RID: 2456
		public GameObject m_upgrade;

		// Token: 0x04000999 RID: 2457
		public UITooltip m_tooltip;
	}
}
