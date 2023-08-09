using System;
using System.Collections.Generic;

// Token: 0x02000139 RID: 313
public class PrivilegeManager
{
	// Token: 0x1700006E RID: 110
	// (get) Token: 0x06000C18 RID: 3096 RVA: 0x000586D2 File Offset: 0x000568D2
	public static ulong PlatformUserId
	{
		get
		{
			if (PrivilegeManager.privilegeData != null)
			{
				return PrivilegeManager.privilegeData.Value.platformUserId;
			}
			ZLog.LogError("Can't get PlatformUserId before the privilege manager has been initialized!");
			return 0UL;
		}
	}

	// Token: 0x06000C19 RID: 3097 RVA: 0x000586FC File Offset: 0x000568FC
	public static void SetPrivilegeData(PrivilegeData privilegeData)
	{
		if (privilegeData.platformCanAccess == null)
		{
			string text = "The platformCanAccess delegate cannot be null!";
			ZLog.LogError(text);
			throw new ArgumentException(text);
		}
		PrivilegeManager.privilegeData = new PrivilegeData?(privilegeData);
	}

	// Token: 0x06000C1A RID: 3098 RVA: 0x00058722 File Offset: 0x00056922
	public static void ResetPrivilegeData()
	{
		PrivilegeManager.privilegeData = null;
	}

	// Token: 0x06000C1B RID: 3099 RVA: 0x0005872F File Offset: 0x0005692F
	public static string GetNetworkUserId()
	{
		return string.Format("{0}{1}", PrivilegeManager.GetPlatformPrefix(PrivilegeManager.GetCurrentPlatform()), PrivilegeManager.PlatformUserId);
	}

	// Token: 0x06000C1C RID: 3100 RVA: 0x0000290F File Offset: 0x00000B0F
	public static PrivilegeManager.Platform GetCurrentPlatform()
	{
		return PrivilegeManager.Platform.Steam;
	}

	// Token: 0x06000C1D RID: 3101 RVA: 0x0005874F File Offset: 0x0005694F
	public static string GetPlatformName(PrivilegeManager.Platform platform)
	{
		return string.Format("{0}", platform);
	}

	// Token: 0x06000C1E RID: 3102 RVA: 0x00058761 File Offset: 0x00056961
	public static string GetPlatformPrefix(PrivilegeManager.Platform platform)
	{
		return PrivilegeManager.GetPlatformName(platform) + "_";
	}

	// Token: 0x06000C1F RID: 3103 RVA: 0x00058773 File Offset: 0x00056973
	public static void FlushCache()
	{
		PrivilegeManager.Cache.Clear();
	}

	// Token: 0x1700006F RID: 111
	// (get) Token: 0x06000C20 RID: 3104 RVA: 0x0005877F File Offset: 0x0005697F
	public static bool CanAccessOnlineMultiplayer
	{
		get
		{
			if (PrivilegeManager.privilegeData != null)
			{
				return PrivilegeManager.privilegeData.Value.canAccessOnlineMultiplayer;
			}
			ZLog.LogError("Can't check \"CanAccessOnlineMultiplayer\" privilege before the privilege manager has been initialized!");
			return false;
		}
	}

	// Token: 0x17000070 RID: 112
	// (get) Token: 0x06000C21 RID: 3105 RVA: 0x000587A8 File Offset: 0x000569A8
	public static bool CanViewUserGeneratedContentAll
	{
		get
		{
			if (PrivilegeManager.privilegeData != null)
			{
				return PrivilegeManager.privilegeData.Value.canViewUserGeneratedContentAll;
			}
			ZLog.LogError("Can't check \"CanViewUserGeneratedContentAll\" privilege before the privilege manager has been initialized!");
			return false;
		}
	}

	// Token: 0x17000071 RID: 113
	// (get) Token: 0x06000C22 RID: 3106 RVA: 0x000587D1 File Offset: 0x000569D1
	public static bool CanCrossplay
	{
		get
		{
			if (PrivilegeManager.privilegeData != null)
			{
				return PrivilegeManager.privilegeData.Value.canCrossplay;
			}
			ZLog.LogError("Can't check \"CanCrossplay\" privilege before the privilege manager has been initialized!");
			return false;
		}
	}

	// Token: 0x06000C23 RID: 3107 RVA: 0x000587FA File Offset: 0x000569FA
	public static void CanViewUserGeneratedContent(string user, CanAccessResult canViewUserGeneratedContentResult)
	{
		PrivilegeManager.CanAccess(PrivilegeManager.Permission.ViewTargetUserCreatedContent, user, canViewUserGeneratedContentResult);
	}

	// Token: 0x06000C24 RID: 3108 RVA: 0x00058804 File Offset: 0x00056A04
	public static void CanCommunicateWith(string user, CanAccessResult canCommunicateWithResult)
	{
		PrivilegeManager.CanAccess(PrivilegeManager.Permission.CommunicateUsingText, user, canCommunicateWithResult);
	}

