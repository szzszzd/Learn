using System;
using UnityEngine;

// Token: 0x0200001A RID: 26
public class ItemStyle : MonoBehaviour, IEquipmentVisual
{
	// Token: 0x060001AD RID: 429 RVA: 0x0000BFCB File Offset: 0x0000A1CB
	public void Setup(int style)
	{
		base.GetComponent<Renderer>().material.SetFloat("_Style", (float)style);
	}
}
