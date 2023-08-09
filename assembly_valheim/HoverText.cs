using System;
using UnityEngine;

// Token: 0x0200024C RID: 588
public class HoverText : MonoBehaviour, Hoverable
{
	// Token: 0x0600170C RID: 5900 RVA: 0x00098D19 File Offset: 0x00096F19
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_text);
	}

	// Token: 0x0600170D RID: 5901 RVA: 0x00098D19 File Offset: 0x00096F19
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_text);
	}

	// Token: 0x0400186A RID: 6250
	public string m_text = "";
}
