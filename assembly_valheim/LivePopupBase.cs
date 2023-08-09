using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Token: 0x02000114 RID: 276
public abstract class LivePopupBase : PopupBase
{
	// Token: 0x06000AD1 RID: 2769 RVA: 0x000513C5 File Offset: 0x0004F5C5
	public LivePopupBase(RetrieveFromStringSource headerRetrievalFunc, RetrieveFromStringSource textRetrievalFunc, RetrieveFromBoolSource isActiveRetrievalFunc)
	{
		this.headerRetrievalFunc = headerRetrievalFunc;
		this.textRetrievalFunc = textRetrievalFunc;
		this.shouldCloseRetrievalFunc = isActiveRetrievalFunc;
	}

	// Token: 0x06000AD2 RID: 2770 RVA: 0x000513E2 File Offset: 0x0004F5E2
	protected void SetUpdateRoutine(IEnumerator updateRoutine)
	{
		this.updateRoutine = updateRoutine;
	}

	// Token: 0x06000AD3 RID: 2771 RVA: 0x000513EB File Offset: 0x0004F5EB
	public void SetUpdateCoroutineReference(Coroutine updateCoroutine)
	{
		this.updateCoroutine = updateCoroutine;
	}

	// Token: 0x06000AD4 RID: 2772 RVA: 0x000513F4 File Offset: 0x0004F5F4
	public void SetTextReferences(TextMeshProUGUI headerText, TextMeshProUGUI bodyText)
	{
		this.headerText = headerText;
		this.bodyText = bodyText;
	}

	// Token: 0x17000064 RID: 100
	// (get) Token: 0x06000AD5 RID: 2773 RVA: 0x00051404 File Offset: 0x0004F604
	// (set) Token: 0x06000AD6 RID: 2774 RVA: 0x0005140C File Offset: 0x0004F60C
	public IEnumerator updateRoutine { get; private set; }

	// Token: 0x17000065 RID: 101
	// (get) Token: 0x06000AD7 RID: 2775 RVA: 0x00051415 File Offset: 0x0004F615
	// (set) Token: 0x06000AD8 RID: 2776 RVA: 0x0005141D File Offset: 0x0004F61D
	public Coroutine updateCoroutine { get; private set; }

	// Token: 0x17000066 RID: 102
	// (get) Token: 0x06000AD9 RID: 2777 RVA: 0x00051426 File Offset: 0x0004F626
	// (set) Token: 0x06000ADA RID: 2778 RVA: 0x0005142E File Offset: 0x0004F62E
	public bool ShouldClose { get; protected set; }

	// Token: 0x04000D13 RID: 3347
	protected TextMeshProUGUI headerText;

	// Token: 0x04000D14 RID: 3348
	protected TextMeshProUGUI bodyText;

	// Token: 0x04000D15 RID: 3349
	public readonly RetrieveFromStringSource headerRetrievalFunc;

	// Token: 0x04000D16 RID: 3350
	public readonly RetrieveFromStringSource textRetrievalFunc;

	// Token: 0x04000D17 RID: 3351
	public readonly RetrieveFromBoolSource shouldCloseRetrievalFunc;
}
