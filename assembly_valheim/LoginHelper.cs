using System;

// Token: 0x020001F4 RID: 500
public static class LoginHelper
{
	// Token: 0x1400000B RID: 11
	// (add) Token: 0x06001436 RID: 5174 RVA: 0x00084288 File Offset: 0x00082488
	// (remove) Token: 0x06001437 RID: 5175 RVA: 0x000842BC File Offset: 0x000824BC
	public static event OnLoginDoneCallback OnLoginDone;

	// Token: 0x06001438 RID: 5176 RVA: 0x000842EF File Offset: 0x000824EF
	public static void SetDone()
	{
		LoginHelper.IsDone = true;
		OnLoginDoneCallback onLoginDone = LoginHelper.OnLoginDone;
		if (onLoginDone == null)
		{
			return;
		}
		onLoginDone();
	}

	// Token: 0x040014E6 RID: 5350
	public static bool IsDone;
}
