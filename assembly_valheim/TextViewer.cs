using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000103 RID: 259
public class TextViewer : MonoBehaviour
{
	// Token: 0x06000A90 RID: 2704 RVA: 0x00050498 File Offset: 0x0004E698
	private void Awake()
	{
		TextViewer.m_instance = this;
		this.m_root.SetActive(true);
		this.m_introRoot.SetActive(true);
		this.m_ravenRoot.SetActive(true);
		this.m_animator = this.m_root.GetComponent<Animator>();
		this.m_animatorIntro = this.m_introRoot.GetComponent<Animator>();
		this.m_animatorRaven = this.m_ravenRoot.GetComponent<Animator>();
	}

	// Token: 0x06000A91 RID: 2705 RVA: 0x00050502 File Offset: 0x0004E702
	private void OnDestroy()
	{
		TextViewer.m_instance = null;
	}

	// Token: 0x1700005F RID: 95
	// (get) Token: 0x06000A92 RID: 2706 RVA: 0x0005050A File Offset: 0x0004E70A
	public static TextViewer instance
	{
		get
		{
			return TextViewer.m_instance;
		}
	}

	// Token: 0x06000A93 RID: 2707 RVA: 0x00050514 File Offset: 0x0004E714
	private void LateUpdate()
	{
		if (!this.IsVisible())
		{
			return;
		}
		this.m_showTime += Time.deltaTime;
		if (this.m_showTime > 0.2f)
		{
			if (this.m_autoHide && Player.m_localPlayer && Vector3.Distance(Player.m_localPlayer.transform.position, this.m_openPlayerPos) > 3f)
			{
				this.Hide();
			}
			if (ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse") || Input.GetKeyDown(KeyCode.Escape))
			{
				this.Hide();
			}
		}
	}

	// Token: 0x06000A94 RID: 2708 RVA: 0x000505AC File Offset: 0x0004E7AC
	public void ShowText(TextViewer.Style style, string topic, string text, bool autoHide)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		topic = Localization.instance.Localize(topic);
		text = Localization.instance.Localize(text);
		if (style == TextViewer.Style.Rune)
		{
			this.m_topic.text = topic;
			this.m_text.text = text;
			this.m_runeText.text = text;
			this.m_animator.SetBool(TextViewer.s_visibleID, true);
		}
		else if (style == TextViewer.Style.Intro)
		{
			this.m_introTopic.text = topic;
			this.m_introText.text = text;
			this.m_animatorIntro.SetTrigger("play");
			ZLog.Log("Show intro " + Time.frameCount.ToString());
		}
		else if (style == TextViewer.Style.Raven)
		{
			this.m_ravenTopic.text = topic;
			this.m_ravenText.text = text;
			this.m_animatorRaven.SetBool(TextViewer.s_visibleID, true);
		}
		this.m_autoHide = autoHide;
		this.m_openPlayerPos = Player.m_localPlayer.transform.position;
		this.m_showTime = 0f;
		ZLog.Log("Show text " + topic + ":" + text);
	}

	// Token: 0x06000A95 RID: 2709 RVA: 0x000506D0 File Offset: 0x0004E8D0
	public void Hide()
	{
		this.m_autoHide = false;
		this.m_animator.SetBool(TextViewer.s_visibleID, false);
		this.m_animatorRaven.SetBool(TextViewer.s_visibleID, false);
	}

	// Token: 0x06000A96 RID: 2710 RVA: 0x000506FC File Offset: 0x0004E8FC
	public bool IsVisible()
	{
		return TextViewer.m_instance.m_animatorIntro.GetCurrentAnimatorStateInfo(0).tagHash == TextViewer.s_animatorTagVisible || this.m_animator.GetBool(TextViewer.s_visibleID) || this.m_animatorIntro.GetBool(TextViewer.s_visibleID) || this.m_animatorRaven.GetBool(TextViewer.s_visibleID);
	}

	// Token: 0x06000A97 RID: 2711 RVA: 0x00050760 File Offset: 0x0004E960
	public static bool IsShowingIntro()
	{
		return TextViewer.m_instance != null && TextViewer.m_instance.m_animatorIntro.GetCurrentAnimatorStateInfo(0).tagHash == TextViewer.s_animatorTagVisible;
	}

	// Token: 0x04000CC5 RID: 3269
	private static TextViewer m_instance;

	// Token: 0x04000CC6 RID: 3270
	private Animator m_animator;

	// Token: 0x04000CC7 RID: 3271
	private Animator m_animatorIntro;

	// Token: 0x04000CC8 RID: 3272
	private Animator m_animatorRaven;

	// Token: 0x04000CC9 RID: 3273
	[Header("Rune")]
	public GameObject m_root;

	// Token: 0x04000CCA RID: 3274
	public Text m_topic;

	// Token: 0x04000CCB RID: 3275
	public Text m_text;

	// Token: 0x04000CCC RID: 3276
	public Text m_runeText;

	// Token: 0x04000CCD RID: 3277
	public GameObject m_closeText;

	// Token: 0x04000CCE RID: 3278
	[Header("Intro")]
	public GameObject m_introRoot;

	// Token: 0x04000CCF RID: 3279
	public Text m_introTopic;

	// Token: 0x04000CD0 RID: 3280
	public Text m_introText;

	// Token: 0x04000CD1 RID: 3281
	[Header("Raven")]
	public GameObject m_ravenRoot;

	// Token: 0x04000CD2 RID: 3282
	public Text m_ravenTopic;

	// Token: 0x04000CD3 RID: 3283
	public Text m_ravenText;

	// Token: 0x04000CD4 RID: 3284
	private static readonly int s_visibleID = ZSyncAnimation.GetHash("visible");

	// Token: 0x04000CD5 RID: 3285
	private static readonly int s_animatorTagVisible = ZSyncAnimation.GetHash("visible");

	// Token: 0x04000CD6 RID: 3286
	private float m_showTime;

	// Token: 0x04000CD7 RID: 3287
	private bool m_autoHide;

	// Token: 0x04000CD8 RID: 3288
	private Vector3 m_openPlayerPos = Vector3.zero;

	// Token: 0x02000104 RID: 260
	public enum Style
	{
		// Token: 0x04000CDA RID: 3290
		Rune,
		// Token: 0x04000CDB RID: 3291
		Intro,
		// Token: 0x04000CDC RID: 3292
		Raven
	}
}
