using System;
using System.Collections.Generic;

// Token: 0x020001E9 RID: 489
public class SaveFile
{
	// Token: 0x060013D4 RID: 5076 RVA: 0x00082249 File Offset: 0x00080449
	public SaveFile(string path, FileHelpers.FileSource source, SaveWithBackups parentSaveWithBackups, Action modifiedCallback)
	{
		this.m_paths = new List<string>();
		this.m_source = source;
		this.ParentSaveWithBackups = parentSaveWithBackups;
		this.m_modifiedCallback = modifiedCallback;
		this.AddAssociatedFile(path);
	}

	// Token: 0x060013D5 RID: 5077 RVA: 0x00082284 File Offset: 0x00080484
	public SaveFile(string[] paths, FileHelpers.FileSource source, SaveWithBackups parentSaveWithBackups, Action modifiedCallback)
	{
		this.m_paths = new List<string>();
		this.m_source = source;
		this.ParentSaveWithBackups = parentSaveWithBackups;
		this.m_modifiedCallback = modifiedCallback;
		this.AddAssociatedFiles(paths);
	}

	// Token: 0x060013D6 RID: 5078 RVA: 0x000822C0 File Offset: 0x000804C0
	public SaveFile(FilePathAndSource pathAndSource, SaveWithBackups inSaveFile, Action modifiedCallback)
	{
		this.m_paths = new List<string>();
		this.m_source = pathAndSource.source;
		this.Size = 0UL;
		this.LastModified = FileHelpers.GetLastWriteTime(pathAndSource.path, pathAndSource.source);
		this.ParentSaveWithBackups = inSaveFile;
		this.m_modifiedCallback = modifiedCallback;
		this.AddAssociatedFile(pathAndSource.path);
	}

	// Token: 0x060013D7 RID: 5079 RVA: 0x00082330 File Offset: 0x00080530
	public void AddAssociatedFile(string path)
	{
		this.m_paths.Add(path);
		this.Size += FileHelpers.GetFileSize(path, this.m_source);
		DateTime lastWriteTime = FileHelpers.GetLastWriteTime(path, this.m_source);
		if (lastWriteTime > this.LastModified)
		{
			this.LastModified = lastWriteTime;
		}
		this.OnModified();
	}

	// Token: 0x060013D8 RID: 5080 RVA: 0x0008238C File Offset: 0x0008058C
	public void AddAssociatedFiles(string[] paths)
	{
		this.m_paths.AddRange(paths);
		for (int i = 0; i < paths.Length; i++)
		{
			this.Size += FileHelpers.GetFileSize(paths[i], this.m_source);
			DateTime lastWriteTime = FileHelpers.GetLastWriteTime(paths[i], this.m_source);
			if (lastWriteTime > this.LastModified)
			{
				this.LastModified = lastWriteTime;
			}
		}
		this.OnModified();
	}

	// Token: 0x170000CC RID: 204
	// (get) Token: 0x060013D9 RID: 5081 RVA: 0x000823F8 File Offset: 0x000805F8
	public string PathPrimary
	{
		get
		{
			this.EnsureSorted();
			return this.m_paths[0];
		}
	}

	// Token: 0x170000CD RID: 205
	// (get) Token: 0x060013DA RID: 5082 RVA: 0x0008240C File Offset: 0x0008060C
	public string[] PathsAssociated
	{
		get
		{
			this.EnsureSorted();
			string[] array = new string[this.m_paths.Count - 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = this.m_paths[i + 1];
			}
			return array;
		}
	}

	// Token: 0x170000CE RID: 206
	// (get) Token: 0x060013DB RID: 5083 RVA: 0x00082452 File Offset: 0x00080652
	public string[] AllPaths
	{
		get
		{
			this.EnsureSorted();
			return this.m_paths.ToArray();
		}
	}

