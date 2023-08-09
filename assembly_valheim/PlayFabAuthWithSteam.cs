using System;
using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using Steamworks;

// Token: 0x02000190 RID: 400
public static class PlayFabAuthWithSteam
{
	// Token: 0x06001047 RID: 4167 RVA: 0x0006BAD0 File Offset: 0x00069CD0
	private static void OnEncryptedAppTicketResponse(EncryptedAppTicketResponse_t param, bool bIOFailure)
	{
		if (bIOFailure)
		{
			ZLog.LogError("OnEncryptedAppTicketResponse: Failed to get Steam encrypted app ticket - IO Failure");
			return;
		}
		if (param.m_eResult != EResult.k_EResultOK && param.m_eResult != EResult.k_EResultLimitExceeded && param.m_eResult != EResult.k_EResultDuplicateRequest)
		{
			ZLog.LogError("OnEncryptedAppTicketResponse: Failed to get Steam encrypted app ticket - " + param.m_eResult.ToString());
			return;
		}
		PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest
		{
			CreateAccount = new bool?(true),
			SteamTicket = PlayFabAuthWithSteam.GetSteamAuthTicket()
		}, new Action<LoginResult>(PlayFabAuthWithSteam.OnSteamLoginSuccess), new Action<PlayFabError>(PlayFabAuthWithSteam.OnSteamLoginFailed), null, null);
	}

	// Token: 0x06001048 RID: 4168 RVA: 0x0006BB68 File Offset: 0x00069D68
	public static string GetSteamAuthTicket()
	{
		byte[] array = new byte[1024];
		uint num;
		HAuthTicket authSessionTicket = SteamUser.GetAuthSessionTicket(array, array.Length, out num);
		ZLog.Log(string.Format("PlayFab Steam auth using ticket {0} of length {1}", authSessionTicket, num));
		Array.Resize<byte>(ref array, (int)num);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in array)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	// Token: 0x06001049 RID: 4169 RVA: 0x0006BBEB File Offset: 0x00069DEB
	private static void OnSteamLoginFailed(PlayFabError error)
	{
		ZLog.LogError("Failed to logged in PlayFab user via Steam encrypted app ticket: " + error.GenerateErrorReport());
	}

	// Token: 0x0600104A RID: 4170 RVA: 0x0006BC02 File Offset: 0x00069E02
	private static void OnSteamLoginSuccess(LoginResult result)
	{
		ZLog.Log("Logged in PlayFab user via Steam encrypted app ticket");
	}

	// Token: 0x0600104B RID: 4171 RVA: 0x0006BC10 File Offset: 0x00069E10
	public static void Login()
	{
		SteamAPICall_t hAPICall = SteamUser.RequestEncryptedAppTicket(null, 0);
		PlayFabAuthWithSteam.OnEncryptedAppTicketResponseCallResult.Set(hAPICall, null);
	}

	// Token: 0x0400113A RID: 4410
	private static CallResult<EncryptedAppTicketResponse_t> OnEncryptedAppTicketResponseCallResult = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(PlayFabAuthWithSteam.OnEncryptedAppTicketResponse));
}
