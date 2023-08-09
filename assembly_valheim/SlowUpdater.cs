using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001F9 RID: 505
public class SlowUpdater : MonoBehaviour
{
	// Token: 0x06001450 RID: 5200 RVA: 0x00084517 File Offset: 0x00082717
	private void Awake()
	{
		base.StartCoroutine("UpdateLoop");
	}

	// Token: 0x06001451 RID: 5201 RVA: 0x00084525 File Offset: 0x00082725
	private IEnumerator UpdateLoop()
	{
		for (;;)
		{
			List<SlowUpdate> instances = SlowUpdate.GetAllInstaces();
			int index = 0;
			while (index < instances.Count)
			{
				int num = 0;
				while (num < 100 && instances.Count != 0 && index < instances.Count)
				{
					instances[index].SUpdate();
					int num2 = index + 1;
					index = num2;
					num++;
				}
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			instances = null;
		}
		yield break;
	}

	// Token: 0x040014F0 RID: 5360
	private const int m_updatesPerFrame = 100;
}
