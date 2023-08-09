using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

// Token: 0x0200015E RID: 350
public class ZDOMan
{
	// Token: 0x17000093 RID: 147
	// (get) Token: 0x06000DA2 RID: 3490 RVA: 0x0005D14A File Offset: 0x0005B34A
	public static ZDOMan instance
	{
		get
		{
			return ZDOMan.s_instance;
		}
	}

	// Token: 0x06000DA3 RID: 3491 RVA: 0x0005D154 File Offset: 0x0005B354
	public ZDOMan(int width)
	{
		ZDOMan.s_instance = this;
		ZRoutedRpc.instance.Register<ZPackage>("DestroyZDO", new Action<long, ZPackage>(this.RPC_DestroyZDO));
		ZRoutedRpc.instance.Register<ZDOID>("RequestZDO", new Action<long, ZDOID>(this.RPC_RequestZDO));
		this.m_width = width;
		this.m_halfWidth = this.m_width / 2;
		this.ResetSectorArray();
	}

	// Token: 0x06000DA4 RID: 3492 RVA: 0x0005D25B File Offset: 0x0005B45B
	private void ResetSectorArray()
	{
		this.m_objectsBySector = new List<ZDO>[this.m_width * this.m_width];
		this.m_objectsByOutsideSector.Clear();
	}

	// Token: 0x06000DA5 RID: 3493 RVA: 0x0005D280 File Offset: 0x0005B480
	public void ShutDown()
	{
		if (!ZNet.instance.IsServer())
		{
			this.FlushClientObjects();
		}
		ZDOPool.Release(this.m_objectsByID);
		this.m_objectsByID.Clear();
		this.m_tempToSync.Clear();
		this.m_tempToSyncDistant.Clear();
		this.m_tempNearObjects.Clear();
		this.m_tempRemoveList.Clear();
		this.m_peers.Clear();
		this.ResetSectorArray();
		Game.instance.CollectResources(false);
	}

