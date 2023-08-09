using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

// Token: 0x020001EB RID: 491
public class SaveCollection
{
	// Token: 0x060013F6 RID: 5110 RVA: 0x00082A3B File Offset: 0x00080C3B
	public SaveCollection(SaveDataType dataType)
	{
		this.m_dataType = dataType;
	}

	// Token: 0x170000D9 RID: 217
	// (get) Token: 0x060013F7 RID: 5111 RVA: 0x00082A6C File Offset: 0x00080C6C
	public SaveWithBackups[] Saves
	{
		get
		{
			this.EnsureLoadedAndSorted();
			return this.m_saves.ToArray();
		}
	}

	// Token: 0x060013F8 RID: 5112 RVA: 0x00082A7F File Offset: 0x00080C7F
	public void Add(SaveWithBackups save)
	{
		this.m_saves.Add(save);
		this.SetNeedsSort();
	}

	// Token: 0x060013F9 RID: 5113 RVA: 0x00082A93 File Offset: 0x00080C93
	public void Remove(SaveWithBackups save)
	{
		this.m_saves.Remove(save);
		this.SetNeedsSort();
	}

	// Token: 0x060013FA RID: 5114 RVA: 0x00082AA8 File Offset: 0x00080CA8
	public void EnsureLoadedAndSorted()
	{
		this.EnsureLoaded();
		if (this.m_needsSort)
		{
			this.Sort();
		}
	}

	// Token: 0x060013FB RID: 5115 RVA: 0x00082ABE File Offset: 0x00080CBE
	private void EnsureLoaded()
	{
		if (this.m_needsReload)
		{
			this.Reload();
		}
	}

	// Token: 0x060013FC RID: 5116 RVA: 0x00082ACE File Offset: 0x00080CCE
	public void InvalidateCache()
	{
		this.m_needsReload = true;
	}

	// Token: 0x060013FD RID: 5117 RVA: 0x00082AD7 File Offset: 0x00080CD7
	public bool TryGetSaveByName(string name, out SaveWithBackups save)
	{
		this.EnsureLoaded();
		return this.m_savesByName.TryGetValue(name, out save);
	}

	// Token: 0x060013FE RID: 5118 RVA: 0x00082AEC File Offset: 0x00080CEC
	private void Reload()
	{
		this.m_saves.Clear();
		this.m_savesByName.Clear();
		List<string> list = new List<string>();
		if (FileHelpers.m_cloudEnabled)
		{
			SaveCollection.<Reload>g__GetAllFilesInSource|14_0(this.m_dataType, FileHelpers.FileSource.Cloud, ref list);
		}
		int count = list.Count;
		if (Directory.Exists(SaveSystem.GetSavePath(this.m_dataType, FileHelpers.FileSource.Local)))
		{
			SaveCollection.<Reload>g__GetAllFilesInSource|14_0(this.m_dataType, FileHelpers.FileSource.Local, ref list);
		}
		int count2 = list.Count;
		if (Directory.Exists(SaveSystem.GetSavePath(this.m_dataType, FileHelpers.FileSource.Legacy)))
		{
			SaveCollection.<Reload>g__GetAllFilesInSource|14_0(this.m_dataType, FileHelpers.FileSource.Legacy, ref list);
		}
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			string text2;
			SaveFileType saveFileType;
			string text3;
			DateTime? dateTime;
			if (SaveSystem.GetSaveInfo(text, out text2, out saveFileType, out text3, out dateTime))
			{
				FileHelpers.FileSource fileSource = SaveCollection.<Reload>g__SourceByIndexAndEntryCount|14_1(count, count2, i);
				SaveWithBackups saveWithBackups;
				if (!this.m_savesByName.TryGetValue(text2, out saveWithBackups))
				{
					saveWithBackups = new SaveWithBackups(text2, this, new Action(this.SetNeedsSort));
					this.m_saves.Add(saveWithBackups);
					this.m_savesByName.Add(text2, saveWithBackups);
				}
				saveWithBackups.AddSaveFile(text, fileSource);
			}
		}
		this.m_needsReload = false;
		this.SetNeedsSort();
	}

	// Token: 0x060013FF RID: 5119 RVA: 0x00082C0E File Offset: 0x00080E0E
	private void Sort()
	{
		this.m_saves.Sort(new SaveWithBackupsComparer());
		this.m_needsSort = false;
	}

	// Token: 0x06001400 RID: 5120 RVA: 0x00082C27 File Offset: 0x00080E27
	private void SetNeedsSort()
	{
		this.m_needsSort = true;
	}

	// Token: 0x06001401 RID: 5121 RVA: 0x00082C30 File Offset: 0x00080E30
	[CompilerGenerated]
	internal static bool <Reload>g__GetAllFilesInSource|14_0(SaveDataType dataType, FileHelpers.FileSource source, ref List<string> listToAddTo)
	{
		string savePath = SaveSystem.GetSavePath(dataType, source);
		string[] files = FileHelpers.GetFiles(source, savePath, null, null);
		if (source == FileHelpers.FileSource.Legacy)
		{
			for (int i = 0; i < files.Length; i++)
			{
				if (!files[i].EndsWith("steam_autocloud.vdf"))
				{
					listToAddTo.Add(files[i]);
				}
			}
		}
		else
		{
			listToAddTo.AddRange(files);
		}
		return true;
	}

	// Token: 0x06001402 RID: 5122 RVA: 0x00082C85 File Offset: 0x00080E85
	[CompilerGenerated]
	internal static FileHelpers.FileSource <Reload>g__SourceByIndexAndEntryCount|14_1(int cloudEntries, int localEntries, int i)
	{
		if (i < cloudEntries)
		{
			return FileHelpers.FileSource.Cloud;
		}
		if (i < localEntries)
		{
			return FileHelpers.FileSource.Local;
		}
		return FileHelpers.FileSource.Legacy;
	}

	// Token: 0x040014C9 RID: 5321
	public readonly SaveDataType m_dataType;

	// Token: 0x040014CA RID: 5322
	private List<SaveWithBackups> m_saves = new List<SaveWithBackups>();

	// Token: 0x040014CB RID: 5323
	private Dictionary<string, SaveWithBackups> m_savesByName = new Dictionary<string, SaveWithBackups>(StringComparer.OrdinalIgnoreCase);

	// Token: 0x040014CC RID: 5324
	private bool m_needsSort;

	// Token: 0x040014CD RID: 5325
	private bool m_needsReload = true;
}
