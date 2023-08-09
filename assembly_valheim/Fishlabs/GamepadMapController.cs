using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fishlabs
{
	// Token: 0x020002E4 RID: 740
	public class GamepadMapController : MonoBehaviour
	{
		// Token: 0x06001BE6 RID: 7142 RVA: 0x000B969C File Offset: 0x000B789C
		public void SetGamepadMap(GamepadMapType type, InputLayout layout, bool showUI = false)
		{
			this.controllerLayoutSelector.gameObject.SetActive(showUI);
			this.okButton.gameObject.SetActive(showUI);
			this.gamepadTextDisclaimer.gameObject.SetActive(true);
			this.currentLayout = ZInput.InputLayout;
			this.SetInputLayoutText(this.currentLayout);
			switch (type)
			{
			case GamepadMapType.PS:
				if (this.psMapInstance == null)
				{
					this.psMapInstance = UnityEngine.Object.Instantiate<GamepadMap>(this.psMapPrefab, this.root);
					goto IL_FF;
				}
				goto IL_FF;
			case GamepadMapType.SteamXbox:
				if (this.steamDeckXboxMapInstance == null)
				{
					this.steamDeckXboxMapInstance = UnityEngine.Object.Instantiate<GamepadMap>(this.steamDeckXboxMapPrefab, this.root);
					goto IL_FF;
				}
				goto IL_FF;
			case GamepadMapType.SteamPS:
				if (this.steamDeckPSMapInstance == null)
				{
					this.steamDeckPSMapInstance = UnityEngine.Object.Instantiate<GamepadMap>(this.steamDeckPSMapPrefab, this.root);
					goto IL_FF;
				}
				goto IL_FF;
			}
			if (this.xboxMapInstance == null)
			{
				this.xboxMapInstance = UnityEngine.Object.Instantiate<GamepadMap>(this.xboxMapPrefab, this.root);
			}
			IL_FF:
			this.UpdateGamepadMap(type, layout);
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x000B97B0 File Offset: 0x000B79B0
		private void UpdateGamepadMap(GamepadMapType visibleType, InputLayout layout)
		{
			if (this.psMapInstance != null)
			{
				this.psMapInstance.gameObject.SetActive(visibleType == GamepadMapType.PS);
				if (visibleType == GamepadMapType.PS)
				{
					this.psMapInstance.UpdateMap(layout);
				}
			}
			if (this.steamDeckXboxMapInstance != null)
			{
				this.steamDeckXboxMapInstance.gameObject.SetActive(visibleType == GamepadMapType.SteamXbox);
				if (visibleType == GamepadMapType.SteamXbox)
				{
					this.steamDeckXboxMapInstance.UpdateMap(layout);
				}
			}
			if (this.steamDeckPSMapInstance != null)
			{
				this.steamDeckPSMapInstance.gameObject.SetActive(visibleType == GamepadMapType.SteamPS);
				if (visibleType == GamepadMapType.SteamPS)
				{
					this.steamDeckPSMapInstance.UpdateMap(layout);
				}
			}
			if (this.xboxMapInstance != null)
			{
				this.xboxMapInstance.gameObject.SetActive(visibleType == GamepadMapType.Default);
				if (visibleType == GamepadMapType.Default)
				{
					this.xboxMapInstance.UpdateMap(layout);
				}
			}
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x000B9884 File Offset: 0x000B7A84
		public void OnLeft()
		{
			InputLayout inputLayout = GamepadMapController.PrevLayout(this.newLayout);
			ZInput.instance.ChangeLayout(inputLayout);
			this.SetInputLayoutText(inputLayout);
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x000B98B0 File Offset: 0x000B7AB0
		public void OnRight()
		{
			InputLayout inputLayout = GamepadMapController.NextLayout(this.newLayout);
			ZInput.instance.ChangeLayout(inputLayout);
			this.SetInputLayoutText(inputLayout);
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x000B98DC File Offset: 0x000B7ADC
		private void SetInputLayoutText(InputLayout layout)
		{
			this.newLayout = layout;
			if (layout != InputLayout.Default)
			{
				if (layout != InputLayout.Alternative1)
				{
				}
				this.m_controllerLayoutKey = "$settings_controller_default";
				this.controllerLayoutSelector.SetText(Localization.instance.Localize(this.m_controllerLayoutKey));
				return;
			}
			this.m_controllerLayoutKey = "$settings_controller_classic";
			this.controllerLayoutSelector.SetText(Localization.instance.Localize(this.m_controllerLayoutKey));
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x000B9946 File Offset: 0x000B7B46
		private static InputLayout NextLayout(InputLayout mode)
		{
			if (mode != InputLayout.Default && mode == InputLayout.Alternative1)
			{
				return InputLayout.Default;
			}
			return InputLayout.Alternative1;
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x000B9946 File Offset: 0x000B7B46
		private static InputLayout PrevLayout(InputLayout mode)
		{
			if (mode != InputLayout.Default && mode == InputLayout.Alternative1)
			{
				return InputLayout.Default;
			}
			return InputLayout.Alternative1;
		}

		// Token: 0x06001BED RID: 7149 RVA: 0x000B9952 File Offset: 0x000B7B52
		public void OnOk()
		{
			ZInput.instance.ChangeLayout(this.newLayout);
			this.currentLayout = this.newLayout;
			Settings.instance.HideGamepadMap();
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x000B997A File Offset: 0x000B7B7A
		public void OnBack()
		{
			ZInput.instance.ChangeLayout(this.currentLayout);
			Settings.instance.HideGamepadMap();
		}

		// Token: 0x04001E11 RID: 7697
		[SerializeField]
		private GamepadMap xboxMapPrefab;

		// Token: 0x04001E12 RID: 7698
		[SerializeField]
		private GamepadMap psMapPrefab;

		// Token: 0x04001E13 RID: 7699
		[SerializeField]
		private GamepadMap steamDeckXboxMapPrefab;

		// Token: 0x04001E14 RID: 7700
		[SerializeField]
		private GamepadMap steamDeckPSMapPrefab;

		// Token: 0x04001E15 RID: 7701
		[SerializeField]
		private RectTransform root;

		// Token: 0x04001E16 RID: 7702
		[SerializeField]
		private Text gamepadTextDisclaimer;

		// Token: 0x04001E17 RID: 7703
		[SerializeField]
		private Selector controllerLayoutSelector;

		// Token: 0x04001E18 RID: 7704
		[SerializeField]
		private Button okButton;

		// Token: 0x04001E19 RID: 7705
		private GamepadMap xboxMapInstance;

		// Token: 0x04001E1A RID: 7706
		private GamepadMap psMapInstance;

		// Token: 0x04001E1B RID: 7707
		private GamepadMap steamDeckXboxMapInstance;

		// Token: 0x04001E1C RID: 7708
		private GamepadMap steamDeckPSMapInstance;

		// Token: 0x04001E1D RID: 7709
		private string m_controllerLayoutKey = "";

		// Token: 0x04001E1E RID: 7710
		private InputLayout newLayout;

		// Token: 0x04001E1F RID: 7711
		private InputLayout currentLayout;
	}
}
