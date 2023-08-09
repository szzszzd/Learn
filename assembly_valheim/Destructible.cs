using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200022A RID: 554
public class Destructible : MonoBehaviour, IDestructible
{
	// Token: 0x060015DC RID: 5596 RVA: 0x0008F888 File Offset: 0x0008DA88
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
			if (this.m_autoCreateFragments)
			{
				this.m_nview.Register("CreateFragments", new Action<long>(this.RPC_CreateFragments));
			}
			if (this.m_ttl > 0f)
			{
				base.InvokeRepeating("DestroyNow", this.m_ttl, 1f);
			}
		}
	}

	// Token: 0x060015DD RID: 5597 RVA: 0x0008F92A File Offset: 0x0008DB2A
	private void Start()
	{
		this.m_firstFrame = false;
	}

	// Token: 0x060015DE RID: 5598 RVA: 0x00006475 File Offset: 0x00004675
	public GameObject GetParentObject()
	{
		return null;
	}

	// Token: 0x060015DF RID: 5599 RVA: 0x0008F933 File Offset: 0x0008DB33
	public DestructibleType GetDestructibleType()
	{
		return this.m_destructibleType;
	}

	// Token: 0x060015E0 RID: 5600 RVA: 0x0008F93B File Offset: 0x0008DB3B
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

	// Token: 0x060015E1 RID: 5601 RVA: 0x0008F970 File Offset: 0x0008DB70
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_destroyed)
		{
			return;
		}
		float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health);
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damages, out type);
		float totalDamage = hit.GetTotalDamage();
		if (this.m_body)
		{
			this.m_body.AddForceAtPosition(hit.m_dir * hit.m_pushForce, hit.m_point, ForceMode.Impulse);
		}
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
		if (this.m_triggerPrivateArea)
		{
			Character attacker = hit.GetAttacker();
			if (attacker)
			{
				bool destroyed = num <= 0f;
				PrivateArea.OnObjectDamaged(base.transform.position, attacker, destroyed);
			}
		}
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
		if (this.m_onDamaged != null)
		{
			this.m_onDamaged();
		}
		if (this.m_hitNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_hitNoise);
			}
		}
		if (num <= 0f)
		{
			this.Destroy(hit.m_point, hit.m_dir);
		}
	}

	// Token: 0x060015E2 RID: 5602 RVA: 0x0008FB1B File Offset: 0x0008DD1B
	public void DestroyNow()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			this.Destroy(Vector3.zero, Vector3.zero);
		}
	}

	// Token: 0x060015E3 RID: 5603 RVA: 0x0008FB48 File Offset: 0x0008DD48
	public void Destroy(Vector3 hitPoint, Vector3 hitDir)
	{
		this.CreateDestructionEffects(hitPoint, hitDir);
		if (this.m_destroyNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_destroyNoise);
			}
		}
		if (this.m_spawnWhenDestroyed)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_spawnWhenDestroyed, base.transform.position, base.transform.rotation);
			gameObject.GetComponent<ZNetView>().SetLocalScale(base.transform.localScale);
			Gibber component = gameObject.GetComponent<Gibber>();
			if (component)
			{
				component.Setup(hitPoint, hitDir);
			}
		}
		if (this.m_onDestroyed != null)
		{
			this.m_onDestroyed();
		}
		ZNetScene.instance.Destroy(base.gameObject);
		this.m_destroyed = true;
	}

	// Token: 0x060015E4 RID: 5604 RVA: 0x0008FC18 File Offset: 0x0008DE18
	private void CreateDestructionEffects(Vector3 hitPoint, Vector3 hitDir)
	{
		GameObject[] array = this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		for (int i = 0; i < array.Length; i++)
		{
			Gibber component = array[i].GetComponent<Gibber>();
			if (component)
			{
				component.Setup(hitPoint, hitDir);
			}
		}
		if (this.m_autoCreateFragments)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "CreateFragments", Array.Empty<object>());
		}
	}

	// Token: 0x060015E5 RID: 5605 RVA: 0x0008FC9C File Offset: 0x0008DE9C
	private void RPC_CreateFragments(long peer)
	{
		Destructible.CreateFragments(base.gameObject, true);
	}

	// Token: 0x060015E6 RID: 5606 RVA: 0x0008FCAC File Offset: 0x0008DEAC
	public static void CreateFragments(GameObject rootObject, bool visibleOnly = true)
	{
		MeshRenderer[] componentsInChildren = rootObject.GetComponentsInChildren<MeshRenderer>(true);
		int layer = LayerMask.NameToLayer("effect");
		List<Rigidbody> list = new List<Rigidbody>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.gameObject.activeInHierarchy && (!visibleOnly || meshRenderer.isVisible))
			{
				MeshFilter component = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if (!(component == null))
				{
					if (component.sharedMesh == null)
					{
						ZLog.Log("Meshfilter missing mesh " + component.gameObject.name);
					}
					else
					{
						GameObject gameObject = new GameObject();
						gameObject.layer = layer;
						gameObject.transform.position = component.gameObject.transform.position;
						gameObject.transform.rotation = component.gameObject.transform.rotation;
						gameObject.transform.localScale = component.gameObject.transform.lossyScale * 0.9f;
						gameObject.AddComponent<MeshFilter>().sharedMesh = component.sharedMesh;
						MeshRenderer meshRenderer2 = gameObject.AddComponent<MeshRenderer>();
						meshRenderer2.sharedMaterials = meshRenderer.sharedMaterials;
						meshRenderer2.material.SetFloat("_RippleDistance", 0f);
						meshRenderer2.material.SetFloat("_ValueNoise", 0f);
						Rigidbody item = gameObject.AddComponent<Rigidbody>();
						gameObject.AddComponent<BoxCollider>();
						list.Add(item);
						gameObject.AddComponent<TimedDestruction>().Trigger((float)UnityEngine.Random.Range(2, 4));
					}
				}
			}
		}
		if (list.Count > 0)
		{
			Vector3 vector = Vector3.zero;
			int num = 0;
			foreach (Rigidbody rigidbody in list)
			{
				vector += rigidbody.worldCenterOfMass;
				num++;
			}
			vector /= (float)num;
			foreach (Rigidbody rigidbody2 in list)
			{
				Vector3 vector2 = (rigidbody2.worldCenterOfMass - vector).normalized * 4f;
				vector2 += UnityEngine.Random.onUnitSphere * 1f;
				rigidbody2.AddForce(vector2, ForceMode.VelocityChange);
			}
		}
	}

	// Token: 0x040016CB RID: 5835
	public Action m_onDestroyed;

	// Token: 0x040016CC RID: 5836
	public Action m_onDamaged;

	// Token: 0x040016CD RID: 5837
	[Header("Destruction")]
	public DestructibleType m_destructibleType = DestructibleType.Default;

	// Token: 0x040016CE RID: 5838
	public float m_health = 1f;

	// Token: 0x040016CF RID: 5839
	public HitData.DamageModifiers m_damages;

	// Token: 0x040016D0 RID: 5840
	public float m_minDamageTreshold;

	// Token: 0x040016D1 RID: 5841
	public int m_minToolTier;

	// Token: 0x040016D2 RID: 5842
	public float m_hitNoise;

	// Token: 0x040016D3 RID: 5843
	public float m_destroyNoise;

	// Token: 0x040016D4 RID: 5844
	public bool m_triggerPrivateArea;

	// Token: 0x040016D5 RID: 5845
	public float m_ttl;

	// Token: 0x040016D6 RID: 5846
	public GameObject m_spawnWhenDestroyed;

	// Token: 0x040016D7 RID: 5847
	[Header("Effects")]
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x040016D8 RID: 5848
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x040016D9 RID: 5849
	public bool m_autoCreateFragments;

	// Token: 0x040016DA RID: 5850
	private ZNetView m_nview;

	// Token: 0x040016DB RID: 5851
	private Rigidbody m_body;

	// Token: 0x040016DC RID: 5852
	private bool m_firstFrame = true;

	// Token: 0x040016DD RID: 5853
	private bool m_destroyed;
}
