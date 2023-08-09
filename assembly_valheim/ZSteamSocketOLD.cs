using System;
using System.Collections.Generic;
using System.Threading;
using Steamworks;

// Token: 0x0200018C RID: 396
public class ZSteamSocketOLD : IDisposable, ISocket
{
	// Token: 0x06000FFD RID: 4093 RVA: 0x0006A15C File Offset: 0x0006835C
	public ZSteamSocketOLD()
	{
		ZSteamSocketOLD.m_sockets.Add(this);
		ZSteamSocketOLD.RegisterGlobalCallbacks();
	}

	// Token: 0x06000FFE RID: 4094 RVA: 0x0006A1AC File Offset: 0x000683AC
	public ZSteamSocketOLD(CSteamID peerID)
	{
		ZSteamSocketOLD.m_sockets.Add(this);
		this.m_peerID = peerID;
		ZSteamSocketOLD.RegisterGlobalCallbacks();
	}

	// Token: 0x06000FFF RID: 4095 RVA: 0x0006A204 File Offset: 0x00068404
	private static void RegisterGlobalCallbacks()
	{
		if (ZSteamSocketOLD.m_connectionFailed == null)
		{
			ZLog.Log("ZSteamSocketOLD  Registering global callbacks");
			ZSteamSocketOLD.m_connectionFailed = Callback<P2PSessionConnectFail_t>.Create(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(ZSteamSocketOLD.OnConnectionFailed));
		}
		if (ZSteamSocketOLD.m_SessionRequest == null)
		{
			ZSteamSocketOLD.m_SessionRequest = Callback<P2PSessionRequest_t>.Create(new Callback<P2PSessionRequest_t>.DispatchDelegate(ZSteamSocketOLD.OnSessionRequest));
		}
	}

	// Token: 0x06001000 RID: 4096 RVA: 0x0006A258 File Offset: 0x00068458
	private static void UnregisterGlobalCallbacks()
	{
		ZLog.Log("ZSteamSocket  UnregisterGlobalCallbacks, existing sockets:" + ZSteamSocketOLD.m_sockets.Count.ToString());
		if (ZSteamSocketOLD.m_connectionFailed != null)
		{
			ZSteamSocketOLD.m_connectionFailed.Dispose();
			ZSteamSocketOLD.m_connectionFailed = null;
		}
		if (ZSteamSocketOLD.m_SessionRequest != null)
		{
			ZSteamSocketOLD.m_SessionRequest.Dispose();
			ZSteamSocketOLD.m_SessionRequest = null;
		}
	}

