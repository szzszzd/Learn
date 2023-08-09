using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002B RID: 43
public class RandomAnimation : MonoBehaviour
{
	// Token: 0x060002F1 RID: 753 RVA: 0x00017052 File Offset: 0x00015252
	private void Start()
	{
		this.m_anim = base.GetComponentInChildren<Animator>();
		this.m_nview = base.GetComponent<ZNetView>();
	}

	// Token: 0x060002F2 RID: 754 RVA: 0x0001706C File Offset: 0x0001526C
	private void FixedUpdate()
	{
		if (this.m_nview != null && !this.m_nview.IsValid())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		foreach (RandomAnimation.RandomValue randomValue in this.m_values)
		{
			if (this.m_nview == null || this.m_nview.IsOwner())
			{
				randomValue.m_timer += fixedDeltaTime;
				if (randomValue.m_timer > randomValue.m_interval)
				{
					randomValue.m_timer = 0f;
					randomValue.m_value = UnityEngine.Random.Range(0, randomValue.m_values);
					if (this.m_nview)
					{
						this.m_nview.GetZDO().Set("RA_" + randomValue.m_name, randomValue.m_value);
					}
					if (!randomValue.m_floatValue)
					{
						this.m_anim.SetInteger(randomValue.m_name, randomValue.m_value);
					}
				}
			}
			if (this.m_nview && !this.m_nview.IsOwner())
			{
				int @int = this.m_nview.GetZDO().GetInt("RA_" + randomValue.m_name, 0);
				if (@int != randomValue.m_value)
				{
					randomValue.m_value = @int;
					if (!randomValue.m_floatValue)
					{
						this.m_anim.SetInteger(randomValue.m_name, randomValue.m_value);
					}
				}
			}
			if (randomValue.m_floatValue)
			{
				if (randomValue.m_hashValues == null || randomValue.m_hashValues.Length != randomValue.m_values)
				{
					randomValue.m_hashValues = new int[randomValue.m_values];
					for (int i = 0; i < randomValue.m_values; i++)
					{
						randomValue.m_hashValues[i] = ZSyncAnimation.GetHash(randomValue.m_name + i.ToString());
					}
				}
				for (int j = 0; j < randomValue.m_values; j++)
				{
					float num = this.m_anim.GetFloat(randomValue.m_hashValues[j]);
					if (j == randomValue.m_value)
					{
						num = Mathf.MoveTowards(num, 1f, fixedDeltaTime / randomValue.m_floatTransition);
					}
					else
					{
						num = Mathf.MoveTowards(num, 0f, fixedDeltaTime / randomValue.m_floatTransition);
					}
					this.m_anim.SetFloat(randomValue.m_hashValues[j], num);
				}
			}
		}
	}

	// Token: 0x040002C2 RID: 706
	public List<RandomAnimation.RandomValue> m_values = new List<RandomAnimation.RandomValue>();

	// Token: 0x040002C3 RID: 707
	private Animator m_anim;

	// Token: 0x040002C4 RID: 708
	private ZNetView m_nview;

	// Token: 0x0200002C RID: 44
	[Serializable]
	public class RandomValue
	{
		// Token: 0x040002C5 RID: 709
		public string m_name;

		// Token: 0x040002C6 RID: 710
		public int m_values;

		// Token: 0x040002C7 RID: 711
		public float m_interval;

		// Token: 0x040002C8 RID: 712
		public bool m_floatValue;

		// Token: 0x040002C9 RID: 713
		public float m_floatTransition = 1f;

		// Token: 0x040002CA RID: 714
		[NonSerialized]
		public float m_timer;

		// Token: 0x040002CB RID: 715
		[NonSerialized]
		public int m_value;

		// Token: 0x040002CC RID: 716
		[NonSerialized]
		public int[] m_hashValues;
	}
}
