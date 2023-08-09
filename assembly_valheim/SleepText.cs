using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000293 RID: 659
public class SleepText : MonoBehaviour
{
	// Token: 0x06001936 RID: 6454 RVA: 0x000A7B4C File Offset: 0x000A5D4C
	private void OnEnable()
	{
		this.m_textField.canvasRenderer.SetAlpha(0f);
		this.m_textField.CrossFadeAlpha(1f, 1f, true);
		this.m_dreamField.enabled = false;
		base.Invoke("CollectResources", 5f);
		base.Invoke("HideZZZ", 2f);
		base.Invoke("ShowDreamText", 4f);
	}

	// Token: 0x06001937 RID: 6455 RVA: 0x000A7BC0 File Offset: 0x000A5DC0
	private void HideZZZ()
	{
		this.m_textField.CrossFadeAlpha(0f, 2f, true);
	}

	// Token: 0x06001938 RID: 6456 RVA: 0x000A7BD8 File Offset: 0x000A5DD8
	private void CollectResources()
	{
		Game.instance.CollectResourcesCheck();
	}

	// Token: 0x06001939 RID: 6457 RVA: 0x000A7BE4 File Offset: 0x000A5DE4
	private void ShowDreamText()
	{
		DreamTexts.DreamText randomDreamText = this.m_dreamTexts.GetRandomDreamText();
		if (randomDreamText == null)
		{
			return;
		}
		this.m_dreamField.enabled = true;
		this.m_dreamField.canvasRenderer.SetAlpha(0f);
		this.m_dreamField.CrossFadeAlpha(1f, 1.5f, true);
		this.m_dreamField.text = Localization.instance.Localize(randomDreamText.m_text);
		base.Invoke("HideDreamText", 6.5f);
	}

	// Token: 0x0600193A RID: 6458 RVA: 0x000A7C63 File Offset: 0x000A5E63
	private void HideDreamText()
	{
		this.m_dreamField.CrossFadeAlpha(0f, 1.5f, true);
	}

	// Token: 0x04001B2E RID: 6958
	public Text m_textField;

	// Token: 0x04001B2F RID: 6959
	public Text m_dreamField;

	// Token: 0x04001B30 RID: 6960
	public DreamTexts m_dreamTexts;
}
