using System;
using UnityEngine;

// Token: 0x020002A0 RID: 672
public class TeleportWorld : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	// Token: 0x060019A1 RID: 6561 RVA: 0x000A9928 File Offset: 0x000A7B28
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		this.m_hadTarget = this.HaveTarget();
		this.m_nview.Register<string, string>("SetTag", new Action<long, string, string>(this.RPC_SetTag));
		base.InvokeRepeating("UpdatePortal", 0.5f, 0.5f);
	}

	// Token: 0x060019A2 RID: 6562 RVA: 0x000A9994 File Offset: 0x000A7B94
	public string GetHoverText()
	{
		string text = this.GetText().RemoveRichTextTags();
		string text2 = this.HaveTarget() ? "$piece_portal_connected" : "$piece_portal_unconnected";
		return Localization.instance.Localize(string.Concat(new string[]
		{
			"$piece_portal $piece_portal_tag:\"",
			text,
			"\"  [",
			text2,
			"]\n[<color=yellow><b>$KEY_Use</b></color>] $piece_portal_settag"
		}));
	}

	// Token: 0x060019A3 RID: 6563 RVA: 0x000A99F7 File Offset: 0x000A7BF7
	public string GetHoverName()
	{
		return "Teleport";
	}

	// Token: 0x060019A4 RID: 6564 RVA: 0x000A9A00 File Offset: 0x000A7C00
	public bool Interact(Humanoid human, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			human.Message(MessageHud.MessageType.Center, "$piece_noaccess", 0, null);
			return true;
		}
		TextInput.instance.RequestText(this, "$piece_portal_tag", 10);
		return true;
	}

	// Token: 0x060019A5 RID: 6565 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060019A6 RID: 6566 RVA: 0x000A9A50 File Offset: 0x000A7C50
	private void UpdatePortal()
	{
		if (!this.m_nview.IsValid() || this.m_proximityRoot == null)
		{
			return;
		}
		Player closestPlayer = Player.GetClosestPlayer(this.m_proximityRoot.position, this.m_activationRange);
		bool flag = this.HaveTarget();
		if (flag && !this.m_hadTarget)
		{
			this.m_connected.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		}
		this.m_hadTarget = flag;
		this.m_target_found.SetActive(closestPlayer && closestPlayer.IsTeleportable() && this.TargetFound());
	}

	// Token: 0x060019A7 RID: 6567 RVA: 0x000A9AF8 File Offset: 0x000A7CF8
	private void Update()
	{
		this.m_colorAlpha = Mathf.MoveTowards(this.m_colorAlpha, this.m_hadTarget ? 1f : 0f, Time.deltaTime);
		this.m_model.material.SetColor("_EmissionColor", Color.Lerp(this.m_colorUnconnected, this.m_colorTargetfound, this.m_colorAlpha));
	}

	// Token: 0x060019A8 RID: 6568 RVA: 0x000A9B5C File Offset: 0x000A7D5C
	public void Teleport(Player player)
	{
		if (!this.TargetFound())
		{
			return;
		}
		if (ZoneSystem.instance.GetGlobalKey("noportals"))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_blocked", 0, null);
			return;
		}
		if (!player.IsTeleportable())
		{
			player.Message(MessageHud.MessageType.Center, "$msg_noteleport", 0, null);
			return;
		}
		ZLog.Log("Teleporting " + player.GetPlayerName());
		ZDO zdo = ZDOMan.instance.GetZDO(this.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal));
		if (zdo == null)
		{
			return;
		}
		Vector3 position = zdo.GetPosition();
		Quaternion rotation = zdo.GetRotation();
		Vector3 a = rotation * Vector3.forward;
		Vector3 pos = position + a * this.m_exitDistance + Vector3.up;
		player.TeleportTo(pos, rotation, true);
	}

	// Token: 0x060019A9 RID: 6569 RVA: 0x000A9C24 File Offset: 0x000A7E24
	public string GetText()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null)
		{
			return "";
		}
		return zdo.GetString(ZDOVars.s_tag, "");
	}

	// Token: 0x060019AA RID: 6570 RVA: 0x000A9C58 File Offset: 0x000A7E58
	private void GetTagSignature(out string tagRaw, out string authorId)
	{
		ZDO zdo = this.m_nview.GetZDO();
		tagRaw = zdo.GetString(ZDOVars.s_tag, "");
		authorId = zdo.GetString(ZDOVars.s_tagauthor, "");
	}

	// Token: 0x060019AB RID: 6571 RVA: 0x000A9C95 File Offset: 0x000A7E95
	public void SetText(string text)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("SetTag", new object[]
		{
			text,
			PrivilegeManager.GetNetworkUserId()
		});
	}

	// Token: 0x060019AC RID: 6572 RVA: 0x000A9CC8 File Offset: 0x000A7EC8
	private void RPC_SetTag(long sender, string tag, string authorId)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		string a;
		string a2;
		this.GetTagSignature(out a, out a2);
		if (a == tag && a2 == authorId)
		{
			return;
		}
		ZDO zdo = this.m_nview.GetZDO();
		ZDOID connectionZDOID = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
		zdo.UpdateConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
		ZDO zdo2 = ZDOMan.instance.GetZDO(connectionZDOID);
		if (zdo2 != null)
		{
			zdo2.UpdateConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
		}
		zdo.Set(ZDOVars.s_tag, tag);
		zdo.Set(ZDOVars.s_tagauthor, authorId);
	}

	// Token: 0x060019AD RID: 6573 RVA: 0x000A9D5F File Offset: 0x000A7F5F
	private bool HaveTarget()
	{
		return !(this.m_nview == null) && this.m_nview.GetZDO() != null && this.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None;
	}

	// Token: 0x060019AE RID: 6574 RVA: 0x000A9D9C File Offset: 0x000A7F9C
	private bool TargetFound()
	{
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return false;
		}
		ZDOID connectionZDOID = this.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
		if (connectionZDOID == ZDOID.None)
		{
			return false;
		}
		if (ZDOMan.instance.GetZDO(connectionZDOID) == null)
		{
			ZDOMan.instance.RequestZDO(connectionZDOID);
			return false;
		}
		return true;
	}

	// Token: 0x04001B71 RID: 7025
	public float m_activationRange = 5f;

	// Token: 0x04001B72 RID: 7026
	public float m_exitDistance = 1f;

	// Token: 0x04001B73 RID: 7027
	public Transform m_proximityRoot;

	// Token: 0x04001B74 RID: 7028
	[ColorUsage(true, true)]
	public Color m_colorUnconnected = Color.white;

	// Token: 0x04001B75 RID: 7029
	[ColorUsage(true, true)]
	public Color m_colorTargetfound = Color.white;

	// Token: 0x04001B76 RID: 7030
	public EffectFade m_target_found;

	// Token: 0x04001B77 RID: 7031
	public MeshRenderer m_model;

	// Token: 0x04001B78 RID: 7032
	public EffectList m_connected;

	// Token: 0x04001B79 RID: 7033
	private ZNetView m_nview;

	// Token: 0x04001B7A RID: 7034
	private bool m_hadTarget;

	// Token: 0x04001B7B RID: 7035
	private float m_colorAlpha;
}
