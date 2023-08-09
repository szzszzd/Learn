using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020001DB RID: 475
public class PlayerProfile
{
	// Token: 0x06001364 RID: 4964 RVA: 0x00080444 File Offset: 0x0007E644
	public PlayerProfile(string filename = null, FileHelpers.FileSource fileSource = FileHelpers.FileSource.Auto)
	{
		this.m_filename = filename;
		if (fileSource == FileHelpers.FileSource.Auto)
		{
			this.m_fileSource = (FileHelpers.m_cloudEnabled ? FileHelpers.FileSource.Cloud : FileHelpers.FileSource.Local);
		}
		else
		{
			this.m_fileSource = fileSource;
		}
		this.m_playerName = "Stranger";
		this.m_playerID = Utils.GenerateUID();
	}

	// Token: 0x06001365 RID: 4965 RVA: 0x000804CF File Offset: 0x0007E6CF
	public bool Load()
	{
		return this.m_filename != null && this.LoadPlayerFromDisk();
	}

	// Token: 0x06001366 RID: 4966 RVA: 0x000804E1 File Offset: 0x0007E6E1
	public bool Save()
	{
		return this.m_filename != null && this.SavePlayerToDisk();
	}

	// Token: 0x06001367 RID: 4967 RVA: 0x000804F4 File Offset: 0x0007E6F4
	public bool HaveIncompatiblPlayerData()
	{
		if (this.m_filename == null)
		{
			return false;
		}
		ZPackage zpackage = this.LoadPlayerDataFromDisk();
		if (zpackage == null)
		{
			return false;
		}
		if (!global::Version.IsPlayerVersionCompatible(zpackage.ReadInt()))
		{
			ZLog.Log("Player data is not compatible, ignoring");
			return true;
		}
		return false;
	}

	// Token: 0x06001368 RID: 4968 RVA: 0x00080534 File Offset: 0x0007E734
	public void SavePlayerData(Player player)
	{
		ZPackage zpackage = new ZPackage();
		player.Save(zpackage);
		this.m_playerData = zpackage.GetArray();
	}

	// Token: 0x06001369 RID: 4969 RVA: 0x0008055C File Offset: 0x0007E75C
	public void LoadPlayerData(Player player)
	{
		player.SetPlayerID(this.m_playerID, this.GetName());
		if (this.m_playerData != null)
		{
			ZPackage pkg = new ZPackage(this.m_playerData);
			player.Load(pkg);
			return;
		}
		player.GiveDefaultItems();
	}

	// Token: 0x0600136A RID: 4970 RVA: 0x0008059D File Offset: 0x0007E79D
	public void SaveLogoutPoint()
	{
		if (Player.m_localPlayer && !Player.m_localPlayer.IsDead() && !Player.m_localPlayer.InIntro())
		{
			this.SetLogoutPoint(Player.m_localPlayer.transform.position);
		}
	}

