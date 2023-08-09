using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001D6 RID: 470
public class Pathfinding : MonoBehaviour
{
	// Token: 0x170000C8 RID: 200
	// (get) Token: 0x0600133B RID: 4923 RVA: 0x0007EC4C File Offset: 0x0007CE4C
	public static Pathfinding instance
	{
		get
		{
			return Pathfinding.m_instance;
		}
	}

	// Token: 0x0600133C RID: 4924 RVA: 0x0007EC53 File Offset: 0x0007CE53
	private void Awake()
	{
		Pathfinding.m_instance = this;
		this.SetupAgents();
		this.m_path = new NavMeshPath();
	}

	// Token: 0x0600133D RID: 4925 RVA: 0x0007EC6C File Offset: 0x0007CE6C
	private void ClearAgentSettings()
	{
		List<NavMeshBuildSettings> list = new List<NavMeshBuildSettings>();
		for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
		{
			list.Add(NavMesh.GetSettingsByIndex(i));
		}
		foreach (NavMeshBuildSettings navMeshBuildSettings in list)
		{
			if (navMeshBuildSettings.agentTypeID != 0)
			{
				NavMesh.RemoveSettings(navMeshBuildSettings.agentTypeID);
			}
		}
	}

	// Token: 0x0600133E RID: 4926 RVA: 0x0007ECEC File Offset: 0x0007CEEC
	private void OnDestroy()
	{
		foreach (Pathfinding.NavMeshTile navMeshTile in this.m_tiles.Values)
		{
			this.ClearLinks(navMeshTile);
			if (navMeshTile.m_data)
			{
				NavMesh.RemoveNavMeshData(navMeshTile.m_instance);
			}
		}
		this.m_tiles.Clear();
		this.DestroyAllLinks();
	}

	// Token: 0x0600133F RID: 4927 RVA: 0x0007ED70 File Offset: 0x0007CF70
	private Pathfinding.AgentSettings AddAgent(Pathfinding.AgentType type, Pathfinding.AgentSettings copy = null)
	{
		while (type + 1 > (Pathfinding.AgentType)this.m_agentSettings.Count)
		{
			this.m_agentSettings.Add(null);
		}
		Pathfinding.AgentSettings agentSettings = new Pathfinding.AgentSettings(type);
		if (copy != null)
		{
			agentSettings.m_build.agentHeight = copy.m_build.agentHeight;
			agentSettings.m_build.agentClimb = copy.m_build.agentClimb;
			agentSettings.m_build.agentRadius = copy.m_build.agentRadius;
			agentSettings.m_build.agentSlope = copy.m_build.agentSlope;
		}
		this.m_agentSettings[(int)type] = agentSettings;
		return agentSettings;
	}

