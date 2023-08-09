using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020000DD RID: 221
public class Menu : MonoBehaviour
{
	// Token: 0x1700004C RID: 76
	// (get) Token: 0x060008FF RID: 2303 RVA: 0x00044A6F File Offset: 0x00042C6F
	public static Menu instance
	{
		get
		{
			return Menu.m_instance;
		}
	}

	// Token: 0x06000900 RID: 2304 RVA: 0x00044A78 File Offset: 0x00042C78
	private void Start()
	{
		Menu.m_instance = this;
		this.Hide();
		if (this.m_gamepadRoot)
		{
			this.m_gamepadRoot.gameObject.SetActive(false);
		}
		this.UpdateNavigation();
		this.m_rebuildLayout = true;
		ConnectedStorage.SavingFinished = (Action)Delegate.Remove(ConnectedStorage.SavingFinished, new Action(this.SavingFinished));
		ConnectedStorage.SavingFinished = (Action)Delegate.Combine(ConnectedStorage.SavingFinished, new Action(this.SavingFinished));
		PlayerProfile.SavingFinished = (Action)Delegate.Remove(PlayerProfile.SavingFinished, new Action(this.SavingFinished));
		PlayerProfile.SavingFinished = (Action)Delegate.Combine(PlayerProfile.SavingFinished, new Action(this.SavingFinished));
	}

