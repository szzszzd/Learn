using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// Token: 0x02000152 RID: 338
[StructLayout(0, Pack = 1)]
public class ZDO : IEquatable<ZDO>
{
	// Token: 0x06000CB8 RID: 3256 RVA: 0x0005A050 File Offset: 0x00058250
	public void Initialize(ZDOID id, Vector3 position)
	{
		this.m_uid = id;
		this.m_position = position;
		Vector2i zone = ZoneSystem.instance.GetZone(this.m_position);
		this.m_sector = zone.ClampToShort();
		ZDOMan.instance.AddToSector(this, zone);
		this.m_dataFlags = ZDO.DataFlags.None;
		this.Valid = true;
	}

	// Token: 0x06000CB9 RID: 3257 RVA: 0x0005A0A2 File Offset: 0x000582A2
	public void Init()
	{
		this.m_dataFlags = ZDO.DataFlags.None;
		this.Valid = true;
	}

	// Token: 0x06000CBA RID: 3258 RVA: 0x0005A0B2 File Offset: 0x000582B2
	public override string ToString()
	{
		return this.m_uid.ToString();
	}

	// Token: 0x06000CBB RID: 3259 RVA: 0x0005A0C5 File Offset: 0x000582C5
	public bool IsValid()
	{
		return this.Valid;
	}

	// Token: 0x06000CBC RID: 3260 RVA: 0x0005A0CD File Offset: 0x000582CD
	public override int GetHashCode()
	{
		return this.m_uid.GetHashCode();
	}

	// Token: 0x06000CBD RID: 3261 RVA: 0x0005A0E0 File Offset: 0x000582E0
	public bool Equals(ZDO other)
	{
		return this == other;
	}

	// Token: 0x06000CBE RID: 3262 RVA: 0x0005A0E8 File Offset: 0x000582E8
	public void Reset()
	{
		if (!this.SaveClone)
		{
			ZDOExtraData.Release(this, this.m_uid);
		}
		this.m_uid = ZDOID.None;
		this.m_dataFlags = ZDO.DataFlags.None;
		this.OwnerRevision = 0;
		this.DataRevision = 0U;
		this.m_tempSortValue = 0f;
		this.m_prefab = 0;
		this.m_sector = Vector2s.zero;
		this.m_position = Vector3.zero;
		this.m_rotation = Quaternion.identity.eulerAngles;
	}

	// Token: 0x06000CBF RID: 3263 RVA: 0x0005A164 File Offset: 0x00058364
	public ZDO Clone()
	{
		ZDO zdo = base.MemberwiseClone() as ZDO;
		zdo.SaveClone = true;
		return zdo;
	}

	// Token: 0x06000CC0 RID: 3264 RVA: 0x0005A178 File Offset: 0x00058378
	public void Set(string name, ZDOID id)
	{
		this.Set(ZDO.GetHashZDOID(name), id);
	}

	// Token: 0x06000CC1 RID: 3265 RVA: 0x0005A187 File Offset: 0x00058387
	public void Set(KeyValuePair<int, int> hashPair, ZDOID id)
	{
		this.Set(hashPair.Key, id.UserID);
		this.Set(hashPair.Value, (long)((ulong)id.ID));
	}

	// Token: 0x06000CC2 RID: 3266 RVA: 0x0005A1B2 File Offset: 0x000583B2
	public static KeyValuePair<int, int> GetHashZDOID(string name)
	{
		return new KeyValuePair<int, int>((name + "_u").GetStableHashCode(), (name + "_i").GetStableHashCode());
	}

	// Token: 0x06000CC3 RID: 3267 RVA: 0x0005A1D9 File Offset: 0x000583D9
	public ZDOID GetZDOID(string name)
	{
		return this.GetZDOID(ZDO.GetHashZDOID(name));
	}

	// Token: 0x06000CC4 RID: 3268 RVA: 0x0005A1E8 File Offset: 0x000583E8
	public ZDOID GetZDOID(KeyValuePair<int, int> hashPair)
	{
		long @long = this.GetLong(hashPair.Key, 0L);
		uint num = (uint)this.GetLong(hashPair.Value, 0L);
		if (@long == 0L || num == 0U)
		{
			return ZDOID.None;
		}
		return new ZDOID(@long, num);
	}

