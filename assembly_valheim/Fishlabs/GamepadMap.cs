using System;
using TMPro;
using UnityEngine;

namespace Fishlabs
{
	// Token: 0x020002E3 RID: 739
	public class GamepadMap : MonoBehaviour
	{
		// Token: 0x06001BE3 RID: 7139 RVA: 0x000B93B0 File Offset: 0x000B75B0
		public void UpdateMap(InputLayout layout)
		{
			this.joyButton0.Label.text = GamepadMap.GetText("JoystickButton0", KeyCode.JoystickButton0);
			this.joyButton1.Label.text = GamepadMap.GetText("JoystickButton1", KeyCode.JoystickButton1);
			this.joyButton2.Label.text = GamepadMap.GetText("JoystickButton2", KeyCode.JoystickButton2);
			this.joyButton3.Label.text = GamepadMap.GetText("JoystickButton3", KeyCode.JoystickButton3);
			this.joyButton4.Label.text = GamepadMap.GetText("JoystickButton4", KeyCode.JoystickButton4);
			this.joyButton5.Label.text = GamepadMap.GetText("JoystickButton5", KeyCode.JoystickButton5);
			this.joyButton6.Label.text = GamepadMap.GetText("JoystickButton6", KeyCode.JoystickButton6);
			this.joyButton7.Label.text = GamepadMap.GetText("JoystickButton7", KeyCode.JoystickButton7);
			this.joyAxis9.Label.text = GamepadMap.GetText("JoyAxis 3_inverted", KeyCode.None);
			this.joyAxis10.Label.text = GamepadMap.GetText("JoyAxis 3", KeyCode.None);
			this.joyAxis9And10.gameObject.SetActive(layout == InputLayout.Alternative1);
			this.joyAxis9And10.Label.text = Localization.instance.Localize("$settings_gp");
			this.joyAxis1And2.Label.text = Localization.instance.Localize("$settings_move");
			this.joyAxis4And5.Label.text = Localization.instance.Localize("$settings_look");
			this.joyButton8.Label.text = GamepadMap.GetText("JoystickButton8", KeyCode.JoystickButton8);
			this.joyButton9.Label.text = GamepadMap.GetText("JoystickButton9", KeyCode.JoystickButton9);
			this.joyAxis6LeftRight.Label.text = GamepadMap.GetText("JoyAxis 6", KeyCode.None);
			this.joyAxis7Up.Label.text = GamepadMap.GetText("JoyAxis 7", KeyCode.None);
			this.joyAxis7Down.Label.text = GamepadMap.GetText("JoyAxis 7_inverted", KeyCode.None);
			this.alternateButtonLabel.text = Localization.instance.Localize("$alternate_key_label ") + ZInput.instance.GetBoundKeyString("JoyAltKeys", false);
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x000B961C File Offset: 0x000B781C
		private static string GetText(string name, KeyCode keycode = KeyCode.None)
		{
			string result;
			if (keycode != KeyCode.None)
			{
				string boundActionString = ZInput.instance.GetBoundActionString(ZInput.JoyWinToOSKeyCode(keycode));
				result = Localization.instance.Localize(boundActionString);
			}
			else
			{
				bool flag = name.Contains("_inverted");
				name = (flag ? name.Substring(0, name.Length - "_inverted".Length) : name);
				bool inverted;
				string boundActionString = ZInput.instance.GetBoundActionString(ZInput.JoyWinToOSKeyAxis(name, out inverted, flag), inverted);
				result = Localization.instance.Localize(boundActionString);
			}
			return result;
		}

		// Token: 0x04001DFB RID: 7675
		[Header("Face Buttons")]
		[SerializeField]
		private GamepadMapLabel joyButton0;

		// Token: 0x04001DFC RID: 7676
		[SerializeField]
		private GamepadMapLabel joyButton1;

		// Token: 0x04001DFD RID: 7677
		[SerializeField]
		private GamepadMapLabel joyButton2;

		// Token: 0x04001DFE RID: 7678
		[SerializeField]
		private GamepadMapLabel joyButton3;

		// Token: 0x04001DFF RID: 7679
		[SerializeField]
		[Header("Bumpers")]
		private GamepadMapLabel joyButton4;

		// Token: 0x04001E00 RID: 7680
		[SerializeField]
		private GamepadMapLabel joyButton5;

		// Token: 0x04001E01 RID: 7681
		[Header("Center")]
		[SerializeField]
		private GamepadMapLabel joyButton6;

		// Token: 0x04001E02 RID: 7682
		[SerializeField]
		private GamepadMapLabel joyButton7;

		// Token: 0x04001E03 RID: 7683
		[Header("Triggers")]
		[SerializeField]
		private GamepadMapLabel joyAxis9;

		// Token: 0x04001E04 RID: 7684
		[SerializeField]
		private GamepadMapLabel joyAxis10;

		// Token: 0x04001E05 RID: 7685
		[SerializeField]
		private GamepadMapLabel joyAxis9And10;

		// Token: 0x04001E06 RID: 7686
		[Header("Sticks")]
		[SerializeField]
		private GamepadMapLabel joyButton8;

		// Token: 0x04001E07 RID: 7687
		[SerializeField]
		private GamepadMapLabel joyButton9;

		// Token: 0x04001E08 RID: 7688
		[SerializeField]
		private GamepadMapLabel joyAxis1And2;

		// Token: 0x04001E09 RID: 7689
		[SerializeField]
		private GamepadMapLabel joyAxis4And5;

		// Token: 0x04001E0A RID: 7690
		[SerializeField]
		[Header("Dpad")]
		private GamepadMapLabel joyAxis6And7;

		// Token: 0x04001E0B RID: 7691
		[SerializeField]
		private GamepadMapLabel joyAxis6Left;

		// Token: 0x04001E0C RID: 7692
		[SerializeField]
		private GamepadMapLabel joyAxis6Right;

		// Token: 0x04001E0D RID: 7693
		[SerializeField]
		private GamepadMapLabel joyAxis6LeftRight;

		// Token: 0x04001E0E RID: 7694
		[SerializeField]
		private GamepadMapLabel joyAxis7Up;

		// Token: 0x04001E0F RID: 7695
		[SerializeField]
		private GamepadMapLabel joyAxis7Down;

		// Token: 0x04001E10 RID: 7696
		[SerializeField]
		private TextMeshProUGUI alternateButtonLabel;
	}
}
