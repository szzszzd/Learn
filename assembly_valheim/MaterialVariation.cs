using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200001D RID: 29
public class MaterialVariation : MonoBehaviour
{
	// Token: 0x060001B6 RID: 438 RVA: 0x0000C264 File Offset: 0x0000A464
	private void Start()
	{
		this.m_nview = base.GetComponentInParent<ZNetView>();
		this.m_renderer = base.GetComponent<SkinnedMeshRenderer>();
		if (!this.m_nview || !this.m_renderer)
		{
			ZLog.LogError("Missing nview or renderer on '" + base.transform.gameObject.name + "'");
		}
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x0000C2C8 File Offset: 0x0000A4C8
	private void Update()
	{
		if (this.m_variation < 0 && this.m_nview && this.m_renderer)
		{
			this.m_variation = this.m_nview.GetZDO().GetInt("MatVar" + this.m_materialIndex.ToString(), -1);
			if (this.m_variation < 0 && this.m_nview.IsOwner())
			{
				this.m_variation = this.GetWeightedVariation();
				this.m_nview.GetZDO().Set("MatVar" + this.m_materialIndex.ToString(), this.m_variation);
			}
			if (this.m_variation >= 0)
			{
				Material[] materials = this.m_renderer.materials;
				materials[this.m_materialIndex] = this.m_materials[this.m_variation].m_material;
				this.m_renderer.materials = materials;
			}
		}
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x0000C3BC File Offset: 0x0000A5BC
	private int GetWeightedVariation()
	{
		float num = 0f;
		foreach (MaterialVariation.MaterialEntry materialEntry in this.m_materials)
		{
			num += materialEntry.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		for (int i = 0; i < this.m_materials.Count; i++)
		{
			num3 += this.m_materials[i].m_weight;
			if (num2 <= num3)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x040001B4 RID: 436
	public int m_materialIndex;

	// Token: 0x040001B5 RID: 437
	public List<MaterialVariation.MaterialEntry> m_materials = new List<MaterialVariation.MaterialEntry>();

	// Token: 0x040001B6 RID: 438
	private ZNetView m_nview;

	// Token: 0x040001B7 RID: 439
	private SkinnedMeshRenderer m_renderer;

	// Token: 0x040001B8 RID: 440
	private int m_variation = -1;

	// Token: 0x0200001E RID: 30
	[Serializable]
	public class MaterialEntry
	{
		// Token: 0x040001B9 RID: 441
		public Material m_material;

		// Token: 0x040001BA RID: 442
		public float m_weight = 1f;
	}
}
