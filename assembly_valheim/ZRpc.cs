using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

// Token: 0x0200017C RID: 380
public class ZRpc : IDisposable
{
	// Token: 0x06000F3F RID: 3903 RVA: 0x000664A4 File Offset: 0x000646A4
	public ZRpc(ISocket socket)
	{
		this.m_socket = socket;
	}

	// Token: 0x06000F40 RID: 3904 RVA: 0x000664C9 File Offset: 0x000646C9
	public void Dispose()
	{
		this.m_socket.Dispose();
	}

	// Token: 0x06000F41 RID: 3905 RVA: 0x000664D6 File Offset: 0x000646D6
	public ISocket GetSocket()
	{
		return this.m_socket;
	}

	// Token: 0x06000F42 RID: 3906 RVA: 0x000664E0 File Offset: 0x000646E0
	public ZRpc.ErrorCode Update(float dt)
	{
		if (!this.m_socket.IsConnected())
		{
			return ZRpc.ErrorCode.Disconnected;
		}
		for (ZPackage zpackage = this.m_socket.Recv(); zpackage != null; zpackage = this.m_socket.Recv())
		{
			this.m_recvPackages++;
			this.m_recvData += zpackage.Size();
			try
			{
				this.HandlePackage(zpackage);
			}
			catch (EndOfStreamException ex)
			{
				ZLog.LogError("EndOfStreamException in ZRpc::HandlePackage: Assume incompatible version: " + ex.Message);
				return ZRpc.ErrorCode.IncompatibleVersion;
			}
			catch (Exception ex2)
			{
				string str = "Exception in ZRpc::HandlePackage: ";
				Exception ex3 = ex2;
				ZLog.Log(str + ((ex3 != null) ? ex3.ToString() : null));
			}
		}
		this.UpdatePing(dt);
		return ZRpc.ErrorCode.Success;
	}

	// Token: 0x06000F43 RID: 3907 RVA: 0x000665A4 File Offset: 0x000647A4
	private void UpdatePing(float dt)
	{
		this.m_pingTimer += dt;
		if (this.m_pingTimer > ZRpc.m_pingInterval)
		{
			this.m_pingTimer = 0f;
			this.m_pkg.Clear();
			this.m_pkg.Write(0);
			this.m_pkg.Write(true);
			this.SendPackage(this.m_pkg);
		}
		this.m_timeSinceLastPing += dt;
		if (this.m_timeSinceLastPing > ZRpc.m_timeout)
		{
			ZLog.LogWarning("ZRpc timeout detected");
			this.m_socket.Close();
		}
	}

	// Token: 0x06000F44 RID: 3908 RVA: 0x00066638 File Offset: 0x00064838
	private void ReceivePing(ZPackage package)
	{
		if (package.ReadBool())
		{
			this.m_pkg.Clear();
			this.m_pkg.Write(0);
			this.m_pkg.Write(false);
			this.SendPackage(this.m_pkg);
			return;
		}
		this.m_timeSinceLastPing = 0f;
	}

	// Token: 0x06000F45 RID: 3909 RVA: 0x00066688 File Offset: 0x00064888
	public float GetTimeSinceLastPing()
	{
		return this.m_timeSinceLastPing;
	}

	// Token: 0x06000F46 RID: 3910 RVA: 0x00066690 File Offset: 0x00064890
	public bool IsConnected()
	{
		return this.m_socket.IsConnected();
	}

	// Token: 0x06000F47 RID: 3911 RVA: 0x000666A0 File Offset: 0x000648A0
	private void HandlePackage(ZPackage package)
	{
		int num = package.ReadInt();
		if (num == 0)
		{
			this.ReceivePing(package);
			return;
		}
		ZRpc.RpcMethodBase rpcMethodBase2;
		if (ZRpc.m_DEBUG)
		{
			package.ReadString();
			ZRpc.RpcMethodBase rpcMethodBase;
			if (this.m_functions.TryGetValue(num, out rpcMethodBase))
			{
				rpcMethodBase.Invoke(this, package);
				return;
			}
		}
		else if (this.m_functions.TryGetValue(num, out rpcMethodBase2))
		{
			rpcMethodBase2.Invoke(this, package);
		}
	}

