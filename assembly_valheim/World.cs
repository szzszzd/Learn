using System;
using System.IO;
using UnityEngine;

// Token: 0x02000207 RID: 519
public class World
{
	// Token: 0x060014AC RID: 5292 RVA: 0x0008676D File Offset: 0x0008496D
	public World()
	{
	}

	// Token: 0x060014AD RID: 5293 RVA: 0x000867A0 File Offset: 0x000849A0
	public World(SaveWithBackups save, World.SaveDataError dataError)
	{
		this.m_fileName = (this.m_name = save.m_name);
		this.m_dataError = dataError;
		this.m_fileSource = save.PrimaryFile.m_source;
	}

	// Token: 0x060014AE RID: 5294 RVA: 0x00086808 File Offset: 0x00084A08
	public World(string name, string seed)
	{
		this.m_name = name;
		this.m_fileName = name;
		this.m_seedName = seed;
		this.m_seed = ((this.m_seedName == "") ? 0 : this.m_seedName.GetStableHashCode());
		this.m_uid = (long)name.GetStableHashCode() + Utils.GenerateUID();
		this.m_worldGenVersion = 2;
	}

	// Token: 0x060014AF RID: 5295 RVA: 0x0008689A File Offset: 0x00084A9A
	public static string GetWorldSavePath(FileHelpers.FileSource fileSource = FileHelpers.FileSource.Auto)
	{
		return Utils.GetSaveDataPath(fileSource) + ((fileSource == FileHelpers.FileSource.Local) ? "/worlds_local" : "/worlds");
	}

