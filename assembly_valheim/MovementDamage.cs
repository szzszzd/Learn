using System;
using UnityEngine;

// Token: 0x0200001F RID: 31
public class MovementDamage : MonoBehaviour
{
	// Token: 0x060001BB RID: 443 RVA: 0x0000C494 File Offset: 0x0000A694
	private void Awake()
	{
		this.m_character = base.GetComponent<Character>();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		Aoe component = this.m_runDamageObject.GetComponent<Aoe>();
		if (component)
		{
			component.Setup(this.m_character, Vector3.zero, 0f, null, null, null);
		}
	}

	// Token: 0x060001BC RID: 444 RVA: 0x0000C4F4 File Offset: 0x0000A6F4
	private void Update()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			this.m_runDamageObject.SetActive(false);
			return;
		}
		bool active = this.m_body.velocity.magnitude > this.m_speedTreshold;
		this.m_runDamageObject.SetActive(active);
	}

	// Token: 0x040001BB RID: 443
	public GameObject m_runDamageObject;

	// Token: 0x040001BC RID: 444
	public float m_speedTreshold = 6f;

	// Token: 0x040001BD RID: 445
	private Character m_character;

	// Token: 0x040001BE RID: 446
	private ZNetView m_nview;

	// Token: 0x040001BF RID: 447
	private Rigidbody m_body;
}
