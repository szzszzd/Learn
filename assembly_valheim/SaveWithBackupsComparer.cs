using System;
using System.Collections.Generic;

// Token: 0x020001F0 RID: 496
public class SaveWithBackupsComparer : IComparer<SaveWithBackups>
{
	// Token: 0x0600142C RID: 5164 RVA: 0x000841A8 File Offset: 0x000823A8
	public int Compare(SaveWithBackups x, SaveWithBackups y)
	{
		if (x.IsDeleted || y.IsDeleted)
		{
			return 0 + (x.IsDeleted ? -1 : 0) + (y.IsDeleted ? 1 : 0);
		}
		return DateTime.Compare(y.PrimaryFile.LastModified, x.PrimaryFile.LastModified);
	}
}
