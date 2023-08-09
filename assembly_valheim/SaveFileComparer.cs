using System;
using System.Collections.Generic;

// Token: 0x020001EF RID: 495
public class SaveFileComparer : IComparer<SaveFile>
{
	// Token: 0x0600142A RID: 5162 RVA: 0x00084195 File Offset: 0x00082395
	public int Compare(SaveFile x, SaveFile y)
	{
		return DateTime.Compare(y.LastModified, x.LastModified);
	}
}
