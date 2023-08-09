using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

// Token: 0x020001B9 RID: 441
public class DLCMan : MonoBehaviour
{
	// Token: 0x170000BA RID: 186
	// (get) Token: 0x060011A5 RID: 4517 RVA: 0x000746D4 File Offset: 0x000728D4
	public static DLCMan instance
	{
		get
		{
			return DLCMan.m_instance;
		}
	}

	// Token: 0x060011A6 RID: 4518 RVA: 0x000746DB File Offset: 0x000728DB
	private void Awake()
	{
		DLCMan.m_instance = this;
		this.CheckDLCsSTEAM();
	}

	// Token: 0x060011A7 RID: 4519 RVA: 0x000746E9 File Offset: 0x000728E9
	private void OnDestroy()
	{
		if (DLCMan.m_instance == this)
		{
			DLCMan.m_instance = null;
		}
	}

	// Token: 0x060011A8 RID: 4520 RVA: 0x00074700 File Offset: 0x00072900
	public bool IsDLCInstalled(string name)
	{
		if (name.Length == 0)
		{
			return true;
		}
		foreach (DLCMan.DLCInfo dlcinfo in this.m_dlcs)
		{
			if (dlcinfo.m_name == name)
			{
				return dlcinfo.m_installed;
			}
		}
		ZLog.LogWarning("DLC " + name + " not registered in DLCMan");
		return false;
	}

	// Token: 0x060011A9 RID: 4521 RVA: 0x00074788 File Offset: 0x00072988
	private void CheckDLCsSTEAM()
	{
		if (!SteamManager.Initialized)
		{
			ZLog.Log("Steam not initialized");
			return;
		}
		ZLog.Log("Checking for installed DLCs");
		foreach (DLCMan.DLCInfo dlcinfo in this.m_dlcs)
		{
			dlcinfo.m_installed = this.IsDLCInstalled(dlcinfo);
			ZLog.Log("DLC:" + dlcinfo.m_name + " installed:" + dlcinfo.m_installed.ToString());
		}
	}

	// Token: 0x060011AA RID: 4522 RVA: 0x00074824 File Offset: 0x00072A24
	private bool IsDLCInstalled(DLCMan.DLCInfo dlc)
	{
		foreach (uint id in dlc.m_steamAPPID)
		{
			if (this.IsDLCInstalled(id))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060011AB RID: 4523 RVA: 0x00074858 File Offset: 0x00072A58
	private bool IsDLCInstalled(uint id)
	{
		AppId_t x = new AppId_t(id);
		int dlccount = SteamApps.GetDLCCount();
		for (int i = 0; i < dlccount; i++)
		{
			AppId_t appId_t;
			bool flag;
			string text;
			if (SteamApps.BGetDLCDataByIndex(i, out appId_t, out flag, out text, 200) && x == appId_t)
			{
				ZLog.Log("DLC installed:" + id.ToString());
				return SteamApps.BIsDlcInstalled(appId_t);
			}
		}
		return false;
	}

	// Token: 0x04001259 RID: 4697
	private static DLCMan m_instance;

	// Token: 0x0400125A RID: 4698
	public List<DLCMan.DLCInfo> m_dlcs = new List<DLCMan.DLCInfo>();

	// Token: 0x020001BA RID: 442
	[Serializable]
	public class DLCInfo
	{
		// Token: 0x0400125B RID: 4699
		public string m_name = "DLC";

		// Token: 0x0400125C RID: 4700
		public uint[] m_steamAPPID = new uint[0];

		// Token: 0x0400125D RID: 4701
		[NonSerialized]
		public bool m_installed;
	}
}
