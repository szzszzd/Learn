using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002BE RID: 702
public class WaterVolume : MonoBehaviour
{
	// Token: 0x06001A88 RID: 6792 RVA: 0x000B0180 File Offset: 0x000AE380
	private void Awake()
	{
		this.m_collider = base.GetComponent<Collider>();
		if (WaterVolume.s_createWaveTangents == null)
		{
			WaterVolume.s_createWaveTangents = new Vector2[]
			{
				new Vector2(-WaterVolume.s_createWaveDirections[0].y, WaterVolume.s_createWaveDirections[0].x),
				new Vector2(-WaterVolume.s_createWaveDirections[1].y, WaterVolume.s_createWaveDirections[1].x),
				new Vector2(-WaterVolume.s_createWaveDirections[2].y, WaterVolume.s_createWaveDirections[2].x),
				new Vector2(-WaterVolume.s_createWaveDirections[3].y, WaterVolume.s_createWaveDirections[3].x),
				new Vector2(-WaterVolume.s_createWaveDirections[4].y, WaterVolume.s_createWaveDirections[4].x),
				new Vector2(-WaterVolume.s_createWaveDirections[5].y, WaterVolume.s_createWaveDirections[5].x),
				new Vector2(-WaterVolume.s_createWaveDirections[6].y, WaterVolume.s_createWaveDirections[6].x),
				new Vector2(-WaterVolume.s_createWaveDirections[7].y, WaterVolume.s_createWaveDirections[7].x),
				new Vector2(-WaterVolume.s_createWaveDirections[8].y, WaterVolume.s_createWaveDirections[8].x),
				new Vector2(-WaterVolume.s_createWaveDirections[9].y, WaterVolume.s_createWaveDirections[9].x)
			};
		}
	}

	// Token: 0x06001A89 RID: 6793 RVA: 0x000B0374 File Offset: 0x000AE574
	private void Start()
	{
		this.DetectWaterDepth();
		this.SetupMaterial();
	}

	// Token: 0x06001A8A RID: 6794 RVA: 0x000B0382 File Offset: 0x000AE582
	private void OnEnable()
	{
		WaterVolume.Instances.Add(this);
	}

	// Token: 0x06001A8B RID: 6795 RVA: 0x000B038F File Offset: 0x000AE58F
	private void OnDisable()
	{
		WaterVolume.Instances.Remove(this);
	}

	// Token: 0x06001A8C RID: 6796 RVA: 0x000B03A0 File Offset: 0x000AE5A0
	private void DetectWaterDepth()
	{
		if (this.m_heightmap)
		{
			float[] oceanDepth = this.m_heightmap.GetOceanDepth();
			this.m_normalizedDepth[0] = Mathf.Clamp01(oceanDepth[0] / 10f);
			this.m_normalizedDepth[1] = Mathf.Clamp01(oceanDepth[1] / 10f);
			this.m_normalizedDepth[2] = Mathf.Clamp01(oceanDepth[2] / 10f);
			this.m_normalizedDepth[3] = Mathf.Clamp01(oceanDepth[3] / 10f);
			return;
		}
		this.m_normalizedDepth[0] = this.m_forceDepth;
		this.m_normalizedDepth[1] = this.m_forceDepth;
		this.m_normalizedDepth[2] = this.m_forceDepth;
		this.m_normalizedDepth[3] = this.m_forceDepth;
	}

	// Token: 0x06001A8D RID: 6797 RVA: 0x000B0457 File Offset: 0x000AE657
	public static void StaticUpdate()
	{
		WaterVolume.UpdateWaterTime(Time.deltaTime);
		if (EnvMan.instance)
		{
			EnvMan.instance.GetWindData(out WaterVolume.s_globalWind1, out WaterVolume.s_globalWind2, out WaterVolume.s_globalWindAlpha);
		}
	}

	// Token: 0x06001A8E RID: 6798 RVA: 0x000B0488 File Offset: 0x000AE688
	public void Update1()
	{
		this.UpdateFloaters();
	}

	// Token: 0x06001A8F RID: 6799 RVA: 0x000B0490 File Offset: 0x000AE690
	public void Update2()
	{
		this.m_waterSurface.material.SetFloat(WaterVolume.s_shaderWaterTime, WaterVolume.s_waterTime);
	}

