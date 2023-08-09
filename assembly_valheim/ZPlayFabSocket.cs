using System;
using System.Collections.Generic;
using PlayFab.Party;
using UnityEngine;

// Token: 0x020001AE RID: 430
public class ZPlayFabSocket : ZNetStats, IDisposable, ISocket
{
	// Token: 0x0600111F RID: 4383 RVA: 0x0006EDD0 File Offset: 0x0006CFD0
	public ZPlayFabSocket()
	{
		this.m_state = ZPlayFabSocketState.LISTEN;
		PlayFabMultiplayerManager.Get().LogLevel = PlayFabMultiplayerManager.LogLevelType.None;
	}

	// Token: 0x06001120 RID: 4384 RVA: 0x0006EE5C File Offset: 0x0006D05C
	public ZPlayFabSocket(string remotePlayerId, Action<PlayFabMatchmakingServerData> serverDataFoundCallback)
	{
		PlayFabMultiplayerManager.Get().LogLevel = PlayFabMultiplayerManager.LogLevelType.None;
		this.m_state = ZPlayFabSocketState.CONNECTING;
		this.m_remotePlayerId = remotePlayerId;
		this.ClientConnect();
		PlayFabMultiplayerManager.Get().OnDataMessageReceived += this.OnDataMessageReceived;
		PlayFabMultiplayerManager.Get().OnRemotePlayerJoined += this.OnRemotePlayerJoined;
		this.m_isClient = true;
		this.m_platformPlayerId = PrivilegeManager.GetNetworkUserId();
		this.m_serverDataFoundCallback = serverDataFoundCallback;
		ZPackage zpackage = new ZPackage();
		zpackage.Write(1);
		zpackage.Write(this.m_platformPlayerId);
		this.Send(zpackage, 64);
		ZLog.Log("PlayFab socket with remote ID " + remotePlayerId + " sent local Platform ID " + this.GetHostName());
	}

	// Token: 0x06001121 RID: 4385 RVA: 0x0006EF74 File Offset: 0x0006D174
	private void ClientConnect()
	{
		ZPlayFabMatchmaking.CheckHostOnlineStatus(this.m_remotePlayerId, new ZPlayFabMatchmakingSuccessCallback(this.OnRemotePlayerSessionFound), new ZPlayFabMatchmakingFailedCallback(this.OnRemotePlayerNotFound), true);
	}

	// Token: 0x06001122 RID: 4386 RVA: 0x0006EF9C File Offset: 0x0006D19C
	private ZPlayFabSocket(PlayFabPlayer remotePlayer)
	{
		this.InitRemotePlayer(remotePlayer);
		this.Connect(remotePlayer);
		this.m_isClient = false;
		this.m_remotePlayerId = remotePlayer.EntityKey.Id;
		PlayFabMultiplayerManager.Get().OnDataMessageReceived += this.OnDataMessageReceived;
		ZLog.Log("PlayFab listen socket child connected to remote player " + this.m_remotePlayerId);
	}

	// Token: 0x06001123 RID: 4387 RVA: 0x0006F064 File Offset: 0x0006D264
	private void InitRemotePlayer(PlayFabPlayer remotePlayer)
	{
		this.m_delayedInitActions.Add(delegate
		{
			remotePlayer.IsMuted = true;
			ZLog.Log("Muted PlayFab remote player " + remotePlayer.EntityKey.Id);
		});
	}

	// Token: 0x06001124 RID: 4388 RVA: 0x0006F098 File Offset: 0x0006D298
	private void OnRemotePlayerSessionFound(PlayFabMatchmakingServerData serverData)
	{
		Action<PlayFabMatchmakingServerData> serverDataFoundCallback = this.m_serverDataFoundCallback;
		if (serverDataFoundCallback != null)
		{
			serverDataFoundCallback(serverData);
		}
		if (this.m_state == ZPlayFabSocketState.CLOSED)
		{
			return;
		}
		string networkId = PlayFabMultiplayerManager.Get().NetworkId;
		this.m_lobbyId = serverData.lobbyId;
		if (this.m_state == ZPlayFabSocketState.CONNECTING)
		{
			ZLog.Log(string.Concat(new string[]
			{
				"Joining server '",
				serverData.serverName,
				"' at PlayFab network ",
				serverData.networkId,
				" from lobby ",
				serverData.lobbyId
			}));
			PlayFabMultiplayerManager.Get().JoinNetwork(serverData.networkId);
			PlayFabMultiplayerManager.Get().OnNetworkJoined += this.OnNetworkJoined;
			return;
		}
		if (networkId == null || networkId != serverData.networkId || this.m_partyNetworkLeft)
		{
			ZLog.Log("Re-joining server '" + serverData.serverName + "' at new PlayFab network " + serverData.networkId);
			PlayFabMultiplayerManager.Get().JoinNetwork(serverData.networkId);
			this.m_partyNetworkLeft = false;
			return;
		}
		if (this.PartyResetInProgress())
		{
			ZLog.Log(string.Concat(new string[]
			{
				"Leave server '",
				serverData.serverName,
				"' at new PlayFab network ",
				serverData.networkId,
				", try to re-join later"
			}));
			this.ResetPartyTimeout();
			PlayFabMultiplayerManager.Get().LeaveNetwork();
			this.m_partyNetworkLeft = true;
		}
	}

