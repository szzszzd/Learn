using System;

// Token: 0x02000116 RID: 278
public class WarningPopup : FixedPopupBase
{
	// Token: 0x06000ADD RID: 2781 RVA: 0x00051452 File Offset: 0x0004F652
	public WarningPopup(string header, string text, PopupButtonCallback okCallback, bool localizeText = true) : base(header, text, localizeText)
	{
		this.okCallback = okCallback;
	}

	// Token: 0x17000068 RID: 104
	// (get) Token: 0x06000ADE RID: 2782 RVA: 0x0000290F File Offset: 0x00000B0F
	public override PopupType Type
	{
		get
		{
			return PopupType.Warning;
		}
	}

	// Token: 0x04000D1D RID: 3357
	public readonly PopupButtonCallback okCallback;
}
