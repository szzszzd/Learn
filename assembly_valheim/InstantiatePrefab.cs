using System;
using UnityEngine;

// Token: 0x020001CF RID: 463
public class InstantiatePrefab : MonoBehaviour
{
	// Token: 0x06001302 RID: 4866 RVA: 0x0007D6B4 File Offset: 0x0007B8B4
	private void Awake()
	{
		if (this.m_attach)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform).transform.SetAsFirstSibling();
			return;
		}
		UnityEngine.Object.Instantiate<GameObject>(this.m_prefab);
	}

	// Token: 0x040013DB RID: 5083
	public GameObject m_prefab;

	// Token: 0x040013DC RID: 5084
	public bool m_attach = true;

	// Token: 0x040013DD RID: 5085
	public bool m_moveToTop;
}
