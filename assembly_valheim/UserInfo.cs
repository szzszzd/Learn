using System;
using Fishlabs.Core.Data;

// Token: 0x0200014B RID: 331
public class UserInfo : ISerializableParameter
{
	// Token: 0x06000C84 RID: 3204 RVA: 0x00059675 File Offset: 0x00057875
	public static UserInfo GetLocalUser()
	{
		return new UserInfo
		{
			Name = Game.instance.GetPlayerProfile().GetName(),
			Gamertag = UserInfo.GetLocalPlayerGamertag(),
			NetworkUserId = PrivilegeManager.GetNetworkUserId()
		};
	}

	// Token: 0x06000C85 RID: 3205 RVA: 0x000596A7 File Offset: 0x000578A7
	public void Deserialize(ref ZPackage pkg)
	{
		this.Name = pkg.ReadString();
		this.Gamertag = pkg.ReadString();
		this.NetworkUserId = pkg.ReadString();
	}

	// Token: 0x06000C86 RID: 3206 RVA: 0x000596D0 File Offset: 0x000578D0
	public void Serialize(ref ZPackage pkg)
	{
		pkg.Write(this.Name);
		pkg.Write(this.Gamertag);
		pkg.Write(this.NetworkUserId);
	}

	// Token: 0x06000C87 RID: 3207 RVA: 0x000596F9 File Offset: 0x000578F9
	public string GetDisplayName(string networkUserId)
	{
		return this.Name + UserInfo.GamertagSuffix(this.Gamertag);
	}

	// Token: 0x06000C88 RID: 3208 RVA: 0x00059711 File Offset: 0x00057911
	public void UpdateGamertag(string gamertag)
	{
		this.Gamertag = gamertag;
	}

	// Token: 0x06000C89 RID: 3209 RVA: 0x0005971A File Offset: 0x0005791A
	private static string GetLocalPlayerGamertag()
	{
		if (UserInfo.GetLocalGamerTagFunc != null)
		{
			return UserInfo.GetLocalGamerTagFunc();
		}
		return "";
	}

	// Token: 0x06000C8A RID: 3210 RVA: 0x00059733 File Offset: 0x00057933
	public static string GamertagSuffix(string gamertag)
	{
		if (string.IsNullOrEmpty(gamertag))
		{
			return "";
		}
		return " [" + gamertag + "]";
	}

	// Token: 0x04000EBC RID: 3772
	public string Name;

	// Token: 0x04000EBD RID: 3773
	public string Gamertag;

	// Token: 0x04000EBE RID: 3774
	public string NetworkUserId;

	// Token: 0x04000EBF RID: 3775
	public static Action<PrivilegeManager.User, Action<Profile>> GetProfile = delegate(PrivilegeManager.User user, Action<Profile> callback)
	{
		if (callback != null)
		{
			callback(new Profile(user.id, "", "", "", "", ""));
		}
	};

	// Token: 0x04000EC0 RID: 3776
	public static Action<Action<Profile, Profile>> PlatformRegisterForProfileUpdates;

	// Token: 0x04000EC1 RID: 3777
	public static Action<Action<Profile, Profile>> PlatformUnregisterForProfileUpdates;

	// Token: 0x04000EC2 RID: 3778
	public static Func<string> GetLocalGamerTagFunc;
}
