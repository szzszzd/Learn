using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002AE RID: 686
public class Trader : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001A02 RID: 6658 RVA: 0x000AC329 File Offset: 0x000AA529
	private void Start()
	{
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_lookAt = base.GetComponentInChildren<LookAt>();
		base.InvokeRepeating("RandomTalk", this.m_randomTalkInterval, this.m_randomTalkInterval);
	}

	// Token: 0x06001A03 RID: 6659 RVA: 0x000AC35C File Offset: 0x000AA55C
	private void Update()
	{
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_standRange);
		if (closestPlayer)
		{
			this.m_animator.SetBool("Stand", true);
			this.m_lookAt.SetLoockAtTarget(closestPlayer.GetHeadPoint());
			float num = Vector3.Distance(closestPlayer.transform.position, base.transform.position);
			if (!this.m_didGreet && num < this.m_greetRange)
			{
				this.m_didGreet = true;
				this.Say(this.m_randomGreets, "Greet");
				this.m_randomGreetFX.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
			}
			if (this.m_didGreet && !this.m_didGoodbye && num > this.m_byeRange)
			{
				this.m_didGoodbye = true;
				this.Say(this.m_randomGoodbye, "Greet");
				this.m_randomGoodbyeFX.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
				return;
			}
		}
		else
		{
			this.m_animator.SetBool("Stand", false);
			this.m_lookAt.ResetTarget();
		}
	}

	// Token: 0x06001A04 RID: 6660 RVA: 0x000AC488 File Offset: 0x000AA688
	private void RandomTalk()
	{
		if (this.m_animator.GetBool("Stand") && !StoreGui.IsVisible() && Player.IsPlayerInRange(base.transform.position, this.m_greetRange))
		{
			this.Say(this.m_randomTalk, "Talk");
			this.m_randomTalkFX.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		}
	}

	// Token: 0x06001A05 RID: 6661 RVA: 0x000AC4FA File Offset: 0x000AA6FA
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");
	}

	// Token: 0x06001A06 RID: 6662 RVA: 0x000AC516 File Offset: 0x000AA716
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06001A07 RID: 6663 RVA: 0x000AC528 File Offset: 0x000AA728
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		StoreGui.instance.Show(this);
		this.Say(this.m_randomStartTrade, "Talk");
		this.m_randomStartTradeFX.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		return false;
	}

	// Token: 0x06001A08 RID: 6664 RVA: 0x000AC57C File Offset: 0x000AA77C
	private void DiscoverItems(Player player)
	{
		foreach (Trader.TradeItem tradeItem in this.GetAvailableItems())
		{
			player.AddKnownItem(tradeItem.m_prefab.m_itemData);
		}
	}

	// Token: 0x06001A09 RID: 6665 RVA: 0x000AC5DC File Offset: 0x000AA7DC
	private void Say(List<string> texts, string trigger)
	{
		this.Say(texts[UnityEngine.Random.Range(0, texts.Count)], trigger);
	}

	// Token: 0x06001A0A RID: 6666 RVA: 0x000AC5F8 File Offset: 0x000AA7F8
	private void Say(string text, string trigger)
	{
		Chat.instance.SetNpcText(base.gameObject, Vector3.up * 1.5f, 20f, this.m_hideDialogDelay, "", text, false);
		if (trigger.Length > 0)
		{
			this.m_animator.SetTrigger(trigger);
		}
	}

	// Token: 0x06001A0B RID: 6667 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001A0C RID: 6668 RVA: 0x000AC64B File Offset: 0x000AA84B
	public void OnBought(Trader.TradeItem item)
	{
		this.Say(this.m_randomBuy, "Buy");
		this.m_randomBuyFX.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x06001A0D RID: 6669 RVA: 0x000AC681 File Offset: 0x000AA881
	public void OnSold()
	{
		this.Say(this.m_randomSell, "Sell");
		this.m_randomSellFX.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x06001A0E RID: 6670 RVA: 0x000AC6B8 File Offset: 0x000AA8B8
	public List<Trader.TradeItem> GetAvailableItems()
	{
		List<Trader.TradeItem> list = new List<Trader.TradeItem>();
		foreach (Trader.TradeItem tradeItem in this.m_items)
		{
			if (string.IsNullOrEmpty(tradeItem.m_requiredGlobalKey) || ZoneSystem.instance.GetGlobalKey(tradeItem.m_requiredGlobalKey))
			{
				list.Add(tradeItem);
			}
		}
		return list;
	}

	// Token: 0x04001BD6 RID: 7126
	public string m_name = "Haldor";

	// Token: 0x04001BD7 RID: 7127
	public float m_standRange = 15f;

	// Token: 0x04001BD8 RID: 7128
	public float m_greetRange = 5f;

	// Token: 0x04001BD9 RID: 7129
	public float m_byeRange = 5f;

	// Token: 0x04001BDA RID: 7130
	public List<Trader.TradeItem> m_items = new List<Trader.TradeItem>();

	// Token: 0x04001BDB RID: 7131
	[Header("Dialog")]
	public float m_hideDialogDelay = 5f;

	// Token: 0x04001BDC RID: 7132
	public float m_randomTalkInterval = 30f;

	// Token: 0x04001BDD RID: 7133
	public List<string> m_randomTalk = new List<string>();

	// Token: 0x04001BDE RID: 7134
	public List<string> m_randomGreets = new List<string>();

	// Token: 0x04001BDF RID: 7135
	public List<string> m_randomGoodbye = new List<string>();

	// Token: 0x04001BE0 RID: 7136
	public List<string> m_randomStartTrade = new List<string>();

	// Token: 0x04001BE1 RID: 7137
	public List<string> m_randomBuy = new List<string>();

	// Token: 0x04001BE2 RID: 7138
	public List<string> m_randomSell = new List<string>();

	// Token: 0x04001BE3 RID: 7139
	public EffectList m_randomTalkFX = new EffectList();

	// Token: 0x04001BE4 RID: 7140
	public EffectList m_randomGreetFX = new EffectList();

	// Token: 0x04001BE5 RID: 7141
	public EffectList m_randomGoodbyeFX = new EffectList();

	// Token: 0x04001BE6 RID: 7142
	public EffectList m_randomStartTradeFX = new EffectList();

	// Token: 0x04001BE7 RID: 7143
	public EffectList m_randomBuyFX = new EffectList();

	// Token: 0x04001BE8 RID: 7144
	public EffectList m_randomSellFX = new EffectList();

	// Token: 0x04001BE9 RID: 7145
	private bool m_didGreet;

	// Token: 0x04001BEA RID: 7146
	private bool m_didGoodbye;

	// Token: 0x04001BEB RID: 7147
	private Animator m_animator;

	// Token: 0x04001BEC RID: 7148
	private LookAt m_lookAt;

	// Token: 0x020002AF RID: 687
	[Serializable]
	public class TradeItem
	{
		// Token: 0x04001BED RID: 7149
		public ItemDrop m_prefab;

		// Token: 0x04001BEE RID: 7150
		public int m_stack = 1;

		// Token: 0x04001BEF RID: 7151
		public int m_price = 100;

		// Token: 0x04001BF0 RID: 7152
		public string m_requiredGlobalKey;
	}
}
