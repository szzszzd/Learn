using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200016C RID: 364
public class ZNetScene : MonoBehaviour
{
	// Token: 0x1700009B RID: 155
	// (get) Token: 0x06000E91 RID: 3729 RVA: 0x000640B2 File Offset: 0x000622B2
	public static ZNetScene instance
	{
		get
		{
			return ZNetScene.s_instance;
		}
	}

	// Token: 0x06000E92 RID: 3730 RVA: 0x000640BC File Offset: 0x000622BC
	private void Awake()
	{
		ZNetScene.s_instance = this;
		foreach (GameObject gameObject in this.m_prefabs)
		{
			this.m_namedPrefabs.Add(gameObject.name.GetStableHashCode(), gameObject);
		}
		foreach (GameObject gameObject2 in this.m_nonNetViewPrefabs)
		{
			this.m_namedPrefabs.Add(gameObject2.name.GetStableHashCode(), gameObject2);
		}
		ZDOMan instance = ZDOMan.instance;
		instance.m_onZDODestroyed = (Action<ZDO>)Delegate.Combine(instance.m_onZDODestroyed, new Action<ZDO>(this.OnZDODestroyed));
		ZRoutedRpc.instance.Register<Vector3, Quaternion, int>("SpawnObject", new Action<long, Vector3, Quaternion, int>(this.RPC_SpawnObject));
	}

	// Token: 0x06000E93 RID: 3731 RVA: 0x000641B8 File Offset: 0x000623B8
	private void OnDestroy()
	{
		ZLog.Log("Net scene destroyed");
		if (ZNetScene.s_instance == this)
		{
			ZNetScene.s_instance = null;
		}
	}

	// Token: 0x06000E94 RID: 3732 RVA: 0x000641D8 File Offset: 0x000623D8
	public void Shutdown()
	{
		foreach (KeyValuePair<ZDO, ZNetView> keyValuePair in this.m_instances)
		{
			if (keyValuePair.Value)
			{
				keyValuePair.Value.ResetZDO();
				UnityEngine.Object.Destroy(keyValuePair.Value.gameObject);
			}
		}
		this.m_instances.Clear();
		base.enabled = false;
	}

	// Token: 0x06000E95 RID: 3733 RVA: 0x00064264 File Offset: 0x00062464
	public void AddInstance(ZDO zdo, ZNetView nview)
	{
		this.m_instances[zdo] = nview;
	}

	// Token: 0x06000E96 RID: 3734 RVA: 0x00064274 File Offset: 0x00062474
	private bool IsPrefabZDOValid(ZDO zdo)
	{
		int prefab = zdo.GetPrefab();
		return prefab != 0 && this.GetPrefab(prefab) != null;
	}

	// Token: 0x06000E97 RID: 3735 RVA: 0x0006429C File Offset: 0x0006249C
	private GameObject CreateObject(ZDO zdo)
	{
		int prefab = zdo.GetPrefab();
		if (prefab == 0)
		{
			return null;
		}
		GameObject prefab2 = this.GetPrefab(prefab);
		if (prefab2 == null)
		{
			return null;
		}
		Vector3 position = zdo.GetPosition();
		Quaternion rotation = zdo.GetRotation();
		ZNetView.m_useInitZDO = true;
		ZNetView.m_initZDO = zdo;
		GameObject result = UnityEngine.Object.Instantiate<GameObject>(prefab2, position, rotation);
		if (ZNetView.m_initZDO != null)
		{
			string str = "ZDO ";
			ZDOID uid = zdo.m_uid;
			ZLog.LogWarning(str + uid.ToString() + " not used when creating object " + prefab2.name);
			ZNetView.m_initZDO = null;
		}
		ZNetView.m_useInitZDO = false;
		return result;
	}

	// Token: 0x06000E98 RID: 3736 RVA: 0x0006432C File Offset: 0x0006252C
	public void Destroy(GameObject go)
	{
		ZNetView component = go.GetComponent<ZNetView>();
		if (component && component.GetZDO() != null)
		{
			ZDO zdo = component.GetZDO();
			component.ResetZDO();
			this.m_instances.Remove(zdo);
			if (zdo.IsOwner())
			{
				ZDOMan.instance.DestroyZDO(zdo);
			}
		}
		UnityEngine.Object.Destroy(go);
	}

