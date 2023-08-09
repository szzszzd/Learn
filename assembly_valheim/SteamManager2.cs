using System;
using System.Text;
using Steamworks;
using UnityEngine;

// Token: 0x020001B2 RID: 434
[DisallowMultipleComponent]
public class SteamManager2 : MonoBehaviour
{
	// Token: 0x170000B7 RID: 183
	// (get) Token: 0x06001175 RID: 4469 RVA: 0x000704C5 File Offset: 0x0006E6C5
	protected static SteamManager2 Instance
	{
		get
		{
			if (SteamManager2.s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager2>();
			}
			return SteamManager2.s_instance;
		}
	}

	// Token: 0x170000B8 RID: 184
	// (get) Token: 0x06001176 RID: 4470 RVA: 0x000704E9 File Offset: 0x0006E6E9
	public static bool Initialized
	{
		get
		{
			return SteamManager2.Instance.m_bInitialized;
		}
	}

	// Token: 0x06001177 RID: 4471 RVA: 0x00070240 File Offset: 0x0006E440
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	// Token: 0x06001178 RID: 4472 RVA: 0x000704F8 File Offset: 0x0006E6F8
	protected virtual void Awake()
	{
		if (SteamManager2.s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		SteamManager2.s_instance = this;
		if (SteamManager2.s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			string str = "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n";
			DllNotFoundException ex2 = ex;
			Debug.LogError(str + ((ex2 != null) ? ex2.ToString() : null), this);
			Application.Quit();
			return;
		}
		this.m_bInitialized = SteamAPI.Init();
		if (!this.m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		SteamManager2.s_EverInitialized = true;
	}

	// Token: 0x06001179 RID: 4473 RVA: 0x000705D8 File Offset: 0x0006E7D8
	protected virtual void OnEnable()
	{
		if (SteamManager2.s_instance == null)
		{
			SteamManager2.s_instance = this;
		}
		if (!this.m_bInitialized)
		{
			return;
		}
		if (this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager2.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	// Token: 0x0600117A RID: 4474 RVA: 0x00070626 File Offset: 0x0006E826
	protected virtual void OnDestroy()
	{
		if (SteamManager2.s_instance != this)
		{
			return;
		}
		SteamManager2.s_instance = null;
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.Shutdown();
	}

	// Token: 0x0600117B RID: 4475 RVA: 0x0007064A File Offset: 0x0006E84A
	protected virtual void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
	}

	// Token: 0x0400120E RID: 4622
	protected static bool s_EverInitialized;

	// Token: 0x0400120F RID: 4623
	protected static SteamManager2 s_instance;

	// Token: 0x04001210 RID: 4624
	protected bool m_bInitialized;

	// Token: 0x04001211 RID: 4625
	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
