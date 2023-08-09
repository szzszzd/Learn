using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000105 RID: 261
public class TextsDialog : MonoBehaviour
{
	// Token: 0x06000A9A RID: 2714 RVA: 0x000507D0 File Offset: 0x0004E9D0
	private void Awake()
	{
		this.m_baseListSize = this.m_listRoot.rect.height;
	}

	// Token: 0x06000A9B RID: 2715 RVA: 0x000507F8 File Offset: 0x0004E9F8
	public void Setup(Player player)
	{
		base.gameObject.SetActive(true);
		this.FillTextList();
		if (this.m_texts.Count > 0)
		{
			this.ShowText(this.m_texts[0]);
			return;
		}
		this.m_textAreaTopic.text = "";
		this.m_textArea.text = "";
	}

	// Token: 0x06000A9C RID: 2716 RVA: 0x00050858 File Offset: 0x0004EA58
	private void Update()
	{
		this.UpdateGamepadInput();
		if (this.m_texts.Count > 0)
		{
			RectTransform rectTransform = this.m_leftScrollRect.transform as RectTransform;
			RectTransform listRoot = this.m_listRoot;
			this.m_leftScrollbar.size = rectTransform.rect.height / listRoot.rect.height;
		}
	}

	// Token: 0x06000A9D RID: 2717 RVA: 0x000508B9 File Offset: 0x0004EAB9
	private IEnumerator FocusOnCurrentLevel(ScrollRect scrollRect, RectTransform listRoot, RectTransform element)
	{
		yield return null;
		yield return null;
		Canvas.ForceUpdateCanvases();
		this.SnapTo(scrollRect, this.m_listRoot, element);
		yield break;
	}

	// Token: 0x06000A9E RID: 2718 RVA: 0x000508D8 File Offset: 0x0004EAD8
	private void SnapTo(ScrollRect scrollRect, RectTransform listRoot, RectTransform target)
	{
		Canvas.ForceUpdateCanvases();
		listRoot.anchoredPosition = scrollRect.transform.InverseTransformPoint(listRoot.position) - scrollRect.transform.InverseTransformPoint(target.position) - new Vector2(target.sizeDelta.x / 2f, 0f);
	}

