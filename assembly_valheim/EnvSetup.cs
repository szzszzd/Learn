using System;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x020001BE RID: 446
[Serializable]
public class EnvSetup
{
	// Token: 0x060011B3 RID: 4531 RVA: 0x00074A9A File Offset: 0x00072C9A
	public EnvSetup Clone()
	{
		return base.MemberwiseClone() as EnvSetup;
	}

	// Token: 0x0400126D RID: 4717
	public string m_name = "";

	// Token: 0x0400126E RID: 4718
	public bool m_default;

	// Token: 0x0400126F RID: 4719
	[Header("Gameplay")]
	public bool m_isWet;

	// Token: 0x04001270 RID: 4720
	public bool m_isFreezing;

	// Token: 0x04001271 RID: 4721
	public bool m_isFreezingAtNight;

	// Token: 0x04001272 RID: 4722
	public bool m_isCold;

	// Token: 0x04001273 RID: 4723
	public bool m_isColdAtNight = true;

	// Token: 0x04001274 RID: 4724
	public bool m_alwaysDark;

	// Token: 0x04001275 RID: 4725
	[Header("Ambience")]
	public Color m_ambColorNight = Color.white;

	// Token: 0x04001276 RID: 4726
	public Color m_ambColorDay = Color.white;

	// Token: 0x04001277 RID: 4727
	[Header("Fog-ambient")]
	public Color m_fogColorNight = Color.white;

	// Token: 0x04001278 RID: 4728
	public Color m_fogColorMorning = Color.white;

	// Token: 0x04001279 RID: 4729
	public Color m_fogColorDay = Color.white;

	// Token: 0x0400127A RID: 4730
	public Color m_fogColorEvening = Color.white;

	// Token: 0x0400127B RID: 4731
	[Header("Fog-sun")]
	public Color m_fogColorSunNight = Color.white;

	// Token: 0x0400127C RID: 4732
	public Color m_fogColorSunMorning = Color.white;

	// Token: 0x0400127D RID: 4733
	public Color m_fogColorSunDay = Color.white;

	// Token: 0x0400127E RID: 4734
	public Color m_fogColorSunEvening = Color.white;

	// Token: 0x0400127F RID: 4735
	[Header("Fog-distance")]
	public float m_fogDensityNight = 0.01f;

	// Token: 0x04001280 RID: 4736
	public float m_fogDensityMorning = 0.01f;

	// Token: 0x04001281 RID: 4737
	public float m_fogDensityDay = 0.01f;

	// Token: 0x04001282 RID: 4738
	public float m_fogDensityEvening = 0.01f;

	// Token: 0x04001283 RID: 4739
	[Header("Sun")]
	public Color m_sunColorNight = Color.white;

	// Token: 0x04001284 RID: 4740
	public Color m_sunColorMorning = Color.white;

	// Token: 0x04001285 RID: 4741
	public Color m_sunColorDay = Color.white;

	// Token: 0x04001286 RID: 4742
	public Color m_sunColorEvening = Color.white;

	// Token: 0x04001287 RID: 4743
	public float m_lightIntensityDay = 1.2f;

	// Token: 0x04001288 RID: 4744
	public float m_lightIntensityNight;

	// Token: 0x04001289 RID: 4745
	public float m_sunAngle = 60f;

	// Token: 0x0400128A RID: 4746
	[Header("Wind")]
	public float m_windMin;

	// Token: 0x0400128B RID: 4747
	public float m_windMax = 1f;

	// Token: 0x0400128C RID: 4748
	[Header("Effects")]
	public GameObject m_envObject;

	// Token: 0x0400128D RID: 4749
	public GameObject[] m_psystems;

	// Token: 0x0400128E RID: 4750
	public bool m_psystemsOutsideOnly;

	// Token: 0x0400128F RID: 4751
	public float m_rainCloudAlpha;

	// Token: 0x04001290 RID: 4752
	[Header("Audio")]
	public AudioClip m_ambientLoop;

	// Token: 0x04001291 RID: 4753
	public float m_ambientVol = 0.3f;

	// Token: 0x04001292 RID: 4754
	public string m_ambientList = "";

	// Token: 0x04001293 RID: 4755
	[Header("Music overrides")]
	public string m_musicMorning = "";

	// Token: 0x04001294 RID: 4756
	public string m_musicEvening = "";

	// Token: 0x04001295 RID: 4757
	[FormerlySerializedAs("m_musicRandomDay")]
	public string m_musicDay = "";

	// Token: 0x04001296 RID: 4758
	[FormerlySerializedAs("m_musicRandomNight")]
	public string m_musicNight = "";
}
