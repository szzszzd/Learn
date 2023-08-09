using System;
using UnityEngine;

// Token: 0x02000266 RID: 614
[ExecuteInEditMode]
public class MenuScene : MonoBehaviour
{
	// Token: 0x060017B3 RID: 6067 RVA: 0x0009D9A8 File Offset: 0x0009BBA8
	private void Awake()
	{
		Shader.SetGlobalFloat("_Wet", 0f);
	}

	// Token: 0x060017B4 RID: 6068 RVA: 0x0009D9BC File Offset: 0x0009BBBC
	private void Update()
	{
		Shader.SetGlobalVector("_SkyboxSunDir", -this.m_dirLight.transform.forward);
		Shader.SetGlobalVector("_SunDir", -this.m_dirLight.transform.forward);
		Shader.SetGlobalColor("_SunFogColor", this.m_sunFogColor);
		Shader.SetGlobalColor("_SunColor", this.m_dirLight.color * this.m_dirLight.intensity);
		Shader.SetGlobalColor("_AmbientColor", RenderSettings.ambientLight);
		RenderSettings.fogColor = this.m_fogColor;
		RenderSettings.fogDensity = this.m_fogDensity;
		RenderSettings.ambientLight = this.m_ambientLightColor;
		Vector3 normalized = this.m_windDir.normalized;
		Shader.SetGlobalVector("_GlobalWindForce", normalized * this.m_windIntensity);
		Shader.SetGlobalVector("_GlobalWind1", new Vector4(normalized.x, normalized.y, normalized.z, this.m_windIntensity));
		Shader.SetGlobalVector("_GlobalWind2", Vector4.one);
		Shader.SetGlobalFloat("_GlobalWindAlpha", 0f);
	}

	// Token: 0x04001924 RID: 6436
	public Light m_dirLight;

	// Token: 0x04001925 RID: 6437
	public Color m_sunFogColor = Color.white;

	// Token: 0x04001926 RID: 6438
	public Color m_fogColor = Color.white;

	// Token: 0x04001927 RID: 6439
	public Color m_ambientLightColor = Color.white;

	// Token: 0x04001928 RID: 6440
	public float m_fogDensity = 1f;

	// Token: 0x04001929 RID: 6441
	public Vector3 m_windDir = Vector3.left;

	// Token: 0x0400192A RID: 6442
	public float m_windIntensity = 0.5f;
}
