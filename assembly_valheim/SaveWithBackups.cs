using System;
using System.Collections.Generic;

// Token: 0x020001EA RID: 490
public class SaveWithBackups
{
	// Token: 0x060013E8 RID: 5096 RVA: 0x000826B3 File Offset: 0x000808B3
	public SaveWithBackups(string name, SaveCollection parentSaveCollection, Action modifiedCallback)
	{
		this.m_name = name;
		this.ParentSaveCollection = parentSaveCollection;
		this.m_modifiedCallback = modifiedCallback;
	}

	// Token: 0x060013E9 RID: 5097 RVA: 0x000826F4 File Offset: 0x000808F4
	public SaveFile AddSaveFile(string filePath, FileHelpers.FileSource fileSource)
	{
		SaveFile saveFile = new SaveFile(filePath, fileSource, this, new Action(this.OnModified));
		string key = saveFile.FileName + "_" + saveFile.m_source.ToString();
		SaveFile saveFile2;
		if (this.m_saveFiles.Count > 0 && this.m_saveFilesByNameAndSource.TryGetValue(key, out saveFile2))
		{
			saveFile2.AddAssociatedFiles(saveFile.AllPaths);
		}
		else
		{
			this.m_saveFiles.Add(saveFile);
			this.m_saveFilesByNameAndSource.Add(key, saveFile);
		}
		this.OnModified();
		return saveFile;
	}

	// Token: 0x060013EA RID: 5098 RVA: 0x00082784 File Offset: 0x00080984
	public SaveFile AddSaveFile(string[] filePaths, FileHelpers.FileSource fileSource)
	{
		SaveFile saveFile = new SaveFile(filePaths, fileSource, this, new Action(this.OnModified));
		string key = saveFile.FileName + "_" + saveFile.m_source.ToString();
		SaveFile saveFile2;
		if (this.m_saveFiles.Count > 0 && this.m_saveFilesByNameAndSource.TryGetValue(key, out saveFile2))
		{
			saveFile2.AddAssociatedFiles(saveFile.AllPaths);
		}
		else
		{
			this.m_saveFiles.Add(saveFile);
			this.m_saveFilesByNameAndSource.Add(key, saveFile);
		}
		this.OnModified();
		return saveFile;
	}

	// Token: 0x060013EB RID: 5099 RVA: 0x00082814 File Offset: 0x00080A14
	public void RemoveSaveFile(SaveFile saveFile)
	{
		this.m_saveFiles.Remove(saveFile);
		string key = saveFile.FileName + "_" + saveFile.m_source.ToString();
		this.m_saveFilesByNameAndSource.Remove(key);
		this.OnModified();
	}

	// Token: 0x170000D3 RID: 211
	// (get) Token: 0x060013EC RID: 5100 RVA: 0x00082863 File Offset: 0x00080A63
	public SaveFile PrimaryFile
	{
		get
		{
			this.EnsureSortedAndPrimaryFileDetermined();
			return this.m_primaryFile;
		}
	}

	// Token: 0x170000D4 RID: 212
	// (get) Token: 0x060013ED RID: 5101 RVA: 0x00082871 File Offset: 0x00080A71
	public SaveFile[] BackupFiles
	{
		get
		{
			this.EnsureSortedAndPrimaryFileDetermined();
			return this.m_backupFiles.ToArray();
		}
	}

	// Token: 0x170000D5 RID: 213
	// (get) Token: 0x060013EE RID: 5102 RVA: 0x00082884 File Offset: 0x00080A84
	public SaveFile[] AllFiles
	{
		get
		{
			return this.m_saveFiles.ToArray();
		}
	}

	// Token: 0x170000D6 RID: 214
	// (get) Token: 0x060013EF RID: 5103 RVA: 0x00082894 File Offset: 0x00080A94
	public ulong SizeWithBackups
	{
		get
		{
			ulong num = 0UL;
			for (int i = 0; i < this.m_saveFiles.Count; i++)
			{
				num += this.m_saveFiles[i].Size;
			}
			return num;
		}
	}

	// Token: 0x170000D7 RID: 215
	// (get) Token: 0x060013F0 RID: 5104 RVA: 0x000828CF File Offset: 0x00080ACF
	public bool IsDeleted
	{
		get
		{
			return this.PrimaryFile == null;
		}
	}

	// Token: 0x170000D8 RID: 216
	// (get) Token: 0x060013F1 RID: 5105 RVA: 0x000828DA File Offset: 0x00080ADA
	// (set) Token: 0x060013F2 RID: 5106 RVA: 0x000828E2 File Offset: 0x00080AE2
	public SaveCollection ParentSaveCollection { get; private set; }

	// Token: 0x060013F3 RID: 5107 RVA: 0x000828EC File Offset: 0x00080AEC
	private void EnsureSortedAndPrimaryFileDetermined()
	{
		if (!this.m_isDirty)
		{
			return;
		}
		this.m_saveFiles.Sort(new SaveFileComparer());
		this.m_primaryFile = null;
		for (int i = 0; i < this.m_saveFiles.Count; i++)
		{
			string text;
			SaveFileType saveFileType;
			string text2;
			DateTime? dateTime;
			if (SaveSystem.GetSaveInfo(this.m_saveFiles[i].PathPrimary, out text, out saveFileType, out text2, out dateTime) && saveFileType == SaveFileType.Single && (this.m_primaryFile == null || this.m_saveFiles[i].m_source == FileHelpers.FileSource.Cloud || (this.m_saveFiles[i].m_source == FileHelpers.FileSource.Local && this.m_primaryFile.m_source == FileHelpers.FileSource.Legacy)))
			{
				this.m_primaryFile = this.m_saveFiles[i];
			}
		}
		this.m_backupFiles.Clear();
		if (this.m_primaryFile == null)
		{
			this.m_backupFiles.AddRange(this.m_saveFiles);
		}
		else
		{
			for (int j = 0; j < this.m_saveFiles.Count; j++)
			{
				if (this.m_saveFiles[j] != this.m_primaryFile)
				{
					this.m_backupFiles.Add(this.m_saveFiles[j]);
				}
			}
		}
		this.m_isDirty = false;
	}

	// Token: 0x060013F4 RID: 5108 RVA: 0x00082A1A File Offset: 0x00080C1A
	private void OnModified()
	{
		this.SetDirty();
		Action modifiedCallback = this.m_modifiedCallback;
		if (modifiedCallback == null)
		{
			return;
		}
		modifiedCallback();
	}

	// Token: 0x060013F5 RID: 5109 RVA: 0x00082A32 File Offset: 0x00080C32
	private void SetDirty()
	{
		this.m_isDirty = true;
	}

	// Token: 0x040014C1 RID: 5313
	public readonly string m_name;

	// Token: 0x040014C2 RID: 5314
	private List<SaveFile> m_saveFiles = new List<SaveFile>();

	// Token: 0x040014C4 RID: 5316
	private Action m_modifiedCallback;

	// Token: 0x040014C5 RID: 5317
	private bool m_isDirty;

	// Token: 0x040014C6 RID: 5318
	private SaveFile m_primaryFile;

	// Token: 0x040014C7 RID: 5319
	private List<SaveFile> m_backupFiles = new List<SaveFile>();

	// Token: 0x040014C8 RID: 5320
	private Dictionary<string, SaveFile> m_saveFilesByNameAndSource = new Dictionary<string, SaveFile>();
}
