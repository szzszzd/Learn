using System;
using Fishlabs.Core.Data;

namespace Valheim.UI
{
	// Token: 0x020002D4 RID: 724
	public interface ISessionPlayListActions
	{
		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06001B63 RID: 7011
		SessionPlayerList PlayerListInstance { get; }

		// Token: 0x06001B64 RID: 7012
		void OnDestroy();

		// Token: 0x06001B65 RID: 7013
		void OnInit();

		// Token: 0x06001B66 RID: 7014
		void OnViewCard(ulong xBoxUserId);

		// Token: 0x06001B67 RID: 7015
		void OnRemoveCallbacks(ulong xBoxUserId, Action<ulong, Profile> callback);

		// Token: 0x06001B68 RID: 7016
		void OnGetProfile(ulong xBoxUserId, Action<ulong, Profile> callback);
	}
}
