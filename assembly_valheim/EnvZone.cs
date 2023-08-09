using System;
using UnityEngine;

// Token: 0x02000231 RID: 561
public class EnvZone : MonoBehaviour
{
	// Token: 0x06001604 RID: 5636 RVA: 0x00090A77 File Offset: 0x0008EC77
	private void Awake()
	{
		if (this.m_exteriorMesh)
		{
			this.m_exteriorMesh.forceRenderingOff = true;
		}
	}

	// Token: 0x06001605 RID: 5637 RVA: 0x00090A94 File Offset: 0x0008EC94
	private void OnTriggerStay(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		if (this.m_force && string.IsNullOrEmpty(EnvMan.instance.m_debugEnv))
		{
			EnvMan.instance.SetForceEnvironment(this.m_environment);
		}
		EnvZone.s_triggered = this;
		if (this.m_exteriorMesh)
		{
			this.m_exteriorMesh.forceRenderingOff = false;
		}
	}

	// Token: 0x06001606 RID: 5638 RVA: 0x00090B08 File Offset: 0x0008ED08
	private void OnTriggerExit(Collider collider)
	{
		if (EnvZone.s_triggered != this)
		{
			return;
		}
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		if (this.m_force)
		{
			EnvMan.instance.SetForceEnvironment("");
		}
		EnvZone.s_triggered = null;
	}

	// Token: 0x06001607 RID: 5639 RVA: 0x00090B5F File Offset: 0x0008ED5F
	public static string GetEnvironment()
	{
		if (EnvZone.s_triggered && !EnvZone.s_triggered.m_force)
		{
			return EnvZone.s_triggered.m_environment;
		}
		return null;
	}

	// Token: 0x06001608 RID: 5640 RVA: 0x00090B85 File Offset: 0x0008ED85
	private void Update()
	{
		if (this.m_exteriorMesh)
		{
			this.m_exteriorMesh.forceRenderingOff = (EnvZone.s_triggered != this);
		}
	}

	// Token: 0x04001703 RID: 5891
	public string m_environment = "";

	// Token: 0x04001704 RID: 5892
	public bool m_force = true;

	// Token: 0x04001705 RID: 5893
	public MeshRenderer m_exteriorMesh;

	// Token: 0x04001706 RID: 5894
	private static EnvZone s_triggered;
}
