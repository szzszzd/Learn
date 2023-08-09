using System;
using UnityEngine;

// Token: 0x0200005C RID: 92
public class SE_Harpooned : StatusEffect
{
	// Token: 0x060004EA RID: 1258 RVA: 0x000275AC File Offset: 0x000257AC
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060004EB RID: 1259 RVA: 0x00027FF8 File Offset: 0x000261F8
	public override void SetAttacker(Character attacker)
	{
		ZLog.Log("Setting attacker " + attacker.m_name);
		this.m_attacker = attacker;
		this.m_time = 0f;
		if (this.m_character.IsBoss())
		{
			this.m_broken = true;
			return;
		}
		float num = Vector3.Distance(this.m_attacker.transform.position, this.m_character.transform.position);
		if (num > this.m_maxDistance)
		{
			this.m_attacker.Message(MessageHud.MessageType.Center, "$msg_harpoon_targettoofar", 0, null);
			this.m_broken = true;
			return;
		}
		this.m_baseDistance = num;
		this.m_attacker.Message(MessageHud.MessageType.Center, this.m_character.m_name + " $msg_harpoon_harpooned", 0, null);
		foreach (GameObject gameObject in this.m_startEffectInstances)
		{
			if (gameObject)
			{
				LineConnect component = gameObject.GetComponent<LineConnect>();
				if (component)
				{
					component.SetPeer(this.m_attacker.GetComponent<ZNetView>());
					this.m_line = component;
				}
			}
		}
	}

	// Token: 0x060004EC RID: 1260 RVA: 0x00028104 File Offset: 0x00026304
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (!this.m_attacker)
		{
			return;
		}
		Rigidbody component = this.m_character.GetComponent<Rigidbody>();
		if (component)
		{
			float num = Vector3.Distance(this.m_attacker.transform.position, this.m_character.transform.position);
			if (this.m_character.GetStandingOnShip() == null && !this.m_character.IsAttached())
			{
				float num2 = Utils.Pull(component, this.m_attacker.transform.position, this.m_baseDistance, this.m_pullSpeed, this.m_pullForce, this.m_smoothDistance, true, true, this.m_forcePower);
				this.m_drainStaminaTimer += dt;
				if (this.m_drainStaminaTimer > this.m_staminaDrainInterval && num2 > 0f)
				{
					this.m_drainStaminaTimer = 0f;
					float stamina = this.m_staminaDrain * num2 * this.m_character.GetMass();
					this.m_attacker.UseStamina(stamina);
				}
			}
			if (this.m_line)
			{
				this.m_line.SetSlack((1f - Utils.LerpStep(this.m_baseDistance / 2f, this.m_baseDistance, num)) * this.m_maxLineSlack);
			}
			if (num - this.m_baseDistance > this.m_breakDistance)
			{
				this.m_broken = true;
				this.m_attacker.Message(MessageHud.MessageType.Center, "$msg_harpoon_linebroke", 0, null);
			}
			if (!this.m_attacker.HaveStamina(0f))
			{
				this.m_broken = true;
				this.m_attacker.Message(MessageHud.MessageType.Center, this.m_character.m_name + " $msg_harpoon_released", 0, null);
			}
		}
	}

	// Token: 0x060004ED RID: 1261 RVA: 0x000282B4 File Offset: 0x000264B4
	public override bool IsDone()
	{
		if (base.IsDone())
		{
			return true;
		}
		if (this.m_broken)
		{
			return true;
		}
		if (!this.m_attacker)
		{
			return true;
		}
		if (this.m_time > 2f && (this.m_attacker.IsBlocking() || this.m_attacker.InAttack()))
		{
			this.m_attacker.Message(MessageHud.MessageType.Center, this.m_character.m_name + " released", 0, null);
			return true;
		}
		return false;
	}

	// Token: 0x040005BC RID: 1468
	[Header("SE_Harpooned")]
	public float m_pullForce;

	// Token: 0x040005BD RID: 1469
	public float m_forcePower = 2f;

	// Token: 0x040005BE RID: 1470
	public float m_pullSpeed = 5f;

	// Token: 0x040005BF RID: 1471
	public float m_smoothDistance = 2f;

	// Token: 0x040005C0 RID: 1472
	public float m_maxLineSlack = 0.3f;

	// Token: 0x040005C1 RID: 1473
	public float m_breakDistance = 4f;

	// Token: 0x040005C2 RID: 1474
	public float m_maxDistance = 30f;

	// Token: 0x040005C3 RID: 1475
	public float m_staminaDrain = 10f;

	// Token: 0x040005C4 RID: 1476
	public float m_staminaDrainInterval = 0.1f;

	// Token: 0x040005C5 RID: 1477
	private bool m_broken;

	// Token: 0x040005C6 RID: 1478
	private Character m_attacker;

	// Token: 0x040005C7 RID: 1479
	private float m_baseDistance = 999999f;

	// Token: 0x040005C8 RID: 1480
	private LineConnect m_line;

	// Token: 0x040005C9 RID: 1481
	private float m_drainStaminaTimer;
}
