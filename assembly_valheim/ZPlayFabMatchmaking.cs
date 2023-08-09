using System;
using System.Collections.Generic;
using System.Threading;
using PartyCSharpSDK;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab.Party;
using UnityEngine;

// Token: 0x020001A8 RID: 424
public class ZPlayFabMatchmaking
{
	// Token: 0x14000006 RID: 6
	// (add) Token: 0x060010C4 RID: 4292 RVA: 0x0006D410 File Offset: 0x0006B610
	// (remove) Token: 0x060010C5 RID: 4293 RVA: 0x0006D444 File Offset: 0x0006B644
	public static event ZPlayFabMatchmakeServerStarted ServerStarted;

	// Token: 0x14000007 RID: 7
	// (add) Token: 0x060010C6 RID: 4294 RVA: 0x0006D478 File Offset: 0x0006B678
	// (remove) Token: 0x060010C7 RID: 4295 RVA: 0x0006D4AC File Offset: 0x0006B6AC
	public static event ZPlayFabMatchmakeServerStopped ServerStopped;

	// Token: 0x14000008 RID: 8
	// (add) Token: 0x060010C8 RID: 4296 RVA: 0x0006D4E0 File Offset: 0x0006B6E0
	// (remove) Token: 0x060010C9 RID: 4297 RVA: 0x0006D514 File Offset: 0x0006B714
	public static event ZPlayFabMatchmakeLobbyLeftCallback LobbyLeft;

	// Token: 0x170000AC RID: 172
	// (get) Token: 0x060010CA RID: 4298 RVA: 0x0006D547 File Offset: 0x0006B747
	public static ZPlayFabMatchmaking instance
	{
		get
		{
			if (ZPlayFabMatchmaking.m_instance == null)
			{
				ZPlayFabMatchmaking.m_instance = new ZPlayFabMatchmaking();
			}
			return ZPlayFabMatchmaking.m_instance;
		}
	}

	// Token: 0x170000AD RID: 173
	// (get) Token: 0x060010CB RID: 4299 RVA: 0x0006D55F File Offset: 0x0006B75F
	// (set) Token: 0x060010CC RID: 4300 RVA: 0x0006D566 File Offset: 0x0006B766
	public static string JoinCode { get; internal set; }

	// Token: 0x170000AE RID: 174
	// (get) Token: 0x060010CD RID: 4301 RVA: 0x0006D56E File Offset: 0x0006B76E
	// (set) Token: 0x060010CE RID: 4302 RVA: 0x0006D575 File Offset: 0x0006B775
	public static string MyXboxUserId { get; set; } = "";

	// Token: 0x170000AF RID: 175
	// (get) Token: 0x060010CF RID: 4303 RVA: 0x0006D580 File Offset: 0x0006B780
	// (set) Token: 0x060010D0 RID: 4304 RVA: 0x0006D5C0 File Offset: 0x0006B7C0
	public static string PublicIP
	{
		get
		{
			object mtx = ZPlayFabMatchmaking.m_mtx;
			string publicIP;
			lock (mtx)
			{
				publicIP = ZPlayFabMatchmaking.m_publicIP;
			}
			return publicIP;
		}
		private set
		{
			object mtx = ZPlayFabMatchmaking.m_mtx;
			lock (mtx)
			{
				ZPlayFabMatchmaking.m_publicIP = value;
			}
		}
	}

	// Token: 0x060010D1 RID: 4305 RVA: 0x0006D600 File Offset: 0x0006B800
	public static void Initialize(bool isServer)
	{
		ZPlayFabMatchmaking.JoinCode = (isServer ? "" : "000000");
	}

	// Token: 0x060010D2 RID: 4306 RVA: 0x0006D616 File Offset: 0x0006B816
	public void Update(float deltaTime)
	{
		if (this.ReconnectNetwork(deltaTime))
		{
			return;
		}
		this.RefreshLobby(deltaTime);
		this.RetryJoinCodeUniquenessCheck(deltaTime);
		this.UpdateActiveLobbySearches(deltaTime);
		this.UpdateBackgroundLobbySearches(deltaTime);
	}

	// Token: 0x060010D3 RID: 4307 RVA: 0x0006D63E File Offset: 0x0006B83E
	private bool IsJoinedToNetwork()
	{
		return this.m_serverData != null && !string.IsNullOrEmpty(this.m_serverData.networkId);
	}

	// Token: 0x060010D4 RID: 4308 RVA: 0x0006D65D File Offset: 0x0006B85D
	private bool IsReconnectNetworkTimerActive()
	{
		return this.m_lostNetworkRetryIn > 0f;
	}

	// Token: 0x060010D5 RID: 4309 RVA: 0x0006D66C File Offset: 0x0006B86C
	private void StartReconnectNetworkTimer(int code = -1)
	{
		this.m_lostNetworkRetryIn = 30f;
		if (ZPlayFabMatchmaking.DoFastRecovery(code))
		{
			ZLog.Log("PlayFab host fast recovery");
			this.m_lostNetworkRetryIn = 12f;
		}
	}

	// Token: 0x060010D6 RID: 4310 RVA: 0x0006D696 File Offset: 0x0006B896
	private static bool DoFastRecovery(int code)
	{
		return code == 63 || code == 11;
	}

	// Token: 0x060010D7 RID: 4311 RVA: 0x0006D6A4 File Offset: 0x0006B8A4
	private void StopReconnectNetworkTimer()
	{
		this.m_isResettingNetwork = false;
		this.m_lostNetworkRetryIn = -1f;
		if (this.m_serverData != null && !this.IsJoinedToNetwork())
		{
			this.CreateAndJoinNetwork();
		}
	}