	// Token: 0x0600136B RID: 4971 RVA: 0x000805D8 File Offset: 0x0007E7D8
	private bool SavePlayerToDisk()
	{
		Action savingStarted = PlayerProfile.SavingStarted;
		if (savingStarted != null)
		{
			savingStarted();
		}
		DateTime now = DateTime.Now;
		bool flag = SaveSystem.CheckMove(this.m_filename, SaveDataType.Character, ref this.m_fileSource, now, 0UL);
		if (this.m_createBackupBeforeSaving && !flag)
		{
			SaveWithBackups saveWithBackups;
			if (SaveSystem.TryGetSaveByName(this.m_filename, SaveDataType.Character, out saveWithBackups) && !saveWithBackups.IsDeleted)
			{
				if (SaveSystem.CreateBackup(saveWithBackups.PrimaryFile, DateTime.Now, this.m_fileSource))
				{
					ZLog.Log("Migrating character save from an old save format, created backup!");
				}
				else
				{
					ZLog.LogError("Failed to create backup of character save " + this.m_filename + "!");
				}
			}
			else
			{
				ZLog.LogError("Failed to get character save " + this.m_filename + " from save system, so a backup couldn't be created!");
			}
		}
		this.m_createBackupBeforeSaving = false;
		string text = PlayerProfile.GetCharacterFolderPath(this.m_fileSource) + this.m_filename + ".fch";
		string oldFile = text + ".old";
		string text2 = text + ".new";
		string characterFolderPath = PlayerProfile.GetCharacterFolderPath(this.m_fileSource);
		if (!Directory.Exists(characterFolderPath) && this.m_fileSource != FileHelpers.FileSource.Cloud)
		{
			Directory.CreateDirectory(characterFolderPath);
		}
		ZPackage zpackage = new ZPackage();
		zpackage.Write(37);
		zpackage.Write(this.m_playerStats.m_kills);
		zpackage.Write(this.m_playerStats.m_deaths);
		zpackage.Write(this.m_playerStats.m_crafts);
		zpackage.Write(this.m_playerStats.m_builds);
		zpackage.Write(this.m_worldData.Count);
		foreach (KeyValuePair<long, PlayerProfile.WorldPlayerData> keyValuePair in this.m_worldData)
		{
			zpackage.Write(keyValuePair.Key);
			zpackage.Write(keyValuePair.Value.m_haveCustomSpawnPoint);
			zpackage.Write(keyValuePair.Value.m_spawnPoint);
			zpackage.Write(keyValuePair.Value.m_haveLogoutPoint);
			zpackage.Write(keyValuePair.Value.m_logoutPoint);
			zpackage.Write(keyValuePair.Value.m_haveDeathPoint);
			zpackage.Write(keyValuePair.Value.m_deathPoint);
			zpackage.Write(keyValuePair.Value.m_homePoint);
			zpackage.Write(keyValuePair.Value.m_mapData != null);
			if (keyValuePair.Value.m_mapData != null)
			{
				zpackage.Write(keyValuePair.Value.m_mapData);
			}
		}
		zpackage.Write(this.m_playerName);
		zpackage.Write(this.m_playerID);
		zpackage.Write(this.m_startSeed);
		if (this.m_playerData != null)
		{
			zpackage.Write(true);
			zpackage.Write(this.m_playerData);
		}
		else
		{
			zpackage.Write(false);
		}
		byte[] array = zpackage.GenerateHash();
		byte[] array2 = zpackage.GetArray();
		FileWriter fileWriter = new FileWriter(text2, FileHelpers.FileHelperType.Binary, this.m_fileSource);
		fileWriter.m_binary.Write(array2.Length);
		fileWriter.m_binary.Write(array2);
		fileWriter.m_binary.Write(array.Length);
		fileWriter.m_binary.Write(array);
		fileWriter.Finish();
		SaveSystem.InvalidateCache();
		if (fileWriter.Status != FileWriter.WriterStatus.CloseSucceeded && this.m_fileSource == FileHelpers.FileSource.Cloud)
		{
			string text3 = string.Concat(new string[]
			{
				PlayerProfile.GetCharacterFolderPath(FileHelpers.FileSource.Local),
				this.m_filename,
				"_backup_cloud-",
				now.ToString("yyyyMMdd-HHmmss"),
				".fch"
			});
			fileWriter.DumpCloudWriteToLocalFile(text3);
			SaveSystem.InvalidateCache();
			ZLog.LogError(string.Concat(new string[]
			{
				"Cloud save to location \"",
				text,
				"\" failed! Saved as local backup \"",
				text3,
				"\". Use the \"Manage saves\" menu to restore this backup."
			}));
		}
		else
		{
			FileHelpers.ReplaceOldFile(text, text2, oldFile, this.m_fileSource);
			SaveSystem.InvalidateCache();
			ZNet.ConsiderAutoBackup(this.m_filename, SaveDataType.Character, now);
		}
		Action savingFinished = PlayerProfile.SavingFinished;
		if (savingFinished != null)
		{
			savingFinished();
		}
		return true;
	}

