using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002D1 RID: 721
public class Room : MonoBehaviour
{
	// Token: 0x06001B55 RID: 6997 RVA: 0x000B75F0 File Offset: 0x000B57F0
	private void Awake()
	{
		if (this.m_musicPrefab)
		{
			UnityEngine.Object.Instantiate<MusicVolume>(this.m_musicPrefab, base.transform).m_sizeFromRoom = this;
		}
	}

	// Token: 0x06001B56 RID: 6998 RVA: 0x000B7618 File Offset: 0x000B5818
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawWireCube(Vector3.zero, new Vector3((float)this.m_size.x, (float)this.m_size.y, (float)this.m_size.z));
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x06001B57 RID: 6999 RVA: 0x000B76B4 File Offset: 0x000B58B4
	public int GetHash()
	{
		return Utils.GetPrefabName(base.gameObject).GetStableHashCode();
	}

	// Token: 0x06001B58 RID: 7000 RVA: 0x000B76C6 File Offset: 0x000B58C6
	private void OnEnable()
	{
		this.m_roomConnections = null;
	}

	// Token: 0x06001B59 RID: 7001 RVA: 0x000B76CF File Offset: 0x000B58CF
	public RoomConnection[] GetConnections()
	{
		if (this.m_roomConnections == null)
		{
			this.m_roomConnections = base.GetComponentsInChildren<RoomConnection>(false);
		}
		return this.m_roomConnections;
	}

	// Token: 0x06001B5A RID: 7002 RVA: 0x000B76EC File Offset: 0x000B58EC
	public RoomConnection GetConnection(RoomConnection other)
	{
		RoomConnection[] connections = this.GetConnections();
		Room.tempConnections.Clear();
		foreach (RoomConnection roomConnection in connections)
		{
			if (roomConnection.m_type == other.m_type)
			{
				Room.tempConnections.Add(roomConnection);
			}
		}
		if (Room.tempConnections.Count == 0)
		{
			return null;
		}
		return Room.tempConnections[UnityEngine.Random.Range(0, Room.tempConnections.Count)];
	}

	// Token: 0x06001B5B RID: 7003 RVA: 0x000B7764 File Offset: 0x000B5964
	public RoomConnection GetEntrance()
	{
		RoomConnection[] connections = this.GetConnections();
		ZLog.Log("Connections " + connections.Length.ToString());
		foreach (RoomConnection roomConnection in connections)
		{
			if (roomConnection.m_entrance)
			{
				return roomConnection;
			}
		}
		return null;
	}

	// Token: 0x06001B5C RID: 7004 RVA: 0x000B77B4 File Offset: 0x000B59B4
	public bool HaveConnection(RoomConnection other)
	{
		RoomConnection[] connections = this.GetConnections();
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].m_type == other.m_type)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001B5D RID: 7005 RVA: 0x000B77F0 File Offset: 0x000B59F0
	public override string ToString()
	{
		return string.Format("{0}, Enabled: {1}, {2}, {3}", new object[]
		{
			base.name,
			this.m_enabled,
			this.m_theme,
			this.m_entrance ? "Entrance" : (this.m_endCap ? "EndCap" : "Room")
		});
	}

	// Token: 0x04001D9F RID: 7583
	private static List<RoomConnection> tempConnections = new List<RoomConnection>();

	// Token: 0x04001DA0 RID: 7584
	public Vector3Int m_size = new Vector3Int(8, 4, 8);

	// Token: 0x04001DA1 RID: 7585
	[BitMask(typeof(Room.Theme))]
	public Room.Theme m_theme = Room.Theme.Crypt;

	// Token: 0x04001DA2 RID: 7586
	public bool m_enabled = true;

	// Token: 0x04001DA3 RID: 7587
	public bool m_entrance;

	// Token: 0x04001DA4 RID: 7588
	public bool m_endCap;

	// Token: 0x04001DA5 RID: 7589
	public bool m_divider;

	// Token: 0x04001DA6 RID: 7590
	public int m_endCapPrio;

	// Token: 0x04001DA7 RID: 7591
	public int m_minPlaceOrder;

	// Token: 0x04001DA8 RID: 7592
	public float m_weight = 1f;

	// Token: 0x04001DA9 RID: 7593
	public bool m_faceCenter;

	// Token: 0x04001DAA RID: 7594
	public bool m_perimeter;

	// Token: 0x04001DAB RID: 7595
	[NonSerialized]
	public int m_placeOrder;

	// Token: 0x04001DAC RID: 7596
	[NonSerialized]
	public int m_seed;

	// Token: 0x04001DAD RID: 7597
	public MusicVolume m_musicPrefab;

	// Token: 0x04001DAE RID: 7598
	private RoomConnection[] m_roomConnections;

	// Token: 0x020002D2 RID: 722
	public enum Theme
	{
		// Token: 0x04001DB0 RID: 7600
		Crypt = 1,
		// Token: 0x04001DB1 RID: 7601
		SunkenCrypt,
		// Token: 0x04001DB2 RID: 7602
		Cave = 4,
		// Token: 0x04001DB3 RID: 7603
		ForestCrypt = 8,
		// Token: 0x04001DB4 RID: 7604
		GoblinCamp = 16,
		// Token: 0x04001DB5 RID: 7605
		MeadowsVillage = 32,
		// Token: 0x04001DB6 RID: 7606
		MeadowsFarm = 64,
		// Token: 0x04001DB7 RID: 7607
		DvergerTown = 128,
		// Token: 0x04001DB8 RID: 7608
		DvergerBoss = 256
	}
}
