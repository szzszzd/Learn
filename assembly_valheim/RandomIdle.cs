using System;
using UnityEngine;

// Token: 0x0200002D RID: 45
public class RandomIdle : StateMachineBehaviour
{
	// Token: 0x060002F5 RID: 757 RVA: 0x0001730C File Offset: 0x0001550C
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int randomIdle = this.GetRandomIdle(animator);
		animator.SetFloat(this.m_valueName, (float)randomIdle);
		this.m_last = stateInfo.normalizedTime % 1f;
	}

	// Token: 0x060002F6 RID: 758 RVA: 0x00017344 File Offset: 0x00015544
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float num = stateInfo.normalizedTime % 1f;
		if (num < this.m_last)
		{
			int randomIdle = this.GetRandomIdle(animator);
			animator.SetFloat(this.m_valueName, (float)randomIdle);
		}
		this.m_last = num;
	}

	// Token: 0x060002F7 RID: 759 RVA: 0x00017388 File Offset: 0x00015588
	private int GetRandomIdle(Animator animator)
	{
		if (!this.m_haveSetup)
		{
			this.m_haveSetup = true;
			this.m_baseAI = animator.GetComponentInParent<BaseAI>();
			this.m_character = animator.GetComponentInParent<Character>();
		}
		if (this.m_baseAI && this.m_alertedIdle >= 0 && this.m_baseAI.IsAlerted())
		{
			return this.m_alertedIdle;
		}
		return UnityEngine.Random.Range(0, (this.m_animationsWhenTamed > 0 && this.m_character != null && this.m_character.IsTamed()) ? this.m_animationsWhenTamed : this.m_animations);
	}

	// Token: 0x040002CD RID: 717
	public int m_animations = 4;

	// Token: 0x040002CE RID: 718
	public int m_animationsWhenTamed;

	// Token: 0x040002CF RID: 719
	public string m_valueName = "";

	// Token: 0x040002D0 RID: 720
	public int m_alertedIdle = -1;

	// Token: 0x040002D1 RID: 721
	private float m_last;

	// Token: 0x040002D2 RID: 722
	private bool m_haveSetup;

	// Token: 0x040002D3 RID: 723
	private BaseAI m_baseAI;

	// Token: 0x040002D4 RID: 724
	private Character m_character;
}
