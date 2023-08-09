using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000155 RID: 341
public static class ZDOExtraData
{
	// Token: 0x06000D2B RID: 3371 RVA: 0x0005BE58 File Offset: 0x0005A058
	public static void Reset()
	{
		ZDOExtraData.s_floats.Clear();
		ZDOExtraData.s_vec3.Clear();
		ZDOExtraData.s_quats.Clear();
		ZDOExtraData.s_ints.Clear();
		ZDOExtraData.s_longs.Clear();
		ZDOExtraData.s_strings.Clear();
		ZDOExtraData.s_byteArrays.Clear();
		ZDOExtraData.s_connections.Clear();
		ZDOExtraData.s_owner.Clear();
		ZDOExtraData.s_tempTimeCreated.Clear();
	}

	// Token: 0x06000D2C RID: 3372 RVA: 0x0005BECC File Offset: 0x0005A0CC
	public static void Reserve(ZDOID zid, ZDOExtraData.Type type, int size)
	{
		switch (type)
		{
		case ZDOExtraData.Type.Float:
			ZDOExtraData.s_floats.InitAndReserve(zid, size);
			return;
		case ZDOExtraData.Type.Vec3:
			ZDOExtraData.s_vec3.InitAndReserve(zid, size);
			return;
		case ZDOExtraData.Type.Quat:
			ZDOExtraData.s_quats.InitAndReserve(zid, size);
			return;
		case ZDOExtraData.Type.Int:
			ZDOExtraData.s_ints.InitAndReserve(zid, size);
			return;
		case ZDOExtraData.Type.Long:
			ZDOExtraData.s_longs.InitAndReserve(zid, size);
			return;
		case ZDOExtraData.Type.String:
			ZDOExtraData.s_strings.InitAndReserve(zid, size);
			return;
		case ZDOExtraData.Type.ByteArray:
			ZDOExtraData.s_byteArrays.InitAndReserve(zid, size);
			return;
		default:
			return;
		}
	}

	// Token: 0x06000D2D RID: 3373 RVA: 0x0005BF56 File Offset: 0x0005A156
	public static void Add(ZDOID zid, int hash, float value)
	{
		ZDOExtraData.s_floats[zid][hash] = value;
	}

	// Token: 0x06000D2E RID: 3374 RVA: 0x0005BF6A File Offset: 0x0005A16A
	public static void Add(ZDOID zid, int hash, string value)
	{
		ZDOExtraData.s_strings[zid][hash] = value;
	}

	// Token: 0x06000D2F RID: 3375 RVA: 0x0005BF7E File Offset: 0x0005A17E
	public static void Add(ZDOID zid, int hash, Vector3 value)
	{
		ZDOExtraData.s_vec3[zid][hash] = value;
	}

	// Token: 0x06000D30 RID: 3376 RVA: 0x0005BF92 File Offset: 0x0005A192
	public static void Add(ZDOID zid, int hash, Quaternion value)
	{
		ZDOExtraData.s_quats[zid][hash] = value;
	}

	// Token: 0x06000D31 RID: 3377 RVA: 0x0005BFA6 File Offset: 0x0005A1A6
	public static void Add(ZDOID zid, int hash, int value)
	{
		ZDOExtraData.s_ints[zid][hash] = value;
	}

	// Token: 0x06000D32 RID: 3378 RVA: 0x0005BFBA File Offset: 0x0005A1BA
	public static void Add(ZDOID zid, int hash, long value)
	{
		ZDOExtraData.s_longs[zid][hash] = value;
	}

	// Token: 0x06000D33 RID: 3379 RVA: 0x0005BFCE File Offset: 0x0005A1CE
	public static void Add(ZDOID zid, int hash, byte[] value)
	{
		ZDOExtraData.s_byteArrays[zid][hash] = value;
	}

