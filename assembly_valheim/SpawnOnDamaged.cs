using System;
using UnityEngine;

// Token: 0x02000297 RID: 663
public class SpawnOnDamaged : MonoBehaviour
{
	// Token: 0x0600196A RID: 6506 RVA: 0x000A8F44 File Offset: 0x000A7144
	private void Start()
	{
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDamaged = (Action)Delegate.Combine(wearNTear.m_onDamaged, new Action(this.OnDamaged));
		}
		Destructible component2 = base.GetComponent<Destructible>();
		if (component2)
		{
			Destructible destructible = component2;
			destructible.m_onDamaged = (Action)Delegate.Combine(destructible.m_onDamaged, new Action(this.OnDamaged));
		}
	}

	// Token: 0x0600196B RID: 6507 RVA: 0x000A8FB3 File Offset: 0x000A71B3
	private void OnDamaged()
	{
		if (this.m_spawnOnDamage)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_spawnOnDamage, base.transform.position, Quaternion.identity);
		}
	}

	// Token: 0x04001B57 RID: 6999
	public GameObject m_spawnOnDamage;
}
