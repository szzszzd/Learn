using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.MultiplayerModels;

// Token: 0x0200019C RID: 412
internal class ZPlayFabLobbySearch
{
	// Token: 0x170000AB RID: 171
	// (get) Token: 0x0600108E RID: 4238 RVA: 0x0006C703 File Offset: 0x0006A903
	// (set) Token: 0x0600108F RID: 4239 RVA: 0x0006C70B File Offset: 0x0006A90B
	internal bool IsDone { get; private set; }

	// Token: 0x06001090 RID: 4240 RVA: 0x0006C714 File Offset: 0x0006A914
	internal ZPlayFabLobbySearch(ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, string searchFilter, string serverFilter)
	{
		this.m_successAction = successAction;
		this.m_failedAction = failedAction;
		this.m_searchFilter = searchFilter;
		this.m_serverFilter = serverFilter;
		if (serverFilter == null)
		{
			this.FindLobby();
			this.m_retries = 1;
			return;
		}
		this.m_pages = this.CreatePages();
	}

	// Token: 0x06001091 RID: 4241 RVA: 0x0006C784 File Offset: 0x0006A984
	internal ZPlayFabLobbySearch(ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, string searchFilter, bool joinLobby)
	{
		this.m_successAction = successAction;
		this.m_failedAction = failedAction;
		this.m_searchFilter = searchFilter;
		this.m_joinLobby = joinLobby;
		if (joinLobby)
		{
			this.FindLobby();
			this.m_retries = 3;
		}
	}

	// Token: 0x06001092 RID: 4242 RVA: 0x0006C7E8 File Offset: 0x0006A9E8
	private Queue<int> CreatePages()
	{
		Queue<int> queue = new Queue<int>();
		for (int i = 0; i < 4; i++)
		{
			queue.Enqueue(i);
		}
		return queue;
	}

	// Token: 0x06001093 RID: 4243 RVA: 0x0006C80F File Offset: 0x0006AA0F
	internal void Update(float deltaTime)
	{
		if (this.m_retryIn > 0f)
		{
			this.m_retryIn -= deltaTime;
			if (this.m_retryIn <= 0f)
			{
				this.FindLobby();
			}
		}
		this.TickAPICallRateLimiter();
	}

	// Token: 0x06001094 RID: 4244 RVA: 0x0006C848 File Offset: 0x0006AA48
	internal void FindLobby()
	{
		if (this.m_serverFilter == null)
		{
			FindLobbiesRequest request = new FindLobbiesRequest
			{
				Filter = this.m_searchFilter
			};
			this.QueueAPICall(delegate
			{
				PlayFabMultiplayerAPI.FindLobbies(request, new Action<FindLobbiesResult>(this.OnFindLobbySuccess), new Action<PlayFabError>(this.OnFindLobbyFailed), null, null);
			});
			return;
		}
		this.FindLobbyWithPagination(this.m_pages.Dequeue());
	}

	// Token: 0x06001095 RID: 4245 RVA: 0x0006C8A8 File Offset: 0x0006AAA8
	private void FindLobbyWithPagination(int page)
	{
		FindLobbiesRequest request = new FindLobbiesRequest
		{
			Filter = this.m_searchFilter + string.Format(" and {0} eq {1}", "number_key11", page),
			Pagination = new PaginationRequest
			{
				PageSizeRequested = new uint?(50U)
			}
		};
		if (this.m_verboseLog)
		{
			ZLog.Log(string.Format("Page {0}, {1} remains: {2}", page, this.m_pages.Count, request.Filter));
		}
		this.QueueAPICall(delegate
		{
			PlayFabMultiplayerAPI.FindLobbies(request, new Action<FindLobbiesResult>(this.OnFindServersSuccess), new Action<PlayFabError>(this.OnFindLobbyFailed), null, null);
		});
	}

	// Token: 0x06001096 RID: 4246 RVA: 0x0006C958 File Offset: 0x0006AB58
	private void RetryOrFail(string error)
	{
		if (this.m_retries > 0)
		{
			this.m_retries--;
			this.m_retryIn = 1f;
			return;
		}
		ZLog.Log(string.Format("PlayFab lobby matching search filter '{0}': {1}", this.m_searchFilter, error));
		this.OnFailed(ZPLayFabMatchmakingFailReason.Unknown);
	}

	// Token: 0x06001097 RID: 4247 RVA: 0x0006C9A5 File Offset: 0x0006ABA5
	private void OnFindLobbyFailed(PlayFabError error)
	{
		if (!this.IsDone)
		{
			this.RetryOrFail(error.ToString());
		}
	}

