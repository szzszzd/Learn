using System;
using System.Runtime.CompilerServices;

// Token: 0x02000206 RID: 518
public struct GameVersion
{
	// Token: 0x0600149E RID: 5278 RVA: 0x00086406 File Offset: 0x00084606
	public GameVersion(int major, int minor, int patch)
	{
		this.m_major = major;
		this.m_minor = minor;
		this.m_patch = patch;
	}

	// Token: 0x0600149F RID: 5279 RVA: 0x00086420 File Offset: 0x00084620
	public static bool TryParseGameVersion(string versionString, out GameVersion version)
	{
		version = new GameVersion(0, 0, 0);
		string[] array = versionString.Split(new char[]
		{
			'.'
		});
		if (array.Length < 2)
		{
			return false;
		}
		if (!GameVersion.<TryParseGameVersion>g__TryGetFirstNumberFromString|4_0(array[0], out version.m_major) || !GameVersion.<TryParseGameVersion>g__TryGetFirstNumberFromString|4_0(array[1], out version.m_minor))
		{
			return false;
		}
		if (array.Length == 2)
		{
			return true;
		}
		if (array[2].StartsWith("rc"))
		{
			if (!GameVersion.<TryParseGameVersion>g__TryGetFirstNumberFromString|4_0(array[2].Substring(2), out version.m_patch))
			{
				return false;
			}
			version.m_patch = -version.m_patch;
		}
		else if (!GameVersion.<TryParseGameVersion>g__TryGetFirstNumberFromString|4_0(array[2], out version.m_patch))
		{
			return false;
		}
		return true;
	}

	// Token: 0x060014A0 RID: 5280 RVA: 0x000864C8 File Offset: 0x000846C8
	public bool Equals(GameVersion other)
	{
		return this.m_major == other.m_major && this.m_minor == other.m_minor && this.m_patch == other.m_patch;
	}

	// Token: 0x060014A1 RID: 5281 RVA: 0x000864F8 File Offset: 0x000846F8
	private static bool IsVersionNewer(GameVersion other, GameVersion reference)
	{
		if (other.m_major > reference.m_major)
		{
			return true;
		}
		if (other.m_major == reference.m_major && other.m_minor > reference.m_minor)
		{
			return true;
		}
		if (other.m_major != reference.m_major || other.m_minor != reference.m_minor)
		{
			return false;
		}
		if (reference.m_patch >= 0)
		{
			return other.m_patch > reference.m_patch;
		}
		return other.m_patch >= 0 || other.m_patch < reference.m_patch;
	}

	// Token: 0x060014A2 RID: 5282 RVA: 0x00086584 File Offset: 0x00084784
	public override string ToString()
	{
		string result;
		if (this.m_patch == 0)
		{
			result = this.m_major.ToString() + "." + this.m_minor.ToString();
		}
		else if (this.m_patch < 0)
		{
			result = string.Concat(new string[]
			{
				this.m_major.ToString(),
				".",
				this.m_minor.ToString(),
				".rc",
				(-this.m_patch).ToString()
			});
		}
		else
		{
			result = string.Concat(new string[]
			{
				this.m_major.ToString(),
				".",
				this.m_minor.ToString(),
				".",
				this.m_patch.ToString()
			});
		}
		return result;
	}

	// Token: 0x060014A3 RID: 5283 RVA: 0x0008665B File Offset: 0x0008485B
	public override bool Equals(object other)
	{
		return other != null && other is GameVersion && this.Equals((GameVersion)other);
	}

	// Token: 0x060014A4 RID: 5284 RVA: 0x00086678 File Offset: 0x00084878
	public override int GetHashCode()
	{
		return ((313811945 * -1521134295 + this.m_major.GetHashCode()) * -1521134295 + this.m_minor.GetHashCode()) * -1521134295 + this.m_patch.GetHashCode();
	}

	// Token: 0x060014A5 RID: 5285 RVA: 0x000866B5 File Offset: 0x000848B5
	public static bool operator ==(GameVersion lhs, GameVersion rhs)
	{
		return lhs.Equals(rhs);
	}

	// Token: 0x060014A6 RID: 5286 RVA: 0x000866BF File Offset: 0x000848BF
	public static bool operator !=(GameVersion lhs, GameVersion rhs)
	{
		return !(lhs == rhs);
	}

	// Token: 0x060014A7 RID: 5287 RVA: 0x000866CB File Offset: 0x000848CB
	public static bool operator >(GameVersion lhs, GameVersion rhs)
	{
		return GameVersion.IsVersionNewer(lhs, rhs);
	}

	// Token: 0x060014A8 RID: 5288 RVA: 0x000866D4 File Offset: 0x000848D4
	public static bool operator <(GameVersion lhs, GameVersion rhs)
	{
		return GameVersion.IsVersionNewer(rhs, lhs);
	}

	// Token: 0x060014A9 RID: 5289 RVA: 0x000866DD File Offset: 0x000848DD
	public static bool operator >=(GameVersion lhs, GameVersion rhs)
	{
		return lhs == rhs || lhs > rhs;
	}

	// Token: 0x060014AA RID: 5290 RVA: 0x000866F1 File Offset: 0x000848F1
	public static bool operator <=(GameVersion lhs, GameVersion rhs)
	{
		return lhs == rhs || lhs < rhs;
	}

	// Token: 0x060014AB RID: 5291 RVA: 0x00086708 File Offset: 0x00084908
	[CompilerGenerated]
	internal static bool <TryParseGameVersion>g__TryGetFirstNumberFromString|4_0(string input, out int output)
	{
		output = 0;
		char[] array = new char[input.Length];
		int num = 0;
		for (int i = 0; i < input.Length; i++)
		{
			if (char.IsNumber(input[i]))
			{
				array[num++] = input[i];
			}
			else if (num > 0)
			{
				break;
			}
		}
		return num > 0 && int.TryParse(new string(array, 0, num), out output);
	}

	// Token: 0x0400155D RID: 5469
	public int m_major;

	// Token: 0x0400155E RID: 5470
	public int m_minor;

	// Token: 0x0400155F RID: 5471
	public int m_patch;
}
