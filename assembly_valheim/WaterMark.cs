using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200011F RID: 287
public class WaterMark : MonoBehaviour
{
	// Token: 0x06000B08 RID: 2824 RVA: 0x00051D84 File Offset: 0x0004FF84
	private void Awake()
	{
		this.m_text.text = "Version: " + global::Version.GetVersionString(false);
	}

	// Token: 0x04000D3E RID: 3390
	public Text m_text;
}
