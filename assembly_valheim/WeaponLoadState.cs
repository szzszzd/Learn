using System;
using UnityEngine;

// Token: 0x02000131 RID: 305
public class WeaponLoadState : MonoBehaviour
{
	// Token: 0x06000BE2 RID: 3042 RVA: 0x000576C0 File Offset: 0x000558C0
	private void Start()
	{
		this.m_owner = base.GetComponentInParent<Player>();
	}

	// Token: 0x06000BE3 RID: 3043 RVA: 0x000576D0 File Offset: 0x000558D0
	private void Update()
	{
		if (this.m_owner)
		{
			bool flag = this.m_owner.IsWeaponLoaded();
			this.m_unloaded.SetActive(!flag);
			this.m_loaded.SetActive(flag);
		}
	}

	// Token: 0x04000E4B RID: 3659
	public GameObject m_unloaded;

	// Token: 0x04000E4C RID: 3660
	public GameObject m_loaded;

	// Token: 0x04000E4D RID: 3661
	private Player m_owner;
}
