using System;

// Token: 0x0200016F RID: 367
internal class RoutedMethod : RoutedMethodBase
{
	// Token: 0x06000EB9 RID: 3769 RVA: 0x00064EBA File Offset: 0x000630BA
	public RoutedMethod(Action<long> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000EBA RID: 3770 RVA: 0x00064EC9 File Offset: 0x000630C9
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action(rpc);
	}

	// Token: 0x0400106C RID: 4204
	private Action<long> m_action;
}