	// Token: 0x0600136C RID: 4972 RVA: 0x000809F0 File Offset: 0x0007EBF0
	private bool LoadPlayerFromDisk()
	{
		try
		{
			ZPackage zpackage = this.LoadPlayerDataFromDisk();
			if (zpackage == null)
			{
				ZLog.LogWarning("No player data");
				return false;
			}
			int num = zpackage.ReadInt();
			if (!global::Version.IsPlayerVersionCompatible(num))
			{
				ZLog.Log("Player data is not compatible, ignoring");
				return false;
			}
			if (num != 37)
			{
				this.m_createBackupBeforeSaving = true;
			}
			if (num >= 28)
			{
				this.m_playerStats.m_kills = zpackage.ReadInt();
				this.m_playerStats.m_deaths = zpackage.ReadInt();
				this.m_playerStats.m_crafts = zpackage.ReadInt();
				this.m_playerStats.m_builds = zpackage.ReadInt();
			}
			this.m_worldData.Clear();
			int num2 = zpackage.ReadInt();
			for (int i = 0; i < num2; i++)
			{
				long key = zpackage.ReadLong();
				PlayerProfile.WorldPlayerData worldPlayerData = new PlayerProfile.WorldPlayerData();
				worldPlayerData.m_haveCustomSpawnPoint = zpackage.ReadBool();
				worldPlayerData.m_spawnPoint = zpackage.ReadVector3();
				worldPlayerData.m_haveLogoutPoint = zpackage.ReadBool();
				worldPlayerData.m_logoutPoint = zpackage.ReadVector3();
				if (num >= 30)
				{
					worldPlayerData.m_haveDeathPoint = zpackage.ReadBool();
					worldPlayerData.m_deathPoint = zpackage.ReadVector3();
				}
				worldPlayerData.m_homePoint = zpackage.ReadVector3();
				if (num >= 29 && zpackage.ReadBool())
				{
					worldPlayerData.m_mapData = zpackage.ReadByteArray();
				}
				this.m_worldData.Add(key, worldPlayerData);
			}
			this.SetName(zpackage.ReadString());
			this.m_playerID = zpackage.ReadLong();
			this.m_startSeed = zpackage.ReadString();
			if (zpackage.ReadBool())
			{
				this.m_playerData = zpackage.ReadByteArray();
			}
			else
			{
				this.m_playerData = null;
			}
		}
		catch (Exception ex)
		{
			ZLog.LogWarning("Exception while loading player profile:" + this.m_filename + " , " + ex.ToString());
		}
		return true;
	}

	// Token: 0x0600136D RID: 4973 RVA: 0x00080BD4 File Offset: 0x0007EDD4
	private ZPackage LoadPlayerDataFromDisk()
	{
		string path = PlayerProfile.GetPath(this.m_fileSource, this.m_filename);
		FileReader fileReader;
		try
		{
			fileReader = new FileReader(path, this.m_fileSource, FileHelpers.FileHelperType.Binary);
		}
		catch (Exception ex)
		{
			ZLog.Log(string.Concat(new string[]
			{
				"  failed to load: ",
				path,
				" (",
				ex.Message,
				")"
			}));
			return null;
		}
		byte[] data;
		try
		{
			BinaryReader binary = fileReader.m_binary;
			int count = binary.ReadInt32();
			data = binary.ReadBytes(count);
			int count2 = binary.ReadInt32();
			binary.ReadBytes(count2);
		}
		catch (Exception ex2)
		{
			ZLog.LogError(string.Format("  error loading player.dat. Source: {0}, Path: {1}, Error: {2}", this.m_fileSource, path, ex2.Message));
			fileReader.Dispose();
			return null;
		}
		fileReader.Dispose();
		return new ZPackage(data);
	}

