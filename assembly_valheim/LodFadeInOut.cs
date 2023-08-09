using System;
using UnityEngine;

// Token: 0x02000263 RID: 611
public class LodFadeInOut : MonoBehaviour
{
	// Token: 0x0600179F RID: 6047 RVA: 0x0009D1AC File Offset: 0x0009B3AC
	private void Awake()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (Vector3.Distance(mainCamera.transform.position, base.transform.position) > 20f)
		{
			this.m_lodGroup = base.GetComponent<LODGroup>();
			if (this.m_lodGroup)
			{
				this.m_originalLocalRef = this.m_lodGroup.localReferencePoint;
				this.m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
				base.Invoke("FadeIn", UnityEngine.Random.Range(0.1f, 0.3f));
			}
		}
	}

	// Token: 0x060017A0 RID: 6048 RVA: 0x0009D24E File Offset: 0x0009B44E
	private void FadeIn()
	{
		this.m_lodGroup.localReferencePoint = this.m_originalLocalRef;
	}

	// Token: 0x0400190D RID: 6413
	private Vector3 m_originalLocalRef;

	// Token: 0x0400190E RID: 6414
	private LODGroup m_lodGroup;

	// Token: 0x0400190F RID: 6415
	private const float m_minTriggerDistance = 20f;
}
