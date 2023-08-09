using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020000FC RID: 252
public class SkillsDialog : MonoBehaviour
{
	// Token: 0x06000A52 RID: 2642 RVA: 0x0004EDB4 File Offset: 0x0004CFB4
	private void Awake()
	{
		this.m_baseListSize = this.m_listRoot.rect.height;
	}

	// Token: 0x06000A53 RID: 2643 RVA: 0x0004EDDA File Offset: 0x0004CFDA
	private IEnumerator SelectFirstEntry()
	{
		yield return null;
		yield return null;
		if (this.m_elements.Count > 0)
		{
			this.m_selectionIndex = 0;
			EventSystem.current.SetSelectedGameObject(this.m_elements[this.m_selectionIndex]);
			base.StartCoroutine(this.FocusOnCurrentLevel(this.m_elements[this.m_selectionIndex].transform as RectTransform));
			this.skillListScrollRect.verticalNormalizedPosition = 1f;
		}
		yield return null;
		yield break;
	}

	// Token: 0x06000A54 RID: 2644 RVA: 0x0004EDE9 File Offset: 0x0004CFE9
	private IEnumerator FocusOnCurrentLevel(RectTransform element)
	{
		yield return null;
		yield return null;
		Canvas.ForceUpdateCanvases();
		this.SnapTo(element);
		yield break;
	}

	// Token: 0x06000A55 RID: 2645 RVA: 0x0004EE00 File Offset: 0x0004D000
	private void SnapTo(RectTransform target)
	{
		Canvas.ForceUpdateCanvases();
		this.m_listRoot.anchoredPosition = this.skillListScrollRect.transform.InverseTransformPoint(this.m_listRoot.position) - this.skillListScrollRect.transform.InverseTransformPoint(target.position) - new Vector2(target.sizeDelta.x / 2f, 0f);
	}

	// Token: 0x06000A56 RID: 2646 RVA: 0x0004EE80 File Offset: 0x0004D080
	private void Update()
	{
		if (this.m_inputDelayTimer > 0f)
		{
			this.m_inputDelayTimer -= Time.unscaledDeltaTime;
			return;
		}
		if (ZInput.IsGamepadActive() && this.m_elements.Count > 0)
		{
			float joyRightStickY = ZInput.GetJoyRightStickY();
			float joyLeftStickY = ZInput.GetJoyLeftStickY(true);
			bool buttonDown = ZInput.GetButtonDown("JoyDPadUp");
			bool flag = joyLeftStickY < -0.1f || joyRightStickY < -0.1f;
			bool buttonDown2 = ZInput.GetButtonDown("JoyDPadDown");
			bool flag2 = joyLeftStickY > 0.1f || joyRightStickY > 0.1f;
			if ((flag || buttonDown) && this.m_selectionIndex > 0)
			{
				this.m_selectionIndex--;
			}
			if ((buttonDown2 || flag2) && this.m_selectionIndex < this.m_elements.Count - 1)
			{
				this.m_selectionIndex++;
			}
			GameObject gameObject = this.m_elements[this.m_selectionIndex];
			EventSystem.current.SetSelectedGameObject(gameObject);
			base.StartCoroutine(this.FocusOnCurrentLevel(gameObject.transform as RectTransform));
			gameObject.GetComponentInChildren<UITooltip>().OnHoverStart(gameObject);
			if (flag || flag2)
			{
				this.m_inputDelayTimer = this.m_inputDelay;
			}
		}
		if (this.m_elements.Count > 0)
		{
			RectTransform rectTransform = this.skillListScrollRect.transform as RectTransform;
			RectTransform listRoot = this.m_listRoot;
			this.scrollbar.size = rectTransform.rect.height / listRoot.rect.height;
		}
	}