	// Token: 0x06001A90 RID: 6800 RVA: 0x000B04AC File Offset: 0x000AE6AC
	private static void UpdateWaterTime(float dt)
	{
		WaterVolume.s_wrappedDayTimeSeconds = ZNet.instance.GetWrappedDayTimeSeconds();
		float num = WaterVolume.s_wrappedDayTimeSeconds;
		WaterVolume.s_waterTime += dt;
		if (Mathf.Abs(num - WaterVolume.s_waterTime) > 10f)
		{
			WaterVolume.s_waterTime = num;
		}
		WaterVolume.s_waterTime = Mathf.Lerp(WaterVolume.s_waterTime, num, 0.05f);
	}

	// Token: 0x06001A91 RID: 6801 RVA: 0x000B0508 File Offset: 0x000AE708
	private void SetupMaterial()
	{
		if (this.m_forceDepth >= 0f)
		{
			this.m_waterSurface.material.SetFloatArray(WaterVolume.s_shaderDepth, new float[]
			{
				this.m_forceDepth,
				this.m_forceDepth,
				this.m_forceDepth,
				this.m_forceDepth
			});
		}
		else
		{
			this.m_waterSurface.material.SetFloatArray(WaterVolume.s_shaderDepth, this.m_normalizedDepth);
		}
		this.m_waterSurface.material.SetFloat(WaterVolume.s_shaderUseGlobalWind, this.m_useGlobalWind ? 1f : 0f);
	}

	// Token: 0x06001A92 RID: 6802 RVA: 0x0000247B File Offset: 0x0000067B
	public LiquidType GetLiquidType()
	{
		return LiquidType.Water;
	}

	// Token: 0x06001A93 RID: 6803 RVA: 0x000B05A8 File Offset: 0x000AE7A8
	public float GetWaterSurface(Vector3 point, float waveFactor = 1f)
	{
		float waterTime = WaterVolume.s_wrappedDayTimeSeconds;
		float num = this.Depth(point);
		float num2 = (num == 0f) ? 0f : this.CalcWave(point, num, waterTime, waveFactor);
		float num3 = base.transform.position.y + num2 + this.m_surfaceOffset;
		if (this.m_forceDepth < 0f && Utils.LengthXZ(point) > 10500f)
		{
			num3 -= 100f;
		}
		return num3;
	}

	// Token: 0x06001A94 RID: 6804 RVA: 0x000B061A File Offset: 0x000AE81A
	private float TrochSin(float x, float k)
	{
		return Mathf.Sin(x - Mathf.Cos(x) * k) * 0.5f + 0.5f;
	}

	// Token: 0x06001A95 RID: 6805 RVA: 0x000B0638 File Offset: 0x000AE838
	private float CreateWave(Vector3 worldPos, float time, float waveSpeed, float waveLength, float waveHeight, Vector2 dir, Vector2 tangent, float sharpness)
	{
		Vector2 vector = -(worldPos.z * dir + worldPos.x * tangent);
		float num = time * waveSpeed;
		return (this.TrochSin(num + vector.y * waveLength, sharpness) * this.TrochSin(num * 0.123f + vector.x * 0.13123f * waveLength, sharpness) - 0.2f) * waveHeight;
	}

