using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000050 RID: 80
public class AnimalAI : BaseAI
{
	// Token: 0x0600041E RID: 1054 RVA: 0x00021A4D File Offset: 0x0001FC4D
	protected override void Awake()
	{
		base.Awake();
		this.m_updateTargetTimer = UnityEngine.Random.Range(0f, 2f);
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x00021A6A File Offset: 0x0001FC6A
	protected override void OnEnable()
	{
		base.OnEnable();
		AnimalAI.Instances.Add(this);
	}

	// Token: 0x06000420 RID: 1056 RVA: 0x00021A7D File Offset: 0x0001FC7D
	protected override void OnDisable()
	{
		base.OnDisable();
		AnimalAI.Instances.Remove(this);
	}

	// Token: 0x06000421 RID: 1057 RVA: 0x00021A91 File Offset: 0x0001FC91
	protected override void OnDamaged(float damage, Character attacker)
	{
		base.OnDamaged(damage, attacker);
		this.SetAlerted(true);
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x00021AA4 File Offset: 0x0001FCA4
	public new void UpdateAI(float dt)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_afraidOfFire && base.AvoidFire(dt, null, true))
		{
			return;
		}
		this.m_updateTargetTimer -= dt;
		if (this.m_updateTargetTimer <= 0f)
		{
			this.m_updateTargetTimer = (Character.IsCharacterInRange(base.transform.position, 32f) ? 2f : 10f);
			Character character = base.FindEnemy();
			if (character)
			{
				this.m_target = character;
			}
		}
		if (this.m_target && this.m_target.IsDead())
		{
			this.m_target = null;
		}
		if (this.m_target)
		{
			bool flag = base.CanSenseTarget(this.m_target);
			base.SetTargetInfo(this.m_target.GetZDOID());
			if (flag)
			{
				this.SetAlerted(true);
			}
		}
		else
		{
			base.SetTargetInfo(ZDOID.None);
		}
		if (base.IsAlerted())
		{
			this.m_inDangerTimer += dt;
			if (this.m_inDangerTimer > this.m_timeToSafe)
			{
				this.m_target = null;
				this.SetAlerted(false);
			}
		}
		if (this.m_target)
		{
			base.Flee(dt, this.m_target.transform.position);
			this.m_target.OnTargeted(false, false);
			return;
		}
		base.IdleMovement(dt);
	}

	// Token: 0x06000423 RID: 1059 RVA: 0x00021BFA File Offset: 0x0001FDFA
	protected override void SetAlerted(bool alert)
	{
		if (alert)
		{
			this.m_inDangerTimer = 0f;
		}
		base.SetAlerted(alert);
	}

	// Token: 0x1700000B RID: 11
	// (get) Token: 0x06000424 RID: 1060 RVA: 0x00021C11 File Offset: 0x0001FE11
	public new static List<AnimalAI> Instances { get; } = new List<AnimalAI>();

	// Token: 0x040004CF RID: 1231
	private const float m_updateTargetFarRange = 32f;

	// Token: 0x040004D0 RID: 1232
	private const float m_updateTargetIntervalNear = 2f;

	// Token: 0x040004D1 RID: 1233
	private const float m_updateTargetIntervalFar = 10f;

	// Token: 0x040004D2 RID: 1234
	public float m_timeToSafe = 4f;

	// Token: 0x040004D3 RID: 1235
	private Character m_target;

	// Token: 0x040004D4 RID: 1236
	private float m_inDangerTimer;

	// Token: 0x040004D5 RID: 1237
	private float m_updateTargetTimer;
}
