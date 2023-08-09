using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200021D RID: 541
public class ClutterSystem : MonoBehaviour
{
	// Token: 0x170000E4 RID: 228
	// (get) Token: 0x06001578 RID: 5496 RVA: 0x0008C912 File Offset: 0x0008AB12
	public static ClutterSystem instance
	{
		get
		{
			return ClutterSystem.m_instance;
		}
	}

	// Token: 0x06001579 RID: 5497 RVA: 0x0008C91C File Offset: 0x0008AB1C
	private void Awake()
	{
		ClutterSystem.m_instance = this;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			return;
		}
		this.ApplySettings();
		this.m_placeRayMask = LayerMask.GetMask(new string[]
		{
			"terrain"
		});
		this.m_grassRoot = new GameObject("grassroot");
		this.m_grassRoot.transform.SetParent(base.transform);
	}

	// Token: 0x0600157A RID: 5498 RVA: 0x0008C980 File Offset: 0x0008AB80
	public void ApplySettings()
	{
		ClutterSystem.Quality @int = (ClutterSystem.Quality)PlatformPrefs.GetInt("ClutterQuality", 2);
		if (this.m_quality == @int)
		{
			return;
		}
		this.m_quality = @int;
		this.ClearAll();
	}

	// Token: 0x0600157B RID: 5499 RVA: 0x0008C9B0 File Offset: 0x0008ABB0
	private void LateUpdate()
	{
		if (!RenderGroupSystem.IsGroupActive(RenderGroup.Overworld))
		{
			this.ClearAll();
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 center = (!GameCamera.InFreeFly() && Player.m_localPlayer) ? Player.m_localPlayer.transform.position : mainCamera.transform.position;
		if (this.m_forceRebuild)
		{
			if (this.IsHeightmapReady())
			{
				this.m_forceRebuild = false;
				this.UpdateGrass(Time.deltaTime, true, center);
			}
		}
		else if (this.IsHeightmapReady())
		{
			this.UpdateGrass(Time.deltaTime, false, center);
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null)
		{
			this.m_oldPlayerPos = Vector3.Lerp(this.m_oldPlayerPos, localPlayer.transform.position, this.m_playerPushFade);
			Shader.SetGlobalVector("_PlayerPosition", localPlayer.transform.position);
			Shader.SetGlobalVector("_PlayerOldPosition", this.m_oldPlayerPos);
			return;
		}
		Shader.SetGlobalVector("_PlayerPosition", new Vector3(999999f, 999999f, 999999f));
		Shader.SetGlobalVector("_PlayerOldPosition", new Vector3(999999f, 999999f, 999999f));
	}

	// Token: 0x0600157C RID: 5500 RVA: 0x0008CAF0 File Offset: 0x0008ACF0
	public Vector2Int GetVegPatch(Vector3 point)
	{
		int x = Mathf.FloorToInt((point.x + this.m_grassPatchSize / 2f) / this.m_grassPatchSize);
		int y = Mathf.FloorToInt((point.z + this.m_grassPatchSize / 2f) / this.m_grassPatchSize);
		return new Vector2Int(x, y);
	}

	// Token: 0x0600157D RID: 5501 RVA: 0x0008CB42 File Offset: 0x0008AD42
	public Vector3 GetVegPatchCenter(Vector2Int p)
	{
		return new Vector3((float)p.x * this.m_grassPatchSize, 0f, (float)p.y * this.m_grassPatchSize);
	}

	// Token: 0x0600157E RID: 5502 RVA: 0x0008CB6C File Offset: 0x0008AD6C
	private bool IsHeightmapReady()
	{
		Camera mainCamera = Utils.GetMainCamera();
		return mainCamera && !Heightmap.HaveQueuedRebuild(mainCamera.transform.position, this.m_distance);
	}

	// Token: 0x0600157F RID: 5503 RVA: 0x0008CBA4 File Offset: 0x0008ADA4
	private void UpdateGrass(float dt, bool rebuildAll, Vector3 center)
	{
		if (this.m_quality == ClutterSystem.Quality.Off)
		{
			return;
		}
		this.GeneratePatches(rebuildAll, center);
		this.TimeoutPatches(dt);
	}

	// Token: 0x06001580 RID: 5504 RVA: 0x0008CBC0 File Offset: 0x0008ADC0
	private void GeneratePatches(bool rebuildAll, Vector3 center)
	{
		bool flag = false;
		Vector2Int vegPatch = this.GetVegPatch(center);
		this.GeneratePatch(center, vegPatch, ref flag, rebuildAll);
		int num = Mathf.CeilToInt((this.m_distance - this.m_grassPatchSize / 2f) / this.m_grassPatchSize);
		for (int i = 1; i <= num; i++)
		{
			for (int j = vegPatch.x - i; j <= vegPatch.x + i; j++)
			{
				this.GeneratePatch(center, new Vector2Int(j, vegPatch.y - i), ref flag, rebuildAll);
				this.GeneratePatch(center, new Vector2Int(j, vegPatch.y + i), ref flag, rebuildAll);
			}
			for (int k = vegPatch.y - i + 1; k <= vegPatch.y + i - 1; k++)
			{
				this.GeneratePatch(center, new Vector2Int(vegPatch.x - i, k), ref flag, rebuildAll);
				this.GeneratePatch(center, new Vector2Int(vegPatch.x + i, k), ref flag, rebuildAll);
			}
		}
	}

	// Token: 0x06001581 RID: 5505 RVA: 0x0008CCC0 File Offset: 0x0008AEC0
	private void GeneratePatch(Vector3 camPos, Vector2Int p, ref bool generated, bool rebuildAll)
	{
		if (Utils.DistanceXZ(this.GetVegPatchCenter(p), camPos) > this.m_distance)
		{
			return;
		}
		ClutterSystem.PatchData patchData;
		if (this.m_patches.TryGetValue(p, out patchData) && !patchData.m_reset)
		{
			patchData.m_timer = 0f;
			return;
		}
		if (rebuildAll || !generated || this.m_menuHack)
		{
			ClutterSystem.PatchData patchData2 = this.GenerateVegPatch(p, this.m_grassPatchSize);
			if (patchData2 != null)
			{
				ClutterSystem.PatchData patchData3;
				if (this.m_patches.TryGetValue(p, out patchData3))
				{
					foreach (GameObject obj in patchData3.m_objects)
					{
						UnityEngine.Object.Destroy(obj);
					}
					this.FreePatch(patchData3);
					this.m_patches.Remove(p);
				}
				this.m_patches.Add(p, patchData2);
				generated = true;
			}
		}
	}

	// Token: 0x06001582 RID: 5506 RVA: 0x0008CDA0 File Offset: 0x0008AFA0
	private void TimeoutPatches(float dt)
	{
		this.m_tempToRemovePair.Clear();
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> item in this.m_patches)
		{
			item.Value.m_timer += dt;
			if (item.Value.m_timer >= 2f)
			{
				this.m_tempToRemovePair.Add(item);
			}
		}
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> keyValuePair in this.m_tempToRemovePair)
		{
			foreach (GameObject obj in keyValuePair.Value.m_objects)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.m_patches.Remove(keyValuePair.Key);
			this.FreePatch(keyValuePair.Value);
		}
	}

	// Token: 0x06001583 RID: 5507 RVA: 0x0008CECC File Offset: 0x0008B0CC
	public void ClearAll()
	{
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> keyValuePair in this.m_patches)
		{
			foreach (GameObject obj in keyValuePair.Value.m_objects)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.FreePatch(keyValuePair.Value);
		}
		this.m_patches.Clear();
		this.m_forceRebuild = true;
	}

	// Token: 0x06001584 RID: 5508 RVA: 0x0008CF7C File Offset: 0x0008B17C
	public void ResetGrass(Vector3 center, float radius)
	{
		float num = this.m_grassPatchSize / 2f;
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> keyValuePair in this.m_patches)
		{
			Vector3 center2 = keyValuePair.Value.center;
			if (center2.x + num >= center.x - radius && center2.x - num <= center.x + radius && center2.z + num >= center.z - radius && center2.z - num <= center.z + radius)
			{
				keyValuePair.Value.m_reset = true;
				this.m_forceRebuild = true;
			}
		}
	}

	// Token: 0x06001585 RID: 5509 RVA: 0x0008D040 File Offset: 0x0008B240
	public bool GetGroundInfo(Vector3 p, out Vector3 point, out Vector3 normal, out Heightmap hmap, out Heightmap.Biome biome)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * 500f, Vector3.down, out raycastHit, 1000f, this.m_placeRayMask))
		{
			point = raycastHit.point;
			normal = raycastHit.normal;
			hmap = raycastHit.collider.GetComponent<Heightmap>();
			biome = hmap.GetBiome(point);
			return true;
		}
		point = p;
		normal = Vector3.up;
		hmap = null;
		biome = Heightmap.Biome.Meadows;
		return false;
	}

	// Token: 0x06001586 RID: 5510 RVA: 0x0008D0D4 File Offset: 0x0008B2D4
	private Heightmap.Biome GetPatchBiomes(Vector3 center, float halfSize)
	{
		Heightmap.Biome biome = Heightmap.FindBiomeClutter(new Vector3(center.x - halfSize, 0f, center.z - halfSize));
		Heightmap.Biome biome2 = Heightmap.FindBiomeClutter(new Vector3(center.x + halfSize, 0f, center.z - halfSize));
		Heightmap.Biome biome3 = Heightmap.FindBiomeClutter(new Vector3(center.x - halfSize, 0f, center.z + halfSize));
		Heightmap.Biome biome4 = Heightmap.FindBiomeClutter(new Vector3(center.x + halfSize, 0f, center.z + halfSize));
		if (biome == Heightmap.Biome.None || biome2 == Heightmap.Biome.None || biome3 == Heightmap.Biome.None || biome4 == Heightmap.Biome.None)
		{
			return Heightmap.Biome.None;
		}
		return biome | biome2 | biome3 | biome4;
	}

	// Token: 0x06001587 RID: 5511 RVA: 0x0008D178 File Offset: 0x0008B378
	private ClutterSystem.PatchData GenerateVegPatch(Vector2Int patchID, float size)
	{
		Vector3 vegPatchCenter = this.GetVegPatchCenter(patchID);
		float num = size / 2f;
		Heightmap.Biome patchBiomes = this.GetPatchBiomes(vegPatchCenter, num);
		if (patchBiomes == Heightmap.Biome.None)
		{
			return null;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		ClutterSystem.PatchData patchData = this.AllocatePatch();
		patchData.center = vegPatchCenter;
		for (int i = 0; i < this.m_clutter.Count; i++)
		{
			ClutterSystem.Clutter clutter = this.m_clutter[i];
			if (clutter.m_enabled && (patchBiomes & clutter.m_biome) != Heightmap.Biome.None)
			{
				InstanceRenderer instanceRenderer = null;
				UnityEngine.Random.InitState(patchID.x * (patchID.y * 1374) + i * 9321);
				Vector3 b = new Vector3(clutter.m_fractalOffset, 0f, 0f);
				float num2 = Mathf.Cos(0.017453292f * clutter.m_maxTilt);
				float num3 = Mathf.Cos(0.017453292f * clutter.m_minTilt);
				int num4 = (this.m_quality == ClutterSystem.Quality.High) ? clutter.m_amount : (clutter.m_amount / 2);
				num4 = (int)((float)num4 * this.m_amountScale);
				int j = 0;
				while (j < num4)
				{
					Vector3 vector = new Vector3(UnityEngine.Random.Range(vegPatchCenter.x - num, vegPatchCenter.x + num), 0f, UnityEngine.Random.Range(vegPatchCenter.z - num, vegPatchCenter.z + num));
					float num5 = (float)UnityEngine.Random.Range(0, 360);
					if (!clutter.m_inForest)
					{
						goto IL_175;
					}
					float forestFactor = WorldGenerator.GetForestFactor(vector);
					if (forestFactor >= clutter.m_forestTresholdMin && forestFactor <= clutter.m_forestTresholdMax)
					{
						goto IL_175;
					}
					IL_42E:
					j++;
					continue;
					IL_175:
					if (clutter.m_fractalScale > 0f)
					{
						float num6 = Utils.Fbm(vector * 0.01f * clutter.m_fractalScale + b, 3, 1.6f, 0.7f);
						if (num6 < clutter.m_fractalTresholdMin || num6 > clutter.m_fractalTresholdMax)
						{
							goto IL_42E;
						}
					}
					Vector3 vector2;
					Vector3 vector3;
					Heightmap heightmap;
					Heightmap.Biome biome;
					if (!this.GetGroundInfo(vector, out vector2, out vector3, out heightmap, out biome) || (clutter.m_biome & biome) == Heightmap.Biome.None)
					{
						goto IL_42E;
					}
					float num7 = vector2.y - this.m_waterLevel;
					if (num7 < clutter.m_minAlt || num7 > clutter.m_maxAlt || vector3.y < num2 || vector3.y > num3)
					{
						goto IL_42E;
					}
					if (clutter.m_minOceanDepth != clutter.m_maxOceanDepth)
					{
						float oceanDepth = heightmap.GetOceanDepth(vector);
						if (oceanDepth < clutter.m_minOceanDepth || oceanDepth > clutter.m_maxOceanDepth)
						{
							goto IL_42E;
						}
					}
					if (clutter.m_minVegetation != clutter.m_maxVegetation)
					{
						float vegetationMask = heightmap.GetVegetationMask(vector2);
						if (vegetationMask > clutter.m_maxVegetation || vegetationMask < clutter.m_minVegetation)
						{
							goto IL_42E;
						}
					}
					if (!clutter.m_onCleared || !clutter.m_onUncleared)
					{
						bool flag = heightmap.IsCleared(vector2);
						if ((clutter.m_onCleared && !flag) || (clutter.m_onUncleared && flag))
						{
							goto IL_42E;
						}
					}
					vector = vector2;
					if (clutter.m_snapToWater)
					{
						vector.y = this.m_waterLevel;
					}
					if (clutter.m_randomOffset != 0f)
					{
						vector.y += UnityEngine.Random.Range(-clutter.m_randomOffset, clutter.m_randomOffset);
					}
					Quaternion quaternion = Quaternion.identity;
					if (clutter.m_terrainTilt)
					{
						quaternion = Quaternion.AngleAxis(num5, vector3);
					}
					else
					{
						quaternion = Quaternion.Euler(0f, num5, 0f);
					}
					if (clutter.m_instanced)
					{
						if (instanceRenderer == null)
						{
							GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(clutter.m_prefab, vegPatchCenter, Quaternion.identity, this.m_grassRoot.transform);
							instanceRenderer = gameObject.GetComponent<InstanceRenderer>();
							if (instanceRenderer.m_lodMaxDistance > this.m_distance - this.m_grassPatchSize / 2f)
							{
								instanceRenderer.m_lodMaxDistance = this.m_distance - this.m_grassPatchSize / 2f;
							}
							patchData.m_objects.Add(gameObject);
						}
						float scale = UnityEngine.Random.Range(clutter.m_scaleMin, clutter.m_scaleMax);
						instanceRenderer.AddInstance(vector, quaternion, scale);
						goto IL_42E;
					}
					GameObject item = UnityEngine.Object.Instantiate<GameObject>(clutter.m_prefab, vector, quaternion, this.m_grassRoot.transform);
					patchData.m_objects.Add(item);
					goto IL_42E;
				}
			}
		}
		UnityEngine.Random.state = state;
		return patchData;
	}

	// Token: 0x06001588 RID: 5512 RVA: 0x0008D5E2 File Offset: 0x0008B7E2
	private ClutterSystem.PatchData AllocatePatch()
	{
		if (this.m_freePatches.Count > 0)
		{
			return this.m_freePatches.Pop();
		}
		return new ClutterSystem.PatchData();
	}

	// Token: 0x06001589 RID: 5513 RVA: 0x0008D603 File Offset: 0x0008B803
	private void FreePatch(ClutterSystem.PatchData patch)
	{
		patch.center = Vector3.zero;
		patch.m_objects.Clear();
		patch.m_timer = 0f;
		patch.m_reset = false;
		this.m_freePatches.Push(patch);
	}

	// Token: 0x0400164A RID: 5706
	private static ClutterSystem m_instance;

	// Token: 0x0400164B RID: 5707
	private int m_placeRayMask;

	// Token: 0x0400164C RID: 5708
	public List<ClutterSystem.Clutter> m_clutter = new List<ClutterSystem.Clutter>();

	// Token: 0x0400164D RID: 5709
	public float m_grassPatchSize = 8f;

	// Token: 0x0400164E RID: 5710
	public float m_distance = 40f;

	// Token: 0x0400164F RID: 5711
	public float m_waterLevel = 27f;

	// Token: 0x04001650 RID: 5712
	public float m_playerPushFade = 0.05f;

	// Token: 0x04001651 RID: 5713
	public float m_amountScale = 1f;

	// Token: 0x04001652 RID: 5714
	public bool m_menuHack;

	// Token: 0x04001653 RID: 5715
	private Dictionary<Vector2Int, ClutterSystem.PatchData> m_patches = new Dictionary<Vector2Int, ClutterSystem.PatchData>();

	// Token: 0x04001654 RID: 5716
	private Stack<ClutterSystem.PatchData> m_freePatches = new Stack<ClutterSystem.PatchData>();

	// Token: 0x04001655 RID: 5717
	private GameObject m_grassRoot;

	// Token: 0x04001656 RID: 5718
	private Vector3 m_oldPlayerPos = Vector3.zero;

	// Token: 0x04001657 RID: 5719
	private List<Vector2Int> m_tempToRemove = new List<Vector2Int>();

	// Token: 0x04001658 RID: 5720
	private List<KeyValuePair<Vector2Int, ClutterSystem.PatchData>> m_tempToRemovePair = new List<KeyValuePair<Vector2Int, ClutterSystem.PatchData>>();

	// Token: 0x04001659 RID: 5721
	private ClutterSystem.Quality m_quality = ClutterSystem.Quality.High;

	// Token: 0x0400165A RID: 5722
	private bool m_forceRebuild;

	// Token: 0x0200021E RID: 542
	[Serializable]
	public class Clutter
	{
		// Token: 0x0400165B RID: 5723
		public string m_name = "";

		// Token: 0x0400165C RID: 5724
		public bool m_enabled = true;

		// Token: 0x0400165D RID: 5725
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x0400165E RID: 5726
		public bool m_instanced;

		// Token: 0x0400165F RID: 5727
		public GameObject m_prefab;

		// Token: 0x04001660 RID: 5728
		public int m_amount = 80;

		// Token: 0x04001661 RID: 5729
		public bool m_onUncleared = true;

		// Token: 0x04001662 RID: 5730
		public bool m_onCleared;

		// Token: 0x04001663 RID: 5731
		public float m_minVegetation;

		// Token: 0x04001664 RID: 5732
		public float m_maxVegetation;

		// Token: 0x04001665 RID: 5733
		public float m_scaleMin = 1f;

		// Token: 0x04001666 RID: 5734
		public float m_scaleMax = 1f;

		// Token: 0x04001667 RID: 5735
		public float m_maxTilt = 18f;

		// Token: 0x04001668 RID: 5736
		public float m_minTilt;

		// Token: 0x04001669 RID: 5737
		public float m_maxAlt = 1000f;

		// Token: 0x0400166A RID: 5738
		public float m_minAlt = 27f;

		// Token: 0x0400166B RID: 5739
		public bool m_snapToWater;

		// Token: 0x0400166C RID: 5740
		public bool m_terrainTilt;

		// Token: 0x0400166D RID: 5741
		public float m_randomOffset;

		// Token: 0x0400166E RID: 5742
		[Header("Ocean depth ")]
		public float m_minOceanDepth;

		// Token: 0x0400166F RID: 5743
		public float m_maxOceanDepth;

		// Token: 0x04001670 RID: 5744
		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		// Token: 0x04001671 RID: 5745
		public float m_forestTresholdMin;

		// Token: 0x04001672 RID: 5746
		public float m_forestTresholdMax = 1f;

		// Token: 0x04001673 RID: 5747
		[Header("Fractal placement (m_fractalScale > 0 == enabled) ")]
		public float m_fractalScale;

		// Token: 0x04001674 RID: 5748
		public float m_fractalOffset;

		// Token: 0x04001675 RID: 5749
		public float m_fractalTresholdMin = 0.5f;

		// Token: 0x04001676 RID: 5750
		public float m_fractalTresholdMax = 1f;
	}

	// Token: 0x0200021F RID: 543
	private class PatchData
	{
		// Token: 0x04001677 RID: 5751
		public Vector3 center;

		// Token: 0x04001678 RID: 5752
		public List<GameObject> m_objects = new List<GameObject>();

		// Token: 0x04001679 RID: 5753
		public float m_timer;

		// Token: 0x0400167A RID: 5754
		public bool m_reset;
	}

	// Token: 0x02000220 RID: 544
	public enum Quality
	{
		// Token: 0x0400167C RID: 5756
		Off,
		// Token: 0x0400167D RID: 5757
		Med,
		// Token: 0x0400167E RID: 5758
		High
	}
}