	// Token: 0x060010D8 RID: 4312 RVA: 0x0006D6D0 File Offset: 0x0006B8D0
	private bool ReconnectNetwork(float deltaTime)
	{
		if (!this.IsReconnectNetworkTimerActive())
		{
			if (this.IsJoinedToNetwork() && !PlayFabMultiplayerManager.Get().IsConnectedToNetworkState())
			{
				PlayFabMultiplayerManager.Get().ResetParty();
				this.StartReconnectNetworkTimer(-1);
				this.m_serverData.networkId = null;
			}
			return false;
		}
		this.m_lostNetworkRetryIn -= deltaTime;
		if (this.m_lostNetworkRetryIn <= 0f)
		{
			ZLog.Log(string.Format("PlayFab reconnect server '{0}'", this.m_serverData.serverName));
			this.m_isConnectingToNetwork = false;
			this.m_serverData.networkId = null;
			this.StopReconnectNetworkTimer();
		}
		else if (!this.m_isConnectingToNetwork && !this.m_isResettingNetwork && this.m_lostNetworkRetryIn <= 12f)
		{
			PlayFabMultiplayerManager.Get().ResetParty();
			this.m_isResettingNetwork = true;
			this.m_isConnectingToNetwork = false;
		}
		return true;
	}

	// Token: 0x060010D9 RID: 4313 RVA: 0x0006D79E File Offset: 0x0006B99E
	private void StartRefreshLobbyTimer()
	{
		this.m_refreshLobbyTimer = UnityEngine.Random.Range(540f, 840f);
	}

	// Token: 0x060010DA RID: 4314 RVA: 0x0006D7B8 File Offset: 0x0006B9B8
	private void RefreshLobby(float deltaTime)
	{
		if (this.m_serverData == null || this.m_serverData.networkId == null)
		{
			return;
		}
		bool flag = this.m_serverData.isDedicatedServer && string.IsNullOrEmpty(this.m_serverData.serverIp) && !string.IsNullOrEmpty(ZPlayFabMatchmaking.PublicIP);
		this.m_refreshLobbyTimer -= deltaTime;
		if (this.m_refreshLobbyTimer < 0f || flag)
		{
			this.StartRefreshLobbyTimer();
			UpdateLobbyRequest updateLobbyRequest = new UpdateLobbyRequest
			{
				LobbyId = this.m_serverData.lobbyId
			};
			if (flag)
			{
				this.m_serverData.serverIp = this.GetServerIP();
				ZLog.Log("Updating lobby with public IP " + this.m_serverData.serverIp);
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["string_key10"] = this.m_serverData.serverIp;
				Dictionary<string, string> searchData = dictionary;
				updateLobbyRequest.SearchData = searchData;
			}
			PlayFabMultiplayerAPI.UpdateLobby(updateLobbyRequest, delegate(LobbyEmptyResult _)
			{
				ZLog.Log(string.Format("Lobby {0} for world '{1}' and network {2} refreshed", this.m_serverData.lobbyId, this.m_serverData.serverName, this.m_serverData.networkId));
			}, new Action<PlayFabError>(this.OnRefreshFailed), null, null);
		}
	}

	// Token: 0x060010DB RID: 4315 RVA: 0x0006D8BB File Offset: 0x0006BABB
	private void OnRefreshFailed(PlayFabError err)
	{
		this.CreateLobby(true, delegate(CreateLobbyResult _)
		{
			ZLog.Log(string.Format("Lobby {0} for world '{1}' recreated", this.m_serverData.lobbyId, this.m_serverData.serverName));
		}, delegate(PlayFabError err)
		{
			ZLog.LogWarning(string.Format("Failed to refresh lobby {0} for world '{1}': {2}", this.m_serverData.lobbyId, this.m_serverData.serverName, err.GenerateErrorReport()));
		});
	}

	// Token: 0x060010DC RID: 4316 RVA: 0x0006D8DC File Offset: 0x0006BADC
	private void RetryJoinCodeUniquenessCheck(float deltaTime)
	{
		if (this.m_retryIn > 0f)
		{
			this.m_retryIn -= deltaTime;
			if (this.m_retryIn <= 0f)
			{
				this.CheckJoinCodeIsUnique();
			}
		}
	}

	// Token: 0x060010DD RID: 4317 RVA: 0x0006D90C File Offset: 0x0006BB0C
	private void UpdateActiveLobbySearches(float deltaTime)
	{
		for (int i = 0; i < this.m_activeSearches.Count; i++)
		{
			ZPlayFabLobbySearch zplayFabLobbySearch = this.m_activeSearches[i];
			if (zplayFabLobbySearch.IsDone)
			{
				this.m_activeSearches.RemoveAt(i);
				i--;
			}
			else
			{
				zplayFabLobbySearch.Update(deltaTime);
			}
		}
	}

	// Token: 0x060010DE RID: 4318 RVA: 0x0006D960 File Offset: 0x0006BB60
	private void UpdateBackgroundLobbySearches(float deltaTime)
	{
		if (this.m_submitBackgroundSearchIn >= 0f)
		{
			this.m_submitBackgroundSearchIn -= deltaTime;
			return;
		}
		if (this.m_pendingSearches.Count > 0)
		{
			this.m_submitBackgroundSearchIn = 2f;
			ZPlayFabLobbySearch zplayFabLobbySearch = this.m_pendingSearches.Dequeue();
			zplayFabLobbySearch.FindLobby();
			this.m_activeSearches.Add(zplayFabLobbySearch);
		}
	}

	// Token: 0x060010DF RID: 4319 RVA: 0x0006D9C0 File Offset: 0x0006BBC0
	private void OnFailed(string what, PlayFabError error)
	{
		ZLog.LogError("PlayFab " + what + " failed: " + error.ToString());
		this.UnregisterServer();
	}

