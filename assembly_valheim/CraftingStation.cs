using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000120 RID: 288
public class CraftingStation : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000B0A RID: 2826 RVA: 0x00051DA4 File Offset: 0x0004FFA4
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.GetZDO() == null)
		{
			return;
		}
		CraftingStation.m_allStations.Add(this);
		if (this.m_areaMarker)
		{
			this.m_areaMarker.SetActive(false);
		}
		if (this.m_craftRequireFire)
		{
			base.InvokeRepeating("CheckFire", 1f, 1f);
		}
	}

	// Token: 0x06000B0B RID: 2827 RVA: 0x00051E19 File Offset: 0x00050019
	private void OnDestroy()
	{
		CraftingStation.m_allStations.Remove(this);
	}

	// Token: 0x06000B0C RID: 2828 RVA: 0x00051E28 File Offset: 0x00050028
	public bool Interact(Humanoid user, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (user == Player.m_localPlayer)
		{
			if (!this.InUseDistance(user))
			{
				return false;
			}
			Player player = user as Player;
			if (this.CheckUsable(player, true))
			{
				player.SetCraftingStation(this);
				InventoryGui.instance.Show(null, 3);
				return false;
			}
		}
		return false;
	}

	// Token: 0x06000B0D RID: 2829 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000B0E RID: 2830 RVA: 0x00051E7C File Offset: 0x0005007C
	public bool CheckUsable(Player player, bool showMessage)
	{
		if (this.m_craftRequireRoof)
		{
			float num;
			bool flag;
			Cover.GetCoverForPoint(this.m_roofCheckPoint.position, out num, out flag, 0.5f);
			if (!flag)
			{
				if (showMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_stationneedroof", 0, null);
				}
				return false;
			}
			if (num < 0.7f)
			{
				if (showMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_stationtooexposed", 0, null);
				}
				return false;
			}
		}
		if (this.m_craftRequireFire && !this.m_haveFire)
		{
			if (showMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_needfire", 0, null);
			}
			return false;
		}
		return true;
	}

	// Token: 0x06000B0F RID: 2831 RVA: 0x00051EFF File Offset: 0x000500FF
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use ");
	}

	// Token: 0x06000B10 RID: 2832 RVA: 0x00051F38 File Offset: 0x00050138
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000B11 RID: 2833 RVA: 0x00051F40 File Offset: 0x00050140
	public void ShowAreaMarker()
	{
		if (this.m_areaMarker)
		{
			this.m_areaMarker.SetActive(true);
			base.CancelInvoke("HideMarker");
			base.Invoke("HideMarker", 0.5f);
			this.PokeInUse();
		}
	}

	// Token: 0x06000B12 RID: 2834 RVA: 0x00051F7C File Offset: 0x0005017C
	private void HideMarker()
	{
		this.m_areaMarker.SetActive(false);
	}

	// Token: 0x06000B13 RID: 2835 RVA: 0x00051F8C File Offset: 0x0005018C
	public static void UpdateKnownStationsInRange(Player player)
	{
		Vector3 position = player.transform.position;
		foreach (CraftingStation craftingStation in CraftingStation.m_allStations)
		{
			if (Vector3.Distance(craftingStation.transform.position, position) < craftingStation.m_discoverRange)
			{
				player.AddKnownStation(craftingStation);
			}
		}
	}

	// Token: 0x06000B14 RID: 2836 RVA: 0x00052004 File Offset: 0x00050204
	private void FixedUpdate()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		this.m_useTimer += Time.fixedDeltaTime;
		this.m_updateExtensionTimer += Time.fixedDeltaTime;
		if (this.m_inUseObject)
		{
			this.m_inUseObject.SetActive(this.m_useTimer < 1f);
		}
	}

	// Token: 0x06000B15 RID: 2837 RVA: 0x00052078 File Offset: 0x00050278
	private void CheckFire()
	{
		this.m_haveFire = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Burning, 0.25f);
		if (this.m_haveFireObject)
		{
			this.m_haveFireObject.SetActive(this.m_haveFire);
		}
	}

	// Token: 0x06000B16 RID: 2838 RVA: 0x000520C4 File Offset: 0x000502C4
	public void PokeInUse()
	{
		this.m_useTimer = 0f;
		this.TriggerExtensionEffects();
	}

	// Token: 0x06000B17 RID: 2839 RVA: 0x000520D8 File Offset: 0x000502D8
	public static CraftingStation GetCraftingStation(Vector3 point)
	{
		if (CraftingStation.m_triggerMask == 0)
		{
			CraftingStation.m_triggerMask = LayerMask.GetMask(new string[]
			{
				"character_trigger"
			});
		}
		foreach (Collider collider in Physics.OverlapSphere(point, 0.1f, CraftingStation.m_triggerMask, QueryTriggerInteraction.Collide))
		{
			if (collider.gameObject.CompareTag("StationUseArea"))
			{
				CraftingStation componentInParent = collider.GetComponentInParent<CraftingStation>();
				if (componentInParent != null)
				{
					return componentInParent;
				}
			}
		}
		return null;
	}

	// Token: 0x06000B18 RID: 2840 RVA: 0x00052150 File Offset: 0x00050350
	public static CraftingStation HaveBuildStationInRange(string name, Vector3 point)
	{
		foreach (CraftingStation craftingStation in CraftingStation.m_allStations)
		{
			if (!(craftingStation.m_name != name))
			{
				float rangeBuild = craftingStation.m_rangeBuild;
				if (Vector3.Distance(craftingStation.transform.position, point) < rangeBuild)
				{
					return craftingStation;
				}
			}
		}
		return null;
	}

	// Token: 0x06000B19 RID: 2841 RVA: 0x000521CC File Offset: 0x000503CC
	public static void FindStationsInRange(string name, Vector3 point, float range, List<CraftingStation> stations)
	{
		foreach (CraftingStation craftingStation in CraftingStation.m_allStations)
		{
			if (!(craftingStation.m_name != name) && Vector3.Distance(craftingStation.transform.position, point) < range)
			{
				stations.Add(craftingStation);
			}
		}
	}

	// Token: 0x06000B1A RID: 2842 RVA: 0x00052240 File Offset: 0x00050440
	public static CraftingStation FindClosestStationInRange(string name, Vector3 point, float range)
	{
		CraftingStation craftingStation = null;
		float num = 99999f;
		foreach (CraftingStation craftingStation2 in CraftingStation.m_allStations)
		{
			if (!(craftingStation2.m_name != name))
			{
				float num2 = Vector3.Distance(craftingStation2.transform.position, point);
				if (num2 < range && (num2 < num || craftingStation == null))
				{
					craftingStation = craftingStation2;
					num = num2;
				}
			}
		}
		return craftingStation;
	}

	// Token: 0x06000B1B RID: 2843 RVA: 0x000522D0 File Offset: 0x000504D0
	private List<StationExtension> GetExtensions()
	{
		if (this.m_updateExtensionTimer > 2f)
		{
			this.m_updateExtensionTimer = 0f;
			this.m_attachedExtensions.Clear();
			StationExtension.FindExtensions(this, base.transform.position, this.m_attachedExtensions);
		}
		return this.m_attachedExtensions;
	}

	// Token: 0x06000B1C RID: 2844 RVA: 0x00052320 File Offset: 0x00050520
	private void TriggerExtensionEffects()
	{
		Vector3 connectionEffectPoint = this.GetConnectionEffectPoint();
		foreach (StationExtension stationExtension in this.GetExtensions())
		{
			if (stationExtension)
			{
				stationExtension.StartConnectionEffect(connectionEffectPoint, 1f);
			}
		}
	}

	// Token: 0x06000B1D RID: 2845 RVA: 0x00052388 File Offset: 0x00050588
	public Vector3 GetConnectionEffectPoint()
	{
		if (this.m_connectionPoint)
		{
			return this.m_connectionPoint.position;
		}
		return base.transform.position;
	}

	// Token: 0x06000B1E RID: 2846 RVA: 0x000523AE File Offset: 0x000505AE
	public int GetLevel()
	{
		return 1 + this.GetExtensions().Count;
	}

	// Token: 0x06000B1F RID: 2847 RVA: 0x000523BD File Offset: 0x000505BD
	public bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, base.transform.position) < this.m_useDistance;
	}

	// Token: 0x04000D3F RID: 3391
	public string m_name = "";

	// Token: 0x04000D40 RID: 3392
	public Sprite m_icon;

	// Token: 0x04000D41 RID: 3393
	public float m_discoverRange = 4f;

	// Token: 0x04000D42 RID: 3394
	public float m_rangeBuild = 10f;

	// Token: 0x04000D43 RID: 3395
	public bool m_craftRequireRoof = true;

	// Token: 0x04000D44 RID: 3396
	public bool m_craftRequireFire = true;

	// Token: 0x04000D45 RID: 3397
	public Transform m_roofCheckPoint;

	// Token: 0x04000D46 RID: 3398
	public Transform m_connectionPoint;

	// Token: 0x04000D47 RID: 3399
	public bool m_showBasicRecipies;

	// Token: 0x04000D48 RID: 3400
	public float m_useDistance = 2f;

	// Token: 0x04000D49 RID: 3401
	public int m_useAnimation;

	// Token: 0x04000D4A RID: 3402
	public GameObject m_areaMarker;

	// Token: 0x04000D4B RID: 3403
	public GameObject m_inUseObject;

	// Token: 0x04000D4C RID: 3404
	public GameObject m_haveFireObject;

	// Token: 0x04000D4D RID: 3405
	public EffectList m_craftItemEffects = new EffectList();

	// Token: 0x04000D4E RID: 3406
	public EffectList m_craftItemDoneEffects = new EffectList();

	// Token: 0x04000D4F RID: 3407
	public EffectList m_repairItemDoneEffects = new EffectList();

	// Token: 0x04000D50 RID: 3408
	private const float m_updateExtensionInterval = 2f;

	// Token: 0x04000D51 RID: 3409
	private float m_updateExtensionTimer;

	// Token: 0x04000D52 RID: 3410
	private float m_useTimer = 10f;

	// Token: 0x04000D53 RID: 3411
	private bool m_haveFire;

	// Token: 0x04000D54 RID: 3412
	private ZNetView m_nview;

	// Token: 0x04000D55 RID: 3413
	private List<StationExtension> m_attachedExtensions = new List<StationExtension>();

	// Token: 0x04000D56 RID: 3414
	private static List<CraftingStation> m_allStations = new List<CraftingStation>();

	// Token: 0x04000D57 RID: 3415
	private static int m_triggerMask = 0;
}