	// Token: 0x06001A96 RID: 6806 RVA: 0x000B06AC File Offset: 0x000AE8AC
	private float CalcWave(Vector3 worldPos, float depth, Vector4 wind, float waterTime, float waveFactor)
	{
		WaterVolume.s_createWaveDirections[0].x = wind.x;
		WaterVolume.s_createWaveDirections[0].y = wind.z;
		WaterVolume.s_createWaveDirections[0].Normalize();
		WaterVolume.s_createWaveTangents[0].x = -WaterVolume.s_createWaveDirections[0].y;
		WaterVolume.s_createWaveTangents[0].y = WaterVolume.s_createWaveDirections[0].x;
		float w = wind.w;
		float num = Mathf.LerpUnclamped(0f, w, depth);
		float time = waterTime / 20f;
		float num2 = this.CreateWave(worldPos, time, 10f, 0.04f, 8f, WaterVolume.s_createWaveDirections[0], WaterVolume.s_createWaveTangents[0], 0.5f);
		float num3 = this.CreateWave(worldPos, time, 14.123f, 0.08f, 6f, WaterVolume.s_createWaveDirections[1], WaterVolume.s_createWaveTangents[1], 0.5f);
		float num4 = this.CreateWave(worldPos, time, 22.312f, 0.1f, 4f, WaterVolume.s_createWaveDirections[2], WaterVolume.s_createWaveTangents[2], 0.5f);
		float num5 = this.CreateWave(worldPos, time, 31.42f, 0.2f, 2f, WaterVolume.s_createWaveDirections[3], WaterVolume.s_createWaveTangents[3], 0.5f);
		float num6 = this.CreateWave(worldPos, time, 35.42f, 0.4f, 1f, WaterVolume.s_createWaveDirections[4], WaterVolume.s_createWaveTangents[4], 0.5f);
		float num7 = this.CreateWave(worldPos, time, 38.1223f, 1f, 0.8f, WaterVolume.s_createWaveDirections[5], WaterVolume.s_createWaveTangents[5], 0.7f);
		float num8 = this.CreateWave(worldPos, time, 41.1223f, 1.2f, 0.6f * waveFactor, WaterVolume.s_createWaveDirections[6], WaterVolume.s_createWaveTangents[6], 0.8f);
		float num9 = this.CreateWave(worldPos, time, 51.5123f, 1.3f, 0.4f * waveFactor, WaterVolume.s_createWaveDirections[7], WaterVolume.s_createWaveTangents[7], 0.9f);
		float num10 = this.CreateWave(worldPos, time, 54.2f, 1.3f, 0.3f * waveFactor, WaterVolume.s_createWaveDirections[8], WaterVolume.s_createWaveTangents[8], 0.9f);
		float num11 = this.CreateWave(worldPos, time, 56.123f, 1.5f, 0.2f * waveFactor, WaterVolume.s_createWaveDirections[9], WaterVolume.s_createWaveTangents[9], 0.9f);
		return (num2 + num3 + num4 + num5 + num6 + num7 + num8 + num9 + num10 + num11) * num;
	}

	// Token: 0x06001A97 RID: 6807 RVA: 0x000B0984 File Offset: 0x000AEB84
	public float CalcWave(Vector3 worldPos, float depth, float waterTime, float waveFactor)
	{
		if (WaterVolume.s_globalWindAlpha == 0f)
		{
			return this.CalcWave(worldPos, depth, WaterVolume.s_globalWind1, waterTime, waveFactor);
		}
		float a = this.CalcWave(worldPos, depth, WaterVolume.s_globalWind1, waterTime, waveFactor);
		float b = this.CalcWave(worldPos, depth, WaterVolume.s_globalWind2, waterTime, waveFactor);
		return Mathf.LerpUnclamped(a, b, WaterVolume.s_globalWindAlpha);
	}

