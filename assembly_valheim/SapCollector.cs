using System;
using UnityEngine;

// Token: 0x0200028A RID: 650
public class SapCollector : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x060018C8 RID: 6344 RVA: 0x000A5120 File Offset: 0x000A3320
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
		this.m_nview.Register("RPC_UpdateEffects", new Action<long>(this.RPC_UpdateEffects));
		base.InvokeRepeating("UpdateTick", UnityEngine.Random.Range(0f, 2f), 5f);
	}

	// Token: 0x060018C9 RID: 6345 RVA: 0x000A5204 File Offset: 0x000A3404
	public string GetHoverText()
	{
		int level = this.GetLevel();
		string statusText = this.GetStatusText();
		string text = string.Concat(new string[]
		{
			this.m_name,
			" ( ",
			statusText,
			", ",
			level.ToString(),
			" / ",
			this.m_maxLevel.ToString(),
			" )"
		});
		if (level > 0)
		{
			text = text + "\n[<color=yellow><b>$KEY_Use</b></color>] " + this.m_extractText;
		}
		return Localization.instance.Localize(text);
	}

	// Token: 0x060018CA RID: 6346 RVA: 0x000A5291 File Offset: 0x000A3491
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060018CB RID: 6347 RVA: 0x000A5299 File Offset: 0x000A3499
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
		if (this.GetLevel() > 0)
		{
			this.Extract();
			return true;
		}
		return false;
	}

	// Token: 0x060018CC RID: 6348 RVA: 0x000A52D0 File Offset: 0x000A34D0
	private string GetStatusText()
	{
		if (this.GetLevel() >= this.m_maxLevel)
		{
			return this.m_fullText;
		}
		if (!this.m_root)
		{
			return this.m_notConnectedText;
		}
		if (this.m_root.IsLevelLow())
		{
			return this.m_drainingSlowText;
		}
		return this.m_drainingText;
	}

	// Token: 0x060018CD RID: 6349 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060018CE RID: 6350 RVA: 0x000A5320 File Offset: 0x000A3520
	private void Extract()
	{
		this.m_nview.InvokeRPC("RPC_Extract", Array.Empty<object>());
	}

	// Token: 0x060018CF RID: 6351 RVA: 0x000A5338 File Offset: 0x000A3538
	private void RPC_Extract(long caller)
	{
		int level = this.GetLevel();
		if (level > 0)
		{
			this.m_spawnEffect.Create(this.m_spawnPoint.position, Quaternion.identity, null, 1f, -1);
			for (int i = 0; i < level; i++)
			{
				Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
				Vector3 position = this.m_spawnPoint.position + insideUnitSphere * 0.2f;
				UnityEngine.Object.Instantiate<ItemDrop>(this.m_spawnItem, position, Quaternion.identity);
			}
			this.ResetLevel();
			this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateEffects", Array.Empty<object>());
		}
	}

	// Token: 0x060018D0 RID: 6352 RVA: 0x000A53D8 File Offset: 0x000A35D8
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

	// Token: 0x060018D1 RID: 6353 RVA: 0x000A5463 File Offset: 0x000A3663
	private void ResetLevel()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_level, 0, false);
	}

	// Token: 0x060018D2 RID: 6354 RVA: 0x000A547C File Offset: 0x000A367C
	private void IncreseLevel(int i)
	{
		int num = this.GetLevel();
		num += i;
		num = Mathf.Clamp(num, 0, this.m_maxLevel);
		this.m_nview.GetZDO().Set(ZDOVars.s_level, num, false);
	}

	// Token: 0x060018D3 RID: 6355 RVA: 0x000A54B9 File Offset: 0x000A36B9
	private int GetLevel()
	{
		return this.m_nview.GetZDO().GetInt(ZDOVars.s_level, 0);
	}

	// Token: 0x060018D4 RID: 6356 RVA: 0x000A54D4 File Offset: 0x000A36D4
	private void UpdateTick()
	{
		if (this.m_mustConnectTo && !this.m_root)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 0.2f);
			for (int i = 0; i < array.Length; i++)
			{
				ResourceRoot componentInParent = array[i].GetComponentInParent<ResourceRoot>();
				if (componentInParent != null)
				{
					this.m_root = componentInParent;
					break;
				}
			}
		}
		if (this.m_nview.IsOwner())
		{
			float timeSinceLastUpdate = this.GetTimeSinceLastUpdate();
			if (this.GetLevel() < this.m_maxLevel && this.m_root && this.m_root.CanDrain(1f))
			{
				float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_product, 0f);
				num += timeSinceLastUpdate;
				if (num > this.m_secPerUnit)
				{
					int num2 = (int)(num / this.m_secPerUnit);
					if (this.m_root)
					{
						num2 = Mathf.Min((int)this.m_root.GetLevel(), num2);
					}
					if (num2 > 0)
					{
						this.IncreseLevel(num2);
						if (this.m_root)
						{
							this.m_root.Drain((float)num2);
						}
					}
					num = 0f;
				}
				this.m_nview.GetZDO().Set(ZDOVars.s_product, num);
			}
		}
		this.UpdateEffects();
	}

	// Token: 0x060018D5 RID: 6357 RVA: 0x000A5628 File Offset: 0x000A3828
	private void RPC_UpdateEffects(long caller)
	{
		this.UpdateEffects();
	}

	// Token: 0x060018D6 RID: 6358 RVA: 0x000A5630 File Offset: 0x000A3830
	private void UpdateEffects()
	{
		int level = this.GetLevel();
		bool active = level < this.m_maxLevel && this.m_root && this.m_root.CanDrain(1f);
		this.m_notEmptyEffect.SetActive(level > 0);
		this.m_workingEffect.SetActive(active);
	}

	// Token: 0x04001AB9 RID: 6841
	public string m_name = "";

	// Token: 0x04001ABA RID: 6842
	public Transform m_spawnPoint;

	// Token: 0x04001ABB RID: 6843
	public GameObject m_workingEffect;

	// Token: 0x04001ABC RID: 6844
	public GameObject m_notEmptyEffect;

	// Token: 0x04001ABD RID: 6845
	public float m_secPerUnit = 10f;

	// Token: 0x04001ABE RID: 6846
	public int m_maxLevel = 4;

	// Token: 0x04001ABF RID: 6847
	public ItemDrop m_spawnItem;

	// Token: 0x04001AC0 RID: 6848
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x04001AC1 RID: 6849
	public ZNetView m_mustConnectTo;

	// Token: 0x04001AC2 RID: 6850
	public bool m_rayCheckConnectedBelow;

	// Token: 0x04001AC3 RID: 6851
	[Header("Texts")]
	public string m_extractText = "$piece_sapcollector_extract";

	// Token: 0x04001AC4 RID: 6852
	public string m_drainingText = "$piece_sapcollector_draining";

	// Token: 0x04001AC5 RID: 6853
	public string m_drainingSlowText = "$piece_sapcollector_drainingslow";

	// Token: 0x04001AC6 RID: 6854
	public string m_notConnectedText = "$piece_sapcollector_notconnected";

	// Token: 0x04001AC7 RID: 6855
	public string m_fullText = "$piece_sapcollector_isfull";

	// Token: 0x04001AC8 RID: 6856
	private ZNetView m_nview;

	// Token: 0x04001AC9 RID: 6857
	private Collider m_collider;

	// Token: 0x04001ACA RID: 6858
	private Piece m_piece;

	// Token: 0x04001ACB RID: 6859
	private ZNetView m_connectedObject;

	// Token: 0x04001ACC RID: 6860
	private ResourceRoot m_root;
}
