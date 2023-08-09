using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

// Token: 0x02000188 RID: 392
public class ZSteamMatchmaking
{
	// Token: 0x1700009E RID: 158
	// (get) Token: 0x06000FA3 RID: 4003 RVA: 0x00068265 File Offset: 0x00066465
	public static ZSteamMatchmaking instance
	{
		get
		{
			return ZSteamMatchmaking.m_instance;
		}
	}

	// Token: 0x06000FA4 RID: 4004 RVA: 0x0006826C File Offset: 0x0006646C
	public static void Initialize()
	{
		if (ZSteamMatchmaking.m_instance == null)
		{
			ZSteamMatchmaking.m_instance = new ZSteamMatchmaking();
		}
	}

	// Token: 0x06000FA5 RID: 4005 RVA: 0x00068280 File Offset: 0x00066480
	private ZSteamMatchmaking()
	{
		this.m_steamServerCallbackHandler = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.OnServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.OnServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.OnRefreshComplete));
		this.m_joinServerCallbackHandler = new ISteamMatchmakingPingResponse(new ISteamMatchmakingPingResponse.ServerResponded(this.OnJoinServerRespond), new ISteamMatchmakingPingResponse.ServerFailedToRespond(this.OnJoinServerFailed));
		this.m_lobbyCreated = CallResult<LobbyCreated_t>.Create(new CallResult<LobbyCreated_t>.APIDispatchDelegate(this.OnLobbyCreated));
		this.m_lobbyMatchList = CallResult<LobbyMatchList_t>.Create(new CallResult<LobbyMatchList_t>.APIDispatchDelegate(this.OnLobbyMatchList));
		this.m_changeServer = Callback<GameServerChangeRequested_t>.Create(new Callback<GameServerChangeRequested_t>.DispatchDelegate(this.OnChangeServerRequest));
		this.m_joinRequest = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(this.OnJoinRequest));
		this.m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
		this.m_authSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(new Callback<GetAuthSessionTicketResponse_t>.DispatchDelegate(this.OnAuthSessionTicketResponse));
	}

	// Token: 0x06000FA6 RID: 4006 RVA: 0x000683E4 File Offset: 0x000665E4
	public byte[] RequestSessionTicket()
	{
		this.ReleaseSessionTicket();
		byte[] array = new byte[1024];
		uint num = 0U;
		this.m_authTicket = SteamUser.GetAuthSessionTicket(array, 1024, out num);
		if (this.m_authTicket == HAuthTicket.Invalid)
		{
			return null;
		}
		byte[] array2 = new byte[num];
		Buffer.BlockCopy(array, 0, array2, 0, (int)num);
		return array2;
	}

	// Token: 0x06000FA7 RID: 4007 RVA: 0x0006843D File Offset: 0x0006663D
	public void ReleaseSessionTicket()
	{
		if (this.m_authTicket == HAuthTicket.Invalid)
		{
			return;
		}
		SteamUser.CancelAuthTicket(this.m_authTicket);
		this.m_authTicket = HAuthTicket.Invalid;
		ZLog.Log("Released session ticket");
	}

	// Token: 0x06000FA8 RID: 4008 RVA: 0x00068472 File Offset: 0x00066672
	public bool VerifySessionTicket(byte[] ticket, CSteamID steamID)
	{
		return SteamUser.BeginAuthSession(ticket, ticket.Length, steamID) == EBeginAuthSessionResult.k_EBeginAuthSessionResultOK;
	}

	// Token: 0x06000FA9 RID: 4009 RVA: 0x00068481 File Offset: 0x00066681
	private void OnAuthSessionTicketResponse(GetAuthSessionTicketResponse_t data)
	{
		ZLog.Log("Session auth respons callback");
	}

	// Token: 0x06000FAA RID: 4010 RVA: 0x0006848D File Offset: 0x0006668D
	private void OnSteamServersConnected(SteamServersConnected_t data)
	{
		ZLog.Log("Game server connected");
	}

	// Token: 0x06000FAB RID: 4011 RVA: 0x00068499 File Offset: 0x00066699
	private void OnSteamServersDisconnected(SteamServersDisconnected_t data)
	{
		ZLog.LogWarning("Game server disconnected");
	}

	// Token: 0x06000FAC RID: 4012 RVA: 0x000684A5 File Offset: 0x000666A5
	private void OnSteamServersConnectFail(SteamServerConnectFailure_t data)
	{
		ZLog.LogWarning("Game server connected failed");
	}

	// Token: 0x06000FAD RID: 4013 RVA: 0x000684B1 File Offset: 0x000666B1
	private void OnChangeServerRequest(GameServerChangeRequested_t data)
	{
		ZLog.Log("ZSteamMatchmaking got change server request to:" + data.m_rgchServer);
		this.QueueServerJoin(data.m_rgchServer);
	}

	// Token: 0x06000FAE RID: 4014 RVA: 0x000684D4 File Offset: 0x000666D4
	private void OnJoinRequest(GameLobbyJoinRequested_t data)
	{
		string str = "ZSteamMatchmaking got join request friend:";
		CSteamID csteamID = data.m_steamIDFriend;
		string str2 = csteamID.ToString();
		string str3 = "  lobby:";
		csteamID = data.m_steamIDLobby;
		ZLog.Log(str + str2 + str3 + csteamID.ToString());
		this.QueueLobbyJoin(data.m_steamIDLobby);
	}

	// Token: 0x06000FAF RID: 4015 RVA: 0x0006852C File Offset: 0x0006672C
	private IPAddress FindIP(string host)
	{
		IPAddress result;
		try
		{
			IPAddress ipaddress;
			if (IPAddress.TryParse(host, out ipaddress))
			{
				result = ipaddress;
			}
			else
			{
				ZLog.Log("Not an ip address " + host + " doing dns lookup");
				IPHostEntry hostEntry = Dns.GetHostEntry(host);
				if (hostEntry.AddressList.Length == 0)
				{
					ZLog.Log("Dns lookup failed");
					result = null;
				}
				else
				{
					ZLog.Log("Got dns entries: " + hostEntry.AddressList.Length.ToString());
					foreach (IPAddress ipaddress2 in hostEntry.AddressList)
					{
						if (ipaddress2.AddressFamily == AddressFamily.InterNetwork)
						{
							return ipaddress2;
						}
					}
					result = null;
				}
			}
		}
		catch (Exception ex)
		{
			ZLog.Log("Exception while finding ip:" + ex.ToString());
			result = null;
		}
		return result;
	}

	// Token: 0x06000FB0 RID: 4016 RVA: 0x000685FC File Offset: 0x000667FC
	public bool ResolveIPFromAddrString(string addr, ref SteamNetworkingIPAddr destination)
	{
		bool result;
		try
		{
			string[] array = addr.Split(new char[]
			{
				':'
			});
			if (array.Length < 2)
			{
				result = false;
			}
			else
			{
				IPAddress ipaddress = this.FindIP(array[0]);
				if (ipaddress == null)
				{
					ZLog.Log("Invalid address " + array[0]);
					result = false;
				}
				else
				{
					uint nIP = (uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(ipaddress.GetAddressBytes(), 0));
					int num = int.Parse(array[1]);
					ZLog.Log("connect to ip:" + ipaddress.ToString() + " port:" + num.ToString());
					destination.SetIPv4(nIP, (ushort)num);
					result = true;
				}
			}
		}
		catch (Exception ex)
		{
			string str = "Exception when resolving IP address: ";
			Exception ex2 = ex;
			ZLog.Log(str + ((ex2 != null) ? ex2.ToString() : null));
			result = false;
		}
		return result;
	}

	// Token: 0x06000FB1 RID: 4017 RVA: 0x000686D0 File Offset: 0x000668D0
	public void QueueServerJoin(string addr)
	{
		SteamNetworkingIPAddr steamNetworkingIPAddr = default(SteamNetworkingIPAddr);
		if (this.ResolveIPFromAddrString(addr, ref steamNetworkingIPAddr))
		{
			this.m_joinData = new ServerJoinDataDedicated(steamNetworkingIPAddr.GetIPv4(), steamNetworkingIPAddr.m_port);
			return;
		}
		ZLog.Log("Couldn't resolve IP address.");
	}

	// Token: 0x06000FB2 RID: 4018 RVA: 0x00068714 File Offset: 0x00066914
	private void OnJoinServerRespond(gameserveritem_t serverData)
	{
		string str = "Got join server data ";
		string serverName = serverData.GetServerName();
		string str2 = "  ";
		CSteamID steamID = serverData.m_steamID;
		ZLog.Log(str + serverName + str2 + steamID.ToString());
		SteamNetworkingIPAddr steamNetworkingIPAddr = default(SteamNetworkingIPAddr);
		steamNetworkingIPAddr.SetIPv4(serverData.m_NetAdr.GetIP(), serverData.m_NetAdr.GetConnectionPort());
		this.m_joinData = new ServerJoinDataDedicated(steamNetworkingIPAddr.GetIPv4(), steamNetworkingIPAddr.m_port);
	}

	// Token: 0x06000FB3 RID: 4019 RVA: 0x0006878C File Offset: 0x0006698C
	private void OnJoinServerFailed()
	{
		ZLog.Log("Failed to get join server data");
	}

	// Token: 0x06000FB4 RID: 4020 RVA: 0x00068798 File Offset: 0x00066998
	private bool TryGetLobbyData(CSteamID lobbyID)
	{
		uint num;
		ushort num2;
		CSteamID csteamID;
		if (!SteamMatchmaking.GetLobbyGameServer(lobbyID, out num, out num2, out csteamID))
		{
			return false;
		}
		string str = "  hostid: ";
		CSteamID csteamID2 = csteamID;
		ZLog.Log(str + csteamID2.ToString());
		this.m_queuedJoinLobby = CSteamID.Nil;
		ServerStatus lobbyServerData = this.GetLobbyServerData(lobbyID);
		this.m_joinData = lobbyServerData.m_joinData;
		return true;
	}

	// Token: 0x06000FB5 RID: 4021 RVA: 0x000687F4 File Offset: 0x000669F4
	public void QueueLobbyJoin(CSteamID lobbyID)
	{
		if (!this.TryGetLobbyData(lobbyID))
		{
			string str = "Failed to get lobby data for lobby ";
			CSteamID csteamID = lobbyID;
			ZLog.Log(str + csteamID.ToString() + ", requesting lobby data");
			this.m_queuedJoinLobby = lobbyID;
			SteamMatchmaking.RequestLobbyData(lobbyID);
		}
		if (FejdStartup.instance == null)
		{
			if (UnifiedPopup.IsAvailable() && Menu.instance != null)
			{
				UnifiedPopup.Push(new YesNoPopup("$menu_joindifferentserver", "$menu_logoutprompt", delegate()
				{
					UnifiedPopup.Pop();
					if (Menu.instance != null)
					{
						Menu.instance.OnLogoutYes();
					}
				}, delegate()
				{
					UnifiedPopup.Pop();
					this.m_queuedJoinLobby = CSteamID.Nil;
					this.m_joinData = null;
				}, true));
				return;
			}
			Debug.LogWarning("Couldn't handle invite appropriately! Ignoring.");
			this.m_queuedJoinLobby = CSteamID.Nil;
			this.m_joinData = null;
		}
	}

	// Token: 0x06000FB6 RID: 4022 RVA: 0x000688BC File Offset: 0x00066ABC
	private void OnLobbyDataUpdate(LobbyDataUpdate_t data)
	{
		CSteamID csteamID = new CSteamID(data.m_ulSteamIDLobby);
		if (csteamID == this.m_queuedJoinLobby)
		{
			if (this.TryGetLobbyData(csteamID))
			{
				ZLog.Log("Got lobby data, for queued lobby");
				return;
			}
		}
		else
		{
			ZLog.Log("Got requested lobby data");
			foreach (KeyValuePair<CSteamID, string> keyValuePair in this.m_requestedFriendGames)
			{
				if (keyValuePair.Key == csteamID)
				{
					ServerStatus lobbyServerData = this.GetLobbyServerData(csteamID);
					if (lobbyServerData != null)
					{
						lobbyServerData.m_joinData.m_serverName = keyValuePair.Value + " [" + lobbyServerData.m_joinData.m_serverName + "]";
						this.m_friendServers.Add(lobbyServerData);
						this.m_serverListRevision++;
					}
				}
			}
		}
	}

	// Token: 0x06000FB7 RID: 4023 RVA: 0x000689A4 File Offset: 0x00066BA4
	public void RegisterServer(string name, bool password, string gameVersion, uint networkVersion, bool publicServer, string worldName, ZSteamMatchmaking.ServerRegistered serverRegisteredCallback)
	{
		this.UnregisterServer();
		this.serverRegisteredCallback = serverRegisteredCallback;
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(publicServer ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypeFriendsOnly, 32);
		this.m_lobbyCreated.Set(hAPICall, null);
		this.m_registerServerName = name;
		this.m_registerPassword = password;
		this.m_registerGameVerson = gameVersion;
		this.m_registerNetworkVerson = networkVersion;
		ZLog.Log("Registering lobby");
	}

	// Token: 0x06000FB8 RID: 4024 RVA: 0x00068A04 File Offset: 0x00066C04
	private void OnLobbyCreated(LobbyCreated_t data, bool ioError)
	{
		ZLog.Log(string.Concat(new string[]
		{
			"Lobby was created ",
			data.m_eResult.ToString(),
			"  ",
			data.m_ulSteamIDLobby.ToString(),
			"  error:",
			ioError.ToString()
		}));
		if (ioError)
		{
			ZSteamMatchmaking.ServerRegistered serverRegistered = this.serverRegisteredCallback;
			if (serverRegistered == null)
			{
				return;
			}
			serverRegistered(false);
			return;
		}
		else if (data.m_eResult == EResult.k_EResultNoConnection)
		{
			ZLog.LogWarning("Failed to connect to Steam to register the server!");
			ZSteamMatchmaking.ServerRegistered serverRegistered2 = this.serverRegisteredCallback;
			if (serverRegistered2 == null)
			{
				return;
			}
			serverRegistered2(false);
			return;
		}
		else
		{
			this.m_myLobby = new CSteamID(data.m_ulSteamIDLobby);
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "name", this.m_registerServerName))
			{
				Debug.LogError("Couldn't set name in lobby");
			}
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "password", this.m_registerPassword ? "1" : "0"))
			{
				Debug.LogError("Couldn't set password in lobby");
			}
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "version", this.m_registerGameVerson))
			{
				Debug.LogError("Couldn't set game version in lobby");
			}
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "networkversion", this.m_registerNetworkVerson.ToString()))
			{
				Debug.LogError("Couldn't set network version in lobby");
			}
			OnlineBackendType onlineBackend = ZNet.m_onlineBackend;
			string pchValue;
			string pchValue2;
			string pchValue3;
			if (onlineBackend == OnlineBackendType.CustomSocket)
			{
				pchValue = "Dedicated";
				pchValue2 = ZNet.GetServerString(false);
				pchValue3 = "1";
			}
			else if (onlineBackend == OnlineBackendType.Steamworks)
			{
				pchValue = "Steam user";
				pchValue2 = "";
				pchValue3 = "0";
			}
			else if (onlineBackend == OnlineBackendType.PlayFab)
			{
				pchValue = "PlayFab user";
				pchValue2 = PlayFabManager.instance.Entity.Id;
				pchValue3 = "1";
			}
			else
			{
				Debug.LogError("Can't create lobby for server with unknown or unsupported backend");
				pchValue = "";
				pchValue2 = "";
				pchValue3 = "";
			}
			if (!PrivilegeManager.CanCrossplay)
			{
				pchValue3 = "0";
			}
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "serverType", pchValue))
			{
				Debug.LogError("Couldn't set backend in lobby");
			}
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "hostID", pchValue2))
			{
				Debug.LogError("Couldn't set host in lobby");
			}
			if (!SteamMatchmaking.SetLobbyData(this.m_myLobby, "isCrossplay", pchValue3))
			{
				Debug.LogError("Couldn't set crossplay in lobby");
			}
			SteamMatchmaking.SetLobbyGameServer(this.m_myLobby, 0U, 0, SteamUser.GetSteamID());
			ZSteamMatchmaking.ServerRegistered serverRegistered3 = this.serverRegisteredCallback;
			if (serverRegistered3 == null)
			{
				return;
			}
			serverRegistered3(true);
			return;
		}
	}

	// Token: 0x06000FB9 RID: 4025 RVA: 0x00068C48 File Offset: 0x00066E48
	private void OnLobbyEnter(LobbyEnter_t data, bool ioError)
	{
		ZLog.LogWarning("Entering lobby " + data.m_ulSteamIDLobby.ToString());
	}

	// Token: 0x06000FBA RID: 4026 RVA: 0x00068C65 File Offset: 0x00066E65
	public void UnregisterServer()
	{
		if (this.m_myLobby != CSteamID.Nil)
		{
			SteamMatchmaking.SetLobbyJoinable(this.m_myLobby, false);
			SteamMatchmaking.LeaveLobby(this.m_myLobby);
			this.m_myLobby = CSteamID.Nil;
		}
	}

	// Token: 0x06000FBB RID: 4027 RVA: 0x00068C9C File Offset: 0x00066E9C
	public void RequestServerlist()
	{
		this.IsRefreshing = true;
		this.RequestFriendGames();
		this.RequestPublicLobbies();
		this.RequestDedicatedServers();
	}

	// Token: 0x06000FBC RID: 4028 RVA: 0x00068CB7 File Offset: 0x00066EB7
	public void StopServerListing()
	{
		if (this.m_haveListRequest)
		{
			SteamMatchmakingServers.ReleaseRequest(this.m_serverListRequest);
			this.m_haveListRequest = false;
			this.IsRefreshing = false;
		}
	}

	// Token: 0x06000FBD RID: 4029 RVA: 0x00068CDC File Offset: 0x00066EDC
	private void RequestFriendGames()
	{
		this.m_friendServers.Clear();
		this.m_requestedFriendGames.Clear();
		int num = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		if (num == -1)
		{
			ZLog.Log("GetFriendCount returned -1, the current user is not logged in.");
			num = 0;
		}
		for (int i = 0; i < num; i++)
		{
			CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			string friendPersonaName = SteamFriends.GetFriendPersonaName(friendByIndex);
			FriendGameInfo_t friendGameInfo_t;
			if (SteamFriends.GetFriendGamePlayed(friendByIndex, out friendGameInfo_t) && friendGameInfo_t.m_gameID == (CGameID)((ulong)SteamManager.APP_ID) && friendGameInfo_t.m_steamIDLobby != CSteamID.Nil)
			{
				ZLog.Log("Friend is in our game");
				this.m_requestedFriendGames.Add(new KeyValuePair<CSteamID, string>(friendGameInfo_t.m_steamIDLobby, friendPersonaName));
				SteamMatchmaking.RequestLobbyData(friendGameInfo_t.m_steamIDLobby);
			}
		}
		this.m_serverListRevision++;
	}

	// Token: 0x06000FBE RID: 4030 RVA: 0x00068DA0 File Offset: 0x00066FA0
	private void RequestPublicLobbies()
	{
		SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
		this.m_lobbyMatchList.Set(hAPICall, null);
		this.m_refreshingPublicGames = true;
	}

	// Token: 0x06000FBF RID: 4031 RVA: 0x00068DC8 File Offset: 0x00066FC8
	private void RequestDedicatedServers()
	{
		if (this.m_haveListRequest)
		{
			SteamMatchmakingServers.ReleaseRequest(this.m_serverListRequest);
			this.m_haveListRequest = false;
		}
		this.m_dedicatedServers.Clear();
		this.m_serverListRequest = SteamMatchmakingServers.RequestInternetServerList(SteamUtils.GetAppID(), new MatchMakingKeyValuePair_t[0], 0U, this.m_steamServerCallbackHandler);
		this.m_haveListRequest = true;
	}

	// Token: 0x06000FC0 RID: 4032 RVA: 0x00068E20 File Offset: 0x00067020
	private void OnLobbyMatchList(LobbyMatchList_t data, bool ioError)
	{
		this.m_refreshingPublicGames = false;
		this.m_matchmakingServers.Clear();
		int num = 0;
		while ((long)num < (long)((ulong)data.m_nLobbiesMatching))
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(num);
			ServerStatus lobbyServerData = this.GetLobbyServerData(lobbyByIndex);
			if (lobbyServerData != null)
			{
				this.m_matchmakingServers.Add(lobbyServerData);
			}
			num++;
		}
		this.m_serverListRevision++;
	}

	// Token: 0x06000FC1 RID: 4033 RVA: 0x00068E80 File Offset: 0x00067080
	private ServerStatus GetLobbyServerData(CSteamID lobbyID)
	{
		string lobbyData = SteamMatchmaking.GetLobbyData(lobbyID, "name");
		bool isPasswordProtected = SteamMatchmaking.GetLobbyData(lobbyID, "password") == "1";
		string lobbyData2 = SteamMatchmaking.GetLobbyData(lobbyID, "version");
		uint num = uint.TryParse(SteamMatchmaking.GetLobbyData(lobbyID, "networkversion"), out num) ? num : 0U;
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
		uint num2;
		ushort num3;
		CSteamID joinUserID;
		if (SteamMatchmaking.GetLobbyGameServer(lobbyID, out num2, out num3, out joinUserID))
		{
			string lobbyData3 = SteamMatchmaking.GetLobbyData(lobbyID, "hostID");
			string lobbyData4 = SteamMatchmaking.GetLobbyData(lobbyID, "serverType");
			string lobbyData5 = SteamMatchmaking.GetLobbyData(lobbyID, "isCrossplay");
			if (lobbyData4 != null)
			{
				ServerStatus serverStatus;
				if (lobbyData4 == null || lobbyData4.Length != 0)
				{
					if (!(lobbyData4 == "Steam user"))
					{
						if (!(lobbyData4 == "PlayFab user"))
						{
							if (!(lobbyData4 == "Dedicated"))
							{
								goto IL_124;
							}
							ServerJoinDataDedicated serverJoinDataDedicated = new ServerJoinDataDedicated(lobbyData3);
							if (!serverJoinDataDedicated.IsValid())
							{
								return null;
							}
							serverStatus = new ServerStatus(serverJoinDataDedicated);
						}
						else
						{
							serverStatus = new ServerStatus(new ServerJoinDataPlayFabUser(lobbyData3));
							if (!serverStatus.m_joinData.IsValid())
							{
								return null;
							}
						}
					}
					else
					{
						serverStatus = new ServerStatus(new ServerJoinDataSteamUser(joinUserID));
					}
				}
				else
				{
					serverStatus = new ServerStatus(new ServerJoinDataSteamUser(joinUserID));
				}
				serverStatus.UpdateStatus(OnlineStatus.Online, lobbyData, (uint)numLobbyMembers, lobbyData2, num, isPasswordProtected, (lobbyData5 == "1") ? PrivilegeManager.Platform.None : PrivilegeManager.Platform.Steam, true);
				return serverStatus;
			}
			IL_124:
			ZLog.LogError("Couldn't get lobby data for unknown backend \"" + lobbyData4 + "\"! " + this.KnownBackendsString());
			return null;
		}
		ZLog.Log("Failed to get lobby gameserver");
		return null;
	}

	// Token: 0x06000FC2 RID: 4034 RVA: 0x00069000 File Offset: 0x00067200
	public string KnownBackendsString()
	{
		List<string> list = new List<string>();
		list.Add("Steam user");
		list.Add("PlayFab user");
		list.Add("Dedicated");
		return "Known backends: " + string.Join(", ", from s in list
		select "\"" + s + "\"");
	}

	// Token: 0x06000FC3 RID: 4035 RVA: 0x0006906D File Offset: 0x0006726D
	public void GetServers(List<ServerStatus> allServers)
	{
		if (this.m_friendsFilter)
		{
			this.FilterServers(this.m_friendServers, allServers);
			return;
		}
		this.FilterServers(this.m_matchmakingServers, allServers);
		this.FilterServers(this.m_dedicatedServers, allServers);
	}

	// Token: 0x06000FC4 RID: 4036 RVA: 0x000690A0 File Offset: 0x000672A0
	private void FilterServers(List<ServerStatus> input, List<ServerStatus> allServers)
	{
		string text = this.m_nameFilter.ToLowerInvariant();
		foreach (ServerStatus serverStatus in input)
		{
			if (text.Length == 0 || serverStatus.m_joinData.m_serverName.ToLowerInvariant().Contains(text))
			{
				allServers.Add(serverStatus);
			}
			if (allServers.Count >= 200)
			{
				break;
			}
		}
	}

	// Token: 0x06000FC5 RID: 4037 RVA: 0x0006912C File Offset: 0x0006732C
	public bool CheckIfOnline(ServerJoinData dataToMatchAgainst, ref ServerStatus status)
	{
		for (int i = 0; i < this.m_friendServers.Count; i++)
		{
			if (this.m_friendServers[i].m_joinData.Equals(dataToMatchAgainst))
			{
				status = this.m_friendServers[i];
				return true;
			}
		}
		for (int j = 0; j < this.m_matchmakingServers.Count; j++)
		{
			if (this.m_matchmakingServers[j].m_joinData.Equals(dataToMatchAgainst))
			{
				status = this.m_matchmakingServers[j];
				return true;
			}
		}
		for (int k = 0; k < this.m_dedicatedServers.Count; k++)
		{
			if (this.m_dedicatedServers[k].m_joinData.Equals(dataToMatchAgainst))
			{
				status = this.m_dedicatedServers[k];
				return true;
			}
		}
		if (!this.IsRefreshing)
		{
			status = new ServerStatus(dataToMatchAgainst);
			status.UpdateStatus(OnlineStatus.Offline, dataToMatchAgainst.m_serverName, 0U, "", 0U, false, PrivilegeManager.Platform.Unknown, true);
			return true;
		}
		return false;
	}

	// Token: 0x06000FC6 RID: 4038 RVA: 0x00069221 File Offset: 0x00067421
	public bool GetJoinHost(out ServerJoinData joinData)
	{
		joinData = this.m_joinData;
		if (this.m_joinData == null)
		{
			return false;
		}
		if (!this.m_joinData.IsValid())
		{
			return false;
		}
		this.m_joinData = null;
		return true;
	}

	// Token: 0x06000FC7 RID: 4039 RVA: 0x00069254 File Offset: 0x00067454
	private void OnServerResponded(HServerListRequest request, int iServer)
	{
		gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(request, iServer);
		string serverName = serverDetails.GetServerName();
		SteamNetworkingIPAddr steamNetworkingIPAddr = default(SteamNetworkingIPAddr);
		steamNetworkingIPAddr.SetIPv4(serverDetails.m_NetAdr.GetIP(), serverDetails.m_NetAdr.GetConnectionPort());
		ServerStatus serverStatus = new ServerStatus(new ServerJoinDataDedicated(steamNetworkingIPAddr.GetIPv4(), steamNetworkingIPAddr.m_port));
		Dictionary<string, string> dictionary;
		string gameTags;
		string s;
		uint networkVersion;
		if (!ZSteamMatchmaking.<OnServerResponded>g__TryConvertTagsStringToDictionary|37_0(serverDetails.GetGameTags(), out dictionary) || !dictionary.TryGetValue("gameversion", out gameTags) || !dictionary.TryGetValue("networkversion", out s) || !uint.TryParse(s, out networkVersion))
		{
			gameTags = serverDetails.GetGameTags();
			networkVersion = 0U;
		}
		serverStatus.UpdateStatus(OnlineStatus.Online, serverName, (uint)serverDetails.m_nPlayers, gameTags, networkVersion, serverDetails.m_bPassword, PrivilegeManager.Platform.Steam, true);
		this.m_dedicatedServers.Add(serverStatus);
		this.m_updateTriggerAccumulator++;
		if (this.m_updateTriggerAccumulator > 100)
		{
			this.m_updateTriggerAccumulator = 0;
			this.m_serverListRevision++;
		}
	}

	// Token: 0x06000FC8 RID: 4040 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnServerFailedToRespond(HServerListRequest request, int iServer)
	{
	}

	// Token: 0x06000FC9 RID: 4041 RVA: 0x00069348 File Offset: 0x00067548
	private void OnRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
	{
		ZLog.Log("Refresh complete " + this.m_dedicatedServers.Count.ToString() + "  " + response.ToString());
		this.IsRefreshing = false;
		this.m_serverListRevision++;
	}

	// Token: 0x06000FCA RID: 4042 RVA: 0x0006939E File Offset: 0x0006759E
	public void SetNameFilter(string filter)
	{
		if (this.m_nameFilter == filter)
		{
			return;
		}
		this.m_nameFilter = filter;
		this.m_serverListRevision++;
	}

	// Token: 0x06000FCB RID: 4043 RVA: 0x000693C4 File Offset: 0x000675C4
	public void SetFriendFilter(bool enabled)
	{
		if (this.m_friendsFilter == enabled)
		{
			return;
		}
		this.m_friendsFilter = enabled;
		this.m_serverListRevision++;
	}

	// Token: 0x06000FCC RID: 4044 RVA: 0x000693E5 File Offset: 0x000675E5
	public int GetServerListRevision()
	{
		return this.m_serverListRevision;
	}

	// Token: 0x06000FCD RID: 4045 RVA: 0x000693ED File Offset: 0x000675ED
	public int GetTotalNrOfServers()
	{
		return this.m_matchmakingServers.Count + this.m_dedicatedServers.Count + this.m_friendServers.Count;
	}

	// Token: 0x1700009F RID: 159
	// (get) Token: 0x06000FCE RID: 4046 RVA: 0x00069412 File Offset: 0x00067612
	// (set) Token: 0x06000FCF RID: 4047 RVA: 0x0006941A File Offset: 0x0006761A
	public bool IsRefreshing { get; private set; }

	// Token: 0x06000FD1 RID: 4049 RVA: 0x0006943C File Offset: 0x0006763C
	[CompilerGenerated]
	internal static bool <OnServerResponded>g__TryConvertTagsStringToDictionary|37_0(string tagsString, out Dictionary<string, string> tags)
	{
		tags = new Dictionary<string, string>();
		bool flag = false;
		bool flag2 = false;
		char[] array = new char[tagsString.Length - 2];
		int num = 0;
		string text = null;
		string text2 = null;
		for (int i = 0; i < tagsString.Length; i++)
		{
			if (flag)
			{
				if (flag2)
				{
					array[num++] = tagsString[i];
					flag2 = false;
				}
				else if (tagsString[i] == '\\')
				{
					flag2 = true;
				}
				else if (tagsString[i] == '"')
				{
					flag = false;
				}
				else
				{
					array[num++] = tagsString[i];
				}
			}
			else if (!char.IsWhiteSpace(tagsString[i]))
			{
				if (tagsString[i] == '"')
				{
					if (num != 0)
					{
						return false;
					}
					flag = true;
				}
				else if (tagsString[i] == '=' || tagsString[i] == ',')
				{
					string text3;
					if (num == 0)
					{
						text3 = "";
					}
					else
					{
						text3 = new string(array, 0, num);
						num = 0;
					}
					if (tagsString[i] == '=')
					{
						if (text != null || text2 != null)
						{
							return false;
						}
						text = text3;
					}
					else
					{
						if (text == null || text2 != null)
						{
							return false;
						}
						text2 = text3;
						tags.Add(text, text2);
						text = null;
						text2 = null;
					}
				}
			}
		}
		if (text != null && text2 == null)
		{
			string text4;
			if (num == 0)
			{
				text4 = "";
			}
			else
			{
				text4 = new string(array, 0, num);
			}
			text2 = text4;
			tags.Add(text, text2);
		}
		return true;
	}

	// Token: 0x040010CF RID: 4303
	private static ZSteamMatchmaking m_instance;

	// Token: 0x040010D0 RID: 4304
	private const int maxServers = 200;

	// Token: 0x040010D1 RID: 4305
	private List<ServerStatus> m_matchmakingServers = new List<ServerStatus>();

	// Token: 0x040010D2 RID: 4306
	private List<ServerStatus> m_dedicatedServers = new List<ServerStatus>();

	// Token: 0x040010D3 RID: 4307
	private List<ServerStatus> m_friendServers = new List<ServerStatus>();

	// Token: 0x040010D4 RID: 4308
	private int m_serverListRevision;

	// Token: 0x040010D5 RID: 4309
	private int m_updateTriggerAccumulator;

	// Token: 0x040010D6 RID: 4310
	private CallResult<LobbyCreated_t> m_lobbyCreated;

	// Token: 0x040010D7 RID: 4311
	private CallResult<LobbyMatchList_t> m_lobbyMatchList;

	// Token: 0x040010D8 RID: 4312
	private CallResult<LobbyEnter_t> m_lobbyEntered;

	// Token: 0x040010D9 RID: 4313
	private Callback<GameServerChangeRequested_t> m_changeServer;

	// Token: 0x040010DA RID: 4314
	private Callback<GameLobbyJoinRequested_t> m_joinRequest;

	// Token: 0x040010DB RID: 4315
	private Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

	// Token: 0x040010DC RID: 4316
	private Callback<GetAuthSessionTicketResponse_t> m_authSessionTicketResponse;

	// Token: 0x040010DD RID: 4317
	private Callback<SteamServerConnectFailure_t> m_steamServerConnectFailure;

	// Token: 0x040010DE RID: 4318
	private Callback<SteamServersConnected_t> m_steamServersConnected;

	// Token: 0x040010DF RID: 4319
	private Callback<SteamServersDisconnected_t> m_steamServersDisconnected;

	// Token: 0x040010E0 RID: 4320
	private ZSteamMatchmaking.ServerRegistered serverRegisteredCallback;

	// Token: 0x040010E1 RID: 4321
	private CSteamID m_myLobby = CSteamID.Nil;

	// Token: 0x040010E2 RID: 4322
	private CSteamID m_queuedJoinLobby = CSteamID.Nil;

	// Token: 0x040010E3 RID: 4323
	private ServerJoinData m_joinData;

	// Token: 0x040010E4 RID: 4324
	private List<KeyValuePair<CSteamID, string>> m_requestedFriendGames = new List<KeyValuePair<CSteamID, string>>();

	// Token: 0x040010E5 RID: 4325
	private ISteamMatchmakingServerListResponse m_steamServerCallbackHandler;

	// Token: 0x040010E6 RID: 4326
	private ISteamMatchmakingPingResponse m_joinServerCallbackHandler;

	// Token: 0x040010E7 RID: 4327
	private HServerQuery m_joinQuery;

	// Token: 0x040010E8 RID: 4328
	private HServerListRequest m_serverListRequest;

	// Token: 0x040010E9 RID: 4329
	private bool m_haveListRequest;

	// Token: 0x040010EA RID: 4330
	private bool m_refreshingDedicatedServers;

	// Token: 0x040010EB RID: 4331
	private bool m_refreshingPublicGames;

	// Token: 0x040010ED RID: 4333
	private string m_registerServerName = "";

	// Token: 0x040010EE RID: 4334
	private bool m_registerPassword;

	// Token: 0x040010EF RID: 4335
	private string m_registerGameVerson = "";

	// Token: 0x040010F0 RID: 4336
	private uint m_registerNetworkVerson;

	// Token: 0x040010F1 RID: 4337
	private string m_nameFilter = "";

	// Token: 0x040010F2 RID: 4338
	private bool m_friendsFilter = true;

	// Token: 0x040010F3 RID: 4339
	private HAuthTicket m_authTicket = HAuthTicket.Invalid;

	// Token: 0x02000189 RID: 393
	// (Invoke) Token: 0x06000FD3 RID: 4051
	public delegate void ServerRegistered(bool success);
}
