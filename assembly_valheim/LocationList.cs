using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001D0 RID: 464
public class LocationList : MonoBehaviour
{
	// Token: 0x06001304 RID: 4868 RVA: 0x0007D6F5 File Offset: 0x0007B8F5
	private void Awake()
	{
		LocationList.m_allLocationLists.Add(this);
	}

	// Token: 0x06001305 RID: 4869 RVA: 0x0007D702 File Offset: 0x0007B902
	private void OnDestroy()
	{
		LocationList.m_allLocationLists.Remove(this);
	}

	// Token: 0x06001306 RID: 4870 RVA: 0x0007D710 File Offset: 0x0007B910
	public static List<LocationList> GetAllLocationLists()
	{
		return LocationList.m_allLocationLists;
	}

	// Token: 0x040013DE RID: 5086
	private static List<LocationList> m_allLocationLists = new List<LocationList>();

	// Token: 0x040013DF RID: 5087
	public int m_sortOrder;

	// Token: 0x040013E0 RID: 5088
	public List<ZoneSystem.ZoneLocation> m_locations = new List<ZoneSystem.ZoneLocation>();

	// Token: 0x040013E1 RID: 5089
	public List<ZoneSystem.ZoneVegetation> m_vegetation = new List<ZoneSystem.ZoneVegetation>();

	// Token: 0x040013E2 RID: 5090
	public List<EnvSetup> m_environments = new List<EnvSetup>();

	// Token: 0x040013E3 RID: 5091
	public List<BiomeEnvSetup> m_biomeEnvironments = new List<BiomeEnvSetup>();

	// Token: 0x040013E4 RID: 5092
	public List<RandomEvent> m_events = new List<RandomEvent>();

	// Token: 0x040013E5 RID: 5093
	public List<ClutterSystem.Clutter> m_clutter = new List<ClutterSystem.Clutter>();

	// Token: 0x040013E6 RID: 5094
	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();

	// Token: 0x040013E7 RID: 5095
	[HideInInspector]
	public List<Heightmap.Biome> m_vegetationFolded = new List<Heightmap.Biome>();

	// Token: 0x040013E8 RID: 5096
	[HideInInspector]
	public List<Heightmap.Biome> m_locationFolded = new List<Heightmap.Biome>();
}
