using System;
using UnityEngine;

// Token: 0x020001BB RID: 443
public class DepthCamera : MonoBehaviour
{
	// Token: 0x060011AE RID: 4526 RVA: 0x000748EB File Offset: 0x00072AEB
	private void Start()
	{
		this.m_camera = base.GetComponent<Camera>();
		base.InvokeRepeating("RenderDepth", this.m_updateInterval, this.m_updateInterval);
	}

	// Token: 0x060011AF RID: 4527 RVA: 0x00074910 File Offset: 0x00072B10
	private void RenderDepth()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 vector = (Player.m_localPlayer ? Player.m_localPlayer.transform.position : mainCamera.transform.position) + Vector3.up * this.m_offset;
		vector.x = Mathf.Round(vector.x);
		vector.y = Mathf.Round(vector.y);
		vector.z = Mathf.Round(vector.z);
		base.transform.position = vector;
		float lodBias = QualitySettings.lodBias;
		QualitySettings.lodBias = 10f;
		this.m_camera.RenderWithShader(this.m_depthShader, "RenderType");
		QualitySettings.lodBias = lodBias;
		Shader.SetGlobalTexture("_SkyAlphaTexture", this.m_texture);
		Shader.SetGlobalVector("_SkyAlphaPosition", base.transform.position);
	}

	// Token: 0x0400125E RID: 4702
	public Shader m_depthShader;

	// Token: 0x0400125F RID: 4703
	public float m_offset = 50f;

	// Token: 0x04001260 RID: 4704
	public RenderTexture m_texture;

	// Token: 0x04001261 RID: 4705
	public float m_updateInterval = 1f;

	// Token: 0x04001262 RID: 4706
	private Camera m_camera;
}
