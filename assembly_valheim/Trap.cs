using System;
using UnityEngine;

// Token: 0x020002B0 RID: 688
public class Trap : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001A11 RID: 6673 RVA: 0x000AC830 File Offset: 0x000AAA30
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_aoe = this.m_AOE.GetComponent<Aoe>();
		this.m_piece = base.GetComponent<Piece>();
		if (!this.m_aoe)
		{
			ZLog.LogError("Trap '" + base.gameObject.name + "' is missing AOE!");
		}
		this.m_aoe.gameObject.SetActive(false);
		if (this.m_nview)
		{
			this.m_nview.Register<int>("RPC_SetState", new Action<long, int>(this.RPC_SetState));
			this.UpdateState();
		}
	}

	// Token: 0x06001A12 RID: 6674 RVA: 0x000AC8D4 File Offset: 0x000AAAD4
	private void Update()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner() && this.IsActive() && !this.IsCoolingDown())
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetState", new object[]
			{
				0
			});
		}
	}

	// Token: 0x06001A13 RID: 6675 RVA: 0x000AC92F File Offset: 0x000AAB2F
	private bool IsArmed()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0) == 1;
	}

	// Token: 0x06001A14 RID: 6676 RVA: 0x000AC959 File Offset: 0x000AAB59
	private bool IsActive()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0) == 2;
	}

	// Token: 0x06001A15 RID: 6677 RVA: 0x000AC983 File Offset: 0x000AAB83
	private bool IsCoolingDown()
	{
		return this.m_nview.IsValid() && (double)(this.m_nview.GetZDO().GetFloat(ZDOVars.s_triggered, 0f) + (float)this.m_rearmCooldown) > ZNet.instance.GetTimeSeconds();
	}

	// Token: 0x06001A16 RID: 6678 RVA: 0x000AC9C4 File Offset: 0x000AABC4
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		if (this.IsArmed())
		{
			return Localization.instance.Localize(this.m_name + " ($piece_trap_armed)");
		}
		if (this.IsCoolingDown())
		{
			return Localization.instance.Localize(this.m_name);
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_trap_arm");
	}

	// Token: 0x06001A17 RID: 6679 RVA: 0x000ACA6E File Offset: 0x000AAC6E
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001A18 RID: 6680 RVA: 0x000ACA78 File Offset: 0x000AAC78
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		if (this.IsArmed())
		{
			return false;
		}
		if (this.IsCoolingDown())
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_trap_cooldown"), 0, null);
			return true;
		}
		this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetState", new object[]
		{
			1
		});
		return true;
	}

	// Token: 0x06001A19 RID: 6681 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001A1A RID: 6682 RVA: 0x000ACAFC File Offset: 0x000AACFC
	private void RPC_SetState(long uid, int value)
	{
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.ClaimOwnership();
		}
		if (this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0) != value)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_state, value, false);
			if (value == 2)
			{
				this.m_triggerEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				this.m_nview.GetZDO().Set(ZDOVars.s_triggered, (float)ZNet.instance.GetTimeSeconds());
			}
			else if (value == 1)
			{
				this.m_armEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				this.m_piece.m_randomTarget = false;
			}
			else if (value == 0)
			{
				this.m_piece.m_randomTarget = true;
			}
		}
		this.UpdateState();
	}

	// Token: 0x06001A1B RID: 6683 RVA: 0x000ACBF8 File Offset: 0x000AADF8
	private void UpdateState()
	{
		if (!this.m_nview || !this.m_nview.IsValid() || this.m_nview.GetZDO() == null)
		{
			return;
		}
		Trap.TrapState @int = (Trap.TrapState)this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
		if (@int == Trap.TrapState.Active)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_aoe.gameObject, base.transform).SetActive(true);
		}
		this.m_visualArmed.SetActive(@int == Trap.TrapState.Armed);
		this.m_visualUnarmed.SetActive(@int != Trap.TrapState.Armed);
	}

	// Token: 0x06001A1C RID: 6684 RVA: 0x000ACC88 File Offset: 0x000AAE88
	private void OnTriggerEnter(Collider collider)
	{
		if (!this.m_triggeredByPlayers && collider.GetComponentInParent<Player>() != null)
		{
			return;
		}
		if (!this.m_triggeredByEnemies && collider.GetComponentInParent<MonsterAI>() != null)
		{
			return;
		}
		if (this.IsArmed())
		{
			if (this.m_forceStagger)
			{
				Humanoid componentInParent = collider.GetComponentInParent<Humanoid>();
				if (componentInParent != null)
				{
					componentInParent.Stagger(Vector3.zero);
				}
			}
			this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetState", new object[]
			{
				2
			});
		}
	}

	// Token: 0x04001BF1 RID: 7153
	public string m_name = "Trap";

	// Token: 0x04001BF2 RID: 7154
	public GameObject m_AOE;

	// Token: 0x04001BF3 RID: 7155
	public Collider m_trigger;

	// Token: 0x04001BF4 RID: 7156
	public int m_rearmCooldown = 60;

	// Token: 0x04001BF5 RID: 7157
	public GameObject m_visualArmed;

	// Token: 0x04001BF6 RID: 7158
	public GameObject m_visualUnarmed;

	// Token: 0x04001BF7 RID: 7159
	public bool m_triggeredByEnemies;

	// Token: 0x04001BF8 RID: 7160
	public bool m_triggeredByPlayers;

	// Token: 0x04001BF9 RID: 7161
	public bool m_forceStagger = true;

	// Token: 0x04001BFA RID: 7162
	public EffectList m_triggerEffects;

	// Token: 0x04001BFB RID: 7163
	public EffectList m_armEffects;

	// Token: 0x04001BFC RID: 7164
	private ZNetView m_nview;

	// Token: 0x04001BFD RID: 7165
	private Aoe m_aoe;

	// Token: 0x04001BFE RID: 7166
	private Piece m_piece;

	// Token: 0x020002B1 RID: 689
	private enum TrapState
	{
		// Token: 0x04001C00 RID: 7168
		Unarmed,
		// Token: 0x04001C01 RID: 7169
		Armed,
		// Token: 0x04001C02 RID: 7170
		Active
	}
}
