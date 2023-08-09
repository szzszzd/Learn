using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200001B RID: 27
public class LevelEffects : MonoBehaviour
{
	// Token: 0x060001AF RID: 431 RVA: 0x0000BFEC File Offset: 0x0000A1EC
	private void Start()
	{
		this.m_character = base.GetComponentInParent<Character>();
		Character character = this.m_character;
		character.m_onLevelSet = (Action<int>)Delegate.Combine(character.m_onLevelSet, new Action<int>(this.OnLevelSet));
		this.SetupLevelVisualization(this.m_character.GetLevel());
	}

	// Token: 0x060001B0 RID: 432 RVA: 0x0000C03D File Offset: 0x0000A23D
	private void OnLevelSet(int level)
	{
		this.SetupLevelVisualization(level);
	}

	// Token: 0x060001B1 RID: 433 RVA: 0x0000C048 File Offset: 0x0000A248
	private void SetupLevelVisualization(int level)
	{
		if (level <= 1)
		{
			return;
		}
		if (this.m_levelSetups.Count >= level - 1)
		{
			LevelEffects.LevelSetup levelSetup = this.m_levelSetups[level - 2];
			base.transform.localScale = new Vector3(levelSetup.m_scale, levelSetup.m_scale, levelSetup.m_scale);
			if (this.m_mainRender)
			{
				string key = Utils.GetPrefabName(this.m_character.gameObject) + level.ToString();
				Material material;
				if (LevelEffects.m_materials.TryGetValue(key, out material))
				{
					Material[] sharedMaterials = this.m_mainRender.sharedMaterials;
					sharedMaterials[0] = material;
					this.m_mainRender.sharedMaterials = sharedMaterials;
				}
				else
				{
					Material[] sharedMaterials2 = this.m_mainRender.sharedMaterials;
					sharedMaterials2[0] = new Material(sharedMaterials2[0]);
					sharedMaterials2[0].SetFloat("_Hue", levelSetup.m_hue);
					sharedMaterials2[0].SetFloat("_Saturation", levelSetup.m_saturation);
					sharedMaterials2[0].SetFloat("_Value", levelSetup.m_value);
					if (levelSetup.m_setEmissiveColor)
					{
						sharedMaterials2[0].SetColor("_EmissionColor", levelSetup.m_emissiveColor);
					}
					this.m_mainRender.sharedMaterials = sharedMaterials2;
					LevelEffects.m_materials[key] = sharedMaterials2[0];
				}
			}
			if (this.m_baseEnableObject)
			{
				this.m_baseEnableObject.SetActive(false);
			}
			if (levelSetup.m_enableObject)
			{
				levelSetup.m_enableObject.SetActive(true);
			}
		}
	}

	// Token: 0x060001B2 RID: 434 RVA: 0x0000C1BC File Offset: 0x0000A3BC
	public void GetColorChanges(out float hue, out float saturation, out float value)
	{
		int level = this.m_character.GetLevel();
		if (level > 1 && this.m_levelSetups.Count >= level - 1)
		{
			LevelEffects.LevelSetup levelSetup = this.m_levelSetups[level - 2];
			hue = levelSetup.m_hue;
			saturation = levelSetup.m_saturation;
			value = levelSetup.m_value;
			return;
		}
		hue = 0f;
		saturation = 0f;
		value = 0f;
	}

	// Token: 0x040001A8 RID: 424
	public Renderer m_mainRender;

	// Token: 0x040001A9 RID: 425
	public GameObject m_baseEnableObject;

	// Token: 0x040001AA RID: 426
	public List<LevelEffects.LevelSetup> m_levelSetups = new List<LevelEffects.LevelSetup>();

	// Token: 0x040001AB RID: 427
	private static Dictionary<string, Material> m_materials = new Dictionary<string, Material>();

	// Token: 0x040001AC RID: 428
	private Character m_character;

	// Token: 0x0200001C RID: 28
	[Serializable]
	public class LevelSetup
	{
		// Token: 0x040001AD RID: 429
		public float m_scale = 1f;

		// Token: 0x040001AE RID: 430
		public float m_hue;

		// Token: 0x040001AF RID: 431
		public float m_saturation;

		// Token: 0x040001B0 RID: 432
		public float m_value;

		// Token: 0x040001B1 RID: 433
		public bool m_setEmissiveColor;

		// Token: 0x040001B2 RID: 434
		[ColorUsage(false, true)]
		public Color m_emissiveColor = Color.white;

		// Token: 0x040001B3 RID: 435
		public GameObject m_enableObject;
	}
}
