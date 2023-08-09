using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002A2 RID: 674
public class TerrainComp : MonoBehaviour
{
	// Token: 0x060019B3 RID: 6579 RVA: 0x000A9E88 File Offset: 0x000A8088
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_hmap = Heightmap.FindHeightmap(base.transform.position);
		if (this.m_hmap == null)
		{
			ZLog.LogWarning("Terrain compiler could not find hmap");
			return;
		}
		TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(base.transform.position);
		if (terrainComp)
		{
			ZLog.LogWarning("Found another terrain compiler in this area, removing it");
			ZNetScene.instance.Destroy(terrainComp.gameObject);
		}
		TerrainComp.s_instances.Add(this);
		this.m_nview.Register<ZPackage>("ApplyOperation", new Action<long, ZPackage>(this.RPC_ApplyOperation));
		this.Initialize();
		this.CheckLoad();
	}

	// Token: 0x060019B4 RID: 6580 RVA: 0x000A9F36 File Offset: 0x000A8136
	private void OnDestroy()
	{
		TerrainComp.s_instances.Remove(this);
	}

	// Token: 0x060019B5 RID: 6581 RVA: 0x000A9F44 File Offset: 0x000A8144
	private void Update()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.CheckLoad();
	}

	// Token: 0x060019B6 RID: 6582 RVA: 0x000A9F5C File Offset: 0x000A815C
	private void Initialize()
	{
		this.m_initialized = true;
		this.m_width = this.m_hmap.m_width;
		this.m_size = (float)this.m_width * this.m_hmap.m_scale;
		int num = this.m_width + 1;
		this.m_modifiedHeight = new bool[num * num];
		this.m_levelDelta = new float[num * num];
		this.m_smoothDelta = new float[num * num];
		this.m_modifiedPaint = new bool[this.m_width * this.m_width];
		this.m_paintMask = new Color[this.m_width * this.m_width];
	}

	// Token: 0x060019B7 RID: 6583 RVA: 0x000AA000 File Offset: 0x000A8200
	private void Save()
	{
		if (!this.m_initialized)
		{
			return;
		}
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		ZPackage zpackage = new ZPackage();
		zpackage.Write(1);
		zpackage.Write(this.m_operations);
		zpackage.Write(this.m_lastOpPoint);
		zpackage.Write(this.m_lastOpRadius);
		zpackage.Write(this.m_modifiedHeight.Length);
		for (int i = 0; i < this.m_modifiedHeight.Length; i++)
		{
			zpackage.Write(this.m_modifiedHeight[i]);
			if (this.m_modifiedHeight[i])
			{
				zpackage.Write(this.m_levelDelta[i]);
				zpackage.Write(this.m_smoothDelta[i]);
			}
		}
		zpackage.Write(this.m_modifiedPaint.Length);
		for (int j = 0; j < this.m_modifiedPaint.Length; j++)
		{
			zpackage.Write(this.m_modifiedPaint[j]);
			if (this.m_modifiedPaint[j])
			{
				zpackage.Write(this.m_paintMask[j].r);
				zpackage.Write(this.m_paintMask[j].g);
				zpackage.Write(this.m_paintMask[j].b);
				zpackage.Write(this.m_paintMask[j].a);
			}
		}
		byte[] bytes = Utils.Compress(zpackage.GetArray());
		this.m_nview.GetZDO().Set(ZDOVars.s_TCData, bytes);
		this.m_lastDataRevision = this.m_nview.GetZDO().DataRevision;
	}

	// Token: 0x060019B8 RID: 6584 RVA: 0x000AA188 File Offset: 0x000A8388
	private void CheckLoad()
	{
		if (this.m_nview.GetZDO().DataRevision != this.m_lastDataRevision)
		{
			int operations = this.m_operations;
			if (this.Load())
			{
				this.m_hmap.Poke(false);
				if (ClutterSystem.instance)
				{
					if (this.m_operations == operations + 1)
					{
						ClutterSystem.instance.ResetGrass(this.m_lastOpPoint, this.m_lastOpRadius);
						return;
					}
					ClutterSystem.instance.ResetGrass(this.m_hmap.transform.position, (float)this.m_hmap.m_width * this.m_hmap.m_scale / 2f);
				}
			}
		}
	}

	// Token: 0x060019B9 RID: 6585 RVA: 0x000AA234 File Offset: 0x000A8434
	private bool Load()
	{
		this.m_lastDataRevision = this.m_nview.GetZDO().DataRevision;
		byte[] byteArray = this.m_nview.GetZDO().GetByteArray(ZDOVars.s_TCData, null);
		if (byteArray == null)
		{
			return false;
		}
		ZPackage zpackage = new ZPackage(Utils.Decompress(byteArray));
		zpackage.ReadInt();
		this.m_operations = zpackage.ReadInt();
		this.m_lastOpPoint = zpackage.ReadVector3();
		this.m_lastOpRadius = zpackage.ReadSingle();
		int num = zpackage.ReadInt();
		if (num != this.m_modifiedHeight.Length)
		{
			ZLog.LogWarning("Terrain data load error, height array missmatch");
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			this.m_modifiedHeight[i] = zpackage.ReadBool();
			if (this.m_modifiedHeight[i])
			{
				this.m_levelDelta[i] = zpackage.ReadSingle();
				this.m_smoothDelta[i] = zpackage.ReadSingle();
			}
			else
			{
				this.m_levelDelta[i] = 0f;
				this.m_smoothDelta[i] = 0f;
			}
		}
		int num2 = zpackage.ReadInt();
		for (int j = 0; j < num2; j++)
		{
			this.m_modifiedPaint[j] = zpackage.ReadBool();
			if (this.m_modifiedPaint[j])
			{
				Color color = default(Color);
				color.r = zpackage.ReadSingle();
				color.g = zpackage.ReadSingle();
				color.b = zpackage.ReadSingle();
				color.a = zpackage.ReadSingle();
				this.m_paintMask[j] = color;
			}
			else
			{
				this.m_paintMask[j] = Color.black;
			}
		}
		return true;
	}

	// Token: 0x060019BA RID: 6586 RVA: 0x000AA3C4 File Offset: 0x000A85C4
	public static TerrainComp FindTerrainCompiler(Vector3 pos)
	{
		foreach (TerrainComp terrainComp in TerrainComp.s_instances)
		{
			float num = terrainComp.m_size / 2f;
			Vector3 position = terrainComp.transform.position;
			if (pos.x >= position.x - num && pos.x <= position.x + num && pos.z >= position.z - num && pos.z <= position.z + num)
			{
				return terrainComp;
			}
		}
		return null;
	}

	// Token: 0x060019BB RID: 6587 RVA: 0x000AA474 File Offset: 0x000A8674
	public void ApplyToHeightmap(Texture2D clearedMask, List<float> heights, float[] baseHeights, float[] levelOnlyHeights, Heightmap hm)
	{
		if (!this.m_initialized)
		{
			return;
		}
		int num = this.m_width + 1;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num2 = i * num + j;
				float num3 = this.m_levelDelta[num2];
				float num4 = this.m_smoothDelta[num2];
				if (num3 != 0f || num4 != 0f)
				{
					float num5 = heights[num2];
					float num6 = baseHeights[num2];
					float value = num5 + num3 + num4;
					value = Mathf.Clamp(value, num6 - 8f, num6 + 8f);
					heights[num2] = value;
				}
			}
		}
		for (int k = 0; k < this.m_width; k++)
		{
			for (int l = 0; l < this.m_width; l++)
			{
				int num7 = k * this.m_width + l;
				if (this.m_modifiedPaint[num7])
				{
					clearedMask.SetPixel(l, k, this.m_paintMask[num7]);
				}
			}
		}
	}

	// Token: 0x060019BC RID: 6588 RVA: 0x000AA568 File Offset: 0x000A8768
	public void ApplyOperation(TerrainOp modifier)
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(modifier.transform.position);
		modifier.m_settings.Serialize(zpackage);
		this.m_nview.InvokeRPC("ApplyOperation", new object[]
		{
			zpackage
		});
	}

	// Token: 0x060019BD RID: 6589 RVA: 0x000AA5B4 File Offset: 0x000A87B4
	private void RPC_ApplyOperation(long sender, ZPackage pkg)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		TerrainOp.Settings settings = new TerrainOp.Settings();
		Vector3 pos = pkg.ReadVector3();
		settings.Deserialize(pkg);
		this.DoOperation(pos, settings);
	}

	// Token: 0x060019BE RID: 6590 RVA: 0x000AA5EC File Offset: 0x000A87EC
	private void DoOperation(Vector3 pos, TerrainOp.Settings modifier)
	{
		if (!this.m_initialized)
		{
			return;
		}
		this.InternalDoOperation(pos, modifier);
		this.Save();
		this.m_hmap.Poke(false);
		if (ClutterSystem.instance)
		{
			ClutterSystem.instance.ResetGrass(pos, modifier.GetRadius());
		}
	}

	// Token: 0x060019BF RID: 6591 RVA: 0x000AA63C File Offset: 0x000A883C
	private void InternalDoOperation(Vector3 pos, TerrainOp.Settings modifier)
	{
		if (modifier.m_level)
		{
			this.LevelTerrain(pos + Vector3.up * modifier.m_levelOffset, modifier.m_levelRadius, modifier.m_square);
		}
		if (modifier.m_raise)
		{
			this.RaiseTerrain(pos, modifier.m_raiseRadius, modifier.m_raiseDelta, modifier.m_square, modifier.m_raisePower);
		}
		if (modifier.m_smooth)
		{
			this.SmoothTerrain(pos + Vector3.up * modifier.m_levelOffset, modifier.m_smoothRadius, modifier.m_square, modifier.m_smoothPower);
		}
		if (modifier.m_paintCleared)
		{
			this.PaintCleared(pos, modifier.m_paintRadius, modifier.m_paintType, modifier.m_paintHeightCheck, false);
		}
		this.m_operations++;
		this.m_lastOpPoint = pos;
		this.m_lastOpRadius = modifier.GetRadius();
	}

	// Token: 0x060019C0 RID: 6592 RVA: 0x000AA71C File Offset: 0x000A891C
	private void LevelTerrain(Vector3 worldPos, float radius, bool square)
	{
		int num;
		int num2;
		this.m_hmap.WorldToVertex(worldPos, out num, out num2);
		Vector3 vector = worldPos - base.transform.position;
		float num3 = radius / this.m_hmap.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		int num5 = this.m_width + 1;
		Vector2 a = new Vector2((float)num, (float)num2);
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if ((square || Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3) && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float height = this.m_hmap.GetHeight(j, i);
					float num6 = vector.y - height;
					int num7 = i * num5 + j;
					num6 += this.m_smoothDelta[num7];
					this.m_smoothDelta[num7] = 0f;
					this.m_levelDelta[num7] += num6;
					this.m_levelDelta[num7] = Mathf.Clamp(this.m_levelDelta[num7], -8f, 8f);
					this.m_modifiedHeight[num7] = true;
				}
			}
		}
	}

	// Token: 0x060019C1 RID: 6593 RVA: 0x000AA86C File Offset: 0x000A8A6C
	private void RaiseTerrain(Vector3 worldPos, float radius, float delta, bool square, float power)
	{
		int num;
		int num2;
		this.m_hmap.WorldToVertex(worldPos, out num, out num2);
		Vector3 vector = worldPos - base.transform.position;
		float num3 = radius / this.m_hmap.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		int num5 = this.m_width + 1;
		Vector2 a = new Vector2((float)num, (float)num2);
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if (j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float num6 = 1f;
					if (!square)
					{
						float num7 = Vector2.Distance(a, new Vector2((float)j, (float)i));
						if (num7 > num3)
						{
							goto IL_191;
						}
						if (power > 0f)
						{
							num6 = num7 / num3;
							num6 = 1f - num6;
							if (power != 1f)
							{
								num6 = Mathf.Pow(num6, power);
							}
						}
					}
					float height = this.m_hmap.GetHeight(j, i);
					float num8 = delta * num6;
					float num9 = vector.y + num8;
					if (delta >= 0f || num9 <= height)
					{
						if (delta > 0f)
						{
							if (num9 < height)
							{
								goto IL_191;
							}
							if (num9 > height + num8)
							{
								num9 = height + num8;
							}
						}
						int num10 = i * num5 + j;
						float num11 = num9 - height + this.m_smoothDelta[num10];
						this.m_smoothDelta[num10] = 0f;
						this.m_levelDelta[num10] += num11;
						this.m_levelDelta[num10] = Mathf.Clamp(this.m_levelDelta[num10], -8f, 8f);
						this.m_modifiedHeight[num10] = true;
					}
				}
				IL_191:;
			}
		}
	}

	// Token: 0x060019C2 RID: 6594 RVA: 0x000AAA2C File Offset: 0x000A8C2C
	private void SmoothTerrain(Vector3 worldPos, float radius, bool square, float power)
	{
		int num;
		int num2;
		this.m_hmap.WorldToVertex(worldPos, out num, out num2);
		float b = worldPos.y - base.transform.position.y;
		float num3 = radius / this.m_hmap.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		int num5 = this.m_width + 1;
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				float num6 = Vector2.Distance(a, new Vector2((float)j, (float)i));
				if (num6 <= num3 && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float num7 = num6 / num3;
					if (power == 3f)
					{
						num7 = num7 * num7 * num7;
					}
					else
					{
						num7 = Mathf.Pow(num7, power);
					}
					float height = this.m_hmap.GetHeight(j, i);
					float t = 1f - num7;
					float num8 = Mathf.Lerp(height, b, t) - height;
					int num9 = i * num5 + j;
					this.m_smoothDelta[num9] += num8;
					this.m_smoothDelta[num9] = Mathf.Clamp(this.m_smoothDelta[num9], -1f, 1f);
					this.m_modifiedHeight[num9] = true;
				}
			}
		}
	}

	// Token: 0x060019C3 RID: 6595 RVA: 0x000AAB9C File Offset: 0x000A8D9C
	private void PaintCleared(Vector3 worldPos, float radius, TerrainModifier.PaintType paintType, bool heightCheck, bool apply)
	{
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		float num = worldPos.y - base.transform.position.y;
		int num2;
		int num3;
		this.m_hmap.WorldToVertex(worldPos, out num2, out num3);
		float num4 = radius / this.m_hmap.m_scale;
		int num5 = Mathf.CeilToInt(num4);
		Vector2 a = new Vector2((float)num2, (float)num3);
		for (int i = num3 - num5; i <= num3 + num5; i++)
		{
			for (int j = num2 - num5; j <= num2 + num5; j++)
			{
				float num6 = Vector2.Distance(a, new Vector2((float)j, (float)i));
				if (j >= 0 && i >= 0 && j < this.m_width && i < this.m_width && (!heightCheck || this.m_hmap.GetHeight(j, i) <= num))
				{
					float num7 = 1f - Mathf.Clamp01(num6 / num4);
					num7 = Mathf.Pow(num7, 0.1f);
					Color color = this.m_hmap.GetPaintMask(j, i);
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
					this.m_modifiedPaint[i * this.m_width + j] = true;
					this.m_paintMask[i * this.m_width + j] = color;
				}
			}
		}
	}

	// Token: 0x060019C4 RID: 6596 RVA: 0x000AAD6E File Offset: 0x000A8F6E
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x060019C5 RID: 6597 RVA: 0x000AAD8C File Offset: 0x000A8F8C
	public static void UpgradeTerrain()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		List<Heightmap> list = new List<Heightmap>();
		Heightmap.FindHeightmap(Player.m_localPlayer.transform.position, 150f, list);
		bool flag = false;
		using (List<Heightmap>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (TerrainComp.UpgradeTerrain(enumerator.Current))
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			global::Console.instance.Print("Nothing to optimize");
		}
	}

	// Token: 0x060019C6 RID: 6598 RVA: 0x000AAE20 File Offset: 0x000A9020
	public static bool UpgradeTerrain(Heightmap hmap)
	{
		List<TerrainModifier> allInstances = TerrainModifier.GetAllInstances();
		int num = 0;
		List<TerrainModifier> list = new List<TerrainModifier>();
		foreach (TerrainModifier terrainModifier in allInstances)
		{
			ZNetView component = terrainModifier.GetComponent<ZNetView>();
			if (!(component == null) && component.IsValid() && component.IsOwner() && terrainModifier.m_playerModifiction)
			{
				if (!hmap.CheckTerrainModIsContained(terrainModifier))
				{
					num++;
				}
				else
				{
					list.Add(terrainModifier);
				}
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		TerrainComp andCreateTerrainCompiler = hmap.GetAndCreateTerrainCompiler();
		if (!andCreateTerrainCompiler.IsOwner())
		{
			global::Console.instance.Print("Skipping terrain at " + hmap.transform.position.ToString() + " ( another player is currently the owner )");
			return false;
		}
		int num2 = andCreateTerrainCompiler.m_width + 1;
		float[] array = new float[andCreateTerrainCompiler.m_modifiedHeight.Length];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				array[i * num2 + j] = hmap.GetHeight(j, i);
			}
		}
		Color[] array2 = new Color[andCreateTerrainCompiler.m_paintMask.Length];
		for (int k = 0; k < andCreateTerrainCompiler.m_width; k++)
		{
			for (int l = 0; l < andCreateTerrainCompiler.m_width; l++)
			{
				array2[k * andCreateTerrainCompiler.m_width + l] = hmap.GetPaintMask(l, k);
			}
		}
		foreach (TerrainModifier terrainModifier2 in list)
		{
			terrainModifier2.enabled = false;
			terrainModifier2.GetComponent<ZNetView>().Destroy();
		}
		hmap.Poke(false);
		int num3 = 0;
		for (int m = 0; m < num2; m++)
		{
			for (int n = 0; n < num2; n++)
			{
				int num4 = m * num2 + n;
				float num5 = array[num4];
				float height = hmap.GetHeight(n, m);
				float num6 = num5 - height;
				if (Mathf.Abs(num6) >= 0.001f)
				{
					andCreateTerrainCompiler.m_modifiedHeight[num4] = true;
					andCreateTerrainCompiler.m_levelDelta[num4] += num6;
					num3++;
				}
			}
		}
		int num7 = 0;
		for (int num8 = 0; num8 < andCreateTerrainCompiler.m_width; num8++)
		{
			for (int num9 = 0; num9 < andCreateTerrainCompiler.m_width; num9++)
			{
				int num10 = num8 * andCreateTerrainCompiler.m_width + num9;
				Color color = array2[num10];
				Color paintMask = hmap.GetPaintMask(num9, num8);
				if (!(color == paintMask))
				{
					andCreateTerrainCompiler.m_modifiedPaint[num10] = true;
					andCreateTerrainCompiler.m_paintMask[num10] = color;
					num7++;
				}
			}
		}
		andCreateTerrainCompiler.Save();
		hmap.Poke(false);
		if (ClutterSystem.instance)
		{
			ClutterSystem.instance.ResetGrass(hmap.transform.position, (float)hmap.m_width * hmap.m_scale / 2f);
		}
		global::Console.instance.Print(string.Concat(new string[]
		{
			"Operations optimized:",
			list.Count.ToString(),
			"  height changes:",
			num3.ToString(),
			"  paint changes:",
			num7.ToString()
		}));
		return true;
	}

	// Token: 0x04001B7D RID: 7037
	private const int terrainCompVersion = 1;

	// Token: 0x04001B7E RID: 7038
	private static readonly List<TerrainComp> s_instances = new List<TerrainComp>();

	// Token: 0x04001B7F RID: 7039
	private bool m_initialized;

	// Token: 0x04001B80 RID: 7040
	private int m_width;

	// Token: 0x04001B81 RID: 7041
	private float m_size;

	// Token: 0x04001B82 RID: 7042
	private int m_operations;

	// Token: 0x04001B83 RID: 7043
	private bool[] m_modifiedHeight;

	// Token: 0x04001B84 RID: 7044
	private float[] m_levelDelta;

	// Token: 0x04001B85 RID: 7045
	private float[] m_smoothDelta;

	// Token: 0x04001B86 RID: 7046
	private bool[] m_modifiedPaint;

	// Token: 0x04001B87 RID: 7047
	private Color[] m_paintMask;

	// Token: 0x04001B88 RID: 7048
	private Heightmap m_hmap;

	// Token: 0x04001B89 RID: 7049
	private ZNetView m_nview;

	// Token: 0x04001B8A RID: 7050
	private uint m_lastDataRevision = uint.MaxValue;

	// Token: 0x04001B8B RID: 7051
	private Vector3 m_lastOpPoint;

	// Token: 0x04001B8C RID: 7052
	private float m_lastOpRadius;
}
