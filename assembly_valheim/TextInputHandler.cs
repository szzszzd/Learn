using System;
using Steamworks;

// Token: 0x020000AC RID: 172
public class TextInputHandler
{
	// Token: 0x06000764 RID: 1892 RVA: 0x00038CEC File Offset: 0x00036EEC
	public TextInputHandler(TextInputEvent onTextInput = null)
	{
		this.m_onTextInput = onTextInput;
		this.m_GamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(new Callback<GamepadTextInputDismissed_t>.DispatchDelegate(this.OnGamepadTextInputDismissed));
	}

	// Token: 0x06000765 RID: 1893 RVA: 0x00038D12 File Offset: 0x00036F12
	public bool TryOpenTextInput(int maxLength = 0, string prompt = "", string existingText = "")
	{
		if (SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, prompt, (uint)maxLength, existingText))
		{
			this.m_waitingForCallback = true;
			return true;
		}
		return false;
	}

	// Token: 0x06000766 RID: 1894 RVA: 0x00038D2C File Offset: 0x00036F2C
	private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t pCallback)
	{
		if (this.m_waitingForCallback)
		{
			string text = "";
			if (pCallback.m_bSubmitted)
			{
				this.m_waitingForCallback = false;
				SteamUtils.GetEnteredGamepadTextInput(out text, pCallback.m_unSubmittedText + 1U);
			}
			TextInputEvent onTextInput = this.m_onTextInput;
			if (onTextInput == null)
			{
				return;
			}
			onTextInput(new TextInputEventArgs
			{
				m_submitted = pCallback.m_bSubmitted,
				m_text = text
			});
		}
	}

	// Token: 0x04000903 RID: 2307
	private Callback<GamepadTextInputDismissed_t> m_GamepadTextInputDismissed;

	// Token: 0x04000904 RID: 2308
	public TextInputEvent m_onTextInput;

	// Token: 0x04000905 RID: 2309
	public bool m_waitingForCallback;
}
