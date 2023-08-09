﻿using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000A5 RID: 165
public class DamageText : MonoBehaviour
{
	// Token: 0x17000029 RID: 41
	// (get) Token: 0x0600073D RID: 1853 RVA: 0x00037A10 File Offset: 0x00035C10
	public static DamageText instance
	{
		get
		{
			return DamageText.m_instance;
		}
	}

	// Token: 0x0600073E RID: 1854 RVA: 0x00037A17 File Offset: 0x00035C17
	private void Awake()
	{
		DamageText.m_instance = this;
		ZRoutedRpc.instance.Register<ZPackage>("DamageText", new Action<long, ZPackage>(this.RPC_DamageText));
	}

	// Token: 0x0600073F RID: 1855 RVA: 0x00037A3A File Offset: 0x00035C3A
	private void LateUpdate()
	{
		this.UpdateWorldTexts(Time.deltaTime);
	}

	// Token: 0x06000740 RID: 1856 RVA: 0x00037A48 File Offset: 0x00035C48
	private void UpdateWorldTexts(float dt)
	{
		DamageText.WorldTextInstance worldTextInstance = null;
		Camera mainCamera = Utils.GetMainCamera();
		foreach (DamageText.WorldTextInstance worldTextInstance2 in this.m_worldTexts)
		{
			worldTextInstance2.m_timer += dt;
			if (worldTextInstance2.m_timer > this.m_textDuration && worldTextInstance == null)
			{
				worldTextInstance = worldTextInstance2;
			}
			DamageText.WorldTextInstance worldTextInstance3 = worldTextInstance2;
			worldTextInstance3.m_worldPos.y = worldTextInstance3.m_worldPos.y + dt;
			float f = Mathf.Clamp01(worldTextInstance2.m_timer / this.m_textDuration);
			Color color = worldTextInstance2.m_textField.color;
			color.a = 1f - Mathf.Pow(f, 3f);
			worldTextInstance2.m_textField.color = color;
			Vector3 vector = mainCamera.WorldToScreenPoint(worldTextInstance2.m_worldPos);
			if (vector.x < 0f || vector.x > (float)Screen.width || vector.y < 0f || vector.y > (float)Screen.height || vector.z < 0f)
			{
				worldTextInstance2.m_gui.SetActive(false);
			}
			else
			{
				worldTextInstance2.m_gui.SetActive(true);
				worldTextInstance2.m_gui.transform.position = vector;
			}
		}
		if (worldTextInstance != null)
		{
			UnityEngine.Object.Destroy(worldTextInstance.m_gui);
			this.m_worldTexts.Remove(worldTextInstance);
		}
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x00037BC4 File Offset: 0x00035DC4
	private void AddInworldText(DamageText.TextType type, Vector3 pos, float distance, float dmg, bool mySelf)
	{
		DamageText.WorldTextInstance worldTextInstance = new DamageText.WorldTextInstance();
		worldTextInstance.m_worldPos = pos + UnityEngine.Random.insideUnitSphere * 0.5f;
		worldTextInstance.m_gui = UnityEngine.Object.Instantiate<GameObject>(this.m_worldTextBase, base.transform);
		worldTextInstance.m_textField = worldTextInstance.m_gui.GetComponent<Text>();
		this.m_worldTexts.Add(worldTextInstance);
		Color white;
		if (type == DamageText.TextType.Heal)
		{
			white = new Color(0.5f, 1f, 0.5f, 0.7f);
		}
		else if (mySelf)
		{
			if (dmg == 0f)
			{
				white = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
			else
			{
				white = new Color(1f, 0f, 0f, 1f);
			}
		}
		else
		{
			switch (type)
			{
			case DamageText.TextType.Normal:
				white = new Color(1f, 1f, 1f, 1f);
				goto IL_180;
			case DamageText.TextType.Resistant:
				white = new Color(0.6f, 0.6f, 0.6f, 1f);
				goto IL_180;
			case DamageText.TextType.Weak:
				white = new Color(1f, 1f, 0f, 1f);
				goto IL_180;
			case DamageText.TextType.Immune:
				white = new Color(0.6f, 0.6f, 0.6f, 1f);
				goto IL_180;
			case DamageText.TextType.TooHard:
				white = new Color(0.8f, 0.7f, 0.7f, 1f);
				goto IL_180;
			}
			white = Color.white;
		}
		IL_180:
		worldTextInstance.m_textField.color = white;
		if (distance > this.m_smallFontDistance)
		{
			worldTextInstance.m_textField.fontSize = this.m_smallFontSize;
		}
		else
		{
			worldTextInstance.m_textField.fontSize = this.m_largeFontSize;
		}
		string text;
		switch (type)
		{
		case DamageText.TextType.Heal:
			text = "+" + dmg.ToString("0.#", CultureInfo.InvariantCulture);
			break;
		case DamageText.TextType.TooHard:
			text = Localization.instance.Localize("$msg_toohard");
			break;
		case DamageText.TextType.Blocked:
			text = Localization.instance.Localize("$msg_blocked: ") + dmg.ToString("0.#", CultureInfo.InvariantCulture);
			break;
		default:
			text = dmg.ToString("0.#", CultureInfo.InvariantCulture);
			break;
		}
		worldTextInstance.m_textField.text = text;
		worldTextInstance.m_timer = 0f;
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x00037E24 File Offset: 0x00036024
	public void ShowText(HitData.DamageModifier type, Vector3 pos, float dmg, bool player = false)
	{
		DamageText.TextType type2 = DamageText.TextType.Normal;
		switch (type)
		{
		case HitData.DamageModifier.Normal:
			type2 = DamageText.TextType.Normal;
			break;
		case HitData.DamageModifier.Resistant:
			type2 = DamageText.TextType.Resistant;
			break;
		case HitData.DamageModifier.Weak:
			type2 = DamageText.TextType.Weak;
			break;
		case HitData.DamageModifier.Immune:
			type2 = DamageText.TextType.Immune;
			break;
		case HitData.DamageModifier.VeryResistant:
			type2 = DamageText.TextType.Resistant;
			break;
		case HitData.DamageModifier.VeryWeak:
			type2 = DamageText.TextType.Weak;
			break;
		}
		this.ShowText(type2, pos, dmg, player);
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x00037E78 File Offset: 0x00036078
	public void ShowText(DamageText.TextType type, Vector3 pos, float dmg, bool player = false)
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write((int)type);
		zpackage.Write(pos);
		zpackage.Write(dmg);
		zpackage.Write(player);
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "DamageText", new object[]
		{
			zpackage
		});
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x00037EC8 File Offset: 0x000360C8
	private void RPC_DamageText(long sender, ZPackage pkg)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (!mainCamera)
		{
			return;
		}
		if (Hud.IsUserHidden())
		{
			return;
		}
		DamageText.TextType type = (DamageText.TextType)pkg.ReadInt();
		Vector3 vector = pkg.ReadVector3();
		float dmg = pkg.ReadSingle();
		bool flag = pkg.ReadBool();
		float num = Vector3.Distance(mainCamera.transform.position, vector);
		if (num > this.m_maxTextDistance)
		{
			return;
		}
		bool mySelf = flag && sender == ZNet.GetUID();
		this.AddInworldText(type, vector, num, dmg, mySelf);
	}

	// Token: 0x040008C2 RID: 2242
	private static DamageText m_instance;

	// Token: 0x040008C3 RID: 2243
	public float m_textDuration = 1.5f;

	// Token: 0x040008C4 RID: 2244
	public float m_maxTextDistance = 30f;

	// Token: 0x040008C5 RID: 2245
	public int m_largeFontSize = 16;

	// Token: 0x040008C6 RID: 2246
	public int m_smallFontSize = 8;

	// Token: 0x040008C7 RID: 2247
	public float m_smallFontDistance = 10f;

	// Token: 0x040008C8 RID: 2248
	public GameObject m_worldTextBase;

	// Token: 0x040008C9 RID: 2249
	private List<DamageText.WorldTextInstance> m_worldTexts = new List<DamageText.WorldTextInstance>();

	// Token: 0x020000A6 RID: 166
	public enum TextType
	{
		// Token: 0x040008CB RID: 2251
		Normal,
		// Token: 0x040008CC RID: 2252
		Resistant,
		// Token: 0x040008CD RID: 2253
		Weak,
		// Token: 0x040008CE RID: 2254
		Immune,
		// Token: 0x040008CF RID: 2255
		Heal,
		// Token: 0x040008D0 RID: 2256
		TooHard,
		// Token: 0x040008D1 RID: 2257
		Blocked
	}

	// Token: 0x020000A7 RID: 167
	private class WorldTextInstance
	{
		// Token: 0x040008D2 RID: 2258
		public Vector3 m_worldPos;

		// Token: 0x040008D3 RID: 2259
		public GameObject m_gui;

		// Token: 0x040008D4 RID: 2260
		public float m_timer;

		// Token: 0x040008D5 RID: 2261
		public Text m_textField;
	}
}
