using System;
using UnityEngine;

// Token: 0x02000277 RID: 631
public class Plant : SlowUpdate, Hoverable
{
	// Token: 0x0600182A RID: 6186 RVA: 0x000A1214 File Offset: 0x0009F414
	public override void Awake()
	{
		base.Awake();
		this.m_nview = base.gameObject.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_seed = this.m_nview.GetZDO().GetInt(ZDOVars.s_seed, 0);
		if (this.m_seed == 0)
		{
			this.m_seed = (int)((ulong)this.m_nview.GetZDO().m_uid.ID + (ulong)this.m_nview.GetZDO().m_uid.UserID);
			this.m_nview.GetZDO().Set(ZDOVars.s_seed, this.m_seed, true);
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(ZDOVars.s_plantTime, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_plantTime, ZNet.instance.GetTime().Ticks);
		}
		this.m_spawnTime = Time.time;
	}

	// Token: 0x0600182B RID: 6187 RVA: 0x000A1314 File Offset: 0x0009F514
	public string GetHoverText()
	{
		switch (this.m_status)
		{
		case Plant.Status.Healthy:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_healthy )");
		case Plant.Status.NoSun:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_nosun )");
		case Plant.Status.NoSpace:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_nospace )");
		case Plant.Status.WrongBiome:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_wrongbiome )");
		case Plant.Status.NotCultivated:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_notcultivated )");
		default:
			return "";
		}
	}

	// Token: 0x0600182C RID: 6188 RVA: 0x000A13D3 File Offset: 0x0009F5D3
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x0600182D RID: 6189 RVA: 0x000A13E8 File Offset: 0x0009F5E8
	private double TimeSincePlanted()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_plantTime, ZNet.instance.GetTime().Ticks));
		return (ZNet.instance.GetTime() - d).TotalSeconds;
	}

	// Token: 0x0600182E RID: 6190 RVA: 0x000A143C File Offset: 0x0009F63C
	public override void SUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (Time.time - this.m_updateTime < 10f)
		{
			return;
		}
		this.m_updateTime = Time.time;
		double num = this.TimeSincePlanted();
		this.UpdateHealth(num);
		float growTime = this.GetGrowTime();
		if (this.m_healthyGrown)
		{
			bool flag = num > (double)(growTime * 0.5f);
			this.m_healthy.SetActive(!flag && this.m_status == Plant.Status.Healthy);
			this.m_unhealthy.SetActive(!flag && this.m_status > Plant.Status.Healthy);
			this.m_healthyGrown.SetActive(flag && this.m_status == Plant.Status.Healthy);
			this.m_unhealthyGrown.SetActive(flag && this.m_status > Plant.Status.Healthy);
		}
		else
		{
			this.m_healthy.SetActive(this.m_status == Plant.Status.Healthy);
			this.m_unhealthy.SetActive(this.m_status > Plant.Status.Healthy);
		}
		if (this.m_nview.IsOwner() && Time.time - this.m_spawnTime > 10f && num > (double)growTime)
		{
			this.Grow();
		}
	}

	// Token: 0x0600182F RID: 6191 RVA: 0x000A1564 File Offset: 0x0009F764
	private float GetGrowTime()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.m_seed);
		float value = UnityEngine.Random.value;
		UnityEngine.Random.state = state;
		return Mathf.Lerp(this.m_growTime, this.m_growTimeMax, value);
	}

	// Token: 0x06001830 RID: 6192 RVA: 0x000A15A0 File Offset: 0x0009F7A0
	private void Grow()
	{
		if (this.m_status != Plant.Status.Healthy)
		{
			if (this.m_destroyIfCantGrow)
			{
				this.Destroy();
			}
			return;
		}
		GameObject original = this.m_grownPrefabs[UnityEngine.Random.Range(0, this.m_grownPrefabs.Length)];
		Quaternion quaternion = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, base.transform.position, quaternion);
		ZNetView component = gameObject.GetComponent<ZNetView>();
		float num = UnityEngine.Random.Range(this.m_minScale, this.m_maxScale);
		component.SetLocalScale(new Vector3(num, num, num));
		TreeBase component2 = gameObject.GetComponent<TreeBase>();
		if (component2)
		{
			component2.Grow();
		}
		this.m_nview.Destroy();
		this.m_growEffect.Create(base.transform.position, quaternion, null, num, -1);
	}

	// Token: 0x06001831 RID: 6193 RVA: 0x000A1668 File Offset: 0x0009F868
	private void UpdateHealth(double timeSincePlanted)
	{
		if (timeSincePlanted < 10.0)
		{
			this.m_status = Plant.Status.Healthy;
			return;
		}
		Heightmap heightmap = Heightmap.FindHeightmap(base.transform.position);
		if (heightmap)
		{
			if ((heightmap.GetBiome(base.transform.position) & this.m_biome) == Heightmap.Biome.None)
			{
				this.m_status = Plant.Status.WrongBiome;
				return;
			}
			if (this.m_needCultivatedGround && !heightmap.IsCultivated(base.transform.position))
			{
				this.m_status = Plant.Status.NotCultivated;
				return;
			}
		}
		if (this.HaveRoof())
		{
			this.m_status = Plant.Status.NoSun;
			return;
		}
		if (!this.HaveGrowSpace())
		{
			this.m_status = Plant.Status.NoSpace;
			return;
		}
		this.m_status = Plant.Status.Healthy;
	}

	// Token: 0x06001832 RID: 6194 RVA: 0x000A1710 File Offset: 0x0009F910
	private void Destroy()
	{
		IDestructible component = base.GetComponent<IDestructible>();
		if (component != null)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 9999f;
			component.Damage(hitData);
		}
	}

	// Token: 0x06001833 RID: 6195 RVA: 0x000A1744 File Offset: 0x0009F944
	private bool HaveRoof()
	{
		if (Plant.m_roofMask == 0)
		{
			Plant.m_roofMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"piece"
			});
		}
		return Physics.Raycast(base.transform.position, Vector3.up, 100f, Plant.m_roofMask);
	}

	// Token: 0x06001834 RID: 6196 RVA: 0x000A17A4 File Offset: 0x0009F9A4
	private bool HaveGrowSpace()
	{
		if (Plant.m_spaceMask == 0)
		{
			Plant.m_spaceMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"piece_nonsolid"
			});
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, this.m_growRadius, Plant.m_spaceMask);
		for (int i = 0; i < array.Length; i++)
		{
			Plant component = array[i].GetComponent<Plant>();
			if (!component || (!(component == this) && component.GetStatus() == Plant.Status.Healthy))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001835 RID: 6197 RVA: 0x000A183F File Offset: 0x0009FA3F
	private Plant.Status GetStatus()
	{
		return this.m_status;
	}

	// Token: 0x040019EC RID: 6636
	public string m_name = "Plant";

	// Token: 0x040019ED RID: 6637
	public float m_growTime = 10f;

	// Token: 0x040019EE RID: 6638
	public float m_growTimeMax = 2000f;

	// Token: 0x040019EF RID: 6639
	public GameObject[] m_grownPrefabs = new GameObject[0];

	// Token: 0x040019F0 RID: 6640
	public float m_minScale = 1f;

	// Token: 0x040019F1 RID: 6641
	public float m_maxScale = 1f;

	// Token: 0x040019F2 RID: 6642
	public float m_growRadius = 1f;

	// Token: 0x040019F3 RID: 6643
	public bool m_needCultivatedGround;

	// Token: 0x040019F4 RID: 6644
	public bool m_destroyIfCantGrow;

	// Token: 0x040019F5 RID: 6645
	[SerializeField]
	private GameObject m_healthy;

	// Token: 0x040019F6 RID: 6646
	[SerializeField]
	private GameObject m_unhealthy;

	// Token: 0x040019F7 RID: 6647
	[SerializeField]
	private GameObject m_healthyGrown;

	// Token: 0x040019F8 RID: 6648
	[SerializeField]
	private GameObject m_unhealthyGrown;

	// Token: 0x040019F9 RID: 6649
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	// Token: 0x040019FA RID: 6650
	public EffectList m_growEffect = new EffectList();

	// Token: 0x040019FB RID: 6651
	private Plant.Status m_status;

	// Token: 0x040019FC RID: 6652
	private ZNetView m_nview;

	// Token: 0x040019FD RID: 6653
	private float m_updateTime;

	// Token: 0x040019FE RID: 6654
	private float m_spawnTime;

	// Token: 0x040019FF RID: 6655
	private int m_seed;

	// Token: 0x04001A00 RID: 6656
	private static int m_spaceMask;

	// Token: 0x04001A01 RID: 6657
	private static int m_roofMask;

	// Token: 0x02000278 RID: 632
	private enum Status
	{
		// Token: 0x04001A03 RID: 6659
		Healthy,
		// Token: 0x04001A04 RID: 6660
		NoSun,
		// Token: 0x04001A05 RID: 6661
		NoSpace,
		// Token: 0x04001A06 RID: 6662
		WrongBiome,
		// Token: 0x04001A07 RID: 6663
		NotCultivated
	}
}
