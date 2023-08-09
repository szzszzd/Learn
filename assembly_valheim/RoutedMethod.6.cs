using System;

// Token: 0x02000175 RID: 373
public class RoutedMethod<T, U, V, B, K> : RoutedMethodBase
{
	// Token: 0x06000EC7 RID: 3783 RVA: 0x00064FA7 File Offset: 0x000631A7
	public RoutedMethod(RoutedMethod<T, U, V, B, K>.Method action)
	{
		this.m_action = action;
	}

	// Token: 0x06000EC8 RID: 3784 RVA: 0x00064FB6 File Offset: 0x000631B6
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04001071 RID: 4209
	private RoutedMethod<T, U, V, B, K>.Method m_action;

	// Token: 0x02000176 RID: 374
	// (Invoke) Token: 0x06000ECA RID: 3786
	public delegate void Method(long sender, T p0, U p1, V p2, B p3, K p4);
}
