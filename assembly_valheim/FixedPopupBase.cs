using System;

// Token: 0x02000111 RID: 273
public abstract class FixedPopupBase : PopupBase
{
	// Token: 0x06000AC8 RID: 2760 RVA: 0x0005138F File Offset: 0x0004F58F
	public FixedPopupBase(string header, string text, bool localizeText = true)
	{
		this.header = (localizeText ? Localization.instance.Localize(header) : header);
		this.text = (localizeText ? Localization.instance.Localize(text) : text);
	}

	// Token: 0x04000D11 RID: 3345
	public readonly string header;

	// Token: 0x04000D12 RID: 3346
	public readonly string text;
}
