using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000FF RID: 255
public class StoreGui : MonoBehaviour
{
	// Token: 0x1700005D RID: 93
	// (get) Token: 0x06000A67 RID: 2663 RVA: 0x0004F556 File Offset: 0x0004D756
	public static StoreGui instance
	{
		get
		{
			return StoreGui.m_instance;
		}
	}

	// Token: 0x06000A68 RID: 2664 RVA: 0x0004F560 File Offset: 0x0004D760
	private void Awake()
	{
		StoreGui.m_instance = this;
		this.m_rootPanel.SetActive(false);
		this.m_itemlistBaseSize = this.m_listRoot.rect.height;
	}

	// Token: 0x06000A69 RID: 2665 RVA: 0x0004F598 File Offset: 0x0004D798
	private void OnDestroy()
	{
		if (StoreGui.m_instance == this)
		{
			StoreGui.m_instance = null;
		}
	}

	// Token: 0x06000A6A RID: 2666 RVA: 0x0004F5B0 File Offset: 0x0004D7B0
	private void Update()
	{
		if (!this.m_rootPanel.activeSelf)
		{
			this.m_hiddenFrames++;
			return;
		}
		this.m_hiddenFrames = 0;
		if (!this.m_trader)
		{
			this.Hide();
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || localPlayer.IsDead() || localPlayer.InCutscene())
		{
			this.Hide();
			return;
		}
		if (Vector3.Distance(this.m_trader.transform.position, Player.m_localPlayer.transform.position) > this.m_hideDistance)
		{
			this.Hide();
			return;
		}
		if (InventoryGui.IsVisible() || Minimap.IsOpen())
		{
			this.Hide();
			return;
		}
		if ((Chat.instance == null || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !Menu.IsVisible() && TextViewer.instance && !TextViewer.instance.IsVisible() && !localPlayer.InCutscene() && (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use")))
		{
			ZInput.ResetButtonStatus("JoyButtonB");
			this.Hide();
		}
		this.UpdateBuyButton();
		this.UpdateSellButton();
		this.UpdateRecipeGamepadInput();
		this.m_coinText.text = this.GetPlayerCoins().ToString();
	}

	// Token: 0x06000A6B RID: 2667 RVA: 0x0004F70B File Offset: 0x0004D90B
	public void Show(Trader trader)
	{
		if (this.m_trader == trader && StoreGui.IsVisible())
		{
			return;
		}
		this.m_trader = trader;
		this.m_rootPanel.SetActive(true);
		this.FillList();
	}

	// Token: 0x06000A6C RID: 2668 RVA: 0x0004F73C File Offset: 0x0004D93C
	public void Hide()
	{
		this.m_trader = null;
		this.m_rootPanel.SetActive(false);
	}

	// Token: 0x06000A6D RID: 2669 RVA: 0x0004F751 File Offset: 0x0004D951
	public static bool IsVisible()
	{
		return StoreGui.m_instance && StoreGui.m_instance.m_hiddenFrames <= 1;
	}

	// Token: 0x06000A6E RID: 2670 RVA: 0x0004F771 File Offset: 0x0004D971
	public void OnBuyItem()
	{
		this.BuySelectedItem();
	}

	// Token: 0x06000A6F RID: 2671 RVA: 0x0004F77C File Offset: 0x0004D97C
	private void BuySelectedItem()
	{
		if (this.m_selectedItem == null || !this.CanAfford(this.m_selectedItem))
		{
			return;
		}
		int stack = Mathf.Min(this.m_selectedItem.m_stack, this.m_selectedItem.m_prefab.m_itemData.m_shared.m_maxStackSize);
		int quality = this.m_selectedItem.m_prefab.m_itemData.m_quality;
		int variant = this.m_selectedItem.m_prefab.m_itemData.m_variant;
		if (Player.m_localPlayer.GetInventory().AddItem(this.m_selectedItem.m_prefab.name, stack, quality, variant, 0L, "") != null)
		{
			Player.m_localPlayer.GetInventory().RemoveItem(this.m_coinPrefab.m_itemData.m_shared.m_name, this.m_selectedItem.m_price, -1);
			this.m_trader.OnBought(this.m_selectedItem);
			this.m_buyEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
			Player.m_localPlayer.ShowPickupMessage(this.m_selectedItem.m_prefab.m_itemData, this.m_selectedItem.m_prefab.m_itemData.m_stack);
			this.FillList();
			Gogan.LogEvent("Game", "BoughtItem", this.m_selectedItem.m_prefab.name, 0L);
		}
	}

	// Token: 0x06000A70 RID: 2672 RVA: 0x0004F8DF File Offset: 0x0004DADF
	public void OnSellItem()
	{
		this.SellItem();
	}

	// Token: 0x06000A71 RID: 2673 RVA: 0x0004F8E8 File Offset: 0x0004DAE8
	private void SellItem()
	{
		ItemDrop.ItemData sellableItem = this.GetSellableItem();
		if (sellableItem == null)
		{
			return;
		}
		int stack = sellableItem.m_shared.m_value * sellableItem.m_stack;
		Player.m_localPlayer.GetInventory().RemoveItem(sellableItem);
		Player.m_localPlayer.GetInventory().AddItem(this.m_coinPrefab.gameObject.name, stack, this.m_coinPrefab.m_itemData.m_quality, this.m_coinPrefab.m_itemData.m_variant, 0L, "");
		string text;
		if (sellableItem.m_stack > 1)
		{
			text = sellableItem.m_stack.ToString() + "x" + sellableItem.m_shared.m_name;
		}
		else
		{
			text = sellableItem.m_shared.m_name;
		}
		this.m_sellEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_sold", new string[]
		{
			text,
			stack.ToString()
		}), 0, sellableItem.m_shared.m_icons[0]);
		this.m_trader.OnSold();
		this.FillList();
		Gogan.LogEvent("Game", "SoldItem", text, 0L);
	}

	// Token: 0x06000A72 RID: 2674 RVA: 0x0004FA2C File Offset: 0x0004DC2C
	private int GetPlayerCoins()
	{
		return Player.m_localPlayer.GetInventory().CountItems(this.m_coinPrefab.m_itemData.m_shared.m_name, -1);
	}

	// Token: 0x06000A73 RID: 2675 RVA: 0x0004FA54 File Offset: 0x0004DC54
	private bool CanAfford(Trader.TradeItem item)
	{
		int playerCoins = this.GetPlayerCoins();
		return item.m_price <= playerCoins;
	}

	// Token: 0x06000A74 RID: 2676 RVA: 0x0004FA74 File Offset: 0x0004DC74
	private void FillList()
	{
		int playerCoins = this.GetPlayerCoins();
		int num = this.GetSelectedItemIndex();
		List<Trader.TradeItem> availableItems = this.m_trader.GetAvailableItems();
		foreach (GameObject obj in this.m_itemList)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_itemList.Clear();
		float num2 = (float)availableItems.Count * this.m_itemSpacing;
		num2 = Mathf.Max(this.m_itemlistBaseSize, num2);
		this.m_listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
		for (int i = 0; i < availableItems.Count; i++)
		{
			Trader.TradeItem tradeItem = availableItems[i];
			GameObject element = UnityEngine.Object.Instantiate<GameObject>(this.m_listElement, this.m_listRoot);
			element.SetActive(true);
			(element.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -this.m_itemSpacing);
			bool flag = tradeItem.m_price <= playerCoins;
			Image component = element.transform.Find("icon").GetComponent<Image>();
			component.sprite = tradeItem.m_prefab.m_itemData.m_shared.m_icons[0];
			component.color = (flag ? Color.white : new Color(1f, 0f, 1f, 0f));
			string text = Localization.instance.Localize(tradeItem.m_prefab.m_itemData.m_shared.m_name);
			if (tradeItem.m_stack > 1)
			{
				text = text + " x" + tradeItem.m_stack.ToString();
			}
			Text component2 = element.transform.Find("name").GetComponent<Text>();
			component2.text = text;
			component2.color = (flag ? Color.white : Color.grey);
			UITooltip component3 = element.GetComponent<UITooltip>();
			component3.m_topic = tradeItem.m_prefab.m_itemData.m_shared.m_name;
			component3.m_text = tradeItem.m_prefab.m_itemData.GetTooltip();
			Text component4 = Utils.FindChild(element.transform, "price").GetComponent<Text>();
			component4.text = tradeItem.m_price.ToString();
			if (!flag)
			{
				component4.color = Color.grey;
			}
			element.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnSelectedItem(element);
			});
			this.m_itemList.Add(element);
		}
		if (num < 0)
		{
			num = 0;
		}
		this.SelectItem(num, false);
	}

	// Token: 0x06000A75 RID: 2677 RVA: 0x0004FD48 File Offset: 0x0004DF48
	private void OnSelectedItem(GameObject button)
	{
		int index = this.FindSelectedRecipe(button);
		this.SelectItem(index, false);
	}

	// Token: 0x06000A76 RID: 2678 RVA: 0x0004FD68 File Offset: 0x0004DF68
	private int FindSelectedRecipe(GameObject button)
	{
		for (int i = 0; i < this.m_itemList.Count; i++)
		{
			if (this.m_itemList[i] == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000A77 RID: 2679 RVA: 0x0004FDA4 File Offset: 0x0004DFA4
	private void SelectItem(int index, bool center)
	{
		ZLog.Log("Setting selected recipe " + index.ToString());
		for (int i = 0; i < this.m_itemList.Count; i++)
		{
			bool active = i == index;
			this.m_itemList[i].transform.Find("selected").gameObject.SetActive(active);
		}
		if (center && index >= 0)
		{
			this.m_itemEnsureVisible.CenterOnItem(this.m_itemList[index].transform as RectTransform);
		}
		if (index < 0)
		{
			this.m_selectedItem = null;
			return;
		}
		this.m_selectedItem = this.m_trader.GetAvailableItems()[index];
	}

	// Token: 0x06000A78 RID: 2680 RVA: 0x0004FE53 File Offset: 0x0004E053
	private void UpdateSellButton()
	{
		this.m_sellButton.interactable = (this.GetSellableItem() != null);
	}

	// Token: 0x06000A79 RID: 2681 RVA: 0x0004FE6C File Offset: 0x0004E06C
	private ItemDrop.ItemData GetSellableItem()
	{
		this.m_tempItems.Clear();
		Player.m_localPlayer.GetInventory().GetValuableItems(this.m_tempItems);
		foreach (ItemDrop.ItemData itemData in this.m_tempItems)
		{
			if (itemData.m_shared.m_name != this.m_coinPrefab.m_itemData.m_shared.m_name)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x06000A7A RID: 2682 RVA: 0x0004FF08 File Offset: 0x0004E108
	private int GetSelectedItemIndex()
	{
		int result = 0;
		List<Trader.TradeItem> availableItems = this.m_trader.GetAvailableItems();
		for (int i = 0; i < availableItems.Count; i++)
		{
			if (availableItems[i] == this.m_selectedItem)
			{
				result = i;
			}
		}
		return result;
	}

	// Token: 0x06000A7B RID: 2683 RVA: 0x0004FF48 File Offset: 0x0004E148
	private void UpdateBuyButton()
	{
		UITooltip component = this.m_buyButton.GetComponent<UITooltip>();
		if (this.m_selectedItem == null)
		{
			this.m_buyButton.interactable = false;
			component.m_text = "";
			return;
		}
		bool flag = this.CanAfford(this.m_selectedItem);
		bool flag2 = Player.m_localPlayer.GetInventory().HaveEmptySlot();
		this.m_buyButton.interactable = (flag && flag2);
		if (!flag)
		{
			component.m_text = Localization.instance.Localize("$msg_missingrequirement");
			return;
		}
		if (!flag2)
		{
			component.m_text = Localization.instance.Localize("$inventory_full");
			return;
		}
		component.m_text = "";
	}

	// Token: 0x06000A7C RID: 2684 RVA: 0x0004FFEC File Offset: 0x0004E1EC
	private void UpdateRecipeGamepadInput()
	{
		if (this.m_itemList.Count > 0)
		{
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				this.SelectItem(Mathf.Min(this.m_itemList.Count - 1, this.GetSelectedItemIndex() + 1), true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				this.SelectItem(Mathf.Max(0, this.GetSelectedItemIndex() - 1), true);
			}
		}
	}

	// Token: 0x04000CA1 RID: 3233
	private static StoreGui m_instance;

	// Token: 0x04000CA2 RID: 3234
	public GameObject m_rootPanel;

	// Token: 0x04000CA3 RID: 3235
	public Button m_buyButton;

	// Token: 0x04000CA4 RID: 3236
	public Button m_sellButton;

	// Token: 0x04000CA5 RID: 3237
	public RectTransform m_listRoot;

	// Token: 0x04000CA6 RID: 3238
	public GameObject m_listElement;

	// Token: 0x04000CA7 RID: 3239
	public Scrollbar m_listScroll;

	// Token: 0x04000CA8 RID: 3240
	public ScrollRectEnsureVisible m_itemEnsureVisible;

	// Token: 0x04000CA9 RID: 3241
	public Text m_coinText;

	// Token: 0x04000CAA RID: 3242
	public EffectList m_buyEffects = new EffectList();

	// Token: 0x04000CAB RID: 3243
	public EffectList m_sellEffects = new EffectList();

	// Token: 0x04000CAC RID: 3244
	public float m_hideDistance = 5f;

	// Token: 0x04000CAD RID: 3245
	public float m_itemSpacing = 64f;

	// Token: 0x04000CAE RID: 3246
	public ItemDrop m_coinPrefab;

	// Token: 0x04000CAF RID: 3247
	private List<GameObject> m_itemList = new List<GameObject>();

	// Token: 0x04000CB0 RID: 3248
	private Trader.TradeItem m_selectedItem;

	// Token: 0x04000CB1 RID: 3249
	private Trader m_trader;

	// Token: 0x04000CB2 RID: 3250
	private float m_itemlistBaseSize;

	// Token: 0x04000CB3 RID: 3251
	private int m_hiddenFrames;

	// Token: 0x04000CB4 RID: 3252
	private List<ItemDrop.ItemData> m_tempItems = new List<ItemDrop.ItemData>();
}
