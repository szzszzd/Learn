using System;
using UnityEngine;

// Token: 0x02000200 RID: 512
public class StaticPhysics : SlowUpdate
{
	// Token: 0x0600147A RID: 5242 RVA: 0x00085992 File Offset: 0x00083B92
	public override void Awake()
	{
		base.Awake();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_createTime = Time.time;
	}

	// Token: 0x0600147B RID: 5243 RVA: 0x000859B1 File Offset: 0x00083BB1
	private bool ShouldUpdate()
	{
		return Time.time - this.m_createTime > 20f;
	}

	// Token: 0x0600147C RID: 5244 RVA: 0x000859C8 File Offset: 0x00083BC8
	public override void SUpdate()
	{
		if (!this.ShouldUpdate() || ZNetScene.instance.OutsideActiveArea(base.transform.position) || this.m_falling)
		{
			return;
		}
		if (this.m_fall)
		{
			this.CheckFall();
		}
		if (this.m_pushUp)
		{
			this.PushUp();
		}
	}

	// Token: 0x0600147D RID: 5245 RVA: 0x00085A1C File Offset: 0x00083C1C
	private void CheckFall()
	{
		float fallHeight = this.GetFallHeight();
		if (base.transform.position.y > fallHeight + 0.05f)
		{
			this.Fall();
		}
	}

	// Token: 0x0600147E RID: 5246 RVA: 0x00085A50 File Offset: 0x00083C50
	private float GetFallHeight()
	{
		if (this.m_checkSolids)
		{
			float result;
			if (ZoneSystem.instance.GetSolidHeight(base.transform.position, this.m_fallCheckRadius, out result, base.transform))
			{
				return result;
			}
			return base.transform.position.y;
		}
		else
		{
			float result2;
			if (ZoneSystem.instance.GetGroundHeight(base.transform.position, out result2))
			{
				return result2;
			}
			return base.transform.position.y;
		}
	}

	// Token: 0x0600147F RID: 5247 RVA: 0x00085AC8 File Offset: 0x00083CC8
	private void Fall()
	{
		this.m_falling = true;
		base.gameObject.isStatic = false;
		base.InvokeRepeating("FallUpdate", 0.05f, 0.05f);
	}

	// Token: 0x06001480 RID: 5248 RVA: 0x00085AF4 File Offset: 0x00083CF4
	private void FallUpdate()
	{
		float fallHeight = this.GetFallHeight();
		Vector3 position = base.transform.position;
		position.y -= 0.2f;
		if (position.y <= fallHeight)
		{
			position.y = fallHeight;
			this.StopFalling();
		}
		base.transform.position = position;
		if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().SetPosition(base.transform.position);
		}
	}

	// Token: 0x06001481 RID: 5249 RVA: 0x00085B89 File Offset: 0x00083D89
	private void StopFalling()
	{
		base.gameObject.isStatic = true;
		this.m_falling = false;
		base.CancelInvoke("FallUpdate");
	}

	// Token: 0x06001482 RID: 5250 RVA: 0x00085BAC File Offset: 0x00083DAC
	private void PushUp()
	{
		float num;
		if (ZoneSystem.instance.GetGroundHeight(base.transform.position, out num) && base.transform.position.y < num - 0.05f)
		{
			base.gameObject.isStatic = false;
			Vector3 position = base.transform.position;
			position.y = num;
			base.transform.position = position;
			base.gameObject.isStatic = true;
			if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
			{
				this.m_nview.GetZDO().SetPosition(base.transform.position);
			}
		}
	}

	// Token: 0x04001534 RID: 5428
	public bool m_pushUp = true;

	// Token: 0x04001535 RID: 5429
	public bool m_fall = true;

	// Token: 0x04001536 RID: 5430
	public bool m_checkSolids;

	// Token: 0x04001537 RID: 5431
	public float m_fallCheckRadius;

	// Token: 0x04001538 RID: 5432
	private ZNetView m_nview;

	// Token: 0x04001539 RID: 5433
	private const float m_fallSpeed = 4f;

	// Token: 0x0400153A RID: 5434
	private const float m_fallStep = 0.05f;

	// Token: 0x0400153B RID: 5435
	private float m_createTime;

	// Token: 0x0400153C RID: 5436
	private bool m_falling;
}
