using System;

// Token: 0x02000173 RID: 371
public class RoutedMethod<T, U, V, B> : RoutedMethodBase
{
	// Token: 0x06000EC1 RID: 3777 RVA: 0x00064F73 File Offset: 0x00063173
	public RoutedMethod(RoutedMethod<T, U, V, B>.Method action)
	{
		this.m_action = action;
	}

	// Token: 0x06000EC2 RID: 3778 RVA: 0x00064F82 File Offset: 0x00063182
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04001070 RID: 4208
	private RoutedMethod<T, U, V, B>.Method m_action;

	// Token: 0x02000174 RID: 372
	// (Invoke) Token: 0x06000EC4 RID: 3780
	public delegate void Method(long sender, T p0, U p1, V p2, B p3);
}
