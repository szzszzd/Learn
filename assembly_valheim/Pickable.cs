using System;
using UnityEngine;

// Token: 0x0200012C RID: 300
public class Pickable : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000BB3 RID: 2995 RVA: 0x000565C4 File Offset: 0x000547C4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null)
		{
			return;
		}
		this.m_nview.Register<bool>("SetPicked", new Action<long, bool>(this.RPC_SetPicked));
		this.m_nview.Register("Pick", new Action<long>(this.RPC_Pick));
		this.m_picked = zdo.GetBool(ZDOVars.s_picked, false);
		if (this.m_picked && this.m_hideWhenPicked)
		{
			this.m_hideWhenPicked.SetActive(false);
		}
		if (this.m_respawnTimeMinutes > 0)
		{
			base.InvokeRepeating("UpdateRespawn", UnityEngine.Random.Range(1f, 5f), 60f);
		}
		if (this.m_respawnTimeMinutes <= 0 && this.m_hideWhenPicked == null && this.m_nview.GetZDO().GetBool(ZDOVars.s_picked, false))
		{
			this.m_nview.ClaimOwnership();
			this.m_nview.Destroy();
			ZLog.Log("Destroying old picked " + base.name);
		}
	}

	// Token: 0x06000BB4 RID: 2996 RVA: 0x000566DA File Offset: 0x000548DA
	public string GetHoverText()
	{
		if (this.m_picked)
		{
			return "";
		}
		return Localization.instance.Localize(this.GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	// Token: 0x06000BB5 RID: 2997 RVA: 0x00056704 File Offset: 0x00054904
	public string GetHoverName()
	{
		if (!string.IsNullOrEmpty(this.m_overrideName))
		{
			return this.m_overrideName;
		}
		return this.m_itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
	}

	// Token: 0x06000BB6 RID: 2998 RVA: 0x00056734 File Offset: 0x00054934
	private void UpdateRespawn()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_picked)
		{
			return;
		}
		long @long = this.m_nview.GetZDO().GetLong(ZDOVars.s_pickedTime, 0L);
		DateTime d = new DateTime(@long);
		if ((ZNet.instance.GetTime() - d).TotalMinutes > (double)this.m_respawnTimeMinutes)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "SetPicked", new object[]
			{
				false
			});
		}
	}

	// Token: 0x06000BB7 RID: 2999 RVA: 0x000567CC File Offset: 0x000549CC
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_tarPreventsPicking)
		{
			if (this.m_floating == null)
			{
				this.m_floating = base.GetComponent<Floating>();
			}
			if (this.m_floating && this.m_floating.IsInTar())
			{
				character.Message(MessageHud.MessageType.Center, "$hud_itemstucktar", 0, null);
				return this.m_useInteractAnimation;
			}
		}
		this.m_nview.InvokeRPC("Pick", Array.Empty<object>());
		return this.m_useInteractAnimation;
	}

	// Token: 0x06000BB8 RID: 3000 RVA: 0x00056854 File Offset: 0x00054A54
	private void RPC_Pick(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_picked)
		{
			return;
		}
		Vector3 basePos = this.m_pickEffectAtSpawnPoint ? (base.transform.position + Vector3.up * this.m_spawnOffset) : base.transform.position;
		this.m_pickEffector.Create(basePos, Quaternion.identity, null, 1f, -1);
		int num = 0;
		for (int i = 0; i < this.m_amount; i++)
		{
			this.Drop(this.m_itemPrefab, num++, 1);
		}
		if (!this.m_extraDrops.IsEmpty())
		{
			foreach (ItemDrop.ItemData itemData in this.m_extraDrops.GetDropListItems())
			{
				this.Drop(itemData.m_dropPrefab, num++, itemData.m_stack);
			}
		}
		if (this.m_aggravateRange > 0f)
		{
			BaseAI.AggravateAllInArea(base.transform.position, this.m_aggravateRange, BaseAI.AggravatedReason.Theif);
		}
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetPicked", new object[]
		{
			true
		});
	}

	// Token: 0x06000BB9 RID: 3001 RVA: 0x000569A0 File Offset: 0x00054BA0
	private void RPC_SetPicked(long sender, bool picked)
	{
		this.SetPicked(picked);
	}

	// Token: 0x06000BBA RID: 3002 RVA: 0x000569AC File Offset: 0x00054BAC
	private void SetPicked(bool picked)
	{
		this.m_picked = picked;
		if (this.m_hideWhenPicked)
		{
			this.m_hideWhenPicked.SetActive(!picked);
		}
		if (this.m_nview.IsOwner())
		{
			if (this.m_respawnTimeMinutes > 0 || this.m_hideWhenPicked != null)
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_picked, this.m_picked);
				if (picked && this.m_respawnTimeMinutes > 0)
				{
					DateTime time = ZNet.instance.GetTime();
					this.m_nview.GetZDO().Set(ZDOVars.s_pickedTime, time.Ticks);
					return;
				}
			}
			else if (picked)
			{
				this.m_nview.Destroy();
			}
		}
	}

	// Token: 0x06000BBB RID: 3003 RVA: 0x00056A60 File Offset: 0x00054C60
	private void Drop(GameObject prefab, int offset, int stack)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.2f;
		Vector3 position = base.transform.position + Vector3.up * this.m_spawnOffset + new Vector3(vector.x, 0.5f * (float)offset, vector.y);
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
		gameObject.GetComponent<ItemDrop>().SetStack(stack);
		gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
	}

	// Token: 0x06000BBC RID: 3004 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x04000E20 RID: 3616
	public GameObject m_hideWhenPicked;

	// Token: 0x04000E21 RID: 3617
	public GameObject m_itemPrefab;

	// Token: 0x04000E22 RID: 3618
	public int m_amount = 1;

	// Token: 0x04000E23 RID: 3619
	public DropTable m_extraDrops = new DropTable();

	// Token: 0x04000E24 RID: 3620
	public string m_overrideName = "";

	// Token: 0x04000E25 RID: 3621
	public int m_respawnTimeMinutes;

	// Token: 0x04000E26 RID: 3622
	public float m_spawnOffset = 0.5f;

	// Token: 0x04000E27 RID: 3623
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x04000E28 RID: 3624
	public bool m_pickEffectAtSpawnPoint;

	// Token: 0x04000E29 RID: 3625
	public bool m_useInteractAnimation;

	// Token: 0x04000E2A RID: 3626
	public bool m_tarPreventsPicking;

	// Token: 0x04000E2B RID: 3627
	public float m_aggravateRange;

	// Token: 0x04000E2C RID: 3628
	private ZNetView m_nview;

	// Token: 0x04000E2D RID: 3629
	private Floating m_floating;

	// Token: 0x04000E2E RID: 3630
	private bool m_picked;
}
