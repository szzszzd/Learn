using System;
using UnityEngine;

// Token: 0x0200028E RID: 654
public interface IDoodadController
{
	// Token: 0x0600190B RID: 6411
	void OnUseStop(Player player);

	// Token: 0x0600190C RID: 6412
	void ApplyControlls(Vector3 moveDir, Vector3 lookDir, bool run, bool autoRun, bool block);

	// Token: 0x0600190D RID: 6413
	Component GetControlledComponent();

	// Token: 0x0600190E RID: 6414
	Vector3 GetPosition();

	// Token: 0x0600190F RID: 6415
	bool IsValid();
}
