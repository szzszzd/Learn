using System;
using UnityEngine;

// Token: 0x02000036 RID: 54
public class Talker : MonoBehaviour
{
	// Token: 0x0600033F RID: 831 RVA: 0x00018CFD File Offset: 0x00016EFD
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_character = base.GetComponent<Character>();
		this.m_nview.Register<int, UserInfo, string, string>("Say", new RoutedMethod<int, UserInfo, string, string>.Method(this.RPC_Say));
	}

	// Token: 0x06000340 RID: 832 RVA: 0x00018D34 File Offset: 0x00016F34
	public void Say(Talker.Type type, string text)
	{
		ZLog.Log("Saying " + type.ToString() + "  " + text);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "Say", new object[]
		{
			(int)type,
			UserInfo.GetLocalUser(),
			text,
			PrivilegeManager.GetNetworkUserId()
		});
	}

	// Token: 0x06000341 RID: 833 RVA: 0x00018D9C File Offset: 0x00016F9C
	private void RPC_Say(long sender, int ctype, UserInfo user, string text, string senderNetworkUserId)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		float num = 0f;
		switch (ctype)
		{
		case 0:
			num = this.m_visperDistance;
			break;
		case 1:
			num = this.m_normalDistance;
			break;
		case 2:
			num = this.m_shoutDistance;
			break;
		}
		if (Vector3.Distance(base.transform.position, Player.m_localPlayer.transform.position) < num && Chat.instance)
		{
			Vector3 headPoint = this.m_character.GetHeadPoint();
			Chat.instance.OnNewChatMessage(base.gameObject, sender, headPoint, (Talker.Type)ctype, user, text, senderNetworkUserId);
		}
	}

	// Token: 0x0400032B RID: 811
	public float m_visperDistance = 4f;

	// Token: 0x0400032C RID: 812
	public float m_normalDistance = 15f;

	// Token: 0x0400032D RID: 813
	public float m_shoutDistance = 70f;

	// Token: 0x0400032E RID: 814
	private ZNetView m_nview;

	// Token: 0x0400032F RID: 815
	private Character m_character;

	// Token: 0x02000037 RID: 55
	public enum Type
	{
		// Token: 0x04000331 RID: 817
		Whisper,
		// Token: 0x04000332 RID: 818
		Normal,
		// Token: 0x04000333 RID: 819
		Shout,
		// Token: 0x04000334 RID: 820
		Ping
	}
}
