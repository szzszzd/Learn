using System;
using UnityEngine;

// Token: 0x0200022B RID: 555
public class Door : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x060015E8 RID: 5608 RVA: 0x0008FF5C File Offset: 0x0008E15C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_animator = base.GetComponentInChildren<Animator>();
		if (this.m_nview)
		{
			this.m_nview.Register<bool>("UseDoor", new Action<long, bool>(this.RPC_UseDoor));
		}
		base.InvokeRepeating("UpdateState", 0f, 0.2f);
	}

	// Token: 0x060015E9 RID: 5609 RVA: 0x0008FFD0 File Offset: 0x0008E1D0
	private void UpdateState()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
		this.SetState(@int);
	}

	// Token: 0x060015EA RID: 5610 RVA: 0x0009000C File Offset: 0x0008E20C
	private void SetState(int state)
	{
		if (this.m_animator.GetInteger("state") != state)
		{
			if (state != 0)
			{
				this.m_openEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			}
			else
			{
				this.m_closeEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			}
			this.m_animator.SetInteger("state", state);
		}
		if (this.m_openEnable)
		{
			this.m_openEnable.SetActive(state != 0);
		}
	}

	// Token: 0x060015EB RID: 5611 RVA: 0x000900B0 File Offset: 0x0008E2B0
	private bool CanInteract()
	{
		return ((!(this.m_keyItem != null) && !this.m_canNotBeClosed) || this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0) == 0) && (this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("open") || this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("closed"));
	}

	// Token: 0x060015EC RID: 5612 RVA: 0x00090124 File Offset: 0x0008E324
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		if (this.m_canNotBeClosed && !this.CanInteract())
		{
			return "";
		}
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		if (!this.CanInteract())
		{
			return Localization.instance.Localize(this.m_name);
		}
		if (this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0) != 0)
		{
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + (this.m_invertedOpenClosedText ? "$piece_door_open" : "$piece_door_close"));
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + (this.m_invertedOpenClosedText ? "$piece_door_close" : "$piece_door_open"));
	}

	// Token: 0x060015ED RID: 5613 RVA: 0x00090224 File Offset: 0x0008E424
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060015EE RID: 5614 RVA: 0x0009022C File Offset: 0x0008E42C
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!this.CanInteract())
		{
			return false;
		}
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		if (this.m_keyItem != null)
		{
			if (!this.HaveKey(character))
			{
				this.m_lockedEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
				character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_needkey", new string[]
				{
					this.m_keyItem.m_itemData.m_shared.m_name
				}), 0, null);
				return true;
			}
			character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_usingkey", new string[]
			{
				this.m_keyItem.m_itemData.m_shared.m_name
			}), 0, null);
		}
		Vector3 normalized = (character.transform.position - base.transform.position).normalized;
		this.Open(normalized);
		return true;
	}

	// Token: 0x060015EF RID: 5615 RVA: 0x00090348 File Offset: 0x0008E548
	private void Open(Vector3 userDir)
	{
		bool flag = Vector3.Dot(base.transform.forward, userDir) < 0f;
		this.m_nview.InvokeRPC("UseDoor", new object[]
		{
			flag
		});
	}

	// Token: 0x060015F0 RID: 5616 RVA: 0x00090390 File Offset: 0x0008E590
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (!(this.m_keyItem != null) || !(this.m_keyItem.m_itemData.m_shared.m_name == item.m_shared.m_name))
		{
			return false;
		}
		if (!this.CanInteract())
		{
			return false;
		}
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_usingkey", new string[]
		{
			this.m_keyItem.m_itemData.m_shared.m_name
		}), 0, null);
		Vector3 normalized = (user.transform.position - base.transform.position).normalized;
		this.Open(normalized);
		return true;
	}

	// Token: 0x060015F1 RID: 5617 RVA: 0x00090469 File Offset: 0x0008E669
	private bool HaveKey(Humanoid player)
	{
		return this.m_keyItem == null || player.GetInventory().HaveItem(this.m_keyItem.m_itemData.m_shared.m_name);
	}

	// Token: 0x060015F2 RID: 5618 RVA: 0x0009049C File Offset: 0x0008E69C
	private void RPC_UseDoor(long uid, bool forward)
	{
		if (!this.CanInteract())
		{
			return;
		}
		if (this.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0) == 0)
		{
			if (forward)
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_state, 1, false);
			}
			else
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_state, -1, false);
			}
		}
		else
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_state, 0, false);
		}
		this.UpdateState();
	}

	// Token: 0x040016DE RID: 5854
	public string m_name = "door";

	// Token: 0x040016DF RID: 5855
	public ItemDrop m_keyItem;

	// Token: 0x040016E0 RID: 5856
	public bool m_canNotBeClosed;

	// Token: 0x040016E1 RID: 5857
	public bool m_invertedOpenClosedText;

	// Token: 0x040016E2 RID: 5858
	public bool m_checkGuardStone = true;

	// Token: 0x040016E3 RID: 5859
	public GameObject m_openEnable;

	// Token: 0x040016E4 RID: 5860
	public EffectList m_openEffects = new EffectList();

	// Token: 0x040016E5 RID: 5861
	public EffectList m_closeEffects = new EffectList();

	// Token: 0x040016E6 RID: 5862
	public EffectList m_lockedEffects = new EffectList();

	// Token: 0x040016E7 RID: 5863
	private ZNetView m_nview;

	// Token: 0x040016E8 RID: 5864
	private Animator m_animator;
}
