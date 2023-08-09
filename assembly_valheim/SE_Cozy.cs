using System;
using UnityEngine;

// Token: 0x02000058 RID: 88
public class SE_Cozy : SE_Stats
{
	// Token: 0x060004D7 RID: 1239 RVA: 0x00027779 File Offset: 0x00025979
	private void OnEnable()
	{
		if (!string.IsNullOrEmpty(this.m_statusEffect))
		{
			this.m_statusEffectHash = this.m_statusEffect.GetStableHashCode();
		}
	}

	// Token: 0x060004D8 RID: 1240 RVA: 0x00027799 File Offset: 0x00025999
	public override void Setup(Character character)
	{
		base.Setup(character);
		this.m_character.Message(MessageHud.MessageType.Center, "$se_resting_start", 0, null);
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x000277B5 File Offset: 0x000259B5
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_time > this.m_delay)
		{
			this.m_character.GetSEMan().AddStatusEffect(this.m_statusEffectHash, true, 0, 0f);
		}
	}

	// Token: 0x060004DA RID: 1242 RVA: 0x000277EC File Offset: 0x000259EC
	public override string GetIconText()
	{
		Player player = this.m_character as Player;
		return Localization.instance.Localize("$se_rested_comfort:" + player.GetComfortLevel().ToString());
	}

	// Token: 0x04000599 RID: 1433
	[Header("__SE_Cozy__")]
	public float m_delay = 10f;

	// Token: 0x0400059A RID: 1434
	public string m_statusEffect = "";

	// Token: 0x0400059B RID: 1435
	private int m_statusEffectHash;

	// Token: 0x0400059C RID: 1436
	private int m_comfortLevel;

	// Token: 0x0400059D RID: 1437
	private float m_updateTimer;
}
