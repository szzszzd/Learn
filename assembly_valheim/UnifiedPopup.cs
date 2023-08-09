using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000119 RID: 281
public class UnifiedPopup : MonoBehaviour
{
	// Token: 0x06000AE8 RID: 2792 RVA: 0x00051538 File Offset: 0x0004F738
	private void Awake()
	{
		if (this.buttonLeft != null)
		{
			this.buttonLeftText = this.buttonLeft.GetComponentInChildren<Text>();
		}
		if (this.buttonCenter != null)
		{
			this.buttonCenterText = this.buttonCenter.GetComponentInChildren<Text>();
		}
		if (this.buttonRight != null)
		{
			this.buttonRightText = this.buttonRight.GetComponentInChildren<Text>();
		}
		this.Hide();
	}

	// Token: 0x06000AE9 RID: 2793 RVA: 0x000515A8 File Offset: 0x0004F7A8
	private void OnEnable()
	{
		if (UnifiedPopup.instance != null && UnifiedPopup.instance != this)
		{
			ZLog.LogError("Can't have more than one UnifiedPopup component enabled at the same time!");
			return;
		}
		UnifiedPopup.instance = this;
	}

	// Token: 0x06000AEA RID: 2794 RVA: 0x000515D5 File Offset: 0x0004F7D5
	private void OnDisable()
	{
		if (UnifiedPopup.instance == null)
		{
			ZLog.LogError("Instance of UnifiedPopup was already null! This may have happened because you had more than one UnifiedPopup component enabled at the same time, which isn't allowed!");
			return;
		}
		UnifiedPopup.instance = null;
	}

	// Token: 0x06000AEB RID: 2795 RVA: 0x000515F8 File Offset: 0x0004F7F8
	private void LateUpdate()
	{
		while (this.popupStack.Count > 0 && this.popupStack.Peek() is LivePopupBase && (this.popupStack.Peek() as LivePopupBase).ShouldClose)
		{
			UnifiedPopup.Pop();
		}
		if (!UnifiedPopup.IsVisible())
		{
			this.wasClosedThisFrame = false;
		}
	}

	// Token: 0x06000AEC RID: 2796 RVA: 0x00051651 File Offset: 0x0004F851
	private static bool InstanceIsNullError()
	{
		if (UnifiedPopup.instance == null)
		{
			ZLog.LogError("Can't show popup when there is no enabled UnifiedPopup component in the scene!");
			return true;
		}
		return false;
	}

	// Token: 0x06000AED RID: 2797 RVA: 0x0005166D File Offset: 0x0004F86D
	public static bool IsAvailable()
	{
		return UnifiedPopup.instance != null;
	}

	// Token: 0x06000AEE RID: 2798 RVA: 0x0005167A File Offset: 0x0004F87A
	public static void Push(PopupBase popup)
	{
		if (UnifiedPopup.InstanceIsNullError())
		{
			return;
		}
		UnifiedPopup.instance.popupStack.Push(popup);
		UnifiedPopup.instance.ShowTopmost();
	}

	// Token: 0x06000AEF RID: 2799 RVA: 0x000516A0 File Offset: 0x0004F8A0
	public static void Pop()
	{
		if (UnifiedPopup.InstanceIsNullError())
		{
			return;
		}
		if (UnifiedPopup.instance.popupStack.Count <= 0)
		{
			ZLog.LogError("Push/pop mismatch! Tried to pop a popup element off the stack when it was empty!");
			return;
		}
		PopupBase popupBase = UnifiedPopup.instance.popupStack.Pop();
		if (popupBase is LivePopupBase)
		{
			UnifiedPopup.instance.StopCoroutine((popupBase as LivePopupBase).updateCoroutine);
		}
		if (UnifiedPopup.instance.popupStack.Count <= 0)
		{
			UnifiedPopup.instance.Hide();
			return;
		}
		UnifiedPopup.instance.ShowTopmost();
	}