	// Token: 0x060010E0 RID: 4320 RVA: 0x0006D9E4 File Offset: 0x0006BBE4
	private void OnSessionUpdated(ZPlayFabMatchmaking.State newState)
	{
		this.m_state = newState;
		switch (this.m_state)
		{
		case ZPlayFabMatchmaking.State.Creating:
			ZLog.Log(string.Format("Session \"{0}\" registered with join code {1}", this.m_serverData.serverName, ZPlayFabMatchmaking.JoinCode));
			this.m_retries = 100;
			this.CheckJoinCodeIsUnique();
			return;
		case ZPlayFabMatchmaking.State.RegenerateJoinCode:
			this.RegenerateLobbyJoinCode();
			ZLog.Log(string.Format("Created new join code {0} for session \"{1}\"", ZPlayFabMatchmaking.JoinCode, this.m_serverData.serverName));
			return;
		case ZPlayFabMatchmaking.State.Active:
		{
			ZPlayFabMatchmakeServerStarted serverStarted = ZPlayFabMatchmaking.ServerStarted;
			if (serverStarted != null)
			{
				serverStarted(this.m_serverData.remotePlayerId);
			}
			ZLog.Log(string.Format("Session \"{0}\" with join code {1} is active with {2} player(s)", this.m_serverData.serverName, ZPlayFabMatchmaking.JoinCode, this.m_serverData.numPlayers));
			return;
		}
		default:
			return;
		}
	}

	// Token: 0x060010E1 RID: 4321 RVA: 0x0006DAB4 File Offset: 0x0006BCB4
	private void UpdateNumPlayers(string info)
	{
		this.m_serverData.numPlayers = ZPlayFabSocket.NumSockets();
		if (!this.m_serverData.isDedicatedServer)
		{
			this.m_serverData.numPlayers += 1U;
		}
		ZLog.Log(string.Format("{0} server \"{1}\" that has join code {2}, now {3} player(s)", new object[]
		{
			info,
			this.m_serverData.serverName,
			ZPlayFabMatchmaking.JoinCode,
			this.m_serverData.numPlayers
		}));
	}

	// Token: 0x060010E2 RID: 4322 RVA: 0x0006DB33 File Offset: 0x0006BD33
	private void OnRemotePlayerLeft(object sender, PlayFabPlayer player)
	{
		ZPlayFabSocket.LostConnection(player);
		this.UpdateNumPlayers("Player connection lost");
	}

	// Token: 0x060010E3 RID: 4323 RVA: 0x0006DB46 File Offset: 0x0006BD46
	private void OnRemotePlayerJoined(object sender, PlayFabPlayer player)
	{
		this.StopReconnectNetworkTimer();
		ZPlayFabSocket.QueueConnection(player);
		this.UpdateNumPlayers("Player joined");
	}

	// Token: 0x060010E4 RID: 4324 RVA: 0x0006DB60 File Offset: 0x0006BD60
	private void OnNetworkJoined(object sender, string networkId)
	{
		ZLog.Log(string.Format("Joined PlayFab Party network with ID \"{0}\"", networkId));
		if (this.m_serverData.networkId == null || this.m_serverData.networkId != networkId)
		{
			this.m_serverData.networkId = networkId;
			this.CreateLobby(false, new Action<CreateLobbyResult>(this.OnCreateLobbySuccess), delegate(PlayFabError error)
			{
				this.OnFailed("create lobby", error);
			});
		}
		this.m_isConnectingToNetwork = false;
		this.m_isResettingNetwork = false;
		this.StopReconnectNetworkTimer();
		this.StartRefreshLobbyTimer();
	}

	// Token: 0x060010E5 RID: 4325 RVA: 0x0006DBE4 File Offset: 0x0006BDE4
	private void CreateLobby(bool refresh, Action<CreateLobbyResult> resultCallback, Action<PlayFabError> errorCallback)
	{
		PlayFab.MultiplayerModels.EntityKey entityKeyForLocalUser = ZPlayFabMatchmaking.GetEntityKeyForLocalUser();
		List<Member> members = new List<Member>
		{
			new Member
			{
				MemberEntity = entityKeyForLocalUser
			}
		};
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string key = PlayFabAttrKey.HavePassword.ToKeyString();
		dictionary[key] = this.m_serverData.havePassword.ToString();
		string key2 = PlayFabAttrKey.WorldName.ToKeyString();
		dictionary[key2] = this.m_serverData.worldName;
		string key3 = PlayFabAttrKey.NetworkId.ToKeyString();
		dictionary[key3] = this.m_serverData.networkId;
		Dictionary<string, string> lobbyData = dictionary;
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		dictionary2["string_key9"] = DateTime.UtcNow.Ticks.ToString();
		dictionary2["string_key5"] = this.m_serverData.serverName;
		dictionary2["string_key3"] = this.m_serverData.isCommunityServer.ToString();
		dictionary2["string_key4"] = this.m_serverData.joinCode;
		dictionary2["string_key2"] = refresh.ToString();
		dictionary2["string_key1"] = this.m_serverData.remotePlayerId;
		dictionary2["string_key6"] = this.m_serverData.gameVersion;
		dictionary2["number_key13"] = this.m_serverData.networkVersion.ToString();
		dictionary2["string_key7"] = this.m_serverData.isDedicatedServer.ToString();
		dictionary2["string_key8"] = this.m_serverData.xboxUserId;
		dictionary2["string_key10"] = this.m_serverData.serverIp;
		dictionary2["number_key11"] = ZPlayFabMatchmaking.GetSearchPage().ToString();
		dictionary2["string_key12"] = (PrivilegeManager.CanCrossplay ? "None" : PrivilegeManager.GetCurrentPlatform().ToString());
		Dictionary<string, string> searchData = dictionary2;
		CreateLobbyRequest createLobbyRequest = new CreateLobbyRequest();
		createLobbyRequest.AccessPolicy = new AccessPolicy?(AccessPolicy.Public);
		createLobbyRequest.MaxPlayers = 10U;
		createLobbyRequest.Members = members;
		createLobbyRequest.Owner = entityKeyForLocalUser;
		createLobbyRequest.LobbyData = lobbyData;
		createLobbyRequest.SearchData = searchData;
		if (this.m_serverData.isCommunityServer)
		{
			ZPlayFabMatchmaking.AddNameSearchFilter(searchData, this.m_serverData.serverName);
		}
		PlayFabMultiplayerAPI.CreateLobby(createLobbyRequest, resultCallback, errorCallback, null, null);
	}

