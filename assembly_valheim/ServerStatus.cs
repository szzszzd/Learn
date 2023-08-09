using System;

// Token: 0x0200014A RID: 330
public class ServerStatus
{
	// Token: 0x17000077 RID: 119
	// (get) Token: 0x06000C67 RID: 3175 RVA: 0x000591E4 File Offset: 0x000573E4
	// (set) Token: 0x06000C68 RID: 3176 RVA: 0x000591EC File Offset: 0x000573EC
	public ServerJoinData m_joinData { get; private set; }

	// Token: 0x17000078 RID: 120
	// (get) Token: 0x06000C69 RID: 3177 RVA: 0x000591F5 File Offset: 0x000573F5
	// (set) Token: 0x06000C6A RID: 3178 RVA: 0x000591FD File Offset: 0x000573FD
	public ServerPingStatus PingStatus { get; private set; }

	// Token: 0x17000079 RID: 121
	// (get) Token: 0x06000C6B RID: 3179 RVA: 0x00059206 File Offset: 0x00057406
	// (set) Token: 0x06000C6C RID: 3180 RVA: 0x0005920E File Offset: 0x0005740E
	public OnlineStatus OnlineStatus { get; private set; }

	// Token: 0x1700007A RID: 122
	// (get) Token: 0x06000C6D RID: 3181 RVA: 0x00059217 File Offset: 0x00057417
	public bool IsCrossplay
	{
		get
		{
			return this.PlatformRestriction == PrivilegeManager.Platform.None;
		}
	}

	// Token: 0x1700007B RID: 123
	// (get) Token: 0x06000C6E RID: 3182 RVA: 0x00059222 File Offset: 0x00057422
	public bool IsRestrictedToOwnPlatform
	{
		get
		{
			return this.PlatformRestriction == PrivilegeManager.GetCurrentPlatform();
		}
	}

	// Token: 0x1700007C RID: 124
	// (get) Token: 0x06000C6F RID: 3183 RVA: 0x00059231 File Offset: 0x00057431
	public bool IsJoinable
	{
		get
		{
			return this.IsRestrictedToOwnPlatform || (PrivilegeManager.CanCrossplay && this.IsCrossplay);
		}
	}

	// Token: 0x1700007D RID: 125
	// (get) Token: 0x06000C70 RID: 3184 RVA: 0x0005924C File Offset: 0x0005744C
	// (set) Token: 0x06000C71 RID: 3185 RVA: 0x00059254 File Offset: 0x00057454
	public uint m_playerCount { get; private set; }

	// Token: 0x1700007E RID: 126
	// (get) Token: 0x06000C72 RID: 3186 RVA: 0x0005925D File Offset: 0x0005745D
	// (set) Token: 0x06000C73 RID: 3187 RVA: 0x00059265 File Offset: 0x00057465
	public string m_gameVersion { get; private set; }

	// Token: 0x1700007F RID: 127
	// (get) Token: 0x06000C74 RID: 3188 RVA: 0x0005926E File Offset: 0x0005746E
	// (set) Token: 0x06000C75 RID: 3189 RVA: 0x00059276 File Offset: 0x00057476
	public uint m_networkVersion { get; private set; }

	// Token: 0x17000080 RID: 128
	// (get) Token: 0x06000C76 RID: 3190 RVA: 0x0005927F File Offset: 0x0005747F
	// (set) Token: 0x06000C77 RID: 3191 RVA: 0x00059287 File Offset: 0x00057487
	public bool m_isPasswordProtected { get; private set; }

	// Token: 0x17000081 RID: 129
	// (get) Token: 0x06000C78 RID: 3192 RVA: 0x00059290 File Offset: 0x00057490
	// (set) Token: 0x06000C79 RID: 3193 RVA: 0x000592DC File Offset: 0x000574DC
	public PrivilegeManager.Platform PlatformRestriction
	{
		get
		{
			if (this.m_joinData is ServerJoinDataSteamUser)
			{
				return PrivilegeManager.Platform.Steam;
			}
			if (this.OnlineStatus == OnlineStatus.Online && this.m_platformRestriction == PrivilegeManager.Platform.Unknown)
			{
				ZLog.LogError("Platform restriction must always be set when the online status is online, but it wasn't!\nServer: " + this.m_joinData.m_serverName);
			}
			return this.m_platformRestriction;
		}
		private set
		{
			if (this.m_joinData is ServerJoinDataSteamUser && value != PrivilegeManager.Platform.Steam)
			{
				ZLog.LogError("Can't set platform restriction of Steam server to anything other than Steam - it's always restricted to Steam!");
				return;
			}
			this.m_platformRestriction = value;
		}
	}

