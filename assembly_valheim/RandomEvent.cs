using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001DF RID: 479
[Serializable]
public class RandomEvent
{
	// Token: 0x060013AF RID: 5039 RVA: 0x00081B48 File Offset: 0x0007FD48
	public RandomEvent Clone()
	{
		RandomEvent randomEvent = base.MemberwiseClone() as RandomEvent;
		randomEvent.m_spawn = new List<SpawnSystem.SpawnData>();
		foreach (SpawnSystem.SpawnData spawnData in this.m_spawn)
		{
			randomEvent.m_spawn.Add(spawnData.Clone());
		}
		return randomEvent;
	}

	// Token: 0x060013B0 RID: 5040 RVA: 0x00081BC0 File Offset: 0x0007FDC0
	public bool Update(bool server, bool active, bool playerInArea, float dt)
	{
		if (this.m_pauseIfNoPlayerInArea && !playerInArea)
		{
			return false;
		}
		this.m_time += dt;
		return this.m_duration > 0f && this.m_time > this.m_duration;
	}

	// Token: 0x060013B1 RID: 5041 RVA: 0x00081BFC File Offset: 0x0007FDFC
	public void OnActivate()
	{
		this.m_active = true;
		if (this.m_firstActivation)
		{
			this.m_firstActivation = false;
			if (this.m_startMessage != "")
			{
				MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, this.m_startMessage, 0, null);
			}
		}
	}

	// Token: 0x060013B2 RID: 5042 RVA: 0x00081C39 File Offset: 0x0007FE39
	public void OnDeactivate(bool end)
	{
		this.m_active = false;
		if (end && this.m_endMessage != "")
		{
			MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, this.m_endMessage, 0, null);
		}
	}

	// Token: 0x060013B3 RID: 5043 RVA: 0x00081C6A File Offset: 0x0007FE6A
	public string GetHudText()
	{
		return this.m_startMessage;
	}

	// Token: 0x060013B4 RID: 5044 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnStart()
	{
	}

	// Token: 0x060013B5 RID: 5045 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnStop()
	{
	}

	// Token: 0x060013B6 RID: 5046 RVA: 0x00081C72 File Offset: 0x0007FE72
	public bool InEventBiome()
	{
		return (EnvMan.instance.GetCurrentBiome() & this.m_biome) > Heightmap.Biome.None;
	}

	// Token: 0x060013B7 RID: 5047 RVA: 0x00081C88 File Offset: 0x0007FE88
	public float GetTime()
	{
		return this.m_time;
	}

	// Token: 0x04001486 RID: 5254
	public string m_name = "";

	// Token: 0x04001487 RID: 5255
	public bool m_enabled = true;

	// Token: 0x04001488 RID: 5256
	public bool m_random = true;

	// Token: 0x04001489 RID: 5257
	public float m_duration = 60f;

	// Token: 0x0400148A RID: 5258
	public bool m_nearBaseOnly = true;

	// Token: 0x0400148B RID: 5259
	public bool m_pauseIfNoPlayerInArea = true;

	// Token: 0x0400148C RID: 5260
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	// Token: 0x0400148D RID: 5261
	[Header("( Keys required to be TRUE )")]
	public List<string> m_requiredGlobalKeys = new List<string>();

	// Token: 0x0400148E RID: 5262
	[Header("( Keys required to be FALSE )")]
	public List<string> m_notRequiredGlobalKeys = new List<string>();

	// Token: 0x0400148F RID: 5263
	[Space(20f)]
	public string m_startMessage = "";

	// Token: 0x04001490 RID: 5264
	public string m_endMessage = "";

	// Token: 0x04001491 RID: 5265
	public string m_forceMusic = "";

	// Token: 0x04001492 RID: 5266
	public string m_forceEnvironment = "";

	// Token: 0x04001493 RID: 5267
	public List<SpawnSystem.SpawnData> m_spawn = new List<SpawnSystem.SpawnData>();

	// Token: 0x04001494 RID: 5268
	private bool m_firstActivation = true;

	// Token: 0x04001495 RID: 5269
	private bool m_active;

	// Token: 0x04001496 RID: 5270
	[NonSerialized]
	public float m_time;

	// Token: 0x04001497 RID: 5271
	[NonSerialized]
	public Vector3 m_pos = Vector3.zero;
}
