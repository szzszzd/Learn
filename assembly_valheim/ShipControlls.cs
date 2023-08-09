using System;
using UnityEngine;

// Token: 0x0200028F RID: 655
public class ShipControlls : MonoBehaviour, Interactable, Hoverable, IDoodadController
{
	// Token: 0x06001910 RID: 6416 RVA: 0x000A70A8 File Offset: 0x000A52A8
	private void Awake()
	{
		this.m_nview = this.m_ship.GetComponent<ZNetView>();
		this.m_nview.Register<long>("RequestControl", new Action<long, long>(this.RPC_RequestControl));
		this.m_nview.Register<long>("ReleaseControl", new Action<long, long>(this.RPC_ReleaseControl));
		this.m_nview.Register<bool>("RequestRespons", new Action<long, bool>(this.RPC_RequestRespons));
	}

	// Token: 0x06001911 RID: 6417 RVA: 0x0001751D File Offset: 0x0001571D
	public bool IsValid()
	{
		return this;
	}

	// Token: 0x06001912 RID: 6418 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001913 RID: 6419 RVA: 0x000A711C File Offset: 0x000A531C
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.InUseDistance(character))
		{
			return false;
		}
		Player player = character as Player;
		if (player == null || player.IsEncumbered())
		{
			return false;
		}
		if (player.GetStandingOnShip() != this.m_ship)
		{
			return false;
		}
		this.m_nview.InvokeRPC("RequestControl", new object[]
		{
			player.GetPlayerID()
		});
		return false;
	}

	// Token: 0x06001914 RID: 6420 RVA: 0x000A719C File Offset: 0x000A539C
	public Component GetControlledComponent()
	{
		return this.m_ship;
	}

	// Token: 0x06001915 RID: 6421 RVA: 0x00017AB8 File Offset: 0x00015CB8
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x06001916 RID: 6422 RVA: 0x000A71A4 File Offset: 0x000A53A4
	public void ApplyControlls(Vector3 moveDir, Vector3 lookDir, bool run, bool autoRun, bool block)
	{
		this.m_ship.ApplyControlls(moveDir);
	}

	// Token: 0x06001917 RID: 6423 RVA: 0x000A71B2 File Offset: 0x000A53B2
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + this.m_hoverText);
	}

	// Token: 0x06001918 RID: 6424 RVA: 0x000A71EB File Offset: 0x000A53EB
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_hoverText);
	}

	// Token: 0x06001919 RID: 6425 RVA: 0x000A7200 File Offset: 0x000A5400
	private void RPC_RequestControl(long sender, long playerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_ship.IsPlayerInBoat(playerID))
		{
			return;
		}
		if (this.GetUser() == playerID || !this.HaveValidUser())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_user, playerID);
			this.m_nview.InvokeRPC(sender, "RequestRespons", new object[]
			{
				true
			});
			return;
		}
		this.m_nview.InvokeRPC(sender, "RequestRespons", new object[]
		{
			false
		});
	}

	// Token: 0x0600191A RID: 6426 RVA: 0x000A7292 File Offset: 0x000A5492
	private void RPC_ReleaseControl(long sender, long playerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetUser() == playerID)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_user, 0L);
		}
	}

	// Token: 0x0600191B RID: 6427 RVA: 0x000A72C4 File Offset: 0x000A54C4
	private void RPC_RequestRespons(long sender, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			Player.m_localPlayer.StartDoodadControl(this);
			if (this.m_attachPoint != null)
			{
				Player.m_localPlayer.AttachStart(this.m_attachPoint, null, false, false, true, this.m_attachAnimation, this.m_detachOffset);
				return;
			}
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
		}
	}

	// Token: 0x0600191C RID: 6428 RVA: 0x000A7330 File Offset: 0x000A5530
	public void OnUseStop(Player player)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("ReleaseControl", new object[]
		{
			player.GetPlayerID()
		});
		if (this.m_attachPoint != null)
		{
			player.AttachStop();
		}
	}

	// Token: 0x0600191D RID: 6429 RVA: 0x000A7384 File Offset: 0x000A5584
	public bool HaveValidUser()
	{
		long user = this.GetUser();
		return user != 0L && this.m_ship.IsPlayerInBoat(user);
	}

	// Token: 0x0600191E RID: 6430 RVA: 0x000A73A9 File Offset: 0x000A55A9
	private long GetUser()
	{
		if (!this.m_nview.IsValid())
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetLong(ZDOVars.s_user, 0L);
	}

	// Token: 0x0600191F RID: 6431 RVA: 0x000A73D2 File Offset: 0x000A55D2
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, this.m_attachPoint.position) < this.m_maxUseRange;
	}

	// Token: 0x04001B0C RID: 6924
	public string m_hoverText = "";

	// Token: 0x04001B0D RID: 6925
	public Ship m_ship;

	// Token: 0x04001B0E RID: 6926
	public float m_maxUseRange = 10f;

	// Token: 0x04001B0F RID: 6927
	public Transform m_attachPoint;

	// Token: 0x04001B10 RID: 6928
	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x04001B11 RID: 6929
	public string m_attachAnimation = "attach_chair";

	// Token: 0x04001B12 RID: 6930
	private ZNetView m_nview;
}
