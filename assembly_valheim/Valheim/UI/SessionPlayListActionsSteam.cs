using System;
using Fishlabs.Core.Data;

namespace Valheim.UI
{
	// Token: 0x020002D6 RID: 726
	public class SessionPlayListActionsSteam : ISessionPlayListActions
	{
		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06001B6B RID: 7019 RVA: 0x000B7ABD File Offset: 0x000B5CBD
		// (set) Token: 0x06001B6C RID: 7020 RVA: 0x000B7AC5 File Offset: 0x000B5CC5
		public SessionPlayerList PlayerListInstance { get; set; }

		// Token: 0x06001B6D RID: 7021 RVA: 0x000023E2 File Offset: 0x000005E2
		public void OnDestroy()
		{
		}

		// Token: 0x06001B6E RID: 7022 RVA: 0x000023E2 File Offset: 0x000005E2
		public void OnGetProfile(ulong xBoxUserId, Action<ulong, Profile> callback)
		{
		}

		// Token: 0x06001B6F RID: 7023 RVA: 0x000023E2 File Offset: 0x000005E2
		public void OnInit()
		{
		}

		// Token: 0x06001B70 RID: 7024 RVA: 0x000023E2 File Offset: 0x000005E2
		public void OnRemoveCallbacks(ulong xBoxUserId, Action<ulong, Profile> callback)
		{
		}

		// Token: 0x06001B71 RID: 7025 RVA: 0x000023E2 File Offset: 0x000005E2
		public void OnViewCard(ulong xBoxUserId)
		{
		}
	}
}
