using System;
using UnityEngine;

// Token: 0x02000077 RID: 119
public class ImpactEffect : MonoBehaviour
{
	// Token: 0x06000590 RID: 1424 RVA: 0x0002B71B File Offset: 0x0002991B
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		if (this.m_maxVelocity < this.m_minVelocity)
		{
			this.m_maxVelocity = this.m_minVelocity;
		}
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x0002B750 File Offset: 0x00029950
	public void OnCollisionEnter(Collision info)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview && !this.m_nview.IsOwner())
		{
			return;
		}
		if (info.contacts.Length == 0)
		{
			return;
		}
		if (!this.m_hitEffectEnabled)
		{
			return;
		}
		if ((this.m_triggerMask.value & 1 << info.collider.gameObject.layer) == 0)
		{
			return;
		}
		float magnitude = info.relativeVelocity.magnitude;
		if (magnitude < this.m_minVelocity)
		{
			return;
		}
		ContactPoint contactPoint = info.contacts[0];
		Vector3 point = contactPoint.point;
		Vector3 pointVelocity = this.m_body.GetPointVelocity(point);
		this.m_hitEffectEnabled = false;
		base.Invoke("ResetHitTimer", this.m_interval);
		if (this.m_damages.HaveDamage())
		{
			GameObject gameObject = Projectile.FindHitObject(contactPoint.otherCollider);
			float num = Utils.LerpStep(this.m_minVelocity, this.m_maxVelocity, magnitude);
			IDestructible component = gameObject.GetComponent<IDestructible>();
			if (component != null)
			{
				Character character = component as Character;
				if (character)
				{
					if (!this.m_damagePlayers && character.IsPlayer())
					{
						return;
					}
					float num2 = Vector3.Dot(-info.relativeVelocity.normalized, pointVelocity);
					if (num2 < this.m_minVelocity)
					{
						return;
					}
					ZLog.Log("Rel vel " + num2.ToString());
					num = Utils.LerpStep(this.m_minVelocity, this.m_maxVelocity, num2);
					if (character.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.DoubleImpactDamage))
					{
						num *= 2f;
					}
				}
				if (!this.m_damageFish && gameObject.GetComponent<Fish>())
				{
					return;
				}
				HitData hitData = new HitData();
				hitData.m_point = point;
				hitData.m_dir = pointVelocity.normalized;
				hitData.m_hitCollider = info.collider;
				hitData.m_toolTier = (short)this.m_toolTier;
				hitData.m_damage = this.m_damages.Clone();
				hitData.m_damage.Modify(num);
				component.Damage(hitData);
			}
			if (this.m_damageToSelf)
			{
				IDestructible component2 = base.GetComponent<IDestructible>();
				if (component2 != null)
				{
					HitData hitData2 = new HitData();
					hitData2.m_point = point;
					hitData2.m_dir = -pointVelocity.normalized;
					hitData2.m_toolTier = (short)this.m_toolTier;
					hitData2.m_damage = this.m_damages.Clone();
					hitData2.m_damage.Modify(num);
					component2.Damage(hitData2);
				}
			}
		}
		Vector3 rhs = Vector3.Cross(-Vector3.Normalize(info.relativeVelocity), contactPoint.normal);
		Vector3 vector = Vector3.Cross(contactPoint.normal, rhs);
		Quaternion baseRot = Quaternion.identity;
		if (vector != Vector3.zero && contactPoint.normal != Vector3.zero)
		{
			baseRot = Quaternion.LookRotation(vector, contactPoint.normal);
		}
		this.m_hitEffect.Create(point, baseRot, null, 1f, -1);
		if (this.m_firstHit && this.m_hitDestroyChance > 0f && UnityEngine.Random.value <= this.m_hitDestroyChance)
		{
			this.m_destroyEffect.Create(point, baseRot, null, 1f, -1);
			GameObject gameObject2 = base.gameObject;
			if (base.transform.parent)
			{
				Animator componentInParent = base.transform.GetComponentInParent<Animator>();
				if (componentInParent)
				{
					gameObject2 = componentInParent.gameObject;
				}
			}
			UnityEngine.Object.Destroy(gameObject2);
		}
		this.m_firstHit = false;
	}

	// Token: 0x06000592 RID: 1426 RVA: 0x0002BAC8 File Offset: 0x00029CC8
	private Vector3 GetAVGPos(ContactPoint[] points)
	{
		ZLog.Log("Pooints " + points.Length.ToString());
		Vector3 vector = Vector3.zero;
		foreach (ContactPoint contactPoint in points)
		{
			ZLog.Log("P " + contactPoint.otherCollider.gameObject.name);
			vector += contactPoint.point;
		}
		return vector;
	}

	// Token: 0x06000593 RID: 1427 RVA: 0x0002BB3C File Offset: 0x00029D3C
	private void ResetHitTimer()
	{
		this.m_hitEffectEnabled = true;
	}

	// Token: 0x0400069D RID: 1693
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x0400069E RID: 1694
	public EffectList m_destroyEffect = new EffectList();

	// Token: 0x0400069F RID: 1695
	public float m_hitDestroyChance;

	// Token: 0x040006A0 RID: 1696
	public float m_minVelocity;

	// Token: 0x040006A1 RID: 1697
	public float m_maxVelocity;

	// Token: 0x040006A2 RID: 1698
	public bool m_damageToSelf;

	// Token: 0x040006A3 RID: 1699
	public bool m_damagePlayers = true;

	// Token: 0x040006A4 RID: 1700
	public bool m_damageFish;

	// Token: 0x040006A5 RID: 1701
	public int m_toolTier;

	// Token: 0x040006A6 RID: 1702
	public HitData.DamageTypes m_damages;

	// Token: 0x040006A7 RID: 1703
	public LayerMask m_triggerMask;

	// Token: 0x040006A8 RID: 1704
	public float m_interval = 0.5f;

	// Token: 0x040006A9 RID: 1705
	private bool m_firstHit = true;

	// Token: 0x040006AA RID: 1706
	private bool m_hitEffectEnabled = true;

	// Token: 0x040006AB RID: 1707
	private ZNetView m_nview;

	// Token: 0x040006AC RID: 1708
	private Rigidbody m_body;
}
