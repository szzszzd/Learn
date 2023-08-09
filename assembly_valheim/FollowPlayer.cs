using System;
using UnityEngine;

// Token: 0x02000072 RID: 114
public class FollowPlayer : MonoBehaviour
{
	// Token: 0x06000582 RID: 1410 RVA: 0x0002AF60 File Offset: 0x00029160
	private void LateUpdate()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (Player.m_localPlayer == null || mainCamera == null)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (this.m_follow == FollowPlayer.Type.Camera || GameCamera.InFreeFly())
		{
			vector = mainCamera.transform.position;
		}
		else if (this.m_follow == FollowPlayer.Type.Average)
		{
			if (GameCamera.InFreeFly())
			{
				vector = mainCamera.transform.position;
			}
			else
			{
				vector = (mainCamera.transform.position + Player.m_localPlayer.transform.position) * 0.5f;
			}
		}
		else
		{
			vector = Player.m_localPlayer.transform.position;
		}
		if (this.m_lockYPos)
		{
			vector.y = base.transform.position.y;
		}
		if (vector.y > this.m_maxYPos)
		{
			vector.y = this.m_maxYPos;
		}
		base.transform.position = vector;
	}

	// Token: 0x0400067A RID: 1658
	public FollowPlayer.Type m_follow = FollowPlayer.Type.Camera;

	// Token: 0x0400067B RID: 1659
	public bool m_lockYPos;

	// Token: 0x0400067C RID: 1660
	public float m_maxYPos = 1000000f;

	// Token: 0x02000073 RID: 115
	public enum Type
	{
		// Token: 0x0400067E RID: 1662
		Player,
		// Token: 0x0400067F RID: 1663
		Camera,
		// Token: 0x04000680 RID: 1664
		Average
	}
}