	// Token: 0x06000901 RID: 2305 RVA: 0x00044B3C File Offset: 0x00042D3C
	private void UpdateNavigation()
	{
		Button component = this.m_menuDialog.Find("MenuEntries/Logout").GetComponent<Button>();
		Button component2 = this.m_menuDialog.Find("MenuEntries/Exit").GetComponent<Button>();
		Button component3 = this.m_menuDialog.Find("MenuEntries/Continue").GetComponent<Button>();
		Button component4 = this.m_menuDialog.Find("MenuEntries/Settings").GetComponent<Button>();
		this.m_firstMenuButton = component3;
		List<Button> list = new List<Button>();
		list.Add(component3);
		if (this.saveButton.interactable)
		{
			list.Add(this.saveButton);
		}
		if (this.menuCurrentPlayersListButton.gameObject.activeSelf)
		{
			list.Add(this.menuCurrentPlayersListButton);
		}
		list.Add(component4);
		list.Add(component);
		if (component2.gameObject.activeSelf)
		{
			list.Add(component2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			Navigation navigation = list[i].navigation;
			if (i > 0)
			{
				navigation.selectOnUp = list[i - 1];
			}
			else
			{
				navigation.selectOnUp = list[list.Count - 1];
			}
			if (i < list.Count - 1)
			{
				navigation.selectOnDown = list[i + 1];
			}
			else
			{
				navigation.selectOnDown = list[0];
			}
			list[i].navigation = navigation;
		}
	}

	// Token: 0x06000902 RID: 2306 RVA: 0x00044CB4 File Offset: 0x00042EB4
	private void OnDestroy()
	{
		ConnectedStorage.SavingFinished = (Action)Delegate.Remove(ConnectedStorage.SavingFinished, new Action(this.SavingFinished));
		PlayerProfile.SavingFinished = (Action)Delegate.Remove(PlayerProfile.SavingFinished, new Action(this.SavingFinished));
	}

	// Token: 0x06000903 RID: 2307 RVA: 0x00044D01 File Offset: 0x00042F01
	private void SavingFinished()
	{
		this.m_lastSavedDate = DateTime.Now;
		this.m_rebuildLayout = true;
	}

	// Token: 0x06000904 RID: 2308 RVA: 0x00044D18 File Offset: 0x00042F18
	public void Show()
	{
		Gogan.LogEvent("Screen", "Enter", "Menu", 0L);
		this.m_root.gameObject.SetActive(true);
		this.m_menuDialog.gameObject.SetActive(true);
		this.m_logoutDialog.gameObject.SetActive(false);
		this.m_quitDialog.gameObject.SetActive(false);
		this.menuCurrentPlayersListButton.gameObject.SetActive(false);
		this.UpdateNavigation();
		this.saveButton.gameObject.SetActive(true);
		this.lastSaveText.gameObject.SetActive(this.m_lastSavedDate > DateTime.MinValue);
		this.m_rebuildLayout = true;
		if (Player.m_localPlayer != null && !Player.m_localPlayer.InCutscene())
		{
			Game.Pause();
		}
		if (Chat.instance.IsChatDialogWindowVisible())
		{
			Chat.instance.Hide();
		}
		JoinCode.Show(false);
	}

	// Token: 0x06000905 RID: 2309 RVA: 0x00044E08 File Offset: 0x00043008
	private IEnumerator SelectEntry(GameObject entry)
	{
		yield return null;
		yield return null;
		EventSystem.current.SetSelectedGameObject(entry);
		yield break;
	}

	// Token: 0x06000906 RID: 2310 RVA: 0x00044E17 File Offset: 0x00043017
	public void Hide()
	{
		this.m_root.gameObject.SetActive(false);
		JoinCode.Hide();
		Game.Unpause();
	}

	// Token: 0x06000907 RID: 2311 RVA: 0x00044E34 File Offset: 0x00043034
	public static bool IsVisible()
	{
		return !(Menu.m_instance == null) && (Menu.m_instance.m_hiddenFrames <= 2 || UnifiedPopup.WasVisibleThisFrame());
	}

	// Token: 0x06000908 RID: 2312 RVA: 0x00044E5C File Offset: 0x0004305C
	private void Update()
	{
		if (Game.instance.IsShuttingDown())
		{
			this.Hide();
			return;
		}
		if (this.m_root.gameObject.activeSelf)
		{
			this.m_hiddenFrames = 0;
			if ((ZInput.GetKeyDown(KeyCode.Escape) || (ZInput.GetButtonDown("JoyMenu") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper"))) || ZInput.GetButtonDown("JoyButtonB")) && !this.m_settingsInstance && !this.m_currentPlayersInstance && !Feedback.IsVisible() && !UnifiedPopup.IsVisible())
			{
				if (this.m_quitDialog.gameObject.activeSelf)
				{
					this.OnQuitNo();
				}
				else if (this.m_logoutDialog.gameObject.activeSelf)
				{
					this.OnLogoutNo();
				}
				else
				{
					this.Hide();
				}
			}
			if (this.m_gamepadRoot)
			{
				if (ZInput.IsGamepadActive())
				{
					Settings.UpdateGamepadMap(this.m_gamepadRoot, ZInput.PlayStationGlyphs, ZInput.InputLayout, false);
				}
				this.m_gamepadRoot.SetActive(ZInput.IsGamepadActive());
			}
			if (!ZInput.IsGamepadActive() && base.gameObject.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null && this.m_firstMenuButton != null)
			{
				base.StartCoroutine(this.SelectEntry(this.m_firstMenuButton.gameObject));
			}
			if (this.m_lastSavedDate > DateTime.MinValue)
			{
				int minutes = (DateTime.Now - this.m_lastSavedDate).Minutes;
				string text = minutes.ToString();
				if (minutes < 1)
				{
					text = "<1";
				}
				this.lastSaveText.text = Localization.instance.Localize("$menu_manualsavetime", new string[]
				{
					text
				});
			}
			if ((this.saveButton.interactable && (float)this.m_manualSaveCooldownUntil > Time.unscaledTime) || (!this.saveButton.interactable && (float)this.m_manualSaveCooldownUntil < Time.unscaledTime))
			{
				this.saveButton.interactable = ((float)this.m_manualSaveCooldownUntil < Time.unscaledTime);
				this.UpdateNavigation();
			}
			if (this.m_rebuildLayout)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(this.menuEntriesParent);
				this.lastSaveText.gameObject.SetActive(this.m_lastSavedDate > DateTime.MinValue);
				this.m_rebuildLayout = false;
			}
		}
		else
		{
			this.m_hiddenFrames++;
			bool flag = !InventoryGui.IsVisible() && !Minimap.IsOpen() && !global::Console.IsVisible() && !TextInput.IsVisible() && !ZNet.instance.InPasswordDialog() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible() && !UnifiedPopup.IsVisible();
			if ((ZInput.GetKeyDown(KeyCode.Escape) || (ZInput.GetButtonDown("JoyMenu") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper")))) && flag && !Chat.instance.m_wasFocused)
			{
				this.Show();
			}
		}
		if (this.m_updateLocalizationTimer > 30)
		{
			Localization.instance.ReLocalizeVisible(base.transform);
			this.m_updateLocalizationTimer = 0;
			return;
		}
		this.m_updateLocalizationTimer++;
	}

	// Token: 0x06000909 RID: 2313 RVA: 0x0004517E File Offset: 0x0004337E
	public void OnSettings()
	{
		Gogan.LogEvent("Screen", "Enter", "Settings", 0L);
		this.m_settingsInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_settingsPrefab, base.transform);
	}

	// Token: 0x0600090A RID: 2314 RVA: 0x000451AD File Offset: 0x000433AD
	public void OnQuit()
	{
		this.m_quitDialog.gameObject.SetActive(true);
		this.m_menuDialog.gameObject.SetActive(false);
	}

	// Token: 0x0600090B RID: 2315 RVA: 0x000451D1 File Offset: 0x000433D1
	public void OnCurrentPlayers()
	{
		if (this.m_currentPlayersInstance == null)
		{
			this.m_currentPlayersInstance = UnityEngine.Object.Instantiate<GameObject>(Menu.CurrentPlayersPrefab, base.transform);
			return;
		}
		this.m_currentPlayersInstance.SetActive(true);
	}

	// Token: 0x0600090C RID: 2316 RVA: 0x00045204 File Offset: 0x00043404
	public void OnManualSave()
	{
		if ((float)this.m_manualSaveCooldownUntil >= Time.unscaledTime)
		{
			return;
		}
		if (this.ShouldShowCloudStorageWarning())
		{
			this.m_logoutDialog.gameObject.SetActive(false);
			this.ShowCloudStorageFullWarning(new Menu.CloudStorageFullOkCallback(this.Logout));
			return;
		}
		if (ZNet.instance != null)
		{
			World worldIfIsHost = ZNet.GetWorldIfIsHost();
			ZNet.instance.Save(worldIfIsHost != null);
			ZNet.instance.SendClientSave();
			ZNet.instance.ConsoleSave();
			this.m_manualSaveCooldownUntil = (int)Time.unscaledTime + 60;
		}
	}

	// Token: 0x0600090D RID: 2317 RVA: 0x00045290 File Offset: 0x00043490
	private bool ShouldShowCloudStorageWarning()
	{
		World worldIfIsHost = ZNet.GetWorldIfIsHost();
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		bool flag = worldIfIsHost != null && (worldIfIsHost.m_fileSource == FileHelpers.FileSource.Cloud || (FileHelpers.m_cloudEnabled && worldIfIsHost.m_fileSource == FileHelpers.FileSource.Legacy));
		bool flag2 = playerProfile != null && (playerProfile.m_fileSource == FileHelpers.FileSource.Cloud || (FileHelpers.m_cloudEnabled && playerProfile.m_fileSource == FileHelpers.FileSource.Legacy));
		if (flag || flag2)
		{
			ulong num = 0UL;
			if (flag)
			{
				string metaPath = worldIfIsHost.GetMetaPath(worldIfIsHost.m_fileSource);
				string dbpath = worldIfIsHost.GetDBPath(worldIfIsHost.m_fileSource);
				num += 104857600UL;
				if (FileHelpers.Exists(metaPath, worldIfIsHost.m_fileSource))
				{
					num += FileHelpers.GetFileSize(metaPath, worldIfIsHost.m_fileSource) * 2UL;
					if (FileHelpers.Exists(dbpath, worldIfIsHost.m_fileSource))
					{
						num += FileHelpers.GetFileSize(dbpath, worldIfIsHost.m_fileSource) * 2UL;
					}
				}
				else
				{
					ZLog.LogError("World save file doesn't exist! Using less accurate storage usage estimate.");
				}
			}
			if (flag2)
			{
				string path = playerProfile.GetPath();
				num += 2097152UL;
				if (FileHelpers.Exists(path, playerProfile.m_fileSource))
				{
					num += FileHelpers.GetFileSize(path, playerProfile.m_fileSource) * 2UL;
				}
				else
				{
					ZLog.LogError("Player save file doesn't exist! Using less accurate storage usage estimate.");
				}
			}
			return FileHelpers.OperationExceedsCloudCapacity(num);
		}
		return false;
	}

	// Token: 0x0600090E RID: 2318 RVA: 0x000453D3 File Offset: 0x000435D3
	public void OnQuitYes()
	{
		if (this.ShouldShowCloudStorageWarning())
		{
			this.m_quitDialog.gameObject.SetActive(false);
			this.ShowCloudStorageFullWarning(new Menu.CloudStorageFullOkCallback(this.QuitGame));
			return;
		}
		this.QuitGame();
	}

	// Token: 0x0600090F RID: 2319 RVA: 0x00045407 File Offset: 0x00043607
	private void QuitGame()
	{
		Gogan.LogEvent("Game", "Quit", "", 0L);
		Application.Quit();
	}

	// Token: 0x06000910 RID: 2320 RVA: 0x00045424 File Offset: 0x00043624
	public void OnQuitNo()
	{
		this.m_quitDialog.gameObject.SetActive(false);
		this.m_menuDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000911 RID: 2321 RVA: 0x00045448 File Offset: 0x00043648
	public void OnLogout()
	{
		this.m_menuDialog.gameObject.SetActive(false);
		this.m_logoutDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000912 RID: 2322 RVA: 0x0004546C File Offset: 0x0004366C
	public void OnLogoutYes()
	{
		if (this.ShouldShowCloudStorageWarning())
		{
			this.m_logoutDialog.gameObject.SetActive(false);
			this.ShowCloudStorageFullWarning(new Menu.CloudStorageFullOkCallback(this.Logout));
			return;
		}
		this.Logout();
	}

	// Token: 0x06000913 RID: 2323 RVA: 0x000454A0 File Offset: 0x000436A0
	public void Logout()
	{
		Gogan.LogEvent("Game", "LogOut", "", 0L);
		Game.instance.Logout();
	}

	// Token: 0x06000914 RID: 2324 RVA: 0x000454C2 File Offset: 0x000436C2
	public void OnLogoutNo()
	{
		this.m_logoutDialog.gameObject.SetActive(false);
		this.m_menuDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000915 RID: 2325 RVA: 0x000454E6 File Offset: 0x000436E6
	public void OnClose()
	{
		Gogan.LogEvent("Screen", "Exit", "Menu", 0L);
		this.Hide();
	}

	// Token: 0x06000916 RID: 2326 RVA: 0x00045504 File Offset: 0x00043704
	public void OnButtonFeedback()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.m_feedbackPrefab, base.transform);
	}

	// Token: 0x06000917 RID: 2327 RVA: 0x00045518 File Offset: 0x00043718
	public void ShowCloudStorageFullWarning(Menu.CloudStorageFullOkCallback okCallback)
	{
		if (this.m_cloudStorageWarningShown)
		{
			if (okCallback != null)
			{
				okCallback();
			}
			return;
		}
		if (okCallback != null)
		{
			this.cloudStorageFullOkCallbackList.Add(okCallback);
		}
		this.m_cloudStorageWarning.SetActive(true);
	}

	// Token: 0x06000918 RID: 2328 RVA: 0x00045548 File Offset: 0x00043748
	public void OnCloudStorageFullWarningOk()
	{
		int count = this.cloudStorageFullOkCallbackList.Count;
		while (count-- > 0)
		{
			this.cloudStorageFullOkCallbackList[count]();
		}
		this.cloudStorageFullOkCallbackList.Clear();
		this.m_cloudStorageWarningShown = true;
		this.m_cloudStorageWarning.SetActive(false);
	}

	// Token: 0x1700004D RID: 77
	// (get) Token: 0x06000919 RID: 2329 RVA: 0x0004559A File Offset: 0x0004379A
	// (set) Token: 0x0600091A RID: 2330 RVA: 0x000455A1 File Offset: 0x000437A1
	public static GameObject CurrentPlayersPrefab { get; set; }

	// Token: 0x1700004E RID: 78
	// (get) Token: 0x0600091B RID: 2331 RVA: 0x000455A9 File Offset: 0x000437A9
	public bool PlayerListActive
	{
		get
		{
			return this.m_currentPlayersInstance != null && this.m_currentPlayersInstance.activeSelf;
		}
	}

	// Token: 0x04000ADF RID: 2783
	private bool m_cloudStorageWarningShown;

	// Token: 0x04000AE0 RID: 2784
	private List<Menu.CloudStorageFullOkCallback> cloudStorageFullOkCallbackList = new List<Menu.CloudStorageFullOkCallback>();

	// Token: 0x04000AE2 RID: 2786
	private GameObject m_currentPlayersInstance;

	// Token: 0x04000AE3 RID: 2787
	public Button menuCurrentPlayersListButton;

	// Token: 0x04000AE4 RID: 2788
	private GameObject m_settingsInstance;

	// Token: 0x04000AE5 RID: 2789
	public Button saveButton;

	// Token: 0x04000AE6 RID: 2790
	public TMP_Text lastSaveText;

	// Token: 0x04000AE7 RID: 2791
	private DateTime m_lastSavedDate = DateTime.MinValue;

	// Token: 0x04000AE8 RID: 2792
	public RectTransform menuEntriesParent;

	// Token: 0x04000AE9 RID: 2793
	private static Menu m_instance;

	// Token: 0x04000AEA RID: 2794
	public Transform m_root;

	// Token: 0x04000AEB RID: 2795
	public Transform m_menuDialog;

	// Token: 0x04000AEC RID: 2796
	public Transform m_quitDialog;

	// Token: 0x04000AED RID: 2797
	public Transform m_logoutDialog;

	// Token: 0x04000AEE RID: 2798
	public GameObject m_cloudStorageWarning;

	// Token: 0x04000AEF RID: 2799
	public GameObject m_settingsPrefab;

	// Token: 0x04000AF0 RID: 2800
	public GameObject m_feedbackPrefab;

	// Token: 0x04000AF1 RID: 2801
	public GameObject m_gamepadRoot;

	// Token: 0x04000AF2 RID: 2802
	public GameObject m_gamepadTriggers;

	// Token: 0x04000AF3 RID: 2803
	private int m_hiddenFrames;

	// Token: 0x04000AF4 RID: 2804
	private int m_updateLocalizationTimer;

	// Token: 0x04000AF5 RID: 2805
	private int m_manualSaveCooldownUntil;

	// Token: 0x04000AF6 RID: 2806
	private const int ManualSavingCooldownTime = 60;

	// Token: 0x04000AF7 RID: 2807
	private bool m_rebuildLayout;

	// Token: 0x04000AF8 RID: 2808
	private Button m_firstMenuButton;

	// Token: 0x020000DE RID: 222
	// (Invoke) Token: 0x0600091E RID: 2334
	public delegate void CloudStorageFullOkCallback();
}
