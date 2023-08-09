using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Steamworks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Token: 0x02000167 RID: 359
public class ZNet : MonoBehaviour
{
	// Token: 0x17000094 RID: 148
	// (get) Token: 0x06000E05 RID: 3589 RVA: 0x000609FE File Offset: 0x0005EBFE
	public static ZNet instance
	{
		get
		{
			return ZNet.m_instance;
		}
	}

	// Token: 0x06000E06 RID: 3590 RVA: 0x00060A08 File Offset: 0x0005EC08
	private void Awake()
	{
		ZNet.m_instance = this;
		ZNet.m_loadError = false;
		this.m_routedRpc = new ZRoutedRpc(ZNet.m_isServer);
		this.m_zdoMan = new ZDOMan(this.m_zdoSectorsWidth);
		this.m_passwordDialog.gameObject.SetActive(false);
		this.m_connectingDialog.gameObject.SetActive(false);
		WorldGenerator.Deitialize();
		if (SteamManager.Initialize())
		{
			string personaName = SteamFriends.GetPersonaName();
			ZLog.Log("Steam initialized, persona:" + personaName);
			ZSteamMatchmaking.Initialize();
			ZPlayFabMatchmaking.Initialize(ZNet.m_isServer);
			ZNet.m_backupCount = PlatformPrefs.GetInt("AutoBackups", ZNet.m_backupCount);
			ZNet.m_backupShort = PlatformPrefs.GetInt("AutoBackups_short", ZNet.m_backupShort);
			ZNet.m_backupLong = PlatformPrefs.GetInt("AutoBackups_long", ZNet.m_backupLong);
			if (ZNet.m_isServer)
			{
				this.m_adminList = new SyncedList(Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/adminlist.txt", "List admin players ID  ONE per line");
				this.m_bannedList = new SyncedList(Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/bannedlist.txt", "List banned players ID  ONE per line");
				this.m_permittedList = new SyncedList(Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/permittedlist.txt", "List permitted players ID ONE per line");
				if (ZNet.m_world == null)
				{
					ZNet.m_publicServer = false;
					ZNet.m_world = World.GetDevWorld();
				}
				if (ZNet.m_openServer)
				{
					bool flag = ZNet.m_serverPassword != "";
					string gameVersion = global::Version.CurrentVersion.ToString();
					uint networkVersion = 5U;
					ZSteamMatchmaking.instance.RegisterServer(ZNet.m_ServerName, flag, gameVersion, networkVersion, ZNet.m_publicServer, ZNet.m_world.m_seedName, new ZSteamMatchmaking.ServerRegistered(this.OnSteamServerRegistered));
					if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
					{
						ZSteamSocket zsteamSocket = new ZSteamSocket();
						zsteamSocket.StartHost();
						this.m_hostSocket = zsteamSocket;
					}
					if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
					{
						ZPlayFabMatchmaking.instance.RegisterServer(ZNet.m_ServerName, flag, ZNet.m_publicServer, gameVersion, networkVersion, ZNet.m_world.m_seedName, true);
						ZPlayFabSocket zplayFabSocket = new ZPlayFabSocket();
						zplayFabSocket.StartHost();
						this.m_hostSocket = zplayFabSocket;
					}
				}
				WorldGenerator.Initialize(ZNet.m_world);
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connected;
				ZNet.m_externalError = ZNet.ConnectionStatus.None;
			}
			this.m_routedRpc.SetUID(ZDOMan.GetSessionID());
			if (this.IsServer())
			{
				this.SendPlayerList();
			}
			return;
		}
	}

	// Token: 0x06000E07 RID: 3591 RVA: 0x00060C48 File Offset: 0x0005EE48
	private void Start()
	{
		ZRpc.SetLongTimeout(false);
		if (ZNet.m_isServer)
		{
			this.LoadWorld();
			ZoneSystem.instance.GenerateLocationsIfNeeded();
			if (ZNet.m_loadError)
			{
				ZLog.LogError("World db couldn't load correctly, saving has been disabled to prevent .old file from being overwritten.");
			}
			return;
		}
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			ZLog.Log("Connecting to server with PlayFab-backend " + ZNet.m_serverPlayFabPlayerId);
			this.Connect(ZNet.m_serverPlayFabPlayerId);
		}
		if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
		{
			if (ZNet.m_serverSteamID == 0UL)
			{
				ZLog.Log("Connecting to server with Steam-backend " + ZNet.m_serverHost + ":" + ZNet.m_serverHostPort.ToString());
				SteamNetworkingIPAddr host = default(SteamNetworkingIPAddr);
				host.ParseString(ZNet.m_serverHost + ":" + ZNet.m_serverHostPort.ToString());
				this.Connect(host);
				return;
			}
			ZLog.Log("Connecting to server with Steam-backend " + ZNet.m_serverSteamID.ToString());
			this.Connect(new CSteamID(ZNet.m_serverSteamID));
		}
		if (ZNet.m_onlineBackend == OnlineBackendType.CustomSocket)
		{
			ZLog.Log("Connecting to server with socket-backend " + ZNet.m_serverHost + "  " + ZNet.m_serverHostPort.ToString());
			this.Connect(ZNet.m_serverHost, ZNet.m_serverHostPort);
		}
	}

	// Token: 0x06000E08 RID: 3592 RVA: 0x00060D76 File Offset: 0x0005EF76
	private string GetServerIP()
	{
		return ZNet.GetPublicIP();
	}

	// Token: 0x06000E09 RID: 3593 RVA: 0x00060D80 File Offset: 0x0005EF80
	private string LocalIPAddress()
	{
		string text = IPAddress.Loopback.ToString();
		try
		{
			foreach (IPAddress ipaddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
				{
					text = ipaddress.ToString();
					break;
				}
			}
		}
		catch (Exception ex)
		{
			ZLog.Log(string.Format("Failed to get local address, using {0}: {1}", text, ex.Message));
		}
		return text;
	}

	// Token: 0x06000E0A RID: 3594 RVA: 0x00060DFC File Offset: 0x0005EFFC
	public static string GetPublicIP()
	{
		string result;
		try
		{
			string[] array = new string[]
			{
				"http://checkip.dyndns.org/",
				"http://icanhazip.com",
				"https://checkip.amazonaws.com/",
				"https://ipinfo.io/ip/",
				"https://wtfismyip.com/text"
			};
			System.Random random = new System.Random();
			string text = ZNet.<GetPublicIP>g__DownloadString|6_0(array[random.Next(array.Length)], 5000);
			text = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}").Matches(text)[0].ToString();
			result = text;
		}
		catch (Exception ex)
		{
			ZLog.LogError(ex.Message);
			result = "";
		}
		return result;
	}

	// Token: 0x06000E0B RID: 3595 RVA: 0x00060E98 File Offset: 0x0005F098
	private void OnSteamServerRegistered(bool success)
	{
		if (!success)
		{
			this.m_registerAttempts++;
			float num = 1f * Mathf.Pow(2f, (float)(this.m_registerAttempts - 1));
			num = Mathf.Min(num, 30f);
			num *= UnityEngine.Random.Range(0.875f, 1.125f);
			this.<OnSteamServerRegistered>g__RetryRegisterAfterDelay|7_0(num);
		}
	}

	// Token: 0x06000E0C RID: 3596 RVA: 0x00060EF5 File Offset: 0x0005F0F5
	public void Shutdown()
	{
		ZLog.Log("ZNet Shutdown");
		this.Save(true);
		this.StopAll();
		base.enabled = false;
	}

	// Token: 0x06000E0D RID: 3597 RVA: 0x00060F18 File Offset: 0x0005F118
	private void StopAll()
	{
		if (this.m_haveStoped)
		{
			return;
		}
		this.m_haveStoped = true;
		if (this.m_saveThread != null && this.m_saveThread.IsAlive)
		{
			this.m_saveThread.Join();
			this.m_saveThread = null;
		}
		this.m_zdoMan.ShutDown();
		this.SendDisconnect();
		ZSteamMatchmaking.instance.ReleaseSessionTicket();
		ZSteamMatchmaking.instance.UnregisterServer();
		ZPlayFabMatchmaking instance = ZPlayFabMatchmaking.instance;
		if (instance != null)
		{
			instance.UnregisterServer();
		}
		if (this.m_hostSocket != null)
		{
			this.m_hostSocket.Dispose();
		}
		if (this.m_serverConnector != null)
		{
			this.m_serverConnector.Dispose();
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			znetPeer.Dispose();
		}
		this.m_peers.Clear();
	}

	// Token: 0x06000E0E RID: 3598 RVA: 0x00061008 File Offset: 0x0005F208
	private void OnDestroy()
	{
		ZLog.Log("ZNet OnDestroy");
		if (ZNet.m_instance == this)
		{
			ZNet.m_instance = null;
		}
	}

	// Token: 0x06000E0F RID: 3599 RVA: 0x00061028 File Offset: 0x0005F228
	private ZNetPeer Connect(ISocket socket)
	{
		ZNetPeer znetPeer = new ZNetPeer(socket, true);
		this.OnNewConnection(znetPeer);
		ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connecting;
		ZNet.m_externalError = ZNet.ConnectionStatus.None;
		this.m_connectingDialog.gameObject.SetActive(true);
		return znetPeer;
	}

	// Token: 0x06000E10 RID: 3600 RVA: 0x00061064 File Offset: 0x0005F264
	public void Connect(string remotePlayerId)
	{
		ZNet.<>c__DisplayClass12_0 CS$<>8__locals1 = new ZNet.<>c__DisplayClass12_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.socket = null;
		CS$<>8__locals1.peer = null;
		CS$<>8__locals1.socket = new ZPlayFabSocket(remotePlayerId, new Action<PlayFabMatchmakingServerData>(CS$<>8__locals1.<Connect>g__CheckServerData|0));
		CS$<>8__locals1.peer = this.Connect(CS$<>8__locals1.socket);
	}

	// Token: 0x06000E11 RID: 3601 RVA: 0x000610B6 File Offset: 0x0005F2B6
	public void Connect(CSteamID hostID)
	{
		this.Connect(new ZSteamSocket(hostID));
	}

	// Token: 0x06000E12 RID: 3602 RVA: 0x000610C5 File Offset: 0x0005F2C5
	public void Connect(SteamNetworkingIPAddr host)
	{
		this.Connect(new ZSteamSocket(host));
	}

	// Token: 0x06000E13 RID: 3603 RVA: 0x000610D4 File Offset: 0x0005F2D4
	public void Connect(string host, int port)
	{
		this.m_serverConnector = new ZConnector2(host, port);
		ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connecting;
		ZNet.m_externalError = ZNet.ConnectionStatus.None;
		this.m_connectingDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000E14 RID: 3604 RVA: 0x00061100 File Offset: 0x0005F300
	private void UpdateClientConnector(float dt)
	{
		if (this.m_serverConnector != null && this.m_serverConnector.UpdateStatus(dt, true))
		{
			ZSocket2 zsocket = this.m_serverConnector.Complete();
			if (zsocket != null)
			{
				ZLog.Log("Connection established to " + this.m_serverConnector.GetEndPointString());
				ZNetPeer peer = new ZNetPeer(zsocket, true);
				this.OnNewConnection(peer);
			}
			else
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorConnectFailed;
				ZLog.Log("Failed to connect to server");
			}
			this.m_serverConnector.Dispose();
			this.m_serverConnector = null;
		}
	}

	// Token: 0x06000E15 RID: 3605 RVA: 0x00061180 File Offset: 0x0005F380
	private void OnNewConnection(ZNetPeer peer)
	{
		this.m_peers.Add(peer);
		peer.m_rpc.Register<ZPackage>("PeerInfo", new Action<ZRpc, ZPackage>(this.RPC_PeerInfo));
		peer.m_rpc.Register("Disconnect", new ZRpc.RpcMethod.Method(this.RPC_Disconnect));
		peer.m_rpc.Register("ClientSave", new ZRpc.RpcMethod.Method(this.RPC_ClientSave));
		if (ZNet.m_isServer)
		{
			peer.m_rpc.Register("ServerHandshake", new ZRpc.RpcMethod.Method(this.RPC_ServerHandshake));
			return;
		}
		peer.m_rpc.Register("Kicked", new ZRpc.RpcMethod.Method(this.RPC_Kicked));
		peer.m_rpc.Register<int>("Error", new Action<ZRpc, int>(this.RPC_Error));
		peer.m_rpc.Register<bool, string>("ClientHandshake", new Action<ZRpc, bool, string>(this.RPC_ClientHandshake));
		peer.m_rpc.Invoke("ServerHandshake", Array.Empty<object>());
	}

	// Token: 0x06000E16 RID: 3606 RVA: 0x0006127C File Offset: 0x0005F47C
	public void SendClientSave()
	{
		ZLog.Log("Sending client save message");
		if (!this.IsServer())
		{
			ZLog.Log("Not sending client save message as we're not the host");
			return;
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.m_rpc != null)
			{
				ZLog.Log("Sent to " + znetPeer.m_socket.GetEndPointString());
				znetPeer.m_rpc.Invoke("ClientSave", Array.Empty<object>());
			}
		}
	}

