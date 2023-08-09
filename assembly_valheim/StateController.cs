using System;
using UnityEngine;

// Token: 0x0200008E RID: 142
public class StateController : StateMachineBehaviour
{
	// Token: 0x06000636 RID: 1590 RVA: 0x0002F3C0 File Offset: 0x0002D5C0
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.m_enterEffect.HasEffects())
		{
			this.m_enterEffect.Create(this.GetEffectPos(animator), animator.transform.rotation, null, 1f, -1);
		}
		if (this.m_enterDisableChildren)
		{
			for (int i = 0; i < animator.transform.childCount; i++)
			{
				animator.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		if (this.m_enterEnableChildren)
		{
			for (int j = 0; j < animator.transform.childCount; j++)
			{
				animator.transform.GetChild(j).gameObject.SetActive(true);
			}
		}
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x0002F46C File Offset: 0x0002D66C
	private Vector3 GetEffectPos(Animator animator)
	{
		if (this.m_effectJoint.Length == 0)
		{
			return animator.transform.position;
		}
		if (this.m_effectJoinT == null)
		{
			this.m_effectJoinT = Utils.FindChild(animator.transform, this.m_effectJoint);
		}
		return this.m_effectJoinT.position;
	}

	// Token: 0x0400077B RID: 1915
	public string m_effectJoint = "";

	// Token: 0x0400077C RID: 1916
	public EffectList m_enterEffect = new EffectList();

	// Token: 0x0400077D RID: 1917
	public bool m_enterDisableChildren;

	// Token: 0x0400077E RID: 1918
	public bool m_enterEnableChildren;

	// Token: 0x0400077F RID: 1919
	public GameObject[] m_enterDisable = new GameObject[0];

	// Token: 0x04000780 RID: 1920
	public GameObject[] m_enterEnable = new GameObject[0];

	// Token: 0x04000781 RID: 1921
	private Transform m_effectJoinT;
}
