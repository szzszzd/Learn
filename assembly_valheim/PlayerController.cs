using System;
using UnityEngine;

// Token: 0x02000028 RID: 40
public class PlayerController : MonoBehaviour
{
	// Token: 0x060002D6 RID: 726 RVA: 0x00015FB0 File Offset: 0x000141B0
	private void Awake()
	{
		this.m_character = base.GetComponent<Player>();
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		PlayerController.m_mouseSens = PlayerPrefs.GetFloat("MouseSensitivity", PlayerController.m_mouseSens);
		PlayerController.m_gamepadSens = PlayerPrefs.GetFloat("GamepadSensitivity", PlayerController.m_gamepadSens);
		PlayerController.m_invertMouse = (PlayerPrefs.GetInt("InvertMouse", 0) == 1);
	}

	// Token: 0x060002D7 RID: 727 RVA: 0x0001602C File Offset: 0x0001422C
	private void FixedUpdate()
	{
		if (this.m_nview && !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.TakeInput())
		{
			this.m_character.SetControls(Vector3.zero, false, false, false, false, false, false, false, false, false, false, false);
			return;
		}
		bool flag = this.InInventoryEtc();
		bool flag2 = Hud.IsPieceSelectionVisible();
		bool flag3 = (ZInput.GetButton("SecondaryAttack") || ZInput.GetButton("JoySecondaryAttack")) && !flag;
		Vector3 zero = Vector3.zero;
		if (ZInput.GetButton("Forward"))
		{
			zero.z += 1f;
		}
		if (ZInput.GetButton("Backward"))
		{
			zero.z -= 1f;
		}
		if (ZInput.GetButton("Left"))
		{
			zero.x -= 1f;
		}
		if (ZInput.GetButton("Right"))
		{
			zero.x += 1f;
		}
		if (!flag3)
		{
			zero.x += ZInput.GetJoyLeftStickX(false);
			zero.z += -ZInput.GetJoyLeftStickY(true);
		}
		if (zero.magnitude > 1f)
		{
			zero.Normalize();
		}
		bool flag4 = (ZInput.GetButton("Attack") || ZInput.GetButton("JoyAttack")) && !flag;
		bool attackHold = flag4;
		bool attack = flag4 && !this.m_attackWasPressed;
		this.m_attackWasPressed = flag4;
		bool secondaryAttackHold = flag3;
		bool secondaryAttack = flag3 && !this.m_secondAttackWasPressed;
		this.m_secondAttackWasPressed = flag3;
		bool flag5 = (ZInput.GetButton("Block") || ZInput.GetButton("JoyBlock")) && !flag;
		bool blockHold = flag5;
		bool block = flag5 && !this.m_blockWasPressed;
		this.m_blockWasPressed = flag5;
		bool button = ZInput.GetButton("Jump");
		bool jump = (button && !this.m_lastJump) || (ZInput.GetButtonDown("JoyJump") && !flag2 && !flag);
		this.m_lastJump = button;
		bool dodge = ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive() && ZInput.GetButtonDown("JoyDodge") && !flag;
		bool flag6 = InventoryGui.IsVisible();
		bool flag7 = (ZInput.GetButton("Crouch") || ZInput.GetButton("JoyCrouch")) && !flag6;
		bool crouch = flag7 && !this.m_lastCrouch;
		this.m_lastCrouch = flag7;
		if (ZInput.InputLayout == InputLayout.Default || !ZInput.IsGamepadActive())
		{
			this.m_run = (ZInput.GetButton("Run") || ZInput.GetButton("JoyRun"));
		}
		else
		{
			float magnitude = zero.magnitude;
			if ((this.m_run && magnitude < 0.05f && this.m_lastMagnitude < 0.05f) || this.m_character.GetStamina() <= 0f)
			{
				this.m_run = false;
			}
			bool button2 = ZInput.GetButton("JoyRun");
			if (button2 && !this.m_lastRunPressed)
			{
				this.m_run = !this.m_run;
			}
			this.m_lastRunPressed = button2;
			this.m_lastMagnitude = magnitude;
		}
		bool button3 = ZInput.GetButton("AutoRun");
		this.m_character.SetControls(zero, attack, attackHold, secondaryAttack, secondaryAttackHold, block, blockHold, jump, crouch, this.m_run, button3, dodge);
	}

