using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000134 RID: 308
public class MasterClient
{
	// Token: 0x1700006C RID: 108
	// (get) Token: 0x06000BF8 RID: 3064 RVA: 0x00057711 File Offset: 0x00055911
	public static MasterClient instance
	{
		get
		{
			return MasterClient.m_instance;
		}
	}

	// Token: 0x06000BF9 RID: 3065 RVA: 0x00057718 File Offset: 0x00055918
	public static void Initialize()
	{
		if (MasterClient.m_instance == null)
		{
			MasterClient.m_instance = new MasterClient();
		}
	}

	// Token: 0x06000BFA RID: 3066 RVA: 0x0005772B File Offset: 0x0005592B
	public MasterClient()
	{
		this.m_sessionUID = Utils.GenerateUID();
	}

	// Token: 0x06000BFB RID: 3067 RVA: 0x0005776C File Offset: 0x0005596C
	public void Dispose()
	{
		if (this.m_socket != null)
		{
			this.m_socket.Dispose();
		}
		if (this.m_connector != null)
		{
			this.m_connector.Dispose();
		}
		if (this.m_rpc != null)
		{
			this.m_rpc.Dispose();
		}
		if (MasterClient.m_instance == this)
		{
			MasterClient.m_instance = null;
		}
	}

	// Token: 0x06000BFC RID: 3068 RVA: 0x000577C0 File Offset: 0x000559C0
	public void Update(float dt)
	{
	}

