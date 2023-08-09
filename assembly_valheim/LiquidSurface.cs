using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000260 RID: 608
public class LiquidSurface : MonoBehaviour
{
	// Token: 0x0600176B RID: 5995 RVA: 0x0009B00F File Offset: 0x0009920F
	private void Awake()
	{
		this.m_liquid = base.GetComponentInParent<LiquidVolume>();
	}

	// Token: 0x0600176C RID: 5996 RVA: 0x0009B01D File Offset: 0x0009921D
	private void FixedUpdate()
	{
		this.UpdateFloaters();
	}

	// Token: 0x0600176D RID: 5997 RVA: 0x0009B025 File Offset: 0x00099225
	public LiquidType GetLiquidType()
	{
		return this.m_liquid.m_liquidType;
	}

	// Token: 0x0600176E RID: 5998 RVA: 0x0009B032 File Offset: 0x00099232
	public float GetSurface(Vector3 p)
	{
		return this.m_liquid.GetSurface(p);
	}

	// Token: 0x0600176F RID: 5999 RVA: 0x0009B040 File Offset: 0x00099240
	private void OnTriggerEnter(Collider collider)
	{
		IWaterInteractable component = collider.attachedRigidbody.GetComponent<IWaterInteractable>();
		if (component != null)
		{
			component.Increment(this.m_liquid.m_liquidType);
			if (!this.m_inWater.Contains(component))
			{
				this.m_inWater.Add(component);
			}
		}
	}

	// Token: 0x06001770 RID: 6000 RVA: 0x0009B088 File Offset: 0x00099288
	private void UpdateFloaters()
	{
		if (this.m_inWater.Count == 0)
		{
			return;
		}
		LiquidSurface.s_inWaterRemoveIndices.Clear();
		for (int i = 0; i < this.m_inWater.Count; i++)
		{
			IWaterInteractable waterInteractable = this.m_inWater[i];
			if (waterInteractable == null)
			{
				LiquidSurface.s_inWaterRemoveIndices.Add(i);
			}
			else
			{
				Transform transform = waterInteractable.GetTransform();
				if (transform)
				{
					float surface = this.m_liquid.GetSurface(transform.position);
					waterInteractable.SetLiquidLevel(surface, this.m_liquid.m_liquidType, this);
				}
				else
				{
					LiquidSurface.s_inWaterRemoveIndices.Add(i);
				}
			}
		}
		for (int j = LiquidSurface.s_inWaterRemoveIndices.Count - 1; j >= 0; j--)
		{
			this.m_inWater.RemoveAt(LiquidSurface.s_inWaterRemoveIndices[j]);
		}
	}

	// Token: 0x06001771 RID: 6001 RVA: 0x0009B154 File Offset: 0x00099354
	private void OnTriggerExit(Collider collider)
	{
		IWaterInteractable component = collider.attachedRigidbody.GetComponent<IWaterInteractable>();
		if (component != null)
		{
			if (component.Decrement(this.m_liquid.m_liquidType) == 0)
			{
				component.SetLiquidLevel(-10000f, this.m_liquid.m_liquidType, this);
			}
			this.m_inWater.Remove(component);
		}
	}

	// Token: 0x06001772 RID: 6002 RVA: 0x0009B1A8 File Offset: 0x000993A8
	private void OnDestroy()
	{
		foreach (IWaterInteractable waterInteractable in this.m_inWater)
		{
			if (waterInteractable != null && waterInteractable.Decrement(this.m_liquid.m_liquidType) == 0)
			{
				waterInteractable.SetLiquidLevel(-10000f, this.m_liquid.m_liquidType, this);
			}
		}
		this.m_inWater.Clear();
	}

	// Token: 0x040018D2 RID: 6354
	private LiquidVolume m_liquid;

	// Token: 0x040018D3 RID: 6355
	private readonly List<IWaterInteractable> m_inWater = new List<IWaterInteractable>();

	// Token: 0x040018D4 RID: 6356
	private static readonly List<int> s_inWaterRemoveIndices = new List<int>();
}
