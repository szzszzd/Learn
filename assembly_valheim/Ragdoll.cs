using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002A RID: 42
public class Ragdoll : MonoBehaviour
{
	// Token: 0x060002E6 RID: 742 RVA: 0x00016AA4 File Offset: 0x00014CA4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_bodies = base.GetComponentsInChildren<Rigidbody>();
		base.Invoke("RemoveInitVel", 2f);
		if (this.m_mainModel)
		{
			float @float = this.m_nview.GetZDO().GetFloat(ZDOVars.s_hue, 0f);
			float float2 = this.m_nview.GetZDO().GetFloat(ZDOVars.s_saturation, 0f);
			float float3 = this.m_nview.GetZDO().GetFloat(ZDOVars.s_value, 0f);
			this.m_mainModel.material.SetFloat("_Hue", @float);
			this.m_mainModel.material.SetFloat("_Saturation", float2);
			this.m_mainModel.material.SetFloat("_Value", float3);
		}
		base.InvokeRepeating("DestroyNow", this.m_ttl, 1f);
	}

	// Token: 0x060002E7 RID: 743 RVA: 0x00016B94 File Offset: 0x00014D94
	public Vector3 GetAverageBodyPosition()
	{
		if (this.m_bodies.Length == 0)
		{
			return base.transform.position;
		}
		Vector3 a = Vector3.zero;
		foreach (Rigidbody rigidbody in this.m_bodies)
		{
			a += rigidbody.position;
		}
		return a / (float)this.m_bodies.Length;
	}

	// Token: 0x060002E8 RID: 744 RVA: 0x00016BF4 File Offset: 0x00014DF4
	private void DestroyNow()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		Vector3 averageBodyPosition = this.GetAverageBodyPosition();
		this.m_removeEffect.Create(averageBodyPosition, Quaternion.identity, null, 1f, -1);
		this.SpawnLoot(averageBodyPosition);
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x060002E9 RID: 745 RVA: 0x00016C53 File Offset: 0x00014E53
	private void RemoveInitVel()
	{
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_initVel, Vector3.zero);
		}
	}

	// Token: 0x060002EA RID: 746 RVA: 0x00016C7C File Offset: 0x00014E7C
	private void Start()
	{
		Vector3 vec = this.m_nview.GetZDO().GetVec3(ZDOVars.s_initVel, Vector3.zero);
		if (vec != Vector3.zero)
		{
			vec.y = Mathf.Min(vec.y, 4f);
			Rigidbody[] bodies = this.m_bodies;
			for (int i = 0; i < bodies.Length; i++)
			{
				bodies[i].velocity = vec * UnityEngine.Random.value;
			}
		}
	}

	// Token: 0x060002EB RID: 747 RVA: 0x00016CF0 File Offset: 0x00014EF0
	public void Setup(Vector3 velocity, float hue, float saturation, float value, CharacterDrop characterDrop)
	{
		velocity.x *= this.m_velMultiplier;
		velocity.z *= this.m_velMultiplier;
		this.m_nview.GetZDO().Set(ZDOVars.s_initVel, velocity);
		this.m_nview.GetZDO().Set(ZDOVars.s_hue, hue);
		this.m_nview.GetZDO().Set(ZDOVars.s_saturation, saturation);
		this.m_nview.GetZDO().Set(ZDOVars.s_value, value);
		if (this.m_mainModel)
		{
			this.m_mainModel.material.SetFloat("_Hue", hue);
			this.m_mainModel.material.SetFloat("_Saturation", saturation);
			this.m_mainModel.material.SetFloat("_Value", value);
		}
		if (characterDrop)
		{
			this.SaveLootList(characterDrop);
		}
	}

	// Token: 0x060002EC RID: 748 RVA: 0x00016DDC File Offset: 0x00014FDC
	private void SaveLootList(CharacterDrop characterDrop)
	{
		List<KeyValuePair<GameObject, int>> list = characterDrop.GenerateDropList();
		if (list.Count > 0)
		{
			ZDO zdo = this.m_nview.GetZDO();
			zdo.Set(ZDOVars.s_drops, list.Count, false);
			for (int i = 0; i < list.Count; i++)
			{
				KeyValuePair<GameObject, int> keyValuePair = list[i];
				int prefabHash = ZNetScene.instance.GetPrefabHash(keyValuePair.Key);
				zdo.Set("drop_hash" + i.ToString(), prefabHash);
				zdo.Set("drop_amount" + i.ToString(), keyValuePair.Value);
			}
		}
	}

	// Token: 0x060002ED RID: 749 RVA: 0x00016E80 File Offset: 0x00015080
	private void SpawnLoot(Vector3 center)
	{
		ZDO zdo = this.m_nview.GetZDO();
		int @int = zdo.GetInt(ZDOVars.s_drops, 0);
		if (@int <= 0)
		{
			return;
		}
		List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
		for (int i = 0; i < @int; i++)
		{
			int int2 = zdo.GetInt("drop_hash" + i.ToString(), 0);
			int int3 = zdo.GetInt("drop_amount" + i.ToString(), 0);
			GameObject prefab = ZNetScene.instance.GetPrefab(int2);
			if (prefab == null)
			{
				ZLog.LogWarning("Ragdoll: Missing prefab:" + int2.ToString() + " when dropping loot");
			}
			else
			{
				list.Add(new KeyValuePair<GameObject, int>(prefab, int3));
			}
		}
		CharacterDrop.DropItems(list, center + Vector3.up * 0.75f, 0.5f);
	}

	// Token: 0x060002EE RID: 750 RVA: 0x00016F55 File Offset: 0x00015155
	private void FixedUpdate()
	{
		if (this.m_float)
		{
			this.UpdateFloating(Time.fixedDeltaTime);
		}
	}

	// Token: 0x060002EF RID: 751 RVA: 0x00016F6C File Offset: 0x0001516C
	private void UpdateFloating(float dt)
	{
		foreach (Rigidbody rigidbody in this.m_bodies)
		{
			Vector3 worldCenterOfMass = rigidbody.worldCenterOfMass;
			worldCenterOfMass.y += this.m_floatOffset;
			float liquidLevel = Floating.GetLiquidLevel(worldCenterOfMass, 1f, LiquidType.All);
			if (worldCenterOfMass.y < liquidLevel)
			{
				float d = (liquidLevel - worldCenterOfMass.y) / 0.5f;
				Vector3 a = Vector3.up * 20f * d;
				rigidbody.AddForce(a * dt, ForceMode.VelocityChange);
				rigidbody.velocity -= rigidbody.velocity * 0.05f * d;
			}
		}
	}

	// Token: 0x040002B5 RID: 693
	public float m_velMultiplier = 1f;

	// Token: 0x040002B6 RID: 694
	public float m_ttl;

	// Token: 0x040002B7 RID: 695
	public Renderer m_mainModel;

	// Token: 0x040002B8 RID: 696
	public EffectList m_removeEffect = new EffectList();

	// Token: 0x040002B9 RID: 697
	public Action<Vector3> m_onDestroyed;

	// Token: 0x040002BA RID: 698
	public bool m_float;

	// Token: 0x040002BB RID: 699
	public float m_floatOffset = -0.1f;

	// Token: 0x040002BC RID: 700
	private const float m_floatForce = 20f;

	// Token: 0x040002BD RID: 701
	private const float m_damping = 0.05f;

	// Token: 0x040002BE RID: 702
	private ZNetView m_nview;

	// Token: 0x040002BF RID: 703
	private Rigidbody[] m_bodies;

	// Token: 0x040002C0 RID: 704
	private const float m_dropOffset = 0.75f;

	// Token: 0x040002C1 RID: 705
	private const float m_dropArea = 0.5f;
}