	// Token: 0x06001340 RID: 4928 RVA: 0x0007EE10 File Offset: 0x0007D010
	private void SetupAgents()
	{
		this.ClearAgentSettings();
		Pathfinding.AgentSettings agentSettings = this.AddAgent(Pathfinding.AgentType.Humanoid, null);
		agentSettings.m_build.agentHeight = 1.8f;
		agentSettings.m_build.agentClimb = 0.3f;
		agentSettings.m_build.agentRadius = 0.4f;
		agentSettings.m_build.agentSlope = 85f;
		this.AddAgent(Pathfinding.AgentType.HumanoidNoSwim, agentSettings).m_canSwim = false;
		Pathfinding.AgentSettings agentSettings2 = this.AddAgent(Pathfinding.AgentType.HumanoidBig, agentSettings);
		agentSettings2.m_build.agentHeight = 2.5f;
		agentSettings2.m_build.agentClimb = 0.3f;
		agentSettings2.m_build.agentRadius = 0.5f;
		agentSettings2.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings3 = this.AddAgent(Pathfinding.AgentType.HumanoidBigNoSwim, null);
		agentSettings3.m_build.agentHeight = 2.5f;
		agentSettings3.m_build.agentClimb = 0.3f;
		agentSettings3.m_build.agentRadius = 0.5f;
		agentSettings3.m_build.agentSlope = 85f;
		agentSettings3.m_canSwim = false;
		this.AddAgent(Pathfinding.AgentType.HumanoidAvoidWater, agentSettings).m_avoidWater = true;
		Pathfinding.AgentSettings agentSettings4 = this.AddAgent(Pathfinding.AgentType.TrollSize, null);
		agentSettings4.m_build.agentHeight = 7f;
		agentSettings4.m_build.agentClimb = 0.6f;
		agentSettings4.m_build.agentRadius = 1f;
		agentSettings4.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings5 = this.AddAgent(Pathfinding.AgentType.Abomination, null);
		agentSettings5.m_build.agentHeight = 5f;
		agentSettings5.m_build.agentClimb = 0.6f;
		agentSettings5.m_build.agentRadius = 1.5f;
		agentSettings5.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings6 = this.AddAgent(Pathfinding.AgentType.SeekerQueen, null);
		agentSettings6.m_build.agentHeight = 7f;
		agentSettings6.m_build.agentClimb = 0.6f;
		agentSettings6.m_build.agentRadius = 1.5f;
		agentSettings6.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings7 = this.AddAgent(Pathfinding.AgentType.GoblinBruteSize, null);
		agentSettings7.m_build.agentHeight = 3.5f;
		agentSettings7.m_build.agentClimb = 0.3f;
		agentSettings7.m_build.agentRadius = 0.8f;
		agentSettings7.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings8 = this.AddAgent(Pathfinding.AgentType.HugeSize, null);
		agentSettings8.m_build.agentHeight = 10f;
		agentSettings8.m_build.agentClimb = 0.6f;
		agentSettings8.m_build.agentRadius = 2f;
		agentSettings8.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings9 = this.AddAgent(Pathfinding.AgentType.HorseSize, null);
		agentSettings9.m_build.agentHeight = 2.5f;
		agentSettings9.m_build.agentClimb = 0.3f;
		agentSettings9.m_build.agentRadius = 0.8f;
		agentSettings9.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings10 = this.AddAgent(Pathfinding.AgentType.Fish, null);
		agentSettings10.m_build.agentHeight = 0.5f;
		agentSettings10.m_build.agentClimb = 1f;
		agentSettings10.m_build.agentRadius = 0.5f;
		agentSettings10.m_build.agentSlope = 90f;
		agentSettings10.m_canSwim = true;
		agentSettings10.m_canWalk = false;
		agentSettings10.m_swimDepth = 0.4f;
		agentSettings10.m_areaMask = 12;
		Pathfinding.AgentSettings agentSettings11 = this.AddAgent(Pathfinding.AgentType.BigFish, null);
		agentSettings11.m_build.agentHeight = 1.5f;
		agentSettings11.m_build.agentClimb = 1f;
		agentSettings11.m_build.agentRadius = 1f;
		agentSettings11.m_build.agentSlope = 90f;
		agentSettings11.m_canSwim = true;
		agentSettings11.m_canWalk = false;
		agentSettings11.m_swimDepth = 1.5f;
		agentSettings11.m_areaMask = 12;
		NavMesh.SetAreaCost(0, this.m_defaultCost);
		NavMesh.SetAreaCost(3, this.m_waterCost);
	}

	// Token: 0x06001341 RID: 4929 RVA: 0x0007F1B4 File Offset: 0x0007D3B4
	private Pathfinding.AgentSettings GetSettings(Pathfinding.AgentType agentType)
	{
		return this.m_agentSettings[(int)agentType];
	}

	// Token: 0x06001342 RID: 4930 RVA: 0x0007F1C2 File Offset: 0x0007D3C2
	private int GetAgentID(Pathfinding.AgentType agentType)
	{
		return this.GetSettings(agentType).m_build.agentTypeID;
	}

	// Token: 0x06001343 RID: 4931 RVA: 0x0007F1D8 File Offset: 0x0007D3D8
	private void Update()
	{
		if (this.IsBuilding())
		{
			return;
		}
		this.m_updatePathfindingTimer += Time.deltaTime;
		if (this.m_updatePathfindingTimer > 0.1f)
		{
			this.m_updatePathfindingTimer = 0f;
			this.UpdatePathfinding();
		}
		if (!this.IsBuilding())
		{
			this.DestroyQueuedNavmeshData();
		}
	}

	// Token: 0x06001344 RID: 4932 RVA: 0x0007F22C File Offset: 0x0007D42C
	private void DestroyAllLinks()
	{
		while (this.m_linkRemoveQueue.Count > 0 || this.m_tileRemoveQueue.Count > 0)
		{
			this.DestroyQueuedNavmeshData();
		}
	}

	// Token: 0x06001345 RID: 4933 RVA: 0x0007F254 File Offset: 0x0007D454
	private void DestroyQueuedNavmeshData()
	{
		if (this.m_linkRemoveQueue.Count > 0)
		{
			int num = Mathf.Min(this.m_linkRemoveQueue.Count, Mathf.Max(25, this.m_linkRemoveQueue.Count / 40));
			for (int i = 0; i < num; i++)
			{
				NavMesh.RemoveLink(this.m_linkRemoveQueue.Dequeue());
			}
			return;
		}
		if (this.m_tileRemoveQueue.Count > 0)
		{
			NavMesh.RemoveNavMeshData(this.m_tileRemoveQueue.Dequeue());
		}
	}

