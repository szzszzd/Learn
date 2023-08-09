using System;
using System.Collections;
using System.Runtime.CompilerServices;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Party;
using UnityEngine;

// Token: 0x02000194 RID: 404
public class PlayFabManager : MonoBehaviour
{
	// Token: 0x170000A2 RID: 162
	// (get) Token: 0x06001051 RID: 4177 RVA: 0x0006BC49 File Offset: 0x00069E49
	public static bool IsLoggedIn
	{
		get
		{
			return !(PlayFabManager.instance == null) && PlayFabManager.instance.m_loginState == LoginState.LoggedIn;
		}
	}

	// Token: 0x170000A3 RID: 163
	// (get) Token: 0x06001052 RID: 4178 RVA: 0x0006BC67 File Offset: 0x00069E67
	public static LoginState CurrentLoginState
	{
		get
		{
			if (PlayFabManager.instance == null)
			{
				return LoginState.NotLoggedIn;
			}
			return PlayFabManager.instance.m_loginState;
		}
	}

	// Token: 0x170000A4 RID: 164
	// (get) Token: 0x06001053 RID: 4179 RVA: 0x0006BC82 File Offset: 0x00069E82
	// (set) Token: 0x06001054 RID: 4180 RVA: 0x0006BC89 File Offset: 0x00069E89
	public static DateTime NextRetryUtc { get; private set; } = DateTime.MinValue;

	// Token: 0x170000A5 RID: 165
	// (get) Token: 0x06001055 RID: 4181 RVA: 0x0006BC91 File Offset: 0x00069E91
	// (set) Token: 0x06001056 RID: 4182 RVA: 0x0006BC99 File Offset: 0x00069E99
	public EntityKey Entity { get; private set; }

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x06001057 RID: 4183 RVA: 0x0006BCA4 File Offset: 0x00069EA4
	// (remove) Token: 0x06001058 RID: 4184 RVA: 0x0006BCDC File Offset: 0x00069EDC
	public event LoginFinishedCallback LoginFinished;

	// Token: 0x170000A6 RID: 166
	// (get) Token: 0x06001059 RID: 4185 RVA: 0x0006BD11 File Offset: 0x00069F11
	// (set) Token: 0x0600105A RID: 4186 RVA: 0x0006BD18 File Offset: 0x00069F18
	public static PlayFabManager instance { get; private set; }

	// Token: 0x0600105B RID: 4187 RVA: 0x0006BD20 File Offset: 0x00069F20
	public static void SetCustomId(PrivilegeManager.Platform platform, string id)
	{
		PlayFabManager.m_customId = PrivilegeManager.GetPlatformPrefix(platform) + id;
		ZLog.Log(string.Format("PlayFab custom ID set to \"{0}\"", PlayFabManager.m_customId));
		if (PlayFabManager.instance != null && PlayFabManager.CurrentLoginState == LoginState.NotLoggedIn)
		{
			PlayFabManager.instance.Login();
		}
	}

	// Token: 0x0600105C RID: 4188 RVA: 0x0006BD70 File Offset: 0x00069F70
	public static void Initialize()
	{
		if (PlayFabManager.instance == null)
		{
			new GameObject("PlayFabManager").AddComponent<PlayFabManager>();
			new GameObject("PlayFabMultiplayerManager").AddComponent<PlayFabMultiplayerManager>();
		}
	}

	// Token: 0x0600105D RID: 4189 RVA: 0x0006BD9F File Offset: 0x00069F9F
	public void Start()
	{
		if (PlayFabManager.instance != null)
		{
			ZLog.LogError("Tried to create another PlayFabManager when one already exists! Ignoring and destroying the new one.");
			UnityEngine.Object.Destroy(this);
			return;
		}
		PlayFabManager.instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		this.Login();
	}

	// Token: 0x0600105E RID: 4190 RVA: 0x0006BDD8 File Offset: 0x00069FD8
	private void Login()
	{
		this.m_loginAttempts++;
		ZLog.Log(string.Format("Sending PlayFab login request (attempt {0})", this.m_loginAttempts));
		if (PlayFabManager.m_customId != null)
		{
			this.LoginWithCustomId();
			return;
		}
		ZLog.Log("Login postponed until ID has been set.");
	}

