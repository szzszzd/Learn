using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002A5 RID: 677
public class TerrainOp : MonoBehaviour
{
	// Token: 0x060019DA RID: 6618 RVA: 0x000AB8CC File Offset: 0x000A9ACC
	private void Awake()
	{
		if (TerrainOp.m_forceDisableTerrainOps)
		{
			return;
		}
		List<Heightmap> list = new List<Heightmap>();
		Heightmap.FindHeightmap(base.transform.position, this.GetRadius(), list);
		foreach (Heightmap heightmap in list)
		{
			heightmap.GetAndCreateTerrainCompiler().ApplyOperation(this);
		}
		this.OnPlaced();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x060019DB RID: 6619 RVA: 0x000AB954 File Offset: 0x000A9B54
	public float GetRadius()
	{
		return this.m_settings.GetRadius();
	}

	// Token: 0x060019DC RID: 6620 RVA: 0x000AB964 File Offset: 0x000A9B64
	private void OnPlaced()
	{
		this.m_onPlacedEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		if (this.m_spawnOnPlaced)
		{
			if (!this.m_spawnAtMaxLevelDepth && Heightmap.AtMaxLevelDepth(base.transform.position + Vector3.up * this.m_settings.m_levelOffset))
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

	// Token: 0x060019DD RID: 6621 RVA: 0x000ABA6C File Offset: 0x000A9C6C
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position + Vector3.up * this.m_settings.m_levelOffset, Quaternion.identity, new Vector3(1f, 0f, 1f));
		if (this.m_settings.m_level)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_settings.m_levelRadius);
		}
		if (this.m_settings.m_smooth)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_settings.m_smoothRadius);
		}
		if (this.m_settings.m_paintCleared)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_settings.m_paintRadius);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x04001BAD RID: 7085
	public static bool m_forceDisableTerrainOps;

	// Token: 0x04001BAE RID: 7086
	public TerrainOp.Settings m_settings = new TerrainOp.Settings();

	// Token: 0x04001BAF RID: 7087
	[Header("Effects")]
	public EffectList m_onPlacedEffect = new EffectList();

	// Token: 0x04001BB0 RID: 7088
	[Header("Spawn items")]
	public GameObject m_spawnOnPlaced;

	// Token: 0x04001BB1 RID: 7089
	public float m_chanceToSpawn = 1f;

	// Token: 0x04001BB2 RID: 7090
	public int m_maxSpawned = 1;

	// Token: 0x04001BB3 RID: 7091
	public bool m_spawnAtMaxLevelDepth = true;

	// Token: 0x020002A6 RID: 678
	[Serializable]
	public class Settings
	{
		// Token: 0x060019E0 RID: 6624 RVA: 0x000ABB88 File Offset: 0x000A9D88
		public void Serialize(ZPackage pkg)
		{
			pkg.Write(this.m_levelOffset);
			pkg.Write(this.m_level);
			pkg.Write(this.m_levelRadius);
			pkg.Write(this.m_square);
			pkg.Write(this.m_raise);
			pkg.Write(this.m_raiseRadius);
			pkg.Write(this.m_raisePower);
			pkg.Write(this.m_raiseDelta);
			pkg.Write(this.m_smooth);
			pkg.Write(this.m_smoothRadius);
			pkg.Write(this.m_smoothPower);
			pkg.Write(this.m_paintCleared);
			pkg.Write(this.m_paintHeightCheck);
			pkg.Write((int)this.m_paintType);
			pkg.Write(this.m_paintRadius);
		}

		// Token: 0x060019E1 RID: 6625 RVA: 0x000ABC4C File Offset: 0x000A9E4C
		public void Deserialize(ZPackage pkg)
		{
			this.m_levelOffset = pkg.ReadSingle();
			this.m_level = pkg.ReadBool();
			this.m_levelRadius = pkg.ReadSingle();
			this.m_square = pkg.ReadBool();
			this.m_raise = pkg.ReadBool();
			this.m_raiseRadius = pkg.ReadSingle();
			this.m_raisePower = pkg.ReadSingle();
			this.m_raiseDelta = pkg.ReadSingle();
			this.m_smooth = pkg.ReadBool();
			this.m_smoothRadius = pkg.ReadSingle();
			this.m_smoothPower = pkg.ReadSingle();
			this.m_paintCleared = pkg.ReadBool();
			this.m_paintHeightCheck = pkg.ReadBool();
			this.m_paintType = (TerrainModifier.PaintType)pkg.ReadInt();
			this.m_paintRadius = pkg.ReadSingle();
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x000ABD10 File Offset: 0x000A9F10
		public float GetRadius()
		{
			float num = 0f;
			if (this.m_level && this.m_levelRadius > num)
			{
				num = this.m_levelRadius;
			}
			if (this.m_raise && this.m_raiseRadius > num)
			{
				num = this.m_raiseRadius;
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

		// Token: 0x04001BB4 RID: 7092
		public float m_levelOffset;

		// Token: 0x04001BB5 RID: 7093
		[Header("Level")]
		public bool m_level;

		// Token: 0x04001BB6 RID: 7094
		public float m_levelRadius = 2f;

		// Token: 0x04001BB7 RID: 7095
		public bool m_square = true;

		// Token: 0x04001BB8 RID: 7096
		[Header("Raise")]
		public bool m_raise;

		// Token: 0x04001BB9 RID: 7097
		public float m_raiseRadius = 2f;

		// Token: 0x04001BBA RID: 7098
		public float m_raisePower;

		// Token: 0x04001BBB RID: 7099
		public float m_raiseDelta;

		// Token: 0x04001BBC RID: 7100
		[Header("Smooth")]
		public bool m_smooth;

		// Token: 0x04001BBD RID: 7101
		public float m_smoothRadius = 2f;

		// Token: 0x04001BBE RID: 7102
		public float m_smoothPower = 3f;

		// Token: 0x04001BBF RID: 7103
		[Header("Paint")]
		public bool m_paintCleared = true;

		// Token: 0x04001BC0 RID: 7104
		public bool m_paintHeightCheck;

		// Token: 0x04001BC1 RID: 7105
		public TerrainModifier.PaintType m_paintType;

		// Token: 0x04001BC2 RID: 7106
		public float m_paintRadius = 2f;
	}
}