	// Token: 0x06000F48 RID: 3912 RVA: 0x00066700 File Offset: 0x00064900
	public void Register(string name, ZRpc.RpcMethod.Method f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod(f));
	}

	// Token: 0x06000F49 RID: 3913 RVA: 0x00066734 File Offset: 0x00064934
	public void Register<T>(string name, Action<ZRpc, T> f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T>(f));
	}

	// Token: 0x06000F4A RID: 3914 RVA: 0x00066768 File Offset: 0x00064968
	public void Register<T, U>(string name, Action<ZRpc, T, U> f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T, U>(f));
	}

	// Token: 0x06000F4B RID: 3915 RVA: 0x0006679C File Offset: 0x0006499C
	public void Register<T, U, V>(string name, Action<ZRpc, T, U, V> f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T, U, V>(f));
	}

	// Token: 0x06000F4C RID: 3916 RVA: 0x000667D0 File Offset: 0x000649D0
	public void Register<T, U, V, W>(string name, ZRpc.RpcMethod<T, U, V, W>.Method f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T, U, V, W>(f));
	}

	// Token: 0x06000F4D RID: 3917 RVA: 0x00066804 File Offset: 0x00064A04
	public void Unregister(string name)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
	}

	// Token: 0x06000F4E RID: 3918 RVA: 0x00066828 File Offset: 0x00064A28
	public void Invoke(string method, params object[] parameters)
	{
		if (!this.IsConnected())
		{
			return;
		}
		this.m_pkg.Clear();
		int stableHashCode = method.GetStableHashCode();
		this.m_pkg.Write(stableHashCode);
		if (ZRpc.m_DEBUG)
		{
			this.m_pkg.Write(method);
		}
		ZRpc.Serialize(parameters, ref this.m_pkg);
		this.SendPackage(this.m_pkg);
	}

	// Token: 0x06000F4F RID: 3919 RVA: 0x00066887 File Offset: 0x00064A87
	private void SendPackage(ZPackage pkg)
	{
		this.m_sentPackages++;
		this.m_sentData += pkg.Size();
		this.m_socket.Send(this.m_pkg);
	}

	// Token: 0x06000F50 RID: 3920 RVA: 0x000668BC File Offset: 0x00064ABC
	public static void Serialize(object[] parameters, ref ZPackage pkg)
	{
		foreach (object obj in parameters)
		{
			if (obj is int)
			{
				pkg.Write((int)obj);
			}
			else if (obj is uint)
			{
				pkg.Write((uint)obj);
			}
			else if (obj is long)
			{
				pkg.Write((long)obj);
			}
			else if (obj is float)
			{
				pkg.Write((float)obj);
			}
			else if (obj is double)
			{
				pkg.Write((double)obj);
			}
			else if (obj is bool)
			{
				pkg.Write((bool)obj);
			}
			else if (obj is string)
			{
				pkg.Write((string)obj);
			}
			else if (obj is ZPackage)
			{
				pkg.Write((ZPackage)obj);
			}
			else
			{
				if (obj is List<string>)
				{
					List<string> list = obj as List<string>;
					pkg.Write(list.Count);
					using (List<string>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							string data = enumerator.Current;
							pkg.Write(data);
						}
						goto IL_207;
					}
				}
				if (obj is Vector3)
				{
					pkg.Write(((Vector3)obj).x);
					pkg.Write(((Vector3)obj).y);
					pkg.Write(((Vector3)obj).z);
				}
				else if (obj is Quaternion)
				{
					pkg.Write(((Quaternion)obj).x);
					pkg.Write(((Quaternion)obj).y);
					pkg.Write(((Quaternion)obj).z);
					pkg.Write(((Quaternion)obj).w);
				}
				else if (obj is ZDOID)
				{
					pkg.Write((ZDOID)obj);
				}
				else if (obj is HitData)
				{
					(obj as HitData).Serialize(ref pkg);
				}
				else if (obj is ISerializableParameter)
				{
					(obj as ISerializableParameter).Serialize(ref pkg);
				}
			}
			IL_207:;
		}
	}

	// Token: 0x06000F51 RID: 3921 RVA: 0x00066AF0 File Offset: 0x00064CF0
	public static object[] Deserialize(ZRpc rpc, ParameterInfo[] paramInfo, ZPackage pkg)
	{
		List<object> list = new List<object>();
		list.Add(rpc);
		ZRpc.Deserialize(paramInfo, pkg, ref list);
		return list.ToArray();
	}

	// Token: 0x06000F52 RID: 3922 RVA: 0x00066B1C File Offset: 0x00064D1C
	public static void Deserialize(ParameterInfo[] paramInfo, ZPackage pkg, ref List<object> parameters)
	{
		for (int i = 1; i < paramInfo.Length; i++)
		{
			ParameterInfo parameterInfo = paramInfo[i];
			if (parameterInfo.ParameterType == typeof(int))
			{
				parameters.Add(pkg.ReadInt());
			}
			else if (parameterInfo.ParameterType == typeof(uint))
			{
				parameters.Add(pkg.ReadUInt());
			}
			else if (parameterInfo.ParameterType == typeof(long))
			{
				parameters.Add(pkg.ReadLong());
			}
			else if (parameterInfo.ParameterType == typeof(float))
			{
				parameters.Add(pkg.ReadSingle());
			}
			else if (parameterInfo.ParameterType == typeof(double))
			{
				parameters.Add(pkg.ReadDouble());
			}
			else if (parameterInfo.ParameterType == typeof(bool))
			{
				parameters.Add(pkg.ReadBool());
			}
			else if (parameterInfo.ParameterType == typeof(string))
			{
				parameters.Add(pkg.ReadString());
			}
			else if (parameterInfo.ParameterType == typeof(ZPackage))
			{
				parameters.Add(pkg.ReadPackage());
			}
			else if (parameterInfo.ParameterType == typeof(List<string>))
			{
				int num = pkg.ReadInt();
				List<string> list = new List<string>(num);
				for (int j = 0; j < num; j++)
				{
					list.Add(pkg.ReadString());
				}
				parameters.Add(list);
			}
			else if (parameterInfo.ParameterType == typeof(Vector3))
			{
				Vector3 vector = new Vector3(pkg.ReadSingle(), pkg.ReadSingle(), pkg.ReadSingle());
				parameters.Add(vector);
			}
			else if (parameterInfo.ParameterType == typeof(Quaternion))
			{
				Quaternion quaternion = new Quaternion(pkg.ReadSingle(), pkg.ReadSingle(), pkg.ReadSingle(), pkg.ReadSingle());
				parameters.Add(quaternion);
			}
			else if (parameterInfo.ParameterType == typeof(ZDOID))
			{
				parameters.Add(pkg.ReadZDOID());
			}
			else if (parameterInfo.ParameterType == typeof(HitData))
			{
				HitData hitData = new HitData();
				hitData.Deserialize(ref pkg);
				parameters.Add(hitData);
			}
			else if (typeof(ISerializableParameter).IsAssignableFrom(parameterInfo.ParameterType))
			{
				ISerializableParameter serializableParameter = (ISerializableParameter)Activator.CreateInstance(parameterInfo.ParameterType);
				serializableParameter.Deserialize(ref pkg);
				parameters.Add(serializableParameter);
			}
		}
	}

	// Token: 0x06000F53 RID: 3923 RVA: 0x00066E19 File Offset: 0x00065019
	public static void SetLongTimeout(bool enable)
	{
		if (enable)
		{
			ZRpc.m_timeout = 90f;
		}
		else
		{
			ZRpc.m_timeout = 30f;
		}
		ZLog.Log(string.Format("ZRpc timeout set to {0}s ", ZRpc.m_timeout));
	}

	// Token: 0x04001096 RID: 4246
	private ISocket m_socket;

	// Token: 0x04001097 RID: 4247
	private ZPackage m_pkg = new ZPackage();

	// Token: 0x04001098 RID: 4248
	private Dictionary<int, ZRpc.RpcMethodBase> m_functions = new Dictionary<int, ZRpc.RpcMethodBase>();

	// Token: 0x04001099 RID: 4249
	private int m_sentPackages;

	// Token: 0x0400109A RID: 4250
	private int m_sentData;

	// Token: 0x0400109B RID: 4251
	private int m_recvPackages;

	// Token: 0x0400109C RID: 4252
	private int m_recvData;

	// Token: 0x0400109D RID: 4253
	private float m_pingTimer;

	// Token: 0x0400109E RID: 4254
	private float m_timeSinceLastPing;

	// Token: 0x0400109F RID: 4255
	private static float m_pingInterval = 1f;

	// Token: 0x040010A0 RID: 4256
	private static float m_timeout = 30f;

	// Token: 0x040010A1 RID: 4257
	private static bool m_DEBUG = false;

	// Token: 0x0200017D RID: 381
	public enum ErrorCode
	{
		// Token: 0x040010A3 RID: 4259
		Success,
		// Token: 0x040010A4 RID: 4260
		Disconnected,
		// Token: 0x040010A5 RID: 4261
		IncompatibleVersion
	}

	// Token: 0x0200017E RID: 382
	private interface RpcMethodBase
	{
		// Token: 0x06000F55 RID: 3925
		void Invoke(ZRpc rpc, ZPackage pkg);
	}

	// Token: 0x0200017F RID: 383
	public class RpcMethod : ZRpc.RpcMethodBase
	{
		// Token: 0x06000F56 RID: 3926 RVA: 0x00066E69 File Offset: 0x00065069
		public RpcMethod(ZRpc.RpcMethod.Method action)
		{
			this.m_action = action;
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x00066E78 File Offset: 0x00065078
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action(rpc);
		}

		// Token: 0x040010A6 RID: 4262
		private ZRpc.RpcMethod.Method m_action;

		// Token: 0x02000180 RID: 384
		// (Invoke) Token: 0x06000F59 RID: 3929
		public delegate void Method(ZRpc RPC);
	}

	// Token: 0x02000181 RID: 385
	private class RpcMethod<T> : ZRpc.RpcMethodBase
	{
		// Token: 0x06000F5C RID: 3932 RVA: 0x00066E86 File Offset: 0x00065086
		public RpcMethod(Action<ZRpc, T> action)
		{
			this.m_action = action;
		}

		// Token: 0x06000F5D RID: 3933 RVA: 0x00066E95 File Offset: 0x00065095
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x040010A7 RID: 4263
		private Action<ZRpc, T> m_action;
	}

	// Token: 0x02000182 RID: 386
	private class RpcMethod<T, U> : ZRpc.RpcMethodBase
	{
		// Token: 0x06000F5E RID: 3934 RVA: 0x00066EBA File Offset: 0x000650BA
		public RpcMethod(Action<ZRpc, T, U> action)
		{
			this.m_action = action;
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x00066EC9 File Offset: 0x000650C9
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x040010A8 RID: 4264
		private Action<ZRpc, T, U> m_action;
	}

	// Token: 0x02000183 RID: 387
	private class RpcMethod<T, U, V> : ZRpc.RpcMethodBase
	{
		// Token: 0x06000F60 RID: 3936 RVA: 0x00066EEE File Offset: 0x000650EE
		public RpcMethod(Action<ZRpc, T, U, V> action)
		{
			this.m_action = action;
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x00066EFD File Offset: 0x000650FD
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x040010A9 RID: 4265
		private Action<ZRpc, T, U, V> m_action;
	}

	// Token: 0x02000184 RID: 388
	public class RpcMethod<T, U, V, B> : ZRpc.RpcMethodBase
	{
		// Token: 0x06000F62 RID: 3938 RVA: 0x00066F22 File Offset: 0x00065122
		public RpcMethod(ZRpc.RpcMethod<T, U, V, B>.Method action)
		{
			this.m_action = action;
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x00066F31 File Offset: 0x00065131
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x040010AA RID: 4266
		private ZRpc.RpcMethod<T, U, V, B>.Method m_action;

		// Token: 0x02000185 RID: 389
		// (Invoke) Token: 0x06000F65 RID: 3941
		public delegate void Method(ZRpc RPC, T p0, U p1, V p2, B p3);
	}
}