	// Token: 0x06000AF0 RID: 2800 RVA: 0x00051728 File Offset: 0x0004F928
	public static void SetFocus()
	{
		if (UnifiedPopup.instance.buttonCenter != null && UnifiedPopup.instance.buttonCenter.gameObject.activeInHierarchy)
		{
			UnifiedPopup.instance.buttonCenter.Select();
			return;
		}
		if (UnifiedPopup.instance.buttonRight != null && UnifiedPopup.instance.buttonRight.gameObject.activeInHierarchy)
		{
			UnifiedPopup.instance.buttonRight.Select();
			return;
		}
		if (UnifiedPopup.instance.buttonLeft != null && UnifiedPopup.instance.buttonLeft.gameObject.activeInHierarchy)
		{
			UnifiedPopup.instance.buttonLeft.Select();
		}
	}

	// Token: 0x06000AF1 RID: 2801 RVA: 0x000517DC File Offset: 0x0004F9DC
	public static bool IsVisible()
	{
		return UnifiedPopup.IsAvailable() && UnifiedPopup.instance.popupUIParent.activeInHierarchy;
	}

	// Token: 0x06000AF2 RID: 2802 RVA: 0x000517F6 File Offset: 0x0004F9F6
	public static bool WasVisibleThisFrame()
	{
		return UnifiedPopup.IsVisible() || (UnifiedPopup.IsAvailable() && UnifiedPopup.instance.wasClosedThisFrame);
	}

	// Token: 0x06000AF3 RID: 2803 RVA: 0x00051814 File Offset: 0x0004FA14
	private void ShowTopmost()
	{
		this.Show(UnifiedPopup.instance.popupStack.Peek());
	}

	// Token: 0x06000AF4 RID: 2804 RVA: 0x0005182C File Offset: 0x0004FA2C
	private void Show(PopupBase popup)
	{
		this.ResetUI();
		switch (popup.Type)
		{
		case PopupType.YesNo:
			this.ShowYesNo(popup as YesNoPopup);
			break;
		case PopupType.Warning:
			this.ShowWarning(popup as WarningPopup);
			break;
		case PopupType.CancelableTask:
			this.ShowCancelableTask(popup as CancelableTaskPopup);
			break;
		}
		this.popupUIParent.SetActive(true);
	}

	// Token: 0x06000AF5 RID: 2805 RVA: 0x00051890 File Offset: 0x0004FA90
	private void ResetUI()
	{
		this.buttonLeft.onClick.RemoveAllListeners();
		this.buttonCenter.onClick.RemoveAllListeners();
		this.buttonRight.onClick.RemoveAllListeners();
		this.buttonLeft.gameObject.SetActive(false);
		this.buttonCenter.gameObject.SetActive(false);
		this.buttonRight.gameObject.SetActive(false);
	}

	// Token: 0x06000AF6 RID: 2806 RVA: 0x00051900 File Offset: 0x0004FB00
	private void ShowYesNo(YesNoPopup popup)
	{
		this.headerText.text = popup.header;
		this.bodyText.text = popup.text;
		this.buttonRightText.text = Localization.instance.Localize(this.yesText);
		this.buttonRight.gameObject.SetActive(true);
		this.buttonRight.onClick.AddListener(delegate
		{
			PopupButtonCallback yesCallback = popup.yesCallback;
			if (yesCallback == null)
			{
				return;
			}
			yesCallback();
		});
		this.buttonLeftText.text = Localization.instance.Localize(this.noText);
		this.buttonLeft.gameObject.SetActive(true);
		this.buttonLeft.onClick.AddListener(delegate
		{
			PopupButtonCallback noCallback = popup.noCallback;
			if (noCallback == null)
			{
				return;
			}
			noCallback();
		});
	}

	// Token: 0x06000AF7 RID: 2807 RVA: 0x000519D8 File Offset: 0x0004FBD8
	private void ShowWarning(WarningPopup popup)
	{
		this.headerText.text = popup.header;
		this.bodyText.text = popup.text;
		this.buttonCenterText.text = Localization.instance.Localize(this.okText);
		this.buttonCenter.gameObject.SetActive(true);
		this.buttonCenter.onClick.AddListener(delegate
		{
			PopupButtonCallback okCallback = popup.okCallback;
			if (okCallback == null)
			{
				return;
			}
			okCallback();
		});
	}

