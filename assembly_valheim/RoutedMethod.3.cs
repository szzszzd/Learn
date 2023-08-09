using System;

// Token: 0x02000171 RID: 369
internal class RoutedMethod<T, U> : RoutedMethodBase
{
	// Token: 0x06000EBD RID: 3773 RVA: 0x00064F0B File Offset: 0x0006310B
	public RoutedMethod(Action<long, T, U> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000EBE RID: 3774 RVA: 0x00064F1A File Offset: 0x0006311A
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x0400106E RID: 4206
	private Action<long, T, U> m_action;
}
