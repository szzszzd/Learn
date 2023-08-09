using System;
using Fishlabs.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UserManagement;

namespace Valheim.UI
{
	// Token: 0x020002D8 RID: 728
	public class SessionPlayerListEntry : MonoBehaviour
	{
		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06001B86 RID: 7046 RVA: 0x000B8654 File Offset: 0x000B6854
		// (remove) Token: 0x06001B87 RID: 7047 RVA: 0x000B8688 File Offset: 0x000B6888
		public static event Action<ulong> OnViewCardEvent;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06001B88 RID: 7048 RVA: 0x000B86BC File Offset: 0x000B68BC
		// (remove) Token: 0x06001B89 RID: 7049 RVA: 0x000B86F0 File Offset: 0x000B68F0
		public static event Action<ulong, Action<ulong, Profile>> OnRemoveCallbacksEvent;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06001B8A RID: 7050 RVA: 0x000B8724 File Offset: 0x000B6924
		// (remove) Token: 0x06001B8B RID: 7051 RVA: 0x000B8758 File Offset: 0x000B6958
		public static event Action<ulong, Action<ulong, Profile>> OnGetProfileEvent;

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06001B8C RID: 7052 RVA: 0x000B878B File Offset: 0x000B698B
		public bool IsSelected
		{
			get
			{
				return this._selection.enabled;
			}
		}

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x06001B8D RID: 7053 RVA: 0x000B8798 File Offset: 0x000B6998
		// (remove) Token: 0x06001B8E RID: 7054 RVA: 0x000B87D0 File Offset: 0x000B69D0
		public event Action<SessionPlayerListEntry> OnKicked;

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06001B8F RID: 7055 RVA: 0x000B8805 File Offset: 0x000B6A05
		public Selectable FocusObject
		{
			get
			{
				return this._focusPoint;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06001B90 RID: 7056 RVA: 0x000B880D File Offset: 0x000B6A0D
		public Selectable MuteButton
		{
			get
			{
				return this._muteButton;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06001B91 RID: 7057 RVA: 0x000B8815 File Offset: 0x000B6A15
		public Selectable BlockButton
		{
			get
			{
				return this._blockButton;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06001B92 RID: 7058 RVA: 0x000B881D File Offset: 0x000B6A1D
		public Selectable KickButton
		{
			get
			{
				return this._kickButton;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06001B93 RID: 7059 RVA: 0x000B8825 File Offset: 0x000B6A25
		public PrivilegeManager.User User
		{
			get
			{
				return this._user;
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06001B94 RID: 7060 RVA: 0x000B882D File Offset: 0x000B6A2D
		public bool HasFocusObject
		{
			get
			{
				return this._focusPoint.gameObject.activeSelf;
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06001B95 RID: 7061 RVA: 0x000B883F File Offset: 0x000B6A3F
		public bool HasMute
		{
			get
			{
				return this._muteButtonImage.gameObject.activeSelf;
			}
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06001B96 RID: 7062 RVA: 0x000B8851 File Offset: 0x000B6A51
		public bool HasBlock
		{
			get
			{
				return this._blockButtonImage.gameObject.activeSelf;
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06001B97 RID: 7063 RVA: 0x000B8863 File Offset: 0x000B6A63
		public bool HasKick
		{
			get
			{
				return this._kickButtonImage.gameObject.activeSelf;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x06001B98 RID: 7064 RVA: 0x000B8875 File Offset: 0x000B6A75
		public bool HasActivatedButtons
		{
			get
			{
				return this._muteButtonImage.gameObject.activeSelf || this._blockButtonImage.gameObject.activeSelf || this._kickButtonImage.gameObject.activeSelf;
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06001B99 RID: 7065 RVA: 0x000B88AD File Offset: 0x000B6AAD
		private bool IsXbox
		{
			get
			{
				return this._user.platform == PrivilegeManager.Platform.Xbox;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06001B9A RID: 7066 RVA: 0x000B88BD File Offset: 0x000B6ABD
		private bool IsSteam
		{
			get
			{
				return this._user.platform == PrivilegeManager.Platform.Steam;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06001B9B RID: 7067 RVA: 0x000B88CD File Offset: 0x000B6ACD
		// (set) Token: 0x06001B9C RID: 7068 RVA: 0x000B88DF File Offset: 0x000B6ADF
		public bool IsOwnPlayer
		{
			get
			{
				return this._outline.gameObject.activeSelf;
			}
			set
			{
				this._outline.gameObject.SetActive(value);
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06001B9D RID: 7069 RVA: 0x000B88F2 File Offset: 0x000B6AF2
		// (set) Token: 0x06001B9E RID: 7070 RVA: 0x000B8904 File Offset: 0x000B6B04
		public bool IsHost
		{
			get
			{
				return this._hostIcon.gameObject.activeSelf;
			}
			set
			{
				this._hostIcon.gameObject.SetActive(value);
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06001B9F RID: 7071 RVA: 0x000B8863 File Offset: 0x000B6A63
		// (set) Token: 0x06001BA0 RID: 7072 RVA: 0x000B8917 File Offset: 0x000B6B17
		private bool CanBeKicked
		{
			get
			{
				return this._kickButtonImage.gameObject.activeSelf;
			}
			set
			{
				this._kickButtonImage.gameObject.SetActive(value && !this.IsHost);
			}
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06001BA1 RID: 7073 RVA: 0x000B8851 File Offset: 0x000B6A51
		// (set) Token: 0x06001BA2 RID: 7074 RVA: 0x000B8938 File Offset: 0x000B6B38
		private bool CanBeBlocked
		{
			get
			{
				return this._blockButtonImage.gameObject.activeSelf;
			}
			set
			{
				this._blockButtonImage.gameObject.SetActive(value);
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06001BA3 RID: 7075 RVA: 0x000B883F File Offset: 0x000B6A3F
		// (set) Token: 0x06001BA4 RID: 7076 RVA: 0x000B894B File Offset: 0x000B6B4B
		private bool CanBeMuted
		{
			get
			{
				return this._muteButtonImage.gameObject.activeSelf;
			}
			set
			{
				this._muteButtonImage.gameObject.SetActive(value);
			}
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06001BA5 RID: 7077 RVA: 0x000B895E File Offset: 0x000B6B5E
		// (set) Token: 0x06001BA6 RID: 7078 RVA: 0x000B8966 File Offset: 0x000B6B66
		public string Gamertag
		{
			get
			{
				return this._gamertag;
			}
			set
			{
				this._gamertag = value;
				this._gamertagText.text = this._gamertag + ((this.IsHost && this.IsXbox) ? " (Host)" : "");
			}
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06001BA7 RID: 7079 RVA: 0x000B89A1 File Offset: 0x000B6BA1
		// (set) Token: 0x06001BA8 RID: 7080 RVA: 0x000B89AC File Offset: 0x000B6BAC
		public string CharacterName
		{
			get
			{
				return this._characterName;
			}
			set
			{
				this._characterName = (this.IsOwnPlayer ? value : CensorShittyWords.FilterUGC(value, UGCType.CharacterName));
				this._characterNameText.text = this._characterName + ((this.IsHost && !this.IsXbox) ? " (Host)" : "");
			}
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x000B8A03 File Offset: 0x000B6C03
		private void Awake()
		{
			this._selection.enabled = false;
			this._viewPlayerCard.SetActive(false);
			if (this._button != null)
			{
				this._button.enabled = true;
			}
		}

		// Token: 0x06001BAA RID: 7082 RVA: 0x000B8A38 File Offset: 0x000B6C38
		private void Update()
		{
			if (EventSystem.current != null && (EventSystem.current.currentSelectedGameObject == this._focusPoint.gameObject || EventSystem.current.currentSelectedGameObject == this._muteButton.gameObject || EventSystem.current.currentSelectedGameObject == this._blockButton.gameObject || EventSystem.current.currentSelectedGameObject == this._kickButton.gameObject || EventSystem.current.currentSelectedGameObject == this._button.gameObject))
			{
				this.SelectEntry();
			}
			else
			{
				this.Deselect();
			}
			this.UpdateFocusPoint();
		}

		// Token: 0x06001BAB RID: 7083 RVA: 0x000B8AF5 File Offset: 0x000B6CF5
		public void SelectEntry()
		{
			this._selection.enabled = true;
			this._viewPlayerCard.SetActive(this.IsXbox);
		}

		// Token: 0x06001BAC RID: 7084 RVA: 0x000B8B14 File Offset: 0x000B6D14
		public void Deselect()
		{
			this._selection.enabled = false;
			this._viewPlayerCard.SetActive(false);
		}

		// Token: 0x06001BAD RID: 7085 RVA: 0x000B8B30 File Offset: 0x000B6D30
		public void OnMute()
		{
			if (MuteList.IsMuted(this._user.ToString()))
			{
				MuteList.Unmute(this._user.ToString());
			}
			else
			{
				MuteList.Mute(this._user.ToString());
			}
			this.UpdateMuteButton();
		}

		// Token: 0x06001BAE RID: 7086 RVA: 0x000B8B8C File Offset: 0x000B6D8C
		public void OnBlock()
		{
			if (BlockList.IsPlatformBlocked(this._user.ToString()))
			{
				this.OnViewCard();
				return;
			}
			if (BlockList.IsGameBlocked(this._user.ToString()))
			{
				BlockList.Unblock(this._user.ToString());
			}
			else
			{
				BlockList.Block(this._user.ToString());
			}
			this.UpdateBlockButton();
		}

		// Token: 0x06001BAF RID: 7087 RVA: 0x000B8C04 File Offset: 0x000B6E04
		private void UpdateButtons()
		{
			this.UpdateMuteButton();
			this.UpdateBlockButton();
			this.UpdateFocusPoint();
		}

		// Token: 0x06001BB0 RID: 7088 RVA: 0x000B8C18 File Offset: 0x000B6E18
		private void UpdateFocusPoint()
		{
			this._focusPoint.gameObject.SetActive(!this.HasActivatedButtons);
		}

		// Token: 0x06001BB1 RID: 7089 RVA: 0x000B8C33 File Offset: 0x000B6E33
		private void UpdateMuteButton()
		{
			this._muteButtonImage.sprite = (MuteList.IsMuted(this._user.ToString()) ? this._unmuteSprite : this._muteSprite);
		}

		// Token: 0x06001BB2 RID: 7090 RVA: 0x000B8C66 File Offset: 0x000B6E66
		public void UpdateBlockButton()
		{
			this._blockButtonImage.sprite = (BlockList.IsBlocked(this._user.ToString()) ? this._unblockSprite : this._blockSprite);
		}

		// Token: 0x06001BB3 RID: 7091 RVA: 0x000B8C9C File Offset: 0x000B6E9C
		public void OnKick()
		{
			if (ZNet.instance != null)
			{
				UnifiedPopup.Push(new YesNoPopup("$menu_kick_player_title", Localization.instance.Localize("$menu_kick_player", new string[]
				{
					this.CharacterName
				}), delegate()
				{
					ZNet.instance.Kick(this.CharacterName);
					Action<SessionPlayerListEntry> onKicked = this.OnKicked;
					if (onKicked != null)
					{
						onKicked(this);
					}
					UnifiedPopup.Pop();
				}, delegate()
				{
					UnifiedPopup.Pop();
				}, true));
			}
		}

		// Token: 0x06001BB4 RID: 7092 RVA: 0x000B8D10 File Offset: 0x000B6F10
		public void SetValues(string characterName, PrivilegeManager.User user, bool isHost, bool canBeBlocked, bool canBeKicked, bool canBeMuted)
		{
			this._user = user;
			this.IsHost = isHost;
			this.CharacterName = characterName;
			this.Gamertag = "";
			this.CanBeKicked = false;
			this.CanBeBlocked = false;
			this.CanBeMuted = false;
			if (this.IsSteam)
			{
				this._gamerpic.sprite = this.otherPlatformPlayerPic;
			}
			else
			{
				this._gamerpic.sprite = this.otherPlatformPlayerPic;
			}
			this.UpdateButtons();
		}

		// Token: 0x06001BB5 RID: 7093 RVA: 0x000B8D84 File Offset: 0x000B6F84
		private void UpdateProfile(ulong _, Profile userProfile)
		{
			this.Gamertag = userProfile.UniqueGamertag;
			base.StartCoroutine(this._gamerpic.SetSpriteFromUri(userProfile.PictureUri));
			this.UpdateButtons();
		}

		// Token: 0x06001BB6 RID: 7094 RVA: 0x000B8DB2 File Offset: 0x000B6FB2
		public void OnViewCard()
		{
			Action<ulong> onViewCardEvent = SessionPlayerListEntry.OnViewCardEvent;
			if (onViewCardEvent == null)
			{
				return;
			}
			onViewCardEvent(this._user.id);
		}

		// Token: 0x06001BB7 RID: 7095 RVA: 0x000B8DCE File Offset: 0x000B6FCE
		public void RemoveCallbacks()
		{
			Action<ulong, Action<ulong, Profile>> onRemoveCallbacksEvent = SessionPlayerListEntry.OnRemoveCallbacksEvent;
			if (onRemoveCallbacksEvent == null)
			{
				return;
			}
			onRemoveCallbacksEvent(this._user.id, new Action<ulong, Profile>(this.UpdateProfile));
		}

		// Token: 0x04001DCA RID: 7626
		[SerializeField]
		protected Button _button;

		// Token: 0x04001DCB RID: 7627
		[SerializeField]
		protected Selectable _focusPoint;

		// Token: 0x04001DCC RID: 7628
		[SerializeField]
		protected Image _selection;

		// Token: 0x04001DCD RID: 7629
		[SerializeField]
		protected GameObject _viewPlayerCard;

		// Token: 0x04001DCE RID: 7630
		[SerializeField]
		protected Image _outline;

		// Token: 0x04001DCF RID: 7631
		[SerializeField]
		[Header("Player")]
		protected Image _hostIcon;

		// Token: 0x04001DD0 RID: 7632
		[SerializeField]
		protected Image _gamerpic;

		// Token: 0x04001DD1 RID: 7633
		[SerializeField]
		protected Sprite otherPlatformPlayerPic;

		// Token: 0x04001DD2 RID: 7634
		[SerializeField]
		protected TextMeshProUGUI _gamertagText;

		// Token: 0x04001DD3 RID: 7635
		[SerializeField]
		protected TextMeshProUGUI _characterNameText;

		// Token: 0x04001DD4 RID: 7636
		[SerializeField]
		[Header("Mute")]
		protected Button _muteButton;

		// Token: 0x04001DD5 RID: 7637
		[SerializeField]
		protected Image _muteButtonImage;

		// Token: 0x04001DD6 RID: 7638
		[SerializeField]
		protected Sprite _muteSprite;

		// Token: 0x04001DD7 RID: 7639
		[SerializeField]
		protected Sprite _unmuteSprite;

		// Token: 0x04001DD8 RID: 7640
		[SerializeField]
		[Header("Block")]
		protected Button _blockButton;

		// Token: 0x04001DD9 RID: 7641
		[SerializeField]
		protected Image _blockButtonImage;

		// Token: 0x04001DDA RID: 7642
		[SerializeField]
		protected Sprite _blockSprite;

		// Token: 0x04001DDB RID: 7643
		[SerializeField]
		protected Sprite _unblockSprite;

		// Token: 0x04001DDC RID: 7644
		[SerializeField]
		[Header("Kick")]
		protected Button _kickButton;

		// Token: 0x04001DDD RID: 7645
		[SerializeField]
		protected Image _kickButtonImage;

		// Token: 0x04001DDF RID: 7647
		private PrivilegeManager.User _user;

		// Token: 0x04001DE0 RID: 7648
		private string _gamertag;

		// Token: 0x04001DE1 RID: 7649
		private string _characterName;
	}
}