	// Token: 0x06000C7A RID: 3194 RVA: 0x00059301 File Offset: 0x00057501
	public ServerStatus(ServerJoinData joinData)
	{
		this.m_joinData = joinData;
		this.OnlineStatus = OnlineStatus.Unknown;
	}

	// Token: 0x06000C7B RID: 3195 RVA: 0x00059318 File Offset: 0x00057518
	public void UpdateStatus(OnlineStatus onlineStatus, string serverName, uint playerCount, string gameVersion, uint networkVersion, bool isPasswordProtected, PrivilegeManager.Platform platformRestriction, bool affectPingStatus = true)
	{
		this.PlatformRestriction = platformRestriction;
		this.OnlineStatus = onlineStatus;
		this.m_joinData.m_serverName = serverName;
		this.m_playerCount = playerCount;
		this.m_gameVersion = gameVersion;
		this.m_networkVersion = networkVersion;
		this.m_isPasswordProtected = isPasswordProtected;
		if (affectPingStatus)
		{
			switch (onlineStatus)
			{
			case OnlineStatus.Online:
				this.PingStatus = ServerPingStatus.Success;
				return;
			case OnlineStatus.Offline:
				this.PingStatus = ServerPingStatus.CouldNotReach;
				return;
			}
			this.PingStatus = ServerPingStatus.NotStarted;
		}
	}

	// Token: 0x17000082 RID: 130
	// (get) Token: 0x06000C7C RID: 3196 RVA: 0x0005938E File Offset: 0x0005758E
	private bool DoSteamPing
	{
		get
		{
			return this.m_joinData is ServerJoinDataSteamUser || this.m_joinData is ServerJoinDataDedicated;
		}
	}

	// Token: 0x17000083 RID: 131
	// (get) Token: 0x06000C7D RID: 3197 RVA: 0x000593AD File Offset: 0x000575AD
	private bool DoPlayFabPing
	{
		get
		{
			return this.m_joinData is ServerJoinDataPlayFabUser || this.m_joinData is ServerJoinDataDedicated;
		}
	}

	// Token: 0x06000C7E RID: 3198 RVA: 0x000593CC File Offset: 0x000575CC
	private void PlayFabPingSuccess(PlayFabMatchmakingServerData serverData)
	{
		if (this.PingStatus != ServerPingStatus.AwaitingResponse)
		{
			return;
		}
		if (this.OnlineStatus != OnlineStatus.Online)
		{
			if (serverData != null)
			{
				this.UpdateStatus(OnlineStatus.Online, serverData.serverName, serverData.numPlayers, serverData.gameVersion, serverData.networkVersion, serverData.havePassword, PrivilegeManager.ParsePlatform(serverData.platformRestriction), false);
			}
			this.m_isAwaitingPlayFabPingResponse = false;
		}
	}

	// Token: 0x06000C7F RID: 3199 RVA: 0x00059426 File Offset: 0x00057626
	private void PlayFabPingFailed(ZPLayFabMatchmakingFailReason failReason)
	{
		if (this.PingStatus != ServerPingStatus.AwaitingResponse)
		{
			return;
		}
		this.m_isAwaitingPlayFabPingResponse = false;
	}

