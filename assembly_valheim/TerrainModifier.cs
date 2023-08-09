using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002A3 RID: 675
[ExecuteInEditMode]
public class TerrainModifier : MonoBehaviour
{
	// Token: 0x060019C9 RID: 6601 RVA: 0x000AB19C File Offset: 0x000A939C
	private void Awake()
	{
		TerrainModifier.s_instances.Add(this);
		TerrainModifier.s_needsSorting = true;
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_wasEnabled = base.enabled;
		if (base.enabled)
		{
			if (TerrainModifier.m_triggerOnPlaced)
			{
				this.OnPlaced();
			}
			this.PokeHeightmaps(true);
		}
		this.m_creationTime = this.GetCreationTime();
	}

	// Token: 0x060019CA RID: 6602 RVA: 0x000AB1FA File Offset: 0x000A93FA
	private void OnDestroy()
	{
		TerrainModifier.s_instances.Remove(this);
		TerrainModifier.s_needsSorting = true;
		if (this.m_wasEnabled)
		{
			this.PokeHeightmaps(false);
		}
	}

	// Token: 0x060019CB RID: 6603 RVA: 0x000AB21D File Offset: 0x000A941D
	public static void RemoveAll()
	{
		TerrainModifier.s_instances.Clear();
	}

	// Token: 0x060019CC RID: 6604 RVA: 0x000AB22C File Offset: 0x000A942C
	private void PokeHeightmaps(bool forcedDelay = false)
	{
		bool delayed = !TerrainModifier.m_triggerOnPlaced || forcedDelay;
		foreach (Heightmap heightmap in Heightmap.GetAllHeightmaps())
		{
			if (heightmap.TerrainVSModifier(this))
			{
				heightmap.Poke(delayed);
			}
		}
		if (ClutterSystem.instance)
		{
			ClutterSystem.instance.ResetGrass(base.transform.position, this.GetRadius());
		}
	}

	// Token: 0x060019CD RID: 6605 RVA: 0x000AB2BC File Offset: 0x000A94BC
	public float GetRadius()
	{
		float num = 0f;
		if (this.m_level && this.m_levelRadius > num)
		{
			num = this.m_levelRadius;
		}
		if (this.m_smooth && this.m_smoothRadius > num)
		{
			num = this.m_smoothRadius;
		}
		if (this.m_paintCleared && this.m_paintRadius > num)
		{
			num = this.m_paintRadius;
		}
		return num;
	}

	// Token: 0x060019CE RID: 6606 RVA: 0x000AB318 File Offset: 0x000A9518
	public static void SetTriggerOnPlaced(bool trigger)
	{
		TerrainModifier.m_triggerOnPlaced = trigger;
	}

