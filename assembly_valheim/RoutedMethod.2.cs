using System;

// Token: 0x02000170 RID: 368
internal class RoutedMethod<T> : RoutedMethodBase
{
	// Token: 0x06000EBB RID: 3771 RVA: 0x00064ED7 File Offset: 0x000630D7
	public RoutedMethod(Action<long, T> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000EBC RID: 3772 RVA: 0x00064EE6 File Offset: 0x000630E6
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x0400106D RID: 4205
	private Action<long, T> m_action;
}
