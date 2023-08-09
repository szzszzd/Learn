using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Token: 0x02000177 RID: 375
public class ZNetView : MonoBehaviour
{
	// Token: 0x06000ECD RID: 3789 RVA: 0x00064FDC File Offset: 0x000631DC
	private void Awake()
	{
		if (ZNetView.m_forceDisableInit || ZDOMan.instance == null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		this.m_body = base.GetComponent<Rigidbody>();
		if (ZNetView.m_useInitZDO && ZNetView.m_initZDO == null)
		{
			ZLog.LogWarning("Double ZNetview when initializing object " + base.gameObject.name);
		}
		if (ZNetView.m_initZDO != null)
		{
			this.m_zdo = ZNetView.m_initZDO;
			ZNetView.m_initZDO = null;
			if (this.m_zdo.Type != this.m_type && this.m_zdo.IsOwner())
			{
				this.m_zdo.SetType(this.m_type);
			}
			if (this.m_zdo.Distant != this.m_distant && this.m_zdo.IsOwner())
			{
				this.m_zdo.SetDistant(this.m_distant);
			}
			if (this.m_syncInitialScale)
			{
				Vector3 vec = this.m_zdo.GetVec3(ZDOVars.s_scaleHash, Vector3.zero);
				if (vec != Vector3.zero)
				{
					base.transform.localScale = vec;
				}
				else
				{
					float @float = this.m_zdo.GetFloat(ZDOVars.s_scaleScalarHash, base.transform.localScale.x);
					if (!base.transform.localScale.x.Equals(@float))
					{
						base.transform.localScale = new Vector3(@float, @float, @float);
					}
				}
			}
			if (this.m_body)
			{
				this.m_body.Sleep();
			}
		}
		else
		{
			string prefabName = this.GetPrefabName();
			this.m_zdo = ZDOMan.instance.CreateNewZDO(base.transform.position, prefabName.GetStableHashCode());
			this.m_zdo.Persistent = this.m_persistent;
			this.m_zdo.Type = this.m_type;
			this.m_zdo.Distant = this.m_distant;
			this.m_zdo.SetPrefab(prefabName.GetStableHashCode());
			this.m_zdo.SetRotation(base.transform.rotation);
			if (this.m_syncInitialScale)
			{
				this.SyncScale(true);
			}
			if (ZNetView.m_ghostInit)
			{
				this.m_ghost = true;
				return;
			}
		}
		ZNetScene.instance.AddInstance(this.m_zdo, this);
	}

	// Token: 0x06000ECE RID: 3790 RVA: 0x0006520C File Offset: 0x0006340C
	public void SetLocalScale(Vector3 scale)
	{
		if (base.transform.localScale == scale)
		{
			return;
		}
		base.transform.localScale = scale;
		if (this.m_zdo != null && this.m_syncInitialScale && this.IsOwner())
		{
			this.SyncScale(false);
		}
	}

	// Token: 0x06000ECF RID: 3791 RVA: 0x00065258 File Offset: 0x00063458
	private void SyncScale(bool skipOne = false)
	{
		if (!Mathf.Approximately(base.transform.localScale.x, base.transform.localScale.y) || !Mathf.Approximately(base.transform.localScale.x, base.transform.localScale.z))
		{
			this.m_zdo.Set(ZDOVars.s_scaleHash, base.transform.localScale);
			return;
		}
		if (skipOne && Mathf.Approximately(base.transform.localScale.x, 1f))
		{
			return;
		}
		this.m_zdo.Set(ZDOVars.s_scaleScalarHash, base.transform.localScale.x);
	}

	// Token: 0x06000ED0 RID: 3792 RVA: 0x0006530F File Offset: 0x0006350F
	private void OnDestroy()
	{
		ZNetScene.instance;
	}

	// Token: 0x06000ED1 RID: 3793 RVA: 0x0006531C File Offset: 0x0006351C
	private string GetPrefabName()
	{
		return Utils.GetPrefabName(base.gameObject);
	}

	// Token: 0x06000ED2 RID: 3794 RVA: 0x00065329 File Offset: 0x00063529
	public void Destroy()
	{
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x06000ED3 RID: 3795 RVA: 0x0006533B File Offset: 0x0006353B
	public bool IsOwner()
	{
		return this.IsValid() && this.m_zdo.IsOwner();
	}

	// Token: 0x06000ED4 RID: 3796 RVA: 0x00065352 File Offset: 0x00063552
	public bool HasOwner()
	{
		return this.IsValid() && this.m_zdo.HasOwner();
	}

	// Token: 0x06000ED5 RID: 3797 RVA: 0x00065369 File Offset: 0x00063569
	public void ClaimOwnership()
	{
		if (this.IsOwner())
		{
			return;
		}
		this.m_zdo.SetOwner(ZDOMan.GetSessionID());
	}

	// Token: 0x06000ED6 RID: 3798 RVA: 0x00065384 File Offset: 0x00063584
	public ZDO GetZDO()
	{
		return this.m_zdo;
	}

	// Token: 0x06000ED7 RID: 3799 RVA: 0x0006538C File Offset: 0x0006358C
	public bool IsValid()
	{
		return this.m_zdo != null && this.m_zdo.IsValid();
	}

	// Token: 0x06000ED8 RID: 3800 RVA: 0x000653A3 File Offset: 0x000635A3
	public void ResetZDO()
	{
		this.m_zdo = null;
	}

	// Token: 0x06000ED9 RID: 3801 RVA: 0x000653AC File Offset: 0x000635AC
	public void Register(string name, Action<long> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod(f));
	}

	// Token: 0x06000EDA RID: 3802 RVA: 0x000653C5 File Offset: 0x000635C5
	public void Register<T>(string name, Action<long, T> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T>(f));
	}