	// Token: 0x06001098 RID: 4248 RVA: 0x0006C9BC File Offset: 0x0006ABBC
	private void OnFindLobbySuccess(FindLobbiesResult result)
	{
		if (this.IsDone)
		{
			return;
		}
		if (result.Lobbies.Count == 0)
		{
			this.RetryOrFail("Got back zero lobbies");
			return;
		}
		LobbySummary lobbySummary = result.Lobbies[0];
		if (result.Lobbies.Count > 1)
		{
			ZLog.LogWarning(string.Format("Expected zero or one lobby got {0} matching lobbies, returning newest lobby", result.Lobbies.Count));
			long num = long.Parse(lobbySummary.SearchData["string_key9"]);
			foreach (LobbySummary lobbySummary2 in result.Lobbies)
			{
				long num2 = long.Parse(lobbySummary2.SearchData["string_key9"]);
				if (num < num2)
				{
					lobbySummary = lobbySummary2;
					num = num2;
				}
			}
		}
		if (this.m_joinLobby)
		{
			this.JoinLobby(lobbySummary.LobbyId, lobbySummary.ConnectionString);
			ZPlayFabMatchmaking.JoinCode = lobbySummary.SearchData["string_key4"];
			return;
		}
		this.DeliverLobby(lobbySummary);
		this.IsDone = true;
	}

	// Token: 0x06001099 RID: 4249 RVA: 0x0006CADC File Offset: 0x0006ACDC
	private void JoinLobby(string lobbyId, string connectionString)
	{
		JoinLobbyRequest request = new JoinLobbyRequest
		{
			ConnectionString = connectionString,
			MemberEntity = ZPlayFabMatchmaking.GetEntityKeyForLocalUser()
		};
		Action<JoinLobbyResult> <>9__1;
		Action<PlayFabError> <>9__2;
		this.QueueAPICall(delegate
		{
			JoinLobbyRequest request = request;
			Action<JoinLobbyResult> resultCallback;
			if ((resultCallback = <>9__1) == null)
			{
				resultCallback = (<>9__1 = delegate(JoinLobbyResult result)
				{
					this.OnJoinLobbySuccess(result.LobbyId);
				});
			}
			Action<PlayFabError> errorCallback;
			if ((errorCallback = <>9__2) == null)
			{
				errorCallback = (<>9__2 = delegate(PlayFabError error)
				{
					this.OnJoinLobbyFailed(error, lobbyId);
				});
			}
			PlayFabMultiplayerAPI.JoinLobby(request, resultCallback, errorCallback, null, null);
		});
	}

	// Token: 0x0600109A RID: 4250 RVA: 0x0006CB2C File Offset: 0x0006AD2C
	private void OnJoinLobbySuccess(string lobbyId)
	{
		if (this.IsDone)
		{
			return;
		}
		GetLobbyRequest request = new GetLobbyRequest
		{
			LobbyId = lobbyId
		};
		this.QueueAPICall(delegate
		{
			PlayFabMultiplayerAPI.GetLobby(request, new Action<GetLobbyResult>(this.OnGetLobbySuccess), new Action<PlayFabError>(this.OnGetLobbyFailed), null, null);
		});
	}

	// Token: 0x0600109B RID: 4251 RVA: 0x0006CB74 File Offset: 0x0006AD74
	private void OnJoinLobbyFailed(PlayFabError error, string lobbyId)
	{
		PlayFabErrorCode error2 = error.Error;
		if (error2 <= PlayFabErrorCode.APIClientRequestRateLimitExceeded)
		{
			if (error2 != PlayFabErrorCode.APIRequestLimitExceeded && error2 != PlayFabErrorCode.APIClientRequestRateLimitExceeded)
			{
				goto IL_5D;
			}
		}
		else
		{
			if (error2 == PlayFabErrorCode.LobbyPlayerAlreadyJoined)
			{
				this.OnJoinLobbySuccess(lobbyId);
				return;
			}
			if (error2 == PlayFabErrorCode.LobbyNotJoinable)
			{
				ZLog.Log("Can't join lobby because it's not joinable, likely because it's full.");
				this.OnFailed(ZPLayFabMatchmakingFailReason.ServerFull);
				return;
			}
			if (error2 != PlayFabErrorCode.LobbyPlayerMaxLobbyLimitExceeded)
			{
				goto IL_5D;
			}
		}
		this.OnFailed(ZPLayFabMatchmakingFailReason.APIRequestLimitExceeded);
		return;
		IL_5D:
		ZLog.LogError("Failed to get lobby: " + error.ToString());
		this.OnFailed(ZPLayFabMatchmakingFailReason.Unknown);
	}

