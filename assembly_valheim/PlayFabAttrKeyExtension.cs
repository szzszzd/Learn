using System;

// Token: 0x020001A4 RID: 420
public static class PlayFabAttrKeyExtension
{
	// Token: 0x060010B7 RID: 4279 RVA: 0x0006D3E4 File Offset: 0x0006B5E4
	public static string ToKeyString(this PlayFabAttrKey key)
	{
		switch (key)
		{
		case PlayFabAttrKey.WorldName:
			return "WORLD";
		case PlayFabAttrKey.NetworkId:
			return "NETWORK";
		case PlayFabAttrKey.HavePassword:
			return "PASSWORD";
		default:
			return null;
		}
	}
}
