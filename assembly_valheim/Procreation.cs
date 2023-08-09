using System;
using UnityEngine;

// Token: 0x02000029 RID: 41
public class Procreation : MonoBehaviour
{
	// Token: 0x060002DE RID: 734 RVA: 0x000165A0 File Offset: 0x000147A0
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_baseAI = base.GetComponent<BaseAI>();
		this.m_character = base.GetComponent<Character>();
		this.m_tameable = base.GetComponent<Tameable>();
		base.InvokeRepeating("Procreate", UnityEngine.Random.Range(this.m_updateInterval, this.m_updateInterval + this.m_updateInterval * 0.5f), this.m_updateInterval);
	}

	// Token: 0x060002DF RID: 735 RVA: 0x0001660C File Offset: 0x0001480C
	private void Procreate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_character.IsTamed())
		{
			return;
		}
		if (this.m_offspringPrefab == null)
		{
			string prefabName = Utils.GetPrefabName(this.m_offspring);
			this.m_offspringPrefab = ZNetScene.instance.GetPrefab(prefabName);
			int prefab = this.m_nview.GetZDO().GetPrefab();
			this.m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
		}
		if (this.IsPregnant())
		{
			if (this.IsDue())
			{
				this.ResetPregnancy();
				GameObject original = this.m_offspringPrefab;
				if (this.m_noPartnerOffspring)
				{
					int nrOfInstances = SpawnSystem.GetNrOfInstances(this.m_seperatePartner ? this.m_seperatePartner : this.m_myPrefab, base.transform.position, this.m_partnerCheckRange, false, true);
					if ((!this.m_seperatePartner && nrOfInstances < 2) || (this.m_seperatePartner && nrOfInstances < 1))
					{
						original = this.m_noPartnerOffspring;
					}
				}
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, base.transform.position - base.transform.forward * this.m_spawnOffset, Quaternion.LookRotation(-base.transform.forward, Vector3.up));
				Character component = gameObject.GetComponent<Character>();
				if (component)
				{
					component.SetTamed(this.m_character.IsTamed());
					component.SetLevel(Mathf.Max(this.m_minOffspringLevel, this.m_character.GetLevel()));
				}
				this.m_birthEffects.Create(gameObject.transform.position, Quaternion.identity, null, 1f, -1);
				return;
			}
		}
		else
		{
			if (UnityEngine.Random.value <= this.m_pregnancyChance)
			{
				return;
			}
			if (this.m_baseAI.IsAlerted())
			{
				return;
			}
			if (this.m_tameable.IsHungry())
			{
				return;
			}
			int nrOfInstances2 = SpawnSystem.GetNrOfInstances(this.m_myPrefab, base.transform.position, this.m_totalCheckRange, false, false);
			int nrOfInstances3 = SpawnSystem.GetNrOfInstances(this.m_offspringPrefab, base.transform.position, this.m_totalCheckRange, false, false);
			if (nrOfInstances2 + nrOfInstances3 >= this.m_maxCreatures)
			{
				return;
			}
			int nrOfInstances4 = SpawnSystem.GetNrOfInstances(this.m_seperatePartner ? this.m_seperatePartner : this.m_myPrefab, base.transform.position, this.m_partnerCheckRange, false, true);
			if (!this.m_noPartnerOffspring && ((!this.m_seperatePartner && nrOfInstances4 < 2) || (this.m_seperatePartner && nrOfInstances4 < 1)))
			{
				return;
			}
			if (nrOfInstances4 > 0)
			{
				this.m_loveEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			}
			int num = this.m_nview.GetZDO().GetInt(ZDOVars.s_lovePoints, 0);
			num++;
			this.m_nview.GetZDO().Set(ZDOVars.s_lovePoints, num, false);
			if (num >= this.m_requiredLovePoints)
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_lovePoints, 0, false);
				this.MakePregnant();
			}
		}
	}

	// Token: 0x060002E0 RID: 736 RVA: 0x00016930 File Offset: 0x00014B30
	public bool ReadyForProcreation()
	{
		return this.m_character.IsTamed() && !this.IsPregnant() && !this.m_tameable.IsHungry();
	}

	// Token: 0x060002E1 RID: 737 RVA: 0x00016958 File Offset: 0x00014B58
	private void MakePregnant()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_pregnant, ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x060002E2 RID: 738 RVA: 0x0001698C File Offset: 0x00014B8C
	private void ResetPregnancy()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_pregnant, 0L);
	}

	// Token: 0x060002E3 RID: 739 RVA: 0x000169A8 File Offset: 0x00014BA8
	private bool IsDue()
	{
		long @long = this.m_nview.GetZDO().GetLong(ZDOVars.s_pregnant, 0L);
		if (@long == 0L)
		{
			return false;
		}
		DateTime d = new DateTime(@long);
		return (ZNet.instance.GetTime() - d).TotalSeconds > (double)this.m_pregnancyDuration;
	}

	// Token: 0x060002E4 RID: 740 RVA: 0x000169FB File Offset: 0x00014BFB
	private bool IsPregnant()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetLong(ZDOVars.s_pregnant, 0L) != 0L;
	}

	// Token: 0x040002A1 RID: 673
	public float m_updateInterval = 10f;

	// Token: 0x040002A2 RID: 674
	public float m_totalCheckRange = 10f;

	// Token: 0x040002A3 RID: 675
	public int m_maxCreatures = 4;

	// Token: 0x040002A4 RID: 676
	public float m_partnerCheckRange = 3f;

	// Token: 0x040002A5 RID: 677
	public float m_pregnancyChance = 0.5f;

	// Token: 0x040002A6 RID: 678
	public float m_pregnancyDuration = 10f;

	// Token: 0x040002A7 RID: 679
	public int m_requiredLovePoints = 4;

	// Token: 0x040002A8 RID: 680
	public GameObject m_offspring;

	// Token: 0x040002A9 RID: 681
	public int m_minOffspringLevel;

	// Token: 0x040002AA RID: 682
	public float m_spawnOffset = 2f;

	// Token: 0x040002AB RID: 683
	public GameObject m_seperatePartner;

	// Token: 0x040002AC RID: 684
	public GameObject m_noPartnerOffspring;

	// Token: 0x040002AD RID: 685
	public EffectList m_birthEffects = new EffectList();

	// Token: 0x040002AE RID: 686
	public EffectList m_loveEffects = new EffectList();

	// Token: 0x040002AF RID: 687
	private GameObject m_myPrefab;

	// Token: 0x040002B0 RID: 688
	private GameObject m_offspringPrefab;

	// Token: 0x040002B1 RID: 689
	private ZNetView m_nview;

	// Token: 0x040002B2 RID: 690
	private BaseAI m_baseAI;

	// Token: 0x040002B3 RID: 691
	private Character m_character;

	// Token: 0x040002B4 RID: 692
	private Tameable m_tameable;
}
