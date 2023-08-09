using System;
using UnityEngine;

// Token: 0x0200023D RID: 573
public class GuidePoint : MonoBehaviour
{
	// Token: 0x06001696 RID: 5782 RVA: 0x00094D28 File Offset: 0x00092F28
	private void Start()
	{
		if (!Raven.IsInstantiated())
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_ravenPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		}
		this.m_text.m_static = true;
		this.m_text.m_guidePoint = this;
		Raven.RegisterStaticText(this.m_text);
	}

	// Token: 0x06001697 RID: 5783 RVA: 0x00094D84 File Offset: 0x00092F84
	private void OnDestroy()
	{
		Raven.UnregisterStaticText(this.m_text);
	}

	// Token: 0x06001698 RID: 5784 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmos()
	{
	}

	// Token: 0x040017C6 RID: 6086
	public Raven.RavenText m_text = new Raven.RavenText();

	// Token: 0x040017C7 RID: 6087
	public GameObject m_ravenPrefab;
}
