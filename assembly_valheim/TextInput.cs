using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000102 RID: 258
public class TextInput : MonoBehaviour
{
	// Token: 0x06000A83 RID: 2691 RVA: 0x000500D8 File Offset: 0x0004E2D8
	private void Awake()
	{
		TextInput.m_instance = this;
		this.m_panel.SetActive(false);
		this.m_gamepadTextInput = new TextInputHandler(delegate(TextInputEventArgs args)
		{
			if (args.m_submitted)
			{
				if (this.m_textFieldTMP != null)
				{
					this.m_textFieldTMP.text = args.m_text;
					Action<TMP_InputField, string> onGamepadSubmitTMP = this.m_onGamepadSubmitTMP;
					if (onGamepadSubmitTMP != null)
					{
						onGamepadSubmitTMP(this.m_textFieldTMP, args.m_text);
					}
				}
				else if (this.m_textField != null)
				{
					this.m_textField.text = args.m_text;
					Action<InputField, string> onGamepadSubmit = this.m_onGamepadSubmit;
					if (onGamepadSubmit != null)
					{
						onGamepadSubmit(this.m_textField, args.m_text);
					}
				}
				this.setText(args.m_text);
				return;
			}
			if (this.m_textFieldTMP != null)
			{
				Action<TMP_InputField> onGamepadCancelTMP = this.m_onGamepadCancelTMP;
				if (onGamepadCancelTMP == null)
				{
					return;
				}
				onGamepadCancelTMP(this.m_textFieldTMP);
				return;
			}
			else
			{
				Action<InputField> onGamepadCancel = this.m_onGamepadCancel;
				if (onGamepadCancel == null)
				{
					return;
				}
				onGamepadCancel(this.m_textField);
				return;
			}
		});
	}

	// Token: 0x1700005E RID: 94
	// (get) Token: 0x06000A84 RID: 2692 RVA: 0x00050103 File Offset: 0x0004E303
	public static TextInput instance
	{
		get
		{
			return TextInput.m_instance;
		}
	}

	// Token: 0x06000A85 RID: 2693 RVA: 0x0005010A File Offset: 0x0004E30A
	private void OnDestroy()
	{
		TextInput.m_instance = null;
	}

	// Token: 0x06000A86 RID: 2694 RVA: 0x00050112 File Offset: 0x0004E312
	public static bool IsVisible()
	{
		return TextInput.m_instance && TextInput.m_instance.m_visibleFrame;
	}

	// Token: 0x06000A87 RID: 2695 RVA: 0x0005012C File Offset: 0x0004E32C
	private void Update()
	{
		this.m_visibleFrame = TextInput.m_instance.m_panel.gameObject.activeSelf;
		if (!this.m_visibleFrame)
		{
			return;
		}
		if (global::Console.IsVisible() || Chat.instance.HasFocus())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.Hide();
			return;
		}
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			this.OnEnter();
		}
		if (this.m_textField != null)
		{
			if (!this.m_textField.isFocused)
			{
				EventSystem.current.SetSelectedGameObject(this.m_textField.gameObject);
				return;
			}
		}
		else if (this.m_textFieldTMP != null && !this.m_textFieldTMP.isFocused)
		{
			EventSystem.current.SetSelectedGameObject(this.m_textFieldTMP.gameObject);
		}
	}

	// Token: 0x06000A88 RID: 2696 RVA: 0x000501FC File Offset: 0x0004E3FC
	public void OnCancel()
	{
		this.Hide();
	}

	// Token: 0x06000A89 RID: 2697 RVA: 0x00050204 File Offset: 0x0004E404
	public void OnEnter()
	{
		if (this.m_textField != null)
		{
			this.setText(this.m_textField.text);
		}
		else if (this.m_textFieldTMP != null)
		{
			this.setText(this.m_textFieldTMP.text.Replace("\\n", "\n").Replace("\\t", "\t"));
		}
		this.Hide();
	}

	// Token: 0x06000A8A RID: 2698 RVA: 0x00050275 File Offset: 0x0004E475
	private void setText(string text)
	{
		if (this.m_queuedSign != null)
		{
			this.m_queuedSign.SetText(text);
			this.m_queuedSign = null;
		}
	}

	// Token: 0x06000A8B RID: 2699 RVA: 0x00050292 File Offset: 0x0004E492
	public void RequestText(TextReceiver sign, string topic, int charLimit)
	{
		this.m_queuedSign = sign;
		if (!this.m_gamepadTextInput.TryOpenTextInput(charLimit, Localization.instance.Localize(topic), ""))
		{
			this.Show(topic, sign.GetText(), charLimit);
		}
	}

	// Token: 0x06000A8C RID: 2700 RVA: 0x000502C8 File Offset: 0x0004E4C8
	private void Show(string topic, string text, int charLimit)
	{
		this.m_panel.SetActive(true);
		if (this.m_textField != null)
		{
			this.m_textField.text = text;
		}
		else if (this.m_textFieldTMP != null)
		{
			this.m_textFieldTMP.text = text;
		}
		if (this.m_topic != null)
		{
			this.m_topic.text = Localization.instance.Localize(topic);
		}
		else if (this.m_topicTMP != null)
		{
			this.m_topicTMP.text = Localization.instance.Localize(topic);
		}
		if (this.m_textField != null)
		{
			this.m_textField.characterLimit = charLimit;
			this.m_textField.ActivateInputField();
			return;
		}
		if (this.m_textFieldTMP != null)
		{
			this.m_textFieldTMP.characterLimit = charLimit;
			this.m_textFieldTMP.ActivateInputField();
		}
	}

	// Token: 0x06000A8D RID: 2701 RVA: 0x000503AC File Offset: 0x0004E5AC
	public void Hide()
	{
		this.m_panel.SetActive(false);
	}

	// Token: 0x04000CB7 RID: 3255
	private static TextInput m_instance;

	// Token: 0x04000CB8 RID: 3256
	private TextInputHandler m_gamepadTextInput;

	// Token: 0x04000CB9 RID: 3257
	public Action<InputField> m_onGamepadCancel;

	// Token: 0x04000CBA RID: 3258
	public Action<InputField, string> m_onGamepadSubmit;

	// Token: 0x04000CBB RID: 3259
	public Action<TMP_InputField> m_onGamepadCancelTMP;

	// Token: 0x04000CBC RID: 3260
	public Action<TMP_InputField, string> m_onGamepadSubmitTMP;

	// Token: 0x04000CBD RID: 3261
	private bool m_waitingForCallback;

	// Token: 0x04000CBE RID: 3262
	public GameObject m_panel;

	// Token: 0x04000CBF RID: 3263
	public InputField m_textField;

	// Token: 0x04000CC0 RID: 3264
	public Text m_topic;

	// Token: 0x04000CC1 RID: 3265
	public TMP_InputField m_textFieldTMP;

	// Token: 0x04000CC2 RID: 3266
	public TextMeshProUGUI m_topicTMP;

	// Token: 0x04000CC3 RID: 3267
	private TextReceiver m_queuedSign;

	// Token: 0x04000CC4 RID: 3268
	private bool m_visibleFrame;
}