	// Token: 0x06000AF8 RID: 2808 RVA: 0x00051A68 File Offset: 0x0004FC68
	private void ShowCancelableTask(CancelableTaskPopup popup)
	{
		popup.SetTextReferences(this.headerText, this.bodyText);
		popup.SetUpdateCoroutineReference(base.StartCoroutine(popup.updateRoutine));
		this.buttonCenterText.text = Localization.instance.Localize(this.cancelText);
		this.buttonCenter.gameObject.SetActive(true);
		this.buttonCenter.onClick.AddListener(delegate
		{
			PopupButtonCallback cancelCallback = popup.cancelCallback;
			if (cancelCallback != null)
			{
				cancelCallback();
			}
			this.StopCoroutine(popup.updateCoroutine);
		});
	}

	// Token: 0x06000AF9 RID: 2809 RVA: 0x00051B04 File Offset: 0x0004FD04
	private void Hide()
	{
		this.wasClosedThisFrame = true;
		this.popupUIParent.SetActive(false);
	}

	// Token: 0x04000D22 RID: 3362
	private static UnifiedPopup instance;

	// Token: 0x04000D23 RID: 3363
	[SerializeField]
	[global::Tooltip("A reference to the parent object of the rest of the popup. This is what gets enabled and disabled to show and hide the popup.")]
	[Header("References")]
	private GameObject popupUIParent;

	// Token: 0x04000D24 RID: 3364
	[SerializeField]
	[global::Tooltip("A reference to the left button of the popup, assigned to escape on keyboards and B on controllers. This usually gets assigned to \"back\", \"no\" or similar in dual-action popups. It's not necessary to assign buttons to any Unity Events - that is done automatically.")]
	private Button buttonLeft;

	// Token: 0x04000D25 RID: 3365
	[global::Tooltip("A reference to the center button of the popup, assigned to enter on keyboards and A on controllers. This usually gets assigned to \"Ok\" or similar in single-action popups. It's not necessary to assign buttons to any Unity Events - that is done automatically.")]
	[SerializeField]
	private Button buttonCenter;

	// Token: 0x04000D26 RID: 3366
	[global::Tooltip("A reference to the right button of the popup, assigned to enter on keyboards and A on controllers. This usually gets assigned to \"yes\", \"accept\" or similar in dual-action popups. It's not necessary to assign buttons to any Unity Events - that is done automatically.")]
	[SerializeField]
	private Button buttonRight;

	// Token: 0x04000D27 RID: 3367
	[global::Tooltip("A reference to the header text of the popup.")]
	[SerializeField]
	private TextMeshProUGUI headerText;

	// Token: 0x04000D28 RID: 3368
	[global::Tooltip("A reference to the body text of the popup.")]
	[SerializeField]
	private TextMeshProUGUI bodyText;

	// Token: 0x04000D29 RID: 3369
	[Header("Button text")]
	[SerializeField]
	private string yesText = "$menu_yes";

	// Token: 0x04000D2A RID: 3370
	[SerializeField]
	private string noText = "$menu_no";

	// Token: 0x04000D2B RID: 3371
	[SerializeField]
	private string okText = "$menu_ok";

	// Token: 0x04000D2C RID: 3372
	[SerializeField]
	private string cancelText = "$menu_cancel";

	// Token: 0x04000D2D RID: 3373
	private Text buttonLeftText;

	// Token: 0x04000D2E RID: 3374
	private Text buttonCenterText;

	// Token: 0x04000D2F RID: 3375
	private Text buttonRightText;

	// Token: 0x04000D30 RID: 3376
	private bool wasClosedThisFrame;

	// Token: 0x04000D31 RID: 3377
	private Stack<PopupBase> popupStack = new Stack<PopupBase>();
}
