using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Token: 0x020002C8 RID: 712
public class WorldGenerator
{
	// Token: 0x06001AE4 RID: 6884 RVA: 0x000B2F77 File Offset: 0x000B1177
	public static void Initialize(World world)
	{
		WorldGenerator.m_instance = new WorldGenerator(world);
	}

	// Token: 0x06001AE5 RID: 6885 RVA: 0x000B2F84 File Offset: 0x000B1184
	public static void Deitialize()
	{
		WorldGenerator.m_instance = null;
	}

	// Token: 0x170000FA RID: 250
	// (get) Token: 0x06001AE6 RID: 6886 RVA: 0x000B2F8C File Offset: 0x000B118C
	public static WorldGenerator instance
	{
		get
		{
			return WorldGenerator.m_instance;
		}
	}

	// Token: 0x06001AE7 RID: 6887 RVA: 0x000B2F94 File Offset: 0x000B1194
	private WorldGenerator(World world)
	{
		this.m_world = world;
		this.m_version = this.m_world.m_worldGenVersion;
		this.VersionSetup(this.m_version);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.m_world.m_seed);
		this.m_offset0 = (float)UnityEngine.Random.Range(-10000, 10000);
		this.m_offset1 = (float)UnityEngine.Random.Range(-10000, 10000);
		this.m_offset2 = (float)UnityEngine.Random.Range(-10000, 10000);
		this.m_offset3 = (float)UnityEngine.Random.Range(-10000, 10000);
		this.m_riverSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		this.m_streamSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		this.m_offset4 = (float)UnityEngine.Random.Range(-10000, 10000);
		if (!this.m_world.m_menu)
		{
			this.Pregenerate();
		}
		UnityEngine.Random.state = state;
	}

	// Token: 0x06001AE8 RID: 6888 RVA: 0x000B3100 File Offset: 0x000B1300
	private void VersionSetup(int version)
	{
		ZLog.Log("Worldgenerator version setup:" + version.ToString());
		if (version <= 0)
		{
			this.m_minMountainDistance = 1500f;
		}
		if (version <= 1)
		{
			this.minDarklandNoise = 0.5f;
			this.maxMarshDistance = 8000f;
		}
	}

	// Token: 0x06001AE9 RID: 6889 RVA: 0x000B314C File Offset: 0x000B134C
	private void Pregenerate()
	{
		this.FindLakes();
		this.m_rivers = this.PlaceRivers();
		this.m_streams = this.PlaceStreams();
	}

	// Token: 0x06001AEA RID: 6890 RVA: 0x000B316C File Offset: 0x000B136C
	public List<Vector2> GetLakes()
	{
		return this.m_lakes;
	}

	// Token: 0x06001AEB RID: 6891 RVA: 0x000B3174 File Offset: 0x000B1374
	public List<WorldGenerator.River> GetRivers()
	{
		return this.m_rivers;
	}

	// Token: 0x06001AEC RID: 6892 RVA: 0x000B317C File Offset: 0x000B137C
	public List<WorldGenerator.River> GetStreams()
	{
		return this.m_streams;
	}

	// Token: 0x06001AED RID: 6893 RVA: 0x000B3184 File Offset: 0x000B1384
	private void FindLakes()
	{
		DateTime now = DateTime.Now;
		List<Vector2> list = new List<Vector2>();
		for (float num = -10000f; num <= 10000f; num += 128f)
		{
			for (float num2 = -10000f; num2 <= 10000f; num2 += 128f)
			{
				if (new Vector2(num2, num).magnitude <= 10000f && this.GetBaseHeight(num2, num, false) < 0.05f)
				{
					list.Add(new Vector2(num2, num));
				}
			}
		}
		this.m_lakes = this.MergePoints(list, 800f);
		DateTime.Now - now;
	}

	// Token: 0x06001AEE RID: 6894 RVA: 0x000B3220 File Offset: 0x000B1420
	private List<Vector2> MergePoints(List<Vector2> points, float range)
	{
		List<Vector2> list = new List<Vector2>();
		while (points.Count > 0)
		{
			Vector2 vector = points[0];
			points.RemoveAt(0);
			while (points.Count > 0)
			{
				int num = this.FindClosest(points, vector, range);
				if (num == -1)
				{
					break;
				}
				vector = (vector + points[num]) * 0.5f;
				points[num] = points[points.Count - 1];
				points.RemoveAt(points.Count - 1);
			}
			list.Add(vector);
		}
		return list;
	}

	// Token: 0x06001AEF RID: 6895 RVA: 0x000B32AC File Offset: 0x000B14AC
	private int FindClosest(List<Vector2> points, Vector2 p, float maxDistance)
	{
		int result = -1;
		float num = 99999f;
		for (int i = 0; i < points.Count; i++)
		{
			if (!(points[i] == p))
			{
				float num2 = Vector2.Distance(p, points[i]);
				if (num2 < maxDistance && num2 < num)
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	// Token: 0x06001AF0 RID: 6896 RVA: 0x000B32FC File Offset: 0x000B14FC
	private List<WorldGenerator.River> PlaceStreams()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.m_streamSeed);
		List<WorldGenerator.River> list = new List<WorldGenerator.River>();
		int num = 0;
		DateTime now = DateTime.Now;
		for (int i = 0; i < 3000; i++)
		{
			Vector2 vector;
			float num2;
			Vector2 vector2;
			if (this.FindStreamStartPoint(100, 26f, 31f, out vector, out num2) && this.FindStreamEndPoint(100, 36f, 44f, vector, 80f, 200f, out vector2))
			{
				Vector2 vector3 = (vector + vector2) * 0.5f;
				float pregenerationHeight = this.GetPregenerationHeight(vector3.x, vector3.y);
				if (pregenerationHeight >= 26f && pregenerationHeight <= 44f)
				{
					WorldGenerator.River river = new WorldGenerator.River();
					river.p0 = vector;
					river.p1 = vector2;
					river.center = vector3;
					river.widthMax = 20f;
					river.widthMin = 20f;
					float num3 = Vector2.Distance(river.p0, river.p1);
					river.curveWidth = num3 / 15f;
					river.curveWavelength = num3 / 20f;
					list.Add(river);
					num++;
				}
			}
		}
		this.RenderRivers(list);
		UnityEngine.Random.state = state;
		DateTime.Now - now;
		return list;
	}

	// Token: 0x06001AF1 RID: 6897 RVA: 0x000B3458 File Offset: 0x000B1658
	private bool FindStreamEndPoint(int iterations, float minHeight, float maxHeight, Vector2 start, float minLength, float maxLength, out Vector2 end)
	{
		float num = (maxLength - minLength) / (float)iterations;
		float num2 = maxLength;
		for (int i = 0; i < iterations; i++)
		{
			num2 -= num;
			float f = UnityEngine.Random.Range(0f, 6.2831855f);
			Vector2 vector = start + new Vector2(Mathf.Sin(f), Mathf.Cos(f)) * num2;
			float pregenerationHeight = this.GetPregenerationHeight(vector.x, vector.y);
			if (pregenerationHeight > minHeight && pregenerationHeight < maxHeight)
			{
				end = vector;
				return true;
			}
		}
		end = Vector2.zero;
		return false;
	}

	// Token: 0x06001AF2 RID: 6898 RVA: 0x000B34EC File Offset: 0x000B16EC
	private bool FindStreamStartPoint(int iterations, float minHeight, float maxHeight, out Vector2 p, out float starth)
	{
		for (int i = 0; i < iterations; i++)
		{
			float num = UnityEngine.Random.Range(-10000f, 10000f);
			float num2 = UnityEngine.Random.Range(-10000f, 10000f);
			float pregenerationHeight = this.GetPregenerationHeight(num, num2);
			if (pregenerationHeight > minHeight && pregenerationHeight < maxHeight)
			{
				p = new Vector2(num, num2);
				starth = pregenerationHeight;
				return true;
			}
		}
		p = Vector2.zero;
		starth = 0f;
		return false;
	}

	// Token: 0x06001AF3 RID: 6899 RVA: 0x000B3560 File Offset: 0x000B1760
	private List<WorldGenerator.River> PlaceRivers()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.m_riverSeed);
		DateTime now = DateTime.Now;
		List<WorldGenerator.River> list = new List<WorldGenerator.River>();
		List<Vector2> list2 = new List<Vector2>(this.m_lakes);
		while (list2.Count > 1)
		{
			Vector2 vector = list2[0];
			int num = this.FindRandomRiverEnd(list, this.m_lakes, vector, 2000f, 0.4f, 128f);
			if (num == -1 && !this.HaveRiver(list, vector))
			{
				num = this.FindRandomRiverEnd(list, this.m_lakes, vector, 5000f, 0.4f, 128f);
			}
			if (num != -1)
			{
				WorldGenerator.River river = new WorldGenerator.River();
				river.p0 = vector;
				river.p1 = this.m_lakes[num];
				river.center = (river.p0 + river.p1) * 0.5f;
				river.widthMax = UnityEngine.Random.Range(60f, 100f);
				river.widthMin = UnityEngine.Random.Range(60f, river.widthMax);
				float num2 = Vector2.Distance(river.p0, river.p1);
				river.curveWidth = num2 / 15f;
				river.curveWavelength = num2 / 20f;
				list.Add(river);
			}
			else
			{
				list2.RemoveAt(0);
			}
		}
		this.RenderRivers(list);
		DateTime.Now - now;
		UnityEngine.Random.state = state;
		return list;
	}

	// Token: 0x06001AF4 RID: 6900 RVA: 0x000B36DC File Offset: 0x000B18DC
	private int FindClosestRiverEnd(List<WorldGenerator.River> rivers, List<Vector2> points, Vector2 p, float maxDistance, float heightLimit, float checkStep)
	{
		int result = -1;
		float num = 99999f;
		for (int i = 0; i < points.Count; i++)
		{
			if (!(points[i] == p))
			{
				float num2 = Vector2.Distance(p, points[i]);
				if (num2 < maxDistance && num2 < num && !this.HaveRiver(rivers, p, points[i]) && this.IsRiverAllowed(p, points[i], checkStep, heightLimit))
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	// Token: 0x06001AF5 RID: 6901 RVA: 0x000B3754 File Offset: 0x000B1954
	private int FindRandomRiverEnd(List<WorldGenerator.River> rivers, List<Vector2> points, Vector2 p, float maxDistance, float heightLimit, float checkStep)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < points.Count; i++)
		{
			if (!(points[i] == p) && Vector2.Distance(p, points[i]) < maxDistance && !this.HaveRiver(rivers, p, points[i]) && this.IsRiverAllowed(p, points[i], checkStep, heightLimit))
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return -1;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x06001AF6 RID: 6902 RVA: 0x000B37E0 File Offset: 0x000B19E0
	private bool HaveRiver(List<WorldGenerator.River> rivers, Vector2 p0)
	{
		foreach (WorldGenerator.River river in rivers)
		{
			if (river.p0 == p0 || river.p1 == p0)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001AF7 RID: 6903 RVA: 0x000B384C File Offset: 0x000B1A4C
	private bool HaveRiver(List<WorldGenerator.River> rivers, Vector2 p0, Vector2 p1)
	{
		foreach (WorldGenerator.River river in rivers)
		{
			if ((river.p0 == p0 && river.p1 == p1) || (river.p0 == p1 && river.p1 == p0))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001AF8 RID: 6904 RVA: 0x000B38D4 File Offset: 0x000B1AD4
	private bool IsRiverAllowed(Vector2 p0, Vector2 p1, float step, float heightLimit)
	{
		float num = Vector2.Distance(p0, p1);
		Vector2 normalized = (p1 - p0).normalized;
		bool flag = true;
		for (float num2 = step; num2 <= num - step; num2 += step)
		{
			Vector2 vector = p0 + normalized * num2;
			float baseHeight = this.GetBaseHeight(vector.x, vector.y, false);
			if (baseHeight > heightLimit)
			{
				return false;
			}
			if (baseHeight > 0.05f)
			{
				flag = false;
			}
		}
		return !flag;
	}

	// Token: 0x06001AF9 RID: 6905 RVA: 0x000B3950 File Offset: 0x000B1B50
	private void RenderRivers(List<WorldGenerator.River> rivers)
	{
		DateTime now = DateTime.Now;
		Dictionary<Vector2i, List<WorldGenerator.RiverPoint>> dictionary = new Dictionary<Vector2i, List<WorldGenerator.RiverPoint>>();
		foreach (WorldGenerator.River river in rivers)
		{
			float num = river.widthMin / 8f;
			Vector2 normalized = (river.p1 - river.p0).normalized;
			Vector2 a = new Vector2(-normalized.y, normalized.x);
			float num2 = Vector2.Distance(river.p0, river.p1);
			for (float num3 = 0f; num3 <= num2; num3 += num)
			{
				float num4 = num3 / river.curveWavelength;
				float d = Mathf.Sin(num4) * Mathf.Sin(num4 * 0.63412f) * Mathf.Sin(num4 * 0.33412f) * river.curveWidth;
				float r = UnityEngine.Random.Range(river.widthMin, river.widthMax);
				Vector2 p = river.p0 + normalized * num3 + a * d;
				this.AddRiverPoint(dictionary, p, r, river);
			}
		}
		foreach (KeyValuePair<Vector2i, List<WorldGenerator.RiverPoint>> keyValuePair in dictionary)
		{
			WorldGenerator.RiverPoint[] collection;
			if (this.m_riverPoints.TryGetValue(keyValuePair.Key, out collection))
			{
				List<WorldGenerator.RiverPoint> list = new List<WorldGenerator.RiverPoint>(collection);
				list.AddRange(keyValuePair.Value);
				this.m_riverPoints[keyValuePair.Key] = list.ToArray();
			}
			else
			{
				WorldGenerator.RiverPoint[] value = keyValuePair.Value.ToArray();
				this.m_riverPoints.Add(keyValuePair.Key, value);
			}
		}
		DateTime.Now - now;
	}

	// Token: 0x06001AFA RID: 6906 RVA: 0x000B3B58 File Offset: 0x000B1D58
	private void AddRiverPoint(Dictionary<Vector2i, List<WorldGenerator.RiverPoint>> riverPoints, Vector2 p, float r, WorldGenerator.River river)
	{
		Vector2i riverGrid = this.GetRiverGrid(p.x, p.y);
		int num = Mathf.CeilToInt(r / 64f);
		for (int i = riverGrid.y - num; i <= riverGrid.y + num; i++)
		{
			for (int j = riverGrid.x - num; j <= riverGrid.x + num; j++)
			{
				Vector2i grid = new Vector2i(j, i);
				if (this.InsideRiverGrid(grid, p, r))
				{
					this.AddRiverPoint(riverPoints, grid, p, r, river);
				}
			}
		}
	}

	// Token: 0x06001AFB RID: 6907 RVA: 0x000B3BDC File Offset: 0x000B1DDC
	private void AddRiverPoint(Dictionary<Vector2i, List<WorldGenerator.RiverPoint>> riverPoints, Vector2i grid, Vector2 p, float r, WorldGenerator.River river)
	{
		List<WorldGenerator.RiverPoint> list;
		if (riverPoints.TryGetValue(grid, out list))
		{
			list.Add(new WorldGenerator.RiverPoint(p, r));
			return;
		}
		list = new List<WorldGenerator.RiverPoint>();
		list.Add(new WorldGenerator.RiverPoint(p, r));
		riverPoints.Add(grid, list);
	}

	// Token: 0x06001AFC RID: 6908 RVA: 0x000B3C20 File Offset: 0x000B1E20
	public bool InsideRiverGrid(Vector2i grid, Vector2 p, float r)
	{
		Vector2 b = new Vector2((float)grid.x * 64f, (float)grid.y * 64f);
		Vector2 vector = p - b;
		return Mathf.Abs(vector.x) < r + 32f && Mathf.Abs(vector.y) < r + 32f;
	}

	// Token: 0x06001AFD RID: 6909 RVA: 0x000B3C80 File Offset: 0x000B1E80
	public Vector2i GetRiverGrid(float wx, float wy)
	{
		int x = Mathf.FloorToInt((wx + 32f) / 64f);
		int y = Mathf.FloorToInt((wy + 32f) / 64f);
		return new Vector2i(x, y);
	}

	// Token: 0x06001AFE RID: 6910 RVA: 0x000B3CB8 File Offset: 0x000B1EB8
	private void GetRiverWeight(float wx, float wy, out float weight, out float width)
	{
		Vector2i riverGrid = this.GetRiverGrid(wx, wy);
		this.m_riverCacheLock.EnterReadLock();
		if (riverGrid == this.m_cachedRiverGrid)
		{
			if (this.m_cachedRiverPoints != null)
			{
				this.GetWeight(this.m_cachedRiverPoints, wx, wy, out weight, out width);
				this.m_riverCacheLock.ExitReadLock();
				return;
			}
			weight = 0f;
			width = 0f;
			this.m_riverCacheLock.ExitReadLock();
			return;
		}
		else
		{
			this.m_riverCacheLock.ExitReadLock();
			WorldGenerator.RiverPoint[] array;
			if (this.m_riverPoints.TryGetValue(riverGrid, out array))
			{
				this.GetWeight(array, wx, wy, out weight, out width);
				this.m_riverCacheLock.EnterWriteLock();
				this.m_cachedRiverGrid = riverGrid;
				this.m_cachedRiverPoints = array;
				this.m_riverCacheLock.ExitWriteLock();
				return;
			}
			this.m_riverCacheLock.EnterWriteLock();
			this.m_cachedRiverGrid = riverGrid;
			this.m_cachedRiverPoints = null;
			this.m_riverCacheLock.ExitWriteLock();
			weight = 0f;
			width = 0f;
			return;
		}
	}

	// Token: 0x06001AFF RID: 6911 RVA: 0x000B3DA8 File Offset: 0x000B1FA8
	private void GetWeight(WorldGenerator.RiverPoint[] points, float wx, float wy, out float weight, out float width)
	{
		Vector2 b = new Vector2(wx, wy);
		weight = 0f;
		width = 0f;
		float num = 0f;
		float num2 = 0f;
		foreach (WorldGenerator.RiverPoint riverPoint in points)
		{
			float num3 = Vector2.SqrMagnitude(riverPoint.p - b);
			if (num3 < riverPoint.w2)
			{
				float num4 = Mathf.Sqrt(num3);
				float num5 = 1f - num4 / riverPoint.w;
				if (num5 > weight)
				{
					weight = num5;
				}
				num += riverPoint.w * num5;
				num2 += num5;
			}
		}
		if (num2 > 0f)
		{
			width = num / num2;
		}
	}

	// Token: 0x06001B00 RID: 6912 RVA: 0x000B3E58 File Offset: 0x000B2058
	private void GenerateBiomes()
	{
		this.m_biomes = new List<Heightmap.Biome>();
		int num = 400000000;
		for (int i = 0; i < num; i++)
		{
			this.m_biomes[i] = Heightmap.Biome.Meadows;
		}
	}

	// Token: 0x06001B01 RID: 6913 RVA: 0x000B3E90 File Offset: 0x000B2090
	public Heightmap.BiomeArea GetBiomeArea(Vector3 point)
	{
		Heightmap.Biome biome = this.GetBiome(point);
		Heightmap.Biome biome2 = this.GetBiome(point - new Vector3(-64f, 0f, -64f));
		Heightmap.Biome biome3 = this.GetBiome(point - new Vector3(64f, 0f, -64f));
		Heightmap.Biome biome4 = this.GetBiome(point - new Vector3(64f, 0f, 64f));
		Heightmap.Biome biome5 = this.GetBiome(point - new Vector3(-64f, 0f, 64f));
		Heightmap.Biome biome6 = this.GetBiome(point - new Vector3(-64f, 0f, 0f));
		Heightmap.Biome biome7 = this.GetBiome(point - new Vector3(64f, 0f, 0f));
		Heightmap.Biome biome8 = this.GetBiome(point - new Vector3(0f, 0f, -64f));
		Heightmap.Biome biome9 = this.GetBiome(point - new Vector3(0f, 0f, 64f));
		if (biome == biome2 && biome == biome3 && biome == biome4 && biome == biome5 && biome == biome6 && biome == biome7 && biome == biome8 && biome == biome9)
		{
			return Heightmap.BiomeArea.Median;
		}
		return Heightmap.BiomeArea.Edge;
	}

	// Token: 0x06001B02 RID: 6914 RVA: 0x000B3FDA File Offset: 0x000B21DA
	public Heightmap.Biome GetBiome(Vector3 point)
	{
		return this.GetBiome(point.x, point.z);
	}

	// Token: 0x06001B03 RID: 6915 RVA: 0x000B3FF0 File Offset: 0x000B21F0
	public Heightmap.Biome GetBiome(float wx, float wy)
	{
		if (this.m_world.m_menu)
		{
			if (this.GetBaseHeight(wx, wy, true) >= 0.4f)
			{
				return Heightmap.Biome.Mountain;
			}
			return Heightmap.Biome.BlackForest;
		}
		else
		{
			float magnitude = new Vector2(wx, wy).magnitude;
			float baseHeight = this.GetBaseHeight(wx, wy, false);
			float num = this.WorldAngle(wx, wy) * 100f;
			if (new Vector2(wx, wy + -4000f).magnitude > 12000f + num)
			{
				return Heightmap.Biome.AshLands;
			}
			if ((double)baseHeight <= 0.02)
			{
				return Heightmap.Biome.Ocean;
			}
			if (new Vector2(wx, wy + 4000f).magnitude > 12000f + num)
			{
				if (baseHeight > 0.4f)
				{
					return Heightmap.Biome.Mountain;
				}
				return Heightmap.Biome.DeepNorth;
			}
			else
			{
				if (baseHeight > 0.4f)
				{
					return Heightmap.Biome.Mountain;
				}
				if (Mathf.PerlinNoise((this.m_offset0 + wx) * 0.001f, (this.m_offset0 + wy) * 0.001f) > 0.6f && magnitude > 2000f && magnitude < this.maxMarshDistance && baseHeight > 0.05f && baseHeight < 0.25f)
				{
					return Heightmap.Biome.Swamp;
				}
				if (Mathf.PerlinNoise((this.m_offset4 + wx) * 0.001f, (this.m_offset4 + wy) * 0.001f) > this.minDarklandNoise && magnitude > 6000f + num && magnitude < 10000f)
				{
					return Heightmap.Biome.Mistlands;
				}
				if (Mathf.PerlinNoise((this.m_offset1 + wx) * 0.001f, (this.m_offset1 + wy) * 0.001f) > 0.4f && magnitude > 3000f + num && magnitude < 8000f)
				{
					return Heightmap.Biome.Plains;
				}
				if (Mathf.PerlinNoise((this.m_offset2 + wx) * 0.001f, (this.m_offset2 + wy) * 0.001f) > 0.4f && magnitude > 600f + num && magnitude < 6000f)
				{
					return Heightmap.Biome.BlackForest;
				}
				if (magnitude > 5000f + num)
				{
					return Heightmap.Biome.BlackForest;
				}
				return Heightmap.Biome.Meadows;
			}
		}
	}

	// Token: 0x06001B04 RID: 6916 RVA: 0x000B41C2 File Offset: 0x000B23C2
	private float WorldAngle(float wx, float wy)
	{
		return Mathf.Sin(Mathf.Atan2(wx, wy) * 20f);
	}

	// Token: 0x06001B05 RID: 6917 RVA: 0x000B41D8 File Offset: 0x000B23D8
	private float GetBaseHeight(float wx, float wy, bool menuTerrain)
	{
		if (menuTerrain)
		{
			wx += 100000f + this.m_offset0;
			wy += 100000f + this.m_offset1;
			float num = 0f;
			num += Mathf.PerlinNoise(wx * 0.002f * 0.5f, wy * 0.002f * 0.5f) * Mathf.PerlinNoise(wx * 0.003f * 0.5f, wy * 0.003f * 0.5f) * 1f;
			num += Mathf.PerlinNoise(wx * 0.002f * 1f, wy * 0.002f * 1f) * Mathf.PerlinNoise(wx * 0.003f * 1f, wy * 0.003f * 1f) * num * 0.9f;
			num += Mathf.PerlinNoise(wx * 0.005f * 1f, wy * 0.005f * 1f) * Mathf.PerlinNoise(wx * 0.01f * 1f, wy * 0.01f * 1f) * 0.5f * num;
			return num - 0.07f;
		}
		float num2 = Utils.Length(wx, wy);
		wx += 100000f + this.m_offset0;
		wy += 100000f + this.m_offset1;
		float num3 = 0f;
		num3 += Mathf.PerlinNoise(wx * 0.002f * 0.5f, wy * 0.002f * 0.5f) * Mathf.PerlinNoise(wx * 0.003f * 0.5f, wy * 0.003f * 0.5f) * 1f;
		num3 += Mathf.PerlinNoise(wx * 0.002f * 1f, wy * 0.002f * 1f) * Mathf.PerlinNoise(wx * 0.003f * 1f, wy * 0.003f * 1f) * num3 * 0.9f;
		num3 += Mathf.PerlinNoise(wx * 0.005f * 1f, wy * 0.005f * 1f) * Mathf.PerlinNoise(wx * 0.01f * 1f, wy * 0.01f * 1f) * 0.5f * num3;
		num3 -= 0.07f;
		float num4 = Mathf.PerlinNoise(wx * 0.002f * 0.25f + 0.123f, wy * 0.002f * 0.25f + 0.15123f);
		float num5 = Mathf.PerlinNoise(wx * 0.002f * 0.25f + 0.321f, wy * 0.002f * 0.25f + 0.231f);
		float v = Mathf.Abs(num4 - num5);
		float num6 = 1f - Utils.LerpStep(0.02f, 0.12f, v);
		num6 *= Utils.SmoothStep(744f, 1000f, num2);
		num3 *= 1f - num6;
		if (num2 > 10000f)
		{
			float t = Utils.LerpStep(10000f, 10500f, num2);
			num3 = Mathf.Lerp(num3, -0.2f, t);
			float num7 = 10490f;
			if (num2 > num7)
			{
				float t2 = Utils.LerpStep(num7, 10500f, num2);
				num3 = Mathf.Lerp(num3, -2f, t2);
			}
		}
		if (num2 < this.m_minMountainDistance && num3 > 0.28f)
		{
			float t3 = Mathf.Clamp01((num3 - 0.28f) / 0.099999994f);
			num3 = Mathf.Lerp(Mathf.Lerp(0.28f, 0.38f, t3), num3, Utils.LerpStep(this.m_minMountainDistance - 400f, this.m_minMountainDistance, num2));
		}
		return num3;
	}

	// Token: 0x06001B06 RID: 6918 RVA: 0x000B454C File Offset: 0x000B274C
	private float AddRivers(float wx, float wy, float h)
	{
		float num;
		float v;
		this.GetRiverWeight(wx, wy, out num, out v);
		if (num <= 0f)
		{
			return h;
		}
		float t = Utils.LerpStep(20f, 60f, v);
		float num2 = Mathf.Lerp(0.14f, 0.12f, t);
		float num3 = Mathf.Lerp(0.139f, 0.128f, t);
		if (h > num2)
		{
			h = Mathf.Lerp(h, num2, num);
		}
		if (h > num3)
		{
			float t2 = Utils.LerpStep(0.85f, 1f, num);
			h = Mathf.Lerp(h, num3, t2);
		}
		return h;
	}

	// Token: 0x06001B07 RID: 6919 RVA: 0x000B45D8 File Offset: 0x000B27D8
	public float GetHeight(float wx, float wy)
	{
		Heightmap.Biome biome = this.GetBiome(wx, wy);
		Color color;
		return this.GetBiomeHeight(biome, wx, wy, out color, false);
	}

	// Token: 0x06001B08 RID: 6920 RVA: 0x000B45FC File Offset: 0x000B27FC
	public float GetPregenerationHeight(float wx, float wy)
	{
		Heightmap.Biome biome = this.GetBiome(wx, wy);
		Color color;
		return this.GetBiomeHeight(biome, wx, wy, out color, true);
	}

	// Token: 0x06001B09 RID: 6921 RVA: 0x000B4620 File Offset: 0x000B2820
	public float GetBiomeHeight(Heightmap.Biome biome, float wx, float wy, out Color mask, bool preGeneration = false)
	{
		mask = Color.black;
		if (!this.m_world.m_menu)
		{
			if (biome <= Heightmap.Biome.Plains)
			{
				switch (biome)
				{
				case Heightmap.Biome.Meadows:
					return this.GetMeadowsHeight(wx, wy) * 200f;
				case Heightmap.Biome.Swamp:
					return this.GetMarshHeight(wx, wy) * 200f;
				case Heightmap.Biome.Meadows | Heightmap.Biome.Swamp:
					break;
				case Heightmap.Biome.Mountain:
					return this.GetSnowMountainHeight(wx, wy, false) * 200f;
				default:
					if (biome == Heightmap.Biome.BlackForest)
					{
						return this.GetForestHeight(wx, wy) * 200f;
					}
					if (biome == Heightmap.Biome.Plains)
					{
						return this.GetPlainsHeight(wx, wy) * 200f;
					}
					break;
				}
			}
			else if (biome <= Heightmap.Biome.DeepNorth)
			{
				if (biome == Heightmap.Biome.AshLands)
				{
					return this.GetAshlandsHeight(wx, wy) * 200f;
				}
				if (biome == Heightmap.Biome.DeepNorth)
				{
					return this.GetDeepNorthHeight(wx, wy) * 200f;
				}
			}
			else
			{
				if (biome == Heightmap.Biome.Ocean)
				{
					return this.GetOceanHeight(wx, wy) * 200f;
				}
				if (biome == Heightmap.Biome.Mistlands)
				{
					if (preGeneration)
					{
						return this.GetForestHeight(wx, wy) * 200f;
					}
					return this.GetMistlandsHeight(wx, wy, out mask) * 200f;
				}
			}
			return 0f;
		}
		if (biome == Heightmap.Biome.Mountain)
		{
			return this.GetSnowMountainHeight(wx, wy, true) * 200f;
		}
		return this.GetMenuHeight(wx, wy) * 200f;
	}

	// Token: 0x06001B0A RID: 6922 RVA: 0x000B4764 File Offset: 0x000B2964
	private float GetMarshHeight(float wx, float wy)
	{
		float wx2 = wx;
		float wy2 = wy;
		float num = 0.137f;
		wx += 100000f;
		wy += 100000f;
		float num2 = Mathf.PerlinNoise(wx * 0.04f, wy * 0.04f) * Mathf.PerlinNoise(wx * 0.08f, wy * 0.08f);
		num += num2 * 0.03f;
		num = this.AddRivers(wx2, wy2, num);
		num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		return num + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
	}

	// Token: 0x06001B0B RID: 6923 RVA: 0x000B4804 File Offset: 0x000B2A04
	private float GetMeadowsHeight(float wx, float wy)
	{
		float wx2 = wx;
		float wy2 = wy;
		float baseHeight = this.GetBaseHeight(wx, wy, false);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num * 0.5f;
		float num2 = baseHeight;
		num2 += num * 0.1f;
		float num3 = 0.15f;
		float num4 = num2 - num3;
		float num5 = Mathf.Clamp01(baseHeight / 0.4f);
		if (num4 > 0f)
		{
			num2 -= num4 * (1f - num5) * 0.75f;
		}
		num2 = this.AddRivers(wx2, wy2, num2);
		num2 += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		return num2 + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
	}

	// Token: 0x06001B0C RID: 6924 RVA: 0x000B4920 File Offset: 0x000B2B20
	private float GetForestHeight(float wx, float wy)
	{
		float wx2 = wx;
		float wy2 = wy;
		float num = this.GetBaseHeight(wx, wy, false);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num2 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num2 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num2 * 0.5f;
		num += num2 * 0.1f;
		num = this.AddRivers(wx2, wy2, num);
		num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		return num + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
	}

	// Token: 0x06001B0D RID: 6925 RVA: 0x000B4A04 File Offset: 0x000B2C04
	private float GetMistlandsHeight(float wx, float wy, out Color mask)
	{
		float wx2 = wx;
		float wy2 = wy;
		float num = this.GetBaseHeight(wx, wy, false);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num2 = Mathf.PerlinNoise(wx * 0.02f * 0.7f, wy * 0.02f * 0.7f) * Mathf.PerlinNoise(wx * 0.04f * 0.7f, wy * 0.04f * 0.7f);
		num2 += Mathf.PerlinNoise(wx * 0.03f * 0.7f, wy * 0.03f * 0.7f) * Mathf.PerlinNoise(wx * 0.05f * 0.7f, wy * 0.05f * 0.7f) * num2 * 0.5f;
		num2 = ((num2 > 0f) ? Mathf.Pow(num2, 1.5f) : num2);
		num += num2 * 0.4f;
		num = this.AddRivers(wx2, wy2, num);
		float num3 = Mathf.Clamp01(num2 * 7f);
		num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.03f * num3;
		num += Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.01f * num3;
		float num4 = 1f - num3 * 1.2f;
		num4 -= 1f - Utils.LerpStep(0.1f, 0.3f, num3);
		float a = num + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.002f;
		float num5 = num;
		num5 *= 400f;
		num5 = Mathf.Ceil(num5);
		num5 /= 400f;
		num = Mathf.Lerp(a, num5, num3);
		mask = new Color(0f, 0f, 0f, num4);
		return num;
	}

	// Token: 0x06001B0E RID: 6926 RVA: 0x000B4BD0 File Offset: 0x000B2DD0
	private float GetPlainsHeight(float wx, float wy)
	{
		float wx2 = wx;
		float wy2 = wy;
		float baseHeight = this.GetBaseHeight(wx, wy, false);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num * 0.5f;
		float num2 = baseHeight;
		num2 += num * 0.1f;
		float num3 = 0.15f;
		float num4 = num2 - num3;
		float num5 = Mathf.Clamp01(baseHeight / 0.4f);
		if (num4 > 0f)
		{
			num2 -= num4 * (1f - num5) * 0.75f;
		}
		num2 = this.AddRivers(wx2, wy2, num2);
		num2 += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		return num2 + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
	}

	// Token: 0x06001B0F RID: 6927 RVA: 0x000B4CEC File Offset: 0x000B2EEC
	private float GetMenuHeight(float wx, float wy)
	{
		float baseHeight = this.GetBaseHeight(wx, wy, true);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num * 0.5f;
		return baseHeight + num * 0.1f + Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
	}

	// Token: 0x06001B10 RID: 6928 RVA: 0x000B4DB8 File Offset: 0x000B2FB8
	private float GetAshlandsHeight(float wx, float wy)
	{
		float wx2 = wx;
		float wy2 = wy;
		float num = this.GetBaseHeight(wx, wy, false);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num2 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num2 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num2 * 0.5f;
		num += num2 * 0.1f;
		num += 0.1f;
		num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		num += Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
		return this.AddRivers(wx2, wy2, num);
	}

	// Token: 0x06001B11 RID: 6929 RVA: 0x000B4EA4 File Offset: 0x000B30A4
	private float GetEdgeHeight(float wx, float wy)
	{
		float magnitude = new Vector2(wx, wy).magnitude;
		float num = 10490f;
		if (magnitude > num)
		{
			float num2 = Utils.LerpStep(num, 10500f, magnitude);
			return -2f * num2;
		}
		float t = Utils.LerpStep(10000f, 10100f, magnitude);
		float num3 = this.GetBaseHeight(wx, wy, false);
		num3 = Mathf.Lerp(num3, 0f, t);
		return this.AddRivers(wx, wy, num3);
	}

	// Token: 0x06001B12 RID: 6930 RVA: 0x000B4F22 File Offset: 0x000B3122
	private float GetOceanHeight(float wx, float wy)
	{
		return this.GetBaseHeight(wx, wy, false);
	}

	// Token: 0x06001B13 RID: 6931 RVA: 0x000B4F30 File Offset: 0x000B3130
	private float BaseHeightTilt(float wx, float wy)
	{
		float baseHeight = this.GetBaseHeight(wx - 1f, wy, false);
		float baseHeight2 = this.GetBaseHeight(wx + 1f, wy, false);
		float baseHeight3 = this.GetBaseHeight(wx, wy - 1f, false);
		float baseHeight4 = this.GetBaseHeight(wx, wy + 1f, false);
		return Mathf.Abs(baseHeight2 - baseHeight) + Mathf.Abs(baseHeight3 - baseHeight4);
	}

	// Token: 0x06001B14 RID: 6932 RVA: 0x000B4F8C File Offset: 0x000B318C
	private float GetSnowMountainHeight(float wx, float wy, bool menu)
	{
		float wx2 = wx;
		float wy2 = wy;
		float num = this.GetBaseHeight(wx, wy, menu);
		float num2 = this.BaseHeightTilt(wx, wy);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num3 = num - 0.4f;
		num += num3;
		float num4 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num4 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num4 * 0.5f;
		num += num4 * 0.2f;
		num = this.AddRivers(wx2, wy2, num);
		num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		num += Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
		return num + Mathf.PerlinNoise(wx * 0.2f, wy * 0.2f) * 2f * num2;
	}

	// Token: 0x06001B15 RID: 6933 RVA: 0x000B50A8 File Offset: 0x000B32A8
	private float GetDeepNorthHeight(float wx, float wy)
	{
		float wx2 = wx;
		float wy2 = wy;
		float num = this.GetBaseHeight(wx, wy, false);
		wx += 100000f + this.m_offset3;
		wy += 100000f + this.m_offset3;
		float num2 = Mathf.Max(0f, num - 0.4f);
		num += num2;
		float num3 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
		num3 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num3 * 0.5f;
		num += num3 * 0.2f;
		num *= 1.2f;
		num = this.AddRivers(wx2, wy2, num);
		num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
		return num + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
	}

	// Token: 0x06001B16 RID: 6934 RVA: 0x000B51AD File Offset: 0x000B33AD
	public static bool InForest(Vector3 pos)
	{
		return WorldGenerator.GetForestFactor(pos) < 1.15f;
	}

	// Token: 0x06001B17 RID: 6935 RVA: 0x000B51BC File Offset: 0x000B33BC
	public static float GetForestFactor(Vector3 pos)
	{
		float d = 0.4f;
		return Utils.Fbm(pos * 0.01f * d, 3, 1.6f, 0.7f);
	}

	// Token: 0x06001B18 RID: 6936 RVA: 0x000B51F0 File Offset: 0x000B33F0
	public void GetTerrainDelta(Vector3 center, float radius, out float delta, out Vector3 slopeDirection)
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
			float height = this.GetHeight(vector2.x, vector2.z);
			if (height < num3)
			{
				num3 = height;
				a = vector2;
			}
			if (height > num2)
			{
				num2 = height;
				b = vector2;
			}
		}
		delta = num2 - num3;
		slopeDirection = Vector3.Normalize(a - b);
	}

	// Token: 0x06001B19 RID: 6937 RVA: 0x000B5294 File Offset: 0x000B3494
	public int GetSeed()
	{
		return this.m_world.m_seed;
	}

	// Token: 0x04001D26 RID: 7462
	private const float m_waterTreshold = 0.05f;

	// Token: 0x04001D27 RID: 7463
	private static WorldGenerator m_instance;

	// Token: 0x04001D28 RID: 7464
	private World m_world;

	// Token: 0x04001D29 RID: 7465
	private int m_version;

	// Token: 0x04001D2A RID: 7466
	private float m_offset0;

	// Token: 0x04001D2B RID: 7467
	private float m_offset1;

	// Token: 0x04001D2C RID: 7468
	private float m_offset2;

	// Token: 0x04001D2D RID: 7469
	private float m_offset3;

	// Token: 0x04001D2E RID: 7470
	private float m_offset4;

	// Token: 0x04001D2F RID: 7471
	private int m_riverSeed;

	// Token: 0x04001D30 RID: 7472
	private int m_streamSeed;

	// Token: 0x04001D31 RID: 7473
	private List<Vector2> m_lakes;

	// Token: 0x04001D32 RID: 7474
	private List<WorldGenerator.River> m_rivers = new List<WorldGenerator.River>();

	// Token: 0x04001D33 RID: 7475
	private List<WorldGenerator.River> m_streams = new List<WorldGenerator.River>();

	// Token: 0x04001D34 RID: 7476
	private Dictionary<Vector2i, WorldGenerator.RiverPoint[]> m_riverPoints = new Dictionary<Vector2i, WorldGenerator.RiverPoint[]>();

	// Token: 0x04001D35 RID: 7477
	private WorldGenerator.RiverPoint[] m_cachedRiverPoints;

	// Token: 0x04001D36 RID: 7478
	private Vector2i m_cachedRiverGrid = new Vector2i(-999999, -999999);

	// Token: 0x04001D37 RID: 7479
	private ReaderWriterLockSlim m_riverCacheLock = new ReaderWriterLockSlim();

	// Token: 0x04001D38 RID: 7480
	private List<Heightmap.Biome> m_biomes = new List<Heightmap.Biome>();

	// Token: 0x04001D39 RID: 7481
	private const float riverGridSize = 64f;

	// Token: 0x04001D3A RID: 7482
	private const float minRiverWidth = 60f;

	// Token: 0x04001D3B RID: 7483
	private const float maxRiverWidth = 100f;

	// Token: 0x04001D3C RID: 7484
	private const float minRiverCurveWidth = 50f;

	// Token: 0x04001D3D RID: 7485
	private const float maxRiverCurveWidth = 80f;

	// Token: 0x04001D3E RID: 7486
	private const float minRiverCurveWaveLength = 50f;

	// Token: 0x04001D3F RID: 7487
	private const float maxRiverCurveWaveLength = 70f;

	// Token: 0x04001D40 RID: 7488
	private const int streams = 3000;

	// Token: 0x04001D41 RID: 7489
	private const float streamWidth = 20f;

	// Token: 0x04001D42 RID: 7490
	private const float meadowsMaxDistance = 5000f;

	// Token: 0x04001D43 RID: 7491
	private const float minDeepForestNoise = 0.4f;

	// Token: 0x04001D44 RID: 7492
	private const float minDeepForestDistance = 600f;

	// Token: 0x04001D45 RID: 7493
	private const float maxDeepForestDistance = 6000f;

	// Token: 0x04001D46 RID: 7494
	private const float deepForestForestFactorMax = 0.9f;

	// Token: 0x04001D47 RID: 7495
	private const float marshBiomeScale = 0.001f;

	// Token: 0x04001D48 RID: 7496
	private const float minMarshNoise = 0.6f;

	// Token: 0x04001D49 RID: 7497
	private const float minMarshDistance = 2000f;

	// Token: 0x04001D4A RID: 7498
	private float maxMarshDistance = 6000f;

	// Token: 0x04001D4B RID: 7499
	private const float minMarshHeight = 0.05f;

	// Token: 0x04001D4C RID: 7500
	private const float maxMarshHeight = 0.25f;

	// Token: 0x04001D4D RID: 7501
	private const float heathBiomeScale = 0.001f;

	// Token: 0x04001D4E RID: 7502
	private const float minHeathNoise = 0.4f;

	// Token: 0x04001D4F RID: 7503
	private const float minHeathDistance = 3000f;

	// Token: 0x04001D50 RID: 7504
	private const float maxHeathDistance = 8000f;

	// Token: 0x04001D51 RID: 7505
	private const float darklandBiomeScale = 0.001f;

	// Token: 0x04001D52 RID: 7506
	private float minDarklandNoise = 0.4f;

	// Token: 0x04001D53 RID: 7507
	private const float minDarklandDistance = 6000f;

	// Token: 0x04001D54 RID: 7508
	private const float maxDarklandDistance = 10000f;

	// Token: 0x04001D55 RID: 7509
	private const float oceanBiomeScale = 0.0005f;

	// Token: 0x04001D56 RID: 7510
	private const float oceanBiomeMinNoise = 0.4f;

	// Token: 0x04001D57 RID: 7511
	private const float oceanBiomeMaxNoise = 0.6f;

	// Token: 0x04001D58 RID: 7512
	private const float oceanBiomeMinDistance = 1000f;

	// Token: 0x04001D59 RID: 7513
	private const float oceanBiomeMinDistanceBuffer = 256f;

	// Token: 0x04001D5A RID: 7514
	private float m_minMountainDistance = 1000f;

	// Token: 0x04001D5B RID: 7515
	private const float mountainBaseHeightMin = 0.4f;

	// Token: 0x04001D5C RID: 7516
	private const float deepNorthMinDistance = 12000f;

	// Token: 0x04001D5D RID: 7517
	private const float deepNorthYOffset = 4000f;

	// Token: 0x04001D5E RID: 7518
	private const float ashlandsMinDistance = 12000f;

	// Token: 0x04001D5F RID: 7519
	private const float ashlandsYOffset = -4000f;

	// Token: 0x04001D60 RID: 7520
	public const float worldSize = 10000f;

	// Token: 0x04001D61 RID: 7521
	public const float waterEdge = 10500f;

	// Token: 0x020002C9 RID: 713
	public class River
	{
		// Token: 0x04001D62 RID: 7522
		public Vector2 p0;

		// Token: 0x04001D63 RID: 7523
		public Vector2 p1;

		// Token: 0x04001D64 RID: 7524
		public Vector2 center;

		// Token: 0x04001D65 RID: 7525
		public float widthMin;

		// Token: 0x04001D66 RID: 7526
		public float widthMax;

		// Token: 0x04001D67 RID: 7527
		public float curveWidth;

		// Token: 0x04001D68 RID: 7528
		public float curveWavelength;
	}

	// Token: 0x020002CA RID: 714
	public struct RiverPoint
	{
		// Token: 0x06001B1C RID: 6940 RVA: 0x000B52A1 File Offset: 0x000B34A1
		public RiverPoint(Vector2 p_p, float p_w)
		{
			this.p = p_p;
			this.w = p_w;
			this.w2 = p_w * p_w;
		}

		// Token: 0x04001D69 RID: 7529
		public Vector2 p;

		// Token: 0x04001D6A RID: 7530
		public float w;

		// Token: 0x04001D6B RID: 7531
		public float w2;
	}
}
