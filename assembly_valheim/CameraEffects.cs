using System;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

// Token: 0x0200006B RID: 107
public class CameraEffects : MonoBehaviour
{
	// Token: 0x17000010 RID: 16
	// (get) Token: 0x0600055D RID: 1373 RVA: 0x0002A0A3 File Offset: 0x000282A3
	public static CameraEffects instance
	{
		get
		{
			return CameraEffects.m_instance;
		}
	}

	// Token: 0x0600055E RID: 1374 RVA: 0x0002A0AA File Offset: 0x000282AA
	private void Awake()
	{
		CameraEffects.m_instance = this;
		this.m_postProcessing = base.GetComponent<PostProcessingBehaviour>();
		this.m_dof = base.GetComponent<DepthOfField>();
		this.ApplySettings();
	}

	// Token: 0x0600055F RID: 1375 RVA: 0x0002A0D0 File Offset: 0x000282D0
	private void OnDestroy()
	{
		if (CameraEffects.m_instance == this)
		{
			CameraEffects.m_instance = null;
		}
	}

	// Token: 0x06000560 RID: 1376 RVA: 0x0002A0E8 File Offset: 0x000282E8
	public void ApplySettings()
	{
		this.SetDof(PlatformPrefs.GetInt("DOF", 1) == 1);
		this.SetBloom(PlatformPrefs.GetInt("Bloom", 1) == 1);
		this.SetSSAO(PlatformPrefs.GetInt("SSAO", 1) == 1);
		this.SetSunShafts(PlatformPrefs.GetInt("SunShafts", 1) == 1);
		this.SetAntiAliasing(PlatformPrefs.GetInt("AntiAliasing", 1) == 1);
		this.SetCA(PlatformPrefs.GetInt("ChromaticAberration", 1) == 1);
		this.SetMotionBlur(PlatformPrefs.GetInt("MotionBlur", 1) == 1);
	}

	// Token: 0x06000561 RID: 1377 RVA: 0x0002A1A0 File Offset: 0x000283A0
	public void SetSunShafts(bool enabled)
	{
		SunShafts component = base.GetComponent<SunShafts>();
		if (component != null)
		{
			component.enabled = enabled;
		}
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x0002A1C4 File Offset: 0x000283C4
	private void SetBloom(bool enabled)
	{
		this.m_postProcessing.profile.bloom.enabled = enabled;
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x0002A1DC File Offset: 0x000283DC
	private void SetSSAO(bool enabled)
	{
		this.m_postProcessing.profile.ambientOcclusion.enabled = enabled;
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x0002A1F4 File Offset: 0x000283F4
	private void SetMotionBlur(bool enabled)
	{
		this.m_postProcessing.profile.motionBlur.enabled = enabled;
	}

	// Token: 0x06000565 RID: 1381 RVA: 0x0002A20C File Offset: 0x0002840C
	private void SetAntiAliasing(bool enabled)
	{
		this.m_postProcessing.profile.antialiasing.enabled = enabled;
	}

	// Token: 0x06000566 RID: 1382 RVA: 0x0002A224 File Offset: 0x00028424
	private void SetCA(bool enabled)
	{
		this.m_postProcessing.profile.chromaticAberration.enabled = enabled;
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x0002A23C File Offset: 0x0002843C
	private void SetDof(bool enabled)
	{
		this.m_dof.enabled = (enabled || this.m_forceDof);
	}

	// Token: 0x06000568 RID: 1384 RVA: 0x0002A255 File Offset: 0x00028455
	private void LateUpdate()
	{
		this.UpdateDOF();
	}

	// Token: 0x06000569 RID: 1385 RVA: 0x0002A25D File Offset: 0x0002845D
	private bool ControllingShip()
	{
		return Player.m_localPlayer == null || Player.m_localPlayer.GetControlledShip() != null;
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x0002A284 File Offset: 0x00028484
	private void UpdateDOF()
	{
		if (!this.m_dof.enabled || !this.m_dofAutoFocus)
		{
			return;
		}
		float num = this.m_dofMaxDistance;
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit, this.m_dofMaxDistance, this.m_dofRayMask))
		{
			num = raycastHit.distance;
		}
		if (this.ControllingShip() && num < this.m_dofMinDistanceShip)
		{
			num = this.m_dofMinDistanceShip;
		}
		if (num < this.m_dofMinDistance)
		{
			num = this.m_dofMinDistance;
		}
		this.m_dof.focalLength = Mathf.Lerp(this.m_dof.focalLength, num, 0.2f);
	}

	// Token: 0x04000640 RID: 1600
	private static CameraEffects m_instance;

	// Token: 0x04000641 RID: 1601
	public bool m_forceDof;

	// Token: 0x04000642 RID: 1602
	public LayerMask m_dofRayMask;

	// Token: 0x04000643 RID: 1603
	public bool m_dofAutoFocus;

	// Token: 0x04000644 RID: 1604
	public float m_dofMinDistance = 50f;

	// Token: 0x04000645 RID: 1605
	public float m_dofMinDistanceShip = 50f;

	// Token: 0x04000646 RID: 1606
	public float m_dofMaxDistance = 3000f;

	// Token: 0x04000647 RID: 1607
	private PostProcessingBehaviour m_postProcessing;

	// Token: 0x04000648 RID: 1608
	private DepthOfField m_dof;
}