	// Token: 0x06000C80 RID: 3200 RVA: 0x0005943C File Offset: 0x0005763C
	public void Ping()
	{
		this.PingStatus = ServerPingStatus.AwaitingResponse;
		if (this.DoPlayFabPing)
		{
			if (!PlayFabManager.IsLoggedIn)
			{
				return;
			}
			if (this.m_joinData is ServerJoinDataPlayFabUser)
			{
				ZPlayFabMatchmaking.CheckHostOnlineStatus((this.m_joinData as ServerJoinDataPlayFabUser).m_remotePlayerId, new ZPlayFabMatchmakingSuccessCallback(this.PlayFabPingSuccess), new ZPlayFabMatchmakingFailedCallback(this.PlayFabPingFailed), false);
			}
			else if (this.m_joinData is ServerJoinDataDedicated)
			{
				ZPlayFabMatchmaking.FindHostByIp((this.m_joinData as ServerJoinDataDedicated).GetIPPortString(), new ZPlayFabMatchmakingSuccessCallback(this.PlayFabPingSuccess), new ZPlayFabMatchmakingFailedCallback(this.PlayFabPingFailed), false);
			}
			else
			{
				ZLog.LogError("Tried to ping an unsupported server type with server data " + this.m_joinData.ToString());
			}
			this.m_isAwaitingPlayFabPingResponse = true;
		}
		if (this.DoSteamPing)
		{
			this.m_isAwaitingSteamPingResponse = true;
		}
	}

	// Token: 0x06000C81 RID: 3201 RVA: 0x00059510 File Offset: 0x00057710
	private void Update()
	{
		if (this.DoSteamPing && this.m_isAwaitingSteamPingResponse)
		{
			ServerStatus serverStatus = null;
			if (ZSteamMatchmaking.instance.CheckIfOnline(this.m_joinData, ref serverStatus))
			{
				if (serverStatus.m_joinData != null && serverStatus.OnlineStatus == OnlineStatus.Online && this.OnlineStatus != OnlineStatus.Online)
				{
					this.UpdateStatus(OnlineStatus.Online, serverStatus.m_joinData.m_serverName, serverStatus.m_playerCount, serverStatus.m_gameVersion, serverStatus.m_networkVersion, serverStatus.m_isPasswordProtected, serverStatus.PlatformRestriction, true);
				}
				this.m_isAwaitingSteamPingResponse = false;
			}
		}
	}

	// Token: 0x06000C82 RID: 3202 RVA: 0x0005959C File Offset: 0x0005779C
	public bool TryGetResult()
	{
		this.Update();
		uint num = 0U;
		uint num2 = 0U;
		if (this.DoPlayFabPing)
		{
			num += 1U;
			if (!this.m_isAwaitingPlayFabPingResponse)
			{
				num2 += 1U;
				if (this.OnlineStatus == OnlineStatus.Online)
				{
					this.PingStatus = ServerPingStatus.Success;
					return true;
				}
			}
		}
		if (this.DoSteamPing)
		{
			num += 1U;
			if (!this.m_isAwaitingSteamPingResponse)
			{
				num2 += 1U;
				if (this.OnlineStatus == OnlineStatus.Online)
				{
					this.PingStatus = ServerPingStatus.Success;
					return true;
				}
			}
		}
		if (num == num2)
		{
			this.PingStatus = ServerPingStatus.CouldNotReach;
			this.OnlineStatus = OnlineStatus.Offline;
			return true;
		}
		return false;
	}

	// Token: 0x06000C83 RID: 3203 RVA: 0x0005961C File Offset: 0x0005781C
	public void Reset()
	{
		this.PingStatus = ServerPingStatus.NotStarted;
		this.OnlineStatus = OnlineStatus.Unknown;
		this.m_playerCount = 0U;
		this.m_gameVersion = null;
		this.m_networkVersion = 0U;
		this.m_isPasswordProtected = false;
		if (!(this.m_joinData is ServerJoinDataSteamUser))
		{
			this.PlatformRestriction = PrivilegeManager.Platform.Unknown;
		}
		this.m_isAwaitingSteamPingResponse = false;
		this.m_isAwaitingPlayFabPingResponse = false;
	}

	// Token: 0x04000EB9 RID: 3769
	private PrivilegeManager.Platform m_platformRestriction;

	// Token: 0x04000EBA RID: 3770
	private bool m_isAwaitingSteamPingResponse;

	// Token: 0x04000EBB RID: 3771
	private bool m_isAwaitingPlayFabPingResponse;
}
