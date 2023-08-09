using System;
using UnityEngine;

// Token: 0x02000074 RID: 116
public class Gibber : MonoBehaviour
{
	// Token: 0x06000584 RID: 1412 RVA: 0x0002B068 File Offset: 0x00029268
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
	}

	// Token: 0x06000585 RID: 1413 RVA: 0x0002B078 File Offset: 0x00029278
	private void Start()
	{
		Vector3 vector = base.transform.position;
		Vector3 vector2 = Vector3.zero;
		if (this.m_nview && this.m_nview.IsValid())
		{
			vector = this.m_nview.GetZDO().GetVec3(ZDOVars.s_hitPoint, vector);
			vector2 = this.m_nview.GetZDO().GetVec3(ZDOVars.s_hitDir, vector2);
		}
		if (this.m_delay > 0f)
		{
			base.Invoke("Explode", this.m_delay);
			return;
		}
		this.Explode(vector, vector2);
	}

	// Token: 0x06000586 RID: 1414 RVA: 0x0002B108 File Offset: 0x00029308
	public void Setup(Vector3 hitPoint, Vector3 hitDir)
	{
		if (this.m_nview && this.m_nview.IsValid())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_hitPoint, hitPoint);
			this.m_nview.GetZDO().Set(ZDOVars.s_hitDir, hitDir);
		}
	}

	// Token: 0x06000587 RID: 1415 RVA: 0x0002B15C File Offset: 0x0002935C
	private void DestroyAll()
	{
		if (this.m_nview)
		{
			if (!this.m_nview.GetZDO().HasOwner())
			{
				this.m_nview.ClaimOwnership();
			}
			if (this.m_nview.IsOwner())
			{
				ZNetScene.instance.Destroy(base.gameObject);
				return;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000588 RID: 1416 RVA: 0x0002B1BC File Offset: 0x000293BC
	private void CreateBodies()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = componentsInChildren[i].gameObject;
			if (this.m_chanceToRemoveGib > 0f && UnityEngine.Random.value < this.m_chanceToRemoveGib)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			else if (!gameObject.GetComponent<Rigidbody>())
			{
				gameObject.AddComponent<BoxCollider>();
				gameObject.AddComponent<Rigidbody>().maxDepenetrationVelocity = 2f;
				TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
				timedDestruction.m_timeout = UnityEngine.Random.Range(this.m_timeout / 2f, this.m_timeout);
				timedDestruction.Trigger();
			}
		}
	}

	// Token: 0x06000589 RID: 1417 RVA: 0x0002B25D File Offset: 0x0002945D
	private void Explode()
	{
		this.Explode(Vector3.zero, Vector3.zero);
	}

	// Token: 0x0600058A RID: 1418 RVA: 0x0002B270 File Offset: 0x00029470
	private void Explode(Vector3 hitPoint, Vector3 hitDir)
	{
		base.InvokeRepeating("DestroyAll", this.m_timeout, 1f);
		float t = ((double)hitDir.magnitude > 0.01) ? this.m_impactDirectionMix : 0f;
		this.CreateBodies();
		Rigidbody[] componentsInChildren = base.gameObject.GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		int num = 0;
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			vector += rigidbody.worldCenterOfMass;
			num++;
		}
		vector /= (float)num;
		foreach (Rigidbody rigidbody2 in componentsInChildren)
		{
			float d = UnityEngine.Random.Range(this.m_minVel, this.m_maxVel);
			Vector3 a = Vector3.Lerp(Vector3.Normalize(rigidbody2.worldCenterOfMass - vector), hitDir, t);
			rigidbody2.velocity = a * d;
			rigidbody2.angularVelocity = new Vector3(UnityEngine.Random.Range(-this.m_maxRotVel, this.m_maxRotVel), UnityEngine.Random.Range(-this.m_maxRotVel, this.m_maxRotVel), UnityEngine.Random.Range(-this.m_maxRotVel, this.m_maxRotVel));
		}
		foreach (Gibber.GibbData gibbData in this.m_gibbs)
		{
			if (gibbData.m_object && gibbData.m_chanceToSpawn < 1f && UnityEngine.Random.value > gibbData.m_chanceToSpawn)
			{
				UnityEngine.Object.Destroy(gibbData.m_object);
			}
		}
		if ((double)hitDir.magnitude > 0.01)
		{
			Quaternion baseRot = Quaternion.LookRotation(hitDir);
			this.m_punchEffector.Create(hitPoint, baseRot, null, 1f, -1);
		}
	}

	// Token: 0x04000681 RID: 1665
	public EffectList m_punchEffector = new EffectList();

	// Token: 0x04000682 RID: 1666
	public GameObject m_gibHitEffect;

	// Token: 0x04000683 RID: 1667
	public GameObject m_gibDestroyEffect;

	// Token: 0x04000684 RID: 1668
	public float m_gibHitDestroyChance;

	// Token: 0x04000685 RID: 1669
	public Gibber.GibbData[] m_gibbs = new Gibber.GibbData[0];

	// Token: 0x04000686 RID: 1670
	public float m_minVel = 10f;

	// Token: 0x04000687 RID: 1671
	public float m_maxVel = 20f;

	// Token: 0x04000688 RID: 1672
	public float m_maxRotVel = 20f;

	// Token: 0x04000689 RID: 1673
	public float m_impactDirectionMix = 0.5f;

	// Token: 0x0400068A RID: 1674
	public float m_timeout = 5f;

	// Token: 0x0400068B RID: 1675
	public float m_delay;

	// Token: 0x0400068C RID: 1676
	[Range(0f, 1f)]
	public float m_chanceToRemoveGib;

	// Token: 0x0400068D RID: 1677
	private ZNetView m_nview;

	// Token: 0x02000075 RID: 117
	[Serializable]
	public class GibbData
	{
		// Token: 0x0400068E RID: 1678
		public GameObject m_object;

		// Token: 0x0400068F RID: 1679
		public float m_chanceToSpawn = 1f;
	}
}
