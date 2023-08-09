using System;
using UnityEngine;

// Token: 0x0200005F RID: 95
public class SE_Puke : SE_Stats
{
	// Token: 0x060004F5 RID: 1269 RVA: 0x00028617 File Offset: 0x00026817
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060004F6 RID: 1270 RVA: 0x00028620 File Offset: 0x00026820
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_removeTimer += dt;
		if (this.m_removeTimer > this.m_removeInterval)
		{
			this.m_removeTimer = 0f;
			if ((this.m_character as Player).RemoveOneFood())
			{
				Hud.instance.DamageFlash();
			}
		}
	}

	// Token: 0x040005D5 RID: 1493
	[Header("__SE_Puke__")]
	public float m_removeInterval = 1f;

	// Token: 0x040005D6 RID: 1494
	private float m_removeTimer;
}
