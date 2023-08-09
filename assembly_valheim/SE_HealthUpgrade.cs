using System;
using UnityEngine;

// Token: 0x0200005D RID: 93
public class SE_HealthUpgrade : StatusEffect
{
	// Token: 0x060004EF RID: 1263 RVA: 0x000275AC File Offset: 0x000257AC
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060004F0 RID: 1264 RVA: 0x000283AC File Offset: 0x000265AC
	public override void Stop()
	{
		base.Stop();
		Player player = this.m_character as Player;
		if (!player)
		{
			return;
		}
		if (this.m_moreHealth > 0f)
		{
			player.SetMaxHealth(this.m_character.GetMaxHealth() + this.m_moreHealth, true);
			player.SetHealth(this.m_character.GetMaxHealth());
		}
		if (this.m_moreStamina > 0f)
		{
			player.SetMaxStamina(this.m_character.GetMaxStamina() + this.m_moreStamina, true);
		}
		this.m_upgradeEffect.Create(this.m_character.transform.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x040005CA RID: 1482
	[Header("Health")]
	public float m_moreHealth;

	// Token: 0x040005CB RID: 1483
	[Header("Stamina")]
	public float m_moreStamina;

	// Token: 0x040005CC RID: 1484
	public EffectList m_upgradeEffect = new EffectList();
}