	// Token: 0x06000BFD RID: 3069 RVA: 0x000577D0 File Offset: 0x000559D0
	private void SendStats(float duration)
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(2);
		zpackage.Write(this.m_sessionUID);
		zpackage.Write(Time.time);
		bool flag = Player.m_localPlayer != null;
		zpackage.Write(flag ? duration : 0f);
		bool flag2 = ZNet.instance && !ZNet.instance.IsServer();
		zpackage.Write(flag2 ? duration : 0f);
		zpackage.Write(global::Version.CurrentVersion.ToString());
		zpackage.Write(5U);
		bool flag3 = ZNet.instance && ZNet.instance.IsServer();
		zpackage.Write(flag3);
		if (flag3)
		{
			zpackage.Write(ZNet.instance.GetWorldUID());
			zpackage.Write(duration);
			int num = ZNet.instance.GetPeerConnections();
			if (Player.m_localPlayer != null)
			{
				num++;
			}
			zpackage.Write(num);
			bool data = ZNet.instance.GetZNat() != null && ZNet.instance.GetZNat().GetStatus();
			zpackage.Write(data);
		}
		PlayerProfile playerProfile = (Game.instance != null) ? Game.instance.GetPlayerProfile() : null;
		if (playerProfile != null)
		{
			zpackage.Write(true);
			zpackage.Write(playerProfile.GetPlayerID());
			zpackage.Write(playerProfile.m_playerStats.m_kills);
			zpackage.Write(playerProfile.m_playerStats.m_deaths);
			zpackage.Write(playerProfile.m_playerStats.m_crafts);
			zpackage.Write(playerProfile.m_playerStats.m_builds);
		}
		else
		{
			zpackage.Write(false);
		}
		this.m_rpc.Invoke("Stats", new object[]
		{
			zpackage
		});
	}

	// Token: 0x06000BFE RID: 3070 RVA: 0x00057998 File Offset: 0x00055B98
	public void RegisterServer(string name, string host, int port, bool password, bool upnp, long worldUID, string gameVersion, uint networkVersion)
	{
		this.m_registerPkg = new ZPackage();
		this.m_registerPkg.Write(1);
		this.m_registerPkg.Write(name);
		this.m_registerPkg.Write(host);
		this.m_registerPkg.Write(port);
		this.m_registerPkg.Write(password);
		this.m_registerPkg.Write(upnp);
		this.m_registerPkg.Write(worldUID);
		this.m_registerPkg.Write(gameVersion);
		this.m_registerPkg.Write(networkVersion);
		if (this.m_rpc != null)
		{
			this.m_rpc.Invoke("RegisterServer2", new object[]
			{
				this.m_registerPkg
			});
		}
		ZLog.Log(string.Concat(new string[]
		{
			"Registering server ",
			name,
			"  ",
			host,
			":",
			port.ToString()
		}));
	}

	// Token: 0x06000BFF RID: 3071 RVA: 0x00057A82 File Offset: 0x00055C82
	public void UnregisterServer()
	{
		if (this.m_registerPkg == null)
		{
			return;
		}
		if (this.m_rpc != null)
		{
			this.m_rpc.Invoke("UnregisterServer", Array.Empty<object>());
		}
		this.m_registerPkg = null;
	}

	// Token: 0x06000C00 RID: 3072 RVA: 0x00057AB1 File Offset: 0x00055CB1
	public List<ServerStatus> GetServers()
	{
		return this.m_servers;
	}

	// Token: 0x06000C01 RID: 3073 RVA: 0x00057AB9 File Offset: 0x00055CB9
	public bool GetServers(List<ServerStatus> servers)
	{
		if (!this.m_haveServerlist)
		{
			return false;
		}
		servers.Clear();
		servers.AddRange(this.m_servers);
		return true;
	}

	// Token: 0x06000C02 RID: 3074 RVA: 0x00057AD8 File Offset: 0x00055CD8
	public void RequestServerlist()
	{
		if (this.m_rpc != null)
		{
			this.m_rpc.Invoke("RequestServerlist2", Array.Empty<object>());
		}
	}

	// Token: 0x06000C03 RID: 3075 RVA: 0x00057AF8 File Offset: 0x00055CF8
	private void RPC_ServerList(ZRpc rpc, ZPackage pkg)
	{
		this.m_haveServerlist = true;
		this.m_serverListRevision++;
		pkg.ReadInt();
		int num = pkg.ReadInt();
		this.m_servers.Clear();
		for (int i = 0; i < num; i++)
		{
			string serverName = pkg.ReadString();
			string str = pkg.ReadString();
			int num2 = pkg.ReadInt();
			bool isPasswordProtected = pkg.ReadBool();
			pkg.ReadBool();
			pkg.ReadLong();
			string text = pkg.ReadString();
			uint networkVersion = 0U;
			GameVersion lhs;
			if (GameVersion.TryParseGameVersion(text, out lhs) && lhs >= global::Version.FirstVersionWithNetworkVersion)
			{
				networkVersion = pkg.ReadUInt();
			}
			int playerCount = pkg.ReadInt();
			ServerStatus serverStatus = new ServerStatus(new ServerJoinDataDedicated(str + ":" + num2.ToString()));
			serverStatus.UpdateStatus(OnlineStatus.Online, serverName, (uint)playerCount, text, networkVersion, isPasswordProtected, PrivilegeManager.Platform.None, true);
			if (this.m_nameFilter.Length <= 0 || serverStatus.m_joinData.m_serverName.Contains(this.m_nameFilter))
			{
				this.m_servers.Add(serverStatus);
			}
		}
		if (this.m_onServerList != null)
		{
			this.m_onServerList(this.m_servers);
		}
	}

	// Token: 0x06000C04 RID: 3076 RVA: 0x00057C20 File Offset: 0x00055E20
	public int GetServerListRevision()
	{
		return this.m_serverListRevision;
	}

	// Token: 0x06000C05 RID: 3077 RVA: 0x00057C28 File Offset: 0x00055E28
	public bool IsConnected()
	{
		return this.m_rpc != null;
	}

	// Token: 0x06000C06 RID: 3078 RVA: 0x00057C33 File Offset: 0x00055E33
	public void SetNameFilter(string filter)
	{
		this.m_nameFilter = filter;
		ZLog.Log("filter is " + filter);
	}

	// Token: 0x04000E4E RID: 3662
	private const int statVersion = 2;

	// Token: 0x04000E4F RID: 3663
	public Action<List<ServerStatus>> m_onServerList;

	// Token: 0x04000E50 RID: 3664
	private string m_msHost = "dvoid.noip.me";

	// Token: 0x04000E51 RID: 3665
	private int m_msPort = 9983;

	// Token: 0x04000E52 RID: 3666
	private long m_sessionUID;

	// Token: 0x04000E53 RID: 3667
	private ZConnector2 m_connector;

	// Token: 0x04000E54 RID: 3668
	private ZSocket2 m_socket;

	// Token: 0x04000E55 RID: 3669
	private ZRpc m_rpc;

	// Token: 0x04000E56 RID: 3670
	private bool m_haveServerlist;

	// Token: 0x04000E57 RID: 3671
	private List<ServerStatus> m_servers = new List<ServerStatus>();

	// Token: 0x04000E58 RID: 3672
	private ZPackage m_registerPkg;

	// Token: 0x04000E59 RID: 3673
	private float m_sendStatsTimer;

	// Token: 0x04000E5A RID: 3674
	private int m_serverListRevision;

	// Token: 0x04000E5B RID: 3675
	private string m_nameFilter = "";

	// Token: 0x04000E5C RID: 3676
	private static MasterClient m_instance;
}