	// Token: 0x0600105F RID: 4191 RVA: 0x0006BE28 File Offset: 0x0006A028
	private void LoginWithCustomId()
	{
		if (this.m_loginState == LoginState.NotLoggedIn || this.m_loginState == LoginState.WaitingForRetry)
		{
			this.m_loginState = LoginState.AttemptingLogin;
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				CustomId = PlayFabManager.m_customId,
				CreateAccount = new bool?(true)
			}, new Action<LoginResult>(this.OnLoginSuccess), new Action<PlayFabError>(this.OnLoginFailure), null, null);
			return;
		}
		ZLog.LogError(string.Concat(new string[]
		{
			"Tried to log in while in the ",
			this.m_loginState.ToString(),
			" state! Can only log in when in the ",
			LoginState.NotLoggedIn.ToString(),
			" or ",
			LoginState.WaitingForRetry.ToString(),
			" state!"
		}));
	}

	// Token: 0x06001060 RID: 4192 RVA: 0x0006BEF4 File Offset: 0x0006A0F4
	public void OnLoginSuccess(LoginResult result)
	{
		if (PlayFabManager.<OnLoginSuccess>g__IsPlayFab|36_0(PlayFabManager.m_customId) && !PlayFabManager.IsLoggedIn)
		{
			PrivilegeData privilegeData = default(PrivilegeData);
			privilegeData.platformCanAccess = delegate(PrivilegeManager.Permission permission, PrivilegeManager.User targetSteamId, CanAccessResult canAccessCb)
			{
				canAccessCb(PrivilegeManager.Result.Allowed);
			};
			privilegeData.platformUserId = Convert.ToUInt64(result.EntityToken.Entity.Id, 16);
			privilegeData.canAccessOnlineMultiplayer = true;
			privilegeData.canViewUserGeneratedContentAll = true;
			privilegeData.canCrossplay = true;
			PrivilegeManager.SetPrivilegeData(privilegeData);
		}
		this.Entity = result.EntityToken.Entity;
		this.m_entityToken = result.EntityToken.EntityToken;
		this.m_tokenExpiration = result.EntityToken.TokenExpiration;
		if (this.m_tokenExpiration == null)
		{
			ZLog.LogError("Token expiration time was null!");
			this.m_loginState = LoginState.LoggedIn;
			return;
		}
		this.m_refreshThresh = (float)(this.m_tokenExpiration.Value - DateTime.UtcNow).TotalSeconds / 2f;
		if (PlayFabManager.IsLoggedIn)
		{
			ZLog.Log(string.Format("PlayFab local entity ID {0} lifetime extended ", this.Entity.Id));
			LoginFinishedCallback loginFinished = this.LoginFinished;
			if (loginFinished != null)
			{
				loginFinished(LoginType.Refresh);
			}
		}
		else
		{
			if (PlayFabManager.m_customId != null)
			{
				ZLog.Log(string.Format("PlayFab logged in as \"{0}\"", PlayFabManager.m_customId));
			}
			ZLog.Log("PlayFab local entity ID is " + this.Entity.Id);
			this.m_loginState = LoginState.LoggedIn;
			LoginFinishedCallback loginFinished2 = this.LoginFinished;
			if (loginFinished2 != null)
			{
				loginFinished2(LoginType.Success);
			}
		}
		if (this.m_updateEntityTokenCoroutine == null)
		{
			this.m_updateEntityTokenCoroutine = base.StartCoroutine(this.UpdateEntityTokenCoroutine());
		}
		ZPlayFabMatchmaking.OnLogin();
	}

	// Token: 0x06001061 RID: 4193 RVA: 0x0006C09C File Offset: 0x0006A29C
	public void OnLoginFailure(PlayFabError error)
	{
		ZLog.LogError(error.GenerateErrorReport());
		this.RetryLoginAfterDelay(this.GetRetryDelay(this.m_loginAttempts));
	}

	// Token: 0x06001062 RID: 4194 RVA: 0x0006C0BB File Offset: 0x0006A2BB
	private float GetRetryDelay(int attemptCount)
	{
		return Mathf.Min(1f * Mathf.Pow(2f, (float)(attemptCount - 1)), 30f) * UnityEngine.Random.Range(0.875f, 1.125f);
	}

	// Token: 0x06001063 RID: 4195 RVA: 0x0006C0EB File Offset: 0x0006A2EB
	private void RetryLoginAfterDelay(float delay)
	{
		this.m_loginState = LoginState.WaitingForRetry;
		ZLog.Log(string.Format("Retrying login in {0}s", delay));
		base.StartCoroutine(this.<RetryLoginAfterDelay>g__DelayThenLoginCoroutine|39_0(delay));
	}

	// Token: 0x06001064 RID: 4196 RVA: 0x0006C117 File Offset: 0x0006A317
	private IEnumerator UpdateEntityTokenCoroutine()
	{
		for (;;)
		{
			yield return new WaitForSecondsRealtime(420f);
			ZLog.Log("Update PlayFab entity token");
			PlayFabMultiplayerManager.Get().UpdateEntityToken(this.m_entityToken);
			if (this.m_tokenExpiration == null)
			{
				break;
			}
			if ((float)(this.m_tokenExpiration.Value - DateTime.UtcNow).TotalSeconds <= this.m_refreshThresh)
			{
				ZLog.Log("Renew PlayFab entity token");
				this.m_refreshThresh /= 1.5f;
				if (PlayFabManager.m_customId != null)
				{
					PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
					{
						CustomId = PlayFabManager.m_customId
					}, new Action<LoginResult>(this.OnLoginSuccess), new Action<PlayFabError>(this.OnLoginFailure), null, null);
				}
			}
			yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(420f, 840f));
		}
		ZLog.LogError("Token expiration time was null!");
		this.m_updateEntityTokenCoroutine = null;
		yield break;
		yield break;
	}

	// Token: 0x06001065 RID: 4197 RVA: 0x0006C126 File Offset: 0x0006A326
	public void LoginFailed()
	{
		this.RetryLoginAfterDelay(this.GetRetryDelay(this.m_loginAttempts));
	}

	// Token: 0x06001066 RID: 4198 RVA: 0x0006C13A File Offset: 0x0006A33A
	private void Update()
	{
		ZPlayFabMatchmaking instance = ZPlayFabMatchmaking.instance;
		if (instance == null)
		{
			return;
		}
		instance.Update(Time.unscaledDeltaTime);
	}

	// Token: 0x06001069 RID: 4201 RVA: 0x0006C15C File Offset: 0x0006A35C
	[CompilerGenerated]
	internal static bool <OnLoginSuccess>g__IsPlayFab|36_0(string id)
	{
		return PlayFabManager.m_customId != null && id.StartsWith(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.PlayFab));
	}

	// Token: 0x0600106A RID: 4202 RVA: 0x0006C173 File Offset: 0x0006A373
	[CompilerGenerated]
	private IEnumerator <RetryLoginAfterDelay>g__DelayThenLoginCoroutine|39_0(float delay)
	{
		ZLog.Log(string.Format("PlayFab login failed! Retrying in {0}s, total attempts: {1}", delay, this.m_loginAttempts));
		PlayFabManager.NextRetryUtc = DateTime.UtcNow + TimeSpan.FromSeconds((double)delay);
		while (DateTime.UtcNow < PlayFabManager.NextRetryUtc)
		{
			yield return null;
		}
		this.Login();
		yield break;
	}

	// Token: 0x04001144 RID: 4420
	public const string TitleId = "6E223";

	// Token: 0x04001145 RID: 4421
	private LoginState m_loginState;

	// Token: 0x04001146 RID: 4422
	private string m_entityToken;

	// Token: 0x04001147 RID: 4423
	private DateTime? m_tokenExpiration;

	// Token: 0x04001148 RID: 4424
	private float m_refreshThresh;

	// Token: 0x04001149 RID: 4425
	private int m_loginAttempts;

	// Token: 0x0400114A RID: 4426
	private const float EntityTokenUpdateDurationMin = 420f;

	// Token: 0x0400114B RID: 4427
	private const float EntityTokenUpdateDurationMax = 840f;

	// Token: 0x0400114C RID: 4428
	private const float LoginRetryDelay = 1f;

	// Token: 0x0400114D RID: 4429
	private const float LoginRetryDelayMax = 30f;

	// Token: 0x0400114E RID: 4430
	private const float LoginRetryJitterFactor = 0.125f;

	// Token: 0x04001152 RID: 4434
	private static string m_customId;

	// Token: 0x04001154 RID: 4436
	private Coroutine m_updateEntityTokenCoroutine;
}
