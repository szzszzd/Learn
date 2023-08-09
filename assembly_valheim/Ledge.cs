using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200025B RID: 603
public class Ledge : MonoBehaviour
{
	// Token: 0x06001751 RID: 5969 RVA: 0x0009A5F0 File Offset: 0x000987F0
	private void Awake()
	{
		if (base.GetComponent<ZNetView>().GetZDO() == null)
		{
			return;
		}
		this.m_collider.enabled = true;
		TriggerTracker above = this.m_above;
		above.m_changed = (Action)Delegate.Combine(above.m_changed, new Action(this.Changed));
	}

	// Token: 0x06001752 RID: 5970 RVA: 0x0009A640 File Offset: 0x00098840
	private void Changed()
	{
		List<Collider> colliders = this.m_above.GetColliders();
		if (colliders.Count == 0)
		{
			this.m_collider.enabled = true;
			return;
		}
		bool enabled = false;
		using (List<Collider>.Enumerator enumerator = colliders.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.transform.position.y > base.transform.position.y)
				{
					enabled = true;
					break;
				}
			}
		}
		this.m_collider.enabled = enabled;
	}

	// Token: 0x040018AD RID: 6317
	public Collider m_collider;

	// Token: 0x040018AE RID: 6318
	public TriggerTracker m_above;
}
