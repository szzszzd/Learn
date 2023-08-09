using System;
using UnityEngine;

// Token: 0x02000236 RID: 566
public class Fireplace : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001629 RID: 5673 RVA: 0x000916BC File Offset: 0x0008F8BC
	public void Awake()
	{
		this.m_nview = base.gameObject.GetComponent<ZNetView>();
		this.m_piece = base.gameObject.GetComponent<Piece>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (Fireplace.m_solidRayMask == 0)
		{
			Fireplace.m_solidRayMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain"
			});
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, -1f) == -1f)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_fuel, this.m_startFuel);
			if (this.m_startFuel > 0f)
			{
				this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			}
		}
		this.m_nview.Register("AddFuel", new Action<long>(this.RPC_AddFuel));
		base.InvokeRepeating("UpdateFireplace", 0f, 2f);
		base.InvokeRepeating("CheckEnv", 4f, 4f);
	}

	// Token: 0x0600162A RID: 5674 RVA: 0x000917FD File Offset: 0x0008F9FD
	private void Start()
	{
		if (this.m_playerBaseObject && this.m_piece)
		{
			this.m_playerBaseObject.SetActive(this.m_piece.IsPlacedByPlayer());
		}
	}

	// Token: 0x0600162B RID: 5675 RVA: 0x00091830 File Offset: 0x0008FA30
	private double GetTimeSinceLastUpdate()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_lastTime, time.Ticks));
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set(ZDOVars.s_lastTime, time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return num;
	}

	// Token: 0x0600162C RID: 5676 RVA: 0x000918B0 File Offset: 0x0008FAB0
	private void UpdateFireplace()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.m_secPerFuel > 0f)
		{
			float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
			double timeSinceLastUpdate = this.GetTimeSinceLastUpdate();
			if (this.IsBurning())
			{
				float num2 = (float)(timeSinceLastUpdate / (double)this.m_secPerFuel);
				num -= num2;
				if (num <= 0f)
				{
					num = 0f;
				}
				this.m_nview.GetZDO().Set(ZDOVars.s_fuel, num);
			}
		}
		this.UpdateState();
	}

	// Token: 0x0600162D RID: 5677 RVA: 0x00091948 File Offset: 0x0008FB48
	private void CheckEnv()
	{
		this.CheckUnderTerrain();
		if (this.m_enabledObjectLow != null && this.m_enabledObjectHigh != null)
		{
			this.CheckWet();
		}
	}

	// Token: 0x0600162E RID: 5678 RVA: 0x00091974 File Offset: 0x0008FB74
	private void CheckUnderTerrain()
	{
		this.m_blocked = false;
		float num;
		if (Heightmap.GetHeight(base.transform.position, out num) && num > base.transform.position.y + this.m_checkTerrainOffset)
		{
			this.m_blocked = true;
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position + Vector3.up * this.m_coverCheckOffset, Vector3.up, out raycastHit, 0.5f, Fireplace.m_solidRayMask))
		{
			this.m_blocked = true;
			return;
		}
		if (this.m_smokeSpawner && this.m_smokeSpawner.IsBlocked())
		{
			this.m_blocked = true;
			return;
		}
	}

	// Token: 0x0600162F RID: 5679 RVA: 0x00091A20 File Offset: 0x0008FC20
	private void CheckWet()
	{
		float num;
		bool flag;
		Cover.GetCoverForPoint(base.transform.position + Vector3.up * this.m_coverCheckOffset, out num, out flag, 0.5f);
		this.m_wet = false;
		if (EnvMan.instance.GetWindIntensity() >= 0.8f && num < 0.7f)
		{
			this.m_wet = true;
		}
		if (EnvMan.instance.IsWet() && !flag)
		{
			this.m_wet = true;
		}
	}

	// Token: 0x06001630 RID: 5680 RVA: 0x00091A98 File Offset: 0x0008FC98
	private void UpdateState()
	{
		if (this.IsBurning())
		{
			this.m_enabledObject.SetActive(true);
			if (this.m_enabledObjectHigh && this.m_enabledObjectLow)
			{
				this.m_enabledObjectHigh.SetActive(!this.m_wet);
				this.m_enabledObjectLow.SetActive(this.m_wet);
				return;
			}
		}
		else
		{
			this.m_enabledObject.SetActive(false);
			if (this.m_enabledObjectHigh && this.m_enabledObjectLow)
			{
				this.m_enabledObjectLow.SetActive(false);
				this.m_enabledObjectHigh.SetActive(false);
			}
		}
	}

	// Token: 0x06001631 RID: 5681 RVA: 0x00091B38 File Offset: 0x0008FD38
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		float @float = this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
		return Localization.instance.Localize(string.Concat(new string[]
		{
			this.m_name,
			" ( $piece_fire_fuel ",
			Mathf.Ceil(@float).ToString(),
			"/",
			((int)this.m_maxFuel).ToString(),
			" )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use ",
			this.m_fuelItem.m_itemData.m_shared.m_name,
			"\n[<color=yellow><b>1-8</b></color>] $piece_useitem"
		}));
	}

	// Token: 0x06001632 RID: 5682 RVA: 0x00091BEC File Offset: 0x0008FDEC
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001633 RID: 5683 RVA: 0x00091BF4 File Offset: 0x0008FDF4
	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (hold)
		{
			if (this.m_holdRepeatInterval <= 0f)
			{
				return false;
			}
			if (Time.time - this.m_lastUseTime < this.m_holdRepeatInterval)
			{
				return false;
			}
		}
		if (!this.m_nview.HasOwner())
		{
			this.m_nview.ClaimOwnership();
		}
		Inventory inventory = user.GetInventory();
		if (inventory == null)
		{
			return true;
		}
		if (!inventory.HaveItem(this.m_fuelItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_outof " + this.m_fuelItem.m_itemData.m_shared.m_name, 0, null);
			return false;
		}
		if ((float)Mathf.CeilToInt(this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f)) >= this.m_maxFuel)
		{
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[]
			{
				this.m_fuelItem.m_itemData.m_shared.m_name
			}), 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[]
		{
			this.m_fuelItem.m_itemData.m_shared.m_name
		}), 0, null);
		inventory.RemoveItem(this.m_fuelItem.m_itemData.m_shared.m_name, 1, -1);
		this.m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
		return true;
	}

	// Token: 0x06001634 RID: 5684 RVA: 0x00091D64 File Offset: 0x0008FF64
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (item.m_shared.m_name == this.m_fuelItem.m_itemData.m_shared.m_name)
		{
			if ((float)Mathf.CeilToInt(this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f)) >= this.m_maxFuel)
			{
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[]
				{
					item.m_shared.m_name
				}), 0, null);
				return true;
			}
			Inventory inventory = user.GetInventory();
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[]
			{
				item.m_shared.m_name
			}), 0, null);
			inventory.RemoveItem(item, 1);
			this.m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
			return true;
		}
		else
		{
			if (!(this.m_fireworkItem != null) || !(item.m_shared.m_name == this.m_fireworkItem.m_itemData.m_shared.m_name))
			{
				return false;
			}
			if (!this.IsBurning())
			{
				user.Message(MessageHud.MessageType.Center, "$msg_firenotburning", 0, null);
				return true;
			}
			if (user.GetInventory().CountItems(this.m_fireworkItem.m_itemData.m_shared.m_name, -1) < this.m_fireworkItems)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_toofew " + this.m_fireworkItem.m_itemData.m_shared.m_name, 0, null);
				return true;
			}
			user.GetInventory().RemoveItem(item.m_shared.m_name, this.m_fireworkItems, -1);
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_throwinfire", new string[]
			{
				item.m_shared.m_name
			}), 0, null);
			ZNetScene.instance.SpawnObject(base.transform.position, Quaternion.identity, this.m_fireworks);
			this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			return true;
		}
	}

	// Token: 0x06001635 RID: 5685 RVA: 0x00091F80 File Offset: 0x00090180
	private void RPC_AddFuel(long sender)
	{
		if (this.m_nview.IsOwner())
		{
			float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
			if ((float)Mathf.CeilToInt(num) >= this.m_maxFuel)
			{
				return;
			}
			num = Mathf.Clamp(num, 0f, this.m_maxFuel);
			num += 1f;
			num = Mathf.Clamp(num, 0f, this.m_maxFuel);
			this.m_nview.GetZDO().Set(ZDOVars.s_fuel, num);
			this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			this.UpdateState();
		}
	}

	// Token: 0x06001636 RID: 5686 RVA: 0x00092039 File Offset: 0x00090239
	public bool CanBeRemoved()
	{
		return !this.IsBurning();
	}

	// Token: 0x06001637 RID: 5687 RVA: 0x00092044 File Offset: 0x00090244
	public bool IsBurning()
	{
		if (this.m_blocked)
		{
			return false;
		}
		float liquidLevel = Floating.GetLiquidLevel(this.m_enabledObject.transform.position, 1f, LiquidType.All);
		return this.m_enabledObject.transform.position.y >= liquidLevel && this.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f) > 0f;
	}

	// Token: 0x06001638 RID: 5688 RVA: 0x000920B4 File Offset: 0x000902B4
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position + Vector3.up * this.m_coverCheckOffset, 0.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_checkTerrainOffset, new Vector3(1f, 0.01f, 1f));
	}

	// Token: 0x04001724 RID: 5924
	private ZNetView m_nview;

	// Token: 0x04001725 RID: 5925
	private Piece m_piece;

	// Token: 0x04001726 RID: 5926
	[Header("Fire")]
	public string m_name = "Fire";

	// Token: 0x04001727 RID: 5927
	public float m_startFuel = 3f;

	// Token: 0x04001728 RID: 5928
	public float m_maxFuel = 10f;

	// Token: 0x04001729 RID: 5929
	public float m_secPerFuel = 3f;

	// Token: 0x0400172A RID: 5930
	public float m_checkTerrainOffset = 0.2f;

	// Token: 0x0400172B RID: 5931
	public float m_coverCheckOffset = 0.5f;

	// Token: 0x0400172C RID: 5932
	private const float m_minimumOpenSpace = 0.5f;

	// Token: 0x0400172D RID: 5933
	public float m_holdRepeatInterval = 0.2f;

	// Token: 0x0400172E RID: 5934
	public GameObject m_enabledObject;

	// Token: 0x0400172F RID: 5935
	public GameObject m_enabledObjectLow;

	// Token: 0x04001730 RID: 5936
	public GameObject m_enabledObjectHigh;

	// Token: 0x04001731 RID: 5937
	public GameObject m_playerBaseObject;

	// Token: 0x04001732 RID: 5938
	public ItemDrop m_fuelItem;

	// Token: 0x04001733 RID: 5939
	public SmokeSpawner m_smokeSpawner;

	// Token: 0x04001734 RID: 5940
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x04001735 RID: 5941
	[Header("Fireworks")]
	public ItemDrop m_fireworkItem;

	// Token: 0x04001736 RID: 5942
	public int m_fireworkItems = 2;

	// Token: 0x04001737 RID: 5943
	public GameObject m_fireworks;

	// Token: 0x04001738 RID: 5944
	private bool m_blocked;

	// Token: 0x04001739 RID: 5945
	private bool m_wet;

	// Token: 0x0400173A RID: 5946
	private float m_lastUseTime;

	// Token: 0x0400173B RID: 5947
	private static int m_solidRayMask;
}