	// Token: 0x0600109C RID: 4252 RVA: 0x0006CBFC File Offset: 0x0006ADFC
	private void DeliverLobby(LobbySummary lobbySummary)
	{
		try
		{
			bool flag;
			bool flag2;
			long tickCreated;
			uint networkVersion;
			if (!bool.TryParse(lobbySummary.SearchData["string_key3"], out flag) || !bool.TryParse(lobbySummary.SearchData["string_key7"], out flag2) || !long.TryParse(lobbySummary.SearchData["string_key9"], out tickCreated) || !uint.TryParse(lobbySummary.SearchData["number_key13"], out networkVersion))
			{
				ZLog.LogWarning("Got PlayFab lobby entry with invalid data");
			}
			else
			{
				string text = lobbySummary.SearchData["string_key6"];
				GameVersion lhs;
				if (!GameVersion.TryParseGameVersion(text, out lhs) || lhs < global::Version.FirstVersionWithNetworkVersion)
				{
					networkVersion = 0U;
				}
				PlayFabMatchmakingServerData playFabMatchmakingServerData = new PlayFabMatchmakingServerData
				{
					remotePlayerId = lobbySummary.SearchData["string_key1"],
					xboxUserId = lobbySummary.SearchData["string_key8"],
					isCommunityServer = flag,
					havePassword = flag,
					isDedicatedServer = flag2,
					joinCode = lobbySummary.SearchData["string_key4"],
					lobbyId = lobbySummary.LobbyId,
					numPlayers = (uint)((ulong)lobbySummary.CurrentPlayers - (ulong)(flag2 ? 1L : 0L)),
					tickCreated = tickCreated,
					serverIp = lobbySummary.SearchData["string_key10"],
					serverName = lobbySummary.SearchData["string_key5"],
					gameVersion = text,
					networkVersion = networkVersion,
					platformRestriction = lobbySummary.SearchData["string_key12"]
				};
				if (this.m_verboseLog)
				{
					ZLog.Log("Deliver server data\n" + playFabMatchmakingServerData.ToString());
				}
				this.m_successAction(playFabMatchmakingServerData);
			}
		}
		catch (KeyNotFoundException)
		{
			ZLog.LogWarning("Got PlayFab lobby entry with missing key(s)");
			this.m_successAction(null);
		}
	}

	// Token: 0x0600109D RID: 4253 RVA: 0x0006CDE4 File Offset: 0x0006AFE4
	private void OnFindServersSuccess(FindLobbiesResult result)
	{
		if (this.IsDone)
		{
			return;
		}
		foreach (LobbySummary lobbySummary in result.Lobbies)
		{
			if (lobbySummary.SearchData["string_key5"].ToLowerInvariant().Contains(this.m_serverFilter.ToLowerInvariant()))
			{
				this.DeliverLobby(lobbySummary);
			}
		}
		if (this.m_pages.Count == 0)
		{
			this.OnFailed(ZPLayFabMatchmakingFailReason.None);
			return;
		}
		this.FindLobbyWithPagination(this.m_pages.Dequeue());
	}

	// Token: 0x0600109E RID: 4254 RVA: 0x0006CE90 File Offset: 0x0006B090
	private void OnGetLobbySuccess(GetLobbyResult result)
	{
		PlayFabMatchmakingServerData playFabMatchmakingServerData = ZPlayFabLobbySearch.ToServerData(result);
		if (this.IsDone)
		{
			this.OnFailed(ZPLayFabMatchmakingFailReason.Cancelled);
			return;
		}
		if (playFabMatchmakingServerData == null)
		{
			this.OnFailed(ZPLayFabMatchmakingFailReason.InvalidServerData);
			return;
		}
		this.IsDone = true;
		ZLog.Log("Get Lobby\n" + playFabMatchmakingServerData.ToString());
		this.m_successAction(playFabMatchmakingServerData);
	}

	// Token: 0x0600109F RID: 4255 RVA: 0x0006CEE7 File Offset: 0x0006B0E7
	private void OnGetLobbyFailed(PlayFabError error)
	{
		ZLog.LogError("Failed to get lobby: " + error.ToString());
		this.OnFailed(ZPLayFabMatchmakingFailReason.Unknown);
	}

