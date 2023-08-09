using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Token: 0x020001BC RID: 444
[Serializable]
public class BiomeEnvSetup
{
	// Token: 0x04001263 RID: 4707
	public string m_name = "";

	// Token: 0x04001264 RID: 4708
	public Heightmap.Biome m_biome = Heightmap.Biome.Meadows;

	// Token: 0x04001265 RID: 4709
	public List<EnvEntry> m_environments = new List<EnvEntry>();

	// Token: 0x04001266 RID: 4710
	public string m_musicMorning = "morning";

	// Token: 0x04001267 RID: 4711
	public string m_musicEvening = "evening";

	// Token: 0x04001268 RID: 4712
	[FormerlySerializedAs("m_musicRandomDay")]
	public string m_musicDay = "";

	// Token: 0x04001269 RID: 4713
	[FormerlySerializedAs("m_musicRandomNight")]
	public string m_musicNight = "";
}
