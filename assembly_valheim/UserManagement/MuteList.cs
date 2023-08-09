using System;
using System.Collections.Generic;

namespace UserManagement
{
	// Token: 0x020002E1 RID: 737
	public static class MuteList
	{
		// Token: 0x06001BDB RID: 7131 RVA: 0x000B9338 File Offset: 0x000B7538
		public static bool IsMuted(string userId)
		{
			return MuteList._mutedUsers.Contains(userId);
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x000B9345 File Offset: 0x000B7545
		public static void Mute(string userId)
		{
			MuteList._mutedUsers.Add(userId);
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x000B9353 File Offset: 0x000B7553
		public static void Unmute(string userId)
		{
			MuteList._mutedUsers.Remove(userId);
		}

		// Token: 0x04001DF7 RID: 7671
		private static readonly HashSet<string> _mutedUsers = new HashSet<string>();
	}
}
