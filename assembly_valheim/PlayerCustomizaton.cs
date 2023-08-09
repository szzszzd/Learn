using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000ED RID: 237
public class PlayerCustomizaton : MonoBehaviour
{
	// Token: 0x060009A3 RID: 2467 RVA: 0x00049A78 File Offset: 0x00047C78
	private void OnEnable()
	{
		this.m_maleToggle.isOn = true;
		this.m_femaleToggle.isOn = false;
		this.m_beardPanel.gameObject.SetActive(true);
		this.m_beards = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard");
		this.m_hairs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair");
		this.m_beards.Sort((ItemDrop x, ItemDrop y) => Localization.instance.Localize(x.m_itemData.m_shared.m_name).CompareTo(Localization.instance.Localize(y.m_itemData.m_shared.m_name)));
		this.m_hairs.Sort((ItemDrop x, ItemDrop y) => Localization.instance.Localize(x.m_itemData.m_shared.m_name).CompareTo(Localization.instance.Localize(y.m_itemData.m_shared.m_name)));
		this.m_beards.Remove(this.m_noBeard);
		this.m_beards.Insert(0, this.m_noBeard);
		this.m_hairs.Remove(this.m_noHair);
		this.m_hairs.Insert(0, this.m_noHair);
	}

	// Token: 0x060009A4 RID: 2468 RVA: 0x00049B78 File Offset: 0x00047D78
	private void Update()
	{
		if (this.GetPlayer() == null)
		{
			return;
		}
		this.m_selectedHair.text = Localization.instance.Localize(this.GetHair());
		this.m_selectedBeard.text = Localization.instance.Localize(this.GetBeard());
		Color c = Color.Lerp(this.m_skinColor0, this.m_skinColor1, this.m_skinHue.value);
		this.GetPlayer().SetSkinColor(Utils.ColorToVec3(c));
		Color c2 = Color.Lerp(this.m_hairColor0, this.m_hairColor1, this.m_hairTone.value) * Mathf.Lerp(this.m_hairMinLevel, this.m_hairMaxLevel, this.m_hairLevel.value);
		this.GetPlayer().SetHairColor(Utils.ColorToVec3(c2));
	}

	// Token: 0x060009A5 RID: 2469 RVA: 0x00049C47 File Offset: 0x00047E47
	private Player GetPlayer()
	{
		return base.GetComponentInParent<FejdStartup>().GetPreviewPlayer();
	}

	// Token: 0x060009A6 RID: 2470 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnHairHueChange(float v)
	{
	}

	// Token: 0x060009A7 RID: 2471 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnSkinHueChange(float v)
	{
	}

	// Token: 0x060009A8 RID: 2472 RVA: 0x00049C54 File Offset: 0x00047E54
	public void SetPlayerModel(int index)
	{
		Player player = this.GetPlayer();
		if (player == null)
		{
			return;
		}
		player.SetPlayerModel(index);
		if (index == 1)
		{
			this.ResetBeard();
		}
	}

	// Token: 0x060009A9 RID: 2473 RVA: 0x00049C83 File Offset: 0x00047E83
	public void OnHairLeft()
	{
		this.SetHair(this.GetHairIndex() - 1);
	}

	// Token: 0x060009AA RID: 2474 RVA: 0x00049C93 File Offset: 0x00047E93
	public void OnHairRight()
	{
		this.SetHair(this.GetHairIndex() + 1);
	}

	// Token: 0x060009AB RID: 2475 RVA: 0x00049CA3 File Offset: 0x00047EA3
	public void OnBeardLeft()
	{
		if (this.GetPlayer().GetPlayerModel() == 1)
		{
			return;
		}
		this.SetBeard(this.GetBeardIndex() - 1);
	}

	// Token: 0x060009AC RID: 2476 RVA: 0x00049CC2 File Offset: 0x00047EC2
	public void OnBeardRight()
	{
		if (this.GetPlayer().GetPlayerModel() == 1)
		{
			return;
		}
		this.SetBeard(this.GetBeardIndex() + 1);
	}

