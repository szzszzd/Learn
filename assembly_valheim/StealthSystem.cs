using System;
using UnityEngine;

// Token: 0x02000201 RID: 513
public class StealthSystem : MonoBehaviour
{
	// Token: 0x170000DF RID: 223
	// (get) Token: 0x06001484 RID: 5252 RVA: 0x00085C7E File Offset: 0x00083E7E
	public static StealthSystem instance
	{
		get
		{
			return StealthSystem.m_instance;
		}
	}

	// Token: 0x06001485 RID: 5253 RVA: 0x00085C85 File Offset: 0x00083E85
	private void Awake()
	{
		StealthSystem.m_instance = this;
	}

	// Token: 0x06001486 RID: 5254 RVA: 0x00085C8D File Offset: 0x00083E8D
	private void OnDestroy()
	{
		StealthSystem.m_instance = null;
	}

	// Token: 0x06001487 RID: 5255 RVA: 0x00085C98 File Offset: 0x00083E98
	public float GetLightFactor(Vector3 point)
	{
		float lightLevel = this.GetLightLevel(point);
		return Utils.LerpStep(this.m_minLightLevel, this.m_maxLightLevel, lightLevel);
	}

	// Token: 0x06001488 RID: 5256 RVA: 0x00085CC0 File Offset: 0x00083EC0
	public float GetLightLevel(Vector3 point)
	{
		if (Time.time - this.m_lastLightListUpdate > 1f)
		{
			this.m_lastLightListUpdate = Time.time;
			this.m_allLights = UnityEngine.Object.FindObjectsOfType<Light>();
		}
		float num = RenderSettings.ambientIntensity * RenderSettings.ambientLight.grayscale;
		foreach (Light light in this.m_allLights)
		{
			if (!(light == null))
			{
				if (light.type == LightType.Directional)
				{
					float num2 = 1f;
					if (light.shadows != LightShadows.None && (Physics.Raycast(point - light.transform.forward * 1000f, light.transform.forward, 1000f, this.m_shadowTestMask) || Physics.Raycast(point, -light.transform.forward, 1000f, this.m_shadowTestMask)))
					{
						num2 = 1f - light.shadowStrength;
					}
					float num3 = light.intensity * light.color.grayscale * num2;
					num += num3;
				}
				else
				{
					float num4 = Vector3.Distance(light.transform.position, point);
					if (num4 <= light.range)
					{
						float num5 = 1f;
						if (light.shadows != LightShadows.None)
						{
							Vector3 vector = point - light.transform.position;
							if (Physics.Raycast(light.transform.position, vector.normalized, vector.magnitude, this.m_shadowTestMask) || Physics.Raycast(point, -vector.normalized, vector.magnitude, this.m_shadowTestMask))
							{
								num5 = 1f - light.shadowStrength;
							}
						}
						float num6 = 1f - num4 / light.range;
						float num7 = light.intensity * light.color.grayscale * num6 * num5;
						num += num7;
					}
				}
			}
		}
		return num;
	}

	// Token: 0x0400153D RID: 5437
	private static StealthSystem m_instance;

	// Token: 0x0400153E RID: 5438
	public LayerMask m_shadowTestMask;

	// Token: 0x0400153F RID: 5439
	public float m_minLightLevel = 0.2f;

	// Token: 0x04001540 RID: 5440
	public float m_maxLightLevel = 1.6f;

	// Token: 0x04001541 RID: 5441
	private Light[] m_allLights;

	// Token: 0x04001542 RID: 5442
	private float m_lastLightListUpdate;

	// Token: 0x04001543 RID: 5443
	private const float m_lightUpdateInterval = 1f;
}
