using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002C4 RID: 708
public class WearNTearUpdater : MonoBehaviour
{
	// Token: 0x06001AD0 RID: 6864 RVA: 0x000B279C File Offset: 0x000B099C
	private void Update()
	{
		float time = Time.time;
		if (time < this.m_sleepUntil)
		{
			return;
		}
		List<WearNTear> allInstances = WearNTear.GetAllInstances();
		float deltaTime = Time.deltaTime;
		foreach (WearNTear wearNTear in allInstances)
		{
			wearNTear.UpdateCover(deltaTime);
		}
		int num = this.m_index;
		int num2 = 0;
		while (num2 < 50 && allInstances.Count != 0 && num < allInstances.Count)
		{
			allInstances[num].UpdateWear(time);
			num++;
			num2++;
		}
		this.m_index = ((num < allInstances.Count) ? num : 0);
		if (this.m_index == 0)
		{
			this.m_sleepUntil = time + 0.5f;
		}
	}

	// Token: 0x04001CF7 RID: 7415
	private int m_index;

	// Token: 0x04001CF8 RID: 7416
	private float m_sleepUntil;

	// Token: 0x04001CF9 RID: 7417
	private const int c_UpdatesPerFrame = 50;

	// Token: 0x04001CFA RID: 7418
	private const float c_SleepTime = 0.5f;
}