	// Token: 0x060009AD RID: 2477 RVA: 0x00049CE1 File Offset: 0x00047EE1
	private void ResetBeard()
	{
		this.GetPlayer().SetBeard(this.m_noBeard.gameObject.name);
	}

	// Token: 0x060009AE RID: 2478 RVA: 0x00049CFE File Offset: 0x00047EFE
	private void SetBeard(int index)
	{
		if (index < 0 || index >= this.m_beards.Count)
		{
			return;
		}
		this.GetPlayer().SetBeard(this.m_beards[index].gameObject.name);
	}

	// Token: 0x060009AF RID: 2479 RVA: 0x00049D34 File Offset: 0x00047F34
	private void SetHair(int index)
	{
		ZLog.Log("Set hair " + index.ToString());
		if (index < 0 || index >= this.m_hairs.Count)
		{
			return;
		}
		this.GetPlayer().SetHair(this.m_hairs[index].gameObject.name);
	}

	// Token: 0x060009B0 RID: 2480 RVA: 0x00049D8C File Offset: 0x00047F8C
	private int GetBeardIndex()
	{
		string beard = this.GetPlayer().GetBeard();
		for (int i = 0; i < this.m_beards.Count; i++)
		{
			if (this.m_beards[i].gameObject.name == beard)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x060009B1 RID: 2481 RVA: 0x00049DDC File Offset: 0x00047FDC
	private int GetHairIndex()
	{
		string hair = this.GetPlayer().GetHair();
		for (int i = 0; i < this.m_hairs.Count; i++)
		{
			if (this.m_hairs[i].gameObject.name == hair)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x060009B2 RID: 2482 RVA: 0x00049E2C File Offset: 0x0004802C
	private string GetHair()
	{
		return this.m_hairs[this.GetHairIndex()].m_itemData.m_shared.m_name;
	}

	// Token: 0x060009B3 RID: 2483 RVA: 0x00049E4E File Offset: 0x0004804E
	private string GetBeard()
	{
		return this.m_beards[this.GetBeardIndex()].m_itemData.m_shared.m_name;
	}

	// Token: 0x04000BA9 RID: 2985
	public Color m_skinColor0 = Color.white;

	// Token: 0x04000BAA RID: 2986
	public Color m_skinColor1 = Color.white;

	// Token: 0x04000BAB RID: 2987
	public Color m_hairColor0 = Color.white;

	// Token: 0x04000BAC RID: 2988
	public Color m_hairColor1 = Color.white;

	// Token: 0x04000BAD RID: 2989
	public float m_hairMaxLevel = 1f;

	// Token: 0x04000BAE RID: 2990
	public float m_hairMinLevel = 0.1f;

	// Token: 0x04000BAF RID: 2991
	public Text m_selectedBeard;

	// Token: 0x04000BB0 RID: 2992
	public Text m_selectedHair;

	// Token: 0x04000BB1 RID: 2993
	public Slider m_skinHue;

	// Token: 0x04000BB2 RID: 2994
	public Slider m_hairLevel;

	// Token: 0x04000BB3 RID: 2995
	public Slider m_hairTone;

	// Token: 0x04000BB4 RID: 2996
	public RectTransform m_beardPanel;

	// Token: 0x04000BB5 RID: 2997
	public Toggle m_maleToggle;

	// Token: 0x04000BB6 RID: 2998
	public Toggle m_femaleToggle;

	// Token: 0x04000BB7 RID: 2999
	public ItemDrop m_noHair;

	// Token: 0x04000BB8 RID: 3000
	public ItemDrop m_noBeard;

	// Token: 0x04000BB9 RID: 3001
	private List<ItemDrop> m_beards;

	// Token: 0x04000BBA RID: 3002
	private List<ItemDrop> m_hairs;
}
