using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

// Token: 0x02000241 RID: 577
public class HeightmapBuilder
{
	// Token: 0x170000EA RID: 234
	// (get) Token: 0x060016D9 RID: 5849 RVA: 0x00096C52 File Offset: 0x00094E52
	public static HeightmapBuilder instance
	{
		get
		{
			if (HeightmapBuilder.m_instance == null)
			{
				HeightmapBuilder.m_instance = new HeightmapBuilder();
			}
			return HeightmapBuilder.m_instance;
		}
	}

	// Token: 0x060016DA RID: 5850 RVA: 0x00096C6C File Offset: 0x00094E6C
	public HeightmapBuilder()
	{
		HeightmapBuilder.m_instance = this;
		this.m_builder = new Thread(new ThreadStart(this.BuildThread));
		this.m_builder.Start();
	}

	// Token: 0x060016DB RID: 5851 RVA: 0x00096CC8 File Offset: 0x00094EC8
	public void Dispose()
	{
		if (this.m_builder != null)
		{
			ZLog.Log("Stoping build thread");
			this.m_lock.WaitOne();
			this.m_stop = true;
			this.m_builder.Abort();
			this.m_lock.ReleaseMutex();
			this.m_builder = null;
		}
		if (this.m_lock != null)
		{
			this.m_lock.Close();
			this.m_lock = null;
		}
	}

	// Token: 0x060016DC RID: 5852 RVA: 0x00096D34 File Offset: 0x00094F34
	private void BuildThread()
	{
		ZLog.Log("Builder started");
		while (!this.m_stop)
		{
			this.m_lock.WaitOne();
			bool flag = this.m_toBuild.Count > 0;
			this.m_lock.ReleaseMutex();
			if (flag)
			{
				this.m_lock.WaitOne();
				HeightmapBuilder.HMBuildData hmbuildData = this.m_toBuild[0];
				this.m_lock.ReleaseMutex();
				new Stopwatch().Start();
				this.Build(hmbuildData);
				this.m_lock.WaitOne();
				this.m_toBuild.Remove(hmbuildData);
				this.m_ready.Add(hmbuildData);
				while (this.m_ready.Count > 16)
				{
					this.m_ready.RemoveAt(0);
				}
				this.m_lock.ReleaseMutex();
			}
			Thread.Sleep(10);
		}
	}

	// Token: 0x060016DD RID: 5853 RVA: 0x00096E10 File Offset: 0x00095010
	private void Build(HeightmapBuilder.HMBuildData data)
	{
		int num = data.m_width + 1;
		int num2 = num * num;
		Vector3 vector = data.m_center + new Vector3((float)data.m_width * data.m_scale * -0.5f, 0f, (float)data.m_width * data.m_scale * -0.5f);
		WorldGenerator worldGen = data.m_worldGen;
		data.m_cornerBiomes = new Heightmap.Biome[4];
		data.m_cornerBiomes[0] = worldGen.GetBiome(vector.x, vector.z);
		data.m_cornerBiomes[1] = worldGen.GetBiome(vector.x + (float)data.m_width * data.m_scale, vector.z);
		data.m_cornerBiomes[2] = worldGen.GetBiome(vector.x, vector.z + (float)data.m_width * data.m_scale);
		data.m_cornerBiomes[3] = worldGen.GetBiome(vector.x + (float)data.m_width * data.m_scale, vector.z + (float)data.m_width * data.m_scale);
		Heightmap.Biome biome = data.m_cornerBiomes[0];
		Heightmap.Biome biome2 = data.m_cornerBiomes[1];
		Heightmap.Biome biome3 = data.m_cornerBiomes[2];
		Heightmap.Biome biome4 = data.m_cornerBiomes[3];
		data.m_baseHeights = new List<float>(num * num);
		for (int i = 0; i < num2; i++)
		{
			data.m_baseHeights.Add(0f);
		}
		int num3 = data.m_width * data.m_width;
		data.m_baseMask = new Color[num3];
		for (int j = 0; j < num3; j++)
		{
			data.m_baseMask[j] = new Color(0f, 0f, 0f, 0f);
		}
		for (int k = 0; k < num; k++)
		{
			float wy = vector.z + (float)k * data.m_scale;
			float t = Mathf.SmoothStep(0f, 1f, (float)k / (float)data.m_width);
			for (int l = 0; l < num; l++)
			{
				float wx = vector.x + (float)l * data.m_scale;
				float t2 = Mathf.SmoothStep(0f, 1f, (float)l / (float)data.m_width);
				Color color = Color.black;
				float value;
				if (data.m_distantLod)
				{
					Heightmap.Biome biome5 = worldGen.GetBiome(wx, wy);
					value = worldGen.GetBiomeHeight(biome5, wx, wy, out color, false);
				}
				else if (biome3 == biome && biome2 == biome && biome4 == biome)
				{
					value = worldGen.GetBiomeHeight(biome, wx, wy, out color, false);
				}
				else
				{
					Color[] array = new Color[4];
					float biomeHeight = worldGen.GetBiomeHeight(biome, wx, wy, out array[0], false);
					float biomeHeight2 = worldGen.GetBiomeHeight(biome2, wx, wy, out array[1], false);
					float biomeHeight3 = worldGen.GetBiomeHeight(biome3, wx, wy, out array[2], false);
					float biomeHeight4 = worldGen.GetBiomeHeight(biome4, wx, wy, out array[3], false);
					float a = Mathf.Lerp(biomeHeight, biomeHeight2, t2);
					float b = Mathf.Lerp(biomeHeight3, biomeHeight4, t2);
					value = Mathf.Lerp(a, b, t);
					Color a2 = Color.Lerp(array[0], array[1], t2);
					Color b2 = Color.Lerp(array[2], array[3], t2);
					color = Color.Lerp(a2, b2, t);
				}
				data.m_baseHeights[k * num + l] = value;
				if (l < data.m_width && k < data.m_width)
				{
					data.m_baseMask[k * data.m_width + l] = color;
				}
			}
		}
		if (data.m_distantLod)
		{
			for (int m = 0; m < 4; m++)
			{
				List<float> list = new List<float>(data.m_baseHeights);
				for (int n = 1; n < num - 1; n++)
				{
					for (int num4 = 1; num4 < num - 1; num4++)
					{
						float num5 = list[n * num + num4];
						float num6 = list[(n - 1) * num + num4];
						float num7 = list[(n + 1) * num + num4];
						float num8 = list[n * num + num4 - 1];
						float num9 = list[n * num + num4 + 1];
						if (Mathf.Abs(num5 - num6) > 10f)
						{
							num5 = (num5 + num6) * 0.5f;
						}
						if (Mathf.Abs(num5 - num7) > 10f)
						{
							num5 = (num5 + num7) * 0.5f;
						}
						if (Mathf.Abs(num5 - num8) > 10f)
						{
							num5 = (num5 + num8) * 0.5f;
						}
						if (Mathf.Abs(num5 - num9) > 10f)
						{
							num5 = (num5 + num9) * 0.5f;
						}
						data.m_baseHeights[n * num + num4] = num5;
					}
				}
			}
		}
	}

