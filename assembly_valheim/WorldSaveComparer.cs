using System;
using System.Collections.Generic;

// Token: 0x020001F1 RID: 497
public class WorldSaveComparer : IComparer<string>
{
	// Token: 0x0600142E RID: 5166 RVA: 0x000841FC File Offset: 0x000823FC
	public int Compare(string x, string y)
	{
		bool flag = true;
		int num = 0;
		string text;
		SaveFileType saveFileType;
		string a;
		DateTime? dateTime;
		if (!SaveSystem.GetSaveInfo(x, out text, out saveFileType, out a, out dateTime))
		{
			num++;
			flag = false;
		}
		string a2;
		if (!SaveSystem.GetSaveInfo(y, out text, out saveFileType, out a2, out dateTime))
		{
			num--;
			flag = false;
		}
		if (!flag)
		{
			return num;
		}
		if (a == ".fwl")
		{
			num--;
		}
		else if (a != ".db")
		{
			num++;
		}
		if (a2 == ".fwl")
		{
			num++;
		}
		else if (a2 != ".db")
		{
			num--;
		}
		return num;
	}
}