	// Token: 0x060014B0 RID: 5296 RVA: 0x000868B8 File Offset: 0x00084AB8
	public static void RemoveWorld(string name, FileHelpers.FileSource fileSource)
	{
		SaveWithBackups saveWithBackups;
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.World, out saveWithBackups) && !saveWithBackups.IsDeleted)
		{
			SaveSystem.Delete(saveWithBackups.PrimaryFile);
		}
	}

	// Token: 0x060014B1 RID: 5297 RVA: 0x000868E4 File Offset: 0x00084AE4
	public string GetDBPath()
	{
		return this.GetDBPath(this.m_fileSource);
	}

	// Token: 0x060014B2 RID: 5298 RVA: 0x000868F2 File Offset: 0x00084AF2
	public string GetDBPath(FileHelpers.FileSource fileSource)
	{
		return World.GetWorldSavePath(fileSource) + "/" + this.m_fileName + ".db";
	}

	// Token: 0x060014B3 RID: 5299 RVA: 0x0008690F File Offset: 0x00084B0F
	public static string GetDBPath(string name, FileHelpers.FileSource fileSource = FileHelpers.FileSource.Auto)
	{
		return World.GetWorldSavePath(fileSource) + "/" + name + ".db";
	}

	// Token: 0x060014B4 RID: 5300 RVA: 0x00086927 File Offset: 0x00084B27
	public string GetMetaPath()
	{
		return this.GetMetaPath(this.m_fileSource);
	}

	// Token: 0x060014B5 RID: 5301 RVA: 0x00086935 File Offset: 0x00084B35
	public string GetMetaPath(FileHelpers.FileSource fileSource)
	{
		return World.GetWorldSavePath(fileSource) + "/" + this.m_fileName + ".fwl";
	}

	// Token: 0x060014B6 RID: 5302 RVA: 0x00086952 File Offset: 0x00084B52
	public static string GetMetaPath(string name, FileHelpers.FileSource fileSource = FileHelpers.FileSource.Auto)
	{
		return World.GetWorldSavePath(fileSource) + "/" + name + ".fwl";
	}

	// Token: 0x060014B7 RID: 5303 RVA: 0x0008696C File Offset: 0x00084B6C
	public static bool HaveWorld(string name)
	{
		SaveWithBackups saveWithBackups;
		return SaveSystem.TryGetSaveByName(name, SaveDataType.World, out saveWithBackups) && !saveWithBackups.IsDeleted;
	}

	// Token: 0x060014B8 RID: 5304 RVA: 0x0008698F File Offset: 0x00084B8F
	public static World GetMenuWorld()
	{
		return new World("menu", "")
		{
			m_menu = true
		};
	}

	// Token: 0x060014B9 RID: 5305 RVA: 0x000869A7 File Offset: 0x00084BA7
	public static World GetEditorWorld()
	{
		return new World("editor", "");
	}

	// Token: 0x060014BA RID: 5306 RVA: 0x000869B8 File Offset: 0x00084BB8
	public static string GenerateSeed()
	{
		string text = "";
		for (int i = 0; i < 10; i++)
		{
			text += "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ023456789"[UnityEngine.Random.Range(0, "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ023456789".Length)].ToString();
		}
		return text;
	}

	// Token: 0x060014BB RID: 5307 RVA: 0x00086A04 File Offset: 0x00084C04
	public static World GetCreateWorld(string name, FileHelpers.FileSource source)
	{
		ZLog.Log("Get create world " + name);
		SaveWithBackups saveWithBackups;
		World world;
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.World, out saveWithBackups) && !saveWithBackups.IsDeleted)
		{
			world = World.LoadWorld(saveWithBackups);
			if (world.m_dataError == World.SaveDataError.None)
			{
				return world;
			}
			ZLog.LogError(string.Format("Failed to load world with name \"{0}\", data error {1}.", name, world.m_dataError));
		}
		ZLog.Log(" creating");
		world = new World(name, World.GenerateSeed());
		world.m_fileSource = source;
		world.SaveWorldMetaData(DateTime.Now);
		return world;
	}

	// Token: 0x060014BC RID: 5308 RVA: 0x00086A8C File Offset: 0x00084C8C
	public static World GetDevWorld()
	{
		SaveWithBackups saveWithBackups;
		World world;
		if (SaveSystem.TryGetSaveByName(Game.instance.m_devWorldName, SaveDataType.World, out saveWithBackups) && !saveWithBackups.IsDeleted)
		{
			world = World.LoadWorld(saveWithBackups);
			if (world.m_dataError == World.SaveDataError.None)
			{
				return world;
			}
			ZLog.Log(string.Format("Failed to load dev world, data error {0}. Creating...", world.m_dataError));
		}
		world = new World(Game.instance.m_devWorldName, Game.instance.m_devWorldSeed);
		world.m_fileSource = FileHelpers.FileSource.Local;
		world.SaveWorldMetaData(DateTime.Now);
		return world;
	}

	// Token: 0x060014BD RID: 5309 RVA: 0x00086B10 File Offset: 0x00084D10
	public void SaveWorldMetaData(DateTime backupTimestamp)
	{
		bool flag;
		FileWriter fileWriter;
		this.SaveWorldMetaData(backupTimestamp, true, out flag, out fileWriter);
	}

	// Token: 0x060014BE RID: 5310 RVA: 0x00086B2C File Offset: 0x00084D2C
	public void SaveWorldMetaData(DateTime now, bool considerBackup, out bool cloudSaveFailed, out FileWriter metaWriter)
	{
		this.GetDBPath();
		SaveSystem.CheckMove(this.m_fileName, SaveDataType.World, ref this.m_fileSource, now, 0UL);
		ZPackage zpackage = new ZPackage();
		zpackage.Write(31);
		zpackage.Write(this.m_name);
		zpackage.Write(this.m_seedName);
		zpackage.Write(this.m_seed);
		zpackage.Write(this.m_uid);
		zpackage.Write(this.m_worldGenVersion);
		zpackage.Write(this.m_needsDB);
		if (this.m_fileSource != FileHelpers.FileSource.Cloud)
		{
			Directory.CreateDirectory(World.GetWorldSavePath(this.m_fileSource));
		}
		string metaPath = this.GetMetaPath();
		string text = metaPath + ".new";
		string oldFile = metaPath + ".old";
		byte[] array = zpackage.GetArray();
		bool flag = this.m_fileSource == FileHelpers.FileSource.Cloud;
		FileWriter fileWriter = new FileWriter(flag ? metaPath : text, FileHelpers.FileHelperType.Binary, this.m_fileSource);
		fileWriter.m_binary.Write(array.Length);
		fileWriter.m_binary.Write(array);
		fileWriter.Finish();
		SaveSystem.InvalidateCache();
		cloudSaveFailed = (fileWriter.Status != FileWriter.WriterStatus.CloseSucceeded && this.m_fileSource == FileHelpers.FileSource.Cloud);
		if (!cloudSaveFailed)
		{
			if (!flag)
			{
				FileHelpers.ReplaceOldFile(metaPath, text, oldFile, this.m_fileSource);
				SaveSystem.InvalidateCache();
			}
			if (considerBackup)
			{
				ZNet.ConsiderAutoBackup(this.m_fileName, SaveDataType.World, now);
			}
		}
		metaWriter = fileWriter;
	}

	// Token: 0x060014BF RID: 5311 RVA: 0x00086C7C File Offset: 0x00084E7C
	public static World LoadWorld(SaveWithBackups saveFile)
	{
		FileReader fileReader = null;
		if (saveFile.IsDeleted)
		{
			ZLog.Log("save deleted " + saveFile.m_name);
			return new World(saveFile, World.SaveDataError.LoadError);
		}
		FileHelpers.FileSource source = saveFile.PrimaryFile.m_source;
		string pathPrimary = saveFile.PrimaryFile.PathPrimary;
		string text = (saveFile.PrimaryFile.PathsAssociated.Length != 0) ? saveFile.PrimaryFile.PathsAssociated[0] : null;
		if (FileHelpers.IsFileCorrupt(pathPrimary, source) || (text != null && FileHelpers.IsFileCorrupt(text, source)))
		{
			ZLog.Log("  corrupt save " + saveFile.m_name);
			return new World(saveFile, World.SaveDataError.Corrupt);
		}
		try
		{
			fileReader = new FileReader(pathPrimary, source, FileHelpers.FileHelperType.Binary);
		}
		catch (Exception ex)
		{
			if (fileReader != null)
			{
				fileReader.Dispose();
			}
			string str = "  failed to load ";
			string name = saveFile.m_name;
			string str2 = " Exception: ";
			Exception ex2 = ex;
			ZLog.Log(str + name + str2 + ((ex2 != null) ? ex2.ToString() : null));
			return new World(saveFile, World.SaveDataError.LoadError);
		}
		World result;
		try
		{
			BinaryReader binary = fileReader.m_binary;
			int count = binary.ReadInt32();
			ZPackage zpackage = new ZPackage(binary.ReadBytes(count));
			int num = zpackage.ReadInt();
			if (!global::Version.IsWorldVersionCompatible(num))
			{
				ZLog.Log("incompatible world version " + num.ToString());
				result = new World(saveFile, World.SaveDataError.BadVersion);
			}
			else
			{
				World world = new World();
				world.m_fileSource = source;
				world.m_fileName = saveFile.m_name;
				world.m_name = zpackage.ReadString();
				world.m_seedName = zpackage.ReadString();
				world.m_seed = zpackage.ReadInt();
				world.m_uid = zpackage.ReadLong();
				if (num >= 26)
				{
					world.m_worldGenVersion = zpackage.ReadInt();
				}
				world.m_needsDB = (num >= 30 && zpackage.ReadBool());
				if (num != 31 || world.m_worldGenVersion != 2)
				{
					world.m_createBackupBeforeSaving = true;
				}
				if (world.CheckDbFile())
				{
					world.m_dataError = World.SaveDataError.MissingDB;
				}
				result = world;
			}
		}
		catch
		{
			ZLog.LogWarning("  error loading world " + saveFile.m_name);
			result = new World(saveFile, World.SaveDataError.LoadError);
		}
		finally
		{
			if (fileReader != null)
			{
				fileReader.Dispose();
			}
		}
		return result;
	}

	// Token: 0x060014C0 RID: 5312 RVA: 0x00086EE0 File Offset: 0x000850E0
	private bool CheckDbFile()
	{
		return this.m_needsDB && !FileHelpers.Exists(this.GetDBPath(), this.m_fileSource);
	}

	// Token: 0x04001560 RID: 5472
	public string m_fileName = "";

	// Token: 0x04001561 RID: 5473
	public string m_name = "";

	// Token: 0x04001562 RID: 5474
	public string m_seedName = "";

	// Token: 0x04001563 RID: 5475
	public int m_seed;

	// Token: 0x04001564 RID: 5476
	public long m_uid;

	// Token: 0x04001565 RID: 5477
	public int m_worldGenVersion;

	// Token: 0x04001566 RID: 5478
	public bool m_menu;

	// Token: 0x04001567 RID: 5479
	public bool m_needsDB;

	// Token: 0x04001568 RID: 5480
	public bool m_createBackupBeforeSaving;

	// Token: 0x04001569 RID: 5481
	public SaveWithBackups saves;

	// Token: 0x0400156A RID: 5482
	public World.SaveDataError m_dataError;

	// Token: 0x0400156B RID: 5483
	public FileHelpers.FileSource m_fileSource = FileHelpers.FileSource.Local;

	// Token: 0x02000208 RID: 520
	public enum SaveDataError
	{
		// Token: 0x0400156D RID: 5485
		None,
		// Token: 0x0400156E RID: 5486
		BadVersion,
		// Token: 0x0400156F RID: 5487
		LoadError,
		// Token: 0x04001570 RID: 5488
		Corrupt,
		// Token: 0x04001571 RID: 5489
		MissingMeta,
		// Token: 0x04001572 RID: 5490
		MissingDB
	}
}