	// Token: 0x060010E6 RID: 4326 RVA: 0x0006DE1C File Offset: 0x0006C01C
	private static int GetSearchPage()
	{
		return UnityEngine.Random.Range(0, 4);
	}

	// Token: 0x060010E7 RID: 4327 RVA: 0x0006DE28 File Offset: 0x0006C028
	internal static PlayFab.MultiplayerModels.EntityKey GetEntityKeyForLocalUser()
	{
		PlayFab.ClientModels.EntityKey entity = PlayFabManager.instance.Entity;
		return new PlayFab.MultiplayerModels.EntityKey
		{
			Id = entity.Id,
			Type = entity.Type
		};
	}

	// Token: 0x060010E8 RID: 4328 RVA: 0x0006DE5D File Offset: 0x0006C05D
	private void OnCreateLobbySuccess(CreateLobbyResult result)
	{
		ZLog.Log(string.Format("Created PlayFab lobby with ID \"{0}\", ConnectionString \"{1}\" and owned by \"{2}\"", result.LobbyId, result.ConnectionString, this.m_serverData.remotePlayerId));
		this.m_serverData.lobbyId = result.LobbyId;
		this.OnSessionUpdated(ZPlayFabMatchmaking.State.Creating);
	}

	// Token: 0x060010E9 RID: 4329 RVA: 0x0006DEA0 File Offset: 0x0006C0A0
	private void GenerateJoinCode()
	{
		ZPlayFabMatchmaking.JoinCode = UnityEngine.Random.Range(0, (int)Math.Pow(10.0, 6.0)).ToString("D" + 6U.ToString());
		this.m_serverData.joinCode = ZPlayFabMatchmaking.JoinCode;
	}

	// Token: 0x060010EA RID: 4330 RVA: 0x0006DEFC File Offset: 0x0006C0FC
	private void RegenerateLobbyJoinCode()
	{
		this.GenerateJoinCode();
		UpdateLobbyRequest updateLobbyRequest = new UpdateLobbyRequest();
		updateLobbyRequest.LobbyId = this.m_serverData.lobbyId;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["string_key4"] = ZPlayFabMatchmaking.JoinCode;
		updateLobbyRequest.SearchData = dictionary;
		PlayFabMultiplayerAPI.UpdateLobby(updateLobbyRequest, new Action<LobbyEmptyResult>(this.OnSetLobbyJoinCodeSuccess), delegate(PlayFabError error)
		{
			this.OnFailed("set lobby join-code", error);
		}, null, null);
	}

	// Token: 0x060010EB RID: 4331 RVA: 0x0006DF5F File Offset: 0x0006C15F
	private void OnSetLobbyJoinCodeSuccess(LobbyEmptyResult _)
	{
		this.CheckJoinCodeIsUnique();
	}

	// Token: 0x060010EC RID: 4332 RVA: 0x0006DF67 File Offset: 0x0006C167
	private void CheckJoinCodeIsUnique()
	{
		PlayFabMultiplayerAPI.FindLobbies(new FindLobbiesRequest
		{
			Filter = string.Format("{0} eq '{1}'", "string_key4", ZPlayFabMatchmaking.JoinCode)
		}, new Action<FindLobbiesResult>(this.OnCheckJoinCodeSuccess), delegate(PlayFabError error)
		{
			this.OnFailed("find lobbies", error);
		}, null, null);
	}

	// Token: 0x060010ED RID: 4333 RVA: 0x0006DFA7 File Offset: 0x0006C1A7
	private void ScheduleJoinCodeCheck()
	{
		this.m_retryIn = 1f;
	}

	// Token: 0x060010EE RID: 4334 RVA: 0x0006DFB4 File Offset: 0x0006C1B4
	private void OnCheckJoinCodeSuccess(FindLobbiesResult result)
	{
		if (result.Lobbies.Count == 0)
		{
			if (this.m_retries > 0)
			{
				this.m_retries--;
				ZLog.Log("Retry join-code check " + this.m_retries.ToString());
				this.ScheduleJoinCodeCheck();
				return;
			}
			ZLog.LogWarning("Zero lobbies returned, should be at least one");
			this.UnregisterServer();
			return;
		}
		else
		{
			if (result.Lobbies.Count == 1 && result.Lobbies[0].Owner.Id == ZPlayFabMatchmaking.GetEntityKeyForLocalUser().Id)
			{
				this.ActivateSession();
				return;
			}
			this.OnSessionUpdated(ZPlayFabMatchmaking.State.RegenerateJoinCode);
			return;
		}
	}

	// Token: 0x060010EF RID: 4335 RVA: 0x0006E05C File Offset: 0x0006C25C
	private void ActivateSession()
	{
		UpdateLobbyRequest updateLobbyRequest = new UpdateLobbyRequest();
		updateLobbyRequest.LobbyId = this.m_serverData.lobbyId;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["string_key2"] = true.ToString();
		updateLobbyRequest.SearchData = dictionary;
		PlayFabMultiplayerAPI.UpdateLobby(updateLobbyRequest, new Action<LobbyEmptyResult>(this.OnActivateLobbySuccess), delegate(PlayFabError error)
		{
			this.OnFailed("activate lobby", error);
		}, null, null);
	}

	// Token: 0x060010F0 RID: 4336 RVA: 0x0006E0BD File Offset: 0x0006C2BD
	private void OnActivateLobbySuccess(LobbyEmptyResult _)
	{
		this.OnSessionUpdated(ZPlayFabMatchmaking.State.Active);
	}