	// Token: 0x06001125 RID: 4389 RVA: 0x0006F1F4 File Offset: 0x0006D3F4
	private void OnRemotePlayerNotFound(ZPLayFabMatchmakingFailReason failReason)
	{
		ZLog.LogWarning("Failed to locate network session for PlayFab player " + this.m_remotePlayerId);
		switch (failReason)
		{
		case ZPLayFabMatchmakingFailReason.InvalidServerData:
			ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorVersion);
			break;
		case ZPLayFabMatchmakingFailReason.ServerFull:
			ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorFull);
			break;
		case ZPLayFabMatchmakingFailReason.APIRequestLimitExceeded:
			this.ResetPartyTimeout();
			return;
		}
		this.Close();
	}

	// Token: 0x06001126 RID: 4390 RVA: 0x0006F250 File Offset: 0x0006D450
	private void CheckReestablishConnection(byte[] maybeCompressedBuffer)
	{
		try
		{
			this.OnDataMessageReceivedCont(this.m_zlibWorkQueue.UncompressOnThisThread(maybeCompressedBuffer));
			return;
		}
		catch
		{
		}
		byte msgType = this.GetMsgType(maybeCompressedBuffer);
		if (this.GetMsgId(maybeCompressedBuffer) == 0U && msgType == 64)
		{
			ZLog.Log("Assume restarted game session for remote ID " + this.GetEndPointString() + " and Platform ID " + this.GetHostName());
			this.ResetAll();
			this.OnDataMessageReceivedCont(maybeCompressedBuffer);
		}
	}

	// Token: 0x06001127 RID: 4391 RVA: 0x0006F2CC File Offset: 0x0006D4CC
	private void ResetAll()
	{
		this.m_recvQueue.Clear();
		this.m_outOfOrderQueue.Clear();
		this.m_sendQueue.Clear();
		this.m_inFlightQueue.ResetAll();
		this.m_retransmitCache.Clear();
		List<byte[]> list;
		List<byte[]> list2;
		this.m_zlibWorkQueue.Poll(out list, out list2);
		this.m_next = 0U;
		this.m_canKickstartIn = 0f;
		this.m_useCompression = false;
		this.m_didRecover = false;
		this.CancelResetParty();
	}

	// Token: 0x06001128 RID: 4392 RVA: 0x0006F348 File Offset: 0x0006D548
	private void OnDataMessageReceived(object sender, PlayFabPlayer from, byte[] compressedBuffer)
	{
		if (from.EntityKey.Id == this.m_remotePlayerId)
		{
			this.DelayedInit();
			if (this.m_useCompression)
			{
				if (!this.m_isClient && this.m_didRecover)
				{
					this.CheckReestablishConnection(compressedBuffer);
					return;
				}
				this.m_zlibWorkQueue.Decompress(compressedBuffer);
				return;
			}
			else
			{
				this.OnDataMessageReceivedCont(compressedBuffer);
			}
		}
	}

	// Token: 0x06001129 RID: 4393 RVA: 0x0006F3A8 File Offset: 0x0006D5A8
	private void OnDataMessageReceivedCont(byte[] buffer)
	{
		byte msgType = this.GetMsgType(buffer);
		uint msgId = this.GetMsgId(buffer);
		ZPlayFabSocket.s_lastReception = DateTime.UtcNow;
		base.IncRecvBytes(buffer.Length);
		if (msgType == 42)
		{
			this.ProcessAck(msgId);
			return;
		}
		if (this.m_next != msgId)
		{
			this.SendAck(this.m_next);
			if (msgId - this.m_next < 2147483647U && !this.m_outOfOrderQueue.ContainsKey(msgId))
			{
				this.m_outOfOrderQueue.Add(msgId, buffer);
			}
			return;
		}
		if (msgType != 17)
		{
			if (msgType != 64)
			{
				ZLog.LogError("Unknown message type " + msgType.ToString() + " received by socket!\nByte array:\n" + BitConverter.ToString(buffer));
				return;
			}
			this.InternalReceive(new ZPackage(buffer, buffer.Length - 5));
		}
		else
		{
			this.m_recvQueue.Enqueue(new ZPackage(buffer, buffer.Length - 5));
		}
		uint num = this.m_next + 1U;
		this.m_next = num;
		this.SendAck(num);
		if (this.m_outOfOrderQueue.Count != 0)
		{
			this.TryDeliverOutOfOrder();
		}
	}

	// Token: 0x0600112A RID: 4394 RVA: 0x0006F4A8 File Offset: 0x0006D6A8
	private void ProcessAck(uint msgId)
	{
		while (this.m_inFlightQueue.Tail != msgId)
		{
			if (this.m_inFlightQueue.IsEmpty)
			{
				this.Close();
				return;
			}
			this.m_inFlightQueue.Drop();
		}
	}

	// Token: 0x0600112B RID: 4395 RVA: 0x0006F4DC File Offset: 0x0006D6DC
	private void TryDeliverOutOfOrder()
	{
		byte[] buffer;
		while (this.m_outOfOrderQueue.TryGetValue(this.m_next, out buffer))
		{
			this.m_outOfOrderQueue.Remove(this.m_next);
			this.OnDataMessageReceivedCont(buffer);
		}
	}

	// Token: 0x0600112C RID: 4396 RVA: 0x0006F51C File Offset: 0x0006D71C
	private void InternalReceive(ZPackage pkg)
	{
		if (pkg.ReadByte() == 1)
		{
			this.m_platformPlayerId = pkg.ReadString();
			ZLog.Log("PlayFab socket with remote ID " + this.GetEndPointString() + " received local Platform ID " + this.GetHostName());
			return;
		}
		ZLog.LogError("Unknown data in internal receive! Ignoring");
	}

	// Token: 0x0600112D RID: 4397 RVA: 0x0006F569 File Offset: 0x0006D769
	private void SendAck(uint nextMsgId)
	{
		ZPlayFabSocket.SetMsgType(this.m_sndMsg, 42);
		ZPlayFabSocket.SetMsgId(this.m_sndMsg, nextMsgId);
		this.InternalSend(this.m_sndMsg);
	}

	// Token: 0x0600112E RID: 4398 RVA: 0x0006F590 File Offset: 0x0006D790
	private static void SetMsgType(byte[] payload, byte t)
	{
		payload[4] = t;
	}

	// Token: 0x0600112F RID: 4399 RVA: 0x0006F596 File Offset: 0x0006D796
	private static void SetMsgId(byte[] payload, uint id)
	{
		payload[0] = (byte)id;
		payload[1] = (byte)(id >> 8);
		payload[2] = (byte)(id >> 16);
		payload[3] = (byte)(id >> 24);
	}

	// Token: 0x06001130 RID: 4400 RVA: 0x0006F5B4 File Offset: 0x0006D7B4
	private uint GetMsgId(byte[] buffer)
	{
		uint num = 0U;
		int num2 = buffer.Length - 5;
		return num + (uint)buffer[num2] + (uint)((uint)buffer[num2 + 1] << 8) + (uint)((uint)buffer[num2 + 2] << 16) + (uint)((uint)buffer[num2 + 3] << 24);
	}

	// Token: 0x06001131 RID: 4401 RVA: 0x0006F5E6 File Offset: 0x0006D7E6
	private byte GetMsgType(byte[] buffer)
	{
		return buffer[buffer.Length - 1];
	}

	// Token: 0x06001132 RID: 4402 RVA: 0x0006F5F0 File Offset: 0x0006D7F0
	private void DelayedInit()
	{
		if (this.m_delayedInitActions.Count == 0)
		{
			return;
		}
		foreach (Action action in this.m_delayedInitActions)
		{
			action();
		}
		this.m_delayedInitActions.Clear();
	}

	// Token: 0x06001133 RID: 4403 RVA: 0x0006F65C File Offset: 0x0006D85C
	private void OnNetworkJoined(object sender, string networkId)
	{
		ZLog.Log("PlayFab client socket to remote player " + this.m_remotePlayerId + " joined network " + networkId);
		if (this.m_isClient && this.m_state == ZPlayFabSocketState.CONNECTED)
		{
			this.ClientConnect();
		}
		ZRpc.SetLongTimeout(true);
	}

	// Token: 0x06001134 RID: 4404 RVA: 0x0006F696 File Offset: 0x0006D896
	private void OnRemotePlayerJoined(object sender, PlayFabPlayer player)
	{
		this.InitRemotePlayer(player);
		if (player.EntityKey.Id == this.m_remotePlayerId)
		{
			ZLog.Log("PlayFab socket connected to remote player " + this.m_remotePlayerId);
			this.Connect(player);
		}
	}

	// Token: 0x06001135 RID: 4405 RVA: 0x0006F6D4 File Offset: 0x0006D8D4
	private void Connect(PlayFabPlayer remotePlayer)
	{
		string id = remotePlayer.EntityKey.Id;
		if (!ZPlayFabSocket.s_connectSockets.ContainsKey(id))
		{
			ZPlayFabSocket.s_connectSockets.Add(id, this);
			ZPlayFabSocket.s_lastReception = DateTime.UtcNow;
		}
		if (this.m_state == ZPlayFabSocketState.CONNECTED)
		{
			ZLog.Log("Resume TX on " + this.GetEndPointString());
		}
		this.m_peer = new PlayFabPlayer[]
		{
			remotePlayer
		};
		this.m_state = ZPlayFabSocketState.CONNECTED;
		this.CancelResetParty();
		if (this.m_sendQueue.Count > 0)
		{
			this.m_inFlightQueue.ResetRetransTimer(false);
			while (this.m_sendQueue.Count > 0)
			{
				this.InternalSend(this.m_sendQueue.Dequeue());
			}
			return;
		}
		this.KickstartAfterRecovery();
	}

	// Token: 0x06001136 RID: 4406 RVA: 0x0006F78D File Offset: 0x0006D98D
	private bool PartyResetInProgress()
	{
		return this.m_partyResetTimeout > 0f;
	}

	// Token: 0x06001137 RID: 4407 RVA: 0x0006F79C File Offset: 0x0006D99C
	private void CancelResetParty()
	{
		this.m_didRecover = this.PartyResetInProgress();
		this.m_partyNetworkLeft = false;
		this.m_partyResetTimeout = 0f;
		this.m_partyResetConnectTimeout = 0f;
		ZPlayFabSocket.s_durationToPartyReset = 0f;
	}

	// Token: 0x06001138 RID: 4408 RVA: 0x0006F7D4 File Offset: 0x0006D9D4
	private void InternalSend(byte[] payload)
	{
		if (!this.PartyResetInProgress())
		{
			base.IncSentBytes(payload.Length);
			if (this.m_useCompression)
			{
				if (ZNet.instance != null && ZNet.instance.HaveStopped)
				{
					this.InternalSendCont(this.m_zlibWorkQueue.CompressOnThisThread(payload));
					return;
				}
				this.m_zlibWorkQueue.Compress(payload);
				return;
			}
			else
			{
				this.InternalSendCont(payload);
			}
		}
	}

	// Token: 0x06001139 RID: 4409 RVA: 0x0006F83C File Offset: 0x0006DA3C
	private void InternalSendCont(byte[] compressedPayload)
	{
		if (!this.PartyResetInProgress())
		{
			if (PlayFabMultiplayerManager.Get().SendDataMessage(compressedPayload, this.m_peer, DeliveryOption.Guaranteed))
			{
				if (!this.m_isClient)
				{
					ZPlayFabMatchmaking.ForwardProgress();
					return;
				}
			}
			else
			{
				if (this.m_isClient)
				{
					ZPlayFabSocket.ScheduleResetParty();
				}
				this.ResetPartyTimeout();
				ZLog.Log("Failed to send, suspend TX on " + this.GetEndPointString() + " while trying to reconnect");
			}
		}
	}

	// Token: 0x0600113A RID: 4410 RVA: 0x0006F8A0 File Offset: 0x0006DAA0
	private void ResetPartyTimeout()
	{
		this.m_partyResetConnectTimeout = UnityEngine.Random.Range(9f, 11f) + ZPlayFabSocket.s_durationToPartyReset;
		this.m_partyResetTimeout = UnityEngine.Random.Range(18f, 22f) + ZPlayFabSocket.s_durationToPartyReset;
	}

	// Token: 0x0600113B RID: 4411 RVA: 0x0006F8D8 File Offset: 0x0006DAD8
	internal static void ScheduleResetParty()
	{
		if (ZPlayFabSocket.s_durationToPartyReset <= 0f)
		{
			ZPlayFabSocket.s_durationToPartyReset = UnityEngine.Random.Range(2.6999998f, 3.3000002f);
		}
	}

	// Token: 0x0600113C RID: 4412 RVA: 0x0006F8FC File Offset: 0x0006DAFC
	public void Dispose()
	{
		this.m_zlibWorkQueue.Dispose();
		this.ResetAll();
		if (this.m_state == ZPlayFabSocketState.CLOSED)
		{
			return;
		}
		if (this.m_state == ZPlayFabSocketState.LISTEN)
		{
			ZPlayFabSocket.s_listenSocket = null;
			using (Queue<ZPlayFabSocket>.Enumerator enumerator = this.m_backlog.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZPlayFabSocket zplayFabSocket = enumerator.Current;
					zplayFabSocket.Close();
				}
				goto IL_72;
			}
		}
		PlayFabMultiplayerManager.Get().OnDataMessageReceived -= this.OnDataMessageReceived;
		IL_72:
		if (!ZNet.instance.IsServer())
		{
			PlayFabMultiplayerManager.Get().OnRemotePlayerJoined -= this.OnRemotePlayerJoined;
			PlayFabMultiplayerManager.Get().OnNetworkJoined -= this.OnNetworkJoined;
			PlayFabMultiplayerManager.Get().LeaveNetwork();
		}
		if (this.m_state == ZPlayFabSocketState.CONNECTED)
		{
			ZPlayFabSocket.s_connectSockets.Remove(this.m_peer[0].EntityKey.Id);
		}
		if (this.m_lobbyId != null)
		{
			ZPlayFabMatchmaking.LeaveLobby(this.m_lobbyId);
		}
		else
		{
			ZPlayFabMatchmaking.LeaveEmptyLobby();
		}
		this.m_state = ZPlayFabSocketState.CLOSED;
	}

	// Token: 0x0600113D RID: 4413 RVA: 0x0006FA14 File Offset: 0x0006DC14
	private void Update(float dt)
	{
		if (this.m_canKickstartIn >= 0f)
		{
			this.m_canKickstartIn -= dt;
		}
		if (!this.m_isClient)
		{
			return;
		}
		if (this.PartyResetInProgress())
		{
			this.m_partyResetTimeout -= dt;
			if (this.m_partyResetConnectTimeout > 0f)
			{
				this.m_partyResetConnectTimeout -= dt;
				if (this.m_partyResetConnectTimeout <= 0f)
				{
					this.ClientConnect();
					return;
				}
			}
		}
		else if ((DateTime.UtcNow - ZPlayFabSocket.s_lastReception).TotalSeconds >= 26.0 && this.m_state == ZPlayFabSocketState.CONNECTED)
		{
			ZLog.Log("Do a reset party as nothing seems to be received");
			this.ResetPartyTimeout();
			PlayFabMultiplayerManager.Get().ResetParty();
		}
	}

	// Token: 0x0600113E RID: 4414 RVA: 0x0006FAD0 File Offset: 0x0006DCD0
	private void LateUpdate()
	{
		List<byte[]> list;
		List<byte[]> list2;
		this.m_zlibWorkQueue.Poll(out list, out list2);
		if (list != null)
		{
			foreach (byte[] compressedPayload in list)
			{
				this.InternalSendCont(compressedPayload);
			}
		}
		if (list2 != null)
		{
			foreach (byte[] buffer in list2)
			{
				this.OnDataMessageReceivedCont(buffer);
			}
		}
	}

	// Token: 0x0600113F RID: 4415 RVA: 0x0006FB74 File Offset: 0x0006DD74
	public bool IsConnected()
	{
		return this.m_state == ZPlayFabSocketState.CONNECTED || this.m_state == ZPlayFabSocketState.CONNECTING;
	}

	// Token: 0x06001140 RID: 4416 RVA: 0x0006FB8A File Offset: 0x0006DD8A
	public void VersionMatch()
	{
		this.m_useCompression = true;
	}

	// Token: 0x06001141 RID: 4417 RVA: 0x0006FB94 File Offset: 0x0006DD94
	public void Send(ZPackage pkg, byte messageType)
	{
		if (pkg.Size() == 0 || !this.IsConnected())
		{
			return;
		}
		pkg.Write(this.m_inFlightQueue.Head);
		pkg.Write(messageType);
		byte[] array = pkg.GetArray();
		this.m_inFlightQueue.Enqueue(array);
		if (this.m_state == ZPlayFabSocketState.CONNECTED)
		{
			this.InternalSend(array);
			return;
		}
		this.m_sendQueue.Enqueue(array);
	}

	// Token: 0x06001142 RID: 4418 RVA: 0x0006FBFA File Offset: 0x0006DDFA
	public void Send(ZPackage pkg)
	{
		this.Send(pkg, 17);
	}

	// Token: 0x06001143 RID: 4419 RVA: 0x0006FC05 File Offset: 0x0006DE05
	public ZPackage Recv()
	{
		this.CheckRetransmit();
		if (!this.GotNewData())
		{
			return null;
		}
		return this.m_recvQueue.Dequeue();
	}

	// Token: 0x06001144 RID: 4420 RVA: 0x0006FC22 File Offset: 0x0006DE22
	private void CheckRetransmit()
	{
		if (this.m_inFlightQueue.IsEmpty || this.PartyResetInProgress() || this.m_state != ZPlayFabSocketState.CONNECTED)
		{
			return;
		}
		if (Time.time < this.m_inFlightQueue.NextResend)
		{
			return;
		}
		this.DoRetransmit(true);
	}

	// Token: 0x06001145 RID: 4421 RVA: 0x0006FC5D File Offset: 0x0006DE5D
	private void DoRetransmit(bool canKickstart = true)
	{
		if (canKickstart && this.CanKickstartRatelimit())
		{
			this.KickstartAfterRecovery();
			return;
		}
		if (!this.m_inFlightQueue.IsEmpty)
		{
			this.InternalSend(this.m_inFlightQueue.Peek());
			this.m_inFlightQueue.ResetRetransTimer(true);
		}
	}

	// Token: 0x06001146 RID: 4422 RVA: 0x0006FC9B File Offset: 0x0006DE9B
	private bool CanKickstartRatelimit()
	{
		return this.m_canKickstartIn <= 0f;
	}

	// Token: 0x06001147 RID: 4423 RVA: 0x0006FCB0 File Offset: 0x0006DEB0
	private void KickstartAfterRecovery()
	{
		try
		{
			this.TryKickstartAfterRecovery();
		}
		catch (Exception ex)
		{
			ZLog.LogWarning("Failed to resend data on $" + this.GetEndPointString() + ", closing socket: " + ex.Message);
			this.Close();
		}
	}

	// Token: 0x06001148 RID: 4424 RVA: 0x0006FD00 File Offset: 0x0006DF00
	private void TryKickstartAfterRecovery()
	{
		if (!this.m_inFlightQueue.IsEmpty)
		{
			this.m_inFlightQueue.CopyPayloads(this.m_retransmitCache);
			foreach (byte[] payload in this.m_retransmitCache)
			{
				this.InternalSend(payload);
			}
			this.m_retransmitCache.Clear();
			this.m_inFlightQueue.ResetRetransTimer(false);
		}
		this.m_canKickstartIn = 6f;
	}

	// Token: 0x06001149 RID: 4425 RVA: 0x0006FD94 File Offset: 0x0006DF94
	public int GetSendQueueSize()
	{
		return (int)(this.m_inFlightQueue.Bytes * 0.25f);
	}

	// Token: 0x0600114A RID: 4426 RVA: 0x0006FDAA File Offset: 0x0006DFAA
	public int GetCurrentSendRate()
	{
		throw new NotImplementedException();
	}

	// Token: 0x0600114B RID: 4427 RVA: 0x0006FDB1 File Offset: 0x0006DFB1
	internal void StartHost()
	{
		if (ZPlayFabSocket.s_listenSocket != null)
		{
			ZLog.LogError("Multiple PlayFab listen sockets");
			return;
		}
		ZPlayFabSocket.s_listenSocket = this;
	}

	// Token: 0x0600114C RID: 4428 RVA: 0x0006FDCB File Offset: 0x0006DFCB
	public bool IsHost()
	{
		return this.m_state == ZPlayFabSocketState.LISTEN;
	}

	// Token: 0x0600114D RID: 4429 RVA: 0x0006FDD6 File Offset: 0x0006DFD6
	public bool GotNewData()
	{
		return this.m_recvQueue.Count > 0;
	}

	// Token: 0x0600114E RID: 4430 RVA: 0x0006FDE8 File Offset: 0x0006DFE8
	public string GetEndPointString()
	{
		string str = "";
		if (this.m_peer != null)
		{
			str = this.m_peer[0].EntityKey.Id;
		}
		return "playfab/" + str;
	}

	// Token: 0x0600114F RID: 4431 RVA: 0x0006FE21 File Offset: 0x0006E021
	public ISocket Accept()
	{
		if (this.m_backlog.Count == 0)
		{
			return null;
		}
		ZRpc.SetLongTimeout(true);
		return this.m_backlog.Dequeue();
	}

	// Token: 0x06001150 RID: 4432 RVA: 0x0006FE43 File Offset: 0x0006E043
	public int GetHostPort()
	{
		if (!this.IsHost())
		{
			return -1;
		}
		return 0;
	}

	// Token: 0x06001151 RID: 4433 RVA: 0x0006FDAA File Offset: 0x0006DFAA
	public bool Flush()
	{
		throw new NotImplementedException();
	}

	// Token: 0x06001152 RID: 4434 RVA: 0x0006FE50 File Offset: 0x0006E050
	public string GetHostName()
	{
		return this.m_platformPlayerId;
	}

	// Token: 0x06001153 RID: 4435 RVA: 0x0006FE58 File Offset: 0x0006E058
	public void Close()
	{
		this.Dispose();
	}

	// Token: 0x06001154 RID: 4436 RVA: 0x0006FE60 File Offset: 0x0006E060
	internal static void LostConnection(PlayFabPlayer player)
	{
		string id = player.EntityKey.Id;
		ZPlayFabSocket zplayFabSocket;
		if (ZPlayFabSocket.s_connectSockets.TryGetValue(id, out zplayFabSocket))
		{
			ZLog.Log("Keep socket for " + zplayFabSocket.GetEndPointString() + ", try to reconnect before timeout");
		}
	}

	// Token: 0x06001155 RID: 4437 RVA: 0x0006FEA4 File Offset: 0x0006E0A4
	internal static void QueueConnection(PlayFabPlayer player)
	{
		string id = player.EntityKey.Id;
		ZPlayFabSocket zplayFabSocket;
		if (ZPlayFabSocket.s_connectSockets.TryGetValue(id, out zplayFabSocket))
		{
			ZLog.Log("Resume TX on " + zplayFabSocket.GetEndPointString());
			zplayFabSocket.Connect(player);
			return;
		}
		if (ZPlayFabSocket.s_listenSocket != null)
		{
			ZPlayFabSocket.s_listenSocket.m_backlog.Enqueue(new ZPlayFabSocket(player));
			return;
		}
		ZLog.LogError("Incoming PlayFab connection without any open listen socket");
	}

	// Token: 0x06001156 RID: 4438 RVA: 0x0006FF10 File Offset: 0x0006E110
	internal static void DestroyListenSocket()
	{
		while (ZPlayFabSocket.s_connectSockets.Count > 0)
		{
			Dictionary<string, ZPlayFabSocket>.Enumerator enumerator = ZPlayFabSocket.s_connectSockets.GetEnumerator();
			enumerator.MoveNext();
			KeyValuePair<string, ZPlayFabSocket> keyValuePair = enumerator.Current;
			keyValuePair.Value.Close();
		}
		ZPlayFabSocket.s_listenSocket.Close();
		ZPlayFabSocket.s_listenSocket = null;
	}

	// Token: 0x06001157 RID: 4439 RVA: 0x0006FF63 File Offset: 0x0006E163
	internal static uint NumSockets()
	{
		return (uint)ZPlayFabSocket.s_connectSockets.Count;
	}

	// Token: 0x06001158 RID: 4440 RVA: 0x0006FF70 File Offset: 0x0006E170
	internal static void UpdateAllSockets(float dt)
	{
		if (ZPlayFabSocket.s_durationToPartyReset > 0f)
		{
			ZPlayFabSocket.s_durationToPartyReset -= dt;
			if (ZPlayFabSocket.s_durationToPartyReset < 0f)
			{
				ZLog.Log("Reset party to clear network error");
				PlayFabMultiplayerManager.Get().ResetParty();
			}
		}
		foreach (ZPlayFabSocket zplayFabSocket in ZPlayFabSocket.s_connectSockets.Values)
		{
			zplayFabSocket.Update(dt);
		}
	}

	// Token: 0x06001159 RID: 4441 RVA: 0x00070000 File Offset: 0x0006E200
	internal static void LateUpdateAllSocket()
	{
		foreach (ZPlayFabSocket zplayFabSocket in ZPlayFabSocket.s_connectSockets.Values)
		{
			zplayFabSocket.LateUpdate();
		}
	}

	// Token: 0x040011DC RID: 4572
	private const byte PAYLOAD_DAT = 17;

	// Token: 0x040011DD RID: 4573
	private const byte PAYLOAD_ACK = 42;

	// Token: 0x040011DE RID: 4574
	private const byte PAYLOAD_INT = 64;

	// Token: 0x040011DF RID: 4575
	private const int PAYLOAD_HEADER_LEN = 5;

	// Token: 0x040011E0 RID: 4576
	private const float PARTY_RESET_GRACE_SEC = 3f;

	// Token: 0x040011E1 RID: 4577
	private const float PARTY_RESET_TIMEOUT_SEC = 20f;

	// Token: 0x040011E2 RID: 4578
	private const float KICKSTART_COOLDOWN = 6f;

	// Token: 0x040011E3 RID: 4579
	private const float NETWORK_ERROR_WATCHDOG = 26f;

	// Token: 0x040011E4 RID: 4580
	private const float INFLIGHT_SCALING_FACTOR = 0.25f;

	// Token: 0x040011E5 RID: 4581
	private const byte INT_PLATFORM_ID = 1;

	// Token: 0x040011E6 RID: 4582
	private static ZPlayFabSocket s_listenSocket;

	// Token: 0x040011E7 RID: 4583
	private static readonly Dictionary<string, ZPlayFabSocket> s_connectSockets = new Dictionary<string, ZPlayFabSocket>();

	// Token: 0x040011E8 RID: 4584
	private static float s_durationToPartyReset;

	// Token: 0x040011E9 RID: 4585
	private static DateTime s_lastReception;

	// Token: 0x040011EA RID: 4586
	private ZPlayFabSocketState m_state;

	// Token: 0x040011EB RID: 4587
	private PlayFabPlayer[] m_peer;

	// Token: 0x040011EC RID: 4588
	private string m_lobbyId;

	// Token: 0x040011ED RID: 4589
	private readonly byte[] m_sndMsg = new byte[5];

	// Token: 0x040011EE RID: 4590
	private readonly bool m_isClient;

	// Token: 0x040011EF RID: 4591
	private readonly string m_remotePlayerId;

	// Token: 0x040011F0 RID: 4592
	private string m_platformPlayerId;

	// Token: 0x040011F1 RID: 4593
	private readonly Queue<ZPackage> m_recvQueue = new Queue<ZPackage>();

	// Token: 0x040011F2 RID: 4594
	private readonly Dictionary<uint, byte[]> m_outOfOrderQueue = new Dictionary<uint, byte[]>();

	// Token: 0x040011F3 RID: 4595
	private readonly Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x040011F4 RID: 4596
	private readonly ZPlayFabSocket.InFlightQueue m_inFlightQueue = new ZPlayFabSocket.InFlightQueue();

	// Token: 0x040011F5 RID: 4597
	private readonly List<byte[]> m_retransmitCache = new List<byte[]>();

	// Token: 0x040011F6 RID: 4598
	private readonly List<Action> m_delayedInitActions = new List<Action>();

	// Token: 0x040011F7 RID: 4599
	private readonly PlayFabZLibWorkQueue m_zlibWorkQueue = new PlayFabZLibWorkQueue();

	// Token: 0x040011F8 RID: 4600
	private readonly Queue<ZPlayFabSocket> m_backlog = new Queue<ZPlayFabSocket>();

	// Token: 0x040011F9 RID: 4601
	private uint m_next;

	// Token: 0x040011FA RID: 4602
	private float m_partyResetTimeout;

	// Token: 0x040011FB RID: 4603
	private float m_partyResetConnectTimeout;

	// Token: 0x040011FC RID: 4604
	private bool m_partyNetworkLeft;

	// Token: 0x040011FD RID: 4605
	private bool m_didRecover;

	// Token: 0x040011FE RID: 4606
	private float m_canKickstartIn;

	// Token: 0x040011FF RID: 4607
	private bool m_useCompression;

	// Token: 0x04001200 RID: 4608
	private Action<PlayFabMatchmakingServerData> m_serverDataFoundCallback;

	// Token: 0x020001AF RID: 431
	public class InFlightQueue
	{
		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x0600115B RID: 4443 RVA: 0x00070060 File Offset: 0x0006E260
		public uint Bytes
		{
			get
			{
				return this.m_size;
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x0600115C RID: 4444 RVA: 0x00070068 File Offset: 0x0006E268
		public uint Head
		{
			get
			{
				return this.m_head;
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x0600115D RID: 4445 RVA: 0x00070070 File Offset: 0x0006E270
		public uint Tail
		{
			get
			{
				return this.m_tail;
			}
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600115E RID: 4446 RVA: 0x00070078 File Offset: 0x0006E278
		public bool IsEmpty
		{
			get
			{
				return this.m_payloads.Count == 0;
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x0600115F RID: 4447 RVA: 0x00070088 File Offset: 0x0006E288
		public float NextResend
		{
			get
			{
				return this.m_nextResend;
			}
		}

		// Token: 0x06001160 RID: 4448 RVA: 0x00070090 File Offset: 0x0006E290
		public void Enqueue(byte[] payload)
		{
			this.m_payloads.Enqueue(payload);
			this.m_size += (uint)payload.Length;
			this.m_head += 1U;
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x000700BC File Offset: 0x0006E2BC
		public void Drop()
		{
			this.m_size -= (uint)this.m_payloads.Dequeue().Length;
			this.m_tail += 1U;
			this.ResetRetransTimer(false);
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x000700ED File Offset: 0x0006E2ED
		public byte[] Peek()
		{
			return this.m_payloads.Peek();
		}

		// Token: 0x06001163 RID: 4451 RVA: 0x000700FC File Offset: 0x0006E2FC
		public void CopyPayloads(List<byte[]> payloads)
		{
			while (this.m_payloads.Count > 0)
			{
				payloads.Add(this.m_payloads.Dequeue());
			}
			foreach (byte[] item in payloads)
			{
				this.m_payloads.Enqueue(item);
			}
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x00070170 File Offset: 0x0006E370
		public void ResetRetransTimer(bool small = false)
		{
			this.m_nextResend = Time.time + (small ? 1f : 3f);
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x0007018D File Offset: 0x0006E38D
		public void ResetAll()
		{
			this.m_payloads.Clear();
			this.m_nextResend = 0f;
			this.m_size = 0U;
			this.m_head = 0U;
			this.m_tail = 0U;
		}

		// Token: 0x04001201 RID: 4609
		private readonly Queue<byte[]> m_payloads = new Queue<byte[]>();

		// Token: 0x04001202 RID: 4610
		private float m_nextResend;

		// Token: 0x04001203 RID: 4611
		private uint m_size;

		// Token: 0x04001204 RID: 4612
		private uint m_head;

		// Token: 0x04001205 RID: 4613
		private uint m_tail;
	}
}