	// Token: 0x06001346 RID: 4934 RVA: 0x0007F2D0 File Offset: 0x0007D4D0
	private void UpdatePathfinding()
	{
		this.Buildtiles();
		this.TimeoutTiles();
	}

	// Token: 0x06001347 RID: 4935 RVA: 0x0007F2DE File Offset: 0x0007D4DE
	public bool HavePath(Vector3 from, Vector3 to, Pathfinding.AgentType agentType)
	{
		return this.GetPath(from, to, null, agentType, true, false, true);
	}

	// Token: 0x06001348 RID: 4936 RVA: 0x0007F2F0 File Offset: 0x0007D4F0
	public bool FindValidPoint(out Vector3 point, Vector3 center, float range, Pathfinding.AgentType agentType)
	{
		this.PokePoint(center, agentType);
		Pathfinding.AgentSettings settings = this.GetSettings(agentType);
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(center, out navMeshHit, range, new NavMeshQueryFilter
		{
			agentTypeID = (int)settings.m_agentType,
			areaMask = settings.m_areaMask
		}))
		{
			point = navMeshHit.position;
			return true;
		}
		point = center;
		return false;
	}

	// Token: 0x06001349 RID: 4937 RVA: 0x0007F354 File Offset: 0x0007D554
	private bool IsUnderTerrain(Vector3 p)
	{
		float num;
		return ZoneSystem.instance.GetGroundHeight(p, out num) && p.y < num - 1f;
	}

	// Token: 0x0600134A RID: 4938 RVA: 0x0007F384 File Offset: 0x0007D584
	public bool GetPath(Vector3 from, Vector3 to, List<Vector3> path, Pathfinding.AgentType agentType, bool requireFullPath = false, bool cleanup = true, bool havePath = false)
	{
		if (path != null)
		{
			path.Clear();
		}
		this.PokeArea(from, agentType);
		this.PokeArea(to, agentType);
		Pathfinding.AgentSettings settings = this.GetSettings(agentType);
		if (!this.SnapToNavMesh(ref from, true, settings))
		{
			return false;
		}
		if (!this.SnapToNavMesh(ref to, !havePath, settings))
		{
			return false;
		}
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = settings.m_build.agentTypeID,
			areaMask = settings.m_areaMask
		};
		if (NavMesh.CalculatePath(from, to, filter, this.m_path))
		{
			if (this.m_path.status == NavMeshPathStatus.PathPartial)
			{
				if (this.IsUnderTerrain(this.m_path.corners[0]) || this.IsUnderTerrain(this.m_path.corners[this.m_path.corners.Length - 1]))
				{
					return false;
				}
				if (requireFullPath)
				{
					return false;
				}
			}
			if (path != null)
			{
				path.AddRange(this.m_path.corners);
				if (cleanup)
				{
					this.CleanPath(path, settings);
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x0600134B RID: 4939 RVA: 0x0007F488 File Offset: 0x0007D688
	private void CleanPath(List<Vector3> basePath, Pathfinding.AgentSettings settings)
	{
		if (basePath.Count <= 2)
		{
			return;
		}
		NavMeshQueryFilter filter = default(NavMeshQueryFilter);
		filter.agentTypeID = settings.m_build.agentTypeID;
		filter.areaMask = settings.m_areaMask;
		int num = 0;
		this.optPath.Clear();
		this.optPath.Add(basePath[num]);
		do
		{
			num = this.FindNextNode(basePath, filter, num);
			this.optPath.Add(basePath[num]);
		}
		while (num < basePath.Count - 1);
		this.tempPath.Clear();
		this.tempPath.Add(this.optPath[0]);
		for (int i = 1; i < this.optPath.Count - 1; i++)
		{
			Vector3 vector = this.optPath[i - 1];
			Vector3 vector2 = this.optPath[i];
			Vector3 vector3 = this.optPath[i + 1];
			Vector3 normalized = (vector3 - vector2).normalized;
			Vector3 normalized2 = (vector2 - vector).normalized;
			Vector3 vector4 = vector2 - (normalized + normalized2).normalized * Vector3.Distance(vector2, vector) * 0.33f;
			vector4.y = (vector2.y + vector.y) * 0.5f;
			Vector3 normalized3 = (vector4 - vector2).normalized;
			NavMeshHit navMeshHit;
			if (!NavMesh.Raycast(vector2 + normalized3 * 0.1f, vector4, out navMeshHit, filter) && !NavMesh.Raycast(vector4, vector, out navMeshHit, filter))
			{
				this.tempPath.Add(vector4);
			}
			this.tempPath.Add(vector2);
			Vector3 vector5 = vector2 + (normalized + normalized2).normalized * Vector3.Distance(vector2, vector3) * 0.33f;
			vector5.y = (vector2.y + vector3.y) * 0.5f;
			Vector3 normalized4 = (vector5 - vector2).normalized;
			if (!NavMesh.Raycast(vector2 + normalized4 * 0.1f, vector5, out navMeshHit, filter) && !NavMesh.Raycast(vector5, vector3, out navMeshHit, filter))
			{
				this.tempPath.Add(vector5);
			}
		}
		this.tempPath.Add(this.optPath[this.optPath.Count - 1]);
		basePath.Clear();
		basePath.AddRange(this.tempPath);
	}

	// Token: 0x0600134C RID: 4940 RVA: 0x0007F720 File Offset: 0x0007D920
	private int FindNextNode(List<Vector3> path, NavMeshQueryFilter filter, int start)
	{
		for (int i = start + 2; i < path.Count; i++)
		{
			NavMeshHit navMeshHit;
			if (NavMesh.Raycast(path[start], path[i], out navMeshHit, filter))
			{
				return i - 1;
			}
		}
		return path.Count - 1;
	}

	// Token: 0x0600134D RID: 4941 RVA: 0x0007F764 File Offset: 0x0007D964
	private bool SnapToNavMesh(ref Vector3 point, bool extendedSearchArea, Pathfinding.AgentSettings settings)
	{
		if (ZoneSystem.instance)
		{
			float num;
			if (ZoneSystem.instance.GetGroundHeight(point, out num) && point.y < num)
			{
				point.y = num;
			}
			if (settings.m_canSwim)
			{
				point.y = Mathf.Max(ZoneSystem.instance.m_waterLevel - settings.m_swimDepth, point.y);
			}
		}
		NavMeshQueryFilter filter = default(NavMeshQueryFilter);
		filter.agentTypeID = settings.m_build.agentTypeID;
		filter.areaMask = settings.m_areaMask;
		NavMeshHit navMeshHit;
		if (extendedSearchArea)
		{
			if (NavMesh.SamplePosition(point, out navMeshHit, 1.5f, filter))
			{
				point = navMeshHit.position;
				return true;
			}
			if (NavMesh.SamplePosition(point, out navMeshHit, 3f, filter))
			{
				point = navMeshHit.position;
				return true;
			}
			if (NavMesh.SamplePosition(point, out navMeshHit, 6f, filter))
			{
				point = navMeshHit.position;
				return true;
			}
			if (NavMesh.SamplePosition(point, out navMeshHit, 12f, filter))
			{
				point = navMeshHit.position;
				return true;
			}
		}
		else if (NavMesh.SamplePosition(point, out navMeshHit, 1f, filter))
		{
			point = navMeshHit.position;
			return true;
		}
		return false;
	}

	// Token: 0x0600134E RID: 4942 RVA: 0x0007F8B0 File Offset: 0x0007DAB0
	private void TimeoutTiles()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (KeyValuePair<Vector3Int, Pathfinding.NavMeshTile> keyValuePair in this.m_tiles)
		{
			if (realtimeSinceStartup - keyValuePair.Value.m_pokeTime > this.m_tileTimeout)
			{
				this.ClearLinks(keyValuePair.Value);
				if (keyValuePair.Value.m_instance.valid)
				{
					this.m_tileRemoveQueue.Enqueue(keyValuePair.Value.m_instance);
				}
				this.m_tiles.Remove(keyValuePair.Key);
				break;
			}
		}
	}

	// Token: 0x0600134F RID: 4943 RVA: 0x0007F964 File Offset: 0x0007DB64
	private void PokeArea(Vector3 point, Pathfinding.AgentType agentType)
	{
		Vector3Int tile = this.GetTile(point, agentType);
		this.PokeTile(tile);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (j != 0 || i != 0)
				{
					Vector3Int tileID = new Vector3Int(tile.x + j, tile.y + i, tile.z);
					this.PokeTile(tileID);
				}
			}
		}
	}

	// Token: 0x06001350 RID: 4944 RVA: 0x0007F9C8 File Offset: 0x0007DBC8
	private void PokePoint(Vector3 point, Pathfinding.AgentType agentType)
	{
		Vector3Int tile = this.GetTile(point, agentType);
		this.PokeTile(tile);
	}

	// Token: 0x06001351 RID: 4945 RVA: 0x0007F9E5 File Offset: 0x0007DBE5
	private void PokeTile(Vector3Int tileID)
	{
		this.GetNavTile(tileID).m_pokeTime = Time.realtimeSinceStartup;
	}

	// Token: 0x06001352 RID: 4946 RVA: 0x0007F9F8 File Offset: 0x0007DBF8
	private void Buildtiles()
	{
		if (this.UpdateAsyncBuild())
		{
			return;
		}
		Pathfinding.NavMeshTile navMeshTile = null;
		float num = 0f;
		foreach (KeyValuePair<Vector3Int, Pathfinding.NavMeshTile> keyValuePair in this.m_tiles)
		{
			float num2 = keyValuePair.Value.m_pokeTime - keyValuePair.Value.m_buildTime;
			if (num2 > this.m_updateInterval && (navMeshTile == null || num2 > num))
			{
				navMeshTile = keyValuePair.Value;
				num = num2;
			}
		}
		if (navMeshTile != null)
		{
			this.BuildTile(navMeshTile);
			navMeshTile.m_buildTime = Time.realtimeSinceStartup;
		}
	}

	// Token: 0x06001353 RID: 4947 RVA: 0x0007FAA4 File Offset: 0x0007DCA4
	private void BuildTile(Pathfinding.NavMeshTile tile)
	{
		DateTime now = DateTime.Now;
		List<NavMeshBuildSource> list = new List<NavMeshBuildSource>();
		List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
		Pathfinding.AgentType z = (Pathfinding.AgentType)tile.m_tile.z;
		Pathfinding.AgentSettings settings = this.GetSettings(z);
		Bounds includedWorldBounds = new Bounds(tile.m_center, new Vector3(this.m_tileSize, 6000f, this.m_tileSize));
		Bounds localBounds = new Bounds(Vector3.zero, new Vector3(this.m_tileSize, 6000f, this.m_tileSize));
		int defaultArea = settings.m_canWalk ? 0 : 1;
		NavMeshBuilder.CollectSources(includedWorldBounds, this.m_layers.value, NavMeshCollectGeometry.PhysicsColliders, defaultArea, markups, list);
		if (settings.m_avoidWater)
		{
			List<NavMeshBuildSource> list2 = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(includedWorldBounds, this.m_waterLayers.value, NavMeshCollectGeometry.PhysicsColliders, 1, markups, list2);
			using (List<NavMeshBuildSource>.Enumerator enumerator = list2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NavMeshBuildSource item = enumerator.Current;
					item.transform *= Matrix4x4.Translate(Vector3.down * 0.2f);
					list.Add(item);
				}
				goto IL_1AE;
			}
		}
		if (settings.m_canSwim)
		{
			List<NavMeshBuildSource> list3 = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(includedWorldBounds, this.m_waterLayers.value, NavMeshCollectGeometry.PhysicsColliders, 3, markups, list3);
			if (settings.m_swimDepth != 0f)
			{
				using (List<NavMeshBuildSource>.Enumerator enumerator = list3.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NavMeshBuildSource item2 = enumerator.Current;
						item2.transform *= Matrix4x4.Translate(Vector3.down * settings.m_swimDepth);
						list.Add(item2);
					}
					goto IL_1AE;
				}
			}
			list.AddRange(list3);
		}
		IL_1AE:
		if (tile.m_data == null)
		{
			tile.m_data = new NavMeshData();
			tile.m_data.position = tile.m_center;
		}
		this.m_buildOperation = NavMeshBuilder.UpdateNavMeshDataAsync(tile.m_data, settings.m_build, list, localBounds);
		this.m_buildTile = tile;
	}

	// Token: 0x06001354 RID: 4948 RVA: 0x0007FCC8 File Offset: 0x0007DEC8
	private bool IsBuilding()
	{
		return this.m_buildOperation != null && !this.m_buildOperation.isDone;
	}

	// Token: 0x06001355 RID: 4949 RVA: 0x0007FCE4 File Offset: 0x0007DEE4
	private bool UpdateAsyncBuild()
	{
		if (this.m_buildOperation == null)
		{
			return false;
		}
		if (!this.m_buildOperation.isDone)
		{
			return true;
		}
		if (!this.m_buildTile.m_instance.valid)
		{
			this.m_buildTile.m_instance = NavMesh.AddNavMeshData(this.m_buildTile.m_data);
		}
		this.RebuildLinks(this.m_buildTile);
		this.m_buildOperation = null;
		this.m_buildTile = null;
		return true;
	}

	// Token: 0x06001356 RID: 4950 RVA: 0x0007FD52 File Offset: 0x0007DF52
	private void ClearLinks(Pathfinding.NavMeshTile tile)
	{
		this.ClearLinks(tile.m_links1);
		this.ClearLinks(tile.m_links2);
	}

	// Token: 0x06001357 RID: 4951 RVA: 0x0007FD6C File Offset: 0x0007DF6C
	private void ClearLinks(List<KeyValuePair<Vector3, NavMeshLinkInstance>> links)
	{
		foreach (KeyValuePair<Vector3, NavMeshLinkInstance> keyValuePair in links)
		{
			this.m_linkRemoveQueue.Enqueue(keyValuePair.Value);
		}
		links.Clear();
	}

	// Token: 0x06001358 RID: 4952 RVA: 0x0007FDCC File Offset: 0x0007DFCC
	private void RebuildLinks(Pathfinding.NavMeshTile tile)
	{
		Pathfinding.AgentType z = (Pathfinding.AgentType)tile.m_tile.z;
		Pathfinding.AgentSettings settings = this.GetSettings(z);
		float num = this.m_tileSize / 2f;
		this.ConnectAlongEdge(tile.m_links1, tile.m_center + new Vector3(num, 0f, num), tile.m_center + new Vector3(num, 0f, -num), this.m_linkWidth, settings);
		this.ConnectAlongEdge(tile.m_links2, tile.m_center + new Vector3(-num, 0f, num), tile.m_center + new Vector3(num, 0f, num), this.m_linkWidth, settings);
	}

	// Token: 0x06001359 RID: 4953 RVA: 0x0007FE80 File Offset: 0x0007E080
	private void ConnectAlongEdge(List<KeyValuePair<Vector3, NavMeshLinkInstance>> links, Vector3 p0, Vector3 p1, float step, Pathfinding.AgentSettings settings)
	{
		Vector3 normalized = (p1 - p0).normalized;
		Vector3 a = Vector3.Cross(Vector3.up, normalized);
		float num = Vector3.Distance(p0, p1);
		bool canSwim = settings.m_canSwim;
		this.tempStitchPoints.Clear();
		for (float num2 = step / 2f; num2 <= num; num2 += step)
		{
			Vector3 p2 = p0 + normalized * num2;
			this.FindGround(p2, canSwim, this.tempStitchPoints, settings);
		}
		if (this.CompareLinks(this.tempStitchPoints, links))
		{
			return;
		}
		this.ClearLinks(links);
		foreach (Vector3 vector in this.tempStitchPoints)
		{
			NavMeshLinkInstance value = NavMesh.AddLink(new NavMeshLinkData
			{
				startPosition = vector - a * 0.1f,
				endPosition = vector + a * 0.1f,
				width = step,
				costModifier = this.m_linkCost,
				bidirectional = true,
				agentTypeID = settings.m_build.agentTypeID,
				area = 2
			});
			if (value.valid)
			{
				links.Add(new KeyValuePair<Vector3, NavMeshLinkInstance>(vector, value));
			}
		}
	}

	// Token: 0x0600135A RID: 4954 RVA: 0x0007FFF0 File Offset: 0x0007E1F0
	private bool CompareLinks(List<Vector3> tempStitchPoints, List<KeyValuePair<Vector3, NavMeshLinkInstance>> links)
	{
		if (tempStitchPoints.Count != links.Count)
		{
			return false;
		}
		for (int i = 0; i < tempStitchPoints.Count; i++)
		{
			if (tempStitchPoints[i] != links[i].Key)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600135B RID: 4955 RVA: 0x00080040 File Offset: 0x0007E240
	private bool SnapToNearestGround(Vector3 p, out Vector3 pos, float range)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up, Vector3.down, out raycastHit, range + 1f, this.m_layers.value | this.m_waterLayers.value))
		{
			pos = raycastHit.point;
			return true;
		}
		if (Physics.Raycast(p + Vector3.up * range, Vector3.down, out raycastHit, range, this.m_layers.value | this.m_waterLayers.value))
		{
			pos = raycastHit.point;
			return true;
		}
		pos = p;
		return false;
	}

	// Token: 0x0600135C RID: 4956 RVA: 0x000800E4 File Offset: 0x0007E2E4
	private void FindGround(Vector3 p, bool testWater, List<Vector3> hits, Pathfinding.AgentSettings settings)
	{
		p.y = 6000f;
		int layerMask = testWater ? (this.m_layers.value | this.m_waterLayers.value) : this.m_layers.value;
		float agentHeight = settings.m_build.agentHeight;
		float y = p.y;
		int num = Physics.RaycastNonAlloc(p, Vector3.down, this.tempHitArray, 10000f, layerMask);
		for (int i = 0; i < num; i++)
		{
			Vector3 point = this.tempHitArray[i].point;
			if (Mathf.Abs(point.y - y) >= agentHeight)
			{
				y = point.y;
				if ((1 << this.tempHitArray[i].collider.gameObject.layer & this.m_waterLayers) != 0)
				{
					point.y -= settings.m_swimDepth;
				}
				hits.Add(point);
			}
		}
	}

	// Token: 0x0600135D RID: 4957 RVA: 0x000801DC File Offset: 0x0007E3DC
	private Pathfinding.NavMeshTile GetNavTile(Vector3 point, Pathfinding.AgentType agent)
	{
		Vector3Int tile = this.GetTile(point, agent);
		return this.GetNavTile(tile);
	}

	// Token: 0x0600135E RID: 4958 RVA: 0x000801FC File Offset: 0x0007E3FC
	private Pathfinding.NavMeshTile GetNavTile(Vector3Int tile)
	{
		if (tile == this.m_cachedTileID)
		{
			return this.m_cachedTile;
		}
		Pathfinding.NavMeshTile navMeshTile;
		if (this.m_tiles.TryGetValue(tile, out navMeshTile))
		{
			this.m_cachedTileID = tile;
			this.m_cachedTile = navMeshTile;
			return navMeshTile;
		}
		navMeshTile = new Pathfinding.NavMeshTile();
		navMeshTile.m_tile = tile;
		navMeshTile.m_center = this.GetTilePos(tile);
		this.m_tiles.Add(tile, navMeshTile);
		this.m_cachedTileID = tile;
		this.m_cachedTile = navMeshTile;
		return navMeshTile;
	}

	// Token: 0x0600135F RID: 4959 RVA: 0x00080274 File Offset: 0x0007E474
	private Vector3Int GetTile(Vector3 point, Pathfinding.AgentType agent)
	{
		int x = Mathf.FloorToInt((point.x + this.m_tileSize / 2f) / this.m_tileSize);
		int y = Mathf.FloorToInt((point.z + this.m_tileSize / 2f) / this.m_tileSize);
		return new Vector3Int(x, y, (int)agent);
	}

	// Token: 0x06001360 RID: 4960 RVA: 0x000802C7 File Offset: 0x0007E4C7
	public Vector3 GetTilePos(Vector3Int id)
	{
		return new Vector3((float)id.x * this.m_tileSize, 2500f, (float)id.y * this.m_tileSize);
	}

	// Token: 0x04001421 RID: 5153
	private List<Vector3> tempPath = new List<Vector3>();

	// Token: 0x04001422 RID: 5154
	private List<Vector3> optPath = new List<Vector3>();

	// Token: 0x04001423 RID: 5155
	private List<Vector3> tempStitchPoints = new List<Vector3>();

	// Token: 0x04001424 RID: 5156
	private RaycastHit[] tempHitArray = new RaycastHit[255];

	// Token: 0x04001425 RID: 5157
	private static Pathfinding m_instance;

	// Token: 0x04001426 RID: 5158
	public LayerMask m_layers;

	// Token: 0x04001427 RID: 5159
	public LayerMask m_waterLayers;

	// Token: 0x04001428 RID: 5160
	private Dictionary<Vector3Int, Pathfinding.NavMeshTile> m_tiles = new Dictionary<Vector3Int, Pathfinding.NavMeshTile>();

	// Token: 0x04001429 RID: 5161
	public float m_tileSize = 32f;

	// Token: 0x0400142A RID: 5162
	public float m_defaultCost = 1f;

	// Token: 0x0400142B RID: 5163
	public float m_waterCost = 4f;

	// Token: 0x0400142C RID: 5164
	public float m_linkCost = 10f;

	// Token: 0x0400142D RID: 5165
	public float m_linkWidth = 1f;

	// Token: 0x0400142E RID: 5166
	public float m_updateInterval = 5f;

	// Token: 0x0400142F RID: 5167
	public float m_tileTimeout = 30f;

	// Token: 0x04001430 RID: 5168
	private const float m_tileHeight = 6000f;

	// Token: 0x04001431 RID: 5169
	private const float m_tileY = 2500f;

	// Token: 0x04001432 RID: 5170
	private float m_updatePathfindingTimer;

	// Token: 0x04001433 RID: 5171
	private Queue<Vector3Int> m_queuedAreas = new Queue<Vector3Int>();

	// Token: 0x04001434 RID: 5172
	private Queue<NavMeshLinkInstance> m_linkRemoveQueue = new Queue<NavMeshLinkInstance>();

	// Token: 0x04001435 RID: 5173
	private Queue<NavMeshDataInstance> m_tileRemoveQueue = new Queue<NavMeshDataInstance>();

	// Token: 0x04001436 RID: 5174
	private Vector3Int m_cachedTileID = new Vector3Int(-9999999, -9999999, -9999999);

	// Token: 0x04001437 RID: 5175
	private Pathfinding.NavMeshTile m_cachedTile;

	// Token: 0x04001438 RID: 5176
	private List<Pathfinding.AgentSettings> m_agentSettings = new List<Pathfinding.AgentSettings>();

	// Token: 0x04001439 RID: 5177
	private AsyncOperation m_buildOperation;

	// Token: 0x0400143A RID: 5178
	private Pathfinding.NavMeshTile m_buildTile;

	// Token: 0x0400143B RID: 5179
	private List<KeyValuePair<Pathfinding.NavMeshTile, Pathfinding.NavMeshTile>> m_edgeBuildQueue = new List<KeyValuePair<Pathfinding.NavMeshTile, Pathfinding.NavMeshTile>>();

	// Token: 0x0400143C RID: 5180
	private NavMeshPath m_path;

	// Token: 0x020001D7 RID: 471
	private class NavMeshTile
	{
		// Token: 0x0400143D RID: 5181
		public Vector3Int m_tile;

		// Token: 0x0400143E RID: 5182
		public Vector3 m_center;

		// Token: 0x0400143F RID: 5183
		public float m_pokeTime = -1000f;

		// Token: 0x04001440 RID: 5184
		public float m_buildTime = -1000f;

		// Token: 0x04001441 RID: 5185
		public NavMeshData m_data;

		// Token: 0x04001442 RID: 5186
		public NavMeshDataInstance m_instance;

		// Token: 0x04001443 RID: 5187
		public List<KeyValuePair<Vector3, NavMeshLinkInstance>> m_links1 = new List<KeyValuePair<Vector3, NavMeshLinkInstance>>();

		// Token: 0x04001444 RID: 5188
		public List<KeyValuePair<Vector3, NavMeshLinkInstance>> m_links2 = new List<KeyValuePair<Vector3, NavMeshLinkInstance>>();
	}

	// Token: 0x020001D8 RID: 472
	public enum AgentType
	{
		// Token: 0x04001446 RID: 5190
		Humanoid = 1,
		// Token: 0x04001447 RID: 5191
		TrollSize,
		// Token: 0x04001448 RID: 5192
		HugeSize,
		// Token: 0x04001449 RID: 5193
		HorseSize,
		// Token: 0x0400144A RID: 5194
		HumanoidNoSwim,
		// Token: 0x0400144B RID: 5195
		HumanoidAvoidWater,
		// Token: 0x0400144C RID: 5196
		Fish,
		// Token: 0x0400144D RID: 5197
		HumanoidBig,
		// Token: 0x0400144E RID: 5198
		BigFish,
		// Token: 0x0400144F RID: 5199
		GoblinBruteSize,
		// Token: 0x04001450 RID: 5200
		HumanoidBigNoSwim,
		// Token: 0x04001451 RID: 5201
		Abomination,
		// Token: 0x04001452 RID: 5202
		SeekerQueen
	}

	// Token: 0x020001D9 RID: 473
	public enum AreaType
	{
		// Token: 0x04001454 RID: 5204
		Default,
		// Token: 0x04001455 RID: 5205
		NotWalkable,
		// Token: 0x04001456 RID: 5206
		Jump,
		// Token: 0x04001457 RID: 5207
		Water
	}

	// Token: 0x020001DA RID: 474
	private class AgentSettings
	{
		// Token: 0x06001363 RID: 4963 RVA: 0x00080415 File Offset: 0x0007E615
		public AgentSettings(Pathfinding.AgentType type)
		{
			this.m_agentType = type;
			this.m_build = NavMesh.CreateSettings();
		}

		// Token: 0x04001458 RID: 5208
		public Pathfinding.AgentType m_agentType;

		// Token: 0x04001459 RID: 5209
		public NavMeshBuildSettings m_build;

		// Token: 0x0400145A RID: 5210
		public bool m_canWalk = true;

		// Token: 0x0400145B RID: 5211
		public bool m_avoidWater;

		// Token: 0x0400145C RID: 5212
		public bool m_canSwim = true;

		// Token: 0x0400145D RID: 5213
		public float m_swimDepth;

		// Token: 0x0400145E RID: 5214
		public int m_areaMask = -1;
	}
}