	// Token: 0x170000CF RID: 207
	// (get) Token: 0x060013DC RID: 5084 RVA: 0x00082468 File Offset: 0x00080668
	public string FileName
	{
		get
		{
			if (this.m_fileName == null)
			{
				string pathPrimary = this.PathPrimary;
				string text;
				SaveFileType saveFileType;
				string text2;
				DateTime? dateTime;
				if (!SaveSystem.GetSaveInfo(pathPrimary, out text, out saveFileType, out text2, out dateTime))
				{
					this.m_fileName = SaveSystem.RemoveDirectoryPart(pathPrimary);
					return this.m_fileName;
				}
				SaveDataType dataType = this.ParentSaveWithBackups.ParentSaveCollection.m_dataType;
				if (dataType != SaveDataType.World)
				{
					if (dataType == SaveDataType.Character)
					{
						if (text2 != ".fch")
						{
							this.m_fileName = SaveSystem.RemoveDirectoryPart(pathPrimary);
							return this.m_fileName;
						}
					}
				}
				else if (text2 != ".fwl" && text2 != ".db")
				{
					this.m_fileName = SaveSystem.RemoveDirectoryPart(pathPrimary);
					return this.m_fileName;
				}
				string text3 = SaveSystem.RemoveDirectoryPart(pathPrimary);
				int num = text3.LastIndexOf(text2);
				if (num < 0)
				{
					this.m_fileName = text3;
				}
				else
				{
					this.m_fileName = text3.Remove(num, text2.Length);
				}
			}
			return this.m_fileName;
		}
	}

	// Token: 0x060013DD RID: 5085 RVA: 0x0008254C File Offset: 0x0008074C
	public override bool Equals(object obj)
	{
		SaveFile saveFile = obj as SaveFile;
		if (saveFile == null)
		{
			return false;
		}
		if (this.m_source != saveFile.m_source)
		{
			return false;
		}
		string[] allPaths = this.AllPaths;
		string[] allPaths2 = saveFile.AllPaths;
		if (allPaths.Length != allPaths2.Length)
		{
			return false;
		}
		for (int i = 0; i < allPaths.Length; i++)
		{
			if (allPaths[i] != allPaths2[i])
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060013DE RID: 5086 RVA: 0x000825AC File Offset: 0x000807AC
	public override int GetHashCode()
	{
		string[] allPaths = this.AllPaths;
		int num = 878520832;
		num = num * -1521134295 + allPaths.Length.GetHashCode();
		for (int i = 0; i < allPaths.Length; i++)
		{
			num = num * -1521134295 + EqualityComparer<string>.Default.GetHashCode(allPaths[i]);
		}
		return num * -1521134295 + this.m_source.GetHashCode();
	}

	// Token: 0x170000D0 RID: 208
	// (get) Token: 0x060013DF RID: 5087 RVA: 0x00082619 File Offset: 0x00080819
	// (set) Token: 0x060013E0 RID: 5088 RVA: 0x00082621 File Offset: 0x00080821
	public DateTime LastModified { get; private set; } = DateTime.MinValue;

	// Token: 0x170000D1 RID: 209
	// (get) Token: 0x060013E1 RID: 5089 RVA: 0x0008262A File Offset: 0x0008082A
	// (set) Token: 0x060013E2 RID: 5090 RVA: 0x00082632 File Offset: 0x00080832
	public ulong Size { get; private set; }

	// Token: 0x170000D2 RID: 210
	// (get) Token: 0x060013E3 RID: 5091 RVA: 0x0008263B File Offset: 0x0008083B
	// (set) Token: 0x060013E4 RID: 5092 RVA: 0x00082643 File Offset: 0x00080843
	public SaveWithBackups ParentSaveWithBackups { get; private set; }

	// Token: 0x060013E5 RID: 5093 RVA: 0x0008264C File Offset: 0x0008084C
	private void EnsureSorted()
	{
		if (!this.m_isDirty)
		{
			return;
		}
		this.m_paths.Sort(SaveSystem.GetComparerByDataType(this.ParentSaveWithBackups.ParentSaveCollection.m_dataType));
		this.m_isDirty = false;
	}

	// Token: 0x060013E6 RID: 5094 RVA: 0x0008267E File Offset: 0x0008087E
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

	// Token: 0x060013E7 RID: 5095 RVA: 0x00082696 File Offset: 0x00080896
	private void SetDirty()
	{
		this.m_isDirty = (this.m_paths.Count > 1);
		this.m_fileName = null;
	}

	// Token: 0x040014B9 RID: 5305
	private List<string> m_paths;

	// Token: 0x040014BA RID: 5306
	public readonly FileHelpers.FileSource m_source;

	// Token: 0x040014BE RID: 5310
	private Action m_modifiedCallback;

	// Token: 0x040014BF RID: 5311
	private bool m_isDirty;

	// Token: 0x040014C0 RID: 5312
	private string m_fileName;
}
