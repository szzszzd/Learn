using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000166 RID: 358
internal static class ShuffleClass
{
	// Token: 0x06000E03 RID: 3587 RVA: 0x0006099C File Offset: 0x0005EB9C
	public static void Shuffle<T>(this IList<T> list, bool useUnityRandom = false)
	{
		int i = list.Count;
		while (i > 1)
		{
			i--;
			int index = useUnityRandom ? UnityEngine.Random.Range(0, i) : ShuffleClass.rng.Next(i + 1);
			T value = list[index];
			list[index] = list[i];
			list[i] = value;
		}
	}

	// Token: 0x0400100C RID: 4108
	private static System.Random rng = new System.Random();
}
