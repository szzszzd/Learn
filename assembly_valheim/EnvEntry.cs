using System;

// Token: 0x020001BD RID: 445
[Serializable]
public class EnvEntry
{
	// Token: 0x0400126A RID: 4714
	public string m_environment = "";

	// Token: 0x0400126B RID: 4715
	public float m_weight = 1f;

	// Token: 0x0400126C RID: 4716
	[NonSerialized]
	public EnvSetup m_env;
}
