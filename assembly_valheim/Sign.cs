using System;
using TMPro;
using UnityEngine;

// Token: 0x02000291 RID: 657
public class Sign : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	// Token: 0x0600192B RID: 6443 RVA: 0x000A7860 File Offset: 0x000A5A60
	private void Awake()
	{
		this.m_currentText = this.m_defaultText;
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.UpdateText();
		base.InvokeRepeating("UpdateText", 2f, 2f);
	}

	// Token: 0x0600192C RID: 6444 RVA: 0x000A78B0 File Offset: 0x000A5AB0
	public string GetHoverText()
	{
		string text = this.m_isViewable ? ("\"" + this.GetText().RemoveRichTextTags() + "\"") : "[TEXT HIDDEN DUE TO UGC SETTINGS]";
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return text;
		}
		return text + "\n" + Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x0600192D RID: 6445 RVA: 0x000A7927 File Offset: 0x000A5B27
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x0600192E RID: 6446 RVA: 0x000A792F File Offset: 0x000A5B2F
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return false;
		}
		TextInput.instance.RequestText(this, "$piece_sign_input", this.m_characterLimit);
		return true;
	}

	// Token: 0x0600192F RID: 6447 RVA: 0x000A7968 File Offset: 0x000A5B68
	private void UpdateText()
	{
		string text = this.m_nview.GetZDO().GetString(ZDOVars.s_text, this.m_defaultText);
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_author, "");
		if (this.m_currentText == text)
		{
			return;
		}
		PrivilegeManager.CanViewUserGeneratedContent(@string, delegate(PrivilegeManager.Result access)
		{
			switch (access)
			{
			case PrivilegeManager.Result.Allowed:
				this.m_currentText = text;
				this.m_textWidget.text = this.m_currentText;
				this.m_isViewable = true;
				return;
			case PrivilegeManager.Result.NotAllowed:
				this.m_currentText = "";
				this.m_textWidget.text = "ᚬᛏᛁᛚᛚᚴᛅᚾᚴᛚᛁᚴ";
				this.m_isViewable = false;
				return;
			}
			this.m_currentText = "";
			this.m_textWidget.text = "ᚬᛏᛁᛚᛚᚴᛅᚾᚴᛚᛁᚴ";
			this.m_isViewable = false;
			ZLog.LogError("Failed to check UGC privilege");
		});
	}

	// Token: 0x06001930 RID: 6448 RVA: 0x000A79E4 File Offset: 0x000A5BE4
	public string GetText()
	{
		return this.m_currentText;
	}

	// Token: 0x06001931 RID: 6449 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001932 RID: 6450 RVA: 0x000A79EC File Offset: 0x000A5BEC
	public void SetText(string text)
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return;
		}
		this.m_nview.ClaimOwnership();
		this.m_nview.GetZDO().Set(ZDOVars.s_text, text);
		this.m_nview.GetZDO().Set(ZDOVars.s_author, PrivilegeManager.GetNetworkUserId());
		this.UpdateText();
	}

	// Token: 0x04001B25 RID: 6949
	public TextMeshProUGUI m_textWidget;

	// Token: 0x04001B26 RID: 6950
	public string m_name = "Sign";

	// Token: 0x04001B27 RID: 6951
	public string m_defaultText = "Sign";

	// Token: 0x04001B28 RID: 6952
	public int m_characterLimit = 50;

	// Token: 0x04001B29 RID: 6953
	private ZNetView m_nview;

	// Token: 0x04001B2A RID: 6954
	private bool m_isViewable = true;

	// Token: 0x04001B2B RID: 6955
	private string m_currentText;
}
