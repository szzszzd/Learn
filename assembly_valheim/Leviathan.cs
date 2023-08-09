using System;
using UnityEngine;

// Token: 0x0200025C RID: 604
public class Leviathan : MonoBehaviour
{
	// Token: 0x06001754 RID: 5972 RVA: 0x0009A6DC File Offset: 0x000988DC
	private void Awake()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_zanimator = base.GetComponent<ZSyncAnimation>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		if (base.GetComponent<MineRock>())
		{
			MineRock mineRock = this.m_mineRock;
			mineRock.m_onHit = (Action)Delegate.Combine(mineRock.m_onHit, new Action(this.OnHit));
		}
	}

	// Token: 0x06001755 RID: 5973 RVA: 0x0009A750 File Offset: 0x00098950
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		float liquidLevel = Floating.GetLiquidLevel(base.transform.position, this.m_waveScale, LiquidType.All);
		if (liquidLevel > -100f)
		{
			Vector3 position = this.m_body.position;
			float num = Mathf.Clamp((liquidLevel - (position.y + this.m_floatOffset)) * this.m_movementSpeed * Time.fixedDeltaTime, -this.m_maxSpeed, this.m_maxSpeed);
			position.y += num;
			this.m_body.MovePosition(position);
		}
		else
		{
			Vector3 position2 = this.m_body.position;
			position2.y = 0f;
			this.m_body.MovePosition(Vector3.MoveTowards(this.m_body.position, position2, Time.deltaTime));
		}
		if (this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("submerged"))
		{
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06001756 RID: 5974 RVA: 0x0009A850 File Offset: 0x00098A50
	private void OnHit()
	{
		if (UnityEngine.Random.value <= this.m_hitReactionChance)
		{
			if (this.m_left)
			{
				return;
			}
			this.m_reactionEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			this.m_zanimator.SetTrigger("shake");
			base.Invoke("Leave", (float)this.m_leaveDelay);
		}
	}

	// Token: 0x06001757 RID: 5975 RVA: 0x0009A8C0 File Offset: 0x00098AC0
	private void Leave()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_left)
		{
			return;
		}
		this.m_left = true;
		this.m_leaveEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		this.m_zanimator.SetTrigger("dive");
	}

	// Token: 0x040018AF RID: 6319
	public float m_waveScale = 0.5f;

	// Token: 0x040018B0 RID: 6320
	public float m_floatOffset;

	// Token: 0x040018B1 RID: 6321
	public float m_movementSpeed = 0.1f;

	// Token: 0x040018B2 RID: 6322
	public float m_maxSpeed = 1f;

	// Token: 0x040018B3 RID: 6323
	public MineRock m_mineRock;

	// Token: 0x040018B4 RID: 6324
	public float m_hitReactionChance = 0.25f;

	// Token: 0x040018B5 RID: 6325
	public int m_leaveDelay = 5;

	// Token: 0x040018B6 RID: 6326
	public EffectList m_reactionEffects = new EffectList();

	// Token: 0x040018B7 RID: 6327
	public EffectList m_leaveEffects = new EffectList();

	// Token: 0x040018B8 RID: 6328
	private Rigidbody m_body;

	// Token: 0x040018B9 RID: 6329
	private ZNetView m_nview;

	// Token: 0x040018BA RID: 6330
	private ZSyncAnimation m_zanimator;

	// Token: 0x040018BB RID: 6331
	private Animator m_animator;

	// Token: 0x040018BC RID: 6332
	private bool m_left;
}
