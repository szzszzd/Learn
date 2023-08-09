using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001FD RID: 509
public class SpawnSystem : MonoBehaviour
{
	// Token: 0x06001463 RID: 5219 RVA: 0x00084B94 File Offset: 0x00082D94
	private void Awake()
	{
		SpawnSystem.m_instances.Add(this);
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_heightmap = Heightmap.FindHeightmap(base.transform.position);
		base.InvokeRepeating("UpdateSpawning", 10f, 4f);
	}

	// Token: 0x06001464 RID: 5220 RVA: 0x00084BE3 File Offset: 0x00082DE3
	private void OnDestroy()
	{
		SpawnSystem.m_instances.Remove(this);
	}

	// Token: 0x06001465 RID: 5221 RVA: 0x00084BF4 File Offset: 0x00082DF4
	private void UpdateSpawning()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (Player.m_localPlayer == null)
		{
			return;
		}
		SpawnSystem.m_tempNearPlayers.Clear();
		this.GetPlayersInZone(SpawnSystem.m_tempNearPlayers);
		if (SpawnSystem.m_tempNearPlayers.Count == 0)
		{
			return;
		}
		DateTime time = ZNet.instance.GetTime();
		foreach (SpawnSystemList spawnSystemList in this.m_spawnLists)
		{
			this.UpdateSpawnList(spawnSystemList.m_spawners, time, false);
		}
		List<SpawnSystem.SpawnData> currentSpawners = RandEventSystem.instance.GetCurrentSpawners();
		if (currentSpawners != null)
		{
			this.UpdateSpawnList(currentSpawners, time, true);
		}
	}

	// Token: 0x06001466 RID: 5222 RVA: 0x00084CBC File Offset: 0x00082EBC
	private void UpdateSpawnList(List<SpawnSystem.SpawnData> spawners, DateTime currentTime, bool eventSpawners)
	{
		string str = eventSpawners ? "e_" : "b_";
		int num = 0;
		foreach (SpawnSystem.SpawnData spawnData in spawners)
		{
			num++;
			if (spawnData.m_enabled && this.m_heightmap.HaveBiome(spawnData.m_biome))
			{
				int stableHashCode = (str + spawnData.m_prefab.name + num.ToString()).GetStableHashCode();
				DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(stableHashCode, 0L));
				TimeSpan timeSpan = currentTime - d;
				int num2 = Mathf.Min(spawnData.m_maxSpawned, (int)(timeSpan.TotalSeconds / (double)spawnData.m_spawnInterval));
				if (num2 > 0)
				{
					this.m_nview.GetZDO().Set(stableHashCode, currentTime.Ticks);
				}
				for (int i = 0; i < num2; i++)
				{
					if (UnityEngine.Random.Range(0f, 100f) <= spawnData.m_spawnChance)
					{
						if ((!string.IsNullOrEmpty(spawnData.m_requiredGlobalKey) && !ZoneSystem.instance.GetGlobalKey(spawnData.m_requiredGlobalKey)) || (spawnData.m_requiredEnvironments.Count > 0 && !EnvMan.instance.IsEnvironment(spawnData.m_requiredEnvironments)) || (!spawnData.m_spawnAtDay && EnvMan.instance.IsDay()) || (!spawnData.m_spawnAtNight && EnvMan.instance.IsNight()))
						{
							break;
						}
						int nrOfInstances = SpawnSystem.GetNrOfInstances(spawnData.m_prefab, Vector3.zero, 0f, eventSpawners, false);
						if (nrOfInstances >= spawnData.m_maxSpawned)
						{
							break;
						}
						Vector3 vector;
						Player player;
						if (this.FindBaseSpawnPoint(spawnData, SpawnSystem.m_tempNearPlayers, out vector, out player) && (spawnData.m_spawnDistance <= 0f || !SpawnSystem.HaveInstanceInRange(spawnData.m_prefab, vector, spawnData.m_spawnDistance)))
						{
							int num3 = Mathf.Min(UnityEngine.Random.Range(spawnData.m_groupSizeMin, spawnData.m_groupSizeMax + 1), spawnData.m_maxSpawned - nrOfInstances);
							float d2 = (num3 > 1) ? spawnData.m_groupRadius : 0f;
							int num4 = 0;
							for (int j = 0; j < num3 * 2; j++)
							{
								Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
								Vector3 a = vector + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * d2;
								if (this.IsSpawnPointGood(spawnData, ref a))
								{
									this.Spawn(spawnData, a + Vector3.up * spawnData.m_groundOffset, eventSpawners);
									num4++;
									if (num4 >= num3)
									{
										break;
									}
								}
							}
							ZLog.Log("Spawned " + spawnData.m_prefab.name + " x " + num4.ToString());
						}
					}
				}
			}
		}
	}

	// Token: 0x06001467 RID: 5223 RVA: 0x00084FA8 File Offset: 0x000831A8
	private void Spawn(SpawnSystem.SpawnData critter, Vector3 spawnPoint, bool eventSpawner)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(critter.m_prefab, spawnPoint, Quaternion.identity);
		BaseAI component = gameObject.GetComponent<BaseAI>();
		if (component != null && critter.m_huntPlayer)
		{
			component.SetHuntPlayer(true);
		}
		if (critter.m_maxLevel > 1 && (critter.m_levelUpMinCenterDistance <= 0f || spawnPoint.magnitude > critter.m_levelUpMinCenterDistance))
		{
			int num = critter.m_minLevel;
			float num2 = (critter.m_overrideLevelupChance >= 0f) ? critter.m_overrideLevelupChance : 10f;
			while (num < critter.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= num2)
			{
				num++;
			}
			if (num > 1)
			{
				Character component2 = gameObject.GetComponent<Character>();
				if (component2 != null)
				{
					component2.SetLevel(num);
				}
				if (gameObject.GetComponent<Fish>() != null)
				{
					ItemDrop component3 = gameObject.GetComponent<ItemDrop>();
					if (component3 != null)
					{
						component3.SetQuality(num);
					}
				}
			}
		}
		MonsterAI monsterAI = component as MonsterAI;
		if (monsterAI != null)
		{
			if (!critter.m_spawnAtDay)
			{
				monsterAI.SetDespawnInDay(true);
			}
			if (eventSpawner)
			{
				monsterAI.SetEventCreature(true);
			}
		}
	}

	// Token: 0x06001468 RID: 5224 RVA: 0x000850AC File Offset: 0x000832AC
	private bool IsSpawnPointGood(SpawnSystem.SpawnData spawn, ref Vector3 spawnPoint)
	{
		Vector3 vector;
		Heightmap.Biome biome;
		Heightmap.BiomeArea biomeArea;
		Heightmap heightmap;
		ZoneSystem.instance.GetGroundData(ref spawnPoint, out vector, out biome, out biomeArea, out heightmap);
		if ((spawn.m_biome & biome) == Heightmap.Biome.None)
		{
			return false;
		}
		if ((spawn.m_biomeArea & biomeArea) == (Heightmap.BiomeArea)0)
		{
			return false;
		}
		if (ZoneSystem.instance.IsBlocked(spawnPoint))
		{
			return false;
		}
		float num = spawnPoint.y - ZoneSystem.instance.m_waterLevel;
		if (num < spawn.m_minAltitude || num > spawn.m_maxAltitude)
		{
			return false;
		}
		float num2 = Mathf.Cos(0.017453292f * spawn.m_maxTilt);
		float num3 = Mathf.Cos(0.017453292f * spawn.m_minTilt);
		if (vector.y < num2 || vector.y > num3)
		{
			return false;
		}
		float range = (spawn.m_spawnRadiusMin > 0f) ? spawn.m_spawnRadiusMin : 40f;
		if (Player.IsPlayerInRange(spawnPoint, range))
		{
			return false;
		}
		if (EffectArea.IsPointInsideArea(spawnPoint, EffectArea.Type.PlayerBase, 0f))
		{
			return false;
		}
		if (!spawn.m_inForest || !spawn.m_outsideForest)
		{
			bool flag = WorldGenerator.InForest(spawnPoint);
			if (!spawn.m_inForest && flag)
			{
				return false;
			}
			if (!spawn.m_outsideForest && !flag)
			{
				return false;
			}
		}
		if (spawn.m_minOceanDepth != spawn.m_maxOceanDepth && heightmap != null)
		{
			float oceanDepth = heightmap.GetOceanDepth(spawnPoint);
			if (oceanDepth < spawn.m_minOceanDepth || oceanDepth > spawn.m_maxOceanDepth)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001469 RID: 5225 RVA: 0x0008521C File Offset: 0x0008341C
	private bool FindBaseSpawnPoint(SpawnSystem.SpawnData spawn, List<Player> allPlayers, out Vector3 spawnCenter, out Player targetPlayer)
	{
		float minInclusive = (spawn.m_spawnRadiusMin > 0f) ? spawn.m_spawnRadiusMin : 40f;
		float maxInclusive = (spawn.m_spawnRadiusMax > 0f) ? spawn.m_spawnRadiusMax : 80f;
		for (int i = 0; i < 20; i++)
		{
			Player player = allPlayers[UnityEngine.Random.Range(0, allPlayers.Count)];
			Vector3 a = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward;
			Vector3 vector = player.transform.position + a * UnityEngine.Random.Range(minInclusive, maxInclusive);
			if (this.IsSpawnPointGood(spawn, ref vector))
			{
				spawnCenter = vector;
				targetPlayer = player;
				return true;
			}
		}
		spawnCenter = Vector3.zero;
		targetPlayer = null;
		return false;
	}

	// Token: 0x0600146A RID: 5226 RVA: 0x000852F0 File Offset: 0x000834F0
	private int GetNrOfInstances(string prefabName)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		int num = 0;
		foreach (Character character in allCharacters)
		{
			if (character.gameObject.name.CustomStartsWith(prefabName) && this.InsideZone(character.transform.position, 0f))
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x0600146B RID: 5227 RVA: 0x00085370 File Offset: 0x00083570
	private void GetPlayersInZone(List<Player> players)
	{
		foreach (Player player in Player.GetAllPlayers())
		{
			if (this.InsideZone(player.transform.position, 0f))
			{
				players.Add(player);
			}
		}
	}

	// Token: 0x0600146C RID: 5228 RVA: 0x000853DC File Offset: 0x000835DC
	private void GetPlayersNearZone(List<Player> players, float marginDistance)
	{
		foreach (Player player in Player.GetAllPlayers())
		{
			if (this.InsideZone(player.transform.position, marginDistance))
			{
				players.Add(player);
			}
		}
	}

	// Token: 0x0600146D RID: 5229 RVA: 0x00085444 File Offset: 0x00083644
	private bool IsPlayerTooClose(List<Player> players, Vector3 point, float minDistance)
	{
		using (List<Player>.Enumerator enumerator = players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, point) < minDistance)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600146E RID: 5230 RVA: 0x000854A4 File Offset: 0x000836A4
	private bool InPlayerRange(List<Player> players, Vector3 point, float minDistance, float maxDistance)
	{
		bool result = false;
		foreach (Player player in players)
		{
			float num = Utils.DistanceXZ(player.transform.position, point);
			if (num < minDistance)
			{
				return false;
			}
			if (num < maxDistance)
			{
				result = true;
			}
		}
		return result;
	}

	// Token: 0x0600146F RID: 5231 RVA: 0x00085510 File Offset: 0x00083710
	private static bool HaveInstanceInRange(GameObject prefab, Vector3 centerPoint, float minDistance)
	{
		string name = prefab.name;
		if (prefab.GetComponent<BaseAI>() != null)
		{
			foreach (BaseAI baseAI in BaseAI.Instances)
			{
				if (baseAI.gameObject.name.CustomStartsWith(name) && Utils.DistanceXZ(baseAI.transform.position, centerPoint) < minDistance)
				{
					return true;
				}
			}
			return false;
		}
		foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("spawned"))
		{
			if (gameObject.gameObject.name.CustomStartsWith(name) && Utils.DistanceXZ(gameObject.transform.position, centerPoint) < minDistance)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001470 RID: 5232 RVA: 0x000855F4 File Offset: 0x000837F4
	public static int GetNrOfInstances(GameObject prefab)
	{
		return SpawnSystem.GetNrOfInstances(prefab, Vector3.zero, 0f, false, false);
	}

	// Token: 0x06001471 RID: 5233 RVA: 0x00085608 File Offset: 0x00083808
	public static int GetNrOfInstances(GameObject prefab, Vector3 center, float maxRange, bool eventCreaturesOnly = false, bool procreationOnly = false)
	{
		string b = prefab.name + "(Clone)";
		if (prefab.GetComponent<BaseAI>() != null)
		{
			List<BaseAI> instances = BaseAI.Instances;
			int num = 0;
			foreach (BaseAI baseAI in instances)
			{
				if (!(baseAI.gameObject.name != b) && (maxRange <= 0f || Vector3.Distance(center, baseAI.transform.position) <= maxRange))
				{
					if (eventCreaturesOnly)
					{
						MonsterAI monsterAI = baseAI as MonsterAI;
						if (monsterAI && !monsterAI.IsEventCreature())
						{
							continue;
						}
					}
					if (procreationOnly)
					{
						Procreation component = baseAI.GetComponent<Procreation>();
						if (component && !component.ReadyForProcreation())
						{
							continue;
						}
					}
					num++;
				}
			}
			return num;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("spawned");
		int num2 = 0;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name.CustomStartsWith(b) && (maxRange <= 0f || Vector3.Distance(center, gameObject.transform.position) <= maxRange))
			{
				num2++;
			}
		}
		return num2;
	}

	// Token: 0x06001472 RID: 5234 RVA: 0x00085744 File Offset: 0x00083944
	private bool InsideZone(Vector3 point, float extra = 0f)
	{
		float num = ZoneSystem.instance.m_zoneSize * 0.5f + extra;
		Vector3 position = base.transform.position;
		return point.x >= position.x - num && point.x <= position.x + num && point.z >= position.z - num && point.z <= position.z + num;
	}

	// Token: 0x06001473 RID: 5235 RVA: 0x000857B5 File Offset: 0x000839B5
	private bool HaveGlobalKeys(SpawnSystem.SpawnData ev)
	{
		return string.IsNullOrEmpty(ev.m_requiredGlobalKey) || ZoneSystem.instance.GetGlobalKey(ev.m_requiredGlobalKey);
	}

	// Token: 0x04001508 RID: 5384
	private static List<SpawnSystem> m_instances = new List<SpawnSystem>();

	// Token: 0x04001509 RID: 5385
	private const float m_spawnDistanceMin = 40f;

	// Token: 0x0400150A RID: 5386
	private const float m_spawnDistanceMax = 80f;

	// Token: 0x0400150B RID: 5387
	private const float m_levelupChance = 10f;

	// Token: 0x0400150C RID: 5388
	public List<SpawnSystemList> m_spawnLists = new List<SpawnSystemList>();

	// Token: 0x0400150D RID: 5389
	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();

	// Token: 0x0400150E RID: 5390
	private static List<Player> m_tempNearPlayers = new List<Player>();

	// Token: 0x0400150F RID: 5391
	private ZNetView m_nview;

	// Token: 0x04001510 RID: 5392
	private Heightmap m_heightmap;

	// Token: 0x020001FE RID: 510
	[Serializable]
	public class SpawnData
	{
		// Token: 0x06001476 RID: 5238 RVA: 0x0008580A File Offset: 0x00083A0A
		public SpawnSystem.SpawnData Clone()
		{
			SpawnSystem.SpawnData spawnData = base.MemberwiseClone() as SpawnSystem.SpawnData;
			spawnData.m_requiredEnvironments = new List<string>(this.m_requiredEnvironments);
			return spawnData;
		}

		// Token: 0x04001511 RID: 5393
		public string m_name = "";

		// Token: 0x04001512 RID: 5394
		public bool m_enabled = true;

		// Token: 0x04001513 RID: 5395
		public GameObject m_prefab;

		// Token: 0x04001514 RID: 5396
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x04001515 RID: 5397
		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		// Token: 0x04001516 RID: 5398
		[Header("Total nr of instances (if near player is set, only instances within the max spawn radius is counted)")]
		public int m_maxSpawned = 1;

		// Token: 0x04001517 RID: 5399
		[Header("How often do we spawn")]
		public float m_spawnInterval = 4f;

		// Token: 0x04001518 RID: 5400
		[Header("Chanse to spawn each spawn interval")]
		[Range(0f, 100f)]
		public float m_spawnChance = 100f;

		// Token: 0x04001519 RID: 5401
		[Header("Minimum distance to another instance")]
		public float m_spawnDistance = 10f;

		// Token: 0x0400151A RID: 5402
		[Header("Spawn range ( 0 = use global setting )")]
		public float m_spawnRadiusMin;

		// Token: 0x0400151B RID: 5403
		public float m_spawnRadiusMax;

		// Token: 0x0400151C RID: 5404
		[Header("Only spawn if this key is set")]
		public string m_requiredGlobalKey = "";

		// Token: 0x0400151D RID: 5405
		[Header("Only spawn if this environment is active")]
		public List<string> m_requiredEnvironments = new List<string>();

		// Token: 0x0400151E RID: 5406
		[Header("Group spawning")]
		public int m_groupSizeMin = 1;

		// Token: 0x0400151F RID: 5407
		public int m_groupSizeMax = 1;

		// Token: 0x04001520 RID: 5408
		public float m_groupRadius = 3f;

		// Token: 0x04001521 RID: 5409
		[Header("Time of day")]
		public bool m_spawnAtNight = true;

		// Token: 0x04001522 RID: 5410
		public bool m_spawnAtDay = true;

		// Token: 0x04001523 RID: 5411
		[Header("Altitude")]
		public float m_minAltitude = -1000f;

		// Token: 0x04001524 RID: 5412
		public float m_maxAltitude = 1000f;

		// Token: 0x04001525 RID: 5413
		[Header("Terrain tilt")]
		public float m_minTilt;

		// Token: 0x04001526 RID: 5414
		public float m_maxTilt = 35f;

		// Token: 0x04001527 RID: 5415
		[Header("Forest")]
		public bool m_inForest = true;

		// Token: 0x04001528 RID: 5416
		public bool m_outsideForest = true;

		// Token: 0x04001529 RID: 5417
		[Header("Ocean depth ")]
		public float m_minOceanDepth;

		// Token: 0x0400152A RID: 5418
		public float m_maxOceanDepth;

		// Token: 0x0400152B RID: 5419
		[Header("States")]
		public bool m_huntPlayer;

		// Token: 0x0400152C RID: 5420
		public float m_groundOffset = 0.5f;

		// Token: 0x0400152D RID: 5421
		[Header("Level")]
		public int m_maxLevel = 1;

		// Token: 0x0400152E RID: 5422
		public int m_minLevel = 1;

		// Token: 0x0400152F RID: 5423
		public float m_levelUpMinCenterDistance;

		// Token: 0x04001530 RID: 5424
		public float m_overrideLevelupChance = -1f;

		// Token: 0x04001531 RID: 5425
		[HideInInspector]
		public bool m_foldout;
	}
}