	// Token: 0x06000A9F RID: 2719 RVA: 0x00050944 File Offset: 0x0004EB44
	private void FillTextList()
	{
		foreach (TextsDialog.TextInfo textInfo in this.m_texts)
		{
			UnityEngine.Object.Destroy(textInfo.m_listElement);
		}
		this.m_texts.Clear();
		this.UpdateTextsList();
		for (int i = 0; i < this.m_texts.Count; i++)
		{
			TextsDialog.TextInfo text = this.m_texts[i];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, Vector3.zero, Quaternion.identity, this.m_listRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)(-(float)i) * this.m_spacing);
			Utils.FindChild(gameObject.transform, "name").GetComponent<Text>().text = Localization.instance.Localize(text.m_topic);
			text.m_listElement = gameObject;
			text.m_selected = Utils.FindChild(gameObject.transform, "selected").gameObject;
			text.m_selected.SetActive(false);
			gameObject.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnSelectText(text);
			});
		}
		float size = Mathf.Max(this.m_baseListSize, (float)this.m_texts.Count * this.m_spacing);
		this.m_listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		if (this.m_texts.Count > 0)
		{
			this.m_recipeEnsureVisible.CenterOnItem(this.m_texts[0].m_listElement.transform as RectTransform);
		}
	}

	// Token: 0x06000AA0 RID: 2720 RVA: 0x00050B1C File Offset: 0x0004ED1C
	private void UpdateGamepadInput()
	{
		if (this.m_inputDelayTimer > 0f)
		{
			this.m_inputDelayTimer -= Time.unscaledDeltaTime;
			return;
		}
		if (ZInput.IsGamepadActive() && this.m_texts.Count > 0)
		{
			float joyRightStickY = ZInput.GetJoyRightStickY();
			float joyLeftStickY = ZInput.GetJoyLeftStickY(true);
			bool buttonDown = ZInput.GetButtonDown("JoyDPadUp");
			bool flag = joyLeftStickY < -0.1f;
			bool buttonDown2 = ZInput.GetButtonDown("JoyDPadDown");
			bool flag2 = joyLeftStickY > 0.1f;
			if ((buttonDown2 || flag2) && this.m_selectionIndex < this.m_texts.Count - 1)
			{
				this.ShowText(Mathf.Min(this.m_texts.Count - 1, this.GetSelectedText() + 1));
				this.m_inputDelayTimer = 0.1f;
			}
			if ((flag || buttonDown) && this.m_selectionIndex > 0)
			{
				this.ShowText(Mathf.Max(0, this.GetSelectedText() - 1));
				this.m_inputDelayTimer = 0.1f;
			}
			if (this.m_rightScrollbar.gameObject.activeSelf && (joyRightStickY < -0.1f || joyRightStickY > 0.1f))
			{
				this.m_rightScrollbar.value = Mathf.Clamp01(this.m_rightScrollbar.value - joyRightStickY * 10f * Time.deltaTime * (1f - this.m_rightScrollbar.size));
				this.m_inputDelayTimer = 0.1f;
			}
		}
	}

	// Token: 0x06000AA1 RID: 2721 RVA: 0x00050C6F File Offset: 0x0004EE6F
	private void OnSelectText(TextsDialog.TextInfo text)
	{
		this.ShowText(text);
	}

	// Token: 0x06000AA2 RID: 2722 RVA: 0x00050C78 File Offset: 0x0004EE78
	private int GetSelectedText()
	{
		for (int i = 0; i < this.m_texts.Count; i++)
		{
			if (this.m_texts[i].m_selected.activeSelf)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x06000AA3 RID: 2723 RVA: 0x00050CB6 File Offset: 0x0004EEB6
	private void ShowText(int i)
	{
		this.m_selectionIndex = i;
		this.ShowText(this.m_texts[i]);
	}

	// Token: 0x06000AA4 RID: 2724 RVA: 0x00050CD4 File Offset: 0x0004EED4
	private void ShowText(TextsDialog.TextInfo text)
	{
		this.m_textAreaTopic.text = Localization.instance.Localize(text.m_topic);
		this.m_textArea.text = Localization.instance.Localize(text.m_text);
		foreach (TextsDialog.TextInfo textInfo in this.m_texts)
		{
			textInfo.m_selected.SetActive(false);
		}
		text.m_selected.SetActive(true);
		base.StartCoroutine(this.FocusOnCurrentLevel(this.m_leftScrollRect, this.m_listRoot, text.m_selected.transform as RectTransform));
	}

	// Token: 0x06000AA5 RID: 2725 RVA: 0x00050D98 File Offset: 0x0004EF98
	public void OnClose()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06000AA6 RID: 2726 RVA: 0x00050DA8 File Offset: 0x0004EFA8
	private void UpdateTextsList()
	{
		this.m_texts.Clear();
		foreach (KeyValuePair<string, string> keyValuePair in Player.m_localPlayer.GetKnownTexts())
		{
			this.m_texts.Add(new TextsDialog.TextInfo(Localization.instance.Localize(keyValuePair.Key), Localization.instance.Localize(keyValuePair.Value)));
		}
		this.m_texts.Sort((TextsDialog.TextInfo a, TextsDialog.TextInfo b) => a.m_topic.CompareTo(b.m_topic));
		this.AddLog();
		this.AddActiveEffects();
	}

	// Token: 0x06000AA7 RID: 2727 RVA: 0x00050E6C File Offset: 0x0004F06C
	private void AddLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string str in MessageHud.instance.GetLog())
		{
			stringBuilder.Append(str + "\n\n");
		}
		this.m_texts.Insert(0, new TextsDialog.TextInfo(Localization.instance.Localize("$inventory_logs"), stringBuilder.ToString()));
	}

	// Token: 0x06000AA8 RID: 2728 RVA: 0x00050EFC File Offset: 0x0004F0FC
	private void AddActiveEffects()
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		List<StatusEffect> list = new List<StatusEffect>();
		Player.m_localPlayer.GetSEMan().GetHUDStatusEffects(list);
		StringBuilder stringBuilder = new StringBuilder(256);
		foreach (StatusEffect statusEffect in list)
		{
			stringBuilder.Append("<color=orange>" + Localization.instance.Localize(statusEffect.m_name) + "</color>\n");
			stringBuilder.Append(Localization.instance.Localize(statusEffect.GetTooltipString()));
			stringBuilder.Append("\n\n");
		}
		StatusEffect statusEffect2;
		float num;
		Player.m_localPlayer.GetGuardianPowerHUD(out statusEffect2, out num);
		if (statusEffect2)
		{
			stringBuilder.Append("<color=yellow>" + Localization.instance.Localize("$inventory_selectedgp") + "</color>\n");
			stringBuilder.Append("<color=orange>" + Localization.instance.Localize(statusEffect2.m_name) + "</color>\n");
			stringBuilder.Append(Localization.instance.Localize(statusEffect2.GetTooltipString()));
		}
		this.m_texts.Insert(0, new TextsDialog.TextInfo(Localization.instance.Localize("$inventory_activeeffects"), stringBuilder.ToString()));
	}

	// Token: 0x04000CDD RID: 3293
	public RectTransform m_listRoot;

	// Token: 0x04000CDE RID: 3294
	public ScrollRect m_leftScrollRect;

	// Token: 0x04000CDF RID: 3295
	public Scrollbar m_leftScrollbar;

	// Token: 0x04000CE0 RID: 3296
	public Scrollbar m_rightScrollbar;

	// Token: 0x04000CE1 RID: 3297
	public GameObject m_elementPrefab;

	// Token: 0x04000CE2 RID: 3298
	public Text m_totalSkillText;

	// Token: 0x04000CE3 RID: 3299
	public float m_spacing = 80f;

	// Token: 0x04000CE4 RID: 3300
	public Text m_textAreaTopic;

	// Token: 0x04000CE5 RID: 3301
	public Text m_textArea;

	// Token: 0x04000CE6 RID: 3302
	public ScrollRectEnsureVisible m_recipeEnsureVisible;

	// Token: 0x04000CE7 RID: 3303
	private List<TextsDialog.TextInfo> m_texts = new List<TextsDialog.TextInfo>();

	// Token: 0x04000CE8 RID: 3304
	private float m_baseListSize;

	// Token: 0x04000CE9 RID: 3305
	private int m_selectionIndex;

	// Token: 0x04000CEA RID: 3306
	private float m_inputDelayTimer;

	// Token: 0x04000CEB RID: 3307
	private const float InputDelay = 0.1f;

	// Token: 0x02000106 RID: 262
	public class TextInfo
	{
		// Token: 0x06000AAA RID: 2730 RVA: 0x0005107A File Offset: 0x0004F27A
		public TextInfo(string topic, string text)
		{
			this.m_topic = topic;
			this.m_text = text;
		}

		// Token: 0x04000CEC RID: 3308
		public string m_topic;

		// Token: 0x04000CED RID: 3309
		public string m_text;

		// Token: 0x04000CEE RID: 3310
		public GameObject m_listElement;

		// Token: 0x04000CEF RID: 3311
		public GameObject m_selected;
	}
}
