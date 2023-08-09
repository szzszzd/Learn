using System;
using System.Collections.Generic;

// Token: 0x0200017A RID: 378
public class ZRoutedRpc
{
	// Token: 0x1700009D RID: 157
	// (get) Token: 0x06000F28 RID: 3880 RVA: 0x00065F67 File Offset: 0x00064167
	public static ZRoutedRpc instance
	{
		get
		{
			return ZRoutedRpc.s_instance;
		}
	}

	// Token: 0x06000F29 RID: 3881 RVA: 0x00065F6E File Offset: 0x0006416E
	public ZRoutedRpc(bool server)
	{
		ZRoutedRpc.s_instance = this;
		this.m_server = server;
	}

	// Token: 0x06000F2A RID: 3882 RVA: 0x00065FA0 File Offset: 0x000641A0
	public void SetUID(long uid)
	{
		this.m_id = uid;
	}

	// Token: 0x06000F2B RID: 3883 RVA: 0x00065FAC File Offset: 0x000641AC
	public void AddPeer(ZNetPeer peer)
	{
		this.m_peers.Add(peer);
		peer.m_rpc.Register<ZPackage>("RoutedRPC", new Action<ZRpc, ZPackage>(this.RPC_RoutedRPC));
		if (this.m_onNewPeer != null)
		{
			this.m_onNewPeer(peer.m_uid);
		}
	}

	// Token: 0x06000F2C RID: 3884 RVA: 0x00065FFA File Offset: 0x000641FA
	public void RemovePeer(ZNetPeer peer)
	{
		this.m_peers.Remove(peer);
	}

	// Token: 0x06000F2D RID: 3885 RVA: 0x0006600C File Offset: 0x0006420C
	private ZNetPeer GetPeer(long uid)
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

	// Token: 0x06000F2E RID: 3886 RVA: 0x00066068 File Offset: 0x00064268
	public void InvokeRoutedRPC(long targetPeerID, string methodName, params object[] parameters)
	{
		this.InvokeRoutedRPC(targetPeerID, ZDOID.None, methodName, parameters);
	}

	// Token: 0x06000F2F RID: 3887 RVA: 0x00066078 File Offset: 0x00064278
	public void InvokeRoutedRPC(string methodName, params object[] parameters)
	{
		this.InvokeRoutedRPC(this.GetServerPeerID(), methodName, parameters);
	}

	// Token: 0x06000F30 RID: 3888 RVA: 0x00066088 File Offset: 0x00064288
	private long GetServerPeerID()
	{
		if (this.m_server)
		{
			return this.m_id;
		}
		if (this.m_peers.Count > 0)
		{
			return this.m_peers[0].m_uid;
		}
		return 0L;
	}

	// Token: 0x06000F31 RID: 3889 RVA: 0x000660BC File Offset: 0x000642BC
	public void InvokeRoutedRPC(long targetPeerID, ZDOID targetZDO, string methodName, params object[] parameters)
	{
		ZRoutedRpc.RoutedRPCData routedRPCData = new ZRoutedRpc.RoutedRPCData();
		ZRoutedRpc.RoutedRPCData routedRPCData2 = routedRPCData;
		long id = this.m_id;
		int rpcMsgID = this.m_rpcMsgID;
		this.m_rpcMsgID = rpcMsgID + 1;
		routedRPCData2.m_msgID = id + (long)rpcMsgID;
		routedRPCData.m_senderPeerID = this.m_id;
		routedRPCData.m_targetPeerID = targetPeerID;
		routedRPCData.m_targetZDO = targetZDO;
		routedRPCData.m_methodHash = methodName.GetStableHashCode();
		ZRpc.Serialize(parameters, ref routedRPCData.m_parameters);
		routedRPCData.m_parameters.SetPos(0);
		if (targetPeerID == this.m_id || targetPeerID == 0L)
		{
			this.HandleRoutedRPC(routedRPCData);
		}
		if (targetPeerID != this.m_id)
		{
			this.RouteRPC(routedRPCData);
		}
	}

