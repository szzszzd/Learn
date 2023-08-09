using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x020001C0 RID: 448
public class FejdStartup : MonoBehaviour
{
	// Token: 0x170000BC RID: 188
	// (get) Token: 0x060011F5 RID: 4597 RVA: 0x00076BB0 File Offset: 0x00074DB0
	public static FejdStartup instance
	{
		get
		{
			return FejdStartup.m_instance;
		}
	}

	// Token: 0x060011F6 RID: 4598 RVA: 0x00076BB8 File Offset: 0x00074DB8
	private void Awake()
	{
		FejdStartup.m_instance = this;
		this.ParseArguments();
		this.m_crossplayServerToggle.gameObject.SetActive(true);
		if (!FejdStartup.AwakePlatforms())
		{
			return;
		}
		FileHelpers.UpdateCloudEnabledStatus();
		Settings.SetPlatformDefaultPrefs();
		QualitySettings.maxQueuedFrames = 2;
		ZLog.Log(string.Concat(new string[]
		{
			"Valheim version: ",
			global::Version.GetVersionString(false),
			" (network version ",
			5U.ToString(),
			")"
		}));
		Settings.ApplyStartupSettings();
		WorldGenerator.Initialize(World.GetMenuWorld());
		if (!global::Console.instance)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_consolePrefab);
		}
		this.m_mainCamera.transform.position = this.m_cameraMarkerMain.transform.position;
		this.m_mainCamera.transform.rotation = this.m_cameraMarkerMain.transform.rotation;
		ZLog.Log("Render threading mode:" + SystemInfo.renderingThreadingMode.ToString());
		Gogan.StartSession();
		Gogan.LogEvent("Game", "Version", global::Version.GetVersionString(false), 0L);
		Gogan.LogEvent("Game", "SteamID", SteamManager.APP_ID.ToString(), 0L);
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			Transform transform = this.m_menuList.transform.Find("Menu");
			if (transform != null)
			{
				Transform transform2 = transform.Find("showlog");
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(false);
				}
			}
		}
		this.m_menuButtons = this.m_menuList.GetComponentsInChildren<Button>();
		Game.Unpause();
		Time.timeScale = 1f;
		ZInput.Initialize();
	}

	// Token: 0x060011F7 RID: 4599 RVA: 0x00076D70 File Offset: 0x00074F70
	public static bool AwakePlatforms()
	{
		if (FejdStartup.s_monoUpdaters == null)
		{
			FejdStartup.s_monoUpdaters = new GameObject();
			FejdStartup.s_monoUpdaters.AddComponent<MonoUpdaters>();
			UnityEngine.Object.DontDestroyOnLoad(FejdStartup.s_monoUpdaters);
		}
		if (!FejdStartup.AwakeSteam() || !FejdStartup.AwakePlayFab() || !FejdStartup.AwakeCustom())
		{
			ZLog.LogError("Awake of network backend failed");
			return false;
		}
		return true;
	}

	// Token: 0x060011F8 RID: 4600 RVA: 0x00076DCC File Offset: 0x00074FCC
	private static bool AwakePlayFab()
	{
		PlayFabManager.Initialize();
		PlayFabManager.SetCustomId(PrivilegeManager.Platform.Steam, SteamUser.GetSteamID().ToString());
		return true;
	}

	// Token: 0x060011F9 RID: 4601 RVA: 0x00076DF8 File Offset: 0x00074FF8
	private static bool AwakeSteam()
	{
		return FejdStartup.InitializeSteam();
	}

	// Token: 0x060011FA RID: 4602 RVA: 0x0000290F File Offset: 0x00000B0F
	private static bool AwakeCustom()
	{
		return true;
	}

	// Token: 0x060011FB RID: 4603 RVA: 0x00076E04 File Offset: 0x00075004
	private void OnDestroy()
	{
		SaveSystem.ClearWorldListCache(false);
		FejdStartup.m_instance = null;
	}

	// Token: 0x060011FC RID: 4604 RVA: 0x00076E12 File Offset: 0x00075012
	private void OnEnable()
	{
		this.startGameEvent += this.AddToServerList;
	}

	// Token: 0x060011FD RID: 4605 RVA: 0x00076E26 File Offset: 0x00075026
	private void OnDisable()
	{
		this.startGameEvent -= this.AddToServerList;
	}

	// Token: 0x060011FE RID: 4606 RVA: 0x00076E3A File Offset: 0x0007503A
	private void AddToServerList(object sender, FejdStartup.StartGameEventArgs e)
	{
		if (!e.isHost)
		{
			ServerList.AddToRecentServersList(this.GetServerToJoin());
		}
	}

	// Token: 0x060011FF RID: 4607 RVA: 0x00076E50 File Offset: 0x00075050
	private void Start()
	{
		this.SetupGui();
		this.SetupObjectDB();
		this.m_openServerToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnOpenServerToggleClicked));
		MusicMan.instance.Reset();
		MusicMan.instance.TriggerMusic("menu");
		this.ShowConnectError(ZNet.ConnectionStatus.None);
		ZSteamMatchmaking.Initialize();
		if (FejdStartup.m_firstStartup)
		{
			this.HandleStartupJoin();
		}
		this.m_menuAnimator.SetBool("FirstStartup", FejdStartup.m_firstStartup);
		FejdStartup.m_firstStartup = false;
		string @string = PlayerPrefs.GetString("profile");
		if (@string.Length > 0)
		{
			this.SetSelectedProfile(@string);
		}
		else
		{
			this.m_profiles = SaveSystem.GetAllPlayerProfiles();
			if (this.m_profiles.Count > 0)
			{
				this.SetSelectedProfile(this.m_profiles[0].GetFilename());
			}
			else
			{
				this.UpdateCharacterList();
			}
		}
		SaveSystem.ClearWorldListCache(true);
		Player.m_debugMode = false;
	}

	// Token: 0x06001200 RID: 4608 RVA: 0x00076F34 File Offset: 0x00075134
	private void SetupGui()
	{
		this.HideAll();
		this.m_mainMenu.SetActive(true);
		if (SteamManager.APP_ID == 1223920U)
		{
			this.m_betaText.SetActive(true);
			if (!Debug.isDebugBuild && !this.AcceptedNDA())
			{
				this.m_ndaPanel.SetActive(true);
				this.m_mainMenu.SetActive(false);
			}
		}
		this.m_moddedText.SetActive(Game.isModded);
		this.m_worldListBaseSize = this.m_worldListRoot.rect.height;
		this.m_versionLabel.text = string.Format("Version {0} (n-{1})", global::Version.GetVersionString(false), 5U);
		Localization.instance.Localize(base.transform);
	}

	// Token: 0x06001201 RID: 4609 RVA: 0x00076FEC File Offset: 0x000751EC
	private void HideAll()
	{
		this.m_worldVersionPanel.SetActive(false);
		this.m_playerVersionPanel.SetActive(false);
		this.m_newGameVersionPanel.SetActive(false);
		this.m_loading.SetActive(false);
		this.m_pleaseWait.SetActive(false);
		this.m_characterSelectScreen.SetActive(false);
		this.m_creditsPanel.SetActive(false);
		this.m_startGamePanel.SetActive(false);
		this.m_createWorldPanel.SetActive(false);
		this.m_mainMenu.SetActive(false);
		this.m_ndaPanel.SetActive(false);
		this.m_betaText.SetActive(false);
	}

	// Token: 0x06001202 RID: 4610 RVA: 0x0007708C File Offset: 0x0007528C
	public static bool InitializeSteam()
	{
		if (SteamManager.Initialize())
		{
			string personaName = SteamFriends.GetPersonaName();
			ZLog.Log("Steam initialized, persona:" + personaName);
			FejdStartup.GenerateEncryptedAppTicket();
			PrivilegeManager.SetPrivilegeData(new PrivilegeData
			{
				platformUserId = (ulong)SteamUser.GetSteamID(),
				platformCanAccess = new CanAccessCallback(FejdStartup.OnSteamCanAccess),
				canAccessOnlineMultiplayer = true,
				canViewUserGeneratedContentAll = true,
				canCrossplay = true
			});
			return true;
		}
		ZLog.LogError("Steam is not initialized");
		Application.Quit();
		return false;
	}

	// Token: 0x06001203 RID: 4611 RVA: 0x00077118 File Offset: 0x00075318
	private static void GenerateEncryptedAppTicket()
	{
		FejdStartup.ticket = new byte[1024];
		uint cbDataToInclude;
		SteamUser.GetAuthSessionTicket(FejdStartup.ticket, FejdStartup.ticket.Length, out cbDataToInclude);
		FejdStartup.OnEncryptedAppTicketCallResult = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(FejdStartup.OnEncryptedAppTicketResponse));
		FejdStartup.OnEncryptedAppTicketCallResult.Set(SteamUser.RequestEncryptedAppTicket(FejdStartup.ticket, (int)cbDataToInclude), null);
	}

	// Token: 0x06001204 RID: 4612 RVA: 0x00077174 File Offset: 0x00075374
	private static void OnEncryptedAppTicketResponse(EncryptedAppTicketResponse_t param, bool bIOFailure)
	{
		if (param.m_eResult == EResult.k_EResultOK && !bIOFailure)
		{
			uint num;
			SteamUser.GetEncryptedAppTicket(null, 0, out num);
			if (num > 0U)
			{
				byte[] array = new byte[num];
				if (SteamUser.GetEncryptedAppTicket(array, (int)num, out num))
				{
					string str = "Ticket is ";
					byte[] array2 = array;
					ZLog.Log(str + ((array2 != null) ? array2.ToString() : null) + " of length " + num.ToString());
				}
			}
		}
	}

	// Token: 0x06001205 RID: 4613 RVA: 0x000771D8 File Offset: 0x000753D8
	private void HandleStartupJoin()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string a = commandLineArgs[i];
			if (a == "+connect" && i < commandLineArgs.Length - 1)
			{
				string text = commandLineArgs[i + 1];
				ZLog.Log("JOIN " + text);
				ZSteamMatchmaking.instance.QueueServerJoin(text);
			}
			else if (a == "+connect_lobby" && i < commandLineArgs.Length - 1)
			{
				string s = commandLineArgs[i + 1];
				CSteamID lobbyID = new CSteamID(ulong.Parse(s));
				ZSteamMatchmaking.instance.QueueLobbyJoin(lobbyID);
			}
		}
	}

	// Token: 0x06001206 RID: 4614 RVA: 0x0007726C File Offset: 0x0007546C
	private static void OnSteamCanAccess(PrivilegeManager.Permission permission, PrivilegeManager.User user, CanAccessResult cb)
	{
		if (user.platform == PrivilegeManager.Platform.Steam)
		{
			EFriendRelationship friendRelationship = SteamFriends.GetFriendRelationship((CSteamID)user.id);
			if (friendRelationship == EFriendRelationship.k_EFriendRelationshipIgnored || friendRelationship == EFriendRelationship.k_EFriendRelationshipIgnoredFriend)
			{
				cb(PrivilegeManager.Result.NotAllowed);
				return;
			}
		}
		cb(PrivilegeManager.Result.Allowed);
	}

	// Token: 0x06001207 RID: 4615 RVA: 0x000772AC File Offset: 0x000754AC
	private void ParseArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-console")
			{
				global::Console.SetConsoleEnabled(true);
			}
		}
	}

	// Token: 0x06001208 RID: 4616 RVA: 0x000772E4 File Offset: 0x000754E4
	private bool ParseServerArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string text = "Dedicated";
		string password = "";
		string text2 = "";
		int num = 2456;
		bool flag = true;
		ZNet.m_backupCount = 4;
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text3 = commandLineArgs[i].ToLower();
			int backupCount;
			int b;
			int b2;
			int b3;
			if (text3 == "-world")
			{
				string text4 = commandLineArgs[i + 1];
				if (text4 != "")
				{
					text = text4;
				}
				i++;
			}
			else if (text3 == "-name")
			{
				string text5 = commandLineArgs[i + 1];
				if (text5 != "")
				{
					text2 = text5;
				}
				i++;
			}
			else if (text3 == "-port")
			{
				string text6 = commandLineArgs[i + 1];
				if (text6 != "")
				{
					num = int.Parse(text6);
				}
				i++;
			}
			else if (text3 == "-password")
			{
				password = commandLineArgs[i + 1];
				i++;
			}
			else if (text3 == "-savedir")
			{
				string text7 = commandLineArgs[i + 1];
				Utils.SetSaveDataPath(text7);
				ZLog.Log("Setting -savedir to: " + text7);
				i++;
			}
			else if (text3 == "-public")
			{
				string a = commandLineArgs[i + 1];
				if (a != "")
				{
					flag = (a == "1");
				}
				i++;
			}
			else if (text3 == "-logfile")
			{
				ZLog.Log("Setting -logfile to: " + commandLineArgs[i + 1]);
			}
			else if (text3 == "-crossplay")
			{
				ZNet.m_onlineBackend = OnlineBackendType.PlayFab;
			}
			else if (text3 == "-instanceid" && commandLineArgs.Length > i + 1)
			{
				FejdStartup.InstanceId = commandLineArgs[i + 1];
				i++;
			}
			else if (text3.ToLower() == "-backups" && int.TryParse(commandLineArgs[i + 1], out backupCount))
			{
				ZNet.m_backupCount = backupCount;
			}
			else if (text3 == "-backupshort" && int.TryParse(commandLineArgs[i + 1], out b))
			{
				ZNet.m_backupShort = Mathf.Max(5, b);
			}
			else if (text3 == "-backuplong" && int.TryParse(commandLineArgs[i + 1], out b2))
			{
				ZNet.m_backupLong = Mathf.Max(5, b2);
			}
			else if (text3 == "-saveinterval" && int.TryParse(commandLineArgs[i + 1], out b3))
			{
				Game.m_saveInterval = (float)Mathf.Max(5, b3);
			}
		}
		if (text2 == "")
		{
			text2 = text;
		}
		World createWorld = World.GetCreateWorld(text, FileHelpers.FileSource.Local);
		if (flag && !this.IsPublicPasswordValid(password, createWorld))
		{
			string publicPasswordError = this.GetPublicPasswordError(password, createWorld);
			ZLog.LogError("Error bad password:" + publicPasswordError);
			Application.Quit();
			return false;
		}
		ZNet.SetServer(true, true, flag, text2, password, createWorld);
		ZNet.ResetServerHost();
		SteamManager.SetServerPort(num);
		ZSteamSocket.SetDataPort(num);
		ZPlayFabMatchmaking.SetDataPort(num);
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			ZPlayFabMatchmaking.LookupPublicIP();
		}
		return true;
	}

	// Token: 0x06001209 RID: 4617 RVA: 0x0007760C File Offset: 0x0007580C
	private void SetupObjectDB()
	{
		ObjectDB objectDB = base.gameObject.AddComponent<ObjectDB>();
		ObjectDB component = this.m_objectDBPrefab.GetComponent<ObjectDB>();
		objectDB.CopyOtherDB(component);
	}

	// Token: 0x0600120A RID: 4618 RVA: 0x00077638 File Offset: 0x00075838
	private void ShowConnectError(ZNet.ConnectionStatus statusOverride = ZNet.ConnectionStatus.None)
	{
		ZNet.ConnectionStatus connectionStatus = (statusOverride == ZNet.ConnectionStatus.None) ? ZNet.GetConnectionStatus() : statusOverride;
		if (ZNet.m_loadError)
		{
			this.m_connectionFailedPanel.SetActive(true);
			this.m_connectionFailedError.text = Localization.instance.Localize("$error_worldfileload");
		}
		if (ZNet.m_loadError)
		{
			this.m_connectionFailedPanel.SetActive(true);
			this.m_connectionFailedError.text = Localization.instance.Localize("$error_worldfileload");
		}
		if (connectionStatus != ZNet.ConnectionStatus.Connected && connectionStatus != ZNet.ConnectionStatus.Connecting && connectionStatus != ZNet.ConnectionStatus.None)
		{
			this.m_connectionFailedPanel.SetActive(true);
			switch (connectionStatus)
			{
			case ZNet.ConnectionStatus.ErrorVersion:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_incompatibleversion");
				return;
			case ZNet.ConnectionStatus.ErrorDisconnected:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_disconnected");
				return;
			case ZNet.ConnectionStatus.ErrorConnectFailed:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_failedconnect");
				return;
			case ZNet.ConnectionStatus.ErrorPassword:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_password");
				return;
			case ZNet.ConnectionStatus.ErrorAlreadyConnected:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_alreadyconnected");
				return;
			case ZNet.ConnectionStatus.ErrorBanned:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_banned");
				return;
			case ZNet.ConnectionStatus.ErrorFull:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_serverfull");
				return;
			case ZNet.ConnectionStatus.ErrorPlatformExcluded:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_platformexcluded");
				return;
			case ZNet.ConnectionStatus.ErrorCrossplayPrivilege:
				this.m_connectionFailedError.text = Localization.instance.Localize("$xbox_error_crossplayprivilege");
				return;
			case ZNet.ConnectionStatus.ErrorKicked:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_kicked");
				break;
			default:
				return;
			}
		}
	}

	// Token: 0x0600120B RID: 4619 RVA: 0x00077809 File Offset: 0x00075A09
	public void OnNewVersionButtonDownload()
	{
		Application.OpenURL(this.m_downloadUrl);
		Application.Quit();
	}

	// Token: 0x0600120C RID: 4620 RVA: 0x0007781B File Offset: 0x00075A1B
	public void OnNewVersionButtonContinue()
	{
		this.m_newGameVersionPanel.SetActive(false);
	}

	// Token: 0x0600120D RID: 4621 RVA: 0x00077829 File Offset: 0x00075A29
	public void OnStartGame()
	{
		Gogan.LogEvent("Screen", "Enter", "StartGame", 0L);
		this.m_mainMenu.SetActive(false);
		this.ShowCharacterSelection();
	}

	// Token: 0x0600120E RID: 4622 RVA: 0x00077853 File Offset: 0x00075A53
	private void ShowStartGame()
	{
		this.m_mainMenu.SetActive(false);
		this.m_startGamePanel.SetActive(true);
		this.m_createWorldPanel.SetActive(false);
	}

	// Token: 0x0600120F RID: 4623 RVA: 0x00077879 File Offset: 0x00075A79
	public void OnSelectWorldTab()
	{
		this.RefreshWorldSelection();
	}

	// Token: 0x06001210 RID: 4624 RVA: 0x00077884 File Offset: 0x00075A84
	private void RefreshWorldSelection()
	{
		this.UpdateWorldList(true);
		if (this.m_world != null)
		{
			this.m_world = this.FindWorld(this.m_world.m_name);
			if (this.m_world != null)
			{
				this.UpdateWorldList(true);
			}
		}
		if (this.m_world == null)
		{
			string @string = PlayerPrefs.GetString("world");
			if (@string.Length > 0)
			{
				this.m_world = this.FindWorld(@string);
			}
			if (this.m_world == null)
			{
				this.m_world = ((this.m_worlds.Count > 0) ? this.m_worlds[0] : null);
			}
			if (this.m_world != null)
			{
				this.UpdateWorldList(true);
			}
			this.m_crossplayServerToggle.isOn = (PlayerPrefs.GetInt("crossplay", 1) == 1);
		}
	}

	// Token: 0x06001211 RID: 4625 RVA: 0x00077946 File Offset: 0x00075B46
	public void OnServerListTab()
	{
		if (!PrivilegeManager.CanAccessOnlineMultiplayer)
		{
			this.m_startGamePanel.transform.GetChild(0).GetComponent<TabHandler>().SetActiveTab(0);
			this.ShowOnlineMultiplayerPrivilegeWarning();
		}
	}

	// Token: 0x06001212 RID: 4626 RVA: 0x00077971 File Offset: 0x00075B71
	private void OnOpenServerToggleClicked(bool value)
	{
		if (value && !PrivilegeManager.CanAccessOnlineMultiplayer)
		{
			this.m_openServerToggle.isOn = false;
			this.ShowOnlineMultiplayerPrivilegeWarning();
		}
	}

	// Token: 0x06001213 RID: 4627 RVA: 0x0007798F File Offset: 0x00075B8F
	private void ShowOnlineMultiplayerPrivilegeWarning()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_privilegerequiredheader", "$menu_onlineprivilegetext", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x06001214 RID: 4628 RVA: 0x000779C8 File Offset: 0x00075BC8
	private World FindWorld(string name)
	{
		foreach (World world in this.m_worlds)
		{
			if (world.m_name == name)
			{
				return world;
			}
		}
		return null;
	}

	// Token: 0x06001215 RID: 4629 RVA: 0x00077A2C File Offset: 0x00075C2C
	private void UpdateWorldList(bool centerSelection)
	{
		this.m_worlds = SaveSystem.GetWorldList();
		foreach (GameObject obj in this.m_worldListElements)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_worldListElements.Clear();
		float num = (float)this.m_worlds.Count * this.m_worldListElementStep;
		num = Mathf.Max(this.m_worldListBaseSize, num);
		this.m_worldListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
		for (int i = 0; i < this.m_worlds.Count; i++)
		{
			World world = this.m_worlds[i];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_worldListElement, this.m_worldListRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -this.m_worldListElementStep);
			gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnSelectWorld));
			Text component = gameObject.transform.Find("seed").GetComponent<Text>();
			component.text = "Seed: " + world.m_seedName;
			Text component2 = gameObject.transform.Find("name").GetComponent<Text>();
			if (world.m_name == world.m_fileName)
			{
				component2.text = world.m_name;
			}
			else
			{
				component2.text = world.m_name + " (" + world.m_fileName + ")";
			}
			Transform transform = gameObject.transform.Find("source_cloud");
			if (transform != null)
			{
				transform.gameObject.SetActive(world.m_fileSource == FileHelpers.FileSource.Cloud);
			}
			Transform transform2 = gameObject.transform.Find("source_local");
			if (transform2 != null)
			{
				transform2.gameObject.SetActive(world.m_fileSource == FileHelpers.FileSource.Local);
			}
			Transform transform3 = gameObject.transform.Find("source_legacy");
			if (transform3 != null)
			{
				transform3.gameObject.SetActive(world.m_fileSource == FileHelpers.FileSource.Legacy);
			}
			switch (world.m_dataError)
			{
			case World.SaveDataError.None:
				break;
			case World.SaveDataError.BadVersion:
				component.text = " [BAD VERSION]";
				break;
			case World.SaveDataError.LoadError:
				component.text = " [LOAD ERROR]";
				break;
			case World.SaveDataError.Corrupt:
				component.text = " [CORRUPT]";
				break;
			case World.SaveDataError.MissingMeta:
				component.text = " [MISSING META]";
				break;
			case World.SaveDataError.MissingDB:
				component.text = " [MISSING DB]";
				break;
			default:
				component.text = string.Format(" [{0}]", world.m_dataError);
				break;
			}
			RectTransform rectTransform = gameObject.transform.Find("selected") as RectTransform;
			bool flag = this.m_world != null && world.m_fileName == this.m_world.m_fileName;
			rectTransform.gameObject.SetActive(flag);
			if (flag && centerSelection)
			{
				this.m_worldListEnsureVisible.CenterOnItem(rectTransform);
			}
			this.m_worldListElements.Add(gameObject);
		}
		this.m_worldSourceInfo.text = "";
		this.m_worldSourceInfoPanel.SetActive(false);
		if (this.m_world != null)
		{
			this.m_worldSourceInfo.text = Localization.instance.Localize(((this.m_world.m_fileSource == FileHelpers.FileSource.Legacy) ? "$menu_legacynotice \n\n$menu_legacynotice_worlds \n\n" : "") + ((!FileHelpers.m_cloudEnabled) ? "$menu_cloudsavesdisabled" : ""));
			this.m_worldSourceInfoPanel.SetActive(this.m_worldSourceInfo.text.Length > 0);
		}
	}

	// Token: 0x06001216 RID: 4630 RVA: 0x00077DCC File Offset: 0x00075FCC
	public void OnWorldRemove()
	{
		if (this.m_world == null)
		{
			return;
		}
		this.m_removeWorldName.text = this.m_world.m_fileName;
		this.m_removeWorldDialog.SetActive(true);
	}

	// Token: 0x06001217 RID: 4631 RVA: 0x00077DFC File Offset: 0x00075FFC
	public void OnButtonRemoveWorldYes()
	{
		World.RemoveWorld(this.m_world.m_fileName, this.m_world.m_fileSource);
		this.m_world = null;
		this.m_worlds = SaveSystem.GetWorldList();
		this.SetSelectedWorld(0, true);
		this.m_removeWorldDialog.SetActive(false);
	}

	// Token: 0x06001218 RID: 4632 RVA: 0x00077E4A File Offset: 0x0007604A
	public void OnButtonRemoveWorldNo()
	{
		this.m_removeWorldDialog.SetActive(false);
	}

	// Token: 0x06001219 RID: 4633 RVA: 0x00077E58 File Offset: 0x00076058
	private void OnSelectWorld()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		int index = this.FindSelectedWorld(currentSelectedGameObject);
		this.SetSelectedWorld(index, false);
	}

	// Token: 0x0600121A RID: 4634 RVA: 0x00077E80 File Offset: 0x00076080
	private void SetSelectedWorld(int index, bool centerSelection)
	{
		if (this.m_worlds.Count > 0)
		{
			index = Mathf.Clamp(index, 0, this.m_worlds.Count - 1);
			this.m_world = this.m_worlds[index];
		}
		this.UpdateWorldList(centerSelection);
	}

	// Token: 0x0600121B RID: 4635 RVA: 0x00077EC0 File Offset: 0x000760C0
	private int GetSelectedWorld()
	{
		if (this.m_world == null)
		{
			return -1;
		}
		for (int i = 0; i < this.m_worlds.Count; i++)
		{
			if (this.m_worlds[i].m_fileName == this.m_world.m_fileName)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x0600121C RID: 4636 RVA: 0x00077F14 File Offset: 0x00076114
	private int FindSelectedWorld(GameObject button)
	{
		for (int i = 0; i < this.m_worldListElements.Count; i++)
		{
			if (this.m_worldListElements[i] == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x0600121D RID: 4637 RVA: 0x00077F4E File Offset: 0x0007614E
	private FileHelpers.FileSource GetMoveTarget(FileHelpers.FileSource source)
	{
		if (source == FileHelpers.FileSource.Cloud)
		{
			return FileHelpers.FileSource.Local;
		}
		return FileHelpers.FileSource.Cloud;
	}

	// Token: 0x0600121E RID: 4638 RVA: 0x00077F57 File Offset: 0x00076157
	public void OnWorldNew()
	{
		this.m_createWorldPanel.SetActive(true);
		this.m_newWorldName.text = "";
		this.m_newWorldSeed.text = World.GenerateSeed();
	}

	// Token: 0x0600121F RID: 4639 RVA: 0x00077F88 File Offset: 0x00076188
	public void OnNewWorldDone(bool forceLocal)
	{
		string text = this.m_newWorldName.text;
		string text2 = this.m_newWorldSeed.text;
		if (World.HaveWorld(text))
		{
			UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$menu_newworldalreadyexists"), Localization.instance.Localize("$menu_newworldalreadyexistsmessage", new string[]
			{
				text
			}), delegate()
			{
				UnifiedPopup.Pop();
			}, false));
			return;
		}
		this.m_world = new World(text, text2);
		this.m_world.m_fileSource = ((FileHelpers.m_cloudEnabled && !forceLocal) ? FileHelpers.FileSource.Cloud : FileHelpers.FileSource.Local);
		this.m_world.m_needsDB = false;
		if (this.m_world.m_fileSource == FileHelpers.FileSource.Cloud && FileHelpers.OperationExceedsCloudCapacity(2097152UL))
		{
			this.ShowCloudQuotaWorldDialog();
			ZLog.LogWarning("This operation may exceed the cloud save quota and has therefore been aborted! Prompt shown to user.");
			return;
		}
		this.m_world.SaveWorldMetaData(DateTime.Now);
		this.UpdateWorldList(true);
		this.ShowStartGame();
		Gogan.LogEvent("Menu", "NewWorld", text, 0L);
	}

	// Token: 0x06001220 RID: 4640 RVA: 0x00078095 File Offset: 0x00076295
	public void OnNewWorldBack()
	{
		this.ShowStartGame();
	}

	// Token: 0x06001221 RID: 4641 RVA: 0x000780A0 File Offset: 0x000762A0
	public void OnWorldStart()
	{
		if (this.m_world == null || this.m_startingWorld)
		{
			return;
		}
		switch (this.m_world.m_dataError)
		{
		case World.SaveDataError.None:
		{
			PlayerPrefs.SetString("world", this.m_world.m_name);
			if (this.m_crossplayServerToggle.IsInteractable())
			{
				PlayerPrefs.SetInt("crossplay", this.m_crossplayServerToggle.isOn ? 1 : 0);
			}
			bool isOn = this.m_publicServerToggle.isOn;
			bool isOn2 = this.m_openServerToggle.isOn;
			bool isOn3 = this.m_crossplayServerToggle.isOn;
			string text = this.m_serverPassword.text;
			OnlineBackendType onlineBackend = this.GetOnlineBackend(isOn3);
			if (isOn2 && onlineBackend == OnlineBackendType.PlayFab && !PlayFabManager.IsLoggedIn)
			{
				this.ContinueWhenLoggedInPopup(new FejdStartup.ContinueAction(this.OnWorldStart));
				return;
			}
			ZNet.m_onlineBackend = onlineBackend;
			ZSteamMatchmaking.instance.StopServerListing();
			this.m_startingWorld = true;
			ZNet.SetServer(true, isOn2, isOn, this.m_world.m_name, text, this.m_world);
			ZNet.ResetServerHost();
			string eventLabel = "open:" + isOn2.ToString() + ",public:" + isOn.ToString();
			Gogan.LogEvent("Menu", "WorldStart", eventLabel, 0L);
			FejdStartup.StartGameEventHandler startGameEventHandler = this.startGameEvent;
			if (startGameEventHandler != null)
			{
				startGameEventHandler(this, new FejdStartup.StartGameEventArgs(true));
			}
			this.TransitionToMainScene();
			return;
		}
		case World.SaveDataError.BadVersion:
			return;
		case World.SaveDataError.LoadError:
		case World.SaveDataError.Corrupt:
		{
			SaveWithBackups saveWithBackups;
			if (!SaveSystem.TryGetSaveByName(this.m_world.m_name, SaveDataType.World, out saveWithBackups))
			{
				UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$menu_checklogfile", new PopupButtonCallback(UnifiedPopup.Pop), true));
				ZLog.LogError("Failed to restore backup! Couldn't get world " + this.m_world.m_name + " by name from save system.");
				return;
			}
			if (saveWithBackups.IsDeleted)
			{
				UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$menu_checklogfile", new PopupButtonCallback(UnifiedPopup.Pop), true));
				ZLog.LogError("Failed to restore backup! World " + this.m_world.m_name + " retrieved from save system was deleted.");
				return;
			}
			if (SaveSystem.HasRestorableBackup(saveWithBackups))
			{
				this.<OnWorldStart>g__RestoreBackupPrompt|47_1(saveWithBackups);
				return;
			}
			UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$error_nosuitablebackupfound", new PopupButtonCallback(UnifiedPopup.Pop), true));
			return;
		}
		case World.SaveDataError.MissingMeta:
		{
			SaveWithBackups saveWithBackups2;
			if (!SaveSystem.TryGetSaveByName(this.m_world.m_name, SaveDataType.World, out saveWithBackups2))
			{
				UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$menu_checklogfile", new PopupButtonCallback(UnifiedPopup.Pop), true));
				ZLog.LogError("Failed to restore meta file! Couldn't get world " + this.m_world.m_name + " by name from save system.");
				return;
			}
			if (saveWithBackups2.IsDeleted)
			{
				UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$menu_checklogfile", new PopupButtonCallback(UnifiedPopup.Pop), true));
				ZLog.LogError("Failed to restore meta file! World " + this.m_world.m_name + " retrieved from save system was deleted.");
				return;
			}
			if (SaveSystem.HasBackupWithMeta(saveWithBackups2))
			{
				this.<OnWorldStart>g__RestoreMetaFromBackupPrompt|47_0(saveWithBackups2);
				return;
			}
			UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$error_nosuitablebackupfound", new PopupButtonCallback(UnifiedPopup.Pop), true));
			return;
		}
		default:
			return;
		}
	}

	// Token: 0x06001222 RID: 4642 RVA: 0x000783B0 File Offset: 0x000765B0
	private void ContinueWhenLoggedInPopup(FejdStartup.ContinueAction continueAction)
	{
		string headerText = Localization.instance.Localize("$menu_loginheader");
		string loggingInText = Localization.instance.Localize("$menu_logintext");
		string retryText = "";
		int previousRetryCountdown = -1;
		UnifiedPopup.Push(new CancelableTaskPopup(() => headerText, delegate()
		{
			if (PlayFabManager.CurrentLoginState == LoginState.WaitingForRetry)
			{
				int num = Mathf.CeilToInt((float)(PlayFabManager.NextRetryUtc - DateTime.UtcNow).TotalSeconds);
				if (previousRetryCountdown != num)
				{
					previousRetryCountdown = num;
					retryText = Localization.instance.Localize("$menu_loginfailedtext") + "\n" + Localization.instance.Localize("$menu_loginretrycountdowntext", new string[]
					{
						num.ToString()
					});
				}
				return retryText;
			}
			return loggingInText;
		}, delegate()
		{
			if (PlayFabManager.IsLoggedIn)
			{
				FejdStartup.ContinueAction continueAction2 = continueAction;
				if (continueAction2 != null)
				{
					continueAction2();
				}
			}
			return PlayFabManager.IsLoggedIn;
		}, delegate()
		{
			UnifiedPopup.Pop();
		}));
	}

	// Token: 0x06001223 RID: 4643 RVA: 0x00078454 File Offset: 0x00076654
	private OnlineBackendType GetOnlineBackend(bool crossplayServer)
	{
		OnlineBackendType result = OnlineBackendType.Steamworks;
		if (crossplayServer)
		{
			result = OnlineBackendType.PlayFab;
		}
		return result;
	}

	// Token: 0x06001224 RID: 4644 RVA: 0x0007846C File Offset: 0x0007666C
	private void ShowCharacterSelection()
	{
		Gogan.LogEvent("Screen", "Enter", "CharacterSelection", 0L);
		ZLog.Log("show character selection");
		this.m_characterSelectScreen.SetActive(true);
		this.m_selectCharacterPanel.SetActive(true);
		this.m_newCharacterPanel.SetActive(false);
	}

	// Token: 0x06001225 RID: 4645 RVA: 0x000784BD File Offset: 0x000766BD
	public void OnJoinStart()
	{
		this.JoinServer();
	}

	// Token: 0x06001226 RID: 4646 RVA: 0x000784C8 File Offset: 0x000766C8
	public void JoinServer()
	{
		if (!PlayFabManager.IsLoggedIn && this.m_joinServer.m_joinData is ServerJoinDataPlayFabUser)
		{
			this.ContinueWhenLoggedInPopup(new FejdStartup.ContinueAction(this.JoinServer));
			return;
		}
		if (!PrivilegeManager.CanAccessOnlineMultiplayer)
		{
			ZLog.LogWarning("You should always prevent JoinServer() from being called when user does not have online multiplayer privilege!");
			this.HideAll();
			this.m_mainMenu.SetActive(true);
			this.ShowOnlineMultiplayerPrivilegeWarning();
			return;
		}
		if (this.m_joinServer.OnlineStatus == OnlineStatus.Online && this.m_joinServer.m_networkVersion != 5U)
		{
			UnifiedPopup.Push(new WarningPopup("$error_incompatibleversion", (5U < this.m_joinServer.m_networkVersion) ? "$error_needslocalupdatetojoin" : "$error_needsserverupdatetojoin", delegate()
			{
				UnifiedPopup.Pop();
			}, true));
			return;
		}
		if (this.m_joinServer.PlatformRestriction != PrivilegeManager.Platform.Unknown && !this.m_joinServer.IsJoinable)
		{
			if (this.m_joinServer.IsCrossplay)
			{
				UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$error_failedconnect"), Localization.instance.Localize("$error_crossplayprivilege"), delegate()
				{
					UnifiedPopup.Pop();
				}, false));
				return;
			}
			if (!this.m_joinServer.IsRestrictedToOwnPlatform)
			{
				UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$error_failedconnect"), Localization.instance.Localize("$error_platformexcluded"), delegate()
				{
					UnifiedPopup.Pop();
				}, false));
				return;
			}
			ZLog.LogWarning("This part of the code should be unreachable unless the way ServerStatus works has been changed. The connection should've been prevented but it will be tried anyway.");
		}
		ZNet.SetServer(false, false, false, "", "", null);
		bool flag = false;
		if (this.m_joinServer.m_joinData is ServerJoinDataSteamUser)
		{
			ZNet.SetServerHost((ulong)(this.m_joinServer.m_joinData as ServerJoinDataSteamUser).m_joinUserID);
			flag = true;
		}
		if (this.m_joinServer.m_joinData is ServerJoinDataPlayFabUser)
		{
			ZNet.SetServerHost((this.m_joinServer.m_joinData as ServerJoinDataPlayFabUser).m_remotePlayerId);
			flag = true;
		}
		if (this.m_joinServer.m_joinData is ServerJoinDataDedicated)
		{
			ServerJoinDataDedicated serverJoin = this.m_joinServer.m_joinData as ServerJoinDataDedicated;
			if (serverJoin.IsValid())
			{
				if (PlayFabManager.IsLoggedIn)
				{
					ZNet.ResetServerHost();
					ZPlayFabMatchmaking.FindHostByIp(serverJoin.GetIPPortString(), delegate(PlayFabMatchmakingServerData result)
					{
						if (result != null)
						{
							ZNet.SetServerHost(result.remotePlayerId);
							ZLog.Log("Determined backend of dedicated server to be PlayFab");
							return;
						}
						FejdStartup.retries = 50;
					}, delegate(ZPLayFabMatchmakingFailReason failReason)
					{
						ZNet.SetServerHost(serverJoin.GetIPString(), (int)serverJoin.m_port, OnlineBackendType.Steamworks);
						ZLog.Log("Determined backend of dedicated server to be Steamworks");
					}, true);
				}
				else
				{
					ZNet.SetServerHost(serverJoin.GetIPString(), (int)serverJoin.m_port, OnlineBackendType.Steamworks);
					ZLog.Log("Determined backend of dedicated server to be Steamworks");
				}
				flag = true;
			}
			else
			{
				flag = false;
			}
		}
		if (!flag)
		{
			Debug.LogError("Couldn't set the server host!");
			return;
		}
		Gogan.LogEvent("Menu", "JoinServer", "", 0L);
		FejdStartup.StartGameEventHandler startGameEventHandler = this.startGameEvent;
		if (startGameEventHandler != null)
		{
			startGameEventHandler(this, new FejdStartup.StartGameEventArgs(false));
		}
		this.TransitionToMainScene();
	}

	// Token: 0x06001227 RID: 4647 RVA: 0x000787D2 File Offset: 0x000769D2
	public void OnStartGameBack()
	{
		this.m_startGamePanel.SetActive(false);
		this.ShowCharacterSelection();
	}

	// Token: 0x06001228 RID: 4648 RVA: 0x000787E8 File Offset: 0x000769E8
	public void OnCredits()
	{
		this.m_creditsPanel.SetActive(true);
		this.m_mainMenu.SetActive(false);
		Gogan.LogEvent("Screen", "Enter", "Credits", 0L);
		this.m_creditsList.anchoredPosition = new Vector2(0f, 0f);
	}

	// Token: 0x06001229 RID: 4649 RVA: 0x0007883D File Offset: 0x00076A3D
	public void OnCreditsBack()
	{
		this.m_mainMenu.SetActive(true);
		this.m_creditsPanel.SetActive(false);
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
	}

	// Token: 0x0600122A RID: 4650 RVA: 0x0007886D File Offset: 0x00076A6D
	public void OnSelelectCharacterBack()
	{
		this.m_characterSelectScreen.SetActive(false);
		this.m_mainMenu.SetActive(true);
		this.m_queuedJoinServer = null;
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
	}

	// Token: 0x0600122B RID: 4651 RVA: 0x000788A4 File Offset: 0x00076AA4
	public void OnAbort()
	{
		Application.Quit();
	}

	// Token: 0x0600122C RID: 4652 RVA: 0x000788AB File Offset: 0x00076AAB
	public void OnWorldVersionYes()
	{
		this.m_worldVersionPanel.SetActive(false);
	}

	// Token: 0x0600122D RID: 4653 RVA: 0x000788B9 File Offset: 0x00076AB9
	public void OnPlayerVersionOk()
	{
		this.m_playerVersionPanel.SetActive(false);
	}

	// Token: 0x0600122E RID: 4654 RVA: 0x000788C7 File Offset: 0x00076AC7
	private void FixedUpdate()
	{
		ZInput.FixedUpdate(Time.fixedDeltaTime);
	}

	// Token: 0x0600122F RID: 4655 RVA: 0x000788D3 File Offset: 0x00076AD3
	private void UpdateCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = ZInput.IsMouseActive();
	}

	// Token: 0x06001230 RID: 4656 RVA: 0x000788E8 File Offset: 0x00076AE8
	private void Update()
	{
		int num = (Settings.FPSLimit != 29) ? Mathf.Min(Settings.FPSLimit, 60) : 60;
		Application.targetFrameRate = ((Settings.ReduceBackgroundUsage && !Application.isFocused) ? Mathf.Min(30, num) : num);
		if (Terminal.m_showTests)
		{
			Terminal.m_testList["fps limit"] = Application.targetFrameRate.ToString();
		}
		ZInput.Update(Time.deltaTime);
		this.UpdateCursor();
		Localization.instance.ReLocalizeVisible(base.transform);
		this.UpdateGamepad();
		this.UpdateKeyboard();
		this.CheckPendingSteamJoinRequest();
		if (MasterClient.instance != null)
		{
			MasterClient.instance.Update(Time.deltaTime);
		}
		if (ZBroastcast.instance != null)
		{
			ZBroastcast.instance.Update(Time.deltaTime);
		}
		this.UpdateCharacterRotation(Time.deltaTime);
		this.UpdateCamera(Time.deltaTime);
		if (this.m_newCharacterPanel.activeInHierarchy)
		{
			this.m_csNewCharacterDone.interactable = (this.m_csNewCharacterName.text.Length >= 3);
			Navigation navigation = this.m_csNewCharacterName.navigation;
			navigation.selectOnDown = (this.m_csNewCharacterDone.interactable ? this.m_csNewCharacterDone : this.m_csNewCharacterCancel);
			this.m_csNewCharacterName.navigation = navigation;
		}
		if (this.m_newCharacterPanel.activeInHierarchy)
		{
			this.m_csNewCharacterDone.interactable = (this.m_csNewCharacterName.text.Length >= 3);
		}
		if (this.m_createWorldPanel.activeInHierarchy)
		{
			this.m_newWorldDone.interactable = (this.m_newWorldName.text.Length >= 5);
		}
		if (this.m_startGamePanel.activeInHierarchy)
		{
			this.m_worldStart.interactable = this.CanStartServer();
			this.m_worldRemove.interactable = (this.m_world != null);
			this.UpdatePasswordError();
		}
		if (this.m_startGamePanel.activeInHierarchy)
		{
			bool flag = this.m_openServerToggle.isOn && this.m_openServerToggle.interactable;
			this.SetToggleState(this.m_publicServerToggle, flag);
			this.SetToggleState(this.m_crossplayServerToggle, flag);
			this.m_serverPassword.interactable = flag;
		}
		if (this.m_creditsPanel.activeInHierarchy)
		{
			RectTransform rectTransform = this.m_creditsList.parent as RectTransform;
			Vector3[] array = new Vector3[4];
			this.m_creditsList.GetWorldCorners(array);
			Vector3[] array2 = new Vector3[4];
			rectTransform.GetWorldCorners(array2);
			float num2 = array2[1].y - array2[0].y;
			if ((double)array[3].y < (double)num2 * 0.5)
			{
				Vector3 position = this.m_creditsList.position;
				position.y += Time.deltaTime * this.m_creditsSpeed * num2;
				this.m_creditsList.position = position;
			}
		}
	}

	// Token: 0x06001231 RID: 4657 RVA: 0x00078BC1 File Offset: 0x00076DC1
	private void OnGUI()
	{
		ZInput.OnGUI();
	}

	// Token: 0x06001232 RID: 4658 RVA: 0x00078BC8 File Offset: 0x00076DC8
	private void SetToggleState(Toggle toggle, bool active)
	{
		toggle.interactable = active;
		Color toggleColor = this.m_toggleColor;
		Graphic componentInChildren = toggle.GetComponentInChildren<Text>();
		if (!active)
		{
			float num = 0.5f;
			float num2 = toggleColor.linear.r * 0.2126f + toggleColor.linear.g * 0.7152f + toggleColor.linear.b * 0.0722f;
			num2 *= num;
			toggleColor.r = (toggleColor.g = (toggleColor.b = Mathf.LinearToGammaSpace(num2)));
		}
		componentInChildren.color = toggleColor;
	}

	// Token: 0x06001233 RID: 4659 RVA: 0x00078C56 File Offset: 0x00076E56
	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.F11))
		{
			GameCamera.ScreenShot();
		}
	}

	// Token: 0x06001234 RID: 4660 RVA: 0x00078C6C File Offset: 0x00076E6C
	private void UpdateKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.Return) && this.m_menuList.activeInHierarchy && !this.m_passwordError.gameObject.activeInHierarchy)
		{
			if (this.m_menuSelectedButton != null)
			{
				this.m_menuSelectedButton.OnSubmit(null);
			}
			else
			{
				this.OnStartGame();
			}
		}
		if (this.m_worldListPanel.GetComponent<UIGamePad>().IsBlocked())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (this.m_worldListPanel.activeInHierarchy)
			{
				this.SetSelectedWorld(this.GetSelectedWorld() - 1, true);
			}
			if (this.m_menuList.activeInHierarchy)
			{
				if (this.m_menuSelectedButton == null)
				{
					this.m_menuSelectedButton = this.m_menuButtons[0];
					this.m_menuSelectedButton.Select();
				}
				else
				{
					for (int i = 1; i < this.m_menuButtons.Length; i++)
					{
						if (this.m_menuButtons[i] == this.m_menuSelectedButton)
						{
							this.m_menuSelectedButton = this.m_menuButtons[i - 1];
							this.m_menuSelectedButton.Select();
							break;
						}
					}
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (this.m_worldListPanel.activeInHierarchy)
			{
				this.SetSelectedWorld(this.GetSelectedWorld() + 1, true);
			}
			if (this.m_menuList.activeInHierarchy)
			{
				if (this.m_menuSelectedButton == null)
				{
					this.m_menuSelectedButton = this.m_menuButtons[0];
					this.m_menuSelectedButton.Select();
					return;
				}
				for (int j = 0; j < this.m_menuButtons.Length - 1; j++)
				{
					if (this.m_menuButtons[j] == this.m_menuSelectedButton)
					{
						this.m_menuSelectedButton = this.m_menuButtons[j + 1];
						this.m_menuSelectedButton.Select();
						return;
					}
				}
			}
		}
	}

	// Token: 0x06001235 RID: 4661 RVA: 0x00078E24 File Offset: 0x00077024
	private void UpdateGamepad()
	{
		if (ZInput.IsGamepadActive() && this.m_menuList.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null && this.m_menuButtons != null && this.m_menuButtons.Length != 0)
		{
			base.StartCoroutine(this.SelectFirstMenuEntry(this.m_menuButtons[0]));
		}
		if (!ZInput.IsGamepadActive() || this.m_worldListPanel.GetComponent<UIGamePad>().IsBlocked())
		{
			return;
		}
		if (this.m_worldListPanel.activeInHierarchy)
		{
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				this.SetSelectedWorld(this.GetSelectedWorld() + 1, true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				this.SetSelectedWorld(this.GetSelectedWorld() - 1, true);
			}
		}
		if (this.m_characterSelectScreen.activeInHierarchy && !this.m_newCharacterPanel.activeInHierarchy && this.m_csLeftButton.interactable && ZInput.GetButtonDown("JoyDPadLeft"))
		{
			this.OnCharacterLeft();
		}
		if (this.m_characterSelectScreen.activeInHierarchy && !this.m_newCharacterPanel.activeInHierarchy && this.m_csRightButton.interactable && ZInput.GetButtonDown("JoyDPadRight"))
		{
			this.OnCharacterRight();
		}
		if (this.m_patchLogScroll.gameObject.activeInHierarchy)
		{
			this.m_patchLogScroll.value -= ZInput.GetJoyRightStickY() * 0.02f;
		}
	}

	// Token: 0x06001236 RID: 4662 RVA: 0x00078F93 File Offset: 0x00077193
	private IEnumerator SelectFirstMenuEntry(Button button)
	{
		if (Event.current != null)
		{
			Event.current.Use();
		}
		yield return null;
		yield return null;
		if (UnifiedPopup.IsVisible())
		{
			UnifiedPopup.SetFocus();
			yield break;
		}
		this.m_menuSelectedButton = button;
		this.m_menuSelectedButton.Select();
		yield break;
	}

	// Token: 0x06001237 RID: 4663 RVA: 0x00078FAC File Offset: 0x000771AC
	private void CheckPendingSteamJoinRequest()
	{
		ServerJoinData queuedJoinServer;
		if (ZSteamMatchmaking.instance != null && ZSteamMatchmaking.instance.GetJoinHost(out queuedJoinServer))
		{
			if (PrivilegeManager.CanAccessOnlineMultiplayer)
			{
				this.m_queuedJoinServer = queuedJoinServer;
				if (this.m_serverListPanel.activeInHierarchy)
				{
					this.m_joinServer = new ServerStatus(this.m_queuedJoinServer);
					this.m_queuedJoinServer = null;
					this.JoinServer();
					return;
				}
				this.HideAll();
				this.ShowCharacterSelection();
				return;
			}
			else
			{
				this.ShowOnlineMultiplayerPrivilegeWarning();
			}
		}
	}

	// Token: 0x06001238 RID: 4664 RVA: 0x0007901C File Offset: 0x0007721C
	private void UpdateCharacterRotation(float dt)
	{
		if (this.m_playerInstance == null)
		{
			return;
		}
		if (!this.m_characterSelectScreen.activeInHierarchy)
		{
			return;
		}
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			float axis = Input.GetAxis("Mouse X");
			this.m_playerInstance.transform.Rotate(0f, -axis * this.m_characterRotateSpeed, 0f);
		}
		float joyRightStickX = ZInput.GetJoyRightStickX();
		if (joyRightStickX != 0f)
		{
			this.m_playerInstance.transform.Rotate(0f, -joyRightStickX * this.m_characterRotateSpeedGamepad * dt, 0f);
		}
	}

	// Token: 0x06001239 RID: 4665 RVA: 0x000790BC File Offset: 0x000772BC
	private void UpdatePasswordError()
	{
		string text = "";
		if (this.NeedPassword())
		{
			text = this.GetPublicPasswordError(this.m_serverPassword.text, this.m_world);
		}
		this.m_passwordError.text = text;
	}

	// Token: 0x0600123A RID: 4666 RVA: 0x000790FB File Offset: 0x000772FB
	private bool NeedPassword()
	{
		return (this.m_publicServerToggle.isOn | this.m_crossplayServerToggle.isOn) & this.m_openServerToggle.isOn;
	}

	// Token: 0x0600123B RID: 4667 RVA: 0x00079120 File Offset: 0x00077320
	private string GetPublicPasswordError(string password, World world)
	{
		if (password.Length < this.m_minimumPasswordLength)
		{
			return Localization.instance.Localize("$menu_passwordshort");
		}
		if (world != null && (world.m_name.Contains(password) || world.m_seedName.Contains(password)))
		{
			return Localization.instance.Localize("$menu_passwordinvalid");
		}
		return "";
	}

	// Token: 0x0600123C RID: 4668 RVA: 0x0007917F File Offset: 0x0007737F
	private bool IsPublicPasswordValid(string password, World world)
	{
		return password.Length >= this.m_minimumPasswordLength && !world.m_name.Contains(password) && !world.m_seedName.Contains(password);
	}

	// Token: 0x0600123D RID: 4669 RVA: 0x000791B4 File Offset: 0x000773B4
	private bool CanStartServer()
	{
		if (this.m_world == null)
		{
			return false;
		}
		switch (this.m_world.m_dataError)
		{
		case World.SaveDataError.None:
		case World.SaveDataError.LoadError:
		case World.SaveDataError.Corrupt:
		case World.SaveDataError.MissingMeta:
			return !this.NeedPassword() || this.IsPublicPasswordValid(this.m_serverPassword.text, this.m_world);
		default:
			return false;
		}
	}

	// Token: 0x0600123E RID: 4670 RVA: 0x00079218 File Offset: 0x00077418
	private void UpdateCamera(float dt)
	{
		Transform transform = this.m_cameraMarkerMain;
		if (this.m_characterSelectScreen.activeSelf)
		{
			transform = this.m_cameraMarkerCharacter;
		}
		else if (this.m_creditsPanel.activeSelf)
		{
			transform = this.m_cameraMarkerCredits;
		}
		else if (this.m_startGamePanel.activeSelf)
		{
			transform = this.m_cameraMarkerGame;
		}
		else if (this.m_manageSavesMenu.IsVisible())
		{
			transform = this.m_cameraMarkerSaves;
		}
		this.m_mainCamera.transform.position = Vector3.SmoothDamp(this.m_mainCamera.transform.position, transform.position, ref this.camSpeed, 1.5f, 1000f, dt);
		Vector3 forward = Vector3.SmoothDamp(this.m_mainCamera.transform.forward, transform.forward, ref this.camRotSpeed, 1.5f, 1000f, dt);
		forward.Normalize();
		this.m_mainCamera.transform.rotation = Quaternion.LookRotation(forward);
	}

	// Token: 0x0600123F RID: 4671 RVA: 0x00079308 File Offset: 0x00077508
	public void ShowCloudQuotaWarning()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_cloudstoragefull", "$menu_cloudstoragefulloperationfailed", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x06001240 RID: 4672 RVA: 0x00079340 File Offset: 0x00077540
	public void ShowCloudQuotaWorldDialog()
	{
		UnifiedPopup.Push(new YesNoPopup("$menu_cloudstoragefull", "$menu_cloudstoragefullworldprompt", delegate()
		{
			UnifiedPopup.Pop();
			this.OnNewWorldDone(true);
		}, delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x06001241 RID: 4673 RVA: 0x00079390 File Offset: 0x00077590
	public void ShowCloudQuotaCharacterDialog()
	{
		UnifiedPopup.Push(new YesNoPopup("$menu_cloudstoragefull", "$menu_cloudstoragefullcharacterprompt", delegate()
		{
			UnifiedPopup.Pop();
			this.OnNewCharacterDone(true);
		}, delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x06001242 RID: 4674 RVA: 0x000793E0 File Offset: 0x000775E0
	public void OnManageSaves(int index)
	{
		this.HideAll();
		if (index == 0)
		{
			this.m_manageSavesMenu.Open(SaveDataType.World, (this.m_world != null) ? this.m_world.m_fileName : null, new ManageSavesMenu.ClosedCallback(this.ShowStartGame), new ManageSavesMenu.SavesModifiedCallback(this.OnSavesModified));
			return;
		}
		if (index != 1)
		{
			return;
		}
		this.m_manageSavesMenu.Open(SaveDataType.Character, (this.m_profileIndex >= 0 && this.m_profileIndex < this.m_profiles.Count && this.m_profiles[this.m_profileIndex] != null) ? this.m_profiles[this.m_profileIndex].m_filename : null, new ManageSavesMenu.ClosedCallback(this.ShowCharacterSelection), new ManageSavesMenu.SavesModifiedCallback(this.OnSavesModified));
	}

	// Token: 0x06001243 RID: 4675 RVA: 0x000794A4 File Offset: 0x000776A4
	private void OnSavesModified(SaveDataType dataType)
	{
		if (dataType == SaveDataType.World)
		{
			SaveSystem.ClearWorldListCache(true);
			this.RefreshWorldSelection();
			return;
		}
		if (dataType != SaveDataType.Character)
		{
			return;
		}
		string selectedProfile = null;
		if (this.m_profileIndex < this.m_profiles.Count && this.m_profileIndex >= 0)
		{
			selectedProfile = this.m_profiles[this.m_profileIndex].GetFilename();
		}
		this.m_profiles = SaveSystem.GetAllPlayerProfiles();
		this.SetSelectedProfile(selectedProfile);
		this.m_manageSavesMenu.Open(dataType, new ManageSavesMenu.ClosedCallback(this.ShowCharacterSelection), new ManageSavesMenu.SavesModifiedCallback(this.OnSavesModified));
	}

	// Token: 0x06001244 RID: 4676 RVA: 0x00079534 File Offset: 0x00077734
	private void UpdateCharacterList()
	{
		if (this.m_profiles == null)
		{
			this.m_profiles = SaveSystem.GetAllPlayerProfiles();
		}
		if (this.m_profileIndex >= this.m_profiles.Count)
		{
			this.m_profileIndex = this.m_profiles.Count - 1;
		}
		this.m_csRemoveButton.gameObject.SetActive(this.m_profiles.Count > 0);
		this.m_csStartButton.gameObject.SetActive(this.m_profiles.Count > 0);
		this.m_csNewButton.gameObject.SetActive(this.m_profiles.Count > 0);
		this.m_csNewBigButton.gameObject.SetActive(this.m_profiles.Count == 0);
		this.m_csLeftButton.interactable = (this.m_profileIndex > 0);
		this.m_csRightButton.interactable = (this.m_profileIndex < this.m_profiles.Count - 1);
		if (this.m_profileIndex >= 0 && this.m_profileIndex < this.m_profiles.Count)
		{
			PlayerProfile playerProfile = this.m_profiles[this.m_profileIndex];
			if (playerProfile.GetName().ToLower() == playerProfile.m_filename.ToLower())
			{
				this.m_csName.text = playerProfile.GetName();
			}
			else
			{
				this.m_csName.text = playerProfile.GetName() + " (" + playerProfile.m_filename + ")";
			}
			this.m_csName.gameObject.SetActive(true);
			this.m_csFileSource.gameObject.SetActive(true);
			this.m_csFileSource.text = Localization.instance.Localize(FileHelpers.GetSourceString(playerProfile.m_fileSource));
			this.m_csSourceInfo.text = Localization.instance.Localize(((playerProfile.m_fileSource == FileHelpers.FileSource.Legacy) ? "$menu_legacynotice \n\n" : "") + ((!FileHelpers.m_cloudEnabled) ? "$menu_cloudsavesdisabled" : ""));
			Transform transform = this.m_csFileSource.transform.Find("source_cloud");
			if (transform != null)
			{
				transform.gameObject.SetActive(playerProfile.m_fileSource == FileHelpers.FileSource.Cloud);
			}
			Transform transform2 = this.m_csFileSource.transform.Find("source_local");
			if (transform2 != null)
			{
				transform2.gameObject.SetActive(playerProfile.m_fileSource == FileHelpers.FileSource.Local);
			}
			Transform transform3 = this.m_csFileSource.transform.Find("source_legacy");
			if (transform3 != null)
			{
				transform3.gameObject.SetActive(playerProfile.m_fileSource == FileHelpers.FileSource.Legacy);
			}
			this.SetupCharacterPreview(playerProfile);
			return;
		}
		this.m_csName.gameObject.SetActive(false);
		this.m_csFileSource.gameObject.SetActive(false);
		this.ClearCharacterPreview();
	}

	// Token: 0x06001245 RID: 4677 RVA: 0x000797F0 File Offset: 0x000779F0
	private void SetSelectedProfile(string filename)
	{
		if (this.m_profiles == null)
		{
			this.m_profiles = SaveSystem.GetAllPlayerProfiles();
		}
		this.m_profileIndex = 0;
		if (filename != null)
		{
			for (int i = 0; i < this.m_profiles.Count; i++)
			{
				if (this.m_profiles[i].GetFilename() == filename)
				{
					this.m_profileIndex = i;
					break;
				}
			}
		}
		this.UpdateCharacterList();
	}

	// Token: 0x06001246 RID: 4678 RVA: 0x00079858 File Offset: 0x00077A58
	public void OnNewCharacterDone(bool forceLocal)
	{
		string text = this.m_csNewCharacterName.text;
		string text2 = text.ToLower();
		PlayerProfile playerProfile = new PlayerProfile(text2, FileHelpers.FileSource.Auto);
		if (forceLocal)
		{
			playerProfile.m_fileSource = FileHelpers.FileSource.Local;
		}
		if (playerProfile.m_fileSource == FileHelpers.FileSource.Cloud && FileHelpers.OperationExceedsCloudCapacity(1048576UL * 3UL))
		{
			this.ShowCloudQuotaCharacterDialog();
			ZLog.LogWarning("The character save operation may exceed the cloud save quota and has therefore been aborted! Prompt shown to user.");
			return;
		}
		if (PlayerProfile.HaveProfile(text2))
		{
			this.m_newCharacterError.SetActive(true);
			return;
		}
		Player component = this.m_playerInstance.GetComponent<Player>();
		component.GiveDefaultItems();
		playerProfile.SetName(text);
		playerProfile.SavePlayerData(component);
		playerProfile.Save();
		this.m_selectCharacterPanel.SetActive(true);
		this.m_newCharacterPanel.SetActive(false);
		this.m_profiles = null;
		this.SetSelectedProfile(text2);
		Gogan.LogEvent("Menu", "NewCharacter", text, 0L);
	}

	// Token: 0x06001247 RID: 4679 RVA: 0x00079928 File Offset: 0x00077B28
	public void OnNewCharacterCancel()
	{
		this.m_selectCharacterPanel.SetActive(true);
		this.m_newCharacterPanel.SetActive(false);
		this.UpdateCharacterList();
	}

	// Token: 0x06001248 RID: 4680 RVA: 0x00079948 File Offset: 0x00077B48
	public void OnCharacterNew()
	{
		this.m_newCharacterPanel.SetActive(true);
		this.m_selectCharacterPanel.SetActive(false);
		this.m_csNewCharacterName.text = "";
		this.m_newCharacterError.SetActive(false);
		this.SetupCharacterPreview(null);
		Gogan.LogEvent("Screen", "Enter", "CreateCharacter", 0L);
	}

	// Token: 0x06001249 RID: 4681 RVA: 0x000799A8 File Offset: 0x00077BA8
	public void OnCharacterRemove()
	{
		if (this.m_profileIndex < 0 || this.m_profileIndex >= this.m_profiles.Count)
		{
			return;
		}
		PlayerProfile playerProfile = this.m_profiles[this.m_profileIndex];
		this.m_removeCharacterName.text = playerProfile.GetName() + " (" + Localization.instance.Localize(FileHelpers.GetSourceString(playerProfile.m_fileSource)) + ")";
		this.m_tempRemoveCharacterName = playerProfile.GetFilename();
		this.m_tempRemoveCharacterSource = playerProfile.m_fileSource;
		this.m_tempRemoveCharacterIndex = this.m_profileIndex;
		this.m_removeCharacterDialog.SetActive(true);
	}

	// Token: 0x0600124A RID: 4682 RVA: 0x00079A49 File Offset: 0x00077C49
	public void OnButtonRemoveCharacterYes()
	{
		ZLog.Log("Remove character");
		PlayerProfile.RemoveProfile(this.m_tempRemoveCharacterName, this.m_tempRemoveCharacterSource);
		this.m_profiles.RemoveAt(this.m_tempRemoveCharacterIndex);
		this.UpdateCharacterList();
		this.m_removeCharacterDialog.SetActive(false);
	}

	// Token: 0x0600124B RID: 4683 RVA: 0x00079A89 File Offset: 0x00077C89
	public void OnButtonRemoveCharacterNo()
	{
		this.m_removeCharacterDialog.SetActive(false);
	}

	// Token: 0x0600124C RID: 4684 RVA: 0x00079A97 File Offset: 0x00077C97
	public void OnCharacterLeft()
	{
		if (this.m_profileIndex > 0)
		{
			this.m_profileIndex--;
		}
		this.UpdateCharacterList();
	}

	// Token: 0x0600124D RID: 4685 RVA: 0x00079AB6 File Offset: 0x00077CB6
	public void OnCharacterRight()
	{
		if (this.m_profileIndex < this.m_profiles.Count - 1)
		{
			this.m_profileIndex++;
		}
		this.UpdateCharacterList();
	}

	// Token: 0x0600124E RID: 4686 RVA: 0x00079AE4 File Offset: 0x00077CE4
	public void OnCharacterStart()
	{
		ZLog.Log("OnCharacterStart");
		if (this.m_profileIndex < 0 || this.m_profileIndex >= this.m_profiles.Count)
		{
			return;
		}
		PlayerProfile playerProfile = this.m_profiles[this.m_profileIndex];
		PlayerPrefs.SetString("profile", playerProfile.GetFilename());
		Game.SetProfile(playerProfile.GetFilename(), playerProfile.m_fileSource);
		this.m_characterSelectScreen.SetActive(false);
		if (this.m_queuedJoinServer != null)
		{
			this.m_joinServer = new ServerStatus(this.m_queuedJoinServer);
			this.m_queuedJoinServer = null;
			this.JoinServer();
			return;
		}
		this.ShowStartGame();
		if (this.m_worlds.Count == 0)
		{
			this.OnWorldNew();
		}
	}

	// Token: 0x0600124F RID: 4687 RVA: 0x00079B9D File Offset: 0x00077D9D
	private void TransitionToMainScene()
	{
		this.m_menuAnimator.SetTrigger("FadeOut");
		FejdStartup.retries = 0;
		base.Invoke("LoadMainSceneIfBackendSelected", 1.5f);
	}

	// Token: 0x06001250 RID: 4688 RVA: 0x00079BC8 File Offset: 0x00077DC8
	private void LoadMainSceneIfBackendSelected()
	{
		if (this.m_startingWorld || ZNet.HasServerHost())
		{
			ZLog.Log("Loading main scene");
			this.LoadMainScene();
			return;
		}
		FejdStartup.retries++;
		if (FejdStartup.retries > 50)
		{
			ZLog.Log("Max retries reached, reloading startup scene with connection error");
			ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorConnectFailed);
			SceneManager.LoadScene("start");
			return;
		}
		base.Invoke("LoadMainSceneIfBackendSelected", 0.25f);
		ZLog.Log("Backend not retreived yet, checking again in 0.25 seconds...");
	}

	// Token: 0x06001251 RID: 4689 RVA: 0x00079C3F File Offset: 0x00077E3F
	private void LoadMainScene()
	{
		this.m_loading.SetActive(true);
		SceneManager.LoadScene("main");
		this.m_startingWorld = false;
	}

	// Token: 0x06001252 RID: 4690 RVA: 0x00079C60 File Offset: 0x00077E60
	public void OnButtonSettings()
	{
		this.m_mainMenu.SetActive(false);
		this.m_settingsPopup = UnityEngine.Object.Instantiate<GameObject>(this.m_settingsPrefab, base.transform);
		this.m_settingsPopup.GetComponent<Settings>().SettingsPopupDestroyed += delegate()
		{
			this.m_mainMenu.SetActive(true);
		};
	}

	// Token: 0x06001253 RID: 4691 RVA: 0x00079CAC File Offset: 0x00077EAC
	public void OnButtonFeedback()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.m_feedbackPrefab, base.transform);
	}

	// Token: 0x06001254 RID: 4692 RVA: 0x00079CC0 File Offset: 0x00077EC0
	public void OnButtonTwitter()
	{
		Application.OpenURL("https://twitter.com/valheimgame");
	}

	// Token: 0x06001255 RID: 4693 RVA: 0x00079CCC File Offset: 0x00077ECC
	public void OnButtonWebPage()
	{
		Application.OpenURL("http://valheimgame.com/");
	}

	// Token: 0x06001256 RID: 4694 RVA: 0x00079CD8 File Offset: 0x00077ED8
	public void OnButtonDiscord()
	{
		Application.OpenURL("https://discord.gg/44qXMJH");
	}

	// Token: 0x06001257 RID: 4695 RVA: 0x00079CE4 File Offset: 0x00077EE4
	public void OnButtonFacebook()
	{
		Application.OpenURL("https://www.facebook.com/valheimgame/");
	}

	// Token: 0x06001258 RID: 4696 RVA: 0x00079CF0 File Offset: 0x00077EF0
	public void OnButtonShowLog()
	{
		Application.OpenURL(Application.persistentDataPath + "/");
	}

	// Token: 0x06001259 RID: 4697 RVA: 0x00079D06 File Offset: 0x00077F06
	private bool AcceptedNDA()
	{
		return PlayerPrefs.GetInt("accepted_nda", 0) == 1;
	}

	// Token: 0x0600125A RID: 4698 RVA: 0x00079D16 File Offset: 0x00077F16
	public void OnButtonNDAAccept()
	{
		PlayerPrefs.SetInt("accepted_nda", 1);
		this.m_ndaPanel.SetActive(false);
		this.m_mainMenu.SetActive(true);
	}

	// Token: 0x0600125B RID: 4699 RVA: 0x000788A4 File Offset: 0x00076AA4
	public void OnButtonNDADecline()
	{
		Application.Quit();
	}

	// Token: 0x0600125C RID: 4700 RVA: 0x00079D3B File Offset: 0x00077F3B
	public void OnConnectionFailedOk()
	{
		this.m_connectionFailedPanel.SetActive(false);
	}

	// Token: 0x0600125D RID: 4701 RVA: 0x00079D49 File Offset: 0x00077F49
	public Player GetPreviewPlayer()
	{
		if (this.m_playerInstance != null)
		{
			return this.m_playerInstance.GetComponent<Player>();
		}
		return null;
	}

	// Token: 0x0600125E RID: 4702 RVA: 0x00079D68 File Offset: 0x00077F68
	private void ClearCharacterPreview()
	{
		if (this.m_playerInstance)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_changeEffectPrefab, this.m_characterPreviewPoint.position, this.m_characterPreviewPoint.rotation);
			UnityEngine.Object.Destroy(this.m_playerInstance);
			this.m_playerInstance = null;
		}
	}

	// Token: 0x0600125F RID: 4703 RVA: 0x00079DB8 File Offset: 0x00077FB8
	private void SetupCharacterPreview(PlayerProfile profile)
	{
		this.ClearCharacterPreview();
		ZNetView.m_forceDisableInit = true;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_playerPrefab, this.m_characterPreviewPoint.position, this.m_characterPreviewPoint.rotation);
		ZNetView.m_forceDisableInit = false;
		UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
		Animator[] componentsInChildren = gameObject.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateMode = AnimatorUpdateMode.Normal;
		}
		Player component = gameObject.GetComponent<Player>();
		if (profile != null)
		{
			try
			{
				profile.LoadPlayerData(component);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Error loading player data: " + profile.GetPath() + ", error: " + ex.Message);
			}
		}
		this.m_playerInstance = gameObject;
	}

	// Token: 0x06001260 RID: 4704 RVA: 0x00079E74 File Offset: 0x00078074
	public void SetServerToJoin(ServerStatus serverData)
	{
		this.m_joinServer = serverData;
	}

	// Token: 0x06001261 RID: 4705 RVA: 0x00079E7D File Offset: 0x0007807D
	public bool HasServerToJoin()
	{
		return this.m_joinServer != null;
	}

	// Token: 0x06001262 RID: 4706 RVA: 0x00079E88 File Offset: 0x00078088
	public ServerJoinData GetServerToJoin()
	{
		if (this.m_joinServer == null)
		{
			return null;
		}
		return this.m_joinServer.m_joinData;
	}

	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06001263 RID: 4707 RVA: 0x00079EA0 File Offset: 0x000780A0
	// (remove) Token: 0x06001264 RID: 4708 RVA: 0x00079ED8 File Offset: 0x000780D8
	public event FejdStartup.StartGameEventHandler startGameEvent;

	// Token: 0x170000BD RID: 189
	// (get) Token: 0x06001265 RID: 4709 RVA: 0x00079F0D File Offset: 0x0007810D
	// (set) Token: 0x06001266 RID: 4710 RVA: 0x00079F14 File Offset: 0x00078114
	public static string InstanceId { get; private set; } = null;

	// Token: 0x06001269 RID: 4713 RVA: 0x0007A000 File Offset: 0x00078200
	[CompilerGenerated]
	private void <OnWorldStart>g__RestoreMetaFromBackupPrompt|47_0(SaveWithBackups saveToRestore)
	{
		UnifiedPopup.Push(new YesNoPopup("$menu_restorebackup", "$menu_missingmetarestore", delegate()
		{
			UnifiedPopup.Pop();
			SaveSystem.RestoreBackupResult restoreBackupResult = SaveSystem.RestoreMetaFromMostRecentBackup(saveToRestore.PrimaryFile);
			switch (restoreBackupResult)
			{
			case SaveSystem.RestoreBackupResult.Success:
				this.RefreshWorldSelection();
				return;
			case SaveSystem.RestoreBackupResult.NoBackup:
				UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$error_nosuitablebackupfound", new PopupButtonCallback(UnifiedPopup.Pop), true));
				return;
			}
			UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$menu_checklogfile", new PopupButtonCallback(UnifiedPopup.Pop), true));
			ZLog.LogError(string.Format("Failed to restore meta file! Result: {0}", restoreBackupResult));
		}, new PopupButtonCallback(UnifiedPopup.Pop), true));
	}

	// Token: 0x0600126A RID: 4714 RVA: 0x0007A050 File Offset: 0x00078250
	[CompilerGenerated]
	private void <OnWorldStart>g__RestoreBackupPrompt|47_1(SaveWithBackups saveToRestore)
	{
		UnifiedPopup.Push(new YesNoPopup("$menu_restorebackup", "$menu_corruptsaverestore", delegate()
		{
			UnifiedPopup.Pop();
			SaveSystem.RestoreBackupResult restoreBackupResult = SaveSystem.RestoreMostRecentBackup(saveToRestore);
			switch (restoreBackupResult)
			{
			case SaveSystem.RestoreBackupResult.Success:
				SaveSystem.ClearWorldListCache(true);
				this.RefreshWorldSelection();
				return;
			case SaveSystem.RestoreBackupResult.NoBackup:
				UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$error_nosuitablebackupfound", new PopupButtonCallback(UnifiedPopup.Pop), true));
				return;
			}
			UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$menu_checklogfile", new PopupButtonCallback(UnifiedPopup.Pop), true));
			ZLog.LogError(string.Format("Failed to restore backup! Result: {0}", restoreBackupResult));
		}, new PopupButtonCallback(UnifiedPopup.Pop), true));
	}

	// Token: 0x040012D9 RID: 4825
	private static CallResult<EncryptedAppTicketResponse_t> OnEncryptedAppTicketCallResult;

	// Token: 0x040012DA RID: 4826
	private static byte[] ticket;

	// Token: 0x040012DB RID: 4827
	private Vector3 camSpeed = Vector3.zero;

	// Token: 0x040012DC RID: 4828
	private Vector3 camRotSpeed = Vector3.zero;

	// Token: 0x040012DD RID: 4829
	private const int maxRetries = 50;

	// Token: 0x040012DE RID: 4830
	private static int retries = 0;

	// Token: 0x040012DF RID: 4831
	private static FejdStartup m_instance;

	// Token: 0x040012E0 RID: 4832
	[Header("Start")]
	public Animator m_menuAnimator;

	// Token: 0x040012E1 RID: 4833
	public GameObject m_worldVersionPanel;

	// Token: 0x040012E2 RID: 4834
	public GameObject m_playerVersionPanel;

	// Token: 0x040012E3 RID: 4835
	public GameObject m_newGameVersionPanel;

	// Token: 0x040012E4 RID: 4836
	public GameObject m_connectionFailedPanel;

	// Token: 0x040012E5 RID: 4837
	public Text m_connectionFailedError;

	// Token: 0x040012E6 RID: 4838
	public Text m_newVersionName;

	// Token: 0x040012E7 RID: 4839
	public GameObject m_loading;

	// Token: 0x040012E8 RID: 4840
	public GameObject m_pleaseWait;

	// Token: 0x040012E9 RID: 4841
	public Text m_versionLabel;

	// Token: 0x040012EA RID: 4842
	public GameObject m_mainMenu;

	// Token: 0x040012EB RID: 4843
	public GameObject m_ndaPanel;

	// Token: 0x040012EC RID: 4844
	public GameObject m_betaText;

	// Token: 0x040012ED RID: 4845
	public GameObject m_moddedText;

	// Token: 0x040012EE RID: 4846
	public Scrollbar m_patchLogScroll;

	// Token: 0x040012EF RID: 4847
	public GameObject m_characterSelectScreen;

	// Token: 0x040012F0 RID: 4848
	public GameObject m_selectCharacterPanel;

	// Token: 0x040012F1 RID: 4849
	public GameObject m_newCharacterPanel;

	// Token: 0x040012F2 RID: 4850
	public GameObject m_creditsPanel;

	// Token: 0x040012F3 RID: 4851
	public GameObject m_startGamePanel;

	// Token: 0x040012F4 RID: 4852
	public GameObject m_createWorldPanel;

	// Token: 0x040012F5 RID: 4853
	public GameObject m_menuList;

	// Token: 0x040012F6 RID: 4854
	private Button[] m_menuButtons;

	// Token: 0x040012F7 RID: 4855
	private Button m_menuSelectedButton;

	// Token: 0x040012F8 RID: 4856
	public RectTransform m_creditsList;

	// Token: 0x040012F9 RID: 4857
	public float m_creditsSpeed = 100f;

	// Token: 0x040012FA RID: 4858
	[Header("Camera")]
	public GameObject m_mainCamera;

	// Token: 0x040012FB RID: 4859
	public Transform m_cameraMarkerStart;

	// Token: 0x040012FC RID: 4860
	public Transform m_cameraMarkerMain;

	// Token: 0x040012FD RID: 4861
	public Transform m_cameraMarkerCharacter;

	// Token: 0x040012FE RID: 4862
	public Transform m_cameraMarkerCredits;

	// Token: 0x040012FF RID: 4863
	public Transform m_cameraMarkerGame;

	// Token: 0x04001300 RID: 4864
	public Transform m_cameraMarkerSaves;

	// Token: 0x04001301 RID: 4865
	public float m_cameraMoveSpeed = 1.5f;

	// Token: 0x04001302 RID: 4866
	public float m_cameraMoveSpeedStart = 1.5f;

	// Token: 0x04001303 RID: 4867
	[Header("Join")]
	public GameObject m_serverListPanel;

	// Token: 0x04001304 RID: 4868
	public Toggle m_publicServerToggle;

	// Token: 0x04001305 RID: 4869
	public Toggle m_openServerToggle;

	// Token: 0x04001306 RID: 4870
	public Toggle m_crossplayServerToggle;

	// Token: 0x04001307 RID: 4871
	public Color m_toggleColor = new Color(1f, 0.6308316f, 0.2352941f);

	// Token: 0x04001308 RID: 4872
	public InputField m_serverPassword;

	// Token: 0x04001309 RID: 4873
	public Text m_passwordError;

	// Token: 0x0400130A RID: 4874
	public int m_minimumPasswordLength = 5;

	// Token: 0x0400130B RID: 4875
	public float m_characterRotateSpeed = 4f;

	// Token: 0x0400130C RID: 4876
	public float m_characterRotateSpeedGamepad = 200f;

	// Token: 0x0400130D RID: 4877
	public int m_joinHostPort = 2456;

	// Token: 0x0400130E RID: 4878
	[Header("World")]
	public GameObject m_worldListPanel;

	// Token: 0x0400130F RID: 4879
	public RectTransform m_worldListRoot;

	// Token: 0x04001310 RID: 4880
	public GameObject m_worldListElement;

	// Token: 0x04001311 RID: 4881
	public ScrollRectEnsureVisible m_worldListEnsureVisible;

	// Token: 0x04001312 RID: 4882
	public float m_worldListElementStep = 28f;

	// Token: 0x04001313 RID: 4883
	public TextMeshProUGUI m_worldSourceInfo;

	// Token: 0x04001314 RID: 4884
	public GameObject m_worldSourceInfoPanel;

	// Token: 0x04001315 RID: 4885
	public Button m_moveWorldButton;

	// Token: 0x04001316 RID: 4886
	public Text m_moveWorldText;

	// Token: 0x04001317 RID: 4887
	public InputField m_newWorldName;

	// Token: 0x04001318 RID: 4888
	public InputField m_newWorldSeed;

	// Token: 0x04001319 RID: 4889
	public Button m_newWorldDone;

	// Token: 0x0400131A RID: 4890
	public Button m_worldStart;

	// Token: 0x0400131B RID: 4891
	public Button m_worldRemove;

	// Token: 0x0400131C RID: 4892
	public GameObject m_removeWorldDialog;

	// Token: 0x0400131D RID: 4893
	public Text m_removeWorldName;

	// Token: 0x0400131E RID: 4894
	public GameObject m_removeCharacterDialog;

	// Token: 0x0400131F RID: 4895
	public Text m_removeCharacterName;

	// Token: 0x04001320 RID: 4896
	[Header("Character selection")]
	public Button m_csStartButton;

	// Token: 0x04001321 RID: 4897
	public Button m_csNewBigButton;

	// Token: 0x04001322 RID: 4898
	public Button m_csNewButton;

	// Token: 0x04001323 RID: 4899
	public Button m_csRemoveButton;

	// Token: 0x04001324 RID: 4900
	public Button m_csLeftButton;

	// Token: 0x04001325 RID: 4901
	public Button m_csRightButton;

	// Token: 0x04001326 RID: 4902
	public Button m_csNewCharacterDone;

	// Token: 0x04001327 RID: 4903
	public Button m_csNewCharacterCancel;

	// Token: 0x04001328 RID: 4904
	public GameObject m_newCharacterError;

	// Token: 0x04001329 RID: 4905
	public Text m_csName;

	// Token: 0x0400132A RID: 4906
	public Text m_csFileSource;

	// Token: 0x0400132B RID: 4907
	public Text m_csSourceInfo;

	// Token: 0x0400132C RID: 4908
	public InputField m_csNewCharacterName;

	// Token: 0x0400132D RID: 4909
	public Button m_moveCharacterButton;

	// Token: 0x0400132E RID: 4910
	public Text m_moveCharacterText;

	// Token: 0x0400132F RID: 4911
	[Header("Misc")]
	public Transform m_characterPreviewPoint;

	// Token: 0x04001330 RID: 4912
	public GameObject m_playerPrefab;

	// Token: 0x04001331 RID: 4913
	public GameObject m_objectDBPrefab;

	// Token: 0x04001332 RID: 4914
	public GameObject m_settingsPrefab;

	// Token: 0x04001333 RID: 4915
	public GameObject m_consolePrefab;

	// Token: 0x04001334 RID: 4916
	public GameObject m_feedbackPrefab;

	// Token: 0x04001335 RID: 4917
	public GameObject m_changeEffectPrefab;

	// Token: 0x04001336 RID: 4918
	public ManageSavesMenu m_manageSavesMenu;

	// Token: 0x04001337 RID: 4919
	private GameObject m_settingsPopup;

	// Token: 0x04001338 RID: 4920
	private string m_downloadUrl = "";

	// Token: 0x04001339 RID: 4921
	[TextArea]
	public string m_versionXmlUrl = "https://dl.dropboxusercontent.com/s/5ibm05oelbqt8zq/fejdversion.xml?dl=0";

	// Token: 0x0400133A RID: 4922
	private World m_world;

	// Token: 0x0400133B RID: 4923
	private bool m_startingWorld;

	// Token: 0x0400133C RID: 4924
	private ServerStatus m_joinServer;

	// Token: 0x0400133D RID: 4925
	private ServerJoinData m_queuedJoinServer;

	// Token: 0x0400133E RID: 4926
	private float m_worldListBaseSize;

	// Token: 0x0400133F RID: 4927
	private List<PlayerProfile> m_profiles;

	// Token: 0x04001340 RID: 4928
	private int m_profileIndex;

	// Token: 0x04001341 RID: 4929
	private string m_tempRemoveCharacterName = "";

	// Token: 0x04001342 RID: 4930
	private FileHelpers.FileSource m_tempRemoveCharacterSource;

	// Token: 0x04001343 RID: 4931
	private int m_tempRemoveCharacterIndex = -1;

	// Token: 0x04001344 RID: 4932
	private BackgroundWorker m_moveFileWorker;

	// Token: 0x04001345 RID: 4933
	private List<GameObject> m_worldListElements = new List<GameObject>();

	// Token: 0x04001346 RID: 4934
	private List<World> m_worlds;

	// Token: 0x04001347 RID: 4935
	private GameObject m_playerInstance;

	// Token: 0x04001348 RID: 4936
	private static bool m_firstStartup = true;

	// Token: 0x0400134B RID: 4939
	private static GameObject s_monoUpdaters = null;

	// Token: 0x020001C1 RID: 449
	// (Invoke) Token: 0x0600126F RID: 4719
	private delegate void ContinueAction();

	// Token: 0x020001C2 RID: 450
	public struct StartGameEventArgs
	{
		// Token: 0x06001272 RID: 4722 RVA: 0x0007A0C8 File Offset: 0x000782C8
		public StartGameEventArgs(bool isHost)
		{
			this.isHost = isHost;
		}

		// Token: 0x0400134C RID: 4940
		public bool isHost;
	}

	// Token: 0x020001C3 RID: 451
	// (Invoke) Token: 0x06001274 RID: 4724
	public delegate void StartGameEventHandler(object sender, FejdStartup.StartGameEventArgs e);
}
