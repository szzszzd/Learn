﻿using System;
using System.Collections.Generic;
using System.Linq;

// Token: 0x0200015A RID: 346
public static class ZDOHelper
{
	// Token: 0x06000D79 RID: 3449 RVA: 0x0005C9E0 File Offset: 0x0005ABE0
	public static string ToStringFast(this ZDOExtraData.ConnectionType value)
	{
		switch (value & ~ZDOExtraData.ConnectionType.Target)
		{
		case ZDOExtraData.ConnectionType.Portal:
			return "Portal";
		case ZDOExtraData.ConnectionType.SyncTransform:
			return "SyncTransform";
		case ZDOExtraData.ConnectionType.Spawned:
			return "Spawned";
		default:
			return value.ToString();
		}
	}

	// Token: 0x06000D7A RID: 3450 RVA: 0x0005CA2A File Offset: 0x0005AC2A
	public static TValue GetValueOrDefaultPiktiv<TKey, TValue>(this IDictionary<TKey, TValue> container, TKey zid, TValue defaultValue)
	{
		if (!container.ContainsKey(zid))
		{
			return defaultValue;
		}
		return container[zid];
	}

	// Token: 0x06000D7B RID: 3451 RVA: 0x0005CA3E File Offset: 0x0005AC3E
	public static bool InitAndSet<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, TType value)
	{
		container.Init(zid);
		return container[zid].SetValue(hash, value);
	}

	// Token: 0x06000D7C RID: 3452 RVA: 0x0005CA55 File Offset: 0x0005AC55
	public static bool Update<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, TType value)
	{
		return container[zid].SetValue(hash, value);
	}

	// Token: 0x06000D7D RID: 3453 RVA: 0x0005CA65 File Offset: 0x0005AC65
	public static void InitAndReserve<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int size)
	{
		container.Init(zid);
		container[zid].Reserve(size);
	}

	// Token: 0x06000D7E RID: 3454 RVA: 0x0005CA7C File Offset: 0x0005AC7C
	public static List<ZDOID> GetAllZDOIDsWithHash<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, int hash)
	{
		List<ZDOID> list = new List<ZDOID>();
		foreach (KeyValuePair<ZDOID, BinarySearchDictionary<int, TType>> keyValuePair in container)
		{
			foreach (KeyValuePair<int, TType> keyValuePair2 in keyValuePair.Value)
			{
				if (keyValuePair2.Key == hash)
				{
					list.Add(keyValuePair.Key);
					break;
				}
			}
		}
		return list;
	}

	// Token: 0x06000D7F RID: 3455 RVA: 0x0005CB1C File Offset: 0x0005AD1C
	public static List<KeyValuePair<int, TType>> GetValuesOrEmpty<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid)
	{
		if (!container.ContainsKey(zid))
		{
			return Array.Empty<KeyValuePair<int, TType>>().ToList<KeyValuePair<int, TType>>();
		}
		return container[zid].ToList<KeyValuePair<int, TType>>();
	}

	// Token: 0x06000D80 RID: 3456 RVA: 0x0005CB3E File Offset: 0x0005AD3E
	public static TType GetValueOrDefault<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, TType defaultValue)
	{
		if (!container.ContainsKey(zid))
		{
			return defaultValue;
		}
		return container[zid].GetValueOrDefault(hash, defaultValue);
	}

	// Token: 0x06000D81 RID: 3457 RVA: 0x0005CB59 File Offset: 0x0005AD59
	public static void Release<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid)
	{
		if (!container.ContainsKey(zid))
		{
			return;
		}
		container[zid].Clear();
		Pool<BinarySearchDictionary<int, TType>>.Release(container[zid]);
		container[zid] = null;
		container.Remove(zid);
	}

	// Token: 0x06000D82 RID: 3458 RVA: 0x0005CB8D File Offset: 0x0005AD8D
	private static void Init<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid)
	{
		if (!container.ContainsKey(zid))
		{
			container.Add(zid, Pool<BinarySearchDictionary<int, TType>>.Create());
		}
	}

	// Token: 0x06000D83 RID: 3459 RVA: 0x0005CBA4 File Offset: 0x0005ADA4
	public static bool Remove<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID id, int hash)
	{
		if (!container.ContainsKey(id) || !container[id].ContainsKey(hash))
		{
			return false;
		}
		container[id].Remove(hash);
		if (container[id].Count == 0)
		{
			Pool<BinarySearchDictionary<int, TType>>.Release(container[id]);
			container[id] = null;
			container.Remove(id);
		}
		return true;
	}

	// Token: 0x06000D84 RID: 3460 RVA: 0x0005CC04 File Offset: 0x0005AE04
	public static Dictionary<ZDOID, BinarySearchDictionary<int, TType>> Clone<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container)
	{
		return container.ToDictionary((KeyValuePair<ZDOID, BinarySearchDictionary<int, TType>> entry) => entry.Key, (KeyValuePair<ZDOID, BinarySearchDictionary<int, TType>> entry) => (BinarySearchDictionary<int, TType>)entry.Value.Clone());
	}

	// Token: 0x06000D85 RID: 3461 RVA: 0x0005CC58 File Offset: 0x0005AE58
	public static Dictionary<ZDOID, ZDOConnectionHashData> Clone(this Dictionary<ZDOID, ZDOConnectionHashData> container)
	{
		return container.ToDictionary((KeyValuePair<ZDOID, ZDOConnectionHashData> entry) => entry.Key, (KeyValuePair<ZDOID, ZDOConnectionHashData> entry) => entry.Value);
	}

	// Token: 0x04000F24 RID: 3876
	public static readonly List<int> s_stripOldData = new List<int>
	{
		"generated".GetStableHashCode(),
		"patrolSpawnPoint".GetStableHashCode(),
		"autoDespawn".GetStableHashCode(),
		"targetHear".GetStableHashCode(),
		"targetSee".GetStableHashCode(),
		"burnt0".GetStableHashCode(),
		"burnt1".GetStableHashCode(),
		"burnt2".GetStableHashCode(),
		"burnt3".GetStableHashCode(),
		"burnt4".GetStableHashCode(),
		"burnt5".GetStableHashCode(),
		"burnt6".GetStableHashCode(),
		"burnt7".GetStableHashCode(),
		"burnt8".GetStableHashCode(),
		"burnt9".GetStableHashCode(),
		"burnt10".GetStableHashCode(),
		"LookDir".GetStableHashCode(),
		"RideSpeed".GetStableHashCode()
	};

	// Token: 0x04000F25 RID: 3877
	public static readonly List<int> s_stripOldLongData = new List<int>
	{
		ZDOVars.s_zdoidUser.Key,
		ZDOVars.s_zdoidUser.Value,
		ZDOVars.s_zdoidRodOwner.Key,
		ZDOVars.s_zdoidRodOwner.Value,
		ZDOVars.s_sessionCatchID.Key,
		ZDOVars.s_sessionCatchID.Value
	};

	// Token: 0x04000F26 RID: 3878
	public static readonly List<int> s_stripOldDataByteArray = new List<int>
	{
		"health".GetStableHashCode()
	};
}