	// Token: 0x0600136E RID: 4974 RVA: 0x00080CC4 File Offset: 0x0007EEC4
	public void SetLogoutPoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint = true;
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_logoutPoint = point;
	}

	// Token: 0x0600136F RID: 4975 RVA: 0x00080CF2 File Offset: 0x0007EEF2
	public void SetDeathPoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveDeathPoint = true;
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_deathPoint = point;
	}

	// Token: 0x06001370 RID: 4976 RVA: 0x00080D20 File Offset: 0x0007EF20
	public void SetMapData(byte[] data)
	{
		long worldUID = ZNet.instance.GetWorldUID();
		if (worldUID != 0L)
		{
			this.GetWorldData(worldUID).m_mapData = data;
		}
	}

	// Token: 0x06001371 RID: 4977 RVA: 0x00080D48 File Offset: 0x0007EF48
	public byte[] GetMapData()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_mapData;
	}

	// Token: 0x06001372 RID: 4978 RVA: 0x00080D5F File Offset: 0x0007EF5F
	public void ClearLoguoutPoint()
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint = false;
	}

	// Token: 0x06001373 RID: 4979 RVA: 0x00080D77 File Offset: 0x0007EF77
	public bool HaveLogoutPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint;
	}

	// Token: 0x06001374 RID: 4980 RVA: 0x00080D8E File Offset: 0x0007EF8E
	public Vector3 GetLogoutPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_logoutPoint;
	}

	// Token: 0x06001375 RID: 4981 RVA: 0x00080DA5 File Offset: 0x0007EFA5
	public bool HaveDeathPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveDeathPoint;
	}

	// Token: 0x06001376 RID: 4982 RVA: 0x00080DBC File Offset: 0x0007EFBC
	public Vector3 GetDeathPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_deathPoint;
	}

	// Token: 0x06001377 RID: 4983 RVA: 0x00080DD3 File Offset: 0x0007EFD3
	public void SetCustomSpawnPoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint = true;
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_spawnPoint = point;
	}

	// Token: 0x06001378 RID: 4984 RVA: 0x00080E01 File Offset: 0x0007F001
	public Vector3 GetCustomSpawnPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_spawnPoint;
	}

	// Token: 0x06001379 RID: 4985 RVA: 0x00080E18 File Offset: 0x0007F018
	public bool HaveCustomSpawnPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint;
	}

	// Token: 0x0600137A RID: 4986 RVA: 0x00080E2F File Offset: 0x0007F02F
	public void ClearCustomSpawnPoint()
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint = false;
	}

	// Token: 0x0600137B RID: 4987 RVA: 0x00080E47 File Offset: 0x0007F047
	public void SetHomePoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_homePoint = point;
	}

	// Token: 0x0600137C RID: 4988 RVA: 0x00080E5F File Offset: 0x0007F05F
	public Vector3 GetHomePoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_homePoint;
	}

	// Token: 0x0600137D RID: 4989 RVA: 0x00080E76 File Offset: 0x0007F076
	public void SetName(string name)
	{
		this.m_playerName = name;
	}

	// Token: 0x0600137E RID: 4990 RVA: 0x00080E7F File Offset: 0x0007F07F
	public string GetName()
	{
		return this.m_playerName;
	}

	// Token: 0x0600137F RID: 4991 RVA: 0x00080E87 File Offset: 0x0007F087
	public long GetPlayerID()
	{
		return this.m_playerID;
	}

	// Token: 0x06001380 RID: 4992 RVA: 0x00080E90 File Offset: 0x0007F090
	public static void RemoveProfile(string name, FileHelpers.FileSource fileSource = FileHelpers.FileSource.Auto)
	{
		SaveWithBackups saveWithBackups;
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.Character, out saveWithBackups) && !saveWithBackups.IsDeleted)
		{
			SaveSystem.Delete(saveWithBackups.PrimaryFile);
		}
	}

	// Token: 0x06001381 RID: 4993 RVA: 0x00080EBC File Offset: 0x0007F0BC
	public static bool HaveProfile(string name)
	{
		SaveWithBackups saveWithBackups;
		return SaveSystem.TryGetSaveByName(name, SaveDataType.Character, out saveWithBackups) && !saveWithBackups.IsDeleted;
	}

	// Token: 0x06001382 RID: 4994 RVA: 0x00080EDF File Offset: 0x0007F0DF
	private static string GetCharacterFolder(FileHelpers.FileSource fileSource)
	{
		if (fileSource != FileHelpers.FileSource.Local)
		{
			return "/characters/";
		}
		return "/characters_local/";
	}

	// Token: 0x06001383 RID: 4995 RVA: 0x00080EF0 File Offset: 0x0007F0F0
	public static string GetCharacterFolderPath(FileHelpers.FileSource fileSource)
	{
		return Utils.GetSaveDataPath(fileSource) + PlayerProfile.GetCharacterFolder(fileSource);
	}

	// Token: 0x06001384 RID: 4996 RVA: 0x00080F03 File Offset: 0x0007F103
	public string GetFilename()
	{
		return this.m_filename;
	}

	// Token: 0x06001385 RID: 4997 RVA: 0x00080F0B File Offset: 0x0007F10B
	public string GetPath()
	{
		return PlayerProfile.GetPath(this.m_fileSource, this.m_filename);
	}

	// Token: 0x06001386 RID: 4998 RVA: 0x00080F1E File Offset: 0x0007F11E
	public static string GetPath(FileHelpers.FileSource fileSource, string name)
	{
		return PlayerProfile.GetCharacterFolderPath(fileSource) + name + ".fch";
	}

	// Token: 0x06001387 RID: 4999 RVA: 0x00080F34 File Offset: 0x0007F134
	private PlayerProfile.WorldPlayerData GetWorldData(long worldUID)
	{
		PlayerProfile.WorldPlayerData worldPlayerData;
		if (this.m_worldData.TryGetValue(worldUID, out worldPlayerData))
		{
			return worldPlayerData;
		}
		worldPlayerData = new PlayerProfile.WorldPlayerData();
		this.m_worldData.Add(worldUID, worldPlayerData);
		return worldPlayerData;
	}

	// Token: 0x0400145F RID: 5215
	public static Action SavingStarted;

	// Token: 0x04001460 RID: 5216
	public static Action SavingFinished;

	// Token: 0x04001461 RID: 5217
	public static Vector3 m_originalSpawnPoint = new Vector3(-676f, 50f, 299f);

	// Token: 0x04001462 RID: 5218
	public readonly PlayerProfile.PlayerStats m_playerStats = new PlayerProfile.PlayerStats();

	// Token: 0x04001463 RID: 5219
	public FileHelpers.FileSource m_fileSource = FileHelpers.FileSource.Local;

	// Token: 0x04001464 RID: 5220
	public readonly string m_filename = "";

	// Token: 0x04001465 RID: 5221
	private string m_playerName = "";

	// Token: 0x04001466 RID: 5222
	private long m_playerID;

	// Token: 0x04001467 RID: 5223
	private string m_startSeed = "";

	// Token: 0x04001468 RID: 5224
	private byte[] m_playerData;

	// Token: 0x04001469 RID: 5225
	private readonly Dictionary<long, PlayerProfile.WorldPlayerData> m_worldData = new Dictionary<long, PlayerProfile.WorldPlayerData>();

	// Token: 0x0400146A RID: 5226
	private bool m_createBackupBeforeSaving;

	// Token: 0x020001DC RID: 476
	private class WorldPlayerData
	{
		// Token: 0x0400146B RID: 5227
		public Vector3 m_spawnPoint = Vector3.zero;

		// Token: 0x0400146C RID: 5228
		public bool m_haveCustomSpawnPoint;

		// Token: 0x0400146D RID: 5229
		public Vector3 m_logoutPoint = Vector3.zero;

		// Token: 0x0400146E RID: 5230
		public bool m_haveLogoutPoint;

		// Token: 0x0400146F RID: 5231
		public Vector3 m_deathPoint = Vector3.zero;

		// Token: 0x04001470 RID: 5232
		public bool m_haveDeathPoint;

		// Token: 0x04001471 RID: 5233
		public Vector3 m_homePoint = Vector3.zero;

		// Token: 0x04001472 RID: 5234
		public byte[] m_mapData;
	}

	// Token: 0x020001DD RID: 477
	public class PlayerStats
	{
		// Token: 0x04001473 RID: 5235
		public int m_kills;

		// Token: 0x04001474 RID: 5236
		public int m_deaths;

		// Token: 0x04001475 RID: 5237
		public int m_crafts;

		// Token: 0x04001476 RID: 5238
		public int m_builds;
	}
}
