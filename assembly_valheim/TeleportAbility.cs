using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200004E RID: 78
public class TeleportAbility : MonoBehaviour, IProjectile
{
	// Token: 0x06000417 RID: 1047 RVA: 0x000218C0 File Offset: 0x0001FAC0
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		this.m_owner = owner;
		GameObject gameObject = this.FindTarget();
		if (gameObject)
		{
			Vector3 position = gameObject.transform.position;
			if (ZoneSystem.instance.FindFloor(position, out position.y))
			{
				this.m_owner.transform.position = position;
				this.m_owner.transform.rotation = gameObject.transform.rotation;
				if (this.m_message.Length > 0)
				{
					Player.MessageAllInRange(base.transform.position, 100f, MessageHud.MessageType.Center, this.m_message, null);
				}
			}
		}
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x0002196C File Offset: 0x0001FB6C
	private GameObject FindTarget()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(this.m_targetTag);
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (Vector3.Distance(gameObject.transform.position, this.m_owner.transform.position) <= this.m_maxTeleportRange)
			{
				list.Add(gameObject);
			}
		}
		if (list.Count == 0)
		{
			ZLog.Log("No valid telport target in range");
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x0000C988 File Offset: 0x0000AB88
	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	// Token: 0x040004C9 RID: 1225
	public string m_targetTag = "";

	// Token: 0x040004CA RID: 1226
	public string m_message = "";

	// Token: 0x040004CB RID: 1227
	public float m_maxTeleportRange = 100f;

	// Token: 0x040004CC RID: 1228
	private Character m_owner;
}