	// Token: 0x06000E99 RID: 3737 RVA: 0x00064384 File Offset: 0x00062584
	public GameObject GetPrefab(int hash)
	{
		GameObject result;
		if (this.m_namedPrefabs.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06000E9A RID: 3738 RVA: 0x000643A4 File Offset: 0x000625A4
	public GameObject GetPrefab(string name)
	{
		return this.GetPrefab(name.GetStableHashCode());
	}

	// Token: 0x06000E9B RID: 3739 RVA: 0x000643B2 File Offset: 0x000625B2
	public int GetPrefabHash(GameObject go)
	{
		return go.name.GetStableHashCode();
	}

	// Token: 0x06000E9C RID: 3740 RVA: 0x000643C0 File Offset: 0x000625C0
	public bool IsAreaReady(Vector3 point)
	{
		Vector2i zone = ZoneSystem.instance.GetZone(point);
		if (!ZoneSystem.instance.IsZoneLoaded(zone))
		{
			return false;
		}
		this.m_tempCurrentObjects.Clear();
		ZDOMan.instance.FindSectorObjects(zone, 1, 0, this.m_tempCurrentObjects, null);
		foreach (ZDO zdo in this.m_tempCurrentObjects)
		{
			if (this.IsPrefabZDOValid(zdo) && !this.FindInstance(zdo))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000E9D RID: 3741 RVA: 0x00064464 File Offset: 0x00062664
	private bool InLoadingScreen()
	{
		return Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting();
	}

	// Token: 0x06000E9E RID: 3742 RVA: 0x00064480 File Offset: 0x00062680
	private void CreateObjects(List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
	{
		int maxCreatedPerFrame = 10;
		if (this.InLoadingScreen())
		{
			maxCreatedPerFrame = 100;
		}
		byte tempCreateEarmark = (byte)(Time.frameCount & 255);
		foreach (ZDO zdo in this.m_instances.Keys)
		{
			zdo.TempCreateEarmark = tempCreateEarmark;
		}
		int num = 0;
		this.CreateObjectsSorted(currentNearObjects, maxCreatedPerFrame, ref num);
		this.CreateDistantObjects(currentDistantObjects, maxCreatedPerFrame, ref num);
	}

	// Token: 0x06000E9F RID: 3743 RVA: 0x00064508 File Offset: 0x00062708
	private void CreateObjectsSorted(List<ZDO> currentNearObjects, int maxCreatedPerFrame, ref int created)
	{
		if (!ZoneSystem.instance.IsActiveAreaLoaded())
		{
			return;
		}
		this.m_tempCurrentObjects2.Clear();
		byte b = (byte)(Time.frameCount & 255);
		Vector3 referencePosition = ZNet.instance.GetReferencePosition();
		foreach (ZDO zdo in currentNearObjects)
		{
			if (zdo.TempCreateEarmark != b)
			{
				zdo.m_tempSortValue = Utils.DistanceSqr(referencePosition, zdo.GetPosition());
				this.m_tempCurrentObjects2.Add(zdo);
			}
		}
		int num = Mathf.Max(this.m_tempCurrentObjects2.Count / 100, maxCreatedPerFrame);
		this.m_tempCurrentObjects2.Sort(new Comparison<ZDO>(ZNetScene.ZDOCompare));
		foreach (ZDO zdo2 in this.m_tempCurrentObjects2)
		{
			if (this.CreateObject(zdo2) != null)
			{
				created++;
				if (created > num)
				{
					break;
				}
			}
			else if (ZNet.instance.IsServer())
			{
				zdo2.SetOwner(ZDOMan.GetSessionID());
				string str = "Destroyed invalid predab ZDO:";
				ZDOID uid = zdo2.m_uid;
				ZLog.Log(str + uid.ToString());
				ZDOMan.instance.DestroyZDO(zdo2);
			}
		}
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x0006467C File Offset: 0x0006287C
	private static int ZDOCompare(ZDO x, ZDO y)
	{
		if (x.Type == y.Type)
		{
			return Utils.CompareFloats(x.m_tempSortValue, y.m_tempSortValue);
		}
		return ((int)y.Type).CompareTo((int)x.Type);
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x000646C0 File Offset: 0x000628C0
	private void CreateDistantObjects(List<ZDO> objects, int maxCreatedPerFrame, ref int created)
	{
		if (created > maxCreatedPerFrame)
		{
			return;
		}
		byte b = (byte)(Time.frameCount & 255);
		foreach (ZDO zdo in objects)
		{
			if (zdo.TempCreateEarmark != b)
			{
				if (this.CreateObject(zdo) != null)
				{
					created++;
					if (created > maxCreatedPerFrame)
					{
						break;
					}
				}
				else if (ZNet.instance.IsServer())
				{
					zdo.SetOwner(ZDOMan.GetSessionID());
					string str = "Destroyed invalid predab ZDO:";
					ZDOID uid = zdo.m_uid;
					ZLog.Log(str + uid.ToString() + "  prefab hash:" + zdo.GetPrefab().ToString());
					ZDOMan.instance.DestroyZDO(zdo);
				}
			}
		}
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x000647A0 File Offset: 0x000629A0
	private void OnZDODestroyed(ZDO zdo)
	{
		ZNetView znetView;
		if (this.m_instances.TryGetValue(zdo, out znetView))
		{
			znetView.ResetZDO();
			UnityEngine.Object.Destroy(znetView.gameObject);
			this.m_instances.Remove(zdo);
		}
	}

	// Token: 0x06000EA3 RID: 3747 RVA: 0x000647DC File Offset: 0x000629DC
	private void RemoveObjects(List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
	{
		byte b = (byte)(Time.frameCount & 255);
		foreach (ZDO zdo in currentNearObjects)
		{
			zdo.TempRemoveEarmark = b;
		}
		foreach (ZDO zdo2 in currentDistantObjects)
		{
			zdo2.TempRemoveEarmark = b;
		}
		this.m_tempRemoved.Clear();
		foreach (ZNetView znetView in this.m_instances.Values)
		{
			if (znetView.GetZDO().TempRemoveEarmark != b)
			{
				this.m_tempRemoved.Add(znetView);
			}
		}
		for (int i = 0; i < this.m_tempRemoved.Count; i++)
		{
			ZNetView znetView2 = this.m_tempRemoved[i];
			ZDO zdo3 = znetView2.GetZDO();
			znetView2.ResetZDO();
			UnityEngine.Object.Destroy(znetView2.gameObject);
			if (!zdo3.Persistent && zdo3.IsOwner())
			{
				ZDOMan.instance.DestroyZDO(zdo3);
			}
			this.m_instances.Remove(zdo3);
		}
	}

	// Token: 0x06000EA4 RID: 3748 RVA: 0x00064940 File Offset: 0x00062B40
	public ZNetView FindInstance(ZDO zdo)
	{
		ZNetView result;
		if (this.m_instances.TryGetValue(zdo, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x00064960 File Offset: 0x00062B60
	public bool HaveInstance(ZDO zdo)
	{
		return this.m_instances.ContainsKey(zdo);
	}

	// Token: 0x06000EA6 RID: 3750 RVA: 0x00064970 File Offset: 0x00062B70
	public GameObject FindInstance(ZDOID id)
	{
		ZDO zdo = ZDOMan.instance.GetZDO(id);
		if (zdo != null)
		{
			ZNetView znetView = this.FindInstance(zdo);
			if (znetView)
			{
				return znetView.gameObject;
			}
		}
		return null;
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x000649A4 File Offset: 0x00062BA4
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		this.m_createDestroyTimer += deltaTime;
		if (this.m_createDestroyTimer >= 0.033333335f)
		{
			this.m_createDestroyTimer = 0f;
			this.CreateDestroyObjects();
		}
	}

	// Token: 0x06000EA8 RID: 3752 RVA: 0x000649E4 File Offset: 0x00062BE4
	private void CreateDestroyObjects()
	{
		Vector2i zone = ZoneSystem.instance.GetZone(ZNet.instance.GetReferencePosition());
		this.m_tempCurrentObjects.Clear();
		this.m_tempCurrentDistantObjects.Clear();
		ZDOMan.instance.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, ZoneSystem.instance.m_activeDistantArea, this.m_tempCurrentObjects, this.m_tempCurrentDistantObjects);
		this.CreateObjects(this.m_tempCurrentObjects, this.m_tempCurrentDistantObjects);
		this.RemoveObjects(this.m_tempCurrentObjects, this.m_tempCurrentDistantObjects);
	}

	// Token: 0x06000EA9 RID: 3753 RVA: 0x00064A6C File Offset: 0x00062C6C
	public static bool InActiveArea(Vector2i zone, Vector3 refPoint)
	{
		Vector2i zone2 = ZoneSystem.instance.GetZone(refPoint);
		return ZNetScene.InActiveArea(zone, zone2);
	}

	// Token: 0x06000EAA RID: 3754 RVA: 0x00064A8C File Offset: 0x00062C8C
	public static bool InActiveArea(Vector2i zone, Vector2i refCenterZone)
	{
		int num = ZoneSystem.instance.m_activeArea - 1;
		return zone.x >= refCenterZone.x - num && zone.x <= refCenterZone.x + num && zone.y <= refCenterZone.y + num && zone.y >= refCenterZone.y - num;
	}

	// Token: 0x06000EAB RID: 3755 RVA: 0x00064AEB File Offset: 0x00062CEB
	public bool OutsideActiveArea(Vector3 point)
	{
		return this.OutsideActiveArea(point, ZNet.instance.GetReferencePosition());
	}

	// Token: 0x06000EAC RID: 3756 RVA: 0x00064B00 File Offset: 0x00062D00
	private bool OutsideActiveArea(Vector3 point, Vector3 refPoint)
	{
		Vector2i zone = ZoneSystem.instance.GetZone(refPoint);
		Vector2i zone2 = ZoneSystem.instance.GetZone(point);
		return zone2.x <= zone.x - ZoneSystem.instance.m_activeArea || zone2.x >= zone.x + ZoneSystem.instance.m_activeArea || zone2.y >= zone.y + ZoneSystem.instance.m_activeArea || zone2.y <= zone.y - ZoneSystem.instance.m_activeArea;
	}

	// Token: 0x06000EAD RID: 3757 RVA: 0x00064B90 File Offset: 0x00062D90
	public bool HaveInstanceInSector(Vector2i sector)
	{
		foreach (KeyValuePair<ZDO, ZNetView> keyValuePair in this.m_instances)
		{
			if (keyValuePair.Value && !keyValuePair.Value.m_distant && ZoneSystem.instance.GetZone(keyValuePair.Value.transform.position) == sector)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000EAE RID: 3758 RVA: 0x00064C24 File Offset: 0x00062E24
	public int NrOfInstances()
	{
		return this.m_instances.Count;
	}

	// Token: 0x06000EAF RID: 3759 RVA: 0x00064C34 File Offset: 0x00062E34
	public void SpawnObject(Vector3 pos, Quaternion rot, GameObject prefab)
	{
		int prefabHash = this.GetPrefabHash(prefab);
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SpawnObject", new object[]
		{
			pos,
			rot,
			prefabHash
		});
	}

	// Token: 0x06000EB0 RID: 3760 RVA: 0x00064C80 File Offset: 0x00062E80
	public List<string> GetPrefabNames()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<int, GameObject> keyValuePair in this.m_namedPrefabs)
		{
			list.Add(keyValuePair.Value.name);
		}
		return list;
	}

	// Token: 0x06000EB1 RID: 3761 RVA: 0x00064CE8 File Offset: 0x00062EE8
	private void RPC_SpawnObject(long spawner, Vector3 pos, Quaternion rot, int prefabHash)
	{
		GameObject prefab = this.GetPrefab(prefabHash);
		if (prefab == null)
		{
			ZLog.Log("Missing prefab " + prefabHash.ToString());
			return;
		}
		UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
	}

	// Token: 0x04001059 RID: 4185
	private static ZNetScene s_instance;

	// Token: 0x0400105A RID: 4186
	private const int m_maxCreatedPerFrame = 10;

	// Token: 0x0400105B RID: 4187
	private const float m_createDestroyFps = 30f;

	// Token: 0x0400105C RID: 4188
	public List<GameObject> m_prefabs = new List<GameObject>();

	// Token: 0x0400105D RID: 4189
	public List<GameObject> m_nonNetViewPrefabs = new List<GameObject>();

	// Token: 0x0400105E RID: 4190
	private readonly Dictionary<int, GameObject> m_namedPrefabs = new Dictionary<int, GameObject>();

	// Token: 0x0400105F RID: 4191
	private readonly Dictionary<ZDO, ZNetView> m_instances = new Dictionary<ZDO, ZNetView>();

	// Token: 0x04001060 RID: 4192
	private readonly List<ZDO> m_tempCurrentObjects = new List<ZDO>();

	// Token: 0x04001061 RID: 4193
	private readonly List<ZDO> m_tempCurrentObjects2 = new List<ZDO>();

	// Token: 0x04001062 RID: 4194
	private readonly List<ZDO> m_tempCurrentDistantObjects = new List<ZDO>();

	// Token: 0x04001063 RID: 4195
	private readonly List<ZNetView> m_tempRemoved = new List<ZNetView>();

	// Token: 0x04001064 RID: 4196
	private float m_createDestroyTimer;
}
