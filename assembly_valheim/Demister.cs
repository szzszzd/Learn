using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000227 RID: 551
public class Demister : MonoBehaviour
{
	// Token: 0x060015D2 RID: 5586 RVA: 0x0008F7C0 File Offset: 0x0008D9C0
	private void Awake()
	{
		this.m_forceField = base.GetComponent<ParticleSystemForceField>();
		this.m_lastUpdatePosition = base.transform.position;
		if (this.m_disableForcefieldDelay > 0f)
		{
			base.Invoke("DisableForcefield", this.m_disableForcefieldDelay);
		}
	}

	// Token: 0x060015D3 RID: 5587 RVA: 0x0008F7FD File Offset: 0x0008D9FD
	private void OnEnable()
	{
		Demister.m_instances.Add(this);
	}

	// Token: 0x060015D4 RID: 5588 RVA: 0x0008F80A File Offset: 0x0008DA0A
	private void OnDisable()
	{
		Demister.m_instances.Remove(this);
	}

	// Token: 0x060015D5 RID: 5589 RVA: 0x0008F818 File Offset: 0x0008DA18
	private void DisableForcefield()
	{
		this.m_forceField.enabled = false;
	}

	// Token: 0x060015D6 RID: 5590 RVA: 0x0008F828 File Offset: 0x0008DA28
	public float GetMovedDistance()
	{
		Vector3 position = base.transform.position;
		if (position == this.m_lastUpdatePosition)
		{
			return 0f;
		}
		float a = Vector3.Distance(position, this.m_lastUpdatePosition);
		this.m_lastUpdatePosition = position;
		return Mathf.Min(a, 10f);
	}

	// Token: 0x060015D7 RID: 5591 RVA: 0x0008F872 File Offset: 0x0008DA72
	public static List<Demister> GetDemisters()
	{
		return Demister.m_instances;
	}

	// Token: 0x040016C1 RID: 5825
	public float m_disableForcefieldDelay;

	// Token: 0x040016C2 RID: 5826
	[NonSerialized]
	public ParticleSystemForceField m_forceField;

	// Token: 0x040016C3 RID: 5827
	private Vector3 m_lastUpdatePosition;

	// Token: 0x040016C4 RID: 5828
	private static List<Demister> m_instances = new List<Demister>();
}
