using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000B4 RID: 180
public class InputFieldSubmit : MonoBehaviour
{
	// Token: 0x060007AE RID: 1966 RVA: 0x0003B9F8 File Offset: 0x00039BF8
	private void Awake()
	{
		this.m_field = base.GetComponent<InputField>();
	}

	// Token: 0x060007AF RID: 1967 RVA: 0x0003BA08 File Offset: 0x00039C08
	private void Update()
	{
		if (this.m_field.text != "" && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || ZInput.GetButtonDown("JoyButtonA")))
		{
			this.m_onSubmit(this.m_field.text);
			this.m_field.text = "";
		}
	}

	// Token: 0x0400099A RID: 2458
	public Action<string> m_onSubmit;

	// Token: 0x0400099B RID: 2459
	private InputField m_field;
}
