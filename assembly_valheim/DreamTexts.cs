using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200022C RID: 556
public class DreamTexts : MonoBehaviour
{
	// Token: 0x060015F4 RID: 5620 RVA: 0x00090558 File Offset: 0x0008E758
	public DreamTexts.DreamText GetRandomDreamText()
	{
		List<DreamTexts.DreamText> list = new List<DreamTexts.DreamText>();
		foreach (DreamTexts.DreamText dreamText in this.m_texts)
		{
			if (this.HaveGlobalKeys(dreamText))
			{
				list.Add(dreamText);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		DreamTexts.DreamText dreamText2 = list[UnityEngine.Random.Range(0, list.Count)];
		if (UnityEngine.Random.value <= dreamText2.m_chanceToDream)
		{
			return dreamText2;
		}
		return null;
	}

	// Token: 0x060015F5 RID: 5621 RVA: 0x000905E8 File Offset: 0x0008E7E8
	private bool HaveGlobalKeys(DreamTexts.DreamText dream)
	{
		foreach (string name in dream.m_trueKeys)
		{
			if (!ZoneSystem.instance.GetGlobalKey(name))
			{
				return false;
			}
		}
		foreach (string name2 in dream.m_falseKeys)
		{
			if (ZoneSystem.instance.GetGlobalKey(name2))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x040016E9 RID: 5865
	public List<DreamTexts.DreamText> m_texts = new List<DreamTexts.DreamText>();

	// Token: 0x0200022D RID: 557
	[Serializable]
	public class DreamText
	{
		// Token: 0x040016EA RID: 5866
		public string m_text = "Fluffy sheep";

		// Token: 0x040016EB RID: 5867
		public float m_chanceToDream = 0.1f;

		// Token: 0x040016EC RID: 5868
		public List<string> m_trueKeys = new List<string>();

		// Token: 0x040016ED RID: 5869
		public List<string> m_falseKeys = new List<string>();
	}
}
