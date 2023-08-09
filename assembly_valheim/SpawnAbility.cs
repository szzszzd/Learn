using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200004A RID: 74
public class SpawnAbility : MonoBehaviour, IProjectile
{
	// Token: 0x0600040A RID: 1034 RVA: 0x00020FAA File Offset: 0x0001F1AA
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		this.m_owner = owner;
		this.m_weapon = item;
		base.StartCoroutine("Spawn");
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x0000C988 File Offset: 0x0000AB88
	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x00020FC7 File Offset: 0x0001F1C7
	private IEnumerator Spawn()
	{
		if (this.m_initialSpawnDelay > 0f)
		{
			yield return new WaitForSeconds(this.m_initialSpawnDelay);
		}
		int toSpawn = UnityEngine.Random.Range(this.m_minToSpawn, this.m_maxToSpawn);
		Skills skills = this.m_owner.GetSkills();
		int num;
		for (int i = 0; i < toSpawn; i = num)
		{
			Vector3 targetPosition = base.transform.position;
			bool foundSpawnPoint = false;
			int tries = (this.m_targetType == SpawnAbility.TargetType.RandomPathfindablePosition) ? 5 : 1;
			int j = 0;
			while (j < tries && !(foundSpawnPoint = this.FindTarget(out targetPosition)))
			{
				if (this.m_targetType == SpawnAbility.TargetType.RandomPathfindablePosition)
				{
					if (j == tries - 1)
					{
						Terminal.LogWarning(string.Format("SpawnAbility failed to pathfindable target after {0} tries, defaulting to transform position.", tries));
						targetPosition = base.transform.position;
						foundSpawnPoint = true;
					}
					else
					{
						Terminal.Log("SpawnAbility failed to pathfindable target, waiting before retry.");
						yield return new WaitForSeconds(0.2f);
					}
				}
				num = j;
				j = num + 1;
			}
			if (!foundSpawnPoint)
			{
				Terminal.LogWarning("SpawnAbility failed to find spawn point, aborting spawn.");
			}
			else
			{
				Vector3 spawnPoint = targetPosition;
				if (this.m_targetType != SpawnAbility.TargetType.RandomPathfindablePosition)
				{
					Vector3 vector = this.m_spawnAtTarget ? targetPosition : base.transform.position;
					Vector2 vector2 = UnityEngine.Random.insideUnitCircle * this.m_spawnRadius;
					spawnPoint = vector + new Vector3(vector2.x, 0f, vector2.y);
					if (this.m_snapToTerrain)
					{
						float y;
						ZoneSystem.instance.GetSolidHeight(spawnPoint, out y, this.m_getSolidHeightMargin);
						spawnPoint.y = y;
					}
					spawnPoint.y += this.m_spawnGroundOffset;
					if (Mathf.Abs(spawnPoint.y - vector.y) > 100f)
					{
						goto IL_563;
					}
				}
				GameObject prefab = this.m_spawnPrefab[UnityEngine.Random.Range(0, this.m_spawnPrefab.Length)];
				if (this.m_maxSpawned <= 0 || SpawnSystem.GetNrOfInstances(prefab) < this.m_maxSpawned)
				{
					this.m_preSpawnEffects.Create(spawnPoint, Quaternion.identity, null, 1f, -1);
					if (this.m_preSpawnDelay > 0f)
					{
						yield return new WaitForSeconds(this.m_preSpawnDelay);
					}
					Terminal.Log("SpawnAbility spawning a " + prefab.name);
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, spawnPoint, Quaternion.Euler(0f, UnityEngine.Random.value * 3.1415927f * 2f, 0f));
					ZNetView component = gameObject.GetComponent<ZNetView>();
					Projectile component2 = gameObject.GetComponent<Projectile>();
					if (component2)
					{
						this.SetupProjectile(component2, targetPosition);
					}
					if (this.m_copySkill != Skills.SkillType.None && this.m_copySkillToRandomFactor > 0f)
					{
						component.GetZDO().Set(ZDOVars.s_randomSkillFactor, 1f + skills.GetSkillLevel(this.m_copySkill) * this.m_copySkillToRandomFactor);
					}
					if (this.m_levelUpSettings.Count > 0)
					{
						Character component3 = gameObject.GetComponent<Character>();
						if (component3 != null)
						{
							int k = this.m_levelUpSettings.Count - 1;
							while (k >= 0)
							{
								SpawnAbility.LevelUpSettings levelUpSettings = this.m_levelUpSettings[k];
								if (skills.GetSkillLevel(levelUpSettings.m_skill) >= (float)levelUpSettings.m_skillLevel)
								{
									component3.SetLevel(levelUpSettings.m_setLevel);
									int num2 = this.m_setMaxInstancesFromWeaponLevel ? this.m_weapon.m_quality : levelUpSettings.m_maxSpawns;
									if (num2 > 0)
									{
										component.GetZDO().Set(ZDOVars.s_maxInstances, num2, false);
										break;
									}
									break;
								}
								else
								{
									k--;
								}
							}
						}
					}
					if (this.m_commandOnSpawn)
					{
						Tameable component4 = gameObject.GetComponent<Tameable>();
						if (component4 != null)
						{
							Humanoid humanoid = this.m_owner as Humanoid;
							if (humanoid != null)
							{
								component4.Command(humanoid, false);
							}
						}
					}
					if (this.m_wakeUpAnimation)
					{
						ZSyncAnimation component5 = gameObject.GetComponent<ZSyncAnimation>();
						if (component5 != null)
						{
							component5.SetBool("wakeup", true);
						}
					}
					BaseAI component6 = gameObject.GetComponent<BaseAI>();
					if (component6 != null)
					{
						if (this.m_alertSpawnedCreature)
						{
							component6.Alert();
						}
						BaseAI baseAI = this.m_owner.GetBaseAI();
						if (component6.m_aggravatable && baseAI && baseAI.m_aggravatable)
						{
							component6.SetAggravated(baseAI.IsAggravated(), BaseAI.AggravatedReason.Damage);
						}
					}
					this.m_spawnEffects.Create(spawnPoint, Quaternion.identity, null, 1f, -1);
					if (this.m_spawnDelay > 0f)
					{
						yield return new WaitForSeconds(this.m_spawnDelay);
					}
					targetPosition = default(Vector3);
					spawnPoint = default(Vector3);
					prefab = null;
				}
			}
			IL_563:
			num = i + 1;
		}
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x00020FD8 File Offset: 0x0001F1D8
	private void SetupProjectile(Projectile projectile, Vector3 targetPoint)
	{
		Vector3 vector = (targetPoint - projectile.transform.position).normalized;
		Vector3 axis = Vector3.Cross(vector, Vector3.up);
		Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-this.m_projectileAccuracy, this.m_projectileAccuracy), Vector3.up);
		vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-this.m_projectileAccuracy, this.m_projectileAccuracy), axis) * vector;
		vector = rotation * vector;
		projectile.Setup(this.m_owner, vector * this.m_projectileVelocity, -1f, null, null, null);
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x00021070 File Offset: 0x0001F270
	private bool FindTarget(out Vector3 point)
	{
		point = Vector3.zero;
		switch (this.m_targetType)
		{
		case SpawnAbility.TargetType.ClosestEnemy:
		{
			if (this.m_owner == null)
			{
				return false;
			}
			Character character = BaseAI.FindClosestEnemy(this.m_owner, base.transform.position, this.m_maxTargetRange);
			if (character != null)
			{
				point = character.transform.position;
				return true;
			}
			return false;
		}
		case SpawnAbility.TargetType.RandomEnemy:
		{
			if (this.m_owner == null)
			{
				return false;
			}
			Character character2 = BaseAI.FindRandomEnemy(this.m_owner, base.transform.position, this.m_maxTargetRange);
			if (character2 != null)
			{
				point = character2.transform.position;
				return true;
			}
			return false;
		}
		case SpawnAbility.TargetType.Caster:
			if (this.m_owner == null)
			{
				return false;
			}
			point = this.m_owner.transform.position;
			return true;
		case SpawnAbility.TargetType.Position:
			point = base.transform.position;
			return true;
		case SpawnAbility.TargetType.RandomPathfindablePosition:
		{
			List<Vector3> list = new List<Vector3>();
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(this.m_spawnRadius / 2f, this.m_spawnRadius);
			point = base.transform.position + new Vector3(vector.x, 2f, vector.y);
			float y;
			ZoneSystem.instance.GetSolidHeight(point, out y, 2);
			point.y = y;
			if (Pathfinding.instance.GetPath(this.m_owner.transform.position, point, list, this.m_targetWhenPathfindingType, true, false, true))
			{
				Terminal.Log(string.Format("SpawnAbility found path target, distance: {0}", Vector3.Distance(base.transform.position, list[0])));
				point = list[list.Count - 1];
				return true;
			}
			return false;
		}
		default:
			return false;
		}
	}

	// Token: 0x04000497 RID: 1175
	[Header("Spawn")]
	public GameObject[] m_spawnPrefab;

	// Token: 0x04000498 RID: 1176
	public bool m_alertSpawnedCreature = true;

	// Token: 0x04000499 RID: 1177
	public bool m_spawnAtTarget = true;

	// Token: 0x0400049A RID: 1178
	public int m_minToSpawn = 1;

	// Token: 0x0400049B RID: 1179
	public int m_maxToSpawn = 1;

	// Token: 0x0400049C RID: 1180
	public int m_maxSpawned = 3;

	// Token: 0x0400049D RID: 1181
	public float m_spawnRadius = 3f;

	// Token: 0x0400049E RID: 1182
	public bool m_snapToTerrain = true;

	// Token: 0x0400049F RID: 1183
	public float m_spawnGroundOffset;

	// Token: 0x040004A0 RID: 1184
	public int m_getSolidHeightMargin = 1000;

	// Token: 0x040004A1 RID: 1185
	public float m_initialSpawnDelay;

	// Token: 0x040004A2 RID: 1186
	public float m_spawnDelay;

	// Token: 0x040004A3 RID: 1187
	public float m_preSpawnDelay;

	// Token: 0x040004A4 RID: 1188
	public bool m_commandOnSpawn;

	// Token: 0x040004A5 RID: 1189
	public bool m_wakeUpAnimation;

	// Token: 0x040004A6 RID: 1190
	public Skills.SkillType m_copySkill;

	// Token: 0x040004A7 RID: 1191
	public float m_copySkillToRandomFactor;

	// Token: 0x040004A8 RID: 1192
	public bool m_setMaxInstancesFromWeaponLevel;

	// Token: 0x040004A9 RID: 1193
	public List<SpawnAbility.LevelUpSettings> m_levelUpSettings;

	// Token: 0x040004AA RID: 1194
	public SpawnAbility.TargetType m_targetType;

	// Token: 0x040004AB RID: 1195
	public Pathfinding.AgentType m_targetWhenPathfindingType = Pathfinding.AgentType.Humanoid;

	// Token: 0x040004AC RID: 1196
	public float m_maxTargetRange = 40f;

	// Token: 0x040004AD RID: 1197
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x040004AE RID: 1198
	public EffectList m_preSpawnEffects = new EffectList();

	// Token: 0x040004AF RID: 1199
	[Header("Projectile")]
	public float m_projectileVelocity = 10f;

	// Token: 0x040004B0 RID: 1200
	public float m_projectileAccuracy = 10f;

	// Token: 0x040004B1 RID: 1201
	private Character m_owner;

	// Token: 0x040004B2 RID: 1202
	private ItemDrop.ItemData m_weapon;

	// Token: 0x0200004B RID: 75
	public enum TargetType
	{
		// Token: 0x040004B4 RID: 1204
		ClosestEnemy,
		// Token: 0x040004B5 RID: 1205
		RandomEnemy,
		// Token: 0x040004B6 RID: 1206
		Caster,
		// Token: 0x040004B7 RID: 1207
		Position,
		// Token: 0x040004B8 RID: 1208
		RandomPathfindablePosition
	}

	// Token: 0x0200004C RID: 76
	[Serializable]
	public class LevelUpSettings
	{
		// Token: 0x040004B9 RID: 1209
		public Skills.SkillType m_skill;

		// Token: 0x040004BA RID: 1210
		public int m_skillLevel;

		// Token: 0x040004BB RID: 1211
		public int m_setLevel;

		// Token: 0x040004BC RID: 1212
		public int m_maxSpawns;
	}
}
