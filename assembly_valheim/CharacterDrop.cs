using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000008 RID: 8
[RequireComponent(typeof(Character))]
public class CharacterDrop : MonoBehaviour
{
	// Token: 0x0600011B RID: 283 RVA: 0x00007D10 File Offset: 0x00005F10
	private void Start()
	{
		this.m_character = base.GetComponent<Character>();
		if (this.m_character)
		{
			Character character = this.m_character;
			character.m_onDeath = (Action)Delegate.Combine(character.m_onDeath, new Action(this.OnDeath));
		}
	}

	// Token: 0x0600011C RID: 284 RVA: 0x00007D5D File Offset: 0x00005F5D
	public void SetDropsEnabled(bool enabled)
	{
		this.m_dropsEnabled = enabled;
	}

	// Token: 0x0600011D RID: 285 RVA: 0x00007D68 File Offset: 0x00005F68
	private void OnDeath()
	{
		if (!this.m_dropsEnabled)
		{
			return;
		}
		List<KeyValuePair<GameObject, int>> drops = this.GenerateDropList();
		Vector3 centerPos = this.m_character.GetCenterPoint() + base.transform.TransformVector(this.m_spawnOffset);
		CharacterDrop.DropItems(drops, centerPos, 0.5f);
	}

	// Token: 0x0600011E RID: 286 RVA: 0x00007DB4 File Offset: 0x00005FB4
	public List<KeyValuePair<GameObject, int>> GenerateDropList()
	{
		List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
		int num = this.m_character ? Mathf.Max(1, (int)Mathf.Pow(2f, (float)(this.m_character.GetLevel() - 1))) : 1;
		foreach (CharacterDrop.Drop drop in this.m_drops)
		{
			if (!(drop.m_prefab == null))
			{
				float num2 = drop.m_chance;
				if (drop.m_levelMultiplier)
				{
					num2 *= (float)num;
				}
				if (UnityEngine.Random.value <= num2)
				{
					int num3 = UnityEngine.Random.Range(drop.m_amountMin, drop.m_amountMax);
					if (drop.m_levelMultiplier)
					{
						num3 *= num;
					}
					if (drop.m_onePerPlayer)
					{
						num3 = ZNet.instance.GetNrOfPlayers();
					}
					if (num3 > 0)
					{
						list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, num3));
					}
				}
			}
		}
		return list;
	}

	// Token: 0x0600011F RID: 287 RVA: 0x00007EB8 File Offset: 0x000060B8
	public static void DropItems(List<KeyValuePair<GameObject, int>> drops, Vector3 centerPos, float dropArea)
	{
		foreach (KeyValuePair<GameObject, int> keyValuePair in drops)
		{
			for (int i = 0; i < keyValuePair.Value; i++)
			{
				Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				Vector3 b = UnityEngine.Random.insideUnitSphere * dropArea;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(keyValuePair.Key, centerPos + b, rotation);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component)
				{
					Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
					if (insideUnitSphere.y < 0f)
					{
						insideUnitSphere.y = -insideUnitSphere.y;
					}
					component.AddForce(insideUnitSphere * 5f, ForceMode.VelocityChange);
				}
			}
		}
	}

	// Token: 0x040000F8 RID: 248
	public Vector3 m_spawnOffset = Vector3.zero;

	// Token: 0x040000F9 RID: 249
	public List<CharacterDrop.Drop> m_drops = new List<CharacterDrop.Drop>();

	// Token: 0x040000FA RID: 250
	private const float m_dropArea = 0.5f;

	// Token: 0x040000FB RID: 251
	private const float m_vel = 5f;

	// Token: 0x040000FC RID: 252
	private bool m_dropsEnabled = true;

	// Token: 0x040000FD RID: 253
	private Character m_character;

	// Token: 0x02000009 RID: 9
	[Serializable]
	public class Drop
	{
		// Token: 0x040000FE RID: 254
		public GameObject m_prefab;

		// Token: 0x040000FF RID: 255
		public int m_amountMin = 1;

		// Token: 0x04000100 RID: 256
		public int m_amountMax = 1;

		// Token: 0x04000101 RID: 257
		public float m_chance = 1f;

		// Token: 0x04000102 RID: 258
		public bool m_onePerPlayer;

		// Token: 0x04000103 RID: 259
		public bool m_levelMultiplier = true;
	}
}