	// Token: 0x06000C25 RID: 3109 RVA: 0x00058810 File Offset: 0x00056A10
	private static void CanAccess(PrivilegeManager.Permission permission, string platformUser, CanAccessResult canAccessResult)
	{
		PrivilegeManager.User user = PrivilegeManager.ParseUser(platformUser);
		PrivilegeManager.PrivilegeLookupKey key = new PrivilegeManager.PrivilegeLookupKey(permission, user);
		PrivilegeManager.Result access;
		if (PrivilegeManager.Cache.TryGetValue(key, out access))
		{
			canAccessResult(access);
			return;
		}
		if (PrivilegeManager.privilegeData != null)
		{
			PrivilegeManager.privilegeData.Value.platformCanAccess(permission, user, delegate(PrivilegeManager.Result res)
			{
				PrivilegeManager.CacheAndDeliverResult(res, canAccessResult, key);
			});
			return;
		}
		ZLog.LogError("Can't check \"" + permission.ToString() + "\" privilege before the privilege manager has been initialized!");
		CanAccessResult canAccessResult2 = canAccessResult;
		if (canAccessResult2 == null)
		{
			return;
		}
		canAccessResult2(PrivilegeManager.Result.Failed);
	}

	// Token: 0x06000C26 RID: 3110 RVA: 0x000588C0 File Offset: 0x00056AC0
	private static void CacheAndDeliverResult(PrivilegeManager.Result res, CanAccessResult canAccessResult, PrivilegeManager.PrivilegeLookupKey key)
	{
		if (res != PrivilegeManager.Result.Failed)
		{
			PrivilegeManager.Cache[key] = res;
		}
		canAccessResult(res);
	}

	// Token: 0x06000C27 RID: 3111 RVA: 0x000588DC File Offset: 0x00056ADC
	public static PrivilegeManager.User ParseUser(string platformUser)
	{
		PrivilegeManager.User result = new PrivilegeManager.User(PrivilegeManager.Platform.Unknown, 0UL);
		string[] array = platformUser.Split(new char[]
		{
			'_'
		});
		ulong i;
		if (array.Length == 2 && ulong.TryParse(array[1], out i))
		{
			if (array[0] == PrivilegeManager.GetPlatformName(PrivilegeManager.Platform.Steam))
			{
				result = new PrivilegeManager.User(PrivilegeManager.Platform.Steam, i);
			}
			else if (array[0] == PrivilegeManager.GetPlatformName(PrivilegeManager.Platform.Xbox))
			{
				result = new PrivilegeManager.User(PrivilegeManager.Platform.Xbox, i);
			}
			else if (array[0] == PrivilegeManager.GetPlatformName(PrivilegeManager.Platform.PlayFab))
			{
				result = new PrivilegeManager.User(PrivilegeManager.Platform.PlayFab, i);
			}
		}
		return result;
	}

	// Token: 0x06000C28 RID: 3112 RVA: 0x00058968 File Offset: 0x00056B68
	public static PrivilegeManager.Platform ParsePlatform(string platformString)
	{
		PrivilegeManager.Platform result;
		if (Enum.TryParse<PrivilegeManager.Platform>(platformString, out result))
		{
			return result;
		}
		ZLog.LogError("Failed to parse platform!");
		return PrivilegeManager.Platform.Unknown;
	}

	// Token: 0x04000E7A RID: 3706
	private static readonly Dictionary<PrivilegeManager.PrivilegeLookupKey, PrivilegeManager.Result> Cache = new Dictionary<PrivilegeManager.PrivilegeLookupKey, PrivilegeManager.Result>();

	// Token: 0x04000E7B RID: 3707
	private static PrivilegeData? privilegeData;

	// Token: 0x0200013A RID: 314
	public enum Platform
	{
		// Token: 0x04000E7D RID: 3709
		Unknown,
		// Token: 0x04000E7E RID: 3710
		Steam,
		// Token: 0x04000E7F RID: 3711
		Xbox,
		// Token: 0x04000E80 RID: 3712
		PlayFab,
		// Token: 0x04000E81 RID: 3713
		None
	}

	// Token: 0x0200013B RID: 315
	public struct User
	{
		// Token: 0x06000C2B RID: 3115 RVA: 0x00058998 File Offset: 0x00056B98
		public User(PrivilegeManager.Platform p, ulong i)
		{
			this.platform = p;
			this.id = i;
		}

		// Token: 0x04000E82 RID: 3714
		public readonly PrivilegeManager.Platform platform;

		// Token: 0x04000E83 RID: 3715
		public readonly ulong id;
	}

	// Token: 0x0200013C RID: 316
	public enum Result
	{
		// Token: 0x04000E85 RID: 3717
		Allowed,
		// Token: 0x04000E86 RID: 3718
		NotAllowed,
		// Token: 0x04000E87 RID: 3719
		Failed
	}

	// Token: 0x0200013D RID: 317
	public enum Permission
	{
		// Token: 0x04000E89 RID: 3721
		CommunicateUsingText,
		// Token: 0x04000E8A RID: 3722
		ViewTargetUserCreatedContent
	}

	// Token: 0x0200013E RID: 318
	private struct PrivilegeLookupKey
	{
		// Token: 0x06000C2C RID: 3116 RVA: 0x000589A8 File Offset: 0x00056BA8
		internal PrivilegeLookupKey(PrivilegeManager.Permission p, PrivilegeManager.User u)
		{
			this.permission = p;
			this.user = u;
		}

		// Token: 0x04000E8B RID: 3723
		internal readonly PrivilegeManager.Permission permission;

		// Token: 0x04000E8C RID: 3724
		internal readonly PrivilegeManager.User user;
	}
}
