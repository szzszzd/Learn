using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000068 RID: 104
public class AnimationEffect : MonoBehaviour
{
	// Token: 0x0600054B RID: 1355 RVA: 0x00029C4A File Offset: 0x00027E4A
	private void Start()
	{
		this.m_animator = base.GetComponent<Animator>();
	}

	// Token: 0x0600054C RID: 1356 RVA: 0x00029C58 File Offset: 0x00027E58
	public void Effect(AnimationEvent e)
	{
		string stringParameter = e.stringParameter;
		GameObject gameObject = e.objectReferenceParameter as GameObject;
		if (gameObject == null)
		{
			return;
		}
		Transform transform = null;
		if (stringParameter.Length > 0)
		{
			transform = Utils.FindChild(base.transform, stringParameter);
		}
		if (transform == null)
		{
			transform = (this.m_effectRoot ? this.m_effectRoot : base.transform);
		}
		UnityEngine.Object.Instantiate<GameObject>(gameObject, transform.position, transform.rotation);
	}

	// Token: 0x0600054D RID: 1357 RVA: 0x00029CD4 File Offset: 0x00027ED4
	public void Attach(AnimationEvent e)
	{
		string stringParameter = e.stringParameter;
		GameObject gameObject = e.objectReferenceParameter as GameObject;
		if (gameObject == null)
		{
			return;
		}
		Transform transform = Utils.FindChild(base.transform, stringParameter);
		if (transform == null)
		{
			ZLog.LogWarning("Failed to find attach joint " + stringParameter);
			return;
		}
		this.ClearAttachment(transform);
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, transform.position, transform.rotation);
		gameObject2.transform.SetParent(transform, true);
		if (this.m_attachments == null)
		{
			this.m_attachments = new List<GameObject>();
		}
		this.m_attachments.Add(gameObject2);
		this.m_attachStateHash = e.animatorStateInfo.fullPathHash;
		base.CancelInvoke("UpdateAttachments");
		base.InvokeRepeating("UpdateAttachments", 0.1f, 0.1f);
	}

	// Token: 0x0600054E RID: 1358 RVA: 0x00029DA0 File Offset: 0x00027FA0
	private void ClearAttachment(Transform parent)
	{
		if (this.m_attachments == null)
		{
			return;
		}
		foreach (GameObject gameObject in this.m_attachments)
		{
			if (gameObject && gameObject.transform.parent == parent)
			{
				this.m_attachments.Remove(gameObject);
				UnityEngine.Object.Destroy(gameObject);
				break;
			}
		}
	}

	// Token: 0x0600054F RID: 1359 RVA: 0x00029E28 File Offset: 0x00028028
	public void RemoveAttachments()
	{
		if (this.m_attachments == null)
		{
			return;
		}
		foreach (GameObject obj in this.m_attachments)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_attachments.Clear();
	}

	// Token: 0x06000550 RID: 1360 RVA: 0x00029E8C File Offset: 0x0002808C
	private void UpdateAttachments()
	{
		if (this.m_attachments != null && this.m_attachments.Count > 0)
		{
			if (this.m_attachStateHash != this.m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash && this.m_attachStateHash != this.m_animator.GetNextAnimatorStateInfo(0).fullPathHash)
			{
				this.RemoveAttachments();
				return;
			}
		}
		else
		{
			base.CancelInvoke("UpdateAttachments");
		}
	}

	// Token: 0x04000632 RID: 1586
	public Transform m_effectRoot;

	// Token: 0x04000633 RID: 1587
	private Animator m_animator;

	// Token: 0x04000634 RID: 1588
	private List<GameObject> m_attachments;

	// Token: 0x04000635 RID: 1589
	private int m_attachStateHash;
}
