using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001FF RID: 511
public class SpawnSystemList : MonoBehaviour
{
	// Token: 0x06001478 RID: 5240 RVA: 0x0008590C File Offset: 0x00083B0C
	public void GetSpawners(Heightmap.Biome biome, List<SpawnSystem.SpawnData> spawners)
	{
		foreach (SpawnSystem.SpawnData spawnData in this.m_spawners)
		{
			if ((spawnData.m_biome & biome) != Heightmap.Biome.None || spawnData.m_biome == biome)
			{
				spawners.Add(spawnData);
			}
		}
	}

	// Token: 0x04001532 RID: 5426
	public List<SpawnSystem.SpawnData> m_spawners = new List<SpawnSystem.SpawnData>();

	// Token: 0x04001533 RID: 5427
	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();
}
