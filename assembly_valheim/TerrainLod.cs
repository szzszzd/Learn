using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000202 RID: 514
public class TerrainLod : MonoBehaviour
{
	// Token: 0x0600148A RID: 5258 RVA: 0x00085EED File Offset: 0x000840ED
	private void OnEnable()
	{
		this.CreateMeshes();
	}

	// Token: 0x0600148B RID: 5259 RVA: 0x00085EF5 File Offset: 0x000840F5
	private void OnDisable()
	{
		this.ResetMeshes();
	}

	// Token: 0x0600148C RID: 5260 RVA: 0x00085F00 File Offset: 0x00084100
	private void CreateMeshes()
	{
		float num = this.m_terrainSize / (float)this.m_regionsPerAxis;
		float num2 = Mathf.Round(this.m_vertexDistance);
		int width = Mathf.RoundToInt(num / num2);
		for (int i = 0; i < this.m_regionsPerAxis; i++)
		{
			for (int j = 0; j < this.m_regionsPerAxis; j++)
			{
				Vector3 offset = new Vector3(((float)i * 2f - (float)this.m_regionsPerAxis + 1f) * this.m_terrainSize * 0.5f / (float)this.m_regionsPerAxis, 0f, ((float)j * 2f - (float)this.m_regionsPerAxis + 1f) * this.m_terrainSize * 0.5f / (float)this.m_regionsPerAxis);
				this.CreateMesh(num2, width, offset);
			}
		}
	}

	// Token: 0x0600148D RID: 5261 RVA: 0x00085FC4 File Offset: 0x000841C4
	private void CreateMesh(float scale, int width, Vector3 offset)
	{
		GameObject gameObject = new GameObject("lodMesh");
		gameObject.transform.position = offset;
		gameObject.transform.SetParent(base.transform);
		Heightmap heightmap = gameObject.AddComponent<Heightmap>();
		this.m_heightmaps.Add(new TerrainLod.HeightmapWithOffset(heightmap, offset));
		heightmap.m_scale = scale;
		heightmap.m_width = width;
		heightmap.m_material = this.m_material;
		heightmap.IsDistantLod = true;
		heightmap.enabled = true;
	}

	// Token: 0x0600148E RID: 5262 RVA: 0x00086038 File Offset: 0x00084238
	private void ResetMeshes()
	{
		for (int i = 0; i < this.m_heightmaps.Count; i++)
		{
			UnityEngine.Object.Destroy(this.m_heightmaps[i].m_heightmap.gameObject);
		}
		this.m_heightmaps.Clear();
		this.m_lastPoint = new Vector3(99999f, 0f, 99999f);
		this.m_heightmapState = TerrainLod.HeightmapState.Done;
	}

	// Token: 0x0600148F RID: 5263 RVA: 0x000860A2 File Offset: 0x000842A2
	private void Update()
	{
		this.UpdateHeightmaps();
	}

	// Token: 0x06001490 RID: 5264 RVA: 0x000860AA File Offset: 0x000842AA
	private void UpdateHeightmaps()
	{
		if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
		{
			return;
		}
		if (!this.NeedsRebuild())
		{
			return;
		}
		if (!this.IsAllTerrainReady())
		{
			return;
		}
		this.RebuildAllHeightmaps();
	}

	// Token: 0x06001491 RID: 5265 RVA: 0x000860D0 File Offset: 0x000842D0
	private void RebuildAllHeightmaps()
	{
		for (int i = 0; i < this.m_heightmaps.Count; i++)
		{
			this.RebuildHeightmap(this.m_heightmaps[i]);
		}
		this.m_heightmapState = TerrainLod.HeightmapState.Done;
	}

	// Token: 0x06001492 RID: 5266 RVA: 0x0008610C File Offset: 0x0008430C
	private bool IsAllTerrainReady()
	{
		int num = 0;
		for (int i = 0; i < this.m_heightmaps.Count; i++)
		{
			if (this.IsTerrainReady(this.m_heightmaps[i]))
			{
				num++;
			}
		}
		return num == this.m_heightmaps.Count;
	}

