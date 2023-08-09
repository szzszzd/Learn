using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200023C RID: 572
public class Floating : MonoBehaviour, IWaterInteractable
{
	// Token: 0x06001681 RID: 5761 RVA: 0x00094578 File Offset: 0x00092778
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_collider = base.GetComponentInChildren<Collider>();
		this.SetSurfaceEffect(false);
		Floating.s_waterVolumeMask = LayerMask.GetMask(new string[]
		{
			"WaterVolume"
		});
		base.InvokeRepeating("TerrainCheck", UnityEngine.Random.Range(10f, 30f), 30f);
	}

	// Token: 0x06001682 RID: 5762 RVA: 0x000945E7 File Offset: 0x000927E7
	private void OnEnable()
	{
		Floating.Instances.Add(this);
	}

	// Token: 0x06001683 RID: 5763 RVA: 0x000945F4 File Offset: 0x000927F4
	private void OnDisable()
	{
		Floating.Instances.Remove(this);
	}

	// Token: 0x06001684 RID: 5764 RVA: 0x0000652E File Offset: 0x0000472E
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x06001685 RID: 5765 RVA: 0x00094604 File Offset: 0x00092804
	private void TerrainCheck()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		if (base.transform.position.y - groundHeight < -1f)
		{
			Vector3 position = base.transform.position;
			position.y = groundHeight + 1f;
			base.transform.position = position;
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.velocity = Vector3.zero;
			}
			ZLog.Log("Moved up item " + base.gameObject.name);
		}
	}

	// Token: 0x06001686 RID: 5766 RVA: 0x000946B8 File Offset: 0x000928B8
	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.HaveLiquidLevel())
		{
			this.SetSurfaceEffect(false);
			return;
		}
		this.UpdateImpactEffect();
		float floatDepth = this.GetFloatDepth();
		if (floatDepth > 0f)
		{
			this.SetSurfaceEffect(false);
			return;
		}
		this.SetSurfaceEffect(true);
		Vector3 position = this.m_collider.ClosestPoint(base.transform.position + Vector3.down * 1000f);
		Vector3 worldCenterOfMass = this.m_body.worldCenterOfMass;
		float num = Mathf.Clamp01(Mathf.Abs(floatDepth) / this.m_forceDistance);
		Vector3 vector = this.m_force * num * (fixedDeltaTime * 50f) * Vector3.up;
		this.m_body.WakeUp();
		this.m_body.AddForceAtPosition(vector * this.m_balanceForceFraction, position, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(vector, worldCenterOfMass, ForceMode.VelocityChange);
		this.m_body.velocity = this.m_body.velocity - this.m_damping * num * this.m_body.velocity;
		this.m_body.angularVelocity = this.m_body.angularVelocity - this.m_damping * num * this.m_body.angularVelocity;
	}

	// Token: 0x06001687 RID: 5767 RVA: 0x00094814 File Offset: 0x00092A14
	public bool HaveLiquidLevel()
	{
		return this.m_waterLevel > -10000f || this.m_tarLevel > -10000f;
	}

	// Token: 0x06001688 RID: 5768 RVA: 0x00094832 File Offset: 0x00092A32
	private void SetSurfaceEffect(bool enabled)
	{
		if (this.m_surfaceEffects != null)
		{
			this.m_surfaceEffects.SetActive(enabled);
		}
	}

	// Token: 0x06001689 RID: 5769 RVA: 0x00094850 File Offset: 0x00092A50
	private void UpdateImpactEffect()
	{
		if (this.m_body.IsSleeping() || !this.m_impactEffects.HasEffects())
		{
			return;
		}
		Vector3 vector = this.m_collider.ClosestPoint(base.transform.position + Vector3.down * 1000f);
		float num = Mathf.Max(this.m_waterLevel, this.m_tarLevel);
		if (vector.y < num)
		{
			if (!this.m_wasInWater)
			{
				this.m_wasInWater = true;
				Vector3 basePos = vector;
				basePos.y = num;
				if (this.m_body.GetPointVelocity(vector).magnitude > 0.5f)
				{
					this.m_impactEffects.Create(basePos, Quaternion.identity, null, 1f, -1);
					return;
				}
			}
		}
		else
		{
			this.m_wasInWater = false;
		}
	}

	// Token: 0x0600168A RID: 5770 RVA: 0x00094914 File Offset: 0x00092B14
	private float GetFloatDepth()
	{
		ref Vector3 worldCenterOfMass = this.m_body.worldCenterOfMass;
		float num = Mathf.Max(this.m_waterLevel, this.m_tarLevel);
		return worldCenterOfMass.y - num - this.m_waterLevelOffset;
	}

	// Token: 0x0600168B RID: 5771 RVA: 0x0009494C File Offset: 0x00092B4C
	public bool IsInTar()
	{
		return this.m_tarLevel > -10000f && this.m_body.worldCenterOfMass.y - this.m_tarLevel - this.m_waterLevelOffset < -0.2f;
	}

	// Token: 0x0600168C RID: 5772 RVA: 0x00094984 File Offset: 0x00092B84
	public void SetLiquidLevel(float level, LiquidType type, Component liquidObj)
	{
		if (type != LiquidType.Water && type != LiquidType.Tar)
		{
			return;
		}
		if (type == LiquidType.Water)
		{
			this.m_waterLevel = level;
		}
		else
		{
			this.m_tarLevel = level;
		}
		if (!this.m_beenFloating && level > -10000f && this.GetFloatDepth() < 0f)
		{
			this.m_beenFloating = true;
		}
	}

	// Token: 0x0600168D RID: 5773 RVA: 0x000949D0 File Offset: 0x00092BD0
	public bool BeenFloating()
	{
		return this.m_beenFloating;
	}

	// Token: 0x0600168E RID: 5774 RVA: 0x000949D8 File Offset: 0x00092BD8
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.down * this.m_waterLevelOffset, new Vector3(1f, 0.05f, 1f));
	}

	// Token: 0x0600168F RID: 5775 RVA: 0x00094A28 File Offset: 0x00092C28
	public static float GetLiquidLevel(Vector3 p, float waveFactor = 1f, LiquidType type = LiquidType.All)
	{
		float num = -10000f;
		int num2 = Physics.OverlapSphereNonAlloc(p, 0f, Floating.s_tempColliderArray, Floating.s_waterVolumeMask);
		for (int i = 0; i < num2; i++)
		{
			Collider collider = Floating.s_tempColliderArray[i];
			int instanceID = collider.GetInstanceID();
			WaterVolume component;
			if (!Floating.s_waterVolumeCache.TryGetValue(instanceID, out component))
			{
				component = collider.GetComponent<WaterVolume>();
				Floating.s_waterVolumeCache[instanceID] = component;
			}
			if (component)
			{
				if (type == LiquidType.All || component.GetLiquidType() == type)
				{
					num = Mathf.Max(num, component.GetWaterSurface(p, waveFactor));
				}
			}
			else
			{
				LiquidSurface component2;
				if (!Floating.s_liquidSurfaceCache.TryGetValue(instanceID, out component2))
				{
					component2 = collider.GetComponent<LiquidSurface>();
					Floating.s_liquidSurfaceCache[instanceID] = component2;
				}
				if (component2 && (type == LiquidType.All || component2.GetLiquidType() == type))
				{
					num = Mathf.Max(num, component2.GetSurface(p));
				}
			}
		}
		return num;
	}

	// Token: 0x06001690 RID: 5776 RVA: 0x00094B14 File Offset: 0x00092D14
	public static float GetWaterLevel(Vector3 p, ref WaterVolume previousAndOut)
	{
		if (previousAndOut != null && previousAndOut.gameObject.GetComponent<Collider>().bounds.Contains(p))
		{
			return previousAndOut.GetWaterSurface(p, 1f);
		}
		float num = -10000f;
		int num2 = Physics.OverlapSphereNonAlloc(p, 0f, Floating.s_tempColliderArray, Floating.s_waterVolumeMask);
		for (int i = 0; i < num2; i++)
		{
			Collider collider = Floating.s_tempColliderArray[i];
			int instanceID = collider.GetInstanceID();
			WaterVolume component;
			if (!Floating.s_waterVolumeCache.TryGetValue(instanceID, out component))
			{
				component = collider.GetComponent<WaterVolume>();
				Floating.s_waterVolumeCache[instanceID] = component;
			}
			if (component)
			{
				if (component.GetLiquidType() == LiquidType.Water)
				{
					float waterSurface = component.GetWaterSurface(p, 1f);
					if (waterSurface > num)
					{
						num = waterSurface;
						previousAndOut = component;
					}
				}
			}
			else
			{
				LiquidSurface component2;
				if (!Floating.s_liquidSurfaceCache.TryGetValue(instanceID, out component2))
				{
					component2 = collider.GetComponent<LiquidSurface>();
					Floating.s_liquidSurfaceCache[instanceID] = component2;
				}
				if (component2 && component2.GetLiquidType() == LiquidType.Water)
				{
					num = Mathf.Max(num, component2.GetSurface(p));
				}
			}
		}
		return num;
	}

	// Token: 0x06001691 RID: 5777 RVA: 0x00094C34 File Offset: 0x00092E34
	public int Increment(LiquidType type)
	{
		int[] liquids = this.m_liquids;
		int num = liquids[(int)type] + 1;
		liquids[(int)type] = num;
		return num;
	}

	// Token: 0x06001692 RID: 5778 RVA: 0x00094C58 File Offset: 0x00092E58
	public int Decrement(LiquidType type)
	{
		int[] liquids = this.m_liquids;
		int num = liquids[(int)type] - 1;
		liquids[(int)type] = num;
		return num;
	}

	// Token: 0x170000E6 RID: 230
	// (get) Token: 0x06001693 RID: 5779 RVA: 0x00094C79 File Offset: 0x00092E79
	public static List<Floating> Instances { get; } = new List<Floating>();

	// Token: 0x040017B1 RID: 6065
	public float m_waterLevelOffset;

	// Token: 0x040017B2 RID: 6066
	public float m_forceDistance = 1f;

	// Token: 0x040017B3 RID: 6067
	public float m_force = 0.5f;

	// Token: 0x040017B4 RID: 6068
	public float m_balanceForceFraction = 0.02f;

	// Token: 0x040017B5 RID: 6069
	public float m_damping = 0.05f;

	// Token: 0x040017B6 RID: 6070
	public EffectList m_impactEffects = new EffectList();

	// Token: 0x040017B7 RID: 6071
	public GameObject m_surfaceEffects;

	// Token: 0x040017B8 RID: 6072
	private static int s_waterVolumeMask = 0;

	// Token: 0x040017B9 RID: 6073
	private static readonly Collider[] s_tempColliderArray = new Collider[256];

	// Token: 0x040017BA RID: 6074
	private static readonly Dictionary<int, WaterVolume> s_waterVolumeCache = new Dictionary<int, WaterVolume>();

	// Token: 0x040017BB RID: 6075
	private static readonly Dictionary<int, LiquidSurface> s_liquidSurfaceCache = new Dictionary<int, LiquidSurface>();

	// Token: 0x040017BC RID: 6076
	private float m_waterLevel = -10000f;

	// Token: 0x040017BD RID: 6077
	private float m_tarLevel = -10000f;

	// Token: 0x040017BE RID: 6078
	private bool m_beenFloating;

	// Token: 0x040017BF RID: 6079
	private bool m_wasInWater = true;

	// Token: 0x040017C0 RID: 6080
	private const float c_MinImpactEffectVelocity = 0.5f;

	// Token: 0x040017C1 RID: 6081
	private Rigidbody m_body;

	// Token: 0x040017C2 RID: 6082
	private Collider m_collider;

	// Token: 0x040017C3 RID: 6083
	private ZNetView m_nview;

	// Token: 0x040017C4 RID: 6084
	private readonly int[] m_liquids = new int[2];
}
