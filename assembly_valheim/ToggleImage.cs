using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200010A RID: 266
public class ToggleImage : MonoBehaviour
{
	// Token: 0x06000AB6 RID: 2742 RVA: 0x0005115C File Offset: 0x0004F35C
	private void Awake()
	{
		this.m_toggle = base.GetComponent<Toggle>();
	}

	// Token: 0x06000AB7 RID: 2743 RVA: 0x0005116A File Offset: 0x0004F36A
	private void Update()
	{
		if (this.m_toggle.isOn)
		{
			this.m_targetImage.sprite = this.m_onImage;
			return;
		}
		this.m_targetImage.sprite = this.m_offImage;
	}

	// Token: 0x04000CF9 RID: 3321
	private Toggle m_toggle;

	// Token: 0x04000CFA RID: 3322
	public Image m_targetImage;

	// Token: 0x04000CFB RID: 3323
	public Sprite m_onImage;

	// Token: 0x04000CFC RID: 3324
	public Sprite m_offImage;
}
