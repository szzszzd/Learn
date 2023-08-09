using System;
using UnityEngine;

// Token: 0x0200000D RID: 13
public class EggGrow : MonoBehaviour, Hoverable
{
	// Token: 0x06000133 RID: 307 RVA: 0x0000863C File Offset: 0x0000683C
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_item = base.GetComponent<ItemDrop>();
		base.InvokeRepeating("GrowUpdate", UnityEngine.Random.Range(this.m_updateInterval, this.m_updateInterval * 2f), this.m_updateInterval);
		if (this.m_growingObject)
		{
			this.m_growingObject.SetActive(false);
		}
		if (this.m_notGrowingObject)
		{
			this.m_notGrowingObject.SetActive(true);
		}
	}

	// Token: 0x06000134 RID: 308 RVA: 0x000086BC File Offset: 0x000068BC
	private void GrowUpdate()
	{
		float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_growStart, 0f);
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner() || this.m_item.m_itemData.m_stack > 1)
		{
			this.UpdateEffects(num);
			return;
		}
		if (this.CanGrow())
		{
			if (num == 0f)
			{
				num = (float)ZNet.instance.GetTimeSeconds();
			}
		}
		else
		{
			num = 0f;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_growStart, num);
		this.UpdateEffects(num);
		if (num > 0f && ZNet.instance.GetTimeSeconds() > (double)(num + this.m_growTime))
		{
			Character component = UnityEngine.Object.Instantiate<GameObject>(this.m_grownPrefab, base.transform.position, base.transform.rotation).GetComponent<Character>();
			this.m_hatchEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			if (component)
			{
				component.SetTamed(this.m_tamed);
				component.SetLevel(this.m_item.m_itemData.m_quality);
			}
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06000135 RID: 309 RVA: 0x00008804 File Offset: 0x00006A04
	private bool CanGrow()
	{
		if (this.m_item.m_itemData.m_stack > 1)
		{
			return false;
		}
		if (this.m_requireNearbyFire && !EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Heat, 0.5f))
		{
			return false;
		}
		if (this.m_requireUnderRoof)
		{
			float num;
			bool flag;
			Cover.GetCoverForPoint(base.transform.position, out num, out flag, 0.1f);
			if (!flag || num < this.m_requireCoverPercentige)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000136 RID: 310 RVA: 0x00008880 File Offset: 0x00006A80
	private void UpdateEffects(float grow)
	{
		if (this.m_growingObject)
		{
			this.m_growingObject.SetActive(grow > 0f);
		}
		if (this.m_notGrowingObject)
		{
			this.m_notGrowingObject.SetActive(grow == 0f);
		}
	}

	// Token: 0x06000137 RID: 311 RVA: 0x000088D0 File Offset: 0x00006AD0
	public string GetHoverText()
	{
		if (!this.m_item)
		{
			return "";
		}
		if (!this.m_nview || !this.m_nview.IsValid())
		{
			return this.m_item.GetHoverText();
		}
		bool flag = this.m_nview.GetZDO().GetFloat(ZDOVars.s_growStart, 0f) > 0f;
		string text = (this.m_item.m_itemData.m_stack > 1) ? "$item_chicken_egg_stacked" : (flag ? "$item_chicken_egg_warm" : "$item_chicken_egg_cold");
		string hoverText = this.m_item.GetHoverText();
		int num = hoverText.IndexOf('\n');
		if (num > 0)
		{
			return hoverText.Substring(0, num) + " " + Localization.instance.Localize(text) + hoverText.Substring(num);
		}
		return this.m_item.GetHoverText();
	}

	// Token: 0x06000138 RID: 312 RVA: 0x000089AB File Offset: 0x00006BAB
	public string GetHoverName()
	{
		return this.m_item.GetHoverName();
	}

	// Token: 0x0400011D RID: 285
	public float m_growTime = 60f;

	// Token: 0x0400011E RID: 286
	public GameObject m_grownPrefab;

	// Token: 0x0400011F RID: 287
	public bool m_tamed;

	// Token: 0x04000120 RID: 288
	public float m_updateInterval = 5f;

	// Token: 0x04000121 RID: 289
	public bool m_requireNearbyFire = true;

	// Token: 0x04000122 RID: 290
	public bool m_requireUnderRoof = true;

	// Token: 0x04000123 RID: 291
	public float m_requireCoverPercentige = 0.7f;

	// Token: 0x04000124 RID: 292
	public EffectList m_hatchEffect;

	// Token: 0x04000125 RID: 293
	public GameObject m_growingObject;

	// Token: 0x04000126 RID: 294
	public GameObject m_notGrowingObject;

	// Token: 0x04000127 RID: 295
	private ZNetView m_nview;

	// Token: 0x04000128 RID: 296
	private ItemDrop m_item;
}
