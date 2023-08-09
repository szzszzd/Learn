using System;
using UnityEngine;

// Token: 0x02000218 RID: 536
public class Beehive : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06001550 RID: 5456 RVA: 0x0008BDE4 File Offset: 0x00089FE4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_collider = base.GetComponentInChildren<Collider>();
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(ZDOVars.s_lastTime, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_lastTime, ZNet.instance.GetTime().Ticks);
		}
		this.m_nview.Register("RPC_Extract", new Action<long>(this.RPC_Extract));
		base.InvokeRepeating("UpdateBees", 0f, 10f);
	}

	// Token: 0x06001551 RID: 5457 RVA: 0x0008BEA4 File Offset: 0x0008A0A4
	public string GetHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		int honeyLevel = this.GetHoneyLevel();
		if (honeyLevel > 0)
		{
			return Localization.instance.Localize(string.Format("{0} ( {1} x {2} )\n[<color=yellow><b>$KEY_Use</b></color>] {3}", new object[]
			{
				this.m_name,
				this.m_honeyItem.m_itemData.m_shared.m_name,
				honeyLevel,
				this.m_extractText
			}));
		}
		return Localization.instance.Localize(this.m_name + " ( $piece_container_empty )\n[<color=yellow><b>$KEY_Use</b></color>] " + this.m_checkText);
	}

	// Token: 0x06001552 RID: 5458 RVA: 0x0008BF5E File Offset: 0x0008A15E
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001553 RID: 5459 RVA: 0x0008BF68 File Offset: 0x0008A168
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		if (this.GetHoneyLevel() > 0)
		{
			this.Extract();
		}
		else
		{
			if (!this.CheckBiome())
			{
				character.Message(MessageHud.MessageType.Center, this.m_areaText, 0, null);
				return true;
			}
			if (!this.HaveFreeSpace())
			{
				character.Message(MessageHud.MessageType.Center, this.m_freespaceText, 0, null);
				return true;
			}
			if (!EnvMan.instance.IsDaylight() && this.m_effectOnlyInDaylight)
			{
				character.Message(MessageHud.MessageType.Center, this.m_sleepText, 0, null);
				return true;
			}
			character.Message(MessageHud.MessageType.Center, this.m_happyText, 0, null);
		}
		return true;
	}

	// Token: 0x06001554 RID: 5460 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001555 RID: 5461 RVA: 0x0008C00D File Offset: 0x0008A20D
	private void Extract()
	{
		this.m_nview.InvokeRPC("RPC_Extract", Array.Empty<object>());
	}

	// Token: 0x06001556 RID: 5462 RVA: 0x0008C024 File Offset: 0x0008A224
	private void RPC_Extract(long caller)
	{
		int honeyLevel = this.GetHoneyLevel();
		if (honeyLevel > 0)
		{
			this.m_spawnEffect.Create(this.m_spawnPoint.position, Quaternion.identity, null, 1f, -1);
			for (int i = 0; i < honeyLevel; i++)
			{
				Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.5f;
				Vector3 position = this.m_spawnPoint.position + new Vector3(vector.x, 0.25f * (float)i, vector.y);
				UnityEngine.Object.Instantiate<ItemDrop>(this.m_honeyItem, position, Quaternion.identity);
			}
			this.ResetLevel();
		}
	}

	// Token: 0x06001557 RID: 5463 RVA: 0x0008C0C0 File Offset: 0x0008A2C0
	private float GetTimeSinceLastUpdate()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_lastTime, ZNet.instance.GetTime().Ticks));
		DateTime time = ZNet.instance.GetTime();
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set(ZDOVars.s_lastTime, time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return (float)num;
	}

	// Token: 0x06001558 RID: 5464 RVA: 0x0008C14B File Offset: 0x0008A34B
	private void ResetLevel()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_level, 0, false);
	}

	// Token: 0x06001559 RID: 5465 RVA: 0x0008C164 File Offset: 0x0008A364
	private void IncreseLevel(int i)
	{
		int num = this.GetHoneyLevel();
		num += i;
		num = Mathf.Clamp(num, 0, this.m_maxHoney);
		this.m_nview.GetZDO().Set(ZDOVars.s_level, num, false);
	}

	// Token: 0x0600155A RID: 5466 RVA: 0x0008C1A1 File Offset: 0x0008A3A1
	private int GetHoneyLevel()
	{
		return this.m_nview.GetZDO().GetInt(ZDOVars.s_level, 0);
	}

	// Token: 0x0600155B RID: 5467 RVA: 0x0008C1BC File Offset: 0x0008A3BC
	private void UpdateBees()
	{
		bool flag = this.CheckBiome() && this.HaveFreeSpace();
		bool active = flag && (!this.m_effectOnlyInDaylight || EnvMan.instance.IsDaylight());
		this.m_beeEffect.SetActive(active);
		if (this.m_nview.IsOwner() && flag)
		{
			float timeSinceLastUpdate = this.GetTimeSinceLastUpdate();
			float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_product, 0f);
			num += timeSinceLastUpdate;
			if (num > this.m_secPerUnit)
			{
				int i = (int)(num / this.m_secPerUnit);
				this.IncreseLevel(i);
				num = 0f;
			}
			this.m_nview.GetZDO().Set(ZDOVars.s_product, num);
		}
	}

	// Token: 0x0600155C RID: 5468 RVA: 0x0008C274 File Offset: 0x0008A474
	private bool HaveFreeSpace()
	{
		if (this.m_maxCover <= 0f)
		{
			return true;
		}
		float num;
		bool flag;
		Cover.GetCoverForPoint(this.m_coverPoint.position, out num, out flag, 0.5f);
		return num < this.m_maxCover;
	}

	// Token: 0x0600155D RID: 5469 RVA: 0x0008C2B2 File Offset: 0x0008A4B2
	private bool CheckBiome()
	{
		return (Heightmap.FindBiome(base.transform.position) & this.m_biome) > Heightmap.Biome.None;
	}

	// Token: 0x04001616 RID: 5654
	public string m_name = "";

	// Token: 0x04001617 RID: 5655
	public Transform m_coverPoint;

	// Token: 0x04001618 RID: 5656
	public Transform m_spawnPoint;

	// Token: 0x04001619 RID: 5657
	public GameObject m_beeEffect;

	// Token: 0x0400161A RID: 5658
	public bool m_effectOnlyInDaylight = true;

	// Token: 0x0400161B RID: 5659
	public float m_maxCover = 0.25f;

	// Token: 0x0400161C RID: 5660
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	// Token: 0x0400161D RID: 5661
	public float m_secPerUnit = 10f;

	// Token: 0x0400161E RID: 5662
	public int m_maxHoney = 4;

	// Token: 0x0400161F RID: 5663
	public ItemDrop m_honeyItem;

	// Token: 0x04001620 RID: 5664
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x04001621 RID: 5665
	[Header("Texts")]
	public string m_extractText = "$piece_beehive_extract";

	// Token: 0x04001622 RID: 5666
	public string m_checkText = "$piece_beehive_check";

	// Token: 0x04001623 RID: 5667
	public string m_areaText = "$piece_beehive_area";

	// Token: 0x04001624 RID: 5668
	public string m_freespaceText = "$piece_beehive_freespace";

	// Token: 0x04001625 RID: 5669
	public string m_sleepText = "$piece_beehive_sleep";

	// Token: 0x04001626 RID: 5670
	public string m_happyText = "$piece_beehive_happy";

	// Token: 0x04001627 RID: 5671
	public string m_notConnectedText;

	// Token: 0x04001628 RID: 5672
	public string m_blockedText;

	// Token: 0x04001629 RID: 5673
	private ZNetView m_nview;

	// Token: 0x0400162A RID: 5674
	private Collider m_collider;

	// Token: 0x0400162B RID: 5675
	private Piece m_piece;

	// Token: 0x0400162C RID: 5676
	private ZNetView m_connectedObject;

	// Token: 0x0400162D RID: 5677
	private Piece m_blockingPiece;
}
