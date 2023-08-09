using System;

// Token: 0x02000172 RID: 370
internal class RoutedMethod<T, U, V> : RoutedMethodBase
{
	// Token: 0x06000EBF RID: 3775 RVA: 0x00064F3F File Offset: 0x0006313F
	public RoutedMethod(Action<long, T, U, V> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x00064F4E File Offset: 0x0006314E
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x0400106F RID: 4207
	private Action<long, T, U, V> m_action;
}
