using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200024E RID: 590
public class Incinerator : MonoBehaviour
{
	// Token: 0x06001711 RID: 5905 RVA: 0x00098D40 File Offset: 0x00096F40
	private void Awake()
	{
		Switch incinerateSwitch = this.m_incinerateSwitch;
		incinerateSwitch.m_onUse = (Switch.Callback)Delegate.Combine(incinerateSwitch.m_onUse, new Switch.Callback(this.OnIncinerate));
		Switch incinerateSwitch2 = this.m_incinerateSwitch;
		incinerateSwitch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(incinerateSwitch2.m_onHover, new Switch.TooltipCallback(this.GetLeverHoverText));
		this.m_conversions.Sort((Incinerator.IncineratorConversion a, Incinerator.IncineratorConversion b) => b.m_priority.CompareTo(a.m_priority));
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<long>("RPC_RequestIncinerate", new Action<long, long>(this.RPC_RequestIncinerate));
		this.m_nview.Register<int>("RPC_IncinerateRespons", new Action<long, int>(this.RPC_IncinerateRespons));
		this.m_nview.Register("RPC_AnimateLever", new Action<long>(this.RPC_AnimateLever));
		this.m_nview.Register("RPC_AnimateLeverReturn", new Action<long>(this.RPC_AnimateLeverReturn));
	}

	// Token: 0x06001712 RID: 5906 RVA: 0x00098E5D File Offset: 0x0009705D
	private void StopAOE()
	{
		this.isInUse = false;
	}