	// Token: 0x060010F1 RID: 4337 RVA: 0x0006E0C8 File Offset: 0x0006C2C8
	public void RegisterServer(string name, bool havePassword, bool isCommunityServer, string gameVersion, uint networkVersion, string worldName, bool needServerAccount = true)
	{
		bool flag = false;
		if (!PlayFabMultiplayerAPI.IsEntityLoggedIn())
		{
			ZLog.LogWarning("Calling ZPlayFabMatchmaking.RegisterServer() without logged in user");
			this.m_pendingRegisterServer = delegate()
			{
				this.RegisterServer(name, havePassword, isCommunityServer, gameVersion, networkVersion, worldName, needServerAccount);
			};
			return;
		}
		this.m_serverData = new PlayFabMatchmakingServerData
		{
			havePassword = havePassword,
			isCommunityServer = isCommunityServer,
			isDedicatedServer = flag,
			remotePlayerId = PlayFabManager.instance.Entity.Id,
			serverName = name,
			gameVersion = gameVersion,
			networkVersion = networkVersion,
			worldName = worldName
		};
		this.m_serverData.serverIp = this.GetServerIP();
		this.UpdateNumPlayers("New session");
		ZLog.Log(string.Format("Register PlayFab server \"{0}\"{1}", name, flag ? (" with IP " + this.m_serverData.serverIp) : ""));
		this.GenerateJoinCode();
		this.CreateAndJoinNetwork();
		PlayFabMultiplayerManager playFabMultiplayerManager = PlayFabMultiplayerManager.Get();
		playFabMultiplayerManager.OnNetworkJoined -= this.OnNetworkJoined;
		playFabMultiplayerManager.OnNetworkJoined += this.OnNetworkJoined;
		playFabMultiplayerManager.OnNetworkChanged -= this.OnNetworkChanged;
		playFabMultiplayerManager.OnNetworkChanged += this.OnNetworkChanged;
		playFabMultiplayerManager.OnError -= this.OnNetworkError;
		playFabMultiplayerManager.OnError += this.OnNetworkError;
		playFabMultiplayerManager.OnRemotePlayerJoined -= this.OnRemotePlayerJoined;
		playFabMultiplayerManager.OnRemotePlayerJoined += this.OnRemotePlayerJoined;
		playFabMultiplayerManager.OnRemotePlayerLeft -= this.OnRemotePlayerLeft;
		playFabMultiplayerManager.OnRemotePlayerLeft += this.OnRemotePlayerLeft;
	}

	// Token: 0x060010F2 RID: 4338 RVA: 0x0006E2C0 File Offset: 0x0006C4C0
	private string GetServerIP()
	{
		if (!this.m_serverData.isDedicatedServer || string.IsNullOrEmpty(ZPlayFabMatchmaking.PublicIP))
		{
			return "";
		}
		return string.Format("{0}:{1}", ZPlayFabMatchmaking.PublicIP, this.m_serverPort);
	}

	// Token: 0x060010F3 RID: 4339 RVA: 0x0006E2FC File Offset: 0x0006C4FC
	public static void LookupPublicIP()
	{
		if (string.IsNullOrEmpty(ZPlayFabMatchmaking.PublicIP) && ZPlayFabMatchmaking.m_publicIpLookupThread == null)
		{
			ZPlayFabMatchmaking.m_publicIpLookupThread = new Thread(new ParameterizedThreadStart(ZPlayFabMatchmaking.BackgroundLookupPublicIP));
			ZPlayFabMatchmaking.m_publicIpLookupThread.Name = "PlayfabLooupThread";
			ZPlayFabMatchmaking.m_publicIpLookupThread.Start();
		}
	}

	// Token: 0x060010F4 RID: 4340 RVA: 0x0006E34B File Offset: 0x0006C54B
	private static void BackgroundLookupPublicIP(object obj)
	{
		while (string.IsNullOrEmpty(ZPlayFabMatchmaking.PublicIP))
		{
			ZPlayFabMatchmaking.PublicIP = ZNet.GetPublicIP();
			Thread.Sleep(10);
		}
	}

	// Token: 0x060010F5 RID: 4341 RVA: 0x0006E36C File Offset: 0x0006C56C
	private void CreateAndJoinNetwork()
	{
		PlayFabNetworkConfiguration networkConfiguration = new PlayFabNetworkConfiguration
		{
			MaxPlayerCount = 10U,
			DirectPeerConnectivityOptions = (PARTY_DIRECT_PEER_CONNECTIVITY_OPTIONS)15U
		};
		ZLog.Log(string.Format("Server '{0}' begin PlayFab create and join network for server ", this.m_serverData.serverName));
		PlayFabMultiplayerManager.Get().CreateAndJoinNetwork(networkConfiguration);
		this.m_isConnectingToNetwork = true;
		this.StartReconnectNetworkTimer(-1);
	}

	// Token: 0x060010F6 RID: 4342 RVA: 0x0006E3C4 File Offset: 0x0006C5C4
	public void UnregisterServer()
	{
		if (this.m_state == ZPlayFabMatchmaking.State.Active)
		{
			ZPlayFabMatchmakeServerStopped serverStopped = ZPlayFabMatchmaking.ServerStopped;
			if (serverStopped != null)
			{
				serverStopped();
			}
		}
		if (this.m_state != ZPlayFabMatchmaking.State.Uninitialized)
		{
			ZLog.Log(string.Format("Unregister PlayFab server \"{0}\" and leaving network \"{1}\"", this.m_serverData.serverName, this.m_serverData.networkId));
			ZPlayFabMatchmaking.DeleteLobby(this.m_serverData.lobbyId);
			ZPlayFabSocket.DestroyListenSocket();
			PlayFabMultiplayerManager.Get().LeaveNetwork();
			PlayFabMultiplayerManager.Get().OnNetworkJoined -= this.OnNetworkJoined;
			PlayFabMultiplayerManager.Get().OnNetworkChanged -= this.OnNetworkChanged;
			PlayFabMultiplayerManager.Get().OnError -= this.OnNetworkError;
			PlayFabMultiplayerManager.Get().OnRemotePlayerJoined -= this.OnRemotePlayerJoined;
			PlayFabMultiplayerManager.Get().OnRemotePlayerLeft -= this.OnRemotePlayerLeft;
			this.m_serverData = null;
			this.m_retries = 0;
			this.m_state = ZPlayFabMatchmaking.State.Uninitialized;
			this.StopReconnectNetworkTimer();
			return;
		}
		ZPlayFabMatchmakeLobbyLeftCallback lobbyLeft = ZPlayFabMatchmaking.LobbyLeft;
		if (lobbyLeft == null)
		{
			return;
		}
		lobbyLeft(true);
	}

