using System;
using UnityEngine;

// Token: 0x020002A7 RID: 679
public class TestCollision : MonoBehaviour
{
	// Token: 0x060019E4 RID: 6628 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Start()
	{
	}

	// Token: 0x060019E5 RID: 6629 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Update()
	{
	}

	// Token: 0x060019E6 RID: 6630 RVA: 0x000ABDDC File Offset: 0x000A9FDC
	public void OnCollisionEnter(Collision info)
	{
		ZLog.Log("Hit by " + info.rigidbody.gameObject.name);
		ZLog.Log("rel vel " + info.relativeVelocity.ToString() + " " + info.relativeVelocity.ToString());
		ZLog.Log("Vel " + info.rigidbody.velocity.ToString() + "  " + info.rigidbody.angularVelocity.ToString());
	}
}
