using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000272 RID: 626
public class OfferingBowl : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600180B RID: 6155 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Awake()
	{
	}

	// Token: 0x0600180C RID: 6156 RVA: 0x000A0420 File Offset: 0x0009E620
	public string GetHoverText()
	{
		if (this.m_useItemStands)
		{
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] ") + Localization.instance.Localize(this.m_useItemText);
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>1-8</b></color>] " + this.m_useItemText);
	}

	// Token: 0x0600180D RID: 6157 RVA: 0x000A0485 File Offset: 0x0009E685
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x0600180E RID: 6158 RVA: 0x000A0490 File Offset: 0x0009E690
	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (hold || this.IsBossSpawnQueued() || !this.m_useItemStands)
		{
			return false;
		}
		List<ItemStand> list = this.FindItemStands();
		using (List<ItemStand>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.HaveAttachment())
				{
					user.Message(MessageHud.MessageType.Center, "$msg_incompleteoffering", 0, null);
					return false;
				}
			}
		}
		if (this.SpawnBoss(this.GetSpawnPosition()))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_offerdone", 0, null);
			foreach (ItemStand itemStand in list)
			{
				itemStand.DestroyAttachment();
			}
			if (this.m_itemSpawnPoint)
			{
				this.m_fuelAddedEffects.Create(this.m_itemSpawnPoint.position, base.transform.rotation, null, 1f, -1);
			}
		}
		return true;
	}

	// Token: 0x0600180F RID: 6159 RVA: 0x000A05A0 File Offset: 0x0009E7A0
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.m_useItemStands)
		{
			return false;
		}
		if (this.IsBossSpawnQueued())
		{
			return true;
		}
		if (!(this.m_bossItem != null))
		{
			return false;
		}
		if (!(item.m_shared.m_name == this.m_bossItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_offerwrong", 0, null);
			return true;
		}
		int num = user.GetInventory().CountItems(this.m_bossItem.m_itemData.m_shared.m_name, -1);
		if (num < this.m_bossItems)
		{
			user.Message(MessageHud.MessageType.Center, string.Concat(new string[]
			{
				"$msg_incompleteoffering: ",
				this.m_bossItem.m_itemData.m_shared.m_name,
				" ",
				num.ToString(),
				" / ",
				this.m_bossItems.ToString()
			}), 0, null);
			return true;
		}
		if (this.m_bossPrefab != null)
		{
			if (this.SpawnBoss(this.GetSpawnPosition()))
			{
				user.GetInventory().RemoveItem(item.m_shared.m_name, this.m_bossItems, -1);
				user.ShowRemovedMessage(this.m_bossItem.m_itemData, this.m_bossItems);
				user.Message(MessageHud.MessageType.Center, "$msg_offerdone", 0, null);
				if (this.m_itemSpawnPoint)
				{
					this.m_fuelAddedEffects.Create(this.m_itemSpawnPoint.position, base.transform.rotation, null, 1f, -1);
				}
			}
		}
		else if (this.m_itemPrefab != null && this.SpawnItem(this.m_itemPrefab, user as Player))
		{
			user.GetInventory().RemoveItem(item.m_shared.m_name, this.m_bossItems, -1);
			user.ShowRemovedMessage(this.m_bossItem.m_itemData, this.m_bossItems);
			user.Message(MessageHud.MessageType.Center, "$msg_offerdone", 0, null);
			this.m_fuelAddedEffects.Create(this.m_itemSpawnPoint.position, base.transform.rotation, null, 1f, -1);
		}
		if (!string.IsNullOrEmpty(this.m_setGlobalKey))
		{
			ZoneSystem.instance.SetGlobalKey(this.m_setGlobalKey);
		}
		return true;
	}

	// Token: 0x06001810 RID: 6160 RVA: 0x000A07E4 File Offset: 0x0009E9E4
	private bool SpawnItem(ItemDrop item, Player player)
	{
		if (item.m_itemData.m_shared.m_questItem && player.HaveUniqueKey(item.m_itemData.m_shared.m_name))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_cantoffer", 0, null);
			return false;
		}
		UnityEngine.Object.Instantiate<ItemDrop>(item, this.m_itemSpawnPoint.position, Quaternion.identity);
		return true;
	}

	// Token: 0x06001811 RID: 6161 RVA: 0x000A0844 File Offset: 0x0009EA44
	private Vector3 GetSpawnPosition()
	{
		if (this.m_spawnPoints.Count > 0)
		{
			return this.m_spawnPoints[UnityEngine.Random.Range(0, this.m_spawnPoints.Count)].transform.position;
		}
		return base.transform.position;
	}

	// Token: 0x06001812 RID: 6162 RVA: 0x000A0894 File Offset: 0x0009EA94
	private bool SpawnBoss(Vector3 point)
	{
		int i = 0;
		while (i < 100)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * this.m_spawnBossMaxDistance;
			Vector3 vector2 = point + new Vector3(vector.x, 0f, vector.y);
			if (this.m_enableSolidHeightCheck)
			{
				float num;
				ZoneSystem.instance.GetSolidHeight(vector2, out num, this.m_getSolidHeightMargin);
				if (num < 0f || Mathf.Abs(num - base.transform.position.y) > this.m_spawnBossMaxYDistance)
				{
					i++;
					continue;
				}
				vector2.y = num + this.m_spawnOffset;
			}
			this.m_spawnBossStartEffects.Create(vector2, Quaternion.identity, null, 1f, -1);
			this.m_bossSpawnPoint = vector2;
			base.Invoke("DelayedSpawnBoss", this.m_spawnBossDelay);
			return true;
		}
		return false;
	}

	// Token: 0x06001813 RID: 6163 RVA: 0x000A0968 File Offset: 0x0009EB68
	private bool IsBossSpawnQueued()
	{
		return base.IsInvoking("DelayedSpawnBoss");
	}

	// Token: 0x06001814 RID: 6164 RVA: 0x000A0978 File Offset: 0x0009EB78
	private void DelayedSpawnBoss()
	{
		BaseAI component = UnityEngine.Object.Instantiate<GameObject>(this.m_bossPrefab, this.m_bossSpawnPoint, Quaternion.identity).GetComponent<BaseAI>();
		if (component != null)
		{
			component.SetPatrolPoint();
		}
		this.m_spawnBossDoneffects.Create(this.m_bossSpawnPoint, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x06001815 RID: 6165 RVA: 0x000A09D0 File Offset: 0x0009EBD0
	private List<ItemStand> FindItemStands()
	{
		List<ItemStand> list = new List<ItemStand>();
		foreach (ItemStand itemStand in UnityEngine.Object.FindObjectsOfType<ItemStand>())
		{
			if (Vector3.Distance(base.transform.position, itemStand.transform.position) <= this.m_itemstandMaxRange && itemStand.gameObject.name.CustomStartsWith(this.m_itemStandPrefix))
			{
				list.Add(itemStand);
			}
		}
		return list;
	}

	// Token: 0x04001990 RID: 6544
	public string m_name = "Ancient bowl";

	// Token: 0x04001991 RID: 6545
	public string m_useItemText = "Burn item";

	// Token: 0x04001992 RID: 6546
	public ItemDrop m_bossItem;

	// Token: 0x04001993 RID: 6547
	public int m_bossItems = 1;

	// Token: 0x04001994 RID: 6548
	public GameObject m_bossPrefab;

	// Token: 0x04001995 RID: 6549
	public ItemDrop m_itemPrefab;

	// Token: 0x04001996 RID: 6550
	public Transform m_itemSpawnPoint;

	// Token: 0x04001997 RID: 6551
	public string m_setGlobalKey = "";

	// Token: 0x04001998 RID: 6552
	[Header("Boss")]
	public float m_spawnBossDelay = 5f;

	// Token: 0x04001999 RID: 6553
	public float m_spawnBossMaxDistance = 40f;

	// Token: 0x0400199A RID: 6554
	public float m_spawnBossMaxYDistance = 9999f;

	// Token: 0x0400199B RID: 6555
	public int m_getSolidHeightMargin = 1000;

	// Token: 0x0400199C RID: 6556
	public bool m_enableSolidHeightCheck = true;

	// Token: 0x0400199D RID: 6557
	public float m_spawnOffset = 1f;

	// Token: 0x0400199E RID: 6558
	public List<GameObject> m_spawnPoints = new List<GameObject>();

	// Token: 0x0400199F RID: 6559
	[Header("Use itemstands")]
	public bool m_useItemStands;

	// Token: 0x040019A0 RID: 6560
	public string m_itemStandPrefix = "";

	// Token: 0x040019A1 RID: 6561
	public float m_itemstandMaxRange = 20f;

	// Token: 0x040019A2 RID: 6562
	[Header("Effects")]
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x040019A3 RID: 6563
	public EffectList m_spawnBossStartEffects = new EffectList();

	// Token: 0x040019A4 RID: 6564
	public EffectList m_spawnBossDoneffects = new EffectList();

	// Token: 0x040019A5 RID: 6565
	private Vector3 m_bossSpawnPoint;
}
