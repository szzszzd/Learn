using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200022F RID: 559
public class EffectArea : MonoBehaviour
{
	// Token: 0x060015FB RID: 5627 RVA: 0x00090858 File Offset: 0x0008EA58
	private void Awake()
	{
		if (!string.IsNullOrEmpty(this.m_statusEffect))
		{
			this.m_statusEffectHash = this.m_statusEffect.GetStableHashCode();
		}
		if (EffectArea.m_characterMask == 0)
		{
			EffectArea.m_characterMask = LayerMask.GetMask(new string[]
			{
				"character_trigger"
			});
		}
		this.m_collider = base.GetComponent<Collider>();
		EffectArea.m_allAreas.Add(this);
	}

	// Token: 0x060015FC RID: 5628 RVA: 0x000908B9 File Offset: 0x0008EAB9
	private void OnDestroy()
	{
		EffectArea.m_allAreas.Remove(this);
	}

	// Token: 0x060015FD RID: 5629 RVA: 0x000908C8 File Offset: 0x0008EAC8
	private void OnTriggerStay(Collider collider)
	{
		if (ZNet.instance == null)
		{
			return;
		}
		Character component = collider.GetComponent<Character>();
		if (component && component.IsOwner())
		{
			if (this.m_playerOnly && !component.IsPlayer())
			{
				return;
			}
			if (!string.IsNullOrEmpty(this.m_statusEffect))
			{
				component.GetSEMan().AddStatusEffect(this.m_statusEffectHash, true, 0, 0f);
			}
			if ((this.m_type & EffectArea.Type.Heat) != (EffectArea.Type)0)
			{
				component.OnNearFire(base.transform.position);
			}
		}
	}

	// Token: 0x060015FE RID: 5630 RVA: 0x0009094C File Offset: 0x0008EB4C
	public float GetRadius()
	{
		SphereCollider sphereCollider = this.m_collider as SphereCollider;
		if (sphereCollider != null)
		{
			return sphereCollider.radius;
		}
		return this.m_collider.bounds.size.magnitude;
	}

	// Token: 0x060015FF RID: 5631 RVA: 0x00090990 File Offset: 0x0008EB90
	public static EffectArea IsPointInsideArea(Vector3 p, EffectArea.Type type, float radius = 0f)
	{
		int num = Physics.OverlapSphereNonAlloc(p, radius, EffectArea.m_tempColliders, EffectArea.m_characterMask);
		for (int i = 0; i < num; i++)
		{
			EffectArea component = EffectArea.m_tempColliders[i].GetComponent<EffectArea>();
			if (component && (component.m_type & type) != (EffectArea.Type)0)
			{
				return component;
			}
		}
		return null;
	}

	// Token: 0x06001600 RID: 5632 RVA: 0x000909E0 File Offset: 0x0008EBE0
	public static int GetBaseValue(Vector3 p, float radius)
	{
		int num = 0;
		int num2 = Physics.OverlapSphereNonAlloc(p, radius, EffectArea.m_tempColliders, EffectArea.m_characterMask);
		for (int i = 0; i < num2; i++)
		{
			EffectArea component = EffectArea.m_tempColliders[i].GetComponent<EffectArea>();
			if (component && (component.m_type & EffectArea.Type.PlayerBase) != (EffectArea.Type)0)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06001601 RID: 5633 RVA: 0x00090A31 File Offset: 0x0008EC31
	public static List<EffectArea> GetAllAreas()
	{
		return EffectArea.m_allAreas;
	}

	// Token: 0x040016F1 RID: 5873
	[BitMask(typeof(EffectArea.Type))]
	public EffectArea.Type m_type = EffectArea.Type.None;

	// Token: 0x040016F2 RID: 5874
	public string m_statusEffect = "";

	// Token: 0x040016F3 RID: 5875
	private int m_statusEffectHash;

	// Token: 0x040016F4 RID: 5876
	public bool m_playerOnly;

	// Token: 0x040016F5 RID: 5877
	private static int m_characterMask = 0;

	// Token: 0x040016F6 RID: 5878
	private Collider m_collider;

	// Token: 0x040016F7 RID: 5879
	private static List<EffectArea> m_allAreas = new List<EffectArea>();

	// Token: 0x040016F8 RID: 5880
	private static Collider[] m_tempColliders = new Collider[128];

	// Token: 0x02000230 RID: 560
	public enum Type
	{
		// Token: 0x040016FA RID: 5882
		Heat = 1,
		// Token: 0x040016FB RID: 5883
		Fire,
		// Token: 0x040016FC RID: 5884
		PlayerBase = 4,
		// Token: 0x040016FD RID: 5885
		Burning = 8,
		// Token: 0x040016FE RID: 5886
		Teleport = 16,
		// Token: 0x040016FF RID: 5887
		NoMonsters = 32,
		// Token: 0x04001700 RID: 5888
		WarmCozyArea = 64,
		// Token: 0x04001701 RID: 5889
		PrivateProperty = 128,
		// Token: 0x04001702 RID: 5890
		None = 999
	}
}