	// Token: 0x06001713 RID: 5907 RVA: 0x00098E66 File Offset: 0x00097066
	public string GetLeverHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return Localization.instance.Localize("$piece_incinerator\n$piece_noaccess");
		}
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] $piece_pulllever");
	}

	// Token: 0x06001714 RID: 5908 RVA: 0x00098EA0 File Offset: 0x000970A0
	private bool OnIncinerate(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.HasOwner())
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return false;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		this.m_nview.InvokeRPC("RPC_RequestIncinerate", new object[]
		{
			playerID
		});
		return true;
	}

	// Token: 0x06001715 RID: 5909 RVA: 0x00098F14 File Offset: 0x00097114
	private void RPC_RequestIncinerate(long uid, long playerID)
	{
		ZLog.Log(string.Concat(new string[]
		{
			"Player ",
			uid.ToString(),
			" wants to incinerate ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.GetSessionID().ToString()
		}));
		if (!this.m_nview.IsOwner())
		{
			ZLog.Log("  but im not the owner");
			return;
		}
		if (this.m_container.IsInUse() || this.isInUse)
		{
			this.m_nview.InvokeRPC(uid, "RPC_IncinerateRespons", new object[]
			{
				0
			});
			ZLog.Log("  but it's in use");
			return;
		}
		if (this.m_container.GetInventory().NrOfItems() == 0)
		{
			this.m_nview.InvokeRPC(uid, "RPC_IncinerateRespons", new object[]
			{
				3
			});
			ZLog.Log("  but it's empty");
			return;
		}
		base.StartCoroutine(this.Incinerate(uid));
	}

	// Token: 0x06001716 RID: 5910 RVA: 0x00099014 File Offset: 0x00097214
	private IEnumerator Incinerate(long uid)
	{
		this.isInUse = true;
		this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_AnimateLever", Array.Empty<object>());
		this.m_leverEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		yield return new WaitForSeconds(UnityEngine.Random.Range(this.m_effectDelayMin, this.m_effectDelayMax));
		this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_AnimateLeverReturn", Array.Empty<object>());
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner() || this.m_container.IsInUse())
		{
			this.isInUse = false;
			yield break;
		}
		base.Invoke("StopAOE", 4f);
		UnityEngine.Object.Instantiate<GameObject>(this.m_lightingAOEs, base.transform.position, base.transform.rotation);
		Inventory inventory = this.m_container.GetInventory();
		List<ItemDrop> list = new List<ItemDrop>();
		int num = 0;
		foreach (Incinerator.IncineratorConversion incineratorConversion in this.m_conversions)
		{
			num += incineratorConversion.AttemptCraft(inventory, list);
		}
		if (this.m_defaultResult != null && this.m_defaultCost > 0)
		{
			int num2 = inventory.NrOfItemsIncludingStacks() / this.m_defaultCost;
			num += num2;
			for (int i = 0; i < num2; i++)
			{
				list.Add(this.m_defaultResult);
			}
		}
		inventory.RemoveAll();
		foreach (ItemDrop itemDrop in list)
		{
			inventory.AddItem(itemDrop.gameObject, 1);
		}
		this.m_nview.InvokeRPC(uid, "RPC_IncinerateRespons", new object[]
		{
			(num > 0) ? 2 : 1
		});
		yield break;
	}

	// Token: 0x06001717 RID: 5911 RVA: 0x0009902C File Offset: 0x0009722C
	private void RPC_IncinerateRespons(long uid, int r)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		string msg;
		switch (r)
		{
		default:
			msg = "$piece_incinerator_fail";
			break;
		case 1:
			msg = "$piece_incinerator_success";
			break;
		case 2:
			msg = "$piece_incinerator_conversion";
			break;
		case 3:
			msg = "$piece_incinerator_empty";
			break;
		}
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);
	}

	// Token: 0x06001718 RID: 5912 RVA: 0x0009908C File Offset: 0x0009728C
	private void RPC_AnimateLever(long uid)
	{
		ZLog.Log("DO THE THING WITH THE LEVER!");
		this.m_leverAnim.SetBool("Pulled", true);
	}

	// Token: 0x06001719 RID: 5913 RVA: 0x000990A9 File Offset: 0x000972A9
	private void RPC_AnimateLeverReturn(long uid)
	{
		ZLog.Log("Lever return");
		this.m_leverAnim.SetBool("Pulled", false);
	}

	// Token: 0x0400186B RID: 6251
	public Switch m_incinerateSwitch;

	// Token: 0x0400186C RID: 6252
	public Container m_container;

	// Token: 0x0400186D RID: 6253
	public Animator m_leverAnim;

	// Token: 0x0400186E RID: 6254
	public GameObject m_lightingAOEs;

	// Token: 0x0400186F RID: 6255
	public EffectList m_leverEffects = new EffectList();

	// Token: 0x04001870 RID: 6256
	public float m_effectDelayMin = 5f;

	// Token: 0x04001871 RID: 6257
	public float m_effectDelayMax = 7f;

	// Token: 0x04001872 RID: 6258
	[Header("Conversion")]
	public List<Incinerator.IncineratorConversion> m_conversions;

	// Token: 0x04001873 RID: 6259
	public ItemDrop m_defaultResult;

	// Token: 0x04001874 RID: 6260
	public int m_defaultCost = 1;

	// Token: 0x04001875 RID: 6261
	private ZNetView m_nview;

	// Token: 0x04001876 RID: 6262
	private bool isInUse;

	// Token: 0x0200024F RID: 591
	[Serializable]
	public class IncineratorConversion
	{
		// Token: 0x0600171B RID: 5915 RVA: 0x000990F8 File Offset: 0x000972F8
		public int AttemptCraft(Inventory inv, List<ItemDrop> toAdd)
		{
			int num = int.MaxValue;
			int num2 = 0;
			Incinerator.Requirement requirement = null;
			foreach (Incinerator.Requirement requirement2 in this.m_requirements)
			{
				int num3 = inv.CountItems(requirement2.m_resItem.m_itemData.m_shared.m_name, -1) / requirement2.m_amount;
				if (num3 == 0 && !this.m_requireOnlyOneIngredient)
				{
					return 0;
				}
				if (num3 > num2)
				{
					num2 = num3;
					requirement = requirement2;
				}
				if (num3 < num)
				{
					num = num3;
				}
			}
			int num4 = this.m_requireOnlyOneIngredient ? num2 : num;
			if (num4 == 0)
			{
				return 0;
			}
			if (this.m_requireOnlyOneIngredient)
			{
				inv.RemoveItem(requirement.m_resItem.m_itemData.m_shared.m_name, requirement.m_amount * num4, -1);
			}
			else
			{
				foreach (Incinerator.Requirement requirement3 in this.m_requirements)
				{
					inv.RemoveItem(requirement3.m_resItem.m_itemData.m_shared.m_name, requirement3.m_amount * num4, -1);
				}
			}
			num4 *= this.m_resultAmount;
			for (int i = 0; i < num4; i++)
			{
				toAdd.Add(this.m_result);
			}
			return num4;
		}

		// Token: 0x04001877 RID: 6263
		public List<Incinerator.Requirement> m_requirements;

		// Token: 0x04001878 RID: 6264
		public ItemDrop m_result;

		// Token: 0x04001879 RID: 6265
		public int m_resultAmount = 1;

		// Token: 0x0400187A RID: 6266
		public int m_priority;

		// Token: 0x0400187B RID: 6267
		[global::Tooltip("True: Requires only one of the list of ingredients to be able to produce the result. False: All of the ingredients are required.")]
		public bool m_requireOnlyOneIngredient;
	}

	// Token: 0x02000250 RID: 592
	[Serializable]
	public class Requirement
	{
		// Token: 0x0400187C RID: 6268
		public ItemDrop m_resItem;

		// Token: 0x0400187D RID: 6269
		public int m_amount = 1;
	}

	// Token: 0x02000251 RID: 593
	private enum Response
	{
		// Token: 0x0400187F RID: 6271
		Fail,
		// Token: 0x04001880 RID: 6272
		Success,
		// Token: 0x04001881 RID: 6273
		Conversion,
		// Token: 0x04001882 RID: 6274
		Empty
	}
}
