using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000093 RID: 147
public class ChangeLog : MonoBehaviour
{
	// Token: 0x06000650 RID: 1616 RVA: 0x00030240 File Offset: 0x0002E440
	private void Start()
	{
		string text = this.m_changeLog.text;
		this.m_textField.text = text;
	}

	// Token: 0x06000651 RID: 1617 RVA: 0x00030265 File Offset: 0x0002E465
	private void LateUpdate()
	{
		if (!this.m_hasSetScroll)
		{
			this.m_hasSetScroll = true;
			if (this.m_scrollbar != null)
			{
				this.m_scrollbar.value = 1f;
			}
		}
	}

	// Token: 0x040007C5 RID: 1989
	private bool m_hasSetScroll;

	// Token: 0x040007C6 RID: 1990
	public Text m_textField;

	// Token: 0x040007C7 RID: 1991
	public TextAsset m_changeLog;

	// Token: 0x040007C8 RID: 1992
	public TextAsset m_xboxChangeLog;

	// Token: 0x040007C9 RID: 1993
	public Scrollbar m_scrollbar;
}
