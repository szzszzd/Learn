using System;

// Token: 0x02000140 RID: 320
public static class OnlineBackendTypeExtentions
{
	// Token: 0x06000C2F RID: 3119 RVA: 0x000589CC File Offset: 0x00056BCC
	public static string ConvertToString(this OnlineBackendType backend)
	{
		switch (backend)
		{
		case OnlineBackendType.Steamworks:
			return "steamworks";
		case OnlineBackendType.PlayFab:
			return "playfab";
		case OnlineBackendType.EOS:
			return "eos";
		case OnlineBackendType.CustomSocket:
			return "socket";
		}
		return "none";
	}

	// Token: 0x06000C30 RID: 3120 RVA: 0x00058A08 File Offset: 0x00056C08
	public static OnlineBackendType ConvertFromString(string backend)
	{
		if (backend != null)
		{
			if (backend == "steamworks")
			{
				return OnlineBackendType.Steamworks;
			}
			if (backend == "eos")
			{
				return OnlineBackendType.EOS;
			}
			if (backend == "playfab")
			{
				return OnlineBackendType.PlayFab;
			}
			if (backend == "socket")
			{
				return OnlineBackendType.CustomSocket;
			}
			if (!(backend == "none"))
			{
			}
		}
		return OnlineBackendType.None;
	}
}
