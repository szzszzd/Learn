using System;
using UnityEngine;

// Token: 0x0200012D RID: 301
public class PickableItem : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000BBE RID: 3006 RVA: 0x00056B40 File Offset: 0x00054D40
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.SetupRandomPrefab();
		this.m_nview.Register("Pick", new Action<long>(this.RPC_Pick));
		this.SetupItem(true);
	}

	// Token: 0x06000BBF RID: 3007 RVA: 0x00056B90 File Offset: 0x00054D90
	private void SetupRandomPrefab()
	{
		if (this.m_itemPrefab == null && this.m_randomItemPrefabs.Length != 0)
		{
			int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_itemPrefab, 0);
			if (@int == 0)
			{
				if (this.m_nview.IsOwner())
				{
					PickableItem.RandomItem randomItem = this.m_randomItemPrefabs[UnityEngine.Random.Range(0, this.m_randomItemPrefabs.Length)];
					this.m_itemPrefab = randomItem.m_itemPrefab;
					this.m_stack = UnityEngine.Random.Range(randomItem.m_stackMin, randomItem.m_stackMax + 1);
					int prefabHash = ObjectDB.instance.GetPrefabHash(this.m_itemPrefab.gameObject);
					this.m_nview.GetZDO().Set(ZDOVars.s_itemPrefab, prefabHash, false);
					this.m_nview.GetZDO().Set(ZDOVars.s_itemStack, this.m_stack, false);
					return;
				}
				return;
			}
			else
			{
				GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(@int);
				if (itemPrefab == null)
				{
					ZLog.LogError("Failed to find saved prefab " + @int.ToString() + " in PickableItem " + base.gameObject.name);
					return;
				}
				this.m_itemPrefab = itemPrefab.GetComponent<ItemDrop>();
				this.m_stack = this.m_nview.GetZDO().GetInt(ZDOVars.s_itemStack, 0);
			}
		}
	}

	// Token: 0x06000BC0 RID: 3008 RVA: 0x00056CD5 File Offset: 0x00054ED5
	public string GetHoverText()
	{
		if (this.m_picked)
		{
			return "";
		}
		return Localization.instance.Localize(this.GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	// Token: 0x06000BC1 RID: 3009 RVA: 0x00056D00 File Offset: 0x00054F00
	public string GetHoverName()
	{
		if (!this.m_itemPrefab)
		{
			return "None";
		}
		int stackSize = this.GetStackSize();
		if (stackSize > 1)
		{
			return this.m_itemPrefab.m_itemData.m_shared.m_name + " x " + stackSize.ToString();
		}
		return this.m_itemPrefab.m_itemData.m_shared.m_name;
	}

	// Token: 0x06000BC2 RID: 3010 RVA: 0x00056D67 File Offset: 0x00054F67
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		this.m_nview.InvokeRPC("Pick", Array.Empty<object>());
		return true;
	}

	// Token: 0x06000BC3 RID: 3011 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000BC4 RID: 3012 RVA: 0x00056D90 File Offset: 0x00054F90
	private void RPC_Pick(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_picked)
		{
			return;
		}
		this.m_picked = true;
		this.m_pickEffector.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		this.Drop();
		this.m_nview.Destroy();
	}

	// Token: 0x06000BC5 RID: 3013 RVA: 0x00056DF0 File Offset: 0x00054FF0
	private void Drop()
	{
		Vector3 position = base.transform.position + Vector3.up * 0.2f;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_itemPrefab.gameObject, position, base.transform.rotation);
		gameObject.GetComponent<ItemDrop>().m_itemData.m_stack = this.GetStackSize();
		gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
	}

	// Token: 0x06000BC6 RID: 3014 RVA: 0x00056E68 File Offset: 0x00055068
	private int GetStackSize()
	{
		return Mathf.Clamp((this.m_stack > 0) ? this.m_stack : this.m_itemPrefab.m_itemData.m_stack, 1, this.m_itemPrefab.m_itemData.m_shared.m_maxStackSize);
	}

	// Token: 0x06000BC7 RID: 3015 RVA: 0x00056EA8 File Offset: 0x000550A8
	private GameObject GetAttachPrefab()
	{
		Transform transform = this.m_itemPrefab.transform.Find("attach");
		if (transform)
		{
			return transform.gameObject;
		}
		return null;
	}

	// Token: 0x06000BC8 RID: 3016 RVA: 0x00056EDC File Offset: 0x000550DC
	private void SetupItem(bool enabled)
	{
		if (!enabled)
		{
			if (this.m_instance)
			{
				UnityEngine.Object.Destroy(this.m_instance);
				this.m_instance = null;
			}
			return;
		}
		if (this.m_instance)
		{
			return;
		}
		if (this.m_itemPrefab == null)
		{
			return;
		}
		GameObject attachPrefab = this.GetAttachPrefab();
		if (attachPrefab == null)
		{
			ZLog.LogWarning("Failed to get attach prefab for item " + this.m_itemPrefab.name);
			return;
		}
		this.m_instance = UnityEngine.Object.Instantiate<GameObject>(attachPrefab, base.transform.position, base.transform.rotation, base.transform);
		this.m_instance.transform.localPosition = attachPrefab.transform.localPosition;
		this.m_instance.transform.localRotation = attachPrefab.transform.localRotation;
	}

	// Token: 0x06000BC9 RID: 3017 RVA: 0x00056FB4 File Offset: 0x000551B4
	private bool DrawPrefabMesh(ItemDrop prefab)
	{
		if (prefab == null)
		{
			return false;
		}
		bool result = false;
		Gizmos.color = Color.yellow;
		foreach (MeshFilter meshFilter in prefab.gameObject.GetComponentsInChildren<MeshFilter>())
		{
			if (meshFilter && meshFilter.sharedMesh)
			{
				Vector3 position = prefab.transform.position;
				Quaternion lhs = Quaternion.Inverse(prefab.transform.rotation);
				Vector3 point = meshFilter.transform.position - position;
				Vector3 position2 = base.transform.position + base.transform.rotation * point;
				Quaternion rhs = lhs * meshFilter.transform.rotation;
				Quaternion rotation = base.transform.rotation * rhs;
				Gizmos.DrawMesh(meshFilter.sharedMesh, position2, rotation, meshFilter.transform.lossyScale);
				result = true;
			}
		}
		return result;
	}

	// Token: 0x04000E2F RID: 3631
	public ItemDrop m_itemPrefab;

	// Token: 0x04000E30 RID: 3632
	public int m_stack;

	// Token: 0x04000E31 RID: 3633
	public PickableItem.RandomItem[] m_randomItemPrefabs = Array.Empty<PickableItem.RandomItem>();

	// Token: 0x04000E32 RID: 3634
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x04000E33 RID: 3635
	private ZNetView m_nview;

	// Token: 0x04000E34 RID: 3636
	private GameObject m_instance;

	// Token: 0x04000E35 RID: 3637
	private bool m_picked;

	// Token: 0x0200012E RID: 302
	[Serializable]
	public struct RandomItem
	{
		// Token: 0x04000E36 RID: 3638
		public ItemDrop m_itemPrefab;

		// Token: 0x04000E37 RID: 3639
		public int m_stackMin;

		// Token: 0x04000E38 RID: 3640
		public int m_stackMax;
	}
}
