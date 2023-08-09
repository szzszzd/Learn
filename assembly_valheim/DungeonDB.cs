using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020002CB RID: 715
public class DungeonDB : MonoBehaviour
{
	// Token: 0x170000FB RID: 251
	// (get) Token: 0x06001B1D RID: 6941 RVA: 0x000B52BA File Offset: 0x000B34BA
	public static DungeonDB instance
	{
		get
		{
			return DungeonDB.m_instance;
		}
	}

	// Token: 0x06001B1E RID: 6942 RVA: 0x000B52C4 File Offset: 0x000B34C4
	private void Awake()
	{
		DungeonDB.m_instance = this;
		foreach (string sceneName in this.m_roomScenes)
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		}
		ZLog.Log("DungeonDB Awake " + Time.frameCount.ToString());
	}

	// Token: 0x06001B1F RID: 6943 RVA: 0x000B5338 File Offset: 0x000B3538
	public bool SkipSaving()
	{
		return this.m_error;
	}

	// Token: 0x06001B20 RID: 6944 RVA: 0x000B5340 File Offset: 0x000B3540
	private void Start()
	{
		ZLog.Log("DungeonDB Start " + Time.frameCount.ToString());
		this.m_rooms = DungeonDB.SetupRooms();
		this.GenerateHashList();
	}

	// Token: 0x06001B21 RID: 6945 RVA: 0x000B537A File Offset: 0x000B357A
	public static List<DungeonDB.RoomData> GetRooms()
	{
		return DungeonDB.m_instance.m_rooms;
	}

	// Token: 0x06001B22 RID: 6946 RVA: 0x000B5388 File Offset: 0x000B3588
	private static List<DungeonDB.RoomData> SetupRooms()
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		List<DungeonDB.RoomData> list = new List<DungeonDB.RoomData>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name == "_Rooms")
			{
				GameObject gameObject2 = gameObject;
				if (gameObject2 == null || (DungeonDB.m_instance && gameObject2.activeSelf))
				{
					if (DungeonDB.m_instance)
					{
						DungeonDB.m_instance.m_error = true;
					}
					ZLog.LogError("Rooms are fucked, missing _Rooms or its enabled");
				}
				for (int j = 0; j < gameObject2.transform.childCount; j++)
				{
					Room component = gameObject2.transform.GetChild(j).GetComponent<Room>();
					DungeonDB.RoomData roomData = new DungeonDB.RoomData();
					roomData.m_room = component;
					ZoneSystem.PrepareNetViews(component.gameObject, roomData.m_netViews);
					ZoneSystem.PrepareRandomSpawns(component.gameObject, roomData.m_randomSpawns);
					list.Add(roomData);
				}
			}
		}
		return list;
	}

	// Token: 0x06001B23 RID: 6947 RVA: 0x000B5480 File Offset: 0x000B3680
	public DungeonDB.RoomData GetRoom(int hash)
	{
		DungeonDB.RoomData result;
		if (this.m_roomByHash.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001B24 RID: 6948 RVA: 0x000B54A0 File Offset: 0x000B36A0
	private void GenerateHashList()
	{
		this.m_roomByHash.Clear();
		foreach (DungeonDB.RoomData roomData in this.m_rooms)
		{
			int stableHashCode = roomData.m_room.gameObject.name.GetStableHashCode();
			if (this.m_roomByHash.ContainsKey(stableHashCode))
			{
				ZLog.LogError("Room with name " + roomData.m_room.gameObject.name + " already registered");
			}
			else
			{
				this.m_roomByHash.Add(stableHashCode, roomData);
			}
		}
	}

	// Token: 0x04001D6C RID: 7532
	private static DungeonDB m_instance;

	// Token: 0x04001D6D RID: 7533
	public List<string> m_roomScenes = new List<string>();

	// Token: 0x04001D6E RID: 7534
	private List<DungeonDB.RoomData> m_rooms = new List<DungeonDB.RoomData>();

	// Token: 0x04001D6F RID: 7535
	private Dictionary<int, DungeonDB.RoomData> m_roomByHash = new Dictionary<int, DungeonDB.RoomData>();

	// Token: 0x04001D70 RID: 7536
	private bool m_error;

	// Token: 0x020002CC RID: 716
	public class RoomData
	{
		// Token: 0x06001B26 RID: 6950 RVA: 0x000B5579 File Offset: 0x000B3779
		public override string ToString()
		{
			return this.m_room.ToString();
		}

		// Token: 0x04001D71 RID: 7537
		public Room m_room;

		// Token: 0x04001D72 RID: 7538
		[NonSerialized]
		public List<ZNetView> m_netViews = new List<ZNetView>();

		// Token: 0x04001D73 RID: 7539
		[NonSerialized]
		public List<RandomSpawn> m_randomSpawns = new List<RandomSpawn>();
	}
}