	// Token: 0x060016DE RID: 5854 RVA: 0x000972F4 File Offset: 0x000954F4
	public HeightmapBuilder.HMBuildData RequestTerrainSync(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		HeightmapBuilder.HMBuildData hmbuildData;
		do
		{
			hmbuildData = this.RequestTerrain(center, width, scale, distantLod, worldGen);
		}
		while (hmbuildData == null);
		return hmbuildData;
	}

	// Token: 0x060016DF RID: 5855 RVA: 0x00097314 File Offset: 0x00095514
	public HeightmapBuilder.HMBuildData RequestTerrain(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		this.m_lock.WaitOne();
		for (int i = 0; i < this.m_ready.Count; i++)
		{
			HeightmapBuilder.HMBuildData hmbuildData = this.m_ready[i];
			if (hmbuildData.IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_ready.RemoveAt(i);
				this.m_lock.ReleaseMutex();
				return hmbuildData;
			}
		}
		for (int j = 0; j < this.m_toBuild.Count; j++)
		{
			if (this.m_toBuild[j].IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_lock.ReleaseMutex();
				return null;
			}
		}
		this.m_toBuild.Add(new HeightmapBuilder.HMBuildData(center, width, scale, distantLod, worldGen));
		this.m_lock.ReleaseMutex();
		return null;
	}

	// Token: 0x060016E0 RID: 5856 RVA: 0x000973D8 File Offset: 0x000955D8
	public bool IsTerrainReady(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		this.m_lock.WaitOne();
		for (int i = 0; i < this.m_ready.Count; i++)
		{
			if (this.m_ready[i].IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_lock.ReleaseMutex();
				return true;
			}
		}
		for (int j = 0; j < this.m_toBuild.Count; j++)
		{
			if (this.m_toBuild[j].IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_lock.ReleaseMutex();
				return false;
			}
		}
		this.m_toBuild.Add(new HeightmapBuilder.HMBuildData(center, width, scale, distantLod, worldGen));
		this.m_lock.ReleaseMutex();
		return false;
	}

	// Token: 0x04001800 RID: 6144
	private static HeightmapBuilder m_instance;

	// Token: 0x04001801 RID: 6145
	private const int m_maxReadyQueue = 16;

	// Token: 0x04001802 RID: 6146
	private List<HeightmapBuilder.HMBuildData> m_toBuild = new List<HeightmapBuilder.HMBuildData>();

	// Token: 0x04001803 RID: 6147
	private List<HeightmapBuilder.HMBuildData> m_ready = new List<HeightmapBuilder.HMBuildData>();

	// Token: 0x04001804 RID: 6148
	private Thread m_builder;

	// Token: 0x04001805 RID: 6149
	private Mutex m_lock = new Mutex();

	// Token: 0x04001806 RID: 6150
	private bool m_stop;

	// Token: 0x02000242 RID: 578
	public class HMBuildData
	{
		// Token: 0x060016E2 RID: 5858 RVA: 0x0009748E File Offset: 0x0009568E
		public HMBuildData(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
		{
			this.m_center = center;
			this.m_width = width;
			this.m_scale = scale;
			this.m_distantLod = distantLod;
			this.m_worldGen = worldGen;
		}

		// Token: 0x060016E3 RID: 5859 RVA: 0x000974BB File Offset: 0x000956BB
		public bool IsEqual(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
		{
			return this.m_center == center && this.m_width == width && this.m_scale == scale && this.m_distantLod == distantLod && this.m_worldGen == worldGen;
		}

		// Token: 0x04001807 RID: 6151
		public Vector3 m_center;

		// Token: 0x04001808 RID: 6152
		public int m_width;

		// Token: 0x04001809 RID: 6153
		public float m_scale;

		// Token: 0x0400180A RID: 6154
		public bool m_distantLod;

		// Token: 0x0400180B RID: 6155
		public bool m_menu;

		// Token: 0x0400180C RID: 6156
		public WorldGenerator m_worldGen;

		// Token: 0x0400180D RID: 6157
		public Heightmap.Biome[] m_cornerBiomes;

		// Token: 0x0400180E RID: 6158
		public List<float> m_baseHeights;

		// Token: 0x0400180F RID: 6159
		public Color[] m_baseMask;
	}
}
