using System;
using System.IO;
using UnityEngine;

// Token: 0x020001F7 RID: 503
public class ServerCtrl
{
	// Token: 0x170000DC RID: 220
	// (get) Token: 0x06001444 RID: 5188 RVA: 0x000843D1 File Offset: 0x000825D1
	public static ServerCtrl instance
	{
		get
		{
			return ServerCtrl.m_instance;
		}
	}

	// Token: 0x06001445 RID: 5189 RVA: 0x000843D8 File Offset: 0x000825D8
	public static void Initialize()
	{
		if (ServerCtrl.m_instance == null)
		{
			ServerCtrl.m_instance = new ServerCtrl();
		}
	}

	// Token: 0x06001446 RID: 5190 RVA: 0x000843EB File Offset: 0x000825EB
	private ServerCtrl()
	{
		this.ClearExitFile();
	}

	// Token: 0x06001447 RID: 5191 RVA: 0x000843F9 File Offset: 0x000825F9
	public void Update(float dt)
	{
		this.CheckExit(dt);
	}

	// Token: 0x06001448 RID: 5192 RVA: 0x00084402 File Offset: 0x00082602
	private void CheckExit(float dt)
	{
		this.m_checkTimer += dt;
		if (this.m_checkTimer > 2f)
		{
			this.m_checkTimer = 0f;
			if (File.Exists("server_exit.drp"))
			{
				Application.Quit();
			}
		}
	}

	// Token: 0x06001449 RID: 5193 RVA: 0x0008443C File Offset: 0x0008263C
	private void ClearExitFile()
	{
		try
		{
			File.Delete("server_exit.drp");
		}
		catch
		{
		}
	}

	// Token: 0x040014EC RID: 5356
	private static ServerCtrl m_instance;

	// Token: 0x040014ED RID: 5357
	private float m_checkTimer;
}