	// Token: 0x06000DA6 RID: 3494 RVA: 0x0005D300 File Offset: 0x0005B500
	public void PrepareSave()
	{
		this.m_saveData = new ZDOMan.SaveData();
		this.m_saveData.m_sessionID = this.m_sessionID;
		this.m_saveData.m_nextUid = this.m_nextUid;
		Stopwatch stopwatch = Stopwatch.StartNew();
		this.m_saveData.m_zdos = this.GetSaveClone();
		ZLog.Log("PrepareSave: clone done in " + stopwatch.ElapsedMilliseconds.ToString() + "ms");
		stopwatch = Stopwatch.StartNew();
		ZDOExtraData.PrepareSave();
		ZLog.Log("PrepareSave: ZDOExtraData.PrepareSave done in " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
	}

	// Token: 0x06000DA7 RID: 3495 RVA: 0x0005D3A0 File Offset: 0x0005B5A0
	public void SaveAsync(BinaryWriter writer)
	{
		writer.Write(this.m_saveData.m_sessionID);
		writer.Write(this.m_saveData.m_nextUid);
		ZPackage zpackage = new ZPackage();
		writer.Write(this.m_saveData.m_zdos.Count);
		zpackage.SetWriter(writer);
		foreach (ZDO zdo in this.m_saveData.m_zdos)
		{
			zdo.Save(zpackage);
		}
		ZLog.Log("Saved " + this.m_saveData.m_zdos.Count.ToString() + " ZDOs");
		foreach (ZDO zdo2 in this.m_saveData.m_zdos)
		{
			zdo2.Reset();
		}
		this.m_saveData.m_zdos.Clear();
		this.m_saveData = null;
		ZDOExtraData.ClearSave();
	}

	// Token: 0x06000DA8 RID: 3496 RVA: 0x0005D4C8 File Offset: 0x0005B6C8
	public void Load(BinaryReader reader, int version)
	{
		reader.ReadInt64();
		uint nextUid = reader.ReadUInt32();
		int num = reader.ReadInt32();
		ZDOPool.Release(this.m_objectsByID);
		this.m_objectsByID.Clear();
		this.ResetSectorArray();
		ZLog.Log(string.Concat(new string[]
		{
			"Loading ",
			num.ToString(),
			" zdos, my sessionID: ",
			this.m_sessionID.ToString(),
			", data version: ",
			version.ToString()
		}));
		List<ZDO> list = new List<ZDO>();
		list.Capacity = num;
		ZLog.Log("Creating ZDOs");
		for (int i = 0; i < num; i++)
		{
			ZDO item = ZDOPool.Create();
			list.Add(item);
		}
		ZLog.Log("Loading in ZDOs");
		ZPackage zpackage = new ZPackage();
		if (version < 31)
		{
			using (List<ZDO>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZDO zdo = enumerator.Current;
					zdo.m_uid = new ZDOID(reader);
					int count = reader.ReadInt32();
					byte[] data = reader.ReadBytes(count);
					zpackage.Load(data);
					zdo.LoadOldFormat(zpackage, version);
					zdo.SetOwner(0L);
				}
				goto IL_16A;
			}
		}
		zpackage.SetReader(reader);
		foreach (ZDO zdo2 in list)
		{
			zdo2.Load(zpackage, version);
		}
		nextUid = (uint)(list.Count + 1);
		IL_16A:
		ZLog.Log("Adding to Dictionary");
		foreach (ZDO zdo3 in list)
		{
			this.m_objectsByID.Add(zdo3.m_uid, zdo3);
			if (zdo3.GetPrefab() == Game.instance.PortalPrefabHash)
			{
				this.m_portalObjects.Add(zdo3);
			}
		}
		ZLog.Log("Adding to Sectors");
		foreach (ZDO zdo4 in list)
		{
			this.AddToSector(zdo4, zdo4.GetSector());
		}
		if (version < 31)
		{
			ZLog.Log("Converting Ships & Fishing-rods ownership");
			this.ConvertOwnerships(list);
			ZLog.Log("Converting & mapping CreationTime");
			this.ConvertCreationTime(list);
			ZLog.Log("Converting portals");
			this.ConvertPortals();
			ZLog.Log("Converting spawners");
			this.ConvertSpawners();
			ZLog.Log("Converting ZSyncTransforms");
			this.ConvertSyncTransforms();
			ZLog.Log("Converting ItemSeeds");
			this.ConvertSeed();
		}
		else
		{
			ZLog.Log("Connecting Portals, Spawners & ZSyncTransforms");
			this.ConnectPortals();
			this.ConnectSpawners();
			this.ConnectSyncTransforms();
		}
		Game.instance.ConnectPortals();
		this.m_deadZDOs.Clear();
		if (version < 31)
		{
			int num2 = reader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				reader.ReadInt64();
				reader.ReadUInt32();
				reader.ReadInt64();
			}
		}
		this.m_nextUid = nextUid;
	}

	// Token: 0x06000DA9 RID: 3497 RVA: 0x0005D7F8 File Offset: 0x0005B9F8
	public ZDO CreateNewZDO(Vector3 position, int prefabHash)
	{
		long sessionID = this.m_sessionID;
		uint nextUid = this.m_nextUid;
		this.m_nextUid = nextUid + 1U;
		ZDOID zdoid = new ZDOID(sessionID, nextUid);
		while (this.GetZDO(zdoid) != null)
		{
			long sessionID2 = this.m_sessionID;
			nextUid = this.m_nextUid;
			this.m_nextUid = nextUid + 1U;
			zdoid = new ZDOID(sessionID2, nextUid);
		}
		return this.CreateNewZDO(zdoid, position, prefabHash);
	}

	// Token: 0x06000DAA RID: 3498 RVA: 0x0005D858 File Offset: 0x0005BA58
	private ZDO CreateNewZDO(ZDOID uid, Vector3 position, int prefabHashIn = 0)
	{
		ZDO zdo = ZDOPool.Create(uid, position);
		zdo.SetOwnerInternal(this.m_sessionID);
		this.m_objectsByID.Add(uid, zdo);
		if (((prefabHashIn != 0) ? prefabHashIn : zdo.GetPrefab()) == Game.instance.PortalPrefabHash)
		{
			this.m_portalObjects.Add(zdo);
		}
		return zdo;
	}

	// Token: 0x06000DAB RID: 3499 RVA: 0x0005D8AC File Offset: 0x0005BAAC
	public void AddToSector(ZDO zdo, Vector2i sector)
	{
		int num = this.SectorToIndex(sector);
		if (num >= 0)
		{
			if (this.m_objectsBySector[num] != null)
			{
				this.m_objectsBySector[num].Add(zdo);
				return;
			}
			List<ZDO> list = new List<ZDO>();
			list.Add(zdo);
			this.m_objectsBySector[num] = list;
			return;
		}
		else
		{
			List<ZDO> list2;
			if (this.m_objectsByOutsideSector.TryGetValue(sector, out list2))
			{
				list2.Add(zdo);
				return;
			}
			list2 = new List<ZDO>();
			list2.Add(zdo);
			this.m_objectsByOutsideSector.Add(sector, list2);
			return;
		}
	}

	// Token: 0x06000DAC RID: 3500 RVA: 0x0005D928 File Offset: 0x0005BB28
	public void ZDOSectorInvalidated(ZDO zdo)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			zdopeer.ZDOSectorInvalidated(zdo);
		}
	}

	// Token: 0x06000DAD RID: 3501 RVA: 0x0005D97C File Offset: 0x0005BB7C
	public void RemoveFromSector(ZDO zdo, Vector2i sector)
	{
		int num = this.SectorToIndex(sector);
		List<ZDO> list;
		if (num >= 0)
		{
			if (this.m_objectsBySector[num] != null)
			{
				this.m_objectsBySector[num].Remove(zdo);
				return;
			}
		}
		else if (this.m_objectsByOutsideSector.TryGetValue(sector, out list))
		{
			list.Remove(zdo);
		}
	}

	// Token: 0x06000DAE RID: 3502 RVA: 0x0005D9C8 File Offset: 0x0005BBC8
	public ZDO GetZDO(ZDOID id)
	{
		if (id == ZDOID.None)
		{
			return null;
		}
		ZDO result;
		if (this.m_objectsByID.TryGetValue(id, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06000DAF RID: 3503 RVA: 0x0005D9F8 File Offset: 0x0005BBF8
	public void AddPeer(ZNetPeer netPeer)
	{
		ZDOMan.ZDOPeer zdopeer = new ZDOMan.ZDOPeer();
		zdopeer.m_peer = netPeer;
		this.m_peers.Add(zdopeer);
		zdopeer.m_peer.m_rpc.Register<ZPackage>("ZDOData", new Action<ZRpc, ZPackage>(this.RPC_ZDOData));
	}

	// Token: 0x06000DB0 RID: 3504 RVA: 0x0005DA40 File Offset: 0x0005BC40
	public void RemovePeer(ZNetPeer netPeer)
	{
		ZDOMan.ZDOPeer zdopeer = this.FindPeer(netPeer);
		if (zdopeer != null)
		{
			this.m_peers.Remove(zdopeer);
			if (ZNet.instance.IsServer())
			{
				this.RemoveOrphanNonPersistentZDOS();
			}
		}
	}

	// Token: 0x06000DB1 RID: 3505 RVA: 0x0005DA78 File Offset: 0x0005BC78
	private ZDOMan.ZDOPeer FindPeer(ZNetPeer netPeer)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			if (zdopeer.m_peer == netPeer)
			{
				return zdopeer;
			}
		}
		return null;
	}

	// Token: 0x06000DB2 RID: 3506 RVA: 0x0005DAD4 File Offset: 0x0005BCD4
	private ZDOMan.ZDOPeer FindPeer(ZRpc rpc)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			if (zdopeer.m_peer.m_rpc == rpc)
			{
				return zdopeer;
			}
		}
		return null;
	}

	// Token: 0x06000DB3 RID: 3507 RVA: 0x0005DB38 File Offset: 0x0005BD38
	public void Update(float dt)
	{
		if (ZNet.instance.IsServer())
		{
			this.ReleaseZDOS(dt);
		}
		this.SendZDOToPeers2(dt);
		this.SendDestroyed();
		this.UpdateStats(dt);
	}

	// Token: 0x06000DB4 RID: 3508 RVA: 0x0005DB64 File Offset: 0x0005BD64
	private void UpdateStats(float dt)
	{
		this.m_statTimer += dt;
		if (this.m_statTimer >= 1f)
		{
			this.m_statTimer = 0f;
			this.m_zdosSentLastSec = this.m_zdosSent;
			this.m_zdosRecvLastSec = this.m_zdosRecv;
			this.m_zdosRecv = 0;
			this.m_zdosSent = 0;
		}
	}

	// Token: 0x06000DB5 RID: 3509 RVA: 0x0005DBC0 File Offset: 0x0005BDC0
	private void SendZDOToPeers2(float dt)
	{
		if (this.m_peers.Count == 0)
		{
			return;
		}
		this.m_sendTimer += dt;
		if (this.m_nextSendPeer < 0)
		{
			if (this.m_sendTimer > 0.05f)
			{
				this.m_nextSendPeer = 0;
				this.m_sendTimer = 0f;
				return;
			}
		}
		else
		{
			if (this.m_nextSendPeer < this.m_peers.Count)
			{
				this.SendZDOs(this.m_peers[this.m_nextSendPeer], false);
			}
			this.m_nextSendPeer++;
			if (this.m_nextSendPeer >= this.m_peers.Count)
			{
				this.m_nextSendPeer = -1;
			}
		}
	}

	// Token: 0x06000DB6 RID: 3510 RVA: 0x0005DC68 File Offset: 0x0005BE68
	private void FlushClientObjects()
	{
		foreach (ZDOMan.ZDOPeer peer in this.m_peers)
		{
			this.SendAllZDOs(peer);
		}
	}

	// Token: 0x06000DB7 RID: 3511 RVA: 0x0005DCBC File Offset: 0x0005BEBC
	private void ReleaseZDOS(float dt)
	{
		this.m_releaseZDOTimer += dt;
		if (this.m_releaseZDOTimer > 2f)
		{
			this.m_releaseZDOTimer = 0f;
			this.ReleaseNearbyZDOS(ZNet.instance.GetReferencePosition(), this.m_sessionID);
			foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
			{
				this.ReleaseNearbyZDOS(zdopeer.m_peer.m_refPos, zdopeer.m_peer.m_uid);
			}
		}
	}

	// Token: 0x06000DB8 RID: 3512 RVA: 0x0005DD60 File Offset: 0x0005BF60
	private bool IsInPeerActiveArea(Vector2i sector, long uid)
	{
		if (uid == this.m_sessionID)
		{
			return ZNetScene.InActiveArea(sector, ZNet.instance.GetReferencePosition());
		}
		ZNetPeer peer = ZNet.instance.GetPeer(uid);
		return peer != null && ZNetScene.InActiveArea(sector, peer.GetRefPos());
	}

	// Token: 0x06000DB9 RID: 3513 RVA: 0x0005DDA4 File Offset: 0x0005BFA4
	private void ReleaseNearbyZDOS(Vector3 refPosition, long uid)
	{
		Vector2i zone = ZoneSystem.instance.GetZone(refPosition);
		this.m_tempNearObjects.Clear();
		this.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, 0, this.m_tempNearObjects, null);
		foreach (ZDO zdo in this.m_tempNearObjects)
		{
			if (zdo.Persistent)
			{
				if (zdo.GetOwner() == uid)
				{
					if (!ZNetScene.InActiveArea(zdo.GetSector(), zone))
					{
						zdo.SetOwner(0L);
					}
				}
				else if ((!zdo.HasOwner() || !this.IsInPeerActiveArea(zdo.GetSector(), zdo.GetOwner())) && ZNetScene.InActiveArea(zdo.GetSector(), zone))
				{
					zdo.SetOwner(uid);
				}
			}
		}
	}

	// Token: 0x06000DBA RID: 3514 RVA: 0x0005DE7C File Offset: 0x0005C07C
	public void DestroyZDO(ZDO zdo)
	{
		if (!zdo.IsOwner())
		{
			return;
		}
		this.m_destroySendList.Add(zdo.m_uid);
	}

	// Token: 0x06000DBB RID: 3515 RVA: 0x0005DE98 File Offset: 0x0005C098
	private void SendDestroyed()
	{
		if (this.m_destroySendList.Count == 0)
		{
			return;
		}
		ZPackage zpackage = new ZPackage();
		zpackage.Write(this.m_destroySendList.Count);
		foreach (ZDOID id in this.m_destroySendList)
		{
			zpackage.Write(id);
		}
		this.m_destroySendList.Clear();
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "DestroyZDO", new object[]
		{
			zpackage
		});
	}

	// Token: 0x06000DBC RID: 3516 RVA: 0x0005DF3C File Offset: 0x0005C13C
	private void RPC_DestroyZDO(long sender, ZPackage pkg)
	{
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			ZDOID uid = pkg.ReadZDOID();
			this.HandleDestroyedZDO(uid);
		}
	}

	// Token: 0x06000DBD RID: 3517 RVA: 0x0005DF6C File Offset: 0x0005C16C
	private void HandleDestroyedZDO(ZDOID uid)
	{
		if (uid.UserID == this.m_sessionID && uid.ID >= this.m_nextUid)
		{
			this.m_nextUid = uid.ID + 1U;
		}
		ZDO zdo = this.GetZDO(uid);
		if (zdo == null)
		{
			return;
		}
		if (this.m_onZDODestroyed != null)
		{
			this.m_onZDODestroyed(zdo);
		}
		this.RemoveFromSector(zdo, zdo.GetSector());
		this.m_objectsByID.Remove(zdo.m_uid);
		if (zdo.GetPrefab() == Game.instance.PortalPrefabHash)
		{
			this.m_portalObjects.Remove(zdo);
		}
		ZDOPool.Release(zdo);
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			zdopeer.m_zdos.Remove(uid);
		}
		if (ZNet.instance.IsServer())
		{
			long ticks = ZNet.instance.GetTime().Ticks;
			this.m_deadZDOs[uid] = ticks;
		}
	}

	// Token: 0x06000DBE RID: 3518 RVA: 0x0005E080 File Offset: 0x0005C280
	private void SendAllZDOs(ZDOMan.ZDOPeer peer)
	{
		while (this.SendZDOs(peer, true))
		{
		}
	}

	// Token: 0x06000DBF RID: 3519 RVA: 0x0005E08C File Offset: 0x0005C28C
	private bool SendZDOs(ZDOMan.ZDOPeer peer, bool flush)
	{
		int sendQueueSize = peer.m_peer.m_socket.GetSendQueueSize();
		if (!flush && sendQueueSize > 10240)
		{
			return false;
		}
		int num = 10240 - sendQueueSize;
		if (num < 2048)
		{
			return false;
		}
		this.m_tempToSync.Clear();
		this.CreateSyncList(peer, this.m_tempToSync);
		if (this.m_tempToSync.Count == 0 && peer.m_invalidSector.Count == 0)
		{
			return false;
		}
		ZPackage zpackage = new ZPackage();
		bool flag = false;
		if (peer.m_invalidSector.Count > 0)
		{
			flag = true;
			zpackage.Write(peer.m_invalidSector.Count);
			foreach (ZDOID id in peer.m_invalidSector)
			{
				zpackage.Write(id);
			}
			peer.m_invalidSector.Clear();
		}
		else
		{
			zpackage.Write(0);
		}
		float time = Time.time;
		ZPackage zpackage2 = new ZPackage();
		bool flag2 = false;
		foreach (ZDO zdo in this.m_tempToSync)
		{
			if (zpackage.Size() > num)
			{
				break;
			}
			peer.m_forceSend.Remove(zdo.m_uid);
			if (!ZNet.instance.IsServer())
			{
				this.m_clientChangeQueue.Remove(zdo.m_uid);
			}
			zpackage.Write(zdo.m_uid);
			zpackage.Write(zdo.OwnerRevision);
			zpackage.Write(zdo.DataRevision);
			zpackage.Write(zdo.GetOwner());
			zpackage.Write(zdo.GetPosition());
			zpackage2.Clear();
			zdo.Serialize(zpackage2);
			zpackage.Write(zpackage2);
			peer.m_zdos[zdo.m_uid] = new ZDOMan.ZDOPeer.PeerZDOInfo(zdo.DataRevision, zdo.OwnerRevision, time);
			flag2 = true;
			this.m_zdosSent++;
		}
		zpackage.Write(ZDOID.None);
		if (flag2 || flag)
		{
			peer.m_peer.m_rpc.Invoke("ZDOData", new object[]
			{
				zpackage
			});
		}
		return flag2 || flag;
	}

	// Token: 0x06000DC0 RID: 3520 RVA: 0x0005E2DC File Offset: 0x0005C4DC
	private void RPC_ZDOData(ZRpc rpc, ZPackage pkg)
	{
		ZDOMan.ZDOPeer zdopeer = this.FindPeer(rpc);
		if (zdopeer == null)
		{
			ZLog.Log("ZDO data from unkown host, ignoring");
			return;
		}
		float time = Time.time;
		int num = 0;
		ZPackage pkg2 = new ZPackage();
		int num2 = pkg.ReadInt();
		for (int i = 0; i < num2; i++)
		{
			ZDOID id = pkg.ReadZDOID();
			ZDO zdo = this.GetZDO(id);
			if (zdo != null)
			{
				zdo.InvalidateSector();
			}
		}
		for (;;)
		{
			ZDOID zdoid = pkg.ReadZDOID();
			if (zdoid.IsNone())
			{
				break;
			}
			num++;
			ushort num3 = pkg.ReadUShort();
			uint num4 = pkg.ReadUInt();
			long ownerInternal = pkg.ReadLong();
			Vector3 vector = pkg.ReadVector3();
			pkg.ReadPackage(ref pkg2);
			ZDO zdo2 = this.GetZDO(zdoid);
			bool flag = false;
			if (zdo2 != null)
			{
				if (num4 <= zdo2.DataRevision)
				{
					if (num3 > zdo2.OwnerRevision)
					{
						zdo2.SetOwnerInternal(ownerInternal);
						zdo2.OwnerRevision = num3;
						zdopeer.m_zdos[zdoid] = new ZDOMan.ZDOPeer.PeerZDOInfo(num4, num3, time);
						continue;
					}
					continue;
				}
			}
			else
			{
				zdo2 = this.CreateNewZDO(zdoid, vector, 0);
				flag = true;
			}
			zdo2.OwnerRevision = num3;
			zdo2.DataRevision = num4;
			zdo2.SetOwnerInternal(ownerInternal);
			zdo2.InternalSetPosition(vector);
			zdopeer.m_zdos[zdoid] = new ZDOMan.ZDOPeer.PeerZDOInfo(zdo2.DataRevision, zdo2.OwnerRevision, time);
			zdo2.Deserialize(pkg2);
			if (zdo2.GetPrefab() == Game.instance.PortalPrefabHash)
			{
				this.AddPortal(zdo2);
			}
			if (ZNet.instance.IsServer() && flag && this.m_deadZDOs.ContainsKey(zdoid))
			{
				zdo2.SetOwner(this.m_sessionID);
				this.DestroyZDO(zdo2);
			}
		}
		this.m_zdosRecv += num;
	}

	// Token: 0x06000DC1 RID: 3521 RVA: 0x0005E4A0 File Offset: 0x0005C6A0
	public void FindSectorObjects(Vector2i sector, int area, int distantArea, List<ZDO> sectorObjects, List<ZDO> distantSectorObjects = null)
	{
		this.FindObjects(sector, sectorObjects);
		for (int i = 1; i <= area; i++)
		{
			for (int j = sector.x - i; j <= sector.x + i; j++)
			{
				this.FindObjects(new Vector2i(j, sector.y - i), sectorObjects);
				this.FindObjects(new Vector2i(j, sector.y + i), sectorObjects);
			}
			for (int k = sector.y - i + 1; k <= sector.y + i - 1; k++)
			{
				this.FindObjects(new Vector2i(sector.x - i, k), sectorObjects);
				this.FindObjects(new Vector2i(sector.x + i, k), sectorObjects);
			}
		}
		List<ZDO> objects = distantSectorObjects ?? sectorObjects;
		for (int l = area + 1; l <= area + distantArea; l++)
		{
			for (int m = sector.x - l; m <= sector.x + l; m++)
			{
				this.FindDistantObjects(new Vector2i(m, sector.y - l), objects);
				this.FindDistantObjects(new Vector2i(m, sector.y + l), objects);
			}
			for (int n = sector.y - l + 1; n <= sector.y + l - 1; n++)
			{
				this.FindDistantObjects(new Vector2i(sector.x - l, n), objects);
				this.FindDistantObjects(new Vector2i(sector.x + l, n), objects);
			}
		}
	}

	// Token: 0x06000DC2 RID: 3522 RVA: 0x0005E61C File Offset: 0x0005C81C
	private void CreateSyncList(ZDOMan.ZDOPeer peer, List<ZDO> toSync)
	{
		if (ZNet.instance.IsServer())
		{
			Vector3 refPos = peer.m_peer.GetRefPos();
			Vector2i zone = ZoneSystem.instance.GetZone(refPos);
			this.m_tempSectorObjects.Clear();
			this.m_tempToSyncDistant.Clear();
			this.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, ZoneSystem.instance.m_activeDistantArea, this.m_tempSectorObjects, this.m_tempToSyncDistant);
			foreach (ZDO zdo in this.m_tempSectorObjects)
			{
				if (peer.ShouldSend(zdo))
				{
					toSync.Add(zdo);
				}
			}
			this.ServerSortSendZDOS(toSync, refPos, peer);
			if (toSync.Count < 10)
			{
				foreach (ZDO zdo2 in this.m_tempToSyncDistant)
				{
					if (peer.ShouldSend(zdo2))
					{
						toSync.Add(zdo2);
					}
				}
			}
			this.AddForceSendZdos(peer, toSync);
			return;
		}
		this.m_tempRemoveList.Clear();
		foreach (ZDOID zdoid in this.m_clientChangeQueue)
		{
			ZDO zdo3 = this.GetZDO(zdoid);
			if (zdo3 != null && peer.ShouldSend(zdo3))
			{
				toSync.Add(zdo3);
			}
			else
			{
				this.m_tempRemoveList.Add(zdoid);
			}
		}
		foreach (ZDOID item in this.m_tempRemoveList)
		{
			this.m_clientChangeQueue.Remove(item);
		}
		this.ClientSortSendZDOS(toSync, peer);
		this.AddForceSendZdos(peer, toSync);
	}

	// Token: 0x06000DC3 RID: 3523 RVA: 0x0005E818 File Offset: 0x0005CA18
	private void AddForceSendZdos(ZDOMan.ZDOPeer peer, List<ZDO> syncList)
	{
		if (peer.m_forceSend.Count <= 0)
		{
			return;
		}
		this.m_tempRemoveList.Clear();
		foreach (ZDOID zdoid in peer.m_forceSend)
		{
			ZDO zdo = this.GetZDO(zdoid);
			if (zdo != null && peer.ShouldSend(zdo))
			{
				syncList.Insert(0, zdo);
			}
			else
			{
				this.m_tempRemoveList.Add(zdoid);
			}
		}
		foreach (ZDOID item in this.m_tempRemoveList)
		{
			peer.m_forceSend.Remove(item);
		}
	}

	// Token: 0x06000DC4 RID: 3524 RVA: 0x0005E8F4 File Offset: 0x0005CAF4
	private static int ServerSendCompare(ZDO x, ZDO y)
	{
		bool flag = x.Type == ZDO.ObjectType.Prioritized && x.HasOwner() && x.GetOwner() != ZDOMan.s_compareReceiver;
		bool flag2 = y.Type == ZDO.ObjectType.Prioritized && y.HasOwner() && y.GetOwner() != ZDOMan.s_compareReceiver;
		if (flag && flag2)
		{
			return Utils.CompareFloats(x.m_tempSortValue, y.m_tempSortValue);
		}
		if (flag != flag2)
		{
			if (!flag)
			{
				return 1;
			}
			return -1;
		}
		else
		{
			if (x.Type == y.Type)
			{
				return Utils.CompareFloats(x.m_tempSortValue, y.m_tempSortValue);
			}
			return ((int)y.Type).CompareTo((int)x.Type);
		}
	}

	// Token: 0x06000DC5 RID: 3525 RVA: 0x0005E9A4 File Offset: 0x0005CBA4
	private void ServerSortSendZDOS(List<ZDO> objects, Vector3 refPos, ZDOMan.ZDOPeer peer)
	{
		float time = Time.time;
		foreach (ZDO zdo in objects)
		{
			Vector3 position = zdo.GetPosition();
			zdo.m_tempSortValue = Vector3.Distance(position, refPos);
			float num = 100f;
			ZDOMan.ZDOPeer.PeerZDOInfo peerZDOInfo;
			if (peer.m_zdos.TryGetValue(zdo.m_uid, out peerZDOInfo))
			{
				num = Mathf.Clamp(time - peerZDOInfo.m_syncTime, 0f, 100f);
			}
			zdo.m_tempSortValue -= num * 1.5f;
		}
		ZDOMan.s_compareReceiver = peer.m_peer.m_uid;
		objects.Sort(new Comparison<ZDO>(ZDOMan.ServerSendCompare));
	}

	// Token: 0x06000DC6 RID: 3526 RVA: 0x0005EA74 File Offset: 0x0005CC74
	private static int ClientSendCompare(ZDO x, ZDO y)
	{
		if (x.Type == y.Type)
		{
			return Utils.CompareFloats(x.m_tempSortValue, y.m_tempSortValue);
		}
		if (x.Type == ZDO.ObjectType.Prioritized)
		{
			return -1;
		}
		if (y.Type == ZDO.ObjectType.Prioritized)
		{
			return 1;
		}
		return Utils.CompareFloats(x.m_tempSortValue, y.m_tempSortValue);
	}

	// Token: 0x06000DC7 RID: 3527 RVA: 0x0005EAC8 File Offset: 0x0005CCC8
	private void ClientSortSendZDOS(List<ZDO> objects, ZDOMan.ZDOPeer peer)
	{
		float time = Time.time;
		foreach (ZDO zdo in objects)
		{
			zdo.m_tempSortValue = 0f;
			float num = 100f;
			ZDOMan.ZDOPeer.PeerZDOInfo peerZDOInfo;
			if (peer.m_zdos.TryGetValue(zdo.m_uid, out peerZDOInfo))
			{
				num = Mathf.Clamp(time - peerZDOInfo.m_syncTime, 0f, 100f);
			}
			zdo.m_tempSortValue -= num * 1.5f;
		}
		objects.Sort(new Comparison<ZDO>(ZDOMan.ClientSendCompare));
	}

	// Token: 0x06000DC8 RID: 3528 RVA: 0x0005EB7C File Offset: 0x0005CD7C
	private void AddDistantObjects(ZDOMan.ZDOPeer peer, int maxItems, List<ZDO> toSync)
	{
		if (peer.m_sendIndex >= this.m_objectsByID.Count)
		{
			peer.m_sendIndex = 0;
		}
		IEnumerable<KeyValuePair<ZDOID, ZDO>> enumerable = this.m_objectsByID.Skip(peer.m_sendIndex).Take(maxItems);
		peer.m_sendIndex += maxItems;
		foreach (KeyValuePair<ZDOID, ZDO> keyValuePair in enumerable)
		{
			toSync.Add(keyValuePair.Value);
		}
	}

	// Token: 0x06000DC9 RID: 3529 RVA: 0x0005EC08 File Offset: 0x0005CE08
	public static long GetSessionID()
	{
		return ZDOMan.s_instance.m_sessionID;
	}

	// Token: 0x06000DCA RID: 3530 RVA: 0x0005EC14 File Offset: 0x0005CE14
	private int SectorToIndex(Vector2i s)
	{
		int num = s.x + this.m_halfWidth;
		int num2 = s.y + this.m_halfWidth;
		if (num < 0 || num2 < 0 || num >= this.m_width || num2 >= this.m_width)
		{
			return -1;
		}
		return num2 * this.m_width + num;
	}

	// Token: 0x06000DCB RID: 3531 RVA: 0x0005EC64 File Offset: 0x0005CE64
	private void FindObjects(Vector2i sector, List<ZDO> objects)
	{
		int num = this.SectorToIndex(sector);
		List<ZDO> collection;
		if (num >= 0)
		{
			if (this.m_objectsBySector[num] != null)
			{
				objects.AddRange(this.m_objectsBySector[num]);
				return;
			}
		}
		else if (this.m_objectsByOutsideSector.TryGetValue(sector, out collection))
		{
			objects.AddRange(collection);
		}
	}

	// Token: 0x06000DCC RID: 3532 RVA: 0x0005ECB0 File Offset: 0x0005CEB0
	private void FindDistantObjects(Vector2i sector, List<ZDO> objects)
	{
		int num = this.SectorToIndex(sector);
		if (num >= 0)
		{
			List<ZDO> list = this.m_objectsBySector[num];
			if (list == null)
			{
				return;
			}
			using (List<ZDO>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZDO zdo = enumerator.Current;
					if (zdo.Distant)
					{
						objects.Add(zdo);
					}
				}
				return;
			}
		}
		List<ZDO> list2;
		if (!this.m_objectsByOutsideSector.TryGetValue(sector, out list2))
		{
			return;
		}
		foreach (ZDO zdo2 in list2)
		{
			if (zdo2.Distant)
			{
				objects.Add(zdo2);
			}
		}
	}

	// Token: 0x06000DCD RID: 3533 RVA: 0x0005ED7C File Offset: 0x0005CF7C
	private void RemoveOrphanNonPersistentZDOS()
	{
		foreach (KeyValuePair<ZDOID, ZDO> keyValuePair in this.m_objectsByID)
		{
			ZDO value = keyValuePair.Value;
			if (!value.Persistent && (!value.HasOwner() || !this.IsPeerConnected(value.GetOwner())))
			{
				string str = "Destroying abandoned non persistent zdo ";
				ZDOID uid = value.m_uid;
				ZLog.Log(str + uid.ToString() + " owner " + value.GetOwner().ToString());
				value.SetOwner(this.m_sessionID);
				this.DestroyZDO(value);
			}
		}
	}

	// Token: 0x06000DCE RID: 3534 RVA: 0x0005EE3C File Offset: 0x0005D03C
	private bool IsPeerConnected(long uid)
	{
		if (this.m_sessionID == uid)
		{
			return true;
		}
		using (List<ZDOMan.ZDOPeer>.Enumerator enumerator = this.m_peers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_peer.m_uid == uid)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000DCF RID: 3535 RVA: 0x0005EEA8 File Offset: 0x0005D0A8
	private static bool InvalidZDO(ZDO zdo)
	{
		return !zdo.IsValid();
	}

	// Token: 0x06000DD0 RID: 3536 RVA: 0x0005EEB4 File Offset: 0x0005D0B4
	public bool GetAllZDOsWithPrefabIterative(string prefab, List<ZDO> zdos, ref int index)
	{
		int stableHashCode = prefab.GetStableHashCode();
		if (index >= this.m_objectsBySector.Length)
		{
			foreach (List<ZDO> list in this.m_objectsByOutsideSector.Values)
			{
				foreach (ZDO zdo in list)
				{
					if (zdo.GetPrefab() == stableHashCode)
					{
						zdos.Add(zdo);
					}
				}
			}
			zdos.RemoveAll(new Predicate<ZDO>(ZDOMan.InvalidZDO));
			return true;
		}
		int num = 0;
		while (index < this.m_objectsBySector.Length)
		{
			List<ZDO> list2 = this.m_objectsBySector[index];
			if (list2 != null)
			{
				foreach (ZDO zdo2 in list2)
				{
					if (zdo2.GetPrefab() == stableHashCode)
					{
						zdos.Add(zdo2);
					}
				}
				num++;
				if (num > 400)
				{
					break;
				}
			}
			index++;
		}
		return false;
	}

	// Token: 0x06000DD1 RID: 3537 RVA: 0x0005EFF4 File Offset: 0x0005D1F4
	private List<ZDO> GetSaveClone()
	{
		List<ZDO> list = new List<ZDO>();
		for (int i = 0; i < this.m_objectsBySector.Length; i++)
		{
			if (this.m_objectsBySector[i] != null)
			{
				foreach (ZDO zdo in this.m_objectsBySector[i])
				{
					if (zdo.Persistent)
					{
						list.Add(zdo.Clone());
					}
				}
			}
		}
		foreach (List<ZDO> list2 in this.m_objectsByOutsideSector.Values)
		{
			foreach (ZDO zdo2 in list2)
			{
				if (zdo2.Persistent)
				{
					list.Add(zdo2.Clone());
				}
			}
		}
		return list;
	}

	// Token: 0x06000DD2 RID: 3538 RVA: 0x0005F108 File Offset: 0x0005D308
	public List<ZDO> GetPortals()
	{
		return this.m_portalObjects;
	}

	// Token: 0x06000DD3 RID: 3539 RVA: 0x0005F110 File Offset: 0x0005D310
	public int NrOfObjects()
	{
		return this.m_objectsByID.Count;
	}

	// Token: 0x06000DD4 RID: 3540 RVA: 0x0005F11D File Offset: 0x0005D31D
	public int GetSentZDOs()
	{
		return this.m_zdosSentLastSec;
	}

	// Token: 0x06000DD5 RID: 3541 RVA: 0x0005F125 File Offset: 0x0005D325
	public int GetRecvZDOs()
	{
		return this.m_zdosRecvLastSec;
	}

	// Token: 0x06000DD6 RID: 3542 RVA: 0x0005F12D File Offset: 0x0005D32D
	public int GetClientChangeQueue()
	{
		return this.m_clientChangeQueue.Count;
	}

	// Token: 0x06000DD7 RID: 3543 RVA: 0x0005F13A File Offset: 0x0005D33A
	public void GetAverageStats(out float sentZdos, out float recvZdos)
	{
		sentZdos = (float)this.m_zdosSentLastSec / 20f;
		recvZdos = (float)this.m_zdosRecvLastSec / 20f;
	}

	// Token: 0x06000DD8 RID: 3544 RVA: 0x0005F15A File Offset: 0x0005D35A
	public void RequestZDO(ZDOID id)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("RequestZDO", new object[]
		{
			id
		});
	}

	// Token: 0x06000DD9 RID: 3545 RVA: 0x0005F17A File Offset: 0x0005D37A
	private void RPC_RequestZDO(long sender, ZDOID id)
	{
		ZDOMan.ZDOPeer peer = this.GetPeer(sender);
		if (peer == null)
		{
			return;
		}
		peer.ForceSendZDO(id);
	}

	// Token: 0x06000DDA RID: 3546 RVA: 0x0005F190 File Offset: 0x0005D390
	private ZDOMan.ZDOPeer GetPeer(long uid)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			if (zdopeer.m_peer.m_uid == uid)
			{
				return zdopeer;
			}
		}
		return null;
	}

	// Token: 0x06000DDB RID: 3547 RVA: 0x0005F1F4 File Offset: 0x0005D3F4
	public void ForceSendZDO(ZDOID id)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			zdopeer.ForceSendZDO(id);
		}
	}

	// Token: 0x06000DDC RID: 3548 RVA: 0x0005F248 File Offset: 0x0005D448
	public void ForceSendZDO(long peerID, ZDOID id)
	{
		if (ZNet.instance.IsServer())
		{
			ZDOMan.ZDOPeer peer = this.GetPeer(peerID);
			if (peer != null)
			{
				peer.ForceSendZDO(id);
				return;
			}
		}
		else
		{
			foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
			{
				zdopeer.ForceSendZDO(id);
			}
		}
	}

	// Token: 0x06000DDD RID: 3549 RVA: 0x0005F2B8 File Offset: 0x0005D4B8
	public void ClientChanged(ZDOID id)
	{
		this.m_clientChangeQueue.Add(id);
	}

	// Token: 0x06000DDE RID: 3550 RVA: 0x0005F2C7 File Offset: 0x0005D4C7
	private void AddPortal(ZDO zdo)
	{
		if (!this.m_portalObjects.Contains(zdo))
		{
			this.m_portalObjects.Add(zdo);
		}
	}

	// Token: 0x06000DDF RID: 3551 RVA: 0x0005F2E4 File Offset: 0x0005D4E4
	private void ConvertOwnerships(List<ZDO> zdos)
	{
		foreach (ZDO zdo in zdos)
		{
			ZDOID zdoid = zdo.GetZDOID(ZDOVars.s_zdoidUser);
			if (zdoid != ZDOID.None)
			{
				zdo.SetOwnerInternal(ZDOMan.GetSessionID());
				zdo.Set(ZDOVars.s_user, zdoid.UserID);
			}
			ZDOID zdoid2 = zdo.GetZDOID(ZDOVars.s_zdoidRodOwner);
			if (zdoid2 != ZDOID.None)
			{
				zdo.SetOwnerInternal(ZDOMan.GetSessionID());
				zdo.Set(ZDOVars.s_rodOwner, zdoid2.UserID);
			}
		}
	}

	// Token: 0x06000DE0 RID: 3552 RVA: 0x0005F398 File Offset: 0x0005D598
	private void ConvertCreationTime(List<ZDO> zdos)
	{
		if (!ZDOExtraData.HasTimeCreated())
		{
			return;
		}
		List<int> list = new List<int>
		{
			"cultivate".GetStableHashCode(),
			"raise".GetStableHashCode(),
			"path".GetStableHashCode(),
			"paved_road".GetStableHashCode(),
			"HeathRockPillar".GetStableHashCode(),
			"HeathRockPillar_frac".GetStableHashCode(),
			"ship_construction".GetStableHashCode(),
			"replant".GetStableHashCode(),
			"digg".GetStableHashCode(),
			"mud_road".GetStableHashCode(),
			"LevelTerrain".GetStableHashCode(),
			"digg_v2".GetStableHashCode()
		};
		int num = 0;
		foreach (ZDO zdo in zdos)
		{
			if (list.Contains(zdo.GetPrefab()))
			{
				num++;
				long timeCreated = ZDOExtraData.GetTimeCreated(zdo.m_uid);
				zdo.SetOwner(ZDOMan.GetSessionID());
				zdo.Set(ZDOVars.s_terrainModifierTimeCreated, timeCreated);
			}
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log("Converted " + num.ToString() + " Creation Times.");
		}
	}

	// Token: 0x06000DE1 RID: 3553 RVA: 0x0005F508 File Offset: 0x0005D708
	private void ConvertPortals()
	{
		UnityEngine.Debug.Log("ConvertPortals => Make sure all " + this.m_portalObjects.Count.ToString() + " portals are in a good state.");
		int num = 0;
		foreach (ZDO zdo in this.m_portalObjects)
		{
			string @string = zdo.GetString(ZDOVars.s_tag, "");
			ZDOID zdoid = zdo.GetZDOID(ZDOVars.s_toRemoveTarget);
			zdo.RemoveZDOID(ZDOVars.s_toRemoveTarget);
			if (!(zdoid == ZDOID.None) && !(@string == ""))
			{
				ZDO zdo2 = this.GetZDO(zdoid);
				if (zdo2 != null)
				{
					ZDOID zdoid2 = zdo2.GetZDOID(ZDOVars.s_toRemoveTarget);
					string string2 = zdo2.GetString(ZDOVars.s_tag, "");
					zdo2.RemoveZDOID(ZDOVars.s_toRemoveTarget);
					if (@string == string2 && zdoid == zdo2.m_uid && zdoid2 == zdo.m_uid)
					{
						zdo.SetOwner(ZDOMan.GetSessionID());
						zdo2.SetOwner(ZDOMan.GetSessionID());
						num++;
						zdo.SetConnection(ZDOExtraData.ConnectionType.Portal, zdo2.m_uid);
						zdo2.SetConnection(ZDOExtraData.ConnectionType.Portal, zdo.m_uid);
						ZDOMan.instance.ForceSendZDO(zdo.m_uid);
						ZDOMan.instance.ForceSendZDO(zdo2.m_uid);
					}
				}
			}
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log("ConvertPortals => fixed " + num.ToString() + " portals.");
		}
	}

	// Token: 0x06000DE2 RID: 3554 RVA: 0x0005F6BC File Offset: 0x0005D8BC
	private void ConnectPortals()
	{
		List<ZDOID> allConnectionZDOIDs = ZDOExtraData.GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType.Portal);
		List<ZDOID> allConnectionZDOIDs2 = ZDOExtraData.GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType.Portal | ZDOExtraData.ConnectionType.Target);
		int num = 0;
		foreach (ZDOID zdoid in allConnectionZDOIDs)
		{
			ZDO zdo = this.GetZDO(zdoid);
			if (zdo != null)
			{
				ZDOConnectionHashData connectionHashData = zdo.GetConnectionHashData(ZDOExtraData.ConnectionType.Portal);
				if (connectionHashData != null)
				{
					foreach (ZDOID zdoid2 in allConnectionZDOIDs2)
					{
						if (!(zdoid2 == zdoid) && ZDOExtraData.GetConnectionType(zdoid2) == ZDOExtraData.ConnectionType.None)
						{
							ZDO zdo2 = this.GetZDO(zdoid2);
							if (zdo2 != null)
							{
								ZDOConnectionHashData connectionHashData2 = ZDOExtraData.GetConnectionHashData(zdoid2, ZDOExtraData.ConnectionType.Portal | ZDOExtraData.ConnectionType.Target);
								if (connectionHashData2 != null && connectionHashData.m_hash == connectionHashData2.m_hash)
								{
									num++;
									zdo.SetOwner(ZDOMan.GetSessionID());
									zdo2.SetOwner(ZDOMan.GetSessionID());
									zdo.SetConnection(ZDOExtraData.ConnectionType.Portal, zdoid2);
									zdo2.SetConnection(ZDOExtraData.ConnectionType.Portal, zdoid);
									break;
								}
							}
						}
					}
				}
			}
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log("ConnectPortals => Connected " + num.ToString() + " portals.");
		}
	}

	// Token: 0x06000DE3 RID: 3555 RVA: 0x0005F804 File Offset: 0x0005DA04
	private void ConvertSpawners()
	{
		List<ZDOID> allZDOIDsWithHash = ZDOExtraData.GetAllZDOIDsWithHash(ZDOExtraData.Type.Long, "spawn_id_u".GetStableHashCode());
		if (allZDOIDsWithHash.Count > 0)
		{
			UnityEngine.Debug.Log("ConvertSpawners => Will try and convert " + allZDOIDsWithHash.Count.ToString() + " spawners.");
		}
		int num = 0;
		int num2 = 0;
		foreach (ZDO zdo in from id in allZDOIDsWithHash
		select this.GetZDO(id))
		{
			zdo.SetOwner(ZDOMan.GetSessionID());
			ZDOID zdoid = zdo.GetZDOID(ZDOVars.s_toRemoveSpawnID);
			zdo.RemoveZDOID(ZDOVars.s_toRemoveSpawnID);
			ZDO zdo2 = this.GetZDO(zdoid);
			if (zdo2 != null)
			{
				num++;
				zdo.SetConnection(ZDOExtraData.ConnectionType.Spawned, zdo2.m_uid);
			}
			else
			{
				num2++;
				zdo.SetConnection(ZDOExtraData.ConnectionType.Spawned, ZDOID.None);
			}
		}
		if (num > 0 || num2 > 0)
		{
			UnityEngine.Debug.Log(string.Concat(new string[]
			{
				"ConvertSpawners => Converted ",
				num.ToString(),
				" spawners, and ",
				num2.ToString(),
				" 'done' spawners."
			}));
		}
	}

	// Token: 0x06000DE4 RID: 3556 RVA: 0x0005F93C File Offset: 0x0005DB3C
	private void ConnectSpawners()
	{
		List<ZDOID> allConnectionZDOIDs = ZDOExtraData.GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType.Spawned);
		List<ZDOID> allConnectionZDOIDs2 = ZDOExtraData.GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType.Portal | ZDOExtraData.ConnectionType.SyncTransform | ZDOExtraData.ConnectionType.Target);
		int num = 0;
		int num2 = 0;
		foreach (ZDOID zdoid in allConnectionZDOIDs)
		{
			ZDO zdo = this.GetZDO(zdoid);
			if (zdo != null)
			{
				zdo.SetOwner(ZDOMan.GetSessionID());
				bool flag = false;
				ZDOConnectionHashData connectionHashData = zdo.GetConnectionHashData(ZDOExtraData.ConnectionType.Spawned);
				if (connectionHashData != null)
				{
					foreach (ZDOID zdoid2 in allConnectionZDOIDs2)
					{
						if (!(zdoid2 == zdoid))
						{
							ZDOConnectionHashData connectionHashData2 = ZDOExtraData.GetConnectionHashData(zdoid2, ZDOExtraData.ConnectionType.Portal | ZDOExtraData.ConnectionType.SyncTransform | ZDOExtraData.ConnectionType.Target);
							if (connectionHashData2 != null && connectionHashData.m_hash == connectionHashData2.m_hash)
							{
								flag = true;
								num++;
								zdo.SetConnection(ZDOExtraData.ConnectionType.Spawned, zdoid2);
								break;
							}
						}
					}
				}
				if (!flag)
				{
					num2++;
					zdo.SetConnection(ZDOExtraData.ConnectionType.Spawned, ZDOID.None);
				}
			}
		}
		if (num > 0 || num2 > 0)
		{
			UnityEngine.Debug.Log(string.Concat(new string[]
			{
				"ConnectSpawners => Connected ",
				num.ToString(),
				" spawners and ",
				num2.ToString(),
				" 'done' spawners."
			}));
		}
	}

	// Token: 0x06000DE5 RID: 3557 RVA: 0x0005FA98 File Offset: 0x0005DC98
	private void ConvertSyncTransforms()
	{
		List<ZDOID> allZDOIDsWithHash = ZDOExtraData.GetAllZDOIDsWithHash(ZDOExtraData.Type.Long, "parentID_u".GetStableHashCode());
		if (allZDOIDsWithHash.Count > 0)
		{
			UnityEngine.Debug.Log("ConvertSyncTransforms => Will try and convert " + allZDOIDsWithHash.Count.ToString() + " SyncTransforms.");
		}
		int num = 0;
		foreach (ZDO zdo in allZDOIDsWithHash.Select(new Func<ZDOID, ZDO>(this.GetZDO)))
		{
			zdo.SetOwner(ZDOMan.GetSessionID());
			ZDOID zdoid = zdo.GetZDOID(ZDOVars.s_toRemoveParentID);
			zdo.RemoveZDOID(ZDOVars.s_toRemoveParentID);
			ZDO zdo2 = this.GetZDO(zdoid);
			if (zdo2 != null)
			{
				num++;
				zdo.SetConnection(ZDOExtraData.ConnectionType.SyncTransform, zdo2.m_uid);
			}
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log("ConvertSyncTransforms => Converted " + num.ToString() + " SyncTransforms.");
		}
	}

	// Token: 0x06000DE6 RID: 3558 RVA: 0x0005FB90 File Offset: 0x0005DD90
	private void ConvertSeed()
	{
		IEnumerable<ZDOID> allZDOIDsWithHash = ZDOExtraData.GetAllZDOIDsWithHash(ZDOExtraData.Type.Int, ZDOVars.s_leftItem);
		int num = 0;
		foreach (ZDO zdo in allZDOIDsWithHash.Select(new Func<ZDOID, ZDO>(this.GetZDO)))
		{
			num++;
			int hashCode = zdo.m_uid.GetHashCode();
			zdo.Set(ZDOVars.s_seed, hashCode, true);
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log("ConvertSeed => Converted " + num.ToString() + " ZDOs.");
		}
	}

	// Token: 0x06000DE7 RID: 3559 RVA: 0x0005FC30 File Offset: 0x0005DE30
	private void ConnectSyncTransforms()
	{
		List<ZDOID> allConnectionZDOIDs = ZDOExtraData.GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType.SyncTransform);
		List<ZDOID> allConnectionZDOIDs2 = ZDOExtraData.GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType.SyncTransform | ZDOExtraData.ConnectionType.Target);
		int num = 0;
		foreach (ZDOID zid in allConnectionZDOIDs)
		{
			ZDOConnectionHashData connectionHashData = ZDOExtraData.GetConnectionHashData(zid, ZDOExtraData.ConnectionType.SyncTransform);
			if (connectionHashData != null)
			{
				foreach (ZDOID zdoid in allConnectionZDOIDs2)
				{
					ZDOConnectionHashData connectionHashData2 = ZDOExtraData.GetConnectionHashData(zdoid, ZDOExtraData.ConnectionType.SyncTransform | ZDOExtraData.ConnectionType.Target);
					if (connectionHashData2 != null && connectionHashData.m_hash == connectionHashData2.m_hash)
					{
						num++;
						ZDOExtraData.SetConnection(zid, ZDOExtraData.ConnectionType.SyncTransform, zdoid);
						break;
					}
				}
			}
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log("ConnectSyncTransforms => Connected " + num.ToString() + " SyncTransforms.");
		}
	}

	// Token: 0x04000F36 RID: 3894
	public Action<ZDO> m_onZDODestroyed;

	// Token: 0x04000F37 RID: 3895
	private readonly long m_sessionID = Utils.GenerateUID();

	// Token: 0x04000F38 RID: 3896
	private uint m_nextUid = 1U;

	// Token: 0x04000F39 RID: 3897
	private readonly List<ZDO> m_portalObjects = new List<ZDO>();

	// Token: 0x04000F3A RID: 3898
	private readonly Dictionary<Vector2i, List<ZDO>> m_objectsByOutsideSector = new Dictionary<Vector2i, List<ZDO>>();

	// Token: 0x04000F3B RID: 3899
	private readonly List<ZDOMan.ZDOPeer> m_peers = new List<ZDOMan.ZDOPeer>();

	// Token: 0x04000F3C RID: 3900
	private readonly Dictionary<ZDOID, long> m_deadZDOs = new Dictionary<ZDOID, long>();

	// Token: 0x04000F3D RID: 3901
	private readonly List<ZDOID> m_destroySendList = new List<ZDOID>();

	// Token: 0x04000F3E RID: 3902
	private readonly HashSet<ZDOID> m_clientChangeQueue = new HashSet<ZDOID>();

	// Token: 0x04000F3F RID: 3903
	private readonly Dictionary<ZDOID, ZDO> m_objectsByID = new Dictionary<ZDOID, ZDO>();

	// Token: 0x04000F40 RID: 3904
	private List<ZDO>[] m_objectsBySector;

	// Token: 0x04000F41 RID: 3905
	private readonly int m_width;

	// Token: 0x04000F42 RID: 3906
	private readonly int m_halfWidth;

	// Token: 0x04000F43 RID: 3907
	private float m_sendTimer;

	// Token: 0x04000F44 RID: 3908
	private const float c_SendFPS = 20f;

	// Token: 0x04000F45 RID: 3909
	private float m_releaseZDOTimer;

	// Token: 0x04000F46 RID: 3910
	private int m_zdosSent;

	// Token: 0x04000F47 RID: 3911
	private int m_zdosRecv;

	// Token: 0x04000F48 RID: 3912
	private int m_zdosSentLastSec;

	// Token: 0x04000F49 RID: 3913
	private int m_zdosRecvLastSec;

	// Token: 0x04000F4A RID: 3914
	private float m_statTimer;

	// Token: 0x04000F4B RID: 3915
	private ZDOMan.SaveData m_saveData;

	// Token: 0x04000F4C RID: 3916
	private int m_nextSendPeer = -1;

	// Token: 0x04000F4D RID: 3917
	private readonly List<ZDO> m_tempToSync = new List<ZDO>();

	// Token: 0x04000F4E RID: 3918
	private readonly List<ZDO> m_tempToSyncDistant = new List<ZDO>();

	// Token: 0x04000F4F RID: 3919
	private readonly List<ZDO> m_tempNearObjects = new List<ZDO>();

	// Token: 0x04000F50 RID: 3920
	private readonly List<ZDOID> m_tempRemoveList = new List<ZDOID>();

	// Token: 0x04000F51 RID: 3921
	private readonly List<ZDO> m_tempSectorObjects = new List<ZDO>();

	// Token: 0x04000F52 RID: 3922
	private static ZDOMan s_instance;

	// Token: 0x04000F53 RID: 3923
	private static long s_compareReceiver;

	// Token: 0x0200015F RID: 351
	private class ZDOPeer
	{
		// Token: 0x06000DEA RID: 3562 RVA: 0x0005FD28 File Offset: 0x0005DF28
		public void ZDOSectorInvalidated(ZDO zdo)
		{
			if (zdo.GetOwner() == this.m_peer.m_uid)
			{
				return;
			}
			if (this.m_zdos.ContainsKey(zdo.m_uid) && !ZNetScene.InActiveArea(zdo.GetSector(), this.m_peer.GetRefPos()))
			{
				this.m_invalidSector.Add(zdo.m_uid);
				this.m_zdos.Remove(zdo.m_uid);
			}
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x0005FD98 File Offset: 0x0005DF98
		public void ForceSendZDO(ZDOID id)
		{
			this.m_forceSend.Add(id);
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x0005FDA8 File Offset: 0x0005DFA8
		public bool ShouldSend(ZDO zdo)
		{
			ZDOMan.ZDOPeer.PeerZDOInfo peerZDOInfo;
			return !this.m_zdos.TryGetValue(zdo.m_uid, out peerZDOInfo) || zdo.OwnerRevision > peerZDOInfo.m_ownerRevision || zdo.DataRevision > peerZDOInfo.m_dataRevision;
		}

		// Token: 0x04000F54 RID: 3924
		public ZNetPeer m_peer;

		// Token: 0x04000F55 RID: 3925
		public readonly Dictionary<ZDOID, ZDOMan.ZDOPeer.PeerZDOInfo> m_zdos = new Dictionary<ZDOID, ZDOMan.ZDOPeer.PeerZDOInfo>();

		// Token: 0x04000F56 RID: 3926
		public readonly HashSet<ZDOID> m_forceSend = new HashSet<ZDOID>();

		// Token: 0x04000F57 RID: 3927
		public readonly HashSet<ZDOID> m_invalidSector = new HashSet<ZDOID>();

		// Token: 0x04000F58 RID: 3928
		public int m_sendIndex;

		// Token: 0x02000160 RID: 352
		public struct PeerZDOInfo
		{
			// Token: 0x06000DEE RID: 3566 RVA: 0x0005FE13 File Offset: 0x0005E013
			public PeerZDOInfo(uint dataRevision, ushort ownerRevision, float syncTime)
			{
				this.m_dataRevision = dataRevision;
				this.m_ownerRevision = ownerRevision;
				this.m_syncTime = syncTime;
			}

			// Token: 0x04000F59 RID: 3929
			public readonly uint m_dataRevision;

			// Token: 0x04000F5A RID: 3930
			public readonly ushort m_ownerRevision;

			// Token: 0x04000F5B RID: 3931
			public readonly float m_syncTime;
		}
	}

	// Token: 0x02000161 RID: 353
	private class SaveData
	{
		// Token: 0x04000F5C RID: 3932
		public long m_sessionID;

		// Token: 0x04000F5D RID: 3933
		public uint m_nextUid = 1U;

		// Token: 0x04000F5E RID: 3934
		public List<ZDO> m_zdos;
	}
}
