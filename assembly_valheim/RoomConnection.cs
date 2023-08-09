using System;
using UnityEngine;

// Token: 0x020002D3 RID: 723
public class RoomConnection : MonoBehaviour
{
	// Token: 0x06001B60 RID: 7008 RVA: 0x000B7894 File Offset: 0x000B5A94
	private void OnDrawGizmos()
	{
		if (this.m_entrance)
		{
			Gizmos.color = Color.white;
		}
		else
		{
			Gizmos.color = new Color(1f, 1f, 0f, 1f);
		}
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawCube(Vector3.zero, new Vector3(2f, 0.02f, 0.2f));
		Gizmos.DrawCube(new Vector3(0f, 0f, 0.35f), new Vector3(0.2f, 0.02f, 0.5f));
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x06001B61 RID: 7009 RVA: 0x000B795C File Offset: 0x000B5B5C
	public bool TestContact(RoomConnection other)
	{
		return Vector3.Distance(base.transform.position, other.transform.position) < 0.1f;
	}

	// Token: 0x04001DB9 RID: 7609
	public string m_type = "";

	// Token: 0x04001DBA RID: 7610
	public bool m_entrance;

	// Token: 0x04001DBB RID: 7611
	public bool m_allowDoor = true;

	// Token: 0x04001DBC RID: 7612
	public bool m_doorOnlyIfOtherAlsoAllowsDoor;

	// Token: 0x04001DBD RID: 7613
	[NonSerialized]
	public int m_placeOrder;
}
