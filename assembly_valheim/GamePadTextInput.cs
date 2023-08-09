using System;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020000AB RID: 171
public class GamePadTextInput : MonoBehaviour, ISubmitHandler, IEventSystemHandler, ISelectHandler
{
	// Token: 0x0600075B RID: 1883 RVA: 0x00038A7C File Offset: 0x00036C7C
	private void Awake()
	{
		this.m_input = base.GetComponent<InputField>();
		this.m_gamepadInput = base.GetComponent<UIGamePad>();
		if (this.m_input && this.m_input.characterLimit > 0)
		{
			this.m_maxLength = this.m_input.characterLimit;
		}
		this.m_gamepadTextInput = new TextInputHandler(delegate(TextInputEventArgs args)
		{
			if (args.m_submitted)
			{
				if (this.m_input != null)
				{
					this.m_input.text = args.m_text;
				}
				Action<InputField, string> onSubmit = this.m_onSubmit;
				if (onSubmit == null)
				{
					return;
				}
				onSubmit(this.m_input, args.m_text);
				return;
			}
			else
			{
				Action<InputField> onCancel = this.m_onCancel;
				if (onCancel == null)
				{
					return;
				}
				onCancel(this.m_input);
				return;
			}
		});
	}

	// Token: 0x0600075C RID: 1884 RVA: 0x00038AE4 File Offset: 0x00036CE4
	private void Update()
	{
		if (this.m_input.gameObject == EventSystem.current.currentSelectedGameObject)
		{
			if (this.m_gamepadInput != null)
			{
				if (this.m_gamepadInput.ButtonPressed())
				{
					this.OpenTextInput();
				}
			}
			else if (ZInput.GetButtonDown("JoyButtonA"))
			{
				this.OpenTextInput();
			}
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				GamePadTextInput.<Update>g__trySelect|1_0(this.m_input.FindSelectableOnDown());
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				GamePadTextInput.<Update>g__trySelect|1_0(this.m_input.FindSelectableOnUp());
			}
			if (ZInput.GetButtonDown("JoyLStickLeft") || ZInput.GetButtonDown("JoyDPadLeft"))
			{
				GamePadTextInput.<Update>g__trySelect|1_0(this.m_input.FindSelectableOnLeft());
			}
			if (ZInput.GetButtonDown("JoyLStickRight") || ZInput.GetButtonDown("JoyDPadRight"))
			{
				GamePadTextInput.<Update>g__trySelect|1_0(this.m_input.FindSelectableOnRight());
			}
		}
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x00038BE5 File Offset: 0x00036DE5
	private void OnEnable()
	{
		if (this.m_openOnEnable && ZInput.IsGamepadActive())
		{
			this.OpenTextInput();
		}
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x00038BFC File Offset: 0x00036DFC
	public void OnSelect(BaseEventData eventData)
	{
		if ((Settings.IsSteamRunningOnSteamDeck() || SteamUtils.IsSteamInBigPictureMode()) && !ZInput.IsGamepadActive())
		{
			this.OpenTextInput();
		}
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x00038C19 File Offset: 0x00036E19
	public void OnSubmit(BaseEventData eventData)
	{
		if (ZInput.IsGamepadActive())
		{
			this.OpenTextInput();
		}
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x00038C28 File Offset: 0x00036E28
	public void OpenTextInput()
	{
		this.m_gamepadTextInput.TryOpenTextInput(this.m_maxLength, Localization.instance.Localize(this.m_description), Localization.instance.Localize(this.m_existingText));
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x00038CD3 File Offset: 0x00036ED3
	[CompilerGenerated]
	internal static void <Update>g__trySelect|1_0(Selectable sel)
	{
		if (sel != null && sel.interactable)
		{
			sel.Select();
		}
	}

	// Token: 0x040008F8 RID: 2296
	private InputField m_input;

	// Token: 0x040008F9 RID: 2297
	private Selectable m_nextSelect;

	// Token: 0x040008FA RID: 2298
	private UIGamePad m_gamepadInput;

	// Token: 0x040008FB RID: 2299
	private TextInputHandler m_gamepadTextInput;

	// Token: 0x040008FC RID: 2300
	public string m_description;

	// Token: 0x040008FD RID: 2301
	public int m_maxLength = 64;

	// Token: 0x040008FE RID: 2302
	public string m_existingText;

	// Token: 0x040008FF RID: 2303
	public bool m_openOnEnable;

	// Token: 0x04000900 RID: 2304
	[global::Tooltip("Gamepads get stuck when navigating to InputFields in Unity for some unfathomable reason, so this hack moves us away manually.")]
	public bool m_forceGamepadMoveAway;

	// Token: 0x04000901 RID: 2305
	public Action<InputField> m_onCancel;

	// Token: 0x04000902 RID: 2306
	public Action<InputField, string> m_onSubmit;
}
