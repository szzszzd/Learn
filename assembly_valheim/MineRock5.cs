using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000269 RID: 617
public class MineRock5 : MonoBehaviour, IDestructible, Hoverable
{
	// Token: 0x060017C5 RID: 6085 RVA: 0x0009E1FC File Offset: 0x0009C3FC
	private void Start()
	{
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>();
		this.m_hitAreas = new List<MineRock5.HitArea>(componentsInChildren.Length);
		this.m_extraRenderers = new List<Renderer>();
		foreach (Collider collider in componentsInChildren)
		{
			MineRock5.HitArea hitArea = new MineRock5.HitArea();
			hitArea.m_collider = collider;
			hitArea.m_meshFilter = collider.GetComponent<MeshFilter>();
			hitArea.m_meshRenderer = collider.GetComponent<MeshRenderer>();
			collider.GetComponent<StaticPhysics>();
			hitArea.m_health = this.m_health;
			for (int j = 0; j < collider.transform.childCount; j++)
			{
				Renderer[] componentsInChildren2 = collider.transform.GetChild(j).GetComponentsInChildren<Renderer>();
				this.m_extraRenderers.AddRange(componentsInChildren2);
			}
			this.m_hitAreas.Add(hitArea);
		}
		if (MineRock5.m_rayMask == 0)
		{
			MineRock5.m_rayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"Default",
				"static_solid",
				"Default_small",
				"terrain"
			});
		}
		if (MineRock5.m_groundLayer == 0)
		{
			MineRock5.m_groundLayer = LayerMask.NameToLayer("terrain");
		}
		Material[] array = null;
		foreach (MineRock5.HitArea hitArea2 in this.m_hitAreas)
		{
			if (array == null || hitArea2.m_meshRenderer.sharedMaterials.Length > array.Length)
			{
				array = hitArea2.m_meshRenderer.sharedMaterials;
			}
		}
		this.m_meshFilter = base.gameObject.AddComponent<MeshFilter>();
		this.m_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		this.m_meshRenderer.sharedMaterials = array;
		this.m_meshFilter.mesh = new Mesh();
		this.m_meshFilter.name = "___MineRock5 m_meshFilter";
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.Register<HitData, int>("Damage", new Action<long, HitData, int>(this.RPC_Damage));
			this.m_nview.Register<int, float>("SetAreaHealth", new Action<long, int, float>(this.RPC_SetAreaHealth));
		}
		this.CheckForUpdate();
		base.InvokeRepeating("CheckForUpdate", UnityEngine.Random.Range(5f, 10f), 10f);
	}

	// Token: 0x060017C6 RID: 6086 RVA: 0x0009E458 File Offset: 0x0009C658
	private void CheckSupport()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateSupport();
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			MineRock5.HitArea hitArea = this.m_hitAreas[i];
			if (hitArea.m_health > 0f && !hitArea.m_supported)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = this.m_health;
				hitData.m_point = hitArea.m_collider.bounds.center;
				hitData.m_toolTier = 100;
				this.DamageArea(i, hitData);
			}
		}
	}

	// Token: 0x060017C7 RID: 6087 RVA: 0x0009E4FF File Offset: 0x0009C6FF
	private void CheckForUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.GetZDO().DataRevision != this.m_lastDataRevision)
		{
			this.LoadHealth();
			this.UpdateMesh();
		}
	}

	// Token: 0x060017C8 RID: 6088 RVA: 0x0009E534 File Offset: 0x0009C734
	private void LoadHealth()
	{
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_health, "");
		if (@string.Length > 0)
		{
			ZPackage zpackage = new ZPackage(Convert.FromBase64String(@string));
			int num = zpackage.ReadInt();
			for (int i = 0; i < num; i++)
			{
				float health = zpackage.ReadSingle();
				MineRock5.HitArea hitArea = this.GetHitArea(i);
				if (hitArea != null)
				{
					hitArea.m_health = health;
				}
			}
		}
		this.m_lastDataRevision = this.m_nview.GetZDO().DataRevision;
	}

	// Token: 0x060017C9 RID: 6089 RVA: 0x0009E5B8 File Offset: 0x0009C7B8
	private void SaveHealth()
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(this.m_hitAreas.Count);
		foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
		{
			zpackage.Write(hitArea.m_health);
		}
		string value = Convert.ToBase64String(zpackage.GetArray());
		this.m_nview.GetZDO().Set(ZDOVars.s_health, value);
		this.m_lastDataRevision = this.m_nview.GetZDO().DataRevision;
	}

	// Token: 0x060017CA RID: 6090 RVA: 0x0009E660 File Offset: 0x0009C860
	private void UpdateMesh()
	{
		MineRock5.m_tempInstancesA.Clear();
		MineRock5.m_tempInstancesB.Clear();
		Material y = this.m_meshRenderer.sharedMaterials[0];
		Matrix4x4 inverse = base.transform.localToWorldMatrix.inverse;
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			MineRock5.HitArea hitArea = this.m_hitAreas[i];
			if (hitArea.m_health > 0f)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = hitArea.m_meshFilter.sharedMesh;
				item.transform = inverse * hitArea.m_meshFilter.transform.localToWorldMatrix;
				for (int j = 0; j < hitArea.m_meshFilter.sharedMesh.subMeshCount; j++)
				{
					item.subMeshIndex = j;
					if (hitArea.m_meshRenderer.sharedMaterials[j] == y)
					{
						MineRock5.m_tempInstancesA.Add(item);
					}
					else
					{
						MineRock5.m_tempInstancesB.Add(item);
					}
				}
				hitArea.m_meshRenderer.enabled = false;
				hitArea.m_collider.gameObject.SetActive(true);
			}
			else
			{
				hitArea.m_collider.gameObject.SetActive(false);
			}
		}
		if (MineRock5.m_tempMeshA == null)
		{
			MineRock5.m_tempMeshA = new Mesh();
			MineRock5.m_tempMeshB = new Mesh();
			MineRock5.m_tempMeshA.name = "___MineRock5 m_tempMeshA";
			MineRock5.m_tempMeshB.name = "___MineRock5 m_tempMeshB";
		}
		MineRock5.m_tempMeshA.CombineMeshes(MineRock5.m_tempInstancesA.ToArray());
		MineRock5.m_tempMeshB.CombineMeshes(MineRock5.m_tempInstancesB.ToArray());
		CombineInstance combineInstance = default(CombineInstance);
		combineInstance.mesh = MineRock5.m_tempMeshA;
		CombineInstance combineInstance2 = default(CombineInstance);
		combineInstance2.mesh = MineRock5.m_tempMeshB;
		this.m_meshFilter.mesh.CombineMeshes(new CombineInstance[]
		{
			combineInstance,
			combineInstance2
		}, false, false);
		this.m_meshRenderer.enabled = true;
		Renderer[] array = new Renderer[this.m_extraRenderers.Count + 1];
		this.m_extraRenderers.CopyTo(0, array, 0, this.m_extraRenderers.Count);
		array[array.Length - 1] = this.m_meshRenderer;
		LODGroup component = base.gameObject.GetComponent<LODGroup>();
		LOD[] lods = component.GetLODs();
		lods[0].renderers = array;
		component.SetLODs(lods);
	}

	// Token: 0x060017CB RID: 6091 RVA: 0x0009E8D3 File Offset: 0x0009CAD3
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x060017CC RID: 6092 RVA: 0x0009E8E5 File Offset: 0x0009CAE5
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060017CD RID: 6093 RVA: 0x0000290F File Offset: 0x00000B0F
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x060017CE RID: 6094 RVA: 0x0009E8F0 File Offset: 0x0009CAF0
	public void Damage(HitData hit)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_hitAreas == null)
		{
			return;
		}
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
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit,
			areaIndex
		});
	}

	// Token: 0x060017CF RID: 6095 RVA: 0x0009E98C File Offset: 0x0009CB8C
	private void RPC_Damage(long sender, HitData hit, int hitAreaIndex)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		bool flag = this.DamageArea(hitAreaIndex, hit);
		if (flag && this.m_supportCheck)
		{
			this.CheckSupport();
		}
		if (this.m_triggerPrivateArea)
		{
			Character attacker = hit.GetAttacker();
			if (attacker != null)
			{
				PrivateArea.OnObjectDamaged(base.transform.position, attacker, flag);
			}
		}
	}

	// Token: 0x060017D0 RID: 6096 RVA: 0x0009E9F4 File Offset: 0x0009CBF4
	private bool DamageArea(int hitAreaIndex, HitData hit)
	{
		ZLog.Log("hit mine rock " + hitAreaIndex.ToString());
		MineRock5.HitArea hitArea = this.GetHitArea(hitAreaIndex);
		if (hitArea == null)
		{
			ZLog.Log("Missing hit area " + hitAreaIndex.ToString());
			return false;
		}
		this.LoadHealth();
		if (hitArea.m_health <= 0f)
		{
			ZLog.Log("Already destroyed");
			return false;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damageModifiers, out type);
		float totalDamage = hit.GetTotalDamage();
		if ((int)hit.m_toolTier < this.m_minToolTier)
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f, false);
			return false;
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return false;
		}
		hitArea.m_health -= totalDamage;
		this.SaveHealth();
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
		Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
		if (closestPlayer)
		{
			closestPlayer.AddNoise(100f);
		}
		if (hitArea.m_health <= 0f)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "SetAreaHealth", new object[]
			{
				hitAreaIndex,
				hitArea.m_health
			});
			this.m_destroyedEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
			foreach (GameObject original in this.m_dropItems.GetDropList())
			{
				Vector3 position = hit.m_point + UnityEngine.Random.insideUnitSphere * 0.3f;
				UnityEngine.Object.Instantiate<GameObject>(original, position, Quaternion.identity);
			}
			if (this.AllDestroyed())
			{
				this.m_nview.Destroy();
			}
			return true;
		}
		return false;
	}

	// Token: 0x060017D1 RID: 6097 RVA: 0x0009EBEC File Offset: 0x0009CDEC
	private bool AllDestroyed()
	{
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			if (this.m_hitAreas[i].m_health > 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060017D2 RID: 6098 RVA: 0x0009EC2C File Offset: 0x0009CE2C
	private bool NonDestroyed()
	{
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			if (this.m_hitAreas[i].m_health <= 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060017D3 RID: 6099 RVA: 0x0009EC6C File Offset: 0x0009CE6C
	private void RPC_SetAreaHealth(long sender, int index, float health)
	{
		MineRock5.HitArea hitArea = this.GetHitArea(index);
		if (hitArea != null)
		{
			hitArea.m_health = health;
		}
		this.UpdateMesh();
	}

	// Token: 0x060017D4 RID: 6100 RVA: 0x0009EC94 File Offset: 0x0009CE94
	private int GetAreaIndex(Collider area)
	{
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			if (this.m_hitAreas[i].m_collider == area)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060017D5 RID: 6101 RVA: 0x0009ECD3 File Offset: 0x0009CED3
	private MineRock5.HitArea GetHitArea(int index)
	{
		if (index < 0 || index >= this.m_hitAreas.Count)
		{
			return null;
		}
		return this.m_hitAreas[index];
	}

	// Token: 0x060017D6 RID: 6102 RVA: 0x0009ECF8 File Offset: 0x0009CEF8
	private void UpdateSupport()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!this.m_haveSetupBounds)
		{
			this.SetupColliders();
			this.m_haveSetupBounds = true;
		}
		foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
		{
			hitArea.m_supported = false;
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < 3; i++)
		{
			foreach (MineRock5.HitArea hitArea2 in this.m_hitAreas)
			{
				if (!hitArea2.m_supported)
				{
					int num = Physics.OverlapBoxNonAlloc(position + hitArea2.m_bound.m_pos, hitArea2.m_bound.m_size, MineRock5.m_tempColliders, hitArea2.m_bound.m_rot, MineRock5.m_rayMask);
					for (int j = 0; j < num; j++)
					{
						Collider collider = MineRock5.m_tempColliders[j];
						if (!(collider == hitArea2.m_collider) && !(collider.attachedRigidbody != null) && !collider.isTrigger)
						{
							hitArea2.m_supported = (hitArea2.m_supported || this.GetSupport(collider));
							if (hitArea2.m_supported)
							{
								break;
							}
						}
					}
				}
			}
		}
		ZLog.Log("Suport time " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
	}

	// Token: 0x060017D7 RID: 6103 RVA: 0x0009EE98 File Offset: 0x0009D098
	private bool GetSupport(Collider c)
	{
		if (c.gameObject.layer == MineRock5.m_groundLayer)
		{
			return true;
		}
		IDestructible componentInParent = c.gameObject.GetComponentInParent<IDestructible>();
		if (componentInParent != null)
		{
			if (componentInParent == this)
			{
				foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
				{
					if (hitArea.m_collider == c)
					{
						return hitArea.m_supported;
					}
				}
			}
			return c.transform.position.y < base.transform.position.y;
		}
		return true;
	}

	// Token: 0x060017D8 RID: 6104 RVA: 0x0009EF48 File Offset: 0x0009D148
	private void SetupColliders()
	{
		Vector3 position = base.transform.position;
		foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
		{
			hitArea.m_bound.m_rot = Quaternion.identity;
			hitArea.m_bound.m_pos = hitArea.m_collider.bounds.center - position;
			hitArea.m_bound.m_size = hitArea.m_collider.bounds.size * 0.5f;
		}
	}

	// Token: 0x0400193D RID: 6461
	private static Mesh m_tempMeshA;

	// Token: 0x0400193E RID: 6462
	private static Mesh m_tempMeshB;

	// Token: 0x0400193F RID: 6463
	private static List<CombineInstance> m_tempInstancesA = new List<CombineInstance>();

	// Token: 0x04001940 RID: 6464
	private static List<CombineInstance> m_tempInstancesB = new List<CombineInstance>();

	// Token: 0x04001941 RID: 6465
	public string m_name = "";

	// Token: 0x04001942 RID: 6466
	public float m_health = 2f;

	// Token: 0x04001943 RID: 6467
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x04001944 RID: 6468
	public int m_minToolTier;

	// Token: 0x04001945 RID: 6469
	public bool m_supportCheck = true;

	// Token: 0x04001946 RID: 6470
	public bool m_triggerPrivateArea;

	// Token: 0x04001947 RID: 6471
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04001948 RID: 6472
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04001949 RID: 6473
	public DropTable m_dropItems;

	// Token: 0x0400194A RID: 6474
	private List<MineRock5.HitArea> m_hitAreas;

	// Token: 0x0400194B RID: 6475
	private List<Renderer> m_extraRenderers;

	// Token: 0x0400194C RID: 6476
	private bool m_haveSetupBounds;

	// Token: 0x0400194D RID: 6477
	private ZNetView m_nview;

	// Token: 0x0400194E RID: 6478
	private MeshFilter m_meshFilter;

	// Token: 0x0400194F RID: 6479
	private MeshRenderer m_meshRenderer;

	// Token: 0x04001950 RID: 6480
	private uint m_lastDataRevision = uint.MaxValue;

	// Token: 0x04001951 RID: 6481
	private const int m_supportIterations = 3;

	// Token: 0x04001952 RID: 6482
	private static int m_rayMask = 0;

	// Token: 0x04001953 RID: 6483
	private static int m_groundLayer = 0;

	// Token: 0x04001954 RID: 6484
	private static Collider[] m_tempColliders = new Collider[128];

	// Token: 0x0200026A RID: 618
	private struct BoundData
	{
		// Token: 0x04001955 RID: 6485
		public Vector3 m_pos;

		// Token: 0x04001956 RID: 6486
		public Quaternion m_rot;

		// Token: 0x04001957 RID: 6487
		public Vector3 m_size;
	}

	// Token: 0x0200026B RID: 619
	private class HitArea
	{
		// Token: 0x04001958 RID: 6488
		public Collider m_collider;

		// Token: 0x04001959 RID: 6489
		public MeshRenderer m_meshRenderer;

		// Token: 0x0400195A RID: 6490
		public MeshFilter m_meshFilter;

		// Token: 0x0400195B RID: 6491
		public float m_health;

		// Token: 0x0400195C RID: 6492
		public MineRock5.BoundData m_bound;

		// Token: 0x0400195D RID: 6493
		public bool m_supported;
	}
}
