using System;
using System.IO;
using Fishlabs.Common.AssetBundles;
using UnityEngine;

namespace Valheim.UI
{
	// Token: 0x020002DC RID: 732
	public static class SessionPlayerListLoader
	{
		// Token: 0x06001BC6 RID: 7110 RVA: 0x000B8F78 File Offset: 0x000B7178
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void OnRuntimeMethodLoad()
		{
			SessionPlayerListLoader.actions = new SessionPlayListActionsSteam();
			SessionPlayerList.OnDestroyEvent += SessionPlayerListLoader.actions.OnDestroy;
			SessionPlayerList.OnInitEvent += SessionPlayerListLoader.actions.OnInit;
			SessionPlayerListEntry.OnViewCardEvent += SessionPlayerListLoader.actions.OnViewCard;
			SessionPlayerListEntry.OnRemoveCallbacksEvent += SessionPlayerListLoader.actions.OnRemoveCallbacks;
			SessionPlayerListEntry.OnGetProfileEvent += SessionPlayerListLoader.actions.OnGetProfile;
			SessionPlayerListLoader.LoadSessionPlayerList();
		}

		// Token: 0x06001BC7 RID: 7111 RVA: 0x000B9002 File Offset: 0x000B7202
		private static void LoadSessionPlayerList()
		{
			AssetBundleManager.PreloadAssetBundleAsync(Path.Combine(Application.streamingAssetsPath, "general", "ui", "session_player_list"), delegate(bool success, AssetBundle assetBundle)
			{
				if (success)
				{
					GameObject gameObject = assetBundle.LoadAsset("SessionPlayerList") as GameObject;
					SessionPlayerListLoader.actions.PlayerListInstance = gameObject.GetComponent<SessionPlayerList>();
					Menu.CurrentPlayersPrefab = gameObject;
					return;
				}
				Debug.Log("Failed to load Prefab from AssetBundle!");
			});
		}

		// Token: 0x04001DE9 RID: 7657
		private const string SessionPlayerListPath = "session_player_list";

		// Token: 0x04001DEA RID: 7658
		private const string SessionPlayerListPrefabName = "SessionPlayerList";

		// Token: 0x04001DEB RID: 7659
		private static SessionPlayListActionsSteam actions;
	}
}
