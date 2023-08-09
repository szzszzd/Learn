using System;
using UnityEngine;

// Token: 0x0200023B RID: 571
public interface IWaterInteractable
{
	// Token: 0x0600167D RID: 5757
	void SetLiquidLevel(float level, LiquidType type, Component liquidObj);

	// Token: 0x0600167E RID: 5758
	Transform GetTransform();

	// Token: 0x0600167F RID: 5759
	int Increment(LiquidType type);

	// Token: 0x06001680 RID: 5760
	int Decrement(LiquidType type);
}
