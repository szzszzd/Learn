using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200023E RID: 574
[ExecuteInEditMode]
public class Heightmap : MonoBehaviour
{
	// Token: 0x0600169A RID: 5786 RVA: 0x00094DA4 File Offset: 0x00092FA4
	private void Awake()
	{
		if (!this.m_isDistantLod)
		{
			Heightmap.s_heightmaps.Add(this);
		}
		if (Heightmap.s_shaderPropertyClearedMaskTex == 0)
		{
			Heightmap.s_shaderPropertyClearedMaskTex = Shader.PropertyToID("_ClearedMaskTex");
		}
		this.m_collider = base.GetComponent<MeshCollider>();
		if (this.m_material == null)
		{
			base.enabled = false;
		}
		this.UpdateShadowSettings();
		this.m_renderMaterialPropertyBlock = new MaterialPropertyBlock();
	}

	// Token: 0x0600169B RID: 5787 RVA: 0x00094E0C File Offset: 0x0009300C
	private void OnDestroy()
	{
		if (!this.m_isDistantLod)
		{
			Heightmap.s_heightmaps.Remove(this);
		}
		if (this.m_materialInstance)
		{
			UnityEngine.Object.DestroyImmediate(this.m_materialInstance);
		}
		if (this.m_collisionMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_collisionMesh);
		}
		if (this.m_renderMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_renderMesh);
		}
		if (this.m_paintMask != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_paintMask);
		}
	}

	// Token: 0x0600169C RID: 5788 RVA: 0x00094E90 File Offset: 0x00093090
	private void OnEnable()
	{
		Heightmap.Instances.Add(this);
		this.UpdateShadowSettings();
		if (this.m_isDistantLod && Application.isPlaying && !this.m_distantLodEditorHax)
		{
			return;
		}
		this.Regenerate();
	}

	// Token: 0x0600169D RID: 5789 RVA: 0x00094EC1 File Offset: 0x000930C1
	private void OnDisable()
	{
		Heightmap.Instances.Remove(this);
	}

	// Token: 0x0600169E RID: 5790 RVA: 0x00094ECF File Offset: 0x000930CF
	public void CustomUpdate()
	{
		if (this.m_dirty)
		{
			this.m_dirty = false;
			this.m_materialInstance.SetTexture(Heightmap.s_shaderPropertyClearedMaskTex, this.m_paintMask);
			this.RebuildRenderMesh();
		}
		this.Render();
	}

	// Token: 0x0600169F RID: 5791 RVA: 0x00094F04 File Offset: 0x00093104
	private void Render()
	{
		if (!this.m_renderMesh)
		{
			return;
		}
		this.m_renderMatrix.SetTRS(base.transform.position, Quaternion.identity, Vector3.one);
		Graphics.DrawMesh(this.m_renderMesh, this.m_renderMatrix, this.m_materialInstance, base.gameObject.layer, Camera.current, 0, this.m_renderMaterialPropertyBlock, this.m_shadowMode, this.m_receiveShadows);
	}

	// Token: 0x060016A0 RID: 5792 RVA: 0x00094F79 File Offset: 0x00093179
	public void CustomLateUpdate()
	{
		if (!this.m_doLateUpdate)
		{
			return;
		}
		this.m_doLateUpdate = false;
		this.Regenerate();
	}

	// Token: 0x060016A1 RID: 5793 RVA: 0x00094F91 File Offset: 0x00093191
	private void UpdateShadowSettings()
	{
		if (this.m_isDistantLod)
		{
			this.m_shadowMode = (Heightmap.EnableDistantTerrainShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			this.m_receiveShadows = false;
			return;
		}
		this.m_shadowMode = (Heightmap.EnableDistantTerrainShadows ? ShadowCastingMode.On : ShadowCastingMode.TwoSided);
		this.m_receiveShadows = true;
	}

	// Token: 0x060016A2 RID: 5794 RVA: 0x00094FCC File Offset: 0x000931CC
	public static void ForceGenerateAll()
	{
		foreach (Heightmap heightmap in Heightmap.s_heightmaps)
		{
			if (heightmap.HaveQueuedRebuild())
			{
				ZLog.Log("Force generating hmap " + heightmap.transform.position.ToString());
				heightmap.Regenerate();
			}
		}
	}

	// Token: 0x060016A3 RID: 5795 RVA: 0x00095050 File Offset: 0x00093250
	public void Poke(bool delayed)
	{
		if (delayed)
		{
			this.m_doLateUpdate = true;
			return;
		}
		this.Regenerate();
	}

	// Token: 0x060016A4 RID: 5796 RVA: 0x00095063 File Offset: 0x00093263
	public bool HaveQueuedRebuild()
	{
		return this.m_doLateUpdate;
	}

	// Token: 0x060016A5 RID: 5797 RVA: 0x0009506B File Offset: 0x0009326B
	public void Regenerate()
	{
		this.m_doLateUpdate = false;
		this.Generate();
		this.RebuildCollisionMesh();
		this.UpdateCornerDepths();
		this.m_dirty = true;
	}

	// Token: 0x060016A6 RID: 5798 RVA: 0x00095090 File Offset: 0x00093290
	private void UpdateCornerDepths()
	{
		float num = ZoneSystem.instance ? ZoneSystem.instance.m_waterLevel : 30f;
		this.m_oceanDepth[0] = this.GetHeight(0, this.m_width);
		this.m_oceanDepth[1] = this.GetHeight(this.m_width, this.m_width);
		this.m_oceanDepth[2] = this.GetHeight(this.m_width, 0);
		this.m_oceanDepth[3] = this.GetHeight(0, 0);
		this.m_oceanDepth[0] = Mathf.Max(0f, num - this.m_oceanDepth[0]);
		this.m_oceanDepth[1] = Mathf.Max(0f, num - this.m_oceanDepth[1]);
		this.m_oceanDepth[2] = Mathf.Max(0f, num - this.m_oceanDepth[2]);
		this.m_oceanDepth[3] = Mathf.Max(0f, num - this.m_oceanDepth[3]);
		this.m_materialInstance.SetFloatArray("_depth", this.m_oceanDepth);
	}

	// Token: 0x060016A7 RID: 5799 RVA: 0x00095195 File Offset: 0x00093395
	public float[] GetOceanDepth()
	{
		return this.m_oceanDepth;
	}

	// Token: 0x060016A8 RID: 5800 RVA: 0x000951A0 File Offset: 0x000933A0
	public float GetOceanDepth(Vector3 worldPos)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float t = (float)num / (float)this.m_width;
		float t2 = (float)num2 / (float)this.m_width;
		float a = Mathf.Lerp(this.m_oceanDepth[3], this.m_oceanDepth[2], t);
		float b = Mathf.Lerp(this.m_oceanDepth[0], this.m_oceanDepth[1], t);
		return Mathf.Lerp(a, b, t2);
	}

	// Token: 0x060016A9 RID: 5801 RVA: 0x00095204 File Offset: 0x00093404
	private void Initialize()
	{
		int num = this.m_width + 1;
		int num2 = num * num;
		if (this.m_heights.Count == num2)
		{
			return;
		}
		this.m_heights.Clear();
		for (int i = 0; i < num2; i++)
		{
			this.m_heights.Add(0f);
		}
		this.m_paintMask = new Texture2D(this.m_width, this.m_width);
		this.m_paintMask.name = "_Heightmap m_paintMask";
		this.m_paintMask.wrapMode = TextureWrapMode.Clamp;
		this.m_materialInstance = new Material(this.m_material);
		this.m_materialInstance.SetTexture(Heightmap.s_shaderPropertyClearedMaskTex, this.m_paintMask);
	}

	// Token: 0x060016AA RID: 5802 RVA: 0x000952AC File Offset: 0x000934AC
	private void Generate()
	{
		if (WorldGenerator.instance == null)
		{
			ZLog.LogError("The WorldGenerator instance was null");
			throw new NullReferenceException("The WorldGenerator instance was null");
		}
		this.Initialize();
		int num = this.m_width + 1;
		int num2 = num * num;
		Vector3 position = base.transform.position;
		if (this.m_buildData == null || this.m_buildData.m_baseHeights.Count != num2 || this.m_buildData.m_center != position || this.m_buildData.m_scale != this.m_scale || this.m_buildData.m_worldGen != WorldGenerator.instance)
		{
			this.m_buildData = HeightmapBuilder.instance.RequestTerrainSync(position, this.m_width, this.m_scale, this.m_isDistantLod, WorldGenerator.instance);
			this.m_cornerBiomes = this.m_buildData.m_cornerBiomes;
		}
		for (int i = 0; i < num2; i++)
		{
			this.m_heights[i] = this.m_buildData.m_baseHeights[i];
		}
		this.m_paintMask.SetPixels(this.m_buildData.m_baseMask);
		this.ApplyModifiers();
	}

	// Token: 0x060016AB RID: 5803 RVA: 0x000953C4 File Offset: 0x000935C4
	private static float Distance(float x, float y, float rx, float ry)
	{
		float num = x - rx;
		float num2 = y - ry;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float num4 = 1.414f - num3;
		return num4 * num4 * num4;
	}

	// Token: 0x060016AC RID: 5804 RVA: 0x000953F1 File Offset: 0x000935F1
	public bool HaveBiome(Heightmap.Biome biome)
	{
		return (this.m_cornerBiomes[0] & biome) != Heightmap.Biome.None || (this.m_cornerBiomes[1] & biome) != Heightmap.Biome.None || (this.m_cornerBiomes[2] & biome) != Heightmap.Biome.None || (this.m_cornerBiomes[3] & biome) > Heightmap.Biome.None;
	}

	// Token: 0x060016AD RID: 5805 RVA: 0x00095428 File Offset: 0x00093628
	public Heightmap.Biome GetBiome(Vector3 point)
	{
		if (this.m_isDistantLod)
		{
			return WorldGenerator.instance.GetBiome(point.x, point.z);
		}
		if (this.m_cornerBiomes[0] == this.m_cornerBiomes[1] && this.m_cornerBiomes[0] == this.m_cornerBiomes[2] && this.m_cornerBiomes[0] == this.m_cornerBiomes[3])
		{
			return this.m_cornerBiomes[0];
		}
		float x = point.x;
		float z = point.z;
		this.WorldToNormalizedHM(point, out x, out z);
		for (int i = 1; i < Heightmap.s_tempBiomeWeights.Length; i++)
		{
			Heightmap.s_tempBiomeWeights[i] = 0f;
		}
		Heightmap.s_tempBiomeWeights[Heightmap.s_biomeToIndex[this.m_cornerBiomes[0]]] += Heightmap.Distance(x, z, 0f, 0f);
		Heightmap.s_tempBiomeWeights[Heightmap.s_biomeToIndex[this.m_cornerBiomes[1]]] += Heightmap.Distance(x, z, 1f, 0f);
		Heightmap.s_tempBiomeWeights[Heightmap.s_biomeToIndex[this.m_cornerBiomes[2]]] += Heightmap.Distance(x, z, 0f, 1f);
		Heightmap.s_tempBiomeWeights[Heightmap.s_biomeToIndex[this.m_cornerBiomes[3]]] += Heightmap.Distance(x, z, 1f, 1f);
		int num = Heightmap.s_biomeToIndex[Heightmap.Biome.None];
		float num2 = -99999f;
		for (int j = 1; j < Heightmap.s_tempBiomeWeights.Length; j++)
		{
			if (Heightmap.s_tempBiomeWeights[j] > num2)
			{
				num = j;
				num2 = Heightmap.s_tempBiomeWeights[j];
			}
		}
		return Heightmap.s_indexToBiome[num];
	}

	// Token: 0x060016AE RID: 5806 RVA: 0x000955D9 File Offset: 0x000937D9
	public Heightmap.BiomeArea GetBiomeArea()
	{
		if (!this.IsBiomeEdge())
		{
			return Heightmap.BiomeArea.Median;
		}
		return Heightmap.BiomeArea.Edge;
	}

	// Token: 0x060016AF RID: 5807 RVA: 0x000955E6 File Offset: 0x000937E6
	public bool IsBiomeEdge()
	{
		return this.m_cornerBiomes[0] != this.m_cornerBiomes[1] || this.m_cornerBiomes[0] != this.m_cornerBiomes[2] || this.m_cornerBiomes[0] != this.m_cornerBiomes[3];
	}

	// Token: 0x060016B0 RID: 5808 RVA: 0x00095624 File Offset: 0x00093824
	private void ApplyModifiers()
	{
		List<TerrainModifier> allInstances = TerrainModifier.GetAllInstances();
		float[] array = null;
		float[] array2 = null;
		foreach (TerrainModifier terrainModifier in allInstances)
		{
			if (terrainModifier.enabled && this.TerrainVSModifier(terrainModifier))
			{
				if (terrainModifier.m_playerModifiction && array == null)
				{
					array = this.m_heights.ToArray();
					array2 = this.m_heights.ToArray();
				}
				this.ApplyModifier(terrainModifier, array, array2);
			}
		}
		TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(base.transform.position);
		if (terrainComp)
		{
			if (array == null)
			{
				array = this.m_heights.ToArray();
				array2 = this.m_heights.ToArray();
			}
			terrainComp.ApplyToHeightmap(this.m_paintMask, this.m_heights, array, array2, this);
		}
		this.m_paintMask.Apply();
	}

	// Token: 0x060016B1 RID: 5809 RVA: 0x0009570C File Offset: 0x0009390C
	private void ApplyModifier(TerrainModifier modifier, float[] baseHeights, float[] levelOnly)
	{
		if (modifier.m_level)
		{
			this.LevelTerrain(modifier.transform.position + Vector3.up * modifier.m_levelOffset, modifier.m_levelRadius, modifier.m_square, baseHeights, levelOnly, modifier.m_playerModifiction);
		}
		if (modifier.m_smooth)
		{
			this.SmoothTerrain2(modifier.transform.position + Vector3.up * modifier.m_levelOffset, modifier.m_smoothRadius, modifier.m_square, levelOnly, modifier.m_smoothPower, modifier.m_playerModifiction);
		}
		if (modifier.m_paintCleared)
		{
			this.PaintCleared(modifier.transform.position, modifier.m_paintRadius, modifier.m_paintType, modifier.m_paintHeightCheck, false);
		}
	}

	// Token: 0x060016B2 RID: 5810 RVA: 0x000957D0 File Offset: 0x000939D0
	public bool CheckTerrainModIsContained(TerrainModifier modifier)
	{
		Vector3 position = modifier.transform.position;
		float num = modifier.GetRadius() + 0.1f;
		Vector3 position2 = base.transform.position;
		float num2 = (float)this.m_width * this.m_scale * 0.5f;
		return position.x + num <= position2.x + num2 && position.x - num >= position2.x - num2 && position.z + num <= position2.z + num2 && position.z - num >= position2.z - num2;
	}

	// Token: 0x060016B3 RID: 5811 RVA: 0x00095868 File Offset: 0x00093A68
	public bool TerrainVSModifier(TerrainModifier modifier)
	{
		Vector3 position = modifier.transform.position;
		float num = modifier.GetRadius() + 4f;
		Vector3 position2 = base.transform.position;
		float num2 = (float)this.m_width * this.m_scale * 0.5f;
		return position.x + num >= position2.x - num2 && position.x - num <= position2.x + num2 && position.z + num >= position2.z - num2 && position.z - num <= position2.z + num2;
	}

	// Token: 0x060016B4 RID: 5812 RVA: 0x00095900 File Offset: 0x00093B00
	private Vector3 CalcVertex(int x, int y)
	{
		int num = this.m_width + 1;
		Vector3 a = new Vector3((float)this.m_width * this.m_scale * -0.5f, 0f, (float)this.m_width * this.m_scale * -0.5f);
		float y2 = this.m_heights[y * num + x];
		return a + new Vector3((float)x * this.m_scale, y2, (float)y * this.m_scale);
	}

	// Token: 0x060016B5 RID: 5813 RVA: 0x00095978 File Offset: 0x00093B78
	private Color GetBiomeColor(float ix, float iy)
	{
		if (this.m_cornerBiomes[0] == this.m_cornerBiomes[1] && this.m_cornerBiomes[0] == this.m_cornerBiomes[2] && this.m_cornerBiomes[0] == this.m_cornerBiomes[3])
		{
			return Heightmap.GetBiomeColor(this.m_cornerBiomes[0]);
		}
		Color32 biomeColor = Heightmap.GetBiomeColor(this.m_cornerBiomes[0]);
		Color32 biomeColor2 = Heightmap.GetBiomeColor(this.m_cornerBiomes[1]);
		Color32 biomeColor3 = Heightmap.GetBiomeColor(this.m_cornerBiomes[2]);
		Color32 biomeColor4 = Heightmap.GetBiomeColor(this.m_cornerBiomes[3]);
		Color32 a = Color32.Lerp(biomeColor, biomeColor2, ix);
		Color32 b = Color32.Lerp(biomeColor3, biomeColor4, ix);
		return Color32.Lerp(a, b, iy);
	}

	// Token: 0x060016B6 RID: 5814 RVA: 0x00095A24 File Offset: 0x00093C24
	public static Color32 GetBiomeColor(Heightmap.Biome biome)
	{
		if (biome <= Heightmap.Biome.Plains)
		{
			switch (biome)
			{
			case Heightmap.Biome.Meadows:
			case Heightmap.Biome.Meadows | Heightmap.Biome.Swamp:
				break;
			case Heightmap.Biome.Swamp:
				return new Color32(byte.MaxValue, 0, 0, 0);
			case Heightmap.Biome.Mountain:
				return new Color32(0, byte.MaxValue, 0, 0);
			default:
				if (biome == Heightmap.Biome.BlackForest)
				{
					return new Color32(0, 0, byte.MaxValue, 0);
				}
				if (biome == Heightmap.Biome.Plains)
				{
					return new Color32(0, 0, 0, byte.MaxValue);
				}
				break;
			}
		}
		else
		{
			if (biome == Heightmap.Biome.AshLands)
			{
				return new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
			}
			if (biome == Heightmap.Biome.DeepNorth)
			{
				return new Color32(0, byte.MaxValue, 0, 0);
			}
			if (biome == Heightmap.Biome.Mistlands)
			{
				return new Color32(0, 0, byte.MaxValue, byte.MaxValue);
			}
		}
		return new Color32(0, 0, 0, 0);
	}

	// Token: 0x060016B7 RID: 5815 RVA: 0x00095AE0 File Offset: 0x00093CE0
	private void RebuildCollisionMesh()
	{
		if (this.m_collisionMesh == null)
		{
			this.m_collisionMesh = new Mesh();
			this.m_collisionMesh.name = "___Heightmap m_collisionMesh";
		}
		int num = this.m_width + 1;
		float num2 = -999999f;
		float num3 = 999999f;
		Heightmap.s_tempVertices.Clear();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 vector = this.CalcVertex(j, i);
				Heightmap.s_tempVertices.Add(vector);
				if (vector.y > num2)
				{
					num2 = vector.y;
				}
				if (vector.y < num3)
				{
					num3 = vector.y;
				}
			}
		}
		this.m_collisionMesh.SetVertices(Heightmap.s_tempVertices);
		int num4 = (num - 1) * (num - 1) * 6;
		if ((ulong)this.m_collisionMesh.GetIndexCount(0) != (ulong)((long)num4))
		{
			Heightmap.s_tempIndices.Clear();
			for (int k = 0; k < num - 1; k++)
			{
				for (int l = 0; l < num - 1; l++)
				{
					int item = k * num + l;
					int item2 = k * num + l + 1;
					int item3 = (k + 1) * num + l + 1;
					int item4 = (k + 1) * num + l;
					Heightmap.s_tempIndices.Add(item);
					Heightmap.s_tempIndices.Add(item4);
					Heightmap.s_tempIndices.Add(item2);
					Heightmap.s_tempIndices.Add(item2);
					Heightmap.s_tempIndices.Add(item4);
					Heightmap.s_tempIndices.Add(item3);
				}
			}
			this.m_collisionMesh.SetIndices(Heightmap.s_tempIndices.ToArray(), MeshTopology.Triangles, 0);
		}
		if (this.m_collider)
		{
			this.m_collider.sharedMesh = this.m_collisionMesh;
		}
		float num5 = (float)this.m_width * this.m_scale * 0.5f;
		this.m_bounds.SetMinMax(base.transform.position + new Vector3(-num5, num3, -num5), base.transform.position + new Vector3(num5, num2, num5));
		this.m_boundingSphere.position = this.m_bounds.center;
		this.m_boundingSphere.radius = Vector3.Distance(this.m_boundingSphere.position, this.m_bounds.max);
	}

	// Token: 0x060016B8 RID: 5816 RVA: 0x00095D34 File Offset: 0x00093F34
	private void RebuildRenderMesh()
	{
		if (this.m_renderMesh == null)
		{
			this.m_renderMesh = new Mesh();
			this.m_renderMesh.name = "___Heightmap m_renderMesh";
		}
		WorldGenerator instance = WorldGenerator.instance;
		int num = this.m_width + 1;
		Vector3 vector = base.transform.position + new Vector3((float)this.m_width * this.m_scale * -0.5f, 0f, (float)this.m_width * this.m_scale * -0.5f);
		Heightmap.s_tempVertices.Clear();
		Heightmap.s_tempUVs.Clear();
		Heightmap.s_tempIndices.Clear();
		Heightmap.s_tempColors.Clear();
		for (int i = 0; i < num; i++)
		{
			float iy = Mathf.SmoothStep(0f, 1f, (float)i / (float)this.m_width);
			for (int j = 0; j < num; j++)
			{
				float ix = Mathf.SmoothStep(0f, 1f, (float)j / (float)this.m_width);
				Heightmap.s_tempUVs.Add(new Vector2((float)j / (float)this.m_width, (float)i / (float)this.m_width));
				if (this.m_isDistantLod)
				{
					float wx = vector.x + (float)j * this.m_scale;
					float wy = vector.z + (float)i * this.m_scale;
					Heightmap.Biome biome = instance.GetBiome(wx, wy);
					Heightmap.s_tempColors.Add(Heightmap.GetBiomeColor(biome));
				}
				else
				{
					Heightmap.s_tempColors.Add(this.GetBiomeColor(ix, iy));
				}
			}
		}
		this.m_collisionMesh.GetVertices(Heightmap.s_tempVertices);
		this.m_collisionMesh.GetIndices(Heightmap.s_tempIndices, 0);
		this.m_renderMesh.Clear();
		this.m_renderMesh.SetVertices(Heightmap.s_tempVertices);
		this.m_renderMesh.SetColors(Heightmap.s_tempColors);
		this.m_renderMesh.SetUVs(0, Heightmap.s_tempUVs);
		this.m_renderMesh.SetIndices(Heightmap.s_tempIndices, MeshTopology.Triangles, 0, true, 0);
		this.m_renderMesh.RecalculateNormals();
		this.m_renderMesh.RecalculateTangents();
		this.m_renderMesh.RecalculateBounds();
	}

	// Token: 0x060016B9 RID: 5817 RVA: 0x00095F60 File Offset: 0x00094160
	private void SmoothTerrain2(Vector3 worldPos, float radius, bool square, float[] levelOnlyHeights, float power, bool playerModifiction)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float b = worldPos.y - base.transform.position.y;
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		int num5 = this.m_width + 1;
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				float num6 = Vector2.Distance(a, new Vector2((float)j, (float)i));
				if (num6 <= num3)
				{
					float num7 = num6 / num3;
					if (j >= 0 && i >= 0 && j < num5 && i < num5)
					{
						if (power == 3f)
						{
							num7 = num7 * num7 * num7;
						}
						else
						{
							num7 = Mathf.Pow(num7, power);
						}
						float height = this.GetHeight(j, i);
						float t = 1f - num7;
						float num8 = Mathf.Lerp(height, b, t);
						if (playerModifiction)
						{
							float num9 = levelOnlyHeights[i * num5 + j];
							num8 = Mathf.Clamp(num8, num9 - 1f, num9 + 1f);
						}
						this.SetHeight(j, i, num8);
					}
				}
			}
		}
	}

	// Token: 0x060016BA RID: 5818 RVA: 0x000960A0 File Offset: 0x000942A0
	private bool AtMaxWorldLevelDepth(Vector3 worldPos)
	{
		float num;
		this.GetWorldHeight(worldPos, out num);
		float num2;
		this.GetWorldBaseHeight(worldPos, out num2);
		return Mathf.Max(-(num - num2), 0f) >= 7.95f;
	}

	// Token: 0x060016BB RID: 5819 RVA: 0x000960DC File Offset: 0x000942DC
	private bool GetWorldBaseHeight(Vector3 worldPos, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		int num3 = this.m_width + 1;
		if (num < 0 || num2 < 0 || num >= num3 || num2 >= num3)
		{
			height = 0f;
			return false;
		}
		height = this.m_buildData.m_baseHeights[num2 * num3 + num] + base.transform.position.y;
		return true;
	}

	// Token: 0x060016BC RID: 5820 RVA: 0x00096140 File Offset: 0x00094340
	private bool GetWorldHeight(Vector3 worldPos, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		int num3 = this.m_width + 1;
		if (num < 0 || num2 < 0 || num >= num3 || num2 >= num3)
		{
			height = 0f;
			return false;
		}
		height = this.m_heights[num2 * num3 + num] + base.transform.position.y;
		return true;
	}

	// Token: 0x060016BD RID: 5821 RVA: 0x000961A0 File Offset: 0x000943A0
	public static bool AtMaxLevelDepth(Vector3 worldPos)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(worldPos);
		return heightmap && heightmap.AtMaxWorldLevelDepth(worldPos);
	}

	// Token: 0x060016BE RID: 5822 RVA: 0x000961C8 File Offset: 0x000943C8
	public static bool GetHeight(Vector3 worldPos, out float height)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(worldPos);
		if (heightmap && heightmap.GetWorldHeight(worldPos, out height))
		{
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x060016BF RID: 5823 RVA: 0x000961F8 File Offset: 0x000943F8
	private void PaintCleared(Vector3 worldPos, float radius, TerrainModifier.PaintType paintType, bool heightCheck, bool apply)
	{
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		float num = worldPos.y - base.transform.position.y;
		int num2;
		int num3;
		this.WorldToVertex(worldPos, out num2, out num3);
		float num4 = radius / this.m_scale;
		int num5 = Mathf.CeilToInt(num4);
		Vector2 a = new Vector2((float)num2, (float)num3);
		for (int i = num3 - num5; i <= num3 + num5; i++)
		{
			for (int j = num2 - num5; j <= num2 + num5; j++)
			{
				if (j >= 0 && i >= 0 && j < this.m_paintMask.width && i < this.m_paintMask.height && (!heightCheck || this.GetHeight(j, i) <= num))
				{
					float num6 = Vector2.Distance(a, new Vector2((float)j, (float)i));
					float num7 = 1f - Mathf.Clamp01(num6 / num4);
					num7 = Mathf.Pow(num7, 0.1f);
					Color color = this.m_paintMask.GetPixel(j, i);
					float a2 = color.a;
					switch (paintType)
					{
					case TerrainModifier.PaintType.Dirt:
						color = Color.Lerp(color, Heightmap.m_paintMaskDirt, num7);
						break;
					case TerrainModifier.PaintType.Cultivate:
						color = Color.Lerp(color, Heightmap.m_paintMaskCultivated, num7);
						break;
					case TerrainModifier.PaintType.Paved:
						color = Color.Lerp(color, Heightmap.m_paintMaskPaved, num7);
						break;
					case TerrainModifier.PaintType.Reset:
						color = Color.Lerp(color, Heightmap.m_paintMaskNothing, num7);
						break;
					}
					color.a = a2;
					this.m_paintMask.SetPixel(j, i, color);
				}
			}
		}
		if (apply)
		{
			this.m_paintMask.Apply();
		}
	}

	// Token: 0x060016C0 RID: 5824 RVA: 0x000963B8 File Offset: 0x000945B8
	public float GetVegetationMask(Vector3 worldPos)
	{
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		int x;
		int y;
		this.WorldToVertex(worldPos, out x, out y);
		return this.m_paintMask.GetPixel(x, y).a;
	}

	// Token: 0x060016C1 RID: 5825 RVA: 0x00096404 File Offset: 0x00094604
	public bool IsCleared(Vector3 worldPos)
	{
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		int x;
		int y;
		this.WorldToVertex(worldPos, out x, out y);
		Color pixel = this.m_paintMask.GetPixel(x, y);
		return pixel.r > 0.5f || pixel.g > 0.5f || pixel.b > 0.5f;
	}

	// Token: 0x060016C2 RID: 5826 RVA: 0x00096474 File Offset: 0x00094674
	public bool IsCultivated(Vector3 worldPos)
	{
		int x;
		int y;
		this.WorldToVertex(worldPos, out x, out y);
		return this.m_paintMask.GetPixel(x, y).g > 0.5f;
	}

	// Token: 0x060016C3 RID: 5827 RVA: 0x000964A8 File Offset: 0x000946A8
	public void WorldToVertex(Vector3 worldPos, out int x, out int y)
	{
		Vector3 vector = worldPos - base.transform.position;
		x = Mathf.FloorToInt(vector.x / this.m_scale + 0.5f) + this.m_width / 2;
		y = Mathf.FloorToInt(vector.z / this.m_scale + 0.5f) + this.m_width / 2;
	}

	// Token: 0x060016C4 RID: 5828 RVA: 0x00096510 File Offset: 0x00094710
	private void WorldToNormalizedHM(Vector3 worldPos, out float x, out float y)
	{
		float num = (float)this.m_width * this.m_scale;
		Vector3 vector = worldPos - base.transform.position;
		x = vector.x / num + 0.5f;
		y = vector.z / num + 0.5f;
	}

	// Token: 0x060016C5 RID: 5829 RVA: 0x00096560 File Offset: 0x00094760
	private void LevelTerrain(Vector3 worldPos, float radius, bool square, float[] baseHeights, float[] levelOnly, bool playerModifiction)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		Vector3 vector = worldPos - base.transform.position;
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		int num5 = this.m_width + 1;
		Vector2 a = new Vector2((float)num, (float)num2);
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if ((square || Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3) && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float num6 = vector.y;
					if (playerModifiction)
					{
						float num7 = baseHeights[i * num5 + j];
						num6 = Mathf.Clamp(num6, num7 - 8f, num7 + 8f);
						levelOnly[i * num5 + j] = num6;
					}
					this.SetHeight(j, i, num6);
				}
			}
		}
	}

	// Token: 0x060016C6 RID: 5830 RVA: 0x0009665E File Offset: 0x0009485E
	public Color GetPaintMask(int x, int y)
	{
		if (x < 0 || y < 0 || x >= this.m_width || y >= this.m_width)
		{
			return Color.black;
		}
		return this.m_paintMask.GetPixel(x, y);
	}

	// Token: 0x060016C7 RID: 5831 RVA: 0x00096690 File Offset: 0x00094890
	public float GetHeight(int x, int y)
	{
		int num = this.m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			return 0f;
		}
		return this.m_heights[y * num + x];
	}

	// Token: 0x060016C8 RID: 5832 RVA: 0x000966CC File Offset: 0x000948CC
	public void SetHeight(int x, int y, float h)
	{
		int num = this.m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			return;
		}
		this.m_heights[y * num + x] = h;
	}

	// Token: 0x060016C9 RID: 5833 RVA: 0x00096704 File Offset: 0x00094904
	public bool IsPointInside(Vector3 point, float radius = 0f)
	{
		float num = (float)this.m_width * this.m_scale * 0.5f;
		Vector3 position = base.transform.position;
		return point.x + radius >= position.x - num && point.x - radius <= position.x + num && point.z + radius >= position.z - num && point.z - radius <= position.z + num;
	}

	// Token: 0x060016CA RID: 5834 RVA: 0x0009677D File Offset: 0x0009497D
	public static List<Heightmap> GetAllHeightmaps()
	{
		return Heightmap.s_heightmaps;
	}

	// Token: 0x060016CB RID: 5835 RVA: 0x00096784 File Offset: 0x00094984
	public static Heightmap FindHeightmap(Vector3 point)
	{
		foreach (Heightmap heightmap in Heightmap.s_heightmaps)
		{
			if (heightmap.IsPointInside(point, 0f))
			{
				return heightmap;
			}
		}
		return null;
	}

	// Token: 0x060016CC RID: 5836 RVA: 0x000967E4 File Offset: 0x000949E4
	public static void FindHeightmap(Vector3 point, float radius, List<Heightmap> heightmaps)
	{
		foreach (Heightmap heightmap in Heightmap.s_heightmaps)
		{
			if (heightmap.IsPointInside(point, radius))
			{
				heightmaps.Add(heightmap);
			}
		}
	}

	// Token: 0x060016CD RID: 5837 RVA: 0x00096840 File Offset: 0x00094A40
	public static Heightmap.Biome FindBiome(Vector3 point)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(point);
		if (!heightmap)
		{
			return Heightmap.Biome.None;
		}
		return heightmap.GetBiome(point);
	}

	// Token: 0x060016CE RID: 5838 RVA: 0x00096868 File Offset: 0x00094A68
	public static bool HaveQueuedRebuild(Vector3 point, float radius)
	{
		Heightmap.s_tempHmaps.Clear();
		Heightmap.FindHeightmap(point, radius, Heightmap.s_tempHmaps);
		using (List<Heightmap>.Enumerator enumerator = Heightmap.s_tempHmaps.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.HaveQueuedRebuild())
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060016CF RID: 5839 RVA: 0x000968D8 File Offset: 0x00094AD8
	public static Heightmap.Biome FindBiomeClutter(Vector3 point)
	{
		if (ZoneSystem.instance && !ZoneSystem.instance.IsZoneLoaded(point))
		{
			return Heightmap.Biome.None;
		}
		Heightmap heightmap = Heightmap.FindHeightmap(point);
		if (heightmap)
		{
			return heightmap.GetBiome(point);
		}
		return Heightmap.Biome.None;
	}

	// Token: 0x060016D0 RID: 5840 RVA: 0x00096918 File Offset: 0x00094B18
	public void Clear()
	{
		this.m_heights.Clear();
		this.m_paintMask = null;
		this.m_materialInstance = null;
		this.m_buildData = null;
		if (this.m_collisionMesh)
		{
			this.m_collisionMesh.Clear();
		}
		if (this.m_renderMesh)
		{
			this.m_renderMesh.Clear();
		}
		if (this.m_collider)
		{
			this.m_collider.sharedMesh = null;
		}
	}

	// Token: 0x060016D1 RID: 5841 RVA: 0x00096990 File Offset: 0x00094B90
	public TerrainComp GetAndCreateTerrainCompiler()
	{
		TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(base.transform.position);
		if (terrainComp)
		{
			return terrainComp;
		}
		return UnityEngine.Object.Instantiate<GameObject>(this.m_terrainCompilerPrefab, base.transform.position, Quaternion.identity).GetComponent<TerrainComp>();
	}

	// Token: 0x170000E7 RID: 231
	// (get) Token: 0x060016D2 RID: 5842 RVA: 0x000969D8 File Offset: 0x00094BD8
	// (set) Token: 0x060016D3 RID: 5843 RVA: 0x000969E0 File Offset: 0x00094BE0
	public bool IsDistantLod
	{
		get
		{
			return this.m_isDistantLod;
		}
		set
		{
			if (this.m_isDistantLod == value)
			{
				return;
			}
			if (value)
			{
				Heightmap.s_heightmaps.Remove(this);
			}
			else
			{
				Heightmap.s_heightmaps.Add(this);
			}
			this.m_isDistantLod = value;
			this.UpdateShadowSettings();
		}
	}

	// Token: 0x170000E8 RID: 232
	// (get) Token: 0x060016D4 RID: 5844 RVA: 0x00096A15 File Offset: 0x00094C15
	// (set) Token: 0x060016D5 RID: 5845 RVA: 0x00096A1C File Offset: 0x00094C1C
	public static bool EnableDistantTerrainShadows
	{
		get
		{
			return Heightmap.s_enableDistantTerrainShadows;
		}
		set
		{
			if (Heightmap.s_enableDistantTerrainShadows == value)
			{
				return;
			}
			Heightmap.s_enableDistantTerrainShadows = value;
			foreach (Heightmap heightmap in Heightmap.Instances)
			{
				heightmap.UpdateShadowSettings();
			}
		}
	}

	// Token: 0x170000E9 RID: 233
	// (get) Token: 0x060016D6 RID: 5846 RVA: 0x00096A7C File Offset: 0x00094C7C
	public static List<Heightmap> Instances { get; } = new List<Heightmap>();

	// Token: 0x040017C8 RID: 6088
	private static readonly Dictionary<Heightmap.Biome, int> s_biomeToIndex = new Dictionary<Heightmap.Biome, int>
	{
		{
			Heightmap.Biome.None,
			0
		},
		{
			Heightmap.Biome.Meadows,
			1
		},
		{
			Heightmap.Biome.Swamp,
			2
		},
		{
			Heightmap.Biome.Mountain,
			3
		},
		{
			Heightmap.Biome.BlackForest,
			4
		},
		{
			Heightmap.Biome.Plains,
			5
		},
		{
			Heightmap.Biome.AshLands,
			6
		},
		{
			Heightmap.Biome.DeepNorth,
			7
		},
		{
			Heightmap.Biome.Ocean,
			8
		},
		{
			Heightmap.Biome.Mistlands,
			9
		}
	};

	// Token: 0x040017C9 RID: 6089
	private static readonly Heightmap.Biome[] s_indexToBiome = new Heightmap.Biome[]
	{
		Heightmap.Biome.None,
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Swamp,
		Heightmap.Biome.Mountain,
		Heightmap.Biome.BlackForest,
		Heightmap.Biome.Plains,
		Heightmap.Biome.AshLands,
		Heightmap.Biome.DeepNorth,
		Heightmap.Biome.Ocean,
		Heightmap.Biome.Mistlands
	};

	// Token: 0x040017CA RID: 6090
	private static readonly float[] s_tempBiomeWeights = new float[Enum.GetValues(typeof(Heightmap.Biome)).Length];

	// Token: 0x040017CB RID: 6091
	public GameObject m_terrainCompilerPrefab;

	// Token: 0x040017CC RID: 6092
	public int m_width = 32;

	// Token: 0x040017CD RID: 6093
	public float m_scale = 1f;

	// Token: 0x040017CE RID: 6094
	public Material m_material;

	// Token: 0x040017CF RID: 6095
	public const float c_LevelMaxDelta = 8f;

	// Token: 0x040017D0 RID: 6096
	public const float c_SmoothMaxDelta = 1f;

	// Token: 0x040017D1 RID: 6097
	[SerializeField]
	private bool m_isDistantLod;

	// Token: 0x040017D2 RID: 6098
	private ShadowCastingMode m_shadowMode = ShadowCastingMode.ShadowsOnly;

	// Token: 0x040017D3 RID: 6099
	private bool m_receiveShadows;

	// Token: 0x040017D4 RID: 6100
	public bool m_distantLodEditorHax;

	// Token: 0x040017D5 RID: 6101
	private MaterialPropertyBlock m_renderMaterialPropertyBlock;

	// Token: 0x040017D6 RID: 6102
	private Matrix4x4 m_renderMatrix;

	// Token: 0x040017D7 RID: 6103
	private static readonly List<Heightmap> s_tempHmaps = new List<Heightmap>();

	// Token: 0x040017D8 RID: 6104
	private readonly List<float> m_heights = new List<float>();

	// Token: 0x040017D9 RID: 6105
	private HeightmapBuilder.HMBuildData m_buildData;

	// Token: 0x040017DA RID: 6106
	private Texture2D m_paintMask;

	// Token: 0x040017DB RID: 6107
	private Material m_materialInstance;

	// Token: 0x040017DC RID: 6108
	private MeshCollider m_collider;

	// Token: 0x040017DD RID: 6109
	private readonly float[] m_oceanDepth = new float[4];

	// Token: 0x040017DE RID: 6110
	private Heightmap.Biome[] m_cornerBiomes = new Heightmap.Biome[]
	{
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Meadows
	};

	// Token: 0x040017DF RID: 6111
	private Bounds m_bounds;

	// Token: 0x040017E0 RID: 6112
	private BoundingSphere m_boundingSphere;

	// Token: 0x040017E1 RID: 6113
	private Mesh m_collisionMesh;

	// Token: 0x040017E2 RID: 6114
	private Mesh m_renderMesh;

	// Token: 0x040017E3 RID: 6115
	private bool m_dirty;

	// Token: 0x040017E4 RID: 6116
	private bool m_doLateUpdate;

	// Token: 0x040017E5 RID: 6117
	private static readonly List<Heightmap> s_heightmaps = new List<Heightmap>();

	// Token: 0x040017E6 RID: 6118
	private static readonly List<Vector3> s_tempVertices = new List<Vector3>();

	// Token: 0x040017E7 RID: 6119
	private static readonly List<Vector2> s_tempUVs = new List<Vector2>();

	// Token: 0x040017E8 RID: 6120
	private static readonly List<int> s_tempIndices = new List<int>();

	// Token: 0x040017E9 RID: 6121
	private static readonly List<Color32> s_tempColors = new List<Color32>();

	// Token: 0x040017EA RID: 6122
	public static Color m_paintMaskDirt = new Color(1f, 0f, 0f, 1f);

	// Token: 0x040017EB RID: 6123
	public static Color m_paintMaskCultivated = new Color(0f, 1f, 0f, 1f);

	// Token: 0x040017EC RID: 6124
	public static Color m_paintMaskPaved = new Color(0f, 0f, 1f, 1f);

	// Token: 0x040017ED RID: 6125
	public static Color m_paintMaskNothing = new Color(0f, 0f, 0f, 1f);

	// Token: 0x040017EE RID: 6126
	private static bool s_enableDistantTerrainShadows = false;

	// Token: 0x040017EF RID: 6127
	private static int s_shaderPropertyClearedMaskTex = 0;

	// Token: 0x0200023F RID: 575
	[Flags]
	public enum Biome
	{
		// Token: 0x040017F2 RID: 6130
		None = 0,
		// Token: 0x040017F3 RID: 6131
		Meadows = 1,
		// Token: 0x040017F4 RID: 6132
		Swamp = 2,
		// Token: 0x040017F5 RID: 6133
		Mountain = 4,
		// Token: 0x040017F6 RID: 6134
		BlackForest = 8,
		// Token: 0x040017F7 RID: 6135
		Plains = 16,
		// Token: 0x040017F8 RID: 6136
		AshLands = 32,
		// Token: 0x040017F9 RID: 6137
		DeepNorth = 64,
		// Token: 0x040017FA RID: 6138
		Ocean = 256,
		// Token: 0x040017FB RID: 6139
		Mistlands = 512
	}

	// Token: 0x02000240 RID: 576
	[Flags]
	public enum BiomeArea
	{
		// Token: 0x040017FD RID: 6141
		Edge = 1,
		// Token: 0x040017FE RID: 6142
		Median = 2,
		// Token: 0x040017FF RID: 6143
		Everything = 3
	}
}
