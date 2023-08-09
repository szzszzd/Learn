using System;
using UnityEngine;

// Token: 0x020002AA RID: 682
public class ThorFly : MonoBehaviour
{
	// Token: 0x060019EF RID: 6639 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Start()
	{
	}

	// Token: 0x060019F0 RID: 6640 RVA: 0x000AC114 File Offset: 0x000AA314
	private void Update()
	{
		base.transform.position = base.transform.position + base.transform.forward * this.m_speed * Time.deltaTime;
		this.m_timer += Time.deltaTime;
		if (this.m_timer > this.m_ttl)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x04001BC8 RID: 7112
	public float m_speed = 100f;

	// Token: 0x04001BC9 RID: 7113
	public float m_ttl = 10f;

	// Token: 0x04001BCA RID: 7114
	private float m_timer;
}