	// Token: 0x06000EDB RID: 3803 RVA: 0x000653DE File Offset: 0x000635DE
	public void Register<T, U>(string name, Action<long, T, U> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U>(f));
	}

	// Token: 0x06000EDC RID: 3804 RVA: 0x000653F7 File Offset: 0x000635F7
	public void Register<T, U, V>(string name, Action<long, T, U, V> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V>(f));
	}

	// Token: 0x06000EDD RID: 3805 RVA: 0x00065410 File Offset: 0x00063610
	public void Register<T, U, V, B>(string name, RoutedMethod<T, U, V, B>.Method f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V, B>(f));
	}

	// Token: 0x06000EDE RID: 3806 RVA: 0x0006542C File Offset: 0x0006362C
	public void Unregister(string name)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
	}

	// Token: 0x06000EDF RID: 3807 RVA: 0x00065450 File Offset: 0x00063650
	public void HandleRoutedRPC(ZRoutedRpc.RoutedRPCData rpcData)
	{
		RoutedMethodBase routedMethodBase;
		if (this.m_functions.TryGetValue(rpcData.m_methodHash, out routedMethodBase))
		{
			routedMethodBase.Invoke(rpcData.m_senderPeerID, rpcData.m_parameters);
			return;
		}
		ZLog.LogWarning("Failed to find rpc method " + rpcData.m_methodHash.ToString());
	}

	// Token: 0x06000EE0 RID: 3808 RVA: 0x0006549F File Offset: 0x0006369F
	public void InvokeRPC(long targetID, string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(targetID, this.m_zdo.m_uid, method, parameters);
	}

	// Token: 0x06000EE1 RID: 3809 RVA: 0x000654B9 File Offset: 0x000636B9
	public void InvokeRPC(string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(this.m_zdo.GetOwner(), this.m_zdo.m_uid, method, parameters);
	}

	// Token: 0x06000EE2 RID: 3810 RVA: 0x000654E0 File Offset: 0x000636E0
	public static object[] Deserialize(long callerID, ParameterInfo[] paramInfo, ZPackage pkg)
	{
		List<object> list = new List<object>();
		list.Add(callerID);
		ZRpc.Deserialize(paramInfo, pkg, ref list);
		return list.ToArray();
	}

	// Token: 0x06000EE3 RID: 3811 RVA: 0x0006550E File Offset: 0x0006370E
	public static void StartGhostInit()
	{
		ZNetView.m_ghostInit = true;
	}

	// Token: 0x06000EE4 RID: 3812 RVA: 0x00065516 File Offset: 0x00063716
	public static void FinishGhostInit()
	{
		ZNetView.m_ghostInit = false;
	}

	// Token: 0x04001072 RID: 4210
	public static long Everybody;

	// Token: 0x04001073 RID: 4211
	public bool m_persistent;

	// Token: 0x04001074 RID: 4212
	public bool m_distant;

	// Token: 0x04001075 RID: 4213
	public ZDO.ObjectType m_type;

	// Token: 0x04001076 RID: 4214
	public bool m_syncInitialScale;

	// Token: 0x04001077 RID: 4215
	public static bool m_useInitZDO;

	// Token: 0x04001078 RID: 4216
	public static ZDO m_initZDO;

	// Token: 0x04001079 RID: 4217
	public static bool m_forceDisableInit;

	// Token: 0x0400107A RID: 4218
	private ZDO m_zdo;

	// Token: 0x0400107B RID: 4219
	private Rigidbody m_body;

	// Token: 0x0400107C RID: 4220
	private Dictionary<int, RoutedMethodBase> m_functions = new Dictionary<int, RoutedMethodBase>();

	// Token: 0x0400107D RID: 4221
	private bool m_ghost;

	// Token: 0x0400107E RID: 4222
	private static bool m_ghostInit;
}
