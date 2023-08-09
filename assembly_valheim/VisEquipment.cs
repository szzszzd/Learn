using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003C RID: 60
public class VisEquipment : MonoBehaviour
{
	// Token: 0x06000389 RID: 905 RVA: 0x0001AA34 File Offset: 0x00018C34
	private void Awake()
	{
		this.m_nview = ((this.m_nViewOverride != null) ? this.m_nViewOverride : base.GetComponent<ZNetView>());
		Transform transform = base.transform.Find("Visual");
		if (transform == null)
		{
			transform = base.transform;
		}
		this.m_visual = transform.gameObject;
		this.m_lodGroup = this.m_visual.GetComponentInChildren<LODGroup>();
		if (this.m_bodyModel != null && this.m_bodyModel.material.HasProperty("_ChestTex"))
		{
			this.m_emptyBodyTexture = this.m_bodyModel.material.GetTexture("_ChestTex");
		}
		if (this.m_bodyModel != null && this.m_bodyModel.material.HasProperty("_LegsTex"))
		{
			this.m_emptyLegsTexture = this.m_bodyModel.material.GetTexture("_LegsTex");
		}
	}

	// Token: 0x0600038A RID: 906 RVA: 0x0001AB21 File Offset: 0x00018D21
	private void OnEnable()
	{
		VisEquipment.Instances.Add(this);
	}

	// Token: 0x0600038B RID: 907 RVA: 0x0001AB2E File Offset: 0x00018D2E
	private void OnDisable()
	{
		VisEquipment.Instances.Remove(this);
	}

	// Token: 0x0600038C RID: 908 RVA: 0x0001AB3C File Offset: 0x00018D3C
	private void Start()
	{
		this.UpdateVisuals();
	}

