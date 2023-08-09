using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000296 RID: 662
[ExecuteInEditMode]
public class SnapToGround : MonoBehaviour
{
	// Token: 0x06001963 RID: 6499 RVA: 0x000A8E04 File Offset: 0x000A7004
	private void Awake()
	{
		SnapToGround.m_allSnappers.Add(this);
		this.m_inList = true;
	}

	// Token: 0x06001964 RID: 6500 RVA: 0x000A8E18 File Offset: 0x000A7018
	private void OnDestroy()
	{
		if (this.m_inList)
		{
			SnapToGround.m_allSnappers.Remove(this);
			this.m_inList = false;
		}
	}

	// Token: 0x06001965 RID: 6501 RVA: 0x000A8E38 File Offset: 0x000A7038
	private void Snap()
	{
		if (ZoneSystem.instance == null)
		{
			return;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		Vector3 position = base.transform.position;
		position.y = groundHeight + this.m_offset;
		base.transform.position = position;
		ZNetView component = base.GetComponent<ZNetView>();
		if (component != null && component.IsOwner())
		{
			component.GetZDO().SetPosition(position);
		}
	}

	// Token: 0x06001966 RID: 6502 RVA: 0x000A8EB4 File Offset: 0x000A70B4
	public bool HaveUnsnapped()
	{
		return SnapToGround.m_allSnappers.Count > 0;
	}

	// Token: 0x06001967 RID: 6503 RVA: 0x000A8EC4 File Offset: 0x000A70C4
	public static void SnappAll()
	{
		if (SnapToGround.m_allSnappers.Count == 0)
		{
			return;
		}
		Heightmap.ForceGenerateAll();
		foreach (SnapToGround snapToGround in SnapToGround.m_allSnappers)
		{
			snapToGround.Snap();
			snapToGround.m_inList = false;
		}
		SnapToGround.m_allSnappers.Clear();
	}

	// Token: 0x04001B54 RID: 6996
	public float m_offset;

	// Token: 0x04001B55 RID: 6997
	private static List<SnapToGround> m_allSnappers = new List<SnapToGround>();

	// Token: 0x04001B56 RID: 6998
	private bool m_inList;
}