	// Token: 0x06000A57 RID: 2647 RVA: 0x0004F004 File Offset: 0x0004D204
	public void Setup(Player player)
	{
		base.gameObject.SetActive(true);
		List<Skills.Skill> skillList = player.GetSkills().GetSkillList();
		int num = skillList.Count - this.m_elements.Count;
		for (int i = 0; i < num; i++)
		{
			GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, Vector3.zero, Quaternion.identity, this.m_listRoot);
			this.m_elements.Add(item);
		}
		for (int j = 0; j < skillList.Count; j++)
		{
			Skills.Skill skill = skillList[j];
			GameObject gameObject = this.m_elements[j];
			gameObject.SetActive(true);
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.anchoredPosition = new Vector2(0f, (float)(-(float)j) * this.m_spacing);
			gameObject.GetComponentInChildren<UITooltip>().Set("", skill.m_info.m_description, this.m_tooltipAnchor, new Vector2(0f, Math.Min(255f, rectTransform.localPosition.y + 10f)));
			Utils.FindChild(gameObject.transform, "icon").GetComponent<Image>().sprite = skill.m_info.m_icon;
			Utils.FindChild(gameObject.transform, "name").GetComponent<Text>().text = Localization.instance.Localize("$skill_" + skill.m_info.m_skill.ToString().ToLower());
			float skillLevel = player.GetSkills().GetSkillLevel(skill.m_info.m_skill);
			Utils.FindChild(gameObject.transform, "leveltext").GetComponent<Text>().text = ((int)skill.m_level).ToString();
			Text component = Utils.FindChild(gameObject.transform, "bonustext").GetComponent<Text>();
			if (skillLevel != skill.m_level)
			{
				component.text = (skillLevel - skill.m_level).ToString("+0");
			}
			else
			{
				component.gameObject.SetActive(false);
			}
			Utils.FindChild(gameObject.transform, "levelbar_total").GetComponent<GuiBar>().SetValue(skillLevel / 100f);
			Utils.FindChild(gameObject.transform, "levelbar").GetComponent<GuiBar>().SetValue(skill.m_level / 100f);
			Utils.FindChild(gameObject.transform, "currentlevel").GetComponent<GuiBar>().SetValue(skill.GetLevelPercentage());
		}
		float size = Mathf.Max(this.m_baseListSize, (float)skillList.Count * this.m_spacing);
		this.m_listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		this.m_totalSkillText.text = string.Concat(new string[]
		{
			"<color=orange>",
			player.GetSkills().GetTotalSkill().ToString("0"),
			"</color><color=white> / </color><color=orange>",
			player.GetSkills().GetTotalSkillCap().ToString("0"),
			"</color>"
		});
		base.StartCoroutine(this.SelectFirstEntry());
	}

	// Token: 0x06000A58 RID: 2648 RVA: 0x0004F324 File Offset: 0x0004D524
	public void OnClose()
	{
		base.gameObject.SetActive(false);
		foreach (GameObject gameObject in this.m_elements)
		{
			gameObject.SetActive(false);
		}
		this.m_elements.Clear();
	}

	// Token: 0x06000A59 RID: 2649 RVA: 0x0004F38C File Offset: 0x0004D58C
	public void SkillClicked(GameObject selectedObject)
	{
		this.m_selectionIndex = this.m_elements.IndexOf(selectedObject);
	}

	// Token: 0x04000C8E RID: 3214
	public RectTransform m_listRoot;

	// Token: 0x04000C8F RID: 3215
	[SerializeField]
	private ScrollRect skillListScrollRect;

	// Token: 0x04000C90 RID: 3216
	[SerializeField]
	private Scrollbar scrollbar;

	// Token: 0x04000C91 RID: 3217
	public RectTransform m_tooltipAnchor;

	// Token: 0x04000C92 RID: 3218
	public GameObject m_elementPrefab;

	// Token: 0x04000C93 RID: 3219
	public Text m_totalSkillText;

	// Token: 0x04000C94 RID: 3220
	public float m_spacing = 80f;

	// Token: 0x04000C95 RID: 3221
	public float m_inputDelay = 0.1f;

	// Token: 0x04000C96 RID: 3222
	private int m_selectionIndex;

	// Token: 0x04000C97 RID: 3223
	private float m_inputDelayTimer;

	// Token: 0x04000C98 RID: 3224
	private float m_baseListSize;

	// Token: 0x04000C99 RID: 3225
	private readonly List<GameObject> m_elements = new List<GameObject>();
}
