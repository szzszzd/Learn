using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// Token: 0x0200015D RID: 349
[StructLayout(0, Pack = 2)]
public struct ZDOID : IEquatable<ZDOID>, IComparable<ZDOID>
{
	// Token: 0x06000D8F RID: 3471 RVA: 0x0005CEC0 File Offset: 0x0005B0C0
	public static ushort AddUser(long userID)
	{
		int num = ZDOID.m_userIDs.IndexOf(userID);
		if (num < 0)
		{
			ZDOID.m_userIDs.Add(userID);
			ushort userIDCount = ZDOID.m_userIDCount;
			ZDOID.m_userIDCount = userIDCount + 1;
			return userIDCount;
		}
		if (userID == 0L)
		{
			return 0;
		}
		return (ushort)num;
	}

	// Token: 0x06000D90 RID: 3472 RVA: 0x0005CEFE File Offset: 0x0005B0FE
	public static long GetUserID(ushort userKey)
	{
		return ZDOID.m_userIDs[(int)userKey];
	}

	// Token: 0x06000D91 RID: 3473 RVA: 0x0005CF0B File Offset: 0x0005B10B
	public ZDOID(BinaryReader reader)
	{
		this.UserKey = ZDOID.AddUser(reader.ReadInt64());
		this.ID = reader.ReadUInt32();
	}

	// Token: 0x06000D92 RID: 3474 RVA: 0x0005CF2A File Offset: 0x0005B12A
	public ZDOID(long userID, uint id)
	{
		this.UserKey = ZDOID.AddUser(userID);
		this.ID = id;
	}

	// Token: 0x06000D93 RID: 3475 RVA: 0x0005CF3F File Offset: 0x0005B13F
	public void SetID(uint id)
	{
		this.ID = id;
		this.UserKey = ZDOID.UnknownFormerUserKey;
	}

	// Token: 0x06000D94 RID: 3476 RVA: 0x0005CF54 File Offset: 0x0005B154
	public override string ToString()
	{
		return ZDOID.GetUserID(this.UserKey).ToString() + ":" + this.ID.ToString();
	}

	// Token: 0x06000D95 RID: 3477 RVA: 0x0005CF8C File Offset: 0x0005B18C
	public static bool operator ==(ZDOID a, ZDOID b)
	{
		return a.UserKey == b.UserKey && a.ID == b.ID;
	}

	// Token: 0x06000D96 RID: 3478 RVA: 0x0005CFB0 File Offset: 0x0005B1B0
	public static bool operator !=(ZDOID a, ZDOID b)
	{
		return a.UserKey != b.UserKey || a.ID != b.ID;
	}

	// Token: 0x06000D97 RID: 3479 RVA: 0x0005CFD7 File Offset: 0x0005B1D7
	public bool Equals(ZDOID other)
	{
		return other.UserKey == this.UserKey && other.ID == this.ID;
	}

	// Token: 0x06000D98 RID: 3480 RVA: 0x0005CFFC File Offset: 0x0005B1FC
	public override bool Equals(object other)
	{
		if (other is ZDOID)
		{
			ZDOID b = (ZDOID)other;
			return this == b;
		}
		return false;
	}

	// Token: 0x06000D99 RID: 3481 RVA: 0x0005D028 File Offset: 0x0005B228
	public int CompareTo(ZDOID other)
	{
		if (this.UserKey != other.UserKey)
		{
			if (this.UserKey >= other.UserKey)
			{
				return 1;
			}
			return -1;
		}
		else
		{
			if (this.ID < other.ID)
			{
				return -1;
			}
			if (this.ID <= other.ID)
			{
				return 0;
			}
			return 1;
		}
	}

	// Token: 0x06000D9A RID: 3482 RVA: 0x0005D07C File Offset: 0x0005B27C
	public override int GetHashCode()
	{
		return ZDOID.GetUserID(this.UserKey).GetHashCode() ^ this.ID.GetHashCode();
	}

	// Token: 0x06000D9B RID: 3483 RVA: 0x0005D0AB File Offset: 0x0005B2AB
	public bool IsNone()
	{
		return this.UserKey == 0 && this.ID == 0U;
	}

	// Token: 0x17000090 RID: 144
	// (get) Token: 0x06000D9C RID: 3484 RVA: 0x0005D0C0 File Offset: 0x0005B2C0
	public long UserID
	{
		get
		{
			return ZDOID.GetUserID(this.UserKey);
		}
	}

	// Token: 0x17000091 RID: 145
	// (get) Token: 0x06000D9D RID: 3485 RVA: 0x0005D0CD File Offset: 0x0005B2CD
	// (set) Token: 0x06000D9E RID: 3486 RVA: 0x0005D0D5 File Offset: 0x0005B2D5
	private ushort UserKey { readonly get; set; }

	// Token: 0x17000092 RID: 146
	// (get) Token: 0x06000D9F RID: 3487 RVA: 0x0005D0DE File Offset: 0x0005B2DE
	// (set) Token: 0x06000DA0 RID: 3488 RVA: 0x0005D0E6 File Offset: 0x0005B2E6
	public uint ID { readonly get; private set; }

	// Token: 0x04000F2D RID: 3885
	private static readonly long NullUser = 0L;

	// Token: 0x04000F2E RID: 3886
	private static readonly long UnknownFormerUser = 1L;

	// Token: 0x04000F2F RID: 3887
	private static readonly ushort UnknownFormerUserKey = 1;

	// Token: 0x04000F30 RID: 3888
	public static uint m_loadID = 0U;

	// Token: 0x04000F31 RID: 3889
	private static readonly List<long> m_userIDs = new List<long>
	{
		ZDOID.NullUser,
		ZDOID.UnknownFormerUser
	};

	// Token: 0x04000F32 RID: 3890
	public static readonly ZDOID None = new ZDOID(0L, 0U);

	// Token: 0x04000F33 RID: 3891
	private static ushort m_userIDCount = 2;
}
