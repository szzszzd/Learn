using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x020002CD RID: 717
public class DungeonGenerator : MonoBehaviour
{
	// Token: 0x06001B28 RID: 6952 RVA: 0x000B55A4 File Offset: 0x000B37A4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.Load();
	}

	// Token: 0x06001B29 RID: 6953 RVA: 0x000B55B8 File Offset: 0x000B37B8
	public void Clear()
	{
		while (base.transform.childCount > 0)
		{
			UnityEngine.Object.DestroyImmediate(base.transform.GetChild(0).gameObject);
		}
	}

	// Token: 0x06001B2A RID: 6954 RVA: 0x000B55E0 File Offset: 0x000B37E0
	public void Generate(ZoneSystem.SpawnMode mode)
	{
		int seed = this.GetSeed();
		this.Generate(seed, mode);
	}

	// Token: 0x06001B2B RID: 6955 RVA: 0x000B55FC File Offset: 0x000B37FC
	public int GetSeed()
	{
		if (this.m_hasGeneratedSeed)
		{
			return this.m_generatedSeed;
		}
		if (DungeonGenerator.m_forceSeed != -2147483648)
		{
			this.m_generatedSeed = DungeonGenerator.m_forceSeed;
			DungeonGenerator.m_forceSeed = int.MinValue;
		}
		else
		{
			int seed = WorldGenerator.instance.GetSeed();
			Vector3 position = base.transform.position;
			Vector2i zone = ZoneSystem.instance.GetZone(base.transform.position);
			this.m_generatedSeed = seed + zone.x * 4271 + zone.y * -7187 + (int)position.x * -4271 + (int)position.y * 9187 + (int)position.z * -2134;
		}
		this.m_hasGeneratedSeed = true;
		return this.m_generatedSeed;
	}

	// Token: 0x06001B2C RID: 6956 RVA: 0x000B56C0 File Offset: 0x000B38C0
	public void Generate(int seed, ZoneSystem.SpawnMode mode)
	{
		DateTime now = DateTime.Now;
		this.m_generatedSeed = seed;
		this.Clear();
		this.SetupColliders();
		this.SetupAvailableRooms();
		if (ZoneSystem.instance)
		{
			Vector2i zone = ZoneSystem.instance.GetZone(base.transform.position);
			this.m_zoneCenter = ZoneSystem.instance.GetZonePos(zone);
			this.m_zoneCenter.y = base.transform.position.y - this.m_originalPosition.y;
		}
		Bounds bounds = new Bounds(this.m_zoneCenter, this.m_zoneSize);
		ZLog.Log(string.Format("Generating {0}, Seed: {1}, Bounds diff: {2} / {3}", new object[]
		{
			base.name,
			seed,
			bounds.min - base.transform.position,
			bounds.max - base.transform.position
		}));
		ZLog.Log("Available rooms:" + DungeonGenerator.m_availableRooms.Count.ToString());
		ZLog.Log("To place:" + this.m_maxRooms.ToString());
		DungeonGenerator.m_placedRooms.Clear();
		DungeonGenerator.m_openConnections.Clear();
		DungeonGenerator.m_doorConnections.Clear();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		this.GenerateRooms(mode);
		this.Save();
		ZLog.Log("Placed " + DungeonGenerator.m_placedRooms.Count.ToString() + " rooms");
		string text = "";
		foreach (Room room in DungeonGenerator.m_placedRooms)
		{
			text += room.name;
		}
		this.m_generatedHash = text.GetHashCode();
		ZLog.Log(string.Format("Dungeon generated with seed {0} and hash {1}", seed, this.m_generatedHash));
		UnityEngine.Random.state = state;
		SnapToGround.SnappAll();
		if (mode == ZoneSystem.SpawnMode.Ghost)
		{
			foreach (Room room2 in DungeonGenerator.m_placedRooms)
			{
				UnityEngine.Object.DestroyImmediate(room2.gameObject);
			}
		}
		DungeonGenerator.m_placedRooms.Clear();
		DungeonGenerator.m_openConnections.Clear();
		DungeonGenerator.m_doorConnections.Clear();
		UnityEngine.Object.DestroyImmediate(this.m_colliderA);
		UnityEngine.Object.DestroyImmediate(this.m_colliderB);
		DateTime.Now - now;
	}

	// Token: 0x06001B2D RID: 6957 RVA: 0x000B5970 File Offset: 0x000B3B70
	private void GenerateRooms(ZoneSystem.SpawnMode mode)
	{
		switch (this.m_algorithm)
		{
		case DungeonGenerator.Algorithm.Dungeon:
			this.GenerateDungeon(mode);
			return;
		case DungeonGenerator.Algorithm.CampGrid:
			this.GenerateCampGrid(mode);
			return;
		case DungeonGenerator.Algorithm.CampRadial:
			this.GenerateCampRadial(mode);
			return;
		default:
			return;
		}
	}

	// Token: 0x06001B2E RID: 6958 RVA: 0x000B59AE File Offset: 0x000B3BAE
	private void GenerateDungeon(ZoneSystem.SpawnMode mode)
	{
		this.PlaceStartRoom(mode);
		this.PlaceRooms(mode);
		this.PlaceEndCaps(mode);
		this.PlaceDoors(mode);
	}

	// Token: 0x06001B2F RID: 6959 RVA: 0x000B59CC File Offset: 0x000B3BCC
	private void GenerateCampGrid(ZoneSystem.SpawnMode mode)
	{
		float num = Mathf.Cos(0.017453292f * this.m_maxTilt);
		Vector3 a = base.transform.position + new Vector3((float)(-(float)this.m_gridSize) * this.m_tileWidth * 0.5f, 0f, (float)(-(float)this.m_gridSize) * this.m_tileWidth * 0.5f);
		for (int i = 0; i < this.m_gridSize; i++)
		{
			for (int j = 0; j < this.m_gridSize; j++)
			{
				if (UnityEngine.Random.value <= this.m_spawnChance)
				{
					Vector3 pos = a + new Vector3((float)j * this.m_tileWidth, 0f, (float)i * this.m_tileWidth);
					DungeonDB.RoomData randomWeightedRoom = this.GetRandomWeightedRoom(false);
					if (randomWeightedRoom != null)
					{
						if (ZoneSystem.instance)
						{
							Vector3 vector;
							Heightmap.Biome biome;
							Heightmap.BiomeArea biomeArea;
							Heightmap heightmap;
							ZoneSystem.instance.GetGroundData(ref pos, out vector, out biome, out biomeArea, out heightmap);
							if (vector.y < num)
							{
								goto IL_FE;
							}
						}
						Quaternion rot = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 16) * 22.5f, 0f);
						this.PlaceRoom(randomWeightedRoom, pos, rot, null, mode);
					}
				}
				IL_FE:;
			}
		}
	}

	// Token: 0x06001B30 RID: 6960 RVA: 0x000B5AF8 File Offset: 0x000B3CF8
	private void GenerateCampRadial(ZoneSystem.SpawnMode mode)
	{
		float num = UnityEngine.Random.Range(this.m_campRadiusMin, this.m_campRadiusMax);
		float num2 = Mathf.Cos(0.017453292f * this.m_maxTilt);
		int num3 = UnityEngine.Random.Range(this.m_minRooms, this.m_maxRooms);
		int num4 = num3 * 20;
		int num5 = 0;
		for (int i = 0; i < num4; i++)
		{
			Vector3 vector = base.transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(0f, num - this.m_perimeterBuffer);
			DungeonDB.RoomData randomWeightedRoom = this.GetRandomWeightedRoom(false);
			if (randomWeightedRoom != null)
			{
				if (ZoneSystem.instance)
				{
					Vector3 vector2;
					Heightmap.Biome biome;
					Heightmap.BiomeArea biomeArea;
					Heightmap heightmap;
					ZoneSystem.instance.GetGroundData(ref vector, out vector2, out biome, out biomeArea, out heightmap);
					if (vector2.y < num2 || vector.y - ZoneSystem.instance.m_waterLevel < this.m_minAltitude)
					{
						goto IL_11D;
					}
				}
				Quaternion campRoomRotation = this.GetCampRoomRotation(randomWeightedRoom, vector);
				if (!this.TestCollision(randomWeightedRoom.m_room, vector, campRoomRotation))
				{
					this.PlaceRoom(randomWeightedRoom, vector, campRoomRotation, null, mode);
					num5++;
					if (num5 >= num3)
					{
						break;
					}
				}
			}
			IL_11D:;
		}
		if (this.m_perimeterSections > 0)
		{
			this.PlaceWall(num, this.m_perimeterSections, mode);
		}
	}

	// Token: 0x06001B31 RID: 6961 RVA: 0x000B5C48 File Offset: 0x000B3E48
	private Quaternion GetCampRoomRotation(DungeonDB.RoomData room, Vector3 pos)
	{
		if (room.m_room.m_faceCenter)
		{
			Vector3 vector = base.transform.position - pos;
			vector.y = 0f;
			if (vector == Vector3.zero)
			{
				vector = Vector3.forward;
			}
			vector.Normalize();
			float y = Mathf.Round(Utils.YawFromDirection(vector) / 22.5f) * 22.5f;
			return Quaternion.Euler(0f, y, 0f);
		}
		return Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 16) * 22.5f, 0f);
	}

	// Token: 0x06001B32 RID: 6962 RVA: 0x000B5CE4 File Offset: 0x000B3EE4
	private void PlaceWall(float radius, int sections, ZoneSystem.SpawnMode mode)
	{
		float num = Mathf.Cos(0.017453292f * this.m_maxTilt);
		int num2 = 0;
		int num3 = sections * 20;
		for (int i = 0; i < num3; i++)
		{
			DungeonDB.RoomData randomWeightedRoom = this.GetRandomWeightedRoom(true);
			if (randomWeightedRoom != null)
			{
				Vector3 vector = base.transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * radius;
				Quaternion campRoomRotation = this.GetCampRoomRotation(randomWeightedRoom, vector);
				if (ZoneSystem.instance)
				{
					Vector3 vector2;
					Heightmap.Biome biome;
					Heightmap.BiomeArea biomeArea;
					Heightmap heightmap;
					ZoneSystem.instance.GetGroundData(ref vector, out vector2, out biome, out biomeArea, out heightmap);
					if (vector2.y < num || vector.y - ZoneSystem.instance.m_waterLevel < this.m_minAltitude)
					{
						goto IL_E6;
					}
				}
				if (!this.TestCollision(randomWeightedRoom.m_room, vector, campRoomRotation))
				{
					this.PlaceRoom(randomWeightedRoom, vector, campRoomRotation, null, mode);
					num2++;
					if (num2 >= sections)
					{
						break;
					}
				}
			}
			IL_E6:;
		}
	}

	// Token: 0x06001B33 RID: 6963 RVA: 0x000B5DE4 File Offset: 0x000B3FE4
	private void Save()
	{
		if (this.m_nview == null)
		{
			return;
		}
		ZDO zdo = this.m_nview.GetZDO();
		zdo.Set(ZDOVars.s_rooms, DungeonGenerator.m_placedRooms.Count, false);
		for (int i = 0; i < DungeonGenerator.m_placedRooms.Count; i++)
		{
			Room room = DungeonGenerator.m_placedRooms[i];
			string text = "room" + i.ToString();
			zdo.Set(text, room.GetHash());
			zdo.Set(text + "_pos", room.transform.position);
			zdo.Set(text + "_rot", room.transform.rotation);
			zdo.Set(text + "_seed", room.m_seed);
		}
	}

	// Token: 0x06001B34 RID: 6964 RVA: 0x000B5EB4 File Offset: 0x000B40B4
	private void Load()
	{
		if (this.m_nview == null)
		{
			return;
		}
		DateTime now = DateTime.Now;
		ZLog.Log("Loading dungeon");
		ZDO zdo = this.m_nview.GetZDO();
		int @int = zdo.GetInt(ZDOVars.s_rooms, 0);
		for (int i = 0; i < @int; i++)
		{
			string text = "room" + i.ToString();
			int int2 = zdo.GetInt(text, 0);
			Vector3 vec = zdo.GetVec3(text + "_pos", Vector3.zero);
			Quaternion quaternion = zdo.GetQuaternion(text + "_rot", Quaternion.identity);
			DungeonDB.RoomData room = DungeonDB.instance.GetRoom(int2);
			if (room == null)
			{
				ZLog.LogWarning("Missing room:" + int2.ToString());
			}
			else
			{
				this.PlaceRoom(room, vec, quaternion, null, ZoneSystem.SpawnMode.Client);
			}
		}
		ZLog.Log("Dungeon loaded " + @int.ToString());
		ZLog.Log("Dungeon load time " + (DateTime.Now - now).TotalMilliseconds.ToString() + " ms");
	}

	// Token: 0x06001B35 RID: 6965 RVA: 0x000B5FE0 File Offset: 0x000B41E0
	private void SetupAvailableRooms()
	{
		DungeonGenerator.m_availableRooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonDB.GetRooms())
		{
			if ((roomData.m_room.m_theme & this.m_themes) != (Room.Theme)0 && roomData.m_room.m_enabled)
			{
				DungeonGenerator.m_availableRooms.Add(roomData);
			}
		}
	}

	// Token: 0x06001B36 RID: 6966 RVA: 0x000B6064 File Offset: 0x000B4264
	private DungeonGenerator.DoorDef FindDoorType(string type)
	{
		List<DungeonGenerator.DoorDef> list = new List<DungeonGenerator.DoorDef>();
		foreach (DungeonGenerator.DoorDef doorDef in this.m_doorTypes)
		{
			if (doorDef.m_connectionType == type)
			{
				list.Add(doorDef);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x06001B37 RID: 6967 RVA: 0x000B60E8 File Offset: 0x000B42E8
	private void PlaceDoors(ZoneSystem.SpawnMode mode)
	{
		int num = 0;
		foreach (RoomConnection roomConnection in DungeonGenerator.m_doorConnections)
		{
			DungeonGenerator.DoorDef doorDef = this.FindDoorType(roomConnection.m_type);
			if (doorDef == null)
			{
				ZLog.Log("No door type for connection:" + roomConnection.m_type);
			}
			else if ((doorDef.m_chance <= 0f || UnityEngine.Random.value <= doorDef.m_chance) && (doorDef.m_chance > 0f || UnityEngine.Random.value <= this.m_doorChance))
			{
				GameObject obj = UnityEngine.Object.Instantiate<GameObject>(doorDef.m_prefab, roomConnection.transform.position, roomConnection.transform.rotation);
				if (mode == ZoneSystem.SpawnMode.Ghost)
				{
					UnityEngine.Object.Destroy(obj);
				}
				num++;
			}
		}
		ZLog.Log("placed " + num.ToString() + " doors");
	}

	// Token: 0x06001B38 RID: 6968 RVA: 0x000B61E4 File Offset: 0x000B43E4
	private void PlaceEndCaps(ZoneSystem.SpawnMode mode)
	{
		for (int i = 0; i < DungeonGenerator.m_openConnections.Count; i++)
		{
			RoomConnection roomConnection = DungeonGenerator.m_openConnections[i];
			RoomConnection roomConnection2 = null;
			for (int j = 0; j < DungeonGenerator.m_openConnections.Count; j++)
			{
				if (j != i && roomConnection.TestContact(DungeonGenerator.m_openConnections[j]))
				{
					roomConnection2 = DungeonGenerator.m_openConnections[j];
					break;
				}
			}
			if (roomConnection2 != null)
			{
				if (roomConnection.m_type != roomConnection2.m_type)
				{
					this.FindDividers(DungeonGenerator.m_tempRooms);
					if (DungeonGenerator.m_tempRooms.Count > 0)
					{
						DungeonDB.RoomData weightedRoom = this.GetWeightedRoom(DungeonGenerator.m_tempRooms);
						RoomConnection[] connections = weightedRoom.m_room.GetConnections();
						Vector3 vector;
						Quaternion rot;
						this.CalculateRoomPosRot(connections[0], roomConnection.transform.position, roomConnection.transform.rotation, out vector, out rot);
						bool flag = false;
						foreach (Room room in DungeonGenerator.m_placedRooms)
						{
							if (room.m_divider && Vector3.Distance(room.transform.position, vector) < 0.5f)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							this.PlaceRoom(weightedRoom, vector, rot, roomConnection, mode);
							ZLog.Log(string.Concat(new string[]
							{
								"Cyclic detected. Door missmatch for cyclic room '",
								roomConnection.m_type,
								"'-'",
								roomConnection2.m_type,
								"', placing divider: ",
								weightedRoom.m_room.name
							}));
						}
					}
					else
					{
						ZLog.LogWarning(string.Concat(new string[]
						{
							"Cyclic detected. Door missmatch for cyclic room '",
							roomConnection.m_type,
							"'-'",
							roomConnection2.m_type,
							"', but no dividers defined!"
						}));
					}
				}
				else
				{
					ZLog.Log("cyclic detected and door types match, cool");
				}
			}
			else
			{
				this.FindEndCaps(roomConnection, DungeonGenerator.m_tempRooms);
				bool flag2 = false;
				if (this.m_alternativeFunctionality)
				{
					for (int k = 0; k < 5; k++)
					{
						DungeonDB.RoomData weightedRoom2 = this.GetWeightedRoom(DungeonGenerator.m_tempRooms);
						if (this.PlaceRoom(roomConnection, weightedRoom2, mode))
						{
							flag2 = true;
							break;
						}
					}
				}
				IOrderedEnumerable<DungeonDB.RoomData> orderedEnumerable = from item in DungeonGenerator.m_tempRooms
				orderby item.m_room.m_endCapPrio descending
				select item;
				if (!flag2)
				{
					foreach (DungeonDB.RoomData roomData in orderedEnumerable)
					{
						if (this.PlaceRoom(roomConnection, roomData, mode))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2)
				{
					ZLog.LogWarning("Failed to place end cap " + roomConnection.name + " " + roomConnection.transform.parent.gameObject.name);
				}
			}
		}
	}

	// Token: 0x06001B39 RID: 6969 RVA: 0x000B64E0 File Offset: 0x000B46E0
	private void FindDividers(List<DungeonDB.RoomData> rooms)
	{
		rooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (roomData.m_room.m_divider)
			{
				rooms.Add(roomData);
			}
		}
		rooms.Shuffle(true);
	}

	// Token: 0x06001B3A RID: 6970 RVA: 0x000B654C File Offset: 0x000B474C
	private void FindEndCaps(RoomConnection connection, List<DungeonDB.RoomData> rooms)
	{
		rooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (roomData.m_room.m_endCap && roomData.m_room.HaveConnection(connection))
			{
				rooms.Add(roomData);
			}
		}
		rooms.Shuffle(true);
	}

	// Token: 0x06001B3B RID: 6971 RVA: 0x000B65C8 File Offset: 0x000B47C8
	private DungeonDB.RoomData FindEndCap(RoomConnection connection)
	{
		DungeonGenerator.m_tempRooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (roomData.m_room.m_endCap && roomData.m_room.HaveConnection(connection))
			{
				DungeonGenerator.m_tempRooms.Add(roomData);
			}
		}
		if (DungeonGenerator.m_tempRooms.Count == 0)
		{
			return null;
		}
		return DungeonGenerator.m_tempRooms[UnityEngine.Random.Range(0, DungeonGenerator.m_tempRooms.Count)];
	}

	// Token: 0x06001B3C RID: 6972 RVA: 0x000B666C File Offset: 0x000B486C
	private void PlaceRooms(ZoneSystem.SpawnMode mode)
	{
		for (int i = 0; i < this.m_maxRooms; i++)
		{
			this.PlaceOneRoom(mode);
			if (this.CheckRequiredRooms() && DungeonGenerator.m_placedRooms.Count > this.m_minRooms)
			{
				ZLog.Log("All required rooms have been placed, stopping generation");
				return;
			}
		}
	}

	// Token: 0x06001B3D RID: 6973 RVA: 0x000B66B8 File Offset: 0x000B48B8
	private void PlaceStartRoom(ZoneSystem.SpawnMode mode)
	{
		DungeonDB.RoomData roomData = this.FindStartRoom();
		RoomConnection entrance = roomData.m_room.GetEntrance();
		Quaternion rotation = base.transform.rotation;
		Vector3 pos;
		Quaternion rot;
		this.CalculateRoomPosRot(entrance, base.transform.position, rotation, out pos, out rot);
		this.PlaceRoom(roomData, pos, rot, entrance, mode);
	}

	// Token: 0x06001B3E RID: 6974 RVA: 0x000B6708 File Offset: 0x000B4908
	private bool PlaceOneRoom(ZoneSystem.SpawnMode mode)
	{
		RoomConnection openConnection = this.GetOpenConnection();
		if (openConnection == null)
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			DungeonDB.RoomData roomData = this.m_alternativeFunctionality ? this.GetRandomWeightedRoom(openConnection) : this.GetRandomRoom(openConnection);
			if (roomData == null)
			{
				break;
			}
			if (this.PlaceRoom(openConnection, roomData, mode))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001B3F RID: 6975 RVA: 0x000B6760 File Offset: 0x000B4960
	private void CalculateRoomPosRot(RoomConnection roomCon, Vector3 exitPos, Quaternion exitRot, out Vector3 pos, out Quaternion rot)
	{
		Quaternion rhs = Quaternion.Inverse(roomCon.transform.localRotation);
		rot = exitRot * rhs;
		Vector3 localPosition = roomCon.transform.localPosition;
		pos = exitPos - rot * localPosition;
	}

	// Token: 0x06001B40 RID: 6976 RVA: 0x000B67B4 File Offset: 0x000B49B4
	private bool PlaceRoom(RoomConnection connection, DungeonDB.RoomData roomData, ZoneSystem.SpawnMode mode)
	{
		Room room = roomData.m_room;
		Quaternion quaternion = connection.transform.rotation;
		quaternion *= Quaternion.Euler(0f, 180f, 0f);
		RoomConnection connection2 = room.GetConnection(connection);
		if (connection2.transform.parent.gameObject != room.gameObject)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Connection '",
				room.name,
				"->",
				connection2.name,
				"' is not placed as a direct child of room!"
			}));
		}
		Vector3 pos;
		Quaternion rot;
		this.CalculateRoomPosRot(connection2, connection.transform.position, quaternion, out pos, out rot);
		if (room.m_size.x != 0 && room.m_size.z != 0 && this.TestCollision(room, pos, rot))
		{
			return false;
		}
		this.PlaceRoom(roomData, pos, rot, connection, mode);
		if (!room.m_endCap)
		{
			if (connection.m_allowDoor && (!connection.m_doorOnlyIfOtherAlsoAllowsDoor || connection2.m_allowDoor))
			{
				DungeonGenerator.m_doorConnections.Add(connection);
			}
			DungeonGenerator.m_openConnections.Remove(connection);
		}
		return true;
	}

	// Token: 0x06001B41 RID: 6977 RVA: 0x000B68D4 File Offset: 0x000B4AD4
	private void PlaceRoom(DungeonDB.RoomData room, Vector3 pos, Quaternion rot, RoomConnection fromConnection, ZoneSystem.SpawnMode mode)
	{
		Vector3 vector = pos;
		if (this.m_useCustomInteriorTransform)
		{
			vector = pos - base.transform.position;
		}
		int seed = (int)vector.x * 4271 + (int)vector.y * 9187 + (int)vector.z * 2134;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
		{
			foreach (ZNetView znetView in room.m_netViews)
			{
				znetView.gameObject.SetActive(true);
			}
			UnityEngine.Random.InitState(seed);
			foreach (RandomSpawn randomSpawn in room.m_randomSpawns)
			{
				randomSpawn.Randomize();
			}
			Vector3 position = room.m_room.transform.position;
			Quaternion quaternion = Quaternion.Inverse(room.m_room.transform.rotation);
			using (List<ZNetView>.Enumerator enumerator = room.m_netViews.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZNetView znetView2 = enumerator.Current;
					if (znetView2.gameObject.activeSelf)
					{
						Vector3 point = quaternion * (znetView2.gameObject.transform.position - position);
						Vector3 position2 = pos + rot * point;
						Quaternion rhs = quaternion * znetView2.gameObject.transform.rotation;
						Quaternion rotation = rot * rhs;
						GameObject obj = UnityEngine.Object.Instantiate<GameObject>(znetView2.gameObject, position2, rotation);
						if (mode == ZoneSystem.SpawnMode.Ghost)
						{
							UnityEngine.Object.Destroy(obj);
						}
					}
				}
				goto IL_1ED;
			}
		}
		UnityEngine.Random.InitState(seed);
		foreach (RandomSpawn randomSpawn2 in room.m_randomSpawns)
		{
			randomSpawn2.Randomize();
		}
		IL_1ED:
		foreach (ZNetView znetView3 in room.m_netViews)
		{
			znetView3.gameObject.SetActive(false);
		}
		Room component = UnityEngine.Object.Instantiate<GameObject>(room.m_room.gameObject, pos, rot, base.transform).GetComponent<Room>();
		component.gameObject.name = room.m_room.gameObject.name;
		if (mode != ZoneSystem.SpawnMode.Client)
		{
			component.m_placeOrder = (fromConnection ? (fromConnection.m_placeOrder + 1) : 0);
			component.m_seed = seed;
			DungeonGenerator.m_placedRooms.Add(component);
			this.AddOpenConnections(component, fromConnection);
		}
		UnityEngine.Random.state = state;
	}

	// Token: 0x06001B42 RID: 6978 RVA: 0x000B6BC4 File Offset: 0x000B4DC4
	private void AddOpenConnections(Room newRoom, RoomConnection skipConnection)
	{
		RoomConnection[] connections = newRoom.GetConnections();
		if (skipConnection != null)
		{
			foreach (RoomConnection roomConnection in connections)
			{
				if (!roomConnection.m_entrance && Vector3.Distance(roomConnection.transform.position, skipConnection.transform.position) >= 0.1f)
				{
					roomConnection.m_placeOrder = newRoom.m_placeOrder;
					DungeonGenerator.m_openConnections.Add(roomConnection);
				}
			}
			return;
		}
		RoomConnection[] array = connections;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].m_placeOrder = newRoom.m_placeOrder;
		}
		DungeonGenerator.m_openConnections.AddRange(connections);
	}

	// Token: 0x06001B43 RID: 6979 RVA: 0x000B6C60 File Offset: 0x000B4E60
	private void SetupColliders()
	{
		if (this.m_colliderA != null)
		{
			return;
		}
		BoxCollider[] componentsInChildren = base.gameObject.GetComponentsInChildren<BoxCollider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
		}
		this.m_colliderA = base.gameObject.AddComponent<BoxCollider>();
		this.m_colliderB = base.gameObject.AddComponent<BoxCollider>();
	}

	// Token: 0x06001B44 RID: 6980 RVA: 0x000023E2 File Offset: 0x000005E2
	public void Derp()
	{
	}

	// Token: 0x06001B45 RID: 6981 RVA: 0x000B6CC0 File Offset: 0x000B4EC0
	private bool IsInsideDungeon(Room room, Vector3 pos, Quaternion rot)
	{
		Bounds bounds = new Bounds(this.m_zoneCenter, this.m_zoneSize);
		Vector3 vector = room.m_size;
		vector *= 0.5f;
		return bounds.Contains(pos + rot * new Vector3(vector.x, vector.y, -vector.z)) && bounds.Contains(pos + rot * new Vector3(-vector.x, vector.y, -vector.z)) && bounds.Contains(pos + rot * new Vector3(vector.x, vector.y, vector.z)) && bounds.Contains(pos + rot * new Vector3(-vector.x, vector.y, vector.z)) && bounds.Contains(pos + rot * new Vector3(vector.x, -vector.y, -vector.z)) && bounds.Contains(pos + rot * new Vector3(-vector.x, -vector.y, -vector.z)) && bounds.Contains(pos + rot * new Vector3(vector.x, -vector.y, vector.z)) && bounds.Contains(pos + rot * new Vector3(-vector.x, -vector.y, vector.z));
	}

	// Token: 0x06001B46 RID: 6982 RVA: 0x000B6E78 File Offset: 0x000B5078
	private bool TestCollision(Room room, Vector3 pos, Quaternion rot)
	{
		if (!this.IsInsideDungeon(room, pos, rot))
		{
			return true;
		}
		this.m_colliderA.size = new Vector3((float)room.m_size.x - 0.1f, (float)room.m_size.y - 0.1f, (float)room.m_size.z - 0.1f);
		foreach (Room room2 in DungeonGenerator.m_placedRooms)
		{
			this.m_colliderB.size = room2.m_size;
			Vector3 vector;
			float num;
			if (Physics.ComputePenetration(this.m_colliderA, pos, rot, this.m_colliderB, room2.transform.position, room2.transform.rotation, out vector, out num))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001B47 RID: 6983 RVA: 0x000B6F64 File Offset: 0x000B5164
	private DungeonDB.RoomData GetRandomWeightedRoom(bool perimeterRoom)
	{
		DungeonGenerator.m_tempRooms.Clear();
		float num = 0f;
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (!roomData.m_room.m_entrance && !roomData.m_room.m_endCap && !roomData.m_room.m_divider && roomData.m_room.m_perimeter == perimeterRoom)
			{
				num += roomData.m_room.m_weight;
				DungeonGenerator.m_tempRooms.Add(roomData);
			}
		}
		if (DungeonGenerator.m_tempRooms.Count == 0)
		{
			return null;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (DungeonDB.RoomData roomData2 in DungeonGenerator.m_tempRooms)
		{
			num3 += roomData2.m_room.m_weight;
			if (num2 <= num3)
			{
				return roomData2;
			}
		}
		return DungeonGenerator.m_tempRooms[0];
	}

	// Token: 0x06001B48 RID: 6984 RVA: 0x000B7098 File Offset: 0x000B5298
	private DungeonDB.RoomData GetRandomWeightedRoom(RoomConnection connection)
	{
		DungeonGenerator.m_tempRooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (!roomData.m_room.m_entrance && !roomData.m_room.m_endCap && !roomData.m_room.m_divider && (!connection || (roomData.m_room.HaveConnection(connection) && connection.m_placeOrder >= roomData.m_room.m_minPlaceOrder)))
			{
				DungeonGenerator.m_tempRooms.Add(roomData);
			}
		}
		if (DungeonGenerator.m_tempRooms.Count == 0)
		{
			return null;
		}
		return this.GetWeightedRoom(DungeonGenerator.m_tempRooms);
	}

	// Token: 0x06001B49 RID: 6985 RVA: 0x000B7164 File Offset: 0x000B5364
	private DungeonDB.RoomData GetWeightedRoom(List<DungeonDB.RoomData> rooms)
	{
		float num = 0f;
		foreach (DungeonDB.RoomData roomData in rooms)
		{
			num += roomData.m_room.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (DungeonDB.RoomData roomData2 in rooms)
		{
			num3 += roomData2.m_room.m_weight;
			if (num2 <= num3)
			{
				return roomData2;
			}
		}
		return DungeonGenerator.m_tempRooms[0];
	}

	// Token: 0x06001B4A RID: 6986 RVA: 0x000B7234 File Offset: 0x000B5434
	private DungeonDB.RoomData GetRandomRoom(RoomConnection connection)
	{
		DungeonGenerator.m_tempRooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (!roomData.m_room.m_entrance && !roomData.m_room.m_endCap && !roomData.m_room.m_divider && (!connection || (roomData.m_room.HaveConnection(connection) && connection.m_placeOrder >= roomData.m_room.m_minPlaceOrder)))
			{
				DungeonGenerator.m_tempRooms.Add(roomData);
			}
		}
		if (DungeonGenerator.m_tempRooms.Count != 0)
		{
			return DungeonGenerator.m_tempRooms[UnityEngine.Random.Range(0, DungeonGenerator.m_tempRooms.Count)];
		}
		return null;
	}

	// Token: 0x06001B4B RID: 6987 RVA: 0x000B730C File Offset: 0x000B550C
	private RoomConnection GetOpenConnection()
	{
		if (DungeonGenerator.m_openConnections.Count != 0)
		{
			return DungeonGenerator.m_openConnections[UnityEngine.Random.Range(0, DungeonGenerator.m_openConnections.Count)];
		}
		return null;
	}

	// Token: 0x06001B4C RID: 6988 RVA: 0x000B7338 File Offset: 0x000B5538
	private DungeonDB.RoomData FindStartRoom()
	{
		DungeonGenerator.m_tempRooms.Clear();
		foreach (DungeonDB.RoomData roomData in DungeonGenerator.m_availableRooms)
		{
			if (roomData.m_room.m_entrance)
			{
				DungeonGenerator.m_tempRooms.Add(roomData);
			}
		}
		return DungeonGenerator.m_tempRooms[UnityEngine.Random.Range(0, DungeonGenerator.m_tempRooms.Count)];
	}

	// Token: 0x06001B4D RID: 6989 RVA: 0x000B73C0 File Offset: 0x000B55C0
	private bool CheckRequiredRooms()
	{
		if (this.m_minRequiredRooms == 0 || this.m_requiredRooms.Count == 0)
		{
			return false;
		}
		int num = 0;
		foreach (Room room in DungeonGenerator.m_placedRooms)
		{
			if (this.m_requiredRooms.Contains(room.gameObject.name))
			{
				num++;
			}
		}
		return num >= this.m_minRequiredRooms;
	}

	// Token: 0x06001B4E RID: 6990 RVA: 0x000B744C File Offset: 0x000B564C
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0f, 1.5f, 0f, 0.5f);
		Gizmos.DrawWireCube(this.m_zoneCenter, new Vector3(this.m_zoneSize.x, this.m_zoneSize.y, this.m_zoneSize.z));
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x04001D74 RID: 7540
	public static int m_forceSeed = int.MinValue;

	// Token: 0x04001D75 RID: 7541
	public DungeonGenerator.Algorithm m_algorithm;

	// Token: 0x04001D76 RID: 7542
	public int m_maxRooms = 3;

	// Token: 0x04001D77 RID: 7543
	public int m_minRooms = 20;

	// Token: 0x04001D78 RID: 7544
	public int m_minRequiredRooms;

	// Token: 0x04001D79 RID: 7545
	public List<string> m_requiredRooms = new List<string>();

	// Token: 0x04001D7A RID: 7546
	public bool m_alternativeFunctionality;

	// Token: 0x04001D7B RID: 7547
	[BitMask(typeof(Room.Theme))]
	public Room.Theme m_themes = Room.Theme.Crypt;

	// Token: 0x04001D7C RID: 7548
	[Header("Dungeon")]
	public List<DungeonGenerator.DoorDef> m_doorTypes = new List<DungeonGenerator.DoorDef>();

	// Token: 0x04001D7D RID: 7549
	[Range(0f, 1f)]
	public float m_doorChance = 0.5f;

	// Token: 0x04001D7E RID: 7550
	[Header("Camp")]
	public float m_maxTilt = 10f;

	// Token: 0x04001D7F RID: 7551
	public float m_tileWidth = 8f;

	// Token: 0x04001D80 RID: 7552
	public int m_gridSize = 4;

	// Token: 0x04001D81 RID: 7553
	public float m_spawnChance = 1f;

	// Token: 0x04001D82 RID: 7554
	[Header("Camp radial")]
	public float m_campRadiusMin = 15f;

	// Token: 0x04001D83 RID: 7555
	public float m_campRadiusMax = 30f;

	// Token: 0x04001D84 RID: 7556
	public float m_minAltitude = 1f;

	// Token: 0x04001D85 RID: 7557
	public int m_perimeterSections;

	// Token: 0x04001D86 RID: 7558
	public float m_perimeterBuffer = 2f;

	// Token: 0x04001D87 RID: 7559
	[Header("Misc")]
	public Vector3 m_zoneCenter = new Vector3(0f, 0f, 0f);

	// Token: 0x04001D88 RID: 7560
	public Vector3 m_zoneSize = new Vector3(64f, 64f, 64f);

	// Token: 0x04001D89 RID: 7561
	public bool m_useCustomInteriorTransform;

	// Token: 0x04001D8A RID: 7562
	[HideInInspector]
	public int m_generatedSeed;

	// Token: 0x04001D8B RID: 7563
	[HideInInspector]
	public int m_generatedHash;

	// Token: 0x04001D8C RID: 7564
	private bool m_hasGeneratedSeed;

	// Token: 0x04001D8D RID: 7565
	private static List<Room> m_placedRooms = new List<Room>();

	// Token: 0x04001D8E RID: 7566
	private static List<RoomConnection> m_openConnections = new List<RoomConnection>();

	// Token: 0x04001D8F RID: 7567
	private static List<RoomConnection> m_doorConnections = new List<RoomConnection>();

	// Token: 0x04001D90 RID: 7568
	private static List<DungeonDB.RoomData> m_availableRooms = new List<DungeonDB.RoomData>();

	// Token: 0x04001D91 RID: 7569
	private static List<DungeonDB.RoomData> m_tempRooms = new List<DungeonDB.RoomData>();

	// Token: 0x04001D92 RID: 7570
	private BoxCollider m_colliderA;

	// Token: 0x04001D93 RID: 7571
	private BoxCollider m_colliderB;

	// Token: 0x04001D94 RID: 7572
	private ZNetView m_nview;

	// Token: 0x04001D95 RID: 7573
	[HideInInspector]
	public Vector3 m_originalPosition;

	// Token: 0x020002CE RID: 718
	[Serializable]
	public class DoorDef
	{
		// Token: 0x04001D96 RID: 7574
		public GameObject m_prefab;

		// Token: 0x04001D97 RID: 7575
		public string m_connectionType = "";

		// Token: 0x04001D98 RID: 7576
		[global::Tooltip("Will use default door chance set in DungeonGenerator if set to zero to default to old behaviour")]
		[Range(0f, 1f)]
		public float m_chance;
	}

	// Token: 0x020002CF RID: 719
	public enum Algorithm
	{
		// Token: 0x04001D9A RID: 7578
		Dungeon,
		// Token: 0x04001D9B RID: 7579
		CampGrid,
		// Token: 0x04001D9C RID: 7580
		CampRadial
	}
}