	// Token: 0x06001A98 RID: 6808 RVA: 0x000B09DC File Offset: 0x000AEBDC
	public float Depth(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point);
		float t = (vector.x + this.m_collider.bounds.size.x / 2f) / this.m_collider.bounds.size.x;
		float t2 = (vector.z + this.m_collider.bounds.size.z / 2f) / this.m_collider.bounds.size.z;
		float a = Mathf.Lerp(this.m_normalizedDepth[3], this.m_normalizedDepth[2], t);
		float b = Mathf.Lerp(this.m_normalizedDepth[0], this.m_normalizedDepth[1], t);
		return Mathf.Lerp(a, b, t2);
	}

	// Token: 0x06001A99 RID: 6809 RVA: 0x000B0AA8 File Offset: 0x000AECA8
	private void OnTriggerEnter(Collider triggerCollider)
	{
		IWaterInteractable component = triggerCollider.attachedRigidbody.GetComponent<IWaterInteractable>();
		if (component == null)
		{
			return;
		}
		component.Increment(LiquidType.Water);
		if (!this.m_inWater.Contains(component))
		{
			this.m_inWater.Add(component);
		}
	}

	// Token: 0x06001A9A RID: 6810 RVA: 0x000B0AE8 File Offset: 0x000AECE8
	private void UpdateFloaters()
	{
		if (this.m_inWater.Count == 0)
		{
			return;
		}
		WaterVolume.s_inWaterRemoveIndices.Clear();
		for (int i = 0; i < this.m_inWater.Count; i++)
		{
			IWaterInteractable waterInteractable = this.m_inWater[i];
			if (waterInteractable == null)
			{
				WaterVolume.s_inWaterRemoveIndices.Add(i);
			}
			else
			{
				Transform transform = waterInteractable.GetTransform();
				if (transform)
				{
					float waterSurface = this.GetWaterSurface(transform.position, 1f);
					waterInteractable.SetLiquidLevel(waterSurface, LiquidType.Water, this);
				}
				else
				{
					WaterVolume.s_inWaterRemoveIndices.Add(i);
				}
			}
		}
		for (int j = WaterVolume.s_inWaterRemoveIndices.Count - 1; j >= 0; j--)
		{
			this.m_inWater.RemoveAt(WaterVolume.s_inWaterRemoveIndices[j]);
		}
	}

	// Token: 0x06001A9B RID: 6811 RVA: 0x000B0BAC File Offset: 0x000AEDAC
	private void OnTriggerExit(Collider triggerCollider)
	{
		IWaterInteractable component = triggerCollider.attachedRigidbody.GetComponent<IWaterInteractable>();
		if (component == null)
		{
			return;
		}
		if (component.Decrement(LiquidType.Water) == 0)
		{
			component.SetLiquidLevel(-10000f, LiquidType.Water, this);
		}
		this.m_inWater.Remove(component);
	}

	// Token: 0x06001A9C RID: 6812 RVA: 0x000B0BEC File Offset: 0x000AEDEC
	private void OnDestroy()
	{
		foreach (IWaterInteractable waterInteractable in this.m_inWater)
		{
			if (waterInteractable != null && waterInteractable.Decrement(LiquidType.Water) == 0)
			{
				waterInteractable.SetLiquidLevel(-10000f, LiquidType.Water, this);
			}
		}
		this.m_inWater.Clear();
	}

	// Token: 0x06001A9D RID: 6813 RVA: 0x000B0C5C File Offset: 0x000AEE5C
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_surfaceOffset, new Vector3(2f, 0.05f, 2f));
	}

	// Token: 0x170000F9 RID: 249
	// (get) Token: 0x06001A9E RID: 6814 RVA: 0x000B0CAC File Offset: 0x000AEEAC
	public static List<WaterVolume> Instances { get; } = new List<WaterVolume>();

	// Token: 0x04001CA4 RID: 7332
	private Collider m_collider;

	// Token: 0x04001CA5 RID: 7333
	private readonly float[] m_normalizedDepth = new float[4];

	// Token: 0x04001CA6 RID: 7334
	private readonly List<IWaterInteractable> m_inWater = new List<IWaterInteractable>();

	// Token: 0x04001CA7 RID: 7335
	public MeshRenderer m_waterSurface;

	// Token: 0x04001CA8 RID: 7336
	public Heightmap m_heightmap;

	// Token: 0x04001CA9 RID: 7337
	public float m_forceDepth = -1f;

	// Token: 0x04001CAA RID: 7338
	public float m_surfaceOffset;

	// Token: 0x04001CAB RID: 7339
	public bool m_useGlobalWind = true;

	// Token: 0x04001CAC RID: 7340
	private const bool c_MenuWater = false;

	// Token: 0x04001CAD RID: 7341
	private static float s_waterTime = 0f;

	// Token: 0x04001CAE RID: 7342
	private static readonly int s_shaderWaterTime = Shader.PropertyToID("_WaterTime");

	// Token: 0x04001CAF RID: 7343
	private static readonly int s_shaderDepth = Shader.PropertyToID("_depth");

	// Token: 0x04001CB0 RID: 7344
	private static readonly int s_shaderUseGlobalWind = Shader.PropertyToID("_UseGlobalWind");

	// Token: 0x04001CB1 RID: 7345
	private static Vector4 s_globalWind1 = new Vector4(1f, 0f, 0f, 0f);

	// Token: 0x04001CB2 RID: 7346
	private static Vector4 s_globalWind2 = new Vector4(1f, 0f, 0f, 0f);

	// Token: 0x04001CB3 RID: 7347
	private static float s_globalWindAlpha = 0f;

	// Token: 0x04001CB4 RID: 7348
	private static float s_wrappedDayTimeSeconds = 0f;

	// Token: 0x04001CB5 RID: 7349
	private static readonly List<int> s_inWaterRemoveIndices = new List<int>();

	// Token: 0x04001CB6 RID: 7350
	private static readonly Vector2[] s_createWaveDirections = new Vector2[]
	{
		new Vector2(1.0312f, 0.312f).normalized,
		new Vector2(1.0312f, 0.312f).normalized,
		new Vector2(-0.123f, 1.12f).normalized,
		new Vector2(0.423f, 0.124f).normalized,
		new Vector2(0.123f, -0.64f).normalized,
		new Vector2(-0.523f, -0.64f).normalized,
		new Vector2(0.223f, 0.74f).normalized,
		new Vector2(0.923f, -0.24f).normalized,
		new Vector2(-0.323f, 0.44f).normalized,
		new Vector2(0.5312f, -0.812f).normalized
	};

	// Token: 0x04001CB7 RID: 7351
	private static Vector2[] s_createWaveTangents = null;
}