	// Token: 0x06001493 RID: 5267 RVA: 0x00086158 File Offset: 0x00084358
	private bool IsTerrainReady(TerrainLod.HeightmapWithOffset heightmapWithOffset)
	{
		Heightmap heightmap = heightmapWithOffset.m_heightmap;
		Vector3 offset = heightmapWithOffset.m_offset;
		if (heightmapWithOffset.m_state == TerrainLod.HeightmapState.ReadyToRebuild)
		{
			return true;
		}
		if (HeightmapBuilder.instance.IsTerrainReady(this.m_lastPoint + offset, heightmap.m_width, heightmap.m_scale, heightmap.IsDistantLod, WorldGenerator.instance))
		{
			heightmapWithOffset.m_state = TerrainLod.HeightmapState.ReadyToRebuild;
			return true;
		}
		return false;
	}

	// Token: 0x06001494 RID: 5268 RVA: 0x000861B8 File Offset: 0x000843B8
	private void RebuildHeightmap(TerrainLod.HeightmapWithOffset heightmapWithOffset)
	{
		Heightmap heightmap = heightmapWithOffset.m_heightmap;
		Vector3 offset = heightmapWithOffset.m_offset;
		heightmap.transform.position = this.m_lastPoint + offset;
		heightmap.Regenerate();
		heightmapWithOffset.m_state = TerrainLod.HeightmapState.Done;
	}

	// Token: 0x06001495 RID: 5269 RVA: 0x000861F8 File Offset: 0x000843F8
	private bool NeedsRebuild()
	{
		if (this.m_heightmapState == TerrainLod.HeightmapState.NeedsRebuild)
		{
			return true;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return false;
		}
		Vector3 position = mainCamera.transform.position;
		if (Utils.DistanceXZ(position, this.m_lastPoint) > this.m_updateStepDistance && this.m_heightmapState == TerrainLod.HeightmapState.Done)
		{
			for (int i = 0; i < this.m_heightmaps.Count; i++)
			{
				this.m_heightmaps[i].m_state = TerrainLod.HeightmapState.NeedsRebuild;
			}
			this.m_lastPoint = new Vector3(Mathf.Round(position.x / this.m_vertexDistance) * this.m_vertexDistance, 0f, Mathf.Round(position.z / this.m_vertexDistance) * this.m_vertexDistance);
			this.m_heightmapState = TerrainLod.HeightmapState.NeedsRebuild;
			return true;
		}
		return false;
	}

	// Token: 0x04001544 RID: 5444
	[SerializeField]
	private float m_updateStepDistance = 256f;

	// Token: 0x04001545 RID: 5445
	[SerializeField]
	private float m_terrainSize = 2400f;

	// Token: 0x04001546 RID: 5446
	[SerializeField]
	private int m_regionsPerAxis = 3;

	// Token: 0x04001547 RID: 5447
	[SerializeField]
	private float m_vertexDistance = 10f;

	// Token: 0x04001548 RID: 5448
	[SerializeField]
	private Material m_material;

	// Token: 0x04001549 RID: 5449
	private List<TerrainLod.HeightmapWithOffset> m_heightmaps = new List<TerrainLod.HeightmapWithOffset>();

	// Token: 0x0400154A RID: 5450
	private Vector3 m_lastPoint = new Vector3(99999f, 0f, 99999f);

	// Token: 0x0400154B RID: 5451
	private TerrainLod.HeightmapState m_heightmapState = TerrainLod.HeightmapState.Done;

	// Token: 0x02000203 RID: 515
	private enum HeightmapState
	{
		// Token: 0x0400154D RID: 5453
		NeedsRebuild,
		// Token: 0x0400154E RID: 5454
		ReadyToRebuild,
		// Token: 0x0400154F RID: 5455
		Done
	}

	// Token: 0x02000204 RID: 516
	private class HeightmapWithOffset
	{
		// Token: 0x06001497 RID: 5271 RVA: 0x00086327 File Offset: 0x00084527
		public HeightmapWithOffset(Heightmap heightmap, Vector3 offset)
		{
			this.m_heightmap = heightmap;
			this.m_offset = offset;
			this.m_state = TerrainLod.HeightmapState.NeedsRebuild;
		}

		// Token: 0x04001550 RID: 5456
		public Heightmap m_heightmap;

		// Token: 0x04001551 RID: 5457
		public Vector3 m_offset;

		// Token: 0x04001552 RID: 5458
		public TerrainLod.HeightmapState m_state;
	}
}
