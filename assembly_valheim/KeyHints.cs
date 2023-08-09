using System;
using TMPro;
using UnityEngine;

// Token: 0x020000BB RID: 187
public class KeyHints : MonoBehaviour
{
	// Token: 0x0600081A RID: 2074 RVA: 0x00040882 File Offset: 0x0003EA82
	private void OnDestroy()
	{
		KeyHints.m_instance = null;
	}

	// Token: 0x17000034 RID: 52
	// (get) Token: 0x0600081B RID: 2075 RVA: 0x0004088A File Offset: 0x0003EA8A
	public static KeyHints instance
	{
		get
		{
			return KeyHints.m_instance;
		}
	}

	// Token: 0x0600081C RID: 2076 RVA: 0x00040891 File Offset: 0x0003EA91
	private void Awake()
	{
		KeyHints.m_instance = this;
		this.ApplySettings();
	}

	// Token: 0x0600081D RID: 2077 RVA: 0x000408A0 File Offset: 0x0003EAA0
	public void SetGamePadBindings()
	{
		if (this.m_buildMenuKey != null)
		{
			Localization.instance.RemoveTextFromCache(this.m_buildMenuKey);
			InputLayout inputLayout = ZInput.InputLayout;
			if (inputLayout != InputLayout.Default)
			{
				if (inputLayout == InputLayout.Alternative1)
				{
					this.m_buildMenuKey.text = "<mspace=0.6em>$KEY_BuildMenu </mspace>$hud_buildmenu";
				}
			}
			else
			{
				this.m_buildMenuKey.text = "<mspace=0.6em>$KEY_Use </mspace>$hud_buildmenu";
			}
			Localization.instance.Localize(this.m_buildMenuKey.transform);
		}
		if (this.m_buildRotateKey != null)
		{
			Localization.instance.RemoveTextFromCache(this.m_buildRotateKey);
			InputLayout inputLayout = ZInput.InputLayout;
			if (inputLayout != InputLayout.Default)
			{
				if (inputLayout == InputLayout.Alternative1)
				{
					this.m_buildRotateKey.text = "<mspace=0.6em>$KEY_LTrigger / $KEY_RTrigger </mspace>$hud_rotate";
				}
			}
			else
			{
				this.m_buildRotateKey.text = "<mspace=0.6em>$KEY_Block + $KEY_RightStick </mspace>$hud_rotate";
			}
			Localization.instance.Localize(this.m_buildRotateKey.transform);
		}
		if (this.m_dodgeKey != null)
		{
			Localization.instance.RemoveTextFromCache(this.m_dodgeKey);
			InputLayout inputLayout = ZInput.InputLayout;
			if (inputLayout != InputLayout.Default)
			{
				if (inputLayout == InputLayout.Alternative1)
				{
					this.m_dodgeKey.text = "<mspace=0.6em>$KEY_Block + $KEY_Dodge </mspace>$settings_dodge";
				}
			}
			else
			{
				this.m_dodgeKey.text = "<mspace=0.6em>$KEY_Block + $KEY_Jump </mspace>$settings_dodge";
			}
			Localization.instance.Localize(this.m_dodgeKey.transform);
		}
	}

	// Token: 0x0600081E RID: 2078 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Start()
	{
	}

	// Token: 0x0600081F RID: 2079 RVA: 0x000409D9 File Offset: 0x0003EBD9
	public void ApplySettings()
	{
		this.m_keyHintsEnabled = (PlayerPrefs.GetInt("KeyHints", 1) == 1);
		this.SetGamePadBindings();
	}

	// Token: 0x06000820 RID: 2080 RVA: 0x000409FC File Offset: 0x0003EBFC
	private void Update()
	{
		this.UpdateHints();
		if (Input.GetKeyDown(KeyCode.F9))
		{
			InputLayout inputLayout = ZInput.InputLayout;
			if (inputLayout != InputLayout.Default && inputLayout == InputLayout.Alternative1)
			{
				ZInput.instance.ChangeLayout(InputLayout.Default);
			}
			else
			{
				ZInput.instance.ChangeLayout(InputLayout.Alternative1);
			}
			this.ApplySettings();
		}
	}

