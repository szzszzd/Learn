using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000AA RID: 170
public class Feedback : MonoBehaviour
{
	// Token: 0x06000752 RID: 1874 RVA: 0x00038952 File Offset: 0x00036B52
	private void Awake()
	{
		Feedback.m_instance = this;
	}

	// Token: 0x06000753 RID: 1875 RVA: 0x0003895A File Offset: 0x00036B5A
	private void OnDestroy()
	{
		if (Feedback.m_instance == this)
		{
			Feedback.m_instance = null;
		}
	}

	// Token: 0x06000754 RID: 1876 RVA: 0x0003896F File Offset: 0x00036B6F
	public static bool IsVisible()
	{
		return Feedback.m_instance != null;
	}

	// Token: 0x06000755 RID: 1877 RVA: 0x0003897C File Offset: 0x00036B7C
	private void LateUpdate()
	{
		this.m_sendButton.interactable = this.IsValid();
		if (Feedback.IsVisible() && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")))
		{
			this.OnBack();
		}
	}

	// Token: 0x06000756 RID: 1878 RVA: 0x000389B1 File Offset: 0x00036BB1
	private bool IsValid()
	{
		return this.m_subject.text.Length != 0 && this.m_text.text.Length != 0;
	}

	// Token: 0x06000757 RID: 1879 RVA: 0x000389DC File Offset: 0x00036BDC
	public void OnBack()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000758 RID: 1880 RVA: 0x000389EC File Offset: 0x00036BEC
	public void OnSend()
	{
		if (!this.IsValid())
		{
			return;
		}
		string category = this.GetCategory();
		Gogan.LogEvent("Feedback_" + category, this.m_subject.text, this.m_text.text, 0L);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000759 RID: 1881 RVA: 0x00038A3C File Offset: 0x00036C3C
	private string GetCategory()
	{
		if (this.m_catBug.isOn)
		{
			return "Bug";
		}
		if (this.m_catFeedback.isOn)
		{
			return "Feedback";
		}
		if (this.m_catIdea.isOn)
		{
			return "Idea";
		}
		return "";
	}

	// Token: 0x040008F1 RID: 2289
	private static Feedback m_instance;

	// Token: 0x040008F2 RID: 2290
	public Text m_subject;

	// Token: 0x040008F3 RID: 2291
	public Text m_text;

	// Token: 0x040008F4 RID: 2292
	public Button m_sendButton;

	// Token: 0x040008F5 RID: 2293
	public Toggle m_catBug;

	// Token: 0x040008F6 RID: 2294
	public Toggle m_catFeedback;

	// Token: 0x040008F7 RID: 2295
	public Toggle m_catIdea;
}
