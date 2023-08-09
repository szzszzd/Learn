using System;
using UnityEngine;

// Token: 0x02000268 RID: 616
public class MineRock : MonoBehaviour, IDestructible, Hoverable
{
	// Token: 0x060017B9 RID: 6073 RVA: 0x0009DBE4 File Offset: 0x0009BDE4
	private void Start()
	{
		this.m_hitAreas = ((this.m_areaRoot != null) ? this.m_areaRoot.GetComponentsInChildren<Collider>() : base.gameObject.GetComponentsInChildren<Collider>());
		if (this.m_baseModel)
		{
			this.m_areaMeshes = new MeshRenderer[this.m_hitAreas.Length][];
			for (int i = 0; i < this.m_hitAreas.Length; i++)
			{
				this.m_areaMeshes[i] = this.m_hitAreas[i].GetComponents<MeshRenderer>();
			}
		}
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.Register<HitData, int>("Hit", new Action<long, HitData, int>(this.RPC_Hit));
			this.m_nview.Register<int>("Hide", new Action<long, int>(this.RPC_Hide));
		}
		base.InvokeRepeating("UpdateVisability", UnityEngine.Random.Range(1f, 2f), 10f);
	}

	// Token: 0x060017BA RID: 6074 RVA: 0x0009DCE2 File Offset: 0x0009BEE2
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x060017BB RID: 6075 RVA: 0x0009DCF4 File Offset: 0x0009BEF4
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060017BC RID: 6076 RVA: 0x0009DCFC File Offset: 0x0009BEFC
	private void UpdateVisability()
	{
		bool flag = false;
		for (int i = 0; i < this.m_hitAreas.Length; i++)
		{
			Collider collider = this.m_hitAreas[i];
			if (collider)
			{
				string name = "Health" + i.ToString();
				bool flag2 = this.m_nview.GetZDO().GetFloat(name, this.m_health) > 0f;
				collider.gameObject.SetActive(flag2);
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if (this.m_baseModel)
		{
			this.m_baseModel.SetActive(!flag);
			foreach (MeshRenderer[] array in this.m_areaMeshes)
			{
				for (int k = 0; k < array.Length; k++)
				{
					array[k].enabled = flag;
				}
			}
		}
	}

	// Token: 0x060017BD RID: 6077 RVA: 0x0000290F File Offset: 0x00000B0F
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x060017BE RID: 6078 RVA: 0x0009DDD0 File Offset: 0x0009BFD0
	public void Damage(HitData hit)
	{
		if (hit.m_hitCollider == null)
		{
			ZLog.Log("Minerock hit has no collider");
			return;
		}
		int areaIndex = this.GetAreaIndex(hit.m_hitCollider);
		if (areaIndex == -1)
		{
			ZLog.Log("Invalid hit area on " + base.gameObject.name);
			return;
		}
		ZLog.Log("Hit mine rock area " + areaIndex.ToString());
		this.m_nview.InvokeRPC("Hit", new object[]
		{
			hit,
			areaIndex
		});
	}

	// Token: 0x060017BF RID: 6079 RVA: 0x0009DE5C File Offset: 0x0009C05C
	private void RPC_Hit(long sender, HitData hit, int hitAreaIndex)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		Collider hitArea = this.GetHitArea(hitAreaIndex);
		if (hitArea == null)
		{
			ZLog.Log("Missing hit area " + hitAreaIndex.ToString());
			return;
		}
		string name = "Health" + hitAreaIndex.ToString();
		float num = this.m_nview.GetZDO().GetFloat(name, this.m_health);
		if (num <= 0f)
		{
			ZLog.Log("Already destroyed");
			return;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damageModifiers, out type);
		float totalDamage = hit.GetTotalDamage();
		if ((int)hit.m_toolTier < this.m_minToolTier)
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f, false);
			return;
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return;
		}
		num -= totalDamage;
		this.m_nview.GetZDO().Set(name, num);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
		Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
		if (closestPlayer)
		{
			closestPlayer.AddNoise(100f);
		}
		if (this.m_onHit != null)
		{
			this.m_onHit();
		}
		if (num <= 0f)
		{
			this.m_destroyedEffect.Create(hitArea.bounds.center, Quaternion.identity, null, 1f, -1);
			this.m_nview.InvokeRPC(ZNetView.Everybody, "Hide", new object[]
			{
				hitAreaIndex
			});
			foreach (GameObject original in this.m_dropItems.GetDropList())
			{
				Vector3 position = hit.m_point - hit.m_dir * 0.2f + UnityEngine.Random.insideUnitSphere * 0.3f;
				UnityEngine.Object.Instantiate<GameObject>(original, position, Quaternion.identity);
			}
			if (this.m_removeWhenDestroyed && this.AllDestroyed())
			{
				this.m_nview.Destroy();
			}
		}
	}

	// Token: 0x060017C0 RID: 6080 RVA: 0x0009E098 File Offset: 0x0009C298
	private bool AllDestroyed()
	{
		for (int i = 0; i < this.m_hitAreas.Length; i++)
		{
			string name = "Health" + i.ToString();
			if (this.m_nview.GetZDO().GetFloat(name, this.m_health) > 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060017C1 RID: 6081 RVA: 0x0009E0EC File Offset: 0x0009C2EC
	private void RPC_Hide(long sender, int index)
	{
		Collider hitArea = this.GetHitArea(index);
		if (hitArea)
		{
			hitArea.gameObject.SetActive(false);
		}
		if (this.m_baseModel && this.m_baseModel.activeSelf)
		{
			this.m_baseModel.SetActive(false);
			foreach (MeshRenderer[] array in this.m_areaMeshes)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j].enabled = true;
				}
			}
		}
	}

	// Token: 0x060017C2 RID: 6082 RVA: 0x0009E170 File Offset: 0x0009C370
	private int GetAreaIndex(Collider area)
	{
		for (int i = 0; i < this.m_hitAreas.Length; i++)
		{
			if (this.m_hitAreas[i] == area)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060017C3 RID: 6083 RVA: 0x0009E1A3 File Offset: 0x0009C3A3
	private Collider GetHitArea(int index)
	{
		if (index < 0 || index >= this.m_hitAreas.Length)
		{
			return null;
		}
		return this.m_hitAreas[index];
	}

	// Token: 0x0400192F RID: 6447
	public string m_name = "";

	// Token: 0x04001930 RID: 6448
	public float m_health = 2f;

	// Token: 0x04001931 RID: 6449
	public bool m_removeWhenDestroyed = true;

	// Token: 0x04001932 RID: 6450
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x04001933 RID: 6451
	public int m_minToolTier;

	// Token: 0x04001934 RID: 6452
	public GameObject m_areaRoot;

	// Token: 0x04001935 RID: 6453
	public GameObject m_baseModel;

	// Token: 0x04001936 RID: 6454
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04001937 RID: 6455
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04001938 RID: 6456
	public DropTable m_dropItems;

	// Token: 0x04001939 RID: 6457
	public Action m_onHit;

	// Token: 0x0400193A RID: 6458
	private Collider[] m_hitAreas;

	// Token: 0x0400193B RID: 6459
	private MeshRenderer[][] m_areaMeshes;

	// Token: 0x0400193C RID: 6460
	private ZNetView m_nview;
}