	// Token: 0x06000821 RID: 2081 RVA: 0x00040A48 File Offset: 0x0003EC48
	private void UpdateHints()
	{
		Player localPlayer = Player.m_localPlayer;
		if (!this.m_keyHintsEnabled || localPlayer == null || localPlayer.IsDead() || Chat.instance.IsChatDialogWindowVisible() || Game.IsPaused() || (InventoryGui.instance != null && (InventoryGui.instance.IsSkillsPanelOpen || InventoryGui.instance.IsTrophisPanelOpen || InventoryGui.instance.IsTextPanelOpen)))
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(false);
			this.m_inventoryHints.SetActive(false);
			this.m_inventoryWithContainerHints.SetActive(false);
			this.m_fishingHints.SetActive(false);
			return;
		}
		bool activeSelf = this.m_buildHints.activeSelf;
		bool activeSelf2 = this.m_buildHints.activeSelf;
		ItemDrop.ItemData currentWeapon = localPlayer.GetCurrentWeapon();
		if (InventoryGui.IsVisible())
		{
			bool flag = InventoryGui.instance.IsContainerOpen();
			bool flag2 = InventoryGui.instance.ActiveGroup == 0;
			ItemDrop.ItemData itemData = flag2 ? InventoryGui.instance.ContainerGrid.GetGamepadSelectedItem() : InventoryGui.instance.m_playerGrid.GetGamepadSelectedItem();
			bool flag3 = itemData != null && itemData.IsEquipable();
			bool flag4 = itemData != null && itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable;
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(false);
			this.m_inventoryHints.SetActive(!flag);
			this.m_inventoryWithContainerHints.SetActive(flag);
			for (int i = 0; i < this.m_equipButtons.Length; i++)
			{
				this.m_equipButtons[i].SetActive(flag4 || (flag3 && !flag2));
			}
			this.m_fishingHints.SetActive(false);
			return;
		}
		if (localPlayer.InPlaceMode())
		{
			if (ZInput.InputLayout == InputLayout.Alternative1)
			{
				string str = Localization.instance.Localize("<mspace=0.6em>$KEY_AltKeys + $KEY_AltPlace</mspace>  $hud_altplacement");
				string str2 = localPlayer.AlternativePlacementActive ? Localization.instance.Localize("$hud_off") : Localization.instance.Localize("$hud_on");
				this.m_buildAlternativePlacingKey.text = str + " " + str2;
			}
			this.m_buildHints.SetActive(true);
			this.m_combatHints.SetActive(false);
			this.m_inventoryHints.SetActive(false);
			this.m_inventoryWithContainerHints.SetActive(false);
			this.m_fishingHints.SetActive(false);
			return;
		}
		if (localPlayer.GetDoodadController() != null)
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(false);
			this.m_inventoryHints.SetActive(false);
			this.m_inventoryWithContainerHints.SetActive(false);
			this.m_fishingHints.SetActive(false);
			return;
		}
		if (currentWeapon != null && currentWeapon.m_shared.m_animationState == ItemDrop.ItemData.AnimationState.FishingRod)
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(false);
			this.m_inventoryHints.SetActive(false);
			this.m_inventoryWithContainerHints.SetActive(false);
			this.m_fishingHints.SetActive(true);
			return;
		}
		if (currentWeapon != null && (currentWeapon != localPlayer.m_unarmedWeapon.m_itemData || localPlayer.IsTargeted()))
		{
			this.m_buildHints.SetActive(false);
			this.m_combatHints.SetActive(true);
			this.m_inventoryHints.SetActive(false);
			this.m_inventoryWithContainerHints.SetActive(false);
			this.m_fishingHints.SetActive(false);
			bool flag5 = currentWeapon.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow && currentWeapon.m_shared.m_skillType != Skills.SkillType.Crossbows;
			bool active = !flag5 && currentWeapon.HavePrimaryAttack();
			bool active2 = !flag5 && currentWeapon.HaveSecondaryAttack();
			this.m_bowDrawGP.SetActive(flag5);
			this.m_bowDrawKB.SetActive(flag5);
			this.m_primaryAttackGP.SetActive(active);
			this.m_primaryAttackKB.SetActive(active);
			this.m_secondaryAttackGP.SetActive(active2);
			this.m_secondaryAttackKB.SetActive(active2);
			return;
		}
		this.m_buildHints.SetActive(false);
		this.m_combatHints.SetActive(false);
		this.m_inventoryHints.SetActive(false);
		this.m_inventoryWithContainerHints.SetActive(false);
		this.m_fishingHints.SetActive(false);
	}

	// Token: 0x04000A3E RID: 2622
	private static KeyHints m_instance;

	// Token: 0x04000A3F RID: 2623
	[Header("Key hints")]
	public GameObject m_buildHints;

	// Token: 0x04000A40 RID: 2624
	public GameObject m_combatHints;

	// Token: 0x04000A41 RID: 2625
	public GameObject m_inventoryHints;

	// Token: 0x04000A42 RID: 2626
	public GameObject m_inventoryWithContainerHints;

	// Token: 0x04000A43 RID: 2627
	public GameObject m_fishingHints;

	// Token: 0x04000A44 RID: 2628
	public GameObject[] m_equipButtons;

	// Token: 0x04000A45 RID: 2629
	public GameObject m_primaryAttackGP;

	// Token: 0x04000A46 RID: 2630
	public GameObject m_primaryAttackKB;

	// Token: 0x04000A47 RID: 2631
	public GameObject m_secondaryAttackGP;

	// Token: 0x04000A48 RID: 2632
	public GameObject m_secondaryAttackKB;

	// Token: 0x04000A49 RID: 2633
	public GameObject m_bowDrawGP;

	// Token: 0x04000A4A RID: 2634
	public GameObject m_bowDrawKB;

	// Token: 0x04000A4B RID: 2635
	private bool m_keyHintsEnabled = true;

	// Token: 0x04000A4C RID: 2636
	public TextMeshProUGUI m_buildMenuKey;

	// Token: 0x04000A4D RID: 2637
	public TextMeshProUGUI m_buildRotateKey;

	// Token: 0x04000A4E RID: 2638
	public TextMeshProUGUI m_buildAlternativePlacingKey;

	// Token: 0x04000A4F RID: 2639
	public TextMeshProUGUI m_dodgeKey;
}