	// Token: 0x06001001 RID: 4097 RVA: 0x0006A2B4 File Offset: 0x000684B4
	private static void OnConnectionFailed(P2PSessionConnectFail_t data)
	{
		string str = "Got connection failed callback: ";
		CSteamID steamIDRemote = data.m_steamIDRemote;
		ZLog.Log(str + steamIDRemote.ToString());
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			if (zsteamSocketOLD.IsPeer(data.m_steamIDRemote))
			{
				zsteamSocketOLD.Close();
			}
		}
	}

	// Token: 0x06001002 RID: 4098 RVA: 0x0006A338 File Offset: 0x00068538
	private static void OnSessionRequest(P2PSessionRequest_t data)
	{
		string str = "Got session request from ";
		CSteamID steamIDRemote = data.m_steamIDRemote;
		ZLog.Log(str + steamIDRemote.ToString());
		if (SteamNetworking.AcceptP2PSessionWithUser(data.m_steamIDRemote))
		{
			ZSteamSocketOLD listner = ZSteamSocketOLD.GetListner();
			if (listner != null)
			{
				listner.QueuePendingConnection(data.m_steamIDRemote);
			}
		}
	}

	// Token: 0x06001003 RID: 4099 RVA: 0x0006A38C File Offset: 0x0006858C
	public void Dispose()
	{
		ZLog.Log("Disposing socket");
		this.Close();
		this.m_pkgQueue.Clear();
		ZSteamSocketOLD.m_sockets.Remove(this);
		if (ZSteamSocketOLD.m_sockets.Count == 0)
		{
			ZLog.Log("Last socket, unregistering callback");
			ZSteamSocketOLD.UnregisterGlobalCallbacks();
		}
	}

	// Token: 0x06001004 RID: 4100 RVA: 0x0006A3DC File Offset: 0x000685DC
	public void Close()
	{
		ZLog.Log("Closing socket " + this.GetEndPointString());
		if (this.m_peerID != CSteamID.Nil)
		{
			this.Flush();
			ZLog.Log("  send queue size:" + this.m_sendQueue.Count.ToString());
			Thread.Sleep(100);
			P2PSessionState_t p2PSessionState_t;
			SteamNetworking.GetP2PSessionState(this.m_peerID, out p2PSessionState_t);
			ZLog.Log("  P2P state, bytes in send queue:" + p2PSessionState_t.m_nBytesQueuedForSend.ToString());
			SteamNetworking.CloseP2PSessionWithUser(this.m_peerID);
			SteamUser.EndAuthSession(this.m_peerID);
			this.m_peerID = CSteamID.Nil;
		}
		this.m_listner = false;
	}

	// Token: 0x06001005 RID: 4101 RVA: 0x0006A492 File Offset: 0x00068692
	public bool StartHost()
	{
		this.m_listner = true;
		this.m_pendingConnections.Clear();
		return true;
	}

	// Token: 0x06001006 RID: 4102 RVA: 0x0006A4A8 File Offset: 0x000686A8
	private ZSteamSocketOLD QueuePendingConnection(CSteamID id)
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in this.m_pendingConnections)
		{
			if (zsteamSocketOLD.IsPeer(id))
			{
				return zsteamSocketOLD;
			}
		}
		ZSteamSocketOLD zsteamSocketOLD2 = new ZSteamSocketOLD(id);
		this.m_pendingConnections.Enqueue(zsteamSocketOLD2);
		return zsteamSocketOLD2;
	}

	// Token: 0x06001007 RID: 4103 RVA: 0x0006A518 File Offset: 0x00068718
	public ISocket Accept()
	{
		if (!this.m_listner)
		{
			return null;
		}
		if (this.m_pendingConnections.Count > 0)
		{
			return this.m_pendingConnections.Dequeue();
		}
		return null;
	}

	// Token: 0x06001008 RID: 4104 RVA: 0x0006A53F File Offset: 0x0006873F
	public bool IsConnected()
	{
		return this.m_peerID != CSteamID.Nil;
	}

	// Token: 0x06001009 RID: 4105 RVA: 0x0006A554 File Offset: 0x00068754
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
		byte[] bytes = BitConverter.GetBytes(array.Length);
		byte[] array2 = new byte[array.Length + bytes.Length];
		bytes.CopyTo(array2, 0);
		array.CopyTo(array2, 4);
		this.m_sendQueue.Enqueue(array);
		this.SendQueuedPackages();
	}

	// Token: 0x0600100A RID: 4106 RVA: 0x0006A5B2 File Offset: 0x000687B2
	public bool Flush()
	{
		this.SendQueuedPackages();
		return this.m_sendQueue.Count == 0;
	}

	// Token: 0x0600100B RID: 4107 RVA: 0x0006A5C8 File Offset: 0x000687C8
	private void SendQueuedPackages()
	{
		if (!this.IsConnected())
		{
			return;
		}
		while (this.m_sendQueue.Count > 0)
		{
			byte[] array = this.m_sendQueue.Peek();
			EP2PSend eP2PSendType = EP2PSend.k_EP2PSendReliable;
			if (!SteamNetworking.SendP2PPacket(this.m_peerID, array, (uint)array.Length, eP2PSendType, 0))
			{
				break;
			}
			this.m_totalSent += array.Length;
			this.m_sendQueue.Dequeue();
		}
	}

	// Token: 0x0600100C RID: 4108 RVA: 0x0006A62C File Offset: 0x0006882C
	public static void Update()
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			zsteamSocketOLD.SendQueuedPackages();
		}
		ZSteamSocketOLD.ReceivePackages();
	}

	// Token: 0x0600100D RID: 4109 RVA: 0x0006A680 File Offset: 0x00068880
	private static void ReceivePackages()
	{
		uint num;
		while (SteamNetworking.IsP2PPacketAvailable(out num, 0))
		{
			byte[] array = new byte[num];
			uint num2;
			CSteamID sender;
			if (!SteamNetworking.ReadP2PPacket(array, num, out num2, out sender, 0))
			{
				break;
			}
			ZSteamSocketOLD.QueueNewPkg(sender, array);
		}
	}

	// Token: 0x0600100E RID: 4110 RVA: 0x0006A6B8 File Offset: 0x000688B8
	private static void QueueNewPkg(CSteamID sender, byte[] data)
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			if (zsteamSocketOLD.IsPeer(sender))
			{
				zsteamSocketOLD.QueuePackage(data);
				return;
			}
		}
		ZSteamSocketOLD listner = ZSteamSocketOLD.GetListner();
		CSteamID csteamID;
		if (listner != null)
		{
			string str = "Got package from unconnected peer ";
			csteamID = sender;
			ZLog.Log(str + csteamID.ToString());
			listner.QueuePendingConnection(sender).QueuePackage(data);
			return;
		}
		string str2 = "Got package from unkown peer ";
		csteamID = sender;
		ZLog.Log(str2 + csteamID.ToString() + " but no active listner");
	}

	// Token: 0x0600100F RID: 4111 RVA: 0x0006A770 File Offset: 0x00068970
	private static ZSteamSocketOLD GetListner()
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			if (zsteamSocketOLD.IsHost())
			{
				return zsteamSocketOLD;
			}
		}
		return null;
	}

	// Token: 0x06001010 RID: 4112 RVA: 0x0006A7CC File Offset: 0x000689CC
	private void QueuePackage(byte[] data)
	{
		ZPackage item = new ZPackage(data);
		this.m_pkgQueue.Enqueue(item);
		this.m_gotData = true;
		this.m_totalRecv += data.Length;
	}

	// Token: 0x06001011 RID: 4113 RVA: 0x0006A803 File Offset: 0x00068A03
	public ZPackage Recv()
	{
		if (!this.IsConnected())
		{
			return null;
		}
		if (this.m_pkgQueue.Count > 0)
		{
			return this.m_pkgQueue.Dequeue();
		}
		return null;
	}

	// Token: 0x06001012 RID: 4114 RVA: 0x0006A82A File Offset: 0x00068A2A
	public string GetEndPointString()
	{
		return this.m_peerID.ToString();
	}

	// Token: 0x06001013 RID: 4115 RVA: 0x0006A82A File Offset: 0x00068A2A
	public string GetHostName()
	{
		return this.m_peerID.ToString();
	}

	// Token: 0x06001014 RID: 4116 RVA: 0x0006A83D File Offset: 0x00068A3D
	public CSteamID GetPeerID()
	{
		return this.m_peerID;
	}

	// Token: 0x06001015 RID: 4117 RVA: 0x0006A845 File Offset: 0x00068A45
	public bool IsPeer(CSteamID peer)
	{
		return this.IsConnected() && peer == this.m_peerID;
	}

	// Token: 0x06001016 RID: 4118 RVA: 0x0006A85D File Offset: 0x00068A5D
	public bool IsHost()
	{
		return this.m_listner;
	}

	// Token: 0x06001017 RID: 4119 RVA: 0x0006A868 File Offset: 0x00068A68
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
		return num;
	}

	// Token: 0x06001018 RID: 4120 RVA: 0x0006A8C8 File Offset: 0x00068AC8
	public bool IsSending()
	{
		return this.IsConnected() && this.m_sendQueue.Count > 0;
	}

	// Token: 0x06001019 RID: 4121 RVA: 0x0006A8E2 File Offset: 0x00068AE2
	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
	}

	// Token: 0x0600101A RID: 4122 RVA: 0x0006A905 File Offset: 0x00068B05
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x0600101B RID: 4123 RVA: 0x0006A925 File Offset: 0x00068B25
	public bool GotNewData()
	{
		bool gotData = this.m_gotData;
		this.m_gotData = false;
		return gotData;
	}

	// Token: 0x0600101C RID: 4124 RVA: 0x0000247B File Offset: 0x0000067B
	public int GetCurrentSendRate()
	{
		return 0;
	}

	// Token: 0x0600101D RID: 4125 RVA: 0x0000247B File Offset: 0x0000067B
	public int GetAverageSendRate()
	{
		return 0;
	}

	// Token: 0x0600101E RID: 4126 RVA: 0x0006A934 File Offset: 0x00068B34
	public int GetHostPort()
	{
		if (this.IsHost())
		{
			return 1;
		}
		return -1;
	}

	// Token: 0x0600101F RID: 4127 RVA: 0x000023E2 File Offset: 0x000005E2
	public void VersionMatch()
	{
	}

	// Token: 0x04001105 RID: 4357
	private static List<ZSteamSocketOLD> m_sockets = new List<ZSteamSocketOLD>();

	// Token: 0x04001106 RID: 4358
	private static Callback<P2PSessionRequest_t> m_SessionRequest;

	// Token: 0x04001107 RID: 4359
	private static Callback<P2PSessionConnectFail_t> m_connectionFailed;

	// Token: 0x04001108 RID: 4360
	private Queue<ZSteamSocketOLD> m_pendingConnections = new Queue<ZSteamSocketOLD>();

	// Token: 0x04001109 RID: 4361
	private CSteamID m_peerID = CSteamID.Nil;

	// Token: 0x0400110A RID: 4362
	private bool m_listner;

	// Token: 0x0400110B RID: 4363
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x0400110C RID: 4364
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x0400110D RID: 4365
	private int m_totalSent;

	// Token: 0x0400110E RID: 4366
	private int m_totalRecv;

	// Token: 0x0400110F RID: 4367
	private bool m_gotData;
}