	// Token: 0x06000F32 RID: 3890 RVA: 0x00066150 File Offset: 0x00064350
	private void RouteRPC(ZRoutedRpc.RoutedRPCData rpcData)
	{
		ZPackage zpackage = new ZPackage();
		rpcData.Serialize(zpackage);
		if (this.m_server)
		{
			if (rpcData.m_targetPeerID != 0L)
			{
				ZNetPeer peer = this.GetPeer(rpcData.m_targetPeerID);
				if (peer != null && peer.IsReady())
				{
					peer.m_rpc.Invoke("RoutedRPC", new object[]
					{
						zpackage
					});
					return;
				}
				return;
			}
			else
			{
				using (List<ZNetPeer>.Enumerator enumerator = this.m_peers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ZNetPeer znetPeer = enumerator.Current;
						if (rpcData.m_senderPeerID != znetPeer.m_uid && znetPeer.IsReady())
						{
							znetPeer.m_rpc.Invoke("RoutedRPC", new object[]
							{
								zpackage
							});
						}
					}
					return;
				}
			}
		}
		foreach (ZNetPeer znetPeer2 in this.m_peers)
		{
			if (znetPeer2.IsReady())
			{
				znetPeer2.m_rpc.Invoke("RoutedRPC", new object[]
				{
					zpackage
				});
			}
		}
	}

	// Token: 0x06000F33 RID: 3891 RVA: 0x00066288 File Offset: 0x00064488
	private void RPC_RoutedRPC(ZRpc rpc, ZPackage pkg)
	{
		ZRoutedRpc.RoutedRPCData routedRPCData = new ZRoutedRpc.RoutedRPCData();
		routedRPCData.Deserialize(pkg);
		if (routedRPCData.m_targetPeerID == this.m_id || routedRPCData.m_targetPeerID == 0L)
		{
			this.HandleRoutedRPC(routedRPCData);
		}
		if (this.m_server && routedRPCData.m_targetPeerID != this.m_id)
		{
			this.RouteRPC(routedRPCData);
		}
	}

	// Token: 0x06000F34 RID: 3892 RVA: 0x000662DC File Offset: 0x000644DC
	private void HandleRoutedRPC(ZRoutedRpc.RoutedRPCData data)
	{
		if (data.m_targetZDO.IsNone())
		{
			RoutedMethodBase routedMethodBase;
			if (this.m_functions.TryGetValue(data.m_methodHash, out routedMethodBase))
			{
				routedMethodBase.Invoke(data.m_senderPeerID, data.m_parameters);
				return;
			}
		}
		else
		{
			ZDO zdo = ZDOMan.instance.GetZDO(data.m_targetZDO);
			if (zdo != null)
			{
				ZNetView znetView = ZNetScene.instance.FindInstance(zdo);
				if (znetView != null)
				{
					znetView.HandleRoutedRPC(data);
				}
			}
		}
	}

	// Token: 0x06000F35 RID: 3893 RVA: 0x0006634E File Offset: 0x0006454E
	public void Register(string name, Action<long> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod(f));
	}

	// Token: 0x06000F36 RID: 3894 RVA: 0x00066367 File Offset: 0x00064567
	public void Register<T>(string name, Action<long, T> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T>(f));
	}

	// Token: 0x06000F37 RID: 3895 RVA: 0x00066380 File Offset: 0x00064580
	public void Register<T, U>(string name, Action<long, T, U> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U>(f));
	}

	// Token: 0x06000F38 RID: 3896 RVA: 0x00066399 File Offset: 0x00064599
	public void Register<T, U, V>(string name, Action<long, T, U, V> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V>(f));
	}

	// Token: 0x06000F39 RID: 3897 RVA: 0x000663B2 File Offset: 0x000645B2
	public void Register<T, U, V, B>(string name, RoutedMethod<T, U, V, B>.Method f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V, B>(f));
	}

	// Token: 0x06000F3A RID: 3898 RVA: 0x000663CB File Offset: 0x000645CB
	public void Register<T, U, V, B, K>(string name, RoutedMethod<T, U, V, B, K>.Method f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V, B, K>(f));
	}

	// Token: 0x04001088 RID: 4232
	public static long Everybody;

	// Token: 0x04001089 RID: 4233
	public Action<long> m_onNewPeer;

	// Token: 0x0400108A RID: 4234
	private int m_rpcMsgID = 1;

	// Token: 0x0400108B RID: 4235
	private bool m_server;

	// Token: 0x0400108C RID: 4236
	private long m_id;

	// Token: 0x0400108D RID: 4237
	private readonly List<ZNetPeer> m_peers = new List<ZNetPeer>();

	// Token: 0x0400108E RID: 4238
	private readonly Dictionary<int, RoutedMethodBase> m_functions = new Dictionary<int, RoutedMethodBase>();

	// Token: 0x0400108F RID: 4239
	private static ZRoutedRpc s_instance;

	// Token: 0x0200017B RID: 379
	public class RoutedRPCData
	{
		// Token: 0x06000F3C RID: 3900 RVA: 0x000663E4 File Offset: 0x000645E4
		public void Serialize(ZPackage pkg)
		{
			pkg.Write(this.m_msgID);
			pkg.Write(this.m_senderPeerID);
			pkg.Write(this.m_targetPeerID);
			pkg.Write(this.m_targetZDO);
			pkg.Write(this.m_methodHash);
			pkg.Write(this.m_parameters);
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x0006643C File Offset: 0x0006463C
		public void Deserialize(ZPackage pkg)
		{
			this.m_msgID = pkg.ReadLong();
			this.m_senderPeerID = pkg.ReadLong();
			this.m_targetPeerID = pkg.ReadLong();
			this.m_targetZDO = pkg.ReadZDOID();
			this.m_methodHash = pkg.ReadInt();
			this.m_parameters = pkg.ReadPackage();
		}

		// Token: 0x04001090 RID: 4240
		public long m_msgID;

		// Token: 0x04001091 RID: 4241
		public long m_senderPeerID;

		// Token: 0x04001092 RID: 4242
		public long m_targetPeerID;

		// Token: 0x04001093 RID: 4243
		public ZDOID m_targetZDO;

		// Token: 0x04001094 RID: 4244
		public int m_methodHash;

		// Token: 0x04001095 RID: 4245
		public ZPackage m_parameters = new ZPackage();
	}
}
