using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200006F RID: 111
[Serializable]
public class EffectList
{
	// Token: 0x0600057B RID: 1403 RVA: 0x0002AC8C File Offset: 0x00028E8C
	public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform baseParent = null, float scale = 1f, int variant = -1)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < this.m_effectPrefabs.Length; i++)
		{
			EffectList.EffectData effectData = this.m_effectPrefabs[i];
			if (effectData.m_enabled && (variant < 0 || effectData.m_variant < 0 || variant == effectData.m_variant))
			{
				Transform transform = baseParent;
				Vector3 position = basePos;
				Quaternion rotation = baseRot;
				if (!string.IsNullOrEmpty(effectData.m_childTransform) && baseParent != null)
				{
					Transform transform2 = Utils.FindChild(transform, effectData.m_childTransform);
					if (transform2)
					{
						transform = transform2;
						position = transform.position;
					}
				}
				if (transform && effectData.m_inheritParentRotation)
				{
					rotation = transform.rotation;
				}
				if (effectData.m_randomRotation)
				{
					rotation = UnityEngine.Random.rotation;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(effectData.m_prefab, position, rotation);
				if (effectData.m_scale)
				{
					if (baseParent && effectData.m_inheritParentScale)
					{
						Vector3 localScale = baseParent.localScale * scale;
						gameObject.transform.localScale = localScale;
					}
					else
					{
						gameObject.transform.localScale = new Vector3(scale, scale, scale);
					}
				}
				else if (baseParent && effectData.m_inheritParentScale)
				{
					gameObject.transform.localScale = baseParent.localScale;
				}
				if (effectData.m_attach && transform != null)
				{
					gameObject.transform.SetParent(transform);
				}
				list.Add(gameObject);
			}
		}
		return list.ToArray();
	}

	// Token: 0x0600057C RID: 1404 RVA: 0x0002AE00 File Offset: 0x00029000
	public bool HasEffects()
	{
		if (this.m_effectPrefabs == null || this.m_effectPrefabs.Length == 0)
		{
			return false;
		}
		EffectList.EffectData[] effectPrefabs = this.m_effectPrefabs;
		for (int i = 0; i < effectPrefabs.Length; i++)
		{
			if (effectPrefabs[i].m_enabled)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0400066C RID: 1644
	public EffectList.EffectData[] m_effectPrefabs = new EffectList.EffectData[0];

	// Token: 0x02000070 RID: 112
	[Serializable]
	public class EffectData
	{
		// Token: 0x0400066D RID: 1645
		public GameObject m_prefab;

		// Token: 0x0400066E RID: 1646
		public bool m_enabled = true;

		// Token: 0x0400066F RID: 1647
		public int m_variant = -1;

		// Token: 0x04000670 RID: 1648
		public bool m_attach;

		// Token: 0x04000671 RID: 1649
		public bool m_inheritParentRotation;

		// Token: 0x04000672 RID: 1650
		public bool m_inheritParentScale;

		// Token: 0x04000673 RID: 1651
		public bool m_randomRotation;

		// Token: 0x04000674 RID: 1652
		public bool m_scale;

		// Token: 0x04000675 RID: 1653
		public string m_childTransform;
	}
}
