using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Steamworks;
using UnityEngine;

// Token: 0x0200018B RID: 395
public class ZSteamSocket : IDisposable, ISocket
{
	// Token: 0x06000FDA RID: 4058 RVA: 0x000695E0 File Offset: 0x000677E0
	public ZSteamSocket()
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x06000FDB RID: 4059 RVA: 0x0006963C File Offset: 0x0006783C
	public ZSteamSocket(SteamNetworkingIPAddr host)
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		string str;
		host.ToString(out str, true);
		ZLog.Log("Starting to connect to " + str);
		this.m_con = SteamNetworkingSockets.ConnectByIPAddress(ref host, 0, null);
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x06000FDC RID: 4060 RVA: 0x000696C0 File Offset: 0x000678C0
	public ZSteamSocket(CSteamID peerID)
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		this.m_peerID.SetSteamID(peerID);
		this.m_con = SteamNetworkingSockets.ConnectP2P(ref this.m_peerID, 0, 0, null);
		ZLog.Log("Connecting to " + this.m_peerID.GetSteamID().ToString());
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x06000FDD RID: 4061 RVA: 0x00069764 File Offset: 0x00067964
	public ZSteamSocket(HSteamNetConnection con)
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		this.m_con = con;
		SteamNetConnectionInfo_t steamNetConnectionInfo_t;
		SteamNetworkingSockets.GetConnectionInfo(this.m_con, out steamNetConnectionInfo_t);
		this.m_peerID = steamNetConnectionInfo_t.m_identityRemote;
		ZLog.Log("Connecting to " + this.m_peerID.ToString());
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x06000FDE RID: 4062 RVA: 0x00069800 File Offset: 0x00067A00
	private static void RegisterGlobalCallbacks()
	{
		if (ZSteamSocket.m_statusChanged == null)
		{
			ZSteamSocket.m_statusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(new Callback<SteamNetConnectionStatusChangedCallback_t>.DispatchDelegate(ZSteamSocket.OnStatusChanged));
			GCHandle gchandle = GCHandle.Alloc(30000f, GCHandleType.Pinned);
			GCHandle gchandle2 = GCHandle.Alloc(1, GCHandleType.Pinned);
			GCHandle gchandle3 = GCHandle.Alloc(153600, GCHandleType.Pinned);
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float, gchandle.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, gchandle2.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendRateMin, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, gchandle3.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendRateMax, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, gchandle3.AddrOfPinnedObject());
			gchandle.Free();
			gchandle2.Free();
			gchandle3.Free();
		}
	}

	// Token: 0x06000FDF RID: 4063 RVA: 0x000698CC File Offset: 0x00067ACC
	private static void UnregisterGlobalCallbacks()
	{
		ZLog.Log("ZSteamSocket  UnregisterGlobalCallbacks, existing sockets:" + ZSteamSocket.m_sockets.Count.ToString());
		if (ZSteamSocket.m_statusChanged != null)
		{
			ZSteamSocket.m_statusChanged.Dispose();
			ZSteamSocket.m_statusChanged = null;
		}
	}

	// Token: 0x06000FE0 RID: 4064 RVA: 0x00069914 File Offset: 0x00067B14
	private static void OnStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
	{
		ZLog.Log("Got status changed msg " + data.m_info.m_eState.ToString());
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected && data.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
		{
			ZLog.Log("Connected");
			ZSteamSocket zsteamSocket = ZSteamSocket.FindSocket(data.m_hConn);
			if (zsteamSocket != null)
			{
				SteamNetConnectionInfo_t steamNetConnectionInfo_t;
				if (SteamNetworkingSockets.GetConnectionInfo(data.m_hConn, out steamNetConnectionInfo_t))
				{
					zsteamSocket.m_peerID = steamNetConnectionInfo_t.m_identityRemote;
				}
				ZLog.Log("Got connection SteamID " + zsteamSocket.m_peerID.GetSteamID().ToString());
			}
		}
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting && data.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None)
		{
			ZLog.Log("New connection");
			ZSteamSocket listner = ZSteamSocket.GetListner();
			if (listner != null)
			{
				listner.OnNewConnection(data.m_hConn);
			}
		}
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
		{
			ZLog.Log("Got problem " + data.m_info.m_eEndReason.ToString() + ":" + data.m_info.m_szEndDebug);
			ZSteamSocket zsteamSocket2 = ZSteamSocket.FindSocket(data.m_hConn);
			if (zsteamSocket2 != null)
			{
				ZLog.Log("  Closing socket " + zsteamSocket2.GetHostName());
				zsteamSocket2.Close();
			}
		}
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer)
		{
			ZLog.Log("Socket closed by peer " + data.ToString());
			ZSteamSocket zsteamSocket3 = ZSteamSocket.FindSocket(data.m_hConn);
			if (zsteamSocket3 != null)
			{
				ZLog.Log("  Closing socket " + zsteamSocket3.GetHostName());
				zsteamSocket3.Close();
			}
		}
	}

	// Token: 0x06000FE1 RID: 4065 RVA: 0x00069AB4 File Offset: 0x00067CB4
	private static ZSteamSocket FindSocket(HSteamNetConnection con)
	{
		foreach (ZSteamSocket zsteamSocket in ZSteamSocket.m_sockets)
		{
			if (zsteamSocket.m_con == con)
			{
				return zsteamSocket;
			}
		}
		return null;
	}

	// Token: 0x06000FE2 RID: 4066 RVA: 0x00069B14 File Offset: 0x00067D14
	public void Dispose()
	{
		ZLog.Log("Disposing socket");
		this.Close();
		this.m_pkgQueue.Clear();
		ZSteamSocket.m_sockets.Remove(this);
		if (ZSteamSocket.m_sockets.Count == 0)
		{
			ZLog.Log("Last socket, unregistering callback");
			ZSteamSocket.UnregisterGlobalCallbacks();
		}
	}

	// Token: 0x06000FE3 RID: 4067 RVA: 0x00069B64 File Offset: 0x00067D64
	public void Close()
	{
		if (this.m_con != HSteamNetConnection.Invalid)
		{
			ZLog.Log("Closing socket " + this.GetEndPointString());
			this.Flush();
			ZLog.Log("  send queue size:" + this.m_sendQueue.Count.ToString());
			Thread.Sleep(100);
			CSteamID steamID = this.m_peerID.GetSteamID();
			SteamNetworkingSockets.CloseConnection(this.m_con, 0, "", false);
			SteamUser.EndAuthSession(steamID);
			this.m_con = HSteamNetConnection.Invalid;
		}
		if (this.m_listenSocket != HSteamListenSocket.Invalid)
		{
			ZLog.Log("Stopping listening socket");
			SteamNetworkingSockets.CloseListenSocket(this.m_listenSocket);
			this.m_listenSocket = HSteamListenSocket.Invalid;
		}
		if (ZSteamSocket.m_hostSocket == this)
		{
			ZSteamSocket.m_hostSocket = null;
		}
		this.m_peerID.Clear();
	}

	// Token: 0x06000FE4 RID: 4068 RVA: 0x00069C42 File Offset: 0x00067E42
	public bool StartHost()
	{
		if (ZSteamSocket.m_hostSocket != null)
		{
			ZLog.Log("Listen socket already started");
			return false;
		}
		this.m_listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
		ZSteamSocket.m_hostSocket = this;
		this.m_pendingConnections.Clear();
		return true;
	}

	// Token: 0x06000FE5 RID: 4069 RVA: 0x00069C78 File Offset: 0x00067E78
	private void OnNewConnection(HSteamNetConnection con)
	{
		EResult eresult = SteamNetworkingSockets.AcceptConnection(con);
		ZLog.Log("Accepting connection " + eresult.ToString());
		if (eresult == EResult.k_EResultOK)
		{
			this.QueuePendingConnection(con);
		}
	}

	// Token: 0x06000FE6 RID: 4070 RVA: 0x00069CB4 File Offset: 0x00067EB4
	private void QueuePendingConnection(HSteamNetConnection con)
	{
		ZSteamSocket item = new ZSteamSocket(con);
		this.m_pendingConnections.Enqueue(item);
	}

	// Token: 0x06000FE7 RID: 4071 RVA: 0x00069CD4 File Offset: 0x00067ED4
	public ISocket Accept()
	{
		if (this.m_listenSocket == HSteamListenSocket.Invalid)
		{
			return null;
		}
		if (this.m_pendingConnections.Count > 0)
		{
			return this.m_pendingConnections.Dequeue();
		}
		return null;
	}

	// Token: 0x06000FE8 RID: 4072 RVA: 0x00069D05 File Offset: 0x00067F05
	public bool IsConnected()
	{
		return this.m_con != HSteamNetConnection.Invalid;
	}

	// Token: 0x06000FE9 RID: 4073 RVA: 0x00069D18 File Offset: 0x00067F18
	public void Send(ZPackage pkg)
	{
		if (pkg.Size() == 0)
		{
			return;
		}
		if (!this.IsConnected())
		{
			return;
		}
		byte[] array = pkg.GetArray();
		this.m_sendQueue.Enqueue(array);
		this.SendQueuedPackages();
	}

	// Token: 0x06000FEA RID: 4074 RVA: 0x00069D50 File Offset: 0x00067F50
	public bool Flush()
	{
		this.SendQueuedPackages();
		HSteamNetConnection con = this.m_con;
		SteamNetworkingSockets.FlushMessagesOnConnection(this.m_con);
		return this.m_sendQueue.Count == 0;
	}

	// Token: 0x06000FEB RID: 4075 RVA: 0x00069D7C File Offset: 0x00067F7C
	private void SendQueuedPackages()
	{
		if (!this.IsConnected())
		{
			return;
		}
		while (this.m_sendQueue.Count > 0)
		{
			byte[] array = this.m_sendQueue.Peek();
			IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
			Marshal.Copy(array, 0, intPtr, array.Length);
			long num;
			EResult eresult = SteamNetworkingSockets.SendMessageToConnection(this.m_con, intPtr, (uint)array.Length, 8, out num);
			Marshal.FreeHGlobal(intPtr);
			if (eresult != EResult.k_EResultOK)
			{
				ZLog.Log("Failed to send data " + eresult.ToString());
				return;
			}
			this.m_totalSent += array.Length;
			this.m_sendQueue.Dequeue();
		}
	}

	// Token: 0x06000FEC RID: 4076 RVA: 0x00069E1C File Offset: 0x0006801C
	public static void UpdateAllSockets(float dt)
	{
		foreach (ZSteamSocket zsteamSocket in ZSteamSocket.m_sockets)
		{
			zsteamSocket.Update(dt);
		}
	}

	// Token: 0x06000FED RID: 4077 RVA: 0x00069E6C File Offset: 0x0006806C
	private void Update(float dt)
	{
		this.SendQueuedPackages();
	}

	// Token: 0x06000FEE RID: 4078 RVA: 0x00069E74 File Offset: 0x00068074
	private static ZSteamSocket GetListner()
	{
		return ZSteamSocket.m_hostSocket;
	}

	// Token: 0x06000FEF RID: 4079 RVA: 0x00069E7C File Offset: 0x0006807C
	public ZPackage Recv()
	{
		if (!this.IsConnected())
		{
			return null;
		}
		IntPtr[] array = new IntPtr[1];
		if (SteamNetworkingSockets.ReceiveMessagesOnConnection(this.m_con, array, 1) == 1)
		{
			SteamNetworkingMessage_t steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(array[0]);
			byte[] array2 = new byte[steamNetworkingMessage_t.m_cbSize];
			Marshal.Copy(steamNetworkingMessage_t.m_pData, array2, 0, steamNetworkingMessage_t.m_cbSize);
			ZPackage zpackage = new ZPackage(array2);
			steamNetworkingMessage_t.m_pfnRelease = array[0];
			steamNetworkingMessage_t.Release();
			this.m_totalRecv += zpackage.Size();
			this.m_gotData = true;
			return zpackage;
		}
		return null;
	}

	// Token: 0x06000FF0 RID: 4080 RVA: 0x00069F08 File Offset: 0x00068108
	public string GetEndPointString()
	{
		return this.m_peerID.GetSteamID().ToString();
	}

	// Token: 0x06000FF1 RID: 4081 RVA: 0x00069F30 File Offset: 0x00068130
	public string GetHostName()
	{
		return this.m_peerID.GetSteamID().ToString();
	}

	// Token: 0x06000FF2 RID: 4082 RVA: 0x00069F56 File Offset: 0x00068156
	public CSteamID GetPeerID()
	{
		return this.m_peerID.GetSteamID();
	}

	// Token: 0x06000FF3 RID: 4083 RVA: 0x00069F63 File Offset: 0x00068163
	public bool IsHost()
	{
		return ZSteamSocket.m_hostSocket != null;
	}

	// Token: 0x06000FF4 RID: 4084 RVA: 0x00069F70 File Offset: 0x00068170
	public int GetSendQueueSize()
	{
		if (!this.IsConnected())
		{
			return 0;
		}
		int num = 0;
		foreach (byte[] array in this.m_sendQueue)
		{
			num += array.Length;
		}
		SteamNetworkingQuickConnectionStatus steamNetworkingQuickConnectionStatus;
		if (SteamNetworkingSockets.GetQuickConnectionStatus(this.m_con, out steamNetworkingQuickConnectionStatus))
		{
			num += steamNetworkingQuickConnectionStatus.m_cbPendingReliable + steamNetworkingQuickConnectionStatus.m_cbPendingUnreliable + steamNetworkingQuickConnectionStatus.m_cbSentUnackedReliable;
		}
		return num;
	}

	// Token: 0x06000FF5 RID: 4085 RVA: 0x00069FF8 File Offset: 0x000681F8
	public int GetCurrentSendRate()
	{
		SteamNetworkingQuickConnectionStatus steamNetworkingQuickConnectionStatus;
		if (!SteamNetworkingSockets.GetQuickConnectionStatus(this.m_con, out steamNetworkingQuickConnectionStatus))
		{
			return 0;
		}
		int num = steamNetworkingQuickConnectionStatus.m_cbPendingReliable + steamNetworkingQuickConnectionStatus.m_cbPendingUnreliable + steamNetworkingQuickConnectionStatus.m_cbSentUnackedReliable;
		foreach (byte[] array in this.m_sendQueue)
		{
			num += array.Length;
		}
		return num / Mathf.Clamp(steamNetworkingQuickConnectionStatus.m_nPing, 5, 250) * 1000;
	}

	// Token: 0x06000FF6 RID: 4086 RVA: 0x0006A08C File Offset: 0x0006828C
	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		SteamNetworkingQuickConnectionStatus steamNetworkingQuickConnectionStatus;
		if (SteamNetworkingSockets.GetQuickConnectionStatus(this.m_con, out steamNetworkingQuickConnectionStatus))
		{
			localQuality = steamNetworkingQuickConnectionStatus.m_flConnectionQualityLocal;
			remoteQuality = steamNetworkingQuickConnectionStatus.m_flConnectionQualityRemote;
			ping = steamNetworkingQuickConnectionStatus.m_nPing;
			outByteSec = steamNetworkingQuickConnectionStatus.m_flOutBytesPerSec;
			inByteSec = steamNetworkingQuickConnectionStatus.m_flInBytesPerSec;
			return;
		}
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
	}

	// Token: 0x06000FF7 RID: 4087 RVA: 0x0006A0F4 File Offset: 0x000682F4
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x06000FF8 RID: 4088 RVA: 0x0006A114 File Offset: 0x00068314
	public bool GotNewData()
	{
		bool gotData = this.m_gotData;
		this.m_gotData = false;
		return gotData;
	}

	// Token: 0x06000FF9 RID: 4089 RVA: 0x0006A123 File Offset: 0x00068323
	public int GetHostPort()
	{
		if (this.IsHost())
		{
			return 1;
		}
		return -1;
	}

	// Token: 0x06000FFA RID: 4090 RVA: 0x0006A130 File Offset: 0x00068330
	public static void SetDataPort(int port)
	{
		ZSteamSocket.m_steamDataPort = port;
	}

	// Token: 0x06000FFB RID: 4091 RVA: 0x000023E2 File Offset: 0x000005E2
	public void VersionMatch()
	{
	}

	// Token: 0x040010F7 RID: 4343
	private static List<ZSteamSocket> m_sockets = new List<ZSteamSocket>();

	// Token: 0x040010F8 RID: 4344
	private static Callback<SteamNetConnectionStatusChangedCallback_t> m_statusChanged;

	// Token: 0x040010F9 RID: 4345
	private static int m_steamDataPort = 2459;

	// Token: 0x040010FA RID: 4346
	private Queue<ZSteamSocket> m_pendingConnections = new Queue<ZSteamSocket>();

	// Token: 0x040010FB RID: 4347
	private HSteamNetConnection m_con = HSteamNetConnection.Invalid;

	// Token: 0x040010FC RID: 4348
	private SteamNetworkingIdentity m_peerID;

	// Token: 0x040010FD RID: 4349
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x040010FE RID: 4350
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x040010FF RID: 4351
	private int m_totalSent;

	// Token: 0x04001100 RID: 4352
	private int m_totalRecv;

	// Token: 0x04001101 RID: 4353
	private bool m_gotData;

	// Token: 0x04001102 RID: 4354
	private HSteamListenSocket m_listenSocket = HSteamListenSocket.Invalid;

	// Token: 0x04001103 RID: 4355
	private static ZSteamSocket m_hostSocket;

	// Token: 0x04001104 RID: 4356
	private static ESteamNetworkingConfigValue[] m_configValues = new ESteamNetworkingConfigValue[1];
}