	// Token: 0x06000E17 RID: 3607 RVA: 0x0006131C File Offset: 0x0005F51C
	private void RPC_ClientSave(ZRpc rpc)
	{
		Debug.Log("RPC Client Save received");
		Game.instance.SavePlayerProfile(false);
	}

	// Token: 0x06000E18 RID: 3608 RVA: 0x00061334 File Offset: 0x0005F534
	private void RPC_ServerHandshake(ZRpc rpc)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer == null)
		{
			return;
		}
		ZLog.Log("Got handshake from client " + peer.m_socket.GetEndPointString());
		this.ClearPlayerData(peer);
		bool flag = !string.IsNullOrEmpty(ZNet.m_serverPassword);
		peer.m_rpc.Invoke("ClientHandshake", new object[]
		{
			flag,
			ZNet.ServerPasswordSalt()
		});
	}

	// Token: 0x06000E19 RID: 3609 RVA: 0x000613A3 File Offset: 0x0005F5A3
	private void UpdatePassword()
	{
		if (this.m_passwordDialog.gameObject.activeSelf)
		{
			this.m_passwordDialog.GetComponentInChildren<InputField>().ActivateInputField();
		}
	}

	// Token: 0x06000E1A RID: 3610 RVA: 0x000613C7 File Offset: 0x0005F5C7
	public bool InPasswordDialog()
	{
		return this.m_passwordDialog.gameObject.activeSelf;
	}

	// Token: 0x06000E1B RID: 3611 RVA: 0x000613DC File Offset: 0x0005F5DC
	private void RPC_ClientHandshake(ZRpc rpc, bool needPassword, string serverPasswordSalt)
	{
		this.m_connectingDialog.gameObject.SetActive(false);
		ZNet.m_serverPasswordSalt = serverPasswordSalt;
		if (needPassword)
		{
			this.m_passwordDialog.gameObject.SetActive(true);
			InputField componentInChildren = this.m_passwordDialog.GetComponentInChildren<InputField>();
			componentInChildren.text = "";
			componentInChildren.ActivateInputField();
			this.m_passwordDialog.GetComponentInChildren<InputFieldSubmit>().m_onSubmit = new Action<string>(this.OnPasswordEnter);
			this.m_tempPasswordRPC = rpc;
			return;
		}
		this.SendPeerInfo(rpc, "");
	}

	// Token: 0x06000E1C RID: 3612 RVA: 0x0006145F File Offset: 0x0005F65F
	private void OnPasswordEnter(string pwd)
	{
		if (!this.m_tempPasswordRPC.IsConnected())
		{
			return;
		}
		this.m_passwordDialog.gameObject.SetActive(false);
		this.SendPeerInfo(this.m_tempPasswordRPC, pwd);
		this.m_tempPasswordRPC = null;
	}

	// Token: 0x06000E1D RID: 3613 RVA: 0x00061494 File Offset: 0x0005F694
	public void OnPasswordOk()
	{
		InputFieldSubmit componentInChildren = this.m_passwordDialog.GetComponentInChildren<InputFieldSubmit>();
		this.OnPasswordEnter(componentInChildren.GetComponentInChildren<Text>().text);
	}

	// Token: 0x06000E1E RID: 3614 RVA: 0x000614C0 File Offset: 0x0005F6C0
	private void SendPeerInfo(ZRpc rpc, string password = "")
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(ZNet.GetUID());
		zpackage.Write(global::Version.CurrentVersion.ToString());
		zpackage.Write(5U);
		zpackage.Write(this.m_referencePosition);
		zpackage.Write(Game.instance.GetPlayerProfile().GetName());
		if (this.IsServer())
		{
			zpackage.Write(ZNet.m_world.m_name);
			zpackage.Write(ZNet.m_world.m_seed);
			zpackage.Write(ZNet.m_world.m_seedName);
			zpackage.Write(ZNet.m_world.m_uid);
			zpackage.Write(ZNet.m_world.m_worldGenVersion);
			zpackage.Write(this.m_netTime);
		}
		else
		{
			string data = string.IsNullOrEmpty(password) ? "" : ZNet.HashPassword(password, ZNet.ServerPasswordSalt());
			zpackage.Write(data);
			byte[] array = ZSteamMatchmaking.instance.RequestSessionTicket();
			if (array == null)
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorConnectFailed;
				return;
			}
			zpackage.Write(array);
		}
		rpc.Invoke("PeerInfo", new object[]
		{
			zpackage
		});
	}

	// Token: 0x06000E1F RID: 3615 RVA: 0x000615D8 File Offset: 0x0005F7D8
	private void RPC_PeerInfo(ZRpc rpc, ZPackage pkg)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer == null)
		{
			return;
		}
		long uid = pkg.ReadLong();
		string versionString = pkg.ReadString();
		uint num = 0U;
		GameVersion gameVersion;
		if (GameVersion.TryParseGameVersion(versionString, out gameVersion) && gameVersion >= global::Version.FirstVersionWithNetworkVersion)
		{
			num = pkg.ReadUInt();
		}
		string endPointString = peer.m_socket.GetEndPointString();
		string hostName = peer.m_socket.GetHostName();
		ZLog.Log("Network version check, their:" + num.ToString() + ", mine:" + 5U.ToString());
		if (num != 5U)
		{
			if (ZNet.m_isServer)
			{
				rpc.Invoke("Error", new object[]
				{
					3
				});
			}
			else
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorVersion;
			}
			string[] array = new string[11];
			array[0] = "Peer ";
			array[1] = endPointString;
			array[2] = " has incompatible version, mine:";
			array[3] = global::Version.CurrentVersion.ToString();
			array[4] = " (network version ";
			array[5] = 5U.ToString();
			array[6] = ")   remote ";
			int num2 = 7;
			GameVersion gameVersion2 = gameVersion;
			array[num2] = gameVersion2.ToString();
			array[8] = " (network version ";
			array[9] = ((num == uint.MaxValue) ? "unknown" : num.ToString());
			array[10] = ")";
			ZLog.Log(string.Concat(array));
			return;
		}
		Vector3 refPos = pkg.ReadVector3();
		string text = pkg.ReadString();
		if (ZNet.m_isServer)
		{
			if (!this.IsAllowed(hostName, text))
			{
				rpc.Invoke("Error", new object[]
				{
					8
				});
				ZLog.Log(string.Concat(new string[]
				{
					"Player ",
					text,
					" : ",
					hostName,
					" is blacklisted or not in whitelist."
				}));
				return;
			}
			string b = pkg.ReadString();
			if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
			{
				ZSteamSocket zsteamSocket = peer.m_socket as ZSteamSocket;
				byte[] ticket = pkg.ReadByteArray();
				if (!ZSteamMatchmaking.instance.VerifySessionTicket(ticket, zsteamSocket.GetPeerID()))
				{
					ZLog.Log("Peer " + endPointString + " has invalid session ticket");
					rpc.Invoke("Error", new object[]
					{
						8
					});
					return;
				}
			}
			if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
			{
				PrivilegeManager.Platform platform;
				if (!Enum.TryParse<PrivilegeManager.Platform>(peer.m_socket.GetHostName().Split(new char[]
				{
					'_'
				})[0], out platform))
				{
					ZLog.LogError("Failed to parse peer platform! Using \"" + PrivilegeManager.Platform.Unknown.ToString() + "\".");
					platform = PrivilegeManager.Platform.Unknown;
				}
				if (!PrivilegeManager.CanCrossplay && PrivilegeManager.GetCurrentPlatform() != platform)
				{
					rpc.Invoke("Error", new object[]
					{
						10
					});
					ZLog.Log("Peer diconnected due to server platform privileges disallowing crossplay. Server platform: " + PrivilegeManager.GetCurrentPlatform().ToString() + "   Peer platform: " + platform.ToString());
					return;
				}
			}
			if (this.GetNrOfPlayers() >= 10)
			{
				rpc.Invoke("Error", new object[]
				{
					9
				});
				ZLog.Log("Peer " + endPointString + " disconnected due to server is full");
				return;
			}
			if (ZNet.m_serverPassword != b)
			{
				rpc.Invoke("Error", new object[]
				{
					6
				});
				ZLog.Log("Peer " + endPointString + " has wrong password");
				return;
			}
			if (this.IsConnected(uid))
			{
				rpc.Invoke("Error", new object[]
				{
					7
				});
				ZLog.Log("Already connected to peer with UID:" + uid.ToString() + "  " + endPointString);
				return;
			}
		}
		else
		{
			ZNet.m_world = new World();
			ZNet.m_world.m_name = pkg.ReadString();
			ZNet.m_world.m_seed = pkg.ReadInt();
			ZNet.m_world.m_seedName = pkg.ReadString();
			ZNet.m_world.m_uid = pkg.ReadLong();
			ZNet.m_world.m_worldGenVersion = pkg.ReadInt();
			WorldGenerator.Initialize(ZNet.m_world);
			this.m_netTime = pkg.ReadDouble();
		}
		peer.m_refPos = refPos;
		peer.m_uid = uid;
		peer.m_playerName = text;
		rpc.Register<Vector3, bool>("RefPos", new Action<ZRpc, Vector3, bool>(this.RPC_RefPos));
		rpc.Register<ZPackage>("PlayerList", new Action<ZRpc, ZPackage>(this.RPC_PlayerList));
		rpc.Register<string>("RemotePrint", new Action<ZRpc, string>(this.RPC_RemotePrint));
		if (ZNet.m_isServer)
		{
			rpc.Register<ZDOID>("CharacterID", new Action<ZRpc, ZDOID>(this.RPC_CharacterID));
			rpc.Register<string>("Kick", new Action<ZRpc, string>(this.RPC_Kick));
			rpc.Register<string>("Ban", new Action<ZRpc, string>(this.RPC_Ban));
			rpc.Register<string>("Unban", new Action<ZRpc, string>(this.RPC_Unban));
			rpc.Register("Save", new ZRpc.RpcMethod.Method(this.RPC_Save));
			rpc.Register("PrintBanned", new ZRpc.RpcMethod.Method(this.RPC_PrintBanned));
		}
		else
		{
			rpc.Register<double>("NetTime", new Action<ZRpc, double>(this.RPC_NetTime));
		}
		if (ZNet.m_isServer)
		{
			this.SendPeerInfo(rpc, "");
			peer.m_socket.VersionMatch();
			this.SendPlayerList();
		}
		else
		{
			peer.m_socket.VersionMatch();
			ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connected;
		}
		this.m_zdoMan.AddPeer(peer);
		this.m_routedRpc.AddPeer(peer);
	}

	// Token: 0x06000E20 RID: 3616 RVA: 0x00061B40 File Offset: 0x0005FD40
	private void SendDisconnect()
	{
		ZLog.Log("Sending disconnect msg");
		foreach (ZNetPeer peer in this.m_peers)
		{
			this.SendDisconnect(peer);
		}
	}

	// Token: 0x06000E21 RID: 3617 RVA: 0x00061BA0 File Offset: 0x0005FDA0
	private void SendDisconnect(ZNetPeer peer)
	{
		if (peer.m_rpc != null)
		{
			ZLog.Log("Sent to " + peer.m_socket.GetEndPointString());
			peer.m_rpc.Invoke("Disconnect", Array.Empty<object>());
		}
	}

	// Token: 0x06000E22 RID: 3618 RVA: 0x00061BDC File Offset: 0x0005FDDC
	private void RPC_Disconnect(ZRpc rpc)
	{
		ZLog.Log("RPC_Disconnect");
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer != null)
		{
			if (peer.m_server)
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorDisconnected;
			}
			this.Disconnect(peer);
		}
	}

	// Token: 0x06000E23 RID: 3619 RVA: 0x00061C14 File Offset: 0x0005FE14
	private void RPC_Error(ZRpc rpc, int error)
	{
		ZNet.ConnectionStatus connectionStatus = (ZNet.ConnectionStatus)error;
		ZNet.m_connectionStatus = connectionStatus;
		ZLog.Log("Got connectoin error msg " + connectionStatus.ToString());
	}

	// Token: 0x06000E24 RID: 3620 RVA: 0x00061C48 File Offset: 0x0005FE48
	public bool IsConnected(long uid)
	{
		if (uid == ZNet.GetUID())
		{
			return true;
		}
		using (List<ZNetPeer>.Enumerator enumerator = this.m_peers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_uid == uid)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000E25 RID: 3621 RVA: 0x00061CAC File Offset: 0x0005FEAC
	private void ClearPlayerData(ZNetPeer peer)
	{
		this.m_routedRpc.RemovePeer(peer);
		this.m_zdoMan.RemovePeer(peer);
	}

	// Token: 0x06000E26 RID: 3622 RVA: 0x00061CC6 File Offset: 0x0005FEC6
	public void Disconnect(ZNetPeer peer)
	{
		this.ClearPlayerData(peer);
		this.m_peers.Remove(peer);
		peer.Dispose();
		if (ZNet.m_isServer)
		{
			this.SendPlayerList();
		}
	}

	// Token: 0x06000E27 RID: 3623 RVA: 0x00061CEF File Offset: 0x0005FEEF
	private void FixedUpdate()
	{
		this.UpdateNetTime(Time.fixedDeltaTime);
	}

	// Token: 0x06000E28 RID: 3624 RVA: 0x00061CFC File Offset: 0x0005FEFC
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		ZSteamSocket.UpdateAllSockets(deltaTime);
		ZPlayFabSocket.UpdateAllSockets(deltaTime);
		if (this.IsServer())
		{
			this.UpdateBanList(deltaTime);
		}
		this.CheckForIncommingServerConnections();
		this.UpdatePeers(deltaTime);
		this.SendPeriodicData(deltaTime);
		this.m_zdoMan.Update(deltaTime);
		this.UpdateSave();
		this.UpdatePassword();
		if (ZNet.PeersToDisconnectAfterKick.Count < 1)
		{
			return;
		}
		foreach (ZNetPeer znetPeer in ZNet.PeersToDisconnectAfterKick.Keys.ToArray<ZNetPeer>())
		{
			if (Time.time >= ZNet.PeersToDisconnectAfterKick[znetPeer])
			{
				this.Disconnect(znetPeer);
				ZNet.PeersToDisconnectAfterKick.Remove(znetPeer);
			}
		}
	}

	// Token: 0x06000E29 RID: 3625 RVA: 0x00061DAB File Offset: 0x0005FFAB
	private void LateUpdate()
	{
		ZPlayFabSocket.LateUpdateAllSocket();
	}

	// Token: 0x06000E2A RID: 3626 RVA: 0x00061DB2 File Offset: 0x0005FFB2
	private void UpdateNetTime(float dt)
	{
		if (this.IsServer())
		{
			if (this.GetNrOfPlayers() > 0)
			{
				this.m_netTime += (double)dt;
				return;
			}
		}
		else
		{
			this.m_netTime += (double)dt;
		}
	}

	// Token: 0x06000E2B RID: 3627 RVA: 0x00061DE4 File Offset: 0x0005FFE4
	private void UpdateBanList(float dt)
	{
		this.m_banlistTimer += dt;
		if (this.m_banlistTimer > 5f)
		{
			this.m_banlistTimer = 0f;
			this.CheckWhiteList();
			foreach (string user in this.m_bannedList.GetList())
			{
				this.InternalKick(user);
			}
		}
	}

	// Token: 0x06000E2C RID: 3628 RVA: 0x00061E68 File Offset: 0x00060068
	private void CheckWhiteList()
	{
		if (this.m_permittedList.Count() == 0)
		{
			return;
		}
		bool flag = false;
		while (!flag)
		{
			flag = true;
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					string hostName = znetPeer.m_socket.GetHostName();
					if (!this.ListContainsId(this.m_permittedList, hostName))
					{
						ZLog.Log("Kicking player not in permitted list " + znetPeer.m_playerName + " host: " + hostName);
						this.InternalKick(znetPeer);
						flag = false;
						break;
					}
				}
			}
		}
	}

	// Token: 0x06000E2D RID: 3629 RVA: 0x00061F14 File Offset: 0x00060114
	public bool IsSaving()
	{
		return this.m_saveThread != null;
	}

	// Token: 0x06000E2E RID: 3630 RVA: 0x00061F20 File Offset: 0x00060120
	public void ConsoleSave()
	{
		if (this.IsServer())
		{
			this.RPC_Save(null);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Save", Array.Empty<object>());
		}
		Game.instance.SavePlayerProfile(false);
	}

	// Token: 0x06000E2F RID: 3631 RVA: 0x00061F64 File Offset: 0x00060164
	private void RPC_Save(ZRpc rpc)
	{
		if (!base.enabled)
		{
			return;
		}
		if (rpc != null && !this.ListContainsId(this.m_adminList, rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.RemotePrint(rpc, "Saving..");
		Game.instance.SavePlayerProfile(false);
		this.Save(false);
	}

	// Token: 0x06000E30 RID: 3632 RVA: 0x00061FC4 File Offset: 0x000601C4
	private bool ListContainsId(SyncedList list, string id)
	{
		if (id.StartsWith(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam)))
		{
			return list.Contains(id) || list.Contains(id.Substring(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam).Length));
		}
		if (!id.Contains("_"))
		{
			return list.Contains(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam) + id) || list.Contains(id);
		}
		return list.Contains(id);
	}

	// Token: 0x06000E31 RID: 3633 RVA: 0x00062034 File Offset: 0x00060234
	public void Save(bool sync)
	{
		Game.instance.m_saveTimer = 0f;
		if (ZNet.m_loadError || ZoneSystem.instance.SkipSaving() || DungeonDB.instance.SkipSaving())
		{
			ZLog.LogWarning("Skipping world save");
			return;
		}
		if (ZNet.m_isServer && ZNet.m_world != null)
		{
			this.SaveWorld(sync);
		}
	}

	// Token: 0x06000E32 RID: 3634 RVA: 0x0006208F File Offset: 0x0006028F
	public static World GetWorldIfIsHost()
	{
		if (ZNet.m_isServer)
		{
			return ZNet.m_world;
		}
		return null;
	}

	// Token: 0x06000E33 RID: 3635 RVA: 0x000620A0 File Offset: 0x000602A0
	private void SendPeriodicData(float dt)
	{
		this.m_periodicSendTimer += dt;
		if (this.m_periodicSendTimer >= 2f)
		{
			this.m_periodicSendTimer = 0f;
			if (this.IsServer())
			{
				this.SendNetTime();
				this.SendPlayerList();
				return;
			}
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					znetPeer.m_rpc.Invoke("RefPos", new object[]
					{
						this.m_referencePosition,
						this.m_publicReferencePosition
					});
				}
			}
		}
	}

	// Token: 0x06000E34 RID: 3636 RVA: 0x00062164 File Offset: 0x00060364
	private void SendNetTime()
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady())
			{
				znetPeer.m_rpc.Invoke("NetTime", new object[]
				{
					this.m_netTime
				});
			}
		}
	}

	// Token: 0x06000E35 RID: 3637 RVA: 0x000621DC File Offset: 0x000603DC
	private void RPC_NetTime(ZRpc rpc, double time)
	{
		this.m_netTime = time;
	}

	// Token: 0x06000E36 RID: 3638 RVA: 0x000621E8 File Offset: 0x000603E8
	private void RPC_RefPos(ZRpc rpc, Vector3 pos, bool publicRefPos)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer != null)
		{
			peer.m_refPos = pos;
			peer.m_publicRefPos = publicRefPos;
		}
	}

	// Token: 0x06000E37 RID: 3639 RVA: 0x00062210 File Offset: 0x00060410
	private void UpdatePeers(float dt)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (!znetPeer.m_rpc.IsConnected())
			{
				if (znetPeer.m_server)
				{
					if (ZNet.m_externalError != ZNet.ConnectionStatus.None)
					{
						ZNet.m_connectionStatus = ZNet.m_externalError;
					}
					else if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connecting)
					{
						ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorConnectFailed;
					}
					else
					{
						ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorDisconnected;
					}
				}
				this.Disconnect(znetPeer);
				break;
			}
		}
		ZNetPeer[] array = this.m_peers.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].m_rpc.Update(dt) == ZRpc.ErrorCode.IncompatibleVersion)
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorVersion;
			}
		}
	}

	// Token: 0x06000E38 RID: 3640 RVA: 0x000622D4 File Offset: 0x000604D4
	private void CheckForIncommingServerConnections()
	{
		if (this.m_hostSocket == null)
		{
			return;
		}
		ISocket socket = this.m_hostSocket.Accept();
		if (socket != null)
		{
			if (!socket.IsConnected())
			{
				socket.Dispose();
				return;
			}
			ZNetPeer peer = new ZNetPeer(socket, false);
			this.OnNewConnection(peer);
		}
	}

	// Token: 0x06000E39 RID: 3641 RVA: 0x00062318 File Offset: 0x00060518
	public ZNetPeer GetPeerByPlayerName(string name)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady() && znetPeer.m_playerName == name)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000E3A RID: 3642 RVA: 0x00062384 File Offset: 0x00060584
	public ZNetPeer GetPeerByHostName(string endpoint)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady() && znetPeer.m_socket.GetHostName() == endpoint)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000E3B RID: 3643 RVA: 0x000623F4 File Offset: 0x000605F4
	public ZNetPeer GetPeer(long uid)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.m_uid == uid)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000E3C RID: 3644 RVA: 0x00062450 File Offset: 0x00060650
	private ZNetPeer GetPeer(ZRpc rpc)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.m_rpc == rpc)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000E3D RID: 3645 RVA: 0x000624AC File Offset: 0x000606AC
	public List<ZNetPeer> GetConnectedPeers()
	{
		return new List<ZNetPeer>(this.m_peers);
	}

	// Token: 0x06000E3E RID: 3646 RVA: 0x000624BC File Offset: 0x000606BC
	private void SaveWorld(bool sync)
	{
		if (this.m_saveThread != null && this.m_saveThread.IsAlive)
		{
			this.m_saveThread.Join();
			this.m_saveThread = null;
		}
		this.m_saveStartTime = Time.realtimeSinceStartup;
		this.m_zdoMan.PrepareSave();
		ZoneSystem.instance.PrepareSave();
		RandEventSystem.instance.PrepareSave();
		ZNet.m_backupCount = PlatformPrefs.GetInt("AutoBackups", ZNet.m_backupCount);
		this.m_saveThreadStartTime = Time.realtimeSinceStartup;
		this.m_saveThread = new Thread(new ThreadStart(this.SaveWorldThread));
		this.m_saveThread.Start();
		if (sync)
		{
			this.m_saveThread.Join();
			this.m_saveThread = null;
		}
	}

	// Token: 0x06000E3F RID: 3647 RVA: 0x00062570 File Offset: 0x00060770
	private void UpdateSave()
	{
		if (this.m_saveThread != null && !this.m_saveThread.IsAlive)
		{
			this.m_saveThread = null;
			float num = this.m_saveThreadStartTime - this.m_saveStartTime;
			float num2 = Time.realtimeSinceStartup - this.m_saveThreadStartTime;
			if (this.m_saveExceededCloudQuota)
			{
				this.m_saveExceededCloudQuota = false;
				MessageHud.instance.MessageAll(MessageHud.MessageType.TopLeft, string.Concat(new string[]
				{
					"$msg_worldsavedcloudstoragefull ( ",
					num.ToString("0.00"),
					"+",
					num2.ToString("0.00"),
					"s )"
				}));
				return;
			}
			MessageHud.instance.MessageAll(MessageHud.MessageType.TopLeft, string.Concat(new string[]
			{
				"$msg_worldsaved ( ",
				num.ToString("0.00"),
				"+",
				num2.ToString("0.00"),
				"s )"
			}));
		}
	}

	// Token: 0x06000E40 RID: 3648 RVA: 0x00062664 File Offset: 0x00060864
	private void SaveWorldThread()
	{
		DateTime now = DateTime.Now;
		try
		{
			ulong num = 52428800UL;
			num += FileHelpers.GetFileSize(ZNet.m_world.GetMetaPath(), ZNet.m_world.m_fileSource);
			if (FileHelpers.Exists(ZNet.m_world.GetDBPath(), ZNet.m_world.m_fileSource))
			{
				num += FileHelpers.GetFileSize(ZNet.m_world.GetDBPath(), ZNet.m_world.m_fileSource);
			}
			bool flag = SaveSystem.CheckMove(ZNet.m_world.m_fileName, SaveDataType.World, ref ZNet.m_world.m_fileSource, now, num);
			bool flag2 = ZNet.m_world.m_createBackupBeforeSaving && !flag;
			if (FileHelpers.m_cloudEnabled && ZNet.m_world.m_fileSource == FileHelpers.FileSource.Cloud)
			{
				num *= (flag2 ? 3UL : 2UL);
				if (FileHelpers.OperationExceedsCloudCapacity(num))
				{
					string metaPath = ZNet.m_world.GetMetaPath();
					string dbpath = ZNet.m_world.GetDBPath();
					ZNet.m_world.m_fileSource = FileHelpers.FileSource.Local;
					string metaPath2 = ZNet.m_world.GetMetaPath();
					string dbpath2 = ZNet.m_world.GetDBPath();
					FileHelpers.FileCopyOutFromCloud(metaPath, metaPath2, true);
					if (FileHelpers.FileExistsCloud(dbpath))
					{
						FileHelpers.FileCopyOutFromCloud(dbpath, dbpath2, true);
					}
					SaveSystem.InvalidateCache();
					this.m_saveExceededCloudQuota = true;
					ZLog.LogWarning("The world save operation may exceed the cloud save quota and it has therefore been moved to local storage!");
				}
			}
			if (flag2)
			{
				SaveWithBackups saveWithBackups;
				if (SaveSystem.TryGetSaveByName(ZNet.m_world.m_fileName, SaveDataType.World, out saveWithBackups) && !saveWithBackups.IsDeleted)
				{
					if (SaveSystem.CreateBackup(saveWithBackups.PrimaryFile, DateTime.Now, ZNet.m_world.m_fileSource))
					{
						ZLog.Log("Migrating world save from an old save format, created backup!");
					}
					else
					{
						ZLog.LogError("Failed to create backup of world save " + ZNet.m_world.m_fileName + "!");
					}
				}
				else
				{
					ZLog.LogError("Failed to get world save " + ZNet.m_world.m_fileName + " from save system, so a backup couldn't be created!");
				}
			}
			ZNet.m_world.m_createBackupBeforeSaving = false;
			DateTime now2 = DateTime.Now;
			bool flag3 = ZNet.m_world.m_fileSource != FileHelpers.FileSource.Cloud;
			string dbpath3 = ZNet.m_world.GetDBPath();
			string text = flag3 ? (dbpath3 + ".new") : dbpath3;
			string oldFile = dbpath3 + ".old";
			ZLog.Log("World save writing starting");
			FileWriter fileWriter = new FileWriter(text, FileHelpers.FileHelperType.Binary, ZNet.m_world.m_fileSource);
			ZLog.Log("World save writing started");
			BinaryWriter binary = fileWriter.m_binary;
			binary.Write(31);
			binary.Write(this.m_netTime);
			this.m_zdoMan.SaveAsync(binary);
			ZoneSystem.instance.SaveASync(binary);
			RandEventSystem.instance.SaveAsync(binary);
			ZLog.Log("World save writing finishing");
			fileWriter.Finish();
			SaveSystem.InvalidateCache();
			ZLog.Log("World save writing finished");
			ZNet.m_world.m_needsDB = true;
			bool flag4;
			FileWriter fileWriter2;
			ZNet.m_world.SaveWorldMetaData(now, false, out flag4, out fileWriter2);
			if (ZNet.m_world.m_fileSource == FileHelpers.FileSource.Cloud && (fileWriter2.Status == FileWriter.WriterStatus.CloseFailed || fileWriter.Status == FileWriter.WriterStatus.CloseFailed))
			{
				string text2 = ZNet.<SaveWorldThread>g__GetBackupPath|61_0(ZNet.m_world.GetMetaPath(FileHelpers.FileSource.Local), now);
				string text3 = ZNet.<SaveWorldThread>g__GetBackupPath|61_0(ZNet.m_world.GetDBPath(FileHelpers.FileSource.Local), now);
				fileWriter2.DumpCloudWriteToLocalFile(text2);
				fileWriter.DumpCloudWriteToLocalFile(text3);
				SaveSystem.InvalidateCache();
				string text4 = "";
				if (fileWriter2.Status == FileWriter.WriterStatus.CloseFailed)
				{
					text4 = text4 + "Cloud save to location \"" + ZNet.m_world.GetMetaPath() + "\" failed!\n";
				}
				if (fileWriter.Status == FileWriter.WriterStatus.CloseFailed)
				{
					text4 = text4 + "Cloud save to location \"" + dbpath3 + "\" failed!\n ";
				}
				text4 = string.Concat(new string[]
				{
					text4,
					"Saved world as local backup \"",
					text2,
					"\" and \"",
					text3,
					"\". Use the \"Manage saves\" menu to restore this backup."
				});
				ZLog.LogError(text4);
			}
			else
			{
				if (flag3)
				{
					FileHelpers.ReplaceOldFile(dbpath3, text, oldFile, ZNet.m_world.m_fileSource);
					SaveSystem.InvalidateCache();
				}
				ZLog.Log("World saved ( " + (DateTime.Now - now2).TotalMilliseconds.ToString() + "ms )");
				now2 = DateTime.Now;
				if (ZNet.ConsiderAutoBackup(ZNet.m_world.m_fileName, SaveDataType.World, now))
				{
					ZLog.Log("World auto backup saved ( " + (DateTime.Now - now2).ToString() + "ms )");
				}
			}
		}
		catch (Exception ex)
		{
			ZLog.LogError("Error saving world! " + ex.Message);
			Terminal.m_threadSafeMessages.Enqueue("Error saving world! See log or console.");
			Terminal.m_threadSafeConsoleLog.Enqueue("Error saving world! " + ex.Message);
		}
	}

	// Token: 0x06000E41 RID: 3649 RVA: 0x00062B08 File Offset: 0x00060D08
	public static bool ConsiderAutoBackup(string saveName, SaveDataType dataType, DateTime now)
	{
		int num = 1200;
		int num2 = (ZNet.m_backupCount == 1) ? 0 : ZNet.m_backupCount;
		string s;
		int num3;
		string s2;
		int num4;
		string s3;
		int num5;
		return num2 > 0 && SaveSystem.ConsiderBackup(saveName, dataType, now, num2, (Terminal.m_testList.TryGetValue("autoshort", out s) && int.TryParse(s, out num3)) ? num3 : ZNet.m_backupShort, (Terminal.m_testList.TryGetValue("autolong", out s2) && int.TryParse(s2, out num4)) ? num4 : ZNet.m_backupLong, (Terminal.m_testList.TryGetValue("autowait", out s3) && int.TryParse(s3, out num5)) ? num5 : num, ZoneSystem.instance ? ZoneSystem.instance.TimeSinceStart() : 0f);
	}

	// Token: 0x06000E42 RID: 3650 RVA: 0x00062BCC File Offset: 0x00060DCC
	private void LoadWorld()
	{
		ZLog.Log(string.Concat(new string[]
		{
			"Load world: ",
			ZNet.m_world.m_name,
			" (",
			ZNet.m_world.m_fileName,
			")"
		}));
		string dbpath = ZNet.m_world.GetDBPath();
		FileReader fileReader;
		try
		{
			fileReader = new FileReader(dbpath, ZNet.m_world.m_fileSource, FileHelpers.FileHelperType.Binary);
		}
		catch
		{
			ZLog.Log("  missing " + dbpath);
			return;
		}
		BinaryReader binary = fileReader.m_binary;
		try
		{
			int num;
			if (!this.CheckDataVersion(binary, out num))
			{
				ZLog.Log("  incompatible data version " + num.ToString());
				ZNet.m_loadError = true;
				binary.Close();
				fileReader.Dispose();
				return;
			}
			if (num >= 4)
			{
				this.m_netTime = binary.ReadDouble();
			}
			this.m_zdoMan.Load(binary, num);
			if (num >= 12)
			{
				ZoneSystem.instance.Load(binary, num);
			}
			if (num >= 15)
			{
				RandEventSystem.instance.Load(binary, num);
			}
			fileReader.Dispose();
		}
		catch (Exception ex)
		{
			ZLog.LogError("Exception while loading world " + dbpath + ":" + ex.ToString());
			ZNet.m_loadError = true;
		}
		Game.instance.CollectResources(false);
	}

	// Token: 0x06000E43 RID: 3651 RVA: 0x00062D20 File Offset: 0x00060F20
	private bool CheckDataVersion(BinaryReader reader, out int version)
	{
		version = reader.ReadInt32();
		return global::Version.IsWorldVersionCompatible(version);
	}

	// Token: 0x06000E44 RID: 3652 RVA: 0x00062D36 File Offset: 0x00060F36
	public int GetHostPort()
	{
		if (this.m_hostSocket != null)
		{
			return this.m_hostSocket.GetHostPort();
		}
		return 0;
	}

	// Token: 0x06000E45 RID: 3653 RVA: 0x00062D4D File Offset: 0x00060F4D
	public static long GetUID()
	{
		return ZDOMan.GetSessionID();
	}

	// Token: 0x06000E46 RID: 3654 RVA: 0x00062D54 File Offset: 0x00060F54
	public long GetWorldUID()
	{
		return ZNet.m_world.m_uid;
	}

	// Token: 0x06000E47 RID: 3655 RVA: 0x00062D60 File Offset: 0x00060F60
	public string GetWorldName()
	{
		if (ZNet.m_world != null)
		{
			return ZNet.m_world.m_name;
		}
		return null;
	}

	// Token: 0x06000E48 RID: 3656 RVA: 0x00062D75 File Offset: 0x00060F75
	public void SetCharacterID(ZDOID id)
	{
		this.m_characterID = id;
		if (!ZNet.m_isServer)
		{
			this.m_peers[0].m_rpc.Invoke("CharacterID", new object[]
			{
				id
			});
		}
	}

	// Token: 0x06000E49 RID: 3657 RVA: 0x00062DB0 File Offset: 0x00060FB0
	private void RPC_CharacterID(ZRpc rpc, ZDOID characterID)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer != null)
		{
			peer.m_characterID = characterID;
			string str = "Got character ZDOID from ";
			string playerName = peer.m_playerName;
			string str2 = " : ";
			ZDOID zdoid = characterID;
			ZLog.Log(str + playerName + str2 + zdoid.ToString());
		}
	}

	// Token: 0x06000E4A RID: 3658 RVA: 0x00062DF8 File Offset: 0x00060FF8
	public void SetPublicReferencePosition(bool pub)
	{
		this.m_publicReferencePosition = pub;
	}

	// Token: 0x06000E4B RID: 3659 RVA: 0x00062E01 File Offset: 0x00061001
	public bool IsReferencePositionPublic()
	{
		return this.m_publicReferencePosition;
	}

	// Token: 0x06000E4C RID: 3660 RVA: 0x00062E09 File Offset: 0x00061009
	public void SetReferencePosition(Vector3 pos)
	{
		this.m_referencePosition = pos;
	}

	// Token: 0x06000E4D RID: 3661 RVA: 0x00062E12 File Offset: 0x00061012
	public Vector3 GetReferencePosition()
	{
		return this.m_referencePosition;
	}

	// Token: 0x06000E4E RID: 3662 RVA: 0x00062E1C File Offset: 0x0006101C
	public List<ZDO> GetAllCharacterZDOS()
	{
		List<ZDO> list = new List<ZDO>();
		ZDO zdo = this.m_zdoMan.GetZDO(this.m_characterID);
		if (zdo != null)
		{
			list.Add(zdo);
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady() && !znetPeer.m_characterID.IsNone())
			{
				ZDO zdo2 = this.m_zdoMan.GetZDO(znetPeer.m_characterID);
				if (zdo2 != null)
				{
					list.Add(zdo2);
				}
			}
		}
		return list;
	}

	// Token: 0x06000E4F RID: 3663 RVA: 0x00062EC0 File Offset: 0x000610C0
	public int GetPeerConnections()
	{
		int num = 0;
		for (int i = 0; i < this.m_peers.Count; i++)
		{
			if (this.m_peers[i].IsReady())
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000E50 RID: 3664 RVA: 0x00062EFD File Offset: 0x000610FD
	public ZNat GetZNat()
	{
		return this.m_nat;
	}

	// Token: 0x06000E51 RID: 3665 RVA: 0x00062F08 File Offset: 0x00061108
	public static void SetServer(bool server, bool openServer, bool publicServer, string serverName, string password, World world)
	{
		ZNet.m_isServer = server;
		ZNet.m_openServer = openServer;
		ZNet.m_publicServer = publicServer;
		ZNet.m_serverPassword = (string.IsNullOrEmpty(password) ? "" : ZNet.HashPassword(password, ZNet.ServerPasswordSalt()));
		ZNet.m_ServerName = serverName;
		ZNet.m_world = world;
	}

	// Token: 0x06000E52 RID: 3666 RVA: 0x00062F58 File Offset: 0x00061158
	private static string HashPassword(string password, string salt)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(password + salt);
		byte[] bytes2 = new MD5CryptoServiceProvider().ComputeHash(bytes);
		return Encoding.ASCII.GetString(bytes2);
	}

	// Token: 0x06000E53 RID: 3667 RVA: 0x00062F8E File Offset: 0x0006118E
	public static void ResetServerHost()
	{
		ZNet.m_serverPlayFabPlayerId = null;
		ZNet.m_serverSteamID = 0UL;
		ZNet.m_serverHost = "";
		ZNet.m_serverHostPort = 0;
	}

	// Token: 0x06000E54 RID: 3668 RVA: 0x00062FAD File Offset: 0x000611AD
	public static bool HasServerHost()
	{
		return ZNet.m_serverHost != "" || ZNet.m_serverPlayFabPlayerId != null || ZNet.m_serverSteamID > 0UL;
	}

	// Token: 0x06000E55 RID: 3669 RVA: 0x00062FD8 File Offset: 0x000611D8
	public static void SetServerHost(string remotePlayerId)
	{
		ZNet.ResetServerHost();
		ZNet.m_serverPlayFabPlayerId = remotePlayerId;
		ZNet.m_onlineBackend = OnlineBackendType.PlayFab;
	}

	// Token: 0x06000E56 RID: 3670 RVA: 0x00062FEB File Offset: 0x000611EB
	public static void SetServerHost(ulong serverID)
	{
		ZNet.ResetServerHost();
		ZNet.m_serverSteamID = serverID;
		ZNet.m_onlineBackend = OnlineBackendType.Steamworks;
	}

	// Token: 0x06000E57 RID: 3671 RVA: 0x00062FFE File Offset: 0x000611FE
	public static void SetServerHost(string host, int port, OnlineBackendType backend)
	{
		ZNet.ResetServerHost();
		ZNet.m_serverHost = host;
		ZNet.m_serverHostPort = port;
		ZNet.m_onlineBackend = backend;
	}

	// Token: 0x06000E58 RID: 3672 RVA: 0x00063018 File Offset: 0x00061218
	public static string GetServerString(bool includeBackend = true)
	{
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			return (includeBackend ? "playfab/" : "") + ZNet.m_serverPlayFabPlayerId;
		}
		if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
		{
			return string.Concat(new string[]
			{
				includeBackend ? "steam/" : "",
				ZNet.m_serverSteamID.ToString(),
				"/",
				ZNet.m_serverHost,
				":",
				ZNet.m_serverHostPort.ToString()
			});
		}
		return (includeBackend ? "socket/" : "") + ZNet.m_serverHost + ":" + ZNet.m_serverHostPort.ToString();
	}

	// Token: 0x06000E59 RID: 3673 RVA: 0x000630C6 File Offset: 0x000612C6
	public bool IsServer()
	{
		return ZNet.m_isServer;
	}

	// Token: 0x06000E5A RID: 3674 RVA: 0x0000247B File Offset: 0x0000067B
	public bool IsDedicated()
	{
		return false;
	}

	// Token: 0x17000095 RID: 149
	// (get) Token: 0x06000E5B RID: 3675 RVA: 0x000630CD File Offset: 0x000612CD
	public static bool IsSinglePlayer
	{
		get
		{
			return ZNet.m_isServer && !ZNet.m_openServer;
		}
	}

	// Token: 0x06000E5C RID: 3676 RVA: 0x000630E0 File Offset: 0x000612E0
	private void UpdatePlayerList()
	{
		this.m_players.Clear();
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
		{
			ZNet.PlayerInfo playerInfo = default(ZNet.PlayerInfo);
			playerInfo.m_name = Game.instance.GetPlayerProfile().GetName();
			playerInfo.m_host = "";
			if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
			{
				playerInfo.m_host = PrivilegeManager.GetNetworkUserId();
			}
			playerInfo.m_characterID = this.m_characterID;
			playerInfo.m_publicPosition = this.m_publicReferencePosition;
			if (playerInfo.m_publicPosition)
			{
				playerInfo.m_position = this.m_referencePosition;
			}
			this.m_players.Add(playerInfo);
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady())
			{
				ZNet.PlayerInfo playerInfo2 = new ZNet.PlayerInfo
				{
					m_characterID = znetPeer.m_characterID,
					m_name = znetPeer.m_playerName,
					m_host = znetPeer.m_socket.GetHostName(),
					m_publicPosition = znetPeer.m_publicRefPos
				};
				if (playerInfo2.m_publicPosition)
				{
					playerInfo2.m_position = znetPeer.m_refPos;
				}
				this.m_players.Add(playerInfo2);
			}
		}
	}

	// Token: 0x06000E5D RID: 3677 RVA: 0x00063224 File Offset: 0x00061424
	private void SendPlayerList()
	{
		this.UpdatePlayerList();
		if (this.m_peers.Count > 0)
		{
			ZPackage zpackage = new ZPackage();
			zpackage.Write(this.m_players.Count);
			foreach (ZNet.PlayerInfo playerInfo in this.m_players)
			{
				zpackage.Write(playerInfo.m_name);
				zpackage.Write(playerInfo.m_host);
				zpackage.Write(playerInfo.m_characterID);
				zpackage.Write(playerInfo.m_publicPosition);
				if (playerInfo.m_publicPosition)
				{
					zpackage.Write(playerInfo.m_position);
				}
			}
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					znetPeer.m_rpc.Invoke("PlayerList", new object[]
					{
						zpackage
					});
				}
			}
		}
	}

	// Token: 0x06000E5E RID: 3678 RVA: 0x00063344 File Offset: 0x00061544
	private void RPC_PlayerList(ZRpc rpc, ZPackage pkg)
	{
		this.m_players.Clear();
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			ZNet.PlayerInfo playerInfo = new ZNet.PlayerInfo
			{
				m_name = pkg.ReadString(),
				m_host = pkg.ReadString(),
				m_characterID = pkg.ReadZDOID(),
				m_publicPosition = pkg.ReadBool()
			};
			if (playerInfo.m_publicPosition)
			{
				playerInfo.m_position = pkg.ReadVector3();
			}
			this.m_players.Add(playerInfo);
		}
	}

	// Token: 0x06000E5F RID: 3679 RVA: 0x000633CC File Offset: 0x000615CC
	public List<ZNet.PlayerInfo> GetPlayerList()
	{
		return this.m_players;
	}

	// Token: 0x17000096 RID: 150
	// (get) Token: 0x06000E60 RID: 3680 RVA: 0x000633D4 File Offset: 0x000615D4
	public ZDOID LocalPlayerCharacterID
	{
		get
		{
			return this.m_characterID;
		}
	}

	// Token: 0x06000E61 RID: 3681 RVA: 0x000633DC File Offset: 0x000615DC
	public void GetOtherPublicPlayers(List<ZNet.PlayerInfo> playerList)
	{
		foreach (ZNet.PlayerInfo playerInfo in this.m_players)
		{
			if (playerInfo.m_publicPosition)
			{
				ZDOID characterID = playerInfo.m_characterID;
				if (!characterID.IsNone() && !(playerInfo.m_characterID == this.m_characterID))
				{
					playerList.Add(playerInfo);
				}
			}
		}
	}

	// Token: 0x06000E62 RID: 3682 RVA: 0x0006345C File Offset: 0x0006165C
	public int GetNrOfPlayers()
	{
		return this.m_players.Count;
	}

	// Token: 0x06000E63 RID: 3683 RVA: 0x0006346C File Offset: 0x0006166C
	public void GetNetStats(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
		if (this.IsServer())
		{
			int num = 0;
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					num++;
					float num2;
					float num3;
					int num4;
					float num5;
					float num6;
					znetPeer.m_socket.GetConnectionQuality(out num2, out num3, out num4, out num5, out num6);
					localQuality += num2;
					remoteQuality += num3;
					ping += num4;
					outByteSec += num5;
					inByteSec += num6;
				}
			}
			if (num > 0)
			{
				localQuality /= (float)num;
				remoteQuality /= (float)num;
				ping /= num;
			}
			return;
		}
		if (ZNet.m_connectionStatus != ZNet.ConnectionStatus.Connected)
		{
			return;
		}
		foreach (ZNetPeer znetPeer2 in this.m_peers)
		{
			if (znetPeer2.IsReady())
			{
				znetPeer2.m_socket.GetConnectionQuality(out localQuality, out remoteQuality, out ping, out outByteSec, out inByteSec);
				break;
			}
		}
	}

	// Token: 0x06000E64 RID: 3684 RVA: 0x000635AC File Offset: 0x000617AC
	public void SetNetTime(double time)
	{
		this.m_netTime = time;
	}

	// Token: 0x06000E65 RID: 3685 RVA: 0x000635B8 File Offset: 0x000617B8
	public DateTime GetTime()
	{
		long ticks = (long)(this.m_netTime * 1000.0 * 10000.0);
		return new DateTime(ticks);
	}

	// Token: 0x06000E66 RID: 3686 RVA: 0x000635E7 File Offset: 0x000617E7
	public float GetWrappedDayTimeSeconds()
	{
		return (float)(this.m_netTime % 86400.0);
	}

	// Token: 0x06000E67 RID: 3687 RVA: 0x000635FA File Offset: 0x000617FA
	public double GetTimeSeconds()
	{
		return this.m_netTime;
	}

	// Token: 0x06000E68 RID: 3688 RVA: 0x00063602 File Offset: 0x00061802
	public static ZNet.ConnectionStatus GetConnectionStatus()
	{
		if (ZNet.m_instance != null && ZNet.m_instance.IsServer())
		{
			return ZNet.ConnectionStatus.Connected;
		}
		if (ZNet.m_externalError != ZNet.ConnectionStatus.None)
		{
			ZNet.m_connectionStatus = ZNet.m_externalError;
		}
		return ZNet.m_connectionStatus;
	}

	// Token: 0x06000E69 RID: 3689 RVA: 0x00063635 File Offset: 0x00061835
	public bool HasBadConnection()
	{
		return this.GetServerPing() > this.m_badConnectionPing;
	}

	// Token: 0x06000E6A RID: 3690 RVA: 0x00063648 File Offset: 0x00061848
	public float GetServerPing()
	{
		if (this.IsServer())
		{
			return 0f;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connecting || ZNet.m_connectionStatus == ZNet.ConnectionStatus.None)
		{
			return 0f;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connected)
		{
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					return znetPeer.m_rpc.GetTimeSinceLastPing();
				}
			}
		}
		return 0f;
	}

	// Token: 0x06000E6B RID: 3691 RVA: 0x000636DC File Offset: 0x000618DC
	public ZNetPeer GetServerPeer()
	{
		if (this.IsServer())
		{
			return null;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connecting || ZNet.m_connectionStatus == ZNet.ConnectionStatus.None)
		{
			return null;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connected)
		{
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					return znetPeer;
				}
			}
		}
		return null;
	}

	// Token: 0x06000E6C RID: 3692 RVA: 0x0006375C File Offset: 0x0006195C
	public ZRpc GetServerRPC()
	{
		ZNetPeer serverPeer = this.GetServerPeer();
		if (serverPeer != null)
		{
			return serverPeer.m_rpc;
		}
		return null;
	}

	// Token: 0x06000E6D RID: 3693 RVA: 0x0006377B File Offset: 0x0006197B
	public List<ZNetPeer> GetPeers()
	{
		return this.m_peers;
	}

	// Token: 0x06000E6E RID: 3694 RVA: 0x00063783 File Offset: 0x00061983
	public void RemotePrint(ZRpc rpc, string text)
	{
		if (rpc == null)
		{
			if (global::Console.instance)
			{
				global::Console.instance.Print(text);
				return;
			}
		}
		else
		{
			rpc.Invoke("RemotePrint", new object[]
			{
				text
			});
		}
	}

	// Token: 0x06000E6F RID: 3695 RVA: 0x000637B5 File Offset: 0x000619B5
	private void RPC_RemotePrint(ZRpc rpc, string text)
	{
		if (global::Console.instance)
		{
			global::Console.instance.Print(text);
		}
	}

	// Token: 0x06000E70 RID: 3696 RVA: 0x000637D0 File Offset: 0x000619D0
	public void Kick(string user)
	{
		if (this.IsServer())
		{
			this.InternalKick(user);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Kick", new object[]
			{
				user
			});
		}
	}

	// Token: 0x06000E71 RID: 3697 RVA: 0x0006380C File Offset: 0x00061A0C
	private void RPC_Kick(ZRpc rpc, string user)
	{
		if (!this.ListContainsId(this.m_adminList, rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.RemotePrint(rpc, "Kicking user " + user);
		this.InternalKick(user);
	}

	// Token: 0x06000E72 RID: 3698 RVA: 0x00063858 File Offset: 0x00061A58
	private void RPC_Kicked(ZRpc rpc)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer == null || !peer.m_server)
		{
			return;
		}
		ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorKicked;
		this.Disconnect(peer);
	}

	// Token: 0x06000E73 RID: 3699 RVA: 0x00063888 File Offset: 0x00061A88
	private void InternalKick(string user)
	{
		if (user == "")
		{
			return;
		}
		ZNetPeer znetPeer = null;
		if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
		{
			if (user.StartsWith(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam)))
			{
				znetPeer = this.GetPeerByHostName(user.Substring(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam).Length));
			}
			else if (!user.Contains("_"))
			{
				znetPeer = this.GetPeerByHostName(user);
			}
		}
		else if (!user.Contains("_"))
		{
			znetPeer = this.GetPeerByHostName(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam) + user);
		}
		else
		{
			znetPeer = this.GetPeerByHostName(user);
		}
		if (znetPeer == null)
		{
			znetPeer = this.GetPeerByPlayerName(user);
		}
		if (znetPeer != null)
		{
			this.InternalKick(znetPeer);
		}
	}

	// Token: 0x06000E74 RID: 3700 RVA: 0x0006392C File Offset: 0x00061B2C
	private void InternalKick(ZNetPeer peer)
	{
		if (!this.IsServer() || peer == null || ZNet.PeersToDisconnectAfterKick.ContainsKey(peer))
		{
			return;
		}
		ZLog.Log("Kicking " + peer.m_playerName);
		peer.m_rpc.Invoke("Kicked", Array.Empty<object>());
		ZNet.PeersToDisconnectAfterKick[peer] = Time.time + 1f;
	}

	// Token: 0x06000E75 RID: 3701 RVA: 0x00063994 File Offset: 0x00061B94
	private bool IsAllowed(string hostName, string playerName)
	{
		return !this.ListContainsId(this.m_bannedList, hostName) && !this.m_bannedList.Contains(playerName) && (this.m_permittedList.Count() <= 0 || this.ListContainsId(this.m_permittedList, hostName));
	}

	// Token: 0x06000E76 RID: 3702 RVA: 0x000639E0 File Offset: 0x00061BE0
	public void Ban(string user)
	{
		if (this.IsServer())
		{
			this.InternalBan(null, user);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Ban", new object[]
			{
				user
			});
		}
	}

	// Token: 0x06000E77 RID: 3703 RVA: 0x00063A1D File Offset: 0x00061C1D
	private void RPC_Ban(ZRpc rpc, string user)
	{
		if (!this.ListContainsId(this.m_adminList, rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.InternalBan(rpc, user);
	}

	// Token: 0x06000E78 RID: 3704 RVA: 0x00063A50 File Offset: 0x00061C50
	private void InternalBan(ZRpc rpc, string user)
	{
		if (!this.IsServer())
		{
			return;
		}
		if (user == "")
		{
			return;
		}
		ZNetPeer peerByPlayerName = this.GetPeerByPlayerName(user);
		if (peerByPlayerName != null)
		{
			user = peerByPlayerName.m_socket.GetHostName();
		}
		this.RemotePrint(rpc, "Banning user " + user);
		this.m_bannedList.Add(user);
	}

	// Token: 0x06000E79 RID: 3705 RVA: 0x00063AAC File Offset: 0x00061CAC
	public void Unban(string user)
	{
		if (this.IsServer())
		{
			this.InternalUnban(null, user);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Unban", new object[]
			{
				user
			});
		}
	}

	// Token: 0x06000E7A RID: 3706 RVA: 0x00063AE9 File Offset: 0x00061CE9
	private void RPC_Unban(ZRpc rpc, string user)
	{
		if (!this.ListContainsId(this.m_adminList, rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.InternalUnban(rpc, user);
	}

	// Token: 0x06000E7B RID: 3707 RVA: 0x00063B19 File Offset: 0x00061D19
	private void InternalUnban(ZRpc rpc, string user)
	{
		if (!this.IsServer())
		{
			return;
		}
		if (user == "")
		{
			return;
		}
		this.RemotePrint(rpc, "Unbanning user " + user);
		this.m_bannedList.Remove(user);
	}

	// Token: 0x17000097 RID: 151
	// (get) Token: 0x06000E7C RID: 3708 RVA: 0x00063B50 File Offset: 0x00061D50
	public List<string> Banned
	{
		get
		{
			return this.m_bannedList.GetList();
		}
	}

	// Token: 0x06000E7D RID: 3709 RVA: 0x00063B60 File Offset: 0x00061D60
	public void PrintBanned()
	{
		if (this.IsServer())
		{
			this.InternalPrintBanned(null);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("PrintBanned", Array.Empty<object>());
		}
	}

	// Token: 0x06000E7E RID: 3710 RVA: 0x00063B97 File Offset: 0x00061D97
	private void RPC_PrintBanned(ZRpc rpc)
	{
		if (!this.ListContainsId(this.m_adminList, rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.InternalPrintBanned(rpc);
	}

	// Token: 0x06000E7F RID: 3711 RVA: 0x00063BC8 File Offset: 0x00061DC8
	private void InternalPrintBanned(ZRpc rpc)
	{
		this.RemotePrint(rpc, "Banned users");
		List<string> list = this.m_bannedList.GetList();
		if (list.Count == 0)
		{
			this.RemotePrint(rpc, "-");
		}
		else
		{
			for (int i = 0; i < list.Count; i++)
			{
				this.RemotePrint(rpc, i.ToString() + ": " + list[i]);
			}
		}
		this.RemotePrint(rpc, "");
		this.RemotePrint(rpc, "Permitted users");
		List<string> list2 = this.m_permittedList.GetList();
		if (list2.Count == 0)
		{
			this.RemotePrint(rpc, "All");
			return;
		}
		for (int j = 0; j < list2.Count; j++)
		{
			this.RemotePrint(rpc, j.ToString() + ": " + list2[j]);
		}
	}

	// Token: 0x06000E80 RID: 3712 RVA: 0x00063C9C File Offset: 0x00061E9C
	private static string ServerPasswordSalt()
	{
		if (ZNet.m_serverPasswordSalt.Length == 0)
		{
			byte[] array = new byte[16];
			RandomNumberGenerator.Create().GetBytes(array);
			ZNet.m_serverPasswordSalt = Encoding.ASCII.GetString(array);
		}
		return ZNet.m_serverPasswordSalt;
	}

	// Token: 0x06000E81 RID: 3713 RVA: 0x00063CDD File Offset: 0x00061EDD
	public static void SetExternalError(ZNet.ConnectionStatus error)
	{
		ZNet.m_externalError = error;
	}

	// Token: 0x17000098 RID: 152
	// (get) Token: 0x06000E82 RID: 3714 RVA: 0x00063CE5 File Offset: 0x00061EE5
	public bool HaveStopped
	{
		get
		{
			return this.m_haveStoped;
		}
	}

	// Token: 0x06000E85 RID: 3717 RVA: 0x00063DFC File Offset: 0x00061FFC
	[CompilerGenerated]
	internal static string <GetPublicIP>g__DownloadString|6_0(string downloadUrl, int timeoutMS)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
		httpWebRequest.Timeout = timeoutMS;
		httpWebRequest.ReadWriteTimeout = timeoutMS;
		string result;
		try
		{
			result = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream()).ReadToEnd();
		}
		catch (Exception ex)
		{
			ZLog.Log("Exception while waiting for respons from " + downloadUrl + " -> " + ex.ToString());
			result = "";
		}
		return result;
	}

	// Token: 0x06000E86 RID: 3718 RVA: 0x00063E78 File Offset: 0x00062078
	[CompilerGenerated]
	private void <OnSteamServerRegistered>g__RetryRegisterAfterDelay|7_0(float delay)
	{
		base.StartCoroutine(this.<OnSteamServerRegistered>g__DelayThenRegisterCoroutine|7_1(delay));
	}

	// Token: 0x06000E87 RID: 3719 RVA: 0x00063E88 File Offset: 0x00062088
	[CompilerGenerated]
	private IEnumerator <OnSteamServerRegistered>g__DelayThenRegisterCoroutine|7_1(float delay)
	{
		ZLog.Log(string.Format("Steam register server failed! Retrying in {0}s, total attempts: {1}", delay, this.m_registerAttempts));
		DateTime NextRetryUtc = DateTime.UtcNow + TimeSpan.FromSeconds((double)delay);
		while (DateTime.UtcNow < NextRetryUtc)
		{
			yield return null;
		}
		bool password = ZNet.m_serverPassword != "";
		string gameVersion = global::Version.CurrentVersion.ToString();
		uint networkVersion = 5U;
		ZSteamMatchmaking.instance.RegisterServer(ZNet.m_ServerName, password, gameVersion, networkVersion, ZNet.m_publicServer, ZNet.m_world.m_seedName, new ZSteamMatchmaking.ServerRegistered(this.OnSteamServerRegistered));
		yield break;
	}

	// Token: 0x06000E88 RID: 3720 RVA: 0x00063EA0 File Offset: 0x000620A0
	[CompilerGenerated]
	internal static string <SaveWorldThread>g__GetBackupPath|61_0(string filePath, DateTime now)
	{
		string text;
		string text2;
		string text3;
		FileHelpers.SplitFilePath(filePath, out text, out text2, out text3);
		return string.Concat(new string[]
		{
			text,
			text2,
			"_backup_cloud-",
			now.ToString("yyyyMMdd-HHmmss"),
			text3
		});
	}

	// Token: 0x0400100D RID: 4109
	private float m_banlistTimer;

	// Token: 0x0400100E RID: 4110
	private static ZNet m_instance;

	// Token: 0x0400100F RID: 4111
	public const int ServerPlayerLimit = 10;

	// Token: 0x04001010 RID: 4112
	public int m_hostPort = 2456;

	// Token: 0x04001011 RID: 4113
	public RectTransform m_passwordDialog;

	// Token: 0x04001012 RID: 4114
	public RectTransform m_connectingDialog;

	// Token: 0x04001013 RID: 4115
	public float m_badConnectionPing = 5f;

	// Token: 0x04001014 RID: 4116
	public int m_zdoSectorsWidth = 512;

	// Token: 0x04001015 RID: 4117
	private ZConnector2 m_serverConnector;

	// Token: 0x04001016 RID: 4118
	private ISocket m_hostSocket;

	// Token: 0x04001017 RID: 4119
	private List<ZNetPeer> m_peers = new List<ZNetPeer>();

	// Token: 0x04001018 RID: 4120
	private Thread m_saveThread;

	// Token: 0x04001019 RID: 4121
	private bool m_saveExceededCloudQuota;

	// Token: 0x0400101A RID: 4122
	private float m_saveStartTime;

	// Token: 0x0400101B RID: 4123
	private float m_saveThreadStartTime;

	// Token: 0x0400101C RID: 4124
	public static bool m_loadError = false;

	// Token: 0x0400101D RID: 4125
	private ZDOMan m_zdoMan;

	// Token: 0x0400101E RID: 4126
	private ZRoutedRpc m_routedRpc;

	// Token: 0x0400101F RID: 4127
	private ZNat m_nat;

	// Token: 0x04001020 RID: 4128
	private double m_netTime = 2040.0;

	// Token: 0x04001021 RID: 4129
	private ZDOID m_characterID = ZDOID.None;

	// Token: 0x04001022 RID: 4130
	private Vector3 m_referencePosition = Vector3.zero;

	// Token: 0x04001023 RID: 4131
	private bool m_publicReferencePosition;

	// Token: 0x04001024 RID: 4132
	private float m_periodicSendTimer;

	// Token: 0x04001025 RID: 4133
	public static int m_backupCount = 2;

	// Token: 0x04001026 RID: 4134
	public static int m_backupShort = 7200;

	// Token: 0x04001027 RID: 4135
	public static int m_backupLong = 43200;

	// Token: 0x04001028 RID: 4136
	private bool m_haveStoped;

	// Token: 0x04001029 RID: 4137
	private static bool m_isServer = true;

	// Token: 0x0400102A RID: 4138
	private static World m_world = null;

	// Token: 0x0400102B RID: 4139
	private int m_registerAttempts;

	// Token: 0x0400102C RID: 4140
	public static OnlineBackendType m_onlineBackend = OnlineBackendType.Steamworks;

	// Token: 0x0400102D RID: 4141
	private static string m_serverPlayFabPlayerId = null;

	// Token: 0x0400102E RID: 4142
	private static ulong m_serverSteamID = 0UL;

	// Token: 0x0400102F RID: 4143
	private static string m_serverHost = "";

	// Token: 0x04001030 RID: 4144
	private static int m_serverHostPort = 0;

	// Token: 0x04001031 RID: 4145
	private static bool m_openServer = true;

	// Token: 0x04001032 RID: 4146
	private static bool m_publicServer = true;

	// Token: 0x04001033 RID: 4147
	private static string m_serverPassword = "";

	// Token: 0x04001034 RID: 4148
	private static string m_serverPasswordSalt = "";

	// Token: 0x04001035 RID: 4149
	private static string m_ServerName = "";

	// Token: 0x04001036 RID: 4150
	private static ZNet.ConnectionStatus m_connectionStatus = ZNet.ConnectionStatus.None;

	// Token: 0x04001037 RID: 4151
	private static ZNet.ConnectionStatus m_externalError = ZNet.ConnectionStatus.None;

	// Token: 0x04001038 RID: 4152
	private SyncedList m_adminList;

	// Token: 0x04001039 RID: 4153
	private SyncedList m_bannedList;

	// Token: 0x0400103A RID: 4154
	private SyncedList m_permittedList;

	// Token: 0x0400103B RID: 4155
	private List<ZNet.PlayerInfo> m_players = new List<ZNet.PlayerInfo>();

	// Token: 0x0400103C RID: 4156
	private ZRpc m_tempPasswordRPC;

	// Token: 0x0400103D RID: 4157
	private static readonly Dictionary<ZNetPeer, float> PeersToDisconnectAfterKick = new Dictionary<ZNetPeer, float>();

	// Token: 0x02000168 RID: 360
	public enum ConnectionStatus
	{
		// Token: 0x0400103F RID: 4159
		None,
		// Token: 0x04001040 RID: 4160
		Connecting,
		// Token: 0x04001041 RID: 4161
		Connected,
		// Token: 0x04001042 RID: 4162
		ErrorVersion,
		// Token: 0x04001043 RID: 4163
		ErrorDisconnected,
		// Token: 0x04001044 RID: 4164
		ErrorConnectFailed,
		// Token: 0x04001045 RID: 4165
		ErrorPassword,
		// Token: 0x04001046 RID: 4166
		ErrorAlreadyConnected,
		// Token: 0x04001047 RID: 4167
		ErrorBanned,
		// Token: 0x04001048 RID: 4168
		ErrorFull,
		// Token: 0x04001049 RID: 4169
		ErrorPlatformExcluded,
		// Token: 0x0400104A RID: 4170
		ErrorCrossplayPrivilege,
		// Token: 0x0400104B RID: 4171
		ErrorKicked
	}

	// Token: 0x02000169 RID: 361
	public struct PlayerInfo
	{
		// Token: 0x0400104C RID: 4172
		public string m_name;

		// Token: 0x0400104D RID: 4173
		public string m_host;

		// Token: 0x0400104E RID: 4174
		public ZDOID m_characterID;

		// Token: 0x0400104F RID: 4175
		public bool m_publicPosition;

		// Token: 0x04001050 RID: 4176
		public Vector3 m_position;
	}
}