	// Token: 0x06000D34 RID: 3380 RVA: 0x0005BFE2 File Offset: 0x0005A1E2
	public static bool Set(ZDOID zid, int hash, float value)
	{
		return ZDOExtraData.s_floats.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D35 RID: 3381 RVA: 0x0005BFF1 File Offset: 0x0005A1F1
	public static bool Set(ZDOID zid, int hash, string value)
	{
		return ZDOExtraData.s_strings.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D36 RID: 3382 RVA: 0x0005C000 File Offset: 0x0005A200
	public static bool Set(ZDOID zid, int hash, Vector3 value)
	{
		return ZDOExtraData.s_vec3.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D37 RID: 3383 RVA: 0x0005C00F File Offset: 0x0005A20F
	public static bool Update(ZDOID zid, int hash, Vector3 value)
	{
		return ZDOExtraData.s_vec3.Update(zid, hash, value);
	}

	// Token: 0x06000D38 RID: 3384 RVA: 0x0005C01E File Offset: 0x0005A21E
	public static bool Set(ZDOID zid, int hash, Quaternion value)
	{
		return ZDOExtraData.s_quats.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D39 RID: 3385 RVA: 0x0005C02D File Offset: 0x0005A22D
	public static bool Set(ZDOID zid, int hash, int value)
	{
		return ZDOExtraData.s_ints.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D3A RID: 3386 RVA: 0x0005C03C File Offset: 0x0005A23C
	public static bool Set(ZDOID zid, int hash, long value)
	{
		return ZDOExtraData.s_longs.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D3B RID: 3387 RVA: 0x0005C04B File Offset: 0x0005A24B
	public static bool Set(ZDOID zid, int hash, byte[] value)
	{
		return ZDOExtraData.s_byteArrays.InitAndSet(zid, hash, value);
	}

	// Token: 0x06000D3C RID: 3388 RVA: 0x0005C05C File Offset: 0x0005A25C
	public static bool SetConnection(ZDOID zid, ZDOExtraData.ConnectionType connectionType, ZDOID target)
	{
		ZDOConnection zdoconnection = new ZDOConnection(connectionType, target);
		ZDOConnection zdoconnection2;
		if (ZDOExtraData.s_connections.TryGetValue(zid, out zdoconnection2) && zdoconnection2.m_type == zdoconnection.m_type && zdoconnection2.m_target == zdoconnection.m_target)
		{
			return false;
		}
		ZDOExtraData.s_connections[zid] = zdoconnection;
		return true;
	}

	// Token: 0x06000D3D RID: 3389 RVA: 0x0005C0B0 File Offset: 0x0005A2B0
	public static bool UpdateConnection(ZDOID zid, ZDOExtraData.ConnectionType connectionType, ZDOID target)
	{
		ZDOConnection zdoconnection = new ZDOConnection(connectionType, target);
		ZDOConnection zdoconnection2;
		if (!ZDOExtraData.s_connections.TryGetValue(zid, out zdoconnection2))
		{
			return false;
		}
		if (zdoconnection2.m_type == zdoconnection.m_type && zdoconnection2.m_target == zdoconnection.m_target)
		{
			return false;
		}
		ZDOExtraData.s_connections[zid] = zdoconnection;
		return true;
	}

	// Token: 0x06000D3E RID: 3390 RVA: 0x0005C108 File Offset: 0x0005A308
	public static void SetConnectionData(ZDOID zid, ZDOExtraData.ConnectionType connectionType, int hash)
	{
		ZDOConnectionHashData value = new ZDOConnectionHashData(connectionType, hash);
		ZDOExtraData.s_connectionsHashData[zid] = value;
	}

	// Token: 0x06000D3F RID: 3391 RVA: 0x0005C129 File Offset: 0x0005A329
	public static void SetOwner(ZDOID zid, ushort ownerKey)
	{
		if (!ZDOExtraData.s_owner.ContainsKey(zid))
		{
			ZDOExtraData.s_owner.Add(zid, ownerKey);
			return;
		}
		if (ownerKey != 0)
		{
			ZDOExtraData.s_owner[zid] = ownerKey;
			return;
		}
		ZDOExtraData.s_owner.Remove(zid);
	}

	// Token: 0x06000D40 RID: 3392 RVA: 0x0005C161 File Offset: 0x0005A361
	public static long GetOwner(ZDOID zid)
	{
		if (!ZDOExtraData.s_owner.ContainsKey(zid))
		{
			return 0L;
		}
		return ZDOID.GetUserID(ZDOExtraData.s_owner[zid]);
	}

	// Token: 0x06000D41 RID: 3393 RVA: 0x0005C183 File Offset: 0x0005A383
	public static float GetFloat(ZDOID zid, int hash, float defaultValue = 0f)
	{
		return ZDOExtraData.s_floats.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D42 RID: 3394 RVA: 0x0005C192 File Offset: 0x0005A392
	public static Vector3 GetVec3(ZDOID zid, int hash, Vector3 defaultValue)
	{
		return ZDOExtraData.s_vec3.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D43 RID: 3395 RVA: 0x0005C1A1 File Offset: 0x0005A3A1
	public static Quaternion GetQuaternion(ZDOID zid, int hash, Quaternion defaultValue)
	{
		return ZDOExtraData.s_quats.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D44 RID: 3396 RVA: 0x0005C1B0 File Offset: 0x0005A3B0
	public static int GetInt(ZDOID zid, int hash, int defaultValue = 0)
	{
		return ZDOExtraData.s_ints.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D45 RID: 3397 RVA: 0x0005C1BF File Offset: 0x0005A3BF
	public static long GetLong(ZDOID zid, int hash, long defaultValue = 0L)
	{
		return ZDOExtraData.s_longs.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D46 RID: 3398 RVA: 0x0005C1CE File Offset: 0x0005A3CE
	public static string GetString(ZDOID zid, int hash, string defaultValue = "")
	{
		return ZDOExtraData.s_strings.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D47 RID: 3399 RVA: 0x0005C1DD File Offset: 0x0005A3DD
	public static byte[] GetByteArray(ZDOID zid, int hash, byte[] defaultValue = null)
	{
		return ZDOExtraData.s_byteArrays.GetValueOrDefault(zid, hash, defaultValue);
	}

	// Token: 0x06000D48 RID: 3400 RVA: 0x0005C1EC File Offset: 0x0005A3EC
	public static ZDOConnection GetConnection(ZDOID zid)
	{
		return ZDOExtraData.s_connections.GetValueOrDefaultPiktiv(zid, null);
	}

	// Token: 0x06000D49 RID: 3401 RVA: 0x0005C1FC File Offset: 0x0005A3FC
	public static ZDOID GetConnectionZDOID(ZDOID zid, ZDOExtraData.ConnectionType type)
	{
		ZDOConnection valueOrDefaultPiktiv = ZDOExtraData.s_connections.GetValueOrDefaultPiktiv(zid, null);
		if (valueOrDefaultPiktiv != null && valueOrDefaultPiktiv.m_type == type)
		{
			return valueOrDefaultPiktiv.m_target;
		}
		return ZDOID.None;
	}

	// Token: 0x06000D4A RID: 3402 RVA: 0x0005C22E File Offset: 0x0005A42E
	public static ZDOExtraData.ConnectionType GetConnectionType(ZDOID zid)
	{
		ZDOConnection valueOrDefaultPiktiv = ZDOExtraData.s_connections.GetValueOrDefaultPiktiv(zid, null);
		if (valueOrDefaultPiktiv == null)
		{
			return ZDOExtraData.ConnectionType.None;
		}
		return valueOrDefaultPiktiv.m_type;
	}

	// Token: 0x06000D4B RID: 3403 RVA: 0x0005C247 File Offset: 0x0005A447
	public static List<KeyValuePair<int, float>> GetFloats(ZDOID zid)
	{
		return ZDOExtraData.s_floats.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D4C RID: 3404 RVA: 0x0005C254 File Offset: 0x0005A454
	public static List<KeyValuePair<int, Vector3>> GetVec3s(ZDOID zid)
	{
		return ZDOExtraData.s_vec3.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D4D RID: 3405 RVA: 0x0005C261 File Offset: 0x0005A461
	public static List<KeyValuePair<int, Quaternion>> GetQuaternions(ZDOID zid)
	{
		return ZDOExtraData.s_quats.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D4E RID: 3406 RVA: 0x0005C26E File Offset: 0x0005A46E
	public static List<KeyValuePair<int, int>> GetInts(ZDOID zid)
	{
		return ZDOExtraData.s_ints.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D4F RID: 3407 RVA: 0x0005C27B File Offset: 0x0005A47B
	public static List<KeyValuePair<int, long>> GetLongs(ZDOID zid)
	{
		return ZDOExtraData.s_longs.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D50 RID: 3408 RVA: 0x0005C288 File Offset: 0x0005A488
	public static List<KeyValuePair<int, string>> GetStrings(ZDOID zid)
	{
		return ZDOExtraData.s_strings.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D51 RID: 3409 RVA: 0x0005C295 File Offset: 0x0005A495
	public static List<KeyValuePair<int, byte[]>> GetByteArrays(ZDOID zid)
	{
		return ZDOExtraData.s_byteArrays.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D52 RID: 3410 RVA: 0x0005C2A2 File Offset: 0x0005A4A2
	public static bool RemoveFloat(ZDOID zid, int hash)
	{
		return ZDOExtraData.s_floats.Remove(zid, hash);
	}

	// Token: 0x06000D53 RID: 3411 RVA: 0x0005C2B0 File Offset: 0x0005A4B0
	public static bool RemoveInt(ZDOID zid, int hash)
	{
		return ZDOExtraData.s_ints.Remove(zid, hash);
	}

	// Token: 0x06000D54 RID: 3412 RVA: 0x0005C2BE File Offset: 0x0005A4BE
	public static bool RemoveLong(ZDOID zid, int hash)
	{
		return ZDOExtraData.s_longs.Remove(zid, hash);
	}

	// Token: 0x06000D55 RID: 3413 RVA: 0x0005C2CC File Offset: 0x0005A4CC
	public static bool RemoveVec3(ZDOID zid, int hash)
	{
		return ZDOExtraData.s_vec3.Remove(zid, hash);
	}

	// Token: 0x06000D56 RID: 3414 RVA: 0x0005C2DA File Offset: 0x0005A4DA
	public static void RemoveIfEmpty(ZDOID id)
	{
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.Float);
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.Vec3);
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.Quat);
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.Int);
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.Long);
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.String);
		ZDOExtraData.RemoveIfEmpty(id, ZDOExtraData.Type.ByteArray);
	}

	// Token: 0x06000D57 RID: 3415 RVA: 0x0005C310 File Offset: 0x0005A510
	public static void RemoveIfEmpty(ZDOID id, ZDOExtraData.Type type)
	{
		switch (type)
		{
		case ZDOExtraData.Type.Float:
			if (ZDOExtraData.s_floats.ContainsKey(id) && ZDOExtraData.s_floats[id].Count == 0)
			{
				ZDOExtraData.ReleaseFloats(id);
				return;
			}
			break;
		case ZDOExtraData.Type.Vec3:
			if (ZDOExtraData.s_vec3.ContainsKey(id) && ZDOExtraData.s_vec3[id].Count == 0)
			{
				ZDOExtraData.ReleaseVec3(id);
				return;
			}
			break;
		case ZDOExtraData.Type.Quat:
			if (ZDOExtraData.s_quats.ContainsKey(id) && ZDOExtraData.s_quats[id].Count == 0)
			{
				ZDOExtraData.ReleaseQuats(id);
				return;
			}
			break;
		case ZDOExtraData.Type.Int:
			if (ZDOExtraData.s_ints.ContainsKey(id) && ZDOExtraData.s_ints[id].Count == 0)
			{
				ZDOExtraData.ReleaseInts(id);
				return;
			}
			break;
		case ZDOExtraData.Type.Long:
			if (ZDOExtraData.s_longs.ContainsKey(id) && ZDOExtraData.s_longs[id].Count == 0)
			{
				ZDOExtraData.ReleaseLongs(id);
				return;
			}
			break;
		case ZDOExtraData.Type.String:
			if (ZDOExtraData.s_strings.ContainsKey(id) && ZDOExtraData.s_strings[id].Count == 0)
			{
				ZDOExtraData.ReleaseStrings(id);
				return;
			}
			break;
		case ZDOExtraData.Type.ByteArray:
			if (ZDOExtraData.s_byteArrays.ContainsKey(id) && ZDOExtraData.s_byteArrays[id].Count == 0)
			{
				ZDOExtraData.ReleaseByteArrays(id);
			}
			break;
		default:
			return;
		}
	}

	// Token: 0x06000D58 RID: 3416 RVA: 0x0005C45E File Offset: 0x0005A65E
	public static void Release(ZDO zdo, ZDOID zid)
	{
		ZDOExtraData.ReleaseFloats(zid);
		ZDOExtraData.ReleaseVec3(zid);
		ZDOExtraData.ReleaseQuats(zid);
		ZDOExtraData.ReleaseInts(zid);
		ZDOExtraData.ReleaseLongs(zid);
		ZDOExtraData.ReleaseStrings(zid);
		ZDOExtraData.ReleaseByteArrays(zid);
		ZDOExtraData.ReleaseOwner(zid);
		ZDOExtraData.ReleaseConnection(zid);
	}

	// Token: 0x06000D59 RID: 3417 RVA: 0x0005C496 File Offset: 0x0005A696
	private static void ReleaseFloats(ZDOID zid)
	{
		ZDOExtraData.s_floats.Release(zid);
	}

	// Token: 0x06000D5A RID: 3418 RVA: 0x0005C4A3 File Offset: 0x0005A6A3
	private static void ReleaseVec3(ZDOID zid)
	{
		ZDOExtraData.s_vec3.Release(zid);
	}

	// Token: 0x06000D5B RID: 3419 RVA: 0x0005C4B0 File Offset: 0x0005A6B0
	private static void ReleaseQuats(ZDOID zid)
	{
		ZDOExtraData.s_quats.Release(zid);
	}

	// Token: 0x06000D5C RID: 3420 RVA: 0x0005C4BD File Offset: 0x0005A6BD
	private static void ReleaseInts(ZDOID zid)
	{
		ZDOExtraData.s_ints.Release(zid);
	}

	// Token: 0x06000D5D RID: 3421 RVA: 0x0005C4CA File Offset: 0x0005A6CA
	private static void ReleaseLongs(ZDOID zid)
	{
		ZDOExtraData.s_longs.Release(zid);
	}

	// Token: 0x06000D5E RID: 3422 RVA: 0x0005C4D7 File Offset: 0x0005A6D7
	private static void ReleaseStrings(ZDOID zid)
	{
		ZDOExtraData.s_strings.Release(zid);
	}

	// Token: 0x06000D5F RID: 3423 RVA: 0x0005C4E4 File Offset: 0x0005A6E4
	private static void ReleaseByteArrays(ZDOID zid)
	{
		ZDOExtraData.s_byteArrays.Release(zid);
	}

	// Token: 0x06000D60 RID: 3424 RVA: 0x0005C4F1 File Offset: 0x0005A6F1
	public static void ReleaseOwner(ZDOID zid)
	{
		ZDOExtraData.s_owner.Remove(zid);
	}

	// Token: 0x06000D61 RID: 3425 RVA: 0x0005C4FF File Offset: 0x0005A6FF
	private static void ReleaseConnection(ZDOID zid)
	{
		ZDOExtraData.s_connections.Remove(zid);
	}

	// Token: 0x06000D62 RID: 3426 RVA: 0x0005C50D File Offset: 0x0005A70D
	public static void SetTimeCreated(ZDOID zid, long timeCreated)
	{
		ZDOExtraData.s_tempTimeCreated.Add(zid, timeCreated);
	}

	// Token: 0x06000D63 RID: 3427 RVA: 0x0005C51C File Offset: 0x0005A71C
	public static long GetTimeCreated(ZDOID zid)
	{
		long result;
		if (ZDOExtraData.s_tempTimeCreated.TryGetValue(zid, out result))
		{
			return result;
		}
		return 0L;
	}

	// Token: 0x06000D64 RID: 3428 RVA: 0x0005C53C File Offset: 0x0005A73C
	public static void ClearTimeCreated()
	{
		ZDOExtraData.s_tempTimeCreated.Clear();
	}

	// Token: 0x06000D65 RID: 3429 RVA: 0x0005C548 File Offset: 0x0005A748
	public static bool HasTimeCreated()
	{
		return ZDOExtraData.s_tempTimeCreated.Count != 0;
	}

	// Token: 0x06000D66 RID: 3430 RVA: 0x0005C557 File Offset: 0x0005A757
	public static List<ZDOID> GetAllZDOIDsWithHash(ZDOExtraData.Type type, int hash)
	{
		if (type == ZDOExtraData.Type.Long)
		{
			return ZDOExtraData.s_longs.GetAllZDOIDsWithHash(hash);
		}
		if (type == ZDOExtraData.Type.Int)
		{
			return ZDOExtraData.s_ints.GetAllZDOIDsWithHash(hash);
		}
		Debug.LogError("This type isn't supported, yet.");
		return Array.Empty<ZDOID>().ToList<ZDOID>();
	}

	// Token: 0x06000D67 RID: 3431 RVA: 0x0005C58D File Offset: 0x0005A78D
	public static List<ZDOID> GetAllConnectionZDOIDs()
	{
		return ZDOExtraData.s_connections.Keys.ToList<ZDOID>();
	}

	// Token: 0x06000D68 RID: 3432 RVA: 0x0005C5A0 File Offset: 0x0005A7A0
	public static List<ZDOID> GetAllConnectionZDOIDs(ZDOExtraData.ConnectionType connectionType)
	{
		List<ZDOID> list = new List<ZDOID>();
		foreach (KeyValuePair<ZDOID, ZDOConnectionHashData> keyValuePair in ZDOExtraData.s_connectionsHashData)
		{
			if (keyValuePair.Value.m_type == connectionType)
			{
				list.Add(keyValuePair.Key);
			}
		}
		return list;
	}

	// Token: 0x06000D69 RID: 3433 RVA: 0x0005C610 File Offset: 0x0005A810
	public static ZDOConnectionHashData GetConnectionHashData(ZDOID zid, ZDOExtraData.ConnectionType type)
	{
		ZDOConnectionHashData valueOrDefaultPiktiv = ZDOExtraData.s_connectionsHashData.GetValueOrDefaultPiktiv(zid, null);
		if (valueOrDefaultPiktiv != null && valueOrDefaultPiktiv.m_type == type)
		{
			return valueOrDefaultPiktiv;
		}
		return null;
	}

	// Token: 0x06000D6A RID: 3434 RVA: 0x0005C63C File Offset: 0x0005A83C
	private static int GetUniqueHash(string name)
	{
		int num = ZDOMan.GetSessionID().GetHashCode() + ZDOExtraData.s_uniqueHashes;
		int num2 = 0;
		int num3;
		do
		{
			num2++;
			num3 = (num ^ (name + "_" + num2.ToString()).GetHashCode());
		}
		while (ZDOExtraData.s_usedHashes.Contains(num3));
		ZDOExtraData.s_usedHashes.Add(num3);
		ZDOExtraData.s_uniqueHashes++;
		return num3;
	}

	// Token: 0x06000D6B RID: 3435 RVA: 0x0005C6A4 File Offset: 0x0005A8A4
	private static void RegenerateConnectionHashData()
	{
		ZDOExtraData.s_usedHashes.Clear();
		ZDOExtraData.s_connectionsHashData.Clear();
		foreach (KeyValuePair<ZDOID, ZDOConnection> keyValuePair in ZDOExtraData.s_connections)
		{
			ZDOExtraData.ConnectionType type = keyValuePair.Value.m_type;
			if (type != ZDOExtraData.ConnectionType.None && (!(keyValuePair.Key == ZDOID.None) || type == ZDOExtraData.ConnectionType.Spawned) && ZDOMan.instance.GetZDO(keyValuePair.Key) != null && (ZDOMan.instance.GetZDO(keyValuePair.Value.m_target) != null || type == ZDOExtraData.ConnectionType.Spawned))
			{
				int uniqueHash = ZDOExtraData.GetUniqueHash(type.ToStringFast());
				ZDOExtraData.s_connectionsHashData[keyValuePair.Key] = new ZDOConnectionHashData(type, uniqueHash);
				if (keyValuePair.Value.m_target != ZDOID.None)
				{
					ZDOExtraData.s_connectionsHashData[keyValuePair.Value.m_target] = new ZDOConnectionHashData(type | ZDOExtraData.ConnectionType.Target, uniqueHash);
				}
			}
		}
	}

	// Token: 0x06000D6C RID: 3436 RVA: 0x0005C7C4 File Offset: 0x0005A9C4
	public static void PrepareSave()
	{
		ZDOExtraData.RegenerateConnectionHashData();
		ZDOExtraData.s_saveFloats = ZDOExtraData.s_floats.Clone<float>();
		ZDOExtraData.s_saveVec3s = ZDOExtraData.s_vec3.Clone<Vector3>();
		ZDOExtraData.s_saveQuats = ZDOExtraData.s_quats.Clone<Quaternion>();
		ZDOExtraData.s_saveInts = ZDOExtraData.s_ints.Clone<int>();
		ZDOExtraData.s_saveLongs = ZDOExtraData.s_longs.Clone<long>();
		ZDOExtraData.s_saveStrings = ZDOExtraData.s_strings.Clone<string>();
		ZDOExtraData.s_saveByteArrays = ZDOExtraData.s_byteArrays.Clone<byte[]>();
		ZDOExtraData.s_saveConnections = ZDOExtraData.s_connectionsHashData.Clone();
	}

	// Token: 0x06000D6D RID: 3437 RVA: 0x0005C84E File Offset: 0x0005AA4E
	public static void ClearSave()
	{
		ZDOExtraData.s_saveFloats = null;
		ZDOExtraData.s_saveVec3s = null;
		ZDOExtraData.s_saveQuats = null;
		ZDOExtraData.s_saveInts = null;
		ZDOExtraData.s_saveLongs = null;
		ZDOExtraData.s_saveStrings = null;
		ZDOExtraData.s_saveByteArrays = null;
		ZDOExtraData.s_saveConnections = null;
	}

	// Token: 0x06000D6E RID: 3438 RVA: 0x0005C880 File Offset: 0x0005AA80
	public static List<KeyValuePair<int, float>> GetSaveFloats(ZDOID zid)
	{
		return ZDOExtraData.s_saveFloats.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D6F RID: 3439 RVA: 0x0005C88D File Offset: 0x0005AA8D
	public static List<KeyValuePair<int, Vector3>> GetSaveVec3s(ZDOID zid)
	{
		return ZDOExtraData.s_saveVec3s.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D70 RID: 3440 RVA: 0x0005C89A File Offset: 0x0005AA9A
	public static List<KeyValuePair<int, Quaternion>> GetSaveQuaternions(ZDOID zid)
	{
		return ZDOExtraData.s_saveQuats.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D71 RID: 3441 RVA: 0x0005C8A7 File Offset: 0x0005AAA7
	public static List<KeyValuePair<int, int>> GetSaveInts(ZDOID zid)
	{
		return ZDOExtraData.s_saveInts.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D72 RID: 3442 RVA: 0x0005C8B4 File Offset: 0x0005AAB4
	public static List<KeyValuePair<int, long>> GetSaveLongs(ZDOID zid)
	{
		return ZDOExtraData.s_saveLongs.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D73 RID: 3443 RVA: 0x0005C8C1 File Offset: 0x0005AAC1
	public static List<KeyValuePair<int, string>> GetSaveStrings(ZDOID zid)
	{
		return ZDOExtraData.s_saveStrings.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D74 RID: 3444 RVA: 0x0005C8CE File Offset: 0x0005AACE
	public static List<KeyValuePair<int, byte[]>> GetSaveByteArrays(ZDOID zid)
	{
		return ZDOExtraData.s_saveByteArrays.GetValuesOrEmpty(zid);
	}

	// Token: 0x06000D75 RID: 3445 RVA: 0x0005C8DB File Offset: 0x0005AADB
	public static ZDOConnectionHashData GetSaveConnections(ZDOID zid)
	{
		return ZDOExtraData.s_saveConnections.GetValueOrDefaultPiktiv(zid, null);
	}

	// Token: 0x04000EFD RID: 3837
	private static readonly Dictionary<ZDOID, long> s_tempTimeCreated = new Dictionary<ZDOID, long>();

	// Token: 0x04000EFE RID: 3838
	private static int s_uniqueHashes = 0;

	// Token: 0x04000EFF RID: 3839
	private static readonly HashSet<int> s_usedHashes = new HashSet<int>();

	// Token: 0x04000F00 RID: 3840
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, float>> s_floats = new Dictionary<ZDOID, BinarySearchDictionary<int, float>>();

	// Token: 0x04000F01 RID: 3841
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, Vector3>> s_vec3 = new Dictionary<ZDOID, BinarySearchDictionary<int, Vector3>>();

	// Token: 0x04000F02 RID: 3842
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, Quaternion>> s_quats = new Dictionary<ZDOID, BinarySearchDictionary<int, Quaternion>>();

	// Token: 0x04000F03 RID: 3843
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, int>> s_ints = new Dictionary<ZDOID, BinarySearchDictionary<int, int>>();

	// Token: 0x04000F04 RID: 3844
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, long>> s_longs = new Dictionary<ZDOID, BinarySearchDictionary<int, long>>();

	// Token: 0x04000F05 RID: 3845
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, string>> s_strings = new Dictionary<ZDOID, BinarySearchDictionary<int, string>>();

	// Token: 0x04000F06 RID: 3846
	private static readonly Dictionary<ZDOID, BinarySearchDictionary<int, byte[]>> s_byteArrays = new Dictionary<ZDOID, BinarySearchDictionary<int, byte[]>>();

	// Token: 0x04000F07 RID: 3847
	private static readonly Dictionary<ZDOID, ZDOConnectionHashData> s_connectionsHashData = new Dictionary<ZDOID, ZDOConnectionHashData>();

	// Token: 0x04000F08 RID: 3848
	private static readonly Dictionary<ZDOID, ZDOConnection> s_connections = new Dictionary<ZDOID, ZDOConnection>();

	// Token: 0x04000F09 RID: 3849
	private static readonly Dictionary<ZDOID, ushort> s_owner = new Dictionary<ZDOID, ushort>();

	// Token: 0x04000F0A RID: 3850
	private static Dictionary<ZDOID, BinarySearchDictionary<int, float>> s_saveFloats = null;

	// Token: 0x04000F0B RID: 3851
	private static Dictionary<ZDOID, BinarySearchDictionary<int, Vector3>> s_saveVec3s = null;

	// Token: 0x04000F0C RID: 3852
	private static Dictionary<ZDOID, BinarySearchDictionary<int, Quaternion>> s_saveQuats = null;

	// Token: 0x04000F0D RID: 3853
	private static Dictionary<ZDOID, BinarySearchDictionary<int, int>> s_saveInts = null;

	// Token: 0x04000F0E RID: 3854
	private static Dictionary<ZDOID, BinarySearchDictionary<int, long>> s_saveLongs = null;

	// Token: 0x04000F0F RID: 3855
	private static Dictionary<ZDOID, BinarySearchDictionary<int, string>> s_saveStrings = null;

	// Token: 0x04000F10 RID: 3856
	private static Dictionary<ZDOID, BinarySearchDictionary<int, byte[]>> s_saveByteArrays = null;

	// Token: 0x04000F11 RID: 3857
	private static Dictionary<ZDOID, ZDOConnectionHashData> s_saveConnections = null;

	// Token: 0x02000156 RID: 342
	public enum Type
	{
		// Token: 0x04000F13 RID: 3859
		Float,
		// Token: 0x04000F14 RID: 3860
		Vec3,
		// Token: 0x04000F15 RID: 3861
		Quat,
		// Token: 0x04000F16 RID: 3862
		Int,
		// Token: 0x04000F17 RID: 3863
		Long,
		// Token: 0x04000F18 RID: 3864
		String,
		// Token: 0x04000F19 RID: 3865
		ByteArray
	}

	// Token: 0x02000157 RID: 343
	[Flags]
	public enum ConnectionType : byte
	{
		// Token: 0x04000F1B RID: 3867
		None = 0,
		// Token: 0x04000F1C RID: 3868
		Portal = 1,
		// Token: 0x04000F1D RID: 3869
		SyncTransform = 2,
		// Token: 0x04000F1E RID: 3870
		Spawned = 3,
		// Token: 0x04000F1F RID: 3871
		Target = 16
	}
}