	// Token: 0x060010A0 RID: 4256 RVA: 0x0006CF08 File Offset: 0x0006B108
	private static PlayFabMatchmakingServerData ToServerData(GetLobbyResult result)
	{
		Dictionary<string, string> lobbyData = result.Lobby.LobbyData;
		Dictionary<string, string> searchData = result.Lobby.SearchData;
		PlayFabMatchmakingServerData result2;
		try
		{
			string text = searchData["string_key6"];
			uint networkVersion = uint.Parse(searchData["number_key13"]);
			GameVersion lhs;
			if (!GameVersion.TryParseGameVersion(text, out lhs) || lhs < global::Version.FirstVersionWithNetworkVersion)
			{
				networkVersion = 0U;
			}
			result2 = new PlayFabMatchmakingServerData
			{
				havePassword = bool.Parse(lobbyData[PlayFabAttrKey.HavePassword.ToKeyString()]),
				isCommunityServer = bool.Parse(searchData["string_key3"]),
				isDedicatedServer = bool.Parse(searchData["string_key7"]),
				joinCode = searchData["string_key4"],
				lobbyId = result.Lobby.LobbyId,
				networkId = lobbyData[PlayFabAttrKey.NetworkId.ToKeyString()],
				numPlayers = (uint)result.Lobby.Members.Count,
				remotePlayerId = searchData["string_key1"],
				serverIp = searchData["string_key10"],
				serverName = searchData["string_key5"],
				tickCreated = long.Parse(searchData["string_key9"]),
				gameVersion = text,
				networkVersion = networkVersion,
				worldName = lobbyData[PlayFabAttrKey.WorldName.ToKeyString()],
				xboxUserId = searchData["string_key8"],
				platformRestriction = searchData["string_key12"]
			};
		}
		catch
		{
			result2 = null;
		}
		return result2;
	}

	// Token: 0x060010A1 RID: 4257 RVA: 0x0006D0AC File Offset: 0x0006B2AC
	private void OnFailed(ZPLayFabMatchmakingFailReason failReason)
	{
		if (!this.IsDone)
		{
			this.IsDone = true;
			if (this.m_failedAction != null)
			{
				this.m_failedAction(failReason);
			}
		}
	}

	// Token: 0x060010A2 RID: 4258 RVA: 0x0006D0D1 File Offset: 0x0006B2D1
	internal void Cancel()
	{
		this.IsDone = true;
	}

	// Token: 0x060010A3 RID: 4259 RVA: 0x0006D0DA File Offset: 0x0006B2DA
	private void QueueAPICall(ZPlayFabLobbySearch.QueueableAPICall apiCallDelegate)
	{
		this.m_APICallQueue.Enqueue(apiCallDelegate);
		this.TickAPICallRateLimiter();
	}

	// Token: 0x060010A4 RID: 4260 RVA: 0x0006D0F0 File Offset: 0x0006B2F0
	private void TickAPICallRateLimiter()
	{
		if (this.m_APICallQueue.Count <= 0)
		{
			return;
		}
		if ((DateTime.UtcNow - this.m_previousAPICallTime).TotalSeconds >= 2.0)
		{
			this.m_APICallQueue.Dequeue()();
			this.m_previousAPICallTime = DateTime.UtcNow;
		}
	}

	// Token: 0x04001170 RID: 4464
	private readonly ZPlayFabMatchmakingSuccessCallback m_successAction;

	// Token: 0x04001171 RID: 4465
	private readonly ZPlayFabMatchmakingFailedCallback m_failedAction;

	// Token: 0x04001172 RID: 4466
	private readonly string m_searchFilter;

	// Token: 0x04001173 RID: 4467
	private readonly string m_serverFilter;

	// Token: 0x04001174 RID: 4468
	private readonly Queue<int> m_pages;

	// Token: 0x04001175 RID: 4469
	private readonly bool m_joinLobby;

	// Token: 0x04001176 RID: 4470
	private readonly bool m_verboseLog;

	// Token: 0x04001177 RID: 4471
	private int m_retries;

	// Token: 0x04001178 RID: 4472
	private float m_retryIn = -1f;

	// Token: 0x0400117A RID: 4474
	private const float rateLimit = 2f;

	// Token: 0x0400117B RID: 4475
	private DateTime m_previousAPICallTime = DateTime.MinValue;

	// Token: 0x0400117C RID: 4476
	private Queue<ZPlayFabLobbySearch.QueueableAPICall> m_APICallQueue = new Queue<ZPlayFabLobbySearch.QueueableAPICall>();

	// Token: 0x0200019D RID: 413
	// (Invoke) Token: 0x060010A6 RID: 4262
	private delegate void QueueableAPICall();
}
