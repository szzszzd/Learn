using System;
using UnityEngine;

// Token: 0x02000205 RID: 517
internal abstract class Version
{
	// Token: 0x170000E0 RID: 224
	// (get) Token: 0x06001498 RID: 5272 RVA: 0x00086344 File Offset: 0x00084544
	public static GameVersion CurrentVersion { get; } = new GameVersion(0, 216, 9);

	// Token: 0x06001499 RID: 5273 RVA: 0x0008634C File Offset: 0x0008454C
	public static string GetVersionString(bool includeMercurialHash = false)
	{
		string text = global::Version.CurrentVersion.ToString();
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			text = "dw-" + text;
		}
		if (includeMercurialHash)
		{
			TextAsset textAsset = Resources.Load<TextAsset>("clientVersion");
			if (textAsset != null)
			{
				text = text + "\n" + textAsset.text;
			}
		}
		return text;
	}

	// Token: 0x0600149A RID: 5274 RVA: 0x000863AA File Offset: 0x000845AA
	public static bool IsWorldVersionCompatible(int version)
	{
		return version <= 31 && version >= 9;
	}

	// Token: 0x0600149B RID: 5275 RVA: 0x000863BB File Offset: 0x000845BB
	public static bool IsPlayerVersionCompatible(int version)
	{
		return version <= 37 && version >= 27;
	}

	// Token: 0x04001554 RID: 5460
	public const uint m_networkVersion = 5U;

	// Token: 0x04001555 RID: 5461
	public const int m_playerVersion = 37;

	// Token: 0x04001556 RID: 5462
	private const int m_oldestForwardCompatiblePlayerVersion = 27;

	// Token: 0x04001557 RID: 5463
	public const int m_worldVersion = 31;

	// Token: 0x04001558 RID: 5464
	private const int m_oldestForwardCompatibleWorldVersion = 9;

	// Token: 0x04001559 RID: 5465
	public const int c_WorldVersionNewSaveFormat = 31;

	// Token: 0x0400155A RID: 5466
	public const int m_worldGenVersion = 2;

	// Token: 0x0400155B RID: 5467
	public static GameVersion FirstVersionWithNetworkVersion = new GameVersion(0, 214, 301);

	// Token: 0x0400155C RID: 5468
	public static GameVersion FirstVersionWithPlatformRestriction = new GameVersion(0, 213, 3);
}
