using System;

// Token: 0x02000138 RID: 312
public struct PrivilegeData
{
	// Token: 0x04000E75 RID: 3701
	public ulong platformUserId;

	// Token: 0x04000E76 RID: 3702
	public bool canAccessOnlineMultiplayer;

	// Token: 0x04000E77 RID: 3703
	public bool canViewUserGeneratedContentAll;

	// Token: 0x04000E78 RID: 3704
	public bool canCrossplay;

	// Token: 0x04000E79 RID: 3705
	public CanAccessCallback platformCanAccess;
}
