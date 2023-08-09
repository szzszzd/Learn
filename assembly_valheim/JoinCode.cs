using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000BA RID: 186
public class JoinCode : MonoBehaviour
{
	// Token: 0x0600080F RID: 2063 RVA: 0x000404CC File Offset: 0x0003E6CC
	public static void Show(bool firstSpawn = false)
	{
		if (JoinCode.m_instance != null)
		{
			if (firstSpawn)
			{
				JoinCode.m_instance.Init();
			}
			JoinCode.m_instance.Activate(firstSpawn);
		}
	}

	// Token: 0x06000810 RID: 2064 RVA: 0x000404F3 File Offset: 0x0003E6F3
	public static void Hide()
	{
		if (JoinCode.m_instance != null)
		{
			JoinCode.m_instance.Deactivate();
		}
	}

	// Token: 0x06000811 RID: 2065 RVA: 0x0004050C File Offset: 0x0003E70C
	private void Start()
	{
		JoinCode.m_instance = this;
		this.m_textAlpha = this.m_text.color.a;
		this.m_darkenAlpha = this.m_darken.GetAlpha();
		this.Deactivate();
	}

	// Token: 0x06000812 RID: 2066 RVA: 0x00040541 File Offset: 0x0003E741
	private void Init()
	{
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			this.m_joinCode = ZPlayFabMatchmaking.JoinCode;
			base.gameObject.SetActive(this.m_joinCode.Length > 0);
			return;
		}
		base.gameObject.SetActive(false);
	}

	// Token: 0x06000813 RID: 2067 RVA: 0x0004057C File Offset: 0x0003E77C
	private void Activate(bool firstSpawn)
	{
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			this.m_joinCode = ZPlayFabMatchmaking.JoinCode;
		}
		this.ResetAlpha();
		this.m_root.SetActive(this.m_joinCode.Length > 0);
		this.m_inMenu = !firstSpawn;
		this.m_isVisible = (firstSpawn ? this.m_firstShowDuration : 0f);
	}

	// Token: 0x06000814 RID: 2068 RVA: 0x000405DB File Offset: 0x0003E7DB
	public void Deactivate()
	{
		this.m_root.SetActive(false);
		this.m_inMenu = false;
		this.m_isVisible = 0f;
	}

	// Token: 0x06000815 RID: 2069 RVA: 0x000405FC File Offset: 0x0003E7FC
	private void ResetAlpha()
	{
		Color color = this.m_text.color;
		color.a = this.m_textAlpha;
		this.m_text.color = color;
		this.m_darken.SetAlpha(this.m_darkenAlpha);
	}

	// Token: 0x06000816 RID: 2070 RVA: 0x00040640 File Offset: 0x0003E840
	private void Update()
	{
		if (this.m_inMenu || this.m_isVisible > 0f)
		{
			this.m_btn.gameObject.GetComponentInChildren<Text>().text = Localization.instance.Localize("$menu_joincode", new string[]
			{
				this.m_joinCode
			});
			if (this.m_inMenu)
			{
				if (Settings.instance == null && (Menu.instance == null || (!Menu.instance.m_logoutDialog.gameObject.activeSelf && !Menu.instance.PlayerListActive)) && this.m_inputBlocked)
				{
					this.m_inputBlocked = false;
					return;
				}
				this.m_inputBlocked = (Settings.instance != null || (Menu.instance != null && (Menu.instance.m_logoutDialog.gameObject.activeSelf || Menu.instance.PlayerListActive)));
				if (this.m_inputBlocked)
				{
					return;
				}
				if (Settings.instance == null && (ZInput.GetButtonDown("JoyButtonX") || Input.GetKeyDown(KeyCode.J)))
				{
					this.CopyJoinCodeToClipboard();
					return;
				}
			}
			else
			{
				this.m_isVisible -= Time.deltaTime;
				if (this.m_isVisible < 0f)
				{
					JoinCode.Hide();
					return;
				}
				if (this.m_isVisible < this.m_fadeOutDuration)
				{
					float t = this.m_isVisible / this.m_fadeOutDuration;
					float a = Mathf.Lerp(0f, this.m_textAlpha, t);
					float alpha = Mathf.Lerp(0f, this.m_darkenAlpha, t);
					Color color = this.m_text.color;
					color.a = a;
					this.m_text.color = color;
					this.m_darken.SetAlpha(alpha);
				}
			}
		}
	}

	// Token: 0x06000817 RID: 2071 RVA: 0x00040801 File Offset: 0x0003EA01
	public void OnClick()
	{
		this.CopyJoinCodeToClipboard();
	}

	// Token: 0x06000818 RID: 2072 RVA: 0x0004080C File Offset: 0x0003EA0C
	private void CopyJoinCodeToClipboard()
	{
		Gogan.LogEvent("Screen", "CopyToClipboard", "JoinCode", 0L);
		GUIUtility.systemCopyBuffer = this.m_joinCode;
		if (MessageHud.instance != null)
		{
			MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "$menu_joincode_copied", 0, null);
		}
	}

	// Token: 0x04000A31 RID: 2609
	private static JoinCode m_instance;

	// Token: 0x04000A32 RID: 2610
	public GameObject m_root;

	// Token: 0x04000A33 RID: 2611
	public Button m_btn;

	// Token: 0x04000A34 RID: 2612
	public Text m_text;

	// Token: 0x04000A35 RID: 2613
	public CanvasRenderer m_darken;

	// Token: 0x04000A36 RID: 2614
	public float m_firstShowDuration = 7f;

	// Token: 0x04000A37 RID: 2615
	public float m_fadeOutDuration = 3f;

	// Token: 0x04000A38 RID: 2616
	private string m_joinCode = "";

	// Token: 0x04000A39 RID: 2617
	private float m_textAlpha;

	// Token: 0x04000A3A RID: 2618
	private float m_darkenAlpha;

	// Token: 0x04000A3B RID: 2619
	private float m_isVisible;

	// Token: 0x04000A3C RID: 2620
	private bool m_inMenu;

	// Token: 0x04000A3D RID: 2621
	private bool m_inputBlocked;
}
