using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement
{
	// Token: 0x020002DE RID: 734
	public static class BlockList
	{
		// Token: 0x06001BCB RID: 7115 RVA: 0x000B9092 File Offset: 0x000B7292
		public static bool IsBlocked(string user)
		{
			return BlockList.IsGameBlocked(user) || BlockList.IsPlatformBlocked(user);
		}

		// Token: 0x06001BCC RID: 7116 RVA: 0x000B90A4 File Offset: 0x000B72A4
		public static bool IsGameBlocked(string user)
		{
			return BlockList._blockedUsers.Contains(user);
		}

		// Token: 0x06001BCD RID: 7117 RVA: 0x000B90B1 File Offset: 0x000B72B1
		public static bool IsPlatformBlocked(string user)
		{
			return BlockList._platformBlockedUsers.Contains(user);
		}

		// Token: 0x06001BCE RID: 7118 RVA: 0x000B90BE File Offset: 0x000B72BE
		public static void Block(string user)
		{
			if (!BlockList._blockedUsers.Contains(user))
			{
				BlockList._blockedUsers.Add(user);
			}
		}

		// Token: 0x06001BCF RID: 7119 RVA: 0x000B90D9 File Offset: 0x000B72D9
		public static void Unblock(string user)
		{
			if (BlockList._blockedUsers.Contains(user))
			{
				BlockList._blockedUsers.Remove(user);
			}
		}

		// Token: 0x06001BD0 RID: 7120 RVA: 0x000B90F4 File Offset: 0x000B72F4
		public static void Persist()
		{
			if (BlockList._blockedUsers.Count > 0)
			{
				Action<byte[]> persistAction = BlockList.PersistAction;
				if (persistAction == null)
				{
					return;
				}
				persistAction(BlockList.Encode());
			}
		}

		// Token: 0x06001BD1 RID: 7121 RVA: 0x000B9118 File Offset: 0x000B7318
		public static void UpdateAvoidList(Action onUpdated = null)
		{
			Func<Action<string[]>, string[]> getPlatformBlocksFunc = BlockList.GetPlatformBlocksFunc;
			BlockList.UpdateAvoidList((getPlatformBlocksFunc != null) ? getPlatformBlocksFunc(delegate(string[] networkIds)
			{
				BlockList.UpdateAvoidList(networkIds);
				Action onUpdated3 = onUpdated;
				if (onUpdated3 == null)
				{
					return;
				}
				onUpdated3();
			}) : null);
			Action onUpdated2 = onUpdated;
			if (onUpdated2 == null)
			{
				return;
			}
			onUpdated2();
		}

		// Token: 0x06001BD2 RID: 7122 RVA: 0x000B9164 File Offset: 0x000B7364
		private static void UpdateAvoidList(string[] networkIds)
		{
			BlockList._platformBlockedUsers.Clear();
			if (networkIds == null)
			{
				return;
			}
			foreach (string item in networkIds)
			{
				BlockList._platformBlockedUsers.Add(item);
			}
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x000B91A0 File Offset: 0x000B73A0
		public static void Load(Action onLoaded)
		{
			if (!BlockList._isLoading)
			{
				if (!BlockList._hasBeenLoaded)
				{
					BlockList._isLoading = true;
					if (BlockList.LoadAction != null)
					{
						Action<Action<byte[]>> loadAction = BlockList.LoadAction;
						if (loadAction == null)
						{
							return;
						}
						loadAction(delegate(byte[] bytes)
						{
							if (bytes != null)
							{
								BlockList.Decode(bytes);
							}
							BlockList._isLoading = false;
							BlockList._hasBeenLoaded = true;
							Action onLoaded4 = onLoaded;
							if (onLoaded4 == null)
							{
								return;
							}
							onLoaded4();
						});
						return;
					}
					else
					{
						BlockList._isLoading = false;
						BlockList._hasBeenLoaded = true;
						Action onLoaded2 = onLoaded;
						if (onLoaded2 == null)
						{
							return;
						}
						onLoaded2();
						return;
					}
				}
				else
				{
					Action onLoaded3 = onLoaded;
					if (onLoaded3 == null)
					{
						return;
					}
					onLoaded3();
				}
			}
		}

		// Token: 0x06001BD4 RID: 7124 RVA: 0x000B9220 File Offset: 0x000B7420
		private static byte[] Encode()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in BlockList._blockedUsers)
			{
				stringBuilder.Append(value).Append('\n');
			}
			return Encoding.Unicode.GetBytes(stringBuilder.ToString());
		}

		// Token: 0x06001BD5 RID: 7125 RVA: 0x000B9290 File Offset: 0x000B7490
		private static void Decode(byte[] bytes)
		{
			BlockList._blockedUsers.Clear();
			foreach (string text in Encoding.Unicode.GetString(bytes).Split(new char[]
			{
				'\n'
			}))
			{
				if (!string.IsNullOrEmpty(text))
				{
					BlockList.Block(text);
				}
			}
		}

		// Token: 0x04001DEE RID: 7662
		private static readonly HashSet<string> _blockedUsers = new HashSet<string>();

		// Token: 0x04001DEF RID: 7663
		private static readonly HashSet<string> _platformBlockedUsers = new HashSet<string>();

		// Token: 0x04001DF0 RID: 7664
		private static bool _hasBeenLoaded;

		// Token: 0x04001DF1 RID: 7665
		private static bool _isLoading;

		// Token: 0x04001DF2 RID: 7666
		public static Action<byte[]> PersistAction;

		// Token: 0x04001DF3 RID: 7667
		public static Action<Action<byte[]>> LoadAction;

		// Token: 0x04001DF4 RID: 7668
		public static Func<Action<string[]>, string[]> GetPlatformBlocksFunc;
	}
}