	// Token: 0x0600038D RID: 909 RVA: 0x0001AB44 File Offset: 0x00018D44
	public void SetWeaponTrails(bool enabled)
	{
		if (this.m_useAllTrails)
		{
			MeleeWeaponTrail[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeleeWeaponTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Emit = enabled;
			}
			return;
		}
		if (this.m_rightItemInstance)
		{
			MeleeWeaponTrail[] componentsInChildren = this.m_rightItemInstance.GetComponentsInChildren<MeleeWeaponTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Emit = enabled;
			}
		}
	}

	// Token: 0x0600038E RID: 910 RVA: 0x0001ABB0 File Offset: 0x00018DB0
	public void SetModel(int index)
	{
		if (this.m_modelIndex == index)
		{
			return;
		}
		if (index < 0 || index >= this.m_models.Length)
		{
			return;
		}
		ZLog.Log("Vis equip model set to " + index.ToString());
		this.m_modelIndex = index;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_modelIndex, this.m_modelIndex, false);
		}
	}

	// Token: 0x0600038F RID: 911 RVA: 0x0001AC2C File Offset: 0x00018E2C
	public void SetSkinColor(Vector3 color)
	{
		if (color == this.m_skinColor)
		{
			return;
		}
		this.m_skinColor = color;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_skinColor, this.m_skinColor);
		}
	}

	// Token: 0x06000390 RID: 912 RVA: 0x0001AC84 File Offset: 0x00018E84
	public void SetHairColor(Vector3 color)
	{
		if (this.m_hairColor == color)
		{
			return;
		}
		this.m_hairColor = color;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_hairColor, this.m_hairColor);
		}
	}

	// Token: 0x06000391 RID: 913 RVA: 0x0001ACDC File Offset: 0x00018EDC
	public void SetItem(VisSlot slot, string name, int variant = 0)
	{
		switch (slot)
		{
		case VisSlot.HandLeft:
			this.SetLeftItem(name, variant);
			return;
		case VisSlot.HandRight:
			this.SetRightItem(name);
			return;
		case VisSlot.BackLeft:
			this.SetLeftBackItem(name, variant);
			return;
		case VisSlot.BackRight:
			this.SetRightBackItem(name);
			return;
		case VisSlot.Chest:
			this.SetChestItem(name);
			return;
		case VisSlot.Legs:
			this.SetLegItem(name);
			return;
		case VisSlot.Helmet:
			this.SetHelmetItem(name);
			return;
		case VisSlot.Shoulder:
			this.SetShoulderItem(name, variant);
			return;
		case VisSlot.Utility:
			this.SetUtilityItem(name);
			return;
		case VisSlot.Beard:
			this.SetBeardItem(name);
			return;
		case VisSlot.Hair:
			this.SetHairItem(name);
			return;
		default:
			throw new NotImplementedException("Unknown slot: " + slot.ToString());
		}
	}

	// Token: 0x06000392 RID: 914 RVA: 0x0001AD94 File Offset: 0x00018F94
	public void SetLeftItem(string name, int variant)
	{
		if (this.m_leftItem == name && this.m_leftItemVariant == variant)
		{
			return;
		}
		this.m_leftItem = name;
		this.m_leftItemVariant = variant;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_leftItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
			this.m_nview.GetZDO().Set(ZDOVars.s_leftItemVariant, variant, false);
		}
	}

	// Token: 0x06000393 RID: 915 RVA: 0x0001AE20 File Offset: 0x00019020
	public void SetRightItem(string name)
	{
		if (this.m_rightItem == name)
		{
			return;
		}
		this.m_rightItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_rightItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x06000394 RID: 916 RVA: 0x0001AE84 File Offset: 0x00019084
	public void SetLeftBackItem(string name, int variant)
	{
		if (this.m_leftBackItem == name && this.m_leftBackItemVariant == variant)
		{
			return;
		}
		this.m_leftBackItem = name;
		this.m_leftBackItemVariant = variant;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_leftBackItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
			this.m_nview.GetZDO().Set(ZDOVars.s_leftBackItemVariant, variant, false);
		}
	}

	// Token: 0x06000395 RID: 917 RVA: 0x0001AF10 File Offset: 0x00019110
	public void SetRightBackItem(string name)
	{
		if (this.m_rightBackItem == name)
		{
			return;
		}
		this.m_rightBackItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_rightBackItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x06000396 RID: 918 RVA: 0x0001AF74 File Offset: 0x00019174
	public void SetChestItem(string name)
	{
		if (this.m_chestItem == name)
		{
			return;
		}
		this.m_chestItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_chestItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x06000397 RID: 919 RVA: 0x0001AFD8 File Offset: 0x000191D8
	public void SetLegItem(string name)
	{
		if (this.m_legItem == name)
		{
			return;
		}
		this.m_legItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_legItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x06000398 RID: 920 RVA: 0x0001B03C File Offset: 0x0001923C
	public void SetHelmetItem(string name)
	{
		if (this.m_helmetItem == name)
		{
			return;
		}
		this.m_helmetItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_helmetItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x06000399 RID: 921 RVA: 0x0001B0A0 File Offset: 0x000192A0
	public void SetShoulderItem(string name, int variant)
	{
		if (this.m_shoulderItem == name && this.m_shoulderItemVariant == variant)
		{
			return;
		}
		this.m_shoulderItem = name;
		this.m_shoulderItemVariant = variant;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_shoulderItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
			this.m_nview.GetZDO().Set(ZDOVars.s_shoulderItemVariant, variant, false);
		}
	}

	// Token: 0x0600039A RID: 922 RVA: 0x0001B12C File Offset: 0x0001932C
	public void SetBeardItem(string name)
	{
		if (this.m_beardItem == name)
		{
			return;
		}
		this.m_beardItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_beardItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x0600039B RID: 923 RVA: 0x0001B190 File Offset: 0x00019390
	public void SetHairItem(string name)
	{
		if (this.m_hairItem == name)
		{
			return;
		}
		this.m_hairItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_hairItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x0600039C RID: 924 RVA: 0x0001B1F4 File Offset: 0x000193F4
	public void SetUtilityItem(string name)
	{
		if (this.m_utilityItem == name)
		{
			return;
		}
		this.m_utilityItem = name;
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_utilityItem, string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode(), false);
		}
	}

	// Token: 0x0600039D RID: 925 RVA: 0x0001AB3C File Offset: 0x00018D3C
	public void CustomUpdate()
	{
		this.UpdateVisuals();
	}

	// Token: 0x0600039E RID: 926 RVA: 0x0001B258 File Offset: 0x00019458
	private void UpdateVisuals()
	{
		this.UpdateEquipmentVisuals();
		if (this.m_isPlayer)
		{
			this.UpdateBaseModel();
			this.UpdateColors();
		}
	}

	// Token: 0x0600039F RID: 927 RVA: 0x0001B274 File Offset: 0x00019474
	private void UpdateColors()
	{
		Color value = Utils.Vec3ToColor(this.m_skinColor);
		Color value2 = Utils.Vec3ToColor(this.m_hairColor);
		if (this.m_nview.GetZDO() != null)
		{
			value = Utils.Vec3ToColor(this.m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one));
			value2 = Utils.Vec3ToColor(this.m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.one));
		}
		this.m_bodyModel.materials[0].SetColor("_SkinColor", value);
		this.m_bodyModel.materials[1].SetColor("_SkinColor", value2);
		if (this.m_beardItemInstance)
		{
			Renderer[] componentsInChildren = this.m_beardItemInstance.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_SkinColor", value2);
			}
		}
		if (this.m_hairItemInstance)
		{
			Renderer[] componentsInChildren = this.m_hairItemInstance.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_SkinColor", value2);
			}
		}
	}

	// Token: 0x060003A0 RID: 928 RVA: 0x0001B38C File Offset: 0x0001958C
	private void UpdateBaseModel()
	{
		if (this.m_models.Length == 0)
		{
			return;
		}
		int num = this.m_modelIndex;
		if (this.m_nview.GetZDO() != null)
		{
			num = this.m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, 0);
		}
		if (this.m_currentModelIndex != num || this.m_bodyModel.sharedMesh != this.m_models[num].m_mesh)
		{
			this.m_currentModelIndex = num;
			this.m_bodyModel.sharedMesh = this.m_models[num].m_mesh;
			this.m_bodyModel.materials[0].SetTexture("_MainTex", this.m_models[num].m_baseMaterial.GetTexture("_MainTex"));
			this.m_bodyModel.materials[0].SetTexture("_SkinBumpMap", this.m_models[num].m_baseMaterial.GetTexture("_SkinBumpMap"));
		}
	}

	// Token: 0x060003A1 RID: 929 RVA: 0x0001B474 File Offset: 0x00019674
	private void UpdateEquipmentVisuals()
	{
		int hash = 0;
		int rightHandEquipped = 0;
		int chestEquipped = 0;
		int legEquipped = 0;
		int hash2 = 0;
		int beardEquipped = 0;
		int num = 0;
		int hash3 = 0;
		int utilityEquipped = 0;
		int leftItem = 0;
		int rightItem = 0;
		int variant = this.m_shoulderItemVariant;
		int variant2 = this.m_leftItemVariant;
		int leftVariant = this.m_leftBackItemVariant;
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo != null)
		{
			hash = zdo.GetInt(ZDOVars.s_leftItem, 0);
			rightHandEquipped = zdo.GetInt(ZDOVars.s_rightItem, 0);
			chestEquipped = zdo.GetInt(ZDOVars.s_chestItem, 0);
			legEquipped = zdo.GetInt(ZDOVars.s_legItem, 0);
			hash2 = zdo.GetInt(ZDOVars.s_helmetItem, 0);
			hash3 = zdo.GetInt(ZDOVars.s_shoulderItem, 0);
			utilityEquipped = zdo.GetInt(ZDOVars.s_utilityItem, 0);
			if (this.m_isPlayer)
			{
				beardEquipped = zdo.GetInt(ZDOVars.s_beardItem, 0);
				num = zdo.GetInt(ZDOVars.s_hairItem, 0);
				leftItem = zdo.GetInt(ZDOVars.s_leftBackItem, 0);
				rightItem = zdo.GetInt(ZDOVars.s_rightBackItem, 0);
				variant = zdo.GetInt(ZDOVars.s_shoulderItemVariant, 0);
				variant2 = zdo.GetInt(ZDOVars.s_leftItemVariant, 0);
				leftVariant = zdo.GetInt(ZDOVars.s_leftBackItemVariant, 0);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(this.m_leftItem))
			{
				hash = this.m_leftItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_rightItem))
			{
				rightHandEquipped = this.m_rightItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_chestItem))
			{
				chestEquipped = this.m_chestItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_legItem))
			{
				legEquipped = this.m_legItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_helmetItem))
			{
				hash2 = this.m_helmetItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_shoulderItem))
			{
				hash3 = this.m_shoulderItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_utilityItem))
			{
				utilityEquipped = this.m_utilityItem.GetStableHashCode();
			}
			if (this.m_isPlayer)
			{
				if (!string.IsNullOrEmpty(this.m_beardItem))
				{
					beardEquipped = this.m_beardItem.GetStableHashCode();
				}
				if (!string.IsNullOrEmpty(this.m_hairItem))
				{
					num = this.m_hairItem.GetStableHashCode();
				}
				if (!string.IsNullOrEmpty(this.m_leftBackItem))
				{
					leftItem = this.m_leftBackItem.GetStableHashCode();
				}
				if (!string.IsNullOrEmpty(this.m_rightBackItem))
				{
					rightItem = this.m_rightBackItem.GetStableHashCode();
				}
			}
		}
		bool flag = false;
		flag = (this.SetRightHandEquipped(rightHandEquipped) || flag);
		flag = (this.SetLeftHandEquipped(hash, variant2) || flag);
		flag = (this.SetChestEquipped(chestEquipped) || flag);
		flag = (this.SetLegEquipped(legEquipped) || flag);
		flag = (this.SetHelmetEquipped(hash2, num) || flag);
		flag = (this.SetShoulderEquipped(hash3, variant) || flag);
		flag = (this.SetUtilityEquipped(utilityEquipped) || flag);
		if (this.m_isPlayer)
		{
			if (this.m_helmetHideBeard)
			{
				beardEquipped = 0;
			}
			flag = (this.SetBeardEquipped(beardEquipped) || flag);
			flag = (this.SetBackEquipped(leftItem, rightItem, leftVariant) || flag);
			if (this.m_helmetHideHair)
			{
				num = 0;
			}
			flag = (this.SetHairEquipped(num) || flag);
		}
		if (flag)
		{
			this.UpdateLodgroup();
		}
	}

	// Token: 0x060003A2 RID: 930 RVA: 0x0001B780 File Offset: 0x00019980
	private void UpdateLodgroup()
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		List<Renderer> list = new List<Renderer>(this.m_visual.GetComponentsInChildren<Renderer>());
		for (int i = list.Count - 1; i >= 0; i--)
		{
			Renderer renderer = list[i];
			LODGroup componentInParent = renderer.GetComponentInParent<LODGroup>();
			if (componentInParent != null && componentInParent != this.m_lodGroup)
			{
				LOD[] lods = componentInParent.GetLODs();
				for (int j = 0; j < lods.Length; j++)
				{
					if (Array.IndexOf<Renderer>(lods[j].renderers, renderer) >= 0)
					{
						list.RemoveAt(i);
						break;
					}
				}
			}
		}
		LOD[] lods2 = this.m_lodGroup.GetLODs();
		lods2[0].renderers = list.ToArray();
		this.m_lodGroup.SetLODs(lods2);
	}

	// Token: 0x060003A3 RID: 931 RVA: 0x0001B84C File Offset: 0x00019A4C
	private bool SetRightHandEquipped(int hash)
	{
		if (this.m_currentRightItemHash == hash)
		{
			return false;
		}
		if (this.m_rightItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_rightItemInstance);
			this.m_rightItemInstance = null;
		}
		this.m_currentRightItemHash = hash;
		if (hash != 0)
		{
			this.m_rightItemInstance = this.AttachItem(hash, 0, this.m_rightHand, true, false);
		}
		return true;
	}

	// Token: 0x060003A4 RID: 932 RVA: 0x0001B8A4 File Offset: 0x00019AA4
	private bool SetLeftHandEquipped(int hash, int variant)
	{
		if (this.m_currentLeftItemHash == hash && this.m_currentLeftItemVariant == variant)
		{
			return false;
		}
		if (this.m_leftItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_leftItemInstance);
			this.m_leftItemInstance = null;
		}
		this.m_currentLeftItemHash = hash;
		this.m_currentLeftItemVariant = variant;
		if (hash != 0)
		{
			this.m_leftItemInstance = this.AttachItem(hash, variant, this.m_leftHand, true, false);
		}
		return true;
	}

	// Token: 0x060003A5 RID: 933 RVA: 0x0001B90C File Offset: 0x00019B0C
	private bool SetBackEquipped(int leftItem, int rightItem, int leftVariant)
	{
		if (this.m_currentLeftBackItemHash == leftItem && this.m_currentRightBackItemHash == rightItem && this.m_currentLeftBackItemVariant == leftVariant)
		{
			return false;
		}
		if (this.m_leftBackItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_leftBackItemInstance);
			this.m_leftBackItemInstance = null;
		}
		if (this.m_rightBackItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_rightBackItemInstance);
			this.m_rightBackItemInstance = null;
		}
		this.m_currentLeftBackItemHash = leftItem;
		this.m_currentRightBackItemHash = rightItem;
		this.m_currentLeftBackItemVariant = leftVariant;
		if (this.m_currentLeftBackItemHash != 0)
		{
			this.m_leftBackItemInstance = this.AttachBackItem(leftItem, leftVariant, false);
		}
		if (this.m_currentRightBackItemHash != 0)
		{
			this.m_rightBackItemInstance = this.AttachBackItem(rightItem, 0, true);
		}
		return true;
	}

	// Token: 0x060003A6 RID: 934 RVA: 0x0001B9B8 File Offset: 0x00019BB8
	private GameObject AttachBackItem(int hash, int variant, bool rightHand)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing back attach item prefab: " + hash.ToString());
			return null;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		ItemDrop.ItemData.ItemType itemType = (component.m_itemData.m_shared.m_attachOverride != ItemDrop.ItemData.ItemType.None) ? component.m_itemData.m_shared.m_attachOverride : component.m_itemData.m_shared.m_itemType;
		if (itemType == ItemDrop.ItemData.ItemType.Torch)
		{
			if (rightHand)
			{
				return this.AttachItem(hash, variant, this.m_backMelee, false, true);
			}
			return this.AttachItem(hash, variant, this.m_backTool, false, true);
		}
		else
		{
			switch (itemType)
			{
			case ItemDrop.ItemData.ItemType.OneHandedWeapon:
				return this.AttachItem(hash, variant, this.m_backMelee, false, true);
			case ItemDrop.ItemData.ItemType.Bow:
				return this.AttachItem(hash, variant, this.m_backBow, false, true);
			case ItemDrop.ItemData.ItemType.Shield:
				return this.AttachItem(hash, variant, this.m_backShield, false, true);
			default:
				if (itemType != ItemDrop.ItemData.ItemType.TwoHandedWeapon)
				{
					switch (itemType)
					{
					case ItemDrop.ItemData.ItemType.Tool:
						return this.AttachItem(hash, variant, this.m_backTool, false, true);
					case ItemDrop.ItemData.ItemType.Attach_Atgeir:
						return this.AttachItem(hash, variant, this.m_backAtgeir, false, true);
					case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
						goto IL_10B;
					}
					return null;
				}
				IL_10B:
				return this.AttachItem(hash, variant, this.m_backTwohandedMelee, false, true);
			}
		}
	}

	// Token: 0x060003A7 RID: 935 RVA: 0x0001BAF4 File Offset: 0x00019CF4
	private bool SetChestEquipped(int hash)
	{
		if (this.m_currentChestItemHash == hash)
		{
			return false;
		}
		this.m_currentChestItemHash = hash;
		if (this.m_bodyModel == null)
		{
			return true;
		}
		if (this.m_chestItemInstances != null)
		{
			foreach (GameObject gameObject in this.m_chestItemInstances)
			{
				if (this.m_lodGroup)
				{
					Utils.RemoveFromLodgroup(this.m_lodGroup, gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			this.m_chestItemInstances = null;
			this.m_bodyModel.material.SetTexture("_ChestTex", this.m_emptyBodyTexture);
			this.m_bodyModel.material.SetTexture("_ChestBumpMap", null);
			this.m_bodyModel.material.SetTexture("_ChestMetal", null);
		}
		if (this.m_currentChestItemHash == 0)
		{
			return true;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing chest item " + hash.ToString());
			return true;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component.m_itemData.m_shared.m_armorMaterial)
		{
			this.m_bodyModel.material.SetTexture("_ChestTex", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestTex"));
			this.m_bodyModel.material.SetTexture("_ChestBumpMap", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestBumpMap"));
			this.m_bodyModel.material.SetTexture("_ChestMetal", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestMetal"));
		}
		this.m_chestItemInstances = this.AttachArmor(hash, -1);
		return true;
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x0001BCD0 File Offset: 0x00019ED0
	private bool SetShoulderEquipped(int hash, int variant)
	{
		if (this.m_currentShoulderItemHash == hash && this.m_currentShoulderItemVariant == variant)
		{
			return false;
		}
		this.m_currentShoulderItemHash = hash;
		this.m_currentShoulderItemVariant = variant;
		if (this.m_bodyModel == null)
		{
			return true;
		}
		if (this.m_shoulderItemInstances != null)
		{
			foreach (GameObject gameObject in this.m_shoulderItemInstances)
			{
				if (this.m_lodGroup)
				{
					Utils.RemoveFromLodgroup(this.m_lodGroup, gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			this.m_shoulderItemInstances = null;
		}
		if (this.m_currentShoulderItemHash == 0)
		{
			return true;
		}
		if (ObjectDB.instance.GetItemPrefab(hash) == null)
		{
			ZLog.Log("Missing shoulder item " + hash.ToString());
			return true;
		}
		this.m_shoulderItemInstances = this.AttachArmor(hash, variant);
		return true;
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x0001BDC0 File Offset: 0x00019FC0
	private bool SetLegEquipped(int hash)
	{
		if (this.m_currentLegItemHash == hash)
		{
			return false;
		}
		this.m_currentLegItemHash = hash;
		if (this.m_bodyModel == null)
		{
			return true;
		}
		if (this.m_legItemInstances != null)
		{
			foreach (GameObject obj in this.m_legItemInstances)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.m_legItemInstances = null;
			this.m_bodyModel.material.SetTexture("_LegsTex", this.m_emptyLegsTexture);
			this.m_bodyModel.material.SetTexture("_LegsBumpMap", null);
			this.m_bodyModel.material.SetTexture("_LegsMetal", null);
		}
		if (this.m_currentLegItemHash == 0)
		{
			return true;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing legs item " + hash.ToString());
			return true;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component.m_itemData.m_shared.m_armorMaterial)
		{
			this.m_bodyModel.material.SetTexture("_LegsTex", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsTex"));
			this.m_bodyModel.material.SetTexture("_LegsBumpMap", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsBumpMap"));
			this.m_bodyModel.material.SetTexture("_LegsMetal", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsMetal"));
		}
		this.m_legItemInstances = this.AttachArmor(hash, -1);
		return true;
	}

	// Token: 0x060003AA RID: 938 RVA: 0x0001BF80 File Offset: 0x0001A180
	private bool SetBeardEquipped(int hash)
	{
		if (this.m_currentBeardItemHash == hash)
		{
			return false;
		}
		if (this.m_beardItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_beardItemInstance);
			this.m_beardItemInstance = null;
		}
		this.m_currentBeardItemHash = hash;
		if (hash != 0)
		{
			this.m_beardItemInstance = this.AttachItem(hash, 0, this.m_helmet, true, false);
		}
		return true;
	}

	// Token: 0x060003AB RID: 939 RVA: 0x0001BFD8 File Offset: 0x0001A1D8
	private bool SetHairEquipped(int hash)
	{
		if (this.m_currentHairItemHash == hash)
		{
			return false;
		}
		if (this.m_hairItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_hairItemInstance);
			this.m_hairItemInstance = null;
		}
		this.m_currentHairItemHash = hash;
		if (hash != 0)
		{
			this.m_hairItemInstance = this.AttachItem(hash, 0, this.m_helmet, true, false);
		}
		return true;
	}

	// Token: 0x060003AC RID: 940 RVA: 0x0001C030 File Offset: 0x0001A230
	private bool SetHelmetEquipped(int hash, int hairHash)
	{
		if (this.m_currentHelmetItemHash == hash)
		{
			return false;
		}
		if (this.m_helmetItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_helmetItemInstance);
			this.m_helmetItemInstance = null;
		}
		this.m_currentHelmetItemHash = hash;
		VisEquipment.HelmetHides(hash, out this.m_helmetHideHair, out this.m_helmetHideBeard);
		if (hash != 0)
		{
			this.m_helmetItemInstance = this.AttachItem(hash, 0, this.m_helmet, true, false);
		}
		return true;
	}

	// Token: 0x060003AD RID: 941 RVA: 0x0001C09C File Offset: 0x0001A29C
	private bool SetUtilityEquipped(int hash)
	{
		if (this.m_currentUtilityItemHash == hash)
		{
			return false;
		}
		if (this.m_utilityItemInstances != null)
		{
			foreach (GameObject gameObject in this.m_utilityItemInstances)
			{
				if (this.m_lodGroup)
				{
					Utils.RemoveFromLodgroup(this.m_lodGroup, gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			this.m_utilityItemInstances = null;
		}
		this.m_currentUtilityItemHash = hash;
		if (hash != 0)
		{
			this.m_utilityItemInstances = this.AttachArmor(hash, -1);
		}
		return true;
	}

	// Token: 0x060003AE RID: 942 RVA: 0x0001C13C File Offset: 0x0001A33C
	private static void HelmetHides(int itemHash, out bool hideHair, out bool hideBeard)
	{
		hideHair = false;
		hideBeard = false;
		if (itemHash == 0)
		{
			return;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if (itemPrefab == null)
		{
			return;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		hideHair = component.m_itemData.m_shared.m_helmetHideHair;
		hideBeard = component.m_itemData.m_shared.m_helmetHideBeard;
	}

	// Token: 0x060003AF RID: 943 RVA: 0x0001C194 File Offset: 0x0001A394
	private List<GameObject> AttachArmor(int itemHash, int variant = -1)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing attach item: " + itemHash.ToString() + "  ob:" + base.gameObject.name);
			return null;
		}
		List<GameObject> list = new List<GameObject>();
		int childCount = itemPrefab.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = itemPrefab.transform.GetChild(i);
			if (child.gameObject.name.CustomStartsWith("attach_"))
			{
				string text = child.gameObject.name.Substring(7);
				GameObject gameObject;
				if (text == "skin")
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(child.gameObject, this.m_bodyModel.transform.position, this.m_bodyModel.transform.parent.rotation, this.m_bodyModel.transform.parent);
					gameObject.SetActive(true);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
					{
						skinnedMeshRenderer.rootBone = this.m_bodyModel.rootBone;
						skinnedMeshRenderer.bones = this.m_bodyModel.bones;
					}
					foreach (Cloth cloth in gameObject.GetComponentsInChildren<Cloth>())
					{
						if (this.m_clothColliders.Length != 0)
						{
							if (cloth.capsuleColliders.Length != 0)
							{
								List<CapsuleCollider> list2 = new List<CapsuleCollider>(this.m_clothColliders);
								list2.AddRange(cloth.capsuleColliders);
								cloth.capsuleColliders = list2.ToArray();
							}
							else
							{
								cloth.capsuleColliders = this.m_clothColliders;
							}
						}
					}
				}
				else
				{
					Transform transform = Utils.FindChild(this.m_visual.transform, text);
					if (transform == null)
					{
						ZLog.LogWarning("Missing joint " + text + " in item " + itemPrefab.name);
						goto IL_255;
					}
					gameObject = UnityEngine.Object.Instantiate<GameObject>(child.gameObject);
					gameObject.SetActive(true);
					gameObject.transform.SetParent(transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
				}
				if (variant >= 0)
				{
					IEquipmentVisual componentInChildren = gameObject.GetComponentInChildren<IEquipmentVisual>();
					if (componentInChildren != null)
					{
						componentInChildren.Setup(variant);
					}
				}
				VisEquipment.CleanupInstance(gameObject);
				VisEquipment.EnableEquippedEffects(gameObject);
				list.Add(gameObject);
			}
			IL_255:;
		}
		return list;
	}

	// Token: 0x060003B0 RID: 944 RVA: 0x0001C404 File Offset: 0x0001A604
	private GameObject AttachItem(int itemHash, int variant, Transform joint, bool enableEquipEffects = true, bool backAttach = false)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if (itemPrefab == null)
		{
			ZLog.Log(string.Concat(new string[]
			{
				"Missing attach item: ",
				itemHash.ToString(),
				"  ob:",
				base.gameObject.name,
				"  joint:",
				joint ? joint.name : "none"
			}));
			return null;
		}
		GameObject gameObject = null;
		int childCount = itemPrefab.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = itemPrefab.transform.GetChild(i);
			if (backAttach && child.gameObject.name == "attach_back")
			{
				gameObject = child.gameObject;
				break;
			}
			if (child.gameObject.name == "attach" || (!backAttach && child.gameObject.name == "attach_skin"))
			{
				gameObject = child.gameObject;
				break;
			}
		}
		if (gameObject == null)
		{
			return null;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
		gameObject2.SetActive(true);
		VisEquipment.CleanupInstance(gameObject2);
		if (enableEquipEffects)
		{
			VisEquipment.EnableEquippedEffects(gameObject2);
		}
		if (gameObject.name == "attach_skin")
		{
			gameObject2.transform.SetParent(this.m_bodyModel.transform.parent);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject2.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				skinnedMeshRenderer.rootBone = this.m_bodyModel.rootBone;
				skinnedMeshRenderer.bones = this.m_bodyModel.bones;
			}
		}
		else
		{
			gameObject2.transform.SetParent(joint);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
		}
		IEquipmentVisual componentInChildren = gameObject2.GetComponentInChildren<IEquipmentVisual>();
		if (componentInChildren != null)
		{
			componentInChildren.Setup(variant);
		}
		return gameObject2;
	}

	// Token: 0x060003B1 RID: 945 RVA: 0x0001C60C File Offset: 0x0001A80C
	private static void CleanupInstance(GameObject instance)
	{
		Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	// Token: 0x060003B2 RID: 946 RVA: 0x0001C638 File Offset: 0x0001A838
	private static void EnableEquippedEffects(GameObject instance)
	{
		Transform transform = instance.transform.Find("equiped");
		if (transform)
		{
			transform.gameObject.SetActive(true);
		}
	}

	// Token: 0x060003B3 RID: 947 RVA: 0x0001C66C File Offset: 0x0001A86C
	public int GetModelIndex()
	{
		int result = this.m_modelIndex;
		if (this.m_nview.IsValid())
		{
			result = this.m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, 0);
		}
		return result;
	}

	// Token: 0x17000008 RID: 8
	// (get) Token: 0x060003B4 RID: 948 RVA: 0x0001C6A5 File Offset: 0x0001A8A5
	public static List<VisEquipment> Instances { get; } = new List<VisEquipment>();

	// Token: 0x0400036B RID: 875
	public SkinnedMeshRenderer m_bodyModel;

	// Token: 0x0400036C RID: 876
	public ZNetView m_nViewOverride;

	// Token: 0x0400036D RID: 877
	[Header("Attachment points")]
	public Transform m_leftHand;

	// Token: 0x0400036E RID: 878
	public Transform m_rightHand;

	// Token: 0x0400036F RID: 879
	public Transform m_helmet;

	// Token: 0x04000370 RID: 880
	public Transform m_backShield;

	// Token: 0x04000371 RID: 881
	public Transform m_backMelee;

	// Token: 0x04000372 RID: 882
	public Transform m_backTwohandedMelee;

	// Token: 0x04000373 RID: 883
	public Transform m_backBow;

	// Token: 0x04000374 RID: 884
	public Transform m_backTool;

	// Token: 0x04000375 RID: 885
	public Transform m_backAtgeir;

	// Token: 0x04000376 RID: 886
	public CapsuleCollider[] m_clothColliders = Array.Empty<CapsuleCollider>();

	// Token: 0x04000377 RID: 887
	public VisEquipment.PlayerModel[] m_models = Array.Empty<VisEquipment.PlayerModel>();

	// Token: 0x04000378 RID: 888
	public bool m_isPlayer;

	// Token: 0x04000379 RID: 889
	public bool m_useAllTrails;

	// Token: 0x0400037A RID: 890
	private string m_leftItem = "";

	// Token: 0x0400037B RID: 891
	private string m_rightItem = "";

	// Token: 0x0400037C RID: 892
	private string m_chestItem = "";

	// Token: 0x0400037D RID: 893
	private string m_legItem = "";

	// Token: 0x0400037E RID: 894
	private string m_helmetItem = "";

	// Token: 0x0400037F RID: 895
	private string m_shoulderItem = "";

	// Token: 0x04000380 RID: 896
	private string m_beardItem = "";

	// Token: 0x04000381 RID: 897
	private string m_hairItem = "";

	// Token: 0x04000382 RID: 898
	private string m_utilityItem = "";

	// Token: 0x04000383 RID: 899
	private string m_leftBackItem = "";

	// Token: 0x04000384 RID: 900
	private string m_rightBackItem = "";

	// Token: 0x04000385 RID: 901
	private int m_shoulderItemVariant;

	// Token: 0x04000386 RID: 902
	private int m_leftItemVariant;

	// Token: 0x04000387 RID: 903
	private int m_leftBackItemVariant;

	// Token: 0x04000388 RID: 904
	private GameObject m_leftItemInstance;

	// Token: 0x04000389 RID: 905
	private GameObject m_rightItemInstance;

	// Token: 0x0400038A RID: 906
	private GameObject m_helmetItemInstance;

	// Token: 0x0400038B RID: 907
	private List<GameObject> m_chestItemInstances;

	// Token: 0x0400038C RID: 908
	private List<GameObject> m_legItemInstances;

	// Token: 0x0400038D RID: 909
	private List<GameObject> m_shoulderItemInstances;

	// Token: 0x0400038E RID: 910
	private List<GameObject> m_utilityItemInstances;

	// Token: 0x0400038F RID: 911
	private GameObject m_beardItemInstance;

	// Token: 0x04000390 RID: 912
	private GameObject m_hairItemInstance;

	// Token: 0x04000391 RID: 913
	private GameObject m_leftBackItemInstance;

	// Token: 0x04000392 RID: 914
	private GameObject m_rightBackItemInstance;

	// Token: 0x04000393 RID: 915
	private int m_currentLeftItemHash;

	// Token: 0x04000394 RID: 916
	private int m_currentRightItemHash;

	// Token: 0x04000395 RID: 917
	private int m_currentChestItemHash;

	// Token: 0x04000396 RID: 918
	private int m_currentLegItemHash;

	// Token: 0x04000397 RID: 919
	private int m_currentHelmetItemHash;

	// Token: 0x04000398 RID: 920
	private int m_currentShoulderItemHash;

	// Token: 0x04000399 RID: 921
	private int m_currentBeardItemHash;

	// Token: 0x0400039A RID: 922
	private int m_currentHairItemHash;

	// Token: 0x0400039B RID: 923
	private int m_currentUtilityItemHash;

	// Token: 0x0400039C RID: 924
	private int m_currentLeftBackItemHash;

	// Token: 0x0400039D RID: 925
	private int m_currentRightBackItemHash;

	// Token: 0x0400039E RID: 926
	private int m_currentShoulderItemVariant;

	// Token: 0x0400039F RID: 927
	private int m_currentLeftItemVariant;

	// Token: 0x040003A0 RID: 928
	private int m_currentLeftBackItemVariant;

	// Token: 0x040003A1 RID: 929
	private bool m_helmetHideHair;

	// Token: 0x040003A2 RID: 930
	private bool m_helmetHideBeard;

	// Token: 0x040003A3 RID: 931
	private Texture m_emptyBodyTexture;

	// Token: 0x040003A4 RID: 932
	private Texture m_emptyLegsTexture;

	// Token: 0x040003A5 RID: 933
	private int m_modelIndex;

	// Token: 0x040003A6 RID: 934
	private Vector3 m_skinColor = Vector3.one;

	// Token: 0x040003A7 RID: 935
	private Vector3 m_hairColor = Vector3.one;

	// Token: 0x040003A8 RID: 936
	private int m_currentModelIndex;

	// Token: 0x040003A9 RID: 937
	private ZNetView m_nview;

	// Token: 0x040003AA RID: 938
	private GameObject m_visual;

	// Token: 0x040003AB RID: 939
	private LODGroup m_lodGroup;

	// Token: 0x0200003D RID: 61
	[Serializable]
	public class PlayerModel
	{
		// Token: 0x040003AD RID: 941
		public Mesh m_mesh;

		// Token: 0x040003AE RID: 942
		public Material m_baseMaterial;
	}
}
