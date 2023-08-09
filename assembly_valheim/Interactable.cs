using System;

// Token: 0x02000254 RID: 596
public interface Interactable
{
	// Token: 0x06001727 RID: 5927
	bool Interact(Humanoid user, bool hold, bool alt);

	// Token: 0x06001728 RID: 5928
	bool UseItem(Humanoid user, ItemDrop.ItemData item);
}