	// Token: 0x060002D8 RID: 728 RVA: 0x0001636C File Offset: 0x0001456C
	private static bool DetectTap(bool pressed, float dt, float minPressTime, bool run, ref float pressTimer, ref float releasedTimer, ref bool tapPressed)
	{
		bool result = false;
		if (pressed)
		{
			if ((releasedTimer > 0f && releasedTimer < minPressTime) & tapPressed)
			{
				tapPressed = false;
				result = true;
			}
			pressTimer += dt;
			releasedTimer = 0f;
		}
		else
		{
			if (pressTimer > 0f)
			{
				tapPressed = (pressTimer < minPressTime);
				if (run & tapPressed)
				{
					tapPressed = false;
					result = true;
				}
			}
			releasedTimer += dt;
			pressTimer = 0f;
		}
		return result;
	}

	// Token: 0x060002D9 RID: 729 RVA: 0x000163E0 File Offset: 0x000145E0
	private bool TakeInput()
	{
		return !GameCamera.InFreeFly() && ((!Chat.instance || !Chat.instance.IsTakingInput()) && !Menu.IsVisible() && !global::Console.IsVisible() && !TextInput.IsVisible() && !Minimap.InTextInput() && (!ZInput.IsGamepadActive() || !Minimap.IsOpen()) && (!ZInput.IsGamepadActive() || !InventoryGui.IsVisible()) && (!ZInput.IsGamepadActive() || !StoreGui.IsVisible())) && (!ZInput.IsGamepadActive() || !Hud.IsPieceSelectionVisible());
	}

	// Token: 0x060002DA RID: 730 RVA: 0x00016467 File Offset: 0x00014667
	private bool InInventoryEtc()
	{
		return InventoryGui.IsVisible() || Minimap.IsOpen() || StoreGui.IsVisible() || Hud.IsPieceSelectionVisible();
	}

	// Token: 0x060002DB RID: 731 RVA: 0x00016488 File Offset: 0x00014688
	private void LateUpdate()
	{
		if (!this.TakeInput() || this.InInventoryEtc())
		{
			this.m_character.SetMouseLook(Vector2.zero);
			return;
		}
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxis("Mouse X") * PlayerController.m_mouseSens;
		zero.y = Input.GetAxis("Mouse Y") * PlayerController.m_mouseSens;
		if (!this.m_character.InPlaceMode() || !ZInput.GetButton("JoyRotate"))
		{
			zero.x += ZInput.GetJoyRightStickX() * 110f * Time.deltaTime * PlayerController.m_gamepadSens;
			zero.y += -ZInput.GetJoyRightStickY() * 110f * Time.deltaTime * PlayerController.m_gamepadSens;
		}
		if (PlayerController.m_invertMouse)
		{
			zero.y *= -1f;
		}
		this.m_character.SetMouseLook(zero);
	}

	// Token: 0x04000293 RID: 659
	private bool m_run;

	// Token: 0x04000294 RID: 660
	private bool m_lastRunPressed;

	// Token: 0x04000295 RID: 661
	private float m_lastMagnitude;

	// Token: 0x04000296 RID: 662
	private Player m_character;

	// Token: 0x04000297 RID: 663
	private ZNetView m_nview;

	// Token: 0x04000298 RID: 664
	public static float m_mouseSens = 1f;

	// Token: 0x04000299 RID: 665
	public static float m_gamepadSens = 1f;

	// Token: 0x0400029A RID: 666
	public static bool m_invertMouse = false;

	// Token: 0x0400029B RID: 667
	public float m_minDodgeTime = 0.2f;

	// Token: 0x0400029C RID: 668
	private bool m_attackWasPressed;

	// Token: 0x0400029D RID: 669
	private bool m_secondAttackWasPressed;

	// Token: 0x0400029E RID: 670
	private bool m_blockWasPressed;

	// Token: 0x0400029F RID: 671
	private bool m_lastJump;

	// Token: 0x040002A0 RID: 672
	private bool m_lastCrouch;
}