	// Token: 0x060019CF RID: 6607 RVA: 0x000AB320 File Offset: 0x000A9520
	private void OnPlaced()
	{
		this.RemoveOthers(base.transform.position, this.GetRadius() / 4f);
		this.m_onPlacedEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		if (this.m_spawnOnPlaced)
		{
			if (!this.m_spawnAtMaxLevelDepth && Heightmap.AtMaxLevelDepth(base.transform.position + Vector3.up * this.m_levelOffset))
			{
				return;
			}
			if (UnityEngine.Random.value <= this.m_chanceToSpawn)
			{
				Vector3 b = UnityEngine.Random.insideUnitCircle * 0.2f;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_spawnOnPlaced, base.transform.position + Vector3.up * 0.5f + b, Quaternion.identity);
				gameObject.GetComponent<ItemDrop>().m_itemData.m_stack = UnityEngine.Random.Range(1, this.m_maxSpawned + 1);
				gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
			}
		}
	}

	// Token: 0x060019D0 RID: 6608 RVA: 0x000AB440 File Offset: 0x000A9640
	private static void GetModifiers(Vector3 point, float range, List<TerrainModifier> modifiers, TerrainModifier ignore = null)
	{
		foreach (TerrainModifier terrainModifier in TerrainModifier.s_instances)
		{
			if (!(terrainModifier == ignore) && Utils.DistanceXZ(point, terrainModifier.transform.position) < range)
			{
				modifiers.Add(terrainModifier);
			}
		}
	}

	// Token: 0x060019D1 RID: 6609 RVA: 0x000AB4B0 File Offset: 0x000A96B0
	public static Piece FindClosestModifierPieceInRange(Vector3 point, float range)
	{
		float num = 999999f;
		TerrainModifier terrainModifier = null;
		foreach (TerrainModifier terrainModifier2 in TerrainModifier.s_instances)
		{
			if (!(terrainModifier2.m_nview == null))
			{
				float num2 = Utils.DistanceXZ(point, terrainModifier2.transform.position);
				if (num2 <= range && num2 <= num)
				{
					num = num2;
					terrainModifier = terrainModifier2;
				}
			}
		}
		if (terrainModifier)
		{
			return terrainModifier.GetComponent<Piece>();
		}
		return null;
	}

	// Token: 0x060019D2 RID: 6610 RVA: 0x000AB544 File Offset: 0x000A9744
	private void RemoveOthers(Vector3 point, float range)
	{
		List<TerrainModifier> list = new List<TerrainModifier>();
		TerrainModifier.GetModifiers(point, range, list, this);
		int num = 0;
		foreach (TerrainModifier terrainModifier in list)
		{
			if ((this.m_level || !terrainModifier.m_level) && (!this.m_paintCleared || this.m_paintType != TerrainModifier.PaintType.Reset || (terrainModifier.m_paintCleared && terrainModifier.m_paintType == TerrainModifier.PaintType.Reset)) && terrainModifier.m_nview && terrainModifier.m_nview.IsValid())
			{
				num++;
				terrainModifier.m_nview.ClaimOwnership();
				terrainModifier.m_nview.Destroy();
			}
		}
	}

	// Token: 0x060019D3 RID: 6611 RVA: 0x000AB604 File Offset: 0x000A9804
	private static int SortByModifiers(TerrainModifier a, TerrainModifier b)
	{
		if (a.m_playerModifiction != b.m_playerModifiction)
		{
			return a.m_playerModifiction.CompareTo(b.m_playerModifiction);
		}
		if (a.m_sortOrder == b.m_sortOrder)
		{
			return a.m_creationTime.CompareTo(b.m_creationTime);
		}
		return a.m_sortOrder.CompareTo(b.m_sortOrder);
	}

	// Token: 0x060019D4 RID: 6612 RVA: 0x000AB662 File Offset: 0x000A9862
	public static List<TerrainModifier> GetAllInstances()
	{
		if (TerrainModifier.s_needsSorting)
		{
			TerrainModifier.s_instances.Sort(new Comparison<TerrainModifier>(TerrainModifier.SortByModifiers));
			TerrainModifier.s_needsSorting = false;
		}
		return TerrainModifier.s_instances;
	}

	// Token: 0x060019D5 RID: 6613 RVA: 0x000AB68C File Offset: 0x000A988C
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position + Vector3.up * this.m_levelOffset, Quaternion.identity, new Vector3(1f, 0f, 1f));
		if (this.m_level)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_levelRadius);
		}
		if (this.m_smooth)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_smoothRadius);
		}
		if (this.m_paintCleared)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_paintRadius);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x060019D6 RID: 6614 RVA: 0x000AB74C File Offset: 0x000A994C
	public ZDOID GetZDOID()
	{
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			return this.m_nview.GetZDO().m_uid;
		}
		return ZDOID.None;
	}

	// Token: 0x060019D7 RID: 6615 RVA: 0x000AB780 File Offset: 0x000A9980
	private long GetCreationTime()
	{
		long num = 0L;
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().GetPrefab();
			ZDO zdo = this.m_nview.GetZDO();
			ZDOID uid = zdo.m_uid;
			num = zdo.GetLong(ZDOVars.s_terrainModifierTimeCreated, 0L);
			if (num == 0L)
			{
				num = ZDOExtraData.GetTimeCreated(uid);
				if (num != 0L)
				{
					zdo.Set(ZDOVars.s_terrainModifierTimeCreated, num);
					Debug.LogError("CreationTime should already be set for " + this.m_nview.name + "  Prefab: " + this.m_nview.GetZDO().GetPrefab().ToString());
				}
			}
		}
		return num;
	}

	// Token: 0x04001B8D RID: 7053
	private static bool m_triggerOnPlaced = false;

	// Token: 0x04001B8E RID: 7054
	public int m_sortOrder;

	// Token: 0x04001B8F RID: 7055
	public bool m_useTerrainCompiler;

	// Token: 0x04001B90 RID: 7056
	public bool m_playerModifiction;

	// Token: 0x04001B91 RID: 7057
	public float m_levelOffset;

	// Token: 0x04001B92 RID: 7058
	[Header("Level")]
	public bool m_level;

	// Token: 0x04001B93 RID: 7059
	public float m_levelRadius = 2f;

	// Token: 0x04001B94 RID: 7060
	public bool m_square = true;

	// Token: 0x04001B95 RID: 7061
	[Header("Smooth")]
	public bool m_smooth;

	// Token: 0x04001B96 RID: 7062
	public float m_smoothRadius = 2f;

	// Token: 0x04001B97 RID: 7063
	public float m_smoothPower = 3f;

	// Token: 0x04001B98 RID: 7064
	[Header("Paint")]
	public bool m_paintCleared = true;

	// Token: 0x04001B99 RID: 7065
	public bool m_paintHeightCheck;

	// Token: 0x04001B9A RID: 7066
	public TerrainModifier.PaintType m_paintType;

	// Token: 0x04001B9B RID: 7067
	public float m_paintRadius = 2f;

	// Token: 0x04001B9C RID: 7068
	[Header("Effects")]
	public EffectList m_onPlacedEffect = new EffectList();

	// Token: 0x04001B9D RID: 7069
	[Header("Spawn items")]
	public GameObject m_spawnOnPlaced;

	// Token: 0x04001B9E RID: 7070
	public float m_chanceToSpawn = 1f;

	// Token: 0x04001B9F RID: 7071
	public int m_maxSpawned = 1;

	// Token: 0x04001BA0 RID: 7072
	public bool m_spawnAtMaxLevelDepth = true;

	// Token: 0x04001BA1 RID: 7073
	private bool m_wasEnabled;

	// Token: 0x04001BA2 RID: 7074
	private long m_creationTime;

	// Token: 0x04001BA3 RID: 7075
	private ZNetView m_nview;

	// Token: 0x04001BA4 RID: 7076
	private static readonly List<TerrainModifier> s_instances = new List<TerrainModifier>();

	// Token: 0x04001BA5 RID: 7077
	private static bool s_needsSorting = false;

	// Token: 0x04001BA6 RID: 7078
	private static bool s_delayedPokeHeightmaps = false;

	// Token: 0x04001BA7 RID: 7079
	private static int s_lastFramePoked = 0;

	// Token: 0x020002A4 RID: 676
	public enum PaintType
	{
		// Token: 0x04001BA9 RID: 7081
		Dirt,
		// Token: 0x04001BAA RID: 7082
		Cultivate,
		// Token: 0x04001BAB RID: 7083
		Paved,
		// Token: 0x04001BAC RID: 7084
		Reset
	}
}
