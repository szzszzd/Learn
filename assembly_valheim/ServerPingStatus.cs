using System;

// Token: 0x02000149 RID: 329
public enum ServerPingStatus
{
	// Token: 0x04000EAC RID: 3756
	NotStarted,
	// Token: 0x04000EAD RID: 3757
	AwaitingResponse,
	// Token: 0x04000EAE RID: 3758
	Success,
	// Token: 0x04000EAF RID: 3759
	TimedOut,
	// Token: 0x04000EB0 RID: 3760
	CouldNotReach,
	// Token: 0x04000EB1 RID: 3761
	Unpingable
}
