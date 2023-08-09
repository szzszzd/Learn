using System;
using System.Collections;

// Token: 0x02000117 RID: 279
public class CancelableTaskPopup : LivePopupBase
{
	// Token: 0x06000ADF RID: 2783 RVA: 0x00051465 File Offset: 0x0004F665
	public CancelableTaskPopup(RetrieveFromStringSource headerRetrievalFunc, RetrieveFromStringSource textRetrievalFunc, RetrieveFromBoolSource shouldCloseRetrievalFunc, PopupButtonCallback cancelCallback) : base(headerRetrievalFunc, textRetrievalFunc, shouldCloseRetrievalFunc)
	{
		base.SetUpdateRoutine(this.UpdateRoutine());
		this.cancelCallback = cancelCallback;
	}

	// Token: 0x06000AE0 RID: 2784 RVA: 0x00051484 File Offset: 0x0004F684
	private IEnumerator UpdateRoutine()
	{
		while (!this.shouldCloseRetrievalFunc())
		{
			this.headerText.text = this.headerRetrievalFunc();
			this.bodyText.text = this.textRetrievalFunc();
			yield return null;
		}
		base.ShouldClose = true;
		yield break;
	}

	// Token: 0x17000069 RID: 105
	// (get) Token: 0x06000AE1 RID: 2785 RVA: 0x00051493 File Offset: 0x0004F693
	public override PopupType Type
	{
		get
		{
			return PopupType.CancelableTask;
		}
	}

	// Token: 0x04000D1E RID: 3358
	public readonly PopupButtonCallback cancelCallback;
}
