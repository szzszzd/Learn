using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002B2 RID: 690
public class TreeBase : MonoBehaviour, IDestructible
{
	// Token: 0x06001A1E RID: 6686 RVA: 0x000ACD30 File Offset: 0x000AAF30
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
		this.m_nview.Register("Grow", new Action<long>(this.RPC_Grow));
		this.m_nview.Register("Shake", new Action<long>(this.RPC_Shake));
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health) <= 0f)
		{
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06001A1F RID: 6687 RVA: 0x00051493 File Offset: 0x0004F693
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Tree;
	}

	// Token: 0x06001A20 RID: 6688 RVA: 0x000ACDD7 File Offset: 0x000AAFD7
	public void Damage(HitData hit)
	{
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit
		});
	}

	// Token: 0x06001A21 RID: 6689 RVA: 0x000ACDF3 File Offset: 0x000AAFF3
	public void Grow()
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "Grow", Array.Empty<object>());
	}

	// Token: 0x06001A22 RID: 6690 RVA: 0x000ACE0F File Offset: 0x000AB00F
	private void RPC_Grow(long uid)
	{
		base.StartCoroutine("GrowAnimation");
	}

	// Token: 0x06001A23 RID: 6691 RVA: 0x000ACE1D File Offset: 0x000AB01D
	private IEnumerator GrowAnimation()
	{
		GameObject animatedTrunk = UnityEngine.Object.Instantiate<GameObject>(this.m_trunk, this.m_trunk.transform.position, this.m_trunk.transform.rotation, base.transform);
		animatedTrunk.isStatic = false;
		LODGroup component = base.transform.GetComponent<LODGroup>();
		if (component)
		{
			component.fadeMode = LODFadeMode.None;
		}
		this.m_trunk.SetActive(false);
		for (float t = 0f; t < 0.3f; t += Time.deltaTime)
		{
			float d = Mathf.Clamp01(t / 0.3f);
			animatedTrunk.transform.localScale = this.m_trunk.transform.localScale * d;
			yield return null;
		}
		UnityEngine.Object.Destroy(animatedTrunk);
		this.m_trunk.SetActive(true);
		if (this.m_nview.IsOwner())
		{
			this.m_respawnEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		}
		yield break;
	}

	// Token: 0x06001A24 RID: 6692 RVA: 0x000ACE2C File Offset: 0x000AB02C
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health);
		if (num <= 0f)
		{
			this.m_nview.Destroy();
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
		this.m_nview.GetZDO().Set(ZDOVars.s_health, num);
		this.Shake();
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
		if (closestPlayer)
		{
			closestPlayer.AddNoise(100f);
		}
		if (num <= 0f)
		{
			this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
			this.SpawnLog(hit.m_dir);
			List<GameObject> dropList = this.m_dropWhenDestroyed.GetDropList();
			for (int i = 0; i < dropList.Count; i++)
			{
				Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.5f;
				Vector3 position = base.transform.position + Vector3.up * this.m_spawnYOffset + new Vector3(vector.x, this.m_spawnYStep * (float)i, vector.y);
				Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				UnityEngine.Object.Instantiate<GameObject>(dropList[i], position, rotation);
			}
			base.gameObject.SetActive(false);
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06001A25 RID: 6693 RVA: 0x000AD044 File Offset: 0x000AB244
	private void Shake()
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "Shake", Array.Empty<object>());
	}

	// Token: 0x06001A26 RID: 6694 RVA: 0x000AD060 File Offset: 0x000AB260
	private void RPC_Shake(long uid)
	{
		base.StopCoroutine("ShakeAnimation");
		base.StartCoroutine("ShakeAnimation");
	}

	// Token: 0x06001A27 RID: 6695 RVA: 0x000AD079 File Offset: 0x000AB279
	private IEnumerator ShakeAnimation()
	{
		this.m_trunk.gameObject.isStatic = false;
		float t = Time.time;
		while (Time.time - t < 1f)
		{
			float time = Time.time;
			float num = 1f - Mathf.Clamp01((time - t) / 1f);
			float num2 = num * num * num * 1.5f;
			Quaternion localRotation = Quaternion.Euler(Mathf.Sin(time * 40f) * num2, 0f, Mathf.Cos(time * 0.9f * 40f) * num2);
			this.m_trunk.transform.localRotation = localRotation;
			yield return null;
		}
		this.m_trunk.transform.localRotation = Quaternion.identity;
		this.m_trunk.gameObject.isStatic = true;
		yield break;
	}

	// Token: 0x06001A28 RID: 6696 RVA: 0x000AD088 File Offset: 0x000AB288
	private void SpawnLog(Vector3 hitDir)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_logPrefab, this.m_logSpawnPoint.position, this.m_logSpawnPoint.rotation);
		gameObject.GetComponent<ZNetView>().SetLocalScale(base.transform.localScale);
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		component.mass *= base.transform.localScale.x;
		component.ResetInertiaTensor();
		component.AddForceAtPosition(hitDir * 0.2f, gameObject.transform.position + Vector3.up * 4f * base.transform.localScale.y, ForceMode.VelocityChange);
		if (this.m_stubPrefab)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_stubPrefab, base.transform.position, base.transform.rotation).GetComponent<ZNetView>().SetLocalScale(base.transform.localScale);
		}
	}

	// Token: 0x04001C03 RID: 7171
	private ZNetView m_nview;

	// Token: 0x04001C04 RID: 7172
	public float m_health = 1f;

	// Token: 0x04001C05 RID: 7173
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x04001C06 RID: 7174
	public int m_minToolTier;

	// Token: 0x04001C07 RID: 7175
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04001C08 RID: 7176
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04001C09 RID: 7177
	public EffectList m_respawnEffect = new EffectList();

	// Token: 0x04001C0A RID: 7178
	public GameObject m_trunk;

	// Token: 0x04001C0B RID: 7179
	public GameObject m_stubPrefab;

	// Token: 0x04001C0C RID: 7180
	public GameObject m_logPrefab;

	// Token: 0x04001C0D RID: 7181
	public Transform m_logSpawnPoint;

	// Token: 0x04001C0E RID: 7182
	[Header("Drops")]
	public DropTable m_dropWhenDestroyed = new DropTable();

	// Token: 0x04001C0F RID: 7183
	public float m_spawnYOffset = 0.5f;

	// Token: 0x04001C10 RID: 7184
	public float m_spawnYStep = 0.3f;
}
