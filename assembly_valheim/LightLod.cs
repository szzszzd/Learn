using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200025D RID: 605
public class LightLod : MonoBehaviour
{
	// Token: 0x06001759 RID: 5977 RVA: 0x0009A990 File Offset: 0x00098B90
	private void Awake()
	{
		this.m_light = base.GetComponent<Light>();
		this.m_baseRange = this.m_light.range;
		this.m_baseShadowStrength = this.m_light.shadowStrength;
		if (this.m_shadowLod && this.m_light.shadows == LightShadows.None)
		{
			this.m_shadowLod = false;
		}
		if (this.m_lightLod)
		{
			this.m_light.range = 0f;
			this.m_light.enabled = false;
		}
		if (this.m_shadowLod)
		{
			this.m_light.shadowStrength = 0f;
			this.m_light.shadows = LightShadows.None;
		}
		LightLod.m_lights.Add(this);
	}

	// Token: 0x0600175A RID: 5978 RVA: 0x00084517 File Offset: 0x00082717
	private void OnEnable()
	{
		base.StartCoroutine("UpdateLoop");
	}

	// Token: 0x0600175B RID: 5979 RVA: 0x0009AA3B File Offset: 0x00098C3B
	private void OnDestroy()
	{
		LightLod.m_lights.Remove(this);
	}

	// Token: 0x0600175C RID: 5980 RVA: 0x0009AA49 File Offset: 0x00098C49
	private IEnumerator UpdateLoop()
	{
		for (;;)
		{
			if (Utils.GetMainCamera() && this.m_light)
			{
				Vector3 lightReferencePoint = LightLod.GetLightReferencePoint();
				float distance = Vector3.Distance(lightReferencePoint, base.transform.position);
				if (this.m_lightLod)
				{
					if (distance < this.m_lightDistance)
					{
						if (this.m_lightPrio >= LightLod.m_lightLimit)
						{
							if (LightLod.m_lightLimit >= 0)
							{
								goto IL_192;
							}
						}
						while (this.m_light)
						{
							if (this.m_light.range >= this.m_baseRange && this.m_light.enabled)
							{
								break;
							}
							this.m_light.enabled = true;
							this.m_light.range = Mathf.Min(this.m_baseRange, this.m_light.range + Time.deltaTime * this.m_baseRange);
							yield return null;
						}
						goto IL_1C4;
					}
					IL_192:
					while (this.m_light && (this.m_light.range > 0f || this.m_light.enabled))
					{
						this.m_light.range = Mathf.Max(0f, this.m_light.range - Time.deltaTime * this.m_baseRange);
						if (this.m_light.range <= 0f)
						{
							this.m_light.enabled = false;
						}
						yield return null;
					}
				}
				IL_1C4:
				if (this.m_shadowLod)
				{
					if (distance < this.m_shadowDistance)
					{
						if (this.m_lightPrio >= LightLod.m_shadowLimit)
						{
							if (LightLod.m_shadowLimit >= 0)
							{
								goto IL_2E5;
							}
						}
						while (this.m_light)
						{
							if (this.m_light.shadowStrength >= this.m_baseShadowStrength && this.m_light.shadows != LightShadows.None)
							{
								break;
							}
							this.m_light.shadows = LightShadows.Soft;
							this.m_light.shadowStrength = Mathf.Min(this.m_baseShadowStrength, this.m_light.shadowStrength + Time.deltaTime * this.m_baseShadowStrength);
							yield return null;
						}
						goto IL_317;
					}
					IL_2E5:
					while (this.m_light && (this.m_light.shadowStrength > 0f || this.m_light.shadows != LightShadows.None))
					{
						this.m_light.shadowStrength = Mathf.Max(0f, this.m_light.shadowStrength - Time.deltaTime * this.m_baseShadowStrength);
						if (this.m_light.shadowStrength <= 0f)
						{
							this.m_light.shadows = LightShadows.None;
						}
						yield return null;
					}
				}
			}
			IL_317:
			yield return new WaitForSeconds(1f);
		}
		yield break;
	}

	// Token: 0x0600175D RID: 5981 RVA: 0x0009AA58 File Offset: 0x00098C58
	private static Vector3 GetLightReferencePoint()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (GameCamera.InFreeFly() || Player.m_localPlayer == null)
		{
			return mainCamera.transform.position;
		}
		return Player.m_localPlayer.transform.position;
	}

	// Token: 0x0600175E RID: 5982 RVA: 0x0009AA9C File Offset: 0x00098C9C
	public static void UpdateLights(float dt)
	{
		if (LightLod.m_lightLimit < 0 && LightLod.m_shadowLimit < 0)
		{
			return;
		}
		LightLod.m_updateTimer += dt;
		if (LightLod.m_updateTimer < 1f)
		{
			return;
		}
		LightLod.m_updateTimer = 0f;
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		Vector3 lightReferencePoint = LightLod.GetLightReferencePoint();
		LightLod.m_sortedLights.Clear();
		foreach (LightLod lightLod in LightLod.m_lights)
		{
			if (lightLod.enabled && lightLod.m_light && lightLod.m_light.type == LightType.Point)
			{
				lightLod.m_cameraDistanceOuter = Vector3.Distance(lightReferencePoint, lightLod.transform.position) - lightLod.m_lightDistance * 0.25f;
				LightLod.m_sortedLights.Add(lightLod);
			}
		}
		LightLod.m_sortedLights.Sort((LightLod a, LightLod b) => a.m_cameraDistanceOuter.CompareTo(b.m_cameraDistanceOuter));
		for (int i = 0; i < LightLod.m_sortedLights.Count; i++)
		{
			LightLod.m_sortedLights[i].m_lightPrio = i;
		}
	}

	// Token: 0x0600175F RID: 5983 RVA: 0x0009ABDC File Offset: 0x00098DDC
	private void OnDrawGizmosSelected()
	{
		if (this.m_lightLod)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, this.m_lightDistance);
		}
		if (this.m_shadowLod)
		{
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(base.transform.position, this.m_shadowDistance);
		}
	}

	// Token: 0x040018BD RID: 6333
	private static HashSet<LightLod> m_lights = new HashSet<LightLod>();

	// Token: 0x040018BE RID: 6334
	private static List<LightLod> m_sortedLights = new List<LightLod>();

	// Token: 0x040018BF RID: 6335
	public static int m_lightLimit = -1;

	// Token: 0x040018C0 RID: 6336
	public static int m_shadowLimit = -1;

	// Token: 0x040018C1 RID: 6337
	public bool m_lightLod = true;

	// Token: 0x040018C2 RID: 6338
	public float m_lightDistance = 40f;

	// Token: 0x040018C3 RID: 6339
	public bool m_shadowLod = true;

	// Token: 0x040018C4 RID: 6340
	public float m_shadowDistance = 20f;

	// Token: 0x040018C5 RID: 6341
	private const float m_lightSizeWeight = 0.25f;

	// Token: 0x040018C6 RID: 6342
	private static float m_updateTimer = 0f;

	// Token: 0x040018C7 RID: 6343
	private int m_lightPrio;

	// Token: 0x040018C8 RID: 6344
	private float m_cameraDistanceOuter;

	// Token: 0x040018C9 RID: 6345
	private Light m_light;

	// Token: 0x040018CA RID: 6346
	private float m_baseRange;

	// Token: 0x040018CB RID: 6347
	private float m_baseShadowStrength;
}