	// Token: 0x060010F7 RID: 4343 RVA: 0x0006E4D3 File Offset: 0x0006C6D3
	internal static void ResetParty()
	{
		if (ZPlayFabMatchmaking.instance != null && ZPlayFabMatchmaking.instance.IsJoinedToNetwork())
		{
			ZPlayFabMatchmaking.instance.OnNetworkError(null, new PlayFabMultiplayerManagerErrorArgs(9999, "Forced ResetParty", PlayFabMultiplayerManagerErrorType.Error));
			return;
		}
		ZLog.Log("No active PlayFab Party to reset");
	}

	// Token: 0x060010F8 RID: 4344 RVA: 0x0006E510 File Offset: 0x0006C710
	private void OnNetworkError(object sender, PlayFabMultiplayerManagerErrorArgs args)
	{
		if (this.IsReconnectNetworkTimerActive())
		{
			return;
		}
		ZLog.LogWarning(string.Format("PlayFab network error in session '{0}' and network {1} with type '{2}' and code '{3}': {4}", new object[]
		{
			this.m_serverData.serverName,
			this.m_serverData.networkId,
			args.Type,
			args.Code,
			args.Message
		}));
		this.StartReconnectNetworkTimer(args.Code);
	}

	// Token: 0x060010F9 RID: 4345 RVA: 0x0006E588 File Offset: 0x0006C788
	private void OnNetworkChanged(object sender, string newNetworkId)
	{
		ZLog.LogWarning(string.Format("PlayFab network session '{0}' and network {1} changed to network {2}", this.m_serverData.serverName, this.m_serverData.networkId, newNetworkId));
		this.m_serverData.networkId = newNetworkId;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string key = PlayFabAttrKey.NetworkId.ToKeyString();
		dictionary[key] = this.m_serverData.networkId;
		Dictionary<string, string> lobbyData = dictionary;
		PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
		{
			LobbyId = this.m_serverData.lobbyId,
			LobbyData = lobbyData
		}, delegate(LobbyEmptyResult _)
		{
			ZLog.Log(string.Format("Lobby {0} for world '{1}' change to network {2}", this.m_serverData.lobbyId, this.m_serverData.serverName, this.m_serverData.networkId));
		}, new Action<PlayFabError>(this.OnRefreshFailed), null, null);
	}

	// Token: 0x060010FA RID: 4346 RVA: 0x0006E624 File Offset: 0x0006C824
	private static void DeleteLobby(string lobbyId)
	{
		UpdateLobbyRequest updateLobbyRequest = new UpdateLobbyRequest();
		updateLobbyRequest.LobbyId = lobbyId;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["string_key2"] = false.ToString();
		updateLobbyRequest.SearchData = dictionary;
		PlayFabMultiplayerAPI.UpdateLobby(updateLobbyRequest, delegate(LobbyEmptyResult _)
		{
			ZLog.Log("Deactivated PlayFab lobby " + lobbyId);
		}, delegate(PlayFabError error)
		{
			ZLog.LogWarning(string.Format("Failed to deactive lobby '{0}': {1}", lobbyId, error.GenerateErrorReport()));
		}, null, null);
		ZPlayFabMatchmaking.LeaveLobby(lobbyId);
	}

	// Token: 0x060010FB RID: 4347 RVA: 0x0006E698 File Offset: 0x0006C898
	public static void LeaveLobby(string lobbyId)
	{
		PlayFabMultiplayerAPI.LeaveLobby(new LeaveLobbyRequest
		{
			LobbyId = lobbyId,
			MemberEntity = ZPlayFabMatchmaking.GetEntityKeyForLocalUser()
		}, delegate(LobbyEmptyResult _)
		{
			ZLog.Log("Left PlayFab lobby " + lobbyId);
			ZPlayFabMatchmakeLobbyLeftCallback lobbyLeft = ZPlayFabMatchmaking.LobbyLeft;
			if (lobbyLeft == null)
			{
				return;
			}
			lobbyLeft(true);
		}, delegate(PlayFabError error)
		{
			ZLog.LogError(string.Format("Failed to leave lobby '{0}': {1}", lobbyId, error.GenerateErrorReport()));
			ZPlayFabMatchmakeLobbyLeftCallback lobbyLeft = ZPlayFabMatchmaking.LobbyLeft;
			if (lobbyLeft == null)
			{
				return;
			}
			lobbyLeft(false);
		}, null, null);
	}

	// Token: 0x060010FC RID: 4348 RVA: 0x0006E6ED File Offset: 0x0006C8ED
	public static void LeaveEmptyLobby()
	{
		ZPlayFabMatchmakeLobbyLeftCallback lobbyLeft = ZPlayFabMatchmaking.LobbyLeft;
		if (lobbyLeft == null)
		{
			return;
		}
		lobbyLeft(true);
	}

	// Token: 0x060010FD RID: 4349 RVA: 0x0006E700 File Offset: 0x0006C900
	public static void ResolveJoinCode(string joinCode, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction)
	{
		string searchFilter = string.Format("{0} eq '{1}' and {2} eq '{3}'", new object[]
		{
			"string_key4",
			joinCode,
			"string_key2",
			true.ToString()
		});
		ZPlayFabMatchmaking.instance.m_activeSearches.Add(new ZPlayFabLobbySearch(successAction, failedAction, searchFilter, null));
	}

	// Token: 0x060010FE RID: 4350 RVA: 0x0006E758 File Offset: 0x0006C958
	public static void CheckHostOnlineStatus(string hostName, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby = false)
	{
		ZPlayFabMatchmaking.FindHostSession(string.Format("{0} eq '{1}' and {2} eq '{3}'", new object[]
		{
			"string_key1",
			hostName,
			"string_key2",
			true.ToString()
		}), successAction, failedAction, joinLobby);
	}

	// Token: 0x060010FF RID: 4351 RVA: 0x0006E7A0 File Offset: 0x0006C9A0
	public static void FindHostByIp(string hostIp, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby = false)
	{
		ZPlayFabMatchmaking.FindHostSession(string.Format("{0} eq '{1}' and {2} eq '{3}'", new object[]
		{
			"string_key10",
			hostIp,
			"string_key2",
			true.ToString()
		}), successAction, failedAction, joinLobby);
	}

	// Token: 0x06001100 RID: 4352 RVA: 0x0006E7E8 File Offset: 0x0006C9E8
	private static Dictionary<char, int> CreateCharHistogram(string str)
	{
		Dictionary<char, int> dictionary = new Dictionary<char, int>();
		foreach (char c in str.ToLowerInvariant())
		{
			if (dictionary.ContainsKey(c))
			{
				Dictionary<char, int> dictionary2 = dictionary;
				char key = c;
				int num = dictionary2[key];
				dictionary2[key] = num + 1;
			}
			else
			{
				dictionary.Add(c, 1);
			}
		}
		return dictionary;
	}

	// Token: 0x06001101 RID: 4353 RVA: 0x0006E848 File Offset: 0x0006CA48
	private static void AddNameSearchFilter(Dictionary<string, string> searchData, string serverName)
	{
		Dictionary<char, int> dictionary = ZPlayFabMatchmaking.CreateCharHistogram(serverName);
		for (char c = 'a'; c <= 'z'; c += '\u0001')
		{
			string key;
			if (ZPlayFabMatchmaking.CharToKeyName(c, out key))
			{
				int num;
				dictionary.TryGetValue(c, out num);
				searchData.Add(key, num.ToString());
			}
		}
	}

	// Token: 0x06001102 RID: 4354 RVA: 0x0006E890 File Offset: 0x0006CA90
	private static string CreateNameSearchFilter(string name)
	{
		Dictionary<char, int> dictionary = ZPlayFabMatchmaking.CreateCharHistogram(name);
		string text = "";
		foreach (char c in name.ToLowerInvariant())
		{
			string arg;
			int num;
			if (ZPlayFabMatchmaking.CharToKeyName(c, out arg) && dictionary.TryGetValue(c, out num))
			{
				text += string.Format(" and {0} ge {1}", arg, num);
			}
		}
		return text;
	}

	// Token: 0x06001103 RID: 4355 RVA: 0x0006E900 File Offset: 0x0006CB00
	private static bool CharToKeyName(char ch, out string key)
	{
		int num = "eariotnslcudpmhgbfywkvxzjq".IndexOf(ch);
		if (num < 0 || num >= 17)
		{
			key = null;
			return false;
		}
		key = string.Format("number_key{0}", num + 13 + 1);
		return true;
	}

	// Token: 0x06001104 RID: 4356 RVA: 0x0006E940 File Offset: 0x0006CB40
	private void CancelPendingSearches()
	{
		foreach (ZPlayFabLobbySearch zplayFabLobbySearch in ZPlayFabMatchmaking.instance.m_activeSearches)
		{
			zplayFabLobbySearch.Cancel();
		}
		this.m_pendingSearches.Clear();
	}

	// Token: 0x06001105 RID: 4357 RVA: 0x0006E9A0 File Offset: 0x0006CBA0
	private static void FindHostSession(string searchFilter, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby)
	{
		if (joinLobby)
		{
			ZPlayFabMatchmaking.instance.CancelPendingSearches();
			ZPlayFabMatchmaking.instance.m_activeSearches.Add(new ZPlayFabLobbySearch(successAction, failedAction, searchFilter, true));
			return;
		}
		ZPlayFabMatchmaking.instance.m_pendingSearches.Enqueue(new ZPlayFabLobbySearch(successAction, failedAction, searchFilter, false));
	}

	// Token: 0x06001106 RID: 4358 RVA: 0x0006E9E0 File Offset: 0x0006CBE0
	public static void ListServers(string nameFilter, ZPlayFabMatchmakingSuccessCallback serverFoundAction, ZPlayFabMatchmakingFailedCallback listDone, bool listP2P = false)
	{
		ZPlayFabMatchmaking.instance.CancelPendingSearches();
		string text = listP2P ? string.Format("{0} eq '{1}' and {2} eq '{3}'", new object[]
		{
			"string_key7",
			false.ToString(),
			"string_key2",
			true.ToString()
		}) : string.Format("{0} eq '{1}' and {2} eq '{3}'", new object[]
		{
			"string_key3",
			true.ToString(),
			"string_key2",
			true.ToString()
		});
		if (string.IsNullOrEmpty(nameFilter))
		{
			text += string.Format(" and {0} eq {1}", "number_key13", 5U);
		}
		else
		{
			text += ZPlayFabMatchmaking.CreateNameSearchFilter(nameFilter);
		}
		if (PrivilegeManager.CanCrossplay)
		{
			string searchFilter = text + " and string_key12 eq 'None'";
			ZPlayFabMatchmaking.instance.m_pendingSearches.Enqueue(new ZPlayFabLobbySearch(serverFoundAction, listDone, searchFilter, nameFilter));
			return;
		}
		text += string.Format(" and {0} eq '{1}'", "string_key12", PrivilegeManager.GetCurrentPlatform());
		ZPlayFabMatchmaking.instance.m_pendingSearches.Enqueue(new ZPlayFabLobbySearch(serverFoundAction, listDone, text, nameFilter));
	}

	// Token: 0x06001107 RID: 4359 RVA: 0x0006EB04 File Offset: 0x0006CD04
	public static void AddFriend(string xboxUserId)
	{
		ZPlayFabMatchmaking.instance.m_friends.Add(xboxUserId);
	}

	// Token: 0x06001108 RID: 4360 RVA: 0x0006EB17 File Offset: 0x0006CD17
	public static bool IsFriendWith(string xboxUserId)
	{
		return ZPlayFabMatchmaking.instance.m_friends.Contains(xboxUserId);
	}

	// Token: 0x06001109 RID: 4361 RVA: 0x0006EB2C File Offset: 0x0006CD2C
	public static bool IsJoinCode(string joinString)
	{
		int num;
		return (long)joinString.Length == 6L && int.TryParse(joinString, out num);
	}

	// Token: 0x0600110A RID: 4362 RVA: 0x0006EB4E File Offset: 0x0006CD4E
	public static void SetDataPort(int serverPort)
	{
		if (ZPlayFabMatchmaking.instance != null)
		{
			ZPlayFabMatchmaking.instance.m_serverPort = serverPort;
		}
	}

	// Token: 0x0600110B RID: 4363 RVA: 0x0006EB62 File Offset: 0x0006CD62
	public static void OnLogin()
	{
		if (ZPlayFabMatchmaking.instance != null && ZPlayFabMatchmaking.instance.m_pendingRegisterServer != null)
		{
			ZPlayFabMatchmaking.instance.m_pendingRegisterServer();
			ZPlayFabMatchmaking.instance.m_pendingRegisterServer = null;
		}
	}

	// Token: 0x0600110C RID: 4364 RVA: 0x0006EB91 File Offset: 0x0006CD91
	internal static void ForwardProgress()
	{
		if (ZPlayFabMatchmaking.instance != null)
		{
			ZPlayFabMatchmaking.instance.StopReconnectNetworkTimer();
		}
	}

	// Token: 0x0400119F RID: 4511
	private static ZPlayFabMatchmaking m_instance;

	// Token: 0x040011A2 RID: 4514
	private static string m_publicIP = "";

	// Token: 0x040011A3 RID: 4515
	private static readonly object m_mtx = new object();

	// Token: 0x040011A4 RID: 4516
	private static Thread m_publicIpLookupThread;

	// Token: 0x040011A5 RID: 4517
	public const uint JoinStringLength = 6U;

	// Token: 0x040011A6 RID: 4518
	public const uint MaxPlayers = 10U;

	// Token: 0x040011A7 RID: 4519
	internal const int NumSearchPages = 4;

	// Token: 0x040011A8 RID: 4520
	public const string RemotePlayerIdSearchKey = "string_key1";

	// Token: 0x040011A9 RID: 4521
	public const string IsActiveSearchKey = "string_key2";

	// Token: 0x040011AA RID: 4522
	public const string IsCommunityServerSearchKey = "string_key3";

	// Token: 0x040011AB RID: 4523
	public const string JoinCodeSearchKey = "string_key4";

	// Token: 0x040011AC RID: 4524
	public const string ServerNameSearchKey = "string_key5";

	// Token: 0x040011AD RID: 4525
	public const string GameVersionSearchKey = "string_key6";

	// Token: 0x040011AE RID: 4526
	public const string IsDedicatedServerSearchKey = "string_key7";

	// Token: 0x040011AF RID: 4527
	public const string XboxUserIdSearchKey = "string_key8";

	// Token: 0x040011B0 RID: 4528
	public const string CreatedSearchKey = "string_key9";

	// Token: 0x040011B1 RID: 4529
	public const string ServerIpSearchKey = "string_key10";

	// Token: 0x040011B2 RID: 4530
	public const string PageSearchKey = "number_key11";

	// Token: 0x040011B3 RID: 4531
	public const string PlatformRestrictionKey = "string_key12";

	// Token: 0x040011B4 RID: 4532
	public const string NetworkVersionSearchKey = "number_key13";

	// Token: 0x040011B5 RID: 4533
	private const int NumStringSearchKeys = 13;

	// Token: 0x040011B6 RID: 4534
	private ZPlayFabMatchmaking.State m_state;

	// Token: 0x040011B7 RID: 4535
	private PlayFabMatchmakingServerData m_serverData;

	// Token: 0x040011B8 RID: 4536
	private int m_retries;

	// Token: 0x040011B9 RID: 4537
	private float m_retryIn = -1f;

	// Token: 0x040011BA RID: 4538
	private const float LostNetworkRetryDuration = 30f;

	// Token: 0x040011BB RID: 4539
	private float m_lostNetworkRetryIn = -1f;

	// Token: 0x040011BC RID: 4540
	private bool m_isConnectingToNetwork;

	// Token: 0x040011BD RID: 4541
	private bool m_isResettingNetwork;

	// Token: 0x040011BE RID: 4542
	private float m_submitBackgroundSearchIn = -1f;

	// Token: 0x040011BF RID: 4543
	private int m_serverPort = -1;

	// Token: 0x040011C0 RID: 4544
	private float m_refreshLobbyTimer;

	// Token: 0x040011C1 RID: 4545
	private const float RefreshLobbyDurationMin = 540f;

	// Token: 0x040011C2 RID: 4546
	private const float RefreshLobbyDurationMax = 840f;

	// Token: 0x040011C3 RID: 4547
	private const float DurationBetwenBackgroundSearches = 2f;

	// Token: 0x040011C4 RID: 4548
	private readonly List<ZPlayFabLobbySearch> m_activeSearches = new List<ZPlayFabLobbySearch>();

	// Token: 0x040011C5 RID: 4549
	private readonly Queue<ZPlayFabLobbySearch> m_pendingSearches = new Queue<ZPlayFabLobbySearch>();

	// Token: 0x040011C6 RID: 4550
	private readonly HashSet<string> m_friends = new HashSet<string>();

	// Token: 0x040011C7 RID: 4551
	private Action m_pendingRegisterServer;

	// Token: 0x020001A9 RID: 425
	private enum State
	{
		// Token: 0x040011C9 RID: 4553
		Uninitialized,
		// Token: 0x040011CA RID: 4554
		Creating,
		// Token: 0x040011CB RID: 4555
		RegenerateJoinCode,
		// Token: 0x040011CC RID: 4556
		Active
	}
}
