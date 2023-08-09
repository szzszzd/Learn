using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002B5 RID: 693
public class TreeLog : MonoBehaviour, IDestructible
{
	// Token: 0x06001A36 RID: 6710 RVA: 0x000AD48C File Offset: 0x000AB68C
	private void Awake()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_body.maxDepenetrationVelocity = 1f;
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
		if (this.m_nview.IsOwner())
		{
			float @float = this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, -1f);
			if (@float == -1f)
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_health, this.m_health);
			}
			else if (@float <= 0f)
			{
				this.m_nview.Destroy();
			}
		}
		base.Invoke("EnableDamage", 0.2f);
	}

	// Token: 0x06001A37 RID: 6711 RVA: 0x000AD54D File Offset: 0x000AB74D
	private void EnableDamage()
	{
		this.m_firstFrame = false;
	}

	// Token: 0x06001A38 RID: 6712 RVA: 0x00051493 File Offset: 0x0004F693
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Tree;
	}

	// Token: 0x06001A39 RID: 6713 RVA: 0x000AD556 File Offset: 0x000AB756
	public void Damage(HitData hit)
	{
		if (this.m_firstFrame)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit
		});
	}

	// Token: 0x06001A3A RID: 6714 RVA: 0x000AD58C File Offset: 0x000AB78C
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, 0f);
		if (num <= 0f)
		{
			return;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damages, out type);
		float totalDamage = hit.GetTotalDamage();
		if ((int)hit.m_toolTier < this.m_minToolTier)
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f, false);
			return;
		}
		if (this.m_body)
		{
			this.m_body.AddForceAtPosition(hit.m_dir * hit.m_pushForce * 2f, hit.m_point, ForceMode.Impulse);
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return;
		}
		num -= totalDamage;
		if (num < 0f)
		{
			num = 0f;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_health, num);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
		if (this.m_hitNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_hitNoise);
			}
		}
		if (num <= 0f)
		{
			this.Destroy();
		}
	}

	// Token: 0x06001A3B RID: 6715 RVA: 0x000AD6EC File Offset: 0x000AB8EC
	private void Destroy()
	{
		ZNetScene.instance.Destroy(base.gameObject);
		this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		List<GameObject> dropList = this.m_dropWhenDestroyed.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector3 position = base.transform.position + base.transform.up * UnityEngine.Random.Range(-this.m_spawnDistance, this.m_spawnDistance) + Vector3.up * 0.3f * (float)i;
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			UnityEngine.Object.Instantiate<GameObject>(dropList[i], position, rotation);
		}
		if (this.m_subLogPrefab != null)
		{
			foreach (Transform transform in this.m_subLogPoints)
			{
				Quaternion rotation2 = this.m_useSubLogPointRotation ? transform.rotation : base.transform.rotation;
				UnityEngine.Object.Instantiate<GameObject>(this.m_subLogPrefab, transform.position, rotation2).GetComponent<ZNetView>().SetLocalScale(base.transform.localScale);
			}
		}
	}

	// Token: 0x04001C1A RID: 7194
	public float m_health = 60f;

	// Token: 0x04001C1B RID: 7195
	public HitData.DamageModifiers m_damages;

	// Token: 0x04001C1C RID: 7196
	public int m_minToolTier;

	// Token: 0x04001C1D RID: 7197
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04001C1E RID: 7198
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04001C1F RID: 7199
	public DropTable m_dropWhenDestroyed = new DropTable();

	// Token: 0x04001C20 RID: 7200
	public GameObject m_subLogPrefab;

	// Token: 0x04001C21 RID: 7201
	public Transform[] m_subLogPoints = Array.Empty<Transform>();

	// Token: 0x04001C22 RID: 7202
	public bool m_useSubLogPointRotation;

	// Token: 0x04001C23 RID: 7203
	public float m_spawnDistance = 2f;

	// Token: 0x04001C24 RID: 7204
	public float m_hitNoise = 100f;

	// Token: 0x04001C25 RID: 7205
	private Rigidbody m_body;

	// Token: 0x04001C26 RID: 7206
	private ZNetView m_nview;

	// Token: 0x04001C27 RID: 7207
	private bool m_firstFrame = true;
}
