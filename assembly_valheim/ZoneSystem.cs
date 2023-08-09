using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000209 RID: 521
public class ZoneSystem : MonoBehaviour
{
	// Token: 0x170000E1 RID: 225
	// (get) Token: 0x060014C1 RID: 5313 RVA: 0x00086F00 File Offset: 0x00085100
	public static ZoneSystem instance
	{
		get
		{
			return ZoneSystem.m_instance;
		}
	}

	// Token: 0x060014C2 RID: 5314 RVA: 0x00086F08 File Offset: 0x00085108
	private void Awake()
	{
		ZoneSystem.m_instance = this;
		this.m_terrainRayMask = LayerMask.GetMask(new string[]
		{
			"terrain"
		});
		this.m_blockRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece"
		});
		this.m_solidRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"terrain"
		});
		this.m_staticSolidRayMask = LayerMask.GetMask(new string[]
		{
			"static_solid",
			"terrain"
		});
		foreach (string text in this.m_locationScenes)
		{
			if (SceneManager.GetSceneByName(text).IsValid())
			{
				ZLog.Log("Location scene " + text + " already loaded");
			}
			else
			{
				SceneManager.LoadScene(text, LoadSceneMode.Additive);
			}
		}
		ZLog.Log("Zonesystem Awake " + Time.frameCount.ToString());
	}

	// Token: 0x060014C3 RID: 5315 RVA: 0x00087048 File Offset: 0x00085248
	private void Start()
	{
		ZLog.Log("Zonesystem Start " + Time.frameCount.ToString());
		this.SetupLocations();
		this.ValidateVegetation();
		ZRoutedRpc instance = ZRoutedRpc.instance;
		instance.m_onNewPeer = (Action<long>)Delegate.Combine(instance.m_onNewPeer, new Action<long>(this.OnNewPeer));
		if (ZNet.instance.IsServer())
		{
			ZRoutedRpc.instance.Register<string>("SetGlobalKey", new Action<long, string>(this.RPC_SetGlobalKey));
			ZRoutedRpc.instance.Register<string>("RemoveGlobalKey", new Action<long, string>(this.RPC_RemoveGlobalKey));
		}
		else
		{
			ZRoutedRpc.instance.Register<List<string>>("GlobalKeys", new Action<long, List<string>>(this.RPC_GlobalKeys));
			ZRoutedRpc.instance.Register<ZPackage>("LocationIcons", new Action<long, ZPackage>(this.RPC_LocationIcons));
		}
		this.m_startTime = (this.m_lastFixedTime = Time.fixedTime);
	}

	// Token: 0x060014C4 RID: 5316 RVA: 0x00087131 File Offset: 0x00085331
	public void GenerateLocationsIfNeeded()
	{
		if (!this.m_locationsGenerated && ZNet.instance.IsServer())
		{
			this.GenerateLocations();
		}
	}

	// Token: 0x060014C5 RID: 5317 RVA: 0x00087150 File Offset: 0x00085350
	private void SendGlobalKeys(long peer)
	{
		List<string> list = new List<string>(this.m_globalKeys);
		ZRoutedRpc.instance.InvokeRoutedRPC(peer, "GlobalKeys", new object[]
		{
			list
		});
	}

	// Token: 0x060014C6 RID: 5318 RVA: 0x00087184 File Offset: 0x00085384
	private void RPC_GlobalKeys(long sender, List<string> keys)
	{
		ZLog.Log("client got keys " + keys.Count.ToString());
		this.m_globalKeys.Clear();
		foreach (string item in keys)
		{
			this.m_globalKeys.Add(item);
		}
	}

	// Token: 0x060014C7 RID: 5319 RVA: 0x00087200 File Offset: 0x00085400
	private void SendLocationIcons(long peer)
	{
		ZPackage zpackage = new ZPackage();
		this.tempIconList.Clear();
		this.GetLocationIcons(this.tempIconList);
		zpackage.Write(this.tempIconList.Count);
		foreach (KeyValuePair<Vector3, string> keyValuePair in this.tempIconList)
		{
			zpackage.Write(keyValuePair.Key);
			zpackage.Write(keyValuePair.Value);
		}
		ZRoutedRpc.instance.InvokeRoutedRPC(peer, "LocationIcons", new object[]
		{
			zpackage
		});
	}

	// Token: 0x060014C8 RID: 5320 RVA: 0x000872B0 File Offset: 0x000854B0
	private void RPC_LocationIcons(long sender, ZPackage pkg)
	{
		ZLog.Log("client got location icons");
		this.m_locationIcons.Clear();
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			Vector3 key = pkg.ReadVector3();
			string value = pkg.ReadString();
			this.m_locationIcons[key] = value;
		}
		ZLog.Log("Icons:" + num.ToString());
	}

	// Token: 0x060014C9 RID: 5321 RVA: 0x00087316 File Offset: 0x00085516
	private void OnNewPeer(long peerID)
	{
		if (ZNet.instance.IsServer())
		{
			ZLog.Log("Server: New peer connected,sending global keys");
			this.SendGlobalKeys(peerID);
			this.SendLocationIcons(peerID);
		}
	}

	// Token: 0x060014CA RID: 5322 RVA: 0x0008733C File Offset: 0x0008553C
	private void SetupLocations()
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		List<Location> list = new List<Location>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name == "_Locations")
			{
				Location[] componentsInChildren = gameObject.GetComponentsInChildren<Location>(true);
				list.AddRange(componentsInChildren);
			}
		}
		List<LocationList> allLocationLists = LocationList.GetAllLocationLists();
		allLocationLists.Sort((LocationList a, LocationList b) => a.m_sortOrder.CompareTo(b.m_sortOrder));
		foreach (LocationList locationList in allLocationLists)
		{
			this.m_locations.AddRange(locationList.m_locations);
			this.m_vegetation.AddRange(locationList.m_vegetation);
			foreach (EnvSetup env in locationList.m_environments)
			{
				EnvMan.instance.AppendEnvironment(env);
			}
			foreach (BiomeEnvSetup biomeEnv in locationList.m_biomeEnvironments)
			{
				EnvMan.instance.AppendBiomeSetup(biomeEnv);
			}
			ClutterSystem.instance.m_clutter.AddRange(locationList.m_clutter);
			ZLog.Log(string.Format("Added {0} locations, {1} vegetations, {2} environments, {3} biome env-setups, {4} clutter  from ", new object[]
			{
				locationList.m_locations.Count,
				locationList.m_vegetation.Count,
				locationList.m_environments.Count,
				locationList.m_biomeEnvironments.Count,
				locationList.m_clutter.Count
			}) + locationList.gameObject.scene.name);
			RandEventSystem.instance.m_events.AddRange(locationList.m_events);
		}
		using (List<Location>.Enumerator enumerator4 = list.GetEnumerator())
		{
			while (enumerator4.MoveNext())
			{
				if (enumerator4.Current.transform.gameObject.activeInHierarchy)
				{
					this.m_error = true;
				}
			}
		}
		foreach (ZoneSystem.ZoneLocation zoneLocation in this.m_locations)
		{
			Transform transform = null;
			foreach (Location location in list)
			{
				if (location.gameObject.name == zoneLocation.m_prefabName)
				{
					transform = location.transform;
					break;
				}
			}
			if (!(transform == null) || zoneLocation.m_enable)
			{
				zoneLocation.m_prefab = transform.gameObject;
				zoneLocation.m_hash = zoneLocation.m_prefab.name.GetStableHashCode();
				Location componentInChildren = zoneLocation.m_prefab.GetComponentInChildren<Location>();
				zoneLocation.m_location = componentInChildren;
				zoneLocation.m_interiorRadius = (componentInChildren.m_hasInterior ? componentInChildren.m_interiorRadius : 0f);
				zoneLocation.m_exteriorRadius = componentInChildren.m_exteriorRadius;
				if (componentInChildren.m_interiorTransform && componentInChildren.m_generator)
				{
					zoneLocation.m_interiorPosition = componentInChildren.m_interiorTransform.localPosition;
					zoneLocation.m_generatorPosition = componentInChildren.m_generator.transform.localPosition;
				}
				if (Application.isPlaying)
				{
					ZoneSystem.PrepareNetViews(zoneLocation.m_prefab, zoneLocation.m_netViews);
					ZoneSystem.PrepareRandomSpawns(zoneLocation.m_prefab, zoneLocation.m_randomSpawns);
					if (!this.m_locationsByHash.ContainsKey(zoneLocation.m_hash))
					{
						this.m_locationsByHash.Add(zoneLocation.m_hash, zoneLocation);
					}
				}
			}
		}
	}

	// Token: 0x060014CB RID: 5323 RVA: 0x000877C0 File Offset: 0x000859C0
	public static void PrepareNetViews(GameObject root, List<ZNetView> views)
	{
		views.Clear();
		foreach (ZNetView znetView in root.GetComponentsInChildren<ZNetView>(true))
		{
			if (Utils.IsEnabledInheirarcy(znetView.gameObject, root))
			{
				views.Add(znetView);
			}
		}
	}

	// Token: 0x060014CC RID: 5324 RVA: 0x00087804 File Offset: 0x00085A04
	public static void PrepareRandomSpawns(GameObject root, List<RandomSpawn> randomSpawns)
	{
		randomSpawns.Clear();
		foreach (RandomSpawn randomSpawn in root.GetComponentsInChildren<RandomSpawn>(true))
		{
			if (Utils.IsEnabledInheirarcy(randomSpawn.gameObject, root))
			{
				randomSpawns.Add(randomSpawn);
				randomSpawn.Prepare();
			}
		}
	}

	// Token: 0x060014CD RID: 5325 RVA: 0x0008784C File Offset: 0x00085A4C
	private void OnDestroy()
	{
		ZoneSystem.m_instance = null;
	}

	// Token: 0x060014CE RID: 5326 RVA: 0x00087854 File Offset: 0x00085A54
	private void ValidateVegetation()
	{
		foreach (ZoneSystem.ZoneVegetation zoneVegetation in this.m_vegetation)
		{
			if (zoneVegetation.m_enable && zoneVegetation.m_prefab && zoneVegetation.m_prefab.GetComponent<ZNetView>() == null)
			{
				ZLog.LogError(string.Concat(new string[]
				{
					"Vegetation ",
					zoneVegetation.m_prefab.name,
					" [ ",
					zoneVegetation.m_name,
					"] is missing ZNetView"
				}));
			}
		}
	}

	// Token: 0x060014CF RID: 5327 RVA: 0x00087908 File Offset: 0x00085B08
	public void PrepareSave()
	{
		this.m_tempGeneratedZonesSaveClone = new HashSet<Vector2i>(this.m_generatedZones);
		this.m_tempGlobalKeysSaveClone = new HashSet<string>(this.m_globalKeys);
		this.m_tempLocationsSaveClone = new List<ZoneSystem.LocationInstance>(this.m_locationInstances.Values);
		this.m_tempLocationsGeneratedSaveClone = this.m_locationsGenerated;
	}

	// Token: 0x060014D0 RID: 5328 RVA: 0x0008795C File Offset: 0x00085B5C
	public void SaveASync(BinaryWriter writer)
	{
		writer.Write(this.m_tempGeneratedZonesSaveClone.Count);
		foreach (Vector2i vector2i in this.m_tempGeneratedZonesSaveClone)
		{
			writer.Write(vector2i.x);
			writer.Write(vector2i.y);
		}
		writer.Write(0);
		writer.Write(this.m_locationVersion);
		writer.Write(this.m_tempGlobalKeysSaveClone.Count);
		foreach (string value in this.m_tempGlobalKeysSaveClone)
		{
			writer.Write(value);
		}
		writer.Write(this.m_tempLocationsGeneratedSaveClone);
		writer.Write(this.m_tempLocationsSaveClone.Count);
		foreach (ZoneSystem.LocationInstance locationInstance in this.m_tempLocationsSaveClone)
		{
			writer.Write(locationInstance.m_location.m_prefabName);
			writer.Write(locationInstance.m_position.x);
			writer.Write(locationInstance.m_position.y);
			writer.Write(locationInstance.m_position.z);
			writer.Write(locationInstance.m_placed);
		}
		this.m_tempGeneratedZonesSaveClone.Clear();
		this.m_tempGeneratedZonesSaveClone = null;
		this.m_tempGlobalKeysSaveClone.Clear();
		this.m_tempGlobalKeysSaveClone = null;
		this.m_tempLocationsSaveClone.Clear();
		this.m_tempLocationsSaveClone = null;
	}

	// Token: 0x060014D1 RID: 5329 RVA: 0x00087B1C File Offset: 0x00085D1C
	public void Load(BinaryReader reader, int version)
	{
		this.m_generatedZones.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Vector2i item = default(Vector2i);
			item.x = reader.ReadInt32();
			item.y = reader.ReadInt32();
			this.m_generatedZones.Add(item);
		}
		if (version >= 13)
		{
			reader.ReadInt32();
			int num2 = (version >= 21) ? reader.ReadInt32() : 0;
			if (version >= 14)
			{
				this.m_globalKeys.Clear();
				int num3 = reader.ReadInt32();
				for (int j = 0; j < num3; j++)
				{
					string item2 = reader.ReadString();
					this.m_globalKeys.Add(item2);
				}
			}
			if (version >= 18)
			{
				if (version >= 20)
				{
					this.m_locationsGenerated = reader.ReadBoolean();
				}
				this.m_locationInstances.Clear();
				int num4 = reader.ReadInt32();
				for (int k = 0; k < num4; k++)
				{
					string text = reader.ReadString();
					Vector3 zero = Vector3.zero;
					zero.x = reader.ReadSingle();
					zero.y = reader.ReadSingle();
					zero.z = reader.ReadSingle();
					bool generated = false;
					if (version >= 19)
					{
						generated = reader.ReadBoolean();
					}
					ZoneSystem.ZoneLocation location = this.GetLocation(text);
					if (location != null)
					{
						this.RegisterLocation(location, zero, generated);
					}
					else
					{
						ZLog.DevLog("Failed to find location " + text);
					}
				}
				ZLog.Log("Loaded " + num4.ToString() + " locations");
				if (num2 != this.m_locationVersion)
				{
					this.m_locationsGenerated = false;
				}
			}
		}
	}

	// Token: 0x060014D2 RID: 5330 RVA: 0x00087CB0 File Offset: 0x00085EB0
	private void Update()
	{
		this.m_lastFixedTime = Time.fixedTime;
		if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
		{
			return;
		}
		if (Terminal.m_showTests)
		{
			Terminal.m_testList["Time"] = Time.fixedTime.ToString("0.00") + " / " + this.TimeSinceStart().ToString("0.00");
		}
		this.m_updateTimer += Time.deltaTime;
		if (this.m_updateTimer > 0.1f)
		{
			this.m_updateTimer = 0f;
			bool flag = this.CreateLocalZones(ZNet.instance.GetReferencePosition());
			this.UpdateTTL(0.1f);
			if (ZNet.instance.IsServer() && !flag)
			{
				this.CreateGhostZones(ZNet.instance.GetReferencePosition());
				foreach (ZNetPeer znetPeer in ZNet.instance.GetPeers())
				{
					this.CreateGhostZones(znetPeer.GetRefPos());
				}
			}
		}
	}

	// Token: 0x060014D3 RID: 5331 RVA: 0x00087DD0 File Offset: 0x00085FD0
	private bool CreateGhostZones(Vector3 refPoint)
	{
		Vector2i zone = this.GetZone(refPoint);
		GameObject gameObject;
		if (!this.IsZoneGenerated(zone) && this.SpawnZone(zone, ZoneSystem.SpawnMode.Ghost, out gameObject))
		{
			return true;
		}
		int num = this.m_activeArea + this.m_activeDistantArea;
		for (int i = zone.y - num; i <= zone.y + num; i++)
		{
			for (int j = zone.x - num; j <= zone.x + num; j++)
			{
				Vector2i zoneID = new Vector2i(j, i);
				GameObject gameObject2;
				if (!this.IsZoneGenerated(zoneID) && this.SpawnZone(zoneID, ZoneSystem.SpawnMode.Ghost, out gameObject2))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060014D4 RID: 5332 RVA: 0x00087E68 File Offset: 0x00086068
	private bool CreateLocalZones(Vector3 refPoint)
	{
		Vector2i zone = this.GetZone(refPoint);
		if (this.PokeLocalZone(zone))
		{
			return true;
		}
		for (int i = zone.y - this.m_activeArea; i <= zone.y + this.m_activeArea; i++)
		{
			for (int j = zone.x - this.m_activeArea; j <= zone.x + this.m_activeArea; j++)
			{
				Vector2i vector2i = new Vector2i(j, i);
				if (!(vector2i == zone) && this.PokeLocalZone(vector2i))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060014D5 RID: 5333 RVA: 0x00087EF0 File Offset: 0x000860F0
	private bool PokeLocalZone(Vector2i zoneID)
	{
		ZoneSystem.ZoneData zoneData;
		if (this.m_zones.TryGetValue(zoneID, out zoneData))
		{
			zoneData.m_ttl = 0f;
			return false;
		}
		ZoneSystem.SpawnMode mode = (ZNet.instance.IsServer() && !this.IsZoneGenerated(zoneID)) ? ZoneSystem.SpawnMode.Full : ZoneSystem.SpawnMode.Client;
		GameObject root;
		if (this.SpawnZone(zoneID, mode, out root))
		{
			ZoneSystem.ZoneData zoneData2 = new ZoneSystem.ZoneData();
			zoneData2.m_root = root;
			this.m_zones.Add(zoneID, zoneData2);
			return true;
		}
		return false;
	}

	// Token: 0x060014D6 RID: 5334 RVA: 0x00087F60 File Offset: 0x00086160
	public bool IsZoneLoaded(Vector3 point)
	{
		Vector2i zone = this.GetZone(point);
		return this.IsZoneLoaded(zone);
	}

	// Token: 0x060014D7 RID: 5335 RVA: 0x00087F7C File Offset: 0x0008617C
	public bool IsZoneLoaded(Vector2i zoneID)
	{
		return this.m_zones.ContainsKey(zoneID);
	}

	// Token: 0x060014D8 RID: 5336 RVA: 0x00087F8C File Offset: 0x0008618C
	public bool IsActiveAreaLoaded()
	{
		Vector2i zone = this.GetZone(ZNet.instance.GetReferencePosition());
		for (int i = zone.y - this.m_activeArea; i <= zone.y + this.m_activeArea; i++)
		{
			for (int j = zone.x - this.m_activeArea; j <= zone.x + this.m_activeArea; j++)
			{
				if (!this.m_zones.ContainsKey(new Vector2i(j, i)))
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x060014D9 RID: 5337 RVA: 0x0008800C File Offset: 0x0008620C
	private bool SpawnZone(Vector2i zoneID, ZoneSystem.SpawnMode mode, out GameObject root)
	{
		Vector3 zonePos = this.GetZonePos(zoneID);
		Heightmap componentInChildren = this.m_zonePrefab.GetComponentInChildren<Heightmap>();
		if (!HeightmapBuilder.instance.IsTerrainReady(zonePos, componentInChildren.m_width, componentInChildren.m_scale, componentInChildren.IsDistantLod, WorldGenerator.instance))
		{
			root = null;
			return false;
		}
		root = UnityEngine.Object.Instantiate<GameObject>(this.m_zonePrefab, zonePos, Quaternion.identity);
		if ((mode == ZoneSystem.SpawnMode.Ghost || mode == ZoneSystem.SpawnMode.Full) && !this.IsZoneGenerated(zoneID))
		{
			Heightmap componentInChildren2 = root.GetComponentInChildren<Heightmap>();
			this.m_tempClearAreas.Clear();
			this.m_tempSpawnedObjects.Clear();
			this.PlaceLocations(zoneID, zonePos, root.transform, componentInChildren2, this.m_tempClearAreas, mode, this.m_tempSpawnedObjects);
			this.PlaceVegetation(zoneID, zonePos, root.transform, componentInChildren2, this.m_tempClearAreas, mode, this.m_tempSpawnedObjects);
			this.PlaceZoneCtrl(zoneID, zonePos, mode, this.m_tempSpawnedObjects);
			if (mode == ZoneSystem.SpawnMode.Ghost)
			{
				foreach (GameObject obj in this.m_tempSpawnedObjects)
				{
					UnityEngine.Object.Destroy(obj);
				}
				this.m_tempSpawnedObjects.Clear();
				UnityEngine.Object.Destroy(root);
				root = null;
			}
			this.SetZoneGenerated(zoneID);
		}
		return true;
	}

	// Token: 0x060014DA RID: 5338 RVA: 0x0008814C File Offset: 0x0008634C
	private void PlaceZoneCtrl(Vector2i zoneID, Vector3 zoneCenterPos, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
	{
		if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
		{
			if (mode == ZoneSystem.SpawnMode.Ghost)
			{
				ZNetView.StartGhostInit();
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_zoneCtrlPrefab, zoneCenterPos, Quaternion.identity);
			gameObject.GetComponent<ZNetView>();
			if (mode == ZoneSystem.SpawnMode.Ghost)
			{
				spawnedObjects.Add(gameObject);
				ZNetView.FinishGhostInit();
			}
		}
	}

	// Token: 0x060014DB RID: 5339 RVA: 0x00088194 File Offset: 0x00086394
	private Vector3 GetRandomPointInRadius(Vector3 center, float radius)
	{
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = UnityEngine.Random.Range(0f, radius);
		return center + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
	}

	// Token: 0x060014DC RID: 5340 RVA: 0x000881E0 File Offset: 0x000863E0
	private void PlaceVegetation(Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ZoneSystem.ClearArea> clearAreas, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		int seed = WorldGenerator.instance.GetSeed();
		float num = this.m_zoneSize / 2f;
		int num2 = 1;
		foreach (ZoneSystem.ZoneVegetation zoneVegetation in this.m_vegetation)
		{
			num2++;
			if (zoneVegetation.m_enable && hmap.HaveBiome(zoneVegetation.m_biome))
			{
				UnityEngine.Random.InitState(seed + zoneID.x * 4271 + zoneID.y * 9187 + zoneVegetation.m_prefab.name.GetStableHashCode());
				int num3 = 1;
				if (zoneVegetation.m_max < 1f)
				{
					if (UnityEngine.Random.value > zoneVegetation.m_max)
					{
						continue;
					}
				}
				else
				{
					num3 = UnityEngine.Random.Range((int)zoneVegetation.m_min, (int)zoneVegetation.m_max + 1);
				}
				bool flag = zoneVegetation.m_prefab.GetComponent<ZNetView>() != null;
				float num4 = Mathf.Cos(0.017453292f * zoneVegetation.m_maxTilt);
				float num5 = Mathf.Cos(0.017453292f * zoneVegetation.m_minTilt);
				float num6 = num - zoneVegetation.m_groupRadius;
				int num7 = zoneVegetation.m_forcePlacement ? (num3 * 50) : num3;
				int num8 = 0;
				for (int i = 0; i < num7; i++)
				{
					Vector3 vector = new Vector3(UnityEngine.Random.Range(zoneCenterPos.x - num6, zoneCenterPos.x + num6), 0f, UnityEngine.Random.Range(zoneCenterPos.z - num6, zoneCenterPos.z + num6));
					int num9 = UnityEngine.Random.Range(zoneVegetation.m_groupSizeMin, zoneVegetation.m_groupSizeMax + 1);
					bool flag2 = false;
					for (int j = 0; j < num9; j++)
					{
						Vector3 vector2 = (j == 0) ? vector : this.GetRandomPointInRadius(vector, zoneVegetation.m_groupRadius);
						float y = (float)UnityEngine.Random.Range(0, 360);
						float num10 = UnityEngine.Random.Range(zoneVegetation.m_scaleMin, zoneVegetation.m_scaleMax);
						float x = UnityEngine.Random.Range(-zoneVegetation.m_randTilt, zoneVegetation.m_randTilt);
						float z = UnityEngine.Random.Range(-zoneVegetation.m_randTilt, zoneVegetation.m_randTilt);
						if (!zoneVegetation.m_blockCheck || !this.IsBlocked(vector2))
						{
							Vector3 vector3;
							Heightmap.Biome biome;
							Heightmap.BiomeArea biomeArea;
							Heightmap heightmap;
							this.GetGroundData(ref vector2, out vector3, out biome, out biomeArea, out heightmap);
							if ((zoneVegetation.m_biome & biome) != Heightmap.Biome.None && (zoneVegetation.m_biomeArea & biomeArea) != (Heightmap.BiomeArea)0)
							{
								float y2;
								Vector3 vector4;
								if (zoneVegetation.m_snapToStaticSolid && this.GetStaticSolidHeight(vector2, out y2, out vector4))
								{
									vector2.y = y2;
									vector3 = vector4;
								}
								float num11 = vector2.y - this.m_waterLevel;
								if (num11 >= zoneVegetation.m_minAltitude && num11 <= zoneVegetation.m_maxAltitude)
								{
									if (zoneVegetation.m_minVegetation != zoneVegetation.m_maxVegetation)
									{
										float vegetationMask = heightmap.GetVegetationMask(vector2);
										if (vegetationMask > zoneVegetation.m_maxVegetation || vegetationMask < zoneVegetation.m_minVegetation)
										{
											goto IL_4EF;
										}
									}
									if (zoneVegetation.m_minOceanDepth != zoneVegetation.m_maxOceanDepth)
									{
										float oceanDepth = heightmap.GetOceanDepth(vector2);
										if (oceanDepth < zoneVegetation.m_minOceanDepth || oceanDepth > zoneVegetation.m_maxOceanDepth)
										{
											goto IL_4EF;
										}
									}
									if (vector3.y >= num4 && vector3.y <= num5)
									{
										if (zoneVegetation.m_terrainDeltaRadius > 0f)
										{
											float num12;
											Vector3 vector5;
											this.GetTerrainDelta(vector2, zoneVegetation.m_terrainDeltaRadius, out num12, out vector5);
											if (num12 > zoneVegetation.m_maxTerrainDelta || num12 < zoneVegetation.m_minTerrainDelta)
											{
												goto IL_4EF;
											}
										}
										if (zoneVegetation.m_inForest)
										{
											float forestFactor = WorldGenerator.GetForestFactor(vector2);
											if (forestFactor < zoneVegetation.m_forestTresholdMin || forestFactor > zoneVegetation.m_forestTresholdMax)
											{
												goto IL_4EF;
											}
										}
										if (!this.InsideClearArea(clearAreas, vector2))
										{
											if (zoneVegetation.m_snapToWater)
											{
												vector2.y = this.m_waterLevel;
											}
											vector2.y += zoneVegetation.m_groundOffset;
											Quaternion rotation = Quaternion.identity;
											if (zoneVegetation.m_chanceToUseGroundTilt > 0f && UnityEngine.Random.value <= zoneVegetation.m_chanceToUseGroundTilt)
											{
												Quaternion rotation2 = Quaternion.Euler(0f, y, 0f);
												rotation = Quaternion.LookRotation(Vector3.Cross(vector3, rotation2 * Vector3.forward), vector3);
											}
											else
											{
												rotation = Quaternion.Euler(x, y, z);
											}
											if (flag)
											{
												if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
												{
													if (mode == ZoneSystem.SpawnMode.Ghost)
													{
														ZNetView.StartGhostInit();
													}
													GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(zoneVegetation.m_prefab, vector2, rotation);
													ZNetView component = gameObject.GetComponent<ZNetView>();
													if (num10 != gameObject.transform.localScale.x)
													{
														component.SetLocalScale(new Vector3(num10, num10, num10));
														foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
														{
															collider.enabled = false;
															collider.enabled = true;
														}
													}
													if (mode == ZoneSystem.SpawnMode.Ghost)
													{
														spawnedObjects.Add(gameObject);
														ZNetView.FinishGhostInit();
													}
												}
											}
											else
											{
												GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(zoneVegetation.m_prefab, vector2, rotation);
												gameObject2.transform.localScale = new Vector3(num10, num10, num10);
												gameObject2.transform.SetParent(parent, true);
											}
											flag2 = true;
										}
									}
								}
							}
						}
						IL_4EF:;
					}
					if (flag2)
					{
						num8++;
					}
					if (num8 >= num3)
					{
						break;
					}
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	// Token: 0x060014DD RID: 5341 RVA: 0x00088748 File Offset: 0x00086948
	private bool InsideClearArea(List<ZoneSystem.ClearArea> areas, Vector3 point)
	{
		foreach (ZoneSystem.ClearArea clearArea in areas)
		{
			if (point.x > clearArea.m_center.x - clearArea.m_radius && point.x < clearArea.m_center.x + clearArea.m_radius && point.z > clearArea.m_center.z - clearArea.m_radius && point.z < clearArea.m_center.z + clearArea.m_radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060014DE RID: 5342 RVA: 0x00088800 File Offset: 0x00086A00
	private ZoneSystem.ZoneLocation GetLocation(int hash)
	{
		ZoneSystem.ZoneLocation result;
		if (this.m_locationsByHash.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x060014DF RID: 5343 RVA: 0x00088820 File Offset: 0x00086A20
	private ZoneSystem.ZoneLocation GetLocation(string name)
	{
		foreach (ZoneSystem.ZoneLocation zoneLocation in this.m_locations)
		{
			if (zoneLocation.m_prefabName == name)
			{
				return zoneLocation;
			}
		}
		return null;
	}

	// Token: 0x060014E0 RID: 5344 RVA: 0x00088884 File Offset: 0x00086A84
	private void ClearNonPlacedLocations()
	{
		Dictionary<Vector2i, ZoneSystem.LocationInstance> dictionary = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();
		foreach (KeyValuePair<Vector2i, ZoneSystem.LocationInstance> keyValuePair in this.m_locationInstances)
		{
			if (keyValuePair.Value.m_placed)
			{
				dictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
		}
		this.m_locationInstances = dictionary;
	}

	// Token: 0x060014E1 RID: 5345 RVA: 0x00088900 File Offset: 0x00086B00
	private void CheckLocationDuplicates()
	{
		ZLog.Log("Checking for location duplicates");
		for (int i = 0; i < this.m_locations.Count; i++)
		{
			ZoneSystem.ZoneLocation zoneLocation = this.m_locations[i];
			if (zoneLocation.m_enable)
			{
				for (int j = i + 1; j < this.m_locations.Count; j++)
				{
					ZoneSystem.ZoneLocation zoneLocation2 = this.m_locations[j];
					if (zoneLocation2.m_enable && zoneLocation.m_prefabName == zoneLocation2.m_prefabName)
					{
						ZLog.LogWarning("Two locations points to the same location prefab " + zoneLocation.m_prefabName);
					}
				}
			}
		}
	}

	// Token: 0x060014E2 RID: 5346 RVA: 0x00088998 File Offset: 0x00086B98
	public void GenerateLocations()
	{
		if (!Application.isPlaying)
		{
			ZLog.Log("Setting up locations");
			this.SetupLocations();
		}
		ZLog.Log("Generating locations");
		DateTime now = DateTime.Now;
		this.m_locationsGenerated = true;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		this.CheckLocationDuplicates();
		this.ClearNonPlacedLocations();
		foreach (ZoneSystem.ZoneLocation zoneLocation in from a in this.m_locations
		orderby a.m_prioritized descending
		select a)
		{
			if (zoneLocation.m_enable && zoneLocation.m_quantity != 0)
			{
				this.GenerateLocations(zoneLocation);
			}
		}
		UnityEngine.Random.state = state;
		ZLog.Log(" Done generating locations, duration:" + (DateTime.Now - now).TotalMilliseconds.ToString() + " ms");
	}

	// Token: 0x060014E3 RID: 5347 RVA: 0x00088A90 File Offset: 0x00086C90
	private int CountNrOfLocation(ZoneSystem.ZoneLocation location)
	{
		int num = 0;
		using (Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection.Enumerator enumerator = this.m_locationInstances.Values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_location.m_prefabName == location.m_prefabName)
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			ZLog.Log("Old location found " + location.m_prefabName + " x " + num.ToString());
		}
		return num;
	}

	// Token: 0x060014E4 RID: 5348 RVA: 0x00088B24 File Offset: 0x00086D24
	private void GenerateLocations(ZoneSystem.ZoneLocation location)
	{
		DateTime now = DateTime.Now;
		UnityEngine.Random.InitState(WorldGenerator.instance.GetSeed() + location.m_prefabName.GetStableHashCode());
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		float locationRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
		int num9 = location.m_prioritized ? 200000 : 100000;
		int num10 = 0;
		int num11 = this.CountNrOfLocation(location);
		float num12 = 10000f;
		if (location.m_centerFirst)
		{
			num12 = location.m_minDistance;
		}
		if (location.m_unique && num11 > 0)
		{
			return;
		}
		int num13 = 0;
		while (num13 < num9 && num11 < location.m_quantity)
		{
			Vector2i randomZone = this.GetRandomZone(num12);
			if (location.m_centerFirst)
			{
				num12 += 1f;
			}
			if (this.m_locationInstances.ContainsKey(randomZone))
			{
				num++;
			}
			else if (!this.IsZoneGenerated(randomZone))
			{
				Vector3 zonePos = this.GetZonePos(randomZone);
				Heightmap.BiomeArea biomeArea = WorldGenerator.instance.GetBiomeArea(zonePos);
				if ((location.m_biomeArea & biomeArea) == (Heightmap.BiomeArea)0)
				{
					num4++;
				}
				else
				{
					for (int i = 0; i < 20; i++)
					{
						num10++;
						Vector3 randomPointInZone = this.GetRandomPointInZone(randomZone, locationRadius);
						float magnitude = randomPointInZone.magnitude;
						if (location.m_minDistance != 0f && magnitude < location.m_minDistance)
						{
							num2++;
						}
						else if (location.m_maxDistance != 0f && magnitude > location.m_maxDistance)
						{
							num2++;
						}
						else
						{
							Heightmap.Biome biome = WorldGenerator.instance.GetBiome(randomPointInZone);
							if ((location.m_biome & biome) == Heightmap.Biome.None)
							{
								num3++;
							}
							else
							{
								randomPointInZone.y = WorldGenerator.instance.GetHeight(randomPointInZone.x, randomPointInZone.z);
								float num14 = randomPointInZone.y - this.m_waterLevel;
								if (num14 < location.m_minAltitude || num14 > location.m_maxAltitude)
								{
									num5++;
								}
								else
								{
									if (location.m_inForest)
									{
										float forestFactor = WorldGenerator.GetForestFactor(randomPointInZone);
										if (forestFactor < location.m_forestTresholdMin || forestFactor > location.m_forestTresholdMax)
										{
											num6++;
											goto IL_27C;
										}
									}
									float num15;
									Vector3 vector;
									WorldGenerator.instance.GetTerrainDelta(randomPointInZone, location.m_exteriorRadius, out num15, out vector);
									if (num15 > location.m_maxTerrainDelta || num15 < location.m_minTerrainDelta)
									{
										num8++;
									}
									else
									{
										if (location.m_minDistanceFromSimilar <= 0f || !this.HaveLocationInRange(location.m_prefabName, location.m_group, randomPointInZone, location.m_minDistanceFromSimilar))
										{
											this.RegisterLocation(location, randomPointInZone, false);
											num11++;
											break;
										}
										num7++;
									}
								}
							}
						}
						IL_27C:;
					}
				}
			}
			num13++;
		}
		if (num11 < location.m_quantity)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Failed to place all ",
				location.m_prefabName,
				", placed ",
				num11.ToString(),
				" out of ",
				location.m_quantity.ToString()
			}));
			ZLog.DevLog("errorLocationInZone " + num.ToString());
			ZLog.DevLog("errorCenterDistance " + num2.ToString());
			ZLog.DevLog("errorBiome " + num3.ToString());
			ZLog.DevLog("errorBiomeArea " + num4.ToString());
			ZLog.DevLog("errorAlt " + num5.ToString());
			ZLog.DevLog("errorForest " + num6.ToString());
			ZLog.DevLog("errorSimilar " + num7.ToString());
			ZLog.DevLog("errorTerrainDelta " + num8.ToString());
		}
		DateTime.Now - now;
	}

	// Token: 0x060014E5 RID: 5349 RVA: 0x00088EE8 File Offset: 0x000870E8
	private Vector2i GetRandomZone(float range)
	{
		int num = (int)range / (int)this.m_zoneSize;
		Vector2i vector2i;
		do
		{
			vector2i = new Vector2i(UnityEngine.Random.Range(-num, num), UnityEngine.Random.Range(-num, num));
		}
		while (this.GetZonePos(vector2i).magnitude >= 10000f);
		return vector2i;
	}

	// Token: 0x060014E6 RID: 5350 RVA: 0x00088F30 File Offset: 0x00087130
	private Vector3 GetRandomPointInZone(Vector2i zone, float locationRadius)
	{
		Vector3 zonePos = this.GetZonePos(zone);
		float num = this.m_zoneSize / 2f;
		float x = UnityEngine.Random.Range(-num + locationRadius, num - locationRadius);
		float z = UnityEngine.Random.Range(-num + locationRadius, num - locationRadius);
		return zonePos + new Vector3(x, 0f, z);
	}

	// Token: 0x060014E7 RID: 5351 RVA: 0x00088F7C File Offset: 0x0008717C
	private Vector3 GetRandomPointInZone(float locationRadius)
	{
		Vector3 point = new Vector3(UnityEngine.Random.Range(-10000f, 10000f), 0f, UnityEngine.Random.Range(-10000f, 10000f));
		Vector2i zone = this.GetZone(point);
		Vector3 zonePos = this.GetZonePos(zone);
		float num = this.m_zoneSize / 2f;
		return new Vector3(UnityEngine.Random.Range(zonePos.x - num + locationRadius, zonePos.x + num - locationRadius), 0f, UnityEngine.Random.Range(zonePos.z - num + locationRadius, zonePos.z + num - locationRadius));
	}

	// Token: 0x060014E8 RID: 5352 RVA: 0x0008900C File Offset: 0x0008720C
	private void PlaceLocations(Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ZoneSystem.ClearArea> clearAreas, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
	{
		this.GenerateLocationsIfNeeded();
		DateTime now = DateTime.Now;
		ZoneSystem.LocationInstance locationInstance;
		if (this.m_locationInstances.TryGetValue(zoneID, out locationInstance))
		{
			if (locationInstance.m_placed)
			{
				return;
			}
			Vector3 position = locationInstance.m_position;
			Vector3 vector;
			Heightmap.Biome biome;
			Heightmap.BiomeArea biomeArea;
			Heightmap heightmap;
			this.GetGroundData(ref position, out vector, out biome, out biomeArea, out heightmap);
			if (locationInstance.m_location.m_snapToWater)
			{
				position.y = this.m_waterLevel;
			}
			if (locationInstance.m_location.m_location.m_clearArea)
			{
				ZoneSystem.ClearArea item = new ZoneSystem.ClearArea(position, locationInstance.m_location.m_exteriorRadius);
				clearAreas.Add(item);
			}
			Quaternion rot = Quaternion.identity;
			if (locationInstance.m_location.m_slopeRotation)
			{
				float num;
				Vector3 vector2;
				this.GetTerrainDelta(position, locationInstance.m_location.m_exteriorRadius, out num, out vector2);
				Vector3 forward = new Vector3(vector2.x, 0f, vector2.z);
				forward.Normalize();
				rot = Quaternion.LookRotation(forward);
				Vector3 eulerAngles = rot.eulerAngles;
				eulerAngles.y = Mathf.Round(eulerAngles.y / 22.5f) * 22.5f;
				rot.eulerAngles = eulerAngles;
			}
			else if (locationInstance.m_location.m_randomRotation)
			{
				rot = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 16) * 22.5f, 0f);
			}
			int seed = WorldGenerator.instance.GetSeed() + zoneID.x * 4271 + zoneID.y * 9187;
			this.SpawnLocation(locationInstance.m_location, seed, position, rot, mode, spawnedObjects);
			locationInstance.m_placed = true;
			this.m_locationInstances[zoneID] = locationInstance;
			TimeSpan timeSpan = DateTime.Now - now;
			string[] array = new string[5];
			array[0] = "Placed locations in zone ";
			int num2 = 1;
			Vector2i vector2i = zoneID;
			array[num2] = vector2i.ToString();
			array[2] = "  duration ";
			array[3] = timeSpan.TotalMilliseconds.ToString();
			array[4] = " ms";
			ZLog.Log(string.Concat(array));
			if (locationInstance.m_location.m_unique)
			{
				this.RemoveUnplacedLocations(locationInstance.m_location);
			}
			if (locationInstance.m_location.m_iconPlaced)
			{
				this.SendLocationIcons(ZRoutedRpc.Everybody);
			}
		}
	}

	// Token: 0x060014E9 RID: 5353 RVA: 0x00089234 File Offset: 0x00087434
	private void RemoveUnplacedLocations(ZoneSystem.ZoneLocation location)
	{
		List<Vector2i> list = new List<Vector2i>();
		foreach (KeyValuePair<Vector2i, ZoneSystem.LocationInstance> keyValuePair in this.m_locationInstances)
		{
			if (keyValuePair.Value.m_location == location && !keyValuePair.Value.m_placed)
			{
				list.Add(keyValuePair.Key);
			}
		}
		foreach (Vector2i key in list)
		{
			this.m_locationInstances.Remove(key);
		}
		ZLog.DevLog("Removed " + list.Count.ToString() + " unplaced locations of type " + location.m_prefabName);
	}

	// Token: 0x060014EA RID: 5354 RVA: 0x00089320 File Offset: 0x00087520
	public bool TestSpawnLocation(string name, Vector3 pos, bool disableSave = true)
	{
		if (!ZNet.instance.IsServer())
		{
			return false;
		}
		ZoneSystem.ZoneLocation location = this.GetLocation(name);
		if (location == null)
		{
			ZLog.Log("Missing location:" + name);
			global::Console.instance.Print("Missing location:" + name);
			return false;
		}
		if (location.m_prefab == null)
		{
			ZLog.Log("Missing prefab in location:" + name);
			global::Console.instance.Print("Missing location:" + name);
			return false;
		}
		float num = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
		Vector2i zone = this.GetZone(pos);
		Vector3 zonePos = this.GetZonePos(zone);
		pos.x = Mathf.Clamp(pos.x, zonePos.x - this.m_zoneSize / 2f + num, zonePos.x + this.m_zoneSize / 2f - num);
		pos.z = Mathf.Clamp(pos.z, zonePos.z - this.m_zoneSize / 2f + num, zonePos.z + this.m_zoneSize / 2f - num);
		string[] array = new string[6];
		array[0] = "radius ";
		array[1] = num.ToString();
		array[2] = "  ";
		int num2 = 3;
		Vector3 vector = zonePos;
		array[num2] = vector.ToString();
		array[4] = " ";
		int num3 = 5;
		vector = pos;
		array[num3] = vector.ToString();
		ZLog.Log(string.Concat(array));
		MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Location spawned, " + (disableSave ? "world saving DISABLED until restart" : "CAUTION! world saving is ENABLED, use normal location command to disable it!"), 0, null);
		this.m_didZoneTest = disableSave;
		float y = (float)UnityEngine.Random.Range(0, 16) * 22.5f;
		List<GameObject> spawnedGhostObjects = new List<GameObject>();
		this.SpawnLocation(location, UnityEngine.Random.Range(0, 99999), pos, Quaternion.Euler(0f, y, 0f), ZoneSystem.SpawnMode.Full, spawnedGhostObjects);
		return true;
	}

	// Token: 0x060014EB RID: 5355 RVA: 0x00089504 File Offset: 0x00087704
	public GameObject SpawnProxyLocation(int hash, int seed, Vector3 pos, Quaternion rot)
	{
		ZoneSystem.ZoneLocation location = this.GetLocation(hash);
		if (location == null)
		{
			ZLog.LogWarning("Missing location:" + hash.ToString());
			return null;
		}
		List<GameObject> spawnedGhostObjects = new List<GameObject>();
		return this.SpawnLocation(location, seed, pos, rot, ZoneSystem.SpawnMode.Client, spawnedGhostObjects);
	}

	// Token: 0x060014EC RID: 5356 RVA: 0x00089548 File Offset: 0x00087748
	private GameObject SpawnLocation(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
	{
		location.m_prefab.transform.position = Vector3.zero;
		location.m_prefab.transform.rotation = Quaternion.identity;
		UnityEngine.Random.InitState(seed);
		Location component = location.m_prefab.GetComponent<Location>();
		bool flag = component && component.m_useCustomInteriorTransform && component.m_interiorTransform && component.m_generator;
		if (flag)
		{
			Vector2i zone = this.GetZone(pos);
			Vector3 zonePos = this.GetZonePos(zone);
			component.m_generator.transform.localPosition = Vector3.zero;
			Vector3 vector = zonePos + location.m_interiorPosition + location.m_generatorPosition - pos;
			Vector3 localPosition = (Matrix4x4.Rotate(Quaternion.Inverse(rot)) * Matrix4x4.Translate(vector)).GetColumn(3);
			localPosition.y = component.m_interiorTransform.localPosition.y;
			component.m_interiorTransform.localPosition = localPosition;
			component.m_interiorTransform.localRotation = Quaternion.Inverse(rot);
		}
		if (component && component.m_generator && component.m_useCustomInteriorTransform != component.m_generator.m_useCustomInteriorTransform)
		{
			ZLog.LogWarning(component.name + " & " + component.m_generator.name + " don't have matching m_useCustomInteriorTransform()! If one has it the other should as well!");
		}
		if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
		{
			foreach (ZNetView znetView in location.m_netViews)
			{
				znetView.gameObject.SetActive(true);
			}
			UnityEngine.Random.InitState(seed);
			foreach (RandomSpawn randomSpawn in location.m_randomSpawns)
			{
				randomSpawn.Randomize();
			}
			WearNTear.m_randomInitialDamage = location.m_location.m_applyRandomDamage;
			foreach (ZNetView znetView2 in location.m_netViews)
			{
				if (znetView2.gameObject.activeSelf)
				{
					Vector3 position = znetView2.gameObject.transform.position;
					Vector3 position2 = pos + rot * position;
					Quaternion rotation = znetView2.gameObject.transform.rotation;
					Quaternion rotation2 = rot * rotation;
					if (mode == ZoneSystem.SpawnMode.Ghost)
					{
						ZNetView.StartGhostInit();
					}
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(znetView2.gameObject, position2, rotation2);
					gameObject.GetComponent<ZNetView>();
					DungeonGenerator component2 = gameObject.GetComponent<DungeonGenerator>();
					if (component2)
					{
						if (flag)
						{
							component2.m_originalPosition = location.m_generatorPosition;
						}
						component2.Generate(mode);
					}
					if (mode == ZoneSystem.SpawnMode.Ghost)
					{
						spawnedGhostObjects.Add(gameObject);
						ZNetView.FinishGhostInit();
					}
				}
			}
			WearNTear.m_randomInitialDamage = false;
			this.CreateLocationProxy(location, seed, pos, rot, mode, spawnedGhostObjects);
			SnapToGround.SnappAll();
			return null;
		}
		UnityEngine.Random.InitState(seed);
		foreach (RandomSpawn randomSpawn2 in location.m_randomSpawns)
		{
			randomSpawn2.Randomize();
		}
		foreach (ZNetView znetView3 in location.m_netViews)
		{
			znetView3.gameObject.SetActive(false);
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(location.m_prefab, pos, rot);
		gameObject2.SetActive(true);
		SnapToGround.SnappAll();
		return gameObject2;
	}

	// Token: 0x060014ED RID: 5357 RVA: 0x00089918 File Offset: 0x00087B18
	private void CreateLocationProxy(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rotation, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
	{
		if (mode == ZoneSystem.SpawnMode.Ghost)
		{
			ZNetView.StartGhostInit();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_locationProxyPrefab, pos, rotation);
		LocationProxy component = gameObject.GetComponent<LocationProxy>();
		bool spawnNow = mode == ZoneSystem.SpawnMode.Full;
		component.SetLocation(location.m_prefab.name, seed, spawnNow);
		if (mode == ZoneSystem.SpawnMode.Ghost)
		{
			spawnedGhostObjects.Add(gameObject);
			ZNetView.FinishGhostInit();
		}
	}

	// Token: 0x060014EE RID: 5358 RVA: 0x00089970 File Offset: 0x00087B70
	private void RegisterLocation(ZoneSystem.ZoneLocation location, Vector3 pos, bool generated)
	{
		ZoneSystem.LocationInstance value = default(ZoneSystem.LocationInstance);
		value.m_location = location;
		value.m_position = pos;
		value.m_placed = generated;
		Vector2i zone = this.GetZone(pos);
		if (this.m_locationInstances.ContainsKey(zone))
		{
			string str = "Location already exist in zone ";
			Vector2i vector2i = zone;
			ZLog.LogWarning(str + vector2i.ToString());
			return;
		}
		this.m_locationInstances.Add(zone, value);
	}

	// Token: 0x060014EF RID: 5359 RVA: 0x000899E0 File Offset: 0x00087BE0
	private bool HaveLocationInRange(string prefabName, string group, Vector3 p, float radius)
	{
		foreach (ZoneSystem.LocationInstance locationInstance in this.m_locationInstances.Values)
		{
			if ((locationInstance.m_location.m_prefabName == prefabName || (group.Length > 0 && group == locationInstance.m_location.m_group)) && Vector3.Distance(locationInstance.m_position, p) < radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060014F0 RID: 5360 RVA: 0x00089A78 File Offset: 0x00087C78
	public bool GetLocationIcon(string name, out Vector3 pos)
	{
		if (ZNet.instance.IsServer())
		{
			using (Dictionary<Vector2i, ZoneSystem.LocationInstance>.Enumerator enumerator = this.m_locationInstances.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<Vector2i, ZoneSystem.LocationInstance> keyValuePair = enumerator.Current;
					if ((keyValuePair.Value.m_location.m_iconAlways || (keyValuePair.Value.m_location.m_iconPlaced && keyValuePair.Value.m_placed)) && keyValuePair.Value.m_location.m_prefabName == name)
					{
						pos = keyValuePair.Value.m_position;
						return true;
					}
				}
				goto IL_F1;
			}
		}
		foreach (KeyValuePair<Vector3, string> keyValuePair2 in this.m_locationIcons)
		{
			if (keyValuePair2.Value == name)
			{
				pos = keyValuePair2.Key;
				return true;
			}
		}
		IL_F1:
		pos = Vector3.zero;
		return false;
	}

	// Token: 0x060014F1 RID: 5361 RVA: 0x00089BA0 File Offset: 0x00087DA0
	public void GetLocationIcons(Dictionary<Vector3, string> icons)
	{
		if (ZNet.instance.IsServer())
		{
			using (Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection.Enumerator enumerator = this.m_locationInstances.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZoneSystem.LocationInstance locationInstance = enumerator.Current;
					if (locationInstance.m_location.m_iconAlways || (locationInstance.m_location.m_iconPlaced && locationInstance.m_placed))
					{
						icons[locationInstance.m_position] = locationInstance.m_location.m_prefabName;
					}
				}
				return;
			}
		}
		foreach (KeyValuePair<Vector3, string> keyValuePair in this.m_locationIcons)
		{
			icons.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	// Token: 0x060014F2 RID: 5362 RVA: 0x00089C88 File Offset: 0x00087E88
	private void GetTerrainDelta(Vector3 center, float radius, out float delta, out Vector3 slopeDirection)
	{
		int num = 10;
		float num2 = -999999f;
		float num3 = 999999f;
		Vector3 b = center;
		Vector3 a = center;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * radius;
			Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
			float groundHeight = this.GetGroundHeight(vector2);
			if (groundHeight < num3)
			{
				num3 = groundHeight;
				a = vector2;
			}
			if (groundHeight > num2)
			{
				num2 = groundHeight;
				b = vector2;
			}
		}
		delta = num2 - num3;
		slopeDirection = Vector3.Normalize(a - b);
	}

	// Token: 0x060014F3 RID: 5363 RVA: 0x00089D20 File Offset: 0x00087F20
	public bool IsBlocked(Vector3 p)
	{
		p.y += 2000f;
		return Physics.Raycast(p, Vector3.down, 10000f, this.m_blockRayMask);
	}

	// Token: 0x060014F4 RID: 5364 RVA: 0x00089D50 File Offset: 0x00087F50
	public float GetAverageGroundHeight(Vector3 p, float radius)
	{
		Vector3 origin = p;
		origin.y = 6000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			return raycastHit.point.y;
		}
		return p.y;
	}

	// Token: 0x060014F5 RID: 5365 RVA: 0x00089D98 File Offset: 0x00087F98
	public float GetGroundHeight(Vector3 p)
	{
		Vector3 origin = p;
		origin.y = 6000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			return raycastHit.point.y;
		}
		return p.y;
	}

	// Token: 0x060014F6 RID: 5366 RVA: 0x00089DE0 File Offset: 0x00087FE0
	public bool GetGroundHeight(Vector3 p, out float height)
	{
		p.y = 6000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x060014F7 RID: 5367 RVA: 0x00089E2C File Offset: 0x0008802C
	public float GetSolidHeight(Vector3 p)
	{
		Vector3 origin = p;
		origin.y += 1000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 2000f, this.m_solidRayMask))
		{
			return raycastHit.point.y;
		}
		return p.y;
	}

	// Token: 0x060014F8 RID: 5368 RVA: 0x00089E78 File Offset: 0x00088078
	public bool GetSolidHeight(Vector3 p, out float height, int heightMargin = 1000)
	{
		p.y += (float)heightMargin;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 2000f, this.m_solidRayMask) && !raycastHit.collider.attachedRigidbody)
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x060014F9 RID: 5369 RVA: 0x00089ED8 File Offset: 0x000880D8
	public bool GetSolidHeight(Vector3 p, float radius, out float height, Transform ignore)
	{
		height = p.y - 1000f;
		p.y += 1000f;
		int num;
		if (radius <= 0f)
		{
			num = Physics.RaycastNonAlloc(p, Vector3.down, this.rayHits, 2000f, this.m_solidRayMask);
		}
		else
		{
			num = Physics.SphereCastNonAlloc(p, radius, Vector3.down, this.rayHits, 2000f, this.m_solidRayMask);
		}
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this.rayHits[i];
			Collider collider = raycastHit.collider;
			if (!(collider.attachedRigidbody != null) && (!(ignore != null) || !Utils.IsParent(collider.transform, ignore)))
			{
				if (raycastHit.point.y > height)
				{
					height = raycastHit.point.y;
				}
				result = true;
			}
		}
		return result;
	}

	// Token: 0x060014FA RID: 5370 RVA: 0x00089FB8 File Offset: 0x000881B8
	public bool GetSolidHeight(Vector3 p, out float height, out Vector3 normal)
	{
		GameObject gameObject;
		return this.GetSolidHeight(p, out height, out normal, out gameObject);
	}

	// Token: 0x060014FB RID: 5371 RVA: 0x00089FD0 File Offset: 0x000881D0
	public bool GetSolidHeight(Vector3 p, out float height, out Vector3 normal, out GameObject go)
	{
		p.y += 1000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 2000f, this.m_solidRayMask) && !raycastHit.collider.attachedRigidbody)
		{
			height = raycastHit.point.y;
			normal = raycastHit.normal;
			go = raycastHit.collider.gameObject;
			return true;
		}
		height = 0f;
		normal = Vector3.zero;
		go = null;
		return false;
	}

	// Token: 0x060014FC RID: 5372 RVA: 0x0008A060 File Offset: 0x00088260
	public bool GetStaticSolidHeight(Vector3 p, out float height, out Vector3 normal)
	{
		p.y += 1000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 2000f, this.m_staticSolidRayMask) && !raycastHit.collider.attachedRigidbody)
		{
			height = raycastHit.point.y;
			normal = raycastHit.normal;
			return true;
		}
		height = 0f;
		normal = Vector3.zero;
		return false;
	}

	// Token: 0x060014FD RID: 5373 RVA: 0x0008A0DC File Offset: 0x000882DC
	public bool FindFloor(Vector3 p, out float height)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * 1f, Vector3.down, out raycastHit, 1000f, this.m_solidRayMask))
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x060014FE RID: 5374 RVA: 0x0008A130 File Offset: 0x00088330
	public void GetGroundData(ref Vector3 p, out Vector3 normal, out Heightmap.Biome biome, out Heightmap.BiomeArea biomeArea, out Heightmap hmap)
	{
		biome = Heightmap.Biome.None;
		biomeArea = Heightmap.BiomeArea.Everything;
		hmap = null;
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * 5000f, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			p.y = raycastHit.point.y;
			normal = raycastHit.normal;
			Heightmap component = raycastHit.collider.GetComponent<Heightmap>();
			if (component)
			{
				biome = component.GetBiome(raycastHit.point);
				biomeArea = component.GetBiomeArea();
				hmap = component;
			}
			return;
		}
		normal = Vector3.up;
	}

	// Token: 0x060014FF RID: 5375 RVA: 0x0008A1D8 File Offset: 0x000883D8
	private void UpdateTTL(float dt)
	{
		foreach (KeyValuePair<Vector2i, ZoneSystem.ZoneData> keyValuePair in this.m_zones)
		{
			keyValuePair.Value.m_ttl += dt;
		}
		foreach (KeyValuePair<Vector2i, ZoneSystem.ZoneData> keyValuePair2 in this.m_zones)
		{
			if (keyValuePair2.Value.m_ttl > this.m_zoneTTL && !ZNetScene.instance.HaveInstanceInSector(keyValuePair2.Key))
			{
				UnityEngine.Object.Destroy(keyValuePair2.Value.m_root);
				this.m_zones.Remove(keyValuePair2.Key);
				break;
			}
		}
	}

	// Token: 0x06001500 RID: 5376 RVA: 0x0008A2C0 File Offset: 0x000884C0
	public bool FindClosestLocation(string name, Vector3 point, out ZoneSystem.LocationInstance closest)
	{
		float num = 999999f;
		closest = default(ZoneSystem.LocationInstance);
		bool result = false;
		foreach (ZoneSystem.LocationInstance locationInstance in this.m_locationInstances.Values)
		{
			float num2 = Vector3.Distance(locationInstance.m_position, point);
			if (locationInstance.m_location.m_prefabName == name && num2 < num)
			{
				num = num2;
				closest = locationInstance;
				result = true;
			}
		}
		return result;
	}

	// Token: 0x06001501 RID: 5377 RVA: 0x0008A354 File Offset: 0x00088554
	public Vector2i GetZone(Vector3 point)
	{
		int x = Mathf.FloorToInt((point.x + this.m_zoneSize / 2f) / this.m_zoneSize);
		int y = Mathf.FloorToInt((point.z + this.m_zoneSize / 2f) / this.m_zoneSize);
		return new Vector2i(x, y);
	}

	// Token: 0x06001502 RID: 5378 RVA: 0x0008A3A6 File Offset: 0x000885A6
	public Vector3 GetZonePos(Vector2i id)
	{
		return new Vector3((float)id.x * this.m_zoneSize, 0f, (float)id.y * this.m_zoneSize);
	}

	// Token: 0x06001503 RID: 5379 RVA: 0x0008A3CE File Offset: 0x000885CE
	private void SetZoneGenerated(Vector2i zoneID)
	{
		this.m_generatedZones.Add(zoneID);
	}

	// Token: 0x06001504 RID: 5380 RVA: 0x0008A3DD File Offset: 0x000885DD
	private bool IsZoneGenerated(Vector2i zoneID)
	{
		return this.m_generatedZones.Contains(zoneID);
	}

	// Token: 0x06001505 RID: 5381 RVA: 0x0008A3EB File Offset: 0x000885EB
	public bool SkipSaving()
	{
		return this.m_error || this.m_didZoneTest;
	}

	// Token: 0x06001506 RID: 5382 RVA: 0x0008A3FD File Offset: 0x000885FD
	public float TimeSinceStart()
	{
		return this.m_lastFixedTime - this.m_startTime;
	}

	// Token: 0x06001507 RID: 5383 RVA: 0x0008A40C File Offset: 0x0008860C
	public void ResetGlobalKeys()
	{
		this.m_globalKeys.Clear();
		this.SendGlobalKeys(ZRoutedRpc.Everybody);
	}

	// Token: 0x06001508 RID: 5384 RVA: 0x0008A424 File Offset: 0x00088624
	public void SetGlobalKey(string name)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("SetGlobalKey", new object[]
		{
			name
		});
	}

	// Token: 0x06001509 RID: 5385 RVA: 0x0008A43F File Offset: 0x0008863F
	public bool GetGlobalKey(string name)
	{
		return this.m_globalKeys.Contains(name);
	}

	// Token: 0x0600150A RID: 5386 RVA: 0x0008A44D File Offset: 0x0008864D
	private void RPC_SetGlobalKey(long sender, string name)
	{
		if (this.m_globalKeys.Contains(name))
		{
			return;
		}
		this.m_globalKeys.Add(name);
		this.SendGlobalKeys(ZRoutedRpc.Everybody);
	}

	// Token: 0x0600150B RID: 5387 RVA: 0x0008A476 File Offset: 0x00088676
	public void RemoveGlobalKey(string name)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("RemoveGlobalKey", new object[]
		{
			name
		});
	}

	// Token: 0x0600150C RID: 5388 RVA: 0x0008A491 File Offset: 0x00088691
	private void RPC_RemoveGlobalKey(long sender, string name)
	{
		if (!this.m_globalKeys.Contains(name))
		{
			return;
		}
		this.m_globalKeys.Remove(name);
		this.SendGlobalKeys(ZRoutedRpc.Everybody);
	}

	// Token: 0x0600150D RID: 5389 RVA: 0x0008A4BA File Offset: 0x000886BA
	public List<string> GetGlobalKeys()
	{
		return new List<string>(this.m_globalKeys);
	}

	// Token: 0x0600150E RID: 5390 RVA: 0x0008A4C7 File Offset: 0x000886C7
	public Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection GetLocationList()
	{
		return this.m_locationInstances.Values;
	}

	// Token: 0x04001573 RID: 5491
	private Dictionary<Vector3, string> tempIconList = new Dictionary<Vector3, string>();

	// Token: 0x04001574 RID: 5492
	private RaycastHit[] rayHits = new RaycastHit[200];

	// Token: 0x04001575 RID: 5493
	private static ZoneSystem m_instance;

	// Token: 0x04001576 RID: 5494
	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();

	// Token: 0x04001577 RID: 5495
	[HideInInspector]
	public List<Heightmap.Biome> m_vegetationFolded = new List<Heightmap.Biome>();

	// Token: 0x04001578 RID: 5496
	[HideInInspector]
	public List<Heightmap.Biome> m_locationFolded = new List<Heightmap.Biome>();

	// Token: 0x04001579 RID: 5497
	[NonSerialized]
	public bool m_drawLocations;

	// Token: 0x0400157A RID: 5498
	[NonSerialized]
	public string m_drawLocationsFilter = "";

	// Token: 0x0400157B RID: 5499
	[global::Tooltip("Zones to load around center sector")]
	public int m_activeArea = 1;

	// Token: 0x0400157C RID: 5500
	public int m_activeDistantArea = 1;

	// Token: 0x0400157D RID: 5501
	[global::Tooltip("Zone size, should match netscene sector size")]
	public float m_zoneSize = 64f;

	// Token: 0x0400157E RID: 5502
	[global::Tooltip("Time before destroying inactive zone")]
	public float m_zoneTTL = 4f;

	// Token: 0x0400157F RID: 5503
	[global::Tooltip("Time before spawning active zone")]
	public float m_zoneTTS = 4f;

	// Token: 0x04001580 RID: 5504
	public GameObject m_zonePrefab;

	// Token: 0x04001581 RID: 5505
	public GameObject m_zoneCtrlPrefab;

	// Token: 0x04001582 RID: 5506
	public GameObject m_locationProxyPrefab;

	// Token: 0x04001583 RID: 5507
	public float m_waterLevel = 30f;

	// Token: 0x04001584 RID: 5508
	[Header("Versions")]
	public int m_locationVersion = 1;

	// Token: 0x04001585 RID: 5509
	[Header("Generation data")]
	public List<string> m_locationScenes = new List<string>();

	// Token: 0x04001586 RID: 5510
	public List<ZoneSystem.ZoneVegetation> m_vegetation = new List<ZoneSystem.ZoneVegetation>();

	// Token: 0x04001587 RID: 5511
	public List<ZoneSystem.ZoneLocation> m_locations = new List<ZoneSystem.ZoneLocation>();

	// Token: 0x04001588 RID: 5512
	private Dictionary<int, ZoneSystem.ZoneLocation> m_locationsByHash = new Dictionary<int, ZoneSystem.ZoneLocation>();

	// Token: 0x04001589 RID: 5513
	private bool m_error;

	// Token: 0x0400158A RID: 5514
	public bool m_didZoneTest;

	// Token: 0x0400158B RID: 5515
	private int m_terrainRayMask;

	// Token: 0x0400158C RID: 5516
	private int m_blockRayMask;

	// Token: 0x0400158D RID: 5517
	private int m_solidRayMask;

	// Token: 0x0400158E RID: 5518
	private int m_staticSolidRayMask;

	// Token: 0x0400158F RID: 5519
	private float m_updateTimer;

	// Token: 0x04001590 RID: 5520
	private float m_startTime;

	// Token: 0x04001591 RID: 5521
	private float m_lastFixedTime;

	// Token: 0x04001592 RID: 5522
	private Dictionary<Vector2i, ZoneSystem.ZoneData> m_zones = new Dictionary<Vector2i, ZoneSystem.ZoneData>();

	// Token: 0x04001593 RID: 5523
	private HashSet<Vector2i> m_generatedZones = new HashSet<Vector2i>();

	// Token: 0x04001594 RID: 5524
	private bool m_locationsGenerated;

	// Token: 0x04001595 RID: 5525
	[HideInInspector]
	public Dictionary<Vector2i, ZoneSystem.LocationInstance> m_locationInstances = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();

	// Token: 0x04001596 RID: 5526
	private Dictionary<Vector3, string> m_locationIcons = new Dictionary<Vector3, string>();

	// Token: 0x04001597 RID: 5527
	private HashSet<string> m_globalKeys = new HashSet<string>();

	// Token: 0x04001598 RID: 5528
	private HashSet<Vector2i> m_tempGeneratedZonesSaveClone;

	// Token: 0x04001599 RID: 5529
	private HashSet<string> m_tempGlobalKeysSaveClone;

	// Token: 0x0400159A RID: 5530
	private List<ZoneSystem.LocationInstance> m_tempLocationsSaveClone;

	// Token: 0x0400159B RID: 5531
	private bool m_tempLocationsGeneratedSaveClone;

	// Token: 0x0400159C RID: 5532
	private List<ZoneSystem.ClearArea> m_tempClearAreas = new List<ZoneSystem.ClearArea>();

	// Token: 0x0400159D RID: 5533
	private List<GameObject> m_tempSpawnedObjects = new List<GameObject>();

	// Token: 0x0200020A RID: 522
	private class ZoneData
	{
		// Token: 0x0400159E RID: 5534
		public GameObject m_root;

		// Token: 0x0400159F RID: 5535
		public float m_ttl;
	}

	// Token: 0x0200020B RID: 523
	private class ClearArea
	{
		// Token: 0x06001511 RID: 5393 RVA: 0x0008A5E8 File Offset: 0x000887E8
		public ClearArea(Vector3 p, float r)
		{
			this.m_center = p;
			this.m_radius = r;
		}

		// Token: 0x040015A0 RID: 5536
		public Vector3 m_center;

		// Token: 0x040015A1 RID: 5537
		public float m_radius;
	}

	// Token: 0x0200020C RID: 524
	[Serializable]
	public class ZoneVegetation
	{
		// Token: 0x06001512 RID: 5394 RVA: 0x0008A5FE File Offset: 0x000887FE
		public ZoneSystem.ZoneVegetation Clone()
		{
			return base.MemberwiseClone() as ZoneSystem.ZoneVegetation;
		}

		// Token: 0x040015A2 RID: 5538
		public string m_name = "veg";

		// Token: 0x040015A3 RID: 5539
		public GameObject m_prefab;

		// Token: 0x040015A4 RID: 5540
		public bool m_enable = true;

		// Token: 0x040015A5 RID: 5541
		public float m_min;

		// Token: 0x040015A6 RID: 5542
		public float m_max = 10f;

		// Token: 0x040015A7 RID: 5543
		public bool m_forcePlacement;

		// Token: 0x040015A8 RID: 5544
		public float m_scaleMin = 1f;

		// Token: 0x040015A9 RID: 5545
		public float m_scaleMax = 1f;

		// Token: 0x040015AA RID: 5546
		public float m_randTilt;

		// Token: 0x040015AB RID: 5547
		public float m_chanceToUseGroundTilt;

		// Token: 0x040015AC RID: 5548
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x040015AD RID: 5549
		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		// Token: 0x040015AE RID: 5550
		public bool m_blockCheck = true;

		// Token: 0x040015AF RID: 5551
		public bool m_snapToStaticSolid;

		// Token: 0x040015B0 RID: 5552
		public float m_minAltitude = -1000f;

		// Token: 0x040015B1 RID: 5553
		public float m_maxAltitude = 1000f;

		// Token: 0x040015B2 RID: 5554
		public float m_minVegetation;

		// Token: 0x040015B3 RID: 5555
		public float m_maxVegetation;

		// Token: 0x040015B4 RID: 5556
		public float m_minOceanDepth;

		// Token: 0x040015B5 RID: 5557
		public float m_maxOceanDepth;

		// Token: 0x040015B6 RID: 5558
		public float m_minTilt;

		// Token: 0x040015B7 RID: 5559
		public float m_maxTilt = 90f;

		// Token: 0x040015B8 RID: 5560
		public float m_terrainDeltaRadius;

		// Token: 0x040015B9 RID: 5561
		public float m_maxTerrainDelta = 2f;

		// Token: 0x040015BA RID: 5562
		public float m_minTerrainDelta;

		// Token: 0x040015BB RID: 5563
		public bool m_snapToWater;

		// Token: 0x040015BC RID: 5564
		public float m_groundOffset;

		// Token: 0x040015BD RID: 5565
		public int m_groupSizeMin = 1;

		// Token: 0x040015BE RID: 5566
		public int m_groupSizeMax = 1;

		// Token: 0x040015BF RID: 5567
		public float m_groupRadius;

		// Token: 0x040015C0 RID: 5568
		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		// Token: 0x040015C1 RID: 5569
		public float m_forestTresholdMin;

		// Token: 0x040015C2 RID: 5570
		public float m_forestTresholdMax = 1f;

		// Token: 0x040015C3 RID: 5571
		[HideInInspector]
		public bool m_foldout;
	}

	// Token: 0x0200020D RID: 525
	[Serializable]
	public class ZoneLocation
	{
		// Token: 0x06001514 RID: 5396 RVA: 0x0008A6A5 File Offset: 0x000888A5
		public ZoneSystem.ZoneLocation Clone()
		{
			return base.MemberwiseClone() as ZoneSystem.ZoneLocation;
		}

		// Token: 0x040015C4 RID: 5572
		public bool m_enable = true;

		// Token: 0x040015C5 RID: 5573
		public string m_prefabName;

		// Token: 0x040015C6 RID: 5574
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x040015C7 RID: 5575
		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		// Token: 0x040015C8 RID: 5576
		public int m_quantity;

		// Token: 0x040015C9 RID: 5577
		public bool m_prioritized;

		// Token: 0x040015CA RID: 5578
		public bool m_centerFirst;

		// Token: 0x040015CB RID: 5579
		public bool m_unique;

		// Token: 0x040015CC RID: 5580
		public string m_group = "";

		// Token: 0x040015CD RID: 5581
		public float m_minDistanceFromSimilar;

		// Token: 0x040015CE RID: 5582
		public bool m_iconAlways;

		// Token: 0x040015CF RID: 5583
		public bool m_iconPlaced;

		// Token: 0x040015D0 RID: 5584
		public bool m_randomRotation = true;

		// Token: 0x040015D1 RID: 5585
		public bool m_slopeRotation;

		// Token: 0x040015D2 RID: 5586
		public bool m_snapToWater;

		// Token: 0x040015D3 RID: 5587
		public float m_minTerrainDelta;

		// Token: 0x040015D4 RID: 5588
		public float m_maxTerrainDelta = 2f;

		// Token: 0x040015D5 RID: 5589
		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		// Token: 0x040015D6 RID: 5590
		public float m_forestTresholdMin;

		// Token: 0x040015D7 RID: 5591
		public float m_forestTresholdMax = 1f;

		// Token: 0x040015D8 RID: 5592
		[Space(10f)]
		public float m_minDistance;

		// Token: 0x040015D9 RID: 5593
		public float m_maxDistance;

		// Token: 0x040015DA RID: 5594
		public float m_minAltitude = -1000f;

		// Token: 0x040015DB RID: 5595
		public float m_maxAltitude = 1000f;

		// Token: 0x040015DC RID: 5596
		[NonSerialized]
		public GameObject m_prefab;

		// Token: 0x040015DD RID: 5597
		[NonSerialized]
		public int m_hash;

		// Token: 0x040015DE RID: 5598
		[NonSerialized]
		public Location m_location;

		// Token: 0x040015DF RID: 5599
		[NonSerialized]
		public float m_interiorRadius = 10f;

		// Token: 0x040015E0 RID: 5600
		[NonSerialized]
		public float m_exteriorRadius = 10f;

		// Token: 0x040015E1 RID: 5601
		[NonSerialized]
		public Vector3 m_interiorPosition;

		// Token: 0x040015E2 RID: 5602
		[NonSerialized]
		public Vector3 m_generatorPosition;

		// Token: 0x040015E3 RID: 5603
		[NonSerialized]
		public List<ZNetView> m_netViews = new List<ZNetView>();

		// Token: 0x040015E4 RID: 5604
		[NonSerialized]
		public List<RandomSpawn> m_randomSpawns = new List<RandomSpawn>();

		// Token: 0x040015E5 RID: 5605
		[HideInInspector]
		public bool m_foldout;
	}

	// Token: 0x0200020E RID: 526
	public struct LocationInstance
	{
		// Token: 0x040015E6 RID: 5606
		public ZoneSystem.ZoneLocation m_location;

		// Token: 0x040015E7 RID: 5607
		public Vector3 m_position;

		// Token: 0x040015E8 RID: 5608
		public bool m_placed;
	}

	// Token: 0x0200020F RID: 527
	public enum SpawnMode
	{
		// Token: 0x040015EA RID: 5610
		Full,
		// Token: 0x040015EB RID: 5611
		Client,
		// Token: 0x040015EC RID: 5612
		Ghost
	}
}
