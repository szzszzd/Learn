using System;

// Token: 0x02000115 RID: 277
public class YesNoPopup : FixedPopupBase
{
	// Token: 0x06000ADB RID: 2779 RVA: 0x00051437 File Offset: 0x0004F637
	public YesNoPopup(string header, string text, PopupButtonCallback yesCallback, PopupButtonCallback noCallback, bool localizeText = true) : base(header, text, localizeText)
	{
		this.yesCallback = yesCallback;
		this.noCallback = noCallback;
	}

	// Token: 0x17000067 RID: 103
	// (get) Token: 0x06000ADC RID: 2780 RVA: 0x0000247B File Offset: 0x0000067B
	public override PopupType Type
	{
		get
		{
			return PopupType.YesNo;
		}
	}

	// Token: 0x04000D1B RID: 3355
	public readonly PopupButtonCallback yesCallback;

	// Token: 0x04000D1C RID: 3356
	public readonly PopupButtonCallback noCallback;
}
