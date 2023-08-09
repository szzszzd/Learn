using System;
using UnityEngine;

// Token: 0x02000217 RID: 535
public class Bed : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600153F RID: 5439 RVA: 0x0008B9A7 File Offset: 0x00089BA7
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<long, string>("SetOwner", new Action<long, long, string>(this.RPC_SetOwner));
	}

	// Token: 0x06001540 RID: 5440 RVA: 0x0008B9E0 File Offset: 0x00089BE0
	public string GetHoverText()
	{
		string ownerName = this.GetOwnerName();
		if (ownerName == "")
		{
			return Localization.instance.Localize("$piece_bed_unclaimed\n[<color=yellow><b>$KEY_Use</b></color>] $piece_bed_claim");
		}
		string text = ownerName + "'s $piece_bed";
		if (!this.IsMine())
		{
			return Localization.instance.Localize(text);
		}
		if (this.IsCurrent())
		{
			return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_bed_sleep");
		}
		return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_bed_setspawn");
	}

	// Token: 0x06001541 RID: 5441 RVA: 0x0008BA64 File Offset: 0x00089C64
	public string GetHoverName()
	{
		return Localization.instance.Localize("$piece_bed");
	}

	// Token: 0x06001542 RID: 5442 RVA: 0x0008BA78 File Offset: 0x00089C78
	public bool Interact(Humanoid human, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (this.m_nview.GetZDO() == null)
		{
			return false;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		bool owner = this.GetOwner() != 0L;
		Player human2 = human as Player;
		if (!owner)
		{
			ZLog.Log("Has no creator");
			if (!this.CheckExposure(human2))
			{
				return false;
			}
			this.SetOwner(playerID, Game.instance.GetPlayerProfile().GetName());
			Game.instance.GetPlayerProfile().SetCustomSpawnPoint(this.GetSpawnPoint());
			human.Message(MessageHud.MessageType.Center, "$msg_spawnpointset", 0, null);
		}
		else if (this.IsMine())
		{
			ZLog.Log("Is mine");
			if (this.IsCurrent())
			{
				ZLog.Log("is current spawnpoint");
				if (!EnvMan.instance.CanSleep())
				{
					human.Message(MessageHud.MessageType.Center, "$msg_cantsleep", 0, null);
					return false;
				}
				if (!this.CheckEnemies(human2))
				{
					return false;
				}
				if (!this.CheckExposure(human2))
				{
					return false;
				}
				if (!this.CheckFire(human2))
				{
					return false;
				}
				if (!this.CheckWet(human2))
				{
					return false;
				}
				human.AttachStart(this.m_spawnPoint, base.gameObject, true, true, false, "attach_bed", new Vector3(0f, 0.5f, 0f));
				return false;
			}
			else
			{
				ZLog.Log("Not current spawn point");
				if (!this.CheckExposure(human2))
				{
					return false;
				}
				Game.instance.GetPlayerProfile().SetCustomSpawnPoint(this.GetSpawnPoint());
				human.Message(MessageHud.MessageType.Center, "$msg_spawnpointset", 0, null);
			}
		}
		return false;
	}

	// Token: 0x06001543 RID: 5443 RVA: 0x0008BBE6 File Offset: 0x00089DE6
	private bool CheckWet(Player human)
	{
		if (human.GetSEMan().HaveStatusEffect("Wet"))
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedwet", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06001544 RID: 5444 RVA: 0x0008BC0B File Offset: 0x00089E0B
	private bool CheckEnemies(Player human)
	{
		if (human.IsSensed())
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedenemiesnearby", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06001545 RID: 5445 RVA: 0x0008BC28 File Offset: 0x00089E28
	private bool CheckExposure(Player human)
	{
		float num;
		bool flag;
		Cover.GetCoverForPoint(this.GetSpawnPoint(), out num, out flag, 0.5f);
		if (!flag)
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedneedroof", 0, null);
			return false;
		}
		if (num < 0.8f)
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedtooexposed", 0, null);
			return false;
		}
		ZLog.Log("exporeusre check " + num.ToString() + "  " + flag.ToString());
		return true;
	}

	// Token: 0x06001546 RID: 5446 RVA: 0x0008BC97 File Offset: 0x00089E97
	private bool CheckFire(Player human)
	{
		if (!EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Heat, 0f))
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bednofire", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06001547 RID: 5447 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001548 RID: 5448 RVA: 0x0008BCC7 File Offset: 0x00089EC7
	public bool IsCurrent()
	{
		return this.IsMine() && Vector3.Distance(this.GetSpawnPoint(), Game.instance.GetPlayerProfile().GetCustomSpawnPoint()) < 1f;
	}

	// Token: 0x06001549 RID: 5449 RVA: 0x0008BCF4 File Offset: 0x00089EF4
	public Vector3 GetSpawnPoint()
	{
		return this.m_spawnPoint.position;
	}

	// Token: 0x0600154A RID: 5450 RVA: 0x0008BD04 File Offset: 0x00089F04
	private bool IsMine()
	{
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		long owner = this.GetOwner();
		return playerID == owner;
	}

	// Token: 0x0600154B RID: 5451 RVA: 0x0008BD2A File Offset: 0x00089F2A
	private void SetOwner(long uid, string name)
	{
		this.m_nview.InvokeRPC("SetOwner", new object[]
		{
			uid,
			name
		});
	}

	// Token: 0x0600154C RID: 5452 RVA: 0x0008BD4F File Offset: 0x00089F4F
	private void RPC_SetOwner(long sender, long uid, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_owner, uid);
		this.m_nview.GetZDO().Set(ZDOVars.s_ownerName, name);
	}

	// Token: 0x0600154D RID: 5453 RVA: 0x0008BD8B File Offset: 0x00089F8B
	private long GetOwner()
	{
		if (this.m_nview.GetZDO() == null)
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetLong(ZDOVars.s_owner, 0L);
	}

	// Token: 0x0600154E RID: 5454 RVA: 0x0008BDB4 File Offset: 0x00089FB4
	private string GetOwnerName()
	{
		return this.m_nview.GetZDO().GetString(ZDOVars.s_ownerName, "");
	}

	// Token: 0x04001613 RID: 5651
	public Transform m_spawnPoint;

	// Token: 0x04001614 RID: 5652
	public float m_monsterCheckRadius = 20f;

	// Token: 0x04001615 RID: 5653
	private ZNetView m_nview;
}
