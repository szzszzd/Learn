using System;
using System.IO;
using System.Linq;
using System.Text;
using Steamworks;
using UnityEngine;

// Token: 0x020001B1 RID: 433
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	// Token: 0x170000B5 RID: 181
	// (get) Token: 0x06001169 RID: 4457 RVA: 0x000701FA File Offset: 0x0006E3FA
	public static SteamManager instance
	{
		get
		{
			return SteamManager.s_instance;
		}
	}

	// Token: 0x0600116A RID: 4458 RVA: 0x00070201 File Offset: 0x0006E401
	public static bool Initialize()
	{
		if (SteamManager.s_instance == null)
		{
			new GameObject("SteamManager").AddComponent<SteamManager>();
		}
		return SteamManager.Initialized;
	}

	// Token: 0x170000B6 RID: 182
	// (get) Token: 0x0600116B RID: 4459 RVA: 0x00070225 File Offset: 0x0006E425
	public static bool Initialized
	{
		get
		{
			return SteamManager.s_instance != null && SteamManager.s_instance.m_bInitialized;
		}
	}

	// Token: 0x0600116C RID: 4460 RVA: 0x00070240 File Offset: 0x0006E440
	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	// Token: 0x0600116D RID: 4461 RVA: 0x00070248 File Offset: 0x0006E448
	public static void SetServerPort(int port)
	{
		SteamManager.m_serverPort = port;
	}

	// Token: 0x0600116E RID: 4462 RVA: 0x00070250 File Offset: 0x0006E450
	private uint LoadAPPID()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SteamAppId");
		if (environmentVariable != null)
		{
			ZLog.Log("Using environment steamid " + environmentVariable);
			return uint.Parse(environmentVariable);
		}
		try
		{
			string s = File.ReadAllText("steam_appid.txt");
			ZLog.Log("Using steam_appid.txt");
			return uint.Parse(s);
		}
		catch
		{
		}
		ZLog.LogWarning("Failed to find APPID");
		return 0U;
	}

	// Token: 0x0600116F RID: 4463 RVA: 0x000702C0 File Offset: 0x0006E4C0
	private void Awake()
	{
		if (SteamManager.s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		SteamManager.s_instance = this;
		SteamManager.APP_ID = this.LoadAPPID();
		ZLog.Log("Using steam APPID:" + SteamManager.APP_ID.ToString());
		if (!SteamManager.ACCEPTED_APPIDs.Contains(SteamManager.APP_ID))
		{
			ZLog.Log("Invalid APPID");
			Application.Quit();
			return;
		}
		if (SteamManager.s_EverInialized)
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
			if (SteamAPI.RestartAppIfNecessary((AppId_t)SteamManager.APP_ID))
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
		ZLog.Log("Authentication:" + SteamNetworkingSockets.InitAuthentication().ToString());
		SteamManager.s_EverInialized = true;
	}

	// Token: 0x06001170 RID: 4464 RVA: 0x0007040C File Offset: 0x0006E60C
	private void OnEnable()
	{
		if (SteamManager.s_instance == null)
		{
			SteamManager.s_instance = this;
		}
		if (!this.m_bInitialized)
		{
			return;
		}
		if (this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	// Token: 0x06001171 RID: 4465 RVA: 0x0007045A File Offset: 0x0006E65A
	private void OnDestroy()
	{
		ZLog.Log("Steam manager on destroy");
		if (SteamManager.s_instance != this)
		{
			return;
		}
		SteamManager.s_instance = null;
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.Shutdown();
	}

	// Token: 0x06001172 RID: 4466 RVA: 0x00070488 File Offset: 0x0006E688
	private void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
	}

	// Token: 0x04001207 RID: 4615
	public static uint[] ACCEPTED_APPIDs = new uint[]
	{
		1223920U,
		892970U
	};

	// Token: 0x04001208 RID: 4616
	public static uint APP_ID = 0U;

	// Token: 0x04001209 RID: 4617
	private static int m_serverPort = 2456;

	// Token: 0x0400120A RID: 4618
	private static SteamManager s_instance;

	// Token: 0x0400120B RID: 4619
	private static bool s_EverInialized;

	// Token: 0x0400120C RID: 4620
	private bool m_bInitialized;

	// Token: 0x0400120D RID: 4621
	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
