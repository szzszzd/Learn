using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000099 RID: 153
public class Console : Terminal
{
	// Token: 0x17000024 RID: 36
	// (get) Token: 0x06000688 RID: 1672 RVA: 0x00031E5B File Offset: 0x0003005B
	public static global::Console instance
	{
		get
		{
			return global::Console.m_instance;
		}
	}

	// Token: 0x06000689 RID: 1673 RVA: 0x00031E64 File Offset: 0x00030064
	public override void Awake()
	{
		base.Awake();
		global::Console.m_instance = this;
		base.AddString(string.Concat(new string[]
		{
			"Valheim ",
			global::Version.GetVersionString(false),
			" (network version ",
			5U.ToString(),
			")"
		}));
		base.AddString("");
		base.AddString("type \"help\" - for commands");
		base.AddString("");
		this.m_chatWindow.gameObject.SetActive(false);
	}

	// Token: 0x0600068A RID: 1674 RVA: 0x00031EF0 File Offset: 0x000300F0
	public override void Update()
	{
		this.m_focused = false;
		if (ZNet.instance && ZNet.instance.InPasswordDialog())
		{
			this.m_chatWindow.gameObject.SetActive(false);
			return;
		}
		if (!this.IsConsoleEnabled())
		{
			return;
		}
		if (ZInput.GetKeyDown(KeyCode.F5) || (global::Console.IsVisible() && ZInput.GetKeyDown(KeyCode.Escape)) || (ZInput.GetButton("JoyLTrigger") && ZInput.GetButton("JoyLBumper") && ZInput.GetButtonDown("JoyStart")))
		{
			this.m_chatWindow.gameObject.SetActive(!this.m_chatWindow.gameObject.activeSelf);
			if (ZInput.IsGamepadActive())
			{
				base.AddString("Gamepad console controls:\n   A: Enter text when empty (only in big picture mode), or send text when not.\n   LB: Erase.\n   DPad up/down: Cycle history.\n   DPad right: Autocomplete.\n   DPad left: Show commands (help).\n   Left Stick: Scroll.\n   RStick + LStick: show/hide console.");
			}
		}
		if (this.m_chatWindow.gameObject.activeInHierarchy)
		{
			this.m_focused = true;
		}
		if (this.m_focused)
		{
			if (ZInput.GetButtonDown("JoyButtonA"))
			{
				if (this.m_input.text.Length == 0)
				{
					base.TryShowGamepadTextInput();
				}
				else
				{
					base.SendInput();
				}
			}
			else if (ZInput.GetButtonDown("JoyTabLeft") && this.m_input.text.Length > 0)
			{
				this.m_input.text = this.m_input.text.Substring(0, this.m_input.text.Length - 1);
			}
			else if (ZInput.GetButtonDown("JoyDPadLeft"))
			{
				base.TryRunCommand("help", false, false);
			}
		}
		string text;
		if (global::Console.instance && Terminal.m_threadSafeConsoleLog.TryDequeue(out text))
		{
			global::Console.instance.AddString(text);
		}
		string msg;
		if (Player.m_localPlayer && Terminal.m_threadSafeMessages.TryDequeue(out msg))
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, msg, 0, null);
		}
		base.Update();
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x000320BA File Offset: 0x000302BA
	public static bool IsVisible()
	{
		return global::Console.m_instance && global::Console.m_instance.m_chatWindow.gameObject.activeInHierarchy;
	}

	// Token: 0x0600068C RID: 1676 RVA: 0x000320DE File Offset: 0x000302DE
	public void Print(string text)
	{
		base.AddString(text);
	}

	// Token: 0x0600068D RID: 1677 RVA: 0x000320E7 File Offset: 0x000302E7
	public bool IsConsoleEnabled()
	{
		return global::Console.m_consoleEnabled;
	}

	// Token: 0x0600068E RID: 1678 RVA: 0x000320EE File Offset: 0x000302EE
	public static void SetConsoleEnabled(bool enabled)
	{
		global::Console.m_consoleEnabled = enabled;
	}

	// Token: 0x17000025 RID: 37
	// (get) Token: 0x0600068F RID: 1679 RVA: 0x00031E5B File Offset: 0x0003005B
	protected override Terminal m_terminalInstance
	{
		get
		{
			return global::Console.m_instance;
		}
	}

	// Token: 0x04000817 RID: 2071
	private static global::Console m_instance;

	// Token: 0x04000818 RID: 2072
	private static bool m_consoleEnabled;

	// Token: 0x04000819 RID: 2073
	public Text m_devTest;
}
