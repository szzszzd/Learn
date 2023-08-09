using System;
using UnityEngine;

// Token: 0x02000002 RID: 2
public class AnimSetTrigger : StateMachineBehaviour
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(this.TriggerOnEnter))
		{
			if (this.TriggerOnEnterEnable)
			{
				animator.SetTrigger(this.TriggerOnEnter);
				return;
			}
			animator.ResetTrigger(this.TriggerOnEnter);
		}
	}

	// Token: 0x06000002 RID: 2 RVA: 0x00002080 File Offset: 0x00000280
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(this.TriggerOnExit))
		{
			if (this.TriggerOnExitEnable)
			{
				animator.SetTrigger(this.TriggerOnExit);
				return;
			}
			animator.ResetTrigger(this.TriggerOnExit);
		}
	}

	// Token: 0x04000001 RID: 1
	public string TriggerOnEnter;

	// Token: 0x04000002 RID: 2
	public bool TriggerOnEnterEnable = true;

	// Token: 0x04000003 RID: 3
	public string TriggerOnExit;

	// Token: 0x04000004 RID: 4
	public bool TriggerOnExitEnable = true;
}
