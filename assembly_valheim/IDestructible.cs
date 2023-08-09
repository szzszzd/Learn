using System;

// Token: 0x02000229 RID: 553
public interface IDestructible
{
	// Token: 0x060015DA RID: 5594
	void Damage(HitData hit);

	// Token: 0x060015DB RID: 5595
	DestructibleType GetDestructibleType();
}