	// Token: 0x06000CC5 RID: 3269 RVA: 0x0005A229 File Offset: 0x00058429
	public void Set(string name, float value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000CC6 RID: 3270 RVA: 0x0005A238 File Offset: 0x00058438
	public void Set(int hash, float value)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CC7 RID: 3271 RVA: 0x0005A24F File Offset: 0x0005844F
	public void Set(string name, Vector3 value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000CC8 RID: 3272 RVA: 0x0005A25E File Offset: 0x0005845E
	public void Set(int hash, Vector3 value)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CC9 RID: 3273 RVA: 0x0005A275 File Offset: 0x00058475
	public void Update(int hash, Vector3 value)
	{
		if (ZDOExtraData.Update(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CCA RID: 3274 RVA: 0x0005A28C File Offset: 0x0005848C
	public void Set(string name, Quaternion value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000CCB RID: 3275 RVA: 0x0005A29B File Offset: 0x0005849B
	public void Set(int hash, Quaternion value)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CCC RID: 3276 RVA: 0x0005A2B2 File Offset: 0x000584B2
	public void Set(string name, int value)
	{
		this.Set(name.GetStableHashCode(), value, false);
	}

	// Token: 0x06000CCD RID: 3277 RVA: 0x0005A2C2 File Offset: 0x000584C2
	public void Set(int hash, int value, bool okForNotOwner = false)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CCE RID: 3278 RVA: 0x0005A2DB File Offset: 0x000584DB
	public void SetConnection(ZDOExtraData.ConnectionType connectionType, ZDOID zid)
	{
		if (ZDOExtraData.SetConnection(this.m_uid, connectionType, zid))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CCF RID: 3279 RVA: 0x0005A2F2 File Offset: 0x000584F2
	public void UpdateConnection(ZDOExtraData.ConnectionType connectionType, ZDOID zid)
	{
		if (ZDOExtraData.UpdateConnection(this.m_uid, connectionType, zid))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CD0 RID: 3280 RVA: 0x0005A309 File Offset: 0x00058509
	public void Set(string name, bool value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000CD1 RID: 3281 RVA: 0x0005A318 File Offset: 0x00058518
	public void Set(int hash, bool value)
	{
		this.Set(hash, value ? 1 : 0, false);
	}

	// Token: 0x06000CD2 RID: 3282 RVA: 0x0005A329 File Offset: 0x00058529
	public void Set(string name, long value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000CD3 RID: 3283 RVA: 0x0005A338 File Offset: 0x00058538
	public void Set(int hash, long value)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CD4 RID: 3284 RVA: 0x0005A34F File Offset: 0x0005854F
	public void Set(string name, byte[] bytes)
	{
		this.Set(name.GetStableHashCode(), bytes);
	}

	// Token: 0x06000CD5 RID: 3285 RVA: 0x0005A35E File Offset: 0x0005855E
	public void Set(int hash, byte[] bytes)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, bytes))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CD6 RID: 3286 RVA: 0x0005A375 File Offset: 0x00058575
	public void Set(string name, string value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000CD7 RID: 3287 RVA: 0x0005A384 File Offset: 0x00058584
	public void Set(int hash, string value)
	{
		if (ZDOExtraData.Set(this.m_uid, hash, value))
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CD8 RID: 3288 RVA: 0x0005A39B File Offset: 0x0005859B
	public void SetPosition(Vector3 pos)
	{
		this.InternalSetPosition(pos);
	}

	// Token: 0x06000CD9 RID: 3289 RVA: 0x0005A3A4 File Offset: 0x000585A4
	public void InternalSetPosition(Vector3 pos)
	{
		if (this.m_position == pos)
		{
			return;
		}
		this.m_position = pos;
		this.SetSector(ZoneSystem.instance.GetZone(this.m_position));
		if (this.IsOwner())
		{
			this.IncreaseDataRevision();
		}
	}

	// Token: 0x06000CDA RID: 3290 RVA: 0x0005A3E0 File Offset: 0x000585E0
	public void InvalidateSector()
	{
		this.SetSector(new Vector2i(int.MinValue, int.MinValue));
	}

	// Token: 0x06000CDB RID: 3291 RVA: 0x0005A3F8 File Offset: 0x000585F8
	private void SetSector(Vector2i sector)
	{
		if (this.m_sector == sector)
		{
			return;
		}
		ZDOMan.instance.RemoveFromSector(this, this.m_sector.ToVector2i());
		this.m_sector = sector.ClampToShort();
		ZDOMan.instance.AddToSector(this, sector);
		if (ZNet.instance.IsServer())
		{
			ZDOMan.instance.ZDOSectorInvalidated(this);
		}
	}

	// Token: 0x06000CDC RID: 3292 RVA: 0x0005A459 File Offset: 0x00058659
	public Vector2i GetSector()
	{
		return this.m_sector.ToVector2i();
	}

	// Token: 0x06000CDD RID: 3293 RVA: 0x0005A466 File Offset: 0x00058666
	public void SetRotation(Quaternion rot)
	{
		if (this.m_rotation == rot.eulerAngles)
		{
			return;
		}
		this.m_rotation = rot.eulerAngles;
		this.IncreaseDataRevision();
	}

	// Token: 0x06000CDE RID: 3294 RVA: 0x0005A490 File Offset: 0x00058690
	public void SetType(ZDO.ObjectType type)
	{
		if (this.Type == type)
		{
			return;
		}
		this.Type = type;
		this.IncreaseDataRevision();
	}

	// Token: 0x06000CDF RID: 3295 RVA: 0x0005A4A9 File Offset: 0x000586A9
	public void SetDistant(bool distant)
	{
		if (this.Distant == distant)
		{
			return;
		}
		this.Distant = distant;
		this.IncreaseDataRevision();
	}

	// Token: 0x06000CE0 RID: 3296 RVA: 0x0005A4C2 File Offset: 0x000586C2
	public void SetPrefab(int prefab)
	{
		if (this.m_prefab == prefab)
		{
			return;
		}
		this.m_prefab = prefab;
		this.IncreaseDataRevision();
	}

	// Token: 0x06000CE1 RID: 3297 RVA: 0x0005A4DB File Offset: 0x000586DB
	public int GetPrefab()
	{
		return this.m_prefab;
	}

	// Token: 0x06000CE2 RID: 3298 RVA: 0x0005A4E3 File Offset: 0x000586E3
	public Vector3 GetPosition()
	{
		return this.m_position;
	}

	// Token: 0x06000CE3 RID: 3299 RVA: 0x0005A4EB File Offset: 0x000586EB
	public Quaternion GetRotation()
	{
		return Quaternion.Euler(this.m_rotation);
	}

	// Token: 0x06000CE4 RID: 3300 RVA: 0x0005A4F8 File Offset: 0x000586F8
	private void IncreaseDataRevision()
	{
		uint dataRevision = this.DataRevision;
		this.DataRevision = dataRevision + 1U;
		if (!ZNet.instance.IsServer())
		{
			ZDOMan.instance.ClientChanged(this.m_uid);
		}
	}

	// Token: 0x06000CE5 RID: 3301 RVA: 0x0005A534 File Offset: 0x00058734
	private void IncreaseOwnerRevision()
	{
		ushort ownerRevision = this.OwnerRevision;
		this.OwnerRevision = ownerRevision + 1;
		if (!ZNet.instance.IsServer())
		{
			ZDOMan.instance.ClientChanged(this.m_uid);
		}
	}

	// Token: 0x06000CE6 RID: 3302 RVA: 0x0005A56E File Offset: 0x0005876E
	public float GetFloat(string name, float defaultValue = 0f)
	{
		return this.GetFloat(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CE7 RID: 3303 RVA: 0x0005A57D File Offset: 0x0005877D
	public float GetFloat(int hash, float defaultValue = 0f)
	{
		return ZDOExtraData.GetFloat(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CE8 RID: 3304 RVA: 0x0005A58C File Offset: 0x0005878C
	public Vector3 GetVec3(string name, Vector3 defaultValue)
	{
		return this.GetVec3(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CE9 RID: 3305 RVA: 0x0005A59B File Offset: 0x0005879B
	public Vector3 GetVec3(int hash, Vector3 defaultValue)
	{
		return ZDOExtraData.GetVec3(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CEA RID: 3306 RVA: 0x0005A5AA File Offset: 0x000587AA
	public Quaternion GetQuaternion(string name, Quaternion defaultValue)
	{
		return this.GetQuaternion(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CEB RID: 3307 RVA: 0x0005A5B9 File Offset: 0x000587B9
	public Quaternion GetQuaternion(int hash, Quaternion defaultValue)
	{
		return ZDOExtraData.GetQuaternion(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CEC RID: 3308 RVA: 0x0005A5C8 File Offset: 0x000587C8
	public int GetInt(string name, int defaultValue = 0)
	{
		return this.GetInt(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CED RID: 3309 RVA: 0x0005A5D7 File Offset: 0x000587D7
	public int GetInt(int hash, int defaultValue = 0)
	{
		return ZDOExtraData.GetInt(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CEE RID: 3310 RVA: 0x0005A5E6 File Offset: 0x000587E6
	public bool GetBool(string name, bool defaultValue = false)
	{
		return this.GetBool(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CEF RID: 3311 RVA: 0x0005A5F5 File Offset: 0x000587F5
	public bool GetBool(int hash, bool defaultValue = false)
	{
		return ZDOExtraData.GetInt(this.m_uid, hash, defaultValue ? 1 : 0) != 0;
	}

	// Token: 0x06000CF0 RID: 3312 RVA: 0x0005A60D File Offset: 0x0005880D
	public long GetLong(string name, long defaultValue = 0L)
	{
		return this.GetLong(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CF1 RID: 3313 RVA: 0x0005A61C File Offset: 0x0005881C
	public long GetLong(int hash, long defaultValue = 0L)
	{
		return ZDOExtraData.GetLong(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CF2 RID: 3314 RVA: 0x0005A62B File Offset: 0x0005882B
	public string GetString(string name, string defaultValue = "")
	{
		return this.GetString(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CF3 RID: 3315 RVA: 0x0005A63A File Offset: 0x0005883A
	public string GetString(int hash, string defaultValue = "")
	{
		return ZDOExtraData.GetString(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CF4 RID: 3316 RVA: 0x0005A649 File Offset: 0x00058849
	public byte[] GetByteArray(string name, byte[] defaultValue = null)
	{
		return this.GetByteArray(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000CF5 RID: 3317 RVA: 0x0005A658 File Offset: 0x00058858
	public byte[] GetByteArray(int hash, byte[] defaultValue = null)
	{
		return ZDOExtraData.GetByteArray(this.m_uid, hash, defaultValue);
	}

	// Token: 0x06000CF6 RID: 3318 RVA: 0x0005A667 File Offset: 0x00058867
	public ZDOID GetConnectionZDOID(ZDOExtraData.ConnectionType type)
	{
		return ZDOExtraData.GetConnectionZDOID(this.m_uid, type);
	}

	// Token: 0x06000CF7 RID: 3319 RVA: 0x0005A675 File Offset: 0x00058875
	public ZDOExtraData.ConnectionType GetConnectionType()
	{
		return ZDOExtraData.GetConnectionType(this.m_uid);
	}

	// Token: 0x06000CF8 RID: 3320 RVA: 0x0005A682 File Offset: 0x00058882
	public ZDOConnection GetConnection()
	{
		return ZDOExtraData.GetConnection(this.m_uid);
	}

	// Token: 0x06000CF9 RID: 3321 RVA: 0x0005A68F File Offset: 0x0005888F
	public ZDOConnectionHashData GetConnectionHashData(ZDOExtraData.ConnectionType type)
	{
		return ZDOExtraData.GetConnectionHashData(this.m_uid, type);
	}

	// Token: 0x06000CFA RID: 3322 RVA: 0x0005A69D File Offset: 0x0005889D
	public bool RemoveInt(int hash)
	{
		return ZDOExtraData.RemoveInt(this.m_uid, hash);
	}

	// Token: 0x06000CFB RID: 3323 RVA: 0x0005A6AB File Offset: 0x000588AB
	public bool RemoveLong(int hash)
	{
		return ZDOExtraData.RemoveLong(this.m_uid, hash);
	}

	// Token: 0x06000CFC RID: 3324 RVA: 0x0005A6B9 File Offset: 0x000588B9
	public bool RemoveFloat(int hash)
	{
		return ZDOExtraData.RemoveFloat(this.m_uid, hash);
	}

	// Token: 0x06000CFD RID: 3325 RVA: 0x0005A6C7 File Offset: 0x000588C7
	public bool RemoveVec3(int hash)
	{
		return ZDOExtraData.RemoveVec3(this.m_uid, hash);
	}

	// Token: 0x06000CFE RID: 3326 RVA: 0x0005A6D8 File Offset: 0x000588D8
	public void RemoveZDOID(string name)
	{
		KeyValuePair<int, int> hashZDOID = ZDO.GetHashZDOID(name);
		ZDOExtraData.RemoveLong(this.m_uid, hashZDOID.Key);
		ZDOExtraData.RemoveLong(this.m_uid, hashZDOID.Value);
	}

	// Token: 0x06000CFF RID: 3327 RVA: 0x0005A712 File Offset: 0x00058912
	public void RemoveZDOID(KeyValuePair<int, int> hashes)
	{
		ZDOExtraData.RemoveLong(this.m_uid, hashes.Key);
		ZDOExtraData.RemoveLong(this.m_uid, hashes.Value);
	}

	// Token: 0x06000D00 RID: 3328 RVA: 0x0005A73C File Offset: 0x0005893C
	public void Serialize(ZPackage pkg)
	{
		List<KeyValuePair<int, float>> floats = ZDOExtraData.GetFloats(this.m_uid);
		List<KeyValuePair<int, Vector3>> vec3s = ZDOExtraData.GetVec3s(this.m_uid);
		List<KeyValuePair<int, Quaternion>> quaternions = ZDOExtraData.GetQuaternions(this.m_uid);
		List<KeyValuePair<int, int>> ints = ZDOExtraData.GetInts(this.m_uid);
		List<KeyValuePair<int, long>> longs = ZDOExtraData.GetLongs(this.m_uid);
		List<KeyValuePair<int, string>> strings = ZDOExtraData.GetStrings(this.m_uid);
		List<KeyValuePair<int, byte[]>> byteArrays = ZDOExtraData.GetByteArrays(this.m_uid);
		ZDOConnection connection = ZDOExtraData.GetConnection(this.m_uid);
		ushort num = 0;
		if (connection != null && connection.m_type != ZDOExtraData.ConnectionType.None)
		{
			num |= 1;
		}
		if (floats.Count > 0)
		{
			num |= 2;
		}
		if (vec3s.Count > 0)
		{
			num |= 4;
		}
		if (quaternions.Count > 0)
		{
			num |= 8;
		}
		if (ints.Count > 0)
		{
			num |= 16;
		}
		if (longs.Count > 0)
		{
			num |= 32;
		}
		if (strings.Count > 0)
		{
			num |= 64;
		}
		if (byteArrays.Count > 0)
		{
			num |= 128;
		}
		bool flag = this.m_rotation != Quaternion.identity.eulerAngles;
		num |= (this.Persistent ? 256 : 0);
		num |= (this.Distant ? 512 : 0);
		num |= (ushort)(this.Type << 10);
		num |= (flag ? 4096 : 0);
		pkg.Write(num);
		pkg.Write(this.m_prefab);
		if (flag)
		{
			pkg.Write(this.m_rotation);
		}
		if ((num & 255) == 0)
		{
			return;
		}
		if ((num & 1) != 0)
		{
			pkg.Write((byte)connection.m_type);
			pkg.Write(connection.m_target);
		}
		if (floats.Count > 0)
		{
			pkg.Write((byte)floats.Count);
			foreach (KeyValuePair<int, float> keyValuePair in floats)
			{
				pkg.Write(keyValuePair.Key);
				pkg.Write(keyValuePair.Value);
			}
		}
		if (vec3s.Count > 0)
		{
			pkg.Write((byte)vec3s.Count);
			foreach (KeyValuePair<int, Vector3> keyValuePair2 in vec3s)
			{
				pkg.Write(keyValuePair2.Key);
				pkg.Write(keyValuePair2.Value);
			}
		}
		if (quaternions.Count > 0)
		{
			pkg.Write((byte)quaternions.Count);
			foreach (KeyValuePair<int, Quaternion> keyValuePair3 in quaternions)
			{
				pkg.Write(keyValuePair3.Key);
				pkg.Write(keyValuePair3.Value);
			}
		}
		if (ints.Count > 0)
		{
			pkg.Write((byte)ints.Count);
			foreach (KeyValuePair<int, int> keyValuePair4 in ints)
			{
				pkg.Write(keyValuePair4.Key);
				pkg.Write(keyValuePair4.Value);
			}
		}
		if (longs.Count > 0)
		{
			pkg.Write((byte)longs.Count);
			foreach (KeyValuePair<int, long> keyValuePair5 in longs)
			{
				pkg.Write(keyValuePair5.Key);
				pkg.Write(keyValuePair5.Value);
			}
		}
		if (strings.Count > 0)
		{
			pkg.Write((byte)strings.Count);
			foreach (KeyValuePair<int, string> keyValuePair6 in strings)
			{
				pkg.Write(keyValuePair6.Key);
				pkg.Write(keyValuePair6.Value);
			}
		}
		if (byteArrays.Count > 0)
		{
			pkg.Write((byte)byteArrays.Count);
			foreach (KeyValuePair<int, byte[]> keyValuePair7 in byteArrays)
			{
				pkg.Write(keyValuePair7.Key);
				pkg.Write(keyValuePair7.Value);
			}
		}
	}

	// Token: 0x06000D01 RID: 3329 RVA: 0x0005ABDC File Offset: 0x00058DDC
	public ZDOExtraData.ConnectionType Deserialize(ZPackage pkg)
	{
		ZDOExtraData.ConnectionType connectionType = ZDOExtraData.ConnectionType.None;
		ushort num = pkg.ReadUShort();
		this.Persistent = ((num & 256) > 0);
		this.Distant = ((num & 512) > 0);
		this.Type = (ZDO.ObjectType)(num >> 10 & 3);
		this.m_prefab = pkg.ReadInt();
		if ((num & 4096) > 0)
		{
			this.m_rotation = pkg.ReadVector3();
		}
		if ((num & 255) == 0)
		{
			return connectionType;
		}
		bool flag = (num & 1) > 0;
		bool flag2 = (num & 2) > 0;
		bool flag3 = (num & 4) > 0;
		bool flag4 = (num & 8) > 0;
		bool flag5 = (num & 16) > 0;
		bool flag6 = (num & 32) > 0;
		bool flag7 = (num & 64) > 0;
		bool flag8 = (num & 128) > 0;
		if (flag)
		{
			ZDOExtraData.ConnectionType connectionType2 = (ZDOExtraData.ConnectionType)pkg.ReadByte();
			ZDOID target = pkg.ReadZDOID();
			ZDOExtraData.SetConnection(this.m_uid, connectionType2, target);
			connectionType |= (connectionType2 & ~ZDOExtraData.ConnectionType.Target);
		}
		if (flag2)
		{
			int num2 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Float, num2);
			for (int i = 0; i < num2; i++)
			{
				int hash = pkg.ReadInt();
				float value = pkg.ReadSingle();
				ZDOExtraData.Set(this.m_uid, hash, value);
			}
		}
		if (flag3)
		{
			int num3 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Vec3, num3);
			for (int j = 0; j < num3; j++)
			{
				int hash2 = pkg.ReadInt();
				Vector3 value2 = pkg.ReadVector3();
				ZDOExtraData.Set(this.m_uid, hash2, value2);
			}
		}
		if (flag4)
		{
			int num4 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Quat, num4);
			for (int k = 0; k < num4; k++)
			{
				int hash3 = pkg.ReadInt();
				Quaternion value3 = pkg.ReadQuaternion();
				ZDOExtraData.Set(this.m_uid, hash3, value3);
			}
		}
		if (flag5)
		{
			int num5 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Int, num5);
			for (int l = 0; l < num5; l++)
			{
				int hash4 = pkg.ReadInt();
				int value4 = pkg.ReadInt();
				ZDOExtraData.Set(this.m_uid, hash4, value4);
			}
		}
		if (flag6)
		{
			int num6 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Long, num6);
			for (int m = 0; m < num6; m++)
			{
				int hash5 = pkg.ReadInt();
				long value5 = pkg.ReadLong();
				ZDOExtraData.Set(this.m_uid, hash5, value5);
			}
		}
		if (flag7)
		{
			int num7 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.String, num7);
			for (int n = 0; n < num7; n++)
			{
				int hash6 = pkg.ReadInt();
				string value6 = pkg.ReadString();
				ZDOExtraData.Set(this.m_uid, hash6, value6);
			}
		}
		if (flag8)
		{
			int num8 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.ByteArray, num8);
			for (int num9 = 0; num9 < num8; num9++)
			{
				int hash7 = pkg.ReadInt();
				byte[] value7 = pkg.ReadByteArray();
				ZDOExtraData.Set(this.m_uid, hash7, value7);
			}
		}
		return connectionType;
	}

	// Token: 0x06000D02 RID: 3330 RVA: 0x0005AECC File Offset: 0x000590CC
	public void Save(ZPackage pkg)
	{
		List<KeyValuePair<int, float>> saveFloats = ZDOExtraData.GetSaveFloats(this.m_uid);
		List<KeyValuePair<int, Vector3>> saveVec3s = ZDOExtraData.GetSaveVec3s(this.m_uid);
		List<KeyValuePair<int, Quaternion>> saveQuaternions = ZDOExtraData.GetSaveQuaternions(this.m_uid);
		List<KeyValuePair<int, int>> saveInts = ZDOExtraData.GetSaveInts(this.m_uid);
		List<KeyValuePair<int, long>> saveLongs = ZDOExtraData.GetSaveLongs(this.m_uid);
		List<KeyValuePair<int, string>> saveStrings = ZDOExtraData.GetSaveStrings(this.m_uid);
		List<KeyValuePair<int, byte[]>> saveByteArrays = ZDOExtraData.GetSaveByteArrays(this.m_uid);
		ZDOConnectionHashData saveConnections = ZDOExtraData.GetSaveConnections(this.m_uid);
		ushort num = 0;
		if (saveConnections != null && saveConnections.m_type != ZDOExtraData.ConnectionType.None)
		{
			num |= 1;
		}
		if (saveFloats.Count > 0)
		{
			num |= 2;
		}
		if (saveVec3s.Count > 0)
		{
			num |= 4;
		}
		if (saveQuaternions.Count > 0)
		{
			num |= 8;
		}
		if (saveInts.Count > 0)
		{
			num |= 16;
		}
		if (saveLongs.Count > 0)
		{
			num |= 32;
		}
		if (saveStrings.Count > 0)
		{
			num |= 64;
		}
		if (saveByteArrays.Count > 0)
		{
			num |= 128;
		}
		bool flag = this.m_rotation != Quaternion.identity.eulerAngles;
		num |= (this.Persistent ? 256 : 0);
		num |= (this.Distant ? 512 : 0);
		num |= (ushort)(this.Type << 10);
		num |= (flag ? 4096 : 0);
		pkg.Write(num);
		pkg.Write(this.m_sector);
		pkg.Write(this.m_position);
		pkg.Write(this.m_prefab);
		if (flag)
		{
			pkg.Write(this.m_rotation);
		}
		if ((num & 255) == 0)
		{
			return;
		}
		if ((num & 1) != 0)
		{
			pkg.Write((byte)saveConnections.m_type);
			pkg.Write(saveConnections.m_hash);
		}
		if (saveFloats.Count > 0)
		{
			pkg.Write((byte)saveFloats.Count);
			foreach (KeyValuePair<int, float> keyValuePair in saveFloats)
			{
				pkg.Write(keyValuePair.Key);
				pkg.Write(keyValuePair.Value);
			}
		}
		if (saveVec3s.Count > 0)
		{
			pkg.Write((byte)saveVec3s.Count);
			foreach (KeyValuePair<int, Vector3> keyValuePair2 in saveVec3s)
			{
				pkg.Write(keyValuePair2.Key);
				pkg.Write(keyValuePair2.Value);
			}
		}
		if (saveQuaternions.Count > 0)
		{
			pkg.Write((byte)saveQuaternions.Count);
			foreach (KeyValuePair<int, Quaternion> keyValuePair3 in saveQuaternions)
			{
				pkg.Write(keyValuePair3.Key);
				pkg.Write(keyValuePair3.Value);
			}
		}
		if (saveInts.Count > 0)
		{
			pkg.Write((byte)saveInts.Count);
			foreach (KeyValuePair<int, int> keyValuePair4 in saveInts)
			{
				pkg.Write(keyValuePair4.Key);
				pkg.Write(keyValuePair4.Value);
			}
		}
		if (saveLongs.Count > 0)
		{
			pkg.Write((byte)saveLongs.Count);
			foreach (KeyValuePair<int, long> keyValuePair5 in saveLongs)
			{
				pkg.Write(keyValuePair5.Key);
				pkg.Write(keyValuePair5.Value);
			}
		}
		if (saveStrings.Count > 0)
		{
			pkg.Write((byte)saveStrings.Count);
			foreach (KeyValuePair<int, string> keyValuePair6 in saveStrings)
			{
				pkg.Write(keyValuePair6.Key);
				pkg.Write(keyValuePair6.Value);
			}
		}
		if (saveByteArrays.Count > 0)
		{
			pkg.Write((byte)saveByteArrays.Count);
			foreach (KeyValuePair<int, byte[]> keyValuePair7 in saveByteArrays)
			{
				pkg.Write(keyValuePair7.Key);
				pkg.Write(keyValuePair7.Value);
			}
		}
	}

	// Token: 0x06000D03 RID: 3331 RVA: 0x0005B384 File Offset: 0x00059584
	private static bool Strip(int key)
	{
		return ZDOHelper.s_stripOldData.Contains(key);
	}

	// Token: 0x06000D04 RID: 3332 RVA: 0x0005B391 File Offset: 0x00059591
	private static bool StripLong(int key)
	{
		return ZDOHelper.s_stripOldLongData.Contains(key);
	}

	// Token: 0x06000D05 RID: 3333 RVA: 0x0005B39E File Offset: 0x0005959E
	private static bool Strip(int key, long data)
	{
		return data == 0L || ZDO.StripLong(key) || ZDO.Strip(key);
	}

	// Token: 0x06000D06 RID: 3334 RVA: 0x0005B3B3 File Offset: 0x000595B3
	private static bool Strip(int key, int data)
	{
		return data == 0 || ZDO.Strip(key);
	}

	// Token: 0x06000D07 RID: 3335 RVA: 0x0005B3C0 File Offset: 0x000595C0
	private static bool Strip(int key, Quaternion data)
	{
		return data == Quaternion.identity || ZDO.Strip(key);
	}

	// Token: 0x06000D08 RID: 3336 RVA: 0x0005B3D7 File Offset: 0x000595D7
	private static bool Strip(int key, string data)
	{
		return string.IsNullOrEmpty(data) || ZDO.Strip(key);
	}

	// Token: 0x06000D09 RID: 3337 RVA: 0x0005B3E9 File Offset: 0x000595E9
	private static bool Strip(int key, byte[] data)
	{
		return data.Length == 0 || ZDOHelper.s_stripOldDataByteArray.Contains(key);
	}

	// Token: 0x06000D0A RID: 3338 RVA: 0x0005B3FC File Offset: 0x000595FC
	private static bool StripConvert(ZDOID zid, int key, long data)
	{
		if (ZDO.Strip(key))
		{
			return true;
		}
		if (key == ZDOVars.s_SpawnTime__DontUse || key == ZDOVars.s_spawn_time__DontUse)
		{
			ZDOExtraData.Set(zid, ZDOVars.s_spawnTime, data);
			return true;
		}
		return false;
	}

	// Token: 0x06000D0B RID: 3339 RVA: 0x0005B428 File Offset: 0x00059628
	private static bool StripConvert(ZDOID zid, int key, Vector3 data)
	{
		if (ZDO.Strip(key))
		{
			return true;
		}
		if (key == ZDOVars.s_SpawnPoint__DontUse)
		{
			ZDOExtraData.Set(zid, ZDOVars.s_spawnPoint, data);
			return true;
		}
		if (Mathf.Approximately(data.x, data.y) && Mathf.Approximately(data.x, data.z))
		{
			if (key == ZDOVars.s_scaleHash)
			{
				if (Mathf.Approximately(data.x, 1f))
				{
					return true;
				}
				ZDOExtraData.Set(zid, ZDOVars.s_scaleScalarHash, data.x);
				return true;
			}
			else if (Mathf.Approximately(data.x, 0f))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000D0C RID: 3340 RVA: 0x0005B4C1 File Offset: 0x000596C1
	private static bool StripConvert(ZDOID zid, int key, float data)
	{
		return ZDO.Strip(key) || (key == ZDOVars.s_scaleScalarHash && Mathf.Approximately(data, 1f));
	}

	// Token: 0x06000D0D RID: 3341 RVA: 0x0005B4E8 File Offset: 0x000596E8
	public void LoadOldFormat(ZPackage pkg, int version)
	{
		pkg.ReadUInt();
		pkg.ReadUInt();
		this.Persistent = pkg.ReadBool();
		pkg.ReadLong();
		long timeCreated = pkg.ReadLong();
		ZDOExtraData.SetTimeCreated(this.m_uid, timeCreated);
		pkg.ReadInt();
		if (version >= 16 && version < 24)
		{
			pkg.ReadInt();
		}
		if (version >= 23)
		{
			this.Type = (ZDO.ObjectType)pkg.ReadSByte();
		}
		if (version >= 22)
		{
			this.Distant = pkg.ReadBool();
		}
		if (version < 13)
		{
			pkg.ReadChar();
			pkg.ReadChar();
		}
		if (version >= 17)
		{
			this.m_prefab = pkg.ReadInt();
		}
		this.m_sector = pkg.ReadVector2i().ClampToShort();
		this.m_position = pkg.ReadVector3();
		this.m_rotation = pkg.ReadQuaternion().eulerAngles;
		int num = (int)pkg.ReadChar();
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				int num2 = pkg.ReadInt();
				float num3 = pkg.ReadSingle();
				if (!ZDO.StripConvert(this.m_uid, num2, num3))
				{
					ZDOExtraData.Set(this.m_uid, num2, num3);
				}
			}
		}
		int num4 = (int)pkg.ReadChar();
		if (num4 > 0)
		{
			for (int j = 0; j < num4; j++)
			{
				int num5 = pkg.ReadInt();
				Vector3 vector = pkg.ReadVector3();
				if (!ZDO.StripConvert(this.m_uid, num5, vector))
				{
					ZDOExtraData.Set(this.m_uid, num5, vector);
				}
			}
		}
		int num6 = (int)pkg.ReadChar();
		if (num6 > 0)
		{
			for (int k = 0; k < num6; k++)
			{
				int num7 = pkg.ReadInt();
				Quaternion value = pkg.ReadQuaternion();
				if (!ZDO.Strip(num7))
				{
					ZDOExtraData.Set(this.m_uid, num7, value);
				}
			}
		}
		int num8 = (int)pkg.ReadChar();
		if (num8 > 0)
		{
			for (int l = 0; l < num8; l++)
			{
				int num9 = pkg.ReadInt();
				int value2 = pkg.ReadInt();
				if (!ZDO.Strip(num9))
				{
					ZDOExtraData.Set(this.m_uid, num9, value2);
				}
			}
		}
		int num10 = (int)pkg.ReadChar();
		if (num10 > 0)
		{
			for (int m = 0; m < num10; m++)
			{
				int num11 = pkg.ReadInt();
				long num12 = pkg.ReadLong();
				if (!ZDO.StripConvert(this.m_uid, num11, num12))
				{
					ZDOExtraData.Set(this.m_uid, num11, num12);
				}
			}
		}
		int num13 = (int)pkg.ReadChar();
		if (num13 > 0)
		{
			for (int n = 0; n < num13; n++)
			{
				int num14 = pkg.ReadInt();
				string value3 = pkg.ReadString();
				if (!ZDO.Strip(num14))
				{
					ZDOExtraData.Set(this.m_uid, num14, value3);
				}
			}
		}
		if (version >= 27)
		{
			int num15 = (int)pkg.ReadChar();
			if (num15 > 0)
			{
				for (int num16 = 0; num16 < num15; num16++)
				{
					int num17 = pkg.ReadInt();
					byte[] value4 = pkg.ReadByteArray();
					if (!ZDO.Strip(num17))
					{
						ZDOExtraData.Set(this.m_uid, num17, value4);
					}
				}
			}
		}
		if (version < 17)
		{
			this.m_prefab = this.GetInt("prefab", 0);
		}
	}

	// Token: 0x06000D0E RID: 3342 RVA: 0x0005B7D8 File Offset: 0x000599D8
	public void Load(ZPackage pkg, int version)
	{
		this.m_uid.SetID(ZDOID.m_loadID += 1U);
		ushort num = pkg.ReadUShort();
		this.Persistent = ((num & 256) > 0);
		this.Distant = ((num & 512) > 0);
		this.Type = (ZDO.ObjectType)(num >> 10 & 3);
		this.m_sector = pkg.ReadVector2s();
		this.m_position = pkg.ReadVector3();
		this.m_prefab = pkg.ReadInt();
		this.OwnerRevision = 0;
		this.DataRevision = 0U;
		this.Owned = false;
		this.Owner = false;
		this.Valid = true;
		this.SaveClone = false;
		if ((num & 4096) > 0)
		{
			this.m_rotation = pkg.ReadVector3();
		}
		if ((num & 255) == 0)
		{
			return;
		}
		bool flag = (num & 1) > 0;
		bool flag2 = (num & 2) > 0;
		bool flag3 = (num & 4) > 0;
		bool flag4 = (num & 8) > 0;
		bool flag5 = (num & 16) > 0;
		bool flag6 = (num & 32) > 0;
		bool flag7 = (num & 64) > 0;
		bool flag8 = (num & 128) > 0;
		if (flag)
		{
			ZDOExtraData.ConnectionType connectionType = (ZDOExtraData.ConnectionType)pkg.ReadByte();
			int hash = pkg.ReadInt();
			ZDOExtraData.SetConnectionData(this.m_uid, connectionType, hash);
		}
		if (flag2)
		{
			int num2 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Float, num2);
			for (int i = 0; i < num2; i++)
			{
				int num3 = pkg.ReadInt();
				float num4 = pkg.ReadSingle();
				if (!ZDO.StripConvert(this.m_uid, num3, num4))
				{
					ZDOExtraData.Add(this.m_uid, num3, num4);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.Float);
		}
		if (flag3)
		{
			int num5 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Vec3, num5);
			for (int j = 0; j < num5; j++)
			{
				int num6 = pkg.ReadInt();
				Vector3 vector = pkg.ReadVector3();
				if (!ZDO.StripConvert(this.m_uid, num6, vector))
				{
					ZDOExtraData.Add(this.m_uid, num6, vector);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.Vec3);
		}
		if (flag4)
		{
			int num7 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Quat, num7);
			for (int k = 0; k < num7; k++)
			{
				int num8 = pkg.ReadInt();
				Quaternion quaternion = pkg.ReadQuaternion();
				if (!ZDO.Strip(num8, quaternion))
				{
					ZDOExtraData.Add(this.m_uid, num8, quaternion);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.Quat);
		}
		if (flag5)
		{
			int num9 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Int, num9);
			for (int l = 0; l < num9; l++)
			{
				int num10 = pkg.ReadInt();
				int num11 = pkg.ReadInt();
				if (!ZDO.Strip(num10, num11))
				{
					ZDOExtraData.Add(this.m_uid, num10, num11);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.Int);
		}
		if (flag6)
		{
			int num12 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.Long, num12);
			for (int m = 0; m < num12; m++)
			{
				int num13 = pkg.ReadInt();
				long num14 = pkg.ReadLong();
				if (!ZDO.Strip(num13, num14))
				{
					ZDOExtraData.Add(this.m_uid, num13, num14);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.Long);
		}
		if (flag7)
		{
			int num15 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.String, num15);
			for (int n = 0; n < num15; n++)
			{
				int num16 = pkg.ReadInt();
				string text = pkg.ReadString();
				if (!ZDO.Strip(num16, text))
				{
					ZDOExtraData.Add(this.m_uid, num16, text);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.String);
		}
		if (flag8)
		{
			int num17 = (int)pkg.ReadByte();
			ZDOExtraData.Reserve(this.m_uid, ZDOExtraData.Type.ByteArray, num17);
			for (int num18 = 0; num18 < num17; num18++)
			{
				int num19 = pkg.ReadInt();
				byte[] array = pkg.ReadByteArray();
				if (!ZDO.Strip(num19, array))
				{
					ZDOExtraData.Add(this.m_uid, num19, array);
				}
			}
			ZDOExtraData.RemoveIfEmpty(this.m_uid, ZDOExtraData.Type.ByteArray);
		}
	}

	// Token: 0x06000D0F RID: 3343 RVA: 0x0005BBB3 File Offset: 0x00059DB3
	public long GetOwner()
	{
		if (!this.Owned)
		{
			return 0L;
		}
		return ZDOExtraData.GetOwner(this.m_uid);
	}

	// Token: 0x06000D10 RID: 3344 RVA: 0x0005BBCB File Offset: 0x00059DCB
	public bool IsOwner()
	{
		return this.Owner;
	}

	// Token: 0x06000D11 RID: 3345 RVA: 0x0005BBD3 File Offset: 0x00059DD3
	public bool HasOwner()
	{
		return this.Owned;
	}

	// Token: 0x06000D12 RID: 3346 RVA: 0x0005BBDB File Offset: 0x00059DDB
	public void SetOwner(long uid)
	{
		if (ZDOExtraData.GetOwner(this.m_uid) == uid)
		{
			return;
		}
		this.SetOwnerInternal(uid);
		this.IncreaseOwnerRevision();
	}

	// Token: 0x06000D13 RID: 3347 RVA: 0x0005BBFC File Offset: 0x00059DFC
	public void SetOwnerInternal(long uid)
	{
		if (uid == 0L)
		{
			ZDOExtraData.ReleaseOwner(this.m_uid);
			this.Owned = false;
			this.Owner = false;
			return;
		}
		ushort ownerKey = ZDOID.AddUser(uid);
		ZDOExtraData.SetOwner(this.m_uid, ownerKey);
		this.Owned = true;
		this.Owner = (uid == ZDOMan.GetSessionID());
	}

	// Token: 0x17000085 RID: 133
	// (get) Token: 0x06000D14 RID: 3348 RVA: 0x0005BC4E File Offset: 0x00059E4E
	// (set) Token: 0x06000D15 RID: 3349 RVA: 0x0005BC5B File Offset: 0x00059E5B
	public bool Persistent
	{
		get
		{
			return (this.m_dataFlags & ZDO.DataFlags.Persistent) > ZDO.DataFlags.None;
		}
		set
		{
			if (value)
			{
				this.m_dataFlags |= ZDO.DataFlags.Persistent;
				return;
			}
			this.m_dataFlags &= ~ZDO.DataFlags.Persistent;
		}
	}

	// Token: 0x17000086 RID: 134
	// (get) Token: 0x06000D16 RID: 3350 RVA: 0x0005BC81 File Offset: 0x00059E81
	// (set) Token: 0x06000D17 RID: 3351 RVA: 0x0005BC8E File Offset: 0x00059E8E
	public bool Distant
	{
		get
		{
			return (this.m_dataFlags & ZDO.DataFlags.Distant) > ZDO.DataFlags.None;
		}
		set
		{
			if (value)
			{
				this.m_dataFlags |= ZDO.DataFlags.Distant;
				return;
			}
			this.m_dataFlags &= ~ZDO.DataFlags.Distant;
		}
	}

	// Token: 0x17000087 RID: 135
	// (get) Token: 0x06000D18 RID: 3352 RVA: 0x0005BCB4 File Offset: 0x00059EB4
	// (set) Token: 0x06000D19 RID: 3353 RVA: 0x0005BCC2 File Offset: 0x00059EC2
	private bool Owner
	{
		get
		{
			return (this.m_dataFlags & ZDO.DataFlags.Owner) > ZDO.DataFlags.None;
		}
		set
		{
			if (value)
			{
				this.m_dataFlags |= ZDO.DataFlags.Owner;
				return;
			}
			this.m_dataFlags &= ~ZDO.DataFlags.Owner;
		}
	}

	// Token: 0x17000088 RID: 136
	// (get) Token: 0x06000D1A RID: 3354 RVA: 0x0005BCE9 File Offset: 0x00059EE9
	// (set) Token: 0x06000D1B RID: 3355 RVA: 0x0005BCF7 File Offset: 0x00059EF7
	private bool Owned
	{
		get
		{
			return (this.m_dataFlags & ZDO.DataFlags.Owned) > ZDO.DataFlags.None;
		}
		set
		{
			if (value)
			{
				this.m_dataFlags |= ZDO.DataFlags.Owned;
				return;
			}
			this.m_dataFlags &= ~ZDO.DataFlags.Owned;
		}
	}

	// Token: 0x17000089 RID: 137
	// (get) Token: 0x06000D1C RID: 3356 RVA: 0x0005BD1E File Offset: 0x00059F1E
	// (set) Token: 0x06000D1D RID: 3357 RVA: 0x0005BD2C File Offset: 0x00059F2C
	private bool Valid
	{
		get
		{
			return (this.m_dataFlags & ZDO.DataFlags.Valid) > ZDO.DataFlags.None;
		}
		set
		{
			if (value)
			{
				this.m_dataFlags |= ZDO.DataFlags.Valid;
				return;
			}
			this.m_dataFlags &= ~ZDO.DataFlags.Valid;
		}
	}

	// Token: 0x1700008A RID: 138
	// (get) Token: 0x06000D1E RID: 3358 RVA: 0x0005BD53 File Offset: 0x00059F53
	// (set) Token: 0x06000D1F RID: 3359 RVA: 0x0005BD60 File Offset: 0x00059F60
	public ZDO.ObjectType Type
	{
		get
		{
			return (ZDO.ObjectType)((this.m_dataFlags & (ZDO.DataFlags.Type | ZDO.DataFlags.Type1)) >> 2);
		}
		set
		{
			this.m_dataFlags = (ZDO.DataFlags)((ZDO.ObjectType)(this.m_dataFlags & ~(ZDO.DataFlags.Type | ZDO.DataFlags.Type1)) | (value & ZDO.ObjectType.Terrain) << 2);
		}
	}

	// Token: 0x1700008B RID: 139
	// (get) Token: 0x06000D20 RID: 3360 RVA: 0x0005BD7B File Offset: 0x00059F7B
	// (set) Token: 0x06000D21 RID: 3361 RVA: 0x0005BD8C File Offset: 0x00059F8C
	private bool SaveClone
	{
		get
		{
			return (this.m_dataFlags & ZDO.DataFlags.SaveClone) > ZDO.DataFlags.None;
		}
		set
		{
			if (value)
			{
				this.m_dataFlags |= ZDO.DataFlags.SaveClone;
				return;
			}
			this.m_dataFlags &= ~ZDO.DataFlags.SaveClone;
		}
	}

	// Token: 0x1700008C RID: 140
	// (get) Token: 0x06000D22 RID: 3362 RVA: 0x0005BDB3 File Offset: 0x00059FB3
	// (set) Token: 0x06000D23 RID: 3363 RVA: 0x0005BDBB File Offset: 0x00059FBB
	public byte TempRemoveEarmark { get; set; } = byte.MaxValue;

	// Token: 0x1700008D RID: 141
	// (get) Token: 0x06000D24 RID: 3364 RVA: 0x0005BDC4 File Offset: 0x00059FC4
	// (set) Token: 0x06000D25 RID: 3365 RVA: 0x0005BDCC File Offset: 0x00059FCC
	public byte TempCreateEarmark { get; set; } = byte.MaxValue;

	// Token: 0x1700008E RID: 142
	// (get) Token: 0x06000D26 RID: 3366 RVA: 0x0005BDD5 File Offset: 0x00059FD5
	// (set) Token: 0x06000D27 RID: 3367 RVA: 0x0005BDDD File Offset: 0x00059FDD
	public ushort OwnerRevision { get; set; }

	// Token: 0x1700008F RID: 143
	// (get) Token: 0x06000D28 RID: 3368 RVA: 0x0005BDE6 File Offset: 0x00059FE6
	// (set) Token: 0x06000D29 RID: 3369 RVA: 0x0005BDEE File Offset: 0x00059FEE
	public uint DataRevision { get; set; }

	// Token: 0x04000EE3 RID: 3811
	public ZDOID m_uid = ZDOID.None;

	// Token: 0x04000EE8 RID: 3816
	public float m_tempSortValue;

	// Token: 0x04000EE9 RID: 3817
	private int m_prefab;

	// Token: 0x04000EEA RID: 3818
	private Vector2s m_sector = Vector2s.zero;

	// Token: 0x04000EEB RID: 3819
	private Vector3 m_rotation = Quaternion.identity.eulerAngles;

	// Token: 0x04000EEC RID: 3820
	private Vector3 m_position = Vector3.zero;

	// Token: 0x04000EED RID: 3821
	private ZDO.DataFlags m_dataFlags;

	// Token: 0x02000153 RID: 339
	[Flags]
	private enum DataFlags : byte
	{
		// Token: 0x04000EEF RID: 3823
		None = 0,
		// Token: 0x04000EF0 RID: 3824
		Persistent = 1,
		// Token: 0x04000EF1 RID: 3825
		Distant = 2,
		// Token: 0x04000EF2 RID: 3826
		Type = 4,
		// Token: 0x04000EF3 RID: 3827
		Type1 = 8,
		// Token: 0x04000EF4 RID: 3828
		Owner = 16,
		// Token: 0x04000EF5 RID: 3829
		Owned = 32,
		// Token: 0x04000EF6 RID: 3830
		Valid = 64,
		// Token: 0x04000EF7 RID: 3831
		SaveClone = 128
	}

	// Token: 0x02000154 RID: 340
	public enum ObjectType
	{
		// Token: 0x04000EF9 RID: 3833
		Default,
		// Token: 0x04000EFA RID: 3834
		Prioritized,
		// Token: 0x04000EFB RID: 3835
		Solid,
		// Token: 0x04000EFC RID: 3836
		Terrain
	}
}
