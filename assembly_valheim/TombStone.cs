using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200003A RID: 58
public class TombStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600036C RID: 876 RVA: 0x00019FD8 File Offset: 0x000181D8
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_container = base.GetComponent<Container>();
		this.m_floating = base.GetComponent<Floating>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_body.maxDepenetrationVelocity = 1f;
		this.m_body.solverIterations = 10;
		Container container = this.m_container;
		container.m_onTakeAllSuccess = (Action)Delegate.Combine(container.m_onTakeAllSuccess, new Action(this.OnTakeAllSuccess));
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(ZDOVars.s_timeOfDeath, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_timeOfDeath, ZNet.instance.GetTime().Ticks);
			this.m_nview.GetZDO().Set(ZDOVars.s_spawnPoint, base.transform.position);
		}
		base.InvokeRepeating("UpdateDespawn", TombStone.m_updateDt, TombStone.m_updateDt);
	}

	// Token: 0x0600036D RID: 877 RVA: 0x0001A0DC File Offset: 0x000182DC
	private void Start()
	{
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_ownerName, "");
		base.GetComponent<Container>().m_name = @string;
		this.m_worldText.text = @string;
	}

	// Token: 0x0600036E RID: 878 RVA: 0x0001A11C File Offset: 0x0001831C
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_ownerName, "");
		string str = this.m_text + " " + @string;
		if (this.m_container.GetInventory().NrOfItems() == 0)
		{
			return "";
		}
		return Localization.instance.Localize(str + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");
	}

	// Token: 0x0600036F RID: 879 RVA: 0x0000C988 File Offset: 0x0000AB88
	public string GetHoverName()
	{
		return "";
	}

	// Token: 0x06000370 RID: 880 RVA: 0x0001A198 File Offset: 0x00018398
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_container.GetInventory().NrOfItems() == 0)
		{
			return false;
		}
		if (this.IsOwner())
		{
			Player player = character as Player;
			if (this.EasyFitInInventory(player))
			{
				ZLog.Log("Grave should fit in inventory, loot all");
				this.m_container.TakeAll(character);
				return true;
			}
		}
		return this.m_container.Interact(character, false, false);
	}

	// Token: 0x06000371 RID: 881 RVA: 0x0001A200 File Offset: 0x00018400
	private void OnTakeAllSuccess()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			localPlayer.m_pickupEffects.Create(localPlayer.transform.position, Quaternion.identity, null, 1f, -1);
			localPlayer.Message(MessageHud.MessageType.Center, "$piece_tombstone_recovered", 0, null);
		}
	}

	// Token: 0x06000372 RID: 882 RVA: 0x0001A24C File Offset: 0x0001844C
	private bool EasyFitInInventory(Player player)
	{
		int num = player.GetInventory().GetEmptySlots() - this.m_container.GetInventory().NrOfItems();
		if (num < 0)
		{
			foreach (ItemDrop.ItemData itemData in this.m_container.GetInventory().GetAllItems())
			{
				if (player.GetInventory().FindFreeStackSpace(itemData.m_shared.m_name) >= itemData.m_stack)
				{
					num++;
				}
			}
			if (num < 0)
			{
				return false;
			}
		}
		return player.GetInventory().GetTotalWeight() + this.m_container.GetInventory().GetTotalWeight() <= player.GetMaxCarryWeight();
	}

	// Token: 0x06000373 RID: 883 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000374 RID: 884 RVA: 0x0001A314 File Offset: 0x00018514
	public void Setup(string ownerName, long ownerUID)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_ownerName, ownerName);
		this.m_nview.GetZDO().Set(ZDOVars.s_owner, ownerUID);
		if (this.m_body)
		{
			this.m_body.velocity = new Vector3(0f, this.m_spawnUpVel, 0f);
		}
	}

	// Token: 0x06000375 RID: 885 RVA: 0x0001A37A File Offset: 0x0001857A
	private long GetOwner()
	{
		if (this.m_nview.IsValid())
		{
			return this.m_nview.GetZDO().GetLong(ZDOVars.s_owner, 0L);
		}
		return 0L;
	}

	// Token: 0x06000376 RID: 886 RVA: 0x0001A3A4 File Offset: 0x000185A4
	private bool IsOwner()
	{
		long owner = this.GetOwner();
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		return owner == playerID;
	}

	// Token: 0x06000377 RID: 887 RVA: 0x0001A3CC File Offset: 0x000185CC
	private void UpdateDespawn()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_floater != null)
		{
			this.UpdateFloater();
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.PositionCheck();
		if (!this.m_container.IsInUse() && this.m_container.GetInventory().NrOfItems() <= 0)
		{
			this.GiveBoost();
			this.m_removeEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06000378 RID: 888 RVA: 0x0001A46C File Offset: 0x0001866C
	private void GiveBoost()
	{
		if (this.m_lootStatusEffect == null)
		{
			return;
		}
		Player player = this.FindOwner();
		if (player)
		{
			player.GetSEMan().AddStatusEffect(this.m_lootStatusEffect.NameHash(), true, 0, 0f);
		}
	}

	// Token: 0x06000379 RID: 889 RVA: 0x0001A4B8 File Offset: 0x000186B8
	private Player FindOwner()
	{
		long owner = this.GetOwner();
		if (owner == 0L)
		{
			return null;
		}
		return Player.GetPlayer(owner);
	}

	// Token: 0x0600037A RID: 890 RVA: 0x0001A4D8 File Offset: 0x000186D8
	private void PositionCheck()
	{
		Vector3 vec = this.m_nview.GetZDO().GetVec3(ZDOVars.s_spawnPoint, base.transform.position);
		if (Utils.DistanceXZ(vec, base.transform.position) > 4f)
		{
			ZLog.Log("Tombstone moved too far from spawn position, reseting position");
			base.transform.position = vec;
			this.m_body.position = vec;
			this.m_body.velocity = Vector3.zero;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		if (base.transform.position.y < groundHeight - 1f)
		{
			Vector3 position = base.transform.position;
			position.y = groundHeight + 0.5f;
			base.transform.position = position;
			this.m_body.position = position;
			this.m_body.velocity = Vector3.zero;
		}
	}

	// Token: 0x0600037B RID: 891 RVA: 0x0001A5C4 File Offset: 0x000187C4
	private void UpdateFloater()
	{
		if (this.m_nview.IsOwner())
		{
			bool flag = this.m_floating.BeenFloating();
			this.m_nview.GetZDO().Set(ZDOVars.s_inWater, flag);
			this.m_floater.SetActive(flag);
			return;
		}
		bool @bool = this.m_nview.GetZDO().GetBool(ZDOVars.s_inWater, false);
		this.m_floater.SetActive(@bool);
	}

	// Token: 0x04000350 RID: 848
	private static float m_updateDt = 2f;

	// Token: 0x04000351 RID: 849
	public string m_text = "$piece_tombstone";

	// Token: 0x04000352 RID: 850
	public GameObject m_floater;

	// Token: 0x04000353 RID: 851
	public Text m_worldText;

	// Token: 0x04000354 RID: 852
	public float m_spawnUpVel = 5f;

	// Token: 0x04000355 RID: 853
	public StatusEffect m_lootStatusEffect;

	// Token: 0x04000356 RID: 854
	public EffectList m_removeEffect = new EffectList();

	// Token: 0x04000357 RID: 855
	private Container m_container;

	// Token: 0x04000358 RID: 856
	private ZNetView m_nview;

	// Token: 0x04000359 RID: 857
	private Floating m_floating;

	// Token: 0x0400035A RID: 858
	private Rigidbody m_body;
}
