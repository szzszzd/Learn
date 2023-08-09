using System;
using System.Collections.Generic;
using System.Text;
using Fishlabs.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x02000094 RID: 148
public class Chat : Terminal
{
	// Token: 0x17000020 RID: 32
	// (get) Token: 0x06000653 RID: 1619 RVA: 0x00030294 File Offset: 0x0002E494
	public static Chat instance
	{
		get
		{
			return Chat.m_instance;
		}
	}

	// Token: 0x06000654 RID: 1620 RVA: 0x0003029B File Offset: 0x0002E49B
	private void OnDestroy()
	{
		Localization.OnLanguageChange = (Action)Delegate.Remove(Localization.OnLanguageChange, new Action(this.OnLanguageChanged));
	}

	// Token: 0x06000655 RID: 1621 RVA: 0x000302C0 File Offset: 0x0002E4C0
	public override void Awake()
	{
		base.Awake();
		Chat.m_instance = this;
		ZRoutedRpc.instance.Register<Vector3, int, UserInfo, string, string>("ChatMessage", new RoutedMethod<Vector3, int, UserInfo, string, string>.Method(this.RPC_ChatMessage));
		ZRoutedRpc.instance.Register<Vector3, Quaternion, bool>("RPC_TeleportPlayer", new Action<long, Vector3, Quaternion, bool>(this.RPC_TeleportPlayer));
		base.AddString(Localization.instance.Localize("/w [text] - $chat_whisper"));
		base.AddString(Localization.instance.Localize("/s [text] - $chat_shout"));
		base.AddString(Localization.instance.Localize("/die - $chat_kill"));
		base.AddString(Localization.instance.Localize("/resetspawn - $chat_resetspawn"));
		base.AddString(Localization.instance.Localize("/[emote]"));
		StringBuilder stringBuilder = new StringBuilder("Emotes: ");
		for (int i = 0; i < 20; i++)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			Emotes emotes = (Emotes)i;
			stringBuilder2.Append(emotes.ToString().ToLower());
			if (i + 1 < 20)
			{
				stringBuilder.Append(", ");
			}
		}
		base.AddString(Localization.instance.Localize(stringBuilder.ToString()));
		base.AddString("");
		this.m_input.gameObject.SetActive(false);
		this.m_worldTextBase.SetActive(false);
		this.m_tabPrefix = '/';
		this.m_maxVisibleBufferLength = 20;
		Terminal.m_bindList = new List<string>(PlayerPrefs.GetString("ConsoleBindings", "").Split(new char[]
		{
			'\n'
		}));
		if (Terminal.m_bindList.Count == 0)
		{
			base.TryRunCommand("resetbinds", false, false);
		}
		Terminal.updateBinds();
		this.m_autoCompleteSecrets = true;
		Localization.OnLanguageChange = (Action)Delegate.Combine(Localization.OnLanguageChange, new Action(this.OnLanguageChanged));
	}

	// Token: 0x06000656 RID: 1622 RVA: 0x0003047C File Offset: 0x0002E67C
	private void OnLanguageChanged()
	{
		foreach (Chat.NpcText npcText in this.m_npcTexts)
		{
			npcText.UpdateText();
		}
	}

	// Token: 0x06000657 RID: 1623 RVA: 0x000304CC File Offset: 0x0002E6CC
	public bool HasFocus()
	{
		return this.m_chatWindow != null && this.m_chatWindow.gameObject.activeInHierarchy && this.m_input.isFocused;
	}

	// Token: 0x06000658 RID: 1624 RVA: 0x000304FB File Offset: 0x0002E6FB
	public bool IsTakingInput()
	{
		return this.m_input.IsActive();
	}

	// Token: 0x06000659 RID: 1625 RVA: 0x00030508 File Offset: 0x0002E708
	public bool IsChatDialogWindowVisible()
	{
		return this.m_chatWindow.gameObject.activeSelf;
	}

	// Token: 0x0600065A RID: 1626 RVA: 0x0003051C File Offset: 0x0002E71C
	public override void Update()
	{
		this.m_focused = false;
		this.m_hideTimer += Time.deltaTime;
		this.m_chatWindow.gameObject.SetActive(this.m_hideTimer < this.m_hideDelay);
		if (!this.m_wasFocused)
		{
			if (Input.GetKeyDown(KeyCode.Return) && Player.m_localPlayer != null && !global::Console.IsVisible() && !TextInput.IsVisible() && !Minimap.InTextInput() && !Menu.IsVisible() && !InventoryGui.IsVisible())
			{
				this.m_hideTimer = 0f;
				this.m_chatWindow.gameObject.SetActive(true);
				this.m_input.gameObject.SetActive(true);
				this.m_input.ActivateInputField();
			}
			if (ZInput.GetButtonDown("JoyChat") && ZInput.GetButton("JoyAltKeys") && !base.TryShowGamepadTextInput())
			{
				this.m_hideTimer = 0f;
			}
		}
		else if (this.m_wasFocused)
		{
			this.m_hideTimer = 0f;
			this.m_focused = true;
			if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB"))
			{
				EventSystem.current.SetSelectedGameObject(null);
				this.m_input.gameObject.SetActive(false);
				this.m_focused = false;
			}
		}
		this.m_wasFocused = this.m_input.isFocused;
		if (!this.m_input.isFocused && (global::Console.instance == null || !global::Console.instance.m_chatWindow.gameObject.activeInHierarchy))
		{
			foreach (KeyValuePair<KeyCode, List<string>> keyValuePair in Terminal.m_binds)
			{
				if (Input.GetKeyDown(keyValuePair.Key))
				{
					foreach (string text in keyValuePair.Value)
					{
						base.TryRunCommand(text, true, true);
					}
				}
			}
		}
		base.Update();
	}

	// Token: 0x0600065B RID: 1627 RVA: 0x00030754 File Offset: 0x0002E954
	public void Hide()
	{
		this.m_hideTimer = this.m_hideDelay;
	}

	// Token: 0x0600065C RID: 1628 RVA: 0x00030762 File Offset: 0x0002E962
	private void LateUpdate()
	{
		this.UpdateWorldTexts(Time.deltaTime);
		this.UpdateNpcTexts(Time.deltaTime);
	}

	// Token: 0x0600065D RID: 1629 RVA: 0x0003077A File Offset: 0x0002E97A
	protected override void onGamePadTextInput(TextInputEventArgs args)
	{
		base.onGamePadTextInput(args);
		base.SendInput();
	}

	// Token: 0x0600065E RID: 1630 RVA: 0x0003078C File Offset: 0x0002E98C
	public void OnNewChatMessage(GameObject go, long senderID, Vector3 pos, Talker.Type type, UserInfo user, string text, string senderNetworkUserId)
	{
		Action<Profile> <>9__2;
		Action <>9__1;
		PrivilegeManager.CanCommunicateWith(senderNetworkUserId, delegate(PrivilegeManager.Result access)
		{
			Chat <>4__this = this;
			Action displayChatMessage;
			if ((displayChatMessage = <>9__1) == null)
			{
				displayChatMessage = (<>9__1 = delegate()
				{
					if (this == null)
					{
						Debug.LogError("Chat has already been destroyed!");
						return;
					}
					Action<PrivilegeManager.User, Action<Profile>> getProfile = UserInfo.GetProfile;
					PrivilegeManager.User arg = PrivilegeManager.ParseUser(senderNetworkUserId);
					Action<Profile> arg2;
					if ((arg2 = <>9__2) == null)
					{
						arg2 = (<>9__2 = delegate(Profile profile)
						{
							user.UpdateGamertag(profile.Gamertag);
							text = text.Replace('<', ' ');
							text = text.Replace('>', ' ');
							this.m_hideTimer = 0f;
							if (type != Talker.Type.Ping)
							{
								this.AddString(user.GetDisplayName(senderNetworkUserId), text, type, false);
							}
							if (Minimap.instance && Player.m_localPlayer && Minimap.instance.m_mode == Minimap.MapMode.None && Vector3.Distance(Player.m_localPlayer.transform.position, pos) > Minimap.instance.m_nomapPingDistance)
							{
								return;
							}
							this.AddInworldText(go, senderID, pos, type, user, text);
						});
					}
					getProfile(arg, arg2);
				});
			}
			<>4__this.OnCanCommunicateWithResult(access, displayChatMessage);
		});
	}

	// Token: 0x0600065F RID: 1631 RVA: 0x000307F2 File Offset: 0x0002E9F2
	private void OnCanCommunicateWithResult(PrivilegeManager.Result access, Action displayChatMessage)
	{
		if (access == PrivilegeManager.Result.Allowed)
		{
			displayChatMessage();
		}
	}

	// Token: 0x06000660 RID: 1632 RVA: 0x00030800 File Offset: 0x0002EA00
	private void UpdateWorldTexts(float dt)
	{
		Chat.WorldTextInstance worldTextInstance = null;
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		foreach (Chat.WorldTextInstance worldTextInstance2 in this.m_worldTexts)
		{
			worldTextInstance2.m_timer += dt;
			if (worldTextInstance2.m_timer > this.m_worldTextTTL && worldTextInstance == null)
			{
				worldTextInstance = worldTextInstance2;
			}
			Chat.WorldTextInstance worldTextInstance3 = worldTextInstance2;
			worldTextInstance3.m_position.y = worldTextInstance3.m_position.y + dt * 0.15f;
			Vector3 vector = Vector3.zero;
			if (worldTextInstance2.m_go)
			{
				Character component = worldTextInstance2.m_go.GetComponent<Character>();
				if (component)
				{
					vector = component.GetHeadPoint() + Vector3.up * 0.3f;
				}
				else
				{
					vector = worldTextInstance2.m_go.transform.position + Vector3.up * 0.3f;
				}
			}
			else
			{
				vector = worldTextInstance2.m_position + Vector3.up * 0.3f;
			}
			Vector3 vector2 = mainCamera.WorldToScreenPoint(vector);
			if (vector2.x < 0f || vector2.x > (float)Screen.width || vector2.y < 0f || vector2.y > (float)Screen.height || vector2.z < 0f)
			{
				Vector3 vector3 = vector - mainCamera.transform.position;
				bool flag = Vector3.Dot(mainCamera.transform.right, vector3) < 0f;
				Vector3 vector4 = vector3;
				vector4.y = 0f;
				float magnitude = vector4.magnitude;
				float y = vector3.y;
				Vector3 a = mainCamera.transform.forward;
				a.y = 0f;
				a.Normalize();
				a *= magnitude;
				Vector3 b = a + Vector3.up * y;
				vector2 = mainCamera.WorldToScreenPoint(mainCamera.transform.position + b);
				vector2.x = (float)(flag ? 0 : Screen.width);
			}
			RectTransform rectTransform = worldTextInstance2.m_gui.transform as RectTransform;
			vector2.x = Mathf.Clamp(vector2.x, rectTransform.rect.width / 2f, (float)Screen.width - rectTransform.rect.width / 2f);
			vector2.y = Mathf.Clamp(vector2.y, rectTransform.rect.height / 2f, (float)Screen.height - rectTransform.rect.height);
			vector2.z = Mathf.Min(vector2.z, 100f);
			worldTextInstance2.m_gui.transform.position = vector2;
		}
		if (worldTextInstance != null)
		{
			UnityEngine.Object.Destroy(worldTextInstance.m_gui);
			this.m_worldTexts.Remove(worldTextInstance);
		}
	}

	// Token: 0x06000661 RID: 1633 RVA: 0x00030B28 File Offset: 0x0002ED28
	private void AddInworldText(GameObject go, long senderID, Vector3 position, Talker.Type type, UserInfo user, string text)
	{
		Chat.WorldTextInstance worldTextInstance = this.FindExistingWorldText(senderID);
		if (worldTextInstance == null)
		{
			worldTextInstance = new Chat.WorldTextInstance();
			worldTextInstance.m_talkerID = senderID;
			worldTextInstance.m_gui = UnityEngine.Object.Instantiate<GameObject>(this.m_worldTextBase, base.transform);
			worldTextInstance.m_gui.gameObject.SetActive(true);
			Transform transform = worldTextInstance.m_gui.transform.Find("Text");
			worldTextInstance.m_textMeshField = transform.GetComponent<TextMeshProUGUI>();
			this.m_worldTexts.Add(worldTextInstance);
		}
		worldTextInstance.m_userInfo = user;
		worldTextInstance.m_type = type;
		worldTextInstance.m_go = go;
		worldTextInstance.m_position = position;
		Color color;
		switch (type)
		{
		case Talker.Type.Whisper:
			color = new Color(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
			goto IL_106;
		case Talker.Type.Shout:
			color = Color.yellow;
			text = text.ToUpper();
			goto IL_106;
		case Talker.Type.Ping:
			color = new Color(0.6f, 0.7f, 1f, 1f);
			text = "PING";
			goto IL_106;
		}
		color = Color.white;
		IL_106:
		worldTextInstance.m_textMeshField.color = color;
		worldTextInstance.m_timer = 0f;
		worldTextInstance.m_text = text;
		this.UpdateWorldTextField(worldTextInstance);
	}

	// Token: 0x06000662 RID: 1634 RVA: 0x00030C64 File Offset: 0x0002EE64
	private void UpdateWorldTextField(Chat.WorldTextInstance wt)
	{
		string text = "";
		if (wt.m_type == Talker.Type.Shout || wt.m_type == Talker.Type.Ping)
		{
			text = wt.m_name + ": ";
		}
		text += wt.m_text;
		wt.m_textMeshField.text = text;
	}

	// Token: 0x06000663 RID: 1635 RVA: 0x00030CB4 File Offset: 0x0002EEB4
	private Chat.WorldTextInstance FindExistingWorldText(long senderID)
	{
		foreach (Chat.WorldTextInstance worldTextInstance in this.m_worldTexts)
		{
			if (worldTextInstance.m_talkerID == senderID)
			{
				return worldTextInstance;
			}
		}
		return null;
	}

	// Token: 0x06000664 RID: 1636 RVA: 0x00030D10 File Offset: 0x0002EF10
	protected override bool isAllowedCommand(Terminal.ConsoleCommand cmd)
	{
		return !cmd.IsCheat && base.isAllowedCommand(cmd);
	}

	// Token: 0x06000665 RID: 1637 RVA: 0x00030D24 File Offset: 0x0002EF24
	protected override void InputText()
	{
		string text = this.m_input.text;
		if (text.Length == 0)
		{
			return;
		}
		if (text[0] == '/')
		{
			text = text.Substring(1);
		}
		else
		{
			text = "say " + text;
		}
		base.TryRunCommand(text, this, false);
	}

	// Token: 0x06000666 RID: 1638 RVA: 0x00030D75 File Offset: 0x0002EF75
	public void TeleportPlayer(long targetPeerID, Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerID, "RPC_TeleportPlayer", new object[]
		{
			pos,
			rot,
			distantTeleport
		});
	}

	// Token: 0x06000667 RID: 1639 RVA: 0x00030DA9 File Offset: 0x0002EFA9
	private void RPC_TeleportPlayer(long sender, Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		if (Player.m_localPlayer != null)
		{
			Player.m_localPlayer.TeleportTo(pos, rot, distantTeleport);
		}
	}

	// Token: 0x06000668 RID: 1640 RVA: 0x00030DC7 File Offset: 0x0002EFC7
	private void RPC_ChatMessage(long sender, Vector3 position, int type, UserInfo userInfo, string text, string senderAccountId)
	{
		this.OnNewChatMessage(null, sender, position, (Talker.Type)type, userInfo, text, senderAccountId);
	}

	// Token: 0x06000669 RID: 1641 RVA: 0x00030DDC File Offset: 0x0002EFDC
	public void SendText(Talker.Type type, string text)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			if (type == Talker.Type.Shout)
			{
				ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
				{
					localPlayer.GetHeadPoint(),
					2,
					UserInfo.GetLocalUser(),
					text,
					PrivilegeManager.GetNetworkUserId()
				});
				return;
			}
			localPlayer.GetComponent<Talker>().Say(type, text);
		}
	}

	// Token: 0x0600066A RID: 1642 RVA: 0x00030E50 File Offset: 0x0002F050
	public void SendPing(Vector3 position)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			Vector3 vector = position;
			vector.y = localPlayer.transform.position.y;
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
			{
				vector,
				3,
				UserInfo.GetLocalUser(),
				"",
				PrivilegeManager.GetNetworkUserId()
			});
			if (Player.m_debugMode && global::Console.instance != null && global::Console.instance.IsCheatsEnabled() && global::Console.instance != null)
			{
				global::Console.instance.AddString(string.Format("Pinged at: {0}, {1}", vector.x, vector.z));
			}
		}
	}

	// Token: 0x0600066B RID: 1643 RVA: 0x00030F24 File Offset: 0x0002F124
	public void GetShoutWorldTexts(List<Chat.WorldTextInstance> texts)
	{
		foreach (Chat.WorldTextInstance worldTextInstance in this.m_worldTexts)
		{
			if (worldTextInstance.m_type == Talker.Type.Shout)
			{
				texts.Add(worldTextInstance);
			}
		}
	}

	// Token: 0x0600066C RID: 1644 RVA: 0x00030F80 File Offset: 0x0002F180
	public void GetPingWorldTexts(List<Chat.WorldTextInstance> texts)
	{
		foreach (Chat.WorldTextInstance worldTextInstance in this.m_worldTexts)
		{
			if (worldTextInstance.m_type == Talker.Type.Ping)
			{
				texts.Add(worldTextInstance);
			}
		}
	}

	// Token: 0x0600066D RID: 1645 RVA: 0x00030FDC File Offset: 0x0002F1DC
	private void UpdateNpcTexts(float dt)
	{
		Chat.NpcText npcText = null;
		Camera mainCamera = Utils.GetMainCamera();
		foreach (Chat.NpcText npcText2 in this.m_npcTexts)
		{
			if (!npcText2.m_go)
			{
				npcText2.m_gui.SetActive(false);
				if (npcText == null)
				{
					npcText = npcText2;
				}
			}
			else
			{
				if (npcText2.m_timeout)
				{
					npcText2.m_ttl -= dt;
					if (npcText2.m_ttl <= 0f)
					{
						npcText2.SetVisible(false);
						if (!npcText2.IsVisible())
						{
							npcText = npcText2;
							continue;
						}
						continue;
					}
				}
				Vector3 vector = npcText2.m_go.transform.position + npcText2.m_offset;
				Vector3 vector2 = mainCamera.WorldToScreenPoint(vector);
				if (vector2.x < 0f || vector2.x > (float)Screen.width || vector2.y < 0f || vector2.y > (float)Screen.height || vector2.z < 0f)
				{
					npcText2.SetVisible(false);
				}
				else
				{
					npcText2.SetVisible(true);
					RectTransform rectTransform = npcText2.m_gui.transform as RectTransform;
					vector2.x = Mathf.Clamp(vector2.x, rectTransform.rect.width / 2f, (float)Screen.width - rectTransform.rect.width / 2f);
					vector2.y = Mathf.Clamp(vector2.y, rectTransform.rect.height / 2f, (float)Screen.height - rectTransform.rect.height);
					npcText2.m_gui.transform.position = vector2;
				}
				if (Vector3.Distance(mainCamera.transform.position, vector) > npcText2.m_cullDistance)
				{
					npcText2.SetVisible(false);
					if (npcText == null && !npcText2.IsVisible())
					{
						npcText = npcText2;
					}
				}
			}
		}
		if (npcText != null)
		{
			this.ClearNpcText(npcText);
		}
		if (Hud.instance.m_userHidden && this.m_npcTexts.Count > 0)
		{
			this.HideAllNpcTexts();
		}
	}

	// Token: 0x0600066E RID: 1646 RVA: 0x00031228 File Offset: 0x0002F428
	public void HideAllNpcTexts()
	{
		for (int i = this.m_npcTexts.Count - 1; i >= 0; i--)
		{
			this.m_npcTexts[i].SetVisible(false);
			this.ClearNpcText(this.m_npcTexts[i]);
		}
	}

	// Token: 0x0600066F RID: 1647 RVA: 0x00031274 File Offset: 0x0002F474
	public void SetNpcText(GameObject talker, Vector3 offset, float cullDistance, float ttl, string topic, string text, bool large)
	{
		if (Hud.instance.m_userHidden)
		{
			return;
		}
		Chat.NpcText npcText = this.FindNpcText(talker);
		if (npcText != null)
		{
			this.ClearNpcText(npcText);
		}
		npcText = new Chat.NpcText();
		npcText.m_topic = topic;
		npcText.m_text = text;
		npcText.m_go = talker;
		npcText.m_gui = UnityEngine.Object.Instantiate<GameObject>(large ? this.m_npcTextBaseLarge : this.m_npcTextBase, base.transform);
		npcText.m_gui.SetActive(true);
		npcText.m_animator = npcText.m_gui.GetComponent<Animator>();
		npcText.m_topicField = npcText.m_gui.transform.Find("Topic").GetComponent<TextMeshProUGUI>();
		npcText.m_textField = npcText.m_gui.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		npcText.m_ttl = ttl;
		npcText.m_timeout = (ttl > 0f);
		npcText.m_offset = offset;
		npcText.m_cullDistance = cullDistance;
		npcText.UpdateText();
		this.m_npcTexts.Add(npcText);
	}

	// Token: 0x06000670 RID: 1648 RVA: 0x00031374 File Offset: 0x0002F574
	public int CurrentNpcTexts()
	{
		return this.m_npcTexts.Count;
	}

	// Token: 0x06000671 RID: 1649 RVA: 0x00031384 File Offset: 0x0002F584
	public bool IsDialogVisible(GameObject talker)
	{
		Chat.NpcText npcText = this.FindNpcText(talker);
		return npcText != null && npcText.IsVisible();
	}

	// Token: 0x06000672 RID: 1650 RVA: 0x000313A4 File Offset: 0x0002F5A4
	public void ClearNpcText(GameObject talker)
	{
		Chat.NpcText npcText = this.FindNpcText(talker);
		if (npcText != null)
		{
			this.ClearNpcText(npcText);
		}
	}

	// Token: 0x06000673 RID: 1651 RVA: 0x000313C3 File Offset: 0x0002F5C3
	private void ClearNpcText(Chat.NpcText npcText)
	{
		UnityEngine.Object.Destroy(npcText.m_gui);
		this.m_npcTexts.Remove(npcText);
	}

	// Token: 0x06000674 RID: 1652 RVA: 0x000313E0 File Offset: 0x0002F5E0
	private Chat.NpcText FindNpcText(GameObject go)
	{
		foreach (Chat.NpcText npcText in this.m_npcTexts)
		{
			if (npcText.m_go == go)
			{
				return npcText;
			}
		}
		return null;
	}

	// Token: 0x17000021 RID: 33
	// (get) Token: 0x06000675 RID: 1653 RVA: 0x00030294 File Offset: 0x0002E494
	protected override Terminal m_terminalInstance
	{
		get
		{
			return Chat.m_instance;
		}
	}

	// Token: 0x040007CA RID: 1994
	private static Chat m_instance;

	// Token: 0x040007CB RID: 1995
	public float m_hideDelay = 10f;

	// Token: 0x040007CC RID: 1996
	public float m_worldTextTTL = 5f;

	// Token: 0x040007CD RID: 1997
	public GameObject m_worldTextBase;

	// Token: 0x040007CE RID: 1998
	public GameObject m_npcTextBase;

	// Token: 0x040007CF RID: 1999
	public GameObject m_npcTextBaseLarge;

	// Token: 0x040007D0 RID: 2000
	private List<Chat.WorldTextInstance> m_worldTexts = new List<Chat.WorldTextInstance>();

	// Token: 0x040007D1 RID: 2001
	private List<Chat.NpcText> m_npcTexts = new List<Chat.NpcText>();

	// Token: 0x040007D2 RID: 2002
	private float m_hideTimer = 9999f;

	// Token: 0x040007D3 RID: 2003
	public bool m_wasFocused;

	// Token: 0x02000095 RID: 149
	public class WorldTextInstance
	{
		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000677 RID: 1655 RVA: 0x00031483 File Offset: 0x0002F683
		public string m_name
		{
			get
			{
				return this.m_userInfo.GetDisplayName(this.m_userInfo.NetworkUserId);
			}
		}

		// Token: 0x040007D4 RID: 2004
		public UserInfo m_userInfo;

		// Token: 0x040007D5 RID: 2005
		public long m_talkerID;

		// Token: 0x040007D6 RID: 2006
		public GameObject m_go;

		// Token: 0x040007D7 RID: 2007
		public Vector3 m_position;

		// Token: 0x040007D8 RID: 2008
		public float m_timer;

		// Token: 0x040007D9 RID: 2009
		public GameObject m_gui;

		// Token: 0x040007DA RID: 2010
		public TextMeshProUGUI m_textMeshField;

		// Token: 0x040007DB RID: 2011
		public Talker.Type m_type;

		// Token: 0x040007DC RID: 2012
		public string m_text = "";
	}

	// Token: 0x02000096 RID: 150
	public class NpcText
	{
		// Token: 0x06000679 RID: 1657 RVA: 0x000314AE File Offset: 0x0002F6AE
		public void SetVisible(bool visible)
		{
			this.m_animator.SetBool("visible", visible);
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x000314C4 File Offset: 0x0002F6C4
		public bool IsVisible()
		{
			return this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("visible") || this.m_animator.GetBool("visible");
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x00031500 File Offset: 0x0002F700
		public void UpdateText()
		{
			if (this.m_topic.Length > 0)
			{
				this.m_textField.text = "<color=orange>" + Localization.instance.Localize(this.m_topic) + "</color>\n" + Localization.instance.Localize(this.m_text);
				return;
			}
			this.m_textField.text = Localization.instance.Localize(this.m_text);
		}

		// Token: 0x040007DD RID: 2013
		public string m_topic;

		// Token: 0x040007DE RID: 2014
		public string m_text;

		// Token: 0x040007DF RID: 2015
		public GameObject m_go;

		// Token: 0x040007E0 RID: 2016
		public Vector3 m_offset = Vector3.zero;

		// Token: 0x040007E1 RID: 2017
		public float m_cullDistance = 20f;

		// Token: 0x040007E2 RID: 2018
		public GameObject m_gui;

		// Token: 0x040007E3 RID: 2019
		public Animator m_animator;

		// Token: 0x040007E4 RID: 2020
		public TextMeshProUGUI m_textField;

		// Token: 0x040007E5 RID: 2021
		public TextMeshProUGUI m_topicField;

		// Token: 0x040007E6 RID: 2022
		public float m_ttl;

		// Token: 0x040007E7 RID: 2023
		public bool m_timeout;
	}
}
