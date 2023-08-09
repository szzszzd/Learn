using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x020002C0 RID: 704
public class WearNTear : MonoBehaviour, IDestructible
{
	// Token: 0x06001AA9 RID: 6825 RVA: 0x000B101C File Offset: 0x000AF21C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<HitData>("WNTDamage", new Action<long, HitData>(this.RPC_Damage));
		this.m_nview.Register("WNTRemove", new Action<long>(this.RPC_Remove));
		this.m_nview.Register("WNTRepair", new Action<long>(this.RPC_Repair));
		this.m_nview.Register<float>("WNTHealthChanged", new Action<long, float>(this.RPC_HealthChanged));
		if (this.m_autoCreateFragments)
		{
			this.m_nview.Register("WNTCreateFragments", new Action<long>(this.RPC_CreateFragments));
		}
		if (WearNTear.s_rayMask == 0)
		{
			WearNTear.s_rayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"Default",
				"static_solid",
				"Default_small",
				"terrain"
			});
		}
		WearNTear.s_allInstances.Add(this);
		this.m_myIndex = WearNTear.s_allInstances.Count - 1;
		this.m_createTime = Time.time;
		this.m_support = this.GetMaxSupport();
		if (WearNTear.m_randomInitialDamage)
		{
			float value = UnityEngine.Random.Range(0.1f * this.m_health, this.m_health * 0.6f);
			this.m_nview.GetZDO().Set(ZDOVars.s_health, value);
		}
		this.UpdateCover(5f);
		this.m_updateCoverTimer = UnityEngine.Random.Range(0f, 4f);
		this.UpdateVisual(false);
	}

	// Token: 0x06001AAA RID: 6826 RVA: 0x000B11B8 File Offset: 0x000AF3B8
	private void OnDestroy()
	{
		if (this.m_myIndex != -1)
		{
			WearNTear.s_allInstances[this.m_myIndex] = WearNTear.s_allInstances[WearNTear.s_allInstances.Count - 1];
			WearNTear.s_allInstances[this.m_myIndex].m_myIndex = this.m_myIndex;
			WearNTear.s_allInstances.RemoveAt(WearNTear.s_allInstances.Count - 1);
		}
	}

	// Token: 0x06001AAB RID: 6827 RVA: 0x000B1228 File Offset: 0x000AF428
	public bool Repair()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health) >= this.m_health)
		{
			return false;
		}
		if (Time.time - this.m_lastRepair < 1f)
		{
			return false;
		}
		this.m_lastRepair = Time.time;
		this.m_nview.InvokeRPC("WNTRepair", Array.Empty<object>());
		return true;
	}

	// Token: 0x06001AAC RID: 6828 RVA: 0x000B12A0 File Offset: 0x000AF4A0
	private void RPC_Repair(long sender)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_health, this.m_health);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", new object[]
		{
			this.m_health
		});
	}

	// Token: 0x06001AAD RID: 6829 RVA: 0x000B130C File Offset: 0x000AF50C
	private float GetSupport()
	{
		if (!this.m_nview.IsValid())
		{
			return this.GetMaxSupport();
		}
		if (!this.m_nview.HasOwner())
		{
			return this.GetMaxSupport();
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_support;
		}
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_support, this.GetMaxSupport());
	}

	// Token: 0x06001AAE RID: 6830 RVA: 0x000B1370 File Offset: 0x000AF570
	private float GetSupportColorValue()
	{
		float num = this.GetSupport();
		float num2;
		float num3;
		float num4;
		float num5;
		this.GetMaterialProperties(out num2, out num3, out num4, out num5);
		if (num >= num2)
		{
			return -1f;
		}
		num -= num3;
		return Mathf.Clamp01(num / (num2 * 0.5f - num3));
	}

	// Token: 0x06001AAF RID: 6831 RVA: 0x000B13B0 File Offset: 0x000AF5B0
	public void OnPlaced()
	{
		this.m_createTime = -1f;
	}

	// Token: 0x06001AB0 RID: 6832 RVA: 0x000B13C0 File Offset: 0x000AF5C0
	private List<Renderer> GetHighlightRenderers()
	{
		MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>(true);
		SkinnedMeshRenderer[] componentsInChildren2 = base.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		List<Renderer> list = new List<Renderer>();
		list.AddRange(componentsInChildren);
		list.AddRange(componentsInChildren2);
		return list;
	}

	// Token: 0x06001AB1 RID: 6833 RVA: 0x000B13F0 File Offset: 0x000AF5F0
	public void Highlight()
	{
		if (this.m_oldMaterials == null)
		{
			this.m_oldMaterials = new List<WearNTear.OldMeshData>();
			foreach (Renderer renderer in this.GetHighlightRenderers())
			{
				WearNTear.OldMeshData oldMeshData = default(WearNTear.OldMeshData);
				oldMeshData.m_materials = renderer.sharedMaterials;
				oldMeshData.m_color = new Color[oldMeshData.m_materials.Length];
				oldMeshData.m_emissiveColor = new Color[oldMeshData.m_materials.Length];
				for (int i = 0; i < oldMeshData.m_materials.Length; i++)
				{
					if (oldMeshData.m_materials[i].HasProperty("_Color"))
					{
						oldMeshData.m_color[i] = oldMeshData.m_materials[i].GetColor("_Color");
					}
					if (oldMeshData.m_materials[i].HasProperty("_EmissionColor"))
					{
						oldMeshData.m_emissiveColor[i] = oldMeshData.m_materials[i].GetColor("_EmissionColor");
					}
				}
				oldMeshData.m_renderer = renderer;
				this.m_oldMaterials.Add(oldMeshData);
			}
		}
		float supportColorValue = this.GetSupportColorValue();
		Color color = new Color(0.6f, 0.8f, 1f);
		if (supportColorValue >= 0f)
		{
			color = Color.Lerp(new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), supportColorValue);
			float h;
			float s;
			float v;
			Color.RGBToHSV(color, out h, out s, out v);
			s = Mathf.Lerp(1f, 0.5f, supportColorValue);
			v = Mathf.Lerp(1.2f, 0.9f, supportColorValue);
			color = Color.HSVToRGB(h, s, v);
		}
		foreach (WearNTear.OldMeshData oldMeshData2 in this.m_oldMaterials)
		{
			if (oldMeshData2.m_renderer)
			{
				foreach (Material material in oldMeshData2.m_renderer.materials)
				{
					material.SetColor("_EmissionColor", color * 0.4f);
					material.color = color;
				}
			}
		}
		base.CancelInvoke("ResetHighlight");
		base.Invoke("ResetHighlight", 0.2f);
	}

	// Token: 0x06001AB2 RID: 6834 RVA: 0x000B1670 File Offset: 0x000AF870
	private void ResetHighlight()
	{
		if (this.m_oldMaterials != null)
		{
			foreach (WearNTear.OldMeshData oldMeshData in this.m_oldMaterials)
			{
				if (oldMeshData.m_renderer)
				{
					Material[] materials = oldMeshData.m_renderer.materials;
					if (materials.Length != 0)
					{
						if (materials[0] == oldMeshData.m_materials[0])
						{
							if (materials.Length == oldMeshData.m_color.Length)
							{
								for (int i = 0; i < materials.Length; i++)
								{
									if (materials[i].HasProperty("_Color"))
									{
										materials[i].SetColor("_Color", oldMeshData.m_color[i]);
									}
									if (materials[i].HasProperty("_EmissionColor"))
									{
										materials[i].SetColor("_EmissionColor", oldMeshData.m_emissiveColor[i]);
									}
								}
							}
						}
						else if (materials.Length == oldMeshData.m_materials.Length)
						{
							oldMeshData.m_renderer.materials = oldMeshData.m_materials;
						}
					}
				}
			}
			this.m_oldMaterials = null;
		}
	}

	// Token: 0x06001AB3 RID: 6835 RVA: 0x000B1794 File Offset: 0x000AF994
	private void SetupColliders()
	{
		this.m_colliders = base.GetComponentsInChildren<Collider>(true);
		this.m_bounds = new List<WearNTear.BoundData>();
		foreach (Collider collider in this.m_colliders)
		{
			if (!collider.isTrigger && !(collider.attachedRigidbody != null))
			{
				WearNTear.BoundData item = default(WearNTear.BoundData);
				if (collider is BoxCollider)
				{
					BoxCollider boxCollider = collider as BoxCollider;
					item.m_rot = boxCollider.transform.rotation;
					item.m_pos = boxCollider.transform.position + boxCollider.transform.TransformVector(boxCollider.center);
					item.m_size = new Vector3(boxCollider.transform.lossyScale.x * boxCollider.size.x, boxCollider.transform.lossyScale.y * boxCollider.size.y, boxCollider.transform.lossyScale.z * boxCollider.size.z);
				}
				else
				{
					item.m_rot = Quaternion.identity;
					item.m_pos = collider.bounds.center;
					item.m_size = collider.bounds.size;
				}
				item.m_size.x = item.m_size.x + 0.3f;
				item.m_size.y = item.m_size.y + 0.3f;
				item.m_size.z = item.m_size.z + 0.3f;
				item.m_size *= 0.5f;
				this.m_bounds.Add(item);
			}
		}
	}

	// Token: 0x06001AB4 RID: 6836 RVA: 0x000B1954 File Offset: 0x000AFB54
	private bool ShouldUpdate()
	{
		return this.m_createTime < 0f || Time.time - this.m_createTime > 30f;
	}

	// Token: 0x06001AB5 RID: 6837 RVA: 0x000B1978 File Offset: 0x000AFB78
	public void UpdateWear(float time)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.ShouldUpdate())
		{
			if (ZNetScene.instance.OutsideActiveArea(base.transform.position))
			{
				this.m_support = this.GetMaxSupport();
				this.m_nview.GetZDO().Set(ZDOVars.s_support, this.m_support);
				return;
			}
			float num = 0f;
			bool flag = !this.m_haveRoof && EnvMan.instance.IsWet();
			if (this.m_wet)
			{
				this.m_wet.SetActive(flag);
			}
			if (this.m_noRoofWear && this.GetHealthPercentage() > 0.5f)
			{
				if (flag || this.IsUnderWater())
				{
					if (this.m_rainTimer == 0f)
					{
						this.m_rainTimer = time;
					}
					else if (time - this.m_rainTimer > 60f)
					{
						this.m_rainTimer = time;
						num += 5f;
					}
				}
				else
				{
					this.m_rainTimer = 0f;
				}
			}
			if (this.m_noSupportWear)
			{
				this.UpdateSupport();
				if (!this.HaveSupport())
				{
					num = 100f;
				}
			}
			if (num > 0f && this.CanBeRemoved())
			{
				float damage = num / 100f * this.m_health;
				this.ApplyDamage(damage);
			}
		}
		this.UpdateVisual(true);
	}

	// Token: 0x06001AB6 RID: 6838 RVA: 0x000B1ACD File Offset: 0x000AFCCD
	private Vector3 GetCOM()
	{
		return base.transform.position + base.transform.rotation * this.m_comOffset;
	}

	// Token: 0x06001AB7 RID: 6839 RVA: 0x000B1AF8 File Offset: 0x000AFCF8
	private void UpdateSupport()
	{
		if (this.m_colliders == null)
		{
			this.SetupColliders();
		}
		float num;
		float num2;
		float num3;
		float num4;
		this.GetMaterialProperties(out num, out num2, out num3, out num4);
		WearNTear.s_tempSupportPoints.Clear();
		WearNTear.s_tempSupportPointValues.Clear();
		Vector3 com = this.GetCOM();
		float num5 = 0f;
		foreach (WearNTear.BoundData boundData in this.m_bounds)
		{
			int num6 = Physics.OverlapBoxNonAlloc(boundData.m_pos, boundData.m_size, WearNTear.s_tempColliders, boundData.m_rot, WearNTear.s_rayMask);
			for (int i = 0; i < num6; i++)
			{
				Collider collider = WearNTear.s_tempColliders[i];
				if (!this.m_colliders.Contains(collider) && !(collider.attachedRigidbody != null) && !collider.isTrigger)
				{
					WearNTear componentInParent = collider.GetComponentInParent<WearNTear>();
					if (componentInParent == null)
					{
						this.m_support = num;
						this.m_nview.GetZDO().Set(ZDOVars.s_support, this.m_support);
						return;
					}
					if (componentInParent.m_supports)
					{
						float num7 = Vector3.Distance(com, componentInParent.transform.position) + 0.1f;
						float support = componentInParent.GetSupport();
						num5 = Mathf.Max(num5, support - num3 * num7 * support);
						Vector3 vector = WearNTear.FindSupportPoint(com, componentInParent, collider);
						if (vector.y < com.y + 0.05f)
						{
							Vector3 normalized = (vector - com).normalized;
							if (normalized.y < 0f)
							{
								float t = Mathf.Acos(1f - Mathf.Abs(normalized.y)) / 1.5707964f;
								float num8 = Mathf.Lerp(num3, num4, t);
								float b = support - num8 * num7 * support;
								num5 = Mathf.Max(num5, b);
							}
							float item = support - num4 * num7 * support;
							WearNTear.s_tempSupportPoints.Add(vector);
							WearNTear.s_tempSupportPointValues.Add(item);
						}
					}
				}
			}
		}
		if (WearNTear.s_tempSupportPoints.Count > 0)
		{
			for (int j = 0; j < WearNTear.s_tempSupportPoints.Count - 1; j++)
			{
				Vector3 from = WearNTear.s_tempSupportPoints[j] - com;
				from.y = 0f;
				for (int k = j + 1; k < WearNTear.s_tempSupportPoints.Count; k++)
				{
					float num9 = (WearNTear.s_tempSupportPointValues[j] + WearNTear.s_tempSupportPointValues[k]) * 0.5f;
					if (num9 > num5)
					{
						Vector3 to = WearNTear.s_tempSupportPoints[k] - com;
						to.y = 0f;
						if (Vector3.Angle(from, to) >= 100f)
						{
							num5 = num9;
						}
					}
				}
			}
		}
		this.m_support = Mathf.Min(num5, num);
		this.m_nview.GetZDO().Set(ZDOVars.s_support, this.m_support);
	}

	// Token: 0x06001AB8 RID: 6840 RVA: 0x000B1E2C File Offset: 0x000B002C
	private static Vector3 FindSupportPoint(Vector3 com, WearNTear wnt, Collider otherCollider)
	{
		MeshCollider meshCollider = otherCollider as MeshCollider;
		if (!(meshCollider != null) || meshCollider.convex)
		{
			return otherCollider.ClosestPoint(com);
		}
		RaycastHit raycastHit;
		if (meshCollider.Raycast(new Ray(com, Vector3.down), out raycastHit, 10f))
		{
			return raycastHit.point;
		}
		return (com + wnt.GetCOM()) * 0.5f;
	}

	// Token: 0x06001AB9 RID: 6841 RVA: 0x000B1E91 File Offset: 0x000B0091
	private bool HaveSupport()
	{
		return this.m_support >= this.GetMinSupport();
	}

	// Token: 0x06001ABA RID: 6842 RVA: 0x000B1EA4 File Offset: 0x000B00A4
	private bool IsUnderWater()
	{
		return base.transform.position.y < Floating.GetLiquidLevel(base.transform.position, 1f, LiquidType.All);
	}

	// Token: 0x06001ABB RID: 6843 RVA: 0x000B1ED0 File Offset: 0x000B00D0
	public void UpdateCover(float dt)
	{
		this.m_updateCoverTimer += dt;
		if (this.m_updateCoverTimer <= 4f)
		{
			return;
		}
		if (EnvMan.instance.IsWet())
		{
			this.m_haveRoof = this.HaveRoof();
		}
		this.m_updateCoverTimer = 0f;
	}

	// Token: 0x06001ABC RID: 6844 RVA: 0x000B1F1C File Offset: 0x000B011C
	private bool HaveRoof()
	{
		if (this.m_roof)
		{
			return true;
		}
		int num = Physics.SphereCastNonAlloc(base.transform.position, 0.1f, Vector3.up, WearNTear.s_raycastHits, 100f, WearNTear.s_rayMask);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = WearNTear.s_raycastHits[i];
			if (!raycastHit.collider.gameObject.CompareTag("leaky"))
			{
				this.m_roof = raycastHit.collider.gameObject;
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001ABD RID: 6845 RVA: 0x000B1FA8 File Offset: 0x000B01A8
	private void RPC_HealthChanged(long peer, float health)
	{
		float health2 = health / this.m_health;
		this.SetHealthVisual(health2, true);
	}

	// Token: 0x06001ABE RID: 6846 RVA: 0x000B1FC6 File Offset: 0x000B01C6
	private void UpdateVisual(bool triggerEffects)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.SetHealthVisual(this.GetHealthPercentage(), triggerEffects);
	}

	// Token: 0x06001ABF RID: 6847 RVA: 0x000B1FE4 File Offset: 0x000B01E4
	private void SetHealthVisual(float health, bool triggerEffects)
	{
		if (this.m_worn == null && this.m_broken == null && this.m_new == null)
		{
			return;
		}
		if (health > 0.75f)
		{
			if (this.m_worn != this.m_new)
			{
				this.m_worn.SetActive(false);
			}
			if (this.m_broken != this.m_new)
			{
				this.m_broken.SetActive(false);
			}
			this.m_new.SetActive(true);
			return;
		}
		if (health > 0.25f)
		{
			if (triggerEffects && !this.m_worn.activeSelf)
			{
				this.m_switchEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
			}
			if (this.m_new != this.m_worn)
			{
				this.m_new.SetActive(false);
			}
			if (this.m_broken != this.m_worn)
			{
				this.m_broken.SetActive(false);
			}
			this.m_worn.SetActive(true);
			return;
		}
		if (triggerEffects && !this.m_broken.activeSelf)
		{
			this.m_switchEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		}
		if (this.m_new != this.m_broken)
		{
			this.m_new.SetActive(false);
		}
		if (this.m_worn != this.m_broken)
		{
			this.m_worn.SetActive(false);
		}
		this.m_broken.SetActive(true);
	}

	// Token: 0x06001AC0 RID: 6848 RVA: 0x000B218B File Offset: 0x000B038B
	public float GetHealthPercentage()
	{
		if (!this.m_nview.IsValid())
		{
			return 1f;
		}
		return Mathf.Clamp01(this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health) / this.m_health);
	}

	// Token: 0x06001AC1 RID: 6849 RVA: 0x0000290F File Offset: 0x00000B0F
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x06001AC2 RID: 6850 RVA: 0x000B21C7 File Offset: 0x000B03C7
	public void Damage(HitData hit)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("WNTDamage", new object[]
		{
			hit
		});
	}

	// Token: 0x06001AC3 RID: 6851 RVA: 0x000B21F1 File Offset: 0x000B03F1
	private bool CanBeRemoved()
	{
		return !this.m_piece || this.m_piece.CanBeRemoved();
	}

	// Token: 0x06001AC4 RID: 6852 RVA: 0x000B2210 File Offset: 0x000B0410
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health) <= 0f)
		{
			return;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damages, out type);
		float totalDamage = hit.GetTotalDamage();
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return;
		}
		if (this.m_triggerPrivateArea)
		{
			Character attacker = hit.GetAttacker();
			if (attacker)
			{
				bool destroyed = totalDamage >= this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health);
				PrivateArea.OnObjectDamaged(base.transform.position, attacker, destroyed);
			}
		}
		this.ApplyDamage(totalDamage);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f, -1);
		if (this.m_hitNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_hitNoise);
			}
		}
		if (this.m_onDamaged != null)
		{
			this.m_onDamaged();
		}
	}

	// Token: 0x06001AC5 RID: 6853 RVA: 0x000B234C File Offset: 0x000B054C
	public bool ApplyDamage(float damage)
	{
		float num = this.m_nview.GetZDO().GetFloat(ZDOVars.s_health, this.m_health);
		if (num <= 0f)
		{
			return false;
		}
		num -= damage;
		this.m_nview.GetZDO().Set(ZDOVars.s_health, num);
		if (num <= 0f)
		{
			this.Destroy();
		}
		else
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", new object[]
			{
				num
			});
		}
		return true;
	}

	// Token: 0x06001AC6 RID: 6854 RVA: 0x000B23CE File Offset: 0x000B05CE
	public void Remove()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("WNTRemove", Array.Empty<object>());
	}

	// Token: 0x06001AC7 RID: 6855 RVA: 0x000B23F3 File Offset: 0x000B05F3
	private void RPC_Remove(long sender)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.Destroy();
	}

	// Token: 0x06001AC8 RID: 6856 RVA: 0x000B2418 File Offset: 0x000B0618
	private void Destroy()
	{
		Bed component = base.GetComponent<Bed>();
		if (component != null && this.m_nview.IsOwner() && Game.instance != null)
		{
			Game.instance.RemoveCustomSpawnPoint(component.GetSpawnPoint());
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_health, 0f);
		if (this.m_piece)
		{
			this.m_piece.DropResources();
		}
		if (this.m_onDestroyed != null)
		{
			this.m_onDestroyed();
		}
		if (this.m_destroyNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_destroyNoise);
			}
		}
		this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		if (this.m_autoCreateFragments)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "WNTCreateFragments", Array.Empty<object>());
		}
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x06001AC9 RID: 6857 RVA: 0x000B253C File Offset: 0x000B073C
	private void RPC_CreateFragments(long peer)
	{
		this.ResetHighlight();
		if (this.m_fragmentRoots != null && this.m_fragmentRoots.Length != 0)
		{
			foreach (GameObject gameObject in this.m_fragmentRoots)
			{
				gameObject.SetActive(true);
				Destructible.CreateFragments(gameObject, false);
			}
			return;
		}
		Destructible.CreateFragments(base.gameObject, true);
	}

	// Token: 0x06001ACA RID: 6858 RVA: 0x000B2594 File Offset: 0x000B0794
	private float GetMaxSupport()
	{
		float result;
		float num;
		float num2;
		float num3;
		this.GetMaterialProperties(out result, out num, out num2, out num3);
		return result;
	}

	// Token: 0x06001ACB RID: 6859 RVA: 0x000B25B0 File Offset: 0x000B07B0
	private float GetMinSupport()
	{
		float num;
		float result;
		float num2;
		float num3;
		this.GetMaterialProperties(out num, out result, out num2, out num3);
		return result;
	}

	// Token: 0x06001ACC RID: 6860 RVA: 0x000B25CC File Offset: 0x000B07CC
	private void GetMaterialProperties(out float maxSupport, out float minSupport, out float horizontalLoss, out float verticalLoss)
	{
		switch (this.m_materialType)
		{
		case WearNTear.MaterialType.Wood:
			maxSupport = 100f;
			minSupport = 10f;
			verticalLoss = 0.125f;
			horizontalLoss = 0.2f;
			return;
		case WearNTear.MaterialType.Stone:
			maxSupport = 1000f;
			minSupport = 100f;
			verticalLoss = 0.125f;
			horizontalLoss = 1f;
			return;
		case WearNTear.MaterialType.Iron:
			maxSupport = 1500f;
			minSupport = 20f;
			verticalLoss = 0.07692308f;
			horizontalLoss = 0.07692308f;
			return;
		case WearNTear.MaterialType.HardWood:
			maxSupport = 140f;
			minSupport = 10f;
			verticalLoss = 0.1f;
			horizontalLoss = 0.16666667f;
			return;
		case WearNTear.MaterialType.Marble:
			maxSupport = 1500f;
			minSupport = 100f;
			verticalLoss = 0.125f;
			horizontalLoss = 0.5f;
			return;
		default:
			maxSupport = 0f;
			minSupport = 0f;
			verticalLoss = 0f;
			horizontalLoss = 0f;
			return;
		}
	}

	// Token: 0x06001ACD RID: 6861 RVA: 0x000B26B2 File Offset: 0x000B08B2
	public static List<WearNTear> GetAllInstances()
	{
		return WearNTear.s_allInstances;
	}

	// Token: 0x04001CBC RID: 7356
	public static bool m_randomInitialDamage = false;

	// Token: 0x04001CBD RID: 7357
	public Action m_onDestroyed;

	// Token: 0x04001CBE RID: 7358
	public Action m_onDamaged;

	// Token: 0x04001CBF RID: 7359
	[Header("Wear")]
	public GameObject m_new;

	// Token: 0x04001CC0 RID: 7360
	public GameObject m_worn;

	// Token: 0x04001CC1 RID: 7361
	public GameObject m_broken;

	// Token: 0x04001CC2 RID: 7362
	public GameObject m_wet;

	// Token: 0x04001CC3 RID: 7363
	public bool m_noRoofWear = true;

	// Token: 0x04001CC4 RID: 7364
	public bool m_noSupportWear = true;

	// Token: 0x04001CC5 RID: 7365
	public WearNTear.MaterialType m_materialType;

	// Token: 0x04001CC6 RID: 7366
	public bool m_supports = true;

	// Token: 0x04001CC7 RID: 7367
	public Vector3 m_comOffset = Vector3.zero;

	// Token: 0x04001CC8 RID: 7368
	[Header("Destruction")]
	public float m_health = 100f;

	// Token: 0x04001CC9 RID: 7369
	public HitData.DamageModifiers m_damages;

	// Token: 0x04001CCA RID: 7370
	public float m_hitNoise;

	// Token: 0x04001CCB RID: 7371
	public float m_destroyNoise;

	// Token: 0x04001CCC RID: 7372
	public bool m_triggerPrivateArea = true;

	// Token: 0x04001CCD RID: 7373
	[Header("Effects")]
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04001CCE RID: 7374
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04001CCF RID: 7375
	public EffectList m_switchEffect = new EffectList();

	// Token: 0x04001CD0 RID: 7376
	public bool m_autoCreateFragments = true;

	// Token: 0x04001CD1 RID: 7377
	public GameObject[] m_fragmentRoots;

	// Token: 0x04001CD2 RID: 7378
	private const float c_RainDamageTime = 60f;

	// Token: 0x04001CD3 RID: 7379
	private const float c_RainDamage = 5f;

	// Token: 0x04001CD4 RID: 7380
	private const float c_ComTestWidth = 0.2f;

	// Token: 0x04001CD5 RID: 7381
	private const float c_ComMinAngle = 100f;

	// Token: 0x04001CD6 RID: 7382
	private static readonly RaycastHit[] s_raycastHits = new RaycastHit[128];

	// Token: 0x04001CD7 RID: 7383
	private static readonly Collider[] s_tempColliders = new Collider[128];

	// Token: 0x04001CD8 RID: 7384
	private static int s_rayMask = 0;

	// Token: 0x04001CD9 RID: 7385
	private static readonly List<WearNTear> s_allInstances = new List<WearNTear>();

	// Token: 0x04001CDA RID: 7386
	private static readonly List<Vector3> s_tempSupportPoints = new List<Vector3>();

	// Token: 0x04001CDB RID: 7387
	private static readonly List<float> s_tempSupportPointValues = new List<float>();

	// Token: 0x04001CDC RID: 7388
	private ZNetView m_nview;

	// Token: 0x04001CDD RID: 7389
	private Collider[] m_colliders;

	// Token: 0x04001CDE RID: 7390
	private float m_support = 1f;

	// Token: 0x04001CDF RID: 7391
	private float m_createTime;

	// Token: 0x04001CE0 RID: 7392
	private int m_myIndex = -1;

	// Token: 0x04001CE1 RID: 7393
	private float m_rainTimer;

	// Token: 0x04001CE2 RID: 7394
	private float m_lastRepair;

	// Token: 0x04001CE3 RID: 7395
	private Piece m_piece;

	// Token: 0x04001CE4 RID: 7396
	private GameObject m_roof;

	// Token: 0x04001CE5 RID: 7397
	private List<WearNTear.BoundData> m_bounds;

	// Token: 0x04001CE6 RID: 7398
	private List<WearNTear.OldMeshData> m_oldMaterials;

	// Token: 0x04001CE7 RID: 7399
	private float m_updateCoverTimer;

	// Token: 0x04001CE8 RID: 7400
	private bool m_haveRoof = true;

	// Token: 0x04001CE9 RID: 7401
	private const float c_UpdateCoverFrequency = 4f;

	// Token: 0x020002C1 RID: 705
	public enum MaterialType
	{
		// Token: 0x04001CEB RID: 7403
		Wood,
		// Token: 0x04001CEC RID: 7404
		Stone,
		// Token: 0x04001CED RID: 7405
		Iron,
		// Token: 0x04001CEE RID: 7406
		HardWood,
		// Token: 0x04001CEF RID: 7407
		Marble
	}

	// Token: 0x020002C2 RID: 706
	private struct BoundData
	{
		// Token: 0x04001CF0 RID: 7408
		public Vector3 m_pos;

		// Token: 0x04001CF1 RID: 7409
		public Quaternion m_rot;

		// Token: 0x04001CF2 RID: 7410
		public Vector3 m_size;
	}

	// Token: 0x020002C3 RID: 707
	private struct OldMeshData
	{
		// Token: 0x04001CF3 RID: 7411
		public Renderer m_renderer;

		// Token: 0x04001CF4 RID: 7412
		public Material[] m_materials;

		// Token: 0x04001CF5 RID: 7413
		public Color[] m_color;

		// Token: 0x04001CF6 RID: 7414
		public Color[] m_emissiveColor;
	}
}
